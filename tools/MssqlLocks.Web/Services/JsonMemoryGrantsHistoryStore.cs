using System.Text.Json;

namespace MssqlLocks.Web.Services;

public sealed class JsonMemoryGrantsHistoryStore : IMemoryGrantsHistoryStore
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly SemaphoreSlim gate = new(1, 1);
    private readonly string historyPath;
    private readonly int maximumPoints;
    private readonly int compactionInterval;
    private int appendsSinceCompaction;

    public JsonMemoryGrantsHistoryStore(string historyPath, int maximumPoints = 10_000)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(historyPath);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maximumPoints);
        this.historyPath = Path.GetFullPath(historyPath);
        this.maximumPoints = maximumPoints;
        compactionInterval = Math.Min(maximumPoints, 256);
    }

    public async Task<IReadOnlyList<MemoryGrantsHistoryPoint>> ReadAsync(CancellationToken cancellationToken)
    {
        await gate.WaitAsync(cancellationToken);
        try
        {
            return await ReadUnsafeAsync(cancellationToken);
        }
        finally
        {
            gate.Release();
        }
    }

    public async Task AppendAsync(MemoryGrantsHistoryPoint point, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(point);
        await gate.WaitAsync(cancellationToken);
        try
        {
            var directory = Path.GetDirectoryName(historyPath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var json = JsonSerializer.Serialize(point, JsonOptions);
            await File.AppendAllTextAsync(historyPath, json + Environment.NewLine, cancellationToken);
            appendsSinceCompaction++;
            if (appendsSinceCompaction >= compactionInterval)
            {
                await CompactUnsafeAsync(cancellationToken);
            }
        }
        finally
        {
            gate.Release();
        }
    }

    public async Task ClearAsync(CancellationToken cancellationToken)
    {
        await gate.WaitAsync(cancellationToken);
        try
        {
            if (File.Exists(historyPath))
            {
                File.Delete(historyPath);
            }
            appendsSinceCompaction = 0;
        }
        finally
        {
            gate.Release();
        }
    }

    private async Task<IReadOnlyList<MemoryGrantsHistoryPoint>> ReadUnsafeAsync(CancellationToken cancellationToken)
    {
        if (!File.Exists(historyPath))
        {
            return [];
        }

        var lines = await File.ReadAllLinesAsync(historyPath, cancellationToken);
        var points = new List<MemoryGrantsHistoryPoint>(Math.Min(lines.Length, maximumPoints));
        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            try
            {
                var point = JsonSerializer.Deserialize<MemoryGrantsHistoryPoint>(line, JsonOptions);
                if (point is not null)
                {
                    points.Add(point);
                    if (points.Count > maximumPoints)
                    {
                        points.RemoveAt(0);
                    }
                }
            }
            catch (JsonException)
            {
            }
        }

        return points;
    }

    private async Task CompactUnsafeAsync(CancellationToken cancellationToken)
    {
        var points = await ReadUnsafeAsync(cancellationToken);
        var temporaryPath = $"{historyPath}.{Guid.NewGuid():N}.tmp";
        try
        {
            var lines = points.Select(point => JsonSerializer.Serialize(point, JsonOptions));
            await File.WriteAllLinesAsync(temporaryPath, lines, cancellationToken);
            File.Move(temporaryPath, historyPath, true);
            appendsSinceCompaction = 0;
        }
        finally
        {
            if (File.Exists(temporaryPath))
            {
                File.Delete(temporaryPath);
            }
        }
    }
}