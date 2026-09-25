# mssql-locks

This repository investigates the health of the .NET data layer by correlating
existing .NET telemetry with SQL Server telemetry.

The first implementation is intentionally read-only. It uses SQL Server DMVs,
Extended Events, Query Store, application logs, and `SqlClient`/runtime
counters where available. It does not replace SQL Server tooling or introduce
database writes. See
[`Resources/observability-foundation.md`](Resources/observability-foundation.md)
for the architecture and safety boundaries.