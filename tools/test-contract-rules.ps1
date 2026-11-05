$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$rulesPath = Join-Path $root 'code/MySimulatedLongevityRoad/Systems/Combat/MclslContractRules.cs'
$source = Get-Content $rulesPath -Raw -Encoding UTF8
# Convert file-scoped namespace to block-scoped so it can join the test unit.
$source = $source.Replace('namespace MySimulatedLongevityRoad.Systems.Combat;', 'namespace MySimulatedLongevityRoad.Systems.Combat {') + "`n}"

$tests = @"
namespace MySimulatedLongevityRoad.Systems.Combat {
    public static class ContractRulesRegression {
        static void Check(bool ok,string message){ if(!ok) throw new System.Exception(message); }
        static void Eq(int a,int b,string message){ if(a!=b) throw new System.Exception(message+" expected "+b+" got "+a); }
        static void Near(double a,double b,string message){ if(System.Math.Abs(a-b)>0.000001) throw new System.Exception(message+" expected "+b+" got "+a); }
        public static string Run(){
            // Legacy migration keeps the lowest-kind LIVE pet, and a same-kind dead entry must not win.
            Eq(MclslContractRules.SelectLegacyKeepIndex(new[]{2,0,3},new[]{true,true,true}),1,"smallest kind wins");
            Eq(MclslContractRules.SelectLegacyKeepIndex(new int[0],new bool[0]),-1,"no live pet");
            Eq(MclslContractRules.SelectLegacyKeepIndex(new[]{0,0,1},new[]{false,true,true}),1,"dead same-kind entry skipped");
            Eq(MclslContractRules.SelectLegacyKeepIndex(new[]{0,1,2},new[]{false,true,true}),1,"dead smallest kind skipped");
            // Growth: health/attack are ratios of owner, with floors and passive1 +20%.
            Eq(MclslContractRules.PetHealth(1000,0,false),350,"wolf health 35%");
            Eq(MclslContractRules.PetHealth(1000,0,true),420,"passive1 +20% health");
            Eq(MclslContractRules.PetHealth(10,1,false),20,"health floor 20");
            Eq(MclslContractRules.PetAttack(1000,3),80,"deer attack 8%");
            Eq(MclslContractRules.PetAttack(1,2),1,"attack floor 1");
            // Additive pet bonuses (no stacking), passive4 heal +20%.
            Near(MclslContractRules.PetDamageFactor(false,false,false,false),1.0,"no buff");
            Near(MclslContractRules.PetDamageFactor(true,true,true,false),1.70,"additive 0.15+0.20+0.35");
            Near(MclslContractRules.PetDamageFactor(false,false,true,true),1.50,"passive5 resonance 0.50");
            Near(MclslContractRules.CareerHealPercent(0.25,true),0.30,"passive4 heal +20%");
            // Death cooldown and time relocation (pause/speed/save-load).
            Near(MclslContractRules.NextSummonAt(120),150,"30s resummon");
            Near(MclslContractRules.ExtendCooldown(0,120),150,"end contract sets 30s");
            Near(MclslContractRules.ExtendCooldown(200,120),200,"later existing deadline kept");
            Near(MclslContractRules.ExtendCooldown(130,120),150,"earlier existing extended");
            Near(MclslContractRules.Remaining(150,120),30,"remaining 30");
            Near(MclslContractRules.Remaining(0,120),0,"no cooldown");
            Near(MclslContractRules.Remaining(100,120),0,"clamped non-negative");
            Near(MclslContractRules.Rebase(100,40,50),110,"rebased to new clock");
            Near(MclslContractRules.Rebase(0,40,50),0,"zero stays zero");
            Near(MclslContractRules.Rebase(30,40,50),50,"past deadline becomes now");
            // Auto priority: revival > resonance > break > infusion > sigil.
            Eq(MclslContractRules.SelectTier(true,true,true,true,true),4,"revive first");
            Eq(MclslContractRules.SelectTier(false,true,true,true,true),5,"resonance second");
            Eq(MclslContractRules.SelectTier(false,false,true,true,true),3,"break third");
            Eq(MclslContractRules.SelectTier(false,false,false,true,true),2,"infusion fourth");
            Eq(MclslContractRules.SelectTier(false,false,false,false,true),1,"sigil last");
            Eq(MclslContractRules.SelectTier(false,false,false,false,false),-1,"nothing available");
            // Deer: single target, lower health ratio within 6 tiles.
            Check(MclslContractRules.DeerHealsOwner(0.3,0.6,true),"owner lower ratio chosen");
            Check(!MclslContractRules.DeerHealsOwner(0.6,0.3,true),"pet lower ratio kept");
            Check(!MclslContractRules.DeerHealsOwner(0.3,0.6,false),"owner out of range");
            // Assist: cooldown advanced before the extra hit, so it cannot retrigger itself.
            Check(MclslContractRules.AssistReady(10,11,12,0),"assist fires once");
            Check(!MclslContractRules.AssistReady(10,11,12,18),"assist blocked by 6s cooldown");
            Check(MclslContractRules.AssistReady(10,11,18,18),"assist ready after cooldown");
            Check(!MclslContractRules.AssistReady(10,20,21,0),"outside 3s window");
            Check(!MclslContractRules.AssistReady(0,11,12,0),"single side cannot trigger");
            // Equal random kind across four kinds.
            int[] seen=new int[4];
            for(int i=0;i<400;i++){int k=MclslContractRules.RandomKind(i); Check(MclslContractRules.ValidKind(k),"random kind valid"); seen[k]++;}
            for(int k=0;k<4;k++) Check(seen[k]==100,"equal distribution kind "+k);
            return "PASS: single-pet migration(dead same-kind), death cooldown, rebase, growth, additive stats, priority, deer, no-loop assist";
        }
    }
}
"@

Add-Type -TypeDefinition ($source + $tests)
[MySimulatedLongevityRoad.Systems.Combat.ContractRulesRegression]::Run()

# Contract lifecycle wiring: API, death notification, single-pet maintenance present.
$contract = Get-Content (Join-Path $root 'code/MySimulatedLongevityRoad/Systems/Combat/MclslCareerContract.cs') -Raw
$death = Get-Content (Join-Path $root 'code/MySimulatedLongevityRoad/Systems/Death/MclslDeathEventRouter.cs') -Raw
$wiring = [ordered]@{
    'Contract API'        = 'internal static MclslContractInfo Contract\(Actor actor\)'
    'Choose API'          = 'internal static bool ChooseContract\(Actor owner, int kind\)'
    'info fields'         = 'internal Actor Owner, Pet;'
    'choose validated'    = 'state\.Career != 5 \|\| state\.ContractKind >= 0'
    'single bound tick'   = 'private static void TickSpirit\('
    'auto summon cost'    = 'SummonManaCost'
    '30s cooldown'        = 'MclslContractRules\.ExtendCooldown\('
    'pet scaling bypass'  = '_directPetDamage'
}
foreach ($entry in $wiring.GetEnumerator()) {
    if ($contract -notmatch $entry.Value) { throw ('Wiring check failed: ' + $entry.Key) }
}
if ($death -notmatch 'MclslCareerCombat\.OnActorDied') { throw 'Wiring check failed: death notified' }
Write-Output 'PASS: contract API/death/single-pet wiring present'
