param([Parameter(Mandatory=$true)][string]$RuntimeRoot,[ValidateSet('baseline','optimized')][string]$Mode='optimized',[int]$TimeoutSeconds=180)
$ErrorActionPreference='Stop'
$runtime=[IO.Path]::GetFullPath($RuntimeRoot).TrimEnd('\')
$project=Split-Path -Parent $PSScriptRoot
$game=Join-Path $runtime 'game'
$install=Join-Path $game 'mods/MySimulatedLongevityRoad'
if ((Get-Content (Join-Path $game 'worldbox_Data/app.info') -Raw) -notmatch 'MCLSLAud') { throw 'An isolated test identity is required.' }
$resolved=[IO.Path]::GetFullPath($install)
if (-not $resolved.StartsWith($runtime+'\',[StringComparison]::OrdinalIgnoreCase)) { throw 'Unsafe installation path' }
if (Test-Path -LiteralPath $install) { Remove-Item -LiteralPath $install -Recurse -Force }
Copy-Item -LiteralPath (Join-Path $runtime "$Mode-player/MySimulatedLongevityRoad") -Destination (Split-Path $install -Parent) -Recurse
Copy-Item -LiteralPath (Join-Path $project '.dev-tools/runtime-acceptance/Probe.cs') -Destination (Join-Path $install 'code/MySimulatedLongevityRoad/Core/MclslAuditAcceptance.cs')
$entry=Join-Path $install 'InterestingTrait.cs'
$source=[IO.File]::ReadAllText($entry)
$anchor='MclslLocalizationRuntimeMarker.MarkLoaded();'
if (($source.Split(@($anchor),[StringSplitOptions]::None)).Length -ne 2) { throw 'Unexpected entry source' }
[IO.File]::WriteAllText($entry,$source.Replace($anchor,$anchor+' MclslAuditAcceptance.Ensure();'),[Text.UTF8Encoding]::new($false))
$tag=[DateTime]::UtcNow.ToString('yyyyMMdd_HHmmss')
$report=Join-Path $runtime "acceptance/$tag-$Mode-result.json"
$log=Join-Path $runtime "logs/$tag-$Mode-acceptance.log"
New-Item -ItemType Directory -Force -Path (Split-Path $report -Parent) | Out-Null
$clock=[Diagnostics.Stopwatch]::StartNew()
$process=Start-Process -FilePath (Join-Path $game 'worldbox.exe') -ArgumentList @('-batchmode','-screen-width','960','-screen-height','540','-logFile',$log,'--mclsl-acceptance-output',$report) -WorkingDirectory $game -WindowStyle Hidden -PassThru
$process.Id | Set-Content -LiteralPath (Join-Path $runtime 'acceptance.pid')
try {
    while (-not $process.HasExited -and $clock.Elapsed.TotalSeconds -lt $TimeoutSeconds -and -not (Test-Path -LiteralPath $report)) {
        if (Test-Path -LiteralPath $log) {
            $text=Get-Content -LiteralPath $log -Raw -Encoding UTF8
            if ($text -match 'Failed to compile mod|Required Harmony patches failed') { throw "Mod initialization failed; inspect $log" }
        }
        Start-Sleep -Milliseconds 500
    }
    if (-not (Test-Path -LiteralPath $report)) { throw "No Unity acceptance report; inspect $log" }
    $result=Get-Content -LiteralPath $report -Raw -Encoding UTF8 | ConvertFrom-Json
    Write-Output "Unity acceptance passed=$($result.passed), checks=$($result.checks.Count), errors=$($result.errors.Count)"
    Write-Output $report
    if (-not $result.passed) { $result.errors | Select-Object -First 3 | Write-Output; exit 1 }
}
finally { if (-not $process.HasExited) { $process.Kill();$process.WaitForExit() };$process.Dispose() }
