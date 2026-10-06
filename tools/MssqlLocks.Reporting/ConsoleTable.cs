namespace MssqlLocks.Reporting;

public sealed class ConsoleTable : IReportResultConsumer
{
    private const int MaxCellWidth = 32;
    private int[] widths = [];

    public Task BeginAsync(IReadOnlyList<ReportColumn> columns, CancellationToken cancellationToken)
    {
        widths = columns
            .Select(column => Math.Min(column.Name.Length, MaxCellWidth))
            .ToArray();
        Console.WriteLine(string.Join(" | ", columns.Select((column, index) => Fit(column.Name, widths[index]))));
        Console.WriteLine(string.Join("-+-", widths.Select(width => new string('-', width))));
        return Task.CompletedTask;
    }

    public Task WriteRowAsync(ReportRow row, CancellationToken cancellationToken)
    {
        var values = row.Values
            .Select((value, index) => Fit(FormatValue(value), widths[index]));
        Console.WriteLine(string.Join(" | ", values));
        return Task.CompletedTask;
    }

    public Task CompleteAsync(long rowCount, TimeSpan elapsed, CancellationToken cancellationToken)
    {
        Console.WriteLine($"Rows: {rowCount}; Elapsed: {elapsed.TotalMilliseconds:N0} ms");
        return Task.CompletedTask;
    }

    private static string FormatValue(object? value)
    {
        if (value is null or DBNull)
        {
            return "NULL";
        }

        return (Convert.ToString(value) ?? string.Empty)
            .Replace('\r', ' ').Replace('\n', ' ').Replace('\t', ' ');
    }

    private static string Fit(string value, int width)
    {
        value = value.Length <= 160 ? value : value[..157] + "...";
        value = value.Length <= width ? value : value[..Math.Max(0, width - 3)] + "...";
        return value.PadRight(width);
    }
}
