#Requires -Version 5.1
<#
.SYNOPSIS
    Run every offline verification step for this mod in one command.

.DESCRIPTION
    Offline checks only: source selection, localization, resource manifests,
    compilation against the local game install, and the self-contained C#
    test programs. Nothing here proves in-game behaviour; Unity acceptance
    still needs a real run (see AGENTS.md).

.PARAMETER PythonExecutable
    Python interpreter that has Pillow available.

.PARAMETER GameRoot
    WorldBox installation used for the compile check. When omitted the
    default inside tools/build-release.ps1 is used.

.PARAMETER WorkDirectory
    Scratch directory for reports and isolated build outputs. Must stay
    inside this workspace. Defaults to build/verify.

.PARAMETER TestProjects
    Projects under .dev-tools to run. They must be self-contained console
    programs that exit non-zero on failure.

.PARAMETER SkipBuild
    Skip the compile/package-input check. Use this on machines without a
    game install (for example CI).

.PARAMETER SkipTests
    Skip the .dev-tools test programs.
#>
param(
    [string]$PythonExecutable = 'python',
    [string]$GameRoot = '',
    [string]$WorkDirectory = 'build/verify',
    [string[]]$TestProjects = @('actor-loading-tests', 'animation-tests', 'death-visual-tests'),
    [switch]$SkipBuild,
    [switch]$SkipTests
)

$ErrorActionPreference = 'Stop'
$TestProjects = @($TestProjects | ForEach-Object { $_ -split ',' } |
    ForEach-Object { $_.Trim() } | Where-Object { $_ })
$projectRoot = Split-Path -Parent $PSScriptRoot
$rootFull = [IO.Path]::GetFullPath($projectRoot).TrimEnd('\') + '\'
$work = if ([IO.Path]::IsPathRooted($WorkDirectory)) {
    [IO.Path]::GetFullPath($WorkDirectory)
} else {
    [IO.Path]::GetFullPath((Join-Path $projectRoot $WorkDirectory))
}
if (-not $work.StartsWith($rootFull, [StringComparison]::OrdinalIgnoreCase)) {
    throw "Work directory must stay inside this workspace: $work"
}
New-Item -ItemType Directory -Path $work -Force | Out-Null

$script:stepCount = 0
function Invoke-Step {
    param([string]$Name, [scriptblock]$Body)
    $script:stepCount++
    Write-Host ""
    Write-Host ("[{0}] {1}" -f $script:stepCount, $Name) -ForegroundColor Cyan
    $started = Get-Date
    & $Body
    if ($LASTEXITCODE -ne 0) { throw "$Name failed (exit $LASTEXITCODE)" }
    Write-Host ("      ok in {0:N1}s" -f ((Get-Date) - $started).TotalSeconds) -ForegroundColor DarkGray
}

function Invoke-Python {
    param([string[]]$Arguments)
    & $PythonExecutable @Arguments
}

$sw = [Diagnostics.Stopwatch]::StartNew()
Push-Location $projectRoot
try {
    if ($SkipBuild) {
        Invoke-Step 'NML source selection' {
            Invoke-Python @('tools/check-nml-sources.py', '--root', $projectRoot, '--json') |
                Out-File (Join-Path $work 'nml-sources.json') -Encoding utf8
        }
        Invoke-Step 'Localization catalogs and callbacks' {
            Invoke-Python @('tools/check-localization.py')
        }
        Invoke-Step 'Actor frames and icons' {
            Invoke-Python @('tools/validate-current-assets.py')
        }
        Invoke-Step 'Beast content and spell stages' {
            $report = $work.Substring($rootFull.Length).Replace('\', '/') + '/validation/beast-resources.json'
            Invoke-Python @('tools/validate-beast-overhaul.py', '--output', $report)
        }
        $compileOutput = $null
    } else {
        $compileOutput = Join-Path $work 'compile'
        Invoke-Step 'Compile and validate (build-release, local development)' {
            $arguments = @{
                LocalDevelopment = $true
                OutputDirectory  = $compileOutput
                PythonExecutable = $PythonExecutable
            }
            if ($GameRoot) { $arguments.GameRoot = $GameRoot }
            & (Join-Path $PSScriptRoot 'build-release.ps1') @arguments
        }
    }

    if (-not $SkipTests) {
        $env:MCLSL_ACTOR_FIXTURE = Join-Path $work 'fixtures/actor-loading'
        $env:MCLSL_ANIMATION_FIXTURE = Join-Path $work 'fixtures/animation'
        foreach ($project in $TestProjects) {
            $projectDirectory = Join-Path $projectRoot ".dev-tools/$project"
            if (-not (Test-Path -LiteralPath $projectDirectory)) { throw "Missing test project: $project" }
            Invoke-Step "Tests: $project" {
                # Keep generated intermediate files inside the scratch directory so
                # a verification run never dirties the source tree.
                & dotnet run --project $projectDirectory `
                    "-p:BaseIntermediateOutputPath=$work/obj/$project/" `
                    "-p:BaseOutputPath=$work/bin/$project/"
            }
        }
    }

    Write-Host ""
    Write-Host ("Verification finished in {0:N0}s. Reports: {1}" -f $sw.Elapsed.TotalSeconds, $work) -ForegroundColor Green
}
catch {
    Write-Host ""
    Write-Host "VERIFICATION FAILED: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}
finally {
    Pop-Location
}
