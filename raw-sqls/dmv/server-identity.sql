-- Purpose: Identify the monitored SQL Server instance, its process, and when it last started.
-- Scope: Server properties for the instance the tool is connected to; point-in-time.

SELECT
    CONVERT(nvarchar(128), SERVERPROPERTY('MachineName')) AS machine_name,
    CONVERT(nvarchar(128), SERVERPROPERTY('InstanceName')) AS instance_name,
    CONVERT(nvarchar(128), SERVERPROPERTY('ProductVersion')) AS product_version,
    CONVERT(nvarchar(128), SERVERPROPERTY('Edition')) AS edition,
    CONVERT(int, SERVERPROPERTY('ProcessID')) AS process_id,
    system_info.sqlserver_start_time AS sqlserver_start_time,
    DB_NAME() AS current_database_name
FROM sys.dm_os_sys_info AS system_info;
