-- Purpose: Rank recent Query Store statements by aggregate runtime cost.
-- Scope: Query Store data retained by the current database; Query Store must be enabled.

;WITH RecentRuntime AS
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
    WHERE runtime_interval.end_time >= DATEADD(HOUR, -24, SYSUTCDATETIME())
    GROUP BY runtime_stats.plan_id
), QueryStoreCosts AS
(
    SELECT
        plan_data.query_id AS query_id,
        recent_data.plan_id AS plan_id,
        DB_NAME() AS database_name,
        query_data.object_id AS object_id,
        OBJECT_SCHEMA_NAME(query_data.object_id) AS object_schema_name,
        OBJECT_NAME(query_data.object_id) AS object_name,
        recent_data.execution_count AS execution_count,
        recent_data.average_duration_us AS average_duration_us,
        recent_data.max_duration_us AS max_duration_us,
        recent_data.average_cpu_time_us AS average_cpu_time_us,
        recent_data.max_cpu_time_us AS max_cpu_time_us,
        recent_data.average_logical_reads AS average_logical_reads,
        recent_data.average_logical_writes AS average_logical_writes,
        query_text.query_sql_text AS query_text
    FROM RecentRuntime AS recent_data
    INNER JOIN sys.query_store_plan AS plan_data
        ON plan_data.plan_id = recent_data.plan_id
    INNER JOIN sys.query_store_query AS query_data
        ON query_data.query_id = plan_data.query_id
    INNER JOIN sys.query_store_query_text AS query_text
        ON query_text.query_text_id = query_data.query_text_id
)
SELECT TOP (100)
    cost_data.query_id AS query_id,
    cost_data.plan_id AS plan_id,
    cost_data.database_name AS database_name,
    cost_data.object_id AS object_id,
    cost_data.object_schema_name AS object_schema_name,
    cost_data.object_name AS object_name,
    cost_data.execution_count AS execution_count,
    cost_data.average_duration_us AS average_duration_us,
    cost_data.max_duration_us AS max_duration_us,
    cost_data.average_cpu_time_us AS average_cpu_time_us,
    cost_data.max_cpu_time_us AS max_cpu_time_us,
    cost_data.average_logical_reads AS average_logical_reads,
    cost_data.average_logical_writes AS average_logical_writes,
    cost_data.query_text AS query_text
FROM QueryStoreCosts AS cost_data
ORDER BY cost_data.average_cpu_time_us DESC, cost_data.execution_count DESC;