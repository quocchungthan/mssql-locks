using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using MssqlLocks.Web.Hubs;
using MssqlLocks.Web.Services;

namespace MssqlLocks.Web.Controllers;

public sealed class WatchController(
    MemoryGrantsWatchState watchState,
    IHubContext<MemoryGrantsHub, IMemoryGrantsWatchClient> hub) : Controller
{
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Start(int intervalSeconds)
    {
        if (intervalSeconds is not (5 or 10 or 30 or 60))
        {
            return BadRequest();
        }

        var status = watchState.Start(intervalSeconds);
        await hub.Clients.All.StatusChanged(status);
        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Stop()
    {
        var status = watchState.Stop();
        await hub.Clients.All.StatusChanged(status);
        return RedirectToAction("Index", "Home");
    }
}