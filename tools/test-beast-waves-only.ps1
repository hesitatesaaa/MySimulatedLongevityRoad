$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$rules = Get-Content (Join-Path $root 'code/MySimulatedLongevityRoad/Data/MclslBeastRules.cs') -Raw
$rules = $rules.Replace('namespace MySimulatedLongevityRoad.Data;', 'namespace MySimulatedLongevityRoad.Data {') + "`n}"
$checks = @"
namespace MySimulatedLongevityRoad.Data {
    public static class WaveOnlyRegression {
        static void Check(bool value,string message) { if(!value)throw new System.Exception(message); }
        public static void Run() {
            Check(MclslBeastRules.PlannedCount(7,10,512,500,5)==7,"existing natural population and queued waves consume the limit");
            Check(MclslBeastRules.PlannedCount(7,10,512,512,0)==0,"population cap");
            Check(MclslBeastRules.PlannedCount(7,10,512,0,0,65536,64)==64,"single-wave cap");
            Check(MclslBeastRules.WeightedLevel(1,0)==1,"level unlock cap");
            Check(MclslBeastRules.Role(0,10)==2,"wave boss retained");
        }
    }
}
"@
Add-Type -TypeDefinition ($rules + $checks)
[MySimulatedLongevityRoad.Data.WaveOnlyRegression]::Run()
$spawn = Get-Content (Join-Path $root 'code/MySimulatedLongevityRoad/Systems/Beasts/MclslBeastSpawnSystem.cs') -Raw
if ($spawn -match 'TickAnnual|Pending|createNewUnit|\.Spawn\(') { throw 'Natural spawn execution remains' }
$lane = Get-Content (Join-Path $root 'code/MySimulatedLongevityRoad/Systems/Runtime/MclslAnnualWorldRuntimeLane.cs') -Raw
if ($lane -notmatch 'MclslBeastWorld.TickAnnual\(_activeYear\)') { throw 'Annual wave trigger missing' }
$world = Get-Content (Join-Path $root 'code/MySimulatedLongevityRoad/Systems/Beasts/MclslBeastWorld.cs') -Raw
if ($world -match 'NaturalPending|LastNaturalYear') { throw 'Old natural spawn queue can be restored' }
Write-Output 'PASS: wave caps, level and boss rules, annual trigger, no natural spawn or pending queue'