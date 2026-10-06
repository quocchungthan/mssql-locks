# mssql-locks
This is created to solve the slow queries problem for dotnet application that has: inbox/outbox pattern, rest apis, background jobs, and it has timeout issue when reaching the database.

## Query Store

Query Store retains query plans and runtime statistics across plan-cache eviction and SQL Server restarts. When `capture-status.sql` reports `actual_state` and `desired_state` as `OFF`, no Query Store history is being collected. An empty cost report therefore means there is no captured history, not that the workload had no expensive queries.

Enabling Query Store changes database configuration and consumes database storage. Review the settings with the database owner, select the intended database explicitly, and run the following with an account that can alter that database:

```sql
ALTER DATABASE [YourDatabase] SET QUERY_STORE = ON;

ALTER DATABASE [YourDatabase] SET QUERY_STORE
(
    OPERATION_MODE = READ_WRITE,
    QUERY_CAPTURE_MODE = AUTO,
    MAX_STORAGE_SIZE_MB = 1024,
    CLEANUP_POLICY = (STALE_QUERY_THRESHOLD_DAYS = 30),
    INTERVAL_LENGTH_MINUTES = 15
);
```

`QUERY_CAPTURE_MODE = AUTO` avoids capturing every trivial statement. Storage, retention, and interval values are starting points and should be adjusted for the workload and available capacity.

Verify the state and query retained costs through the repository tool:

```text
dotnet run --project tools/MssqlLocks.QueryStore -- capture-status.sql
dotnet run --project tools/MssqlLocks.QueryStore -- top-query-costs.sql
dotnet run --project tools/MssqlLocks.QueryStore -- top-query-costs-past-7-days.sql
```

## Live capacity

Run the live console dashboard to refresh session, connection, request, transaction, task, worker, and scheduler counts once per second:

```text
dotnet run --project tools/MssqlLocks.Monitor -- current-capacity-counts.sql
```

Press `Ctrl+C` to stop it.

## Live memory grants

Opt in to refresh the existing read-only memory-grants report every two seconds:

```text
dotnet run --project tools/MssqlLocks.Dmv -- memory-grants.sql --watch
```

Set a different positive refresh interval in seconds with `--interval-seconds N`. Watch mode is limited to `memory-grants.sql`; without `--watch`, DMV reports continue to run once and exit.

## Web dashboard

The MVC dashboard serves a read-only live memory-grants view. From the repository root, restore its local SignalR browser asset and start the web host:

```powershell
Push-Location tools/MssqlLocks.Web
npm ci
npm run copy:signalr
Pop-Location
dotnet run --project tools/MssqlLocks.Web --launch-profile http
```

Open `http://localhost:5127`. The watcher stays stopped until started in the page; refresh intervals are limited to 5, 10, 30, or 60 seconds. Configure `MSSQL_LOCKS_CONNECTION_STRING` through the existing `.env` or environment variable. Keep the host bound to localhost unless authentication and access controls are configured before exposing it to other machines.