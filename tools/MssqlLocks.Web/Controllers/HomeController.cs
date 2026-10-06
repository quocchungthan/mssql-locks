using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using MssqlLocks.Reporting;
using MssqlLocks.Web.Models;
using MssqlLocks.Web.Services;

namespace MssqlLocks.Web.Controllers;

public class HomeController : Controller
{
    private readonly MemoryGrantsWatchState watchState;
    private readonly CapacityWatchState capacityWatchState;
    private readonly ReportCatalogRegistry reportCatalogs;
    private readonly IReportApplicationService reportApplication;

    public HomeController(
        MemoryGrantsWatchState watchState,
        CapacityWatchState capacityWatchState,
        ReportCatalogRegistry reportCatalogs,
        IReportApplicationService reportApplication)
    {
        this.watchState = watchState;
        this.capacityWatchState = capacityWatchState;
        this.reportCatalogs = reportCatalogs;
        this.reportApplication = reportApplication;
    }

    public IActionResult Index(string? category, string? reportName)
    {
        return View(CreateViewModel(category, reportName, null, null));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RunReport(string category, string reportName, CancellationToken cancellationToken)
    {
        if (!reportCatalogs.TryGet(category, out var catalog))
        {
            return BadRequest();
        }

        ReportDefinition report;
        try
        {
            report = catalog.Resolve([reportName]);
        }
        catch (InvalidOperationException)
        {
            return BadRequest();
        }

        try
        {
            var connectionString = DotEnvConnection.Load(catalog.RepositoryRoot);
            var consumer = new ReportTableConsumer();
            await reportApplication.RunAsync(
                catalog.Pack,
                report,
                connectionString,
                consumer,
                cancellationToken);
            return View("Index", CreateViewModel(category, reportName, consumer.Result, null));
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return View("Index", CreateViewModel(category, reportName, null, "Report execution was cancelled."));
        }
        catch (SqlException exception)
        {
            return View("Index", CreateViewModel(category, reportName, null, $"Query failed (SQL error {exception.Number})."));
        }
        catch (InvalidOperationException)
        {
            return View("Index", CreateViewModel(category, reportName, null, "Connection configuration is unavailable."));
        }
        catch (Exception)
        {
            return View("Index", CreateViewModel(category, reportName, null, "Report failed. Check server configuration."));
        }
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    private WatchDashboardViewModel CreateViewModel(
        string? category,
        string? reportName,
        ReportResult? reportResult,
        string? reportError)
    {
        var selectedCategory = reportCatalogs.TryGet(category ?? string.Empty, out var catalog)
            ? catalog.Category
            : reportCatalogs.Categories[0].Name;
        var categoryOption = reportCatalogs.Categories.First(option =>
            string.Equals(option.Name, selectedCategory, StringComparison.OrdinalIgnoreCase));
        var selectedReport = categoryOption.Reports
            .FirstOrDefault(report => string.Equals(report.FileName, reportName, StringComparison.OrdinalIgnoreCase))
            ?.FileName ?? categoryOption.Reports[0].FileName;
        var memoryGrants = watchState.Read();
        var capacity = capacityWatchState.Read();

        return new WatchDashboardViewModel(
            memoryGrants.Status,
            memoryGrants.Snapshot,
            capacity.Status,
            capacity.Snapshot,
            reportCatalogs.Categories,
            selectedCategory,
            selectedReport,
            reportResult,
            reportError);
    }
}
