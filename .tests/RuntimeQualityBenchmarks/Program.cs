using System;
using System.Collections;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;
using MySimulatedLongevityRoad.Core;
using MySimulatedLongevityRoad.Data;
using MySimulatedLongevityRoad.Systems;

// Runs the actual pre-change OR current sources, with identical game boundaries.
// These CoreCLR measurements cannot establish Unity/Mono frame performance.
internal static class Program
{
    private static readonly string Key=(string)typeof(MclslSpellCasting).GetField("SaveKey",BindingFlags.Static|BindingFlags.NonPublic).GetRawConstantValue();
    private static int Count=>((ICollection)typeof(MclslSpellCasting).GetField("Pending",BindingFlags.Static|BindingFlags.NonPublic).GetValue(null)).Count;
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static double Read(MclslCareerSkill skill)=>skill.Mana+skill.Cooldown+skill.Power;
    private static Actor Prepare()
    {
        MclslSpellCasting.Clear();MclslWorldContext.Generation++;World.world=new();
        MclslBeastLoot.Now=0;MclslSpellCasting.Load();
        MclslSpellSystem.Hits.Clear();MclslSpellSystem.Hits.Capacity=1024;
        MclslActorRegistry.Actors.Clear();
        var a=new Actor{Id=1};var b=new Actor{Id=2};a.attack_target=b;b.attack_target=a;
        MclslActorRegistry.Actors.Add(1,a);MclslActorRegistry.Actors.Add(2,b);return a;
    }
    private static void Queue(Actor a)
    {
        for(int i=0;i<128;i++)if(!MclslSpellCasting.QueueTalisman(a,"fire",1))throw new Exception("queue rejected benchmark work");
    }
    private static void Finish()
    {
        MclslBeastLoot.Now=10;
        while(Count>0)MclslSpellCasting.Tick();
        if(MclslSpellSystem.Hits.Count!=640||MclslSpellSystem.Hits.Sum()!=10240)throw new Exception("benchmark work mismatch");
    }
    private static void Main()
    {
        var skill=MclslCombatCareerCatalog.Skill(0,1);double sum=0;
        for(int i=0;i<10000;i++)sum+=Read(skill);
        for(int i=0;i<8;i++){Queue(Prepare());MclslBeastLoot.Now=0.3;MclslSpellCasting.Tick();MclslSpellCasting.Save();MclslSpellCasting.Load();Finish();}
        long start=GC.GetAllocatedBytesForCurrentThread();
        for(int i=0;i<100000;i++)sum+=Read(skill);
        long skillBytes=GC.GetAllocatedBytesForCurrentThread()-start;
        var actor=Prepare();start=GC.GetAllocatedBytesForCurrentThread();Queue(actor);
        long queueBytes=GC.GetAllocatedBytesForCurrentThread()-start;
        MclslBeastLoot.Now=0.3;start=GC.GetAllocatedBytesForCurrentThread();long clock=Stopwatch.GetTimestamp();
        MclslSpellCasting.Tick();
        long primaryTicks=Stopwatch.GetTimestamp()-clock,primaryBytes=GC.GetAllocatedBytesForCurrentThread()-start;
        int pendingAfterPrimary=Count;
        // Warm JSON contracts and compare equivalent remaining damage workloads.
        MclslSpellCasting.Save();MclslSpellCasting.Load();
        start=GC.GetAllocatedBytesForCurrentThread();clock=Stopwatch.GetTimestamp();
        for(int i=0;i<10;i++)MclslSpellCasting.Save();
        long saveTicks=Stopwatch.GetTimestamp()-clock,saveBytes=GC.GetAllocatedBytesForCurrentThread()-start;
        int jsonChars=World.world.map_stats.custom_data.Values[Key].Length;
        start=GC.GetAllocatedBytesForCurrentThread();clock=Stopwatch.GetTimestamp();
        for(int i=0;i<10;i++)MclslSpellCasting.Load();
        long loadTicks=Stopwatch.GetTimestamp()-clock,loadBytes=GC.GetAllocatedBytesForCurrentThread()-start;
        MclslBeastLoot.Now=10;start=GC.GetAllocatedBytesForCurrentThread();clock=Stopwatch.GetTimestamp();
        Finish();
        long restTicks=Stopwatch.GetTimestamp()-clock,restBytes=GC.GetAllocatedBytesForCurrentThread()-start;
        Console.WriteLine(JsonSerializer.Serialize(new{runtime="CoreCLR; game substitutes; NOT Unity acceptance",skillReads=100000,skillBytes,queueBytes,
            primaryBytes,allTickBytes=primaryBytes+restBytes,primaryTicks,allTickTicks=primaryTicks+restTicks,pendingAfterPrimary,
            saveRepeats=10,saveTicks,saveBytes,loadRepeats=10,loadTicks,loadBytes,jsonChars,hits=640,damage=10240,frequency=Stopwatch.Frequency,checksum=sum}));
    }
}
