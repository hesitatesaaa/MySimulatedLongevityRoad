using System;
using System.Text;
using MySimulatedLongevityRoad.Core;
using MySimulatedLongevityRoad.Data;
using Newtonsoft.Json;
using UnityEngine;

namespace MySimulatedLongevityRoad.Systems;

internal static class MclslWorldArchiveStore
{
    private const string Key = "mclsl.architecture.v1";
    private const string BackupKey = "mclsl.architecture.v1.backup";
    private static readonly string[] ReadKeys = { Key, BackupKey };
    private static bool _loaded;
    private static bool _writeLocked;
    private static int _worldSeed = int.MinValue;
    private static MapBox _boundWorld;
    internal static bool WriteLocked => _writeLocked;

    internal static void Load()
    {
        EnsureWorldIdentity();
        if (_loaded) return;
        try
        {
            SaveCustomData data = EnsureData(false);
            if (data == null) { _loaded = true; return; }
            bool imported = false;
            bool existingArchive = false;
            for (int i = 0; i < ReadKeys.Length; i++)
            {
                data.get(ReadKeys[i], out string raw, string.Empty);
                if (string.IsNullOrWhiteSpace(raw)) continue;
                existingArchive = true;
                try
                {
                    MclslWorldArchiveBundle bundle = JsonConvert.DeserializeObject<MclslWorldArchiveBundle>(raw);
                    if (bundle == null) continue;
                    if (MclslWorldRunRepository.ImportArchive(bundle)) { imported = true; break; }
                }
                catch (Exception candidateError)
                {
                    Debug.LogWarning("[模拟长生路][存档] 档案键" + ReadKeys[i] + "读取失败，继续尝试备份: " + candidateError.Message);
                }
            }
            if (!imported)
            {
                data.get("mclsl.architecture.v2.envelope", out string v2Envelope, string.Empty);
                data.get("mclsl.architecture.v2", out string v2Archive, string.Empty);
                _writeLocked = existingArchive || !string.IsNullOrWhiteSpace(v2Envelope) || !string.IsNullOrWhiteSpace(v2Archive);
                MclslWorldRunRepository.ResetWorld();
                if (_writeLocked) Debug.LogWarning("[模拟长生路][存档] 检测到不兼容或损坏的旧档，0.5.3 仅支持新世界；已禁止写入。");
            }
            _loaded = true;
        }
        catch (Exception ex) { Debug.LogWarning("[模拟长生路][存档] 读取失败，将等待下一次载入: " + ex.Message); }
    }

    internal static long Revision { get; private set; }
    internal static void MarkDirty() => Revision++;

    // The native save boundary owns persistence. A second periodic serializer
    // wrote only the same in-memory custom_data, while actual saves already
    // publish this authoritative run through PrepareForSave.
    internal static void SaveNow()
    {
        EnsureWorldIdentity();
        Load();
        if (_writeLocked) return;
        try
        {
            SaveCustomData data = EnsureData(true);
            if (!_loaded) _loaded = true;
            if (data == null) return;
            long exportSample = MclslPerformanceProbe.Begin();
            MclslWorldArchiveBundle archive;
            try { archive = MclslWorldRunRepository.ExportArchive(); }
            finally { MclslPerformanceProbe.End("存档.导出档案", exportSample); }
            long serializationSample = MclslPerformanceProbe.Begin();
            string raw;
            try { raw = JsonConvert.SerializeObject(archive); }
            finally { MclslPerformanceProbe.End("存档.JSON序列化", serializationSample); }
            long writeSample = MclslPerformanceProbe.Begin();
            data.get(Key, out string oldRaw, string.Empty);
            MclslPerformanceProbe.RecordArchiveBytes(
                Encoding.UTF8.GetByteCount(raw),
                string.IsNullOrEmpty(oldRaw) ? 0 : Encoding.UTF8.GetByteCount(oldRaw));
            if (!string.IsNullOrWhiteSpace(oldRaw)) data.set(BackupKey, oldRaw);
            data.set(Key, raw);
            if (string.IsNullOrWhiteSpace(oldRaw)) data.set(BackupKey, raw);
            MclslPerformanceProbe.End("存档.SaveCustomData写入", writeSample);
        }
        catch (Exception ex) { Debug.LogError("[模拟长生路][存档] 写入失败: " + ex); }
    }

    internal static void Clear()
    {
        _loaded = false;
        _writeLocked = false;
        Revision = 0;
        _worldSeed = int.MinValue;
        _boundWorld = null;
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
        if (_worldSeed == seed && ReferenceEquals(_boundWorld, World.world)) return;
        _worldSeed = seed;
        _boundWorld = World.world;
        _loaded = false;
        _writeLocked = false;
    }
}
