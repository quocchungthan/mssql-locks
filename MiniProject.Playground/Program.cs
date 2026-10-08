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
        app.MapGet("/project/{slug}", () =>
            Results.File(profilePagePath, "text/html; charset=utf-8"));
        app.MapGet("/search", () =>
            Results.File(profilePagePath, "text/html; charset=utf-8"));

        app.MapGet("/health", async (
            IDatabaseHealthService healthService,
            CancellationToken cancellationToken) =>
            await healthService.CanConnectAsync(cancellationToken)
                ? Results.Ok(new { status = "Healthy", database = "Connected" })
                : Results.StatusCode(StatusCodes.Status503ServiceUnavailable));

        app.MapGet("/api/portfolios/{profileId:int}/avatar", GetProfileAvatarAsync);
        app.MapGet("/api/portfolios/{profileId:int}", GetPortfolioAsync);
        app.MapGet("/api/projects/{slug}", GetProjectAsync);
        app.MapGet("/api/search", SearchAsync);

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

    private static async Task<IResult> GetProjectAsync(
        string slug,
        IPortfolioService portfolioService,
        CancellationToken cancellationToken)
    {
        var project = await portfolioService.GetProjectBySlugAsync(slug, cancellationToken);

        return project is null ? Results.NotFound() : Results.Ok(project);
    }

    private static async Task<IResult> GetProfileAvatarAsync(
        int profileId,
        IPortfolioService portfolioService,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var avatarSvg = await portfolioService.GetAvatarSvgAsync(profileId, cancellationToken);
        if (avatarSvg is null)
        {
            return Results.NotFound();
        }

        httpContext.Response.Headers.CacheControl = "public,max-age=31536000,immutable";
        return Results.Content(avatarSvg, "image/svg+xml; charset=utf-8");
    }

    private static async Task<IResult> SearchAsync(
        string? q,
        string? types,
        IPortfolioService portfolioService,
        CancellationToken cancellationToken)
    {
        var selectedTypes = string.IsNullOrWhiteSpace(types)
            ? new[] { "profiles", "projects", "skills", "links" }
            : types.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        var allowedTypes = new HashSet<string>(
            ["profiles", "projects", "skills", "links"],
            StringComparer.OrdinalIgnoreCase);

        if (selectedTypes.Length == 0 || selectedTypes.Any(type => !allowedTypes.Contains(type)))
        {
            return Results.BadRequest(new
            {
                error = "Types must be selected from profiles, projects, skills, links."
            });
        }

        if (q?.Trim().Length > 100)
        {
            return Results.BadRequest(new { error = "Search query must be 100 characters or fewer." });
        }

        var results = await portfolioService.SearchAsync(q, selectedTypes, cancellationToken);

        return Results.Ok(new
        {
            query = q?.Trim() ?? string.Empty,
            types = selectedTypes,
            total = results.Count,
            results
        });
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
