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
    bool IsTruncated,
    MemoryGrantsHistoryPoint HistoryPoint);

public sealed record MemoryGrantsHistoryPoint(
    DateTimeOffset CapturedAt,
    double AvailableMemoryKb,
    double TargetMemoryKb,
    double WaitingRequestedMemoryKb,
    double GrantedRequestedMemoryKb,
    double MaximumWaitTimeMs,
    double WaiterCount,
    int GrantObservations,
    int WaitingGrantObservations,
    int GrantedGrantObservations);

public interface IMemoryGrantsHistoryStore
{
    Task<IReadOnlyList<MemoryGrantsHistoryPoint>> ReadAsync(CancellationToken cancellationToken);
    Task AppendAsync(MemoryGrantsHistoryPoint point, CancellationToken cancellationToken);
    Task ClearAsync(CancellationToken cancellationToken);
}

public sealed record WatchObservation(
    WatchStatus Status,
    MemoryGrantSnapshot? Snapshot,
    Task Changed,
    CancellationToken WatchCancellation);

public sealed record ReportResult(
    IReadOnlyList<string> Columns,
    IReadOnlyList<IReadOnlyList<string?>> Rows,
    long TotalRows,
    TimeSpan Elapsed,
    bool IsTruncated);

public sealed record CapacitySnapshot(
    DateTimeOffset CapturedAt,
    IReadOnlyList<string> Columns,
    IReadOnlyList<string?> Values);

public sealed record CapacityWatchObservation(
    WatchStatus Status,
    CapacitySnapshot? Snapshot,
    Task Changed,
    CancellationToken WatchCancellation);