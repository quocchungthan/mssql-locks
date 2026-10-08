using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Data.SqlClient;
using MiniProject.Migrations;

namespace MiniProject.Playground;

internal static class Program
{
    public static void Main(string[] args)
    {
        using var host = CreateHostBuilder(args).Build();
        using var scope = host.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<MiniDbContext>();

        Console.WriteLine($"EF Core provider: {dbContext.Database.ProviderName}");
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((_, configuration) =>
            {
                configuration.AddJsonFile(
                    Path.Combine(AppContext.BaseDirectory, "appsettings.json"),
                    optional: false,
                    reloadOnChange: false);

            })
            .ConfigureServices((context, services) =>
            {
                var connectionString = context.Configuration.GetConnectionString("Default");
                if (string.IsNullOrWhiteSpace(connectionString))
                {
                    throw new InvalidOperationException(
                        "Set ConnectionStrings:Default in appsettings.json.");
                }

                var saPassword = Environment.GetEnvironmentVariable("SA_PASSWORD");
                if (saPassword is null)
                {
                    ReadDotEnvValues().TryGetValue("SA_PASSWORD", out saPassword);
                }

                if (string.IsNullOrWhiteSpace(saPassword))
                {
                    throw new InvalidOperationException(
                        "Set SA_PASSWORD in the root .env file or process environment.");
                }

                var builder = new SqlConnectionStringBuilder(connectionString)
                {
                    Password = saPassword
                };
                connectionString = builder.ConnectionString;

                services.AddDbContext<MiniDbContext>(
                    options => options.UseSqlServer(connectionString));
            });

    private static Dictionary<string, string> ReadDotEnvValues()
    {
        var root = FindRepositoryRoot();
        var envFile = Path.Combine(root, ".env");
        if (!File.Exists(envFile))
        {
            return new Dictionary<string, string>(StringComparer.Ordinal);
        }

        var values = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var line in File.ReadLines(envFile))
        {
            var trimmedLine = line.Trim();
            if (trimmedLine.StartsWith("export ", StringComparison.Ordinal))
            {
                trimmedLine = trimmedLine["export ".Length..].TrimStart();
            }

            var separator = trimmedLine.IndexOf('=');
            if (separator < 0)
            {
                continue;
            }

            var key = trimmedLine[..separator].Trim();
            if (!string.Equals(key, "SA_PASSWORD", StringComparison.Ordinal))
            {
                continue;
            }

            var value = trimmedLine[(separator + 1)..].Trim();
            if (value.Length >= 2 &&
                ((value[0] == '"' && value[^1] == '"') || (value[0] == '\'' && value[^1] == '\'')))
            {
                value = value[1..^1];
            }

            values[key] = value;
        }

        return values;
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "MiniProject.slnx")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        return Directory.GetCurrentDirectory();
    }
}
