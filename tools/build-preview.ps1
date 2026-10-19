param(
    [string]$GameRoot = 'E:\wol\gameversion\0_51_2_0.51.2',
    [string]$OutputDirectory = ''
)
$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path -Parent $PSScriptRoot
$declaration = Get-Content -LiteralPath (Join-Path $projectRoot 'mod.json') -Raw | ConvertFrom-Json
if ($declaration.version -ne '0.5.4') {
    throw "Preview package expects the workspace declaration to remain 0.5.4; found $($declaration.version)."
}
if (-not $OutputDirectory) {
    $OutputDirectory = Join-Path $projectRoot ('build\Preview-0.6.0-' + (Get-Date -Format 'yyyyMMdd-HHmmss'))
}
$outputFullPath = [IO.Path]::GetFullPath($OutputDirectory)
$workspacePrefix = [IO.Path]::GetFullPath($projectRoot).TrimEnd('\') + '\'
if (-not $outputFullPath.StartsWith($workspacePrefix, [StringComparison]::OrdinalIgnoreCase)) {
    throw 'Preview output must remain inside this workspace.'
}
if (Test-Path -LiteralPath $outputFullPath) {
    throw 'Preview output already exists; choose a fresh directory.'
}
$compilerOutput = Join-Path $outputFullPath 'compiler'
& (Join-Path $PSScriptRoot 'build-release.ps1') -GameRoot $GameRoot -OutputDirectory $compilerOutput
if ($LASTEXITCODE -ne 0) { throw 'Preview compiler failed.' }

$package = Join-Path $outputFullPath 'MySimulatedLongevityRoad'
New-Item -ItemType Directory -Path $package -Force | Out-Null
foreach ($name in @('code', 'GameResources', 'Locales', 'resources', 'InterestingTrait.cs', 'default_config.json', 'mod.json', 'icon.png')) {
    Copy-Item -LiteralPath (Join-Path $projectRoot $name) -Destination $package -Recurse
}
if (Test-Path -LiteralPath (Join-Path $projectRoot 'NativeActorResources')) {
    Copy-Item -LiteralPath (Join-Path $projectRoot 'NativeActorResources') -Destination $package -Recurse
}
$progressPath = Join-Path $projectRoot 'output\actor-redraw\progress.json'
$validationPath = Join-Path $projectRoot 'output\actor-redraw\validation.json'
$manifestPath = Join-Path $projectRoot 'resources\actor-frames.manifest.json'
$progress = Get-Content -LiteralPath $progressPath -Raw | ConvertFrom-Json
$validation = Get-Content -LiteralPath $validationPath -Raw | ConvertFrom-Json
$manifest = Get-Content -LiteralPath $manifestPath -Raw | ConvertFrom-Json
$installedFrameCount = 0
foreach ($group in $manifest.groups) {
    if (-not $group.approved) { continue }
    foreach ($action in $group.actions.PSObject.Properties) {
        $installedFrameCount += @($action.Value.frames).Count
    }
}
$installedIconCount = @($manifest.icons | Where-Object approved).Count
Copy-Item -LiteralPath (Join-Path $compilerOutput 'build-record.json') -Destination $package
Copy-Item -LiteralPath $validationPath -Destination (Join-Path $package 'asset-validation.json')
$preview = [ordered]@{
    packageType = 'temporary-validation-preview'
    declaredVersion = $declaration.version
    targetVersion = '0.6.0'
    targetGameBuild = $declaration.targetGameBuild
    createdAtUtc = [DateTime]::UtcNow.ToString('o')
    offlineInstalledForms = $progress.offlineInstalledForms
    targetForms = @($manifest.groups).Count
    offlineCandidateAnimationFrames = $progress.generatedFrames
    installedAnimationFrames = $installedFrameCount
    totalAnimationFrameTarget = $progress.targetFrames
    remainingAnimationFrames = $progress.targetFrames - $installedFrameCount
    offlineCandidatePlacementIcons = $progress.generatedIcons
    installedPlacementIcons = $installedIconCount
    targetPlacementIcons = $progress.targetIcons
    remainingPlacementIcons = $progress.targetIcons - $installedIconCount
    realGameValidationPassed = $false
    saveTestScope = 'new-save-only'
    formalReleaseReady = $false
    note = 'For preview testing in a newly created world. Incomplete redraws still use the workspace baseline resources.'
}
$preview | ConvertTo-Json -Depth 4 | Set-Content -LiteralPath (Join-Path $package 'preview-information.json') -Encoding utf8
$previewNotes = @"
# 0.6.0 临时验证包

这是供新建存档实机检查的临时包，mod.json 中保留版本声明 $($declaration.version)，目标游戏构建为 $($declaration.targetGameBuild)。它不是正式 0.6.0 发布包。

当前有 $($progress.offlineInstalledForms) 套完整形态、$installedFrameCount 张离线审看的动画帧和 $installedIconCount 个专用放置图标已接入。另有 $($progress.generatedFrames) 张动画候选和 $($progress.generatedIcons) 个图标尚未全部审看、接入。其余形态仍使用工作区现有的 0.5.4 资源；正式目标是 $(@($manifest.groups).Count) 套动画和 $($progress.targetIcons) 个图标。世界尺寸、按钮占比、帧播放和性能仍待实机检查。

请在新建世界中检查单位能否生成，$($progress.offlineInstalledForms) 套已接入形态能否显示五类动作，其他单位和放置按钮能否正常工作。存档验证范围只含新存档。实机 FPS 和单位数量检查尚未通过，本包标记为预览，不能作为正式发行版。

随包的 asset-validation.json 和 preview-information.json 记录了此次打包时的资源状态。完成剩余绘图、逐套审看、尺寸校准和实机验收后，再生成正式 0.6.0 包。
"@
Set-Content -LiteralPath (Join-Path $package 'PREVIEW-ONLY.md') -Value $previewNotes -Encoding utf8
$archive = Join-Path $outputFullPath 'MySimulatedLongevityRoad-0.5.4-preview.zip'
Compress-Archive -Path (Join-Path $package '*') -DestinationPath $archive
$archiveRecord = [ordered]@{
    archive = $archive
    archiveSha256 = (Get-FileHash -LiteralPath $archive -Algorithm SHA256).Hash
    package = $package
    buildRecord = Join-Path $package 'build-record.json'
    previewInformation = Join-Path $package 'preview-information.json'
    validationErrors = $validation.errors.Count
    releaseReady = $validation.releaseReady
}
$archiveRecord | ConvertTo-Json -Depth 4 | Set-Content -LiteralPath (Join-Path $outputFullPath 'preview-package-record.json') -Encoding utf8
Write-Output ($archiveRecord | ConvertTo-Json -Depth 4)
