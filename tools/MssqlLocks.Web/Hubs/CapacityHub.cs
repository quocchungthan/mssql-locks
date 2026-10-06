using Microsoft.AspNetCore.SignalR;
using MssqlLocks.Web.Services;

namespace MssqlLocks.Web.Hubs;

public interface ICapacityWatchClient
{
    Task SnapshotReceived(CapacitySnapshot snapshot);
    Task StatusChanged(WatchStatus status);
}

public sealed class CapacityHub : Hub<ICapacityWatchClient>
{
}