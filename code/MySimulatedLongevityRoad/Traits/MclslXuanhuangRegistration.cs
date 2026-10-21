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
            kingdom.list_tags=new HashSet<string>(kingdom.list_tags);
            kingdom.friendly_tags=new HashSet<string>(kingdom.friendly_tags);
            kingdom.list_tags.Remove("human");kingdom.list_tags.Add(Id);
            kingdom.friendly_tags.Remove("human");kingdom.friendly_tags.Add(Id);
            kingdom.path_icon="ui/Icons/Xuanhuang";
        }
        ArchitectureAsset human=AssetManager.architecture_library.get("human");
        ArchitectureAsset architecture=AssetManager.architecture_library.clone(Id,"human");
        architecture.building_ids_for_construction=new Dictionary<string,string>();
        var map=human.building_ids_for_construction.Values.Distinct().ToDictionary(x=>x,x=>Id+"_"+x);
        foreach(var pair in map)
        {
            BuildingAsset source=AssetManager.buildings.get(pair.Key);
            BuildingAsset building=AssetManager.buildings.clone(pair.Value,pair.Key);
            // The human construction/upgrade animation is also the Xuanhuang one.
            // The building asset remains independent so its upgrade chain is local.
            building.sprite_path=source.sprite_path;
            building.main_path=source.main_path;
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
        Asset=AssetManager.actor_library.clone(Id,"human");
        Asset.name_locale=Id;Asset.kingdom_id_wild="nomads_"+Id;Asset.kingdom_id_civilization=Id;
        Asset.architecture_id=Id;Asset.architecture_asset=architecture;
        Asset.icon="Xuanhuang";Asset.banner_id="human";
        Asset.texture_id=Id;
        Asset.skin_citizen_male=new[]{"male_1"};
        Asset.skin_citizen_female=new[]{"female_1"};
        Asset.skin_warrior=new[]{"warrior_1"};
        Asset.need_colored_sprite=false;
        Asset.animation_idle=new[]{"walk_0"};
        Asset.animation_walk=new[]{"walk_0","walk_1","walk_2","walk_3"};
        Asset.animation_swim=new[]{"swim_0","swim_1","swim_2","swim_3"};
        Asset.texture_asset=new ActorTextureSubAsset("actors/species/civs/"+Id+"/",true);
        ActorTextureSubAsset humanTexture=AssetManager.actor_library.get("human").texture_asset;
        Asset.texture_asset.texture_heads_male=humanTexture.texture_heads_male;
        Asset.texture_asset.texture_heads_female=humanTexture.texture_heads_female;
        Asset.texture_asset.texture_heads_old_male=humanTexture.texture_heads_old_male;
        Asset.texture_asset.texture_heads_old_female=humanTexture.texture_heads_old_female;
        Asset.texture_asset.texture_head_king=humanTexture.texture_head_king;
        Asset.texture_asset.texture_head_warrior=humanTexture.texture_head_warrior;
        Asset.phenotypes_dict=Asset.phenotypes_dict.ToDictionary(x=>x.Key,x=>new List<string>(x.Value));
        Asset.texture_path_zombie_for_auto_loader_main="actors/species/civs/"+Id+"/zombie";
        Asset.texture_path_zombie_for_auto_loader_heads=
            AssetManager.actor_library.get("human").texture_path_zombie_for_auto_loader_heads;
        SpriteTextureLoader._cached_sprites["ui/Icons/Xuanhuang"]=
            SpriteTextureLoader._cached_sprites["actors/species/civs/"+Id+"/male_1/walk_0"];
        AssetManager.actor_library.loadTexturesAndSprites(Asset);
    }
    private static string CloneBoat(string source)
    {
        string id=Id+"_"+source;
        var boat=AssetManager.actor_library.clone(id,source);
        boat.texture_id=source;boat.name_locale=id;
        MclslLocalizationBridge.RegisterKey(id,"玄黄族"+(source.Contains("trading")?"商船":source.Contains("transport")?"运输船":"渔船"));
        return id;
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
                return World.world.units.createNewUnit(Id,tile,false,0,null,null,true,false,false,false)!=null;
            },ignore_cursor_icon=true });
        var button=PowerButtonCreator.CreateGodPowerButton(powerId,SpriteTextureLoader.getSprite("ui/Icons/Xuanhuang"));
        PowersTabExtension.AddPowerButton(tab,"tab",button);
    }
}
