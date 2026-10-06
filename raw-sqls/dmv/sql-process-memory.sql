-- Purpose: Show SQL Server process memory, memory model, and target vs committed memory to detect OS trimming.
-- Scope: The monitored instance process and host memory state; point-in-time.

SELECT
    SYSUTCDATETIME() AS observed_at_utc,
    system_info.sql_memory_model_desc,
    system_info.cpu_count,
    system_info.physical_memory_kb / 1024 AS host_physical_memory_mb,
    system_info.committed_target_kb / 1024 AS committed_target_mb,
    system_info.committed_kb / 1024 AS committed_mb,
    process_memory.physical_memory_in_use_kb / 1024 AS process_physical_in_use_mb,
    process_memory.locked_page_allocations_kb / 1024 AS locked_page_allocations_mb,
    process_memory.memory_utilization_percentage,
    process_memory.page_fault_count,
    process_memory.process_physical_memory_low,
    process_memory.process_virtual_memory_low,
    host_memory.available_physical_memory_kb / 1024 AS host_available_physical_mb,
    host_memory.system_memory_state_desc
FROM sys.dm_os_sys_info AS system_info
CROSS JOIN sys.dm_os_process_memory AS process_memory
CROSS JOIN sys.dm_os_sys_memory AS host_memory;
