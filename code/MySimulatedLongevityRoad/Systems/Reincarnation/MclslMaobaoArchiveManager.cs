using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NeoModLoader.General;
using UnityEngine;
using MySimulatedLongevityRoad.Core;
using MySimulatedLongevityRoad.Data;

namespace MySimulatedLongevityRoad.Systems;

/// <summary>
/// 猫宝的完整登名档案。它与世界运行档案分离，避免把跨世界角色快照塞进
/// 单个世界的摘要数据；角色的原生 ActorData 与 Mod custom_data 一起保存。
/// </summary>
internal static class MclslMaobaoArchiveManager
{
    internal const int SavedActorLimit = 49;
    private const string FileName = "mclsl_maobao_saved_actors.json";
    private const int LegacyMigrationVersion = 1;
    private const int RespawnAge = 18;

    private static readonly JsonSerializerSettings JsonSettings = new()
    {
        ContractResolver = new MclslNativeActorDataContractResolver(),
        NullValueHandling = NullValueHandling.Ignore
    };

    private static Dictionary<string, SavedActorPacket> _savedActors = new(StringComparer.Ordinal);
    private static string _selectedActorId = string.Empty;
    private static bool _initialized;

    internal sealed class SavedActorPacket
    {
        public int SchemaVersion { get; set; } = 1;
        public string ActorId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string AssetId { get; set; } = string.Empty;
        public string SaveTime { get; set; } = string.Empty;
        public long SourceActorId { get; set; }
        public int SavedYear { get; set; }
        public ActorData ActorData { get; set; }
    }

    internal static void Init()
    {
        if (_initialized) return;
        _initialized = true;
        MclslWorldRunRepository.EnsureCurrentRun(MclslRuntime.CurrentYear());
        MigrateLegacySummaryRecords();
        LoadFromFile();
    }

    internal static void OnWorldLoaded()
    {
        Init();
        // 每个世界的旧摘要字段都只迁移一次；跨世界完整猫宝档案继续保留。
        MigrateLegacySummaryRecords();
    }

    internal static IReadOnlyList<SavedActorPacket> GetSavedActors()
    {
        Init();
        List<SavedActorPacket> result = new(_savedActors.Count);
        foreach (KeyValuePair<string, SavedActorPacket> pair in _savedActors)
            if (pair.Value?.ActorData != null) result.Add(pair.Value);
        return result;
    }

    internal static bool IsActorSaved(Actor actor)
    {
        Init();
        string id = GetActorStorageId(actor);
        return !string.IsNullOrWhiteSpace(id) && _savedActors.ContainsKey(id);
    }

    internal static bool IsActorSavedById(long actorId)
    {
        Init();
        return actorId > 0L && _savedActors.ContainsKey(actorId.ToString());
    }

    internal static bool SaveActor(Actor actor, out string message)
    {
        Init();
        message = string.Empty;
        if (actor?.data == null || !MclslActorAccessor.Alive(actor))
        {
            message = "猫宝未能照见这名修士。";
            return false;
        }

        try
        {
            actor.prepareForSave();
            string actorId = GetActorStorageId(actor);
            if (string.IsNullOrWhiteSpace(actorId))
            {
                message = "无法取得角色稳定编号。";
                return false;
            }
            if (!_savedActors.ContainsKey(actorId) && _savedActors.Count >= SavedActorLimit)
            {
                message = "猫宝至多保存" + SavedActorLimit + "名角色，请先移除旧档。";
                return false;
            }

            ActorData copy = DeepCopy(actor.data);
            if (copy == null)
            {
                message = "角色完整数据复制失败，未写入猫宝。";
                return false;
            }
            copy.saved_traits = actor.data.saved_traits == null ? null : new List<string>(actor.data.saved_traits);
            SavedActorPacket packet = new()
            {
                ActorId = actorId,
                SourceActorId = MclslActorAccessor.Id(actor),
                Name = SafeName(actor),
                AssetId = actor.asset?.id ?? actor.data.asset_id ?? "human",
                SaveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                SavedYear = MclslRuntime.CurrentYear(),
                ActorData = copy
            };
            _savedActors[actorId] = packet;
            SaveToFile();
            message = "已将“" + packet.Name + "”完整刻入猫宝。";
            return true;
        }
        catch (Exception ex)
        {
            MclslDiagnostics.Error("maobao-save-actor", "猫宝完整保存失败: " + ex.Message);
            message = "猫宝保存失败：" + ex.Message;
            return false;
        }
    }

    internal static bool ToggleActor(Actor actor, out string message)
    {
        if (IsActorSaved(actor)) return RemoveActor(actor, out message);
        return SaveActor(actor, out message);
    }

    internal static bool RemoveActor(Actor actor, out string message)
    {
        return Remove(GetActorStorageId(actor), out message);
    }

    internal static bool Remove(string actorId, out string message)
    {
        Init();
        if (string.IsNullOrWhiteSpace(actorId) || !_savedActors.Remove(actorId))
        {
            message = "未找到这道猫宝完整档案。";
            return false;
        }
        if (string.Equals(_selectedActorId, actorId, StringComparison.Ordinal)) _selectedActorId = string.Empty;
        SaveToFile();
        message = "已从猫宝移除完整档案。";
        return true;
    }

    internal static bool SelectActorToPlace(string actorId)
    {
        Init();
        if (string.IsNullOrWhiteSpace(actorId) || !_savedActors.ContainsKey(actorId)) return false;
        _selectedActorId = actorId;
        return true;
    }

    internal static bool SpawnSavedActor(WorldTile tile, string powerId = null)
    {
        Init();
        if (tile == null || string.IsNullOrWhiteSpace(_selectedActorId)
            || !_savedActors.TryGetValue(_selectedActorId, out SavedActorPacket packet)
            || packet?.ActorData == null) return false;

        ActorData source = DeepCopy(packet.ActorData);
        if (source == null) return false;
        source.asset_id = string.IsNullOrWhiteSpace(packet.AssetId) ? source.asset_id : packet.AssetId;
        if (string.IsNullOrWhiteSpace(source.asset_id)) source.asset_id = "human";
        source.id = World.world.map_stats.getNextId("unit");
        source.x = tile.pos.x;
        source.y = tile.pos.y;
        source.cityID = -1L;
        source.civ_kingdom_id = -1L;
        source.homeBuildingID = -1L;
        source.transportID = -1L;
        source.clan = -1L;
        source.family = -1L;
        source.army = -1L;
        source.lover = -1L;
        source.best_friend_id = -1L;
        source.parent_id_1 = -1L;
        source.parent_id_2 = -1L;
        source.ancestor_family = -1L;
        source.created_time = World.world.getCurWorldTime();
        source.died_time = 0d;
        source.age_overgrowth = RespawnAge;

        try
        {
            // 直接使用原生 loadObject 恢复 ActorData、物品列表、特质和 custom_data，
            // 再以新 ID/坐标接入当前世界，避免复制旧世界的空间索引。
            Actor actor = World.world.units.loadObject(source);
            if (actor?.data == null) return false;
            actor.data.saved_traits = source.saved_traits == null ? null : new List<string>(source.saved_traits);
            actor.setName(string.IsNullOrWhiteSpace(packet.Name) ? actor.getName() : packet.Name, false);
            actor.clearTraitCache();
            actor.setStatsDirty();
            actor.updateStats();
            actor.clearOldPath();
            actor.stopMovement();
            _selectedActorId = string.Empty;
            return true;
        }
        catch (Exception ex)
        {
            MclslDiagnostics.Error("maobao-spawn-actor", "猫宝放置完整角色失败: " + ex.Message);
            return false;
        }
    }

    internal static int RefreshLiveSnapshots()
    {
        Init();
        int refreshed = 0;
        List<KeyValuePair<string, SavedActorPacket>> entries = new(_savedActors);
        for (int i = 0; i < entries.Count; i++)
        {
            SavedActorPacket packet = entries[i].Value;
            if (packet == null || packet.SourceActorId <= 0L) continue;
            if (!MclslActorRegistry.ResolveKnownOrWorld(packet.SourceActorId, out Actor actor) || !MclslActorAccessor.Alive(actor)) continue;
            if (SaveActor(actor, out _)) refreshed++;
        }
        return refreshed;
    }

    private static void MigrateLegacySummaryRecords()
    {
        MclslWorldRunState run = MclslWorldRunRepository.Current;
        if (run == null || run.MaobaoArchiveMigrationVersion >= LegacyMigrationVersion) return;
        // 用户已选择清空旧的摘要记录；新档案不读取它们，避免把旧摘要误当完整登名石。
        run.MaobaoRecords ??= new List<MclslMaobaoRecord>();
        run.MaobaoRecords.Clear();
        run.MaobaoArchiveMigrationVersion = LegacyMigrationVersion;
        MclslWorldArchiveStore.MarkDirty();
    }

    private static string SavePath => Path.Combine(Application.persistentDataPath, FileName);

    private static void SaveToFile()
    {
        string temp = SavePath + ".tmp";
        string backup = SavePath + ".bak";
        try
        {
            string json = JsonConvert.SerializeObject(_savedActors, Formatting.Indented, JsonSettings);
            File.WriteAllText(temp, json);
            if (File.Exists(SavePath)) File.Copy(SavePath, backup, true);
            if (File.Exists(SavePath)) File.Delete(SavePath);
            File.Move(temp, SavePath);
        }
        catch (Exception ex)
        {
            MclslDiagnostics.Error("maobao-save-file", "猫宝存档写入失败: " + ex.Message);
            try { if (File.Exists(temp)) File.Delete(temp); } catch { }
        }
    }

    private static void LoadFromFile()
    {
        string primary = SavePath;
        string backup = SavePath + ".bak";
        string source = File.Exists(primary) ? primary : File.Exists(backup) ? backup : string.Empty;
        if (string.IsNullOrWhiteSpace(source)) return;
        try
        {
            _savedActors = JsonConvert.DeserializeObject<Dictionary<string, SavedActorPacket>>(File.ReadAllText(source), JsonSettings)
                ?? new Dictionary<string, SavedActorPacket>(StringComparer.Ordinal);
            List<string> invalid = new();
            foreach (KeyValuePair<string, SavedActorPacket> pair in _savedActors)
                if (pair.Value?.ActorData == null) invalid.Add(pair.Key);
            for (int i = 0; i < invalid.Count; i++) _savedActors.Remove(invalid[i]);
            if (!string.Equals(source, primary, StringComparison.Ordinal)) SaveToFile();
        }
        catch (Exception ex)
        {
            MclslDiagnostics.Error("maobao-load-file", "猫宝存档读取失败: " + ex.Message);
            _savedActors = new Dictionary<string, SavedActorPacket>(StringComparer.Ordinal);
        }
    }

    private static ActorData DeepCopy(ActorData data)
    {
        if (data == null) return null;
        try { return JsonConvert.DeserializeObject<ActorData>(JsonConvert.SerializeObject(data, JsonSettings), JsonSettings); }
        catch (Exception ex)
        {
            MclslDiagnostics.Error("maobao-deep-copy", "角色数据深拷贝失败: " + ex.Message);
            return null;
        }
    }

    private static string GetActorStorageId(Actor actor)
    {
        long id = MclslActorAccessor.Id(actor);
        return id > 0L ? id.ToString() : string.Empty;
    }

    private static string SafeName(Actor actor)
    {
        try { return string.IsNullOrWhiteSpace(actor?.getName()) ? "未名角色" : actor.getName().Trim(); }
        catch { return "未名角色"; }
    }
}

/// <summary>ActorData 在不同 WorldBox 版本中的私有字段也需要被登名快照保留。</summary>
internal sealed class MclslNativeActorDataContractResolver : DefaultContractResolver
{
    protected override List<MemberInfo> GetSerializableMembers(Type objectType)
    {
        List<MemberInfo> result = base.GetSerializableMembers(objectType);
        HashSet<string> names = new(StringComparer.Ordinal);
        for (int i = 0; i < result.Count; i++) names.Add(result[i].Name);
        MemberInfo[] members = objectType.GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        for (int i = 0; i < members.Length; i++)
        {
            MemberInfo member = members[i];
            if ((member.MemberType != MemberTypes.Field && member.MemberType != MemberTypes.Property)
                || member.GetCustomAttribute<JsonIgnoreAttribute>(true) != null
                || !names.Add(member.Name)) continue;
            if (member is PropertyInfo property
                && (property.GetIndexParameters().Length != 0 || (property.GetGetMethod(true) == null && property.GetSetMethod(true) == null))) continue;
            result.Add(member);
        }
        return result;
    }
}
