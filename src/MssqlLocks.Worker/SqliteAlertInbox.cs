using System.Text.Json;
using Microsoft.Data.Sqlite;

namespace MssqlLocks.Worker;

public sealed class SqliteAlertInbox(string inboxDirectory, TimeProvider? clock = null, int maxAttempts = 5) : IAlertInbox
{
    private readonly TimeProvider _clock = clock ?? TimeProvider.System;
    private readonly string _databasePath = Path.Combine(Path.GetFullPath(inboxDirectory), "alerts.db");
    private readonly string _connectionString = new SqliteConnectionStringBuilder
    {
        DataSource = Path.Combine(Path.GetFullPath(inboxDirectory), "alerts.db"),
        Mode = SqliteOpenMode.ReadWriteCreate,
        Pooling = false,
        DefaultTimeout = 10
    }.ToString();

    public void Initialize()
    {
        if (maxAttempts < 1) throw new ArgumentOutOfRangeException(nameof(maxAttempts));
        Directory.CreateDirectory(Path.GetDirectoryName(_databasePath)!);
        using var db = Open();
        using var transaction = db.BeginTransaction(deferred: false);
        var version = Scalar<long>(db, transaction, "PRAGMA user_version;");
        if (version > 1) throw new InvalidOperationException($"Unsupported inbox schema version {version}.");
        if (version == 0)
        {
            Execute(db, transaction, """
                CREATE TABLE IF NOT EXISTS alerts (
                    event_id TEXT PRIMARY KEY,
                    payload TEXT NOT NULL,
                    state TEXT NOT NULL CHECK(state IN ('pending','leased','completed','dead-letter')),
                    attempts INTEGER NOT NULL DEFAULT 0,
                    next_attempt_utc TEXT NOT NULL,
                    lease_until_utc TEXT,
                    lease_token TEXT,
                    result_ref TEXT,
                    last_error TEXT
                );
                CREATE INDEX IF NOT EXISTS ix_alerts_claim ON alerts(state, next_attempt_utc, lease_until_utc);
                CREATE TABLE IF NOT EXISTS results (
                    event_id TEXT PRIMARY KEY REFERENCES alerts(event_id),
                    lease_token TEXT NOT NULL,
                    result_ref TEXT NOT NULL
                );
                PRAGMA user_version = 1;
                """);
        }
        transaction.Commit();
    }

    public bool Enqueue(AlertEnvelope alert)
    {
        alert.Validate();
        using var db = Open();
        return Execute(db, null, """
            INSERT INTO alerts(event_id,payload,state,next_attempt_utc)
            VALUES($id,$payload,'pending',$now)
            ON CONFLICT(event_id) DO NOTHING;
            """, ("$id", alert.EventId), ("$payload", JsonSerializer.Serialize(alert, AlertEnvelope.JsonOptions)),
            ("$now", Utc(_clock.GetUtcNow()))) == 1;
    }

    public ClaimedAlert? ClaimNext(TimeSpan leaseDuration)
    {
        if (leaseDuration <= TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(leaseDuration));
        var now = _clock.GetUtcNow();
        using var db = Open();
        using var transaction = db.BeginTransaction(deferred: false);
        Execute(db, transaction, """
            UPDATE alerts SET state='dead-letter',lease_token=NULL,lease_until_utc=NULL,
                last_error='Lease expired after maximum attempts'
            WHERE state='leased' AND lease_until_utc <= $now AND attempts >= $max;
            """, ("$now", Utc(now)), ("$max", maxAttempts));
        using var command = Command(db, transaction, """
            SELECT event_id,payload,attempts FROM alerts
            WHERE (state='pending' AND next_attempt_utc <= $now)
               OR (state='leased' AND lease_until_utc <= $now AND attempts < $max)
            ORDER BY next_attempt_utc,event_id LIMIT 1;
            """, ("$now", Utc(now)), ("$max", maxAttempts));
        string? id = null, payload = null;
        var attempts = 0;
        using (var reader = command.ExecuteReader())
        {
            if (reader.Read())
            {
                id = reader.GetString(0);
                payload = reader.GetString(1);
                attempts = reader.GetInt32(2);
            }
        }
        if (id is null)
        {
            transaction.Commit();
            return null;
        }
        var token = Guid.NewGuid().ToString("N");
        var until = now.Add(leaseDuration);
        Execute(db, transaction, """
            UPDATE alerts SET state='leased',attempts=attempts+1,lease_token=$token,
                lease_until_utc=$until,last_error=NULL WHERE event_id=$id;
            """, ("$token", token), ("$until", Utc(until)), ("$id", id));
        transaction.Commit();
        return new ClaimedAlert(AlertEnvelope.Parse(payload!), token, attempts + 1, until);
    }

    public void RecordResult(ClaimedAlert claim, string resultRef)
    {
        if (string.IsNullOrWhiteSpace(resultRef)) throw new ArgumentException("A durable result reference is required.", nameof(resultRef));
        if (!Path.IsPathFullyQualified(resultRef) || !File.Exists(resultRef))
            throw new InvalidOperationException("Result file must exist on disk before it can be recorded.");
        using var db = Open();
        using var transaction = db.BeginTransaction(deferred: false);
        EnsureActive(db, transaction, claim);
        Execute(db, transaction, """
            INSERT INTO results(event_id,lease_token,result_ref) VALUES($id,$token,$ref)
            ON CONFLICT(event_id) DO UPDATE SET lease_token=excluded.lease_token,result_ref=excluded.result_ref;
            """, ("$id", claim.Alert.EventId), ("$token", claim.LeaseToken), ("$ref", resultRef));
        transaction.Commit();
    }

    public void Acknowledge(ClaimedAlert claim)
    {
        using var db = Open();
        using var transaction = db.BeginTransaction(deferred: false);
        EnsureActive(db, transaction, claim);
        using (var command = Command(db, transaction, """
            SELECT result_ref FROM results WHERE event_id=$id AND lease_token=$token;
            """, ("$id", claim.Alert.EventId), ("$token", claim.LeaseToken)))
        {
            var resultRef = command.ExecuteScalar() as string;
            if (resultRef is null || !File.Exists(resultRef))
                throw new InvalidOperationException("Cannot acknowledge without a durable result file for this lease.");
        }
        var changed = Execute(db, transaction, """
            UPDATE alerts SET state='completed',result_ref=(
                SELECT result_ref FROM results WHERE event_id=$id AND lease_token=$token
            ),lease_token=NULL,lease_until_utc=NULL
            WHERE event_id=$id AND EXISTS(
                SELECT 1 FROM results WHERE event_id=$id AND lease_token=$token
            );
            """, ("$id", claim.Alert.EventId), ("$token", claim.LeaseToken));
        if (changed != 1) throw new InvalidOperationException("Cannot acknowledge before a durable result is recorded for this lease.");
        transaction.Commit();
    }

    public void Abandon(ClaimedAlert claim, string reason)
    {
        if (string.IsNullOrWhiteSpace(reason) || reason.Length > 512)
            throw new ArgumentException("Retry reason must contain 1-512 characters.", nameof(reason));
        using var db = Open();
        using var transaction = db.BeginTransaction(deferred: false);
        EnsureActive(db, transaction, claim);
        var next = _clock.GetUtcNow().AddSeconds(Math.Min(3600, 5 * Math.Pow(2, Math.Min(claim.Attempt - 1, 10))));
        Execute(db, transaction, """
            UPDATE alerts SET state=$state,lease_token=NULL,lease_until_utc=NULL,
                next_attempt_utc=$next,last_error=$reason
            WHERE event_id=$id;
            """, ("$state", claim.Attempt >= maxAttempts ? "dead-letter" : "pending"),
            ("$next", Utc(next)), ("$reason", reason), ("$id", claim.Alert.EventId));
        transaction.Commit();
    }

    public IReadOnlyList<InboxItem> List(string? state = null)
    {
        if (state is not null && state is not ("pending" or "leased" or "completed" or "dead-letter"))
            throw new ArgumentException("Unknown inbox state.", nameof(state));
        using var db = Open();
        using var command = Command(db, null, """
            SELECT event_id,state,attempts,next_attempt_utc,result_ref,last_error
            FROM alerts WHERE $state IS NULL OR state=$state ORDER BY next_attempt_utc,event_id;
            """, ("$state", (object?)state ?? DBNull.Value));
        using var reader = command.ExecuteReader();
        var items = new List<InboxItem>();
        while (reader.Read())
            items.Add(new InboxItem(reader.GetString(0), reader.GetString(1), reader.GetInt32(2),
                DateTimeOffset.Parse(reader.GetString(3), System.Globalization.CultureInfo.InvariantCulture),
                reader.IsDBNull(4) ? null : reader.GetString(4), reader.IsDBNull(5) ? null : reader.GetString(5)));
        return items;
    }

    public IReadOnlyDictionary<string, int> Counts()
    {
        using var db = Open();
        using var command = Command(db, null, "SELECT state,COUNT(*) FROM alerts GROUP BY state;");
        using var reader = command.ExecuteReader();
        var counts = new Dictionary<string, int> { ["pending"] = 0, ["leased"] = 0, ["completed"] = 0, ["dead-letter"] = 0 };
        while (reader.Read()) counts[reader.GetString(0)] = reader.GetInt32(1);
        return counts;
    }

    private void EnsureActive(SqliteConnection db, SqliteTransaction transaction, ClaimedAlert claim)
    {
        if (Scalar<long>(db, transaction, """
            SELECT COUNT(*) FROM alerts WHERE event_id=$id AND state='leased'
                AND lease_token=$token AND lease_until_utc > $now AND attempts=$attempt;
            """, ("$id", claim.Alert.EventId), ("$token", claim.LeaseToken),
            ("$now", Utc(_clock.GetUtcNow())), ("$attempt", claim.Attempt)) != 1)
            throw new InvalidOperationException("Lease is expired, replaced, or already finished.");
    }

    private SqliteConnection Open()
    {
        var db = new SqliteConnection(_connectionString);
        db.Open();
        using var command = db.CreateCommand();
        command.CommandText = "PRAGMA busy_timeout=10000; PRAGMA foreign_keys=ON; PRAGMA synchronous=FULL;";
        command.ExecuteNonQuery();
        return db;
    }

    private static string Utc(DateTimeOffset time) => time.UtcDateTime.ToString("O", System.Globalization.CultureInfo.InvariantCulture);

    private static SqliteCommand Command(SqliteConnection db, SqliteTransaction? transaction, string sql, params (string, object)[] parameters)
    {
        var command = db.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = sql;
        foreach (var (name, value) in parameters) command.Parameters.AddWithValue(name, value);
        return command;
    }

    private static int Execute(SqliteConnection db, SqliteTransaction? transaction, string sql, params (string, object)[] parameters)
    {
        using var command = Command(db, transaction, sql, parameters);
        return command.ExecuteNonQuery();
    }

    private static T Scalar<T>(SqliteConnection db, SqliteTransaction? transaction, string sql, params (string, object)[] parameters)
    {
        using var command = Command(db, transaction, sql, parameters);
        return (T)Convert.ChangeType(command.ExecuteScalar()!, typeof(T));
    }
}
