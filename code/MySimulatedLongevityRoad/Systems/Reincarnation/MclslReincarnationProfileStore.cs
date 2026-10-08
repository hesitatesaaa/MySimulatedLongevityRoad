using System;
using System.IO;
using MySimulatedLongevityRoad.Core;
using MySimulatedLongevityRoad.Data;
using Newtonsoft.Json;
using UnityEngine;

namespace MySimulatedLongevityRoad.Systems;

internal static class MclslReincarnationProfileStore
{
    private static MclslReincarnationProfile _current = new();
    private static bool _loaded;
    internal static MclslReincarnationProfile Current { get { EnsureLoaded(); return _current; } }

    internal static void EnsureLoaded()
    {
        if (_loaded) return;
        _loaded = true;
        try
        {
            string path = Path.Combine(Application.persistentDataPath, "MySimulatedLongevityRoad", "ArchitectureV1", "ReincarnationProfile.json");
            if (File.Exists(path))
            {
                MclslReincarnationProfile loaded = JsonConvert.DeserializeObject<MclslReincarnationProfile>(File.ReadAllText(path));
                if (loaded == null || loaded.Version != MclslSaveVersions.ReincarnationProfile)
                    throw new InvalidDataException("还真档案格式不属于当前架构。");
                _current = loaded;
            }
        }
        catch (Exception ex) { Debug.LogWarning("[模拟长生路][还真档案] 读取失败: " + ex.Message); }
        _current ??= new MclslReincarnationProfile();
        _current.KnownKnowledgeIds ??= new System.Collections.Generic.List<string>();
        _current.KnownTechniqueIds ??= new System.Collections.Generic.List<string>();
        _current.KnownTimelineAnchorIds ??= new System.Collections.Generic.List<string>();
        _current.CarrySlotLimit = MclslRuntimeSettings.CarrySlotLimit;
    }

    internal static void Flush()
    {
        EnsureLoaded();
        try
        {
            string dir = Path.Combine(Application.persistentDataPath, "MySimulatedLongevityRoad", "ArchitectureV1");
            Directory.CreateDirectory(dir);
            File.WriteAllText(Path.Combine(dir, "ReincarnationProfile.json"), JsonConvert.SerializeObject(_current, Formatting.Indented));
        }
        catch (Exception ex) { Debug.LogError("[模拟长生路][还真档案] 写入失败: " + ex); }
    }
}
