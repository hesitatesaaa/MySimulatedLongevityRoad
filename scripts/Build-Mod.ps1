[CmdletBinding()]
param(
    [Parameter(Mandatory = $false)]
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Release',

    [Parameter(Mandatory = $false)]
    [string]$WorldBoxDataRoot,

    [Parameter(Mandatory = $false)]
    [switch]$NoRestore,

    [Parameter(Mandatory = $false)]
    [switch]$IncludeDeveloperTools
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$repoRoot = Split-Path -Parent $PSScriptRoot
$solutionPath = Join-Path $repoRoot 'InterestingTrait.sln'

function Resolve-ProjectPath {
    param([Parameter(Mandatory = $true)][string]$Path)

    if ([System.IO.Path]::IsPathRooted($Path)) {
        return [System.IO.Path]::GetFullPath($Path)
    }

    return [System.IO.Path]::GetFullPath((Join-Path $repoRoot $Path))
}

if ([string]::IsNullOrWhiteSpace($WorldBoxDataRoot)) {
    $WorldBoxDataRoot = '..\..\worldbox_Data'
}
$WorldBoxDataRoot = Resolve-ProjectPath -Path $WorldBoxDataRoot

$requiredFiles = @(
    'StreamingAssets\mods\NML\Assemblies\0Harmony.dll',
    'StreamingAssets\mods\NML\Assembly-CSharp-Publicized.dll',
    'Managed\Newtonsoft.Json.dll',
    'Managed\System.Net.Http.dll',
    'Managed\UnityEngine.dll',
    'Managed\UnityEngine.CoreModule.dll',
    'Managed\UnityEngine.IMGUIModule.dll',
    'Managed\UnityEngine.InputLegacyModule.dll',
    'Managed\UnityEngine.TextRenderingModule.dll',
    'Managed\UnityEngine.UI.dll',
    'Managed\UnityEngine.UIModule.dll'
)

$neoModLoaderRelativePath = if (Test-Path -LiteralPath (Join-Path $WorldBoxDataRoot 'StreamingAssets\mods\NML\NeoModLoader.dll')) {
    'StreamingAssets\mods\NML\NeoModLoader.dll'
} else {
    'StreamingAssets\mods\NeoModLoader.dll'
}
$requiredFiles += $neoModLoaderRelativePath

$missingFiles = @(
    foreach ($relativePath in $requiredFiles) {
        $path = Join-Path $WorldBoxDataRoot $relativePath
        if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
            $relativePath
        }
    }
)

if ($missingFiles.Count -gt 0) {
    Write-Error "无法构建：WorldBoxDataRoot 不完整：$WorldBoxDataRoot" -ErrorAction Continue
    foreach ($relativePath in $missingFiles) {
        Write-Error "缺少：$relativePath" -ErrorAction Continue
    }
    exit 2
}

$dotnet = Get-Command dotnet -ErrorAction SilentlyContinue
if ($null -eq $dotnet) {
    Write-Error '无法构建：找不到 dotnet SDK。' -ErrorAction Continue
    exit 2
}

if (-not (Test-Path -LiteralPath $solutionPath -PathType Leaf)) {
    Write-Error "找不到解决方案：$solutionPath" -ErrorAction Continue
    exit 2
}

Write-Host "构建配置：$Configuration"
Write-Host "WorldBoxDataRoot：$WorldBoxDataRoot"

$arguments = @(
    'build',
    $solutionPath,
    '--configuration',
    $Configuration,
    '--nologo',
    '--verbosity',
    'minimal',
    "-p:WorldBoxDataRoot=$WorldBoxDataRoot"
)

if ($NoRestore) {
    $arguments += '--no-restore'
}
if ($IncludeDeveloperTools) {
    $arguments += '-p:IncludeDeveloperTools=true'
}

& $dotnet.Source @arguments
exit $LASTEXITCODE
