using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MiniProject.ComplexLogicInMiddle;

namespace MiniProject.Playground;

internal sealed class InteractiveConsoleService : BackgroundService
{
    private const int DefaultBulkProfileCount = 100_000;

    private readonly IHostApplicationLifetime _applicationLifetime;
    private readonly IServiceScopeFactory _scopeFactory;

    public InteractiveConsoleService(
        IHostApplicationLifetime applicationLifetime,
        IServiceScopeFactory scopeFactory)
    {
        _applicationLifetime = applicationLifetime;
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (Console.IsInputRedirected)
        {
            return;
        }

        Console.WriteLine("Playground console is ready. Type 'help' for commands.");

        while (!stoppingToken.IsCancellationRequested)
        {
            Console.Write("playground> ");
            var input = await Console.In.ReadLineAsync(stoppingToken);
            if (input is null)
            {
                return;
            }

            var parts = input.Trim().ToLowerInvariant()
                .Split(' ', StringSplitOptions.RemoveEmptyEntries);
            switch (parts.FirstOrDefault() ?? string.Empty)
            {
                case "help":
                    Console.WriteLine("Commands:");
                    Console.WriteLine("  seed-bulk [count]  insert dirty talent data (default 100000 profiles)");
                    Console.WriteLine("  help, exit");
                    break;
                case "seed-bulk":
                    await SeedBulkAsync(parts.ElementAtOrDefault(1), stoppingToken);
                    break;
                case "exit":
                case "quit":
                    _applicationLifetime.StopApplication();
                    return;
                case "":
                    break;
                default:
                    Console.WriteLine("Unknown command. Type 'help' for available commands.");
                    break;
            }
        }
    }

    private async Task SeedBulkAsync(string? countArgument, CancellationToken stoppingToken)
    {
        var count = DefaultBulkProfileCount;
        if (countArgument is not null &&
            (!int.TryParse(countArgument, out count) || count < 1 || count > TalentDataGenerator.MaxProfilesPerRun))
        {
            Console.WriteLine($"Count must be between 1 and {TalentDataGenerator.MaxProfilesPerRun}.");
            return;
        }

        Console.WriteLine($"Generating {count:N0} profiles (8 skills, 4 experiences each); SQL logging muted...");
        var stopwatch = Stopwatch.StartNew();
        try
        {
            using var _ = EfCommandLogging.Suppress();
            await using var scope = _scopeFactory.CreateAsyncScope();
            var generator = scope.ServiceProvider.GetRequiredService<ITalentDataGenerator>();
            var inserted = await generator.GenerateAsync(
                count,
                progress => Console.WriteLine($"  ... {progress:N0}/{count:N0} profiles ({stopwatch.Elapsed.TotalSeconds:F1}s)"),
                stoppingToken);
            Console.WriteLine($"Inserted {inserted:N0} profiles in {stopwatch.Elapsed.TotalSeconds:F1}s.");
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            Console.WriteLine($"Bulk seed failed: {exception.GetBaseException().Message}");
        }
    }
}
