<#
.SYNOPSIS
    Verifies the hand-maintained <Compile Include> lists of the PubNub C# SDK projects.

.DESCRIPTION
    PubnubApiPCL, PubnubApiUWP and PubnubApiUnity compile the sources of src/Api/PubnubApi through
    explicit links (EnableDefaultCompileItems=False), so a new file must be linked into all three.
    PubnubApiPCL.Tests, the only unit test project CI runs, links the files of PubnubApi.Tests.

    Exits with 1 when a library project is missing a link or links a file that no longer exists.
    Unlinked test files are reported as warnings (exit 0) unless -Strict is passed.

.EXAMPLE
    pwsh .claude/skills/run-checks/scripts/Test-LinkedSources.ps1
    powershell -ExecutionPolicy Bypass -File .claude/skills/run-checks/scripts/Test-LinkedSources.ps1 -Strict
#>
[CmdletBinding()]
param(
    [switch]$Strict
)

$ErrorActionPreference = 'Stop'
$repoRoot = (git rev-parse --show-toplevel).Trim()
Set-Location $repoRoot

function Get-SourceFiles([string]$Directory) {
    git ls-files --cached --others --exclude-standard -- "$Directory/*.cs" |
        Where-Object { $_ -notmatch '/(bin|obj)/' -and (Test-Path -LiteralPath $_) } |
        ForEach-Object { $_.Substring($Directory.Length + 1).Replace('/', '\') } |
        Sort-Object -Unique
}

function Get-LinkedFiles([string]$Project, [string]$Prefix) {
    [xml]$xml = Get-Content -Raw -LiteralPath $Project
    $xml.Project.ItemGroup |
        ForEach-Object { $_.Compile } |
        Where-Object { $_ -and $_.Include -and $_.Include.StartsWith($Prefix) } |
        ForEach-Object { $_.Include.Substring($Prefix.Length) } |
        Sort-Object -Unique
}

function Compare-Links([string]$Project, [string[]]$Sources, [string]$Prefix) {
    $linked = @(Get-LinkedFiles $Project $Prefix)
    [pscustomobject]@{
        Project = $Project
        Missing = @($Sources | Where-Object { $linked -notcontains $_ })
        Stale   = @($linked | Where-Object { $Sources -notcontains $_ })
        Count   = $linked.Count
    }
}

$failed = $false

$librarySources = @(Get-SourceFiles 'src/Api/PubnubApi')
$libraryProjects = @(
    'src/Api/PubnubApiPCL/PubnubApiPCL.csproj',
    'src/Api/PubnubApiUWP/PubnubApiUWP.csproj',
    'src/Api/PubnubApiUnity/PubnubApiUnity.csproj'
)

Write-Host "src/Api/PubnubApi: $($librarySources.Count) source files"
foreach ($project in $libraryProjects) {
    $result = Compare-Links $project $librarySources '..\PubnubApi\'
    if ($result.Missing.Count -eq 0 -and $result.Stale.Count -eq 0) {
        Write-Host "  OK    $project ($($result.Count) links)"
        continue
    }
    $failed = $true
    Write-Host "  FAIL  $project" -ForegroundColor Red
    $result.Missing | ForEach-Object { Write-Host "        not linked: $_" -ForegroundColor Red }
    $result.Stale | ForEach-Object { Write-Host "        links a missing file: $_" -ForegroundColor Red }
}

# PubnubApiPCL.Tests has its own copies of these.
$testFilesNotShared = @('Properties\AssemblyInfo.cs', 'Resource.Designer.cs')
$testSources = @(Get-SourceFiles 'src/UnitTests/PubnubApi.Tests' | Where-Object { $testFilesNotShared -notcontains $_ })
$testResult = Compare-Links 'src/UnitTests/PubnubApiPCL.Tests/PubnubApiPCL.Tests.csproj' $testSources '..\PubnubApi.Tests\'
Write-Host "src/UnitTests/PubnubApi.Tests: $($testSources.Count) source files"
if ($testResult.Stale.Count -gt 0) {
    $failed = $true
    $testResult.Stale | ForEach-Object { Write-Host "  FAIL  PubnubApiPCL.Tests links a missing file: $_" -ForegroundColor Red }
}
if ($testResult.Missing.Count -eq 0) {
    Write-Host "  OK    PubnubApiPCL.Tests links every test file"
} else {
    if ($Strict) { $failed = $true }
    Write-Host "  WARN  not linked into PubnubApiPCL.Tests, so CI never runs them:" -ForegroundColor Yellow
    $testResult.Missing | ForEach-Object { Write-Host "        $_" -ForegroundColor Yellow }
}

if ($failed) { exit 1 }
exit 0
