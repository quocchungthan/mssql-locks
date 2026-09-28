using Microsoft.Data.SqlClient;
using System.Data;
using System.Diagnostics;

const string RequiredVariable = "MSSQL_LOCKS_CONNECTION_STRING";
const string DefaultReport = "blocking-chains.sql";
var supportedReports = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
{
    "active-requests.sql",
    "blocking-chains.sql",
    "top-cached-query-costs.sql"
};

using var cancellationTokenSource = new CancellationTokenSource();
ConsoleCancelEventHandler cancelHandler = (_, eventArgs) =>
{
    eventArgs.Cancel = true;
    cancellationTokenSource.Cancel();
};
Console.CancelKeyPress += cancelHandler;

try
{
    var repositoryRoot = FindRepositoryRoot();
    var reportName = ResolveReportName(args, supportedReports);
    var reportPath = Path.Combine(repositoryRoot, "raw-sqls", "dmv", reportName);
    var connectionString = LoadSQLConnectionString(repositoryRoot);
    var sql = await File.ReadAllTextAsync(reportPath, cancellationTokenSource.Token);
    var builder = new SqlConnectionStringBuilder(connectionString)
    {
        ApplicationIntent = ApplicationIntent.ReadOnly
    };

    Console.WriteLine($"Report: {reportName}");
    var stopwatch = Stopwatch.StartNew();
    await using var connection = new SqlConnection(builder.ConnectionString);
    await connection.OpenAsync(cancellationTokenSource.Token);
    await using var command = new SqlCommand(sql, connection)
    {
        CommandTimeout = 120
    };
    await using var reader = await command.ExecuteReaderAsync(CommandBehavior.SequentialAccess, cancellationTokenSource.Token);

    var columnCount = reader.FieldCount;
    var widths = Enumerable.Range(0, columnCount)
        .Select(index => Math.Min(reader.GetName(index).Length, 32))
        .ToArray();
    Console.WriteLine(string.Join(" | ", Enumerable.Range(0, columnCount).Select(index => Fit(reader.GetName(index), widths[index]))));
    Console.WriteLine(string.Join("-+-", widths.Select(width => new string('-', width))));

    var rowCount = 0;
    while (await reader.ReadAsync(cancellationTokenSource.Token))
    {
        var values = new string[columnCount];
        for (var index = 0; index < columnCount; index++)
        {
            values[index] = FormatValue(reader.GetValue(index));
        }

        Console.WriteLine(string.Join(" | ", values.Select((value, index) => Fit(value, widths[index]))));
        rowCount++;
    }

    stopwatch.Stop();
    Console.WriteLine($"Rows: {rowCount}; Elapsed: {stopwatch.Elapsed.TotalMilliseconds:N0} ms");
    return 0;
}
catch (OperationCanceledException)
{
    Console.Error.WriteLine("Operation cancelled.");
    return 130;
}
catch (SqlException exception)
{
    Console.Error.WriteLine($"SQL query failed (error {exception.Number}).");
    return 1;
}
catch (FileNotFoundException exception)
{
    Console.Error.WriteLine($"Required file was not found: {Path.GetFileName(exception.FileName)}.");
    return 1;
}
catch (InvalidOperationException exception)
{
    Console.Error.WriteLine(exception.Message);
    return 2;
}
catch (ArgumentException)
{
    Console.Error.WriteLine("Invalid SQL Server connection configuration.");
    return 2;
}
finally
{
    Console.CancelKeyPress -= cancelHandler;
}

static string FindRepositoryRoot()
{
    var directory = new DirectoryInfo(Directory.GetCurrentDirectory());
    while (directory is not null)
    {
        if (File.Exists(Path.Combine(directory.FullName, ".env")) &&
            Directory.Exists(Path.Combine(directory.FullName, "raw-sqls", "dmv")))
        {
            return directory.FullName;
        }

        directory = directory.Parent;
    }

    throw new InvalidOperationException("Could not locate the repository root containing .env and raw-sqls/dmv.");
}

static string ResolveReportName(string[] args, IReadOnlySet<string> supportedReports)
{
    if (args.Length > 1)
    {
        throw new InvalidOperationException("Usage: MssqlLocks.Dmv [report filename or path]");
    }

    var input = args.Length == 0 ? DefaultReport : args[0];
    if (Path.IsPathRooted(input) || input.Contains("..", StringComparison.Ordinal))
    {
        throw new InvalidOperationException("Report path must stay within raw-sqls/dmv.");
    }

    var normalizedInput = input.Replace('/', Path.DirectorySeparatorChar);
    var reportName = Path.GetFileName(normalizedInput);
    if (!string.Equals(input, reportName, StringComparison.Ordinal) &&
        !string.Equals(normalizedInput, Path.Combine("raw-sqls", "dmv", reportName), StringComparison.OrdinalIgnoreCase))
    {
        throw new InvalidOperationException("Report path must be a report filename or raw-sqls/dmv/<filename>.");
    }

    if (!supportedReports.Contains(reportName))
    {
        throw new InvalidOperationException($"Unsupported report '{input}'. Supported reports: {string.Join(", ", supportedReports.Order())}.");
    }

    return reportName;
}

static string LoadRequiredSetting(string envPath, string variableName)
{
    foreach (var line in File.ReadLines(envPath))
    {
        var trimmed = line.Trim();
        if (trimmed.Length == 0 || trimmed.StartsWith('#'))
        {
            continue;
        }

        if (trimmed.StartsWith("export ", StringComparison.Ordinal))
        {
            trimmed = trimmed[7..].TrimStart();
        }

        var separator = trimmed.IndexOf('=');
        if (separator <= 0 || !string.Equals(trimmed[..separator].Trim(), variableName, StringComparison.Ordinal))
        {
            continue;
        }

        var value = trimmed[(separator + 1)..].Trim();
        if (value.Length >= 2 && value[0] == value[^1] && (value[0] == '\'' || value[0] == '"'))
        {
            value = value[1..^1];
        }
        else if (value.Length >= 2 && (value[^1] == '\'' || value[^1] == '"') && value[^2] == ';')
        {
            value = value[..^1];
        }

        if (!string.IsNullOrWhiteSpace(value))
        {
            return value;
        }

        break;
    }

    throw new InvalidOperationException($"Required .env variable '{variableName}' is missing or empty.");
}

static string LoadSQLConnectionString(string repositoryRoot)
{
    var connectionString = LoadRequiredSetting(Path.Combine(repositoryRoot, ".env"), RequiredVariable);
    Console.WriteLine($"Connection string: {Mask(connectionString)}");

    return connectionString;
}

static string Mask(string value)
{
    return value.Length > 8 ? $"{value[..4]}****{value[^4..]}" : "***";
}

static string FormatValue(object value)
{
    if (value is DBNull)
    {
        return "NULL";
    }

    var text = Convert.ToString(value) ?? string.Empty;
    return text.Replace('\r', ' ').Replace('\n', ' ').Replace('\t', ' ');
}

static string Fit(string value, int width)
{
    value = value.Length <= 160 ? value : value[..157] + "...";
    value = value.Length <= width ? value : value[..Math.Max(0, width - 3)] + "...";
    return value.PadRight(width);
}