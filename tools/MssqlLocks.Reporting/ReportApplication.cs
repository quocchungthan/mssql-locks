using Microsoft.Data.SqlClient;

namespace MssqlLocks.Reporting;

public static class ReportApplication
{
    public static async Task<int> RunAsync(string[] args, string category, string projectPath)
    {
        using var cancellationTokenSource = new CancellationTokenSource();
        ConsoleCancelEventHandler cancelHandler = (_, eventArgs) =>
        {
            eventArgs.Cancel = true;
            cancellationTokenSource.Cancel();
        };
        Console.CancelKeyPress += cancelHandler;

        try
        {
            var catalog = ReportCatalog.Discover(Directory.GetCurrentDirectory(), category);
            if (args.Length == 0 || (args.Length == 1 && (args[0] == "--help" || args[0] == "-h")))
            {
                catalog.PrintHelp(projectPath);
                return 0;
            }

            var report = catalog.Resolve(args);
            var connectionString = DotEnvConnection.Load(catalog.RepositoryRoot);
            var runner = new SqlReportRunner();
            Console.WriteLine($"Report: {report.RelativePath}");
            await runner.RunAsync(report, connectionString, ConsoleTable.RenderAsync, cancellationTokenSource.Token);
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
    }
}
