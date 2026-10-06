-- Purpose: Show the full text of the top Query Store queries by total CPU in the past 24 hours.
-- Scope: Top 5 queries retained by Query Store in the current database; Query Store must be enabled.
-- Diagnostic queries over sys.dm_* and sys.query_store_* views are excluded.
-- Text is emitted as ordered 32-character chunks so it survives fixed-width console rendering.

;WITH RecentQueryCosts AS
(
    SELECT
        plan_data.query_id AS query_id,
        SUM(runtime_stats.count_executions) AS execution_count,
        SUM(runtime_stats.avg_cpu_time * runtime_stats.count_executions) AS total_cpu_time_us,
        SUM(runtime_stats.avg_logical_io_reads * runtime_stats.count_executions) AS total_logical_reads,
        MAX(runtime_stats.max_duration) AS max_duration_us,
        MAX(runtime_interval.end_time) AS last_interval_end_utc
    FROM sys.query_store_runtime_stats AS runtime_stats
    INNER JOIN sys.query_store_runtime_stats_interval AS runtime_interval
        ON runtime_interval.runtime_stats_interval_id = runtime_stats.runtime_stats_interval_id
    INNER JOIN sys.query_store_plan AS plan_data
        ON plan_data.plan_id = runtime_stats.plan_id
    WHERE runtime_interval.end_time >= DATEADD(HOUR, -24, SYSUTCDATETIME())
    GROUP BY plan_data.query_id
), TopQueries AS
(
    SELECT TOP (5)
        ROW_NUMBER() OVER (ORDER BY cost_data.total_cpu_time_us DESC) AS cpu_cost_rank,
        cost_data.query_id AS query_id,
        cost_data.execution_count AS execution_count,
        CONVERT(bigint, cost_data.total_cpu_time_us / NULLIF(cost_data.execution_count, 0)) AS average_cpu_time_us,
        CONVERT(bigint, cost_data.total_logical_reads / NULLIF(cost_data.execution_count, 0)) AS average_logical_reads,
        cost_data.max_duration_us AS max_duration_us,
        cost_data.last_interval_end_utc AS last_interval_end_utc,
        REPLACE(REPLACE(REPLACE(query_text.query_sql_text, CHAR(13), N' '), CHAR(10), N' '), CHAR(9), N' ') AS query_text
    FROM RecentQueryCosts AS cost_data
    INNER JOIN sys.query_store_query AS query_data
        ON query_data.query_id = cost_data.query_id
    INNER JOIN sys.query_store_query_text AS query_text
        ON query_text.query_text_id = query_data.query_text_id
    WHERE query_text.query_sql_text NOT LIKE N'%sys.dm[_]%'
        AND query_text.query_sql_text NOT LIKE N'%sys.query[_]store[_]%'
    ORDER BY cost_data.total_cpu_time_us DESC
), Digits AS
(
    SELECT digit FROM (VALUES (0), (1), (2), (3), (4), (5), (6), (7), (8), (9)) AS digit_values (digit)
), Numbers AS
(
    SELECT thousands.digit * 1000 + hundreds.digit * 100 + tens.digit * 10 + ones.digit AS chunk_index
    FROM Digits AS thousands
    CROSS JOIN Digits AS hundreds
    CROSS JOIN Digits AS tens
    CROSS JOIN Digits AS ones
)
SELECT
    top_queries.cpu_cost_rank AS cpu_cost_rank,
    top_queries.query_id AS query_id,
    top_queries.execution_count AS execution_count,
    top_queries.average_cpu_time_us AS average_cpu_time_us,
    top_queries.average_logical_reads AS average_logical_reads,
    top_queries.max_duration_us AS max_duration_us,
    top_queries.last_interval_end_utc AS last_interval_end_utc,
    numbers.chunk_index AS chunk_index,
    SUBSTRING(top_queries.query_text, numbers.chunk_index * 32 + 1, 32) AS statement_text_chunk_of_32_chars
FROM TopQueries AS top_queries
INNER JOIN Numbers AS numbers
    ON numbers.chunk_index * 32 < LEN(top_queries.query_text)
ORDER BY
    top_queries.cpu_cost_rank ASC,
    numbers.chunk_index ASC;
