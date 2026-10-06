using Microsoft.AspNetCore.SignalR;
using Microsoft.Data.SqlClient;
using MssqlLocks.Reporting;
using MssqlLocks.Web.Hubs;

namespace MssqlLocks.Web.Services;

public sealed class MemoryGrantsWatchService(
    IReportApplicationService reportApplication,
    ReportCatalog catalog,
    MemoryGrantsWatchState watchState,
    IHubContext<MemoryGrantsHub, IMemoryGrantsWatchClient> hub,
    ILogger<MemoryGrantsWatchService> logger) : BackgroundService
{
    private const string ReportFileName = "memory-grants-watch.sql";
    private const string ConfigurationError = "Connection configuration is unavailable.";
    private readonly ReportDefinition report = catalog.Resolve([ReportFileName]);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var observation = watchState.Read();
            if (!observation.Status.IsRunning)
            {
                await observation.Changed.WaitAsync(stoppingToken);
                continue;
            }

            await hub.Clients.All.StatusChanged(watchState.MarkSampling());
            using var queryCancellation = CancellationTokenSource.CreateLinkedTokenSource(
                stoppingToken,
                observation.WatchCancellation);
            try
            {
                var connectionString = DotEnvConnection.Load(catalog.RepositoryRoot);
                var consumer = new MemoryGrantsSnapshotConsumer();
                await reportApplication.RunAsync(
                    catalog.Pack,
                    report,
                    connectionString,
                    consumer,
                    queryCancellation.Token);

                if (consumer.Snapshot is { } latestSnapshot)
                {
                    if (watchState.TryMarkLive(latestSnapshot, out var status))
                    {
                        await hub.Clients.All.SnapshotReceived(latestSnapshot);
                        await hub.Clients.All.StatusChanged(status);
                    }
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (OperationCanceledException)
            {
                continue;
            }
            catch (SqlException exception)
            {
                logger.LogWarning("A report query failed with SQL error {SqlErrorNumber}.", exception.Number);
                await hub.Clients.All.StatusChanged(watchState.MarkError($"Query failed (SQL error {exception.Number})."));
            }
            catch (InvalidOperationException)
            {
                await hub.Clients.All.StatusChanged(watchState.MarkError(ConfigurationError));
            }
            catch (Exception)
            {
                logger.LogWarning("A report sample failed. Sensitive exception details are suppressed.");
                await hub.Clients.All.StatusChanged(watchState.MarkError("Sample failed. Check server configuration."));
            }

            var next = watchState.Read();
            if (next.Status.IsRunning)
            {
                var delay = Task.Delay(TimeSpan.FromSeconds(next.Status.IntervalSeconds), stoppingToken);
                await Task.WhenAny(delay, next.Changed.WaitAsync(stoppingToken));
            }
        }
    }
}