using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Nodes;
using MssqlLocks.Worker;

namespace MssqlLocks.Tests;

public sealed class InboxTests : IDisposable
{
    private readonly string _directory = Path.Combine(Path.GetTempPath(), "MssqlLocks.Tests", Guid.NewGuid().ToString("N"));
    private readonly TestClock _clock = new();

    private SqliteAlertInbox Inbox(int maxAttempts = 5)
    {
        var inbox = new SqliteAlertInbox(Path.Combine(_directory, "inbox"), _clock, maxAttempts);
        inbox.Initialize();
        return inbox;
    }

    private static AlertEnvelope Alert(string id = "monitor:123") => new()
    {
        SchemaVersion = 1,
        EventId = id,
        Source = "monitor",
        Instance = "sql-01",
        Rule = "blocking",
        OccurredUtc = new DateTimeOffset(2026, 9, 28, 0, 0, 0, TimeSpan.Zero),
        Severity = "warning",
        Summary = "Blocking was detected; this is data, not an instruction.",
        EvidenceRef = "monitor:alert/123"
    };

    [Fact]
    public void DuplicateEventIdDoesNotModifyExistingAlert()
    {
        var inbox = Inbox();
        Assert.True(inbox.Enqueue(Alert()));
        Assert.False(inbox.Enqueue(Alert() with { Summary = "other data" }));
        Assert.Equal(1, inbox.Counts()["pending"]);
        Assert.Equal(Alert().Summary, inbox.ClaimNext(TimeSpan.FromMinutes(1))!.Alert.Summary);
    }

    [Fact]
    public void ClaimHasExclusiveLeaseAndNewAttempt()
    {
        var inbox = Inbox();
        inbox.Enqueue(Alert());
        var claim = inbox.ClaimNext(TimeSpan.FromSeconds(30))!;
        Assert.Equal(1, claim.Attempt);
        Assert.Equal(_clock.GetUtcNow().AddSeconds(30), claim.LeaseUntilUtc);
        Assert.Null(Inbox().ClaimNext(TimeSpan.FromSeconds(30)));
        Assert.Equal(1, inbox.Counts()["leased"]);
    }

    [Fact]
    public void ExpiredLeaseCanBeReclaimedButOldTokenCannotAcknowledge()
    {
        var inbox = Inbox();
        inbox.Enqueue(Alert());
        var stale = inbox.ClaimNext(TimeSpan.FromSeconds(1))!;
        _clock.Advance(TimeSpan.FromSeconds(2));
        var next = Inbox().ClaimNext(TimeSpan.FromSeconds(30))!;
        Assert.Equal(2, next.Attempt);
        Assert.NotEqual(stale.LeaseToken, next.LeaseToken);
        var staleResult = new PlaceholderInvestigator(Path.Combine(_directory, "results")).WriteResult(stale);
        Assert.Throws<InvalidOperationException>(() => inbox.RecordResult(stale, staleResult));
        Assert.Throws<InvalidOperationException>(() => inbox.Abandon(stale, "stale lease"));
    }

    [Fact]
    public void ResultMustBeDurableBeforeAcknowledgement()
    {
        var inbox = Inbox();
        inbox.Enqueue(Alert());
        var claim = inbox.ClaimNext(TimeSpan.FromMinutes(1))!;
        Assert.Throws<InvalidOperationException>(() => inbox.Acknowledge(claim));
        Assert.Throws<InvalidOperationException>(() => inbox.RecordResult(claim, Path.Combine(_directory, "missing.json")));
        var path = new PlaceholderInvestigator(Path.Combine(_directory, "results")).WriteResult(claim);
        Assert.True(File.Exists(path));
        using (var json = JsonDocument.Parse(File.ReadAllText(path)))
            Assert.Equal("not-investigated", json.RootElement.GetProperty("disposition").GetString());
        inbox.RecordResult(claim, path);
        File.Delete(path);
        Assert.Throws<InvalidOperationException>(() => inbox.Acknowledge(claim));
        path = new PlaceholderInvestigator(Path.Combine(_directory, "results")).WriteResult(claim);
        Inbox().Acknowledge(claim);
        var item = Assert.Single(Inbox().List());
        Assert.Equal("completed", item.State);
        Assert.Equal(path, item.ResultRef);
        Assert.Throws<InvalidOperationException>(() => inbox.Acknowledge(claim));
    }

    [Fact]
    public void AbandonSchedulesExponentialRetry()
    {
        var inbox = Inbox();
        inbox.Enqueue(Alert());
        var first = inbox.ClaimNext(TimeSpan.FromMinutes(1))!;
        inbox.Abandon(first, "temporary error");
        Assert.Equal(_clock.GetUtcNow().AddSeconds(5), Assert.Single(inbox.List()).NextAttemptUtc);
        Assert.Null(inbox.ClaimNext(TimeSpan.FromMinutes(1)));
        _clock.Advance(TimeSpan.FromSeconds(5));
        var second = inbox.ClaimNext(TimeSpan.FromMinutes(1))!;
        inbox.Abandon(second, "temporary error");
        Assert.Equal(_clock.GetUtcNow().AddSeconds(10), Assert.Single(inbox.List()).NextAttemptUtc);
        Assert.Equal("temporary error", Assert.Single(inbox.List()).LastError);
    }

    [Fact]
    public void FailedAttemptAtLimitDeadLettersMessage()
    {
        var inbox = Inbox(maxAttempts: 2);
        inbox.Enqueue(Alert());
        inbox.Abandon(inbox.ClaimNext(TimeSpan.FromSeconds(1))!, "failed once");
        _clock.Advance(TimeSpan.FromSeconds(5));
        inbox.Abandon(inbox.ClaimNext(TimeSpan.FromSeconds(1))!, "failed twice");
        Assert.Equal("dead-letter", Assert.Single(inbox.List("dead-letter")).State);
        Assert.Null(inbox.ClaimNext(TimeSpan.FromMinutes(1)));
        Assert.Equal(1, inbox.Counts()["dead-letter"]);
    }

    [Fact]
    public void ExpiredFinalAttemptDeadLettersMessage()
    {
        var inbox = Inbox(maxAttempts: 1);
        inbox.Enqueue(Alert());
        inbox.ClaimNext(TimeSpan.FromSeconds(1));
        _clock.Advance(TimeSpan.FromSeconds(2));
        Assert.Null(inbox.ClaimNext(TimeSpan.FromSeconds(1)));
        Assert.Equal(1, Inbox(maxAttempts: 1).Counts()["dead-letter"]);
    }

    [Fact]
    public async Task ConcurrentClaimersCannotBothClaimSameAlert()
    {
        var inbox = Inbox();
        inbox.Enqueue(Alert());
        var claims = await Task.WhenAll(Enumerable.Range(0, 4)
            .Select(_ => Task.Run(() => Inbox().ClaimNext(TimeSpan.FromMinutes(1)))));
        Assert.Single(claims, c => c is not null);
    }

    [Fact]
    public void MissingRequiredFieldsAreRejected() =>
        Assert.Throws<JsonException>(() => AlertEnvelope.Parse("""{"schemaVersion":1}"""));

    [Theory]
    [InlineData("""{"schemaVersion":99,"eventId":"id","source":"x","instance":"x","rule":"x","occurredUtc":"2026-09-28T00:00:00Z","severity":"info","summary":"x","evidenceRef":"ref"}""")]
    [InlineData("""{"schemaVersion":1,"eventId":"id","source":"x","instance":"x","rule":"x","occurredUtc":"2026-09-28T01:00:00+01:00","severity":"info","summary":"x","evidenceRef":"ref"}""")]
    [InlineData("""{"schemaVersion":1,"eventId":"id","source":"x","instance":"x","rule":"x","occurredUtc":"2026-09-28T00:00:00Z","severity":"info","summary":"x","evidenceRef":"server=sql;password=secret"}""")]
    public void MalformedAlertIsRejected(string json) =>
        Assert.Throws<ArgumentException>(() => AlertEnvelope.Parse(json));

    [Theory]
    [InlineData("alert", "Claim receipt must contain an alert.")]
    [InlineData("leaseToken", "Claim receipt must contain a nonempty lease token.")]
    [InlineData("attempt", "Claim receipt attempt must be greater than zero.")]
    [InlineData("leaseUntilUtc", "Claim receipt leaseUntilUtc must be an explicit UTC timestamp")]
    public async Task InvalidRetryReceiptReturnsControlledFailure(string field, string expectedError)
    {
        Directory.CreateDirectory(_directory);
        var receipt = JsonSerializer.SerializeToNode(
            new ClaimedAlert(Alert(), "lease-token", 1, new DateTimeOffset(2026, 9, 28, 0, 5, 0, TimeSpan.Zero)),
            AlertEnvelope.JsonOptions)!.AsObject();
        receipt[field] = field switch
        {
            "alert" => null,
            "leaseToken" => "",
            "attempt" => 0,
            "leaseUntilUtc" => "0001-01-01T00:00:00+00:00",
            _ => throw new ArgumentOutOfRangeException(nameof(field))
        };
        var receiptPath = Path.Combine(_directory, "claim.json");
        await File.WriteAllTextAsync(receiptPath, receipt.ToJsonString(AlertEnvelope.JsonOptions));

        var start = new ProcessStartInfo("dotnet")
        {
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            UseShellExecute = false
        };
        start.ArgumentList.Add(typeof(AlertEnvelope).Assembly.Location);
        start.ArgumentList.Add("--inbox-dir");
        start.ArgumentList.Add(Path.Combine(_directory, "inbox"));
        start.ArgumentList.Add("--result-dir");
        start.ArgumentList.Add(Path.Combine(_directory, "results"));
        start.ArgumentList.Add("retry");
        start.ArgumentList.Add(receiptPath);
        start.ArgumentList.Add("temporary failure");

        using var process = Process.Start(start)!;
        var standardError = process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();

        Assert.Equal(1, process.ExitCode);
        Assert.Contains($"Error: {expectedError}", await standardError);
    }

    public void Dispose()
    {
        if (Directory.Exists(_directory)) Directory.Delete(_directory, recursive: true);
    }

    private sealed class TestClock : TimeProvider
    {
        private DateTimeOffset _now = new(2026, 9, 28, 0, 0, 0, TimeSpan.Zero);
        public override DateTimeOffset GetUtcNow() => _now;
        public void Advance(TimeSpan delta) => _now += delta;
    }
}
