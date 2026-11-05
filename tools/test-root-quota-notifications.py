from pathlib import Path
import subprocess,json,sys,os
os.environ['DOTNET_TieredCompilation']='0'
r=Path.cwd();out=r/'build/root-quota-round2-tests';out.mkdir(parents=True,exist_ok=True)
# The existing regression builds a harness around the whole production allocator.
subprocess.run(['python.exe','-X','utf8',str(r/'tools/test-root-population-quota.py')],check=True)
base='#pragma warning disable 0649\nusing MySimulatedLongevityRoad.UI;\n'+(r/'build/root-quota-tests/RootQuotaRegression.cs').read_text(encoding='utf-8-sig')
c=r/'code/MySimulatedLongevityRoad'
def block(s,token):
 a=s.index(token);left=s.index('{',a);depth=1;i=left+1
 while depth:
  if s[i]=='{':depth+=1
  elif s[i]=='}':depth-=1
  i+=1
 return a,i
def extract(s,token):
 a,b=block(s,token);return s[a:b]
def replace(s,token,new):
 a,b=block(s,token);return s[:a]+new+s[b:]
# Instrument the isolated getters, not the production build.
base=base.replace('internal static int Age(Actor a)=>a.Age;','internal static int Age(Actor a){QuotaCounters.AgeReads++;return a.Age;}')
base=base.replace('internal static bool HasActualSpiritualRoot(Actor a)=>a.Root;','internal static bool HasActualSpiritualRoot(Actor a){QuotaCounters.RootReads++;return a.Root;}')
base=base.replace('internal static bool CanCultivate(Actor a)=>a!=null&&a.Alive&&a.Eligible;','internal static bool CanCultivate(Actor a){QuotaCounters.EligibilityReads++;return a!=null&&a.Alive&&a.Eligible;}')
base=replace(base,'internal static class MclslActorDataKeys',extract((c/'Data/MclslActorDataKeys.cs').read_text(encoding='utf-8-sig'),'internal static class MclslActorDataKeys'))
base=replace(base,'internal class Actor {',r'''internal class Actor {
 internal long Id;internal int Age=20;internal bool Alive=true,Sapient=true,Eligible=true,Beast,Root;
 internal FakeData data=new FakeData();internal FakeAsset asset=new FakeAsset();internal FakeOwner kingdom,city;
 internal object current_tile;internal Dictionary<string,float> stats=new Dictionary<string,float>{{"damage",1},{"health",100}};
 internal Dictionary<string,object> Values=new Dictionary<string,object>();internal bool isSapient()=>Sapient;internal float getAge()=>Age;
}''')
base=replace(base,'internal static class MclslActorRegistry {',r'''internal static class MclslActorRegistry {
 internal class Entry {internal Actor Actor;internal byte RootPopulationState;internal bool RootAgeEligible,HasNativeStamp;internal long DataRevision;internal MySimulatedLongevityRoad.Core.MclslRuntimeChanges.NativeStamp NativeStamp;}
 internal static readonly Dictionary<long,Entry> Entries=new Dictionary<long,Entry>();
 internal static bool ResolveEntry(long id,out Entry entry)=>Entries.TryGetValue(id,out entry);
 internal static bool Resolve(long id,out Actor actor){bool ok=Entries.TryGetValue(id,out var e);actor=ok?e.Actor:null;return ok;}
 internal static bool Register(Actor a,out long id,out bool isNew){id=a.Id;isNew=!Entries.TryGetValue(id,out var e);if(isNew||!object.ReferenceEquals(e.Actor,a)){if(!isNew)MclslChildhoodRootSystem.ForgetPopulation(id);Entries[id]=new Entry{Actor=a};MclslRuntimeChanges.Publish(a,MclslActorChange.All);}return true;}
 internal static void RefreshTargets(Actor a,bool forceEligibility=false){}internal static void RefreshTraits(Actor a){}internal static void RefreshLocation(Actor a){}internal static void RefreshIdentity(Actor a){}internal static void RefreshResourceReferences(Actor a){}
}''')
base=replace(base,'internal static class MclslTraitRegistration {',r'''internal static class MclslTraitRegistration {
 internal static Action<Actor> Granted;internal static bool IsGiftTrait(string id)=>id!=null&&id.StartsWith("gifts_");
 internal static void InvalidateNativeRealmTraitSync(Actor a){}
 internal static void SyncGiftTrait(Actor a,int n){a.Root=true;MclslRuntimeChanges.Publish(a,MclslActorChange.Traits|MclslActorChange.RootPopulation);Granted?.Invoke(a);}
}''')
base=replace(base,'internal static class MclslEligibility {',r'''internal static class MclslEligibility {
 internal static bool CanCultivate(Actor a){QuotaCounters.EligibilityReads++;return a!=null&&a.Alive&&a.Eligible;}
 internal static bool Refresh(Subspecies s)=>s.Changed;
}''')
changes=(c/'Core/MclslRuntimeChanges.cs').read_text(encoding='utf-8-sig')
notify='namespace MySimulatedLongevityRoad.Core {\n[Flags]\n'+extract(changes,'internal enum MclslActorChange')+'\ninternal static class MclslRuntimeChanges {\n'+r'''
 internal static bool IsMainThread=true;internal static long Revision;
 private static FakePending Pending=new FakePending();private static FakeHandoff<Actor> Deferred=new FakeHandoff<Actor>();
 internal static void Reset(){IsMainThread=true;Revision=0;Pending=new FakePending();Deferred=new FakeHandoff<Actor>();}
 internal static void DeliverDeferred(){while(Deferred.Items.Count>0){var x=Deferred.Items.Dequeue();if(MclslActorRegistry.Resolve(x.Id,out var actor)&&object.ReferenceEquals(actor,x.Source)&&x.Generation==MclslWorldContext.Generation)Publish(actor,(MclslActorChange)x.Flags);}}
'''+extract(changes,'internal readonly struct NativeStamp')+'\n'+extract(changes,'internal static void Publish(Actor actor')+'\n'+extract(changes,'internal static void OnWrite(Actor actor')+'\n}\n}\n'
extra=r'''
internal static class QuotaCounters {internal static long RootReads,AgeReads,EligibilityReads;internal static void Reset(){RootReads=AgeReads=EligibilityReads=0;}}
internal class FakeData {internal string name="actor";internal long id;}
internal class FakeAsset {internal string id="human";}
internal class FakeOwner {internal FakeData data=new FakeData();}
internal class ActorTrait {internal string id;}
internal class Subspecies {internal bool Changed=true;}
internal class FakePending {internal void Publish(long id,int flags){}}
internal class FakeHandoff<T> {
 internal Queue<(long Id,T Source,int Flags,long Generation)> Items=new Queue<(long,T,int,long)>();
 internal void Publish(long id,T source,int flags,bool repairStats,long generation){Items.Enqueue((id,source,flags,generation));}
}
namespace MySimulatedLongevityRoad.Core {internal static class MclslWorldContext {internal static long Generation=1;}}
namespace MySimulatedLongevityRoad.Systems {
 internal static class MclslActorProjectionIndex {internal static void RefreshUninitiated(Actor a){}}
 internal static class MclslBeastTalentSystem {internal static void Wake(Actor a){}}
 internal static class MclslSpellSystem {internal static void ObserveSelfCaster(Actor a){}}
 internal static class MclslAncientMentorshipSystem {internal static void OnCultivationChanged(Actor a){}}
 internal static class MclslHuanzhenSystem {internal static void NotifyLegacyEligibility(Actor a,string key){}}
 internal static class MclslBeastPopulationIndex {internal static void Moved(Actor a){}}
 internal static class MclslActorRelations {internal static IEnumerable<long> SubspeciesMembers(Subspecies s)=>MclslActorRegistry.Entries.Keys;}
 internal static class MclslTraitGrantRouter {internal static void HandleAddedTrait(Actor a,string id){}internal static void HandleRemovedTrait(Actor a,string id){}}
}
namespace MySimulatedLongevityRoad.Systems.Visual {internal static class MclslRealmHaloVisualSystem {internal static void RefreshQualification(Actor a){}internal static void ObserveTaishangProgress(Actor a){}}}
namespace MySimulatedLongevityRoad.Systems.Combat {internal static class MclslCareerCombat {internal static void Wake(Actor a){}}}
namespace MySimulatedLongevityRoad.UI {internal static class MclslActorInfoPanel {internal static bool HasOpenActor(Actor a)=>false;}}
internal static class MclslXuanhuangDirectionPatch {internal static void ObserveActor(Actor a){}}
internal static class GUILayout {internal static List<string> Labels=new List<string>();internal static void Label(string s){Labels.Add(s);}}
internal class FakeSnapshot {internal int Cultivators,SensingQi;}
internal class FakeNativeMethods {public void setAsset(){}public void setKingdom(){}public void setSubspecies(){}}
'''
# The production beast-registration stub must also support the real lifecycle hook.
base=base.replace('internal static bool IsBeast(Actor a)=>a!=null&&a.Beast;','internal static bool IsBeast(Actor a)=>a!=null&&a.Beast;internal static void RestoreWildOwnership(Actor a){}')
patch=(c/'Patches/MclslPatches.cs').read_text(encoding='utf-8-sig');life=(c/'Patches/MclslLifecyclePatches.cs').read_text(encoding='utf-8-sig')
hooks='internal static class NotificationHooks {\n'+''.join(extract(patch,t)+'\n' for t in ('private static void Actor_AddTrait_Postfix','private static void Actor_AddTraitById_Postfix','private static void Actor_RemoveTrait_Postfix'))+extract(life,'private static void RelationChanged')+'\n'+extract(life,'private static void SubspeciesChanged')+r'''
 internal static void Trait(Actor a,string id,bool remove=false,bool byId=false){if(remove)Actor_RemoveTrait_Postfix(a,id);else if(byId)Actor_AddTraitById_Postfix(a,id,true);else Actor_AddTrait_Postfix(a,new ActorTrait{id=id},true);}
 internal static void Relation(Actor a,string method)=>RelationChanged(a,typeof(FakeNativeMethods).GetMethod(method));
 internal static void Species()=>SubspeciesChanged(new Subspecies());
}'''
ui=(c/'UI/MclslCodexWindow.cs').read_text(encoding='utf-8-sig');a=ui.index('    private (int Population, int Roots, int Percent, bool Ready, int Cultivators, int SensingQi)');b=ui.index('    private void DrawPage()',a)
view='internal class QuotaView {internal FakeSnapshot _snapshot=new FakeSnapshot();\n'+ui[a:b]+r'''
 internal void Draw(){GUILayout.Labels.Clear();DrawRootPopulation();}
}'''
tests=r'''
public static class RootQuotaNotificationRegression {
 static List<object> Results=new List<object>();
 static void Check(bool b,string msg){if(!b)throw new Exception(msg);}
 static void Reset(){MclslActorRegistry.Entries.Clear();MclslChildhoodRootSystem.ClearPopulation();MclslRuntimeChanges.Reset();MclslRuntimeSettings.ChildhoodRootChancePercent=10;MclslWorldBootstrapLane.Completed=true;MclslRuntime.Year=100;MclslAnnualFrameBudget.Operations=int.MaxValue;MclslTraitRegistration.Granted=null;QuotaCounters.Reset();}
 static Actor Add(long id,int age=20,bool root=false,bool completed=true){var a=new Actor{Id=id,Age=age,Root=root};a.data.id=id;if(completed)a.Values[MclslActorDataKeys.ChildhoodRootChecked]=1;MclslActorRegistry.Entries[id]=new MclslActorRegistry.Entry{Actor=a};MclslRuntimeChanges.Publish(a,MclslActorChange.All);return a;}
 static void Drain(){int guard=100000;while(MclslChildhoodRootSystem.HasPending&&guard-->0)MclslChildhoodRootSystem.TickQuota();Check(guard>0,"queue stuck");}
 public static string Run(){
 Reset();var a=Add(1,5);Check(QuotaCounters.RootReads==1,"registration not single root read");MclslRuntimeChanges.Publish(a,MclslActorChange.Native);Check(QuotaCounters.RootReads==1,"first native repeats registration");
 string[] unrelated={MclslActorDataKeys.QiankunBag,MclslActorDataKeys.TechniqueName,MclslActorDataKeys.TechniqueId,MclslActorDataKeys.TechniqueMaxRealm,MclslActorDataKeys.Profession,MclslActorDataKeys.Realm,MclslActorDataKeys.CultivationSystem,MclslActorDataKeys.Aptitude,MclslActorDataKeys.LastCultivationYear,MclslActorDataKeys.ManaCurrent};
 QuotaCounters.Reset();foreach(string key in unrelated)MclslRuntimeChanges.OnWrite(a,key);NotificationHooks.Trait(a,"career_lingqi");NotificationHooks.Trait(a,"career_lingqi",true);NotificationHooks.Relation(a,"setKingdom");Check(QuotaCounters.RootReads==0,"unrelated mutation read roots");
 a.Root=true;MclslRuntimeChanges.OnWrite(a,MclslActorDataKeys.SpiritualRootPrimary);MclslRuntimeChanges.OnWrite(a,MclslActorDataKeys.SpiritualRootAttributes);MclslRuntimeChanges.OnWrite(a,MclslActorDataKeys.SpiritualRootCount);Check(MclslChildhoodRootSystem.RootPopulation==1&&QuotaCounters.RootReads==3,"root field missed or double counted");
 a.Root=false;NotificationHooks.Trait(a,"gifts_one",true);Check(MclslChildhoodRootSystem.RootPopulation==0,"removed gift missed");a.Root=true;NotificationHooks.Trait(a,"gifts_one",false,true);Check(MclslChildhoodRootSystem.RootPopulation==1,"gift id overload missed");a.Root=false;NotificationHooks.Trait(a,"gifts_one");Check(MclslChildhoodRootSystem.RootPopulation==0,"gift asset overload missed");
 a.Root=true;MclslRuntimeChanges.IsMainThread=false;QuotaCounters.Reset();MclslRuntimeChanges.OnWrite(a,MclslActorDataKeys.SpiritualRootCount);Check(QuotaCounters.RootReads==0&&MclslChildhoodRootSystem.RootPopulation==0,"worker bypassed handoff");MclslRuntimeChanges.IsMainThread=true;MclslRuntimeChanges.DeliverDeferred();Check(QuotaCounters.RootReads==1&&MclslChildhoodRootSystem.RootPopulation==1,"deferred root missed");
 a.Sapient=false;NotificationHooks.Species();Check(MclslChildhoodRootSystem.Population==0&&MclslChildhoodRootSystem.RootPopulation==0,"brain loss missed");a.Sapient=true;NotificationHooks.Relation(a,"setSubspecies");Check(MclslChildhoodRootSystem.Population==1,"subspecies regain missed");a.Beast=true;NotificationHooks.Relation(a,"setAsset");Check(MclslChildhoodRootSystem.Population==0,"asset beast transform missed");a.Beast=false;NotificationHooks.Relation(a,"setAsset");Check(MclslChildhoodRootSystem.Population==1,"asset regain missed");
 MclslChildhoodRootSystem.ForgetPopulation(a.Id);MclslChildhoodRootSystem.ForgetPopulation(a.Id);Check(MclslChildhoodRootSystem.Population==0&&MclslChildhoodRootSystem.RootPopulation==0,"death not idempotent");
 Reset();a=Add(1,4,false,false);MclslRuntimeChanges.Publish(a,MclslActorChange.Native);Check(MclslChildhoodRootSystem.Population==0,"underage counted");a.Age=5;MclslRuntimeChanges.Publish(a,MclslActorChange.Native);Check(MclslChildhoodRootSystem.Population==1&&MclslChildhoodRootSystem.HasPending,"age5 crossing missed");MclslRuntimeSettings.ChildhoodRootChancePercent=100;Drain();Check(a.Root,"age5 not awarded");QuotaCounters.Reset();a.Age=6;MclslRuntimeChanges.Publish(a,MclslActorChange.Native);Check(QuotaCounters.RootReads==0&&!MclslChildhoodRootSystem.HasPending,"age6 duplicates completed roll");
 Reset();a=Add(1,4,false,false);a.Age=6;MclslRuntimeChanges.Publish(a,MclslActorChange.Native);Check(MclslChildhoodRootSystem.Population==1&&MclslChildhoodRootSystem.HasPending,"first native age4-to6 crossing missed");
 Reset();a=new Actor{Id=1,Root=true};a.data.id=1;RestoredActorHooks.Complete(a);Check(MclslChildhoodRootSystem.Population==1&&MclslChildhoodRootSystem.RootPopulation==1&&QuotaCounters.RootReads==1,"new restored actor missing or double-counted");
 Reset();a=Add(1);a.Root=true;QuotaCounters.Reset();RestoredActorHooks.Complete(a);Check(MclslChildhoodRootSystem.RootPopulation==1&&QuotaCounters.RootReads==1,"registered restored actor missing notification");
 Reset();var view=new QuotaView();view.Draw();string first=GUILayout.Labels[0],second=GUILayout.Labels[1];view.Draw();Check(object.ReferenceEquals(first,GUILayout.Labels[0])&&object.ReferenceEquals(second,GUILayout.Labels[1]),"unchanged view reallocates text");a=Add(1,5,true);view.Draw();Check(!object.ReferenceEquals(first,GUILayout.Labels[0]),"population view stale");first=GUILayout.Labels[0];MclslRuntimeSettings.ChildhoodRootChancePercent=20;view.Draw();Check(!object.ReferenceEquals(first,GUILayout.Labels[0]),"config view stale");MclslWorldBootstrapLane.Completed=false;view.Draw();Check(GUILayout.Labels[0].Contains("重建"),"rebuild view stale");MclslWorldBootstrapLane.Completed=true;view._snapshot.Cultivators=10;view._snapshot.SensingQi=2;view.Draw();Check(GUILayout.Labels[1].Contains("8")&&GUILayout.Labels[1].Contains("2"),"cultivator view stale");
 // Warm up then report median of 5 runs; getters and fixture allocation are isolated, not Unity FPS.
 foreach(int n in new[]{10000,50000})foreach(string scenario in new[]{"normal","birth-wave","unrelated-data"}){
  var samples=new List<(double Ms,long Alloc,long Roots,long Eligibility)>();string selection="";
  for(int run=0;run<6;run++){
   Reset();for(int i=1;i<=n;i++)Add(i,scenario=="birth-wave"&&i>n*9/10?5:20,scenario=="normal"&&i<=n/10,scenario!="birth-wave"||i<=n*9/10);
   GC.Collect();GC.WaitForPendingFinalizers();GC.Collect();QuotaCounters.Reset();long bytes=GC.GetAllocatedBytesForCurrentThread();var timer=System.Diagnostics.Stopwatch.StartNew();
   if(scenario=="unrelated-data"){foreach(var e in MclslActorRegistry.Entries.Values)foreach(string key in unrelated)MclslRuntimeChanges.OnWrite(e.Actor,key);}
   else {foreach(var e in MclslActorRegistry.Entries.Values)MclslChildhoodRootSystem.ObservePopulation(e.Actor);Drain();}
   timer.Stop();if(run>0)samples.Add((timer.Elapsed.TotalMilliseconds,GC.GetAllocatedBytesForCurrentThread()-bytes,QuotaCounters.RootReads,QuotaCounters.EligibilityReads));
   if(scenario=="birth-wave"){Check(MclslChildhoodRootSystem.RootPopulation==n/10,"birth-wave result changed");selection=string.Join(",",MclslActorRegistry.Entries.Values.Where(e=>e.Actor.Root).Select(e=>e.Actor.Id).OrderBy(x=>x));}
  }
  var median=samples.OrderBy(x=>x.Ms).ElementAt(2);if(scenario=="unrelated-data")Check(median.Roots==0,"bulk unrelated root reads");
  Results.Add(new{Population=n,Scenario=scenario,MedianMs=median.Ms,AllocatedBytes=median.Alloc,RootQueries=median.Roots,EligibilityQueries=median.Eligibility,Selection=selection});
 }
 return System.Text.Json.JsonSerializer.Serialize(new{Pass="precise writes/trait overloads, registration, age5/6, main-thread handoff, sapience/asset, death, restored snapshots, view cache",Results});
 }
}
'''
maobao=(c/'Systems/Reincarnation/MclslMaobaoArchiveManager.cs').read_text(encoding='utf-8-sig')
a=maobao.index('            MclslActorRegistry.Register(actor, out _, out bool isNew);');b=maobao.index('            _selectedActorId',a)
completion='internal static class RestoredActorHooks {internal static void Complete(Actor actor){'+maobao[a:b]+'}}'
cs=out/'RootQuotaNotificationRegression.cs';cs.write_text(base+extra+notify+hooks+view+completion+tests,encoding='utf-8')
result=subprocess.run(['pwsh','-NoProfile','-Command',f"Add-Type -Path '{cs}'; [RootQuotaNotificationRegression]::Run()"],capture_output=True,text=True,encoding='utf-8')
(out/'result.txt').write_text(result.stdout+result.stderr,encoding='utf-8');assert result.returncode==0,result.stdout+result.stderr
report=json.loads(result.stdout.strip());(out/'result.json').write_text(json.dumps(report,ensure_ascii=False,indent=2),encoding='utf-8')
print('PASS',report['Pass'])
for row in report['Results']:print({k:v for k,v in row.items() if k!='Selection'})
# Compare old/new allocator with identical instrumented fixtures and actual algorithms.
if '--compare-baseline' in sys.argv:
    old=(r/'build/root-quota-round2-baseline/code/MySimulatedLongevityRoad/Systems/Cultivation/MclslChildhoodRootSystem.cs').read_text(encoding='utf-8-sig')
    old=extract(old,'internal static class MclslChildhoodRootSystem')
    oldcs=replace(cs.read_text(encoding='utf-8'),'internal static class MclslChildhoodRootSystem',old)
    # Baseline notifications used broad membership/traits/native metadata rather than the precise marker.
    oldcs=oldcs.replace('if ((flags & MclslActorChange.RootPopulation) != 0)\n            MclslChildhoodRootSystem.ObservePopulation(actor);','if ((flags & (MclslActorChange.Membership | MclslActorChange.Traits | MclslActorChange.NativeMetadata)) != 0)\n            MclslChildhoodRootSystem.ObservePopulation(actor);')
    # Only run comparable workloads in the baseline; optimized-specific assertions intentionally do not apply.
    a=oldcs.index(' Reset();var a=Add(1,5);');b=oldcs.index(' // Warm up then report median',a);decl_start=tests.index(' string[] unrelated=');decl_end=tests.index(';',decl_start)+1;oldcs=oldcs[:a]+tests[decl_start:decl_end]+'\n'+oldcs[b:];oldcs=oldcs.replace('if(scenario=="unrelated-data")Check(median.Roots==0,"bulk unrelated root reads");','')
    oldfile=out/'RootQuotaBaselineRegression.cs';oldfile.write_text(oldcs,encoding='utf-8')
    result=subprocess.run(['pwsh','-NoProfile','-Command',f"Add-Type -Path '{oldfile}'; [RootQuotaNotificationRegression]::Run()"],capture_output=True,text=True,encoding='utf-8');assert result.returncode==0,result.stdout+result.stderr
    before=json.loads(result.stdout.strip());(out/'baseline-result.json').write_text(json.dumps(before,ensure_ascii=False,indent=2),encoding='utf-8')
    comparisons=[]
    for prior,current in zip(before['Results'],report['Results']):
     assert prior['Selection']==current['Selection'],'allocation outcome changed'
     if current['Scenario']=='unrelated-data':assert current['RootQueries']==0 and prior['RootQueries']>0
     if current['Scenario']=='birth-wave':assert current['RootQueries']<prior['RootQueries']
     comparisons.append(dict(population=current['Population'],scenario=current['Scenario'],before={k:v for k,v in prior.items() if k!='Selection'},after={k:v for k,v in current.items() if k!='Selection'}))
    (out/'comparison.json').write_text(json.dumps(comparisons,ensure_ascii=False,indent=2),encoding='utf-8')
    print('PASS unchanged selections and reduced root reads compared with pre-change allocator/broad notifications')
# Verify domain wiring, including registration and existing main-thread handoff.
registry=(c/'Systems/Runtime/MclslActorRegistry.cs').read_text(encoding='utf-8');scheduler=(c/'Systems/Runtime/MclslScheduler.cs').read_text(encoding='utf-8')
assert 'MclslChildhoodRootSystem.ObservePopulation(actor)' not in extract(registry,'internal static bool Register(Actor actor, out long actorId, out bool isNew)')
assert 'MclslActorChange.All' in extract(registry,'internal static bool Register(Actor actor, out long actorId, out bool isNew)')
assert 'MclslChildhoodRootSystem.ObservePopulation(actor)' not in extract(scheduler,'internal static void RegisterAndEnqueueAnnualActor(')
assert 'MclslChildhoodRootSystem.ObservePopulation(actor)' in extract(scheduler,'private static void TickAnnualCandidateScan(')
assert 'Publish(actor, (MclslActorChange)nativeFlags)' in changes
print('PASS registration owns one All notification, age goes through native stamp, annual census and existing deferred dispatcher retained')
