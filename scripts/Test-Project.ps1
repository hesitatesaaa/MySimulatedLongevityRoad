[CmdletBinding()]
param(
    [Parameter(Mandatory = $false)]
    [ValidateSet('Static', 'Build', 'Package')]
    [string]$Mode = 'Static',

    [Parameter(Mandatory = $false)]
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Release',

    [Parameter(Mandatory = $false)]
    [string]$WorldBoxDataRoot,

    [Parameter(Mandatory = $false)]
    [switch]$NoRestore,

    [Parameter(Mandatory = $false)]
    [ValidatePattern('^[\p{IsCJKUnifiedIdeographs}]{2,5}$')]
    [string]$ChangeTag = '开发验证',

    [Parameter(Mandatory = $false)]
    [switch]$VersionUpdate
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$repoRoot = Split-Path -Parent $PSScriptRoot
$projectPath = Join-Path $repoRoot 'InterestingTrait.csproj'
$buildScript = Join-Path $PSScriptRoot 'Build-Mod.ps1'
$packageScript = Join-Path $PSScriptRoot 'Package-Mod.ps1'

function Invoke-CheckedScript {
    param(
        [Parameter(Mandatory = $true)][string]$Path,
        [Parameter(Mandatory = $false)][hashtable]$Parameters = @{}
    )

    & $Path @Parameters
    if ($LASTEXITCODE -ne 0) {
        throw "脚本执行失败（$LASTEXITCODE）：$Path"
    }
}

function Test-JsonFiles {
    $jsonFiles = Get-ChildItem -LiteralPath $repoRoot -Recurse -File -Filter '*.json' | Where-Object {
        $relativePath = $_.FullName.Substring($repoRoot.Length + 1)
        $relativePath -notmatch '(?:^|\\)(?:_archive|bin|obj|release|发布包|\.codegraph|\.codex|DeveloperTools|references)(?:\\|$)'
    }

    foreach ($file in $jsonFiles) {
        Get-Content -LiteralPath $file.FullName -Raw | ConvertFrom-Json | Out-Null
    }

    $chPath = Join-Path $repoRoot 'Locales\ch.json'
    $czPath = Join-Path $repoRoot 'Locales\cz.json'
    $ch = Get-Content -LiteralPath $chPath -Raw | ConvertFrom-Json
    $cz = Get-Content -LiteralPath $czPath -Raw | ConvertFrom-Json
    $chKeys = @($ch.PSObject.Properties.Name)
    $czKeys = @($cz.PSObject.Properties.Name)
    $missingInCz = @($chKeys | Where-Object { $_ -notin $czKeys })
    $missingInCh = @($czKeys | Where-Object { $_ -notin $chKeys })

    if ($missingInCz.Count -gt 0 -or $missingInCh.Count -gt 0) {
        throw "本地化键不一致：ch.json 缺少 $($missingInCh.Count) 个，cz.json 缺少 $($missingInCz.Count) 个。"
    }

    Write-Host "JSON 检查通过：$($jsonFiles.Count) 个文件；本地化键：$($chKeys.Count) 个。"
}

function Test-VisibleLocalizationKeys {
    $ch = Get-Content -LiteralPath (Join-Path $repoRoot 'Locales\ch.json') -Raw | ConvertFrom-Json -AsHashtable
    $cz = Get-Content -LiteralPath (Join-Path $repoRoot 'Locales\cz.json') -Raw | ConvertFrom-Json -AsHashtable
    $config = Get-Content -LiteralPath (Join-Path $repoRoot 'default_config.json') -Raw | ConvertFrom-Json
    $required = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::Ordinal)
    foreach ($item in $config.ConfigItems) {
        [void]$required.Add($item.Id)
        [void]$required.Add($item.Id + ' Description')
    }
    foreach ($key in @('trait_group_MclslPhysiques', 'mclsl_aptitude_description',
        'mclsl_spell_insight_description', 'MclslImmortalFate', 'MclslAptitude',
        'MclslSpellInsight', 'MclslMindState', 'MclslMortalMiasma',
        'MclslTrueEssence', 'MclslContribution', 'MclslSpiritStones')) {
        [void]$required.Add($key)
    }
    $uiRoot = Join-Path $repoRoot 'code\MySimulatedLongevityRoad\UI'
    foreach ($file in Get-ChildItem -LiteralPath $uiRoot -Recurse -File -Filter '*.cs') {
        $source = Get-Content -LiteralPath $file.FullName -Raw
        foreach ($match in [regex]::Matches($source,
            '(?:LM\.Get|LocalizedTextManager\.getText)\("([^"\)]+)"\)')) {
            $key = $match.Groups[1].Value
            if ($key -match '^(?:mclsl_|MCLSL_|trait_group_)') { [void]$required.Add($key) }
        }
    }
    foreach ($key in $required) {
        if (-not $ch.ContainsKey($key) -or -not $cz.ContainsKey($key)) {
            throw "玩家可见文本缺少本地化键：$key"
        }
    }
    $overviewSource = Get-Content -LiteralPath (Join-Path $uiRoot 'MclslActorOverviewStatsFormatter.cs') -Raw
    if ($overviewSource -match 'tip\.textOnClick\s*=\s*icon\.DisplayName' -or
        $overviewSource -match 'tip\.textOnClickDescription\s*=\s*LM\.Get') {
        throw '人物属性提示将显示文字再次作为本地化键查询。'
    }
    Write-Host "玩家可见 key 检查通过：$($required.Count) 项。"
}

function Test-CompileBoundary {
    $dotnet = Get-Command dotnet -ErrorAction SilentlyContinue
    if ($null -eq $dotnet) {
        throw '静态检查需要 dotnet SDK。'
    }

    $output = & $dotnet.Source msbuild $projectPath -getItem:Compile -nologo -p:_EnableDefaultWindowsPlatform=false 2>&1 | Out-String
    $exitCode = $LASTEXITCODE
    if ($exitCode -ne 0) {
        throw "无法读取项目编译项：$output"
    }
    if ($output -match 'DeveloperTools\\|references\\|release\\|发布包\\|WorldBoxGameDecompiled') {
        throw '项目编译项包含被排除的 DeveloperTools、references 或发布文件。'
    }
    if ($output -notmatch 'InterestingTrait.cs') {
        throw '项目编译项缺少 InterestingTrait.cs。'
    }

    Write-Host '编译边界检查通过。'
}

function Test-RegressionSafety {
    $dotnet = Get-Command dotnet -ErrorAction SilentlyContinue
    if ($null -eq $dotnet) {
        throw '回归安全测试需要 dotnet SDK。'
    }

    $testProject = Join-Path $repoRoot 'tests\RegressionSafety\RegressionSafety.csproj'
    $nugetConfig = Join-Path $repoRoot 'tests\RegressionSafety\NuGet.Config'
    $previousAppData = $env:APPDATA
    try {
        $env:APPDATA = Join-Path $repoRoot 'obj\RegressionSafetyAppData'
        New-Item -ItemType Directory -Force -Path (Join-Path $env:APPDATA 'NuGet') | Out-Null

        $restoreOutput = & $dotnet.Source restore $testProject --configfile $nugetConfig --verbosity quiet 2>&1 | Out-String
        $restoreExitCode = $LASTEXITCODE
        if ($restoreExitCode -ne 0) {
            throw "回归安全测试依赖还原失败（$restoreExitCode）：$restoreOutput"
        }

        $output = & $dotnet.Source run --project $testProject --configuration Release --no-restore 2>&1 | Out-String
        $exitCode = $LASTEXITCODE
        if ($exitCode -ne 0) {
            throw "回归安全测试失败（$exitCode）：$output"
        }

        Write-Host $output.TrimEnd()
        $cultivationProject = Join-Path $repoRoot 'tests\FirstCultivation\FirstCultivation.csproj'
        $cultivationRestore = & $dotnet.Source restore $cultivationProject --configfile $nugetConfig --verbosity quiet 2>&1 | Out-String
        if ($LASTEXITCODE -ne 0) { throw "首次修行测试还原失败：$cultivationRestore" }
        $cultivationOutput = & $dotnet.Source run --project $cultivationProject --configuration Release --no-restore 2>&1 | Out-String
        if ($LASTEXITCODE -ne 0) { throw "首次修行测试失败：$cultivationOutput" }
        Write-Host $cultivationOutput.TrimEnd()
        Write-Host '回归安全测试通过。'
    }
    finally {
        $env:APPDATA = $previousAppData
    }
}

function Test-GitDiff {
    $gitRoot = $repoRoot
    while (-not (Test-Path -LiteralPath (Join-Path $gitRoot '.git'))) {
        $parent = Split-Path -Parent $gitRoot
        if ([string]::IsNullOrWhiteSpace($parent) -or $parent -eq $gitRoot) {
            Write-Host '当前目录没有 Git 仓库，跳过 Git 空白检查。'
            return
        }
        $gitRoot = $parent
    }
    $safeGitRoot = $gitRoot.Replace('\', '/')
    & git -c "safe.directory=$safeGitRoot" -C $repoRoot diff --check -- .
    if ($LASTEXITCODE -ne 0) {
        throw 'git diff --check 失败。'
    }

    Write-Host 'Git 空白检查通过。'
}

function Test-SourceWhitespace {
    $sourceFiles = @((Join-Path $repoRoot 'InterestingTrait.cs'), (Join-Path $repoRoot 'InterestingTrait.csproj'))
    foreach ($directory in @('code', 'scripts', 'tests')) {
        $sourceFiles += Get-ChildItem -LiteralPath (Join-Path $repoRoot $directory) -Recurse -File |
            Where-Object { $_.Extension -in @('.cs', '.csproj', '.ps1') -and
                $_.FullName -notmatch '[\\/](?:bin|obj|DeveloperTools|references)[\\/]' } |
            ForEach-Object { $_.FullName }
    }
    foreach ($path in $sourceFiles) {
        if ([regex]::IsMatch((Get-Content -LiteralPath $path -Raw), '(?m)[ \t]+(?=\r?$)')) {
            throw "源码存在行尾空白：$path"
        }
    }
    Write-Host "源码空白检查通过：$($sourceFiles.Count) 个文件。"
}

function Test-FeatureAssets {
    Add-Type -AssemblyName System.Drawing
    $spellRoot = Join-Path $repoRoot 'GameResources\effects\Spells'
    $atlases = @(Get-ChildItem -LiteralPath $spellRoot -File -Filter 'S*.png')
    if ($atlases.Count -ne 26 -or @(Get-ChildItem -LiteralPath $spellRoot -Directory).Count -ne 0) {
        throw '法术资源必须是26张图集，不能混入逐帧目录。'
    }
    for ($number = 1; $number -le 26; $number++) {
        $id = 'S{0:D3}' -f $number
        $path = Join-Path $spellRoot ($id + '.png')
        if (-not (Test-Path -LiteralPath $path)) { throw "法术图集缺失：$id" }
        $rows = if ($number -in 8, 9, 10, 17, 18, 19, 20) { 5 }
            elseif ($number -in 11, 12, 21, 22, 23, 24, 25, 26) { 6 }
            else { 4 }
        $atlas = [System.Drawing.Bitmap]::FromFile($path)
        try {
            if ($atlas.Width -ne 768 -or $atlas.Height -ne (192 * $rows)) {
                throw "法术图集尺寸不匹配：$id"
            }
        }
        finally { $atlas.Dispose() }
    }
    $traitRoot = Join-Path $repoRoot 'GameResources\trait'
    $icons = @(Get-ChildItem -LiteralPath $traitRoot -File -Filter 'MclslPhysique*.png')
    if ($icons.Count -ne 17) { throw "玄黄异禀图标数量应为17，实为 $($icons.Count)。" }
    $physiqueSource = Get-Content -LiteralPath (Join-Path $repoRoot 'code\MySimulatedLongevityRoad\Traits\MclslPhysiqueSystem.cs') -Raw
    $registeredIds = @([regex]::Matches($physiqueSource, 'new\("(MclslPhysique[A-Za-z]+)"') |
        ForEach-Object { $_.Groups[1].Value })
    if ($registeredIds.Count -ne 17 -or @($registeredIds | Sort-Object -Unique).Count -ne 17) {
        throw '体质注册 ID 缺失或重复。'
    }
    foreach ($id in $registeredIds) {
        if (-not (Test-Path -LiteralPath (Join-Path $traitRoot ($id + '.png')))) {
            throw "体质缺少独立贴图：$id"
        }
    }
    $hashes = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::Ordinal)
    foreach ($icon in $icons) {
        $bitmap = [System.Drawing.Bitmap]::FromFile($icon.FullName)
        try {
            if ($bitmap.Width -ne 128 -or $bitmap.Height -ne 128 -or $bitmap.GetPixel(0, 0).A -ne 0) {
                throw "体质图标须为128×128透明贴图：$($icon.Name)"
            }
        }
        finally { $bitmap.Dispose() }
        $hash = (Get-FileHash -LiteralPath $icon.FullName -Algorithm SHA256).Hash
        if (-not $hashes.Add($hash)) { throw "体质图标内容重复：$($icon.Name)" }
    }
    $aptitude = Join-Path $repoRoot 'GameResources\ui\Icons\Aptitude.png'
    if (-not (Test-Path -LiteralPath $aptitude)) { throw '资质独立图标缺失。' }
    $config = Get-Content -LiteralPath (Join-Path $repoRoot 'default_config.json') -Raw | ConvertFrom-Json
    $ids = @($config.ConfigItems | ForEach-Object { $_.Id })
    if ($ids.Count -eq 0 -or @($ids | Sort-Object -Unique).Count -ne $ids.Count) {
        throw '默认设置为空或含重复 ID。'
    }
    foreach ($item in $config.ConfigItems) {
        $field = switch ($item.Type) {
            'SWITCH' { 'BoolVal' }
            'INT_SLIDER' { 'IntVal' }
            'SELECT' { 'IntVal' }
            'SLIDER' { 'FloatVal' }
            'TEXT' { 'TextVal' }
            default { throw "未知设置类型：$($item.Type)" }
        }
        if ($null -eq $item.$field) { throw "默认设置缺少值：$($item.Id)" }
    }
    $atlasSource = Get-Content -LiteralPath (Join-Path $repoRoot 'code\MySimulatedLongevityRoad\UI\MclslCodexWindow.Atlas.cs') -Raw
    $factionColors = @('#167F69','#2457A6','#9A6214','#66358C','#176F8F','#A34724',
        '#57636F','#3F7826','#963365','#765226','#9C8218','#8F2834')
    foreach ($color in $factionColors) {
        if (-not $atlasSource.Contains('AtlasColor("' + $color + '")')) {
            throw "势力颜色缺失：$color"
        }
    }
    Write-Host "功能资源检查通过：26 张法术图集、17 张独立体质贴图、资质贴图、12 种势力颜色、$($ids.Count) 项默认设置。"
}

function Test-BeastAssets {
    $audit = Join-Path $repoRoot 'scripts\Audit-Beast-Sprites.py'
    & python $audit
    if ($LASTEXITCODE -ne 0) { throw '妖兽与具名人物动画检查失败。' }
}

Write-Host "项目检查模式：$Mode"
if ($VersionUpdate -and $Mode -ne 'Package') {
    throw '-VersionUpdate 仅用于 Package 模式。'
}
Test-JsonFiles
Test-VisibleLocalizationKeys
Test-CompileBoundary
Test-GitDiff
Test-SourceWhitespace
Test-FeatureAssets
Test-BeastAssets
Test-RegressionSafety

if ($Mode -eq 'Build' -or $Mode -eq 'Package') {
    if ($Mode -eq 'Build') {
        $buildParameters = @{
            Configuration = $Configuration
        }
        if ($PSBoundParameters.ContainsKey('WorldBoxDataRoot')) {
            $buildParameters.WorldBoxDataRoot = $WorldBoxDataRoot
        }
        if ($NoRestore) {
            $buildParameters.NoRestore = $true
        }
        Invoke-CheckedScript -Path $buildScript -Parameters $buildParameters
        $bindingParameters = @{ Configuration = $Configuration }
        if ($PSBoundParameters.ContainsKey('WorldBoxDataRoot')) {
            $bindingParameters.WorldBoxDataRoot = $WorldBoxDataRoot
        }
        Invoke-CheckedScript -Path (Join-Path $PSScriptRoot 'Test-NativeTimeBinding.ps1') -Parameters $bindingParameters
    }

    if ($Mode -eq 'Package') {
        Invoke-CheckedScript -Path $packageScript -Parameters @{ ChangeTag = $ChangeTag; VersionUpdate = $VersionUpdate }
    }
}

Write-Host "项目检查完成：$Mode"
