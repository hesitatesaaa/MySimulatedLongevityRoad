param(
    [string]$GameRoot = 'E:\wol\gameversion\0_51_2_0.51.2',
    [string]$OutputDirectory = '',
    [switch]$AllowUnreviewedAssets,
    [string]$PythonExecutable = 'python'
)
$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path -Parent $PSScriptRoot
if (-not $OutputDirectory) {
    $OutputDirectory = Join-Path $projectRoot ('build\Package-0.6.0-' + (Get-Date -Format 'yyyyMMdd-HHmmss'))
}
$outputFullPath = [IO.Path]::GetFullPath($OutputDirectory)
$workspacePrefix = [IO.Path]::GetFullPath($projectRoot).TrimEnd('\') + '\'
if (-not $outputFullPath.StartsWith($workspacePrefix, [StringComparison]::OrdinalIgnoreCase)) {
    throw 'Package output must remain inside this workspace.'
}
if (Test-Path -LiteralPath $outputFullPath) { throw 'Choose a fresh package output directory.' }
$compiler = "$outputFullPath-compiler"
& (Join-Path $PSScriptRoot 'build-release.ps1') -GameRoot $GameRoot -OutputDirectory $compiler `
    -RequireFinalAssets -AllowUnreviewedAssets:$AllowUnreviewedAssets -PythonExecutable $PythonExecutable
if ($LASTEXITCODE -ne 0) { throw 'Release build failed.' }
& $PythonExecutable (Join-Path $PSScriptRoot 'package_release.py') --output $outputFullPath --compiler $compiler
if ($LASTEXITCODE -ne 0) { throw 'Package generation or extracted verification failed.' }
Write-Output "Verified developer/player packages: $outputFullPath"
