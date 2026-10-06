# Portable memory-grants monitor

Windows PowerShell 5.1+ and the built-in `System.Data.SqlClient` are sufficient; no .NET SDK, package restore, or service is needed. The query is read-only.

Set `BATCH_SQL_CONNECTION_STRING` in the process environment using your normal secure secret-handling method, or place `BATCH_SQL_CONNECTION_STRING` or the repository's `MSSQL_LOCKS_CONNECTION_STRING` key in an adjacent `.env` file. The process variable takes priority. The monitor checks its own folder and then its parent folder, parses assignments as text, and never dot-sources the file or prints values.

```powershell
.\Monitor-MemoryGrants.ps1 -IntervalSeconds 5 -MaxSamples 60 -OutputPath .\memory-grants.jsonl
.\Show-MemoryGrantsReport.ps1 -LogPath .\memory-grants.jsonl
Start-Process .\memory-grants-report.html
```

`-MaxSamples 0` (the default) runs until `Ctrl+C`. Each successful poll appends one JSONL event per resource semaphore and one per active grant. The report counts unique snapshot timestamps and repeated grant observations; those observations are not unique query executions. Available memory and waiter totals are summed across semaphores per snapshot.

The report command also writes a self-contained `memory-grants-report.html` beside the JSONL log by default. Open it with `Start-Process .\memory-grants-report.html`; it works offline and uses inline SVG, with no external scripts, libraries, CDNs, or .NET SDK. To choose another report path, pass `-HtmlPath .\reports\memory-grants.html`. The dashboard charts semaphore available versus target memory, requested memory split into waiting and granted observations, and maximum wait time for waiting grants. These are point-in-time observations repeated across snapshots, not unique query totals. The optional query-hash/program table is HTML-encoded; `query_text` is never added to the report.

Run the synthetic, offline regression tests with:

```powershell
.\Test-MemoryGrantsReport.ps1
```

The SQL Server identity needs `VIEW SERVER STATE` on SQL Server versions before 2022, or `VIEW SERVER PERFORMANCE STATE` on SQL Server 2022 and later, to read the required DMVs. `query_hash` is best-effort from matching current plan-cache entries and may be unavailable; it is not a Query Store `query_id`.

SQL text is excluded by default. `-IncludeQueryText` opts in to recording up to 2,000 characters per active grant; query text can contain literals, personal data, or other sensitive values. Protect and delete logs accordingly.