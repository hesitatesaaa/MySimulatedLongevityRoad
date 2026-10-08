[CmdletBinding()]
param([ValidateSet('玩家包', '开发者包')][string]$Kind = '玩家包')

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$repo = Split-Path -Parent $PSScriptRoot
$game = 'D:\owl\gameversion\0_51_2_imported'
$mods = Join-Path $game 'Mods'
$installed = Join-Path $mods '我的模拟长生路'
$stage = Join-Path $repo ('obj\NmlSmoke054-' + $Kind)
$backup = Join-Path $stage 'original_mod'
$replacement = Join-Path $stage 'replacement_mod'
$removed = Join-Path $stage 'smoked_mod'
$zip = Join-Path $repo ('发布包\我的模拟长生路0.5.4-年度提速-' + $Kind + '.zip')
$log = Join-Path $repo ('nml-smoke-054-' + $Kind + '.log')

$modsFull = [System.IO.Path]::GetFullPath($mods).TrimEnd('\')
$installedFull = [System.IO.Path]::GetFullPath($installed)
$stageFull = [System.IO.Path]::GetFullPath($stage)
$repoFull = [System.IO.Path]::GetFullPath($repo).TrimEnd('\')
if (-not $installedFull.StartsWith($modsFull + '\', [System.StringComparison]::OrdinalIgnoreCase)) { throw '安装目录越界' }
if (-not $stageFull.StartsWith($repoFull + '\', [System.StringComparison]::OrdinalIgnoreCase)) { throw '临时目录越界' }
if (Get-Process worldbox -ErrorAction SilentlyContinue) { throw 'WorldBox 正在运行，停止冒烟测试' }
if (-not (Test-Path -LiteralPath $installed)) { throw '找不到现有模组安装' }
if (-not (Test-Path -LiteralPath $zip)) { throw '找不到发布包' }
if (Test-Path -LiteralPath $stage) { throw '临时目录已存在，先检查上次恢复状态' }
New-Item -ItemType Directory -Path $stage | Out-Null
$process = $null
try {
    Expand-Archive -LiteralPath $zip -DestinationPath (Join-Path $stage 'zip')
    $unpacked = Get-ChildItem -LiteralPath (Join-Path $stage 'zip') -Directory | Select-Object -First 1
    if ($null -eq $unpacked) { throw 'ZIP 未解出模组文件夹' }
    Move-Item -LiteralPath $unpacked.FullName -Destination $replacement
    $originalVersion = (Get-Content -LiteralPath (Join-Path $installed 'mod.json') -Raw | ConvertFrom-Json).version
    $testVersion = (Get-Content -LiteralPath (Join-Path $replacement 'mod.json') -Raw | ConvertFrom-Json).version
    if ($originalVersion -ne '0.5.3' -or $testVersion -ne '0.5.4') { throw "版本不匹配: original=$originalVersion test=$testVersion" }
    Move-Item -LiteralPath $installed -Destination $backup
    Move-Item -LiteralPath $replacement -Destination $installed
    $process = Start-Process -FilePath (Join-Path $game 'worldbox.exe') -ArgumentList @('-batchmode', '-nographics', '-logFile', $log) -PassThru -WindowStyle Hidden
    Start-Sleep -Seconds 55
    if (-not $process.HasExited) { Stop-Process -Id $process.Id -Force }
    $process.WaitForExit(15000) | Out-Null
}
finally {
    if ($null -ne $process -and -not $process.HasExited) { Stop-Process -Id $process.Id -Force }
    if ((Test-Path -LiteralPath $backup) -and (Test-Path -LiteralPath $installed)) {
        Move-Item -LiteralPath $installed -Destination $removed
    }
    if (Test-Path -LiteralPath $backup) { Move-Item -LiteralPath $backup -Destination $installed }
    if (Test-Path -LiteralPath $installed) {
        $restored = (Get-Content -LiteralPath (Join-Path $installed 'mod.json') -Raw | ConvertFrom-Json).version
        Write-Host "Restored installed mod version: $restored"
    }
}
Write-Host "Smoke log: $log"
