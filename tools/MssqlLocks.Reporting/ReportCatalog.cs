namespace MssqlLocks.Reporting;

public sealed class ReportCatalog
{
    private readonly IReadOnlyList<ReportDefinition> reports;

    private ReportCatalog(string repositoryRoot, string category, IReadOnlyList<ReportDefinition> reports)
    {
        RepositoryRoot = repositoryRoot;
        Category = category;
        this.reports = reports;
    }

    public string RepositoryRoot { get; }
    public string Category { get; }

    public static ReportCatalog Discover(string startingDirectory, string category)
    {
        var directory = new DirectoryInfo(Path.GetFullPath(startingDirectory));
        while (directory is not null)
        {
            var categoryPath = Path.Combine(directory.FullName, "raw-sqls", category);
            if (Directory.Exists(categoryPath))
            {
                var discovered = Directory.EnumerateFiles(categoryPath, "*.sql", SearchOption.TopDirectoryOnly)
                    .Select(reportPath => new ReportDefinition(
                        category,
                        Path.GetFileName(reportPath),
                        $"raw-sqls/{category}/{Path.GetFileName(reportPath)}",
                        reportPath))
                    .OrderBy(report => report.FileName, StringComparer.OrdinalIgnoreCase)
                    .ToArray();

                if (discovered.Length == 0)
                {
                    throw new InvalidOperationException($"No SQL reports were found under raw-sqls/{category}/.");
                }

                return new ReportCatalog(directory.FullName, category, discovered);
            }

            directory = directory.Parent;
        }

        throw new InvalidOperationException($"Could not locate the repository directory raw-sqls/{category}/.");
    }

    public ReportDefinition Resolve(string[] args)
    {
        if (args.Length != 1)
        {
            throw new InvalidOperationException($"Usage: dotnet run --project tools/MssqlLocks.{Category} -- <report.sql>");
        }

        var input = args[0].Trim();
        if (Path.IsPathRooted(input) || input.Contains("..", StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Report path must be relative and must not contain '..'.");
        }

        var normalized = input.Replace('\\', '/');
        var categoryPrefix = $"{Category}/";
        var rawPrefix = $"raw-sqls/{Category}/";
        if (normalized.StartsWith(rawPrefix, StringComparison.OrdinalIgnoreCase))
        {
            normalized = normalized[rawPrefix.Length..];
        }
        else if (normalized.StartsWith(categoryPrefix, StringComparison.OrdinalIgnoreCase))
        {
            normalized = normalized[categoryPrefix.Length..];
        }

        var report = reports.FirstOrDefault(candidate =>
            string.Equals(candidate.FileName, normalized, StringComparison.OrdinalIgnoreCase));
        return report ?? throw new InvalidOperationException($"Unknown {Category} report '{input}'. Use --help to list reports.");
    }

    public void PrintHelp(string projectPath)
    {
        Console.WriteLine("Usage:");
        Console.WriteLine($"  dotnet run --project {projectPath} -- <report.sql>");
        Console.WriteLine();
        Console.WriteLine("No argument, --help, or -h displays this help without database access.");
        Console.WriteLine($"Reports in raw-sqls/{Category}/:");
        foreach (var report in reports)
        {
            Console.WriteLine($"  {report.FileName}");
        }
    }
}
