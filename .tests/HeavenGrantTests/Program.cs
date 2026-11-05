using System;
using System.Diagnostics;
using System.Text.Json;
using MySimulatedLongevityRoad.Core;
using MySimulatedLongevityRoad.Data;
using MySimulatedLongevityRoad.Systems;
using MySimulatedLongevityRoad.Traits;
using MySimulatedLongevityRoad.UI;

internal static class Program
{
    private static int checks;
    private static void Check(bool condition,string text)
    {
        if (!condition) throw new Exception("FAIL " + text);
        checks++;
    }
    private static int Int(Actor a,string k) => MclslActorAccessor.GetInt(a,k);
    private static string Text(Actor a,string k) => MclslActorAccessor.GetString(a,k);
    private static Actor Create(string route="")
    {
        var a=new Actor();
        if(route.Length>0) a.data.set(MclslActorDataKeys.CultivationSystem,route);
        return a;
    }
    private static void Click(Actor a,string id)
    {
        int tips=WorldTip.Count;
        a.Traits.Add(id);
        Check(MclslHeavenGrantActions.HandleAdded(a,id),"action consumed");
        Check(!a.hasTrait(id),"action removed");
        // Actual native string overload calls the asset overload; both existing
        // postfixes route the same successful addition.
        Check(MclslHeavenGrantActions.HandleAdded(a,id),"outer overload consumed");
        Check(WorldTip.Count==tips+1,"one result for both overload callbacks");
        Check(!MclslTraitGrantRouter.IsSuppressed,"suppression released");
    }
    private static void Main()
    {
        foreach(string route in new[]{MclslCultivationSystemIds.NewLaw,MclslCultivationSystemIds.AncientLaw,""})
        {
            Actor a=Create(route);
            a.Traits.Add("gifts_2");
            Click(a,MclslHeavenGrantActions.PerfectionTraitId);
            foreach(string key in new[]{MclslActorDataKeys.ImmortalFate,MclslActorDataKeys.Aptitude,
                         MclslActorDataKeys.SpellInsight,MclslActorDataKeys.MindState})
                Check(Int(a,key)==100,"raw attribute 100: "+key);
            Check(Int(a,MclslActorDataKeys.MortalSeparationChecked)==1,"fate judged");
            Check(a.hasTrait("gifts_6")&&!a.hasTrait("gifts_2"),"aptitude trait synchronized");
            Check(Text(a,MclslActorDataKeys.CultivationSystem)==route,"route unchanged");
            Check(!a.data.Values.ContainsKey(MclslActorDataKeys.SpiritualRootPrimary)
                &&!a.data.Values.ContainsKey(MclslActorDataKeys.Realm),"no root or realm manufactured");
            if(route.Length==0) Check(WorldTip.Last.Contains("尚未入道"),"mortal explanation");
            int writes=a.data.Writes,refresh=MclslActorInfoPanel.Refreshes;
            Click(a,MclslHeavenGrantActions.PerfectionTraitId);
            Check(a.data.Writes==writes&&MclslActorInfoPanel.Refreshes==refresh,"unchanged repeated blessing avoids writes/refresh");
            // 0.8.7 移除了 MclslMortalSeparationSystem：仙缘判定改由五—六岁
            // 灵根判定（MclslChildhoodRootSystem）承担，并以 MortalSeparationChecked
            // 作为“已判定”闸门。天道垂赐只置位闸门、不伪造幼年判定，所以后续
            // 年度判定不会重掷已赐予的仙缘。
            Check(Int(a,MclslActorDataKeys.ChildhoodRootChecked)==0,"blessing does not fake childhood check");
            Check(Int(a,MclslActorDataKeys.ImmortalFate)==100&&Int(a,MclslActorDataKeys.MortalSeparationChecked)==1,
                "real annual judgment does not reroll granted fate");
            Actor loaded=Roundtrip(a);
            Check(Int(loaded,MclslActorDataKeys.ImmortalFate)==100&&Int(loaded,MclslActorDataKeys.MortalSeparationChecked)==1,
                "storage roundtrip retains judgment");

            Actor b=Create(route);
            b.data.set(MclslActorDataKeys.TechniqueId,route==MclslCultivationSystemIds.AncientLaw?"spiritual_gongfa_qingye":"gongfa_qingye");
            b.data.set(MclslActorDataKeys.TechniqueName,"青叶培元功");
            b.data.set(MclslActorDataKeys.AncientTechniqueComprehension,100);
            b.data.set(MclslActorDataKeys.TrueEssence,321);
            b.data.set(MclslActorDataKeys.CultivationProgress,12.5f);
            if(route.Length>0)b.data.set(MclslActorDataKeys.Realm,MclslRealmIds.JinDan);
            int changes=MclslTechniqueOccupationSystem.Changes;
            Click(b,MclslHeavenGrantActions.DaoBookTraitId);
            string expected=route==MclslCultivationSystemIds.AncientLaw?"spiritual_gongfa_wuxing":"gongfa_wuxing";
            Check(Text(b,MclslActorDataKeys.TechniqueId)==expected,"highest law overlap and route prefix");
            Check(Text(b,MclslActorDataKeys.TechniqueName)=="五行归真道书"&&Text(b,MclslActorDataKeys.TechniqueMaxRealm)==MclslRealmIds.HeDao,
                "book name/ceiling consistent");
            Check(MclslTechniqueStageSystem.Progress(b)==0,"new book starts at entry");
            Check(MclslTechniqueOccupationSystem.Changes==changes+1,"real accessor notifies index once");
            Check(Int(b,MclslActorDataKeys.TrueEssence)==321&&MclslActorAccessor.GetFloat(b,MclslActorDataKeys.CultivationProgress)==12.5f,
                "essence and breakthrough progress retained");
            Check(Text(b,MclslActorDataKeys.Realm)==(route.Length>0?MclslRealmIds.JinDan:""),"realm retained");
            Check(Text(b,MclslActorDataKeys.CultivationSystem)==route,"book does not grant route");
            MclslTechniqueStageSystem.SetProgress(b,67);
            int before=b.data.Writes;
            Click(b,MclslHeavenGrantActions.DaoBookTraitId);
            Check(b.data.Writes==before&&MclslTechniqueStageSystem.Progress(b)==67,"existing HeDao book entirely retained");
            Check(MclslTechniqueOccupationSystem.Changes==changes+1,"repeat book does not resubmit index");
        }
        foreach(var sample in new[]{
            ("gongfa_yinyang",MclslRealmIds.ZhuJi,true),
            ("gongfa_qingye",MclslRealmIds.HeDao,true),
            ("legacy_example__gongfa_yinyang",MclslRealmIds.ZhuJi,false),
            ("legacy_example__gongfa_yinyang",MclslRealmIds.HeDao,true),
            ("branch_example",MclslRealmIds.ChangSheng,true),
            ("invalid",MclslRealmIds.HeDao,false)})
        {
            Actor a=Create();
            a.data.set(MclslActorDataKeys.TechniqueId,sample.Item1);
            a.data.set(MclslActorDataKeys.TechniqueName,"原有传承");
            a.data.set(MclslActorDataKeys.TechniqueMaxRealm,sample.Item2);
            a.data.set(MclslActorDataKeys.AncientTechniqueComprehension,89);
            int writes=a.data.Writes;
            Click(a,MclslHeavenGrantActions.DaoBookTraitId);
            Check((Text(a,MclslActorDataKeys.TechniqueId)==sample.Item1)==sample.Item3,"effective source ceiling: "+sample.Item1);
            if(sample.Item3)Check(a.data.Writes==writes&&MclslTechniqueStageSystem.Progress(a)==89,"preserved source has zero writes");
        }
        // Real production reward commit: a lower discovery must not mutate any book field.
        foreach (string route in new[]{"", MclslCultivationSystemIds.AncientLaw, MclslCultivationSystemIds.NewLaw})
        foreach (var sample in new[]{
            ("gongfa_yinyang", MclslRealmIds.ZhuJi, true),
            ("gongfa_qingye", MclslRealmIds.HeDao, true),
            ("legacy_example__gongfa_yinyang", MclslRealmIds.HeDao, true),
            ("branch_example", MclslRealmIds.HeDao, true),
            ("legacy_example__gongfa_yinyang", MclslRealmIds.ZhuJi, false),
            ("invalid", MclslRealmIds.HeDao, false)})
        {
            Actor a=Create(route);
            string oldId=route==MclslCultivationSystemIds.AncientLaw ? "spiritual_"+sample.Item1 : sample.Item1;
            a.data.set(MclslActorDataKeys.TechniqueId,oldId);
            a.data.set(MclslActorDataKeys.TechniqueName,"original");
            a.data.set(MclslActorDataKeys.TechniqueMaxRealm,sample.Item2);
            MclslTechniqueStageSystem.SetProgress(a,73);
            a.data.set(MclslActorDataKeys.TrueEssence,456);
            a.data.set(MclslActorDataKeys.CultivationProgress,21.5f);
            int writes=a.data.Writes, notifications=MclslTechniqueOccupationSystem.Changes;
            var lower=new MclslTechniqueDefinition{Id="test_discovery",Name="lower",MaxRealm=MclslRealmIds.YuanYing};
            bool assigned=MclslTechniqueRealmLimit.TryAssignDiscoveredTechnique(a,lower,89);
            Check(assigned!=sample.Item3,"discovery downgrade rule "+oldId);
            if(sample.Item3)
            {
                Check(a.data.Writes==writes,"rejected discovery has zero writes");
                Check(Text(a,MclslActorDataKeys.TechniqueId)==oldId&&Text(a,MclslActorDataKeys.TechniqueName)=="original",
                    "original source and name retained");
                Check(MclslTechniqueStageSystem.Progress(a)==73,"original comprehension retained");
                Check(MclslTechniqueOccupationSystem.Changes==notifications,"rejected discovery has no index notification");
            }
            else Check(MclslTechniqueOccupationSystem.Changes==notifications+1,"adopted discovery notifies once");
            Check(Int(a,MclslActorDataKeys.TrueEssence)==456&&MclslActorAccessor.GetFloat(a,MclslActorDataKeys.CultivationProgress)==21.5f,
                "discovery preserves cultivation resources");
        }
        Actor upgrade=Create();
        upgrade.data.set(MclslActorDataKeys.TechniqueId,"gongfa_qingye");
        Check(MclslTechniqueRealmLimit.TryAssignDiscoveredTechnique(upgrade,MclslCultivationCatalog.Technique("gongfa_yinyang"),92),
            "higher transmission still adopted");
        Check(Text(upgrade,MclslActorDataKeys.TechniqueMaxRealm)==MclslRealmIds.HeDao,"upgrade ceiling calibrated");
        Check(MclslTechniqueStageSystem.Progress(upgrade)==92,"upgrade completeness applied");
        Actor stable=Create();stable.data.id=987654321;
        Click(stable,MclslHeavenGrantActions.DaoBookTraitId);
        string choice=Text(stable,MclslActorDataKeys.TechniqueId);
        stable.data.Values.Clear();
        Click(stable,MclslHeavenGrantActions.DaoBookTraitId);
        Check(Text(stable,MclslActorDataKeys.TechniqueId)==choice,"no old book uses deterministic actor hash");
        Actor opposite=Create();opposite.data.id=987654321;
        opposite.data.set(MclslActorDataKeys.CultivationSystem,MclslCultivationSystemIds.AncientLaw);
        Click(opposite,MclslHeavenGrantActions.DaoBookTraitId);
        Check(Text(opposite,MclslActorDataKeys.TechniqueId)=="spiritual_"+choice,"same tie rule on ancient route");

        foreach(string id in new[]{MclslHeavenGrantActions.PerfectionTraitId,MclslHeavenGrantActions.DaoBookTraitId})
        {
            Actor dead=Create();dead.Living=false;Click(dead,id);Check(dead.data.Writes==0,"dead actor rejected");
            Actor missing=Create();missing.data=null;Click(missing,id);Check(WorldTip.Last.Contains("数据无效"),"missing data rejected and cleared");
            Actor invalid=Create();invalid.data.id=0;Click(invalid,id);Check(invalid.data.Writes==0,"invalid ID rejected");
            World.world=null;Actor absent=Create();Click(absent,id);Check(absent.data.Writes==0,"absent world rejected");World.world=new();
            MclslWorldContext.WorldId="";Actor unbound=Create();Click(unbound,id);Check(unbound.data.Writes==0,"unbound world rejected");MclslWorldContext.WorldId="world";
        }
        MclslTraitRegistration.ThrowSync=true;
        Click(Create(),MclslHeavenGrantActions.PerfectionTraitId);
        Check(MclslDiagnostics.Errors==1,"exception logged and action cleared");
        MclslTraitRegistration.ThrowSync=false;
        Actor invalidBook=Create();
        invalidBook.data.set(MclslActorDataKeys.TechniqueId,"gongfa_qingye");
        var definition=MclslCultivationCatalog.Technique("gongfa_wuxing");
        string originalName=definition.Name;
        int invalidWrites=invalidBook.data.Writes;
        try
        {
            definition.Name="";
            Click(invalidBook,MclslHeavenGrantActions.DaoBookTraitId);
            Check(invalidBook.data.Writes==invalidWrites,"invalid candidate rejected before any writes");
        }
        finally { definition.Name=originalName; }
        Actor noUi=Create();noUi.Living=false;
        noUi.Traits.Add(MclslHeavenGrantActions.PerfectionTraitId);
        WorldTip.Throw=true;
        try
        {
            MclslHeavenGrantActions.HandleAdded(noUi,MclslHeavenGrantActions.PerfectionTraitId);
            Check(!noUi.hasTrait(MclslHeavenGrantActions.PerfectionTraitId),"missing native UI cannot prevent cleanup");
        }
        finally { WorldTip.Throw=false; }
        Actor ordinary=Create();
        for(int i=0;i<10000;i++)MclslHeavenGrantActions.HandleAdded(ordinary,"ordinary");
        var watch=new Stopwatch();
        long allocated=GC.GetAllocatedBytesForCurrentThread();
        watch.Start();
        for(int i=0;i<1000000;i++)MclslHeavenGrantActions.HandleAdded(ordinary,"ordinary");
        watch.Stop();
        long bytes=GC.GetAllocatedBytesForCurrentThread()-allocated;
        Check(bytes==0,"ordinary trait routing has no per-call allocation");
        Check(ordinary.data.Writes==0,"ordinary trait not consumed");
        Check(!MclslHeavenGrantActions.HandleAdded(ordinary,null),"null non-action ID ignored");
        for(int i=0;i<10000;i++)MclslHeavenGrantActions.HandleAdded(ordinary,MclslHeavenGrantActions.DaoBookTraitId);
        allocated=GC.GetAllocatedBytesForCurrentThread();
        for(int i=0;i<100000;i++)MclslHeavenGrantActions.HandleAdded(ordinary,MclslHeavenGrantActions.DaoBookTraitId);
        Check(GC.GetAllocatedBytesForCurrentThread()==allocated,"consumed outer callback allocates zero");
        MeasureRepeatedClick(MclslHeavenGrantActions.PerfectionTraitId);
        MeasureRepeatedClick(MclslHeavenGrantActions.DaoBookTraitId);
        Console.WriteLine($"HeavenGrantTests: {checks} checks passed; 1000000 unrelated routes {watch.Elapsed.TotalMilliseconds:F3}ms, allocated={bytes} bytes. Offline boundaries; Unity acceptance pending.");
    }

    private static void MeasureRepeatedClick(string id)
    {
        var actor=Create(MclslCultivationSystemIds.NewLaw);
        for(int i=0;i<1000;i++) { actor.Traits.Add(id);MclslHeavenGrantActions.HandleAdded(actor,id); }
        int writes=actor.data.Writes,changes=MclslTechniqueOccupationSystem.Changes;
        var timer=new Stopwatch();
        long start=GC.GetAllocatedBytesForCurrentThread();
        timer.Start();
        for(int i=0;i<10000;i++) { actor.Traits.Add(id);MclslHeavenGrantActions.HandleAdded(actor,id); }
        timer.Stop();
        long bytes=GC.GetAllocatedBytesForCurrentThread()-start;
        Check(actor.data.Writes==writes&&MclslTechniqueOccupationSystem.Changes==changes,
            "10000 unchanged clicks avoid data/index writes: "+id);
        Console.WriteLine($"Offline event microprobe {id}: 10000 warmed clicks {timer.Elapsed.TotalMilliseconds:F3}ms; {bytes} bytes; no native UI/rendering costs included.");
    }

    private static Actor Roundtrip(Actor actor)
    {
        string json=JsonSerializer.Serialize(actor.data.Values);
        var loaded=Create();
        using var doc=JsonDocument.Parse(json);
        foreach(var p in doc.RootElement.EnumerateObject())
            if(p.Value.ValueKind==JsonValueKind.Number)loaded.data.set(p.Name,p.Value.GetInt32());
            else loaded.data.set(p.Name,p.Value.GetString());
        return loaded;
    }
}
