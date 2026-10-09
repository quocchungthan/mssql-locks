using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using MssqlLocks.Web.Hubs;
using MssqlLocks.Web.Services;

namespace MssqlLocks.Web.Controllers;

public sealed class WatchController(
    MemoryGrantsWatchState watchState,
    CapacityWatchState capacityWatchState,
    IMemoryGrantsHistoryStore historyStore,
    IHubContext<MemoryGrantsHub, IMemoryGrantsWatchClient> hub,
    IHubContext<CapacityHub, ICapacityWatchClient> capacityHub) : Controller
{
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Start(int intervalSeconds)
    {
        if (intervalSeconds < 1)
        {
            return BadRequest();
        }

        WatchStatus status;
        try
        {
            status = watchState.Start(intervalSeconds);
        }
        catch (InvalidOperationException)
        {
            return Conflict();
        }
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

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> StartCapacity()
    {
        var status = capacityWatchState.Start();
        await capacityHub.Clients.All.StatusChanged(status);
        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> StopCapacity()
    {
        var status = capacityWatchState.Stop();
        await capacityHub.Clients.All.StatusChanged(status);
        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ClearMemoryGrantsHistory(CancellationToken cancellationToken)
    {
        if (!watchState.TryBeginHistoryClear())
        {
            return Conflict();
        }

        try
        {
            await historyStore.ClearAsync(cancellationToken);
            await hub.Clients.All.HistoryCleared();
            return RedirectToAction("Index", "Home");
        }
        finally
        {
            watchState.EndHistoryClear();
        }
    }
}