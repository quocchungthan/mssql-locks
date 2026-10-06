$ErrorActionPreference = 'Stop'

$reportScript = Join-Path $PSScriptRoot 'Show-MemoryGrantsReport.ps1'
$testDirectory = Join-Path ([System.IO.Path]::GetTempPath()) ('memory-grants-report-test-' + [guid]::NewGuid().ToString('N'))
[System.IO.Directory]::CreateDirectory($testDirectory) | Out-Null

function Assert-ReportCondition {
    param(
        [bool]$Condition,
        [string]$Message
    )

    if (-not $Condition) {
        throw $Message
    }
}

function Write-TestJsonl {
    param(
        [string]$Path,
        [object[]]$Records
    )

    $jsonLines = @($Records | ForEach-Object { ConvertTo-Json -InputObject $_ -Compress -Depth 5 })
    [System.IO.File]::WriteAllLines($Path, $jsonLines, (New-Object System.Text.UTF8Encoding($false)))
}

function Assert-ReportSvgs {
    param(
        [string]$Html,
        [int]$ExpectedCount
    )

    $svgMatches = [regex]::Matches($Html, '(?s)<svg\b.*?</svg>')
    Assert-ReportCondition ($svgMatches.Count -eq $ExpectedCount) "Expected $ExpectedCount SVG charts; found $($svgMatches.Count)."
    foreach ($svgMatch in $svgMatches) {
        $xml = New-Object System.Xml.XmlDocument
        $xml.LoadXml($svgMatch.Value)
    }
}

try {
    $logPath = Join-Path $testDirectory 'synthetic.jsonl'
    $defaultHtmlPath = Join-Path $testDirectory 'memory-grants-report.html'
    $explicitHtmlPath = Join-Path $testDirectory 'explicit-report.html'
    $firstTimestamp = '2026-09-30T12:00:00+00:00'
    $secondTimestamp = '2026-09-30T12:00:05+00:00'
    $records = @(
        [ordered]@{
            snapshot_utc = $firstTimestamp
            record_type = 'SEMAPHORE'
            pool_id = 1
            resource_semaphore_id = 1
            semaphore_available_memory_kb = 1048576
            semaphore_target_memory_kb = 2097152
            semaphore_waiter_count = 1
        },
        [ordered]@{
            snapshot_utc = $firstTimestamp
            record_type = 'GRANT'
            grant_state = 'WAITING'
            requested_memory_kb = 524288
            wait_time_ms = 100
            query_hash = '<hash>&one'
            program_name = '<img src=x onerror=alert(1)>'
            query_text = 'PRIVATE_SQL_SENTINEL'
        },
        [ordered]@{
            snapshot_utc = $firstTimestamp
            record_type = 'GRANT'
            grant_state = 'GRANTED'
            requested_memory_kb = 1048576
            granted_memory_kb = 786432
            wait_time_ms = 0
            query_hash = '<hash>&one'
            program_name = '<img src=x onerror=alert(1)>'
            query_text = 'PRIVATE_SQL_SENTINEL'
        },
        [ordered]@{
            snapshot_utc = $secondTimestamp
            record_type = 'SEMAPHORE'
            pool_id = 1
            resource_semaphore_id = 1
            semaphore_available_memory_kb = 3145728
            semaphore_target_memory_kb = 4194304
            semaphore_waiter_count = 2
        },
        [ordered]@{
            snapshot_utc = $secondTimestamp
            record_type = 'GRANT'
            grant_state = 'WAITING'
            requested_memory_kb = 262144
            wait_time_ms = 400
            query_hash = '<hash>&one'
            program_name = '<img src=x onerror=alert(1)>'
            query_text = 'PRIVATE_SQL_SENTINEL'
        }
    )

    Write-TestJsonl -Path $logPath -Records $records
    $aliasedSameFilePath = [System.IO.Path]::Combine($testDirectory, '.', 'SYNTHETIC.JSONL')
    $originalLogContents = [Convert]::ToBase64String([System.IO.File]::ReadAllBytes($logPath))
    $sameFileRejected = $false
    $sameFileError = ''
    try {
        & $reportScript -LogPath $logPath -HtmlPath $aliasedSameFilePath | Out-Null
    }
    catch {
        $sameFileRejected = $true
        $sameFileError = $_.Exception.Message
    }
    Assert-ReportCondition $sameFileRejected 'Same-file HTML output was not rejected.'
    Assert-ReportCondition ($sameFileError.Contains('same file')) 'Same-file refusal did not explain the path conflict.'
    Assert-ReportCondition (
        [Convert]::ToBase64String([System.IO.File]::ReadAllBytes($logPath)) -eq $originalLogContents
    ) 'Same-file refusal modified the JSONL input.'

    $consoleSummary = (& $reportScript -LogPath $logPath -HtmlPath $explicitHtmlPath | Out-String)
    Assert-ReportCondition ($consoleSummary.Contains('Unique snapshots: 2')) 'Existing unique-snapshot summary changed.'
    Assert-ReportCondition ($consoleSummary.Contains('WAITING grant observations: 2')) 'Existing waiting-observation aggregation changed.'
    Assert-ReportCondition ($consoleSummary.Contains('grant_observations=3')) 'Existing query/program observation aggregation changed.'
    Assert-ReportCondition ([System.IO.File]::Exists($explicitHtmlPath)) 'Explicit -HtmlPath was not written.'

    $html = [System.IO.File]::ReadAllText($explicitHtmlPath)
    Assert-ReportSvgs -Html $html -ExpectedCount 3
    Assert-ReportCondition ($html.Contains('Available memory (GiB)')) 'Semaphore available-memory series is missing.'
    Assert-ReportCondition ($html.Contains('Target memory (GiB)')) 'Semaphore target-memory series is missing.'
    Assert-ReportCondition ($html.Contains('Waiting requested (GiB)')) 'Waiting requested-memory series is missing.'
    Assert-ReportCondition ($html.Contains('Granted requested (GiB)')) 'Granted requested-memory series is missing.'
    Assert-ReportCondition ($html.Contains('Maximum waiting-grant wait (ms)')) 'Waiting max-wait series is missing.'
    Assert-ReportCondition ($html.Contains('point-in-time observations')) 'Repeated-observation explanation is missing.'
    Assert-ReportCondition ($html.Contains('09-30 12:00:00')) 'Closely spaced samples did not include seconds in the x-axis labels.'
    Assert-ReportCondition ($html.Contains('09-30 12:00:05')) 'Closely spaced samples did not retain distinct x-axis labels.'
    Assert-ReportCondition ($html.Contains('&lt;img src=x onerror=alert(1)&gt;')) 'Untrusted program label was not HTML encoded.'
    Assert-ReportCondition ($html.Contains('&lt;hash&gt;&amp;one')) 'Untrusted query hash was not HTML encoded.'
    Assert-ReportCondition (-not $html.Contains('<img src=x')) 'Raw untrusted HTML was included in the report.'
    Assert-ReportCondition (-not $html.Contains('PRIVATE_SQL_SENTINEL')) 'Optional SQL text was included in the report.'
    Assert-ReportCondition (-not [regex]::IsMatch($html, '(?i)<script\b|<script\s+src=|<link\b|<img\b|@import')) 'The report contains a script or external resource.'

    & $reportScript -LogPath $logPath 2>$null | Out-Null
    Assert-ReportCondition ([System.IO.File]::Exists($defaultHtmlPath)) 'Default HTML output was not created next to the log.'

    $emptyLogPath = Join-Path $testDirectory 'empty.jsonl'
    $emptyHtmlPath = Join-Path $testDirectory 'empty.html'
    [System.IO.File]::WriteAllText($emptyLogPath, '', (New-Object System.Text.UTF8Encoding($false)))
    & $reportScript -LogPath $emptyLogPath -HtmlPath $emptyHtmlPath 2>$null | Out-Null
    $emptyHtml = [System.IO.File]::ReadAllText($emptyHtmlPath)
    Assert-ReportSvgs -Html $emptyHtml -ExpectedCount 3
    Assert-ReportCondition ($emptyHtml.Contains('No valid data for this chart')) 'Empty input did not show the no-data state.'
    Assert-ReportCondition ($emptyHtml.Contains('No grant observations in this log.')) 'Empty input did not show the empty observation state.'

    $missingHtmlPath = Join-Path $testDirectory 'missing.html'
    $missingConsole = (& $reportScript -LogPath (Join-Path $testDirectory 'absent.jsonl') -HtmlPath $missingHtmlPath 3>$null | Out-String)
    Assert-ReportCondition ([System.IO.File]::Exists($missingHtmlPath)) 'Missing input did not produce a no-data HTML report.'
    Assert-ReportCondition ($missingConsole.Contains('Unique snapshots: 0')) 'Missing input did not retain the console summary.'

    $malformedLogPath = Join-Path $testDirectory 'malformed.jsonl'
    $malformedHtmlPath = Join-Path $testDirectory 'malformed.html'
    $validSingleSample = ConvertTo-Json -InputObject ([ordered]@{
        snapshot_utc = $firstTimestamp
        record_type = 'SEMAPHORE'
        pool_id = 1
        resource_semaphore_id = 1
        semaphore_available_memory_kb = 0
        semaphore_target_memory_kb = 0
        semaphore_waiter_count = 0
    }) -Compress
    [System.IO.File]::WriteAllLines($malformedLogPath, @('{truncated', $validSingleSample), (New-Object System.Text.UTF8Encoding($false)))
    $malformedConsole = (& $reportScript -LogPath $malformedLogPath -HtmlPath $malformedHtmlPath 3>$null | Out-String)
    $malformedHtml = [System.IO.File]::ReadAllText($malformedHtmlPath)
    Assert-ReportCondition ($malformedConsole.Contains('Malformed or truncated lines skipped: 1')) 'Malformed JSONL was not counted and skipped.'
    Assert-ReportSvgs -Html $malformedHtml -ExpectedCount 3
    Assert-ReportCondition ($malformedHtml.Contains('polyline points=')) 'Single-sample line charts did not render a point.'
    Assert-ReportCondition ($malformedHtml.Contains('09-30 12:00</text>')) 'Single-sample labels did not retain the compact format.'
    Assert-ReportCondition (-not $malformedHtml.Contains('09-30 12:00:00')) 'Single-sample labels unexpectedly included seconds.'
    Assert-ReportCondition (-not [regex]::IsMatch($malformedHtml, '(?i)NaN|Infinity')) 'Single-sample chart geometry is invalid.'

    Write-Output 'PASS: multi-snapshot aggregation, all SVG charts, HTML encoding, SQL-text omission, offline output, empty/missing/malformed inputs, default path, and single-sample SVGs.'
}
finally {
    if ([System.IO.Directory]::Exists($testDirectory)) {
        Remove-Item -LiteralPath $testDirectory -Recurse -Force
    }
}