param(
    [Parameter(Mandatory=$true)][string]$BaselineArchive,
    [string]$GameRoot='D:\owl\gameversion\worldbox-ab712b72f394',
    [string]$WorkDirectory='',
    [ValidateRange(5,100)][int]$Pairs=5
)
$ErrorActionPreference='Stop'
$root=Split-Path -Parent $PSScriptRoot
if(-not $WorkDirectory){$WorkDirectory=Join-Path $root 'build/offline-comparison'}
$work=[IO.Path]::GetFullPath($WorkDirectory)
if(-not $work.StartsWith($root+'\',[StringComparison]::OrdinalIgnoreCase)){throw 'Output must stay in this project workspace.'}
if(Test-Path -LiteralPath $work){throw 'Choose a fresh comparison output directory.'}
New-Item -ItemType Directory -Path $work -Force | Out-Null
$names=@('DOTNET_CLI_HOME','APPDATA','DOTNET_SKIP_FIRST_TIME_EXPERIENCE','DOTNET_ADD_GLOBAL_TOOLS_TO_PATH','DOTNET_GENERATE_ASPNET_CERTIFICATE','DOTNET_CLI_TELEMETRY_OPTOUT')
$previous=@{}
foreach($name in $names){$previous[$name]=[Environment]::GetEnvironmentVariable($name,'Process')}
try {
    $env:DOTNET_CLI_HOME=Join-Path $work 'dotnet-home';$env:APPDATA=Join-Path $work 'test-appdata'
    $env:DOTNET_SKIP_FIRST_TIME_EXPERIENCE='1';$env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH='false'
    $env:DOTNET_GENERATE_ASPNET_CERTIFICATE='false';$env:DOTNET_CLI_TELEMETRY_OPTOUT='1'
    New-Item -ItemType Directory -Path $env:APPDATA -Force | Out-Null
    Expand-Archive -LiteralPath $BaselineArchive -DestinationPath (Join-Path $work 'baseline')
    foreach($mode in @('baseline','current')) {
        $source=if($mode -eq 'baseline'){Join-Path $work 'baseline/code/MySimulatedLongevityRoad'}else{Join-Path $root 'code/MySimulatedLongevityRoad'}
        & dotnet build (Join-Path $PSScriptRoot 'RuntimeQualityBenchmarks') -c Release "-p:GameRoot=$GameRoot" `
            "-p:Production=$source" "-p:BaseIntermediateOutputPath=$work/obj/$mode/" "-p:BaseOutputPath=$work/bin/$mode/" *> (Join-Path $work "$mode-build.log")
        if($LASTEXITCODE -ne 0){throw "Benchmark build failed: $mode; see build log"}
    }
    $records=@()
    for($pair=1;$pair -le $Pairs;$pair++) {
        $order=if($pair % 2){@('baseline','current')}else{@('current','baseline')}
        foreach($mode in $order) {
            $json=& dotnet "$work/bin/$mode/Release/net10.0/RuntimeQualityBenchmarks.dll"
            if($LASTEXITCODE -ne 0){throw "Benchmark workload failed: $mode"}
            $sample=$json | ConvertFrom-Json
            $sample | Add-Member NoteProperty mode $mode
            $sample | Add-Member NoteProperty pair $pair
            $records+=$sample
        }
    }
    $metadata=[ordered]@{
        scope='CoreCLR microbenchmarks with substituted game boundaries; NOT Unity performance acceptance'
        baselineArchiveSha256=(Get-FileHash -LiteralPath $BaselineArchive -Algorithm SHA256).Hash
        sourceHashes=@(foreach($mode in @('baseline','current')) {
            $source=if($mode -eq 'baseline'){Join-Path $work 'baseline/code/MySimulatedLongevityRoad'}else{Join-Path $root 'code/MySimulatedLongevityRoad'}
            foreach($file in @('Data/MclslCombatCareerCatalog.cs','Systems/Cultivation/MclslSpellCasting.cs')) {
                @{mode=$mode;path=$file;sha256=(Get-FileHash -LiteralPath (Join-Path $source $file) -Algorithm SHA256).Hash}
            }
        })
        pairs=$Pairs;samples=$records
    }
    $metadata | ConvertTo-Json -Depth 6 | Set-Content (Join-Path $work 'results.json') -Encoding utf8
    foreach($pair in 1..$Pairs) {
        $before=$records | Where-Object { $_.pair -eq $pair -and $_.mode -eq 'baseline' }
        $after=$records | Where-Object { $_.pair -eq $pair -and $_.mode -eq 'current' }
        if($after.skillBytes -ne 0 -or $after.queueBytes -gt $before.queueBytes -or $after.allTickBytes -gt $before.allTickBytes){throw "Allocation regression in pair $pair"}
    }
    Write-Output "Offline comparison complete: $work/results.json (Unity acceptance remains pending)"
}
finally {
    foreach($name in $names){[Environment]::SetEnvironmentVariable($name,$previous[$name],'Process')}
}
