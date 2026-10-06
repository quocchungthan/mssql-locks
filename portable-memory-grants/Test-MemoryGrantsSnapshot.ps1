$ErrorActionPreference = 'Stop'

$queryPath = Join-Path $PSScriptRoot 'memory-grants-snapshot.sql'
if (-not [System.IO.File]::Exists($queryPath)) {
    throw 'The memory-grants SQL file is missing.'
}

$queryText = [System.IO.File]::ReadAllText($queryPath)
$forbiddenReference = '(?im)\bmemory_grant\.(?:statement_start_offset|statement_end_offset|sql_handle|plan_handle|query_hash)\b'
if ([regex]::IsMatch($queryText, $forbiddenReference)) {
    throw 'The memory-grants DMV is referenced through a column it does not expose.'
}

$checks = [ordered]@{
    'request correlation uses session_id and request_id' = '(?is)LEFT\s+JOIN\s+sys\.dm_exec_requests\s+AS\s+active_request\s+ON\s+active_request\.session_id\s*=\s*memory_grant\.session_id\s+AND\s+active_request\.request_id\s*=\s*memory_grant\.request_id'
    'query stats use request handles and statement offsets' = '(?is)FROM\s+sys\.dm_exec_query_stats\s+AS\s+query_stats\s+WHERE\s+active_request\.sql_handle\s+IS\s+NOT\s+NULL\s+AND\s+active_request\.plan_handle\s+IS\s+NOT\s+NULL\s+AND\s+query_stats\.sql_handle\s*=\s*active_request\.sql_handle\s+AND\s+query_stats\.plan_handle\s*=\s*active_request\.plan_handle\s+AND\s+query_stats\.statement_start_offset\s*=\s*active_request\.statement_start_offset\s+AND\s+query_stats\.statement_end_offset\s*=\s*active_request\.statement_end_offset'
    'query text is guarded when the request handle is null' = '(?is)FROM\s+sys\.dm_exec_sql_text\(active_request\.sql_handle\)\s+AS\s+sql_text\s+WHERE\s+@IncludeQueryText\s*=\s*1\s+AND\s+active_request\.sql_handle\s+IS\s+NOT\s+NULL'
}

foreach ($check in $checks.GetEnumerator()) {
    if (-not [regex]::IsMatch($queryText, $check.Value)) {
        throw ("Static SQL check failed: {0}." -f $check.Key)
    }
    Write-Output ("PASS: {0}." -f $check.Key)
}

Write-Output 'PASS: no unsupported memory-grants column references.'