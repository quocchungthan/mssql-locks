namespace MssqlLocks.Web.Services;

public sealed class MemoryGrantsWatchState
{
    private readonly object gate = new();
    private WatchStatus status = WatchStatus.Stopped;
    private MemoryGrantSnapshot? snapshot;
    private CancellationTokenSource? watchCancellation;
    private TaskCompletionSource changed = CreateSignal();
    private bool historyClearInProgress;
    private int activeHistoryWrites;

    public WatchObservation Read()
    {
        lock (gate)
        {
            return new WatchObservation(status, snapshot, changed.Task, watchCancellation?.Token ?? CancellationToken.None);
        }
    }

    public WatchStatus Start(int intervalSeconds)
    {
        if (intervalSeconds < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(intervalSeconds));
        }

        lock (gate)
        {
            if (historyClearInProgress)
            {
                throw new InvalidOperationException("History is being cleared. Start the watch again in a moment.");
            }

            if (!status.IsRunning)
            {
                watchCancellation = new CancellationTokenSource();
            }

            status = status with
            {
                IsRunning = true,
                IntervalSeconds = intervalSeconds,
                State = "Starting",
                Message = "Starting the next sample.",
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
                Message = "Watch stopped. The last sample remains available.",
            };
            SignalChanged();
            updatedStatus = status;
        }

        cancellation?.Cancel();
        cancellation?.Dispose();
        return updatedStatus;
    }

    public WatchStatus MarkSampling() => Update(current => current.IsRunning
        ? current with { State = "Sampling", Message = "Reading the latest sample." }
        : current);

    public bool TryMarkLive(MemoryGrantSnapshot latestSnapshot, out WatchStatus updatedStatus)
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
                Message = $"Refreshing every {status.IntervalSeconds} seconds.",
            };
            SignalChanged();
            updatedStatus = status;
            return true;
        }
    }

    public bool TryBeginHistoryClear()
    {
        lock (gate)
        {
            if (status.IsRunning || historyClearInProgress || activeHistoryWrites > 0)
            {
                return false;
            }

            historyClearInProgress = true;
            snapshot = null;
            SignalChanged();
            return true;
        }
    }

    public bool TryBeginHistoryWrite()
    {
        lock (gate)
        {
            if (!status.IsRunning || historyClearInProgress)
            {
                return false;
            }

            activeHistoryWrites++;
            return true;
        }
    }

    public void EndHistoryWrite()
    {
        lock (gate)
        {
            activeHistoryWrites--;
            SignalChanged();
        }
    }

    public void EndHistoryClear()
    {
        lock (gate)
        {
            historyClearInProgress = false;
            SignalChanged();
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