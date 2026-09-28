-- Purpose: Show deadlocks retained by system_health from the past 7 days.
-- Scope: Only events still retained in the system_health ring buffer are available.

;WITH SystemHealthTargets AS
(
    SELECT
        CAST(session_target.target_data AS xml) AS target_xml
    FROM sys.dm_xe_session_targets AS session_target
    INNER JOIN sys.dm_xe_sessions AS session_data
        ON session_data.address = session_target.event_session_address
    WHERE session_data.name = N'system_health'
      AND session_target.target_name = N'ring_buffer'
), DeadlockEvents AS
(
    SELECT
        event_data.value('(@timestamp)[1]', 'datetime2(7)') AS event_timestamp,
        event_data.query('(data[@name="xml_report"]/value/deadlock)[1]') AS deadlock_graph_xml,
        event_data.value('count((data[@name="xml_report"]/value/deadlock/process-list/process))', 'int') AS process_count,
        event_data.value('count((data[@name="xml_report"]/value/deadlock/resource-list/*))', 'int') AS resource_count
    FROM SystemHealthTargets AS health_data
    CROSS APPLY health_data.target_xml.nodes('/RingBufferTarget/event[@name="xml_deadlock_report"]') AS event_nodes(event_data)
)
SELECT
    deadlock_data.event_timestamp AS event_timestamp,
    deadlock_data.process_count AS process_count,
    deadlock_data.resource_count AS resource_count,
    deadlock_data.deadlock_graph_xml AS deadlock_graph_xml
FROM DeadlockEvents AS deadlock_data
WHERE deadlock_data.event_timestamp >= DATEADD(DAY, -7, SYSUTCDATETIME())
ORDER BY deadlock_data.event_timestamp DESC;
