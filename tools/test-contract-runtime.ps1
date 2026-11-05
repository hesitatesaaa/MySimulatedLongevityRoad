param(
    [string]$RuntimeRoot = '',
    [string]$ProbeSource = '',
    [string]$OriginalGameRoot = 'E:\wol\gameversion\0_51_2_0.51.2',
    [switch]$UseInstalledSources,
    [switch]$SkipCompileCheck,
    [switch]$IdentityOnly,
    [switch]$Rendered,
    [switch]$UiOnly,
    [int]$TimeoutSeconds = 900,
    [int]$ScreenWidth = 1280,
    [int]$ScreenHeight = 720
)
# Minimal real-Unity acceptance runner for the 0.51.2 contract-spirit (契灵) rework.
# It only operates on workspace paths. The isolated game copy is
#   <project>/build/repack-load-check/game
# The original game is never written; it is only read once to assert that its
# app.info still declares WorldBox after the isolated product identity is renamed.
#
# The isolated product is renamed to MCLSLContractAudit by first BREAKING the
# app.info hardlink (delete + recreate) so the shared inode behind the original
# app.info is never modified.
$ErrorActionPreference = 'Stop'

$project = Split-Path -Parent $PSScriptRoot
if (-not $RuntimeRoot) { $RuntimeRoot = Join-Path $project 'build/repack-load-check' }
$runtime = [IO.Path]::GetFullPath($RuntimeRoot).TrimEnd('\')
$game = Join-Path $runtime 'game'
$gameExe = Join-Path $game 'worldbox.exe'
$install = Join-Path $game 'mods/MySimulatedLongevityRoad'
if (-not $ProbeSource) { $ProbeSource = Join-Path $project 'build/contract-rework/runtime/MclslAuditAcceptance.cs' }

function Assert-Under([string]$path, [string]$root, [string]$what) {
    $p = [IO.Path]::GetFullPath($path)
    $r = [IO.Path]::GetFullPath($root).TrimEnd('\')
    if (-not ($p.StartsWith($r + '\', [StringComparison]::OrdinalIgnoreCase))) { throw "Unsafe $what path: $p" }
    return $p
}

Assert-Under $game $runtime 'game' | Out-Null
Assert-Under $install $game 'install' | Out-Null
if (-not (Test-Path -LiteralPath $gameExe)) { throw "Isolated worldbox.exe missing: $gameExe" }
if (-not (Test-Path -LiteralPath $ProbeSource)) { throw "Probe source missing: $ProbeSource" }

$tag = [DateTime]::UtcNow.ToString('yyyyMMdd_HHmmss')
$work = Join-Path $project "build/contract-rework/_runs/$tag"
New-Item -ItemType Directory -Path $work -Force | Out-Null
$log = Join-Path $work 'unity.log'
$report = Join-Path $work 'result.json'
$compileLog = Join-Path $work 'compile.log'

# ---------------------------------------------------------------------------
# 1) True isolated identity: patch Application.productName at its real source
#    (globalgamemanagers) and app.info to the same 8-byte ASCII name MCLSPets.
#    Every write breaks the hardlink first (write sibling, delete entry, rename),
#    and the original game's bytes/SHA are verified unchanged.
# ---------------------------------------------------------------------------
$ProductName = 'MCLSPets'
$isoData = Assert-Under (Join-Path $game 'worldbox_Data') $game 'worldbox_Data'
$ggm = Join-Path $isoData 'globalgamemanagers'
$appInfo = Join-Path $isoData 'app.info'
if (-not (Test-Path -LiteralPath $ggm)) { throw "Isolated globalgamemanagers missing: $ggm" }
if (-not (Test-Path -LiteralPath $appInfo)) { throw "Isolated app.info missing: $appInfo" }
$originalGgm = Join-Path $OriginalGameRoot 'worldbox_Data/globalgamemanagers'
$originalAppInfo = Join-Path $OriginalGameRoot 'worldbox_Data/app.info'
$origGgmShaBefore = $null
if (Test-Path -LiteralPath $originalGgm) { $origGgmShaBefore = (Get-FileHash -LiteralPath $originalGgm -Algorithm SHA256).Hash }

function Write-BreakHardlinkBytes([string]$path, [byte[]]$bytes, [string]$root) {
    $p = Assert-Under $path $root 'file'
    $tmp = $p + '.mclsl-id-new'
    [IO.File]::WriteAllBytes($tmp, $bytes)
    Remove-Item -LiteralPath $p -Force
    Move-Item -LiteralPath $tmp -Destination $p
}
function Write-BreakHardlinkText([string]$path, [string]$text, [string]$root) {
    $p = Assert-Under $path $root 'file'
    $tmp = $p + '.mclsl-id-new'
    [IO.File]::WriteAllText($tmp, $text, (New-Object System.Text.UTF8Encoding $false))
    Remove-Item -LiteralPath $p -Force
    Move-Item -LiteralPath $tmp -Destination $p
}
function Count-Bytes([byte[]]$hay, [byte[]]$needle) {
    $n = 0
    for ($i = 0; $i -le $hay.Length - $needle.Length; $i++) {
        $ok = $true
        for ($j = 0; $j -lt $needle.Length; $j++) { if ($hay[$i + $j] -ne $needle[$j]) { $ok = $false; break } }
        if ($ok) { $n++ }
    }
    return $n
}

# --- globalgamemanagers: replace exactly the one b'\x08\x00\x00\x00WorldBox' product name ---
$patWorldBox = [byte[]](0x08,0x00,0x00,0x00,0x57,0x6F,0x72,0x6C,0x64,0x42,0x6F,0x78) # 08 00 00 00 "WorldBox"
$patPets    = [byte[]](0x08,0x00,0x00,0x00,0x4D,0x43,0x4C,0x53,0x50,0x65,0x74,0x73) # 08 00 00 00 "MCLSPets"
$asciiWorldBox = [Text.Encoding]::ASCII.GetBytes('WorldBox')
$ggmBytes = [IO.File]::ReadAllBytes($ggm)
$hitsWB = Count-Bytes $ggmBytes $patWorldBox
$hitsPets0 = Count-Bytes $ggmBytes $patPets
if ($hitsWB -eq 0 -and $hitsPets0 -eq 1) {
    Write-Output 'globalgamemanagers already patched to MCLSPets (idempotent skip)'
} else {
    if ($hitsWB -ne 1) { throw "Expected exactly one product-name WorldBox sequence in globalgamemanagers, found $hitsWB" }
    $wbBefore = Count-Bytes $ggmBytes $asciiWorldBox
    $idx = -1
    for ($i = 0; $i -le $ggmBytes.Length - $patWorldBox.Length; $i++) {
        $ok = $true
        for ($j = 0; $j -lt $patWorldBox.Length; $j++) { if ($ggmBytes[$i + $j] -ne $patWorldBox[$j]) { $ok = $false; break } }
        if ($ok) { $idx = $i; break }
    }
    if ($idx -lt 0) { throw 'Could not locate globalgamemanagers product-name sequence' }
    for ($k = 0; $k -lt $ProductName.Length; $k++) { $ggmBytes[$idx + 4 + $k] = [byte][char]$ProductName[$k] }
    Write-BreakHardlinkBytes $ggm $ggmBytes $game
    $ggmAfterPatch = [IO.File]::ReadAllBytes($ggm)
    $wbAfter = Count-Bytes $ggmAfterPatch $asciiWorldBox
    if ($wbAfter -ne ($wbBefore - 1)) { throw "globalgamemanagers replaced an unexpected number of WorldBox strings ($wbBefore -> $wbAfter)" }
    Write-Output "globalgamemanagers: product-name bytes -> $ProductName (other WorldBox strings $wbBefore -> $wbAfter, untouched)"
}
# Independent verification after the (idempotent) patch step.
$ggmVerify = [IO.File]::ReadAllBytes($ggm)
$hitsPetsV = Count-Bytes $ggmVerify $patPets
$hitsWBV = Count-Bytes $ggmVerify $patWorldBox
if ($hitsPetsV -ne 1 -or $hitsWBV -ne 0) { throw "globalgamemanagers product patch verification failed (MCLSPets=$hitsPetsV, WorldBox=$hitsWBV)" }

# --- app.info: same product name ---
$appText = [IO.File]::ReadAllText($appInfo)
if ($appText -notmatch [regex]::Escape($ProductName)) {
    $lines = $appText -split "`r?`n"
    $found = $false
    for ($i = 0; $i -lt $lines.Length; $i++) {
        if ($lines[$i].Trim() -eq 'WorldBox' -or $lines[$i].Trim() -eq 'MCLSLContractAudit') { $lines[$i] = $ProductName; $found = $true; break }
    }
    if (-not $found) { throw 'Could not locate product line in isolated app.info' }
    Write-BreakHardlinkText $appInfo ($lines -join "`n") $game
}
$appAfter = [IO.File]::ReadAllText($appInfo)
if ($appAfter -notmatch ('(?m)^' + [regex]::Escape($ProductName) + '\s*$')) { throw 'Isolated app.info product identity is not MCLSPets' }
Write-Output 'app.info product -> MCLSPets'

# --- guards: original game untouched ---
if (Test-Path -LiteralPath $originalGgm) {
    $origGgmShaAfter = (Get-FileHash -LiteralPath $originalGgm -Algorithm SHA256).Hash
    if ($origGgmShaBefore -ne $origGgmShaAfter) { throw "Original globalgamemanagers SHA changed: $origGgmShaBefore -> $origGgmShaAfter" }
    Write-Output "original globalgamemanagers SHA unchanged ($origGgmShaAfter)"
}
if (Test-Path -LiteralPath $originalAppInfo) {
    $orig = [IO.File]::ReadAllText($originalAppInfo)
    if ($orig -notmatch 'WorldBox') { throw "Original app.info no longer declares WorldBox: $originalAppInfo" }
    Write-Output 'original app.info still WorldBox (read-only guard ok)'
}

# 2) Stage sources plus probe into the isolated install.
$stage = Join-Path $work 'staged'
New-Item -ItemType Directory -Path $stage -Force | Out-Null
if (-not $UseInstalledSources) {
    $installCode = Assert-Under (Join-Path $install 'code') $install 'install/code'
    if (Test-Path -LiteralPath $installCode) { Remove-Item -LiteralPath $installCode -Recurse -Force }
    Copy-Item -LiteralPath (Join-Path $project 'code') -Destination $installCode -Recurse
    Copy-Item -LiteralPath (Join-Path $project 'MclslMod.cs') -Destination (Join-Path $install 'MclslMod.cs') -Force
    if (Test-Path (Join-Path $project 'mod.json')) { Copy-Item -LiteralPath (Join-Path $project 'mod.json') -Destination (Join-Path $install 'mod.json') -Force }
    Write-Output "staged workspace sources into isolated install"
} else {
    Write-Output "using installed mod sources (isolated package snapshot)"
}
Copy-Item -LiteralPath (Join-Path $install 'MclslMod.cs') -Destination (Join-Path $stage 'MclslMod.cs.orig') -Force

$probeDest = Join-Path $install 'code/MySimulatedLongevityRoad/Core/MclslAuditAcceptance.cs'
$probeDest = Assert-Under $probeDest $install 'probe'
New-Item -ItemType Directory -Force -Path (Split-Path $probeDest -Parent) | Out-Null
Copy-Item -LiteralPath $ProbeSource -Destination $probeDest -Force

$entry = Join-Path $install 'MclslMod.cs'
$source = [IO.File]::ReadAllText($entry)
# Normalize any previous injection so the anchor stays unique and Ensure is added once.
$source = $source.Replace(' MclslAuditAcceptance.Ensure();', '')
$anchor = 'MclslLocalizationRuntimeMarker.MarkLoaded();'
if (($source.Split(@($anchor), [StringSplitOptions]::None)).Length -ne 2) { throw 'Unexpected mod entry source (MarkLoaded anchor)' }
$source = $source.Replace($anchor, $anchor + ' MclslAuditAcceptance.Ensure();')
[IO.File]::WriteAllText($entry, $source, (New-Object System.Text.UTF8Encoding $false))
Copy-Item -LiteralPath $entry -Destination (Join-Path $stage 'MclslMod.cs.injected') -Force

# 3) Compile-check the staged mod (never touches the production tree).
if (-not $SkipCompileCheck) {
    try {
        $sdk = (& dotnet --list-sdks | Select-Object -Last 1)
        if (-not $sdk -or $sdk -notmatch '^([^ ]+) \[(.+)\]$') { throw 'A .NET SDK with Roslyn is required for compile-check' }
        $csc = Join-Path $Matches[2] "$($Matches[1])\Roslyn\bincore\csc.dll"
        $managed = Join-Path $game 'worldbox_Data\Managed'
        $loaderRoot = Join-Path $game 'worldbox_Data\StreamingAssets\mods'
        $refs = @(Get-ChildItem -LiteralPath $managed -Filter '*.dll' | Where-Object Name -ne 'Assembly-CSharp.dll' | ForEach-Object FullName)
        $refs += @((Join-Path $loaderRoot 'NML\Assembly-CSharp-Publicized.dll'), (Join-Path $loaderRoot 'NeoModLoader.dll'), (Join-Path $loaderRoot 'NML\Assemblies\0Harmony.dll'))
        $outDll = Join-Path $work 'MySimulatedLongevityRoad.dll'
        $cargs = @('/nostdlib+', '/target:library', '/langversion:latest', '/nullable:annotations', '/optimize+', '/debug:portable', '/deterministic+', '/utf8output', "/out:`"$outDll`"")
        foreach ($r in $refs) { $cargs += "/reference:`"$r`"" }
        $srcs = @(Get-ChildItem -LiteralPath (Join-Path $install 'code') -Recurse -Filter '*.cs' | ForEach-Object FullName)
        $srcs += (Join-Path $install 'MclslMod.cs')
        foreach ($s in $srcs) { $cargs += "`"$s`"" }
        $rsp = Join-Path $work 'compile.rsp'
        [IO.File]::WriteAllLines($rsp, $cargs, (New-Object System.Text.UTF8Encoding $false))
        & dotnet $csc /noconfig "@$rsp" 2>&1 | Tee-Object -FilePath $compileLog | Out-Null
        if ($LASTEXITCODE -ne 0) {
            Write-Output "COMPILE FAILED (exit $LASTEXITCODE); see $compileLog"
            Get-Content -LiteralPath $compileLog | Select-Object -First 15 | Write-Output
            exit 2
        }
        Write-Output "compile-check ok ($($srcs.Count) sources)"
    } catch {
        Write-Output "compile-check skipped: $($_.Exception.Message)"
    }
}

# 4) Launch isolated Unity hidden, wait for the report, kill only our own process.
$clock = [Diagnostics.Stopwatch]::StartNew()
$launchArgs = @('-screen-width', "$ScreenWidth", '-screen-height', "$ScreenHeight",
    '-logFile', $log, '--mclsl-acceptance-output', $report)
if ($IdentityOnly) { $launchArgs += '--mclsl-identity-only' }
if (-not $Rendered) { $launchArgs = @('-batchmode') + $launchArgs }
if ($UiOnly) { $launchArgs += '--mclsl-ui-only' }
$process = Start-Process -FilePath $gameExe -ArgumentList $launchArgs -WorkingDirectory $game -WindowStyle Hidden -PassThru
$ownPath = $null
try { $ownPath = (Get-Process -Id $process.Id -ErrorAction Stop).Path } catch { }
Write-Output ('Unity PID=' + $process.Id + ' path=' + $ownPath + ' report=' + $report)
if ($ownPath -and ([IO.Path]::GetFullPath($ownPath) -ne [IO.Path]::GetFullPath($gameExe))) {
    throw "Refusing to continue: started process path '$ownPath' is not the isolated worldbox.exe"
}
try {
    while (-not $process.HasExited -and $clock.Elapsed.TotalSeconds -lt $TimeoutSeconds) {
        if (Test-Path -LiteralPath $report) {
            try {
                $r = Get-Content -LiteralPath $report -Raw -Encoding UTF8 | ConvertFrom-Json
                if ($null -eq $r.complete -or $r.complete) { break }
            } catch { }
        }
        if (Test-Path -LiteralPath $log) {
            $tail = (Get-Content -LiteralPath $log -Tail 120 -Encoding UTF8) -join "`n"
            if ($tail -match 'Failed to compile mod|Required Harmony patches failed') {
                throw "Unity mod initialization failed; inspect $log"
            }
        }
        Start-Sleep -Milliseconds 500
        $process.Refresh()
    }
    if (-not (Test-Path -LiteralPath $report)) { throw "Unity acceptance report missing; inspect $log" }
    $result = Get-Content -LiteralPath $report -Raw -Encoding UTF8 | ConvertFrom-Json
    if ($null -ne $result.complete -and -not $result.complete) { throw "Probe report incomplete (partial is not evidence): $report" }
    Write-Output ('unityProduct=' + $result.productName + ' persistentDataPath=' + $result.persistentDataPath)
    if ($IdentityOnly) {
        if ($result.identityOk -eq $true) {
            Write-Output 'IDENTITY OK: Application.productName and persistentDataPath are the isolated MCLSPets identity.'
            Write-Output ('report=' + $report)
            Write-Output ('log=' + $log)
            exit 0
        }
        Write-Output 'IDENTITY FAILURE: isolated MCLSPets identity not active; stopping without world data writes.'
        Write-Output ('report=' + $report)
        exit 1
    }
    if ($result.identityOk -eq $false) {
        Write-Output 'IDENTITY FAILURE: Application.productName / persistentDataPath is not the isolated MCLSPets identity; stopping.'
        Write-Output ('report=' + $report)
        exit 1
    }
    $line = 'probeReady=' + $result.probeReady + ' passed=' + $result.passed + ' pass=' + $result.summary.pass + ' fail=' + $result.summary.fail + ' pending=' + $result.summary.pending
    Write-Output $line
    if ($result.api -and -not $result.api.present) { Write-Output ('contract-api-present=false missing=' + ($result.api.missing -join ',')) }
    Write-Output ('report=' + $report)
    Write-Output ('log=' + $log)
    if (-not $result.passed) {
        $bad = @($result.checks | Where-Object { $_.status -ne 'pass' })
        foreach ($b in ($bad | Select-Object -First 6)) {
            Write-Output ('  [' + $b.status + '] ' + $b.id + ': ' + $b.detail)
        }
        exit 1
    }
}
finally {
    try {
        if (-not $process.HasExited) {
            $procPath = $null
            try { $procPath = (Get-Process -Id $process.Id -ErrorAction Stop).Path } catch { }
            if (-not $procPath -or [IO.Path]::GetFullPath($procPath) -eq [IO.Path]::GetFullPath($gameExe)) {
                $process.Kill()
                $process.WaitForExit()
            }
        }
    } catch { }
    $process.Dispose()
}
