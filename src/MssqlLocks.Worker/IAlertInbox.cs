namespace MssqlLocks.Worker;

public sealed record ClaimedAlert(AlertEnvelope Alert, string LeaseToken, int Attempt, DateTimeOffset LeaseUntilUtc);
public sealed record InboxItem(string EventId, string State, int Attempts, DateTimeOffset NextAttemptUtc, string? ResultRef, string? LastError);

public interface IAlertInbox
{
    void Initialize();
    bool Enqueue(AlertEnvelope alert);
    ClaimedAlert? ClaimNext(TimeSpan leaseDuration);
    void RecordResult(ClaimedAlert claim, string resultRef);
    void Acknowledge(ClaimedAlert claim);
    void Abandon(ClaimedAlert claim, string reason);
    IReadOnlyList<InboxItem> List(string? state = null);
    IReadOnlyDictionary<string, int> Counts();
}
