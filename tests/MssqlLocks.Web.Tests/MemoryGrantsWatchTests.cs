using MssqlLocks.Reporting;
using MssqlLocks.Web.Services;

namespace MssqlLocks.Web.Tests;

public sealed class MemoryGrantsWatchTests
{
    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(10)]
    [InlineData(15)]
    [InlineData(30)]
    [InlineData(60)]
    [InlineData(300)]
    public void StartAcceptsSupportedIntervals(int intervalSeconds)
    {
        var state = new MemoryGrantsWatchState();

        var status = state.Start(intervalSeconds);

        Assert.True(status.IsRunning);
        Assert.Equal(intervalSeconds, status.IntervalSeconds);
        Assert.Equal("Starting", status.State);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void StartRejectsUnsupportedIntervals(int intervalSeconds)
    {
        var state = new MemoryGrantsWatchState();

        Assert.Throws<ArgumentOutOfRangeException>(() => state.Start(intervalSeconds));
    }

    [Fact]
    public void StopPreventsAnInFlightSampleFromBecomingLive()
    {
        var state = new MemoryGrantsWatchState();
        state.Start(5);
        state.Stop();
        var snapshot = new MemoryGrantSnapshot(
            DateTimeOffset.UtcNow,
            [],
            [],
            0,
            0,
            0,
            false);

        var accepted = state.TryMarkLive(snapshot, out var status);

        Assert.False(accepted);
        Assert.False(status.IsRunning);
        Assert.Null(state.Read().Snapshot);
    }

    [Fact]
    public void StopCancelsTheCurrentWatchToken()
    {
        var state = new MemoryGrantsWatchState();
        state.Start(5);
        var watchCancellation = state.Read().WatchCancellation;

        state.Stop();

        Assert.True(watchCancellation.IsCancellationRequested);
    }

    [Fact]
    public void CapacityMonitorStartsAtTheCliRefreshCadenceAndStops()
    {
        var state = new CapacityWatchState();

        var runningStatus = state.Start();
        var watchCancellation = state.Read().WatchCancellation;
        var stoppedStatus = state.Stop();

        Assert.True(runningStatus.IsRunning);
        Assert.Equal(1, runningStatus.IntervalSeconds);
        Assert.True(watchCancellation.IsCancellationRequested);
        Assert.False(stoppedStatus.IsRunning);
    }

    [Fact]
    public async Task SnapshotConsumerOmitsIdentityAndQueryTextColumns()
    {
        var consumer = new MemoryGrantsSnapshotConsumer();
        var columns = new ReportColumn[]
        {
            new("session_id"),
            new("host_name"),
            new("grant_state"),
            new("requested_memory_kb"),
            new("query_text_start"),
        };

        await consumer.BeginAsync(columns, CancellationToken.None);
        await consumer.WriteRowAsync(new ReportRow(new object?[]
        {
            37,
            "private-host",
            "WAITING",
            4096,
            "select private data",
        }), CancellationToken.None);
        await consumer.CompleteAsync(1, TimeSpan.Zero, CancellationToken.None);

        var snapshot = Assert.IsType<MemoryGrantSnapshot>(consumer.Snapshot);
        Assert.Equal(new[] { "grant_state", "requested_memory_kb" }, snapshot.Columns);
        Assert.Equal(new string?[] { "WAITING", "4096" }, snapshot.Rows[0]);
        Assert.Equal(1, snapshot.WaitingCount);
        Assert.Equal(0, snapshot.GrantedCount);
    }
}