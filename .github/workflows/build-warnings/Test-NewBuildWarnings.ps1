<#
.SYNOPSIS
    Builds the SDK library projects and fails when a critical compiler warning appears on a changed line.

.DESCRIPTION
    Each project listed in critical-warnings.json is rebuilt (--no-incremental, so the compiler runs and reports
    every warning) with an MSBuild file logger that records warnings only. The warnings are deduplicated across
    projects and target frameworks, then compared with the lines changed since the merge base with -Base:
      - a warning with a source location counts as new when its line was added or modified;
      - a warning without a line, or reported from a file outside the repository (NuGet and MSBuild warnings),
        counts as new when its project file changed.
    Locally, uncommitted and untracked files count as changed too.

    New warnings whose ID is listed under "critical" in the configuration fail the check; the others are only
    reported. In GitHub Actions they become error and warning annotations, and the full list is written to the
    job summary. The same summary is saved as new-warnings.md in -LogDirectory.

    Exit codes: 0 no new critical warnings, 1 new critical warnings, 2 a build failed or the base was not found.

.PARAMETER Base
    The commit or ref to compare against; the merge base of it and HEAD is used. Defaults to origin/master.

.PARAMETER ConfigPath
    The JSON file with the project list and the critical warning IDs. Defaults to critical-warnings.json next to
    this script.

.PARAMETER Scope
    Lines (default) keeps warnings on changed lines only. File keeps every warning in a changed file, which also
    catches warnings caused indirectly by a change elsewhere in the same file, but includes the file's older
    warnings too, so it can fail on warnings the change didn't introduce.

.PARAMETER LogDirectory
    Where the build logs and new-warnings.md are written. Defaults to pubnub-build-warnings in the temp folder.

.PARAMETER SkipBuild
    Reuse the build logs already in -LogDirectory instead of building again.

.EXAMPLE
    powershell -ExecutionPolicy Bypass -File .github/workflows/build-warnings/Test-NewBuildWarnings.ps1
    pwsh -File .github/workflows/build-warnings/Test-NewBuildWarnings.ps1 -Base HEAD~1 -Scope File
#>
[CmdletBinding()]
param(
    [string]$Base = 'origin/master',
    [string]$ConfigPath,
    [ValidateSet('Lines', 'File')]
    [string]$Scope = 'Lines',
    [string]$LogDirectory = (Join-Path ([System.IO.Path]::GetTempPath()) 'pubnub-build-warnings'),
    [switch]$SkipBuild
)

$ErrorActionPreference = 'Stop'
$inActions = $env:GITHUB_ACTIONS -eq 'true'
$LogDirectory = $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath($LogDirectory)
if ($ConfigPath) { $ConfigPath = $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath($ConfigPath) }

# Windows PowerShell 5.1 turns stderr output of native commands into terminating errors under 'Stop'.
function Invoke-Native {
    param([string]$FilePath, [string[]]$Arguments)
    $ErrorActionPreference = 'Continue'
    $output = & $FilePath @Arguments 2>$null
    return [pscustomobject]@{ Output = @($output); ExitCode = $LASTEXITCODE }
}

function ConvertTo-RepoPath {
    param([string]$Path)
    try {
        $full = [System.IO.Path]::GetFullPath($Path)
    } catch {
        return $null
    }
    if (-not $full.StartsWith($repoRootPrefix, [System.StringComparison]::OrdinalIgnoreCase)) { return $null }
    return $full.Substring($repoRootPrefix.Length).Replace('\', '/')
}

function Format-CommandValue {
    param([string]$Value, [switch]$Property)
    $Value = $Value.Replace('%', '%25').Replace("`r", '%0D').Replace("`n", '%0A')
    if ($Property) { $Value = $Value.Replace(':', '%3A').Replace(',', '%2C') }
    return $Value
}

$repoRoot = (Invoke-Native git @('rev-parse', '--show-toplevel')).Output | Select-Object -First 1
if (-not $repoRoot) {
    Write-Host 'FAIL: not inside a git repository' -ForegroundColor Red
    exit 2
}
$repoRoot = [System.IO.Path]::GetFullPath($repoRoot.Trim())
$repoRootPrefix = $repoRoot.TrimEnd('\', '/') + [System.IO.Path]::DirectorySeparatorChar
Set-Location $repoRoot

if (-not $ConfigPath) { $ConfigPath = Join-Path $PSScriptRoot 'critical-warnings.json' }
$config = Get-Content -LiteralPath $ConfigPath -Raw | ConvertFrom-Json
$critical = New-Object 'System.Collections.Generic.Dictionary[string,string]' ([System.StringComparer]::OrdinalIgnoreCase)
foreach ($property in $config.critical.PSObject.Properties) { $critical[$property.Name] = [string]$property.Value }

$mergeBase = (Invoke-Native git @('merge-base', $Base, 'HEAD')).Output | Select-Object -First 1
if (-not $mergeBase) {
    Write-Host "FAIL: no merge base between '$Base' and HEAD; fetch the full history (fetch-depth: 0) or pass -Base" -ForegroundColor Red
    exit 2
}
$mergeBase = $mergeBase.Trim()

# 1. Build.
New-Item -ItemType Directory -Force -Path $LogDirectory | Out-Null
$logs = @()
foreach ($project in $config.projects) {
    $configuration = if ($project.configuration) { $project.configuration } else { 'Release' }
    $log = Join-Path $LogDirectory ([System.IO.Path]::GetFileNameWithoutExtension($project.path) + '.warnings.log')
    $logs += $log
    if ($SkipBuild) {
        if (-not (Test-Path -LiteralPath $log)) {
            Write-Host "FAIL: -SkipBuild was passed but $log doesn't exist" -ForegroundColor Red
            exit 2
        }
        continue
    }
    Write-Host "Building $($project.path) ($configuration)"
    $ErrorActionPreference = 'Continue'
    & dotnet build $project.path -c $configuration --no-incremental -nologo '-clp:ErrorsOnly' "-flp:logfile=$log;warningsonly;verbosity=quiet"
    $buildExitCode = $LASTEXITCODE
    $ErrorActionPreference = 'Stop'
    if ($buildExitCode -ne 0) {
        Write-Host "FAIL: building $($project.path) failed with exit code $buildExitCode" -ForegroundColor Red
        exit 2
    }
}

# 2. Parse and deduplicate.
$warningPattern = '^(?<file>.+?)(\((?<line>\d+)(,(?<col>\d+))?(,\d+,\d+)?\))?\s*:\s+warning\s+(?<id>[A-Za-z]+\d+)\s*:\s*(?<msg>.*?)(\s+\[(?<proj>[^\[\]]+)\])?\s*$'
$warnings = [ordered]@{}
$totalLines = 0
foreach ($log in $logs) {
    foreach ($text in [System.IO.File]::ReadAllLines($log)) {
        $match = [regex]::Match($text, $warningPattern)
        if (-not $match.Success) { continue }
        $totalLines++
        $projectFile = ''
        $framework = ''
        if ($match.Groups['proj'].Success) {
            $parts = $match.Groups['proj'].Value -split '::TargetFramework=', 2
            $projectFile = ConvertTo-RepoPath $parts[0]
            if ($parts.Count -gt 1) { $framework = $parts[1] }
        }
        $file = $match.Groups['file'].Value.Trim()
        $repoFile = ConvertTo-RepoPath $file
        $line = if ($match.Groups['line'].Success) { [int]$match.Groups['line'].Value } else { 0 }
        $col = if ($match.Groups['col'].Success) { [int]$match.Groups['col'].Value } else { 0 }
        $id = $match.Groups['id'].Value.ToUpperInvariant()
        $message = $match.Groups['msg'].Value
        $location = if ($repoFile) { $repoFile } else { $file }
        $key = "$id|$($location.ToLowerInvariant())|$line|$col|$message"
        if (-not $warnings.Contains($key)) {
            $warnings[$key] = [pscustomobject]@{
                Id         = $id
                File       = $repoFile
                RawFile    = $file
                Line       = $line
                Column     = $col
                Message    = $message
                Projects   = New-Object 'System.Collections.Generic.SortedSet[string]' ([System.StringComparer]::OrdinalIgnoreCase)
                Frameworks = New-Object 'System.Collections.Generic.SortedSet[string]' ([System.StringComparer]::OrdinalIgnoreCase)
            }
        }
        if ($projectFile) { [void]$warnings[$key].Projects.Add($projectFile) }
        if ($framework) { [void]$warnings[$key].Frameworks.Add($framework) }
    }
}

# 3. Changed lines since the merge base (working tree, so uncommitted changes count locally).
$changed = New-Object 'System.Collections.Generic.Dictionary[string,System.Collections.Generic.List[int[]]]' ([System.StringComparer]::OrdinalIgnoreCase)
$diff = Invoke-Native git @('-c', 'core.quotePath=false', 'diff', '--no-color', '--no-ext-diff', '--unified=0', '-M',
    '--diff-filter=AMR', '--src-prefix=a/', '--dst-prefix=b/', $mergeBase, '--')
if ($diff.ExitCode -ne 0) {
    Write-Host "FAIL: git diff against $mergeBase failed" -ForegroundColor Red
    exit 2
}
$currentFile = $null
$previous = ''
foreach ($text in $diff.Output) {
    $isHeader = $text.StartsWith('+++ ') -and $previous.StartsWith('--- ')
    $previous = [string]$text
    if ($isHeader) {
        $path = $text.Substring(4).TrimEnd("`t")
        $currentFile = if ($path.StartsWith('b/')) { $path.Substring(2) } else { $null }
        if ($currentFile -and -not $changed.ContainsKey($currentFile)) {
            $changed[$currentFile] = New-Object 'System.Collections.Generic.List[int[]]'
        }
    } elseif ($currentFile -and $text -match '^@@ -\d+(,\d+)? \+(?<start>\d+)(,(?<count>\d+))? @@') {
        $start = [int]$Matches['start']
        $count = if ($Matches['count']) { [int]$Matches['count'] } else { 1 }
        if ($count -gt 0) { $changed[$currentFile].Add(@($start, ($start + $count - 1))) }
    }
}
foreach ($path in (Invoke-Native git @('-c', 'core.quotePath=false', 'ls-files', '--others', '--exclude-standard')).Output) {
    if ($path) {
        $list = New-Object 'System.Collections.Generic.List[int[]]'
        $list.Add(@(1, [int]::MaxValue))
        $changed[$path] = $list
    }
}

# 4. Keep the warnings on changed lines.
function Test-IsNew {
    param($Warning)
    if ($Warning.File -and $Warning.Line -gt 0) {
        if (-not $changed.ContainsKey($Warning.File)) { return $false }
        if ($Scope -eq 'File') { return $true }
        foreach ($range in $changed[$Warning.File]) {
            if ($Warning.Line -ge $range[0] -and $Warning.Line -le $range[1]) { return $true }
        }
        return $false
    }
    if ($Warning.File -and $changed.ContainsKey($Warning.File)) { return $true }
    foreach ($projectFile in $Warning.Projects) {
        if ($changed.ContainsKey($projectFile)) { return $true }
    }
    return $false
}

$newWarnings = @($warnings.Values | Where-Object { Test-IsNew $_ } | ForEach-Object {
    $_ | Add-Member -NotePropertyName Critical -NotePropertyValue $critical.ContainsKey($_.Id) -PassThru
} | Sort-Object @{ Expression = { -not $_.Critical } }, File, Line, Column, Id)
$newCritical = @($newWarnings | Where-Object { $_.Critical })

# 5. Report.
$scopeText = if ($Scope -eq 'File') { 'in changed files' } else { 'on changed lines' }
$headline = "$($warnings.Count) unique warning(s) in the build ($totalLines reported); $($newWarnings.Count) $scopeText since $($mergeBase.Substring(0, 10)) ($Base): $($newCritical.Count) critical"

foreach ($warning in $newWarnings) {
    $location = if ($warning.File) { $warning.File } else { $warning.RawFile }
    if ($warning.Line -gt 0) { $location += ":$($warning.Line)" }
    if ($warning.Column -gt 0) { $location += ":$($warning.Column)" }
    $text = "$($warning.Id): $($warning.Message)"
    if ($warning.Critical) { $text += " (critical: $($critical[$warning.Id]))" }
    if ($inActions) {
        $properties = @()
        if ($warning.File) {
            $properties += 'file=' + (Format-CommandValue $warning.File -Property)
            if ($warning.Line -gt 0) { $properties += "line=$($warning.Line)" }
            if ($warning.Column -gt 0) { $properties += "col=$($warning.Column)" }
        }
        $properties += 'title=' + (Format-CommandValue $warning.Id -Property)
        $command = if ($warning.Critical) { 'error' } else { 'warning' }
        Write-Host "::$command $($properties -join ',')::$(Format-CommandValue $text)"
    } else {
        $color = if ($warning.Critical) { 'Red' } else { 'Yellow' }
        Write-Host "  $location $text" -ForegroundColor $color
    }
}

$summary = New-Object System.Text.StringBuilder
[void]$summary.AppendLine('## New build warnings')
[void]$summary.AppendLine()
[void]$summary.AppendLine($headline + '.')
[void]$summary.AppendLine()
if ($newWarnings.Count -gt 0) {
    $maxRows = 300
    [void]$summary.AppendLine('| Severity | ID | Location | Message | Projects (target frameworks) |')
    [void]$summary.AppendLine('| --- | --- | --- | --- | --- |')
    foreach ($warning in ($newWarnings | Select-Object -First $maxRows)) {
        $severity = if ($warning.Critical) { '**critical**' } else { 'warning' }
        $location = if ($warning.File) { $warning.File } else { $warning.RawFile }
        if ($warning.Line -gt 0) { $location += ":$($warning.Line)" }
        if ($warning.Column -gt 0) { $location += ":$($warning.Column)" }
        $projects = (@($warning.Projects) | ForEach-Object { [System.IO.Path]::GetFileNameWithoutExtension($_) }) -join ', '
        if ($warning.Frameworks.Count -gt 0) { $projects += " ($(@($warning.Frameworks) -join ', '))" }
        $cells = @($severity, $warning.Id, "``$location``", $warning.Message, $projects) | ForEach-Object { $_.Replace('|', '\|') }
        [void]$summary.AppendLine('| ' + ($cells -join ' | ') + ' |')
    }
    if ($newWarnings.Count -gt $maxRows) {
        [void]$summary.AppendLine()
        [void]$summary.AppendLine("$($newWarnings.Count - $maxRows) more warning(s) are in the build logs.")
    }
    [void]$summary.AppendLine()
    [void]$summary.AppendLine('Critical IDs are listed in `.github/workflows/build-warnings/critical-warnings.json`.')
}
$utf8 = New-Object System.Text.UTF8Encoding $false
[System.IO.File]::WriteAllText((Join-Path $LogDirectory 'new-warnings.md'), $summary.ToString(), $utf8)
if ($env:GITHUB_STEP_SUMMARY) { [System.IO.File]::AppendAllText($env:GITHUB_STEP_SUMMARY, $summary.ToString(), $utf8) }

if ($newCritical.Count -gt 0) {
    Write-Host "FAIL: $headline" -ForegroundColor Red
    exit 1
}
Write-Host "OK: $headline"
exit 0
