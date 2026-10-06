namespace MssqlLocks.Web.Services;

public sealed record WatchStatus(
    bool IsRunning,
    int IntervalSeconds,
    string State,
    DateTimeOffset? LastSampleAt,
    string Message)
{
    public static WatchStatus Stopped { get; } = new(
        false,
        5,
        "Stopped",
        null,
        "Watch is stopped. Start it when a live sample is needed.");
}

public sealed record MemoryGrantSnapshot(
    DateTimeOffset CapturedAt,
    IReadOnlyList<string> Columns,
    IReadOnlyList<IReadOnlyList<string?>> Rows,
    long TotalRows,
    int WaitingCount,
    int GrantedCount,
    bool IsTruncated);

public sealed record WatchObservation(
    WatchStatus Status,
    MemoryGrantSnapshot? Snapshot,
    Task Changed,
    CancellationToken WatchCancellation);