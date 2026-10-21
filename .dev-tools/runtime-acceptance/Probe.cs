// Injected only into an isolated test installation, never into player packages.
using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Newtonsoft.Json;
using UnityEngine;
using MySimulatedLongevityRoad.Systems;
using MySimulatedLongevityRoad.Data;
using MySimulatedLongevityRoad.Traits;
using MySimulatedLongevityRoad.UI;

namespace MySimulatedLongevityRoad.Core;

internal sealed class MclslAuditAcceptance : MonoBehaviour
{
    private static string Output;
    private readonly List<string> Checks = new();
    private readonly List<string> Errors = new();
    private readonly List<object> Units = new();
    private readonly Dictionary<string,double> Timings = new();

    internal static void Ensure()
    {
        string[] args=Environment.GetCommandLineArgs();
        int index=Array.IndexOf(args,"--mclsl-acceptance-output");
        if(index<0 || index+1>=args.Length) return;
        Output=args[index+1];
        UnityEngine.Object.DontDestroyOnLoad(new GameObject("MclslAuditAcceptance",typeof(MclslAuditAcceptance)));
    }
    private IEnumerator Start()
    {
        IEnumerator work=Run();
        while(true)
        {
            object next;
            try
            {
                if(!work.MoveNext()) { Finish(true);yield break; }
                next=work.Current;
            }
            catch(Exception ex) { Errors.Add(ex.ToString());Finish(false);yield break; }
            yield return next;
        }
    }
    private void Check(bool value,string label)
    {
        if(!value) throw new InvalidOperationException(label);
        Checks.Add(label);
        UnityEngine.Debug.Log("[MCLSL/Audit/PASS] "+label);
    }
    private void Capture(string text,string stack,LogType type)
    {
        if(type==LogType.Exception || type==LogType.Error)
            Errors.Add(text+"\n"+stack);
    }
    private static bool Valid(Actor actor) => actor?.data!=null && actor.asset!=null && actor.isAlive();
    private static bool? Owned(Sprite sprite)
    {
        Type registry=typeof(MclslAuditAcceptance).Assembly.GetType("MySimulatedLongevityRoad.Systems.MclslSpriteOwnership");
        var contains=registry?.GetMethod("Contains",System.Reflection.BindingFlags.Static|System.Reflection.BindingFlags.NonPublic);
        return contains==null ? null : (bool?)contains.Invoke(null,new object[]{sprite});
    }
    private IEnumerator Run()
    {
        float deadline=Time.realtimeSinceStartup+120;
        while((!Config.game_loaded || World.world?.tiles_list==null || SmoothLoader.isLoading()) && Time.realtimeSinceStartup<deadline)
            yield return null;
        yield return new WaitForSecondsRealtime(3);
        Application.logMessageReceived+=Capture;
        Check(Config.game_loaded && World.world?.tiles_list?.Length>0,"world-ready");
        Check(MySimulatedLongevityRoad.Interop.MclslLocalizationRuntimeMarker.IsRuntimeLoaded,"runtime-marker");
        WorldTile tile=World.world.tiles_list.FirstOrDefault(t=>t?.Type!=null && !t.Type.liquid);
        Check(tile!=null,"land-tile");
        string beast=MclslBeastCatalog.All.First().BeastAssetId;
        var ids=new[]{"human",MclslImmortalActorRegistration.BaiId,beast,"mclsl_world_soul_avatar"}
            .Concat(MclslWorldSoulActorDefinitions.All.Select(definition=>definition.ActorId));
        var actors=new List<Actor>();
        foreach(string id in ids)
        {
            var clock=Stopwatch.StartNew();
            Actor actor=World.world.units.createNewUnit(id,tile,pAdultAge:true);
            Timings["spawn:"+id]=clock.Elapsed.TotalMilliseconds;
            Units.Add(new{requested=id,asset=actor?.asset?.id,id=MclslActorAccessor.Id(actor)});
            bool sameBeast=MclslBeastCatalog.ForAsset(id)!=null
                && MclslBeastCatalog.ForAsset(actor?.asset?.id)?.Id==MclslBeastCatalog.ForAsset(id).Id;
            Check(actor?.data!=null && (actor.asset.id==id || sameBeast),"spawn:"+id);
            actors.Add(actor);
            yield return null;
            Sprite main=actor.calculateMainSprite();
            Check(main!=null,"main-sprite:"+id);
            Sprite colored=actor.calculateColoredSprite(main);
            Check(colored!=null,"colored-sprite:"+id);
            if(Owned(main) is bool owned)
            {
                if(id!="human") Check(owned,"sprite-owned:"+id);
                else Check(!owned,"native-sprite-unowned");
            }
            yield return null;
        }

        Type catalog=typeof(MclslAuditAcceptance).Assembly.GetType("MySimulatedLongevityRoad.Systems.MclslSoulFrameCatalog");
        if(catalog!=null)
        {
            var load=catalog.GetMethod("Load",System.Reflection.BindingFlags.Static|System.Reflection.BindingFlags.NonPublic);
            int frames=0;
            foreach(var definition in MclslWorldSoulActorDefinitions.All)
            foreach(string action in new[]{"idle","breathing","run","attack","hit","death"})
            {
                var clip=(Sprite[])load.Invoke(null,new object[]{definition.Folder,action});
                Check(clip.All(sprite=>sprite!=null && sprite.rect.x>=0 && sprite.rect.y>=0
                    && sprite.rect.xMax<=sprite.texture.width && sprite.rect.yMax<=sprite.texture.height),
                    "soul-clip:"+definition.Folder+":"+action);
                frames+=clip.Length;
            }
            Check(frames==904,"all-904-soul-frames-bound");
        }

        foreach(Actor actor in actors.Take(3))
        {
            var clock=Stopwatch.StartNew();
            ScrollWindow.finishAnimations();
            ActionLibrary.openUnitWindow(actor);
            ScrollWindow.finishAnimations();
            yield return null;
            Timings["open-unit:"+actor.asset.id]=clock.Elapsed.TotalMilliseconds;
            UnitWindow window=ScrollWindow.get("unit").GetComponent<UnitWindow>();
            Check(window!=null && window.gameObject.activeInHierarchy && ReferenceEquals(window.actor,actor),"unit-bound:"+actor.asset.id);
            var tabs=window.transform.Find("Background/Tabs").GetComponentsInChildren<WindowMetaTab>(true);
            bool mind=false;
            foreach(var tab in tabs)
            {
                if(!tab.gameObject.activeSelf || !tab.getState()) continue;
                if(tab.tab_elements.Any(t=>t!=null && t.GetComponentInChildren<NeuronsOverview>(true)!=null)) mind=true;
                window.scroll_window.tabs.showTab(tab);
                yield return null;
                Check(ReferenceEquals(window.actor,actor),"tab:"+actor.asset.id+":"+tab.name);
            }
            Check(mind,"mind-tab-visited:"+actor.asset.id);
            SelectedUnit.clear();
            WindowMetaTab mindTab=tabs.First(t=>t.tab_elements.Any(e=>e!=null && e.GetComponentInChildren<NeuronsOverview>(true)!=null));
            window.scroll_window.tabs.startTabAction(mindTab);
            yield return null;
            Check(!window.gameObject.activeInHierarchy,"invalid-selection-closes:"+actor.asset.id);
        }

        // Reopen repeatedly, then kill the selected ordinary unit.
        ActionLibrary.openUnitWindow(actors[0]);
        ScrollWindow.finishAnimations();
        yield return null;
        actors[0].die(true,AttackType.Other,false,false);
        yield return null;yield return null;
        UnitWindow active=ScrollWindow.get("unit").GetComponent<UnitWindow>();
        Check(!active.gameObject.activeInHierarchy || Valid(active.actor),"dead-selection-safe");

        string run=MclslWorldRunRepository.Current.RunId;
        string save=SaveManager.folderPath(Path.Combine(Path.GetDirectoryName(Output),"acceptance-save"));
        MclslWorldArchiveStore.SaveNow();
        SaveManager.saveWorldToDirectory(save,false,false);
        Check(SaveManager.doesSaveExist(save),"save-copy-created");
        World.world.save_manager.loadWorld(save,false);
        yield return null;
        deadline=Time.realtimeSinceStartup+60;
        while(SmoothLoader.isLoading() && Time.realtimeSinceStartup<deadline) yield return null;
        yield return new WaitForSecondsRealtime(2);
        Check(!SmoothLoader.isLoading(),"save-copy-reloaded");
        Check(MclslWorldRunRepository.Current.RunId==run,"world-run-survives-reload");
        Check(Errors.Count==0,"no-runtime-errors-during-suite");
    }
    private void Finish(bool completed)
    {
        Application.logMessageReceived-=Capture;
        Directory.CreateDirectory(Path.GetDirectoryName(Output));
        File.WriteAllText(Output,JsonConvert.SerializeObject(new{
            completed,passed=completed && Errors.Count==0,actualUnity=true,batchMode=Application.isBatchMode,
            interactiveVisualReview=false,product=Application.productName,checks=Checks,errors=Errors,
            units=Units,milliseconds=Timings,persistentDataPath=Application.persistentDataPath},Formatting.Indented));
        UnityEngine.Debug.Log("[MCLSL/Audit/Finished] "+Output);
        Application.Quit(completed && Errors.Count==0 ? 0:1);
    }
}
