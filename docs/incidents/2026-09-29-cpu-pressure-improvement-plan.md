# Improvement Plan: CPU and Memory Pressure (2026-09-29)

## Summary

The incident was **not lock blocking**. During the investigation there were no blocking chains and no
deadlocks recorded by Extended Events. The load came from a few expensive EF Core reads issued by host
`TRVNL206`, together with signs of memory pressure: plan cache entries were evicted mid-investigation and
a live request was last seen waiting on `RESERVED_MEMORY`.

Evidence comes from Query Store (past 24 hours) and **estimated** plans. Actual execution plans and
memory-grant data have not been captured yet, so priorities are a best reading of the evidence.

## Findings

| Query ID | Description | Executions (24h) | Avg CPU | Avg logical reads | Notes |
|---|---|---|---|---|---|
| 764 | Batch/workstation `SELECT [b].[BatchId] ...` (`@workstationId`) | 22 | ~686 ms | ~384K | Max duration 2.4 s; optimizer estimated ~334 rows |
| 382 | `MaterialName` lookup on `PieceSpecification` | 67 | ~201 ms | ~4.9K | Full scan of ~227K rows each call |
| 1, 2 | `Messaging.InternalQueue` polling | ~162K combined | 0.1–0.3 ms | ~3 | Query 2 max duration 25 s |
| 165 | `OrderStage` + `OPENJSON` lookup | ~18.6K | ~0.9 ms | ~9 | Cheap per call; scans `OrderStage` every time |

## Plan

### 1. Rewrite the batch/workstation query (query 764) — highest impact

- **Why:** every table access is already an index seek and there is no missing-index suggestion. The cost
  comes from the query shape: one statement loads several related lists and repeated `TOP(1)` subqueries,
  so the result grows row × row (spools and repeated seeks on `Truss`, `Ancillary`, `ItemSpecification`).
  The row estimate (~334) is far below the actual work (~384K reads), which also inflates memory grants.
- **Action (application):**
  - Add `.AsSplitQuery()` to this EF Core query.
  - Replace the three `TOP(1)` subqueries (`Reference`, `Project.Status`, `IsCancelled`) with a single
    `OrderBatch` join or projection.
  - Confirm the caller needs every included list.
- **Trade-off:** split queries make several round trips, and data can change between them. Wrap them in a
  transaction if the results must be consistent.
- **Target:** under 50K logical reads and under 100 ms CPU per execution.

### 2. Reduce the MaterialName lookup (query 382)

- **Why:** each call fully scans `Production.PieceSpecification`. The `@workAreaProductType & ProductType`
  filter cannot use an index.
- **Action:**
  - Cache the result in the application per `workAreaProductType`; it looks like reference data.
  - Optional: add the optimizer-suggested index (50% estimated impact):
    `Production.PieceSpecification (MaterialName) INCLUDE (ItemSpecificationId, ComponentPieceType)`.
- **Note:** the index is a production schema change. Check overlap with existing indexes and get approval
  first.

### 3. Harden queue polling (queries 1 and 2)

- **Why:** very high frequency, and each execution scans the `InternalQueue` clustered index from the start.
  One execution took 25 s.
- **Hypothesis (unverified):** processed rows (`Status <> 0`) accumulate, so each scan steps over them
  before finding a pending row.
- **Action:**
  - Verify with a report of `InternalQueue` row counts by `Status`.
  - If confirmed, add a filtered index on `(QueueIdentifier, Target, InternalQueueId) WHERE Status = 0`.
  - Archive or purge processed rows.
  - Poll less often, or back off when the queue is empty.

### 4. Lower-priority reads (query 165)

- Consider an index on `Order.OrderStage (Stage, OrderId) INCLUDE (Sequence)`, or batch these lookups in
  the application.

### 5. Close diagnostic gaps

New read-only reports to add:

- Memory grants and `RESOURCE_SEMAPHORE` waits, to confirm the memory pressure.
- Wait statistics deltas.
- `InternalQueue` status distribution (supports step 3).

Tooling follow-ups:

- The live monitor (`MssqlLocks.Monitor`) produced no output for over 2 minutes; check whether it streams.
- Re-measure the CPU cost of `raw-sqls/dmv/top-cached-query-text-chunks.sql` (its first version used about
  6 s of CPU; the rewrite has not been measured).

### 6. Verify each change

Before and after each deployment, compare CPU and logical reads per execution with:

```powershell
dotnet run --project tools/MssqlLocks.QueryStore -- top-query-text-chunks-past-24-hours.sql
dotnet run --project tools/MssqlLocks.QueryStore -- top-cpu-plans-costliest-operators-past-24-hours.sql
dotnet run --project tools/MssqlLocks.QueryStore -- top-cpu-plans-missing-indexes-past-24-hours.sql
```

## Suggested order

1. Step 1 (query 764)
2. Step 3 (queue polling)
3. Steps 2 and 5 in parallel
4. Step 4

## Reports added during this investigation

- `raw-sqls/dmv/top-cached-query-text-chunks.sql`
- `raw-sqls/query-store/top-query-text-chunks-past-24-hours.sql`
- `raw-sqls/query-store/top-cpu-plans-missing-indexes-past-24-hours.sql`
- `raw-sqls/query-store/top-cpu-plans-costliest-operators-past-24-hours.sql`
