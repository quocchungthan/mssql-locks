-- Purpose: Rank Query Store statements by aggregate runtime cost over the past 7 days.
-- Scope: Query Store data retained by the current database; Query Store must be enabled.

;WITH HistoricalRuntime AS
(
    SELECT
        runtime_stats.plan_id AS plan_id,
        SUM(runtime_stats.count_executions) AS execution_count,
        SUM(runtime_stats.avg_duration * runtime_stats.count_executions) / NULLIF(SUM(runtime_stats.count_executions), 0) AS average_duration_us,
        MAX(runtime_stats.max_duration) AS max_duration_us,
        SUM(runtime_stats.avg_cpu_time * runtime_stats.count_executions) / NULLIF(SUM(runtime_stats.count_executions), 0) AS average_cpu_time_us,
        MAX(runtime_stats.max_cpu_time) AS max_cpu_time_us,
        SUM(runtime_stats.avg_logical_io_reads * runtime_stats.count_executions) / NULLIF(SUM(runtime_stats.count_executions), 0) AS average_logical_reads,
        SUM(runtime_stats.avg_logical_io_writes * runtime_stats.count_executions) / NULLIF(SUM(runtime_stats.count_executions), 0) AS average_logical_writes
    FROM sys.query_store_runtime_stats AS runtime_stats
    INNER JOIN sys.query_store_runtime_stats_interval AS runtime_interval
        ON runtime_interval.runtime_stats_interval_id = runtime_stats.runtime_stats_interval_id
    WHERE runtime_interval.end_time >= DATEADD(DAY, -7, SYSUTCDATETIME())
    GROUP BY runtime_stats.plan_id
)
SELECT TOP (100)
    plan_data.query_id AS query_id,
    historical_data.plan_id AS plan_id,
    DB_NAME() AS database_name,
    query_data.object_id AS object_id,
    OBJECT_SCHEMA_NAME(query_data.object_id) AS object_schema_name,
    OBJECT_NAME(query_data.object_id) AS object_name,
    historical_data.execution_count AS execution_count,
    historical_data.average_duration_us AS average_duration_us,
    historical_data.max_duration_us AS max_duration_us,
    historical_data.average_cpu_time_us AS average_cpu_time_us,
    historical_data.max_cpu_time_us AS max_cpu_time_us,
    historical_data.average_logical_reads AS average_logical_reads,
    historical_data.average_logical_writes AS average_logical_writes,
    query_text.query_sql_text AS query_text
FROM HistoricalRuntime AS historical_data
INNER JOIN sys.query_store_plan AS plan_data
    ON plan_data.plan_id = historical_data.plan_id
INNER JOIN sys.query_store_query AS query_data
    ON query_data.query_id = plan_data.query_id
INNER JOIN sys.query_store_query_text AS query_text
    ON query_text.query_text_id = query_data.query_text_id
ORDER BY historical_data.average_cpu_time_us DESC, historical_data.execution_count DESC;
