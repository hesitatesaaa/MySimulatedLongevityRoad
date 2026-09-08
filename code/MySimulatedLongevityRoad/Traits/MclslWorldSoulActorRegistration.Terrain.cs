using System;
using System.Collections.Generic;
using MySimulatedLongevityRoad.Core;
using MySimulatedLongevityRoad.Data;
using MySimulatedLongevityRoad.Systems;
using UnityEngine;

namespace MySimulatedLongevityRoad.Traits;

internal static partial class MclslWorldSoulActorRegistration
{
    private const int MaxTerrainTilesPerTask = 256;
    private const int MaxPendingTerrainTasks = 32;
    private const int TerrainTaskQuantum = 8;
    private const int TerrainDamageRadius = 48;
    private static readonly Queue<TerrainDamageTask> PendingTerrainTasks = new();

    private sealed class TerrainDamageTask
    {
        private readonly Vector2Int _center;
        private readonly int _radius;
        private readonly int _radiusSquared;
        private readonly int _width;
        private readonly int _totalCells;
        private readonly int _samplingStride;
        private readonly string _effectType;
        private int _nextLinearIndex;
        private int _appliedTiles;

        internal TerrainDamageTask(WorldTile centerTile, int radius, string effectType)
        {
            _center = centerTile.pos;
            _radius = Mathf.Max(1, radius);
            _radiusSquared = _radius * _radius;
            _width = _radius * 2 + 1;
            _totalCells = _width * _width;
            int estimatedTileCount = Mathf.Max(1, Mathf.RoundToInt(Mathf.PI * _radiusSquared));
            _samplingStride = Mathf.Max(1, Mathf.CeilToInt((float)estimatedTileCount / MaxTerrainTilesPerTask));
            _effectType = effectType;
        }

        internal bool Advance(MapBox world, int budget, out int consumed)
        {
            consumed = 0;
            while (consumed < budget && _nextLinearIndex < _totalCells && _appliedTiles < MaxTerrainTilesPerTask)
            {
                int index = _nextLinearIndex;
                _nextLinearIndex += _samplingStride;
                consumed++;
                int dx = index % _width - _radius;
                int dy = index / _width - _radius;
                if (dx * dx + dy * dy > _radiusSquared) continue;

                WorldTile tile = TryGetTile(world, _center.x + dx, _center.y + dy);
                if (tile != null && TryApplyTerrainEffectToTile(tile, _effectType))
                    _appliedTiles++;
            }
            return _nextLinearIndex >= _totalCells || _appliedTiles >= MaxTerrainTilesPerTask;
        }
    }

    internal static void TickTerrainEffects()
    {
        if (PendingTerrainTasks.Count == 0) return;
        MapBox world = World.world;
        if ((UnityEngine.Object)(object)world == (UnityEngine.Object)null) return;
        int remaining = MclslRuntimeWorkBudget.ScaleCount(24, 4);
        int tasksThisPass = PendingTerrainTasks.Count;
        for (int i = 0; i < tasksThisPass && remaining > 0 && PendingTerrainTasks.Count > 0; i++)
        {
            TerrainDamageTask task = PendingTerrainTasks.Dequeue();
            int quantum = Mathf.Min(TerrainTaskQuantum, remaining);
            bool completed = task.Advance(world, quantum, out int consumed);
            remaining -= consumed;
            if (!completed) PendingTerrainTasks.Enqueue(task);
        }
    }

    private static void QueueTerrainDamage(Actor source)
    {
        if (PendingTerrainTasks.Count >= MaxPendingTerrainTasks) return;
        MapBox world = World.world;
        if ((UnityEngine.Object)(object)world == (UnityEngine.Object)null) return;
        WorldTile center = TryGetTile(world, SafeX(source), SafeY(source));
        if (center == null) return;
        string effect = TerrainEffectFor(source);
        if (string.IsNullOrWhiteSpace(effect)) return;
        PendingTerrainTasks.Enqueue(new TerrainDamageTask(center, TerrainDamageRadius, effect));
    }

    private static string TerrainEffectFor(Actor source)
    {
        string soulId = MclslActorAccessor.GetString(source, MclslActorDataKeys.WorldSoulEntityId, string.Empty).ToLowerInvariant();
        MclslWorldSoulRecord record = FindWorldSoulById(soulId);
        string tags = ((record?.LawTags ?? string.Empty) + "|" + soulId).ToLowerInvariant();
        if (tags.Contains("雷") || tags.Contains("thunder")) return "CommonLeiFaTerrain";
        if (tags.Contains("火") || tags.Contains("阳") || tags.Contains("fire") || tags.Contains("yang")) return "FireTerrain";
        if (tags.Contains("水") || tags.Contains("阴") || tags.Contains("寒") || tags.Contains("water") || tags.Contains("yin")) return "FrozenTerrain";
        return "SmashTerrain";
    }

    private static WorldTile TryGetTile(MapBox world, int x, int y)
    {
        if ((UnityEngine.Object)(object)world == (UnityEngine.Object)null || x < 0 || y < 0 || x >= MapBox.width || y >= MapBox.height)
            return null;
        try { return world.GetTileSimple(x, y); }
        catch { return null; }
    }

    private static bool TryApplyTerrainEffectToTile(WorldTile tile, string effectType)
    {
        if (tile == null) return false;
        try
        {
            TerraformOptions options = new TerraformOptions
            {
                damage_buildings = true,
                destroy_buildings = true,
                make_ruins = true,
                remove_roads = true,
                remove_trees_fully = true,
                damage = 0
            };

            if (string.Equals(effectType, "FrozenTerrain", StringComparison.Ordinal))
            {
                if (tile.canBeFrozen()) tile.freeze(6);
            }
            else if (string.Equals(effectType, "CommonLeiFaTerrain", StringComparison.Ordinal))
            {
                options.set_fire = true;
                options.add_burned = true;
                options.add_heat = 25;
                options.lightning_effect = true;
                options.shake = true;
                options.shake_duration = 600f;
                options.shake_intensity = 0.003f;
            }
            else if (string.Equals(effectType, "FireTerrain", StringComparison.Ordinal))
            {
                options.set_fire = true;
                options.add_burned = true;
                options.add_heat = 90;
            }
            else if (!string.Equals(effectType, "SmashTerrain", StringComparison.Ordinal)) return false;

            MapAction.decreaseTile(tile, false, options);
            return true;
        }
        catch { return false; }
    }
}
