using MySimulatedLongevityRoad.Systems;
using MySimulatedLongevityRoad.Systems.Combat;
using MySimulatedLongevityRoad.Core;
using Newtonsoft.Json;

int checks=0;
void Check(bool ok,string label){if(!ok)throw new Exception("FAILED: "+label);checks++;}
Actor A(long id,int team=0)=>new(){Id=id,kingdom=new(){Team=team}};
void Reset()
{
    MclslSpiritPact.Clear();MclslBattleCombat.Clear();MclslSpellCasting.Clear();
    World.world=new();Config.paused=false;Finder.Nearby=Array.Empty<Actor>();
    MclslActorRegistry.Actors.Clear();MclslSpellSystem.Learned.Clear();MclslSpellProgression.Levels.Clear();
    MclslSpellSystem.Hits.Clear();MclslSpellSystem.DamageCallback=null;MclslSpellSystem.SelfCallback=null;MclslSpellSystem.SelfCalls=0;
    MclslDiagnostics.Errors.Clear();MclslSpellTerrain.Clear();MclslSpellTerrain.Mode=2;MapAction.Damaged.Clear();MapAction.Burns=0;MapAction.Safe=true;
    MclslBattleVisuals.VisibleFlag=false;MclslNativeActorFrames.Available=false;MclslSpellEffect.Enabled=true;
}
void Advance(double now){World.world.Time=now;MclslSpellCasting.Tick();}
(Actor,Actor) Pair()
{
    var a=A(1);var b=A(2,1);a.attack_target=b;b.attack_target=a;
    MclslActorRegistry.Actors[1]=a;MclslActorRegistry.Actors[2]=b;return(a,b);
}
for(int i=1;i<=26;i++)MclslSpellSystem.All["S"+i.ToString("D3")]=new(){Id="S"+i.ToString("D3"),Name="s"};
for(int i=1;i<=16;i++)MclslSpellSystem.All["C"+i.ToString("D3")]=new(){Id="C"+i.ToString("D3"),Name="c"};

Check(MclslBattleRules.TerrainRadius(2)==0&&MclslBattleRules.TerrainRadius(3)==2&&MclslBattleRules.TerrainRadius(6)==5,"realm terrain limits");
Check(Enumerable.Range(0,7).Select(MclslBattleRules.SpiritLimit).SequenceEqual(new[]{1,1,2,3,3,4,4}),"spirit caps");
Check(!MclslBattleRules.InsightEligible(19.99,2,2,true,false)&&MclslBattleRules.InsightEligible(20,2,3,true,false),"insight exact time and realm boundary");
Check(!MclslBattleRules.InsightEligible(30,2,4,true,false)&&!MclslBattleRules.InsightEligible(30,2,2,false,false)&&!MclslBattleRules.InsightEligible(30,2,2,true,true),"insight excludes mismatch friendly projections");
double start=0,used=0,protection=0;
Check(Math.Abs(MclslBattleRules.ControlDuration(.8,1,ref start,ref used,ref protection)-.8)<.0001,"first control");
Check(Math.Abs(MclslBattleRules.ControlDuration(.8,1.5,ref start,ref used,ref protection)-.4)<.0001,"chained control capped at 1.2");
Check(MclslBattleRules.ControlDuration(1,2,ref start,ref used,ref protection)==0,"protection downgrades control");
Check(MclslBattleRules.ControlDuration(.4,4.3,ref start,ref used,ref protection)>.39,"protection expires");

Reset();var(a,b)=Pair();
Check(MclslSpellCasting.Queue(a,b,MclslSpellSystem.All["S001"]),"queue projectile");
Check(!MclslSpellCasting.Queue(a,b,MclslSpellSystem.All["S004"]),"one action at a time");
Advance(.1);Check(MclslSpellSystem.Hits.Count==0,"windup does no damage");
Advance(.16);a.Alive=false;MclslActorRegistry.Actors.Remove(a.Id);Advance(1);
Check(MclslSpellSystem.Hits.Count==1,"released projectile survives source death and registry removal");
Advance(2);Check(!MclslSpellCasting.IsBusy(a),"recovery releases action");

Reset();(a,b)=Pair();MclslSpellCasting.Queue(a,b,MclslSpellSystem.All["S001"]);a.Alive=false;Advance(1);
Check(MclslSpellSystem.Hits.Count==0&&!MclslSpellCasting.IsBusy(a),"death during windup cancels");
Reset();(a,b)=Pair();MclslSpellCasting.Queue(a,b,MclslSpellSystem.All["S001"]);MclslSpellCasting.InterruptWindup(a);Advance(1);
Check(MclslSpellSystem.Hits.Count==0&&!MclslSpellCasting.IsBusy(a),"windup interruption cancels only pending action");
Reset();(a,b)=Pair();MclslSpellCasting.Queue(a,b,MclslSpellSystem.All["S001"]);Advance(.16);b.kingdom.Team=0;Advance(1);
Check(MclslSpellSystem.Hits.Count==0,"allegiance revalidated at impact");
Reset();(a,b)=Pair();MclslSpellCasting.Queue(a,b,MclslSpellSystem.All["S026"]);Advance(.5);a.Alive=false;Advance(1);
Check(MclslSpellSystem.Hits.Count==0,"channel stops on source death");

Reset();(a,b)=Pair();MclslSpellSystem.All["S010"].MaxTargets=2;var c=A(3,1);MclslActorRegistry.Actors[3]=c;Finder.Nearby=new[]{b,c};
MclslSpellCasting.Queue(a,b,MclslSpellSystem.All["S010"]);
string snapshot=null;
MclslSpellSystem.DamageCallback=(_,_,_)=>{if(snapshot==null){MclslSpellCasting.Save();snapshot=World.world.map_stats.custom_data.Values["mclsl.spells.pending.v1"];}};
Advance(2);Check(snapshot!=null,"save during native damage callback");
MclslSpellSystem.DamageCallback=null;MclslSpellSystem.Hits.Clear();MclslSpellCasting.Clear();World.world.map_stats.custom_data.set("mclsl.spells.pending.v1",snapshot);
MclslSpellCasting.Load();Advance(3);
Check(MclslSpellSystem.Hits.Count==1&&MclslSpellSystem.Hits[0].Target==3,"reload skips target already committed before callback");

Reset();(a,b)=Pair();MclslSpellCasting.Queue(a,a,MclslSpellSystem.All["S002"]);
MclslSpellSystem.SelfCallback=()=>{MclslSpellCasting.Save();snapshot=World.world.map_stats.custom_data.Values["mclsl.spells.pending.v1"];};
Advance(1);MclslSpellSystem.SelfCallback=null;MclslSpellCasting.Clear();World.world.map_stats.custom_data.set("mclsl.spells.pending.v1",snapshot);MclslSpellCasting.Load();Advance(2);
Check(MclslSpellSystem.SelfCalls==1,"self effect does not replay after callback save");

Reset();(a,b)=Pair();
foreach(var spell in new[]{new MclslPendingSpell{Spell="S001",Damage=float.NaN},new MclslPendingSpell{Spell="S001",Due=double.PositiveInfinity},new MclslPendingSpell{Spell="S001",Phase=4},new MclslPendingSpell{Spell="missing"}})
    Check(!MclslSpellCasting.Valid(spell),"corrupt pending record rejected");
World.world.map_stats.custom_data.set("mclsl.spells.pending.v1","[{\"Version\":2,\"Spell\":\"missing\"}]");MclslSpellCasting.Load();
Check(!MclslSpellCasting.Queue(a,b,MclslSpellSystem.All["S001"]),"corrupt load fails closed");

Reset();(a,b)=Pair();a.Realm="r0";
Check(MclslBattleProfessionSystem.CanLearn(a,"C001")&&!MclslBattleProfessionSystem.CanLearn(a,"C003"),"profession learning gates");
MclslSpellSystem.Learn(a,"C001");Check(MclslBattleProfessionSystem.Main(a)=="sword","first basic chooses main");
a.Realm="r4";MclslSpellSystem.Learn(a,"C005");Check(MclslBattleProfessionSystem.Main(a)=="sword"&&MclslBattleProfessionSystem.CanUse(a,"C005"),"cross-profession basic remains usable");
Check(!MclslBattleProfessionSystem.CanUse(a,"C007")&&MclslBattleProfessionSystem.CanUse(a,"C003"),"cross-profession cores sleep");
Check(!MclslBattleProfessionSystem.CanStudy(a,"C003"),"self study cannot manufacture unlearned core");
MclslSpellProgression.Levels["1|C001"]=10;MclslSpellProgression.Levels["1|C002"]=10;
Check(!MclslBattleProfessionSystem.TryBattleInsight(a,500)&&MclslBattleProfessionSystem.TryBattleInsight(a,499),"core insight exact 5 percent boundary");
MclslSpellProgression.Levels["1|C003"]=10;
Check(!MclslBattleProfessionSystem.TryBattleInsight(a,100)&&MclslBattleProfessionSystem.TryBattleInsight(a,99),"divine insight exact 1 percent boundary");
MclslBattleProfessionSystem.SetMain(a,"body");Check(MclslSpellSystem.Knows(a,"C004")&&!MclslBattleProfessionSystem.CanUse(a,"C004"),"switch retains learned divine but sleeps it");

Reset();(a,b)=Pair();MclslBattleProfessionSystem.SetMain(a,"pact");a.Realm="r6";MclslSpiritPact.Summon(a);MclslSpiritPact.Summon(a);
Check(MclslSpiritPact.Count(a)==4,"summon respects cap under repeated requests");
MclslSpiritPact.Guard(a);float damage=50;World.world.Time=.1;MclslSpiritPact.ApplyGuard(a,ref damage);
Check(damage==30,"guard consumes at most projection durability");
float second=50;MclslSpiritPact.ApplyGuard(a,ref second);Check(second==50,"one guard cannot absorb twice");
MclslSpiritPact.Save();MclslSpiritPact.Clear();MclslSpiritPact.Load();Check(MclslSpiritPact.Count(a)==3,"broken projection remains spent across save");
World.world.Time=31;MclslSpiritPact.Tick();Check(MclslSpiritPact.Count(a)==0,"projection lifetime uses simulated time");
Reset();(a,b)=Pair();MclslBattleProfessionSystem.SetMain(a,"sword");MclslSpiritPact.Summon(a);
Check(MclslSpiritPact.Count(a)>0,"basic summon works as secondary practice");
MclslBattleProfessionSystem.SetMain(a,"body");MclslSpiritPact.Tick();Check(MclslSpiritPact.Count(a)==0,"main switch clears projections");

Reset();(a,b)=Pair();MclslBattleCombat.ObserveHit(a,b,1);World.world.Time=21;MclslBattleCombat.ObserveHit(a,b,1);
a.attack_target=null;b.attack_target=null;MclslBattleProfessionSystem.SetMain(a,"sword");
MclslSpellProgression.Levels["1|C001"]=10;MclslSpellProgression.Levels["1|C002"]=10;
World.world.Time=24;MclslBattleCombat.Tick();MclslBattleCombat.Save();MclslBattleCombat.Load();World.world.Time=25;MclslBattleCombat.Tick();
Check(MclslActorAccessor.GetInt(a,"mclsl.battle.v1.encounters")==1,"battle insight evaluated once even across reload");

Reset();(a,b)=Pair();b.Realm="r0";MclslBattleCombat.ObserveHit(a,b,1);World.world.Time=25;b.Realm="r4";
MclslBattleCombat.ObserveHit(a,b,1);a.attack_target=null;b.attack_target=null;World.world.Time=28;MclslBattleCombat.Tick();
Check(MclslActorAccessor.GetInt(a,"mclsl.battle.v1.encounters")==0,"low realm fighting time cannot qualify a last-second equal-realm hit");
Reset();(a,b)=Pair();MclslBattleCombat.ProjectionDamage=true;MclslBattleCombat.ObserveHit(a,b,10);World.world.Time=25;MclslBattleCombat.ObserveHit(a,b,10);
MclslBattleCombat.ProjectionDamage=false;a.attack_target=null;b.attack_target=null;World.world.Time=28;MclslBattleCombat.Tick();
Check(MclslActorAccessor.GetInt(a,"mclsl.battle.v1.encounters")==0,"projection damage cannot manufacture battle insight");

Reset();var center=new WorldTile{pos=new(50,50)};
MclslSpellTerrain.Queue(center,"C008",2);for(int i=0;i<20;i++)MclslSpellTerrain.Tick();
Check(MapAction.Damaged.Count==0,"pre-nascent skills cannot destroy terrain");
MclslSpellTerrain.Mode=0;MclslSpellTerrain.Queue(center,"C008",6);for(int i=0;i<20;i++)MclslSpellTerrain.Tick();
Check(MapAction.Damaged.Count==0&&MapAction.Burns==0,"terrain disabled skips all changes");
MclslSpellTerrain.Mode=1;MclslSpellTerrain.Queue(center,"C008",3);for(int i=0;i<20;i++)MclslSpellTerrain.Tick();
Check(MapAction.Damaged.Count==0&&MapAction.Burns==13,"traces only never damages buildings or lowers tiles");
MclslSpellTerrain.Mode=2;MclslSpellTerrain.Queue(center,"C008",6);MclslSpellTerrain.Tick();
Check(MapAction.Damaged.Count<=8,"terrain per-frame work is bounded");
MclslSpellTerrain.Save();MclslSpellTerrain.Clear();MclslSpellTerrain.Load();for(int i=0;i<30;i++)MclslSpellTerrain.Tick();
Check(MapAction.Damaged.Count==81&&MapAction.Damaged.Distinct().Count()==81,"radius five circle resumes after save without repeated cells");
Check(MapAction.Safe,"building damage is deliberately unfiltered and does not inject indiscriminate unit damage");

Reset();(a,b)=Pair();MclslActorAccessor.Set(a,"mana",50);
Check(MclslSpellSystem.CastPaid(a,b,"S001")&&MclslSpellSystem.CurrentMana(a)==40,"actual payment method debits once after successful queue");
double cooldown=MclslSpellSystem.Cooldown(a,"S001");MclslSpellCasting.InterruptWindup(a);
Check(MclslSpellSystem.CurrentMana(a)==40&&MclslSpellSystem.Cooldown(a,"S001")==cooldown,"interrupt neither refunds mana nor refreshes cooldown");
MclslBattleProfessionSystem.SetMain(a,"body");
Check(MclslSpellSystem.Cooldown(a,"S001")==cooldown,"main switch cannot refresh cooldown");
Check(!MclslSpellSystem.CastPaid(a,a,"S001")&&MclslSpellSystem.CurrentMana(a)==40,"failed target lock cannot consume mana");

Reset();(a,b)=Pair();MclslActorAccessor.Set(a,"mana",50);MclslBattleProfessionSystem.SetMain(a,"arts");
MclslSpellSystem.Learn(a,"S016");MclslSpellSystem.Learn(a,"S004");
Check(MclslSpellSystem.ReserveElementalChain(a).SequenceEqual(new[]{"S016","S004"}),"elemental chain contains only learned affordable ready spells");
Check(MclslSpellSystem.CastPaid(a,b,"C011")&&MclslSpellSystem.CurrentMana(a)==20,"chain pays base plus each real component once");
Check(MclslSpellSystem.Cooldown(a,"S016")>0&&MclslSpellSystem.Cooldown(a,"S004")>0,"chain reserves component cooldowns");
Check(MclslSpellSystem.ReserveElementalChain(a).Length==0,"components cannot bypass cooldown through another chain");

Reset();(a,b)=Pair();a.Realm="r0";MclslBattleProfessionSystem.SetMain(a,"pact");MclslSpiritPact.Summon(a);
MclslBattleVisuals.VisibleFlag=true;MclslNativeActorFrames.Available=true;MclslSpiritPact.Tick();
Check(MclslSpellEffect.Reserved==2,"projection and glowing rim both reserve visual budget");
MclslSpellEffect.Enabled=false;MclslSpiritPact.Tick();Check(MclslSpellEffect.Reserved==0,"disabling visuals returns projection and rim reservations");
MclslSpellEffect.Enabled=true;MclslSpiritPact.Tick();MclslBattleProfessionSystem.SetMain(a,"body");
Check(MclslSpellEffect.Reserved==0&&MclslSpiritPact.Count(a)==0,"main switch immediately releases all projection resources");

Reset();(a,b)=Pair();a.Realm="r4";MclslSpellSystem.Learn(a,"C007");MclslBattleProfessionSystem.SetMain(a,"body");
b.current_tile.pos=new(50,50);MclslSpellCasting.Queue(a,b,MclslSpellSystem.All["C007"]);
MclslSpellSystem.DamageCallback=(_,target,_)=>target.Alive=false;
Advance(1);Advance(2);Advance(3);for(int i=0;i<30;i++)MclslSpellTerrain.Tick();
Check(MapAction.Damaged.Count>0,"lethal first combo beat still produces its final terrain impact");

Console.WriteLine(JsonConvert.SerializeObject(new{checks,passed=true,actualUnityStartupTested=false,actualUnitStressTested=false}));
