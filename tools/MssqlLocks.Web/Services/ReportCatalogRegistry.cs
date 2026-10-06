using MssqlLocks.Reporting;

namespace MssqlLocks.Web.Services;

public sealed class ReportCatalogRegistry
{
    private static readonly (string Category, string Label)[] SupportedCategories =
    [
        ("dmv", "DMV"),
        ("query-store", "Query Store"),
        ("xe", "Extended Events"),
    ];

    private readonly IReadOnlyDictionary<string, ReportCatalog> catalogs;

    public ReportCatalogRegistry(string startingDirectory)
    {
        var discovered = SupportedCategories
            .Where(category => Directory.Exists(FindCategoryPath(startingDirectory, category.Category)))
            .Select(category => ReportCatalog.Discover(startingDirectory, category.Category))
            .ToDictionary(catalog => catalog.Category, StringComparer.OrdinalIgnoreCase);
        catalogs = discovered;
        Categories = SupportedCategories
            .Where(category => catalogs.ContainsKey(category.Category))
            .Select(category => new ReportCategoryOption(
                category.Category,
                category.Label,
                catalogs[category.Category].Pack.Reports))
            .ToArray();
    }

    public IReadOnlyList<ReportCategoryOption> Categories { get; }

    public bool TryGet(string category, out ReportCatalog catalog) => catalogs.TryGetValue(category, out catalog!);

    private static string FindCategoryPath(string startingDirectory, string category)
    {
        var directory = new DirectoryInfo(Path.GetFullPath(startingDirectory));
        while (directory is not null)
        {
            var categoryPath = Path.Combine(directory.FullName, "raw-sqls", category);
            if (Directory.Exists(categoryPath))
            {
                return categoryPath;
            }

            directory = directory.Parent;
        }

        return string.Empty;
    }
}

public sealed record ReportCategoryOption(
    string Name,
    string Label,
    IReadOnlyList<ReportDefinition> Reports);