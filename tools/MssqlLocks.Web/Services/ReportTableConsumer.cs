using System.Globalization;
using System.Diagnostics;
using MssqlLocks.Reporting;

namespace MssqlLocks.Web.Services;

public sealed class ReportTableConsumer : IReportResultConsumer
{
    private const int MaximumVisibleRows = 250;
    private readonly List<IReadOnlyList<string?>> rows = [];
    private string[] columns = [];
    private readonly Stopwatch elapsed = new();

    public ReportResult? Result { get; private set; }

    public Task BeginAsync(IReadOnlyList<ReportColumn> reportColumns, CancellationToken cancellationToken)
    {
        rows.Clear();
        columns = reportColumns.Select(column => column.Name).ToArray();
        elapsed.Restart();
        return Task.CompletedTask;
    }

    public Task WriteRowAsync(ReportRow row, CancellationToken cancellationToken)
    {
        if (rows.Count < MaximumVisibleRows)
        {
            rows.Add(row.Values.Select(FormatValue).ToArray());
        }

        return Task.CompletedTask;
    }

    public Task CompleteAsync(long rowCount, TimeSpan queryElapsed, CancellationToken cancellationToken)
    {
        elapsed.Stop();
        var duration = queryElapsed == TimeSpan.Zero ? elapsed.Elapsed : queryElapsed;
        Result = new ReportResult(columns, rows.ToArray(), rowCount, duration, rowCount > MaximumVisibleRows);
        return Task.CompletedTask;
    }

    private static string? FormatValue(object? value) => value is null or DBNull
        ? null
        : Convert.ToString(value, CultureInfo.InvariantCulture);
}