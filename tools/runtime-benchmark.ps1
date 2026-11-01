param(
    [Parameter(Mandatory=$true)][string]$RuntimeRoot,
    [string[]]$Modes = @('vanilla','nml','baseline','optimized'),
    [int]$Repetitions = 3,
    [int]$TimeoutSeconds = 120
)
$ErrorActionPreference='Stop'
$Modes=@($Modes | ForEach-Object { $_ -split ',' })
foreach($mode in $Modes) { if ($mode -notin @('vanilla','nml','baseline','optimized')) { throw "Unknown mode: $mode" } }
$runtime=[IO.Path]::GetFullPath($RuntimeRoot).TrimEnd('\')
$game=Join-Path $runtime 'game'
$modRoot=Join-Path $game 'mods'
$streamMods=Join-Path $game 'worldbox_Data/StreamingAssets/mods'
$data=Join-Path ([Environment]::GetFolderPath('UserProfile')) 'AppData/LocalLow/mkarpenko/MCLSLAud'
if ((Get-Content (Join-Path $game 'worldbox_Data/app.info') -Raw) -notmatch 'MCLSLAud') { throw 'Test game must have an isolated product identity.' }
if (-not (Test-Path -LiteralPath (Join-Path $data 'worldboxData'))) { throw 'Run the isolated identity once to seed test settings.' }
New-Item -ItemType Directory -Force -Path "$runtime/logs",$modRoot | Out-Null

function Remove-Generated([string]$path) {
    $full=[IO.Path]::GetFullPath($path)
    if (-not $full.StartsWith($runtime+'\',[StringComparison]::OrdinalIgnoreCase)) { throw "Refusing generated cleanup outside runtime: $full" }
    if (Test-Path -LiteralPath $full) { Remove-Item -LiteralPath $full -Recurse -Force }
}
function Convert-Settings([string]$inputText,[bool]$encrypt) {
    $derive=New-Object Security.Cryptography.PasswordDeriveBytes('WorldboxIsAwesome555',$null)
    $cipher=[Security.Cryptography.Aes]::Create()
    try {
        $cipher.Key=$derive.GetBytes(32)
        $cipher.IV=[Text.Encoding]::UTF8.GetBytes('sayHiIfUReadThis')
        if ($encrypt) {
            $bytes=[Text.Encoding]::UTF8.GetBytes($inputText)
            $transform=$cipher.CreateEncryptor()
            return [Convert]::ToBase64String($transform.TransformFinalBlock($bytes,0,$bytes.Length))
        }
        $bytes=[Convert]::FromBase64String($inputText)
        $transform=$cipher.CreateDecryptor()
        return [Text.Encoding]::UTF8.GetString($transform.TransformFinalBlock($bytes,0,$bytes.Length))
    }
    finally { $cipher.Dispose();$derive.Dispose() }
}
$seedFile=Join-Path $runtime 'test-settings.seed'
if (-not (Test-Path -LiteralPath $seedFile)) { Copy-Item -LiteralPath (Join-Path $data 'worldboxData') -Destination $seedFile }
$seed=Convert-Settings (Get-Content -LiteralPath $seedFile -Raw) $false | ConvertFrom-Json
foreach($setting in $seed.list) {
    if ($setting.name -eq 'experimental') { $setting | Add-Member -Force NoteProperty boolVal $true }
    if ($setting.name -eq 'last_used_version') { $setting | Add-Member -Force NoteProperty stringVal '0.51.2' }
}
$settings=Convert-Settings ($seed | ConvertTo-Json -Depth 50 -Compress) $true
$results=New-Object Collections.Generic.List[object]
$runTag=[DateTime]::UtcNow.ToString('yyyyMMdd_HHmmss')
if (Test-Path -LiteralPath "$runtime/benchmark-results.json") {
    function Add-Previous($items) {
        foreach($item in $items) {
            if ($item -is [array]) { Add-Previous $item }
            elseif ($null -ne $item.mode) { $results.Add($item) }
        }
    }
    Add-Previous (Get-Content -LiteralPath "$runtime/benchmark-results.json" -Raw -Encoding UTF8 | ConvertFrom-Json)
}
foreach($mode in $Modes) {
    Remove-Generated $modRoot
    New-Item -ItemType Directory -Force -Path $modRoot | Out-Null
    Remove-Generated $streamMods
    if ($mode -ne 'vanilla') { Copy-Item -LiteralPath "$runtime/nml-template" -Destination $streamMods -Recurse }
    else { New-Item -ItemType Directory -Force -Path $streamMods | Out-Null }
    if ($mode -in @('baseline','optimized')) { Copy-Item -LiteralPath "$runtime/$mode-player/MySimulatedLongevityRoad" -Destination $modRoot -Recurse }
    $conditions=if ($mode -in @('baseline','optimized')) { @('cold-compile','warm-compile-cache') } else { @('not-applicable') }
    foreach($condition in $conditions) {
        for($sample=1;$sample -le $Repetitions;$sample++) {
            if ($condition -eq 'cold-compile') {
                Remove-Generated (Join-Path $streamMods 'NML/CompiledMods')
                Remove-Generated (Join-Path $streamMods 'NML/mod_compile_records.json')
            }
            [IO.File]::WriteAllText((Join-Path $data 'worldboxData'),$settings)
            $log=Join-Path $runtime "logs/$runTag-$mode-$condition-$sample.log"
            $clock=[Diagnostics.Stopwatch]::StartNew()
            $process=Start-Process -FilePath (Join-Path $game 'worldbox.exe') -ArgumentList @('-batchmode','-screen-width','960','-screen-height','540','-logFile',$log) -WorkingDirectory $game -WindowStyle Hidden -PassThru
            $peak=0L;$loaded=$false;$content='';$exitCode=$null
            try {
                while ($clock.Elapsed.TotalSeconds -lt $TimeoutSeconds) {
                    if ($process.HasExited) { $exitCode=$process.ExitCode;break }
                    $process.Refresh();$peak=[Math]::Max($peak,$process.WorkingSet64)
                    if (Test-Path -LiteralPath $log) {
                        $content=Get-Content -LiteralPath $log -Raw -Encoding UTF8
                        if ($content -match 'Loading finished\s*=') { $loaded=$true;break }
                    }
                    Start-Sleep -Milliseconds 250
                }
                $stages=[ordered]@{}
                foreach($line in ($content -split "`n")) {
                    if ($line -match '\d+:\s+(.+?)\s*=\s*([0-9.]+)') { $stages[$matches[1].Trim()]=[double]::Parse($matches[2],[Globalization.CultureInfo]::InvariantCulture) }
                }
                # Keep script matching ASCII-only for Windows PowerShell 5.1,
                # which otherwise interprets UTF-8 scripts as the system codepage.
                $modLoaded=($content -match '\[NML\]: \[[^\r\n]+\]: Loaded') -and ($content -notmatch 'Failed to compile')
                $record=[ordered]@{mode=$mode;condition=$condition;sample=$sample;loadingFinished=$loaded;modLoaded=$modLoaded;wallSeconds=$clock.Elapsed.TotalSeconds;peakWorkingSetBytes=$peak;stages=$stages;log=$log;exitCode=$exitCode;interactiveUiVerified=$false;batchMode=$true;isolatedProduct='MCLSLAud'}
                $results.Add($record)
                $results | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath "$runtime/benchmark-results.json" -Encoding UTF8
                Write-Output ("$mode $condition $sample loaded=$loaded wall={0:N2}s peak={1:N0}MiB" -f $clock.Elapsed.TotalSeconds,($peak/1MB))
            }
            finally { if (-not $process.HasExited) { $process.Kill();$process.WaitForExit() };$process.Dispose() }
            if (-not $loaded) { throw "Startup did not complete: $log" }
            if ($mode -in @('baseline','optimized') -and -not $modLoaded) { throw "Mod did not load: $log" }
        }
    }
}
Write-Output "Raw benchmark results: $runtime/benchmark-results.json"
