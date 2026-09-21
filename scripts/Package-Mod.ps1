[CmdletBinding()]
param(
    [Parameter(Mandatory = $false)]
    [ValidatePattern('^[\p{IsCJKUnifiedIdeographs}]{2,5}$')]
    [string]$ChangeTag = '综合更新'
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$repoRoot = Split-Path -Parent $PSScriptRoot
$modFile = Join-Path $repoRoot 'mod.json'
$packageDirectory = Join-Path $repoRoot '发布包'
$runtimeItems = @(
    'InterestingTrait.cs',
    'InterestingTrait.csproj',
    'InterestingTrait.sln',
    'mod.json',
    'default_config.json',
    'icon.png',
    'README.md',
    'CHANGELOG.md',
    'code',
    'GameResources',
    'Locales'
)
$stagingRoot = $null
$zipPath = $null
$success = $false

function Copy-PackageItem {
    param([string]$RelativePath, [string]$DestinationRoot)
    $source = Join-Path $repoRoot $RelativePath
    if (-not (Test-Path -LiteralPath $source)) { throw "缺少发布包文件或目录：$RelativePath" }
    $destination = Join-Path $DestinationRoot $RelativePath
    $parent = Split-Path -Parent $destination
    if (-not (Test-Path -LiteralPath $parent)) { New-Item -ItemType Directory -Path $parent -Force | Out-Null }
    Copy-Item -LiteralPath $source -Destination $destination -Recurse -Force
}

function Test-SourcePackageContents {
    param([string]$Path, [string]$RootName, [string]$Version)
    Add-Type -AssemblyName System.IO.Compression.FileSystem
    $archive = [System.IO.Compression.ZipFile]::OpenRead($Path)
    try {
        $entries = @($archive.Entries | ForEach-Object { $_.FullName.Replace('\', '/') })
        $root = $RootName.TrimEnd('/') + '/'
        foreach ($required in @('InterestingTrait.cs', 'InterestingTrait.csproj', 'InterestingTrait.sln', 'mod.json', 'default_config.json', 'icon.png', 'README.md', 'CHANGELOG.md')) {
            if ($entries -notcontains ($root + $required)) { throw "发布包缺少必需文件：$required" }
        }
        foreach ($directory in @('code/', 'GameResources/', 'Locales/')) {
            if (-not ($entries | Where-Object { $_.StartsWith($root + $directory, [System.StringComparison]::OrdinalIgnoreCase) })) { throw "发布包缺少必需目录：$directory" }
        }
        $forbidden = $entries | Where-Object {
            $_ -match '(^|/)(references|\.git|bin|obj|DeveloperTools|scripts)(/|$)' -or
            $_ -match '(^|/)(\.vs|\.idea)(/|$)' -or
            $_ -match '\.(user|suo|tmp|log|cache)$'
        }
        if ($forbidden) { throw "发布包包含禁止内容：$($forbidden -join ', ')" }
        $modEntry = $archive.Entries | Where-Object { $_.FullName.Replace('\', '/') -eq ($root + 'mod.json') } | Select-Object -First 1
        $reader = New-Object System.IO.StreamReader($modEntry.Open())
        try { $metadata = $reader.ReadToEnd() | ConvertFrom-Json } finally { $reader.Dispose() }
        if ([string]$metadata.version -ne $Version) { throw "发布包中的 mod.json 版本不匹配：$($metadata.version)，期望 $Version" }
    }
    finally { $archive.Dispose() }
}

try {
    if (-not (Test-Path -LiteralPath $modFile)) { throw "找不到模组元数据：$modFile" }
    $metadata = Get-Content -LiteralPath $modFile -Raw | ConvertFrom-Json
    $version = [string]$metadata.version
    if ([string]::IsNullOrWhiteSpace($version) -or $version -notmatch '^\d+\.\d+\.\d+(\.\d+)?$') { throw "mod.json 中的 version 不是有效版本号：$version" }
    if (-not (Test-Path -LiteralPath $packageDirectory)) { New-Item -ItemType Directory -Path $packageDirectory -Force | Out-Null }

    $packageFolderName = "0.5.1+我的模拟长生路$version-$ChangeTag"
    $stagingRoot = Join-Path $packageDirectory ('.staging-' + [Guid]::NewGuid().ToString('N'))
    $packageRoot = Join-Path $stagingRoot $packageFolderName
    New-Item -ItemType Directory -Path $packageRoot -Force | Out-Null
    foreach ($item in $runtimeItems) { Copy-PackageItem -RelativePath $item -DestinationRoot $packageRoot }

    $zipPath = Join-Path $packageDirectory ($packageFolderName + '.zip')
    if (Test-Path -LiteralPath $zipPath) {
        $stamp = Get-Date -Format 'yyyyMMdd-HHmmss'
        $suffix = 0
        do {
            if ($suffix -eq 0) { $collisionSuffix = "" }
            else { $collisionSuffix = "-" + [string]$suffix }
            $zipPath = Join-Path $packageDirectory ("$packageFolderName-$stamp$collisionSuffix.zip")
            $suffix++
        } while (Test-Path -LiteralPath $zipPath)
    }

    $stagingItems = @(Get-ChildItem -LiteralPath $stagingRoot -Force | ForEach-Object { $_.FullName })
    Compress-Archive -Path $stagingItems -DestinationPath $zipPath -CompressionLevel Optimal
    Test-SourcePackageContents -Path $zipPath -RootName $packageFolderName -Version $version
    $success = $true
    Write-Output $zipPath
}
catch {
    Write-Error $_.Exception.Message
    exit 1
}
finally {
    if ($stagingRoot -and (Test-Path -LiteralPath $stagingRoot)) { Remove-Item -LiteralPath $stagingRoot -Recurse -Force -ErrorAction SilentlyContinue }
    if (-not $success -and $zipPath -and (Test-Path -LiteralPath $zipPath)) { Remove-Item -LiteralPath $zipPath -Force -ErrorAction SilentlyContinue }
}
