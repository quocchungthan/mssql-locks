-- Purpose: Rank point-in-time cached query statements by cumulative resource consumption.
-- Scope: Query statistics and plans currently present in the SQL Server plan cache.
-- Results are point-in-time observations and disappear or reset as cache entries change.

;WITH CachedQueryCosts AS
(
    SELECT
        query_stats.sql_handle AS sql_handle,
        query_stats.plan_handle AS plan_handle,
        query_stats.statement_start_offset AS statement_start_offset,
        query_stats.statement_end_offset AS statement_end_offset,
        query_stats.creation_time AS plan_creation_time,
        query_stats.last_execution_time AS last_execution_time,
        query_stats.execution_count AS execution_count,
        query_stats.total_worker_time AS total_cpu_time_us,
        query_stats.total_elapsed_time AS total_elapsed_time_us,
        query_stats.total_logical_reads AS total_logical_reads,
        query_stats.total_logical_writes AS total_logical_writes,
        query_stats.total_physical_reads AS total_physical_reads,
        query_stats.total_rows AS total_rows,
        query_stats.min_worker_time AS min_cpu_time_us,
        query_stats.max_worker_time AS max_cpu_time_us,
        query_stats.min_elapsed_time AS min_elapsed_time_us,
        query_stats.max_elapsed_time AS max_elapsed_time_us,
        query_stats.min_logical_reads AS min_logical_reads,
        query_stats.max_logical_reads AS max_logical_reads,
        query_stats.min_logical_writes AS min_logical_writes,
        query_stats.max_logical_writes AS max_logical_writes,
        sql_source.database_id AS database_id,
        DB_NAME(sql_source.database_id) AS database_name,
        sql_source.object_id AS object_id,
        sql_source.number AS statement_number,
        sql_source.batch_text AS batch_text,
        SUBSTRING
        (
            sql_source.batch_text,
            (query_stats.statement_start_offset / 2) + 1,
            CASE
                WHEN query_stats.statement_end_offset = -1
                    THEN (DATALENGTH(sql_source.batch_text) - query_stats.statement_start_offset) / 2 + 1
                ELSE (query_stats.statement_end_offset - query_stats.statement_start_offset) / 2 + 1
            END
        ) AS statement_text,
        plan_source.query_plan AS query_plan
    FROM sys.dm_exec_query_stats AS query_stats
    OUTER APPLY
    (
        SELECT
            database_id = sql_text.dbid,
            object_id = sql_text.objectid,
            number = sql_text.number,
            batch_text = CONVERT(nvarchar(max), sql_text.text)
        FROM sys.dm_exec_sql_text(query_stats.sql_handle) AS sql_text
    ) AS sql_source
    OUTER APPLY
    (
        SELECT
            query_plan = query_plan_data.query_plan
        FROM sys.dm_exec_query_plan(query_stats.plan_handle) AS query_plan_data
    ) AS plan_source
), RankedCachedQueryCosts AS
(
    SELECT
        cached_data.sql_handle AS sql_handle,
        cached_data.plan_handle AS plan_handle,
        cached_data.plan_creation_time AS plan_creation_time,
        cached_data.last_execution_time AS last_execution_time,
        cached_data.execution_count AS execution_count,
        cached_data.total_cpu_time_us AS total_cpu_time_us,
        cached_data.total_elapsed_time_us AS total_elapsed_time_us,
        cached_data.total_logical_reads AS total_logical_reads,
        cached_data.total_logical_writes AS total_logical_writes,
        cached_data.total_physical_reads AS total_physical_reads,
        cached_data.total_rows AS total_rows,
        cached_data.total_cpu_time_us / NULLIF(cached_data.execution_count, 0) AS average_cpu_time_us,
        cached_data.total_elapsed_time_us / NULLIF(cached_data.execution_count, 0) AS average_elapsed_time_us,
        cached_data.total_logical_reads / NULLIF(cached_data.execution_count, 0) AS average_logical_reads,
        cached_data.total_logical_writes / NULLIF(cached_data.execution_count, 0) AS average_logical_writes,
        cached_data.min_cpu_time_us AS min_cpu_time_us,
        cached_data.max_cpu_time_us AS max_cpu_time_us,
        cached_data.min_elapsed_time_us AS min_elapsed_time_us,
        cached_data.max_elapsed_time_us AS max_elapsed_time_us,
        cached_data.min_logical_reads AS min_logical_reads,
        cached_data.max_logical_reads AS max_logical_reads,
        cached_data.min_logical_writes AS min_logical_writes,
        cached_data.max_logical_writes AS max_logical_writes,
        cached_data.database_id AS database_id,
        cached_data.database_name AS database_name,
        cached_data.object_id AS object_id,
        cached_data.statement_number AS statement_number,
        cached_data.batch_text AS batch_text,
        cached_data.statement_text AS statement_text,
        cached_data.query_plan AS query_plan,
        ROW_NUMBER() OVER
        (
            ORDER BY
                cached_data.total_cpu_time_us DESC,
                cached_data.total_elapsed_time_us DESC,
                cached_data.execution_count DESC
        ) AS cpu_cost_rank
    FROM CachedQueryCosts AS cached_data
)
SELECT TOP (100)
    ranked_data.cpu_cost_rank AS cpu_cost_rank,
    ranked_data.sql_handle AS sql_handle,
    ranked_data.plan_handle AS plan_handle,
    ranked_data.plan_creation_time AS plan_creation_time,
    ranked_data.last_execution_time AS last_execution_time,
    ranked_data.execution_count AS execution_count,
    ranked_data.total_cpu_time_us AS total_cpu_time_us,
    ranked_data.total_elapsed_time_us AS total_elapsed_time_us,
    ranked_data.total_logical_reads AS total_logical_reads,
    ranked_data.total_logical_writes AS total_logical_writes,
    ranked_data.total_physical_reads AS total_physical_reads,
    ranked_data.total_rows AS total_rows,
    ranked_data.average_cpu_time_us AS average_cpu_time_us,
    ranked_data.average_elapsed_time_us AS average_elapsed_time_us,
    ranked_data.average_logical_reads AS average_logical_reads,
    ranked_data.average_logical_writes AS average_logical_writes,
    ranked_data.min_cpu_time_us AS min_cpu_time_us,
    ranked_data.max_cpu_time_us AS max_cpu_time_us,
    ranked_data.min_elapsed_time_us AS min_elapsed_time_us,
    ranked_data.max_elapsed_time_us AS max_elapsed_time_us,
    ranked_data.min_logical_reads AS min_logical_reads,
    ranked_data.max_logical_reads AS max_logical_reads,
    ranked_data.min_logical_writes AS min_logical_writes,
    ranked_data.max_logical_writes AS max_logical_writes,
    ranked_data.database_id AS database_id,
    ranked_data.database_name AS database_name,
    ranked_data.object_id AS object_id,
    ranked_data.statement_number AS statement_number,
    ranked_data.batch_text AS batch_text,
    ranked_data.statement_text AS statement_text,
    ranked_data.query_plan AS query_plan
FROM RankedCachedQueryCosts AS ranked_data
ORDER BY
    ranked_data.cpu_cost_rank ASC;