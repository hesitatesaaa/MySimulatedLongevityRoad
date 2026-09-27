[CmdletBinding()]
param(
    [Parameter(Mandatory = $false)]
    [string]$ReportPath
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$repoRoot = Split-Path -Parent $PSScriptRoot
$catalogPath = Join-Path $repoRoot 'code\MySimulatedLongevityRoad\Data\MclslItemCatalog.cs'
$economyPath = Join-Path $repoRoot 'code\MySimulatedLongevityRoad\Systems\Crafting\MclslItemEconomy.cs'
$usePath = Join-Path $repoRoot 'code\MySimulatedLongevityRoad\Systems\Crafting\MclslItemUseSystem.cs'
$consumablePolicyPath = Join-Path $repoRoot 'code\MySimulatedLongevityRoad\Systems\Crafting\MclslConsumableInventoryPolicy.cs'
$marketPath = Join-Path $repoRoot 'code\MySimulatedLongevityRoad\Systems\Crafting\MclslTianxuanMarket.cs'
$mentorshipPath = Join-Path $repoRoot 'code\MySimulatedLongevityRoad\Systems\Crafting\MclslMentorshipGiftSystem.cs'
$rainPath = Join-Path $repoRoot 'code\MySimulatedLongevityRoad\Systems\Crafting\MclslArtifactRain.cs'
$equipmentPath = Join-Path $repoRoot 'code\MySimulatedLongevityRoad\Systems\Crafting\MclslArtifactEquipmentSystem.cs'
$professionPolicyPath = Join-Path $repoRoot 'code\MySimulatedLongevityRoad\Systems\Crafting\MclslProfessionCraftingPolicy.cs'
$historyPolicyPath = Join-Path $repoRoot 'code\MySimulatedLongevityRoad\Systems\Crafting\MclslItemAcquisitionHistoryPolicy.cs'
$materialDiscoveryPath = Join-Path $repoRoot 'code\MySimulatedLongevityRoad\Systems\Crafting\MclslMaterialDiscovery.cs'
$worldRepositoryPath = Join-Path $repoRoot 'code\MySimulatedLongevityRoad\Systems\WorldResources\MclslWorldRunRepository.cs'
$ancientMentorshipPath = Join-Path $repoRoot 'code\MySimulatedLongevityRoad\Systems\Cultivation\MclslAncientMentorshipSystem.cs'
$immortalRegistrationPath = Join-Path $repoRoot 'code\MySimulatedLongevityRoad\Traits\MclslImmortalActorRegistration.cs'
$rainEditorPath = Join-Path $repoRoot 'code\MySimulatedLongevityRoad\UI\MclslArtifactRainEditor.cs'
$backpackPath = Join-Path $repoRoot 'code\MySimulatedLongevityRoad\UI\MclslBackpackTabSystem.cs'
$featurePath = Join-Path $repoRoot 'code\MySimulatedLongevityRoad\UI\MclslFeatureWindow.cs'
$localePath = Join-Path $repoRoot 'Locales\ch.json'

$sources = @{}
foreach ($path in @($catalogPath, $economyPath, $usePath, $consumablePolicyPath, $marketPath, $mentorshipPath,
        $rainPath, $equipmentPath, $professionPolicyPath, $historyPolicyPath, $materialDiscoveryPath,
        $worldRepositoryPath, $ancientMentorshipPath, $immortalRegistrationPath,
        $rainEditorPath, $backpackPath, $featurePath)) {
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) { throw "接入校验缺少源码：$path" }
    $sources[$path] = Get-Content -LiteralPath $path -Raw
}

$checks = [System.Collections.Generic.List[object]]::new()
function Add-Check {
    param([string]$Name, [bool]$Passed, [string]$Evidence)
    $checks.Add([pscustomobject]@{ name = $Name; passed = $Passed; evidence = $Evidence })
    if (-not $Passed) { throw "凡阶物品接入校验失败：$Name；$Evidence" }
}

function Test-SourcePattern {
    param([string]$Name, [string]$Path, [string]$Pattern, [string]$Evidence)
    Add-Check $Name ([regex]::IsMatch($sources[$Path], $Pattern,
        [System.Text.RegularExpressions.RegexOptions]::Singleline)) $Evidence
}

$expected = @(
    [pscustomobject]@{ id='D019'; category='Pill';     first='R07'; second='R08' },
    [pscustomobject]@{ id='D020'; category='Pill';     first='R07'; second='R09' },
    [pscustomobject]@{ id='F026'; category='Talisman'; first='R01'; second='R08' },
    [pscustomobject]@{ id='F027'; category='Talisman'; first='R01'; second='R09' },
    [pscustomobject]@{ id='B082'; category='Artifact'; first='R01'; second='R03' },
    [pscustomobject]@{ id='B083'; category='Artifact'; first='R01'; second='R02' }
)
$catalog = $sources[$catalogPath]
$locale = Get-Content -LiteralPath $localePath -Raw | ConvertFrom-Json
$hashOwners = @{}

foreach ($item in $expected) {
    $definitionPattern = 'new\("' + [regex]::Escape($item.id) + '",\s*"[^"]+",\s*"' +
        [regex]::Escape($item.category) + '",\s*0,.*?"' + [regex]::Escape($item.first) +
        '",\s*"' + [regex]::Escape($item.second) + '",'
    $matches = [regex]::Matches($catalog, $definitionPattern,
        [System.Text.RegularExpressions.RegexOptions]::Singleline)
    Add-Check "目录/$($item.id)" ($matches.Count -eq 1) "应唯一登记为 $($item.category) 凡阶，并使用 $($item.first)+$($item.second)。"

    $iconPath = Join-Path $repoRoot "GameResources\ui\Items\$($item.id).png"
    Add-Check "贴图/$($item.id)" (Test-Path -LiteralPath $iconPath -PathType Leaf) "应存在独立物品贴图 ui/Items/$($item.id)。"
    $hash = (Get-FileHash -LiteralPath $iconPath -Algorithm SHA256).Hash
    Add-Check "贴图非空/$($item.id)" ((Get-Item -LiteralPath $iconPath).Length -gt 0) "贴图文件不得为空。"
    Add-Check "贴图唯一/$($item.id)" (-not $hashOwners.ContainsKey($hash)) "六件凡阶成品的 SHA-256 内容必须互不重复。"
    $hashOwners[$hash] = $item.id

    Add-Check "本地化/$($item.id)" ($locale.PSObject.Properties.Name -contains $item.id -and
        $locale.PSObject.Properties.Name -contains "$($item.id) Description" -and
        -not [string]::IsNullOrWhiteSpace([string]$locale.$($item.id)) -and
        -not [string]::IsNullOrWhiteSpace([string]$locale."$($item.id) Description")) "名称与说明必须进入 ch.json。"
}

foreach ($id in @('R01','R02','R03','R07','R08','R09')) {
    Test-SourcePattern "原版材料映射/$id" $economyPath ('"' + $id + '"\s*=>\s*"[^"]+"') "$id 必须映射到 WorldBox 城镇资源。"
}
Test-SourcePattern '学徒配方隔离' $economyPath 'if\s*\(grade\s*==\s*0\).*?IsNativeIngredient\(recipe\.IngredientA\).*?IsNativeIngredient\(recipe\.IngredientB\)' 'Grade=0 只能接受白名单原版材料。'
Test-SourcePattern '高阶材料隔离' $economyPath 'material\s*!=\s*null\s*&&\s*material\.MaterialTier\s*!=\s*MclslMaterialTier\.None' '黄玄地天配方必须使用已登记且已定阶的模组材料。'
Test-SourcePattern '学徒可制作' $economyPath 'item\.Category\s*!=\s*category\s*\|\|\s*!MclslProfessionCraftingPolicy\.CanAttemptRecipe\(grade,\s*item\.Grade\).*?RecipeFitsGrade\(item,\s*item\.Grade\)' '职业制作候选必须包含当前 Grade=0 配方。'
Test-SourcePattern '凡阶丹药可使用' $usePath 'case\s+"D019".*?case\s+"D020"' '两种凡阶丹药必须接入实际使用分支。'
Test-SourcePattern '凡阶符箓可使用' $usePath 'Register\("F026".*?Register\("F027"' '两种凡阶符箓必须注册实际状态效果。'

Test-SourcePattern '乾坤袋展示' $backpackPath 'LoadItems\(bag,\s*"Pill".*?LoadItems\(bag,\s*"Talisman".*?LoadItems\(bag,\s*null' '乾坤袋必须展示丹药、符箓、材料。'
Test-SourcePattern '乾坤袋法宝展示' $backpackPath 'item\?\.Category\s*!=\s*"Artifact".*?item\.IconPath.*?Grade\(item\.Grade\)' '乾坤袋必须按目录贴图和品阶展示法宝。'
Test-SourcePattern '凡阶显示名称' $backpackPath '0\s*=>\s*"凡阶"' 'Grade=0 在乾坤袋中必须显示为凡阶。'
Test-SourcePattern '天玄镜挂单' $marketPath 'TryListSurplus.*?TryListArtifactSurplus.*?item\?\.Category\s*!=\s*"Artifact".*?AddListing' '天玄镜必须允许成品和独立法宝进入挂单链路。'
Test-SourcePattern '天玄镜交易' $marketPath 'TryBuy\(.*?MclslBagSystem\.Add\(bag,\s*item\.Id.*?MclslArtifactSystem\.OnArtifactAcquired' '成交后必须入袋，并刷新法宝装备状态。'
Test-SourcePattern '天玄镜展示' $featurePath 'DrawMarketListings.*?DrawIcon\(item.*?DisplayGradeName\(item\)' '天玄镜必须展示物品贴图和凡阶品阶。'
Test-SourcePattern '丹符逐种保留五个' $consumablePolicyPath 'PerItemReserveTarget\s*=\s*5.*?item\.Category\s+is\s+"Pill"\s+or\s+"Talisman"\s*&&\s*IsCurrentGradeUseful\(actor,\s*item\).*?return\s+PerItemReserveTarget' '每个符合当前境界的丹药、符箓 ID 必须分别保留 5 个。'
Add-Check '丹符储备不限五种' (-not [regex]::IsMatch($sources[$consumablePolicyPath], 'itemIds\.Count\s*>=\s*PerItemReserveTarget')) '储备计划不得把 5 误作最多五种不同丹药或符箓。'
Test-SourcePattern '丹符自动采购仍只补缺' $marketPath 'AddIfMissing.*?Count\(bag,\s*itemId\)\s*==\s*0.*?desired\.Add\(itemId\)' '自动采购保持库存为零时才购买 1 个，不按储备目标补到 5 个。'
Test-SourcePattern '修士年度购买上限不变' $marketPath 'MaxAnnualPurchases\s*=\s*6.*?PurchasesInYear\(actor,\s*year\)\s*>=\s*MaxAnnualPurchases' '每年六次限制只通过具体修士的购买计数执行。'

Test-SourcePattern '师徒赠送法宝' $mentorshipPath 'TryGiftArtifact.*?Category\s*==\s*"Artifact".*?LearnFromOwnedItem' '师徒赠礼必须传递法宝及其配方知识。'
Test-SourcePattern '师徒赠送丹符' $mentorshipPath 'TryGiftConsumable.*?Category\s+is\s+"Pill"\s+or\s+"Talisman".*?LearnFromOwnedItem' '师徒赠礼必须传递凡阶丹药/符箓及其配方知识。'
Test-SourcePattern '师徒传授配方' $mentorshipPath 'TryTeachRecipe.*?x\.Grade\s*<=\s*grade.*?MclslRecipeKnowledge\.Learn' '同职业师徒必须能传授不高于学徒品阶的配方。'

Test-SourcePattern '法宝雨候选' $rainPath 'ChoiceCache\s*=\s*MclslItemCatalog\.All\s*\.Where\(x\s*=>\s*x\.Category\s*==\s*"Artifact"' '法宝雨候选必须直接来自法宝目录，覆盖凡阶法宝。'
Test-SourcePattern '法宝雨编辑器' $rainEditorPath 'MclslArtifactRain\.Choices.*?MclslArtifactRain\.SetSelected' '编辑器必须列出候选并可修改选择。'
Test-SourcePattern '法宝雨实际释放' $rainPath 'DrawRain.*?drop_manager\.spawn.*?OnLanded.*?MclslBagSystem\.Add\(actor' '释放链必须生成落物并在落地后把所选法宝加入修士乾坤袋。'

Test-SourcePattern '制作候选最多降一阶' $professionPolicyPath 'Math\.Max\(0,\s*professionGrade\s*-\s*1\).*?recipeGrade\s*>=\s*minimum\s*&&\s*recipeGrade\s*<=\s*professionGrade' '职业配方只能使用当前阶或低一阶。'
Test-SourcePattern '仅同阶增加熟练度' $professionPolicyPath 'recipeGrade\s*==\s*professionGrade\s*\?\s*1\s*:\s*0' '同阶制作统一增加 1 点，降阶不增加。'
Test-SourcePattern '制作同阶优先' $economyPath 'for\s*\(int\s+recipeGrade\s*=\s*grade;\s*recipeGrade\s*>=\s*minimumGrade;\s*recipeGrade--\).*?HasPair\(actor,\s*bag' '制作选择必须先检查当前阶，再检查低一阶。'
Add-Check '重复法宝允许炼制' (-not [regex]::IsMatch($sources[$economyPath], 'Category\s*==\s*"Artifact"\s*&&\s*MclslBagSystem\.Count\(bag,\s*recipe\.Id\)\s*>\s*0')) '职业配方不得因已持有同 ID 法宝而跳过。'
Test-SourcePattern '重复法宝自动上架' $equipmentPath 'GroupBy\(x\s*=>\s*MclslItemCatalog\.Get\(x\.ItemId\)\.Id.*?duplicates\.Add\(duplicate\).*?TryListArtifactSurplus\(actor,\s*duplicate,\s*year\)' '重复法宝实例必须移交天玄镜挂单或待发布队列。'
Test-SourcePattern '每年最多制作一次' $economyPath 'ProfessionLastCraftYear,\s*-1\)\s*==\s*year.*?TryCraft\(actor,\s*profession,\s*year\).*?ProfessionLastCraftYear,\s*year' '三职业必须共用每年一次的成功制作门禁。'
Test-SourcePattern '成品历史品阶策略' $historyPolicyPath 'category\s+is\s+not\s+\("Pill"\s+or\s+"Talisman"\s+or\s+"Artifact"\).*?grade\s*>=\s*3\s*\|\|\s*lowGradeEnabled' '仅三职业成品进入历史，地天阶走总开关，凡黄玄阶还需低阶开关。'
Test-SourcePattern '材料历史品阶策略' $historyPolicyPath 'materialTier\s*<\s*1\s*\|\|\s*materialTier\s*>\s*4.*?materialTier\s*>=\s*3\s*\|\|\s*lowGradeEnabled' '材料必须沿用相同的总开关和高低阶分流。'
Test-SourcePattern '历史入口强制策略' $worldRepositoryPath 'AddItemAcquisitionEvent.*?ShouldRecordFinishedProduct' '所有成品获取事件必须在仓储入口统一执行品阶开关策略。'
Test-SourcePattern '材料发现统一策略' $materialDiscoveryPath 'ShouldRecordMaterial\(.*?ItemAcquisitionHistoryEnabled.*?RecordLowMaterialAcquisitionHistory' '材料发现必须使用统一历史策略。'
Test-SourcePattern '天玄镜获取历史' $marketPath 'RecordActivity\("Purchased".*?AddItemAcquisitionEvent\(.*?"天玄镜交易"' '成功成交后必须记录买方成品获取。'
Test-SourcePattern '师徒获取历史' $mentorshipPath 'TryGiftArtifact.*?AddItemAcquisitionEvent\(year,\s*student,\s*gift\.ItemId,\s*1,\s*"师徒赠予"\).*?TryGiftConsumable.*?AddItemAcquisitionEvent\(year,\s*student,\s*gift\.ItemId,\s*1,\s*"师徒赠予"\)' '法宝和丹符赠予成功后必须记录学生获取。'
Test-SourcePattern '旧法师徒获取历史' $ancientMentorshipPath 'MclslBagSystem\.Add\(student,\s*itemId.*?AddItemAcquisitionEvent\(year,\s*student,\s*itemId,\s*1,\s*"师徒赠予"\)' '旧法职业赠礼入袋后必须记录成品获取。'
Test-SourcePattern '法宝雨获取历史' $rainPath 'MclslBagSystem\.Add\(actor,\s*itemId.*?AddItemAcquisitionEvent\(year,\s*actor,\s*itemId,\s*1,\s*"法宝雨"\)' '法宝雨落地入袋后必须记录获取。'
Test-SourcePattern '仙人法宝获取历史' $immortalRegistrationPath 'MclslBagSystem\.Add\(actor,\s*artifactId.*?AddItemAcquisitionEvent\(.*?"仙道人物初始法宝"' '仙道人物初始法宝入袋后必须记录获取。'

$summary = [pscustomobject]@{
    generatedAt = (Get-Date).ToString('yyyy-MM-ddTHH:mm:ssK')
    itemCount = $expected.Count
    checkCount = $checks.Count
    passed = @($checks | Where-Object passed).Count
    failed = @($checks | Where-Object { -not $_.passed }).Count
    items = @($expected | ForEach-Object id)
    checks = $checks
}
if (-not [string]::IsNullOrWhiteSpace($ReportPath)) {
    $resolvedReport = if ([System.IO.Path]::IsPathRooted($ReportPath)) { $ReportPath } else { Join-Path $repoRoot $ReportPath }
    $reportDirectory = Split-Path -Parent $resolvedReport
    if (-not (Test-Path -LiteralPath $reportDirectory)) { New-Item -ItemType Directory -Path $reportDirectory -Force | Out-Null }
    $summary | ConvertTo-Json -Depth 6 | Set-Content -LiteralPath $resolvedReport -Encoding utf8
}

Write-Host "凡阶物品端到端接入校验通过：$($summary.itemCount) 件物品，$($summary.checkCount) 项检查。"
