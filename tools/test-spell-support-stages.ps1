$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$policyPath = Join-Path $root 'code/MySimulatedLongevityRoad/Systems/Cultivation/MclslSpellSupportStage.cs'
$systemPath = Join-Path $root 'code/MySimulatedLongevityRoad/Systems/Cultivation/MclslSpellSystem.cs'

$source = Get-Content $policyPath -Raw
$source = $source.Replace('namespace MySimulatedLongevityRoad.Systems;', 'namespace MySimulatedLongevityRoad.Systems {') + "`n}"

$tests = @"
namespace MySimulatedLongevityRoad.Systems {
    public static class SpellSupportStageRegression {
        static void Check(bool ok,string message){ if(!ok) throw new System.Exception(message); }
        static void Near(float actual,float expected,string message){ if(System.Math.Abs(actual-expected)>0.0001f) throw new System.Exception(message+" expected "+expected+" got "+actual); }
        public static string Run(){
            string[] support = {"S002","S003","S005","S006","S011","S014","S018","S020","S024","S025"};
            string[] others = {"S001","S004","S007","S008","S009","S010","S012","S013","S015","S016","S017","S019","S021","S022","S023","S026"};
            foreach(string id in support) Check(MclslSpellSupportStage.Applies(id), id+" must be a staged support spell");
            foreach(string id in others) Check(!MclslSpellSupportStage.Applies(id), id+" must keep its legacy animation");
            Check(MclslSpellSupportStage.IsStage("S002","cast") && MclslSpellSupportStage.IsStage("S025","hit"), "support cast/hit must be staged");
            Check(!MclslSpellSupportStage.IsStage("S001","cast") && !MclslSpellSupportStage.IsStage("S002","travel") && !MclslSpellSupportStage.IsStage("S003",null), "only cast/hit of support spells are staged");
            Check(MclslSpellSupportStage.CastFrames==8 && MclslSpellSupportStage.HitFrames==24, "staged frame counts must be 8 and 24");
            Check(MclslSpellSupportStage.CastSeconds==0.24f && MclslSpellSupportStage.HitSeconds==0.72f, "cast must run 0.24s before the 0.72s hit");
            Near(MclslSpellSupportStage.ResolveDuration("S002","cast",8,0.24f,0f),0.24f,"support cast keeps 0.24s");
            Near(MclslSpellSupportStage.ResolveDuration("S025","hit",24,0.72f,0f),0.72f,"support hit keeps 0.72s");
            Near(MclslSpellSupportStage.ResolveDuration("S011","cast",8,0f,0f),0.24f,"support cast defaults to 0.24s");
            Near(MclslSpellSupportStage.ResolveDuration("S024","hit",24,0f,0f),0.72f,"support hit defaults to 0.72s");
            Near(MclslSpellSupportStage.ResolveDuration("S002","cast",8,0f,0f),0.24f,"support cast does not inherit the 1.5x slowdown");
            Near(MclslSpellSupportStage.ResolveDuration("S001","cast",8,0f,0.96f),System.Math.Max(0.96f,8f/24f)*1.5f,"other spells keep the legacy cast formula");
            Near(MclslSpellSupportStage.ResolveDuration("S007","hit",16,0f,0.72f),System.Math.Max(0.72f,16f/24f)*1.5f,"other spells keep the legacy hit formula");
            Check(MclslSpellSupportStage.ResolveDuration("S002","cast",8,0f,0f) < MclslSpellSupportStage.ResolveDuration("S001","cast",8,0f,0.96f), "support cast is shorter than legacy playback");
            return "PASS: stage ids/phases/frames, 0.24/0.72 timing, legacy formula untouched";
        }
    }
}
"@

Add-Type -TypeDefinition ($source + $tests)
[MySimulatedLongevityRoad.Systems.SpellSupportStageRegression]::Run()

# Source-linked wiring checks: the policy above must actually drive the cast
# stage, the delayed hit stage and the S025 healed-recipient loop.
$system = Get-Content $systemPath -Raw
$wiring = [ordered]@{
    'cast stage on CommitCast'      = 'MclslSpellSupportStage\.Applies\(spell\.Id\)\s*\?\s*MclslSpellEffect\.Show\(actor,actor,spell\.Id,phase:"cast"'
    'legacy cast path retained'     = 'target != null \|\| MclslSpellEffect\.Show\(actor,null,spell\.Id\)'
    'S025 collects healed actors'   = 'supportHitTargets = HealNearbyAllies\(actor, 0\.25f \* strength, 6\)'
    'hit shown per healed actor'    = 'foreach \(Actor recipient in supportHitTargets\)'
    'self hit follows caster'       = 'MclslSpellEffect\.Show\(actor, actor, spellId, phase: "hit"'
    'hit stage reads HitSeconds'    = 'duration: MclslSpellSupportStage\.HitSeconds, delay: MclslSpellSupportStage\.CastSeconds'
    'Show accepts a delay'          = 'string phase = null, float duration = 0f, float delay = 0f'
    'delayed hit stays hidden'      = 'displayNow < _born'
    'stage frame counts wired'      = 'MclslSpellSupportStage\.IsStage\(id, phase\)'
    'missing support stage no fallback' = 'MclslSpellSupportStage\.Applies\(id\) \? null : Get\(id,frame\)'
}
foreach ($entry in $wiring.GetEnumerator()) {
    if ($system -notmatch $entry.Value) { throw ('Wiring check failed: ' + $entry.Key) }
}
foreach ($id in @('S002','S003','S005','S006','S011','S014','S018','S020','S024','S025')) {
    if ($system -notmatch ('Spell\("' + $id + '"')) { throw ('Spell catalogue missing ' + $id) }
}
Write-Output 'PASS: cast-stage, delayed hit, S025 beneficiary and no-fallback wiring present'

# Execute the production heal loop with actor doubles, not a parallel model.
$first = $system.IndexOf('    private static List<Actor> HealNearbyAllies(')
$last = $system.IndexOf('    private static void CommitCast(', $first)
$healMethods = $system.Substring($first, $last-$first)
$healStub = @'
using System;
using System.Collections.Generic;
namespace SupportBeneficiaryRegression {
class Actor {
    public object current_tile = new object(), kingdom;
    public bool alive = true;
    public float health, maximum;
    public Actor(object group,float hp,float max=100f){kingdom=group;health=hp;maximum=max;}
    public float getHealth()=>health;
    public float getMaxHealth()=>maximum;
    public void restoreHealthPercent(float fraction){health=Math.Min(maximum,health+maximum*fraction);}
}
static class Finder {
    public static List<Actor> Units = new List<Actor>();
    public static List<Actor> getUnitsFromChunk(object tile,int a,float b,bool c)=>Units;
}
static class MclslActorAccessor {public static bool Alive(Actor actor)=>actor.alive;}
public static class ActualHealLoop {
'@
$healCases = @'
static void Check(bool ok,string message){if(!ok)throw new Exception(message);}
public static string Run(){
    object friends=new object(),enemy=new object();
    var caster=new Actor(friends,100);
    var other=new Actor(enemy,20); var full=new Actor(friends,100);
    var dead=new Actor(friends,10){alive=false}; var zero=new Actor(friends,0,0);
    var allies=new List<Actor>();
    for(int i=0;i<6;i++)allies.Add(new Actor(friends,50));
    Finder.Units=new List<Actor>{caster,other,full,dead,zero};
    Finder.Units.AddRange(allies);
    var recipients=HealNearbyAllies(caster,0.25f,6);
    Check(recipients.Count==5,"full caster counts toward original cap but receives no hit");
    for(int i=0;i<5;i++)Check(recipients[i]==allies[i]&&allies[i].health==75,"original order and amount");
    Check(allies[5].health==50&&other.health==20&&dead.health==10,"excluded units unchanged");
    caster=new Actor(friends,50){current_tile=null};
    recipients=HealNearbyAllies(caster,0.25f,6);
    Check(recipients.Count==1&&recipients[0]==caster&&caster.health==75,"tile-less caster");
    caster=new Actor(friends,100){current_tile=null};
    Check(HealNearbyAllies(caster,0.25f,6).Count==0,"no recipient without health increase");
    return "PASS: actual S025 heal loop, beneficiary order, six-unit cap, full/dead/enemy exclusions";
}
}
}
'@
Add-Type -TypeDefinition ($healStub + $healMethods + $healCases)
[SupportBeneficiaryRegression.ActualHealLoop]::Run()
