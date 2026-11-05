from pathlib import Path
import re
root=Path(__file__).resolve().parents[1]
r=root/'code/MySimulatedLongevityRoad';out=root/'build/refresh-optimization-tests';out.mkdir(parents=True,exist_ok=True)
def method(text,name):
 a=re.search(r'(?m)^    (?:internal|private|public) static [^\n]*\b'+re.escape(name),text).start();b=text.index('{',a);depth=0
 for i in range(b,len(text)):
  if text[i]=='{':depth+=1
  if text[i]=='}':
   depth-=1
   if depth==0:return text[a:i+1]
 raise Exception(name)
s=(r/'Systems/WorldResources/MclslWorldEpochSystem.cs').read_text(encoding='utf-8-sig')
methods='\n'.join(method(s,n) for n in ('EnsureTransitionMembership(', 'TickDeferredTransitionWork()', 'ProcessAncientTransitionBatch(', 'HasExceededTransitionBudget('))
cs='''using System;
using System.Linq;
using System.Collections.Generic;
using Stopwatch=FakeStopwatch;
class FakeStopwatch {public const long Frequency=1000;public static long Time,Step;public static long GetTimestamp(){long t=Time;Time+=Step;return t;}}
namespace UnityEngine {static class Time{public static int frameCount;}}
class Actor {public long Id;public bool Alive=true,Processed;public string System="old";}
class MclslWorldRunState {public string RunId="r";public List<string> PendingAncientCultivatorIds=new List<string>();}
static class MclslWorldRunRepository {public static MclslWorldRunState Current=new MclslWorldRunState();}
static class MclslWorldContext {public static long Generation=1;}
static class MclslActorDataKeys {public const string CultivationSystem="system",AncientLawProcessed="processed";}
static class MclslCultivationSystemIds {public const string AncientLaw="old";}
static class MclslActorAccessor {public static bool Alive(Actor a)=>a!=null&&a.Alive;public static string GetString(Actor a,string k,string d)=>a.System;public static int GetInt(Actor a,string k,int d)=>a.Processed?1:0;public static long Id(Actor a)=>a.Id;}
static class MclslAnnualFrameBudget {public static bool Expired;public static bool TryConsumeOperation()=>!Expired;}
static class MclslFrameDeadline {public static bool Expired=false;}
static class MclslRuntime {public static int CurrentYear()=>100;}
class Scope:IDisposable {public void Dispose(){}}
static class MclslUnityProfiler {public static Scope Sample(string s)=>new Scope();}
public class TransitionBudgetRegression {
 const int TransitionSeedBudget=12,TransitionConversionBudget=6;const double TransitionWorkBudgetMs=.45;
 static IReadOnlyList<Actor> _ancientQueueSeedActors=Array.Empty<Actor>();static int _ancientQueueSeedCursor,_lastTransitionFrame=-1;
 static long _transitionGeneration=-1;static readonly HashSet<string> PendingTransitionMembership=new HashSet<string>();
 static Dictionary<long,Actor> actors=new Dictionary<long,Actor>();static int conversions;
 static bool HasPendingTransitionWork=>_ancientQueueSeedCursor<_ancientQueueSeedActors.Count||MclslWorldRunRepository.Current.PendingAncientCultivatorIds.Count>0;
 static Actor FindActor(long id)=>actors.TryGetValue(id,out var a)?a:null;
 static bool TryConvertLegacyAncientLongevity(Actor a,int y){a.Processed=true;conversions++;return true;}
 static bool TryConvertAncientRemnant(Actor a,int y,int o)=>false;static void MarkAncientRemnant(Actor a,int y){}
 static void Check(bool ok,string msg){if(!ok)throw new Exception(msg);}
 static void Reset(){MclslWorldContext.Generation++;UnityEngine.Time.frameCount++;MclslWorldRunRepository.Current=new MclslWorldRunState();_ancientQueueSeedActors=Array.Empty<Actor>();_ancientQueueSeedCursor=0;actors.Clear();conversions=0;FakeStopwatch.Time=FakeStopwatch.Step=0;MclslAnnualFrameBudget.Expired=MclslFrameDeadline.Expired=false;}
 public static string Run(){
 Reset();_ancientQueueSeedActors=Enumerable.Range(0,100).Select(x=>(Actor)null).ToArray();TickDeferredTransitionWork();Check(_ancientQueueSeedCursor==12,"invalid seeds bypass cap");TickDeferredTransitionWork();Check(_ancientQueueSeedCursor==12,"same-frame reentry");UnityEngine.Time.frameCount++;TickDeferredTransitionWork();Check(_ancientQueueSeedCursor==24,"next-frame progress");
 Reset();for(int i=0;i<40;i++)MclslWorldRunRepository.Current.PendingAncientCultivatorIds.Add("bad"+i);TickDeferredTransitionWork();Check(MclslWorldRunRepository.Current.PendingAncientCultivatorIds.Count==34,"invalid conversions bypass cap");
 Reset();for(int i=1;i<=20;i++){actors[i]=new Actor{Id=i};MclslWorldRunRepository.Current.PendingAncientCultivatorIds.Add(i.ToString());}TickDeferredTransitionWork();Check(conversions==6,"valid conversion cap");while(HasPendingTransitionWork){UnityEngine.Time.frameCount++;TickDeferredTransitionWork();}Check(conversions==20,"lost conversion");
 Reset();_ancientQueueSeedActors=Enumerable.Range(0,20).Select(i=>new Actor{Id=i+1}).ToArray();FakeStopwatch.Step=1;TickDeferredTransitionWork();Check(_ancientQueueSeedCursor==0,"time budget ignored");
 Reset();_ancientQueueSeedActors=Enumerable.Range(0,20).Select(i=>new Actor{Id=1}).ToArray();actors[1]=_ancientQueueSeedActors[0];TickDeferredTransitionWork();Check(conversions==1,"duplicate seed converted twice");
 Reset();MclslWorldRunRepository.Current.PendingAncientCultivatorIds.Add("99");_lastTransitionFrame=UnityEngine.Time.frameCount;TickDeferredTransitionWork();Check(MclslWorldRunRepository.Current.PendingAncientCultivatorIds.Count==0,"world-generation boundary");
 return "PASS transition: 12/6 bounds incl invalid records, same-frame guard, shared timer, dedup, completion, world reset";
 }
'''+methods+'\n}'
(out/'TransitionBudgetRegression.cs').write_text(cs,encoding='utf-8')
s=(r/'Systems/Cultivation/MclslTechniqueRealmLimit.cs').read_text(encoding='utf-8-sig');a=s.index('    private sealed class FailedLimit');b=s.index('    private static bool CanReachCore',a)
cs='''using System;
class Actor {public object data=new object();}
static class MclslRealmIds {public const string ChangSheng="long";}
static class MclslActorAccessor {public static long Id(Actor a)=>1;}
namespace MySimulatedLongevityRoad.Core {static class MclslRuntimeChanges {public static long Revision=1;public static long DataRevision(long id)=>Revision;}}
public class LimitCacheRegression {static int calls;static bool allow;
 static bool CanReachCore(Actor a,string t,out string reason){calls++;reason=allow?"":"blocked";return allow;}
 static void Check(bool b){if(!b)throw new Exception("limit cache");}
 public static string Run(){var a=new Actor();string why;Check(!CanReach(a,"core",out why));Check(!CanReach(a,"core",out why)&&calls==1);MySimulatedLongevityRoad.Core.MclslRuntimeChanges.Revision++;allow=true;Check(CanReach(a,"core",out why)&&calls==2);Check(CanReach(a,"core",out why)&&calls==3);allow=false;MySimulatedLongevityRoad.Core.MclslRuntimeChanges.Revision=0;CanReach(a,"core",out why);CanReach(a,"core",out why);Check(calls==5);return "PASS limit cache: deterministic failure reused, revision invalidation, success uncached, unregistered actor uncached";}
'''+s[a:b]+'\n}'
(out/'LimitCacheRegression.cs').write_text(cs,encoding='utf-8')
source=(r/'Systems/Cultivation/MclslSpellSystem.cs').read_text(encoding='utf-8-sig')
scan=method(source,'TickSelfCasting(')
cs=r'''using System;
using System.Collections.Generic;
class Actor {public int health=100,mana=100;public bool alive=true,has_attack_target=false;public int getHealth()=>health;}
class Clock {public double Time;public double getCurWorldTime()=>Time;}
static class World {public static Clock world=new Clock();}
static class MclslActorAccessor {public static bool Alive(Actor a)=>a.alive;}
static class MclslFrameDeadline {public static bool Expired=false;}
static class MclslPerformanceProbe {public static long Begin()=>0;public static void End(string s,long n){}public static void RecordCultivatorPollActors(int n){}}
public class SelfScanRegression {
 class RuntimeState {public double NextSelfCheck;public int SelfHealth,SelfMana;}
 static readonly Dictionary<Actor,RuntimeState> states=new Dictionary<Actor,RuntimeState>();
 static List<Actor> SelfCasters=new List<Actor>();static List<Actor> SelfCasterIds=>SelfCasters;static int _selfScanIndex,calls;static double _lastSelfScanWorldTime=-1;
 static RuntimeState GetRuntime(Actor a){if(!states.TryGetValue(a,out var s))states[a]=s=new RuntimeState();return s;}
 static int CurrentMana(Actor a)=>a.mana;static bool TryCastSelf(Actor a){calls++;return false;}
 static void Check(bool b,string m){if(!b)throw new Exception(m);}
 public static string Run(){var a=new Actor();SelfCasters.Add(a);World.world.Time=1;TickSelfCasting(10);Check(calls==0,"20-frame cadence");TickSelfCasting(20);Check(calls==1,"initial scan");World.world.Time=2;TickSelfCasting(40);Check(calls==1,"resting sleep");World.world.Time=4;TickSelfCasting(60);Check(calls==2,"3-second recheck");World.world.Time=4.1;a.health=90;TickSelfCasting(80);Check(calls==3,"damage wake");a.mana=50;World.world.Time=4.2;TickSelfCasting(100);Check(calls==4,"mana wake");TickSelfCasting(120);Check(calls==4,"paused world");SelfCasters.Clear();states.Clear();_selfScanIndex=0;calls=0;for(int i=0;i<100;i++)SelfCasters.Add(new Actor());World.world.Time=5;TickSelfCasting(140);Check(calls==64,"bounded scan");return "PASS self-cast: 20-frame scan, sleeping candidate, 3-second retry, health/mana wake, pause and 64 limit";}
''' +scan+'\n}'
(out/'SelfScanRegression.cs').write_text(cs,encoding='utf-8')

print('Prepared tests from actual production methods')

import subprocess
for name in ('TransitionBudgetRegression','LimitCacheRegression','SelfScanRegression'):
    file=out/(name+'.cs')
    cmd='$ErrorActionPreference="Stop"; Add-Type -Path '+"'"+str(file)+"'"+'; ['+name+']::Run()'
    subprocess.run(['pwsh.exe','-NoProfile','-Command',cmd],check=True)
