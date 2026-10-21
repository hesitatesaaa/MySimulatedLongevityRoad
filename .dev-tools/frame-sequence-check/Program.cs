using System.Text.Json;
using MySimulatedLongevityRoad.Systems;

int checks=0, clips=0;
void Check(bool condition,string reason) { if(!condition)throw new Exception(reason);checks++; }
using var document=JsonDocument.Parse(File.ReadAllText(args[0]));
foreach(var group in document.RootElement.GetProperty("groups").EnumerateArray())
foreach(var property in group.GetProperty("actions").EnumerateObject())
{
    var frames=property.Value.GetProperty("frames").EnumerateArray().ToArray();
    if(frames.Length==0)continue;
    var names=frames.Select(f=>f.GetProperty("name").GetString()!).ToArray();
    var seconds=frames.Select(f=>f.GetProperty("durationMs").GetDouble()/1000d).ToArray();
    bool loop=property.Value.GetProperty("loop").GetBoolean();
    var clip=new MclslAnimationClipData(names,seconds,loop);clips++;
    double start=0;
    for(int i=0;i<frames.Length;i++)
    {
        Check(clip.FrameIndex(start+seconds[i]/2)==i,"Actual frame midpoint "+group.GetProperty("actorId").GetString()+"/"+property.Name);
        start+=seconds[i];
    }
    Check(clip.FrameIndex(0)==0,"Start frame");
    Check(clip.FrameIndex(double.PositiveInfinity)==(loop?0:frames.Length-1),"Loop/terminal behavior");
    foreach(int fps in new[]{30,60,144})
    {
        var observed=new HashSet<int>();
        for(int tick=0;tick<=Math.Ceiling(clip.Duration*fps);tick++)
        {
            int index=clip.FrameIndex((double)tick/fps);
            Check(index>=0 && index<names.Length,"Production timing index bounded");observed.Add(index);
        }
        Check(observed.Count==frames.Length,"Every real authored frame reachable at "+fps+" FPS");
    }
}
var state=new MclslActorAnimationState();
Check(state.Select(1,false,.5,.2,out _)==MclslActorAction.Idle,"Actual idle state");
Check(state.Select(2,true,.5,.2,out var elapsed)==MclslActorAction.Walk && elapsed==0,"Movement starts at first frame");
state.Attack(2.1);
Check(state.Select(2.2,true,.5,.2,out _)==MclslActorAction.Attack,"Attack selection");
state.Hit(2.2,.2);
Check(state.Select(2.3,true,.5,.2,out _)==MclslActorAction.Hit,"Hit has priority");
Check(state.Select(3,false,.5,.2,out elapsed)==MclslActorAction.Idle && elapsed==0,"Stop returns to first idle frame");
Check(state.Select(.1,false,.5,.2,out elapsed)==MclslActorAction.Idle && elapsed==0,"Rewind clears old animation state");
Console.WriteLine(JsonSerializer.Serialize(new{checks,clips,productionCode=true,mockOrStubUsed=false,unityTested=false}));
