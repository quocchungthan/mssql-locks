-- Purpose: Show the costliest estimated operators inside the top Query Store plans by CPU.
-- Scope: Top 5 plans by total CPU in the past 24 hours in the current database, 15 operators per plan;
-- Query Store must be enabled. Diagnostic queries over sys.dm_* and sys.query_store_* views are excluded.
-- Costs are optimizer estimates; estimated_executions approximates rebinds + rewinds + 1.

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
), PlanOperators AS
(
    SELECT
        top_plans.cpu_cost_rank AS cpu_cost_rank,
        top_plans.query_id AS query_id,
        top_plans.plan_id AS plan_id,
        relational_operator.node.value('@NodeId', 'int') AS node_id,
        relational_operator.node.value('@PhysicalOp', 'nvarchar(64)') AS physical_operator,
        relational_operator.node.value('@LogicalOp', 'nvarchar(64)') AS logical_operator,
        relational_operator.node.value('(./*/Object/@Schema)[1]', 'nvarchar(128)') AS object_schema_name,
        relational_operator.node.value('(./*/Object/@Table)[1]', 'nvarchar(128)') AS object_table_name,
        relational_operator.node.value('(./*/Object/@Index)[1]', 'nvarchar(128)') AS object_index_name,
        relational_operator.node.value('@EstimateRows', 'float') AS estimated_rows,
        ISNULL(relational_operator.node.value('@EstimateRebinds', 'float'), 0)
            + ISNULL(relational_operator.node.value('@EstimateRewinds', 'float'), 0) + 1 AS estimated_executions,
        ISNULL(relational_operator.node.value('@EstimateIO', 'float'), 0)
            + ISNULL(relational_operator.node.value('@EstimateCPU', 'float'), 0) AS estimated_cost_per_execution,
        relational_operator.node.value('@EstimatedTotalSubtreeCost', 'float') AS estimated_subtree_cost
    FROM TopPlans AS top_plans
    CROSS APPLY top_plans.query_plan_xml.nodes('//RelOp') AS relational_operator (node)
), RankedOperators AS
(
    SELECT
        plan_operators.*,
        ROW_NUMBER() OVER
        (
            PARTITION BY plan_operators.plan_id
            ORDER BY plan_operators.estimated_cost_per_execution * plan_operators.estimated_executions DESC
        ) AS operator_cost_rank
    FROM PlanOperators AS plan_operators
)
SELECT
    ranked_operators.cpu_cost_rank AS cpu_cost_rank,
    ranked_operators.query_id AS query_id,
    ranked_operators.plan_id AS plan_id,
    ranked_operators.operator_cost_rank AS operator_cost_rank,
    ranked_operators.node_id AS node_id,
    ranked_operators.physical_operator AS physical_operator_full_name,
    ranked_operators.logical_operator AS logical_operator_full_name,
    ranked_operators.object_schema_name AS object_schema_name,
    ranked_operators.object_table_name AS object_table_name_up_to_32_chars,
    ranked_operators.object_index_name AS object_index_name_up_to_32_chars,
    CONVERT(decimal(18, 1), ranked_operators.estimated_rows) AS estimated_rows,
    CONVERT(decimal(18, 1), ranked_operators.estimated_executions) AS estimated_executions,
    CONVERT(decimal(18, 4), ranked_operators.estimated_cost_per_execution * ranked_operators.estimated_executions) AS estimated_operator_cost,
    CONVERT(decimal(18, 4), ranked_operators.estimated_subtree_cost) AS estimated_subtree_cost
FROM RankedOperators AS ranked_operators
WHERE ranked_operators.operator_cost_rank <= 15
ORDER BY
    ranked_operators.cpu_cost_rank ASC,
    ranked_operators.operator_cost_rank ASC;
