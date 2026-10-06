using MssqlLocks.Reporting;
using MssqlLocks.Web.Hubs;
using MssqlLocks.Web.Services;

var builder = WebApplication.CreateBuilder(args);
var dmvCatalog = ReportCatalog.Discover(builder.Environment.ContentRootPath, "dmv");

builder.Services.AddControllersWithViews();
builder.Services.AddSignalR();
builder.Services.AddSingleton(dmvCatalog);
builder.Services.AddSingleton<IReportSqlSource>(new FileReportSqlSource(dmvCatalog.RepositoryRoot));
builder.Services.AddSingleton<IReportQueryExecutor, SqlReportRunner>();
builder.Services.AddSingleton<IReportApplicationService, ReportApplicationService>();
builder.Services.AddSingleton<MemoryGrantsWatchState>();
builder.Services.AddHostedService<MemoryGrantsWatchService>();

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

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();

public partial class Program
{
}
