using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MssqlLocks.Observer.Models;

namespace MssqlLocks.Observer.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        var model = new MonitoringDashboardViewModel(
            ApplicationName: "MssqlLocks.Observer",
            CollectionMode: "Read-only foundation",
            DataSourceStatus: "Not connected",
            Capabilities:
            [
                new(
                    "Live blocking snapshot",
                    "SQL Server DMVs",
                    "Planned",
                    "Read active requests, waits, blockers, and blocked sessions."),
                new(
                    "Deadlock and blocked-process events",
                    "Extended Events",
                    "Planned",
                    "Ingest exported event XML without changing the production server."),
                new(
                    "Query performance history",
                    "Query Store",
                    "Planned",
                    "Compare query duration, CPU, reads, and execution frequency."),
                new(
                    "Application and pool signals",
                    ".NET logs and SqlClient counters",
                    "Planned",
                    "Correlate request activity and connection-pool pressure with SQL evidence.")
            ]);

        return View(model);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
