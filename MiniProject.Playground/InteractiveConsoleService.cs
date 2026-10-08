using Microsoft.Extensions.Hosting;

namespace MiniProject.Playground;

internal sealed class InteractiveConsoleService : BackgroundService
{
    private readonly IHostApplicationLifetime _applicationLifetime;

    public InteractiveConsoleService(IHostApplicationLifetime applicationLifetime)
    {
        _applicationLifetime = applicationLifetime;
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

            switch (input.Trim().ToLowerInvariant())
            {
                case "help":
                    Console.WriteLine("Commands: help, exit");
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
}
