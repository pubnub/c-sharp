<#
.SYNOPSIS
    Lists `await` statements in the SDK library that do not use .ConfigureAwait(false).

.DESCRIPTION
    Every await inside src/Api must be followed by .ConfigureAwait(false): the SDK runs inside
    applications with a synchronization context (Unity, UWP, WinForms, ASP.NET classic), and
    resuming on that context can deadlock callers that block on SDK tasks.

    By default only library files changed against origin/master (plus untracked files) are scanned.
    Pass -All to scan the whole library. Exits with 1 when a violation is found.

.EXAMPLE
    pwsh .claude/skills/run-checks/scripts/Test-ConfigureAwait.ps1
    powershell -ExecutionPolicy Bypass -File .claude/skills/run-checks/scripts/Test-ConfigureAwait.ps1 -All
#>
[CmdletBinding()]
param(
    [switch]$All,
    [string]$Base = 'origin/master'
)

$ErrorActionPreference = 'Stop'
$repoRoot = (git rev-parse --show-toplevel).Trim()
Set-Location $repoRoot

if ($All) {
    $files = git ls-files --cached --others --exclude-standard -- 'src/Api/PubnubApi/*.cs'
} else {
    $mergeBase = (git merge-base $Base HEAD 2>$null)
    if (-not $mergeBase) { $mergeBase = 'HEAD' }
    $files = @(git diff --name-only --diff-filter=d $mergeBase -- 'src/Api/*.cs') +
             @(git ls-files --others --exclude-standard -- 'src/Api/*.cs')
}
$files = @($files | Where-Object { $_ -and $_ -notmatch '/(bin|obj)/' -and (Test-Path -LiteralPath $_) } | Sort-Object -Unique)

$violations = @()
foreach ($file in $files) {
    $lines = @(Get-Content -LiteralPath $file)
    for ($i = 0; $i -lt $lines.Count; $i++) {
        if ($lines[$i] -notmatch '\bawait\s' -or $lines[$i] -match '^\s*(//|\*|/\*)') { continue }
        # An await statement may span several lines; read up to its terminating semicolon.
        $statement = ''
        for ($j = $i; $j -lt [Math]::Min($i + 12, $lines.Count); $j++) {
            $statement += $lines[$j]
            if ($lines[$j] -match ';\s*(//.*)?$') { break }
        }
        if ($statement -notmatch 'ConfigureAwait\(false\)') {
            $violations += "${file}:$($i + 1): $($lines[$i].Trim())"
        }
    }
}

$scope = if ($All) { 'the whole library' } else { "library files changed against $Base" }
if ($violations.Count -eq 0) {
    Write-Host "OK: every await uses .ConfigureAwait(false) ($($files.Count) file(s) scanned: $scope)"
    exit 0
}

Write-Host "FAIL: $($violations.Count) await(s) without .ConfigureAwait(false) ($($files.Count) file(s) scanned: $scope):" -ForegroundColor Red
$violations | ForEach-Object { Write-Host "  $_" -ForegroundColor Red }
exit 1
