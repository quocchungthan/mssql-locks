-- Aggregate-only history snapshot. Does not select session identity, program name, query hash, or query text.
DECLARE @snapshot_utc datetimeoffset(7) = TODATETIMEOFFSET(SYSUTCDATETIME(), '+00:00');

SELECT
    CAST(N'SEMAPHORE' AS nvarchar(16)) AS record_type,
    @snapshot_utc AS snapshot_utc,
    semaphore.pool_id AS pool_id,
    semaphore.resource_semaphore_id AS resource_semaphore_id,
    CAST(NULL AS nvarchar(16)) AS grant_state,
    CAST(NULL AS datetime) AS request_time,
    CAST(NULL AS datetime) AS grant_time,
    CAST(NULL AS int) AS wait_time_ms,
    CAST(NULL AS bigint) AS requested_memory_kb,
    CAST(NULL AS bigint) AS granted_memory_kb,
    semaphore.target_memory_kb AS semaphore_target_memory_kb,
    semaphore.available_memory_kb AS semaphore_available_memory_kb,
    semaphore.waiter_count AS semaphore_waiter_count
FROM sys.dm_exec_query_resource_semaphores AS semaphore

UNION ALL

SELECT
    CAST(N'GRANT' AS nvarchar(16)) AS record_type,
    @snapshot_utc AS snapshot_utc,
    memory_grant.pool_id AS pool_id,
    memory_grant.resource_semaphore_id AS resource_semaphore_id,
    CASE WHEN memory_grant.grant_time IS NULL THEN N'WAITING' ELSE N'GRANTED' END AS grant_state,
    memory_grant.request_time AS request_time,
    memory_grant.grant_time AS grant_time,
    memory_grant.wait_time_ms AS wait_time_ms,
    memory_grant.requested_memory_kb AS requested_memory_kb,
    memory_grant.granted_memory_kb AS granted_memory_kb,
    semaphore.target_memory_kb AS semaphore_target_memory_kb,
    semaphore.available_memory_kb AS semaphore_available_memory_kb,
    semaphore.waiter_count AS semaphore_waiter_count
FROM sys.dm_exec_query_memory_grants AS memory_grant
LEFT JOIN sys.dm_exec_query_resource_semaphores AS semaphore
    ON semaphore.pool_id = memory_grant.pool_id
    AND semaphore.resource_semaphore_id = memory_grant.resource_semaphore_id
ORDER BY record_type, grant_state, wait_time_ms DESC;