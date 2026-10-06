using System.Data;
using Microsoft.Data.SqlClient;
using MssqlLocks.Reporting;

const string reportName = "current-capacity-counts.sql";
var cancellationTokenSource = new CancellationTokenSource();
ConsoleCancelEventHandler cancelHandler = (_, eventArgs) =>
{
    eventArgs.Cancel = true;
    cancellationTokenSource.Cancel();
};
Console.CancelKeyPress += cancelHandler;

try
{
    if (args.Length == 0 || (args.Length == 1 && (args[0] == "--help" || args[0] == "-h")))
    {
        Console.WriteLine("Usage:");
        Console.WriteLine($"  dotnet run --project tools/MssqlLocks.Monitor -- {reportName}");
        Console.WriteLine();
        Console.WriteLine("Refreshes the live capacity dashboard every second until Ctrl+C.");
        return 0;
    }

    if (args.Length != 1 || !string.Equals(args[0], reportName, StringComparison.OrdinalIgnoreCase))
    {
        Console.Error.WriteLine($"The monitor supports only '{reportName}'.");
        return 2;
    }

    var catalog = ReportCatalog.Discover(Directory.GetCurrentDirectory(), "dmv");
    var report = catalog.Resolve(args);
    var sql = await new FileReportSqlSource(catalog.RepositoryRoot)
        .ReadAsync(report, cancellationTokenSource.Token);
    var connectionString = DotEnvConnection.Load(catalog.RepositoryRoot);
    var connectionBuilder = new SqlConnectionStringBuilder(connectionString)
    {
        ApplicationIntent = ApplicationIntent.ReadOnly
    };

    await using var connection = new SqlConnection(connectionBuilder.ConnectionString);
    await connection.OpenAsync(cancellationTokenSource.Token);

    while (!cancellationTokenSource.IsCancellationRequested)
    {
        await using var command = new SqlCommand(sql, connection)
        {
            CommandTimeout = 10
        };
        await using var reader = await command.ExecuteReaderAsync(
            CommandBehavior.SingleRow,
            cancellationTokenSource.Token);

        if (!await reader.ReadAsync(cancellationTokenSource.Token))
        {
            throw new InvalidOperationException("The capacity report returned no row.");
        }

        var values = Enumerable.Range(0, reader.FieldCount)
            .ToDictionary(reader.GetName, reader.GetValue, StringComparer.OrdinalIgnoreCase);

        if (!Console.IsOutputRedirected)
        {
            Console.Clear();
        }

        Console.WriteLine($"SQL Server live capacity | {values["observed_at_utc"]:O} | refresh 1s | Ctrl+C to stop");
        Console.WriteLine();
        Console.WriteLine(
            $"Sessions     total {values["sessions_total"],5} | user {values["user_sessions"],5} | running {values["running_user_sessions"],5} | sleeping {values["sleeping_user_sessions"],5}");
        Console.WriteLine(
            $"Connections  total {values["connections_total"],5} | user {values["user_connections"],5}");
        Console.WriteLine(
            $"Requests     total {values["requests_total"],5} | user {values["user_requests"],5} | blocked {values["blocked_requests"],5} | running {values["running_requests"],5} | runnable {values["runnable_requests"],5} | suspended {values["suspended_requests"],5}");
        Console.WriteLine(
            $"Transactions active {values["active_transactions"],4} | session-owned {values["session_transactions"],4} | sessions {values["sessions_with_transactions"],4}");
        Console.WriteLine(
            $"Tasks        total {values["tasks_total"],5} | running {values["running_tasks"],5} | runnable {values["runnable_tasks"],5} | suspended {values["suspended_tasks"],5}");
        Console.WriteLine(
            $"Workers      total {values["workers_total"],5} | running {values["running_workers"],5} | runnable {values["runnable_workers"],5} | suspended {values["suspended_workers"],5}");
        Console.WriteLine(
            $"Schedulers   online {values["visible_online_schedulers"],4} | tasks {values["scheduler_current_tasks"],5} | workers {values["scheduler_active_workers"],5} | runnable {values["scheduler_runnable_tasks"],5} | queued {values["scheduler_work_queue"],5}");

        if (Console.IsOutputRedirected)
        {
            Console.WriteLine();
        }

        await Task.Delay(TimeSpan.FromSeconds(1), cancellationTokenSource.Token);
    }

    return 0;
}
catch (OperationCanceledException)
{
    return 0;
}
catch (SqlException exception)
{
    Console.Error.WriteLine($"Live capacity query failed (error {exception.Number}).");
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
    cancellationTokenSource.Dispose();
}
