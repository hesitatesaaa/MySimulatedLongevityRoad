[CmdletBinding()]
param(
    [Parameter(Mandatory = $false)]
    [ValidateSet('Static', 'Build', 'Package')]
    [string]$Mode = 'Static',

    [Parameter(Mandatory = $false)]
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Release',

    [Parameter(Mandatory = $false)]
    [string]$WorldBoxDataRoot,

    [Parameter(Mandatory = $false)]
    [switch]$NoRestore,

    [Parameter(Mandatory = $false)]
    [ValidatePattern('^[\p{IsCJKUnifiedIdeographs}]{2,5}$')]
    [string]$ChangeTag = '项目整理'
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$repoRoot = Split-Path -Parent $PSScriptRoot
$projectPath = Join-Path $repoRoot 'InterestingTrait.csproj'
$buildScript = Join-Path $PSScriptRoot 'Build-Mod.ps1'
$packageScript = Join-Path $PSScriptRoot 'Package-Mod.ps1'
$mortalItemIntegrationScript = Join-Path $PSScriptRoot 'Test-MortalItemIntegration.ps1'

function Invoke-CheckedScript {
    param(
        [Parameter(Mandatory = $true)][string]$Path,
        [Parameter(Mandatory = $false)][hashtable]$Parameters = @{}
    )

    & $Path @Parameters
    if ($LASTEXITCODE -ne 0) {
        throw "脚本执行失败（$LASTEXITCODE）：$Path"
    }
}

function Test-JsonFiles {
    $jsonFiles = Get-ChildItem -LiteralPath $repoRoot -Recurse -File -Filter '*.json' | Where-Object {
        $relativePath = $_.FullName.Substring($repoRoot.Length + 1)
        $relativePath -notmatch '^(?:_archive|bin|obj|发布包|\.codegraph|\.codex|DeveloperTools|references)\\'
    }

    foreach ($file in $jsonFiles) {
        Get-Content -LiteralPath $file.FullName -Raw | ConvertFrom-Json | Out-Null
    }

    $chPath = Join-Path $repoRoot 'Locales\ch.json'
    $ch = Get-Content -LiteralPath $chPath -Raw | ConvertFrom-Json
    $chKeys = @($ch.PSObject.Properties.Name)
    if ($chKeys.Count -eq 0 -or @($chKeys | Where-Object { [string]::IsNullOrWhiteSpace([string]$ch.$_) }).Count -gt 0) {
        throw '简体中文本地化为空或包含空白翻译。'
    }

    # WorldBox normalizes punctuation and capital letters in add(), so two
    # differently spelled source keys can overwrite the same visible label.
    $normalized = @{}
    foreach ($key in $chKeys) {
        $canonical = [regex]::Replace($key, '([a-z0-9])([A-Z])', '$1_$2')
        $canonical = [regex]::Replace($canonical, '[^\p{L}\p{Nd}_]+', '_').Trim('_').ToLowerInvariant()
        $value = [string]$ch.$key
        if ($normalized.ContainsKey($canonical) -and $normalized[$canonical].Value -ne $value) {
            throw "简体中文键归一化冲突：$($normalized[$canonical].Key) 与 $key -> $canonical"
        }
        $normalized[$canonical] = @{ Key = $key; Value = $value }
    }

    Write-Host "JSON 检查通过：$($jsonFiles.Count) 个文件；本地化键：$($chKeys.Count) 个。"
}

function Test-CompileBoundary {
    $dotnet = Get-Command dotnet -ErrorAction SilentlyContinue
    if ($null -eq $dotnet) {
        throw '静态检查需要 dotnet SDK。'
    }

    $output = & $dotnet.Source msbuild $projectPath -getItem:Compile -nologo -p:_EnableDefaultWindowsPlatform=false 2>&1 | Out-String
    $exitCode = $LASTEXITCODE
    if ($exitCode -ne 0) {
        throw "无法读取项目编译项：$output"
    }
    if ($output -match 'DeveloperTools\\|references\\|WorldBoxGameDecompiled') {
        throw '项目编译项包含被排除的 DeveloperTools 或 references 文件。'
    }
    if ($output -notmatch 'InterestingTrait.cs') {
        throw '项目编译项缺少 InterestingTrait.cs。'
    }

    Write-Host '编译边界检查通过。'
}

function Test-GitDiff {
    & git -C $repoRoot diff --check
    if ($LASTEXITCODE -ne 0) {
        throw 'git diff --check 失败。'
    }

    Write-Host 'Git 空白检查通过。'
}

Write-Host "项目检查模式：$Mode"
Test-JsonFiles
Test-CompileBoundary
Test-GitDiff
Invoke-CheckedScript -Path $mortalItemIntegrationScript

if ($Mode -eq 'Build' -or $Mode -eq 'Package') {
    if ($Mode -eq 'Build') {
        $buildParameters = @{
            Configuration = $Configuration
        }
        if ($PSBoundParameters.ContainsKey('WorldBoxDataRoot')) {
            $buildParameters.WorldBoxDataRoot = $WorldBoxDataRoot
        }
        if ($NoRestore) {
            $buildParameters.NoRestore = $true
        }
        Invoke-CheckedScript -Path $buildScript -Parameters $buildParameters
    }

    if ($Mode -eq 'Package') {
        Invoke-CheckedScript -Path $packageScript -Parameters @{ ChangeTag = $ChangeTag }
    }
}

Write-Host "项目检查完成：$Mode"
