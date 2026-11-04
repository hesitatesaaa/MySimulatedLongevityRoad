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
        MclslXuanhuangBuildingShadow.Init();
        MclslStaticIconAssets.Init();
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
        // Include shared orders and the complete upgrade graph, not only entry tiers.
        var originals=new HashSet<string>(human.building_ids_for_construction.Values);
        foreach(var order in human.shared_building_orders)originals.Add(order.Item2);
        var pending=new Queue<string>(originals);
        while(pending.Count>0)
        {
            var source=AssetManager.buildings.get(pending.Dequeue());
            foreach(string link in new[]{source.upgrade_to,source.upgraded_from})
                if(!string.IsNullOrEmpty(link)&&originals.Add(link))pending.Enqueue(link);
        }
        // User-approved shared utility art: bonfire and stockpile remain native.
        var shared=new HashSet<string>{"bonfire","stockpile"};
        var map=originals.ToDictionary(x=>x,x=>shared.Contains(x)?x:Id+"_"+x);
        foreach(var pair in map.Where(p=>!shared.Contains(p.Key)))
        {
            BuildingAsset source=AssetManager.buildings.get(pair.Key);
            BuildingAsset building=AssetManager.buildings.clone(pair.Value,pair.Key);
            building.sprite_path="buildings/civ_main/"+Id+"/"+pair.Key;
            building.group=Id;building.civ_kingdom=Id;
            // The two dedicated windmill tiers use 105% of the source building's
            // current scale on X/Y so their base covers the grass below; the tent
            // uses one further 120% step on top of the 115% baseline; every other
            // dedicated building grows to 115%. Z, footprint, pivot and upgrade
            // graph stay native; windmill frames/pivot are not cropped.
            float scale= pair.Key.StartsWith("windmill_")?1.05f:pair.Key=="tent_human"?1.15f*1.2f:1.15f;
            building.scale_base=new Vector3(source.scale_base.x*scale,source.scale_base.y*scale,source.scale_base.z);
            // Four blade poses represent 22.5-degree steps; native playback owns timing.
            if(pair.Key.StartsWith("windmill_"))building.animation_speed=4f;
            building.sprites_are_initiated=false;building.building_sprites=null;
            // AssetLibrary.clone copies serialized fields only, so this runtime-only
            // atlas link stays null on every clone. BuildingLibrary.linkAssets already
            // ran before the mod loaded, so the clone would never be linked again.
            // Building.calculateColoredSprite dereferences atlas_asset the first time a
            // building becomes visible; null there throws inside MapBox.Update and
            // freezes every remaining step of the frame. Relink it like vanilla does.
            string atlasId=string.IsNullOrEmpty(building.atlas_id)?"buildings":building.atlas_id;
            building.atlas_asset=source.atlas_asset??AssetManager.dynamic_sprites_library?.get(atlasId);
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
        boat.get_override_sprite=AssetManager.actor_library.get(source).get_override_sprite;
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
        if(MySimulatedLongevityRoad.Core.MclslDeveloperBridge.IsAvailable)
            AddBuildingPowers(tab);
    }

    internal static string BuildingPowerId(string buildingId)=>Id+"_place_building_"+buildingId;

    private static void AddBuildingPowers(PowersTab tab)
    {
        // Read the live architecture so every construction tier and shared
        // public building gets an entry, including buildings added later.
        var architecture=AssetManager.architecture_library.get(Id);
        foreach(string buildingId in architecture.building_ids_for_construction.Values.Distinct())
        {
            BuildingAsset building=AssetManager.buildings.get(buildingId);
            string powerId=BuildingPowerId(buildingId);
            string label="玄黄族·"+BuildingLabel(buildingId);
            MclslLocalizationBridge.RegisterKey(powerId,label);
            MclslLocalizationBridge.RegisterKey(powerId+" Description",
                "开发者放置："+label+"。选择后点击空地，直接放置完整建筑，无需科技或材料；码头需合适海岸，建筑占地内须无其他建筑。领地内归属当地城市，野外可作为建筑展示。");
            AssetManager.powers.add(new GodPower { id=powerId,name=powerId,
                click_action=(tile,_)=>PlaceBuilding(building,tile),ignore_cursor_icon=true });
            building.loadBuildingSprites();
            Sprite icon=building.building_sprites?.animation_data
                .SelectMany(animation=>animation.main??Array.Empty<Sprite>()).FirstOrDefault()
                ??SpriteTextureLoader.getSprite("ui/Icons/Xuanhuang");
            var button=PowerButtonCreator.CreateGodPowerButton(powerId,icon);
            PowersTabExtension.AddPowerButton(tab,"tab",button);
        }
    }

    private static bool PlaceBuilding(BuildingAsset asset,WorldTile tile)
    {
        if(tile==null||World.world==null||asset==null)return false;
        bool shore=false;
        var footprint=asset.fundament;
        for(int x=0;x<footprint.width;x++)for(int y=0;y<footprint.height;y++)
        {
            WorldTile part=World.world.GetTile(tile.x-footprint.left+x,tile.y-footprint.bottom+y);
            if(part==null||part.building!=null)return false;
            if(!asset.docks)continue;
            if(!part.Type.ocean)return false;
            shore|=part.neighbours.Any(neighbour=>neighbour!=null&&neighbour.Type.ground);
        }
        if(asset.docks&&!shore)return false;
        // Native New dock placement assumes an owning city and compares its
        // island. The developer can also place an unowned harbor for review:
        // check that shoreline above, then retain native terrain checks via Load.
        var placing=asset.docks?BuildPlacingType.Load:BuildPlacingType.New;
        if(!World.world.buildings.canBuildFrom(tile,asset,null,placing))return false;
        return World.world.buildings.addBuilding(asset,tile,false,false,BuildPlacingType.New)!=null;
    }

    private static string BuildingLabel(string id)
    {
        string original=id.StartsWith(Id+"_")?id.Substring(Id.Length+1):id;
        if(original.StartsWith("house_human_"))return "住宅·"+(int.Parse(original.Substring("house_human_".Length))+1)+"级";
        if(original.StartsWith("hall_human_"))return "主厅·"+(int.Parse(original.Substring("hall_human_".Length))+1)+"级";
        if(original.StartsWith("windmill_human_"))return "风车·"+(int.Parse(original.Substring("windmill_human_".Length))+1)+"级";
        return original switch
        {
            "fishing_docks_human"=>"渔业码头", "tent_human"=>"帐篷", "docks_human"=>"港口",
            "watch_tower_human"=>"瞭望塔", "temple_human"=>"神庙", "library_human"=>"图书馆",
            "market_human"=>"市场", "barracks_human"=>"兵营", "bonfire"=>"篝火",
            "statue"=>"雕像", "well"=>"水井", "stockpile"=>"仓储堆场", "mine"=>"矿井",
            "training_dummy"=>"训练木桩", _=>LM.Get(original)
        };
    }
}
