using MssqlLocks.Web.Services;

namespace MssqlLocks.Web.Models;

public sealed record WatchDashboardViewModel(WatchStatus Status, MemoryGrantSnapshot? Snapshot);