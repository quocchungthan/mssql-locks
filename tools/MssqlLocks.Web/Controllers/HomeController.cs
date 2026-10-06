using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MssqlLocks.Web.Models;
using MssqlLocks.Web.Services;

namespace MssqlLocks.Web.Controllers;

public class HomeController : Controller
{
    private readonly MemoryGrantsWatchState watchState;

    public HomeController(MemoryGrantsWatchState watchState)
    {
        this.watchState = watchState;
    }

    public IActionResult Index()
    {
        var current = watchState.Read();
        return View(new WatchDashboardViewModel(current.Status, current.Snapshot));
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
