# Reporting Host Boundaries

## Scope

This note describes the shared report execution architecture. It is intentionally generic: it contains no organization-specific business terminology, credentials, connection details, host names, or captured operational data.

The existing report files remain in their current locations. This change does not move, rewrite, or publish any report content.

## Layers And Contracts

The shared reporting library currently contains these logical layers:

- **Domain:** `ReportDefinition` contains report identity and a repository-relative reference. `ReportPack` is the validated, immutable collection of definitions.
- **Application:** `IReportApplicationService` runs a selected definition from a pack. It depends on `IReportSqlSource` and `IReportQueryExecutor`, so hosts can replace file loading and query execution without changing report-selection logic.
- **Infrastructure:** `ReportCatalog` discovers the current file-based packs; `FileReportSqlSource` loads their SQL after checking that the resolved path stays under the configured root; `SqlReportRunner` executes SQL using the database provider.
- **Host/presentation:** command-line executables compose the implementations and select a result consumer. `ConsoleTable` streams neutral column and row values to the terminal.

These are logical boundaries inside the existing shared project, not separate deployable projects. Dependency-injection containers are not required; hosts compose the interfaces directly today.

## Validation Stages

1. Pack construction rejects empty or unsafe categories, empty packs, mismatched report categories, non-SQL or non-leaf filenames, non-canonical relative references, and duplicate filenames.
2. Catalog resolution selects only a report discovered in the requested pack.
3. The application service rejects a report that is not a member of the supplied pack.
4. The file source resolves the repository-relative reference and rejects paths outside its configured root.
5. The SQL adapter uses read-only application intent and a bounded command timeout. Result delivery remains row-oriented rather than buffering a full result set.

## MVC And SignalR Host

`tools/MssqlLocks.Web` is an MVC host over the same `IReportApplicationService`. Its hosted worker owns one opt-in polling loop; it does not poll once per connected browser. A strongly typed SignalR hub broadcasts status and completed snapshots. The dashboard starts and stops the worker through antiforgery-protected MVC posts.

The web host filters query-text and identity columns before it broadcasts a snapshot, caps each rendered sample at 100 rows, and loads connection configuration only after the watch starts. Keep it bound to localhost until authentication and authorization are configured for a shared deployment. UI-specific state and presentation stay in the web host; report selection and SQL execution remain behind shared contracts.

## Security And Data Handling

- Treat connection credentials as host configuration. Do not place secret values in source, documentation, command output, logs, test fixtures, or captured artifacts.
- Keep generated runtime captures and logs out of source control. Source reports, tests, and documentation remain trackable.
- Do not include production captures in architecture examples or tests. Prefer offline contract tests with fake sources and executors.
- A host must apply its own authorization and exposure policy before making report output available to users.

## Incremental Next Steps

1. Add offline tests for invalid pack definitions, report membership, path containment, and the application service using fake interfaces when a test project is introduced.
2. Keep the present file-backed pack adapter until there is a concrete second storage format; adopt alternatives behind `IReportSqlSource` rather than adding a general plugin framework.
3. If the domain or application layer grows, extract it into a technology-independent project with explicit references and keep SQL-provider dependencies in infrastructure.
4. Add automated tests for watch state transitions, snapshot redaction, and the MVC/SignalR flow; configure authentication before any shared deployment.