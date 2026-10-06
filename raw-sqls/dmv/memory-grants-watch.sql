-- Generic SQL Server memory-grant snapshot. Excludes session identity and query text.
SELECT
    CASE WHEN memory_grant.grant_time IS NULL THEN N'WAITING' ELSE N'GRANTED' END AS grant_state,
    memory_grant.request_time AS request_time,
    memory_grant.grant_time AS grant_time,
    memory_grant.wait_time_ms AS wait_time_ms,
    memory_grant.requested_memory_kb AS requested_memory_kb,
    memory_grant.granted_memory_kb AS granted_memory_kb,
    memory_grant.used_memory_kb AS used_memory_kb,
    memory_grant.max_used_memory_kb AS max_used_memory_kb,
    memory_grant.ideal_memory_kb AS ideal_memory_kb,
    memory_grant.dop AS degree_of_parallelism,
    memory_grant.query_cost AS query_cost,
    semaphore.target_memory_kb AS semaphore_target_memory_kb,
    semaphore.available_memory_kb AS semaphore_available_memory_kb,
    semaphore.granted_memory_kb AS semaphore_granted_memory_kb,
    semaphore.grantee_count AS semaphore_grantee_count,
    semaphore.waiter_count AS semaphore_waiter_count
FROM sys.dm_exec_query_memory_grants AS memory_grant
LEFT JOIN sys.dm_exec_query_resource_semaphores AS semaphore
    ON semaphore.resource_semaphore_id = memory_grant.resource_semaphore_id
    AND semaphore.pool_id = memory_grant.pool_id
ORDER BY
    CASE WHEN memory_grant.grant_time IS NULL THEN 0 ELSE 1 END,
    memory_grant.granted_memory_kb DESC,
    memory_grant.wait_time_ms DESC;