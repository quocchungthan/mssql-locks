using MssqlLocks.Web.Services;

namespace MssqlLocks.Web.Tests;

public sealed class JsonMemoryGrantsHistoryStoreTests
{
    [Fact]
    public async Task StoreRetainsNewestPointsAndClearRemovesHistory()
    {
        var directory = Path.Combine(Path.GetTempPath(), $"memory-history-{Guid.NewGuid():N}");
        var historyPath = Path.Combine(directory, "history.jsonl");
        var store = new JsonMemoryGrantsHistoryStore(historyPath, maximumPoints: 2);
        var first = MakePoint(1);
        var second = MakePoint(2);
        var third = MakePoint(3);

        try
        {
            await store.AppendAsync(first, CancellationToken.None);
            await store.AppendAsync(second, CancellationToken.None);
            await store.AppendAsync(third, CancellationToken.None);

            var retained = await store.ReadAsync(CancellationToken.None);
            Assert.Equal(new[] { second.CapturedAt, third.CapturedAt }, retained.Select(point => point.CapturedAt));
            Assert.Equal(2, retained.Count);

            await store.ClearAsync(CancellationToken.None);

            Assert.Empty(await store.ReadAsync(CancellationToken.None));
            Assert.False(File.Exists(historyPath));
        }
        finally
        {
            if (Directory.Exists(directory))
            {
                Directory.Delete(directory, recursive: true);
            }
        }
    }

    private static MemoryGrantsHistoryPoint MakePoint(int second) => new(
        DateTimeOffset.UnixEpoch.AddSeconds(second),
        second * 1024,
        second * 2048,
        second * 10,
        second * 20,
        second * 30,
        second,
        second,
        second,
        second);
}