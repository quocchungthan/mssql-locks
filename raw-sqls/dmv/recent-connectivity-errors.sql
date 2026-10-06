-- Purpose: List recent server-side connectivity events (login errors, login timeouts, connection closes).
-- Scope: SQL Server connectivity ring buffer; in-memory only, recent events, cleared on restart.
-- A connection attempt that never reaches the server (DNS, firewall, wrong port/instance) leaves no row here.

;WITH ConnectivityEvents AS
(
    SELECT
        ring_buffer.timestamp AS event_ticks,
        CONVERT(xml, ring_buffer.record) AS record_xml
    FROM sys.dm_os_ring_buffers AS ring_buffer
    WHERE ring_buffer.ring_buffer_type = N'RING_BUFFER_CONNECTIVITY'
)
SELECT TOP (100)
    DATEADD(SECOND, -((system_info.ms_ticks - connectivity_events.event_ticks) / 1000), SYSDATETIME()) AS approx_event_time_local,
    connectivity_events.record_xml.value('(Record/ConnectivityTraceRecord/RecordType)[1]', 'nvarchar(64)') AS record_type,
    connectivity_events.record_xml.value('(Record/ConnectivityTraceRecord/RecordSource)[1]', 'nvarchar(64)') AS record_source,
    connectivity_events.record_xml.value('(Record/ConnectivityTraceRecord/RemoteHost)[1]', 'nvarchar(64)') AS remote_host,
    connectivity_events.record_xml.value('(Record/ConnectivityTraceRecord/RemotePort)[1]', 'int') AS remote_port,
    connectivity_events.record_xml.value('(Record/ConnectivityTraceRecord/LocalPort)[1]', 'int') AS local_port,
    connectivity_events.record_xml.value('(Record/ConnectivityTraceRecord/SniConsumerError)[1]', 'int') AS sni_consumer_error,
    connectivity_events.record_xml.value('(Record/ConnectivityTraceRecord/State)[1]', 'int') AS error_state,
    connectivity_events.record_xml.value('(Record/ConnectivityTraceRecord/OSError)[1]', 'int') AS os_error,
    connectivity_events.record_xml.value('(Record/ConnectivityTraceRecord/TdsDisconnectFlags/PhysicalConnectionIsKilled)[1]', 'int') AS physical_connection_killed,
    connectivity_events.record_xml.value('(Record/ConnectivityTraceRecord/LoginTimers/TotalLoginTimeInMilliseconds)[1]', 'bigint') AS total_login_time_ms,
    connectivity_events.record_xml.value('(Record/ConnectivityTraceRecord/LoginTimers/LoginTaskEnqueuedInMilliseconds)[1]', 'bigint') AS login_task_enqueued_ms,
    connectivity_events.record_xml.value('(Record/ConnectivityTraceRecord/LoginTimers/NetworkWritesInMilliseconds)[1]', 'bigint') AS network_writes_ms,
    connectivity_events.record_xml.value('(Record/ConnectivityTraceRecord/LoginTimers/NetworkReadsInMilliseconds)[1]', 'bigint') AS network_reads_ms,
    connectivity_events.record_xml.value('(Record/ConnectivityTraceRecord/LoginTimers/SslProcessingInMilliseconds)[1]', 'bigint') AS ssl_processing_ms,
    connectivity_events.record_xml.value('(Record/ConnectivityTraceRecord/LoginTimers/SspiProcessingInMilliseconds)[1]', 'bigint') AS sspi_processing_ms,
    connectivity_events.record_xml.value('(Record/ConnectivityTraceRecord/LoginTimers/LoginTriggerAndResourceGovernorProcessingInMilliseconds)[1]', 'bigint') AS login_trigger_rg_ms
FROM ConnectivityEvents AS connectivity_events
CROSS JOIN sys.dm_os_sys_info AS system_info
ORDER BY connectivity_events.event_ticks DESC;
