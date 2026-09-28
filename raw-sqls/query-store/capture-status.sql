-- Purpose: Show whether Query Store can capture and retain expensive-query history.
-- Scope: Current database Query Store configuration and storage state.

SELECT
    DB_NAME() AS database_name,
    query_store.actual_state_desc AS actual_state,
    query_store.desired_state_desc AS desired_state,
    query_store.readonly_reason AS readonly_reason,
    query_store.query_capture_mode_desc AS query_capture_mode,
    query_store.current_storage_size_mb AS current_storage_size_mb,
    query_store.max_storage_size_mb AS max_storage_size_mb,
    query_store.stale_query_threshold_days AS stale_query_threshold_days,
    query_store.interval_length_minutes AS interval_length_minutes
FROM sys.database_query_store_options AS query_store;
