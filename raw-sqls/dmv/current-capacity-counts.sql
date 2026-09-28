-- Purpose: Show current engine activity and capacity counts in one snapshot.
-- Scope: Server-wide sessions, connections, requests, transactions, tasks, workers, and visible schedulers.
-- The observer connection is included in connection and session counts but excluded from request counts.

;WITH SessionCounts AS
(
    SELECT
        COUNT_BIG(*) AS sessions_total,
        SUM(CASE WHEN session_data.is_user_process = 1 THEN CONVERT(bigint, 1) ELSE 0 END) AS user_sessions,
        SUM(CASE WHEN session_data.is_user_process = 1 AND session_data.status = N'running' THEN CONVERT(bigint, 1) ELSE 0 END) AS running_user_sessions,
        SUM(CASE WHEN session_data.is_user_process = 1 AND session_data.status = N'sleeping' THEN CONVERT(bigint, 1) ELSE 0 END) AS sleeping_user_sessions
    FROM sys.dm_exec_sessions AS session_data
), ConnectionCounts AS
(
    SELECT
        COUNT_BIG(*) AS connections_total,
        SUM(CASE WHEN session_data.is_user_process = 1 THEN CONVERT(bigint, 1) ELSE 0 END) AS user_connections
    FROM sys.dm_exec_connections AS connection_data
    LEFT JOIN sys.dm_exec_sessions AS session_data
        ON session_data.session_id = connection_data.session_id
), RequestCounts AS
(
    SELECT
        COUNT_BIG(*) AS requests_total,
        SUM(CASE WHEN session_data.is_user_process = 1 THEN CONVERT(bigint, 1) ELSE 0 END) AS user_requests,
        SUM(CASE WHEN request_data.blocking_session_id > 0 THEN CONVERT(bigint, 1) ELSE 0 END) AS blocked_requests,
        SUM(CASE WHEN request_data.status = N'running' THEN CONVERT(bigint, 1) ELSE 0 END) AS running_requests,
        SUM(CASE WHEN request_data.status = N'runnable' THEN CONVERT(bigint, 1) ELSE 0 END) AS runnable_requests,
        SUM(CASE WHEN request_data.status = N'suspended' THEN CONVERT(bigint, 1) ELSE 0 END) AS suspended_requests
    FROM sys.dm_exec_requests AS request_data
    LEFT JOIN sys.dm_exec_sessions AS session_data
        ON session_data.session_id = request_data.session_id
    WHERE request_data.session_id <> @@SPID
), TransactionCounts AS
(
    SELECT
        COUNT_BIG(*) AS active_transactions
    FROM sys.dm_tran_active_transactions
), SessionTransactionCounts AS
(
    SELECT
        COUNT_BIG(DISTINCT transaction_data.transaction_id) AS session_transactions,
        COUNT_BIG(DISTINCT transaction_data.session_id) AS sessions_with_transactions
    FROM sys.dm_tran_session_transactions AS transaction_data
), TaskCounts AS
(
    SELECT
        COUNT_BIG(*) AS tasks_total,
        SUM(CASE WHEN task_data.task_state = N'RUNNING' THEN CONVERT(bigint, 1) ELSE 0 END) AS running_tasks,
        SUM(CASE WHEN task_data.task_state = N'RUNNABLE' THEN CONVERT(bigint, 1) ELSE 0 END) AS runnable_tasks,
        SUM(CASE WHEN task_data.task_state = N'SUSPENDED' THEN CONVERT(bigint, 1) ELSE 0 END) AS suspended_tasks
    FROM sys.dm_os_tasks AS task_data
), WorkerCounts AS
(
    SELECT
        COUNT_BIG(*) AS workers_total,
        SUM(CASE WHEN worker_data.state = N'RUNNING' THEN CONVERT(bigint, 1) ELSE 0 END) AS running_workers,
        SUM(CASE WHEN worker_data.state = N'RUNNABLE' THEN CONVERT(bigint, 1) ELSE 0 END) AS runnable_workers,
        SUM(CASE WHEN worker_data.state = N'SUSPENDED' THEN CONVERT(bigint, 1) ELSE 0 END) AS suspended_workers
    FROM sys.dm_os_workers AS worker_data
), SchedulerCounts AS
(
    SELECT
        COUNT_BIG(*) AS visible_online_schedulers,
        SUM(CONVERT(bigint, scheduler_data.current_tasks_count)) AS scheduler_current_tasks,
        SUM(CONVERT(bigint, scheduler_data.active_workers_count)) AS scheduler_active_workers,
        SUM(CONVERT(bigint, scheduler_data.runnable_tasks_count)) AS scheduler_runnable_tasks,
        SUM(CONVERT(bigint, scheduler_data.work_queue_count)) AS scheduler_work_queue
    FROM sys.dm_os_schedulers AS scheduler_data
    WHERE scheduler_data.status = N'VISIBLE ONLINE'
)
SELECT
    SYSUTCDATETIME() AS observed_at_utc,
    session_counts.sessions_total,
    session_counts.user_sessions,
    session_counts.running_user_sessions,
    session_counts.sleeping_user_sessions,
    connection_counts.connections_total,
    connection_counts.user_connections,
    request_counts.requests_total,
    request_counts.user_requests,
    request_counts.blocked_requests,
    request_counts.running_requests,
    request_counts.runnable_requests,
    request_counts.suspended_requests,
    transaction_counts.active_transactions,
    session_transaction_counts.session_transactions,
    session_transaction_counts.sessions_with_transactions,
    task_counts.tasks_total,
    task_counts.running_tasks,
    task_counts.runnable_tasks,
    task_counts.suspended_tasks,
    worker_counts.workers_total,
    worker_counts.running_workers,
    worker_counts.runnable_workers,
    worker_counts.suspended_workers,
    scheduler_counts.visible_online_schedulers,
    scheduler_counts.scheduler_current_tasks,
    scheduler_counts.scheduler_active_workers,
    scheduler_counts.scheduler_runnable_tasks,
    scheduler_counts.scheduler_work_queue
FROM SessionCounts AS session_counts
CROSS JOIN ConnectionCounts AS connection_counts
CROSS JOIN RequestCounts AS request_counts
CROSS JOIN TransactionCounts AS transaction_counts
CROSS JOIN SessionTransactionCounts AS session_transaction_counts
CROSS JOIN TaskCounts AS task_counts
CROSS JOIN WorkerCounts AS worker_counts
CROSS JOIN SchedulerCounts AS scheduler_counts;
