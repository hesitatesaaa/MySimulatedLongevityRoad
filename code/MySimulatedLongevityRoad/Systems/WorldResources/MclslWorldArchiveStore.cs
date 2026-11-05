using System;
using System.Text;
using MySimulatedLongevityRoad.Core;
using MySimulatedLongevityRoad.Data;
using Newtonsoft.Json;

namespace MySimulatedLongevityRoad.Systems;

internal enum MclslWorldArchiveStatus { Waiting, Missing, Primary, Backup, Faulted }
internal enum MclslWorldArchiveSaveResult { Saved, NotReady, Locked, Failed }

internal static class MclslWorldArchiveStore
{
    private const string Key = "mclsl.080.world-state.v1";
    private const string BackupKey = "mclsl.080.world-state.v1.backup";
    private static readonly string[] ReadKeys = { Key, BackupKey };
    private static int _worldSeed = int.MinValue;
    private static MapBox _boundWorld;
    private static long _boundGeneration = -1;
    private static string _knownGoodPrimary;
    internal static MclslWorldArchiveStatus Status { get; private set; }
    internal static bool WriteLocked => Status == MclslWorldArchiveStatus.Faulted;
    internal static bool CanSave => Status == MclslWorldArchiveStatus.Missing
        || Status == MclslWorldArchiveStatus.Primary || Status == MclslWorldArchiveStatus.Backup;

    internal static MclslWorldArchiveStatus Load()
    {
        try
        {
            EnsureWorldIdentity();
            if (Status != MclslWorldArchiveStatus.Waiting) return Status;
            // A missing world is not evidence of a new, empty archive.
            if (World.world?.map_stats == null) return Status;
            SaveCustomData data = EnsureData(false);
            bool existingOrUnreadable = false;
            if (data != null)
            {
                for (int i = 0; i < ReadKeys.Length; i++)
                {
                    try
                    {
                        data.get(ReadKeys[i], out string raw, string.Empty);
                        // Only the missing-value sentinel is empty. Whitespace is
                        // a present but invalid archive and must not authorize reset.
                        if (string.IsNullOrEmpty(raw)) continue;
                        existingOrUnreadable = true;
                        var bundle = JsonConvert.DeserializeObject<MclslWorldArchiveBundle>(raw);
                        if (bundle == null || !MclslWorldRunRepository.ImportArchive(bundle))
                            throw new InvalidOperationException("Archive is null or could not be imported");
                        Status = i == 0 ? MclslWorldArchiveStatus.Primary : MclslWorldArchiveStatus.Backup;
                        _knownGoodPrimary = i == 0 ? raw : null;
                        MclslDiagnostics.Log("archive.read", "[模拟长生路][存档] "
                            + (i == 0 ? "读取成功" : "主档不可用，已使用备份") + "，世界=" + bundle.CurrentRun.RunId);
                        return Status;
                    }
                    catch (Exception ex)
                    {
                        existingOrUnreadable = true;
                        MclslDiagnostics.Alert("archive.key-read:" + i,
                            "[模拟长生路][存档] 档案键" + ReadKeys[i] + "读取失败: " + ex);
                    }
                }
            }
            if (existingOrUnreadable)
                return Fault("主档及备份不可读取，或档案格式不兼容（支持版本 "
                    + MclslSaveVersions.WorldArchive + "）；请恢复有效备份。");
            MclslWorldRunRepository.ResetWorld();
            Status = MclslWorldArchiveStatus.Missing;
        }
        catch (Exception ex) { return Fault("读取失败: " + ex); }
        return Status;
    }

    private static MclslWorldArchiveStatus Fault(string message)
    {
        LockWrites();
        MclslDiagnostics.Fatal("archive.read-failed", "[模拟长生路][存档] " + message + " 已暂停模组世界运行及写入。");
        return Status;
    }

    // Used by the runtime when module initialization fails after a valid load.
    // Direct save callers (era changes, Huanzhen UI) must obey the same lock.
    internal static void LockWrites()
    {
        Status = MclslWorldArchiveStatus.Faulted;
        _boundWorld = World.world;
        _boundGeneration = MclslWorldContext.Generation;
        _knownGoodPrimary = null;
    }

    internal static long Revision { get; private set; }
    internal static void MarkDirty() => Revision++;

    // Native saving owns disk persistence. Two custom_data keys are recovery
    // generations, not an atomic transaction for the whole game save.
    internal static MclslWorldArchiveSaveResult SaveNow()
    {
        Load();
        if (WriteLocked) return MclslWorldArchiveSaveResult.Locked;
        if (!CanSave) return MclslWorldArchiveSaveResult.NotReady;
        try
        {
            SaveCustomData data = EnsureData(true);
            if (data == null) return MclslWorldArchiveSaveResult.NotReady;
            long sample = MclslPerformanceProbe.Begin();
            MclslWorldArchiveBundle archive;
            try { archive = MclslWorldRunRepository.ExportArchive(); }
            finally { MclslPerformanceProbe.End("存档.导出档案", sample); }
            sample = MclslPerformanceProbe.Begin();
            string raw;
            try { raw = JsonConvert.SerializeObject(archive); }
            finally { MclslPerformanceProbe.End("存档.JSON序列化", sample); }
            sample = MclslPerformanceProbe.Begin();
            try
            {
                MclslPerformanceProbe.RecordArchiveBytes(Encoding.UTF8.GetByteCount(raw),
                    _knownGoodPrimary == null ? 0 : Encoding.UTF8.GetByteCount(_knownGoodPrimary));
                // Never rotate an unvalidated primary over a recovery backup.
                // New archives establish their backup before the first primary.
                if (Status == MclslWorldArchiveStatus.Missing) data.set(BackupKey, raw);
                else if (Status == MclslWorldArchiveStatus.Primary) data.set(BackupKey, _knownGoodPrimary);
                data.set(Key, raw);
                _knownGoodPrimary = raw; // Reference only, not another JSON copy.
                Status = MclslWorldArchiveStatus.Primary;
            }
            finally { MclslPerformanceProbe.End("存档.SaveCustomData写入", sample); }
            return MclslWorldArchiveSaveResult.Saved;
        }
        catch (Exception ex)
        {
            MclslDiagnostics.Fatal("archive.write", "[模拟长生路][存档] 写入失败: " + ex);
            return MclslWorldArchiveSaveResult.Failed;
        }
    }

    internal static void Clear()
    {
        Status = MclslWorldArchiveStatus.Waiting;
        _knownGoodPrimary = null;
        Revision = 0;
        _worldSeed = int.MinValue;
        _boundWorld = null;
        _boundGeneration = -1;
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
        // An identity-read exception is also terminal for this world entry.
        // A later save callback must not silently retry a failed load.
        if (WriteLocked && ReferenceEquals(_boundWorld, World.world)
            && _boundGeneration == MclslWorldContext.Generation) return;
        int seed = MapBox.current_world_seed_id;
        long generation = MclslWorldContext.Generation;
        if (_worldSeed == seed && ReferenceEquals(_boundWorld, World.world) && _boundGeneration == generation) return;
        Clear();
        _worldSeed = seed;
        _boundWorld = World.world;
        _boundGeneration = generation;
    }
}
