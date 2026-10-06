namespace MssqlLocks.Web.Services;

public sealed class CapacityWatchState
{
    private readonly object gate = new();
    private WatchStatus status = WatchStatus.Stopped with { IntervalSeconds = 1 };
    private CapacitySnapshot? snapshot;
    private CancellationTokenSource? watchCancellation;
    private TaskCompletionSource changed = CreateSignal();

    public CapacityWatchObservation Read()
    {
        lock (gate)
        {
            return new CapacityWatchObservation(
                status,
                snapshot,
                changed.Task,
                watchCancellation?.Token ?? CancellationToken.None);
        }
    }

    public WatchStatus Start()
    {
        lock (gate)
        {
            if (!status.IsRunning)
            {
                watchCancellation = new CancellationTokenSource();
            }

            status = status with
            {
                IsRunning = true,
                IntervalSeconds = 1,
                State = "Starting",
                Message = "Starting the one-second capacity sample.",
            };
            SignalChanged();
            return status;
        }
    }

    public WatchStatus Stop()
    {
        CancellationTokenSource? cancellation;
        WatchStatus updatedStatus;
        lock (gate)
        {
            cancellation = watchCancellation;
            watchCancellation = null;
            status = status with
            {
                IsRunning = false,
                State = "Stopped",
                Message = "Capacity monitor stopped. The last sample remains available.",
            };
            SignalChanged();
            updatedStatus = status;
        }

        cancellation?.Cancel();
        cancellation?.Dispose();
        return updatedStatus;
    }

    public WatchStatus MarkSampling() => Update(current => current.IsRunning
        ? current with { State = "Sampling", Message = "Reading live capacity." }
        : current);

    public bool TryMarkLive(CapacitySnapshot latestSnapshot, out WatchStatus updatedStatus)
    {
        ArgumentNullException.ThrowIfNull(latestSnapshot);

        lock (gate)
        {
            if (!status.IsRunning)
            {
                updatedStatus = status;
                return false;
            }

            snapshot = latestSnapshot;
            status = status with
            {
                State = "Live",
                LastSampleAt = latestSnapshot.CapturedAt,
                Message = "Refreshing every second.",
            };
            SignalChanged();
            updatedStatus = status;
            return true;
        }
    }

    public WatchStatus MarkError(string message) => Update(current => current.IsRunning
        ? current with { State = "Error", Message = message }
        : current);

    private WatchStatus Update(Func<WatchStatus, WatchStatus> update)
    {
        lock (gate)
        {
            status = update(status);
            SignalChanged();
            return status;
        }
    }

    private void SignalChanged()
    {
        var previous = changed;
        changed = CreateSignal();
        previous.TrySetResult();
    }

    private static TaskCompletionSource CreateSignal() => new(TaskCreationOptions.RunContinuationsAsynchronously);
}