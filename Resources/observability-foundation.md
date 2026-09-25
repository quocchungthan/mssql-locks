# SQL Server and .NET observability foundation

## Goal

Investigate whether the production .NET data layer contributes to blocking,
deadlocks, long-running requests, pool pressure, or other database pain points.
The tooling must attach to the existing application and SQL Server without
changing application behavior or introducing a second source of truth.

The first version is read-only and diagnostic. It must not kill sessions,
change database configuration, or write to the production database.

## Adopt existing telemetry first

| Question | Existing source | Role of this repository |
| --- | --- | --- |
| What is blocked now? | SQL Server DMVs such as `sys.dm_exec_requests`, `sys.dm_os_waiting_tasks`, and `sys.dm_tran_locks` | Read, normalize, correlate, and explain |
| What blocking or deadlocks occurred? | Extended Events, especially `blocked_process_report` and `xml_deadlock_report` | Consume exported events or a controlled session |
| Which queries consume time and resources? | Query Store and DMVs | Read query statistics and join them to application context |
| Can a profiler catch locking? | SQL Server Profiler/trace can show lock and blocking activity, but it is legacy tooling | Prefer Extended Events for new collection; support Profiler exports only when already used |
| Is the .NET connection pool under pressure? | `Microsoft.Data.SqlClient` EventSource counters and .NET runtime counters | Collect counters beside SQL Server session counts |
| Which application request caused it? | Existing .NET logs, correlation IDs, OpenTelemetry/`Activity` data when available | Correlate; do not require invasive code changes initially |

SQL Server Profiler can observe locking-related events, but it is not the
preferred foundation for new monitoring. Extended Events has lower overhead,
better filtering, and is the supported direction for ongoing diagnostics.
Profiler traces are still useful as an input when an operations team already
has a trace or an existing workflow around them.

## Proposed architecture

```text
SQL Server DMVs / Extended Events / Query Store
                    \
                     -> adapters -> normalized observations -> correlation
.NET logs / Activity / SqlClient counters /
runtime counters  /
                                      \
                                       -> reports and exploratory charts
```

The adapters should be replaceable:

- `ISqlServerObservationSource`
- `IDotnetObservationSource`
- `IObservationStore`
- `IInvestigationReport`
- `IChartDatasetWriter`

The first implementation can use a command-line .NET project and JSON/CSV
artifacts. A dashboard should come later, after the observation model and
correlation rules are validated.

## First investigation slices

1. **Live blocking snapshot**
   - blocking session and blocked sessions
   - wait type, wait duration, database, host, login, application name
   - SQL text and transaction metadata when permitted
2. **Deadlock and blocked-process ingestion**
   - parse Extended Events XML
   - preserve the original event for auditability
   - extract victim, resource, owner, waiter, and statement information
3. **Connection-pool pressure**
   - active/free/stasis/reclaimed `SqlClient` connections when counters are
     available
   - pool key dimensions such as server, database, and application name
   - SQL Server session counts for the same application
4. **Cross-source correlation**
   - timestamp window
   - SQL session ID where available
   - application name, host, request/correlation ID, and normalized SQL hash
5. **Explanatory output**
   - timeline of blocking episodes
   - blocker-to-victim graph
   - wait-duration distribution
   - pool pressure versus blocked-request count
   - query/resource summary with confidence and missing-data notes

## Safety boundaries

- Use a dedicated least-privilege login.
- Default all collectors to read-only queries.
- Do not store connection strings or credentials in the repository.
- Redact secrets and optionally parameter values from captured SQL text.
- Make collection interval, retention, and event filters configurable.
- Record collector errors explicitly; never turn a failed collector into an
  empty successful report.
- Validate overhead on a non-production environment before enabling production
  collection.

## Success criteria for the first prototype

Given a time window, the prototype can produce a reproducible report that:

1. identifies active blockers and blocked requests;
2. ingests at least one Extended Events deadlock or blocked-process event;
3. shows .NET pool/runtime counters when the application exposes them;
4. correlates observations without pretending that missing identifiers match;
5. emits chart-ready CSV/JSON plus a short explanation of evidence and limits.

The prototype is an investigation aid, not an automated diagnosis engine.
