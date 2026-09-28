using System.Text.Json;
using MssqlLocks.Worker;

try
{
    return Run(args);
}
catch (Exception ex) when (ex is ArgumentException or InvalidOperationException or IOException or UnauthorizedAccessException or JsonException or Microsoft.Data.Sqlite.SqliteException)
{
    Console.Error.WriteLine($"Error: {ex.Message}");
    return 1;
}

static int Run(string[] args)
{
    const string usage = "Usage: [--inbox-dir PATH] [--result-dir PATH] init|enqueue FILE|claim|process|status|list|dead-letter|retry CLAIM_JSON REASON";
    var options = new Dictionary<string, string>();
    var positional = new List<string>();
    for (var i = 0; i < args.Length; i++)
    {
        if (args[i] is "--inbox-dir" or "--result-dir")
        {
            if (i + 1 == args.Length || args[i + 1].StartsWith("--", StringComparison.Ordinal) ||
                !options.TryAdd(args[i], args[++i]))
                throw new ArgumentException("Each directory option requires exactly one path and may appear only once.");
        }
        else if (args[i].StartsWith("--", StringComparison.Ordinal))
            throw new ArgumentException($"Unknown option: {args[i]}");
        else positional.Add(args[i]);
    }

    if (positional.Count == 0) throw new ArgumentException(usage);
    var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
    if (string.IsNullOrWhiteSpace(appData) && (!options.ContainsKey("--inbox-dir") || !options.ContainsKey("--result-dir")))
        throw new InvalidOperationException("Local application data is unavailable; supply both directory options.");
    var root = Path.Combine(appData, "MssqlLocks");
    var inbox = new SqliteAlertInbox(options.GetValueOrDefault("--inbox-dir") ?? Path.Combine(root, "inbox"));
    var results = new PlaceholderInvestigator(options.GetValueOrDefault("--result-dir") ?? Path.Combine(root, "results"));
    inbox.Initialize();
    var command = positional[0];
    switch (command)
    {
        case "init" when positional.Count == 1:
            Console.WriteLine("Inbox initialized.");
            break;
        case "enqueue" when positional.Count == 2:
            var alert = AlertEnvelope.Parse(File.ReadAllText(positional[1]));
            Console.WriteLine(inbox.Enqueue(alert) ? "Enqueued." : "Duplicate eventId; unchanged.");
            break;
        case "claim" when positional.Count == 1:
            Print(inbox.ClaimNext(TimeSpan.FromMinutes(5)));
            break;
        case "process" when positional.Count == 1:
            var claim = inbox.ClaimNext(TimeSpan.FromMinutes(5));
            if (claim is null) { Console.WriteLine("No ready alerts."); break; }
            var resultRef = results.WriteResult(claim);
            inbox.RecordResult(claim, resultRef);
            inbox.Acknowledge(claim);
            Print(inbox.List().Single(x => x.EventId == claim.Alert.EventId));
            break;
        case "retry" when positional.Count == 3:
            var receipt = JsonSerializer.Deserialize<ClaimedAlert>(File.ReadAllText(positional[1]), AlertEnvelope.JsonOptions)
                ?? throw new ArgumentException("Claim file must contain a claim receipt.");
            ValidateClaimReceipt(receipt);
            inbox.Abandon(receipt, positional[2]);
            Console.WriteLine("Lease abandoned; retry scheduled or dead-lettered.");
            break;
        case "status" when positional.Count == 1:
            Print(inbox.Counts());
            break;
        case "list" when positional.Count == 1:
            Print(inbox.List());
            break;
        case "dead-letter" when positional.Count == 1:
            Print(inbox.List("dead-letter"));
            break;
        default: throw new ArgumentException(usage);
    }
    return 0;
}

static void ValidateClaimReceipt(ClaimedAlert receipt)
{
    if (receipt.Alert is null) throw new ArgumentException("Claim receipt must contain an alert.");
    receipt.Alert.Validate();
    if (string.IsNullOrWhiteSpace(receipt.LeaseToken))
        throw new ArgumentException("Claim receipt must contain a nonempty lease token.");
    if (receipt.Attempt <= 0)
        throw new ArgumentException("Claim receipt attempt must be greater than zero.");
    if (receipt.LeaseUntilUtc == default || receipt.LeaseUntilUtc.Offset != TimeSpan.Zero)
        throw new ArgumentException("Claim receipt leaseUntilUtc must be an explicit UTC timestamp with Z or +00:00 offset.");
}

static void Print<T>(T value) => Console.WriteLine(JsonSerializer.Serialize(value, AlertEnvelope.JsonOptions));
