-- Purpose: Show the full statement text of the top cached statements by cumulative CPU.
-- Scope: Top 5 statements currently present in the SQL Server plan cache.
-- Text is emitted as ordered 32-character chunks so it survives fixed-width console rendering.
-- Results are point-in-time observations and disappear or reset as cache entries change.

;WITH TopCachedStats AS
(
    SELECT TOP (5)
        query_stats.sql_handle AS sql_handle,
        query_stats.statement_start_offset AS statement_start_offset,
        query_stats.statement_end_offset AS statement_end_offset,
        query_stats.execution_count AS execution_count,
        query_stats.total_worker_time AS total_cpu_time_us,
        query_stats.total_logical_reads AS total_logical_reads
    FROM sys.dm_exec_query_stats AS query_stats
    ORDER BY query_stats.total_worker_time DESC
), TopCachedStatements AS
(
    SELECT
        ROW_NUMBER() OVER (ORDER BY top_stats.total_cpu_time_us DESC) AS cpu_cost_rank,
        top_stats.execution_count AS execution_count,
        top_stats.total_cpu_time_us / NULLIF(top_stats.execution_count, 0) AS average_cpu_time_us,
        top_stats.total_logical_reads / NULLIF(top_stats.execution_count, 0) AS average_logical_reads,
        REPLACE(REPLACE(REPLACE(
            SUBSTRING
            (
                CONVERT(nvarchar(max), sql_text.text),
                (top_stats.statement_start_offset / 2) + 1,
                CASE
                    WHEN top_stats.statement_end_offset = -1
                        THEN (DATALENGTH(sql_text.text) - top_stats.statement_start_offset) / 2 + 1
                    ELSE (top_stats.statement_end_offset - top_stats.statement_start_offset) / 2 + 1
                END
            ),
            CHAR(13), N' '), CHAR(10), N' '), CHAR(9), N' ') AS statement_text
    FROM TopCachedStats AS top_stats
    CROSS APPLY sys.dm_exec_sql_text(top_stats.sql_handle) AS sql_text
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
    top_statements.cpu_cost_rank AS cpu_cost_rank,
    top_statements.execution_count AS execution_count,
    top_statements.average_cpu_time_us AS average_cpu_time_us,
    top_statements.average_logical_reads AS average_logical_reads,
    numbers.chunk_index AS chunk_index,
    SUBSTRING(top_statements.statement_text, numbers.chunk_index * 32 + 1, 32) AS statement_text_chunk_of_32_chars
FROM TopCachedStatements AS top_statements
INNER JOIN Numbers AS numbers
    ON numbers.chunk_index * 32 < LEN(top_statements.statement_text)
ORDER BY
    top_statements.cpu_cost_rank ASC,
    numbers.chunk_index ASC;
