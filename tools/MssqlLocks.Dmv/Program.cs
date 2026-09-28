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
    if (args.Length == 0 || (args.Length == 1 && (args[0] == "--help" || args[0] == "-h")))
    {
        catalog.PrintHelp("tools/MssqlLocks.Dmv");
        return 0;
    }

    var report = catalog.Resolve(args);
    var connectionString = DotEnvConnection.Load(catalog.RepositoryRoot);
    Console.WriteLine($"Report: {report.RelativePath}");
    await new SqlReportRunner().RunAsync(
        report,
        connectionString,
        ConsoleTable.RenderAsync,
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
