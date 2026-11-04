param(
    [string]$GameRoot = 'D:\owl\gameversion\worldbox-ab712b72f394',
    [string]$OutputDirectory = '',
    [int]$MaxWarnings = 0
)
$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path -Parent $PSScriptRoot
if (-not $OutputDirectory) { $OutputDirectory = Join-Path $projectRoot 'build/_compile-check' }
$managedRoot = Join-Path $GameRoot 'worldbox_Data\Managed'
$loaderRoot = Join-Path $GameRoot 'worldbox_Data\StreamingAssets\mods'
$compilerSdk = (& dotnet --list-sdks | Select-Object -Last 1)
if (-not $compilerSdk) { throw 'A .NET SDK is required.' }
if ($compilerSdk -notmatch '^([^ ]+) \[(.+)\]$') { throw 'Cannot resolve installed SDK.' }
$compilerPath = Join-Path $Matches[2] "$($Matches[1])\Roslyn\bincore\csc.dll"
$loaderAssembly = Join-Path $loaderRoot 'NeoModLoader.dll'
$harmonyAssembly = Join-Path $loaderRoot 'NML\Assemblies\0Harmony.dll'
$gamePublicizedAssembly = Join-Path $loaderRoot 'NML\Assembly-CSharp-Publicized.dll'
foreach ($requiredPath in @($managedRoot, $compilerPath, $loaderAssembly, $harmonyAssembly, $gamePublicizedAssembly)) {
    if (-not (Test-Path -LiteralPath $requiredPath)) { throw "Missing compile dependency: $requiredPath" }
}
New-Item -ItemType Directory -Path $OutputDirectory -Force | Out-Null
$assemblyPath = Join-Path $OutputDirectory 'MySimulatedLongevityRoad.dll'
$argumentsFile = Join-Path $OutputDirectory 'compile.rsp'
$compilerArguments = @('/nostdlib+', '/target:library', '/langversion:latest', '/nullable:annotations',
    '/optimize+', '/debug:portable', '/deterministic+', '/utf8output', "/out:`"$assemblyPath`"")
$references = @(Get-ChildItem -LiteralPath $managedRoot -Filter '*.dll' |
    Where-Object Name -ne 'Assembly-CSharp.dll' | Sort-Object Name | ForEach-Object FullName)
$references += @($gamePublicizedAssembly, $loaderAssembly, $harmonyAssembly)
foreach ($assembly in $references) { $compilerArguments += "/reference:`"$assembly`"" }
$sources = @(Get-ChildItem -LiteralPath $projectRoot -Recurse -Filter '*.cs' |
    Where-Object { $_.FullName.Substring($projectRoot.Length) -notmatch '(^|[\\/])(build|obj|bin|\.archive|\.tests|\.dev-tools|\.codegraph)([\\/]|$)' } |
    Sort-Object FullName | ForEach-Object FullName)
if ($sources.Count -eq 0) { throw 'No C# sources found under the project root.' }
foreach ($source in $sources) { $compilerArguments += "`"$source`"" }
[IO.File]::WriteAllLines($argumentsFile, $compilerArguments, [Text.UTF8Encoding]::new($false))
& dotnet $compilerPath /noconfig "@$argumentsFile" 2>&1 | Tee-Object -FilePath (Join-Path $OutputDirectory 'compile.log')
$code = $LASTEXITCODE
Write-Output "sources=$($sources.Count) exit=$code"
$logPath = Join-Path $OutputDirectory 'compile.log'
if (Test-Path -LiteralPath $logPath) {
    $logText = [IO.File]::ReadAllText($logPath)
    $warnCount = ([regex]::Matches($logText, '(?im):\s*warning ')).Count
    Write-Output "warnings=$warnCount budget=$MaxWarnings"
    if ($code -eq 0 -and $warnCount -gt $MaxWarnings) {
        Write-Output "WARNING BUDGET EXCEEDED: $warnCount > $MaxWarnings"
        exit 2
    }
}
exit $code
