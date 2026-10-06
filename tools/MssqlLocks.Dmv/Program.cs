using System.Globalization;
using Microsoft.Data.SqlClient;
using MssqlLocks.Reporting;

using var cancellationTokenSource = new CancellationTokenSource();
ConsoleCancelEventHandler cancelHandler = (_, eventArgs) =>
{
    eventArgs.Cancel = true;
    cancellationTokenSource.Cancel();
};
Console.CancelKeyPress += cancelHandler;

try
{
    var catalog = ReportCatalog.Discover(Directory.GetCurrentDirectory(), "dmv");
    IReportApplicationService reportApplication = new ReportApplicationService(
        new FileReportSqlSource(catalog.RepositoryRoot),
        new SqlReportRunner());
    if (args.Length == 0 || (args.Length == 1 && (args[0] == "--help" || args[0] == "-h")))
    {
        catalog.PrintHelp("tools/MssqlLocks.Dmv");
        return 0;
    }

    if (args.Contains("--watch", StringComparer.Ordinal)
        || args.Contains("--interval-seconds", StringComparer.Ordinal))
    {
        var options = ParseWatchArguments(args);
        var watchReport = catalog.Resolve([options.ReportName]);
        if (!string.Equals(watchReport.FileName, "memory-grants.sql", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Watch mode supports only 'memory-grants.sql'.");
        }

        var watchConnectionString = DotEnvConnection.Load(catalog.RepositoryRoot);
        await RunWatchAsync(
            reportApplication,
            catalog.Pack,
            watchReport,
            watchConnectionString,
            options.IntervalSeconds,
            cancellationTokenSource.Token);
        return 0;
    }

    var report = catalog.Resolve(args);
    var connectionString = DotEnvConnection.Load(catalog.RepositoryRoot);
    Console.WriteLine($"Report: {report.RelativePath}");
    await reportApplication.RunAsync(
        catalog.Pack,
        report,
        connectionString,
        new ConsoleTable(),
        cancellationTokenSource.Token);
    return 0;
}
catch (OperationCanceledException)
{
    Console.Error.WriteLine("DMV report cancelled.");
    return 130;
}
catch (SqlException exception)
{
    Console.Error.WriteLine($"DMV query failed (error {exception.Number}).");
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

static (string ReportName, int IntervalSeconds) ParseWatchArguments(string[] commandLine)
{
    var watchSpecified = false;
    var intervalSpecified = false;
    var intervalSeconds = 2;
    string? reportName = null;

    for (var index = 0; index < commandLine.Length; index++)
    {
        var argument = commandLine[index];
        if (argument == "--watch")
        {
            if (watchSpecified)
            {
                throw new InvalidOperationException("Usage: <memory-grants.sql> --watch [--interval-seconds N].");
            }

            watchSpecified = true;
        }
        else if (argument == "--interval-seconds")
        {
            if (intervalSpecified || index + 1 >= commandLine.Length
                || !int.TryParse(commandLine[index + 1], NumberStyles.None, CultureInfo.InvariantCulture, out intervalSeconds)
                || intervalSeconds < 1)
            {
                throw new InvalidOperationException("The watch interval must be a positive number of seconds.");
            }

            intervalSpecified = true;
            index++;
        }
        else if (argument.StartsWith("--", StringComparison.Ordinal) || reportName is not null)
        {
            throw new InvalidOperationException("Usage: <memory-grants.sql> --watch [--interval-seconds N].");
        }
        else
        {
            reportName = argument;
        }
    }

    if (!watchSpecified || reportName is null)
    {
        throw new InvalidOperationException("Usage: <memory-grants.sql> --watch [--interval-seconds N].");
    }

    return (reportName, intervalSeconds);
}

static async Task RunWatchAsync(
    IReportApplicationService reportApplication,
    ReportPack reportPack,
    ReportDefinition report,
    string connectionString,
    int intervalSeconds,
    CancellationToken cancellationToken)
{
    while (!cancellationToken.IsCancellationRequested)
    {
        if (!Console.IsOutputRedirected)
        {
            Console.Clear();
        }

        Console.WriteLine(
            $"Memory grants | {DateTimeOffset.Now:yyyy-MM-dd HH:mm:ss zzz} | refresh {intervalSeconds}s | Ctrl+C to stop");

        try
        {
            await reportApplication.RunAsync(
                reportPack,
                report,
                connectionString,
                new ConsoleTable(),
                cancellationToken);
        }
        catch (SqlException exception)
        {
            Console.Error.WriteLine($"Memory-grants query failed (error {exception.Number}); retrying in {intervalSeconds}s.");
        }

        await Task.Delay(TimeSpan.FromSeconds(intervalSeconds), cancellationToken);
    }
}
