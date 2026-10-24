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
    private static readonly BindingFlags Private=BindingFlags.Static|BindingFlags.NonPublic;
    internal static void Ensure()
    {
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
        MclslCareerSkillView.Toggle(panel,actor);
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
        MclslCareerSkillView.Toggle(panel,actor);
        Check(!MclslCareerSkillView.Showing(panel),"skill-ui-biography-return");
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
        Config.paused=true;
        WorldTile[] land=World.world.tiles_list.Where(t=>t?.Type!=null&&!t.Type.liquid).ToArray();
        Check(land.Length>100,"land-available");
        WorldTile tile=land[land.Length/2];

        var tabs=UnityEngine.Object.FindObjectsOfType<PowersTab>(true);
        foreach(string id in new[]{"mclsl_living_tab","mclsl_beast_tab","mclsl_immortal_tab","mclsl_sapient_tab"})
            Check(tabs.Any(t=>t.name==id),"tab:"+id);
        var buttons=UnityEngine.Object.FindObjectsOfType<PowerButton>(true);
        buttons.Single(b=>b.name=="mclsl_living_sapient")._button.onClick.Invoke();
        buttons.Single(b=>b.name=="mclsl_sapient_tab_return")._button.onClick.Invoke();
        buttons.Single(b=>b.name=="mclsl_vanilla_xuanhuang")._button.onClick.Invoke();
        var power=AssetManager.powers.get("mclsl_xuanhuang_place");
        Check(power.actor_asset_id==MclslXuanhuangRegistration.Id&&power.click_action(tile,null),"placement-power");
        Check(true,"living-entries");

        Actor male=Spawn(MclslXuanhuangRegistration.Id,tile);
        Actor female=Spawn(MclslXuanhuangRegistration.Id,tile);
        male.data.sex=ActorSex.Male;female.data.sex=ActorSex.Female;
        Check(male.kingdom!=null&&female.kingdom!=null,"xuanhuang-nomad-kingdom");
        Check(AssetManager.kingdoms.get(MclslXuanhuangRegistration.Id).civ,"xuanhuang-civilization-enabled");
        if(!World.world.cities.canStartNewCityCivilizationHere(male))
        {
            tile=land.First(t=>!t.zone.hasCity()&&t.zone.isGoodForNewCity(male)
                &&t.zone.neighbours.All(z=>!z.hasCity()));
            male.spawnOn(tile,0);female.spawnOn(tile,0);
        }
        Check(male.buildCityAndStartCivilization(),"xuanhuang-founded-native-civilization");
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
            inputSha256=InputHash,checks=Checks,errors=Errors,units=Units,buildings=Buildings,
            interactiveVisualReview=false,performanceTested=false,product=Application.productName,
            persistentDataPath=Application.persistentDataPath,atUtc=DateTime.UtcNow.ToString("o")},Formatting.Indented));
        Debug.Log("[MCLSL/070/FINISHED] "+Output);
        Application.Quit(completed&&Errors.Count==0?0:1);
    }
}
