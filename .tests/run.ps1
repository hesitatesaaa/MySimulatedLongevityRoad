param(
    [string]$GameRoot = 'D:\owl\gameversion\worldbox-ab712b72f394',
    [string]$WorkDirectory = '',
    [string[]]$Projects = @()
)
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
if (-not $WorkDirectory) { $WorkDirectory = Join-Path $root 'build/tests' }
$work = [IO.Path]::GetFullPath($WorkDirectory)
if (-not $work.StartsWith($root + '\', [StringComparison]::OrdinalIgnoreCase)) { throw 'Test output must stay in the project workspace.' }
if (-not $Projects.Count) { $Projects = @((Get-Content (Join-Path $PSScriptRoot 'projects.json') -Raw | ConvertFrom-Json) | ForEach-Object { $_ }) }
if (-not $Projects.Count) { throw 'The formal test list is empty.' }
$names = @('DOTNET_CLI_HOME','DOTNET_SKIP_FIRST_TIME_EXPERIENCE','DOTNET_ADD_GLOBAL_TOOLS_TO_PATH',
    'DOTNET_GENERATE_ASPNET_CERTIFICATE','DOTNET_CLI_TELEMETRY_OPTOUT','APPDATA')
$previous = @{}
foreach ($name in $names) { $previous[$name] = [Environment]::GetEnvironmentVariable($name, 'Process') }
try {
    $env:DOTNET_CLI_HOME = Join-Path $work 'dotnet-home'
    $env:APPDATA = Join-Path $work 'test-appdata'
    $env:DOTNET_SKIP_FIRST_TIME_EXPERIENCE = '1'
    $env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = 'false'
    $env:DOTNET_GENERATE_ASPNET_CERTIFICATE = 'false'
    $env:DOTNET_CLI_TELEMETRY_OPTOUT = '1'
    New-Item -ItemType Directory -Force -Path $env:APPDATA | Out-Null
    foreach ($project in $Projects) {
        if ($project -notmatch '^[A-Za-z0-9]+$') { throw "Invalid test project: $project" }
        $file = Join-Path $PSScriptRoot "$project/$project.csproj"
        if (-not (Test-Path -LiteralPath $file)) { throw "Missing formal test project: $file" }
        & dotnet run --project $file --configuration Release '-p:UseAppHost=false' "-p:GameRoot=$GameRoot" `
            "-p:BaseIntermediateOutputPath=$work/obj/$project/" "-p:BaseOutputPath=$work/bin/$project/"
        if ($LASTEXITCODE -ne 0) { throw "Regression failed: $project (exit $LASTEXITCODE)" }
    }
}
finally {
    foreach ($name in $names) { [Environment]::SetEnvironmentVariable($name, $previous[$name], 'Process') }
}
