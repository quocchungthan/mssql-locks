# Manual SQL Server evidence export workflow

This guide defines the first, manual data path for `MssqlLocks.Observer`.
It allows the team to collect production evidence with existing SQL Server
tools, review and redact it, then load it into the application without giving
the application a production connection.

## 1. Define an investigation window

Before exporting anything, record:

- investigation ID;
- production service and environment;
- SQL Server instance and database name, if permitted;
- local time zone and UTC offset;
- start and end timestamps;
- observed symptom, such as blocking, timeout, deadlock, or pool exhaustion;
- application deployment/version;
- person or team performing the export.

Use one investigation ID for every related file:

```text
INV-2026-09-25-001
```

Do not put credentials, access tokens, or connection strings in the
investigation record.

## 2. Use three evidence destinations

The repository separates knowledge from imported evidence:

```text
Areas/
  ProductionServiceMSSQL/       # PARA notes, hypotheses, and findings
Resources/                      # this guide, SQL templates, schemas
Data/                           # local runtime evidence; never commit
  Imports/
    INV-2026-09-25-001/
      01-CurrentSql/
      02-HistoricalSql/
      03-DotnetApplication/
  Reports/
  Samples/
```

Every evidence file belongs to exactly one of these three destinations inside
its investigation folder:

| Destination | Evidence | When to collect |
| --- | --- | --- |
| `01-CurrentSql` | DMV snapshots of active requests, waits, locks, and blockers | During the incident, immediately before and after a reproduction, or on a fixed sampling interval |
| `02-HistoricalSql` | Extended Events and Query Store exports | After the incident window, or immediately after a controlled reproduction |
| `03-DotnetApplication` | Application logs, request traces, and .NET/`SqlClient` counters | During the same time window as the SQL evidence |

`Data/Reports/` is generated output, not raw evidence. `Data/Samples/` contains
sanitized examples only and is not an investigation destination.

Create the investigation folder outside the repository first when the export
contains production data. Copy it into `Data/Imports/` only after the
redaction step and only on a machine approved to handle that data.

From PowerShell, the local folder structure can be created with:

```powershell
$investigationId = "INV-2026-09-25-001"
$root = Join-Path "Data\Imports" $investigationId
New-Item -ItemType Directory -Force -Path `
  (Join-Path $root "01-CurrentSql"), `
  (Join-Path $root "02-HistoricalSql"), `
  (Join-Path $root "03-DotnetApplication") | Out-Null
```

Run this from the repository root only after confirming that the investigation
folder contains no unredacted production data.

## 3. Capture current SQL state in `01-CurrentSql`

### When to capture

Capture a snapshot:

1. as soon as a user reports blocking, timeout, or slow requests;
2. every 15-30 seconds while the symptom is active;
3. immediately before and after a controlled reproduction;
4. once after the symptom clears, to document recovery.

Do not wait until after the incident if the problem is transient.

### SSMS click and typing steps

1. Open **SQL Server Management Studio**.
2. Select **Connect > Database Engine**.
3. Enter the approved production server and authentication details.
4. Select **New Query**.
5. Set the database dropdown to the affected database.
6. Paste the approved read-only DMV query from `Resources/Sql/`.
7. Replace only the documented `@CaptureTimeUtc` or database parameters.
8. Select **Execute** or press **F5**.
9. In the results grid, right-click and choose **Save Results As...**.
10. Choose **CSV (comma delimited)** and save under `01-CurrentSql`.
11. Save the query text separately as a `.sql` file beside the CSV.

If the query is not yet stored in `Resources/Sql/`, copy the exact query text
into the investigation folder before saving the result. Never type credentials
into the query window.

### What to capture

The snapshot should include:

- capture timestamp in UTC;
- session ID and blocking session ID;
- wait type, wait duration, and wait resource;
- database, host, login, and application name;
- transaction state and open transaction count;
- SQL text and command type, subject to the approved redaction policy.

The query should read from SQL Server DMVs such as:

- `sys.dm_exec_requests`;
- `sys.dm_os_waiting_tasks`;
- `sys.dm_exec_sessions`;
- `sys.dm_exec_sql_text`;
- `sys.dm_tran_locks`, when lock-level detail is needed.

Name the output:

```text
01-CurrentSql/INV-2026-09-25-001__dmv-blocking__20260925T085000Z.csv
01-CurrentSql/INV-2026-09-25-001__dmv-blocking__20260925T085000Z.sql
```

If no rows are returned, keep the empty export and record that the snapshot
was successful but no blocking was observed at that instant.

### Controlled reproduction

Only reproduce a problem in production when the service owner and operations
team have approved it. Prefer a staging environment.

For an approved API reproduction:

1. Record the endpoint, request parameters, user/test identity, and expected
   duration.
2. Start the current-state capture before sending the request.
3. Send one request at a time using the normal UI or the approved API client.
4. Record the exact UTC start and end timestamps and correlation ID.
5. Continue snapshots for at least one interval after completion.
6. Stop when the agreed number of reproductions is complete.

For a background service, record the job name and trigger it through its
existing supported mechanism—such as the scheduler, queue message, or
operations command. Do not edit database rows or run ad hoc production SQL to
force a trigger.

## 4. Capture historical SQL in `02-HistoricalSql`

This destination contains two different sources. They answer different
questions and must not be merged into one ambiguous export.

### 4.1 Extended Events

#### When to export

Export after the incident window ends, or immediately after a controlled
reproduction. Use the same UTC window recorded in the investigation manifest.

Prefer an existing approved Extended Events session. Do not create or change a
production session as part of this manual workflow without an operations
approval.

Collect one or both of:

- `xml_deadlock_report`;
- `blocked_process_report`.

#### SSMS click steps for an existing session

1. Open **SQL Server Management Studio** and connect to the approved server.
2. In **Object Explorer**, expand **Management**.
3. Expand **Extended Events**.
4. Expand **Sessions** and select the approved session.
5. Right-click the session and choose **Watch Live Data** only when live
   observation is approved.
6. For historical data, right-click the session and choose **View Target Data**.
7. Select the event rows for the investigation window.
8. Use the grid's save/export command and choose XML or the native XEL format
   offered by the target.
9. Save the original export under `02-HistoricalSql`.

If the approved session is not running or has no retained target data, mark
Extended Events as `Unavailable` in the manifest. Do not create a new session
or change retention settings without approval.

Export the original event XML without editing it. Preserve the event timestamp
and session metadata. If the event contains sensitive SQL text, keep the
original in the approved secure location and place only the redacted copy in
the repository's local `Data/Imports/` directory.

Name the output:

```text
02-HistoricalSql/INV-2026-09-25-001__extended-events__20260925T080000Z-20260925T090000Z.xel
```

If the source tool exports XML instead of XEL, retain the source extension and
record the exporter in the manifest.

### 4.2 Query Store

#### When to export

Export after the incident or reproduction, once the full time window is known.
Query Store may have a collection delay, so do not export immediately and
assume the newest interval is complete.

#### SSMS click and typing steps

1. In **Object Explorer**, expand the affected database.
2. Expand **Query Store**.
3. Select **Tracked Queries** or **Regressed Queries** to inspect the period.
4. Set the time range to the investigation's UTC-equivalent window.
5. Use the query text and plan information to identify candidates.
6. Select **New Query** and run the approved read-only Query Store export query.
7. Select **Execute** or press **F5**.
8. Right-click the results grid, choose **Save Results As...**, and select CSV.
9. Save the result and the exact `.sql` query under `02-HistoricalSql`.

If Query Store is disabled, unavailable, or outside the retention window, record
that fact instead of treating an empty export as evidence.

For the same time window, export the query statistics needed to compare:

- execution count;
- duration;
- CPU time;
- logical reads and writes;
- query and plan identifiers;
- wait statistics, when enabled;
- database and object context, when available.

Prefer CSV or JSON for the first prototype. Save the query text separately
when the export tool does not include it.

Name the output:

```text
02-HistoricalSql/INV-2026-09-25-001__query-store__20260925T080000Z-20260925T090000Z.csv
02-HistoricalSql/INV-2026-09-25-001__query-store__20260925T080000Z-20260925T090000Z.sql
```

Query Store data is historical. Do not describe it as proof that a specific
request caused a lock unless it can be correlated with session or application
evidence.

## 5. Collect .NET application evidence in `03-DotnetApplication`

### When to collect

Collect application evidence during the same window as the SQL snapshots.
Start log/counter collection before reproducing the issue and stop only after
the final post-reproduction snapshot.

### How to trigger or reproduce

Use the normal supported application path:

- API: send the approved request with a test correlation ID;
- UI: perform the documented user action once per trial;
- background service: use the scheduler, queue, or operations trigger;
- batch: use the existing approved job command.

Record the trigger, input identifier, deployment version, UTC start/end, and
correlation ID. Never create artificial locking by running `KILL`, changing
isolation levels, holding open transactions, or editing production data.

### Export steps

1. Open the approved application log viewer or log storage.
2. Filter by the investigation UTC window.
3. Filter by service, deployment version, endpoint/job, and correlation ID.
4. Export JSON or JSONL using the viewer's **Export**, **Download**, or
   **Save results** action.
5. If counters are provided by the service's existing diagnostics endpoint,
   open the approved endpoint and save the JSON response at the same interval
   as the SQL snapshots.
6. Save the files under `03-DotnetApplication`.

Request the application logs for the same window. Include, when available:

- request or trace correlation ID;
- timestamp with time zone;
- endpoint or background job name;
- database operation name;
- elapsed duration;
- timeout, deadlock, or cancellation details;
- SQL client exception number and message;
- pool or runtime counter samples.

Redact access tokens, passwords, personal data, and parameter values that are
not required for the investigation. Keep a redaction note rather than
silently changing evidence.

Name the output:

```text
03-DotnetApplication/INV-2026-09-25-001__dotnet-logs__20260925T080000Z-20260925T090000Z.jsonl
03-DotnetApplication/INV-2026-09-25-001__dotnet-counters__20260925T080000Z-20260925T090000Z.csv
```

If the application has no counter endpoint or pool counters, record
`Unavailable` and explain the limitation. Do not infer pool exhaustion from
request latency alone.

## 6. Create an evidence manifest

Create `manifest.json` in the investigation folder beside the three destination
folders:

```json
{
  "investigationId": "INV-2026-09-25-001",
  "capturedAtUtc": "2026-09-25T09:00:00Z",
  "windowStartUtc": "2026-09-25T08:00:00Z",
  "windowEndUtc": "2026-09-25T09:00:00Z",
  "service": "ProductionService",
  "environment": "Production",
  "files": [
    {
      "path": "01-CurrentSql/INV-2026-09-25-001__dmv-blocking__20260925T085000Z.csv",
      "kind": "SqlServerDmvSnapshot",
      "redacted": true,
      "sha256": "record-after-file-is-final"
    }
  ],
  "notes": [
    "Times were exported in UTC.",
    "SQL parameter values were removed from the repository copy."
  ]
}
```

The manifest is the handoff contract for the application. It makes missing
sources, redactions, and time-window differences explicit.

## 7. Validate before importing

Check every file:

- filename starts with the investigation ID;
- timestamps include a time zone or are explicitly documented;
- CSV files have a header and consistent column counts;
- Extended Events files can be opened by the source tool;
- JSON/JSONL files parse successfully;
- no credentials, tokens, or unapproved sensitive values remain;
- the manifest lists every file and its redaction status;
- hashes are generated only after the file is final.

Do not replace an invalid file with an empty file. Mark the source as
`Unavailable` in the manifest and explain why.

## 8. Place evidence for the MVC application

After validation, copy the redacted investigation folder to:

```text
Data/Imports/INV-2026-09-25-001/
```

The application will initially read these files through a file-based
observation source. It must treat imported evidence as immutable input and
write generated output only to:

```text
Data/Reports/INV-2026-09-25-001/
```

The application must not modify the source files.

## 9. Record findings in PARA notes

After the application produces a report, record the interpretation under the
relevant folder in `Areas/ProductionServiceMSSQL/`:

- **BackgroundServices**: scheduled jobs, workers, and recurring operations;
- **FromRestApi**: API request paths and their database operations;
- **TriggeredByConsumers**: downstream or event-driven operations.

Each finding should link to the investigation ID and distinguish:

- observed evidence;
- correlation;
- hypothesis;
- confirmed root cause;
- unresolved questions;
- next collection required.

The report explains the evidence; the PARA note preserves the durable
knowledge and decision history.

## File handling and repository rules

- Never commit production exports, logs, connection strings, or credentials.
- Keep `Data/Imports/` and `Data/Reports/` ignored by Git.
- Store only sanitized samples under `Data/Samples/` or `Resources/`.
- Use least-privilege read access for every export.
- Prefer Extended Events over new SQL Server Profiler traces.
- Treat a manual export as a point-in-time observation, not continuous
  monitoring.

## Completion checklist

- [ ] Investigation ID and UTC window recorded.
- [ ] DMV blocking snapshot exported.
- [ ] Extended Events evidence checked or marked unavailable.
- [ ] Query Store evidence exported or marked unavailable.
- [ ] .NET logs/counters collected or marked unavailable.
- [ ] Sensitive values redacted.
- [ ] `manifest.json` created and validated.
- [ ] Files copied to the local import directory.
- [ ] Application report generated.
- [ ] Findings recorded under the correct PARA area.
