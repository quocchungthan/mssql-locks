using MssqlLocks.Reporting;
using MssqlLocks.Web.Hubs;
using MssqlLocks.Web.Services;

var builder = WebApplication.CreateBuilder(args);
var reportCatalogs = new ReportCatalogRegistry(builder.Environment.ContentRootPath);
var dmvCatalog = reportCatalogs.Categories
    .First(category => string.Equals(category.Name, "dmv", StringComparison.OrdinalIgnoreCase));
var dmvReportCatalog = ReportCatalog.Discover(builder.Environment.ContentRootPath, dmvCatalog.Name);

builder.Services.AddControllersWithViews();
builder.Services.AddSignalR();
builder.Services.AddSingleton(reportCatalogs);
builder.Services.AddSingleton(dmvReportCatalog);
builder.Services.AddSingleton<IReportSqlSource>(new FileReportSqlSource(dmvReportCatalog.RepositoryRoot));
builder.Services.AddSingleton<IReportQueryExecutor, SqlReportRunner>();
builder.Services.AddSingleton<IReportApplicationService, ReportApplicationService>();
builder.Services.AddSingleton<IMemoryGrantsHistoryStore>(new JsonMemoryGrantsHistoryStore(
    Path.Combine(dmvReportCatalog.RepositoryRoot, "runtime", "memory-grants-history.jsonl")));
builder.Services.AddSingleton<MemoryGrantsWatchState>();
builder.Services.AddSingleton<CapacityWatchState>();
builder.Services.AddHostedService<MemoryGrantsWatchService>();
builder.Services.AddHostedService<CapacityWatchService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();
app.MapHub<MemoryGrantsHub>("/hubs/memory-grants");
app.MapHub<CapacityHub>("/hubs/capacity");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();

public partial class Program
{
}
