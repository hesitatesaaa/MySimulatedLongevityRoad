param(
    [string]$GameRoot = 'E:\wol\gameversion\0_51_2_0.51.2',
    [string]$RunDirectory = ''
)
$ErrorActionPreference = 'Stop'
$workspace = [IO.Path]::GetFullPath((Split-Path -Parent $PSScriptRoot))
$runRoot = if ($RunDirectory) { [IO.Path]::GetFullPath($RunDirectory) } else { Join-Path $workspace 'output\runtime-validation\visual-baihu-20261014' }
$stage = Join-Path $runRoot 'MySimulatedLongevityRoad'
$mods = [IO.Path]::GetFullPath((Join-Path $GameRoot 'mods'))
$backup = Join-Path $runRoot 'original-game-mod'
$used = Join-Path $runRoot 'used-game-test-mod'
$reference = Join-Path $mods 'Thefantasyworld'
$referenceSource = Join-Path $workspace 'output\runtime-validation\fantasy-reference-used'
$referenceUsed = Join-Path $runRoot 'used-fantasy-reference'
$process = $null
$moved = $false
$copiedReference = $false
function Assert-Within([string]$Path, [string]$Parent) {
    $full = [IO.Path]::GetFullPath($Path)
    $prefix = [IO.Path]::GetFullPath($Parent).TrimEnd('\') + '\'
    if (-not $full.StartsWith($prefix, [StringComparison]::OrdinalIgnoreCase)) { throw "Unsafe test path: $full" }
}
function Run-Probe([string]$Kind, [string]$Variable, [int]$TimeoutSeconds) {
    $report = Join-Path $runRoot ($Kind + '.json')
    $log = Join-Path $runRoot ($Kind + '.log')
    if (Test-Path -LiteralPath $report) { throw "Refuse to reuse old runtime report: $report" }
    $env:MCLSL_NATIVE_FRAME_PROBE = $null
    $env:MCLSL_ACTOR_SIZE_PROBE = $null
    $env:MCLSL_ACTOR_POPULATION_PROBE = $null
    [Environment]::SetEnvironmentVariable($Variable, $report, 'Process')
    $script:process = Start-Process -FilePath (Join-Path $GameRoot 'worldbox.exe') -WorkingDirectory $GameRoot -WindowStyle Hidden -ArgumentList @('-batchmode','-force-d3d11','-screen-width','1280','-screen-height','720','-logFile', ('"' + $log + '"')) -PassThru
    Write-Output ("Started $Kind probe, PID=" + $script:process.Id)
    $deadline = [DateTime]::UtcNow.AddSeconds($TimeoutSeconds)
    $complete = $false
    while ([DateTime]::UtcNow -lt $deadline -and -not $script:process.HasExited) {
        if (Test-Path -LiteralPath $report) {
            try {
                $data = Get-Content -LiteralPath $report -Raw -Encoding UTF8 | ConvertFrom-Json
                if ($Variable -eq 'MCLSL_NATIVE_FRAME_PROBE') {
                    $complete = $data.phase -eq 'released' -and $data.totalFrames -gt 0 -and $data.loadedFrames -eq $data.totalFrames -and $data.pendingFrames -eq 0
                }
                else { $complete = $data.capturedUnits -eq $data.expectedUnits }
                if ($complete) { break }
            } catch { }
        }
        Start-Sleep -Seconds 2
        $script:process.Refresh()
    }
    if (-not $script:process.HasExited) { $script:process.Kill(); $script:process.WaitForExit(10000) | Out-Null }
    $script:process = $null
    if (-not $complete) { throw "Runtime probe did not finish: $Kind. Inspect $log" }
    Write-Output ("Completed $Kind probe: $report")
}
Assert-Within $runRoot $workspace
if (Get-Process worldbox -ErrorAction SilentlyContinue) { throw 'An existing WorldBox session is running; do not modify its game files or map.' }
if (-not (Test-Path -LiteralPath $stage)) { throw 'Isolated visual test stage missing.' }
foreach ($path in @($backup,$used,$referenceUsed)) { Assert-Within $path $workspace; if (Test-Path -LiteralPath $path) { throw "Test destination already exists: $path" } }
if (Test-Path -LiteralPath $reference) { throw 'Existing reference mod must remain untouched; choose another test run.' }
$gameMod = $null
foreach ($directory in Get-ChildItem -LiteralPath $mods -Directory) {
    $declaration = Join-Path $directory.FullName 'mod.json'
    if (-not (Test-Path -LiteralPath $declaration)) { continue }
    try { $info = Get-Content -LiteralPath $declaration -Raw -Encoding UTF8 | ConvertFrom-Json } catch { continue }
    if ($info.GUID -eq 'MY_SIMULATED_LONGEVITY_ROAD') {
        if ($gameMod) { throw 'Multiple matching game mods; refuse ambiguous replacement.' }
        $gameMod = $directory.FullName
    }
}
if (-not $gameMod) { throw 'Original game mod not found.' }
Assert-Within $gameMod $mods
Assert-Within $reference $mods
try {
    Move-Item -LiteralPath $gameMod -Destination $backup
    $moved = $true
    Copy-Item -LiteralPath $stage -Destination $gameMod -Recurse
    Run-Probe 'native-frames' 'MCLSL_NATIVE_FRAME_PROBE' 270
    Copy-Item -LiteralPath $referenceSource -Destination $reference -Recurse
    $copiedReference = $true
    Run-Probe 'unit-sizes' 'MCLSL_ACTOR_SIZE_PROBE' 240
} finally {
    if ($process -and -not $process.HasExited) { $process.Kill(); $process.WaitForExit(10000) | Out-Null }
    if ($copiedReference -and (Test-Path -LiteralPath $reference)) { Assert-Within $reference $mods; Move-Item -LiteralPath $reference -Destination $referenceUsed }
    if ($moved) {
        if (Test-Path -LiteralPath $gameMod) { Assert-Within $gameMod $mods; Move-Item -LiteralPath $gameMod -Destination $used }
        Move-Item -LiteralPath $backup -Destination $gameMod
    }
    $env:MCLSL_NATIVE_FRAME_PROBE = $null
    $env:MCLSL_ACTOR_SIZE_PROBE = $null
    $env:MCLSL_ACTOR_POPULATION_PROBE = $null
    Write-Output 'Original game mod restored; tests did not save a player world.'
}
