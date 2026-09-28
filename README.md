# mssql-locks

Phase 1 is a local-only alert handoff prototype, not a monitoring dashboard or
SQL collector. It accepts a normalized JSON alert from an external producer,
stores it in a durable SQLite inbox, and records a **placeholder** investigation
result before acknowledging the message. Alert content is untrusted data,
never instructions; no part of the alert is executed.

## Build and run

Requires the .NET 10 SDK. Run from the repository root in PowerShell:

```powershell
dotnet restore .\MssqlLocks.sln
dotnet build .\MssqlLocks.sln --no-restore
dotnet test .\MssqlLocks.sln --no-build
dotnet run --project .\src\MssqlLocks.Worker -- init
```

The checked-in `.\examples\alert.json` contains this example. The producer
must supply a stable `eventId` for each real event; resending the same ID does
not change its existing record.

```json
{
  "schemaVersion": 1,
  "eventId": "monitor:example-001",
  "source": "monitor",
  "instance": "sql-01",
  "database": "example",
  "rule": "blocking",
  "occurredUtc": "2026-09-28T00:00:00Z",
  "severity": "warning",
  "summary": "Example blocking alert",
  "evidenceRef": "monitor:alert/example-001"
}
```

```powershell
dotnet run --project .\src\MssqlLocks.Worker -- enqueue .\examples\alert.json
dotnet run --project .\src\MssqlLocks.Worker -- status
dotnet run --project .\src\MssqlLocks.Worker -- process
dotnet run --project .\src\MssqlLocks.Worker -- list
dotnet run --project .\src\MssqlLocks.Worker -- dead-letter
```

For manual lease/retry inspection, use `claim` instead of `process`, save its
single JSON output to a file, then call `retry CLAIM_JSON REASON`. Create
`runtime` before using this example:

```powershell
dotnet run --project .\src\MssqlLocks.Worker -- claim > .\runtime\claim.json
dotnet run --project .\src\MssqlLocks.Worker -- retry .\runtime\claim.json "temporary failure"
```

`claim` leases one ready alert for five minutes. `process` claims one alert,
atomically writes a JSON placeholder result, records its durable reference,
then acknowledges it. Failed or abandoned claims are retried after 5, 10,
20, ... seconds (capped at one hour), with at most five claims; expired final
leases are dead-lettered on the next claim. Processing failures leave the
lease to expire so the alert can be retried.

The default SQLite inbox and JSON results are under the user's local
application-data `MssqlLocks\inbox` and `MssqlLocks\results` directories.
Override them on **each invocation** with `--inbox-dir PATH --result-dir PATH`
(for example, `--inbox-dir .\runtime\inbox --result-dir .\runtime\results`).
The runtime directory and local configuration overrides are gitignored. Use
an access-controlled local disk; no secrets, SQL, or raw connection strings
belong in alert fields.

The `IAlertInbox` contract isolates durable queue operations so SQLite can
later be replaced by a managed queue. SQLite schema version 1 is initialized
by each CLI invocation, and `BEGIN IMMEDIATE` transactions serialize claims
for one local file. Results are not analysis: the placeholder does not query
SQL Server or invoke an agent.

**Not enabled:** production SQL/Extended Events collection, scheduled tasks,
services, and unattended Copilot invocation. `KaiVSCodeTranslate` is only
conceptual reference context; this solution neither edits it nor uses Huong
or translation state.
