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
            false,
            new MemoryGrantsHistoryPoint(DateTimeOffset.UtcNow, 0, 0, 0, 0, 0, 0, 0, 0, 0));

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
    public void HistoryCannotBeClearedWhileWatchingAndStartWaitsForClearToFinish()
    {
        var state = new MemoryGrantsWatchState();
        state.Start(5);

        Assert.False(state.TryBeginHistoryClear());
        state.Stop();
        Assert.True(state.TryBeginHistoryClear());
        Assert.Throws<InvalidOperationException>(() => state.Start(5));

        state.EndHistoryClear();

        Assert.True(state.Start(5).IsRunning);
    }

    [Fact]
    public void HistoryClearWaitsForAnInFlightPersistenceWrite()
    {
        var state = new MemoryGrantsWatchState();
        state.Start(5);
        Assert.True(state.TryBeginHistoryWrite());
        state.Stop();

        Assert.False(state.TryBeginHistoryClear());

        state.EndHistoryWrite();

        Assert.True(state.TryBeginHistoryClear());
        state.EndHistoryClear();
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
            new("record_type"),
            new("session_id"),
            new("host_name"),
            new("grant_state"),
            new("requested_memory_kb"),
            new("query_text_start"),
        };

        await consumer.BeginAsync(columns, CancellationToken.None);
        await consumer.WriteRowAsync(new ReportRow(new object?[]
        {
            "GRANT",
            37,
            "private-host",
            "WAITING",
            4096,
            "select private data",
        }), CancellationToken.None);
        await consumer.CompleteAsync(1, TimeSpan.Zero, CancellationToken.None);

        var snapshot = Assert.IsType<MemoryGrantSnapshot>(consumer.Snapshot);
        Assert.Equal(new[] { "record_type", "grant_state", "requested_memory_kb" }, snapshot.Columns);
        Assert.Equal(new string?[] { "GRANT", "WAITING", "4096" }, snapshot.Rows[0]);
        Assert.Equal(1, snapshot.WaitingCount);
        Assert.Equal(0, snapshot.GrantedCount);
    }

    [Fact]
    public async Task SnapshotConsumerBuildsHistoryAggregatesWithoutPersistingIdentifiers()
    {
        var consumer = new MemoryGrantsSnapshotConsumer();
        var columns = new ReportColumn[]
        {
            new("record_type"),
            new("snapshot_utc"),
            new("pool_id"),
            new("resource_semaphore_id"),
            new("session_id"),
            new("query_text"),
            new("grant_state"),
            new("requested_memory_kb"),
            new("wait_time_ms"),
            new("semaphore_available_memory_kb"),
            new("semaphore_target_memory_kb"),
            new("semaphore_waiter_count"),
        };
        var capturedAt = DateTimeOffset.Parse("2026-10-07T12:00:00+00:00");

        await consumer.BeginAsync(columns, CancellationToken.None);
        await consumer.WriteRowAsync(new ReportRow(new object?[]
        {
            "SEMAPHORE", capturedAt, 1, 0, null, null, null, null, null, 8192L, 16384L, 2,
        }), CancellationToken.None);
        await consumer.WriteRowAsync(new ReportRow(new object?[]
        {
            "GRANT", capturedAt, 1, 0, 42, "private sql", "WAITING", 4096L, 700L, null, null, null,
        }), CancellationToken.None);
        await consumer.WriteRowAsync(new ReportRow(new object?[]
        {
            "GRANT", capturedAt, 1, 0, 43, "other private sql", "GRANTED", 2048L, 0, null, null, null,
        }), CancellationToken.None);
        await consumer.CompleteAsync(3, TimeSpan.Zero, CancellationToken.None);

        var snapshot = Assert.IsType<MemoryGrantSnapshot>(consumer.Snapshot);
        Assert.Equal(capturedAt, snapshot.HistoryPoint.CapturedAt);
        Assert.Equal(8192, snapshot.HistoryPoint.AvailableMemoryKb);
        Assert.Equal(16384, snapshot.HistoryPoint.TargetMemoryKb);
        Assert.Equal(6144, snapshot.HistoryPoint.WaitingRequestedMemoryKb + snapshot.HistoryPoint.GrantedRequestedMemoryKb);
        Assert.Equal(700, snapshot.HistoryPoint.MaximumWaitTimeMs);
        Assert.Equal(2, snapshot.HistoryPoint.WaiterCount);
        Assert.Equal(1, snapshot.HistoryPoint.WaitingGrantObservations);
        Assert.Equal(1, snapshot.HistoryPoint.GrantedGrantObservations);
        Assert.DoesNotContain("session_id", snapshot.Columns);
        Assert.DoesNotContain("query_text", snapshot.Columns);
        Assert.DoesNotContain(snapshot.Rows.SelectMany(row => row), value => value?.Contains("private sql", StringComparison.Ordinal) == true);
    }
}