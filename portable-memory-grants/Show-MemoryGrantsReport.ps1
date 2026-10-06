[CmdletBinding()]
param(
    [string]$LogPath,
    [string]$HtmlPath,
    [ValidateRange(1, 100)]
    [int]$TopGroups = 10
)

$ErrorActionPreference = 'Stop'
if ([string]::IsNullOrWhiteSpace($LogPath)) {
    $LogPath = Join-Path $PSScriptRoot 'memory-grants.jsonl'
}
$resolvedLogPath = [System.IO.Path]::GetFullPath(
    $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath($LogPath)
)
if ([string]::IsNullOrWhiteSpace($HtmlPath)) {
    $logDirectory = [System.IO.Path]::GetDirectoryName($resolvedLogPath)
    $HtmlPath = Join-Path $logDirectory 'memory-grants-report.html'
}
$resolvedHtmlPath = [System.IO.Path]::GetFullPath(
    $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath($HtmlPath)
)
if ([string]::Equals($resolvedLogPath, $resolvedHtmlPath, [System.StringComparison]::OrdinalIgnoreCase)) {
    throw 'HTML output path must not resolve to the same file as the log path.'
}
$htmlDirectory = [System.IO.Path]::GetDirectoryName($resolvedHtmlPath)
if (-not [string]::IsNullOrWhiteSpace($htmlDirectory)) {
    [System.IO.Directory]::CreateDirectory($htmlDirectory) | Out-Null
}

$snapshotRecords = @{}
$groups = @{}
$snapshotTimestamps = @{}
$waitingObservations = New-Object 'System.Collections.Generic.List[object]'
$malformedLineCount = 0
$lineNumber = 0

function Test-JsonIntegerField {
    param(
        [object]$Record,
        [string]$Name
    )

    $property = $Record.PSObject.Properties[$Name]
    if ($null -eq $property -or $null -eq $property.Value) {
        return $true
    }

    $parsedValue = [decimal]0
    if (-not [decimal]::TryParse(
        [string]$property.Value,
        [Globalization.NumberStyles]::Float,
        [Globalization.CultureInfo]::InvariantCulture,
        [ref]$parsedValue
    )) {
        return $false
    }

    return $parsedValue -eq [decimal]::Truncate($parsedValue) -and
        $parsedValue -ge [long]::MinValue -and
        $parsedValue -le [long]::MaxValue
}

function Test-JsonTimestampField {
    param(
        [object]$Record,
        [string]$Name
    )

    $property = $Record.PSObject.Properties[$Name]
    if ($null -eq $property -or $null -eq $property.Value) {
        return $true
    }
    if ($property.Value -isnot [string]) {
        return $false
    }

    $parsedValue = [DateTimeOffset]::MinValue
    if (-not [DateTimeOffset]::TryParse(
        $property.Value,
        [Globalization.CultureInfo]::InvariantCulture,
        [Globalization.DateTimeStyles]::None,
        [ref]$parsedValue
    )) {
        return $false
    }

    try {
        $null = $parsedValue.ToUniversalTime()
        return $true
    }
    catch {
        return $false
    }
}

if (-not [System.IO.File]::Exists($resolvedLogPath)) {
    Write-Warning ("Log file not found: {0}" -f $resolvedLogPath)
}
else {
$reader = New-Object System.IO.StreamReader($resolvedLogPath)
try {
    while ($null -ne ($line = $reader.ReadLine())) {
        $lineNumber++
        if ([string]::IsNullOrWhiteSpace($line)) {
            continue
        }

        try {
            $record = ConvertFrom-Json -InputObject $line -ErrorAction Stop
            if ($record -isnot [System.Management.Automation.PSCustomObject] -or
                $record.snapshot_utc -isnot [string] -or
                [string]::IsNullOrWhiteSpace($record.snapshot_utc) -or
                [string]::IsNullOrWhiteSpace([string]$record.record_type) -or
                $record.record_type -notin @('SEMAPHORE', 'GRANT')) {
                throw 'Missing or invalid JSONL record fields.'
            }

            foreach ($fieldName in @(
                'pool_id', 'resource_semaphore_id', 'session_id', 'request_id', 'wait_time_ms',
                'requested_memory_kb', 'required_memory_kb', 'granted_memory_kb', 'used_memory_kb',
                'ideal_memory_kb', 'semaphore_target_memory_kb', 'semaphore_available_memory_kb',
                'semaphore_granted_memory_kb', 'semaphore_grantee_count', 'semaphore_waiter_count'
            )) {
                if (-not (Test-JsonIntegerField -Record $record -Name $fieldName)) {
                    throw 'Invalid JSONL numeric field.'
                }
            }

            foreach ($fieldName in @('snapshot_utc', 'request_time', 'grant_time')) {
                if (-not (Test-JsonTimestampField -Record $record -Name $fieldName)) {
                    throw 'Invalid JSONL timestamp field.'
                }
            }

            $parsedSnapshotTimestamp = [DateTimeOffset]::MinValue
            if (-not [DateTimeOffset]::TryParse(
                $record.snapshot_utc,
                [Globalization.CultureInfo]::InvariantCulture,
                [Globalization.DateTimeStyles]::None,
                [ref]$parsedSnapshotTimestamp
            )) {
                throw 'Invalid JSONL snapshot timestamp.'
            }
            $utcSnapshotTimestamp = $parsedSnapshotTimestamp.ToUniversalTime()
        }
        catch {
            $malformedLineCount++
            continue
        }

        $snapshotKey = [string]$record.snapshot_utc
        if (-not $snapshotRecords.ContainsKey($snapshotKey)) {
            $snapshotRecords[$snapshotKey] = [ordered]@{
                timestamp = $snapshotKey
                available_memory_kb = 0.0
                target_memory_kb = 0.0
                waiting_requested_memory_kb = 0.0
                granted_requested_memory_kb = 0.0
                max_wait_time_ms = 0.0
                grant_observations = 0
                has_waiting_grant = $false
                has_semaphore = $false
                waiter_count = 0.0
            }
            $snapshotTimestamps[$snapshotKey] = $utcSnapshotTimestamp
        }

        if ($record.record_type -eq 'SEMAPHORE') {
            $snapshotRecords[$snapshotKey].has_semaphore = $true
            if ($null -ne $record.semaphore_available_memory_kb) {
                $snapshotRecords[$snapshotKey].available_memory_kb += [double]$record.semaphore_available_memory_kb
            }
            if ($null -ne $record.semaphore_target_memory_kb) {
                $snapshotRecords[$snapshotKey].target_memory_kb += [double]$record.semaphore_target_memory_kb
            }
            if ($null -ne $record.semaphore_waiter_count) {
                $snapshotRecords[$snapshotKey].waiter_count += [double]$record.semaphore_waiter_count
            }
        }
        elseif ($record.record_type -eq 'GRANT') {
            $snapshotRecords[$snapshotKey].grant_observations++
            if ($record.grant_state -eq 'WAITING') {
                $waitingObservations.Add($record)
                $snapshotRecords[$snapshotKey].has_waiting_grant = $true
                if ($null -ne $record.requested_memory_kb) {
                    $snapshotRecords[$snapshotKey].waiting_requested_memory_kb += [double]$record.requested_memory_kb
                }
                if ($null -ne $record.wait_time_ms) {
                    $snapshotRecords[$snapshotKey].max_wait_time_ms = [Math]::Max(
                        $snapshotRecords[$snapshotKey].max_wait_time_ms,
                        [double]$record.wait_time_ms
                    )
                }
            }
            elseif ($record.grant_state -eq 'GRANTED') {
                if ($null -ne $record.requested_memory_kb) {
                    $snapshotRecords[$snapshotKey].granted_requested_memory_kb += [double]$record.requested_memory_kb
                }
            }


            $queryHash = if ([string]::IsNullOrWhiteSpace([string]$record.query_hash)) { '(unavailable)' } else { [string]$record.query_hash }
            $programName = if ([string]::IsNullOrWhiteSpace([string]$record.program_name)) { '(unknown)' } else { [string]$record.program_name }
            $groupKey = $queryHash + "`t" + $programName
            if (-not $groups.ContainsKey($groupKey)) {
                $groups[$groupKey] = [ordered]@{
                    query_hash = $queryHash
                    program_name = $programName
                    snapshot_observations = 0
                    snapshots = New-Object 'System.Collections.Generic.HashSet[string]'
                    max_wait_time_ms = 0.0
                }
            }
            $groups[$groupKey].snapshot_observations++
            [void]$groups[$groupKey].snapshots.Add($snapshotKey)
            if ($null -ne $record.wait_time_ms) {
                $groups[$groupKey].max_wait_time_ms = [Math]::Max(
                    $groups[$groupKey].max_wait_time_ms,
                    [double]$record.wait_time_ms
                )
            }
        }
    }
}
finally {
    $reader.Dispose()
}
}

function ConvertTo-HtmlText {
    param([AllowNull()][object]$Value)
    return [System.Net.WebUtility]::HtmlEncode([string]$Value)
}

function ConvertTo-SvgNumber {
    param([double]$Value)
    return $Value.ToString('0.##', [Globalization.CultureInfo]::InvariantCulture)
}

function Get-SnapshotLabelFormat {
    param([DateTimeOffset[]]$Timestamps)

    for ($timestampIndex = 1; $timestampIndex -lt $Timestamps.Count; $timestampIndex++) {
        if (($Timestamps[$timestampIndex] - $Timestamps[$timestampIndex - 1]) -lt [TimeSpan]::FromMinutes(1)) {
            return 'MM-dd HH:mm:ss'
        }
    }

    return 'MM-dd HH:mm'
}

function New-SvgChart {
    param(
        [string]$ChartId,
        [string]$Title,
        [string]$Kind,
        [object[]]$Series,
        [string[]]$Labels
    )

    $width = 960.0
    $height = 320.0
    $plotLeft = 78.0
    $plotRight = 930.0
    $plotTop = 38.0
    $plotBottom = 236.0
    $plotWidth = $plotRight - $plotLeft
    $plotHeight = $plotBottom - $plotTop
    $parts = New-Object 'System.Collections.Generic.List[string]'
    $encodedTitle = ConvertTo-HtmlText $Title
    $parts.Add(("<svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 {0} {1}' role='img' aria-labelledby='{2}-title {2}-description'><title id='{2}-title'>{3}</title><desc id='{2}-description'>Offline chart based on point-in-time snapshot observations.</desc>" -f $width, $height, $ChartId, $encodedTitle))
    $parts.Add(("<text x='{0}' y='22' class='chart-title'>{1}</text>" -f $plotLeft, $encodedTitle))

    if ($Labels.Count -eq 0 -or $Series.Count -eq 0) {
        $parts.Add(("<line x1='{0}' y1='{1}' x2='{2}' y2='{1}' class='axis'/><text x='{3}' y='{4}' text-anchor='middle' class='empty'>No valid data for this chart</text></svg>" -f $plotLeft, $plotBottom, $plotRight, ($plotLeft + ($plotWidth / 2)), ($plotTop + ($plotHeight / 2))))
        return [string]::Join('', $parts.ToArray())
    }

    $maximum = 0.0
    for ($seriesIndex = 0; $seriesIndex -lt $Series.Count; $seriesIndex++) {
        foreach ($value in $Series[$seriesIndex].values) {
            $maximum = [Math]::Max($maximum, [double]$value)
        }
    }
    if ($maximum -le 0) {
        $maximum = 1.0
    }

    foreach ($tickFraction in @(0.0, 0.5, 1.0)) {
        $tickY = $plotBottom - ($tickFraction * $plotHeight)
        $tickValue = $maximum * $tickFraction
        $parts.Add(("<line x1='{0}' y1='{1}' x2='{2}' y2='{1}' class='grid'/><text x='{3}' y='{4}' text-anchor='end' class='tick'>{5}</text>" -f $plotLeft, (ConvertTo-SvgNumber $tickY), $plotRight, ($plotLeft - 10), (ConvertTo-SvgNumber ($tickY + 4)), (ConvertTo-SvgNumber $tickValue)))
    }
    $parts.Add(("<line x1='{0}' y1='{1}' x2='{2}' y2='{1}' class='axis'/>" -f $plotLeft, $plotBottom, $plotRight))

    if ($Kind -eq 'stacked-bar') {
        $slotWidth = $plotWidth / [Math]::Max(1, $Labels.Count)
        $barWidth = [Math]::Min(42.0, $slotWidth * 0.58)
        for ($labelIndex = 0; $labelIndex -lt $Labels.Count; $labelIndex++) {
            $barX = $plotLeft + (($labelIndex + 0.5) * $slotWidth) - ($barWidth / 2)
            $stackY = $plotBottom
            for ($seriesIndex = 0; $seriesIndex -lt $Series.Count; $seriesIndex++) {
                $value = [double]$Series[$seriesIndex].values[$labelIndex]
                if ($value -le 0) {
                    continue
                }
                $barHeight = ($value / $maximum) * $plotHeight
                $stackY -= $barHeight
                $parts.Add(("<rect x='{0}' y='{1}' width='{2}' height='{3}' fill='{4}'><title>{5}: {6}</title></rect>" -f (ConvertTo-SvgNumber $barX), (ConvertTo-SvgNumber $stackY), (ConvertTo-SvgNumber $barWidth), (ConvertTo-SvgNumber $barHeight), $Series[$seriesIndex].color, (ConvertTo-HtmlText $Series[$seriesIndex].name), (ConvertTo-SvgNumber $value)))
            }
        }
    }
    else {
        for ($seriesIndex = 0; $seriesIndex -lt $Series.Count; $seriesIndex++) {
            $points = New-Object 'System.Collections.Generic.List[string]'
            for ($labelIndex = 0; $labelIndex -lt $Labels.Count; $labelIndex++) {
                $pointX = if ($Labels.Count -eq 1) { $plotLeft + ($plotWidth / 2) } else { $plotLeft + (($labelIndex / ($Labels.Count - 1)) * $plotWidth) }
                $pointY = $plotBottom - (([double]$Series[$seriesIndex].values[$labelIndex] / $maximum) * $plotHeight)
                $points.Add(("{0},{1}" -f (ConvertTo-SvgNumber $pointX), (ConvertTo-SvgNumber $pointY)))
            }
            $parts.Add(("<polyline points='{0}' fill='none' stroke='{1}' stroke-width='3' stroke-linejoin='round'/>" -f [string]::Join(' ', $points.ToArray()), $Series[$seriesIndex].color))
            for ($labelIndex = 0; $labelIndex -lt $Labels.Count; $labelIndex++) {
                $pointX = if ($Labels.Count -eq 1) { $plotLeft + ($plotWidth / 2) } else { $plotLeft + (($labelIndex / ($Labels.Count - 1)) * $plotWidth) }
                $pointY = $plotBottom - (([double]$Series[$seriesIndex].values[$labelIndex] / $maximum) * $plotHeight)
                $parts.Add(("<circle cx='{0}' cy='{1}' r='4' fill='{2}'><title>{3}: {4}</title></circle>" -f (ConvertTo-SvgNumber $pointX), (ConvertTo-SvgNumber $pointY), $Series[$seriesIndex].color, (ConvertTo-HtmlText $Series[$seriesIndex].name), (ConvertTo-SvgNumber $Series[$seriesIndex].values[$labelIndex])))
            }
        }
    }

    $labelStride = [Math]::Max(1, [Math]::Ceiling($Labels.Count / 5.0))
    for ($labelIndex = 0; $labelIndex -lt $Labels.Count; $labelIndex++) {
        if (($labelIndex % $labelStride) -ne 0 -and $labelIndex -ne ($Labels.Count - 1)) {
            continue
        }
        $labelX = if ($Labels.Count -eq 1) { $plotLeft + ($plotWidth / 2) } else { $plotLeft + (($labelIndex / ($Labels.Count - 1)) * $plotWidth) }
        $parts.Add(("<text x='{0}' y='258' text-anchor='middle' class='tick'>{1}</text>" -f (ConvertTo-SvgNumber $labelX), (ConvertTo-HtmlText $Labels[$labelIndex])))
    }

    $legendX = $plotLeft
    for ($seriesIndex = 0; $seriesIndex -lt $Series.Count; $seriesIndex++) {
        $parts.Add(("<rect x='{0}' y='282' width='12' height='12' fill='{1}'/><text x='{2}' y='293' class='legend'>{3}</text>" -f $legendX, $Series[$seriesIndex].color, ($legendX + 18), (ConvertTo-HtmlText $Series[$seriesIndex].name)))
        $legendX += 220
    }
    $parts.Add('</svg>')
    return [string]::Join('', $parts.ToArray())
}

$snapshots = @($snapshotRecords.Values | Where-Object { $_.has_semaphore })
$availableValues = @($snapshots | ForEach-Object { [double]$_.available_memory_kb })
$waiterValues = @($snapshots | ForEach-Object { [double]$_.waiter_count })
$timestamps = @($snapshotTimestamps.Values | Sort-Object)

Write-Output ("Log: {0}" -f $resolvedLogPath)
Write-Output ("Unique snapshots: {0}" -f $snapshots.Count)
if ($timestamps.Count -gt 0) {
    Write-Output ("Time range (UTC): {0:o} to {1:o}" -f $timestamps[0], $timestamps[$timestamps.Count - 1])
    $minimumAvailable = ($availableValues | Measure-Object -Minimum).Minimum
    $averageAvailable = ($availableValues | Measure-Object -Average).Average
    $maximumAvailable = ($availableValues | Measure-Object -Maximum).Maximum
    $maximumWaiters = ($waiterValues | Measure-Object -Maximum).Maximum
    Write-Output ("Available memory KB, summed across semaphores per snapshot: min={0:N0}, avg={1:N2}, max={2:N0}" -f $minimumAvailable, $averageAvailable, $maximumAvailable)
    Write-Output ("Maximum summed semaphore waiters in a snapshot: {0:N0}" -f $maximumWaiters)
}

$waitingTimes = @($waitingObservations | Where-Object { $null -ne $_.wait_time_ms } | ForEach-Object { [double]$_.wait_time_ms })
if ($waitingTimes.Count -gt 0) {
    Write-Output ("WAITING grant observations: {0}; wait time ms: avg={1:N2}, max={2:N0}" -f $waitingObservations.Count, ($waitingTimes | Measure-Object -Average).Average, ($waitingTimes | Measure-Object -Maximum).Maximum)
}
else {
    Write-Output ("WAITING grant observations: {0}" -f $waitingObservations.Count)
}

if ($groups.Count -gt 0) {
    Write-Output 'Top query_hash / program_name groups (grant observations across snapshots, not unique executions):'
    $topGroupRows = @($groups.Values | Sort-Object -Property snapshot_observations -Descending | Select-Object -First $TopGroups)
    foreach ($group in $topGroupRows) {
        Write-Output ("  query_hash={0}; program_name={1}; grant_observations={2}; snapshots={3}; max_wait_ms={4:N0}" -f $group.query_hash, $group.program_name, $group.snapshot_observations, $group.snapshots.Count, $group.max_wait_time_ms)
    }
}

Write-Output ("Malformed or truncated lines skipped: {0}" -f $malformedLineCount)
if ($malformedLineCount -gt 0) {
    Write-Warning 'JSONL records are line-oriented; incomplete/corrupt lines were skipped.'
}
Write-Output 'Repeated rows are point-in-time observations and must not be interpreted as unique query executions.'

$orderedSnapshotKeys = @($snapshotTimestamps.Keys | Sort-Object { $snapshotTimestamps[$_] })
$semaphoreKeys = @($orderedSnapshotKeys | Where-Object { $snapshotRecords[$_].has_semaphore })
$semaphoreTimestamps = @($semaphoreKeys | ForEach-Object { $snapshotTimestamps[$_] })
$allTimestamps = @($orderedSnapshotKeys | ForEach-Object { $snapshotTimestamps[$_] })
$semaphoreLabelFormat = Get-SnapshotLabelFormat -Timestamps $semaphoreTimestamps
$allLabelFormat = Get-SnapshotLabelFormat -Timestamps $allTimestamps
$semaphoreLabels = @($semaphoreTimestamps | ForEach-Object { $_.ToString($semaphoreLabelFormat, [Globalization.CultureInfo]::InvariantCulture) })
$allLabels = @($allTimestamps | ForEach-Object { $_.ToString($allLabelFormat, [Globalization.CultureInfo]::InvariantCulture) })
$semaphoreSeries = @(
    [pscustomobject]@{
        name = 'Available memory (GiB)'
        color = '#167d8d'
        values = @($semaphoreKeys | ForEach-Object { [double]$snapshotRecords[$_].available_memory_kb / 1048576.0 })
    },
    [pscustomobject]@{
        name = 'Target memory (GiB)'
        color = '#d57a27'
        values = @($semaphoreKeys | ForEach-Object { [double]$snapshotRecords[$_].target_memory_kb / 1048576.0 })
    }
)
$requestedSeries = @(
    [pscustomobject]@{
        name = 'Waiting requested (GiB)'
        color = '#d57a27'
        values = @($orderedSnapshotKeys | ForEach-Object { [double]$snapshotRecords[$_].waiting_requested_memory_kb / 1048576.0 })
    },
    [pscustomobject]@{
        name = 'Granted requested (GiB)'
        color = '#167d8d'
        values = @($orderedSnapshotKeys | ForEach-Object { [double]$snapshotRecords[$_].granted_requested_memory_kb / 1048576.0 })
    }
)
$waitSeries = @(
    [pscustomobject]@{
        name = 'Maximum waiting-grant wait (ms)'
        color = '#b34855'
        values = @($orderedSnapshotKeys | ForEach-Object { [double]$snapshotRecords[$_].max_wait_time_ms })
    }
)
$topGroupRows = @($groups.Values | Sort-Object -Property snapshot_observations -Descending | Select-Object -First $TopGroups)
$htmlParts = New-Object 'System.Collections.Generic.List[string]'
$htmlParts.Add('<!doctype html><html lang="en"><head><meta charset="utf-8"><meta name="viewport" content="width=device-width, initial-scale=1"><title>Memory grants report</title><style>')
$htmlParts.Add('body{margin:0;background:#f4f6f5;color:#172a2d;font:15px/1.5 Segoe UI,Arial,sans-serif}main{max-width:1120px;margin:0 auto;padding:28px 22px 48px}h1{font-size:28px;margin:0 0 4px}.subtitle{color:#536467;margin:0 0 24px}.charts{display:grid;grid-template-columns:repeat(auto-fit,minmax(min(100%,460px),1fr));gap:16px}.chart{background:#fff;border:1px solid #d7dfdc;border-radius:6px;padding:14px;min-width:0}.chart svg{display:block;width:100%;height:auto;overflow:visible}.chart-title{font-size:15px;font-weight:600;fill:#172a2d}.axis{stroke:#738487;stroke-width:1}.grid{stroke:#e3e9e7;stroke-width:1}.tick,.legend,.empty{font-size:11px;fill:#536467}.legend{font-size:12px}.empty{font-size:14px}table{width:100%;border-collapse:collapse;background:#fff;margin:12px 0 0}th,td{text-align:left;border-bottom:1px solid #d7dfdc;padding:9px 10px;overflow-wrap:anywhere}th{font-size:12px;text-transform:uppercase;color:#536467}section{margin-top:26px}h2{font-size:19px;margin:0 0 8px}.note{color:#536467;font-size:13px}.empty-table{color:#536467;padding:14px}@media(max-width:520px){main{padding:18px 12px 32px}.chart{padding:9px}}</style></head><body><main><h1>Memory grants</h1>')
$htmlParts.Add(("<p class='subtitle'>{0} unique snapshots; {1} malformed or truncated lines skipped.</p>" -f $snapshots.Count, $malformedLineCount))
$htmlParts.Add('<div class="charts">')
$htmlParts.Add(("<section class='chart'>{0}</section>" -f (New-SvgChart -ChartId 'semaphore-memory' -Title 'Semaphore available vs target memory (GiB)' -Kind 'line' -Series $semaphoreSeries -Labels $semaphoreLabels)))
$htmlParts.Add(("<section class='chart'>{0}</section>" -f (New-SvgChart -ChartId 'requested-memory' -Title 'Requested memory: waiting vs granted (GiB)' -Kind 'stacked-bar' -Series $requestedSeries -Labels $allLabels)))
$htmlParts.Add(("<section class='chart'>{0}</section>" -f (New-SvgChart -ChartId 'maximum-wait' -Title 'Maximum wait for waiting grants (ms)' -Kind 'line' -Series $waitSeries -Labels $allLabels)))
$htmlParts.Add('</div><p class="note">Grant and semaphore values are point-in-time observations. Repeated observations across snapshots are not unique query totals or executions.</p><section><h2>Top query-hash and program observations</h2>')
if ($topGroupRows.Count -eq 0) {
    $htmlParts.Add('<p class="empty-table">No grant observations in this log.</p>')
}
else {
    $htmlParts.Add('<table><thead><tr><th>Query hash</th><th>Program</th><th>Grant observations</th><th>Snapshots</th><th>Maximum wait (ms)</th></tr></thead><tbody>')
    foreach ($group in $topGroupRows) {
        $htmlParts.Add(("<tr><td>{0}</td><td>{1}</td><td>{2}</td><td>{3}</td><td>{4}</td></tr>" -f (ConvertTo-HtmlText $group.query_hash), (ConvertTo-HtmlText $group.program_name), $group.snapshot_observations, $group.snapshots.Count, (ConvertTo-SvgNumber $group.max_wait_time_ms)))
    }
    $htmlParts.Add('</tbody></table>')
}
$htmlParts.Add('</section></main></body></html>')
$htmlEncoding = New-Object System.Text.UTF8Encoding($false)
[System.IO.File]::WriteAllText($resolvedHtmlPath, [string]::Join([Environment]::NewLine, $htmlParts.ToArray()), $htmlEncoding)
Write-Output ("HTML report: {0}" -f $resolvedHtmlPath)