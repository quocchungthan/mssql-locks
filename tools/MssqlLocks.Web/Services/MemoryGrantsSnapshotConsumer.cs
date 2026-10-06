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
    ];

    private readonly List<IReadOnlyList<string?>> rows = [];
    private int[] visibleColumnIndexes = [];
    private string[] visibleColumnNames = [];
    private int grantStateColumnIndex = -1;
    private int waitingCount;
    private int grantedCount;

    public MemoryGrantSnapshot? Snapshot { get; private set; }

    public Task BeginAsync(IReadOnlyList<ReportColumn> columns, CancellationToken cancellationToken)
    {
        rows.Clear();
        waitingCount = 0;
        grantedCount = 0;
        visibleColumnIndexes = Enumerable.Range(0, columns.Count)
            .Where(index => !IsOmittedColumn(columns[index].Name))
            .ToArray();
        visibleColumnNames = visibleColumnIndexes.Select(index => columns[index].Name).ToArray();
        grantStateColumnIndex = Enumerable.Range(0, columns.Count)
            .FirstOrDefault(index => string.Equals(columns[index].Name, "grant_state", StringComparison.OrdinalIgnoreCase), -1);
        return Task.CompletedTask;
    }

    public Task WriteRowAsync(ReportRow row, CancellationToken cancellationToken)
    {
        if (grantStateColumnIndex >= 0)
        {
            var grantState = Convert.ToString(row.Values[grantStateColumnIndex], CultureInfo.InvariantCulture);
            if (string.Equals(grantState, "WAITING", StringComparison.OrdinalIgnoreCase))
            {
                waitingCount++;
            }
            else if (string.Equals(grantState, "GRANTED", StringComparison.OrdinalIgnoreCase))
            {
                grantedCount++;
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
        Snapshot = new MemoryGrantSnapshot(
            DateTimeOffset.UtcNow,
            visibleColumnNames,
            rows.ToArray(),
            rowCount,
            waitingCount,
            grantedCount,
            rowCount > MaximumRows);
        return Task.CompletedTask;
    }

    private static bool IsOmittedColumn(string name) => OmittedColumnFragments.Any(fragment =>
        name.Contains(fragment, StringComparison.OrdinalIgnoreCase));

    private static string? FormatValue(object? value) => value is null or DBNull
        ? null
        : Convert.ToString(value, CultureInfo.InvariantCulture);
}