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
        Assert.Contains("Memory grants", html, StringComparison.Ordinal);
        Assert.Contains("Watch is stopped", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task SignalRHubNegotiatesFromTheWebHost()
    {
        using var client = factory.CreateClient();

        var response = await client.PostAsync("/hubs/memory-grants/negotiate?negotiateVersion=1", null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}