-- Purpose: Show point-in-time blocking relationships with both request sides and SQL text.
-- Scope: Current requests reporting a blocking session, including available transaction context.
-- Results are point-in-time observations and can change while this report is running.

;WITH BlockingRelationshipData AS
(
    SELECT
        blocked_session.session_id AS blocked_session_id,
        blocked_request.request_id AS blocked_request_id,
        blocked_session.login_name AS blocked_login_name,
        blocked_session.host_name AS blocked_host_name,
        blocked_session.program_name AS blocked_program_name,
        blocked_request.database_id AS blocked_database_id,
        DB_NAME(blocked_request.database_id) AS blocked_database_name,
        blocked_request.command AS blocked_command_name,
        blocked_request.status AS blocked_request_status,
        blocked_request.start_time AS blocked_request_start_time,
        blocked_request.total_elapsed_time AS blocked_elapsed_time_ms,
        blocked_request.cpu_time AS blocked_cpu_time_ms,
        blocked_request.reads AS blocked_reads,
        blocked_request.writes AS blocked_writes,
        blocked_request.wait_type AS blocked_wait_type,
        blocked_request.last_wait_type AS blocked_last_wait_type,
        blocked_request.wait_time AS blocked_wait_time_ms,
        blocked_request.wait_resource AS blocked_wait_resource,
        blocked_request.open_transaction_count AS blocked_open_transaction_count,
        blocked_request.transaction_isolation_level AS blocked_transaction_isolation_level,
        blocked_request.transaction_id AS blocked_transaction_id,
        blocked_request.sql_handle AS blocked_sql_handle,
        blocked_request.plan_handle AS blocked_plan_handle,
        blocked_sql.batch_text AS blocked_batch_text,
        SUBSTRING
        (
            blocked_sql.batch_text,
            (blocked_request.statement_start_offset / 2) + 1,
            CASE
                WHEN blocked_request.statement_end_offset = -1
                    THEN (DATALENGTH(blocked_sql.batch_text) - blocked_request.statement_start_offset) / 2 + 1
                ELSE (blocked_request.statement_end_offset - blocked_request.statement_start_offset) / 2 + 1
            END
        ) AS blocked_statement_text,
        blocker_session.session_id AS blocking_session_id,
        blocker_session.login_name AS blocking_login_name,
        blocker_session.host_name AS blocking_host_name,
        blocker_session.program_name AS blocking_program_name,
        blocker_request.request_id AS blocking_request_id,
        blocker_request.database_id AS blocking_database_id,
        DB_NAME(blocker_request.database_id) AS blocking_database_name,
        blocker_request.command AS blocking_command_name,
        blocker_request.status AS blocking_request_status,
        blocker_request.start_time AS blocking_request_start_time,
        blocker_request.total_elapsed_time AS blocking_elapsed_time_ms,
        blocker_request.cpu_time AS blocking_cpu_time_ms,
        blocker_request.reads AS blocking_reads,
        blocker_request.writes AS blocking_writes,
        blocker_request.wait_type AS blocking_wait_type,
        blocker_request.last_wait_type AS blocking_last_wait_type,
        blocker_request.wait_time AS blocking_wait_time_ms,
        blocker_request.wait_resource AS blocking_wait_resource,
        blocker_session.open_transaction_count AS blocking_open_transaction_count,
        blocker_request.transaction_isolation_level AS blocking_transaction_isolation_level,
        blocker_request.transaction_id AS blocking_transaction_id,
        blocker_request.sql_handle AS blocking_sql_handle,
        blocker_request.plan_handle AS blocking_plan_handle,
        blocker_sql.batch_text AS blocking_batch_text,
        SUBSTRING
        (
            blocker_sql.batch_text,
            (blocker_request.statement_start_offset / 2) + 1,
            CASE
                WHEN blocker_request.statement_end_offset = -1
                    THEN (DATALENGTH(blocker_sql.batch_text) - blocker_request.statement_start_offset) / 2 + 1
                ELSE (blocker_request.statement_end_offset - blocker_request.statement_start_offset) / 2 + 1
            END
        ) AS blocking_statement_text
    FROM sys.dm_exec_requests AS blocked_request
    INNER JOIN sys.dm_exec_sessions AS blocked_session
        ON blocked_session.session_id = blocked_request.session_id
    LEFT JOIN sys.dm_exec_sessions AS blocker_session
        ON blocker_session.session_id = NULLIF(blocked_request.blocking_session_id, 0)
    LEFT JOIN sys.dm_exec_requests AS blocker_request
        ON blocker_request.session_id = NULLIF(blocked_request.blocking_session_id, 0)
    OUTER APPLY
    (
        SELECT
            batch_text = CONVERT(nvarchar(max), blocked_sql_source.text)
        FROM sys.dm_exec_sql_text(blocked_request.sql_handle) AS blocked_sql_source
    ) AS blocked_sql
    OUTER APPLY
    (
        SELECT
            batch_text = CONVERT(nvarchar(max), blocking_sql_source.text)
        FROM sys.dm_exec_sql_text(blocker_request.sql_handle) AS blocking_sql_source
    ) AS blocker_sql
    WHERE blocked_request.blocking_session_id > 0
), BlockingChains AS
(
    SELECT
        relationship_data.blocked_session_id AS blocked_session_id,
        relationship_data.blocked_request_id AS blocked_request_id,
        relationship_data.blocked_login_name AS blocked_login_name,
        relationship_data.blocked_host_name AS blocked_host_name,
        relationship_data.blocked_program_name AS blocked_program_name,
        relationship_data.blocked_database_id AS blocked_database_id,
        relationship_data.blocked_database_name AS blocked_database_name,
        relationship_data.blocked_command_name AS blocked_command_name,
        relationship_data.blocked_request_status AS blocked_request_status,
        relationship_data.blocked_request_start_time AS blocked_request_start_time,
        relationship_data.blocked_elapsed_time_ms AS blocked_elapsed_time_ms,
        relationship_data.blocked_cpu_time_ms AS blocked_cpu_time_ms,
        relationship_data.blocked_reads AS blocked_reads,
        relationship_data.blocked_writes AS blocked_writes,
        relationship_data.blocked_wait_type AS blocked_wait_type,
        relationship_data.blocked_last_wait_type AS blocked_last_wait_type,
        relationship_data.blocked_wait_time_ms AS blocked_wait_time_ms,
        relationship_data.blocked_wait_resource AS blocked_wait_resource,
        relationship_data.blocked_open_transaction_count AS blocked_open_transaction_count,
        relationship_data.blocked_transaction_isolation_level AS blocked_transaction_isolation_level,
        relationship_data.blocked_transaction_id AS blocked_transaction_id,
        relationship_data.blocked_sql_handle AS blocked_sql_handle,
        relationship_data.blocked_plan_handle AS blocked_plan_handle,
        relationship_data.blocked_batch_text AS blocked_batch_text,
        relationship_data.blocked_statement_text AS blocked_statement_text,
        relationship_data.blocking_session_id AS blocking_session_id,
        relationship_data.blocking_login_name AS blocking_login_name,
        relationship_data.blocking_host_name AS blocking_host_name,
        relationship_data.blocking_program_name AS blocking_program_name,
        relationship_data.blocking_request_id AS blocking_request_id,
        relationship_data.blocking_database_id AS blocking_database_id,
        relationship_data.blocking_database_name AS blocking_database_name,
        relationship_data.blocking_command_name AS blocking_command_name,
        relationship_data.blocking_request_status AS blocking_request_status,
        relationship_data.blocking_request_start_time AS blocking_request_start_time,
        relationship_data.blocking_elapsed_time_ms AS blocking_elapsed_time_ms,
        relationship_data.blocking_cpu_time_ms AS blocking_cpu_time_ms,
        relationship_data.blocking_reads AS blocking_reads,
        relationship_data.blocking_writes AS blocking_writes,
        relationship_data.blocking_wait_type AS blocking_wait_type,
        relationship_data.blocking_last_wait_type AS blocking_last_wait_type,
        relationship_data.blocking_wait_time_ms AS blocking_wait_time_ms,
        relationship_data.blocking_wait_resource AS blocking_wait_resource,
        relationship_data.blocking_open_transaction_count AS blocking_open_transaction_count,
        relationship_data.blocking_transaction_isolation_level AS blocking_transaction_isolation_level,
        relationship_data.blocking_transaction_id AS blocking_transaction_id,
        relationship_data.blocking_sql_handle AS blocking_sql_handle,
        relationship_data.blocking_plan_handle AS blocking_plan_handle,
        relationship_data.blocking_batch_text AS blocking_batch_text,
        relationship_data.blocking_statement_text AS blocking_statement_text
    FROM BlockingRelationshipData AS relationship_data
)
SELECT
    blocking_data.blocked_session_id AS blocked_session_id,
    blocking_data.blocked_request_id AS blocked_request_id,
    blocking_data.blocked_login_name AS blocked_login_name,
    blocking_data.blocked_host_name AS blocked_host_name,
    blocking_data.blocked_program_name AS blocked_program_name,
    blocking_data.blocked_database_id AS blocked_database_id,
    blocking_data.blocked_database_name AS blocked_database_name,
    blocking_data.blocked_command_name AS blocked_command_name,
    blocking_data.blocked_request_status AS blocked_request_status,
    blocking_data.blocked_request_start_time AS blocked_request_start_time,
    blocking_data.blocked_elapsed_time_ms AS blocked_elapsed_time_ms,
    blocking_data.blocked_cpu_time_ms AS blocked_cpu_time_ms,
    blocking_data.blocked_reads AS blocked_reads,
    blocking_data.blocked_writes AS blocked_writes,
    blocking_data.blocked_wait_type AS blocked_wait_type,
    blocking_data.blocked_last_wait_type AS blocked_last_wait_type,
    blocking_data.blocked_wait_time_ms AS blocked_wait_time_ms,
    blocking_data.blocked_wait_resource AS blocked_wait_resource,
    blocking_data.blocked_open_transaction_count AS blocked_open_transaction_count,
    blocking_data.blocked_transaction_isolation_level AS blocked_transaction_isolation_level,
    blocking_data.blocked_transaction_id AS blocked_transaction_id,
    blocking_data.blocked_sql_handle AS blocked_sql_handle,
    blocking_data.blocked_plan_handle AS blocked_plan_handle,
    blocking_data.blocked_batch_text AS blocked_batch_text,
    blocking_data.blocked_statement_text AS blocked_statement_text,
    blocking_data.blocking_session_id AS blocking_session_id,
    blocking_data.blocking_login_name AS blocking_login_name,
    blocking_data.blocking_host_name AS blocking_host_name,
    blocking_data.blocking_program_name AS blocking_program_name,
    blocking_data.blocking_request_id AS blocking_request_id,
    blocking_data.blocking_database_id AS blocking_database_id,
    blocking_data.blocking_database_name AS blocking_database_name,
    blocking_data.blocking_command_name AS blocking_command_name,
    blocking_data.blocking_request_status AS blocking_request_status,
    blocking_data.blocking_request_start_time AS blocking_request_start_time,
    blocking_data.blocking_elapsed_time_ms AS blocking_elapsed_time_ms,
    blocking_data.blocking_cpu_time_ms AS blocking_cpu_time_ms,
    blocking_data.blocking_reads AS blocking_reads,
    blocking_data.blocking_writes AS blocking_writes,
    blocking_data.blocking_wait_type AS blocking_wait_type,
    blocking_data.blocking_last_wait_type AS blocking_last_wait_type,
    blocking_data.blocking_wait_time_ms AS blocking_wait_time_ms,
    blocking_data.blocking_wait_resource AS blocking_wait_resource,
    blocking_data.blocking_open_transaction_count AS blocking_open_transaction_count,
    blocking_data.blocking_transaction_isolation_level AS blocking_transaction_isolation_level,
    blocking_data.blocking_transaction_id AS blocking_transaction_id,
    blocking_data.blocking_sql_handle AS blocking_sql_handle,
    blocking_data.blocking_plan_handle AS blocking_plan_handle,
    blocking_data.blocking_batch_text AS blocking_batch_text,
    blocking_data.blocking_statement_text AS blocking_statement_text
FROM BlockingChains AS blocking_data
ORDER BY
    blocking_data.blocked_wait_time_ms DESC,
    blocking_data.blocked_session_id ASC;