namespace MssqlLocks.Reporting;

public sealed class FileReportSqlSource : IReportSqlSource
{
    private readonly string repositoryRoot;

    public FileReportSqlSource(string repositoryRoot)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(repositoryRoot);
        this.repositoryRoot = Path.GetFullPath(repositoryRoot);
    }

    public Task<string> ReadAsync(ReportDefinition report, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(report);

        var reportPath = Path.GetFullPath(Path.Combine(
            repositoryRoot,
            report.RelativePath.Replace('/', Path.DirectorySeparatorChar)));
        var relativePath = Path.GetRelativePath(repositoryRoot, reportPath);
        if (Path.IsPathRooted(relativePath)
            || relativePath == ".."
            || relativePath.StartsWith($"..{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Report path must remain inside the selected report root.");
        }

        RejectReparsePoints(reportPath);
        return File.ReadAllTextAsync(reportPath, cancellationToken);
    }

    private void RejectReparsePoints(string reportPath)
    {
        var currentPath = repositoryRoot;
        RejectIfReparsePoint(currentPath);

        var relativePath = Path.GetRelativePath(repositoryRoot, reportPath);
        foreach (var segment in relativePath.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar))
        {
            currentPath = Path.Combine(currentPath, segment);
            RejectIfReparsePoint(currentPath);
        }
    }

    private static void RejectIfReparsePoint(string path)
    {
        if ((File.GetAttributes(path) & FileAttributes.ReparsePoint) != 0)
        {
            throw new InvalidOperationException("Report paths must not contain symbolic links or reparse points.");
        }
    }
}