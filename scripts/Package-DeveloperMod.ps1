[CmdletBinding()]
param(
    [Parameter(Mandatory = $false)]
    [ValidatePattern('^[\p{IsCJKUnifiedIdeographs}]{2,5}$')]
    [string]$ChangeTag = '开发验证'
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$repoRoot = Split-Path -Parent $PSScriptRoot
$packageDirectory = Join-Path $repoRoot '发布包'
$metadata = Get-Content (Join-Path $repoRoot 'mod.json') -Raw | ConvertFrom-Json
$version = [string]$metadata.version
$folderName = "0.5.1+我的模拟长生路$version-$ChangeTag"
$staging = Join-Path $packageDirectory ('.staging-dev-' + [Guid]::NewGuid().ToString('N'))
$root = Join-Path $staging $folderName
$zipPath = $null

try {
    New-Item -ItemType Directory -Path $root -Force | Out-Null
    foreach ($item in @('InterestingTrait.cs','InterestingTrait.csproj','InterestingTrait.sln','mod.json','default_config.json','icon.png','README.md','CHANGELOG.md','code','GameResources','Locales')) {
        Copy-Item -LiteralPath (Join-Path $repoRoot $item) -Destination (Join-Path $root $item) -Recurse -Force
    }
    $devDestination = Join-Path $root 'code\MySimulatedLongevityRoad\DeveloperTools'
    New-Item -ItemType Directory -Path $devDestination -Force | Out-Null
    Get-ChildItem -LiteralPath (Join-Path $repoRoot 'DeveloperTools') -Filter '*.cs' -File |
        Copy-Item -Destination $devDestination -Force
    $zipPath = Join-Path $packageDirectory ($folderName + '.zip')
    if (Test-Path -LiteralPath $zipPath) { $zipPath = Join-Path $packageDirectory ("$folderName-" + (Get-Date -Format 'yyyyMMdd-HHmmss') + '.zip') }
    Compress-Archive -Path (Join-Path $staging '*') -DestinationPath $zipPath -CompressionLevel Optimal
    Add-Type -AssemblyName System.IO.Compression.FileSystem
    $archive = [IO.Compression.ZipFile]::OpenRead($zipPath)
    try {
        $entries = @($archive.Entries.FullName.Replace('\','/'))
        foreach ($required in @('code/MySimulatedLongevityRoad/DeveloperTools/MclslDeveloperActorEditor.cs','code/MySimulatedLongevityRoad/DeveloperTools/MclslDeveloperApi.cs','mod.json')) {
            if ($entries -notcontains ($folderName + '/' + $required)) { throw "开发包缺少：$required" }
        }
        if ($entries | Where-Object { $_ -match '(^|/)(references|\.git|bin|obj|scripts)(/|$)' }) { throw '开发包包含禁止内容。' }
    } finally { $archive.Dispose() }
    Write-Output $zipPath
}
catch {
    if ($zipPath -and (Test-Path -LiteralPath $zipPath)) { Remove-Item -LiteralPath $zipPath -Force }
    throw
}
finally {
    if (Test-Path -LiteralPath $staging) { Remove-Item -LiteralPath $staging -Recurse -Force }
}
