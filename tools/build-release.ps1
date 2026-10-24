param(
    [string]$GameRoot = 'E:\wol\gameversion\0_51_2_0.51.2',
    [string]$OutputDirectory = '',
    [switch]$RequireFinalAssets,
    [switch]$LocalDevelopment,
    [string]$PythonExecutable = 'python'
)
$ErrorActionPreference = 'Stop'
if ($LocalDevelopment -and $RequireFinalAssets) { throw 'Choose local development or formal release validation, not both.' }
$projectRoot = Split-Path -Parent $PSScriptRoot
& $PythonExecutable (Join-Path $PSScriptRoot 'check-localization.py')
if ($LASTEXITCODE -ne 0) { throw 'Localization display-path validation failed.' }
$declaredVersion = (Get-Content (Join-Path $projectRoot 'mod.json') -Raw -Encoding UTF8 | ConvertFrom-Json).version
if ($LocalDevelopment) {
    & $PythonExecutable (Join-Path $PSScriptRoot 'validate-current-assets.py')
    if ($LASTEXITCODE -ne 0) { throw 'Current actor resource validation failed.' }
    & $PythonExecutable (Join-Path $PSScriptRoot 'validate-beast-overhaul.py')
    if ($LASTEXITCODE -ne 0) { throw 'Beast resource validation failed.' }
} elseif ($declaredVersion -eq '0.7.0') {
    & $PythonExecutable (Join-Path $PSScriptRoot 'validate-current-assets.py')
    if ($LASTEXITCODE -ne 0) { throw 'Current actor resource validation failed.' }
    & $PythonExecutable (Join-Path $PSScriptRoot 'validate-beast-overhaul.py')
    if ($LASTEXITCODE -ne 0) { throw 'Beast resource validation failed.' }
    & $PythonExecutable (Join-Path $PSScriptRoot 'validate-living-assets.py') --release
    if ($LASTEXITCODE -ne 0) { throw '0.7.0 release requires current Unity acceptance evidence.' }
} elseif ($RequireFinalAssets -or $declaredVersion -eq '0.6.0') {
    & $PythonExecutable (Join-Path $PSScriptRoot 'validate-beast-overhaul.py') --release
    if ($LASTEXITCODE -ne 0) { throw '0.6.0 beast overhaul release blocked: resources or actual Unity validation incomplete.' }
    & $PythonExecutable (Join-Path $PSScriptRoot 'validate-current-assets.py') --release
    if ($LASTEXITCODE -ne 0) { throw '0.6.0 release blocked: independent frames, icons or in-game validation are incomplete.' }
}
if ($LocalDevelopment -and $declaredVersion -eq '0.7.0') {
    & $PythonExecutable (Join-Path $PSScriptRoot 'validate-living-assets.py')
    if ($LASTEXITCODE -ne 0) { throw 'Living resource validation failed.' }
}
& $PythonExecutable (Join-Path $PSScriptRoot 'build-soul-frame-catalog.py') --check
if ($LASTEXITCODE -ne 0) { throw 'Soul frame catalog is stale.' }
$managedRoot = Join-Path $GameRoot 'worldbox_Data\Managed'
$loaderRoot = Join-Path $GameRoot 'worldbox_Data\StreamingAssets\mods'
if (-not $OutputDirectory) { $OutputDirectory = Join-Path $projectRoot 'build\Release' }
$compilerSdk = (& dotnet --list-sdks | Select-Object -Last 1)
if (-not $compilerSdk) { throw 'A .NET SDK is required.' }
if ($compilerSdk -notmatch '^([^ ]+) \[(.+)\]$') { throw 'Cannot resolve installed SDK.' }
$compilerPath = Join-Path $Matches[2] "$($Matches[1])\Roslyn\bincore\csc.dll"
$loaderAssembly = Join-Path $loaderRoot 'NeoModLoader.dll'
$harmonyAssembly = Join-Path $loaderRoot 'NML\Assemblies\0Harmony.dll'
$gamePublicizedAssembly = Join-Path $loaderRoot 'NML\Assembly-CSharp-Publicized.dll'
foreach ($requiredPath in @($managedRoot, $compilerPath, $loaderAssembly, $harmonyAssembly, $gamePublicizedAssembly)) {
    if (-not (Test-Path -LiteralPath $requiredPath)) { throw "Missing build dependency: $requiredPath" }
}
New-Item -ItemType Directory -Path $OutputDirectory -Force | Out-Null
$argumentsFile = Join-Path $OutputDirectory 'compile.rsp'
$assemblyPath = Join-Path $OutputDirectory 'MySimulatedLongevityRoad.dll'
$compilerArguments = @('/nostdlib+', '/target:library', '/langversion:latest', '/nullable:annotations',
    '/optimize+', '/debug:portable', '/deterministic+', '/utf8output',
    "/out:`"$assemblyPath`"")
$references = @(Get-ChildItem -LiteralPath $managedRoot -Filter '*.dll' | Where-Object Name -ne 'Assembly-CSharp.dll' | Sort-Object Name | ForEach-Object FullName)
$references += @($gamePublicizedAssembly, $loaderAssembly, $harmonyAssembly)
foreach ($assembly in $references) { $compilerArguments += "/reference:`"$assembly`"" }
$sourceSelection = & $PythonExecutable (Join-Path $PSScriptRoot 'check-nml-sources.py') --root $projectRoot --json
if ($LASTEXITCODE -ne 0) { throw 'NML would compile unexpected or missing C# files.' }
$sources = @(($sourceSelection | ConvertFrom-Json).sources)
foreach ($source in $sources) { $compilerArguments += "`"$source`"" }
[IO.File]::WriteAllLines($argumentsFile, $compilerArguments, [Text.UTF8Encoding]::new($false))
& dotnet $compilerPath /noconfig "@$argumentsFile" 2>&1 | Tee-Object -FilePath (Join-Path $OutputDirectory 'build.log')
if ($LASTEXITCODE -ne 0) { throw "Release compiler failed with exit code $LASTEXITCODE" }
$buildRecord = [ordered]@{
    configuration = 'Release'; version = (Get-Content (Join-Path $projectRoot 'mod.json') -Raw -Encoding UTF8 | ConvertFrom-Json).version
    gameRoot = $GameRoot; sources = $sources.Count; builtAtUtc = [DateTime]::UtcNow.ToString('o')
    assemblySha256 = (Get-FileHash -LiteralPath $assemblyPath -Algorithm SHA256).Hash
    references = @($references | ForEach-Object { [ordered]@{ path = $_; sha256 = (Get-FileHash -LiteralPath $_ -Algorithm SHA256).Hash } })
}
$buildRecord | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath (Join-Path $OutputDirectory 'build-record.json') -Encoding utf8
Write-Output "Release build succeeded: $assemblyPath"
