using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace MssqlLocks.Worker;

public sealed record AlertEnvelope
{
    public required int SchemaVersion { get; init; }
    public required string EventId { get; init; }
    public required string Source { get; init; }
    public required string Instance { get; init; }
    public string? Database { get; init; }
    public required string Rule { get; init; }
    public required DateTimeOffset OccurredUtc { get; init; }
    public required string Severity { get; init; }
    public required string Summary { get; init; }
    public required string EvidenceRef { get; init; }

    public static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow
    };

    public static AlertEnvelope Parse(string json)
    {
        var alert = JsonSerializer.Deserialize<AlertEnvelope>(json, JsonOptions)
            ?? throw new ArgumentException("Alert must be a JSON object.");
        alert.Validate();
        return alert;
    }

    public void Validate()
    {
        if (SchemaVersion != 1) throw new ArgumentException("Unsupported schemaVersion; expected 1.");
        if (!Regex.IsMatch(EventId ?? "", @"\A[A-Za-z0-9][A-Za-z0-9._:-]{0,127}\z"))
            throw new ArgumentException("eventId must be a stable, nonempty identifier (maximum 128 characters).");
        CheckText(Source, "source", 128);
        CheckText(Instance, "instance", 128);
        if (Database is not null) CheckText(Database, "database", 128);
        CheckText(Rule, "rule", 128);
        CheckText(Summary, "summary", 1024);
        if (!Regex.IsMatch(EvidenceRef ?? "", @"\A[A-Za-z0-9][A-Za-z0-9._:/#-]{0,255}\z"))
            throw new ArgumentException("evidenceRef must be an opaque reference, not SQL, a URL with credentials, or a connection string.");
        if (OccurredUtc == default || OccurredUtc.Offset != TimeSpan.Zero)
            throw new ArgumentException("occurredUtc must be an explicit UTC timestamp with Z or +00:00 offset.");
        if (Severity is not ("info" or "warning" or "critical"))
            throw new ArgumentException("severity must be info, warning, or critical.");
    }

    private static void CheckText(string? value, string name, int limit)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length > limit ||
            value != value.Trim() || value.Any(char.IsControl))
            throw new ArgumentException($"{name} must be nonempty, trimmed, free of control characters, and at most {limit} characters.");
    }
}
