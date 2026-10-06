-- Purpose: Rank queries by the query memory (grant) they used, to find what exhausts the memory-grant pool.
-- Scope: Past 24 hours in the current database; Query Store must be enabled. Memory is in KB (8 KB pages x 8).
-- Diagnostic queries over sys.dm_* and sys.query_store_* views are excluded.

;WITH PlanMemory AS
(
    SELECT
        plan_data.query_id AS query_id,
        SUM(runtime_stats.count_executions) AS execution_count,
        SUM(CASE WHEN runtime_stats.execution_type = 3 THEN runtime_stats.count_executions ELSE 0 END) AS aborted_count,
        CONVERT(bigint, SUM(runtime_stats.avg_query_max_used_memory * runtime_stats.count_executions)
            / NULLIF(SUM(runtime_stats.count_executions), 0) * 8) AS average_used_memory_kb,
        MAX(runtime_stats.max_query_max_used_memory) * 8 AS max_used_memory_kb,
        CONVERT(bigint, SUM(runtime_stats.avg_duration * runtime_stats.count_executions)
            / NULLIF(SUM(runtime_stats.count_executions), 0)) AS average_duration_us
    FROM sys.query_store_runtime_stats AS runtime_stats
    INNER JOIN sys.query_store_plan AS plan_data
        ON plan_data.plan_id = runtime_stats.plan_id
    WHERE runtime_stats.last_execution_time >= DATEADD(HOUR, -24, SYSUTCDATETIME())
    GROUP BY plan_data.query_id
)
SELECT TOP (20)
    plan_memory.query_id AS query_id,
    plan_memory.execution_count AS execution_count,
    plan_memory.aborted_count AS aborted_count,
    plan_memory.average_used_memory_kb AS average_used_memory_kb,
    plan_memory.max_used_memory_kb AS max_used_memory_kb,
    plan_memory.average_duration_us AS average_duration_us,
    LEFT(REPLACE(REPLACE(query_text.query_sql_text, CHAR(13), N' '), CHAR(10), N' '), 160) AS query_text_start
FROM PlanMemory AS plan_memory
INNER JOIN sys.query_store_query AS query_data
    ON query_data.query_id = plan_memory.query_id
INNER JOIN sys.query_store_query_text AS query_text
    ON query_text.query_text_id = query_data.query_text_id
WHERE query_text.query_sql_text NOT LIKE N'%sys.dm[_]%'
    AND query_text.query_sql_text NOT LIKE N'%sys.query[_]store[_]%'
ORDER BY plan_memory.max_used_memory_kb DESC, plan_memory.average_used_memory_kb DESC;
