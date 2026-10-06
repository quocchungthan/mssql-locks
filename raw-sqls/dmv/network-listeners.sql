-- Purpose: Show which network endpoints SQL Server is listening on.
-- Scope: Current listener state from sys.dm_tcp_listener_states; point-in-time.

SELECT
    listener_state.listener_id AS listener_id,
    listener_state.ip_address AS ip_address,
    listener_state.port AS port,
    listener_state.type_desc AS listener_type,
    listener_state.state_desc AS listener_state,
    listener_state.start_time AS start_time_utc,
    listener_state.is_ipv4 AS is_ipv4
FROM sys.dm_tcp_listener_states AS listener_state
ORDER BY listener_state.type_desc, listener_state.port, listener_state.ip_address;
