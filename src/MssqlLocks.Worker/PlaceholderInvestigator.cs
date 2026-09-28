using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace MssqlLocks.Worker;

public sealed class PlaceholderInvestigator(string resultDirectory)
{
    private readonly string _directory = Path.GetFullPath(resultDirectory);

    public string WriteResult(ClaimedAlert claim)
    {
        claim.Alert.Validate();
        Directory.CreateDirectory(_directory);
        var filename = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(claim.Alert.EventId))) + ".json";
        var path = Path.Combine(_directory, filename);
        var temporary = Path.Combine(_directory, $".{Guid.NewGuid():N}.tmp");
        try
        {
            var json = JsonSerializer.Serialize(new
            {
                schemaVersion = 1,
                claim.Alert.EventId,
                claim.Alert.EvidenceRef,
                claim.Attempt,
                disposition = "not-investigated",
                note = "Phase 1 placeholder only; no SQL or agent was invoked.",
                recordedUtc = DateTimeOffset.UtcNow
            }, AlertEnvelope.JsonOptions);
            using (var file = new FileStream(temporary, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            {
                var bytes = Encoding.UTF8.GetBytes(json);
                file.Write(bytes);
                file.Flush(flushToDisk: true);
            }
            File.Move(temporary, path, overwrite: true);
            return path;
        }
        finally
        {
            if (File.Exists(temporary)) File.Delete(temporary);
        }
    }
}
