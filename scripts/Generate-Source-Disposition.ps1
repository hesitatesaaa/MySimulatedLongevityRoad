[CmdletBinding()]
param(
    [string]$BaselineRoot = 'D:\模拟长胜路\_incoming_0.3.5\MySimulatedLongevityRoad-0.3.5',
    [string]$OutputPath = ''
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path -Parent $PSScriptRoot
if ([string]::IsNullOrWhiteSpace($OutputPath)) {
    $OutputPath = Join-Path $projectRoot 'docs\architecture\0.5.0-source-disposition.md'
}

function Get-Layer([string]$relative) {
    if ($relative -match '(^|/)Patches/') { return '游戏适配层' }
    if ($relative -match '(^|/)UI/') { return '查询与展示层' }
    if ($relative -match '(^|/)Data/') { return '领域数据层' }
    if ($relative -match '(^|/)Modules/|Systems/Runtime/') { return '调度与持久化层' }
    if ($relative -match 'Systems/Reincarnation/|WorldResources/') { return '持久化/世界领域' }
    if ($relative -match '(^|/)Systems/') { return '规则与命令层' }
    if ($relative -match '(^|/)Traits/') { return '游戏适配/规则层' }
    return '基础设施'
}

function Get-Reason([string]$relative, [string]$status) {
    if ($status -eq '删除') { return '已由新架构替代或不再进入 0.5.0 编译。' }
    if ($status -eq '新增') { return '0.5.0 新领域、事务、调度、验证或展示实现。' }
    if ($relative -match 'Catalog|Ids|Definitions|Data.cs|ItemState.cs') {
        return '内容定义可复用；运行读写由 0.5.0 命令、上下文或查询层承接。'
    }
    if ($status -eq '原样复用') { return '已复核为静态内容、无状态规则或稳定适配层；由新架构调用边界继续使用。' }
    return '已修改并接入 0.5.0 架构、事务、调度或查询边界。'
}

function Get-Map([string]$root) {
    $map = @{}
    Get-ChildItem -LiteralPath $root -Filter '*.cs' -File -Recurse |
        Where-Object { $_.FullName -notmatch '[\\/](bin|obj|release|发布包|_archive)[\\/]' } |
        ForEach-Object {
        $relative = [IO.Path]::GetRelativePath($root, $_.FullName).Replace('\','/')
        $map[$relative] = $_.FullName
    }
    return $map
}

$baseline = Get-Map $BaselineRoot
$current = Get-Map $projectRoot
$paths = @($baseline.Keys + $current.Keys | Sort-Object -Unique)
$lines = [Collections.Generic.List[string]]::new()
$lines.Add('# 0.5.0 全源码处置清单')
$lines.Add('')
$lines.Add('基线：实施时只读导入的 0.3.5；当前：本工作树 0.5.0。哈希相同只表示文件文本未改，仍需结合“理由/替代关系”判断是否属于允许复用的内容数据。')
$lines.Add('')
$lines.Add('| 源码 | 层 | 处置 | 理由或替代关系 |')
$lines.Add('|---|---|---|---|')
foreach ($relative in $paths) {
    $hasOld = $baseline.ContainsKey($relative)
    $hasNew = $current.ContainsKey($relative)
    if (!$hasNew) { $status = '删除' }
    elseif (!$hasOld) { $status = '新增' }
    else {
        $oldHash = (Get-FileHash -LiteralPath $baseline[$relative] -Algorithm SHA256).Hash
        $newHash = (Get-FileHash -LiteralPath $current[$relative] -Algorithm SHA256).Hash
        $status = if ($oldHash -eq $newHash) { '原样复用' } else { '重构/修改' }
    }
    $layer = Get-Layer $relative
    $reason = Get-Reason $relative $status
    $lines.Add("| ``$relative`` | $layer | $status | $reason |")
}
$lines.Add('')
$lines.Add("统计：0.3.5 源码 $($baseline.Count) 个；0.5.0 当前源码 $($current.Count) 个；清单共 $($paths.Count) 项。")
Set-Content -LiteralPath $OutputPath -Value $lines -Encoding utf8
Write-Output $OutputPath
