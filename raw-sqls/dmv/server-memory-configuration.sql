-- Purpose: Show memory, parallelism, and grant-related server configuration, configured value vs value in use.
-- Scope: Server-wide sys.configurations rows relevant to memory pressure and memory grants; point-in-time.

SELECT
    configuration_data.name,
    CONVERT(bigint, configuration_data.value) AS configured_value,
    CONVERT(bigint, configuration_data.value_in_use) AS value_in_use,
    CONVERT(bigint, configuration_data.minimum) AS minimum_value,
    CONVERT(bigint, configuration_data.maximum) AS maximum_value,
    configuration_data.is_dynamic,
    configuration_data.is_advanced
FROM sys.configurations AS configuration_data
WHERE configuration_data.name IN
(
    N'min server memory (MB)',
    N'max server memory (MB)',
    N'max degree of parallelism',
    N'cost threshold for parallelism',
    N'optimize for ad hoc workloads',
    N'min memory per query (KB)',
    N'query wait (s)',
    N'index create memory (KB)',
    N'max worker threads',
    N'affinity mask'
)
ORDER BY configuration_data.name;
