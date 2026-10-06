namespace MssqlLocks.Reporting;

public sealed class ReportPack
{
    public ReportPack(string category, IEnumerable<ReportDefinition> reports)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(category);
        ArgumentNullException.ThrowIfNull(reports);

        if (category.Contains('/') || category.Contains('\\') || category is "." or "..")
        {
            throw new ArgumentException("Report-pack category must be a single path segment.", nameof(category));
        }

        var validatedReports = reports.ToArray();
        if (validatedReports.Length == 0)
        {
            throw new ArgumentException("A report pack must contain at least one report.", nameof(reports));
        }

        var fileNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var report in validatedReports)
        {
            if (!string.Equals(report.Category, category, StringComparison.OrdinalIgnoreCase)
                || string.IsNullOrWhiteSpace(report.FileName)
                || !string.Equals(Path.GetFileName(report.FileName), report.FileName, StringComparison.Ordinal)
                || !string.Equals(Path.GetExtension(report.FileName), ".sql", StringComparison.OrdinalIgnoreCase)
                || !string.Equals(report.RelativePath, $"raw-sqls/{category}/{report.FileName}", StringComparison.OrdinalIgnoreCase)
                || !fileNames.Add(report.FileName))
            {
                throw new ArgumentException("Report pack contains an invalid or duplicate report definition.", nameof(reports));
            }
        }

        Category = category;
        Reports = Array.AsReadOnly(validatedReports);
    }

    public string Category { get; }
    public IReadOnlyList<ReportDefinition> Reports { get; }
}