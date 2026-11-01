using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MySimulatedLongevityRoad.Core;
using MySimulatedLongevityRoad.Systems;
static class Program {
 static int checks;
 static void Check(bool b,string message) { if(!b)throw new Exception(message); checks++; }
 static void Worker(Action a) { Exception error=null; var t=new Thread(()=>{try{a();}catch(Exception e){error=e;}});t.Start();t.Join();if(error!=null)throw error; }
 static void Drain() { int guard=0; while(MclslRuntimeChanges.Count>0) { if(++guard>100000)throw new Exception("handoff failed to drain"); MclslRuntimeChanges.Drain(); } }
 static Actor Add(long id,int region,bool natural=true) { var a=new Actor {data=new ActorData{id=id},current_tile=new WorldTile{Region=region},Source=natural?"natural":"wave:1"};MclslActorRegistry.Entries[id]=new Entry{Actor=a};MclslBeastPopulationIndex.Observe(a);return a; }
 static void Reset() { MclslRuntimeChanges.BindMainThread(); MclslRuntimeChanges.Clear(); MclslBeastPopulationIndex.Clear(); MclslActorRegistry.Entries.Clear(); MclslFrameDeadline.End(); }
 static void Main() {
 Reset(); var actors=Enumerable.Range(1,1000).Select(i=>Add(i,1,i%2==0)).ToArray();
 Worker(()=>Parallel.ForEach(actors,a=>{a.current_tile=new WorldTile{Region=2};for(int i=0;i<20;i++)MclslBeastPopulationIndex.Moved(a);}));
 Check(MclslBeastPopulationIndex.Count(false,1)==1000,"workers cannot mutate dictionaries");
 Check(MclslRuntimeChanges.Count==1000,"movement coalesces by actor");
 long rev=MclslRuntimeChanges.Revision; Drain();
 Check(MclslBeastPopulationIndex.Count(false,1)==0 && MclslBeastPopulationIndex.Count(false,2)==1000,"parallel movement regional totals");
 Check(MclslBeastPopulationIndex.Count(true,2)==500 && MclslBeastPopulationIndex.Count(true,-1)==500,"natural totals stable");
 Check(MclslRuntimeChanges.Revision==rev && MclslActorProjectionIndex.Updates==0,"position bypasses unrelated refreshes");
 var a=actors[0];a.current_tile.Region=3;Worker(()=>MclslBeastPopulationIndex.Moved(a));a.current_tile.Region=4;Worker(()=>MclslBeastPopulationIndex.Moved(a));Drain();Check(MclslBeastPopulationIndex.Count(false,4)==1,"latest location wins");
 a.current_tile.Region=5;Worker(()=>MclslBeastPopulationIndex.Moved(a));MclslBeastPopulationIndex.Forget(a.data.id);MclslRuntimeChanges.Remove(a.data.id);Drain();Check(MclslBeastPopulationIndex.Count(false,5)==0,"removal cancels movement");
 a=actors[1];a.current_tile.Region=6;Worker(()=>MclslBeastPopulationIndex.Moved(a));a.Alive=false;Drain();Check(MclslBeastPopulationIndex.Count(false,6)==0,"dead movement ignored");MclslBeastPopulationIndex.Forget(a.data.id);
 a=actors[2];a.current_tile.Region=7;Worker(()=>MclslBeastPopulationIndex.Moved(a));MclslWorldContext.Generation++;Drain();Check(MclslBeastPopulationIndex.Count(false,7)==0,"old generation rejected");
 a=actors[3];a.current_tile.Region=8;Worker(()=>MclslBeastPopulationIndex.Moved(a));MclslRuntimeChanges.Clear();MclslBeastPopulationIndex.Clear();Drain();Check(MclslBeastPopulationIndex.Count(false,-1)==0,"world clear drops moves");
 Reset();a=Add(1,1);a.current_tile.Region=2;MclslBeastPopulationIndex.Moved(a);Check(MclslBeastPopulationIndex.Count(false,2)==1 && MclslRuntimeChanges.Count==0,"main thread immediate");
 a.current_tile.Region=3;Worker(()=>{MclslBeastPopulationIndex.Moved(a);MclslRuntimeChanges.Publish(a,MclslActorChange.Data);});Drain();
 Check(MclslBeastPopulationIndex.Count(false,3)==1 && MclslActorProjectionIndex.Updates==1,"coalesced data flag retained");
 a.current_tile.Region=4;Worker(()=>MclslBeastPopulationIndex.Moved(a));MclslActorRegistry.Entries[1]=new Entry{Actor=new Actor{data=new ActorData{id=1}}};Drain();
 Check(MclslBeastPopulationIndex.Count(false,4)==0,"same id replacement rejects stale actor");
 a.asset.id="human";Worker(()=>MclslBeastPopulationIndex.Moved(a));Check(MclslRuntimeChanges.Count==0,"non beasts bypass queue");
 Reset();actors=Enumerable.Range(1,1000).Select(i=>Add(i,1,i%2==0)).ToArray();
 var stress=Task.Run(()=>Parallel.ForEach(actors,actor=>{for(int i=2;i<=100;i++){actor.current_tile=new WorldTile{Region=i};MclslBeastPopulationIndex.Moved(actor);}}));
 while(!stress.IsCompleted) MclslRuntimeChanges.Drain(); stress.GetAwaiter().GetResult(); Drain();
 Check(MclslBeastPopulationIndex.Count(false,100)==1000 && MclslBeastPopulationIndex.Count(true,100)==500,"concurrent producer/drain preserves totals");
 Check(MclslBeastPopulationIndex.Count(false,-1)==1000,"stress keeps population size");
 var queue=new MclslActorThreadHandoff<Actor>();queue.Publish(1,a,1,false,1);queue.TryTake(out _,out _,out _,out _,out _);
 for(int i=0;i<100;i++){queue.Publish(1,a,1,false,1);queue.TryTake(out _,out _,out _,out _,out _);}
 long before=GC.GetAllocatedBytesForCurrentThread();for(int i=0;i<10000;i++){queue.Publish(1,a,1,false,1);queue.TryTake(out _,out _,out _,out _,out _);}long allocated=GC.GetAllocatedBytesForCurrentThread()-before;
 Check(allocated==0,"warmed handoff allocation: "+allocated);
 queue.Publish(1,a,1,false,1);queue.Cancel(1);Check(queue.Count==0,"cancel recycled node");queue.Publish(2,a,2,true,2);Check(queue.TryTake(out long id,out var source,out int flags,out bool repair,out long gen)&&id==2&&source==a&&flags==2&&repair&&gen==2,"recycled node resets metadata");
 Console.WriteLine($"BeastPopulationTests: {checks} checks passed; warmed handoff: {allocated} bytes / 10000 cycles.");
 }
}
