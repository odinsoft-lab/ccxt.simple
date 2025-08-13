param(
    [switch]$DryRun,
    [switch]$Update,
    [string]$Root = "src/exchanges",
    [string]$Reviewer = "auto",
    [switch]$VerboseHeuristic,
    [switch]$OverrideProgress   # 수동 PROGRESS_STATUS 덮어쓰기 원할 때 지정
)

# ==============================================
# CCXT.Simple Exchange Meta Block Insertion Script
# Usage examples:
#   pwsh scripts/insert-meta.ps1 -DryRun
#   pwsh scripts/insert-meta.ps1
#   pwsh scripts/insert-meta.ps1 -Update
# ==============================================

$standardMethods = @(
    'GetOrderbook',
    'GetPrice',
    'GetCandles',
    'GetTrades',
    'GetBalance',
    'GetAccount',
    'PlaceOrder',
    'CancelOrder',
    'GetOrder',
    'GetOpenOrders',
    'GetOrderHistory',
    'GetTradeHistory',
    'GetDepositAddress',
    'Withdraw',
    'GetDepositHistory',
    'GetWithdrawalHistory'
)

$legacyMethods = @(
    'VerifySymbols',
    'VerifyStates',
    'GetTickers',
    'GetVolumes',
    'GetMarkets',
    'GetBookTickers'
)

$today = (Get-Date).ToString('yyyy-MM-dd')

if (-not (Test-Path $Root)) {
    # Try relative to script directory (scripts/ -> ../src/exchanges)
    $candidate = Join-Path (Split-Path $PSScriptRoot -Parent) 'src/exchanges'
    if (Test-Path $candidate) {
        Write-Host "[info] Provided Root not found. Using detected path: $candidate" -ForegroundColor Yellow
        $Root = $candidate
    }
}

if (-not (Test-Path $Root)) {
    Write-Error "Root path not found: $Root"
    exit 1
}

$files = Get-ChildItem -Path $Root -Recurse -Filter 'X*.cs'
if (-not $files) { Write-Warning 'No exchange files found.'; exit 1 }

$summary = @()

# Regex helper to extract method blocks for deeper heuristic
function Get-MethodBlock {
    param(
        [string]$Code,
        [string]$Method
    )
    # Simplistic capture: match method signature with public / async / ValueTask / Task returning pattern
    $pattern = "(?s)public[^{};]*?\b$Method\s*\([^)]*\)\s*\{(.*?)\}"  # non-greedy inside braces
    $m = [regex]::Matches($Code, $pattern)
    if ($m.Count -gt 0) { return $m[0].Groups[1].Value } else { return $null }
}

function Classify-Method {
    param(
        [string]$Block
    )
    if (-not $Block) { return 'MISSING' }
    if ($Block -match 'throw\s+new\s+NotImplementedException') { return 'UNIMPL' }
    # Real implementation indicators
    $implIndicators = @('HttpClient','SendAsync','GetAsync','PostAsync','JsonConvert','JObject','JArray','mainXchg','await ','WebRequest','RestClient','HttpRequestMessage')
    foreach ($ind in $implIndicators) { if ($Block -match [regex]::Escape($ind)) { return 'IMPL' } }
    # Stub indicators: return default/new empty
    if ($Block -match 'return\s+default' -or $Block -match 'return\s+0;' -or $Block -match 'return\s+new\s+[A-Za-z0-9_]+\s*\(\s*\)\s*;') { return 'STUB' }
    # Fallback: if very short (< 120 chars) treat as stub
    if ($Block.Length -lt 120) { return 'STUB' }
    return 'IMPL'
}

function Get-ImplementedStatus($code, $method) {
    # Heuristic: method signature present?
    $signaturePattern = [regex]::Escape($method) + '\s*\('
    if ($code -notmatch $signaturePattern) { return $false }
    # If any NotImplementedException in same line(s) of method block: quick check
    $niPattern = $method + '.*NotImplementedException'
    if ($code -match $niPattern) { return $false }
    return $true
}

function Get-LegacyImplemented($code) {
    $list = @()
    foreach ($m in $legacyMethods) {
        $pattern = [regex]::Escape($m) + '\s*\('
        if ($code -match $pattern) { $list += $m }
    }
    $list -join ','
}

foreach ($file in $files) {
    $content = Get-Content -Path $file.FullName -Raw
    $hasMeta = $content.Contains('CCXT-SIMPLE-META-BEGIN')
    if ($hasMeta -and -not $Update) {
        $summary += [pscustomobject]@{ File=$file.Name; Status='SKIP(meta-exists)'; Impl=''; Pending=''; FinalStatus='' }
        continue
    }

    # Strip existing meta if updating
    $existingProgress = $null
    $existingManualStatus = $null
    if ($hasMeta) {
        # 추출 (기존 메타 유지용)
        $linesOriginal = $content -split "`r?`n"
        foreach ($line in $linesOriginal) {
            if ($line -match '^// PROGRESS_STATUS:\s*(.*)$') { $existingProgress = $matches[1].Trim() }
            if ($line -match '^// IMPLEMENTATION_STATUS_MANUAL:\s*(.*)$') { $existingManualStatus = $matches[1].Trim() }
        }
    }
    if ($hasMeta -and $Update) {
        $content = ($content -split "`r?`n") | Where-Object { $_ -notmatch 'CCXT-SIMPLE-META-(BEGIN|END)' -and $_ -notmatch '^// (EXCHANGE|IMPLEMENTATION_STATUS(?!_MANUAL)|IMPLEMENTATION_STATUS_MANUAL|PROGRESS_STATUS|CATEGORY|MARKET_SCOPE|STANDARD_METHODS_|LEGACY_METHODS_|NOT_IMPLEMENTED_EXCEPTIONS|LAST_REVIEWED|REVIEWER|NOTES):' } | Out-String
    }

    $notImplCount = ([regex]::Matches($content, 'NotImplementedException')).Count

    $implemented = @()      # Real implemented methods
    $pending = @()          # Unimplemented (throws or missing)
    $stub = @()             # Stub methods (placeholder logic)

    foreach ($m in $standardMethods) {
        $block = Get-MethodBlock -Code $content -Method $m
        $classification = Classify-Method -Block $block
        switch ($classification) {
            'IMPL' { $implemented += $m }
            'UNIMPL' { $pending += $m }
            'MISSING' { $pending += $m }
            'STUB' { $stub += $m }
        }
        if ($VerboseHeuristic) { Write-Host ("[debug] $($file.Name)::$m => $classification") -ForegroundColor DarkGray }
    }

    # Determine implementation status (revised heuristic)
    if ($pending.Count -eq 0 -and $stub.Count -eq 0 -and $implemented.Count -eq $standardMethods.Count) { $implStatus = 'FULL' }
    elseif ($implemented.Count -eq 0) { $implStatus = 'SKELETON' }
    else { $implStatus = 'PARTIAL' }

    # Manual override placeholder (IMPLEMENTATION_STATUS_MANUAL) – 사용자가 직접 추가 가능
    $manualStatus = $existingManualStatus

    # Progress status (수작업용 3단계: DONE / WIP / TODO)
    if (-not $existingProgress -or $OverrideProgress) {
        switch ($implStatus) {
            'FULL' { $progress = 'DONE' }
            'PARTIAL' { $progress = 'WIP' }
            default { $progress = 'TODO' }
        }
    } else {
        $progress = $existingProgress
    }

    # Legacy methods
    $legacy = Get-LegacyImplemented $content
    if (-not $legacy) { $legacy = '' }

    # Exchange name from folder path (lowercase last directory before file)
    $exchangeName = ($file.Directory.Name).ToLower()

    $metaBlock = @()
    $metaBlock += '// == CCXT-SIMPLE-META-BEGIN =='
    $metaBlock += ('// EXCHANGE: ' + $exchangeName)
    if ($manualStatus) {
        # 수동 상태가 존재하면 스크립트 산출치는 IMPLEMENTATION_STATUS_AUTO 로 별도 기록
        $metaBlock += ('// IMPLEMENTATION_STATUS_MANUAL: ' + $manualStatus)
        $metaBlock += ('// IMPLEMENTATION_STATUS: ' + $manualStatus)  # 호환성: 기존 필드도 동일 값 유지
        $metaBlock += ('// IMPLEMENTATION_STATUS_AUTO: ' + $implStatus)
    } else {
        $metaBlock += ('// IMPLEMENTATION_STATUS: ' + $implStatus)
    }
    $metaBlock += ('// PROGRESS_STATUS: ' + $progress)
    $metaBlock += '// CATEGORY: centralized'
    $metaBlock += '// MARKET_SCOPE: spot'
    $metaBlock += ('// STANDARD_METHODS_IMPLEMENTED: ' + ($implemented -join ','))
    $metaBlock += ('// STANDARD_METHODS_PENDING: ' + ($pending -join ','))
    $metaBlock += ('// STANDARD_METHODS_STUB: ' + ($stub -join ','))
    $metaBlock += ('// LEGACY_METHODS_IMPLEMENTED: ' + $legacy)
    $metaBlock += ('// NOT_IMPLEMENTED_EXCEPTIONS: ' + $notImplCount)
    $metaBlock += ('// LAST_REVIEWED: ' + $today)
    $metaBlock += ('// REVIEWER: ' + $Reviewer)
    $metaBlock += '// NOTES: auto-generated by insert-meta.ps1 (heuristic)'
    $metaBlock += '// == CCXT-SIMPLE-META-END =='
    $metaText = ($metaBlock -join [Environment]::NewLine) + [Environment]::NewLine + [Environment]::NewLine

    if (-not $DryRun) {
        # Insert at top (keep any BOM)
        Set-Content -Path $file.FullName -Value ($metaText + $content) -Encoding UTF8
    }

    $summary += [pscustomobject]@{ File=$file.Name; Status=($DryRun ? 'DRY' : ($hasMeta -and $Update ? 'UPDATED' : 'ADDED')); Impl=$implemented.Count; Pending=$pending.Count; Stub=$stub.Count; FinalStatus=$implStatus }
}

$summary | Sort-Object FinalStatus, File | Format-Table -AutoSize

if ($DryRun) {
    Write-Host "Dry run complete. No files modified." -ForegroundColor Yellow
} else {
    Write-Host "Meta insertion complete." -ForegroundColor Green
}

# ---- Summary Counts ----
$total = $summary.Count
$added = ($summary | Where-Object { $_.Status -eq 'ADDED' }).Count
$updated = ($summary | Where-Object { $_.Status -eq 'UPDATED' }).Count
$skipped = ($summary | Where-Object { $_.Status -like 'SKIP*' }).Count
$dry = ($summary | Where-Object { $_.Status -eq 'DRY' }).Count

Write-Host "--- File Processing Summary ---" -ForegroundColor Cyan
Write-Host ("Total scanned : {0}" -f $total)
if ($DryRun) { Write-Host ("Dry-run evaluated : {0}" -f $dry) }
Write-Host ("Added meta     : {0}" -f $added)
Write-Host ("Updated meta   : {0}" -f $updated)
Write-Host ("Skipped (exists): {0}" -f $skipped)

# Implementation status distribution + stub/unimpl stats
$statusDist = $summary | Group-Object FinalStatus | Sort-Object Name
Write-Host "--- Implementation Status Distribution ---" -ForegroundColor Cyan
foreach ($g in $statusDist) { Write-Host ("{0,-10} : {1}" -f $g.Name, $g.Count) }
$totalStubs = ($summary | Measure-Object -Property Stub -Sum).Sum
$totalPending = ($summary | Measure-Object -Property Pending -Sum).Sum
Write-Host ("Total stub methods     : {0}" -f $totalStubs)
Write-Host ("Total unimplemented    : {0}" -f $totalPending)
