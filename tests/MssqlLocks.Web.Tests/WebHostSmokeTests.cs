using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;

namespace MssqlLocks.Web.Tests;

public sealed class WebHostSmokeTests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    [Fact]
    public async Task DashboardLoadsWithoutStartingDatabasePolling()
    {
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/");
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("DMV", html, StringComparison.Ordinal);
        Assert.Contains("Query Store", html, StringComparison.Ordinal);
        Assert.Contains("Extended Events", html, StringComparison.Ordinal);
        Assert.Contains("top-cpu-plans-costliest-operators-past-24-hours.sql", html, StringComparison.Ordinal);
        Assert.Contains("recent-deadlocks.sql", html, StringComparison.Ordinal);
        Assert.Contains("Watch is stopped", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task SignalRHubNegotiatesFromTheWebHost()
    {
        using var client = factory.CreateClient();

        var response = await client.PostAsync("/hubs/memory-grants/negotiate?negotiateVersion=1", null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task CapacityHubNegotiatesFromTheWebHost()
    {
        using var client = factory.CreateClient();

        var response = await client.PostAsync("/hubs/capacity/negotiate?negotiateVersion=1", null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}