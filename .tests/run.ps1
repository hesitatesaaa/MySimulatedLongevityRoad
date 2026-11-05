param(
    [string]$GameRoot = 'D:\owl\gameversion\worldbox-ab712b72f394',
    [string]$WorkDirectory = '',   # reserved; bin/obj are gitignored
    [string[]]$Projects = @(),
    [ValidateSet('All', 'Portable', 'Game')][string]$Profile = 'All'
)
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$formal = @((Get-Content (Join-Path $PSScriptRoot 'projects.json') -Raw | ConvertFrom-Json))
if (-not $formal.Count) { throw 'The formal test list is empty.' }
if (@($formal | Select-Object -Unique).Count -ne $formal.Count) { throw 'Duplicate formal test project.' }
$gameDependencies = Get-Content (Join-Path $PSScriptRoot 'game-dependent-projects.json') -Raw | ConvertFrom-Json
$gameProjects = @($gameDependencies.PSObject.Properties.Name)
foreach ($name in $gameProjects) {
    if ($name -notin $formal) { throw "Game-dependent project is missing from the formal list: $name" }
}
if (-not $Projects.Count) { $Projects = $formal }
foreach ($name in $Projects) {
    if ($name -notin $formal) { throw "Test project is not registered in projects.json: $name" }
}
$selected = @()
foreach ($name in $Projects) {
    $requiresGame = $name -in $gameProjects
    if ($Profile -eq 'Portable' -and $requiresGame) {
        Write-Output "SKIPPED (requires game install): $name - $($gameDependencies.$name)"
    } elseif ($Profile -eq 'Game' -and -not $requiresGame) {
        Write-Output "SKIPPED (portable project, use -Profile Portable or All): $name"
    } else { $selected += $name }
}
$Projects = $selected
if (-not $Projects.Count) { throw "No test projects selected for profile: $Profile" }
Write-Output "Profile=$Profile projects=$($Projects.Count) GameRoot=$GameRoot"
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
