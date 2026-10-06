using MssqlLocks.Web.Services;

namespace MssqlLocks.Web.Models;

public sealed record WatchDashboardViewModel(
	WatchStatus MemoryGrantsStatus,
	MemoryGrantSnapshot? MemoryGrantsSnapshot,
	WatchStatus CapacityStatus,
	CapacitySnapshot? CapacitySnapshot,
	IReadOnlyList<ReportCategoryOption> Categories,
	string SelectedCategory,
	string SelectedReport,
	ReportResult? ReportResult,
	string? ReportError);