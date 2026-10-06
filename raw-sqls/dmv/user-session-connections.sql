-- Purpose: Show how current user sessions are connected (transport, client address, port) and when they logged in.
-- Scope: Current user sessions and connections; point-in-time. Past or failed logins are not visible here.

SELECT
    session_data.session_id AS session_id,
    session_data.host_name AS host_name,
    session_data.program_name AS program_name,
    DB_NAME(session_data.database_id) AS database_name,
    session_data.status AS session_status,
    session_data.login_time AS login_time,
    session_data.last_request_end_time AS last_request_end_time,
    connection_data.net_transport AS net_transport,
    connection_data.protocol_type AS protocol_type,
    connection_data.encrypt_option AS encrypt_option,
    connection_data.auth_scheme AS auth_scheme,
    connection_data.client_net_address AS client_net_address,
    connection_data.local_net_address AS server_net_address,
    connection_data.local_tcp_port AS server_tcp_port,
    connection_data.connect_time AS connect_time
FROM sys.dm_exec_sessions AS session_data
LEFT JOIN sys.dm_exec_connections AS connection_data
    ON connection_data.session_id = session_data.session_id
WHERE session_data.is_user_process = 1
ORDER BY session_data.login_time DESC;
