#Requires -Version 7
<#
.SYNOPSIS
    Builds and runs all embedded benchmarks, then assembles the results
    into a single Markdown file placed next to this script.

.DESCRIPTION
    1.  Builds the benchmark project in Release mode (dotnet build).
    2.  Runs every benchmark class via BenchmarkDotNet.
        BenchmarkDotNet artifact files are written to a "BenchmarkArtifacts"
        subdirectory next to this script.
    3.  Parses the per-class *-report-github.md (or *-report.md) files.
    4.  Assembles them into a document that mirrors the Performance section
        in README.md:  same heading hierarchy, pseudocode blocks, and tables.

.PARAMETER Filter
    BenchmarkDotNet --filter glob (default: '*' = all benchmarks).
    Examples:  '*Insert*'  '*Select*'  '*SelectSinglePK*'

.PARAMETER OutputFile
    Path for the generated Markdown file.
    Default: benchmark-results-<yyyyMMdd-HHmmss>.md  next to this script.

.PARAMETER NoBuild
    Skip the dotnet build step (use when the binary is already up-to-date).

.PARAMETER KeepArtifacts
    Keep the BenchmarkArtifacts directory after the report is built.
    Default: the directory is removed once the Markdown is written.

.EXAMPLE
    .\Run-Benchmarks.ps1
    .\Run-Benchmarks.ps1 -Filter '*Insert*' -NoBuild
    .\Run-Benchmarks.ps1 -Filter '*Select*' -OutputFile results.md -KeepArtifacts
#>
param(
    [string] $Filter         = '*',
    [string] $OutputFile     = '',
    [switch] $NoBuild,
    [switch] $KeepArtifacts
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

# ─── Paths ────────────────────────────────────────────────────────────────────
$ScriptDir    = if ($PSScriptRoot) { $PSScriptRoot } else { (Get-Location).Path }
$ArtifactsDir = Join-Path $ScriptDir 'BenchmarkArtifacts'
$ResultsDir   = Join-Path $ArtifactsDir 'results'
$CsprojPath   = Join-Path $ScriptDir 'ReindexerNet.EmbeddedBenchmarks.csproj'

if (-not $OutputFile) {
    $ts         = Get-Date -Format 'yyyyMMdd-HHmmss'
    $OutputFile = Join-Path $ScriptDir "benchmark-results-$ts.md"
}

# ─── 1. Build ──────────────────────────────────────────────────────────────────
if (-not $NoBuild) {
    Write-Host '► Building (Release)…' -ForegroundColor Cyan
    & dotnet build $ScriptDir -c Release --nologo -v quiet
    if ($LASTEXITCODE -ne 0) { throw "dotnet build failed (exit $LASTEXITCODE)" }
    Write-Host '  Build OK.' -ForegroundColor DarkGreen
}

# ─── 1b. Remove orphaned Claude Code worktrees ────────────────────────────────
# BenchmarkDotNet walks up from the assembly output directory to the solution
# root (.slnx), then searches *recursively* from there for a .csproj whose name
# matches the benchmark assembly.  Claude Code sometimes leaves behind a worktree
# at .claude/worktrees/<name>/ containing a full copy of the repo tree, so the
# search finds two copies and throws NotSupportedException.
# We prune any worktree directory that is NOT registered with git.
$repoRoot = & git -C $ScriptDir rev-parse --show-toplevel 2>$null
if ($LASTEXITCODE -eq 0 -and $repoRoot) {
    # Refresh git's own stale-worktree metadata first
    & git -C $repoRoot worktree prune 2>$null

    $claudeWorktrees = Join-Path $repoRoot '.claude\worktrees'
    if (Test-Path $claudeWorktrees) {
        # Collect paths of every currently-registered worktree (lowercased for comparison)
        $registeredPaths = (& git -C $repoRoot worktree list --porcelain 2>$null) |
            Where-Object { $_ -match '^worktree ' } |
            ForEach-Object { ($_ -replace '^worktree\s+', '').Trim().ToLower() }

        Get-ChildItem $claudeWorktrees -Directory -ErrorAction SilentlyContinue |
            Where-Object { $_.FullName.ToLower() -notin $registeredPaths } |
            ForEach-Object {
                Write-Host "  Removing orphaned worktree: $($_.Name)" -ForegroundColor DarkYellow
                Remove-Item $_.FullName -Recurse -Force -ErrorAction SilentlyContinue
            }
    }
}

# ─── 2. Run benchmarks ────────────────────────────────────────────────────────
Write-Host "► Running benchmarks  [filter: $Filter]  …" -ForegroundColor Cyan
Write-Host "  Artifacts → $ArtifactsDir" -ForegroundColor DarkGray
Write-Host '  This may take a while.' -ForegroundColor DarkGray

Push-Location $ScriptDir
try {
    # --exporters markdown  forces MarkdownExporter.GitHub for ALL benchmark
    # classes — including SelectBenchmarkBase subclasses which use ManualConfig
    # without an explicit exporter.  The resulting files are *-report-github.md.
    & dotnet run -c Release --no-build -- `
        --filter      $Filter `
        --artifacts   $ArtifactsDir `
        --exporters   markdown
    if ($LASTEXITCODE -ne 0) { throw "Benchmark run failed (exit $LASTEXITCODE)" }
} finally {
    Pop-Location
}

# ─── Helpers ──────────────────────────────────────────────────────────────────

# Extract the ```ini … ``` environment block from a BenchmarkDotNet GitHub-md report.
function Get-EnvBlock {
    param([string] $FilePath)
    if (-not $FilePath -or -not (Test-Path $FilePath)) { return '' }
    $lines  = [System.IO.File]::ReadAllLines($FilePath)
    $inside = $false
    $block  = [System.Collections.Generic.List[string]]::new()
    foreach ($line in $lines) {
        if ($line -match '^\s*```\s*ini\s*$')  { $inside = $true; continue }
        if ($inside -and $line -match '^\s*```\s*$') { break }
        if ($inside) { $block.Add($line) }
    }
    # Fallback: if no ini block (PlainExporter), grab non-table header lines
    if ($block.Count -eq 0) {
        foreach ($line in $lines) {
            if ($line -match '^\|') { break }
            $t = $line.Trim()
            if ($t -and $t -notmatch '^```') { $block.Add($t) }
        }
    }
    return ($block -join "`n").Trim()
}

# Extract the markdown table (lines starting with '|') from a report file.
# BenchmarkDotNet inserts blank separator rows between param groups — those are kept.
function Get-MarkdownTable {
    param([string] $FilePath)
    if (-not $FilePath -or -not (Test-Path $FilePath)) { return $null }
    $lines = [System.IO.File]::ReadAllLines($FilePath)

    # Find first header row
    $start = -1
    for ($i = 0; $i -lt $lines.Length; $i++) {
        if ($lines[$i] -match '^\|') { $start = $i; break }
    }
    if ($start -lt 0) { return $null }

    $result = [System.Collections.Generic.List[string]]::new()
    for ($i = $start; $i -lt $lines.Length; $i++) {
        if ($lines[$i] -match '^\|' -or [string]::IsNullOrWhiteSpace($lines[$i])) {
            $result.Add($lines[$i])
        } else {
            break  # hit warnings/legends/etc.
        }
    }
    # Trim trailing blank lines
    while ($result.Count -gt 0 -and [string]::IsNullOrWhiteSpace($result[$result.Count - 1])) {
        $result.RemoveAt($result.Count - 1)
    }
    return $result -join "`n"
}

# Locate the latest report file for a given benchmark class name.
# Tries -report-github.md first (from --exporters markdown), then -report.md
# (from [PlainExporter] attribute).
function Find-ReportFile {
    param([string] $ClassName)
    foreach ($suffix in '-report-github.md', '-report.md') {
        $found = Get-ChildItem $ResultsDir -Filter "*${ClassName}${suffix}" `
                     -ErrorAction SilentlyContinue |
                 Sort-Object LastWriteTime -Descending |
                 Select-Object -First 1
        if ($found) { return $found.FullName }
    }
    return $null
}

# Read package versions from the .csproj to include in the report header.
function Get-PackageVersions {
    param([string] $CsprojFile)
    $versions = [ordered]@{}
    if (-not (Test-Path $CsprojFile)) { return $versions }
    [xml]$xml = Get-Content $CsprojFile
    $xml.Project.ItemGroup | ForEach-Object {
        $ig = [System.Xml.XmlElement]$_
        # Direct references (compile-time). SelectNodes never throws on absent children.
        $ig.SelectNodes('PackageReference') | Where-Object { $_.Include } | ForEach-Object {
            $versions[$_.Include] = $_.Version
        }
        # Download-only references (runtime isolation, e.g. v3)
        $ig.SelectNodes('PackageDownload') | Where-Object { $_.Include } | ForEach-Object {
            $v = $_.Version -replace '^\[|\]$', ''
            $key = "$($_.Include) [v3 isolated]"
            $versions[$key] = $v
        }
    }
    return $versions
}

# ─── 3. Validate results directory ────────────────────────────────────────────
if (-not (Test-Path $ResultsDir)) {
    throw "Results directory not found: $ResultsDir`nThe benchmark run may have produced no output."
}

# ─── 4. Collect env info and package versions ─────────────────────────────────
$firstReport = Get-ChildItem $ResultsDir -Filter '*-report-github.md' |
               Sort-Object LastWriteTime -Descending |
               Select-Object -First 1
if (-not $firstReport) {
    $firstReport = Get-ChildItem $ResultsDir -Filter '*-report.md' |
                   Sort-Object LastWriteTime -Descending |
                   Select-Object -First 1
}
$envBlock = if ($firstReport) { Get-EnvBlock $firstReport.FullName } else { '' }
$pkgVersions = Get-PackageVersions $CsprojPath

# ─── 5. Assemble Markdown ─────────────────────────────────────────────────────
$runDate = Get-Date -Format 'yyyy-MM-dd HH:mm'
$sb = [System.Text.StringBuilder]::new()

$null = $sb.AppendLine('# ReindexerNet Embedded Benchmarks')
$null = $sb.AppendLine()
$null = $sb.AppendLine("> Generated: $runDate")
$null = $sb.AppendLine()

# Environment info block
if ($envBlock) {
    $null = $sb.AppendLine('```ini')
    $null = $sb.AppendLine($envBlock)
    $null = $sb.AppendLine('```')
    $null = $sb.AppendLine()
}

# Package versions table
if ($pkgVersions.Count -gt 0) {
    $null = $sb.AppendLine('## Package Versions')
    $null = $sb.AppendLine()
    $null = $sb.AppendLine('| Package | Version |')
    $null = $sb.AppendLine('|---------|---------|')
    foreach ($kv in $pkgVersions.GetEnumerator()) {
        # Only emit the packages relevant for benchmark comparison
        $include = $kv.Key -match '(?i)(ReindexerNet\.Embedded|BenchmarkDotNet|Cachalot|LiteDB|Realm|SpanJson|MessagePack)'
        if ($include) {
            $null = $sb.AppendLine("| $($kv.Key) | $($kv.Value) |")
        }
    }
    $null = $sb.AppendLine()
}

# ── Insert benchmarks ─────────────────────────────────────────────────────────
$null = $sb.AppendLine('## Insert Benchmarks')
$null = $sb.AppendLine()

$insertSections = @(
    [pscustomobject]@{
        Class = 'InsertBenchmark'
        Title = 'Insert (Without controlling existence)'
    }
    [pscustomobject]@{
        Class = 'UpsertBenchmark'
        Title = 'Upsert (Update if exists, otherwise insert)'
    }
)

foreach ($entry in $insertSections) {
    $reportFile = Find-ReportFile $entry.Class
    $table      = Get-MarkdownTable $reportFile
    if (-not $table) {
        Write-Warning "No report found for $($entry.Class) — section skipped."
        continue
    }
    $null = $sb.AppendLine("### $($entry.Title)")
    $null = $sb.AppendLine()
    $null = $sb.AppendLine($table)
    $null = $sb.AppendLine()
}

# ── Select benchmarks ─────────────────────────────────────────────────────────
$null = $sb.AppendLine('## Select Benchmarks')
$null = $sb.AppendLine()

$selectSections = @(
    [pscustomobject]@{
        Class      = 'SelectSinglePK'
        Title      = 'Search Single Primary Key (Guid) in a Loop'
        QueryBlock = "foreach (id in ids)`n{`n    WHERE Id = id`n}"
    }
    [pscustomobject]@{
        Class      = 'SelectMultiplePK'
        Title      = 'Search Multiple Primary Keys (Guid) at Once'
        QueryBlock = 'WHERE Id IN (id_1,id_2,id_3,...,id_N)'
    }
    [pscustomobject]@{
        Class      = 'SelectSingleHash'
        Title      = 'Search Single Hash Index (string) in a Loop'
        QueryBlock = "foreach (value in values)`n{`n    WHERE StringProperty = value`n}"
    }
    [pscustomobject]@{
        Class      = 'SelectSingleHashParallel'
        Title      = 'Search Single Hash Index (string) in a Parallel Loop'
        QueryBlock = "Parallel.ForEach(values, value =>`n{`n    WHERE StringProperty = value`n});"
    }
    [pscustomobject]@{
        Class      = 'SelectMultipleHash'
        Title      = 'Search Multiple Hash Values at Once'
        QueryBlock = "WHERE StringProperty IN ('abc', 'def', ....)"
    }
    [pscustomobject]@{
        Class      = 'SelectRange'
        Title      = 'Range Filtering'
        QueryBlock = "WHERE IntProperty < i`nWHERE IntProperty >= i"
    }
    [pscustomobject]@{
        Class      = 'SelectArraySingle'
        Title      = 'Array Column Filtering (Single Item IN)'
        QueryBlock = "WHERE N IN Integer_Array`nWHERE 'N' IN String_Array"
    }
    [pscustomobject]@{
        Class      = 'SelectArrayMultiple'
        Title      = 'Array Column Filtering (Contains, All)'
        QueryBlock = "WHERE Integer_Array CONTAINS (1,2,3,...,N)`nWHERE String_Array  CONTAINS ('abc','def',...)`nWHERE Integer_Array ALL (1,2,3,...,N)`nWHERE String_Array  ALL ('abc','def',...)"
    }
)

foreach ($section in $selectSections) {
    $reportFile = Find-ReportFile $section.Class
    $table      = Get-MarkdownTable $reportFile
    if (-not $table) {
        Write-Warning "No report found for $($section.Class) — section skipped."
        continue
    }
    $null = $sb.AppendLine("### $($section.Title)")
    $null = $sb.AppendLine()
    $null = $sb.AppendLine('```')
    $null = $sb.AppendLine($section.QueryBlock)
    $null = $sb.AppendLine('```')
    $null = $sb.AppendLine()
    $null = $sb.AppendLine($table)
    $null = $sb.AppendLine()
}

# ─── 6. Write output file ─────────────────────────────────────────────────────
[System.IO.File]::WriteAllText($OutputFile, $sb.ToString(), [System.Text.Encoding]::UTF8)
Write-Host ''
Write-Host "✔  Report written → $OutputFile" -ForegroundColor Green

# ─── 7. Clean up artifacts ────────────────────────────────────────────────────
if (-not $KeepArtifacts -and (Test-Path $ArtifactsDir)) {
    Remove-Item $ArtifactsDir -Recurse -Force -ErrorAction SilentlyContinue
    Write-Host "   BenchmarkArtifacts cleaned up  (-KeepArtifacts to retain)." -ForegroundColor DarkGray
}
