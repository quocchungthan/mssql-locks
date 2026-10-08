using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MiniProject.ComplexLogicInMiddle;
using MiniProject.Migrations;

namespace MiniProject.Playground;

internal static class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            Args = args,
            WebRootPath = Path.Combine(AppContext.BaseDirectory, "wwwroot")
        });
        builder.Configuration.AddJsonFile(
            Path.Combine(AppContext.BaseDirectory, "appsettings.json"),
            optional: false,
            reloadOnChange: false);

        var connectionString = builder.Configuration.GetConnectionString("Default");
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

        var sqlConnectionString = new SqlConnectionStringBuilder(connectionString)
        {
            Password = saPassword
        }.ConnectionString;

        builder.Services.AddDbContext<MiniDbContext>(
            options => options
                .UseSqlServer(sqlConnectionString)
                .LogTo(
                    Console.WriteLine,
                    new[] { DbLoggerCategory.Database.Command.Name },
                    LogLevel.Information));
        builder.Services.AddScoped<IPortfolioService, PortfolioService>();
        builder.Services.AddScoped<IDatabaseHealthService, DatabaseHealthService>();
        builder.Services.AddHostedService<InteractiveConsoleService>();

        var app = builder.Build();

        app.UseDefaultFiles();
        app.UseStaticFiles();

        var profilePagePath = Path.Combine(app.Environment.WebRootPath, "index.html");
        app.MapGet("/profile/{profileKey}", () =>
            Results.File(profilePagePath, "text/html; charset=utf-8"));

        app.MapGet("/health", async (
            IDatabaseHealthService healthService,
            CancellationToken cancellationToken) =>
            await healthService.CanConnectAsync(cancellationToken)
                ? Results.Ok(new { status = "Healthy", database = "Connected" })
                : Results.StatusCode(StatusCodes.Status503ServiceUnavailable));

        app.MapGet("/api/portfolios/{profileId:int}", GetPortfolioAsync);

        await app.RunAsync();
    }

    private static async Task<IResult> GetPortfolioAsync(
        int profileId,
        IPortfolioService portfolioService,
        CancellationToken cancellationToken)
    {
        var profile = await portfolioService.GetByProfileIdAsync(profileId, cancellationToken);

        if (profile is null)
        {
            return Results.NotFound();
        }

        return Results.Ok(profile);
    }

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
