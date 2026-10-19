$ErrorActionPreference = 'Stop'
$runRoot = 'E:\0.6.0\output\runtime-validation\visual-baihu-20261014'
$mods = 'E:\wol\gameversion\0_51_2_0.51.2\mods'
$backup = Join-Path $runRoot 'original-game-mod'
$used = Join-Path $runRoot 'used-game-test-mod'
$reference = Join-Path $mods 'Thefantasyworld'
$referenceUsed = Join-Path $runRoot 'used-fantasy-reference'
function Assert-Within([string]$Path, [string]$Parent) {
    $full = [IO.Path]::GetFullPath($Path)
    $prefix = [IO.Path]::GetFullPath($Parent).TrimEnd('\') + '\'
    if (-not $full.StartsWith($prefix, [StringComparison]::OrdinalIgnoreCase)) { throw "Unsafe target: $full" }
}
$gameRoot = [IO.Path]::GetFullPath((Split-Path -Parent $mods)).TrimEnd('\') + '\'
foreach ($running in Get-Process worldbox -ErrorAction SilentlyContinue) {
    if (-not $running.Path -or $running.Path.StartsWith($gameRoot, [StringComparison]::OrdinalIgnoreCase)) { throw 'A WorldBox process using the target installation is running; refuse recovery.' }
}
if (-not (Test-Path -LiteralPath $backup)) { throw 'Original backup missing; refuse recovery.' }
$gameMod = $null
foreach ($directory in Get-ChildItem -LiteralPath $mods -Directory) {
    $file = Join-Path $directory.FullName 'mod.json'
    if (-not (Test-Path -LiteralPath $file)) { continue }
    try { $j = Get-Content -LiteralPath $file -Raw -Encoding UTF8 | ConvertFrom-Json } catch { continue }
    if ($j.GUID -eq 'MY_SIMULATED_LONGEVITY_ROAD') {
        if ($gameMod) { throw 'Multiple matching mods.' }
        $gameMod = $directory.FullName
    }
}
if (-not $gameMod) { throw 'Installed test mod missing.' }
Assert-Within $gameMod $mods
Assert-Within $reference $mods
foreach ($path in @($backup,$used,$referenceUsed)) { Assert-Within $path $runRoot }
foreach ($path in @($used,$referenceUsed)) { if (Test-Path -LiteralPath $path) { throw "Recovery destination exists: $path" } }
$expectedManifest = Join-Path $runRoot 'MySimulatedLongevityRoad\resources\actor-frames.manifest.json'
$installedManifest = Join-Path $gameMod 'resources\actor-frames.manifest.json'
if ((Get-FileHash -LiteralPath $expectedManifest).Hash -ne (Get-FileHash -LiteralPath $installedManifest).Hash) { throw 'Installed mod differs from our test stage; refuse move.' }
if (Test-Path -LiteralPath $reference) {
    $originalRef = 'E:\0.6.0\output\runtime-validation\fantasy-reference-used\mod.json'
    if ((Get-FileHash -LiteralPath $originalRef).Hash -ne (Get-FileHash -LiteralPath (Join-Path $reference 'mod.json')).Hash) { throw 'Reference differs from our temporary copy.' }
    Move-Item -LiteralPath $reference -Destination $referenceUsed
}
Move-Item -LiteralPath $gameMod -Destination $used
Move-Item -LiteralPath $backup -Destination $gameMod
[ordered]@{ recovered=$true; originalModRestored=$true; worldBoxRunning=$false; sizeTestPassed=$false; recoveryUtc=[DateTime]::UtcNow.ToString('o') } | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $runRoot 'recovery.json') -Encoding UTF8
Write-Output 'Original game mod restored; temporary test copies retained in workspace.'
