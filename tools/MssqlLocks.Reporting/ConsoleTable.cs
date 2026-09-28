using System.Data.Common;
using System.Diagnostics;

namespace MssqlLocks.Reporting;

public static class ConsoleTable
{
    private const int MaxCellWidth = 32;

    public static async Task RenderAsync(DbDataReader reader, CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        var columnCount = reader.FieldCount;
        var widths = Enumerable.Range(0, columnCount)
            .Select(index => Math.Min(reader.GetName(index).Length, MaxCellWidth))
            .ToArray();
        Console.WriteLine(string.Join(" | ", Enumerable.Range(0, columnCount).Select(index => Fit(reader.GetName(index), widths[index]))));
        Console.WriteLine(string.Join("-+-", widths.Select(width => new string('-', width))));

        var rowCount = 0;
        while (await reader.ReadAsync(cancellationToken))
        {
            var values = Enumerable.Range(0, columnCount)
                .Select(index => Fit(FormatValue(reader.GetValue(index)), widths[index]));
            Console.WriteLine(string.Join(" | ", values));
            rowCount++;
        }

        stopwatch.Stop();
        Console.WriteLine($"Rows: {rowCount}; Elapsed: {stopwatch.Elapsed.TotalMilliseconds:N0} ms");
    }

    private static string FormatValue(object value)
    {
        if (value is DBNull)
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
