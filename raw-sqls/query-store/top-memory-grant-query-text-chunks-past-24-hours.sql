-- Purpose: Show the full text of the top 3 queries by maximum query memory (grant) used in the past 24 hours.
-- Scope: Current database; Query Store must be enabled. Diagnostic queries over sys.dm_* and sys.query_store_* are excluded.
-- Text is emitted as ordered 32-character chunks so it survives fixed-width console rendering.

;WITH QueryMemory AS
(
    SELECT
        plan_data.query_id AS query_id,
        MAX(runtime_stats.max_query_max_used_memory) * 8 AS max_used_memory_kb
    FROM sys.query_store_runtime_stats AS runtime_stats
    INNER JOIN sys.query_store_plan AS plan_data
        ON plan_data.plan_id = runtime_stats.plan_id
    WHERE runtime_stats.last_execution_time >= DATEADD(HOUR, -24, SYSUTCDATETIME())
    GROUP BY plan_data.query_id
), TopQueries AS
(
    SELECT TOP (3)
        ROW_NUMBER() OVER (ORDER BY query_memory.max_used_memory_kb DESC) AS memory_rank,
        query_memory.query_id AS query_id,
        query_memory.max_used_memory_kb AS max_used_memory_kb,
        REPLACE(REPLACE(REPLACE(query_text.query_sql_text, CHAR(13), N' '), CHAR(10), N' '), CHAR(9), N' ') AS query_text
    FROM QueryMemory AS query_memory
    INNER JOIN sys.query_store_query AS query_data
        ON query_data.query_id = query_memory.query_id
    INNER JOIN sys.query_store_query_text AS query_text
        ON query_text.query_text_id = query_data.query_text_id
    WHERE query_text.query_sql_text NOT LIKE N'%sys.dm[_]%'
        AND query_text.query_sql_text NOT LIKE N'%sys.query[_]store[_]%'
    ORDER BY query_memory.max_used_memory_kb DESC
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
    top_queries.memory_rank AS memory_rank,
    top_queries.query_id AS query_id,
    top_queries.max_used_memory_kb AS max_used_memory_kb,
    numbers.chunk_index AS chunk_index,
    SUBSTRING(top_queries.query_text, numbers.chunk_index * 32 + 1, 32) AS statement_text_chunk_of_32_chars
FROM TopQueries AS top_queries
INNER JOIN Numbers AS numbers
    ON numbers.chunk_index * 32 < LEN(top_queries.query_text)
ORDER BY
    top_queries.memory_rank ASC,
    numbers.chunk_index ASC;
