param(
    [string]$GameRoot = 'D:\owl\gameversion\worldbox-ab712b72f394',
    [string]$Assembly = '',
    [string]$Output = ''
)
$ErrorActionPreference = 'Stop'
$modRoot = Split-Path -Parent $PSScriptRoot
if (-not $Assembly) { $Assembly = Join-Path $modRoot 'build/heaven-grants/compile/MySimulatedLongevityRoad.dll' }
Add-Type -Path (Join-Path $GameRoot 'BepInEx/core/Mono.Cecil.dll')
$module = [Mono.Cecil.ModuleDefinition]::ReadModule([IO.Path]::GetFullPath($Assembly))
$native = [Mono.Cecil.ModuleDefinition]::ReadModule((Join-Path $GameRoot 'worldbox_Data/Managed/Assembly-CSharp.dll'))
$checks = [Collections.Generic.List[string]]::new()
function Assert-Wiring([bool]$Condition, [string]$Name) {
    if (-not $Condition) { throw "Wiring check failed: $Name" }
    $checks.Add($Name)
}
function Type-ByName([string]$Name) { $module.Types | Where-Object Name -eq $Name }
function Calls($Method, [string]$Name) {
    @($Method.Body.Instructions | Where-Object { $_.Operand -is [Mono.Cecil.MethodReference] -and $_.Operand.Name -eq $Name })
}
try {
    $registration = Type-ByName 'MclslTraitRegistration'
    $sync = $registration.Methods | Where-Object Name -eq 'SyncGiftTrait'
    Assert-Wiring ($sync.Parameters.Count -eq 4 -and $sync.Parameters[3].Constant -eq $true) 'existing aptitude callers retain default reconciliation'
    $action = Type-ByName 'MclslHeavenGrantActions'
    $perfection = $action.Methods | Where-Object Name -eq 'GrantPerfection'
    $syncCall = @(Calls $perfection 'SyncGiftTrait')
    Assert-Wiring ($syncCall.Count -eq 1 -and $syncCall[0].Previous.OpCode.Name -eq 'ldc.i4.0' -and $syncCall[0].Previous.Previous.OpCode.Name -eq 'ldc.i4.0') 'perfection disables both entry and identity reconciliation'
    $entry = $action.Methods | Where-Object Name -eq 'HandleAdded'
    Assert-Wiring (@($entry.Body.Instructions | Where-Object { $_.OpCode.Name -eq 'newobj' }).Count -eq 0) 'ordinary and consumed trait routing creates no closure'
    $book = $action.Methods | Where-Object Name -eq 'GrantDaoBook'
    Assert-Wiring ((Calls $book 'OnTechniqueAssigned').Count -eq 1 -and (Calls $book 'SetProgress').Count -eq 1) 'book uses production ceiling and progress entry points'
    Assert-Wiring ((Calls $book 'OnTechniqueChanged').Count -eq 0) 'book avoids duplicate index notification'
    $accessor = (Type-ByName 'MclslActorAccessor').Methods | Where-Object { $_.Name -eq 'Set' -and $_.Parameters.Count -eq 3 -and $_.Parameters[2].ParameterType.FullName -eq 'System.String' }
    Assert-Wiring ((Calls $accessor 'OnTechniqueChanged').Count -eq 1) 'production string accessor owns index notification'
    $router = (Type-ByName 'MclslTraitGrantRouter').Methods | Where-Object Name -eq 'HandleAddedTrait'
    $instructions = $router.Body.Instructions
    $actionCall = @(Calls $router 'HandleAdded') | Where-Object { $_.Operand.DeclaringType.Name -eq 'MclslHeavenGrantActions' }
    $normalize = @(Calls $router 'NormalizeTraitId')[0]
    Assert-Wiring ($actionCall.Count -eq 1 -and $instructions.IndexOf($actionCall[0]) -lt $instructions.IndexOf($normalize)) 'one action route precedes normal classification'
    $registerCalls = @($registration.Methods | ForEach-Object { if ($_.HasBody) { Calls $_ 'AddHeavenGrantAction' } })
    Assert-Wiring ($registerCalls.Count -eq 2) 'exactly two action registrations'
    $register = $registration.Methods | Where-Object Name -eq 'AddHeavenGrantAction'
    Assert-Wiring (@($register.Body.Instructions | Where-Object { $_.Operand -ceq 'trait/MclslTiandaoEdict' }).Count -eq 1) 'both actions share one icon path'
    $add = ($native.Types | Where-Object Name -eq 'Actor').Methods | Where-Object { $_.Name -eq 'addTrait' -and $_.Parameters[0].ParameterType.FullName -eq 'System.String' }
    $nested = @(Calls $add 'addTrait') | Where-Object { $_.Operand.Parameters[0].ParameterType.Name -eq 'ActorTrait' }
    Assert-Wiring ($nested.Count -eq 1) 'actual native string addition calls asset addition once'
    $limit = Type-ByName 'MclslTechniqueRealmLimit'
    $reward = $limit.Methods | Where-Object Name -eq 'TryAssignDiscoveredTechnique'
    Assert-Wiring ((Calls $reward 'OwnedTechniqueCeiling').Count -eq 1) 'discovered reward uses owned source ceiling'
    $adventure = Type-ByName 'MclslAdventureSystem'
    $grant = $adventure.Methods | Where-Object Name -eq 'GrantRuinReward'
    if (-not $grant) { $grant = $adventure.Methods | Where-Object { $_.HasBody -and (Calls $_ 'TryAssignDiscoveredTechnique').Count -gt 0 } }
    Assert-Wiring ((Calls $grant 'TryAssignDiscoveredTechnique').Count -eq 1) 'actual ruin reward goes through protected commit'
    $occupation = Type-ByName 'MclslTechniqueOccupationSystem'
    foreach ($name in @('AssignLeastCrowdedTechniqueForRealm','TryChangeToLessCrowdedTechnique')) {
        $method = $occupation.Methods | Where-Object Name -eq $name
        Assert-Wiring ((Calls $method 'OwnedTechniqueCeiling').Count -eq 1) ("automatic method selection protected: " + $name)
    }
    foreach ($name in @('MclslNewLawEntrySystem','MclslAncientLawEntrySystem')) {
        $count = @((Type-ByName $name).Methods | Where-Object HasBody | ForEach-Object { Calls $_ 'OwnedTechniqueCeiling' }).Count
        Assert-Wiring ($count -eq 1) ("initial entry protects pre-owned books: " + $name)
    }
    $mentorship = Type-ByName 'MclslAncientMentorshipSystem'
    Assert-Wiring (@($mentorship.Methods | Where-Object Name -eq 'AddClamped').Count -eq 0) 'mentorship has no duplicate clamped resource implementation'
    $result = @{ checks = $checks; count = $checks.Count; assembly = $Assembly; unityVerified = $false } | ConvertTo-Json -Depth 4
    if ($Output) { [IO.File]::WriteAllText([IO.Path]::GetFullPath($Output), $result, [Text.UTF8Encoding]::new($false)) }
    $result
}
finally { $module.Dispose(); $native.Dispose() }
