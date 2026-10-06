-- Read-only snapshot: one semaphore event per resource semaphore plus active grant events.
-- query_hash is best-effort from the current plan cache, not a Query Store query_id.
DECLARE @snapshot_utc datetimeoffset(7) = TODATETIMEOFFSET(SYSUTCDATETIME(), '+00:00');

SELECT
    CAST(N'SEMAPHORE' AS nvarchar(16)) AS record_type,
    @snapshot_utc AS snapshot_utc,
    semaphore.pool_id AS pool_id,
    semaphore.resource_semaphore_id AS resource_semaphore_id,
    CAST(NULL AS int) AS session_id,
    CAST(NULL AS int) AS request_id,
    CAST(NULL AS nvarchar(128)) AS program_name,
    CAST(NULL AS nvarchar(16)) AS grant_state,
    CAST(NULL AS datetime) AS request_time,
    CAST(NULL AS datetime) AS grant_time,
    CAST(NULL AS int) AS wait_time_ms,
    CAST(NULL AS bigint) AS requested_memory_kb,
    CAST(NULL AS bigint) AS required_memory_kb,
    CAST(NULL AS bigint) AS granted_memory_kb,
    CAST(NULL AS bigint) AS used_memory_kb,
    CAST(NULL AS bigint) AS ideal_memory_kb,
    semaphore.target_memory_kb AS semaphore_target_memory_kb,
    semaphore.available_memory_kb AS semaphore_available_memory_kb,
    semaphore.granted_memory_kb AS semaphore_granted_memory_kb,
    semaphore.grantee_count AS semaphore_grantee_count,
    semaphore.waiter_count AS semaphore_waiter_count,
    CAST(NULL AS varchar(18)) AS query_hash,
    CAST(NULL AS nvarchar(2000)) AS query_text
FROM sys.dm_exec_query_resource_semaphores AS semaphore

UNION ALL

SELECT
    CAST(N'GRANT' AS nvarchar(16)) AS record_type,
    @snapshot_utc AS snapshot_utc,
    memory_grant.pool_id AS pool_id,
    memory_grant.resource_semaphore_id AS resource_semaphore_id,
    CONVERT(int, memory_grant.session_id) AS session_id,
    memory_grant.request_id AS request_id,
    session_data.program_name AS program_name,
    CASE WHEN memory_grant.grant_time IS NULL THEN N'WAITING' ELSE N'GRANTED' END AS grant_state,
    memory_grant.request_time AS request_time,
    memory_grant.grant_time AS grant_time,
    memory_grant.wait_time_ms AS wait_time_ms,
    memory_grant.requested_memory_kb AS requested_memory_kb,
    memory_grant.required_memory_kb AS required_memory_kb,
    memory_grant.granted_memory_kb AS granted_memory_kb,
    memory_grant.used_memory_kb AS used_memory_kb,
    memory_grant.ideal_memory_kb AS ideal_memory_kb,
    semaphore.target_memory_kb AS semaphore_target_memory_kb,
    semaphore.available_memory_kb AS semaphore_available_memory_kb,
    semaphore.granted_memory_kb AS semaphore_granted_memory_kb,
    semaphore.grantee_count AS semaphore_grantee_count,
    semaphore.waiter_count AS semaphore_waiter_count,
    CONVERT(varchar(18), cached_query.query_hash, 1) AS query_hash,
    CASE
        WHEN @IncludeQueryText = 1
            THEN LEFT(REPLACE(REPLACE(query_text.text, CHAR(13), N' '), CHAR(10), N' '), 2000)
        ELSE CAST(NULL AS nvarchar(2000))
    END AS query_text
FROM sys.dm_exec_query_memory_grants AS memory_grant
LEFT JOIN sys.dm_exec_query_resource_semaphores AS semaphore
    ON semaphore.pool_id = memory_grant.pool_id
    AND semaphore.resource_semaphore_id = memory_grant.resource_semaphore_id
LEFT JOIN sys.dm_exec_sessions AS session_data
    ON session_data.session_id = memory_grant.session_id
LEFT JOIN sys.dm_exec_requests AS active_request
    ON active_request.session_id = memory_grant.session_id
    AND active_request.request_id = memory_grant.request_id
OUTER APPLY
(
    SELECT TOP (1) query_stats.query_hash
    FROM sys.dm_exec_query_stats AS query_stats
    WHERE active_request.sql_handle IS NOT NULL
        AND active_request.plan_handle IS NOT NULL
        AND query_stats.sql_handle = active_request.sql_handle
        AND query_stats.plan_handle = active_request.plan_handle
        AND query_stats.statement_start_offset = active_request.statement_start_offset
        AND query_stats.statement_end_offset = active_request.statement_end_offset
    ORDER BY query_stats.last_execution_time DESC
) AS cached_query
OUTER APPLY
(
    SELECT sql_text.text
    FROM sys.dm_exec_sql_text(active_request.sql_handle) AS sql_text
    WHERE @IncludeQueryText = 1
        AND active_request.sql_handle IS NOT NULL
) AS query_text
ORDER BY record_type, grant_state, wait_time_ms DESC;