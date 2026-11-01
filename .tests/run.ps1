param(
    [string]$GameRoot = 'D:\owl\gameversion\worldbox-ab712b72f394',
    [string]$WorkDirectory = '',   # reserved; bin/obj are gitignored
    [string[]]$Projects = @()
)
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
if (-not $Projects.Count) { $Projects = @((Get-Content (Join-Path $PSScriptRoot 'projects.json') -Raw | ConvertFrom-Json)) }
if (-not $Projects.Count) { throw 'The formal test list is empty.' }
Write-Output "GameRoot=$GameRoot"
foreach ($project in $Projects) {
    if ($project -notmatch '^[A-Za-z0-9]+$') { throw "Invalid test project: $project" }
    $file = Join-Path $PSScriptRoot "$project/$project.csproj"
    if (-not (Test-Path -LiteralPath $file)) { throw "Missing formal test project: $file" }
    Write-Output "== $project =="
    # 2>&1 keeps native stderr from being surfaced as a terminating PowerShell error under -ErrorAction Stop.
    $out = & dotnet run --project $file --configuration Release '-p:UseAppHost=false' "-p:GameRoot=$GameRoot" 2>&1
    $code = $LASTEXITCODE
    $out | ForEach-Object { Write-Output $_ }
    if ($code -ne 0) { throw "Regression failed: $project (exit $code)" }
}
Write-Output "ALL TEST PROJECTS PASSED"
