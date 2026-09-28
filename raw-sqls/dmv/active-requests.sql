-- Purpose: Show point-in-time active requests and their execution, wait, and SQL metadata.
-- Scope: Current user requests visible through the SQL Server dynamic management views.
-- Results are point-in-time observations and can change while this report is running.

;WITH ActiveRequestData AS
(
    SELECT
        session_data.session_id AS session_id,
        request_data.request_id AS request_id,
        session_data.login_name AS login_name,
        session_data.host_name AS host_name,
        session_data.program_name AS program_name,
        session_data.client_interface_name AS client_interface_name,
        request_data.database_id AS database_id,
        DB_NAME(request_data.database_id) AS database_name,
        request_data.command AS command_name,
        request_data.status AS request_engine_status,
        request_data.blocking_session_id AS blocking_session_id,
        request_data.wait_type AS wait_type,
        request_data.wait_time AS wait_time_ms,
        request_data.last_wait_type AS last_wait_type,
        request_data.wait_resource AS wait_resource,
        request_data.open_transaction_count AS open_transaction_count,
        request_data.transaction_isolation_level AS request_transaction_isolation_level,
        request_data.start_time AS request_start_time,
        request_data.total_elapsed_time AS total_elapsed_time_ms,
        request_data.cpu_time AS cpu_time_ms,
        request_data.reads AS reads,
        request_data.writes AS writes,
        request_data.logical_reads AS logical_reads,
        request_data.row_count AS row_count,
        request_data.percent_complete AS percent_complete,
        request_data.sql_handle AS sql_handle,
        request_data.plan_handle AS plan_handle,
        sql_source.batch_text AS batch_text,
        SUBSTRING
        (
            sql_source.batch_text,
            (request_data.statement_start_offset / 2) + 1,
            CASE
                WHEN request_data.statement_end_offset = -1
                    THEN (DATALENGTH(sql_source.batch_text) - request_data.statement_start_offset) / 2 + 1
                ELSE (request_data.statement_end_offset - request_data.statement_start_offset) / 2 + 1
            END
        ) AS statement_text
    FROM sys.dm_exec_requests AS request_data
    INNER JOIN sys.dm_exec_sessions AS session_data
        ON session_data.session_id = request_data.session_id
    OUTER APPLY
    (
        SELECT
            batch_text = CONVERT(nvarchar(max), sql_text.text)
        FROM sys.dm_exec_sql_text(request_data.sql_handle) AS sql_text
    ) AS sql_source
    WHERE request_data.session_id <> @@SPID
), ClassifiedRequests AS
(
    SELECT
        active_data.session_id AS session_id,
        active_data.request_id AS request_id,
        active_data.login_name AS login_name,
        active_data.host_name AS host_name,
        active_data.program_name AS program_name,
        active_data.client_interface_name AS client_interface_name,
        active_data.database_id AS database_id,
        active_data.database_name AS database_name,
        active_data.command_name AS command_name,
        active_data.request_engine_status AS request_engine_status,
        CASE
            WHEN active_data.blocking_session_id IS NOT NULL
                 AND active_data.blocking_session_id <> 0 THEN N'BLOCKED'
            WHEN active_data.wait_type IS NOT NULL THEN N'WAITING'
            WHEN active_data.request_engine_status IN (N'running', N'runnable') THEN N'RUNNING'
            ELSE N'ACTIVE'
        END AS derived_request_status,
        active_data.blocking_session_id AS blocking_session_id,
        active_data.wait_type AS wait_type,
        active_data.wait_time_ms AS wait_time_ms,
        active_data.last_wait_type AS last_wait_type,
        active_data.wait_resource AS wait_resource,
        active_data.open_transaction_count AS open_transaction_count,
        active_data.request_transaction_isolation_level AS request_transaction_isolation_level,
        active_data.request_start_time AS request_start_time,
        active_data.total_elapsed_time_ms AS total_elapsed_time_ms,
        active_data.cpu_time_ms AS cpu_time_ms,
        active_data.reads AS reads,
        active_data.writes AS writes,
        active_data.logical_reads AS logical_reads,
        active_data.row_count AS row_count,
        active_data.percent_complete AS percent_complete,
        active_data.sql_handle AS sql_handle,
        active_data.plan_handle AS plan_handle,
        active_data.batch_text AS batch_text,
        active_data.statement_text AS statement_text
    FROM ActiveRequestData AS active_data
)
SELECT
    classified_data.session_id AS session_id,
    classified_data.request_id AS request_id,
    classified_data.login_name AS login_name,
    classified_data.host_name AS host_name,
    classified_data.program_name AS program_name,
    classified_data.client_interface_name AS client_interface_name,
    classified_data.database_id AS database_id,
    classified_data.database_name AS database_name,
    classified_data.command_name AS command_name,
    classified_data.request_engine_status AS request_engine_status,
    classified_data.derived_request_status AS derived_request_status,
    classified_data.blocking_session_id AS blocking_session_id,
    classified_data.wait_type AS wait_type,
    classified_data.wait_time_ms AS wait_time_ms,
    classified_data.last_wait_type AS last_wait_type,
    classified_data.wait_resource AS wait_resource,
    classified_data.open_transaction_count AS open_transaction_count,
    classified_data.request_transaction_isolation_level AS request_transaction_isolation_level,
    classified_data.request_start_time AS request_start_time,
    classified_data.total_elapsed_time_ms AS total_elapsed_time_ms,
    classified_data.cpu_time_ms AS cpu_time_ms,
    classified_data.reads AS reads,
    classified_data.writes AS writes,
    classified_data.logical_reads AS logical_reads,
    classified_data.row_count AS row_count,
    classified_data.percent_complete AS percent_complete,
    classified_data.sql_handle AS sql_handle,
    classified_data.plan_handle AS plan_handle,
    classified_data.batch_text AS batch_text,
    classified_data.statement_text AS statement_text
FROM ClassifiedRequests AS classified_data
ORDER BY
    classified_data.derived_request_status ASC,
    classified_data.total_elapsed_time_ms DESC;