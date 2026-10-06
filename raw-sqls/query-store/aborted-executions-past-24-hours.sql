-- Purpose: List Query Store executions that were aborted (e.g. client command timeout) or failed, with the plan's dominant waits.
-- Scope: Past 24 hours in the current database; Query Store must be enabled. Times are UTC interval boundaries.
-- execution_type: 3 = aborted by client (includes command timeout, error -2), 4 = ended with exception.
-- Wait categories are per plan and interval and include all executions of that plan in the interval.
-- Diagnostic queries over sys.dm_* and sys.query_store_* views are excluded.

;WITH FailedExecutions AS
(
    SELECT
        runtime_stats.plan_id AS plan_id,
        runtime_stats.runtime_stats_interval_id AS runtime_stats_interval_id,
        runtime_stats.execution_type_desc AS execution_type,
        runtime_stats.count_executions AS execution_count,
        CONVERT(bigint, runtime_stats.avg_duration) AS average_duration_us,
        runtime_stats.max_duration AS max_duration_us,
        CONVERT(bigint, runtime_stats.avg_cpu_time) AS average_cpu_time_us,
        CONVERT(bigint, runtime_stats.avg_logical_io_reads) AS average_logical_reads,
        runtime_stats.max_query_max_used_memory * 8 AS max_used_memory_kb,
        runtime_stats.last_execution_time AS last_execution_time_utc
    FROM sys.query_store_runtime_stats AS runtime_stats
    WHERE runtime_stats.execution_type IN (3, 4)
        AND runtime_stats.last_execution_time >= DATEADD(HOUR, -24, SYSUTCDATETIME())
), IntervalWaits AS
(
    SELECT
        wait_stats.plan_id AS plan_id,
        wait_stats.runtime_stats_interval_id AS runtime_stats_interval_id,
        wait_stats.wait_category_desc AS wait_category,
        SUM(wait_stats.total_query_wait_time_ms) AS total_wait_ms,
        ROW_NUMBER() OVER
        (
            PARTITION BY wait_stats.plan_id, wait_stats.runtime_stats_interval_id
            ORDER BY SUM(wait_stats.total_query_wait_time_ms) DESC
        ) AS wait_rank
    FROM sys.query_store_wait_stats AS wait_stats
    GROUP BY wait_stats.plan_id, wait_stats.runtime_stats_interval_id, wait_stats.wait_category_desc
)
SELECT TOP (100)
    failed.last_execution_time_utc AS last_execution_time_utc,
    plan_data.query_id AS query_id,
    failed.plan_id AS plan_id,
    failed.execution_type AS execution_type,
    failed.execution_count AS execution_count,
    failed.average_duration_us AS average_duration_us,
    failed.max_duration_us AS max_duration_us,
    failed.average_cpu_time_us AS average_cpu_time_us,
    failed.average_logical_reads AS average_logical_reads,
    failed.max_used_memory_kb AS max_used_memory_kb,
    first_wait.wait_category AS top_wait_category,
    first_wait.total_wait_ms AS top_wait_ms,
    second_wait.wait_category AS second_wait_category,
    second_wait.total_wait_ms AS second_wait_ms,
    LEFT(REPLACE(REPLACE(query_text.query_sql_text, CHAR(13), N' '), CHAR(10), N' '), 160) AS query_text_start
FROM FailedExecutions AS failed
INNER JOIN sys.query_store_plan AS plan_data
    ON plan_data.plan_id = failed.plan_id
INNER JOIN sys.query_store_query AS query_data
    ON query_data.query_id = plan_data.query_id
INNER JOIN sys.query_store_query_text AS query_text
    ON query_text.query_text_id = query_data.query_text_id
LEFT JOIN IntervalWaits AS first_wait
    ON first_wait.plan_id = failed.plan_id
    AND first_wait.runtime_stats_interval_id = failed.runtime_stats_interval_id
    AND first_wait.wait_rank = 1
LEFT JOIN IntervalWaits AS second_wait
    ON second_wait.plan_id = failed.plan_id
    AND second_wait.runtime_stats_interval_id = failed.runtime_stats_interval_id
    AND second_wait.wait_rank = 2
WHERE query_text.query_sql_text NOT LIKE N'%sys.dm[_]%'
    AND query_text.query_sql_text NOT LIKE N'%sys.query[_]store[_]%'
ORDER BY failed.last_execution_time_utc DESC;
