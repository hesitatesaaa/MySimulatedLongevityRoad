using System;
using System.Collections.Generic;
using MySimulatedLongevityRoad.Core;
using MySimulatedLongevityRoad.Systems;
using NeoModLoader.General;
using NeoModLoader.General.UI.Tab;
using UnityEngine;

namespace MySimulatedLongevityRoad.Traits;

internal static class MclslNamedCharacterRegistration
{
    private sealed class Definition
    {
        internal readonly string Id;
        internal readonly string Name;
        internal readonly string Folder;
        internal readonly bool Beast;
        internal Definition(string id, string name, string folder, bool beast = false)
        { Id = id; Name = name; Folder = folder; Beast = beast; }
        internal string AssetId => "mclsl_named_" + Id;
    }

    private static readonly Definition[] All =
    {
        new("lifan", "李凡", "LiFan"), new("tianyi", "天医", "TianYi"),
        new("moruibin", "墨儒斌", "MoRuBin"),
        new("xuantianwang", "玄天王", "XuanTianWang"), new("xuke", "许克", "XuKe"),
        new("yefeipeng", "叶飞鹏", "YeFeiPeng"), new("xiaoheng", "萧恒", "XiaoHeng"),
        new("xubai", "许白", "XuBai"), new("liping", "李平", "LiPing"),
        new("shanfan", "善凡", "ShanFan"), new("zhangfan", "张凡", "ZhangFan"),
        new("zhenshen", "真神", "ZhenShen"),
        new("diyi", "帝一", "DiYi", true), new("disanmo", "帝叁貘", "DiSanMo", true),
        new("qingshenlong", "青神龙", "QingShenLong", true)
    };
    private static readonly Dictionary<string, Definition> ByAsset = BuildMap();
    private static readonly Dictionary<string, PowerButton> Buttons = new(StringComparer.Ordinal);
    private static bool _initialized;

    internal static bool IsNamedAsset(string assetId)
        => assetId is MclslImmortalActorRegistration.BaiId or MclslImmortalActorRegistration.ChuanfaId
            || assetId != null && ByAsset.ContainsKey(assetId);
    internal static string FolderFor(string assetId)
        => assetId != null && ByAsset.TryGetValue(assetId, out Definition value) ? value.Folder : null;

    internal static void Init()
    {
        if (_initialized || AssetManager.actor_library == null) return;
        ActorAsset human = AssetManager.actor_library.get("human");
        ActorAsset orc = AssetManager.actor_library.get("orc");
        if (human == null || orc == null) return;
        _initialized = true;
        foreach (Definition definition in All)
        {
            if (definition.Beast) continue; // registered as realm-gated beast forms
            try
            {
                ActorAsset template = definition.Beast ? orc : human;
                if (template == null) continue;
                ActorAsset asset = AssetManager.actor_library.get(definition.AssetId)
                    ?? AssetManager.actor_library.clone(definition.AssetId, template.id);
                if (asset == null) continue;
                MclslLocalizationBridge.RegisterKey(definition.AssetId, definition.Name);
                MclslLocalizationBridge.RegisterKey(definition.AssetId + " Description", "仅能手动放置的原著人物模板。");
                asset.name_locale = definition.AssetId;
                asset.can_evolve_into_new_species = false;
                asset.has_baby_form = false;
                asset.use_phenotypes = false;
                asset.has_advanced_textures = false;
                asset.base_stats["scale"] = 0.20f;
                asset.base_stats["birth_rate"] = 0f;
                asset.base_stats["offspring"] = 0f;
                asset.default_subspecies_traits = template.default_subspecies_traits == null
                    ? new List<string>() : new List<string>(template.default_subspecies_traits);
                asset.default_subspecies_traits.RemoveAll(x => x.StartsWith("reproduction_", StringComparison.Ordinal));
                asset.default_subspecies_traits.Add("reproduction_none");
                string path = "actors/Named/" + definition.Folder + "/";
                asset.texture_id = asset.id;
                asset.texture_asset = new ActorTextureSubAsset(path, false);
                asset.texture_asset.texture_path_main = path + "main";
                asset.animation_idle = new[] { "idle_0", "idle_1", "idle_2", "idle_3" };
                asset.animation_walk = new[] { "walk_0", "walk_1", "walk_2", "walk_3" };
                asset.animation_idle_speed = 0.14f;
                asset.animation_walk_speed = 0.10f;
                asset.animation_speed_based_on_walk_speed = false;
                Sprite first = MclslBeastAnimation.LoadSprite(path + "main/idle_0");
                if (first == null)
                    throw new InvalidOperationException("主贴图加载失败: " + path + "main/idle_0");
                asset.cached_sprite = first;
                asset._cached_sprite = first;
                AssetManager.actor_library.loadTexturesAndSprites(asset);
                asset.cached_sprite = first;
                asset._cached_sprite = first;
                asset.get_override_sprite = MclslBeastAnimation.GetSprite;
                asset.has_override_sprite = true;
            }
            catch (Exception ex)
            {
                MclslDiagnostics.Error("named-register:" + definition.Id, "具名人物注册失败: " + ex);
            }
        }
    }

    internal static void AddPowers(PowersTab tab)
    {
        Init();
        if (tab == null) return;
        foreach (Definition definition in All)
        {
            string powerId = definition.AssetId + "_place";
            MclslLocalizationBridge.RegisterKey(powerId, definition.Name);
            MclslLocalizationBridge.RegisterKey(powerId + " Description", "仅手动放置，不会自然出现。");
            if (AssetManager.powers.get(powerId) == null)
                AssetManager.powers.add(new GodPower
                {
                    id = powerId,
                    name = definition.AssetId,
                    click_action = (tile, _) => Place(definition, tile),
                    ignore_cursor_icon = true
                });
            if (Buttons.ContainsKey(powerId)) continue;
            string folder = "actors/Named/" + definition.Folder
                + (definition.Beast ? "/Beast" : "");
            Sprite icon = MclslBeastAnimation.LoadSprite(folder + "/spawn_icon")
                ?? MclslBeastAnimation.LoadSprite(folder + "/main/idle_0")
                ?? AssetManager.actor_library.get(definition.AssetId)?.getSpriteIcon();
            PowerButton button = PowerButtonCreator.CreateGodPowerButton(powerId, icon);
            if (button == null) continue;
            Buttons.Add(powerId, button);
            PowersTabExtension.AddPowerButton(tab, "tab", button);
        }
    }

    private static bool Place(Definition definition, WorldTile tile)
    {
        if (tile == null || World.world?.units == null) return false;
        Actor actor = World.world.units.createNewUnit(definition.AssetId, tile, false, 0f,
            null, null, true, false, false, false);
        if (actor?.data == null) return false;
        actor.data.name = definition.Name;
        if (definition.Beast) MclslBeastActorRegistration.EnsureCultivationSeed(actor);
        return true;
    }

    private static Dictionary<string, Definition> BuildMap()
    {
        var map = new Dictionary<string, Definition>(StringComparer.Ordinal);
        foreach (Definition definition in All) map.Add(definition.AssetId, definition);
        return map;
    }
}
