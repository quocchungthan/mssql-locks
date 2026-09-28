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