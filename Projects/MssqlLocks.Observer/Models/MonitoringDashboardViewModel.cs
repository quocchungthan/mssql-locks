namespace MssqlLocks.Observer.Models;

public sealed record MonitoringDashboardViewModel(
    string ApplicationName,
    string CollectionMode,
    string DataSourceStatus,
    IReadOnlyList<MonitoringCapabilityViewModel> Capabilities);

public sealed record MonitoringCapabilityViewModel(
    string Name,
    string Source,
    string Status,
    string Description);
