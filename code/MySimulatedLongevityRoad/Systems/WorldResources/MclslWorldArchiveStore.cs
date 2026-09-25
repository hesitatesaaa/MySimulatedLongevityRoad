using System;
using MySimulatedLongevityRoad.Data;
using Newtonsoft.Json;
using UnityEngine;

namespace MySimulatedLongevityRoad.Systems;

internal static class MclslWorldArchiveStore
{
    private const string Key = "mclsl.archive.v5";
    private const string BackupKey = "mclsl.archive.v5.backup";
    private static readonly string[] ReadKeys =
    {
        Key, BackupKey,
        "mclsl.archive.v4", "mclsl.archive.v4.backup",
        "mclsl.archive.v3", "mclsl.archive.v3.backup",
        "mclsl.archive.v2", "mclsl.archive.v2.backup"
    };
    private static bool _loaded;
    private static bool _dirty;
    private static int _worldSeed = int.MinValue;

    internal static void Load()
    {
        EnsureWorldIdentity();
        if (_loaded) return;
        try
        {
            SaveCustomData data = EnsureData(false);
            if (data == null) { _loaded = true; return; }
            bool imported = false;
            for (int i = 0; i < ReadKeys.Length; i++)
            {
                data.get(ReadKeys[i], out string raw, string.Empty);
                if (string.IsNullOrWhiteSpace(raw)) continue;
                try
                {
                    MclslWorldArchiveBundle bundle = JsonConvert.DeserializeObject<MclslWorldArchiveBundle>(raw);
                    if (bundle == null) continue;
                    bool migrated = MclslWorldRunRepository.ImportArchive(bundle);
                    if (migrated) _dirty = true;
                    imported = true;
                    break;
                }
                catch (Exception candidateError)
                {
                    Debug.LogWarning("[模拟长生路][存档] 档案键" + ReadKeys[i] + "读取失败，继续尝试备份: " + candidateError.Message);
                }
            }
            if (!imported) MclslWorldRunRepository.ResetWorld();
            _loaded = true;
        }
        catch (Exception ex) { Debug.LogWarning("[模拟长生路][存档] 读取失败，将等待下一次载入: " + ex.Message); }
    }

    internal static void MarkDirty() => _dirty = true;

    internal static void SaveNow()
    {
        EnsureWorldIdentity();
        Load();
        try
        {
            SaveCustomData data = EnsureData(true);
            if (!_loaded) _loaded = true;
            if (data == null) return;
            string raw = JsonConvert.SerializeObject(MclslWorldRunRepository.ExportArchive());
            data.get(Key, out string oldRaw, string.Empty);
            if (!string.IsNullOrWhiteSpace(oldRaw)) data.set(BackupKey, oldRaw);
            data.set(Key, raw);
            if (string.IsNullOrWhiteSpace(oldRaw)) data.set(BackupKey, raw);
            _dirty = false;
        }
        catch (Exception ex) { Debug.LogError("[模拟长生路][存档] 写入失败: " + ex); }
    }

    internal static void TickPeriodic(int frame)
    {
        if (_dirty && frame % 1800 == 0) SaveNow();
    }

    internal static void Clear()
    {
        _loaded = false;
        _dirty = false;
        _worldSeed = int.MinValue;
    }

    private static SaveCustomData EnsureData(bool create)
    {
        MapBox world = World.world;
        if (world?.map_stats == null) return null;
        if (world.map_stats.custom_data == null && create) world.map_stats.custom_data = new SaveCustomData();
        return world.map_stats.custom_data;
    }

    private static void EnsureWorldIdentity()
    {
        int seed;
        try { seed = MapBox.current_world_seed_id; } catch { seed = int.MinValue; }
        if (_worldSeed == seed) return;
        _worldSeed = seed;
        _loaded = false;
        _dirty = false;
    }
}
