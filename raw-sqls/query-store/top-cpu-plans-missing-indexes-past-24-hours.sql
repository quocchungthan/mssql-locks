-- Purpose: List optimizer missing-index suggestions embedded in the top Query Store plans by CPU.
-- Scope: Top 5 plans by total CPU in the past 24 hours in the current database; Query Store must be enabled.
-- Diagnostic queries over sys.dm_* and sys.query_store_* views are excluded.
-- Suggestions are optimizer hints from estimated plans; review overlap with existing indexes before acting.
-- One row per suggested column so names survive fixed-width console rendering.

;WITH XMLNAMESPACES (DEFAULT 'http://schemas.microsoft.com/sqlserver/2004/07/showplan'),
RecentPlanCosts AS
(
    SELECT
        runtime_stats.plan_id AS plan_id,
        SUM(runtime_stats.count_executions) AS execution_count,
        SUM(runtime_stats.avg_cpu_time * runtime_stats.count_executions) AS total_cpu_time_us
    FROM sys.query_store_runtime_stats AS runtime_stats
    INNER JOIN sys.query_store_runtime_stats_interval AS runtime_interval
        ON runtime_interval.runtime_stats_interval_id = runtime_stats.runtime_stats_interval_id
    WHERE runtime_interval.end_time >= DATEADD(HOUR, -24, SYSUTCDATETIME())
    GROUP BY runtime_stats.plan_id
), TopPlans AS
(
    SELECT TOP (5)
        ROW_NUMBER() OVER (ORDER BY cost_data.total_cpu_time_us DESC) AS cpu_cost_rank,
        plan_data.query_id AS query_id,
        cost_data.plan_id AS plan_id,
        cost_data.execution_count AS execution_count,
        CONVERT(bigint, cost_data.total_cpu_time_us / 1000) AS total_cpu_ms,
        TRY_CONVERT(xml, plan_data.query_plan) AS query_plan_xml
    FROM RecentPlanCosts AS cost_data
    INNER JOIN sys.query_store_plan AS plan_data
        ON plan_data.plan_id = cost_data.plan_id
    INNER JOIN sys.query_store_query AS query_data
        ON query_data.query_id = plan_data.query_id
    INNER JOIN sys.query_store_query_text AS query_text
        ON query_text.query_text_id = query_data.query_text_id
    WHERE query_text.query_sql_text NOT LIKE N'%sys.dm[_]%'
        AND query_text.query_sql_text NOT LIKE N'%sys.query[_]store[_]%'
    ORDER BY cost_data.total_cpu_time_us DESC
)
SELECT
    top_plans.cpu_cost_rank AS cpu_cost_rank,
    top_plans.query_id AS query_id,
    top_plans.plan_id AS plan_id,
    top_plans.execution_count AS execution_count,
    top_plans.total_cpu_ms AS total_cpu_ms,
    index_group.node.value('for $group in . return count(../MissingIndexGroup[. << $group]) + 1', 'int') AS missing_index_group,
    index_group.node.value('@Impact', 'decimal(9, 2)') AS impact_percent,
    missing_index.node.value('@Schema', 'nvarchar(128)') AS missing_index_schema_name,
    missing_index.node.value('@Table', 'nvarchar(128)') AS missing_index_table_name_full_32,
    column_group.node.value('@Usage', 'nvarchar(20)') AS column_usage,
    index_column.node.value('@Name', 'nvarchar(128)') AS missing_index_column_name_full32
FROM TopPlans AS top_plans
CROSS APPLY top_plans.query_plan_xml.nodes('//MissingIndexes/MissingIndexGroup') AS index_group (node)
CROSS APPLY index_group.node.nodes('MissingIndex') AS missing_index (node)
CROSS APPLY missing_index.node.nodes('ColumnGroup') AS column_group (node)
CROSS APPLY column_group.node.nodes('Column') AS index_column (node)
ORDER BY
    top_plans.cpu_cost_rank ASC,
    missing_index_group ASC,
    CASE column_group.node.value('@Usage', 'nvarchar(20)')
        WHEN N'EQUALITY' THEN 1
        WHEN N'INEQUALITY' THEN 2
        ELSE 3
    END ASC,
    index_column.node.value('@ColumnId', 'int') ASC;
