param([string]$GameRoot='E:\wol\gameversion\0_51_2_0.51.2',[string]$OutputDirectory='')
$ErrorActionPreference='Stop'
$projectRoot=Split-Path -Parent $PSScriptRoot
if(-not $OutputDirectory){$OutputDirectory=Join-Path $projectRoot ('build\Beast-Development-Preview-'+(Get-Date -Format 'yyyyMMdd-HHmmss'))}
$previewPath=[IO.Path]::GetFullPath($OutputDirectory)
$workspacePrefix=[IO.Path]::GetFullPath($projectRoot).TrimEnd('\')+'\'
if(-not $previewPath.StartsWith($workspacePrefix,[StringComparison]::OrdinalIgnoreCase)){throw 'Preview must remain inside the workspace.'}
if(Test-Path -LiteralPath $previewPath){throw 'Choose a fresh output directory.'}
& python (Join-Path $PSScriptRoot 'validate-beast-overhaul.py')
if($LASTEXITCODE -ne 0){throw 'Invalid beast resources.'}
& (Join-Path $PSScriptRoot 'build-release.ps1') -GameRoot $GameRoot -OutputDirectory (Join-Path $previewPath 'compiler')
$package=Join-Path $previewPath 'MySimulatedLongevityRoad'
New-Item -ItemType Directory -Path $package | Out-Null
foreach($name in @('code','GameResources','NativeActorResources','Locales','resources','InterestingTrait.cs','default_config.json','mod.json','icon.png')){
    $source=Join-Path $projectRoot $name
    if(Test-Path -LiteralPath $source){Copy-Item -LiteralPath $source -Destination $package -Recurse}
}
Copy-Item -LiteralPath (Join-Path $previewPath 'compiler\build-record.json') -Destination $package
Copy-Item -LiteralPath (Join-Path $projectRoot 'output\beast-overhaul\resource-validation.json') -Destination (Join-Path $package 'beast-resource-validation.json')
Copy-Item -LiteralPath (Join-Path $projectRoot 'resources\beast-overhaul-implementation.md') -Destination (Join-Path $package 'PREVIEW-ONLY.md')
if(Test-Path -LiteralPath (Join-Path $projectRoot 'output/beast-overhaul/controlled-unity-summary.json')){
    Copy-Item -LiteralPath (Join-Path $projectRoot 'output/beast-overhaul/controlled-unity-summary.json') -Destination (Join-Path $package 'beast-controlled-unity-summary.json')
}
$archive=Join-Path $previewPath 'MySimulatedLongevityRoad-0.6.0-beast-development-preview.zip'
Compress-Archive -Path (Join-Path $package '*') -DestinationPath $archive
$resourceReport=Get-Content -LiteralPath (Join-Path $projectRoot 'output/beast-overhaul/resource-validation.json') -Raw | ConvertFrom-Json
$taskPackageType=if(@($resourceReport.missing).Count -eq 0){'development-preview-awaiting-runtime-validation'}else{'development-preview-awaiting-art-and-runtime-validation'}
$record=[ordered]@{packageType=$taskPackageType;targetVersion='0.6.0';declaredVersion=(Get-Content -LiteralPath (Join-Path $package 'mod.json') -Raw|ConvertFrom-Json).version;releaseReady=$false;missingResources=@($resourceReport.missing).Count;archive=$archive;sha256=(Get-FileHash -LiteralPath $archive -Algorithm SHA256).Hash;createdAtUtc=[DateTime]::UtcNow.ToString('o')}
$record|ConvertTo-Json|Set-Content -LiteralPath (Join-Path $previewPath 'preview-package-record.json') -Encoding utf8
Write-Output ($record|ConvertTo-Json)
