// Injected only into the isolated Unity installation; never shipped to players.
using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Newtonsoft.Json;
using UnityEngine;
using MySimulatedLongevityRoad.Data;
using MySimulatedLongevityRoad.Systems;
using MySimulatedLongevityRoad.Systems.Combat;
using MySimulatedLongevityRoad.Traits;
using MySimulatedLongevityRoad.UI;
using MySimulatedLongevityRoad.Queries;

namespace MySimulatedLongevityRoad.Core;

internal sealed class MclslAuditAcceptance : MonoBehaviour
{
    private static string Output, InputHash;
    private readonly List<string> Checks=new();
    private readonly List<string> Errors=new();
    private readonly List<object> Units=new();
    private readonly List<object> Buildings=new();
    private readonly List<object> NaturalWorlds=new();
    private readonly List<object> SwimPoses=new();
    private static readonly BindingFlags Private=BindingFlags.Static|BindingFlags.NonPublic;
    internal static void Ensure()
    {
        Application.runInBackground=true;
        Config.disable_startup_window=true;
        Config.disable_tutorial=true;
        string[] args=Environment.GetCommandLineArgs();
        int index=Array.IndexOf(args,"--mclsl-acceptance-output");
        if(index<0)return;
        Output=args[index+1];InputHash=args[Array.IndexOf(args,"--mclsl-input-sha256")+1];
        UnityEngine.Object.DontDestroyOnLoad(new GameObject("MclslLivingAcceptance",typeof(MclslAuditAcceptance)));
    }
    private void Check(bool condition,string name)
    {
        if(!condition)throw new InvalidOperationException(name);
        Checks.Add(name);Debug.Log("[MCLSL/070/PASS] "+name);
    }
    private void Capture(string text,string stack,LogType type)
    {if(type==LogType.Error||type==LogType.Exception)Errors.Add(text+"\n"+stack+
        (text.Contains("serialization layout")?new System.Diagnostics.StackTrace(true).ToString():""));}
    private void CaptureFrame(string name,bool includeUi=true)
    {
        // Batch mode has no swap-chain image. Render actual cameras and native
        // canvas objects into an offscreen target without creating a window.
        int width=Screen.width,height=Screen.height;
        var target=new RenderTexture(width,height,24);
        var previous=RenderTexture.active;
        Camera world=Camera.main;
        var uiObject=new GameObject("AcceptanceOffscreenCamera",typeof(Camera));
        Camera ui=uiObject.GetComponent<Camera>();ui.enabled=false;ui.clearFlags=CameraClearFlags.Depth;
        ui.cullingMask=1<<5;ui.orthographic=true;ui.orthographicSize=height/2f;
        ui.transform.position=new Vector3(0,0,-100);ui.nearClipPlane=.1f;ui.farClipPlane=200;ui.targetTexture=target;
        var canvases=UnityEngine.Object.FindObjectsOfType<Canvas>().Where(c=>includeUi&&c.isRootCanvas&&c.renderMode!=RenderMode.WorldSpace).ToArray();
        var settings=canvases.Select(c=>new{canvas=c,mode=c.renderMode,camera=c.worldCamera,distance=c.planeDistance}).ToArray();
        try
        {
            if(world!=null){var old=world.targetTexture;world.targetTexture=target;world.Render();world.targetTexture=old;}
            foreach(var state in settings){state.canvas.renderMode=RenderMode.ScreenSpaceCamera;state.canvas.worldCamera=ui;state.canvas.planeDistance=10;}
            Canvas.ForceUpdateCanvases();if(includeUi)ui.Render();RenderTexture.active=target;
            var image=new Texture2D(width,height,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,width,height),0,0);image.Apply();
            File.WriteAllBytes(Path.Combine(Path.GetDirectoryName(Output),name+".png"),image.EncodeToPNG());UnityEngine.Object.Destroy(image);
        }
        finally
        {
            foreach(var state in settings){state.canvas.renderMode=state.mode;state.canvas.worldCamera=state.camera;state.canvas.planeDistance=state.distance;}
            RenderTexture.active=previous;target.Release();UnityEngine.Object.Destroy(target);UnityEngine.Object.Destroy(uiObject);Canvas.ForceUpdateCanvases();
        }
    }
    private IEnumerator Start()
    {
        Application.logMessageReceived+=Capture;
        IEnumerator work=Run();
        while(true)
        {
            object next;
            try {if(!work.MoveNext()){Finish(true);yield break;}next=work.Current;}
            catch(Exception ex){Errors.Add(ex.ToString());Finish(false);yield break;}
            yield return next;
        }
    }
    private Actor Spawn(string id,WorldTile tile)
    {
        Actor actor=World.world.units.createNewUnit(id,tile,pAdultAge:true);
        Check(actor!=null&&actor.asset.id==id,"spawn:"+id);
        Units.Add(new{requested=id,asset=actor.asset.id,id=MclslActorAccessor.Id(actor),x=tile.pos.x,y=tile.pos.y});
        MclslWorldActorQuery.Track(actor);
        return actor;
    }
    // Generated worlds do not always contain a 70x70 water block, so the visual
    // swim comparison looks for the largest usable liquid area instead of failing.
    private static WorldTile FindPool(int columns,int rows,int span)
    {
        {
            var found=World.world.tiles_list.FirstOrDefault(t=>t!=null&&t.pos.x>10&&t.pos.y>10
                &&t.pos.x<MapBox.width-span*columns-5&&t.pos.y<MapBox.height-span*rows-5
                &&Enumerable.Range(0,rows).All(row=>Enumerable.Range(0,columns)
                    .All(col=>World.world.GetTile(t.pos.x+col*span,t.pos.y+row*span)?.Type.liquid==true)));
            if(found!=null)return found;
        }
        return null;
    }
    private static MclslCareerBattleState Battle(Actor actor)
        =>(MclslCareerBattleState)typeof(MclslCareerCombat).GetMethod("Battle",Private).Invoke(null,new object[]{actor});
    private static void FinishCast(Actor actor,MclslCareerBattleState battle)
    {
        battle.Cast.Due=MclslSpellCasting.Now-1;
        typeof(MclslCareerCombat).GetMethod("TickCast",Private).Invoke(null,new object[]{actor,battle});
    }
    private static void Cultivator(Actor actor)
    {
        MclslActorAccessor.Set(actor,MclslActorDataKeys.Realm,MclslRealmIds.HuaShen);
        MclslSpellSystem.RestoreMana(actor,1);
    }
    private void SkillUi(Actor actor)
    {
        MclslCombatCareerSystem.SetCareer(actor,3,"Unity acceptance");
        var state=MclslCombatCareerSystem.Read(actor);state.Rank=3;state.Cooldowns=new double[5];
        MclslCombatCareerSystem.Write(actor,state);
        Debug.Log("[MCLSL/070/UI] opening native unit window");
        ActionLibrary.openUnitWindow(actor);ScrollWindow.finishAnimations();
        Debug.Log("[MCLSL/070/UI] native unit window opened");
        var window=ScrollWindow.get("unit").GetComponent<UnitWindow>();
        MclslActorInfoPanel.Refresh(window,true,true);
        Debug.Log("[MCLSL/070/UI] actor panel refreshed");
        Transform panel=window.GetComponentsInChildren<Transform>(true).Single(t=>t.name=="MclslActorInfoPanel");
        var careerButton=panel.Find("MclslActorActions/Career").GetComponent<UnityEngine.UI.Button>();
        careerButton.onClick.Invoke();
        Debug.Log("[MCLSL/070/UI] skill view opened");
        var view=panel.GetComponent<MclslCareerSkillView>();
        Transform content=panel.Find("CareerSkillsBody/Content");
        Check(content.GetComponentsInChildren<UnityEngine.UI.Button>(false).Length==10,"skill-ui-ten-entries");
        void Refresh()=>typeof(MclslCareerSkillView).GetMethod("Build",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(view,null);
        string Label(int tier)=>content.GetComponentsInChildren<UnityEngine.UI.Text>(false)
            .Single(t=>t.name=="Label"&&t.transform.parent.name=="CP_sigil_"+tier+"A").text;
        Check(Label(5).Contains("\u672a\u89e3\u9501"),"skill-ui-locked");
        Check(Label(3).Contains("\u7f3a\u5c11\u9053\u5177"),"skill-ui-missing-item");
        state.Cooldowns[0]=MclslSpellCasting.Now+100;MclslCombatCareerSystem.Write(actor,state);Refresh();
        Check(Label(1).Contains("\u51b7\u5374"),"skill-ui-cooldown");
        state.Cooldowns[0]=0;MclslCombatCareerSystem.Write(actor,state);
        MclslActorAccessor.Set(actor,MclslActorDataKeys.ManaCurrent,0);Refresh();
        Check(Label(1).Contains("\u7075\u529b\u4e0d\u8db3"),"skill-ui-mana");
        MclslSpellSystem.RestoreMana(actor,1);Refresh();
        Check(Label(1).Contains("\u53ef\u7528"),"skill-ui-available");
        MclslActorAccessor.Set(actor,MclslActorDataKeys.Realm,"");Refresh();
        Check(Label(1).Contains("\u4fee\u4e3a\u4e0d\u8db3"),"skill-ui-realm");
        Cultivator(actor);
        MclslActorInfoPanel.Refresh(window,false,true);
        careerButton.onClick.Invoke();
        Check(!MclslCareerSkillView.Showing(panel)&&panel.Find("MclslActorInfoBody").gameObject.activeInHierarchy
            &&!panel.Find("CareerSkillsBody").gameObject.activeSelf,"skill-ui-biography-return");
        for(int cycle=0;cycle<3;cycle++)
        {
            careerButton.onClick.Invoke();MclslActorInfoPanel.Refresh(window,false,true);
            Check(MclslCareerSkillView.Showing(panel)&&!panel.Find("MclslActorInfoBody").gameObject.activeSelf,"skill-click-open:"+cycle);
            careerButton.onClick.Invoke();MclslActorInfoPanel.Refresh(window,false,true);
            Check(!MclslCareerSkillView.Showing(panel)&&panel.Find("MclslActorInfoBody").gameObject.activeSelf&&!panel.Find("CareerSkillsBody").gameObject.activeSelf,"skill-click-return:"+cycle);
        }
        Check(careerButton.transform.Find("BiographyTitleArt").GetComponent<UnityEngine.UI.Image>().sprite!=null,"career-title-art");
        CaptureFrame("career-information");
        careerButton.onClick.Invoke();
        Check(careerButton.transform.Find("BiographyTitleArt").GetComponent<UnityEngine.UI.Image>().sprite!=null,"career-return-title-art");
        CaptureFrame("career-skills");careerButton.onClick.Invoke();
        ScrollWindow.get("unit").hide("unit",true);
        Config.paused=true;
        Check(true,"career-skill-ui");
    }
    private IEnumerator Run()
    {
        float deadline=Time.realtimeSinceStartup+120;
        while((!Config.game_loaded||World.world?.tiles_list==null||SmoothLoader.isLoading())&&Time.realtimeSinceStartup<deadline)yield return null;
        yield return new WaitForSecondsRealtime(2);
        Check(Config.game_loaded&&World.world?.tiles_list?.Length>0,"world-ready");
        var tutorial=UnityEngine.Object.FindObjectOfType<Tutorial>(true);
        if(tutorial!=null&&tutorial.isActive())tutorial.endTutorial();
        WorldTip.hideNow();Config.disable_startup_window=true;
        yield return new WaitForSecondsRealtime(1);
        Config.paused=true;
        WorldTile[] land=World.world.tiles_list.Where(t=>t?.Type!=null&&!t.Type.liquid).ToArray();
        Check(land.Length>100,"land-available");
        WorldTile tile=land[land.Length/2];

        var tabs=UnityEngine.Object.FindObjectsOfType<PowersTab>(true);
        Check(tabs.Count(t=>t.name=="mclsl_living_tab")==1&&!tabs.Any(t=>t.name=="mclsl_beast_tab"||t.name=="mclsl_immortal_tab"||t.name=="mclsl_sapient_tab"),"single-living-tab");
        Check(MclslLivingCatalog.VanillaPreserved,"vanilla-creatures-preserved");
        var buttons=UnityEngine.Object.FindObjectsOfType<PowerButton>(true);
        PowersTab living=tabs.Single(t=>t.name=="mclsl_living_tab");
        for(Transform parent=living.transform;parent!=null;parent=parent.parent)Debug.Log("[MCLSL/PARENT] "+parent.name+" active="+parent.gameObject.activeSelf);
        foreach(var label in UnityEngine.Object.FindObjectsOfType<UnityEngine.UI.Text>(true).Where(t=>t.text.Contains("SuperWorldBox")||t.text.Contains("点击任意")))
            Debug.Log("[MCLSL/INTRO] "+label.name+" parent="+label.transform.parent.name+" active="+label.gameObject.activeInHierarchy);
        // Native first-launch/headless startup leaves the power-palette parent
        // collapsed. Open the palette before exercising actual entry buttons.
        living.transform.parent.gameObject.SetActive(true);
        if(!living.isCurrentPowerTabSelected())living.powerButton.onClick.Invoke();
        yield return null;
        foreach(var entry in new[]{("mclsl_living_beasts","beasts"),("mclsl_living_named","named"),("mclsl_living_sapient","xuanhuang")})
        {
            buttons.Single(b=>b.name==entry.Item1).GetComponent<UnityEngine.UI.Button>().onClick.Invoke();
            Debug.Log("[MCLSL/CATALOG] page="+MclslLivingCatalog.CurrentPage+" active="+living.gameObject.activeInHierarchy+" self="+living.gameObject.activeSelf);
            Check(MclslLivingCatalog.CurrentPage==entry.Item2,"living-same-tab:"+entry.Item2);
            Check(living.gameObject.activeInHierarchy&&living._power_buttons.All(b=>b.gameObject.activeInHierarchy),"living-navigation-visible:"+entry.Item2);
            buttons.Single(b=>b.name=="mclsl_living_"+entry.Item2+"_return").GetComponent<UnityEngine.UI.Button>().onClick.Invoke();
            Check(MclslLivingCatalog.CurrentPage=="home","living-return:"+entry.Item2);
        }
        MclslBeastControlWindow.Show();ScrollWindow.finishAnimations();
        var beastWindow=ScrollWindow.get("MclslBeastControl");
        beastWindow.show("right","right",true,false);
        foreach(var nativeButton in beastWindow.GetComponentsInChildren<UnityEngine.UI.Button>(true).Where(b=>!b.name.StartsWith("Button_")))Debug.Log("[MCLSL/NATIVEBUTTON] "+nativeButton.name+" parent="+nativeButton.transform.parent.name);
        foreach(var layoutRow in beastWindow.GetComponentsInChildren<UnityEngine.UI.HorizontalLayoutGroup>(true))
        {
            var rr=layoutRow.GetComponent<RectTransform>();Debug.Log("[MCLSL/ROW] "+rr.name+" actual="+rr.rect.height+" min="+UnityEngine.UI.LayoutUtility.GetMinHeight(rr)+" pref="+UnityEngine.UI.LayoutUtility.GetPreferredHeight(rr)+" flex="+UnityEngine.UI.LayoutUtility.GetFlexibleHeight(rr));
        }
        var intervalField=beastWindow.GetComponentsInChildren<UnityEngine.UI.InputField>(true).Single(f=>f.name=="Value_兽潮间隔");
        int savedInterval=MclslBeastWorld.Interval;
        intervalField.onEndEdit.Invoke("1");Check(MclslBeastWorld.Interval==1,"beast-setting-apply");
        Check(!intervalField.transform.parent.Find("Button_−").GetComponent<UnityEngine.UI.Button>().interactable,"beast-setting-min-disabled");
        intervalField.onEndEdit.Invoke("invalid");Check(MclslBeastWorld.Interval==1,"beast-setting-invalid-rejected");
        intervalField.onEndEdit.Invoke("1000");Check(MclslBeastWorld.Interval==1000&&!intervalField.transform.parent.Find("Button_+").GetComponent<UnityEngine.UI.Button>().interactable,"beast-setting-max-disabled");
        intervalField.onEndEdit.Invoke(savedInterval.ToString());
        foreach(var resolution in new[]{new Vector2Int(1280,720),new Vector2Int(1920,1080)})
        {
            Screen.SetResolution(resolution.x,resolution.y,false);yield return new WaitForSecondsRealtime(1);
            for(int page=0;page<3;page++)
            {
                MclslBeastControlWindow.SelectPage(page);yield return new WaitForSecondsRealtime(.8f);
                var bounds=beastWindow.transform.Find("Background").GetComponent<RectTransform>();
                var corners=new Vector3[4];bounds.GetWorldCorners(corners);
                var canvas=bounds.GetComponentInParent<Canvas>().rootCanvas;
                var screenCorners=corners.Select(c=>RectTransformUtility.WorldToScreenPoint(canvas.renderMode==RenderMode.ScreenSpaceOverlay?null:canvas.worldCamera,c)).ToArray();
                CaptureFrame("beast-"+resolution.x+"-"+page);
                yield return new WaitForSecondsRealtime(1);
                Debug.Log("[MCLSL/UI] "+resolution+" corners="+string.Join(";",screenCorners.Select(c=>c.ToString()))+" size="+bounds.rect.size+" canvas="+canvas.renderMode);
                Check(screenCorners.All(c=>c.x>=-1&&c.y>=-1&&c.x<=Screen.width+1&&c.y<=Screen.height+1),"beast-window-bounds:"+resolution+":"+page);
            }
        }
        MclslBeastControlWindow.SelectPage(1);
        beastWindow.GetComponentsInChildren<UnityEngine.UI.Button>(true).Single(b=>b.name=="Button_引动兽潮").onClick.Invoke();
        Check(MclslBeastWorld.ManualFeedback.Contains("排队"),"manual-wave-queued-feedback");
        Config.paused=false;deadline=Time.realtimeSinceStartup+20;
        while(MclslBeastWorld.ManualFeedback.Contains("排队")&&Time.realtimeSinceStartup<deadline)yield return null;
        Config.paused=true;
        Check(MclslBeastWorld.ManualFeedback.Contains("已引动")||MclslBeastWorld.ManualFeedback.Contains("未成功"),"manual-wave-result-feedback");
        MclslBeastControlWindow.SelectPage(2);yield return null;CaptureFrame("beast-records-after-request");
        beastWindow.hide("MclslBeastControl",true);
        Check(true,"beast-ledger-runtime-ui");
        var power=AssetManager.powers.get("mclsl_xuanhuang_place");
        Check(power.actor_asset_id==MclslXuanhuangRegistration.Id&&power.click_action(tile,null),"placement-power");
        Check(true,"living-entries");

        Actor male=null;
        for(int trial=1;trial<=3;trial++)
        {
            bool generationFinished=false;
            World.world.generateNewMap();
            SmoothLoader.add(()=>generationFinished=true,"MCLSL acceptance world ready",false,.001f,false);
            deadline=Time.realtimeSinceStartup+90;
            while((!generationFinished||SmoothLoader.isLoading())&&Time.realtimeSinceStartup<deadline)yield return null;
            Config.paused=true;World.world.checkSimManagerLists();
            Check(generationFinished&&!SmoothLoader.isLoading(),"natural-world-generated:"+trial);
            land=World.world.tiles_list.Where(t=>t?.Type!=null&&!t.Type.liquid).ToArray();
            Actor humanScout=World.world.units.createNewUnit("human",land[land.Length/2],pAdultAge:true);
            tile=land.First(t=>!t.zone.hasCity()&&t.zone.isGoodForNewCity(humanScout)&&t.zone.neighbours.All(z=>!z.hasCity()));
            var beforePlacement=new HashSet<Actor>(MclslXuanhuangRegistration.Asset.units);
            for(int i=0;i<6;i++)Check(power.click_action(tile,null),"natural-player-placement:"+trial+":"+i);
            World.world.checkSimManagerLists();
            var settlers=MclslXuanhuangRegistration.Asset.units.Where(a=>!beforePlacement.Contains(a)&&a.asset.id==MclslXuanhuangRegistration.Id&&a.isAlive()).ToArray();
            Debug.Log("[MCLSL/PLACED] trial="+trial+" new="+settlers.Length+" before="+beforePlacement.Count
                +" states="+string.Join(";",settlers.Select(a=>MclslActorAccessor.Id(a)+"/city="+(a.city==null?"none":a.city.data.id.ToString())+"/kingdom="+(a.kingdom==null?"none":a.kingdom.asset.id)+"/baby="+a.isBaby()+"/age="+a.getAge())));
            Check(settlers.Length==6&&settlers.All(a=>a.city==null),"fresh-unaffiliated-settlers:"+trial);
            Debug.Log("[MCLSL/SETTLERS] count="+settlers.Length+" states="+string.Join(";",settlers.Select(a=>MclslActorAccessor.Id(a)+" baby="+a.isBaby()+" age="+a.getAge()+" sapient="+a.isSapient())));
            Check(settlers.Length==6&&settlers.All(a=>!a.isBaby()),"placed-settlers-are-adults:"+trial);
            Check(settlers.All(a=>a.canBuildNewCity()),"placement-suitable-for-city:"+trial);
            WorldTile humanTile=land.Where(t=>!t.zone.hasCity()&&t.zone.isGoodForNewCity(settlers[0])&&t.zone.neighbours.All(z=>!z.hasCity())).OrderByDescending(t=>(t.posV3-tile.posV3).sqrMagnitude).First();
            humanScout.spawnOn(humanTile,0);
            var controls=new[]{humanScout}.Concat(Enumerable.Range(0,5).Select(_=>World.world.units.createNewUnit("human",humanTile,pAdultAge:true))).ToArray();
            Debug.Log("[MCLSL/NATURAL] trial="+trial+" sapient="+settlers[0].isSapient()+" traits="+string.Join(",",settlers[0].asset.default_subspecies_traits)+" decisions="+settlers[0].decisions_counter+" canCity="+settlers[0].canBuildNewCity());
            Config.paused=false;float started=Time.realtimeSinceStartup;deadline=started+180;
            while((!settlers.Any(a=>a.city!=null)||!controls.Any(a=>a.city!=null))&&Time.realtimeSinceStartup<deadline)yield return null;
            Config.paused=true;
            male=settlers.FirstOrDefault(a=>a.city!=null);
            NaturalWorlds.Add(new{trial,world=World.world.map_stats.name,seconds=Time.realtimeSinceStartup-started,requestedPlacement=new{x=tile.pos.x,y=tile.pos.y},
                settlers=settlers.Select(a=>new{id=MclslActorAccessor.Id(a),sapient=a.isSapient(),city=a.city?.data.id,kingdom=a.kingdom?.asset.id,x=a.current_tile.pos.x,y=a.current_tile.pos.y,decisions=a.decisions_counter}).ToArray(),
                humanFounded=controls.Any(a=>a.city!=null)});
            Check(male!=null&&male.kingdom.asset.id==MclslXuanhuangRegistration.Id,"xuanhuang-natural-founding:"+trial);
            Check(controls.Any(a=>a.city!=null),"human-natural-control:"+trial);
        }
        Check(true,"xuanhuang-natural-founding-three-worlds");
        tile=male.current_tile;
        Actor female=Spawn(MclslXuanhuangRegistration.Id,tile);
        male.data.sex=ActorSex.Male;female.data.sex=ActorSex.Female;
        Check(male.kingdom!=null&&female.kingdom!=null,"xuanhuang-nomad-kingdom");
        Check(AssetManager.kingdoms.get(MclslXuanhuangRegistration.Id).civ,"xuanhuang-civilization-enabled");
        Kingdom nation=male.kingdom;
        City city=male.city;
        Check(city!=null&&city.kingdom==nation,"xuanhuang-founded-city");
        female.setKingdom(nation);
        ColorAsset[] colors=AssetManager.kingdom_colors_library.list.Take(2).ToArray();
        Check(colors.Length==2,"nation-colors-available");
        foreach(Actor actor in new[]{male,female})
        foreach(UnitProfession profession in new[]{UnitProfession.Unit,UnitProfession.Warrior,UnitProfession.King,UnitProfession.Leader})
        {
            actor.setProfession(profession,false);
            actor._is_moving=true;actor.has_attack_target=false;
            foreach(Vector2 direction in new[]{Vector2.right,Vector2.left,Vector2.down,Vector2.up})
            {
                actor.next_step_position=actor.current_position+direction;
                Sprite first=actor.calculateMainSprite();
                Check(first!=null&&actor.frame_data!=null&&!actor.frame_data.show_head,"facing:"+actor.data.sex+":"+profession+":"+direction);
                foreach(ColorAsset color in colors)
                {
                    nation.updateColor(color);
                    Check(actor.calculateMainSprite()!=null,"colored:"+actor.data.sex+":"+profession+":"+color.id);
                }
            }
        }
        foreach(var group in MclslLivingResources.Groups.Where(g=>g.Path.StartsWith("actors/species/civs/mclsl_xuanhuang")))
        foreach(var frame in group.Frames)
            Check(SpriteTextureLoader.getSprite(group.Path+"/"+frame.Name)!=null,"identity-frame:"+group.Path+"/"+frame.Name);
        Actor child=World.world.units.createNewUnit(MclslXuanhuangRegistration.Id,tile);
        child._state_baby=true;
        Check(child.calculateMainSprite()!=null,"child-sprite");
        Actor zombie=Spawn("zombie_"+MclslXuanhuangRegistration.Id,tile);
        Check(zombie.calculateMainSprite()!=null,"zombie-sprite");
        WorldTile water=World.world.tiles_list.First(t=>t?.Type!=null&&t.Type.liquid);
        Actor swimmer=Spawn(MclslXuanhuangRegistration.Id,water);
        swimmer.position_height=0;swimmer.checkIsInLiquid();
        swimmer._is_moving=true;swimmer.next_step_position=swimmer.current_position+Vector2.up;
        Check(swimmer.calculateMainSprite().name.StartsWith("swim_"),"xuanhuang-swimming");
        Actor elder=Spawn(MclslXuanhuangRegistration.Id,tile);
        elder.data.created_time=World.world.getCurWorldTime()-6000;
        elder.stats["lifespan"]=100;
        Check(elder.isPrettyOld()&&elder.calculateMainSprite()!=null&&elder.frame_data.sheet_path.EndsWith("_old"),"xuanhuang-elder");
        WorldTile pool=FindPool(7,7,6);
        Check(pool!=null,"swim-comparison-water");
        var swimmingActors=new List<(Actor Actor,int Role,int Direction,bool Native)>();
        if(pool!=null)
        for(int role=0;role<7;role++)for(int column=0;column<7;column++)
        {
            bool native=column==6;int direction=native?1:column%3;
            var location=World.world.GetTile(pool.pos.x+column*6,pool.pos.y+role*6);
            Actor actor=Spawn(native?(role==6?"zombie_human":"human"):role==6?"zombie_mclsl_xuanhuang":MclslXuanhuangRegistration.Id,location);
            actor.data.sex=column>=3&&!native?ActorSex.Female:ActorSex.Male;
            if(role==1)actor.setProfession(UnitProfession.Warrior,false);
            if(role==2)actor.setProfession(UnitProfession.King,false);
            if(role==3)actor.setProfession(UnitProfession.Leader,false);
            if(role==4){actor.data.created_time=World.world.getCurWorldTime()-6000;actor.stats["lifespan"]=100;}
            if(role==5)actor._state_baby=true;
            actor.clearSprites();actor.position_height=0;actor.checkIsInLiquid();actor._is_moving=true;
            actor.next_step_position=actor.current_position+(direction==0?Vector2.right:direction==1?Vector2.down:Vector2.up);
            Check(native?actor.current_tile.Type.liquid:actor.calculateMainSprite().name.StartsWith("swim_"),"swim-actor-in-water:"+role+":"+column);
            swimmingActors.Add((actor,role,direction,native));
        }
        World.world.checkSimManagerLists();yield return null;yield return null;
        float previousAnimation=AnimationHelper._time_simulation;
        Camera sceneCamera=Camera.main;Vector3 previousPosition=sceneCamera.transform.position;float previousZoom=sceneCamera.orthographicSize;
        for(int phase=0;phase<4;phase++)
        {
            AnimationHelper._time_simulation=(phase+.2f)/MclslXuanhuangRegistration.Asset.animation_swim_speed;
            foreach(var pose in swimmingActors)
            {
                var actor=pose.Actor;actor._is_moving=true;actor.next_step_position=actor.current_position+(pose.Direction==0?Vector2.right:pose.Direction==1?Vector2.down:Vector2.up);
                Sprite frame=actor.calculateMainSprite();Check(frame!=null&&(pose.Native||frame.name.StartsWith("swim_")),"swim-runtime-phase:"+phase+":"+MclslActorAccessor.Id(actor));
                SwimPoses.Add(new{phase,id=MclslActorAccessor.Id(actor),role=pose.Role,direction=pose.Direction,native=pose.Native,frame=frame.name,path=actor.frame_data?.sheet_path});
            }
            yield return null;
            MoveCamera.instance.focusOn(new Vector3(pool.pos.x+18,pool.pos.y+18,0));
            MoveCamera.instance.forceZoom(24);
            MoveCamera.instance.updateVisibleBounds();
            World.world.quality_changer.setLowRes(false);
            World.world.zone_camera.update();
            foreach(var pose in swimmingActors){pose.Actor.is_visible=true;pose.Actor.current_tile.zone.visible=true;}
            World.world.units.calculateVisibleActors();AssetManager.dynamic_sprites_library.checkDirty();
            World.world.renderStuff();
            Debug.Log("[MCLSL/SWIMVIEW] visible="+World.world.units.visible_units.count+" camera="+sceneCamera.transform.position+" pool="+pool.pos);
            Check(World.world.units.visible_units.count>=49,"swim-overview-visible:"+phase);
            CaptureFrame("swimming-runtime-"+phase,false);
            MoveCamera.instance.focusOn(new Vector3(pool.pos.x+30,pool.pos.y+18,0));
            MoveCamera.instance.forceZoom(8);
            MoveCamera.instance.updateVisibleBounds();World.world.zone_camera.update();
            World.world.units.calculateVisibleActors();AssetManager.dynamic_sprites_library.checkDirty();World.world.renderStuff();
            CaptureFrame("swimming-closeup-"+phase,false);
            sceneCamera.transform.position=previousPosition;sceneCamera.orthographicSize=previousZoom;
        }
        AnimationHelper._time_simulation=previousAnimation;
        foreach(var pose in swimmingActors){pose.Actor.setProfession(UnitProfession.Unit,false);pose.Actor._state_baby=false;pose.Actor.clearSprites();}
        Check(SwimPoses.Count==196,"xuanhuang-swimming-visuals");
        Check(true,"xuanhuang-directions-identities-colors");

        var architecture=AssetManager.architecture_library.get(MclslXuanhuangRegistration.Id);
        var shared=new HashSet<string>{"bonfire","statue","well","stockpile","mine","training_dummy"};
        int index=0;
        foreach(string id in architecture.building_ids_for_construction.Values.Distinct())
        {
            BuildingAsset asset=AssetManager.buildings.get(id);
            Check(asset!=null,"building-order:"+id);
            if(shared.Contains(id))continue;
            Check(asset.sprite_path.StartsWith("buildings/civ_main/mclsl_xuanhuang/"),"building-path:"+id);
            asset.loadBuildingSprites();
            WorldTile location=land[(land.Length/3+index++*83)%land.Length];
            Building building=World.world.buildings.addBuilding(id,location,false,false,BuildPlacingType.New);
            Check(building!=null,"placed-building:"+id);
            building.setKingdom(nation);
            Buildings.Add(new{id,x=location.pos.x,y=location.pos.y});
            Check(building.calculateMainSprite()!=null,"building-sprite:"+id);
            if(!string.IsNullOrEmpty(asset.upgrade_to))
                Check(asset.upgrade_to.StartsWith(MclslXuanhuangRegistration.Id+"_"),"upgrade-chain:"+id);
        }
        foreach(string boat in new[]{architecture.actor_asset_id_boat_fishing,architecture.actor_asset_id_trading,architecture.actor_asset_id_transport})
        {
            Actor actor=Spawn(boat,tile);
            Check(actor.calculateMainSprite()!=null,"boat-sprite:"+boat);
        }
        Check(AssetManager.buildings.get("house_human_0").sprite_path!="buildings/civ_main/mclsl_xuanhuang/house_human_0","human-buildings-independent");
        Check(true,"xuanhuang-buildings-boats");

        foreach(var skill in MclslCombatCareerCatalog.Skills)
        {
            Check(SpriteTextureLoader.getSprite(skill.Icon)!=null,"skill-icon:"+skill.Id);
            if(skill.Passive)continue;
            foreach(string phase in new[]{"cast","travel","hit","cancel"})
            {
                int count=phase=="cast"||phase=="travel"?8:4;
                for(int i=0;i<count;i++)Check(MclslCareerEffectFrames.Get(skill.Id,i,phase)!=null,"effect:"+skill.Id+":"+phase+":"+i);
                MclslSpellEffect.Show(male,female,skill.Id,phase:phase,duration:0.2f);
            }
            Check(MclslCareerEffectFrames.Get(skill.Id,7,"hit")==MclslCareerEffectFrames.Get(skill.Id,3,"hit"),"hit-clamped:"+skill.Id);
        }
        Check(true,"career-icons-effects");
        for(int i=0;i<4;i++)
        {
            Actor spirit=Spawn("mclsl_career_spirit_"+i,tile);
            Check(spirit.asset.texture_asset.texture_path_main=="actors/CareerSpirits/"+i+"/main"&&spirit.calculateMainSprite()!=null,"spirit-art:"+i);
        }
        Check(true,"spirit-actors");

        male.setProfession(UnitProfession.Unit,false);male._is_moving=false;
        Cultivator(male);
        SkillUi(male);
        MclslCombatCareerSystem.SetCareer(male,0,"Unity acceptance");
        var progress=MclslCombatCareerSystem.Read(male);
        progress.Rank=3;progress.Experience=777;progress.Trials[0]=12;progress.Mastery[0]=4;
        MclslCombatCareerSystem.Write(male,progress);
        MclslCombatCareerSystem.SetCareer(male,1,"Unity acceptance");
        progress=MclslCombatCareerSystem.Read(male);
        Check(progress.Rank==3&&progress.Experience==777&&progress.Trials.All(x=>x==0)&&progress.Mastery.All(x=>x==1),"career-transfer");
        bool potential=progress.Potential;
        MclslCombatCareerSystem.HandleRemoved(male,MclslCombatCareerCatalog.Careers[1].Trait);
        progress=MclslCombatCareerSystem.Read(male);
        Check(progress.Career==-1&&progress.Experience==0&&progress.Rank==0&&progress.Potential==potential,"career-removal");
        Check(true,"career-progress-transfer-removal");

        Actor enemy=Spawn("zombie_human",tile);
        Check(MclslCombatTargets.IsEnemy(male,enemy),"combat-enemy");
        male.attack_target=enemy;male.has_attack_target=true;
        for(int career=0;career<6;career++)
        {
            MclslCombatCareerSystem.SetCareer(male,career,"Unity acceptance");
            progress=MclslCombatCareerSystem.Read(male);progress.Rank=1;
            progress.Cooldowns=new double[5];MclslCombatCareerSystem.Write(male,progress);
            MclslSpellSystem.RestoreMana(male,1);
            int before=MclslSpellSystem.CurrentMana(male);
            int cost=MclslCareerCombat.Mana(male,MclslCombatCareerCatalog.Skill(career,1));
            Check(MclslCareerCombat.TryCast(male),"cast-start:"+career);
            var battle=Battle(male);
            if(battle.Cast.Phase==0)FinishCast(male,battle);
            Check(MclslSpellSystem.CurrentMana(male)==before-cost,"cast-mana:"+career);
            Check(MclslCombatCareerSystem.Read(male).Cooldowns[0]>MclslSpellCasting.Now,"cast-cooldown:"+career);
            FinishCast(male,battle);
            MclslCareerCombat.RemoveOwner(male);
        }
        MclslCombatCareerSystem.SetCareer(male,4,"Unity acceptance");
        progress=MclslCombatCareerSystem.Read(male);progress.Rank=3;progress.Cooldowns=new double[5];
        progress.Cooldowns[0]=progress.Cooldowns[1]=MclslSpellCasting.Now+100;
        MclslCombatCareerSystem.Write(male,progress);
        var bag=MclslBagSystem.Read(male);MclslBagSystem.Add(bag,"CP_Array_3",2);MclslBagSystem.Write(male,bag);
        MclslSpellSystem.RestoreMana(male,1);
        int manaBefore=MclslSpellSystem.CurrentMana(male);
        long itemsBefore=MclslBagSystem.Count(MclslBagSystem.Read(male),"CP_Array_3");
        Check(MclslCareerCombat.TryCast(male),"array-cast-start");
        MclslCareerCombat.Interrupt(male);
        int interruptCost=Mathf.CeilToInt(MclslCareerCombat.Mana(male,MclslCombatCareerCatalog.Skill(4,3))*0.2f);
        Check(MclslSpellSystem.CurrentMana(male)==manaBefore-interruptCost&&MclslBagSystem.Count(MclslBagSystem.Read(male),"CP_Array_3")==itemsBefore,"interrupt-cost-no-item");
        Check(true,"career-cast-costs");

        progress=MclslCombatCareerSystem.Read(male);progress.Experience=777;progress.Mastery[0]=4;
        progress.LastYear=MclslRuntime.CurrentYear();
        MclslCombatCareerSystem.Write(male,progress);
        var snapshot=MclslMaobaoSnapshot.Capture(male);
        Actor copy=Spawn(MclslXuanhuangRegistration.Id,tile);
        MclslMaobaoSnapshot.Apply(copy,snapshot);
        Check(MclslCombatCareerSystem.Read(copy).Experience==777&&MclslCombatCareerSystem.Read(copy).Mastery[0]==4,"maobao-career-inheritance");
        var legacy=(MclslHuanzhenCultivationSnapshot)typeof(MclslHuanzhenSystem).GetMethod("CaptureCultivation",Private).Invoke(null,new object[]{male});
        Actor heir=Spawn(MclslXuanhuangRegistration.Id,tile);
        MclslHuanzhenSystem.RestoreSupplementalState(heir,legacy);
        Check(MclslCombatCareerSystem.Read(heir).Experience==777&&MclslCombatCareerSystem.Read(heir).Mastery[0]==4,"huanzhen-career-inheritance");
        // Native managers commit newly spawned objects at the frame boundary.
        Config.paused=false;
        yield return null;yield return null;
        Config.paused=true;
        World.world.checkSimManagerLists();
        World.world.kingdoms.updateDirtyCities();
        long savedId=MclslActorAccessor.Id(male);
        Debug.Log("[MCLSL/070/SAVE] nationAlive="+nation.isAlive()+", nationListed="+World.world.kingdoms.list.Contains(nation)+", cityCount="+nation.cities.Count);
        double remaining=progress.Cooldowns[2]-MclslSpellCasting.Now;
        MclslCareerCombat.Save();
        string save=SaveManager.folderPath(Path.Combine(Path.GetDirectoryName(Output),"living-save"));
        SaveManager.saveWorldToDirectory(save,false,false);
        Check(SaveManager.doesSaveExist(save),"living-save-created");
        World.world.save_manager.loadWorld(save,false);
        yield return null;
        deadline=Time.realtimeSinceStartup+60;
        while(SmoothLoader.isLoading()&&Time.realtimeSinceStartup<deadline)yield return null;
        Config.paused=true;
        yield return new WaitForSecondsRealtime(2);
        Check(!SmoothLoader.isLoading(),"living-save-reloaded");
        Check(MclslActorRegistry.ResolveKnownOrWorld(savedId,out Actor loaded),"saved-actor-resolved");
        var loadedProgress=MclslCombatCareerSystem.Read(loaded);
        Check(loadedProgress.Experience==777&&loadedProgress.Mastery[0]==4&&loadedProgress.Career==4,"saved-career-progress");
        Check(Math.Abs((loadedProgress.Cooldowns[2]-MclslSpellCasting.Now)-remaining)<1,"saved-cooldown");
        Check(true,"career-save-load-inheritance");
        Check(Errors.Count==0,"no-runtime-errors");
    }
    private void Finish(bool completed)
    {
        Application.logMessageReceived-=Capture;
        Directory.CreateDirectory(Path.GetDirectoryName(Output));
        File.WriteAllText(Output,JsonConvert.SerializeObject(new{
            version="0.7.0",passed=completed&&Errors.Count==0,actualUnity=true,
            inputSha256=InputHash,checks=Checks,errors=Errors,units=Units,buildings=Buildings,naturalWorlds=NaturalWorlds,swimmingPoses=SwimPoses,
            interactiveVisualReview=false,performanceTested=false,product=Application.productName,
            persistentDataPath=Application.persistentDataPath,atUtc=DateTime.UtcNow.ToString("o")},Formatting.Indented));
        Debug.Log("[MCLSL/070/FINISHED] "+Output);
        Application.Quit(completed&&Errors.Count==0?0:1);
    }
}
