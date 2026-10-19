param([ValidateSet('baseline','native','size','runtime','recipe','death','spell','reload')][string]$Mode = 'native', [string]$RunName = '')
$ErrorActionPreference = 'Stop'
$releaseWorkspace = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../..'))
$releaseRun = Join-Path $releaseWorkspace 'output/runtime-validation/release-20261019'
$releaseGame = Join-Path $releaseRun 'game'
if (Get-Process worldbox -ErrorAction SilentlyContinue) { throw 'Existing WorldBox process; refuse overlapping performance measurements.' }
if (-not $RunName) { $RunName = $Mode }
if ($RunName -notmatch '^[a-z0-9-]+$') { throw 'Run name must be a plain lowercase file name.' }
$releaseReport = Join-Path $releaseRun ($RunName + '-observation.json')
$releaseLog = Join-Path $releaseRun ($RunName + '-observation.log')
$releaseGpu = Join-Path $releaseRun ($RunName + '-gpu-memory.csv')
if ((Test-Path -LiteralPath $releaseReport) -or (Test-Path -LiteralPath $releaseGpu)) { throw 'Refuse to overwrite existing runtime evidence.' }
$env:MCLSL_RELEASE_POPULATION_PROBE = if ($Mode -in @('baseline','native')) { $releaseReport } else { $null }
$releaseControlled = $Mode -in @('runtime','recipe','death','spell','reload')
$releaseProbeEnvironment = if ($releaseControlled) { 'MCLSL_BEAST_' + $Mode.ToUpperInvariant() + '_PROBE' } else { $null }
if ($releaseControlled) { [Environment]::SetEnvironmentVariable($releaseProbeEnvironment, $releaseReport) }
$env:MCLSL_ACTOR_SIZE_PROBE = if ($Mode -eq 'size') { $releaseReport } else { $null }
$env:MCLSL_RELEASE_BASELINE = if ($Mode -eq 'baseline') { '1' } else { '0' }
$releaseProcess = $null
try {
    $releaseArguments = @('-force-d3d11','-screen-fullscreen','0','-screen-width','1280','-screen-height','720','-logFile',$releaseLog)
    if ($Mode -eq 'size' -or $releaseControlled) { $releaseArguments = @('-batchmode') + $releaseArguments }
    $releaseWindowStyle = if ($Mode -in @('baseline','native')) { 'Normal' } else { 'Hidden' }
    $releaseProcess = Start-Process -FilePath (Join-Path $releaseGame 'worldbox.exe') -WorkingDirectory $releaseGame -WindowStyle $releaseWindowStyle -ArgumentList $releaseArguments -PassThru
    Write-Output ('Started ' + $Mode + ' gameplay observation PID ' + $releaseProcess.Id)
    $releaseDeadline = [DateTime]::UtcNow.AddMinutes(35)
    while (-not $releaseProcess.HasExited -and [DateTime]::UtcNow -lt $releaseDeadline) {
        $releaseData = $null
        if (Test-Path -LiteralPath $releaseReport) {
            try { $releaseData = Get-Content -LiteralPath $releaseReport -Raw -Encoding UTF8 | ConvertFrom-Json } catch { }
        }
        if ($Mode -eq 'size' -and $releaseData) {
            if ($releaseData.capturedUnits -ne $releaseData.expectedUnits) { throw ('Incomplete Unity size observation: ' + $releaseData.capturedUnits + '/' + $releaseData.expectedUnits) }
            Write-Output ('Completed placed-unit geometry observation: ' + $releaseData.capturedUnits + ' units')
            return
        }
        if (-not $releaseControlled) {
            $releaseSamples = @()
            try {
                $releaseSamples = @(Get-Counter '\GPU Process Memory(*)\Dedicated Usage' -ErrorAction Stop | Select-Object -ExpandProperty CounterSamples | Where-Object { $_.Status -eq 0 -and $_.InstanceName -like ('pid_' + $releaseProcess.Id + '_*') })
            } catch { Write-Warning ('GPU counter sample unavailable: ' + $_.Exception.Message) }
            $releaseGpuBytes = if ($releaseSamples.Count -gt 0) { ($releaseSamples | Measure-Object CookedValue -Sum).Sum } else { $null }
            [pscustomobject]@{ utc=[DateTime]::UtcNow.ToString('o'); pid=$releaseProcess.Id; mode=$Mode; phase=$releaseData.phase; units=$releaseData.currentUnits; status=$releaseData.status; dedicatedGpuBytes=$releaseGpuBytes; gpuCounterAvailable=($null -ne $releaseGpuBytes); processWorkingSetBytes=$releaseProcess.WorkingSet64 } | Export-Csv -LiteralPath $releaseGpu -NoTypeInformation -Append -Encoding UTF8
        }
        Start-Sleep -Seconds 4
        $releaseProcess.Refresh()
    }
    if (-not $releaseProcess.HasExited) { throw 'Runtime observation timed out; inspect retained evidence.' }
    if (-not (Test-Path -LiteralPath $releaseReport)) { throw 'Game exited without a runtime report.' }
    $releaseData = Get-Content -LiteralPath $releaseReport -Raw -Encoding UTF8 | ConvertFrom-Json
    if ($releaseData.status -ne 'completed' -and -not ($releaseControlled -and $releaseData.status -match '-completed$')) { throw ('Runtime observation failed: ' + $releaseData.status + ' ' + $releaseData.error) }
    Write-Output ('Completed ' + $Mode + ' gameplay observation: ' + $releaseReport)
} finally {
    if ($releaseProcess -and -not $releaseProcess.HasExited) {
        $releaseProcess.Kill()
        if (-not $releaseProcess.WaitForExit(10000)) { throw 'Isolated process did not exit; do not start another measurement.' }
    }
    $env:MCLSL_RELEASE_POPULATION_PROBE = $null
    $env:MCLSL_ACTOR_SIZE_PROBE = $null
    $env:MCLSL_RELEASE_BASELINE = $null
    if ($releaseControlled) { [Environment]::SetEnvironmentVariable($releaseProbeEnvironment, $null) }
}
