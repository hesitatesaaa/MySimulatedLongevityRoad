param(
    [string]$GameRoot = 'D:\owl\gameversion\0_51_2_imported',
    [string]$OutputDirectory = '',
    [switch]$LocalDevelopment,
    [string]$PythonExecutable = 'python'
)
$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path -Parent $PSScriptRoot
$declaration=Get-Content -LiteralPath (Join-Path $projectRoot 'mod.json') -Raw -Encoding UTF8 | ConvertFrom-Json
if (-not $OutputDirectory) { $OutputDirectory = Join-Path $projectRoot ('build/Package-'+$declaration.version) }
$outputFullPath = [IO.Path]::GetFullPath($OutputDirectory)
$workspacePrefix = [IO.Path]::GetFullPath($projectRoot).TrimEnd('\') + '\'
if (-not $outputFullPath.StartsWith($workspacePrefix, [StringComparison]::OrdinalIgnoreCase)) {
    throw 'Package output must remain inside this workspace.'
}
if (Test-Path -LiteralPath $outputFullPath) { throw 'Package output already exists; choose a fresh directory.' }
$compilerOutput = $outputFullPath + '-compiler'
if ($LocalDevelopment) {
    & (Join-Path $PSScriptRoot 'build-release.ps1') -GameRoot $GameRoot -OutputDirectory $compilerOutput -LocalDevelopment -PythonExecutable $PythonExecutable
    & $PythonExecutable (Join-Path $PSScriptRoot 'package-release.py') --output $outputFullPath --kind player --compress-assets --local-development
} else {
    & (Join-Path $PSScriptRoot 'build-release.ps1') -GameRoot $GameRoot -OutputDirectory $compilerOutput -RequireFinalAssets -PythonExecutable $PythonExecutable
    & $PythonExecutable (Join-Path $PSScriptRoot 'package-release.py') --output $outputFullPath --kind player --compress-assets
}
if ($LASTEXITCODE -ne 0) { throw 'Release package checks failed; no verified release is available.' }
