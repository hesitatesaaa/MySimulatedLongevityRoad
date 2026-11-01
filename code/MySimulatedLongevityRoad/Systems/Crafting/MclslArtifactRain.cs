using System;
using System.Collections.Generic;
using System.Linq;
using ai;
using MySimulatedLongevityRoad.Core;
using MySimulatedLongevityRoad.Data;
using NeoModLoader.General;
using UnityEngine;

namespace MySimulatedLongevityRoad.Systems;

internal static class MclslArtifactRain
{
    internal const string PowerId = "mclsl_artifact_rain";
    private const string DropId = "mclsl_artifact_rain_drop";
    private const string PoolKey = "mclsl_artifact_rain_pool";
    private static readonly HashSet<string> Selected = new(StringComparer.Ordinal);
    private static readonly HashSet<long> RecentActors = new();
    private static PlayerOptionData _option;
    private static float _recentTime = -1f;
    private static bool _loaded;
    private static bool _registered;
    private static readonly MclslItemDefinition[] ChoiceCache = MclslItemCatalog.All
        .Where(x => x.Category == "Artifact" && x.Id is not "B080" and not "B081").ToArray();

    internal static MclslItemDefinition[] Choices => ChoiceCache;

    internal static void Init()
    {
        if (_registered || AssetManager.powers == null || AssetManager.drops == null) return;
        _registered = true;
        if (AssetManager.drops.get(DropId) == null)
            AssetManager.drops.add(new DropAsset
            {
                id = DropId,
                path_texture = "ui/Icons/ArtifactRain",
                random_frame = false,
                default_scale = 0.12f,
                falling_height = new Vector2(30f, 50f),
                action_landed = new DropsAction(OnLanded),
                material = "mat_world_object_lit",
                type = DropType.DropEquipmentRain
            });
        if (AssetManager.powers.get(PowerId) == null)
            AssetManager.powers.add(new GodPower
            {
                id = PowerId,
                name = "mclsl_artifact_rain_name",
                path_icon = "ui/Icons/ArtifactRain",
                type = PowerActionType.PowerDrawTile,
                click_power_brush_action = new PowerAction(DrawRain),
                drop_id = DropId,
                falling_chance = 0.08f,
                draw_lines = true,
                mouse_hold_animation = MouseHoldAnimation.Draw,
                rank = PowerRank.Rank0_free,
                show_tool_sizes = true,
                hold_action = true,
                click_interval = 0.1f
            });
        MclslLocalizationBridge.RegisterKey("mclsl_artifact_rain_name", "法宝雨");
        MclslLocalizationBridge.RegisterKey("mclsl_artifact_rain_name Description", "将已勾选的法宝洒向附近修士。");
    }

    internal static bool DrawRain(WorldTile tile, GodPower power)
    {
        WorldTile center = World.world?.getMouseTilePos();
        DropAsset drop = AssetManager.drops.get(DropId);
        if (center == null || drop == null || SelectedIds().Count == 0) return true;
        int attempts = 0;
        foreach (BrushPixelData pixel in Config.current_brush_data.pos)
        {
            if (attempts++ >= 16) break;
            WorldTile candidate = World.world.GetTile(center.x + pixel.x, center.y + pixel.y);
            if (candidate == null || !Randy.randomChance(power.falling_chance)) continue;
            World.world.drop_manager.spawn(candidate, DropId, Randy.randomFloat(30f, 50f), -1f, -1L);
        }
        return true;
    }

    private static void OnLanded(WorldTile tile = null, string dropId = null)
    {
        if (tile == null) return;
        List<string> pool = SelectedIds();
        if (pool.Count == 0) return;
        if (Time.time - _recentTime >= 1f) { RecentActors.Clear(); _recentTime = Time.time; }
        int granted = 0;
        foreach (Actor actor in Finder.getUnitsFromChunk(tile, 1, 3f, false))
        {
            if (actor?.data == null || !actor.isAlive() || !RecentActors.Add(actor.data.id)) continue;
            string itemId = pool[Randy.randomInt(0, pool.Count)];
            int year = MclslRuntime.CurrentYear();
            MclslBagSystem.Add(actor, itemId, acquiredYear: year);
            MclslWorldRunRepository.AddItemAcquisitionEvent(year, actor, itemId, 1, "法宝雨");
            if (++granted >= 3) break;
        }
    }

    internal static bool IsSelected(string id) { EnsureLoaded(); return Selected.Contains(id); }
    internal static void SetSelected(string id, bool value)
    {
        if (!Choices.Any(x => x.Id == id)) return;
        EnsureLoaded();
        if (value) Selected.Add(id); else Selected.Remove(id);
        Save();
    }
    internal static void SelectAll() { EnsureLoaded(); foreach (MclslItemDefinition item in Choices) Selected.Add(item.Id); Save(); }
    internal static void ClearAll() { EnsureLoaded(); Selected.Clear(); Save(); }
    private static List<string> SelectedIds()
    {
        EnsureLoaded();
        return Choices.Where(x => Selected.Contains(x.Id)).Select(x => x.Id).ToList();
    }
    private static void EnsureLoaded()
    {
        if (_loaded) return;
        _loaded = true;
        PlayerConfigData data = PlayerConfig.instance?.data;
        if (data == null) { foreach (MclslItemDefinition item in Choices) Selected.Add(item.Id); return; }
        _option = data.get(PoolKey);
        if (_option == null)
        {
            _option = data.add(new PlayerOptionData(PoolKey));
            foreach (MclslItemDefinition item in Choices) Selected.Add(item.Id);
            Save();
        }
        else
        {
            foreach (string id in (_option.stringVal ?? string.Empty).Split('|'))
                if (Choices.Any(x => x.Id == id)) Selected.Add(id);
        }
    }
    private static void Save()
    {
        if (_option == null) return;
        _option.stringVal = string.Join("|", Choices.Where(x => Selected.Contains(x.Id)).Select(x => x.Id));
        PlayerConfig.saveData();
    }
}
