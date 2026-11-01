using System;
using System.Collections.Generic;
public class ActorData { public long id; public string name; }
public class Named { public ActorData data = new(); }
public class ActorAsset { public string id = "mclsl_beast_test"; }
public class WorldTile { public int Region; }
public class Actor { public ActorData data = new(); public ActorAsset asset = new(); public WorldTile current_tile = new(); public Named kingdom, city; public Dictionary<string,float> stats = new(); public string Source = "natural"; public bool Alive = true; public float getAge() => 20; }
namespace MySimulatedLongevityRoad.Data {
 static class MclslActorDataKeys {
 public const string TaishangProgress="a",LearnedSpells="b",HuanzhenIdentity="c",NascentCaveId="d",DivineChangeId="e",QiankunBagBackup="f",QiankunBagCorrupt="g",AnnualStep="h",AnnualActiveYear="i",AnnualLatestRequestedYear="j",AnnualStage="k",AnnualLastCompletedYear="l",LastCultivationYear="m",ManaLastWorldTime="n",ManaInitialized="o",ManaCurrent="p",Realm="q",CultivationSystem="r",ImmortalFate="s",MindState="t",Profession="u",SpiritualRootPrimary="v",SpiritualRootAttributes="w",SpiritualRootCount="x",Aptitude="y",TechniqueId="z",TechniqueName="A",TechniqueMaxRealm="B",QiankunBag="aa";
 }
 static class MclslRealmIds { public static int Index(string s)=>0; }
}
namespace MySimulatedLongevityRoad.Traits {
 static class MclslBeastActorRegistration { public static bool IsOrdinary(Actor a)=>a.asset.id.StartsWith("mclsl_beast_"); }
 static class MclslImmortalActorRegistration { public static bool IsImmortal(Actor a)=>false; }
}
namespace MySimulatedLongevityRoad.Core {
 static class MclslWorldContext { public static long Generation = 1; }
 static class MclslPerformanceProbe { public static void RecordChangeQueueMerged() {} public static long Begin()=>0; public static void RecordChangeQueueDrained(int n){} public static void End(string s,long n){} }
 static class MclslDiagnostics { public static void Error(string a,string b)=>throw new Exception(b); public static void Fatal(string a,string b){} public static void Probe(string a,string b){} public static void Log(string a,string b){} public static void Warn(string a,string b){} public static void Alert(string a,string b){} public static void Notify(string a,string b){} }
}
namespace MySimulatedLongevityRoad.Systems {
 static class MclslActorAccessor { public static long Id(Actor a)=>a?.data?.id??0; public static bool Alive(Actor a)=>a?.Alive==true; public static string Realm(Actor a)=>""; public static string GetString(Actor a,string k)=>a.Source; }
 class Entry { public Actor Actor; public long DataRevision; public bool HasNativeStamp; public Core.MclslRuntimeChanges.NativeStamp NativeStamp; public bool ObserveTargets()=>true; }
 static class MclslActorRegistry {
 public static readonly Dictionary<long,Entry> Entries = new();
 public static bool ResolveEntry(long id,out Entry e)=>Entries.TryGetValue(id,out e);
 public static bool Resolve(long id,out Actor a) { bool ok=Entries.TryGetValue(id,out var e); a=e?.Actor; return ok; }
 public static bool TryPeekAlive(long id,out Actor a)=>Resolve(id,out a)&&a.Alive;
 public static void RefreshTargets(Actor a,bool forceEligibility=false){} public static void RefreshTraits(Actor a){} public static void RefreshLocation(Actor a){} public static void RefreshIdentity(Actor a){} public static void RefreshResourceReferences(Actor a){}
 }
 static class MclslEligibility { public static bool IsCivilizedActor(Actor a)=>false; }
 static class MclslBeastSurfaceIndex { public static int Region(WorldTile t)=>t?.Region??-1; }
 static class MclslBeastWorld { public const string SourceKey="source"; public class StateData { public int Unlocked; } public static StateData State = new(); public static void ApplyRankStats(Actor a){} }
 static class MclslBeastTalentSystem { public static void Wake(Actor a){} }
 static class MclslActorProjectionIndex { public static int Updates; public static void RefreshUninitiated(Actor a)=>Updates++; public static void Update(Actor a)=>Updates++; public static void Remove(long id){} public static void Clear()=>Updates=0; }
 static class MclslRankSnapshotSource { public static void Remove(long id){} public static void QueueUpdate(Actor a,bool b){} public static void Clear(){} }
 static class MclslArtifactSystem { public static void ApplyNativeStatCaps(Actor a){} }
 static class MclslLongevityRules { public static void ApplyRuntimeLifespan(Actor a){} }
 static class MclslSpellSystem { public static void ObserveSelfCaster(Actor a){} }
 static class MclslAncientMentorshipSystem { public static void OnCultivationChanged(Actor a){} }
 static class MclslActorRelations { public static void Refresh(Actor a){} }
 static class MclslFamilySystem { public static void Observe(Actor a){} }
 static class MclslCultivatorCandidateIndex { public static void Observe(Actor a){} }
}
namespace MySimulatedLongevityRoad.Systems.Combat { static class MclslCareerCombat { public static void Wake(Actor a){} } }
namespace MySimulatedLongevityRoad.Systems.Visual { static class MclslRealmHaloVisualSystem { public static void RefreshQualification(Actor a){} public static void ObserveTaishangProgress(Actor a){} public static void Forget(long id){} } }
namespace MySimulatedLongevityRoad.UI { static class MclslActorInfoPanel { public static bool HasAnyOpenWindow=>false; public static bool HasOpenActor(Actor a)=>false; public static void RefreshOpenForActor(Actor a){} } }
namespace MySimulatedLongevityRoad.Patches { static class MclslXuanhuangDirectionPatch { public static void ObserveActor(Actor a){} } }
