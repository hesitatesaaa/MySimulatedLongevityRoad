param(
    [Parameter(Mandatory=$true)][string]$PackageZip,
    [string]$RuntimeRoot='D:\mclsl-audit-060',
    [string]$PythonExecutable='python',
    [int]$TimeoutSeconds=900
)
$ErrorActionPreference='Stop'
$project=Split-Path -Parent $PSScriptRoot
$runtime=[IO.Path]::GetFullPath($RuntimeRoot).TrimEnd('\')
$game=Join-Path $runtime 'game'
if ((Get-Content -LiteralPath (Join-Path $game 'worldbox_Data/app.info') -Raw) -notmatch 'MCLSLAud') {
    throw 'An isolated Unity product identity is required.'
}
$tag=[DateTime]::UtcNow.ToString('yyyyMMdd_HHmmss')
$work=Join-Path $project "build/runtime-070/$tag"
New-Item -ItemType Directory -Path $work -Force | Out-Null
$install=[IO.Path]::GetFullPath((Join-Path $game 'mods/MySimulatedLongevityRoad'))
if (-not $install.StartsWith($runtime+'\',[StringComparison]::OrdinalIgnoreCase)) { throw 'Unsafe installation path.' }
$disabled=Join-Path $runtime "acceptance-backups/$tag"
foreach($existing in @($install,(Join-Path $game 'mods/LivingNativeExport'))) {
    $resolved=[IO.Path]::GetFullPath($existing)
    if (-not $resolved.StartsWith($runtime+'\',[StringComparison]::OrdinalIgnoreCase)) { throw 'Unsafe backup path.' }
    if(Test-Path -LiteralPath $resolved) {
        New-Item -ItemType Directory -Path $disabled -Force | Out-Null
        Move-Item -LiteralPath $resolved -Destination $disabled
    }
}
Expand-Archive -LiteralPath $PackageZip -DestinationPath (Join-Path $work 'extracted')
Move-Item -LiteralPath (Join-Path $work 'extracted/MySimulatedLongevityRoad') -Destination $install
Copy-Item -LiteralPath (Join-Path $project '.dev-tools/runtime-acceptance/LivingProbe.cs') -Destination (Join-Path $install 'code/MySimulatedLongevityRoad/Core/MclslAuditAcceptance.cs')
$entry=Join-Path $install 'InterestingTrait.cs'
$source=[IO.File]::ReadAllText($entry)
$anchor='MclslLocalizationRuntimeMarker.MarkLoaded();'
if (($source.Split(@($anchor),[StringSplitOptions]::None)).Length -ne 2) { throw 'Unexpected mod entry.' }
[IO.File]::WriteAllText($entry,$source.Replace($anchor,$anchor+' MclslAuditAcceptance.Ensure();'),[Text.UTF8Encoding]::new($false))
$hash=(& $PythonExecutable (Join-Path $PSScriptRoot 'validate-living-assets.py') --fingerprint).Trim()
if($LASTEXITCODE -ne 0 -or $hash -notmatch '^[a-f0-9]{64}$') { throw 'Cannot fingerprint acceptance inputs.' }
$packageManifest=Get-Content -LiteralPath (Join-Path $install 'FILE-MANIFEST.json') -Raw -Encoding UTF8 | ConvertFrom-Json
if($packageManifest.sourceInputSha256 -ne $hash) { throw 'Package inputs differ from the current workspace; regenerate the package.' }
$report=Join-Path $work 'result.json'
$log=Join-Path $work 'unity.log'
$packageHash=(Get-FileHash -LiteralPath $PackageZip -Algorithm SHA256).Hash.ToLowerInvariant()
[IO.File]::WriteAllText((Join-Path $work 'input.json'),(@{inputSha256=$hash;packageSha256=$packageHash;package=$PackageZip}|ConvertTo-Json),[Text.UTF8Encoding]::new($false))
$clock=[Diagnostics.Stopwatch]::StartNew()
$process=Start-Process -FilePath (Join-Path $game 'worldbox.exe') -ArgumentList @('-batchmode','-screen-width','1280','-screen-height','720','-logFile',$log,'--mclsl-acceptance-output',$report,'--mclsl-input-sha256',$hash) -WorkingDirectory $game -WindowStyle Hidden -PassThru
Write-Output "Unity PID=$($process.Id), report=$report"
try {
    while(-not $process.HasExited -and $clock.Elapsed.TotalSeconds -lt $TimeoutSeconds -and -not (Test-Path -LiteralPath $report)) {
        if(Test-Path -LiteralPath $log) {
            $currentLog=Get-Content -LiteralPath $log -Raw -Encoding UTF8
            if($currentLog -match 'Failed to compile mod|Required Harmony patches failed|模组加载失败') { throw "Unity mod initialization failed; inspect $log" }
        }
        Start-Sleep -Milliseconds 500
        $process.Refresh()
    }
    if(-not (Test-Path -LiteralPath $report)) { throw "Unity report missing; inspect $log" }
    $result=Get-Content -LiteralPath $report -Raw -Encoding UTF8 | ConvertFrom-Json
    Write-Output "passed=$($result.passed), checks=$($result.checks.Count), errors=$($result.errors.Count)"
    Write-Output $report
    if(-not $result.passed) { $result.errors | Select-Object -First 2 | Write-Output; exit 1 }
}
finally {
    if(-not $process.HasExited) { $process.Kill();$process.WaitForExit() }
    $process.Dispose()
}
