# 资源仓(.git-assets)操作封装：与本仓库根共享同一工作区，只跟踪三个美术目录。
# 用法示例：
#   pwsh tools/git-assets.ps1 status
#   pwsh tools/git-assets.ps1 add -A
#   pwsh tools/git-assets.ps1 commit -m "update art"
#   pwsh tools/git-assets.ps1 log --oneline -5
param(
    [Parameter(ValueFromRemainingArguments = $true)]
    [string[]]$GitArgs
)
$root = Split-Path -Parent $PSScriptRoot
$gd = Join-Path $root '.git-assets'
if (-not (Test-Path $gd)) { throw "资源仓不存在: $gd" }
& git --git-dir="$gd" --work-tree="$root" @GitArgs
exit $LASTEXITCODE
