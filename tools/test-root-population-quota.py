from pathlib import Path
import subprocess
r=Path.cwd();out=r/'build/root-quota-tests';out.mkdir(parents=True,exist_ok=True)
source=(r/'code/MySimulatedLongevityRoad/Systems/Cultivation/MclslChildhoodRootSystem.cs').read_text(encoding='utf-8-sig').replace('namespace MySimulatedLongevityRoad.Systems;','using MySimulatedLongevityRoad.Systems;\nnamespace MySimulatedLongevityRoad.Systems {')+'\n}'
stubs=r'''
namespace MySimulatedLongevityRoad.Core {
 internal static class MclslRuntimeSettings {internal static int ChildhoodRootChancePercent=10;}
 internal static class MclslRuntime {internal static int Year=100;internal static int CurrentYear()=>Year;}
 internal static class MclslAnnualFrameBudget {internal static int Operations=int.MaxValue;internal static bool TryConsumeOperation()=>Operations-->0;}
 internal static class MclslAnnualBackpressure {internal static void RecordProgress(){}}
}
namespace MySimulatedLongevityRoad.Data {
 internal static class MclslActorDataKeys {internal const string ChildhoodRootChecked="checked",MortalSeparationChecked="mortal",ImmortalFate="fate",LastBreakthroughResult="result",Aptitude="aptitude";}
 internal class Gift {internal string Name="root";}
 internal static class MclslAptitudeGiftCatalog {internal static Gift ForAptitude(int n)=>new Gift();internal static int RollAptitude(string s)=>50;internal static bool RollQualification(string s,int chance)=>chance==100;}
}
namespace MySimulatedLongevityRoad.Traits {
 internal static class MclslBeastActorRegistration {internal static bool IsBeast(Actor a)=>a!=null&&a.Beast;}
 internal static class MclslTraitRegistration {internal static System.Action<Actor> Granted;internal static void SyncGiftTrait(Actor a,int n){a.Root=true;MySimulatedLongevityRoad.Systems.MclslChildhoodRootSystem.ObservePopulation(a);Granted?.Invoke(a);}}
}
internal class Actor {
 internal long Id;internal int Age=20;internal bool Alive=true,Sapient=true,Eligible=true,Beast,Root;
 internal object data=new object();internal Dictionary<string,object> Values=new Dictionary<string,object>();
 internal bool isSapient()=>Sapient;
}
internal static class MapBox {internal static int current_world_seed_id=44;}
namespace MySimulatedLongevityRoad.Systems {
 internal static class MclslActorRegistry {
 internal class Entry {internal Actor Actor;internal byte RootPopulationState;internal bool RootAgeEligible;}
 internal static readonly Dictionary<long,Entry> Entries=new Dictionary<long,Entry>();
 internal static bool ResolveEntry(long id,out Entry entry)=>Entries.TryGetValue(id,out entry);
 internal static bool Resolve(long id,out Actor actor){bool ok=Entries.TryGetValue(id,out var e);actor=ok?e.Actor:null;return ok;}
 }
 internal static class MclslActorAccessor {
 internal static long Id(Actor a)=>a==null?0:a.Id;internal static int Age(Actor a)=>a.Age;
 internal static bool Alive(Actor a)=>a!=null&&a.Alive;
 internal static int GetInt(Actor a,string key,int fallback)=>a.Values.TryGetValue(key,out var v)?(int)v:fallback;
 internal static void Set(Actor a,string key,int v){a.Values[key]=v;}
 internal static void Set(Actor a,string key,string v){a.Values[key]=v;}
 }
 internal static class MclslSpiritualRootSystem {internal static bool HasActualSpiritualRoot(Actor a)=>a.Root;}
 internal static class MclslEligibility {internal static bool CanCultivate(Actor a)=>a!=null&&a.Alive&&a.Eligible;}
 internal static class MclslMindSystem {internal static void EnsureMindState(Actor a){}}
 internal static class MclslSpiritualRootEntrySystem {internal static int Calls;internal static void TryEnterFromGiftTrait(Actor a,int y){Calls++;}}
 internal static class MclslScheduler {internal static void WakeAnnualCultivationActor(Actor a){}}
 internal static class MclslWorldBootstrapLane {internal static bool Completed=true;}
}
'''
tests=r'''
public static class RootQuotaRegression {
 static void Check(bool b,string name){if(!b)throw new Exception(name);}
 static void Reset(){MclslActorRegistry.Entries.Clear();MclslChildhoodRootSystem.ClearPopulation();MclslRuntimeSettings.ChildhoodRootChancePercent=10;MclslRuntime.Year=100;MclslWorldBootstrapLane.Completed=true;MclslAnnualFrameBudget.Operations=int.MaxValue;MclslTraitRegistration.Granted=null;MclslSpiritualRootEntrySystem.Calls=0;}
 static Actor Add(long id,int age=20,bool root=false,bool checkedRoot=true){var a=new Actor{Id=id,Age=age,Root=root};if(checkedRoot)a.Values[MclslActorDataKeys.ChildhoodRootChecked]=1;MclslActorRegistry.Entries[id]=new MclslActorRegistry.Entry{Actor=a};MclslChildhoodRootSystem.ObservePopulation(a);return a;}
 static void Drain(){int guard=100000;while(MclslChildhoodRootSystem.HasPending&&guard-->0)MclslChildhoodRootSystem.TickQuota();Check(guard>0,"queue did not finish");}
 static string Selection(bool reverse){Reset();for(int i=1;i<=950;i++)Add(i,20,i<=80);var ids=Enumerable.Range(951,50);if(reverse)ids=ids.Reverse();foreach(int id in ids)Add(id,5,false,false);Drain();Check(MclslChildhoodRootSystem.Population==1000&&MclslChildhoodRootSystem.RootPopulation==100,"1000/80 must grant20");return string.Join(",",MclslActorRegistry.Entries.Values.Where(e=>e.Actor.Id>=951&&e.Actor.Root).Select(e=>e.Actor.Id).OrderBy(i=>i));}
 static void Rebuild(){var people=MclslActorRegistry.Entries.Values.Select(e=>e.Actor).ToArray();MclslChildhoodRootSystem.ClearPopulation();foreach(var e in MclslActorRegistry.Entries.Values)e.RootPopulationState=0;foreach(var a in people)MclslChildhoodRootSystem.ObservePopulation(a);}
 public static string Run(){
 Check(Selection(false)==Selection(true),"selection depends on traversal order");
 Check(!MclslChildhoodRootSystem.HasPending,"completed candidate requeued");var records=MclslActorRegistry.Entries.Values.Select(e=>e.Actor).ToArray();foreach(var a in records)MclslChildhoodRootSystem.TryProcessAgeFiveDeadline(a,100);Drain();Check(MclslChildhoodRootSystem.RootPopulation==100,"repeat grants");Rebuild();Drain();Check(MclslChildhoodRootSystem.RootPopulation==100,"load duplicates");
 foreach(int p in new[]{0,10,100}){Reset();MclslRuntimeSettings.ChildhoodRootChancePercent=p;for(int i=1;i<=7;i++)Add(i,5,false,false);Drain();Check(MclslChildhoodRootSystem.TargetPopulation==7*p/100,"floor arithmetic");Check(MclslChildhoodRootSystem.RootPopulation==7*p/100,"percentage allocation");}
 Reset();for(int i=1;i<=999;i++)Add(i,20,i<=98);Add(1000,6,false,false);Drain();Check(MclslChildhoodRootSystem.RootPopulation==99,"must not awaken adults");
 Reset();for(int i=1;i<=1000;i++)Add(i,20,i<=500);var child=Add(1001,5,false,false);Drain();Check(!child.Root&&MclslChildhoodRootSystem.RootPopulation==500,"excess old roots removed or new granted");
 Reset();var young=Add(1,4,false,false);var beast=Add(2,5,false,false);beast.Beast=true;MclslChildhoodRootSystem.ObservePopulation(beast);var dumb=Add(3,5,false,false);dumb.Sapient=false;MclslChildhoodRootSystem.ObservePopulation(dumb);var boat=Add(4,20);boat.Eligible=false;MclslChildhoodRootSystem.ObservePopulation(boat);Check(MclslChildhoodRootSystem.Population==0,"excluded population counted");Drain();Check(!beast.Root&&!dumb.Root,"excluded granted quota");young.Age=5;MclslChildhoodRootSystem.ObservePopulation(young);Check(MclslChildhoodRootSystem.Population==1,"age transition");MclslRuntimeSettings.ChildhoodRootChancePercent=100;Drain();Check(young.Root,"five-year candidate missing");
 young.Alive=false;MclslChildhoodRootSystem.ForgetPopulation(1);MclslChildhoodRootSystem.ForgetPopulation(1);Check(MclslChildhoodRootSystem.Population==0&&MclslChildhoodRootSystem.RootPopulation==0,"duplicate death subtraction");
 Reset();MclslRuntimeSettings.ChildhoodRootChancePercent=100;child=Add(1,6,false,false);MclslWorldBootstrapLane.Completed=false;MclslChildhoodRootSystem.TickQuota();Check(!child.Root&&MclslChildhoodRootSystem.HasPending,"grant before rebuild");MclslWorldBootstrapLane.Completed=true;MclslAnnualFrameBudget.Operations=0;MclslChildhoodRootSystem.TickQuota();Check(!child.Root,"ignored shared budget");MclslAnnualFrameBudget.Operations=int.MaxValue;Drain();Check(child.Root,"six-year recovery");
 Reset();for(int i=1;i<=90;i++)Add(i);for(int i=91;i<=100;i++)Add(i,5,false,false);MclslAnnualFrameBudget.Operations=1;MclslChildhoodRootSystem.TickQuota();Check(MclslChildhoodRootSystem.RootPopulation==1,"batch budget count");for(int i=1;i<=10;i++){MclslActorRegistry.Resolve(i,out var a);a.Root=true;MclslChildhoodRootSystem.ObservePopulation(a);}MclslAnnualFrameBudget.Operations=int.MaxValue;Drain();Check(MclslChildhoodRootSystem.RootPopulation==11,"special roots not occupying quota");
 Reset();for(int i=1;i<=90;i++)Add(i);for(int i=91;i<=100;i++)Add(i,5,false,false);MclslAnnualFrameBudget.Operations=1;MclslChildhoodRootSystem.TickQuota();for(int i=1;i<=50;i++){MclslActorRegistry.Resolve(i,out var a);a.Alive=false;MclslChildhoodRootSystem.ObservePopulation(a);}MclslRuntimeSettings.ChildhoodRootChancePercent=0;MclslAnnualFrameBudget.Operations=int.MaxValue;Drain();Check(MclslChildhoodRootSystem.RootPopulation==1&&MclslChildhoodRootSystem.Population==50,"mid-batch population/config not applied");
 Reset();MclslRuntimeSettings.ChildhoodRootChancePercent=100;child=Add(1,5,false,false);MclslChildhoodRootSystem.ForgetPopulation(1);MclslActorRegistry.Entries.Remove(1);Drain();Check(MclslChildhoodRootSystem.Population==0&&!child.Root,"removed candidate granted");
 Reset();child=Add(1,20,true);MclslChildhoodRootSystem.ForgetPopulation(1);MclslActorRegistry.Entries[1]=new MclslActorRegistry.Entry{Actor=new Actor{Id=1}};MclslChildhoodRootSystem.ObservePopulation(MclslActorRegistry.Entries[1].Actor);Check(MclslChildhoodRootSystem.Population==1&&MclslChildhoodRootSystem.RootPopulation==0,"replacement counted twice");
 Reset();MclslRuntimeSettings.ChildhoodRootChancePercent=100;child=Add(1,5,false,false);MclslAnnualFrameBudget.Operations=1;MclslChildhoodRootSystem.TickQuota();child.Age=6;MclslChildhoodRootSystem.ObservePopulation(child);Drain();Check(MclslSpiritualRootEntrySystem.Calls==1,"six-year reroll");
 Reset();MclslRuntimeSettings.ChildhoodRootChancePercent=100;child=Add(1,5,true,false);Drain();Check(MclslChildhoodRootSystem.RootPopulation==1,"existing-root child counted twice");
 Reset();for(int i=1;i<=99;i++)Add(i);child=Add(100,5,false,false);MclslRuntimeSettings.ChildhoodRootChancePercent=0;Drain();MclslRuntimeSettings.ChildhoodRootChancePercent=100;MclslChildhoodRootSystem.ObservePopulation(child);Drain();Check(!child.Root,"config rerolled completed child");
 Reset();MclslRuntimeSettings.ChildhoodRootChancePercent=100;child=Add(1,5,false,false);MclslRuntime.Year++;Drain();Check(child.Root&&!MclslChildhoodRootSystem.HasPending,"year boundary lost candidate");
 Reset();child=Add(1,6,false,false);child.Age=7;MclslRuntimeSettings.ChildhoodRootChancePercent=100;Drain();Check(!child.Root,"adult retroactive grant");
 Reset();for(int i=1;i<=90;i++)Add(i);for(int i=91;i<=100;i++)Add(i,5,false,false);MclslAnnualFrameBudget.Operations=1;MclslChildhoodRootSystem.TickQuota();Rebuild();MclslAnnualFrameBudget.Operations=int.MaxValue;Drain();Check(MclslChildhoodRootSystem.RootPopulation==10,"partial batch reload duplicates or loses quota");
 Reset();MclslRuntimeSettings.ChildhoodRootChancePercent=100;child=Add(1,5,false,false);child.Alive=false;MclslChildhoodRootSystem.ObservePopulation(child);Drain();Check(!child.Root&&MclslChildhoodRootSystem.Population==0,"dead candidate granted");
 Reset();for(int i=1;i<=100;i++)Add(i);var special=Add(101,20,true);Check(MclslChildhoodRootSystem.RootPopulation==1,"adult special not counted");special.Root=false;MclslChildhoodRootSystem.ObservePopulation(special);Check(MclslChildhoodRootSystem.RootPopulation==0,"removed root not deducted");special.Sapient=false;MclslChildhoodRootSystem.ObservePopulation(special);Check(MclslChildhoodRootSystem.Population==100,"sapience loss not deducted");
 Reset();Check(MclslChildhoodRootSystem.Population==0&&!MclslChildhoodRootSystem.HasPending,"world state leakage");
 var perf=new List<string>();foreach(int n in new[]{10000,50000}){Reset();long before=GC.GetAllocatedBytesForCurrentThread();var timer=System.Diagnostics.Stopwatch.StartNew();for(int i=1;i<=n;i++)Add(i,i>n*9/10?5:20,false,i<=n*9/10);double register=timer.Elapsed.TotalMilliseconds;timer.Restart();foreach(var e in MclslActorRegistry.Entries.Values)MclslChildhoodRootSystem.ObservePopulation(e.Actor);double census=timer.Elapsed.TotalMilliseconds;timer.Restart();Drain();double allocate=timer.Elapsed.TotalMilliseconds;Check(MclslChildhoodRootSystem.RootPopulation==n/10,"large population allocation");perf.Add(n+" population: register="+register.ToString("F2")+"ms, census="+census.ToString("F2")+"ms, allocation="+allocate.ToString("F2")+"ms, harness allocation="+(GC.GetAllocatedBytesForCurrentThread()-before)+" bytes");}
 return "PASS quota: 0/10/100%, rounding, stable order, shortage/excess, death, specials, sapience, age5/6, no adult refill, config, shared budget, rebuild/load/world reset\n"+string.Join("\n",perf);
 }
}
'''
hash_source=(r/'code/MySimulatedLongevityRoad/Core/MclslStableHash.cs').read_text(encoding='utf-8-sig').replace('namespace MySimulatedLongevityRoad.Core;','namespace MySimulatedLongevityRoad.Core {')+'\n}'
# All using directives must precede namespace declarations.
using='using System;\nusing System.Linq;\nusing System.Collections.Generic;\nusing MySimulatedLongevityRoad.Core;\nusing MySimulatedLongevityRoad.Data;\nusing MySimulatedLongevityRoad.Traits;\nusing MySimulatedLongevityRoad.Systems;\n'
source='\n'.join(line for line in source.splitlines() if not line.startswith('using '))
cs=out/'RootQuotaRegression.cs';cs.write_text(using+stubs+source+hash_source+tests,encoding='utf-8')
result=subprocess.run(['pwsh','-NoProfile','-Command',f"Add-Type -Path '{cs}'; [RootQuotaRegression]::Run()"],capture_output=True,text=True,encoding='utf-8')
(out/'result.txt').write_text(result.stdout+result.stderr,encoding='utf-8');print(result.stdout,end='');print(result.stderr,end='');assert result.returncode==0,'isolated quota regression failed'
# Verify lifecycle wiring, not only the isolated allocator.
c=r/'code/MySimulatedLongevityRoad'
registry=(c/'Systems/Runtime/MclslActorRegistry.cs').read_text(encoding='utf-8')
scheduler=(c/'Systems/Runtime/MclslScheduler.cs').read_text(encoding='utf-8')
changes=(c/'Core/MclslRuntimeChanges.cs').read_text(encoding='utf-8')
assert 'internal byte RootPopulationState' in registry and 'ForgetPopulation(actorId)' in registry and 'ClearPopulation()' in registry
assert '_annualCandidateScanIds = MclslActorRegistry.PopulationIds' in scheduler
assert '!_annualCandidateScanPending && MclslChildhoodRootSystem.HasPending' in scheduler
assert 'MclslChildhoodRootSystem.ObservePopulation(actor)' in changes and 'MclslChildhoodRootSystem.ForgetPopulation(id)' in changes
assert 'getSimpleList' not in source and '.Snapshot(' not in source
print('PASS wiring: registry lifecycle, immediate root-state publication, one shared annual census, no allocator population scan')
