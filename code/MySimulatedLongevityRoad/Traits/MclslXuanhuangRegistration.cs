using System;
using System.Collections.Generic;
using System.Linq;
using MySimulatedLongevityRoad.Systems;
using MySimulatedLongevityRoad.UI;
using NeoModLoader.General;
using NeoModLoader.General.UI.Tab;
using UnityEngine;

namespace MySimulatedLongevityRoad.Traits;

internal static class MclslXuanhuangRegistration
{
    internal const string Id="mclsl_xuanhuang";
    internal static ActorAsset Asset {get;private set;}
    internal static void Init()
    {
        MclslLocalizationBridge.RegisterKey(Id,"玄黄族");
        MclslLocalizationBridge.RegisterKey(Id+" Description","与人类同等体型的玄黄智慧种族；使用原版文明与生活行为，拥有独立人物、建筑和船只。首次仅能手动放置。");
        foreach(string template in new[]{"nomads_human","human"})
        {
            string id=template=="human"?Id:"nomads_"+Id;
            var kingdom=AssetManager.kingdoms.clone(id,template);
            kingdom.list_tags.Remove("human");kingdom.list_tags.Add(Id);
            kingdom.friendly_tags.Remove("human");kingdom.friendly_tags.Add(Id);
            kingdom.path_icon="ui/Icons/Xuanhuang";
        }
        ArchitectureAsset human=AssetManager.architecture_library.get("human");
        ArchitectureAsset architecture=AssetManager.architecture_library.clone(Id,"human");
        architecture.building_ids_for_construction=new Dictionary<string,string>();
        var shared=new HashSet<string>{"bonfire","statue","well","stockpile","mine","training_dummy"};
        var map=human.building_ids_for_construction.Values.Distinct().ToDictionary(x=>x,x=>shared.Contains(x)?x:Id+"_"+x);
        foreach(var pair in map.Where(p=>!shared.Contains(p.Key)))
        {
            BuildingAsset source=AssetManager.buildings.get(pair.Key);
            BuildingAsset building=AssetManager.buildings.clone(pair.Value,pair.Key);
            building.sprite_path="buildings/civ_main/"+Id+"/"+pair.Key;
            building.group=Id;building.civ_kingdom=Id;
            building.sprites_are_initiated=false;building.building_sprites=null;
            building.upgrade_to=map.TryGetValue(source.upgrade_to??"",out string upgrade)?upgrade:source.upgrade_to;
            building.upgraded_from=map.TryGetValue(source.upgraded_from??"",out string previous)?previous:source.upgraded_from;
        }
        foreach(var pair in human.building_ids_for_construction)
            architecture.addBuildingOrderKey(pair.Key,map[pair.Value]);
        architecture.shared_building_orders=human.shared_building_orders.Select(x=>(x.Item1,map[x.Item2])).ToArray();
        architecture.actor_asset_id_boat_fishing=CloneBoat(human.actor_asset_id_boat_fishing);
        architecture.actor_asset_id_trading=CloneBoat(human.actor_asset_id_trading);
        architecture.actor_asset_id_transport=CloneBoat(human.actor_asset_id_transport);
        ActorAsset humanActor=AssetManager.actor_library.get("human");
        Asset=AssetManager.actor_library.clone(Id,"human");
        Asset.default_subspecies_traits=new List<string>(humanActor.default_subspecies_traits);
        Asset.decision_ids=humanActor.decision_ids==null?null:new List<string>(humanActor.decision_ids);
        Asset.job=(string[])humanActor.job?.Clone();
        Asset.job_citizen=(string[])humanActor.job_citizen?.Clone();
        Asset.job_kingdom=(string[])humanActor.job_kingdom?.Clone();
        Asset.job_baby=(string[])humanActor.job_baby?.Clone();
        Asset.name_locale=Id;Asset.kingdom_id_wild="nomads_"+Id;Asset.kingdom_id_civilization=Id;
        Asset.architecture_id=Id;Asset.architecture_asset=architecture;
        Asset.icon="Xuanhuang";Asset.banner_id="human";
        Asset.texture_id=Id;
        Asset.skin_citizen_male=new[]{"male_1"};
        Asset.skin_citizen_female=new[]{"female_1"};
        Asset.skin_warrior=new[]{"warrior_1"};
        Asset.need_colored_sprite=false;
        Asset.render_heads_for_babies=false;
        Asset.animation_idle=new[]{"walk_0"};
        Asset.animation_walk=new[]{"walk_0","walk_1","walk_2","walk_3"};
        Asset.animation_swim=new[]{"swim_0","swim_1","swim_2","swim_3"};
        Asset.texture_asset=new ActorTextureSubAsset("actors/species/civs/"+Id+"/",true);
        // Heads are already authored into these sprites; a separate native head
        // would overlap the crown and ignore the authored facing direction.
        ClearHeads(Asset.texture_asset);
        Asset.texture_asset.loadShadow();
        Asset.phenotypes_dict=Asset.phenotypes_dict.ToDictionary(x=>x.Key,x=>new List<string>(x.Value));
        Asset.texture_path_zombie_for_auto_loader_main="actors/species/civs/"+Id+"/zombie";
        Asset.texture_path_zombie_for_auto_loader_heads="";
        SpriteTextureLoader._cached_sprites["ui/Icons/Xuanhuang"]=
            SpriteTextureLoader._cached_sprites["actors/species/civs/"+Id+"/male_1/walk_0"];
        AssetManager.actor_library.loadTexturesAndSprites(Asset);
        string zombieId="zombie_"+Id;
        Asset.setCanTurnIntoZombieAsset(zombieId,false);
        ActorAsset zombie=AssetManager.actor_library.clone(zombieId,"zombie_human");
        zombie.texture_id=zombieId;zombie.need_colored_sprite=false;zombie.dynamic_sprite_zombie=false;
        zombie.render_heads_for_babies=false;
        zombie.use_phenotypes=false;zombie.has_baby_form=false;
        zombie.animation_idle=Asset.animation_idle;zombie.animation_walk=Asset.animation_walk;
        zombie.animation_swim=Asset.animation_swim;
        zombie.texture_asset=new ActorTextureSubAsset("actors/species/civs/"+Id+"/",false);
        zombie.texture_asset.texture_path_main="actors/species/civs/"+Id+"/zombie";
        ClearHeads(zombie.texture_asset);
        zombie.texture_asset.loadShadow();
        AssetManager.actor_library.loadTexturesAndSprites(zombie);
    }
    private static void ClearHeads(ActorTextureSubAsset texture)
    {
        texture.texture_heads="";texture.texture_heads_male="";texture.texture_heads_female="";
        texture.texture_heads_old_male="";texture.texture_heads_old_female="";
        texture.texture_head_king="";texture.texture_head_warrior="";
        texture.has_old_heads=false;
    }
    private static string CloneBoat(string source)
    {
        string id=Id+"_"+source;
        var boat=AssetManager.actor_library.clone(id,source);
        boat.texture_id=id;boat.name_locale=id;
        boat.has_advanced_textures=false;boat.need_colored_sprite=false;
        boat.has_override_sprite=true;
        boat.animation_idle=new[]{"normal"};
        boat.animation_walk=boat.animation_idle;boat.animation_swim=boat.animation_idle;
        string path="actors/boats/"+id;
        boat.texture_asset=new ActorTextureSubAsset(path+"/",false);
        boat.texture_asset.texture_path_main=path;
        ClearHeads(boat.texture_asset);
        boat.texture_asset.loadShadow();
        AssetManager.actor_library.loadTexturesAndSprites(boat);
        MclslLocalizationBridge.RegisterKey(id,"玄黄族"+(source.Contains("trading")?"商船":source.Contains("transport")?"运输船":"渔船"));
        return id;
    }
    internal static void EnsureWildKingdom()
    {
        string id="nomads_"+Id;
        if(World.world.kingdoms_wild.get(id)==null)
            World.world.kingdoms_wild.newWildKingdom(AssetManager.kingdoms.get(id));
    }
    internal static void AddPowers(PowersTab tab)
    {
        const string powerId=Id+"_place";
        MclslLocalizationBridge.RegisterKey(powerId,"玄黄族");
        MclslLocalizationBridge.RegisterKey(powerId+" Description","放置玄黄族；原版人类AI、独立像素文明外观，可正常繁殖与修炼。");
        AssetManager.powers.add(new GodPower { id=powerId,name=Id,actor_asset_id=Id,
            click_action=(tile,_)=>
            {
                if(tile==null)return false;
                EnsureWildKingdom();
                // Use the same adult placement path as native civilization powers.
                // A newborn has no adult settlement decisions until it grows up.
                return World.world.units.createNewUnit(Id,tile,pAdultAge:true)!=null;
            },ignore_cursor_icon=true });
        var button=PowerButtonCreator.CreateGodPowerButton(powerId,SpriteTextureLoader.getSprite("ui/Icons/Xuanhuang"));
        PowersTabExtension.AddPowerButton(tab,"tab",button);
    }
}
