param([Parameter(Mandatory=$true)][string]$RunDirectory)
$ErrorActionPreference='Stop'
$taskRoot=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../..'))
$taskRun=[IO.Path]::GetFullPath($RunDirectory)
if(-not $taskRun.StartsWith($taskRoot+[IO.Path]::DirectorySeparatorChar,[StringComparison]::OrdinalIgnoreCase)){throw 'Diagnostic checkout must remain inside this workspace'}
$taskStaging=Join-Path $taskRun 'staging'
$taskGameMod=Join-Path $taskRun 'game/mods/我的模拟长生路'
foreach($taskCheckout in @($taskStaging,$taskGameMod)){
    Copy-Item (Join-Path $PSScriptRoot 'MclslBeastSpellProbe.cs') (Join-Path $taskCheckout 'code/MySimulatedLongevityRoad/Systems/Beasts/MclslBeastSpellProbe.cs') -Force
    $taskWrapper='namespace MySimulatedLongevityRoad.Systems; internal static class MclslActorSizeProbe { internal static void Tick() { MclslBeastRuntimeProbe.Tick(); MclslBeastSpellProbe.Tick(); } }'
    [IO.File]::WriteAllText((Join-Path $taskCheckout 'code/MySimulatedLongevityRoad/Systems/Beasts/MclslActorSizeProbe.cs'),$taskWrapper,[Text.UTF8Encoding]::new($false))
    $taskSpellPath=Join-Path $taskCheckout 'code/MySimulatedLongevityRoad/Systems/Cultivation/MclslSpellSystem.cs'
    $taskSource=[IO.File]::ReadAllText((Join-Path $taskRoot 'code/MySimulatedLongevityRoad/Systems/Cultivation/MclslSpellSystem.cs'))
    $taskNeedle='try {target.getHit(damage,true,type,caster,false);}finally {_applyingSpellDamage=previous;}'
    if(-not $taskSource.Contains($taskNeedle)){throw 'Diagnostic damage observation anchor missing'}
    $taskReplacement='var receipt=MclslBeastSpellProbe.BeginHit(caster,target,damage); try {target.getHit(damage,true,type,caster,false);}finally {MclslBeastSpellProbe.EndHit(receipt,target);_applyingSpellDamage=previous;}'
    [IO.File]::WriteAllText($taskSpellPath,$taskSource.Replace($taskNeedle,$taskReplacement),[Text.UTF8Encoding]::new($false))
}
