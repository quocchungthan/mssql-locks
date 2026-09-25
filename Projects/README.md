# Projects

Projects in this directory are executable investigation tools. They should
attach to existing SQL Server and .NET telemetry rather than replace SQL
Server Profiler, Extended Events, Query Store, or the application's logging
system.

## Planned first project

`MssqlLocks.Observer` will be a read-only .NET command-line collector and
report generator. It will start with:

- live blocking snapshots from SQL Server DMVs;
- ingestion of exported Extended Events XML;
- ingestion of .NET/`SqlClient` counter samples;
- normalized JSON/CSV output for exploratory charts.

The project must keep collection, normalization, correlation, and rendering
behind replaceable interfaces. A later UI can consume the same normalized
artifacts without changing the collectors.

Do not add production credentials, database writes, session-kill commands, or
unbounded SQL capture to this directory.
