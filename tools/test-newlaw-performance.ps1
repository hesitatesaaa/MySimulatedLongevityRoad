param(
    [ValidateSet('baseline','optimized')][string]$Mode='optimized',
    [int]$TimeoutSeconds=240
)
$ErrorActionPreference='Stop'
$project=Split-Path -Parent $PSScriptRoot
$work=Join-Path $project 'build/performance-implementation'
& python (Join-Path $work 'prepare-runtime-probe.py')
if($LASTEXITCODE -ne 0){throw 'Probe preparation failed'}
if($Mode -eq 'baseline'){
    $snapshot=Join-Path $work 'baseline-source'
    if(-not(Test-Path -LiteralPath $snapshot)){
        Add-Type -AssemblyName System.IO.Compression.FileSystem
        [IO.Compression.ZipFile]::ExtractToDirectory((Join-Path $work 'baseline-code.zip'),$snapshot)
    }
    $install=Join-Path $project 'build/repack-load-check/game/mods/MySimulatedLongevityRoad'
    $target=[IO.Path]::GetFullPath((Join-Path $install 'code'))
    $allowed=[IO.Path]::GetFullPath((Join-Path $project 'build')).TrimEnd('\')+'\'
    if(-not $target.StartsWith($allowed,[StringComparison]::OrdinalIgnoreCase)){throw "Unsafe staging path: $target"}
    if(Test-Path -LiteralPath $target){Remove-Item -LiteralPath $target -Recurse -Force}
    Copy-Item -LiteralPath (Join-Path $snapshot 'code') -Destination $target -Recurse
    Copy-Item -LiteralPath (Join-Path $snapshot 'MclslMod.cs') -Destination (Join-Path $install 'MclslMod.cs') -Force
}
$options=@{ProbeSource=(Join-Path $work 'MclslAuditAcceptance.cs');UiOnly=$true;TimeoutSeconds=$TimeoutSeconds}
if($Mode -eq 'baseline'){$options.UseInstalledSources=$true}
& (Join-Path $PSScriptRoot 'test-contract-runtime.ps1') @options
exit $LASTEXITCODE
