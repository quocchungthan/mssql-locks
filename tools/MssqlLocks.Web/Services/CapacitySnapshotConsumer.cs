using System.Globalization;
using MssqlLocks.Reporting;

namespace MssqlLocks.Web.Services;

public sealed class CapacitySnapshotConsumer : IReportResultConsumer
{
    private string[] columns = [];
    private string?[] values = [];

    public CapacitySnapshot? Snapshot { get; private set; }

    public Task BeginAsync(IReadOnlyList<ReportColumn> reportColumns, CancellationToken cancellationToken)
    {
        columns = reportColumns.Select(column => column.Name).ToArray();
        values = new string?[columns.Length];
        return Task.CompletedTask;
    }

    public Task WriteRowAsync(ReportRow row, CancellationToken cancellationToken)
    {
        for (var index = 0; index < values.Length; index++)
        {
            values[index] = FormatValue(row.Values[index]);
        }

        return Task.CompletedTask;
    }

    public Task CompleteAsync(long rowCount, TimeSpan elapsed, CancellationToken cancellationToken)
    {
        var observedAtIndex = Array.FindIndex(columns, column =>
            string.Equals(column, "observed_at_utc", StringComparison.OrdinalIgnoreCase));
        var capturedAt = observedAtIndex >= 0
            && DateTimeOffset.TryParse(values[observedAtIndex], CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var observedAt)
            ? observedAt
            : DateTimeOffset.UtcNow;
        Snapshot = new CapacitySnapshot(capturedAt, columns, values);
        return Task.CompletedTask;
    }

    private static string? FormatValue(object? value) => value is null or DBNull
        ? null
        : Convert.ToString(value, CultureInfo.InvariantCulture);
}