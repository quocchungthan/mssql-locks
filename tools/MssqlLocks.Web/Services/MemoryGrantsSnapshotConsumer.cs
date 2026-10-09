using System.Globalization;
using MssqlLocks.Reporting;

namespace MssqlLocks.Web.Services;

public sealed class MemoryGrantsSnapshotConsumer : IReportResultConsumer
{
    private const int MaximumRows = 100;
    private static readonly string[] OmittedColumnFragments = [
        "query_text",
        "sql_text",
        "host_name",
        "program_name",
        "session_id",
        "login_name",
        "pool_id",
        "resource_semaphore_id",
    ];

    private readonly List<IReadOnlyList<string?>> rows = [];
    private int[] visibleColumnIndexes = [];
    private string[] visibleColumnNames = [];
    private int recordTypeColumnIndex = -1;
    private int capturedAtColumnIndex = -1;
    private int grantStateColumnIndex = -1;
    private int requestedMemoryColumnIndex = -1;
    private int waitTimeColumnIndex = -1;
    private int availableMemoryColumnIndex = -1;
    private int targetMemoryColumnIndex = -1;
    private int waiterCountColumnIndex = -1;
    private DateTimeOffset? capturedAt;
    private double availableMemoryKb;
    private double targetMemoryKb;
    private double waitingRequestedMemoryKb;
    private double grantedRequestedMemoryKb;
    private double maximumWaitTimeMs;
    private double waiterCount;
    private int grantObservations;
    private int waitingCount;
    private int grantedCount;

    public MemoryGrantSnapshot? Snapshot { get; private set; }

    public Task BeginAsync(IReadOnlyList<ReportColumn> columns, CancellationToken cancellationToken)
    {
        rows.Clear();
        availableMemoryKb = 0;
        targetMemoryKb = 0;
        waitingRequestedMemoryKb = 0;
        grantedRequestedMemoryKb = 0;
        maximumWaitTimeMs = 0;
        waiterCount = 0;
        grantObservations = 0;
        waitingCount = 0;
        grantedCount = 0;
        capturedAt = null;
        visibleColumnIndexes = Enumerable.Range(0, columns.Count)
            .Where(index => !IsOmittedColumn(columns[index].Name))
            .ToArray();
        visibleColumnNames = visibleColumnIndexes.Select(index => columns[index].Name).ToArray();
        recordTypeColumnIndex = FindColumn(columns, "record_type");
        capturedAtColumnIndex = FindColumn(columns, "snapshot_utc");
        grantStateColumnIndex = FindColumn(columns, "grant_state");
        requestedMemoryColumnIndex = FindColumn(columns, "requested_memory_kb");
        waitTimeColumnIndex = FindColumn(columns, "wait_time_ms");
        availableMemoryColumnIndex = FindColumn(columns, "semaphore_available_memory_kb");
        targetMemoryColumnIndex = FindColumn(columns, "semaphore_target_memory_kb");
        waiterCountColumnIndex = FindColumn(columns, "semaphore_waiter_count");
        return Task.CompletedTask;
    }

    public Task WriteRowAsync(ReportRow row, CancellationToken cancellationToken)
    {
        if (capturedAt is null
            && DateTimeOffset.TryParse(GetString(row, capturedAtColumnIndex), CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var snapshotTime))
        {
            capturedAt = snapshotTime.ToUniversalTime();
        }

        var recordType = GetString(row, recordTypeColumnIndex);
        if (string.Equals(recordType, "SEMAPHORE", StringComparison.OrdinalIgnoreCase))
        {
            availableMemoryKb += GetNumber(row, availableMemoryColumnIndex);
            targetMemoryKb += GetNumber(row, targetMemoryColumnIndex);
            waiterCount += GetNumber(row, waiterCountColumnIndex);
        }
        else if (string.Equals(recordType, "GRANT", StringComparison.OrdinalIgnoreCase))
        {
            grantObservations++;
            var grantState = GetString(row, grantStateColumnIndex);
            if (string.Equals(grantState, "WAITING", StringComparison.OrdinalIgnoreCase))
            {
                waitingCount++;
                waitingRequestedMemoryKb += GetNumber(row, requestedMemoryColumnIndex);
                maximumWaitTimeMs = Math.Max(maximumWaitTimeMs, GetNumber(row, waitTimeColumnIndex));
            }
            else if (string.Equals(grantState, "GRANTED", StringComparison.OrdinalIgnoreCase))
            {
                grantedCount++;
                grantedRequestedMemoryKb += GetNumber(row, requestedMemoryColumnIndex);
            }
        }

        if (rows.Count < MaximumRows)
        {
            rows.Add(visibleColumnIndexes
                .Select(index => FormatValue(row.Values[index]))
                .ToArray());
        }

        return Task.CompletedTask;
    }

    public Task CompleteAsync(long rowCount, TimeSpan elapsed, CancellationToken cancellationToken)
    {
        var sampleTime = capturedAt ?? DateTimeOffset.UtcNow;

        var historyPoint = new MemoryGrantsHistoryPoint(
            sampleTime,
            availableMemoryKb,
            targetMemoryKb,
            waitingRequestedMemoryKb,
            grantedRequestedMemoryKb,
            maximumWaitTimeMs,
            waiterCount,
            grantObservations,
            waitingCount,
            grantedCount);
        Snapshot = new MemoryGrantSnapshot(
            sampleTime,
            visibleColumnNames,
            rows.ToArray(),
            rowCount,
            waitingCount,
            grantedCount,
            rowCount > MaximumRows,
            historyPoint);
        return Task.CompletedTask;
    }

    private static bool IsOmittedColumn(string name) => OmittedColumnFragments.Any(fragment =>
        name.Contains(fragment, StringComparison.OrdinalIgnoreCase));

    private static int FindColumn(IReadOnlyList<ReportColumn> columns, string name) => Enumerable.Range(0, columns.Count)
        .FirstOrDefault(index => string.Equals(columns[index].Name, name, StringComparison.OrdinalIgnoreCase), -1);

    private static string? GetString(ReportRow row, int columnIndex) => columnIndex < 0
        ? null
        : Convert.ToString(row.Values[columnIndex], CultureInfo.InvariantCulture);

    private static double GetNumber(ReportRow row, int columnIndex) => columnIndex < 0 || row.Values[columnIndex] is null or DBNull
        ? 0
        : Convert.ToDouble(row.Values[columnIndex], CultureInfo.InvariantCulture);

    private static string? FormatValue(object? value) => value is null or DBNull
        ? null
        : Convert.ToString(value, CultureInfo.InvariantCulture);
}