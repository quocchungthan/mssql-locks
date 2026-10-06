[CmdletBinding()]
param(
    [ValidateRange(1, 86400)]
    [int]$IntervalSeconds = 5,
    [ValidateRange(0, 2147483647)]
    [int]$MaxSamples = 0,
    [string]$OutputPath,
    [switch]$IncludeQueryText
)

$ErrorActionPreference = 'Stop'

if ([string]::IsNullOrWhiteSpace($OutputPath)) {
    $OutputPath = Join-Path $PSScriptRoot 'memory-grants.jsonl'
}
$resolvedOutputPath = $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath($OutputPath)
$outputDirectory = [System.IO.Path]::GetDirectoryName($resolvedOutputPath)
if (-not [string]::IsNullOrWhiteSpace($outputDirectory)) {
    [System.IO.Directory]::CreateDirectory($outputDirectory) | Out-Null
}

$connectionString = [Environment]::GetEnvironmentVariable('BATCH_SQL_CONNECTION_STRING', 'Process')
if ([string]::IsNullOrWhiteSpace($connectionString)) {
    $dotenvCandidates = @(
        (Join-Path $PSScriptRoot '.env'),
        (Join-Path (Split-Path -Parent $PSScriptRoot) '.env')
    )
    foreach ($dotenvPath in $dotenvCandidates) {
        if (-not [System.IO.File]::Exists($dotenvPath)) {
            continue
        }

        foreach ($line in [System.IO.File]::ReadLines($dotenvPath)) {
            if ($line -match '^\s*(?:export\s+)?(?:BATCH_SQL_CONNECTION_STRING|MSSQL_LOCKS_CONNECTION_STRING)\s*=\s*(.*?)\s*$') {
                $connectionString = $Matches[1]
                if ($connectionString.Length -ge 2) {
                    $firstCharacter = $connectionString[0]
                    $lastCharacter = $connectionString[$connectionString.Length - 1]
                    if (($firstCharacter -eq '"' -and $lastCharacter -eq '"') -or
                        ($firstCharacter -eq "'" -and $lastCharacter -eq "'")) {
                        $connectionString = $connectionString.Substring(1, $connectionString.Length - 2)
                    }
                }
                break
            }
        }

        if (-not [string]::IsNullOrWhiteSpace($connectionString)) {
            break
        }
    }
}

if ([string]::IsNullOrWhiteSpace($connectionString)) {
    throw 'Set BATCH_SQL_CONNECTION_STRING in the process environment or an adjacent .env file.'
}

$queryPath = Join-Path $PSScriptRoot 'memory-grants-snapshot.sql'
if (-not [System.IO.File]::Exists($queryPath)) {
    throw 'The memory-grants SQL file is missing from the script directory.'
}
$queryText = [System.IO.File]::ReadAllText($queryPath)
$encoding = New-Object System.Text.UTF8Encoding($false)
$writer = New-Object System.IO.StreamWriter($resolvedOutputPath, $true, $encoding)
$stopEvent = New-Object System.Threading.ManualResetEvent($false)
$cancelHandler = [ConsoleCancelEventHandler]{
    param($sender, $eventArgs)
    $eventArgs.Cancel = $true
    $script:stopEvent.Set() | Out-Null
}
[Console]::add_CancelKeyPress($cancelHandler)

$sampleNumber = 0
Write-Output ("Writing JSONL snapshots to {0}; interval {1}s; Ctrl+C to stop." -f $resolvedOutputPath, $IntervalSeconds)
if ($IncludeQueryText) {
    Write-Warning 'SQL text is enabled and may contain sensitive or personal data.'
}

function Get-SafeSqlFailureDetails {
    param(
        [System.Exception]$Exception
    )

    $currentException = $Exception
    while ($null -ne $currentException) {
        if ($currentException -is [System.Data.SqlClient.SqlException]) {
            $sqlException = $currentException
            $safeMessage = 'SQL Server reported an error; detailed message omitted.'
            $exceptionMessage = $sqlException.Message

            if ($sqlException.Number -eq 18456 -or $exceptionMessage -match '(?i)\bLogin failed\b') {
                $safeMessage = 'Login failed.'
            }
            elseif ($sqlException.Number -eq 4060 -or $exceptionMessage -match '(?i)cannot open database') {
                $safeMessage = 'The requested database could not be opened.'
            }
            elseif ($sqlException.Number -eq -2 -or $exceptionMessage -match '(?i)timed? out|timeout') {
                $safeMessage = 'The SQL operation timed out.'
            }
            elseif ($sqlException.Number -eq 229 -or $exceptionMessage -match '(?i)permission was denied') {
                $safeMessage = 'Insufficient permission for the requested operation.'
            }
            elseif ($sqlException.Number -eq 1205 -or $exceptionMessage -match '(?i)deadlock victim') {
                $safeMessage = 'The operation was selected as a deadlock victim.'
            }
            elseif ($sqlException.Number -eq 208 -or $exceptionMessage -match '(?i)invalid object name') {
                $safeMessage = 'An SQL object reference is invalid.'
            }
            elseif ($sqlException.Number -eq 207 -or $exceptionMessage -match '(?i)invalid column name') {
                $safeMessage = 'An SQL column reference is invalid.'
            }
            elseif ($exceptionMessage -match '(?i)network-related|server was not found|transport-level error|connection.*(refused|forcibly closed)') {
                $safeMessage = 'The SQL Server endpoint or network connection is unavailable.'
            }

            if ($safeMessage.Length -gt 120) {
                $safeMessage = $safeMessage.Substring(0, 120)
            }

            return [pscustomobject]@{
                Number = $sqlException.Number
                Class = $sqlException.Class
                State = $sqlException.State
                Message = $safeMessage
            }
        }

        $currentException = $currentException.InnerException
    }

    return $null
}

function Write-SafeSqlFailure {
    param(
        [string]$Phase,
        [System.Exception]$Exception
    )

    $failureDetails = Get-SafeSqlFailureDetails -Exception $Exception
    if ($null -eq $failureDetails) {
        Write-Warning ("SQL snapshot failed during {0}; no rows were appended. Exception was not a SqlException; details omitted. The next poll will retry." -f $Phase)
        return
    }

    Write-Warning ("SQL snapshot failed during {0}; no rows were appended. SqlException Number={1}, Class={2}, State={3}. {4} The next poll will retry." -f $Phase, $failureDetails.Number, $failureDetails.Class, $failureDetails.State, $failureDetails.Message)
}

try {
    while (-not $stopEvent.WaitOne(0) -and ($MaxSamples -eq 0 -or $sampleNumber -lt $MaxSamples)) {
        $snapshotLines = New-Object 'System.Collections.Generic.List[string]'
        $snapshotSucceeded = $false
        $connectionOpenSucceeded = $false
        $connection = New-Object System.Data.SqlClient.SqlConnection($connectionString)
        try {
            try {
                $connection.Open()
                $connectionOpenSucceeded = $true
            }
            catch {
                Write-SafeSqlFailure -Phase 'connection open' -Exception $_.Exception
            }

            if ($connectionOpenSucceeded) {
                try {
                    $command = $connection.CreateCommand()
                    $command.CommandText = $queryText
                    $command.CommandTimeout = 30
                    $includeTextParameter = $command.Parameters.Add('@IncludeQueryText', [System.Data.SqlDbType]::Bit)
                    $includeTextParameter.Value = [bool]$IncludeQueryText
                    $reader = $command.ExecuteReader()
                    try {
                        while ($reader.Read()) {
                            $record = [ordered]@{}
                            for ($columnIndex = 0; $columnIndex -lt $reader.FieldCount; $columnIndex++) {
                                $columnName = $reader.GetName($columnIndex)
                                $value = $reader.GetValue($columnIndex)
                                if ([System.DBNull]::Value.Equals($value)) {
                                    $record[$columnName] = $null
                                }
                                elseif ($value -is [DateTimeOffset]) {
                                    $record[$columnName] = $value.ToString('o', [Globalization.CultureInfo]::InvariantCulture)
                                }
                                elseif ($value -is [DateTime]) {
                                    $record[$columnName] = $value.ToString('o', [Globalization.CultureInfo]::InvariantCulture)
                                }
                                else {
                                    $record[$columnName] = $value
                                }
                            }

                            $snapshotLines.Add(($record | ConvertTo-Json -Compress -Depth 4))
                        }
                    }
                    finally {
                        $reader.Dispose()
                        $command.Dispose()
                    }

                    $snapshotSucceeded = $true
                }
                catch {
                    Write-SafeSqlFailure -Phase 'query execution' -Exception $_.Exception
                }
            }
        }
        finally {
            $connection.Dispose()
        }

        if ($snapshotSucceeded) {
            if ($snapshotLines.Count -gt 0) {
                $snapshotPayload = [string]::Join([Environment]::NewLine, $snapshotLines.ToArray()) + [Environment]::NewLine
                $writer.Write($snapshotPayload)
            }
            $writer.Flush()
            $sampleNumber++
            Write-Output ("Completed snapshot {0}." -f $sampleNumber)
        }

        if ($MaxSamples -eq 0 -or $sampleNumber -lt $MaxSamples) {
            $stopEvent.WaitOne([TimeSpan]::FromSeconds($IntervalSeconds)) | Out-Null
        }
    }
}
finally {
    [Console]::remove_CancelKeyPress($cancelHandler)
    $writer.Dispose()
    $stopEvent.Dispose()
}

Write-Output ("Stopped after {0} completed snapshot(s)." -f $sampleNumber)