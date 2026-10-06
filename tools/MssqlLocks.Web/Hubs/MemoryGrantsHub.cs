using Microsoft.AspNetCore.SignalR;
using MssqlLocks.Web.Services;

namespace MssqlLocks.Web.Hubs;

public interface IMemoryGrantsWatchClient
{
    Task SnapshotReceived(MemoryGrantSnapshot snapshot);
    Task StatusChanged(WatchStatus status);
}

public sealed class MemoryGrantsHub : Hub<IMemoryGrantsWatchClient>
{
}