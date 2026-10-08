using System.Text.Json;
using MySimulatedLongevityRoad.Data;

namespace Newtonsoft.Json
{
    internal static class JsonConvert
    {
        public static T? DeserializeObject<T>(string raw) => JsonSerializer.Deserialize<T>(raw);
        public static string SerializeObject<T>(T value) => JsonSerializer.Serialize(value);
    }
}

namespace NeoModLoader.General
{
    internal static class LM { public static string Get(string key) => key; }
}

namespace MySimulatedLongevityRoad.Core
{
    internal static class MclslDeveloperBridge
    {
        internal static bool IsAvailable;
    }
    internal static class MclslRuntimeWorkBudget
    {
        internal static int ScaleCount(int normal, int minimum) => normal;
    }
}

namespace UnityEngine
{
    internal static class Time { internal static float unscaledTime; internal static float timeScale = 1f; internal static int frameCount; }
    internal static class Debug
    {
        public static void LogWarning(string _) { }
        public static void LogError(string _) { }
    }
}

internal sealed class SaveCustomData
{
    internal readonly Dictionary<string, string> Values = new();
    public void get(string key, out string value, string fallback)
        => value = Values.TryGetValue(key, out string? saved) ? saved : fallback;
    public void set(string key, string value) => Values[key] = value;
}

internal sealed class MapStats { public SaveCustomData? custom_data; }
internal sealed class MapBox
{
    public static int current_world_seed_id;
    public MapStats map_stats = new();
}
internal static class World { public static MapBox? world; }
internal static class Config { internal static bool paused; }
internal static class LocalizedTextManager { internal static string getText(string key) => key; }

internal sealed class ActorData { internal double created_time; }
internal sealed class ActorAsset
{
    internal string id = "human";
    internal bool is_boat = false;
    internal string kingdom_id_civilization = "civilization";
    internal List<string> traits = new();
    internal List<string> default_subspecies_traits = new();
}
internal sealed class Subspecies
{
    internal List<string> default_traits = new();
    internal List<string> saved_traits = new();
}
internal sealed class Actor
{
    internal readonly ActorData data = new();
    internal readonly HashSet<string> Traits = new();
    internal readonly Dictionary<string, int> Values = new();
    internal readonly Dictionary<string, string> StringValues = new();
    internal ActorStats stats = new();
    internal string Realm = string.Empty;
    internal string Name = "Test Actor";
    internal long Id;
    internal int Age;
    internal bool Sapient = true;
    internal ActorAsset asset = new();
    internal Subspecies subspecies = new();
    public bool isSapient() => Sapient;
    public bool hasTrait(string id) => Traits.Contains(id);
    public bool addTrait(string id) => Traits.Add(id);
    public bool removeTrait(string id) => Traits.Remove(id);
    public int getAge() => Age;
}
internal sealed class ActorStats
{
    private readonly Dictionary<string, float> values = new();
    public float this[string key]
    {
        get => values.TryGetValue(key, out float value) ? value : 0f;
        set => values[key] = value;
    }
}
internal sealed class BaseStats
{
    internal readonly Dictionary<string, float> Values = new();
    public void set(string id, float value) => Values[id] = value;
}
internal sealed class ActorTrait
{
    internal string group_id = string.Empty;
    internal BaseStats base_stats = new();
    internal int Rarity;
}
internal sealed class TraitLibrary
{
    internal readonly Dictionary<string, ActorTrait> Values = new();
    public ActorTrait? get(string id) => Values.GetValueOrDefault(id);
}
internal static class AssetManager { internal static readonly TraitLibrary traits = new(); }

namespace MySimulatedLongevityRoad.Core
{
    internal static class MclslRuntime { internal static int Year = 1; internal static int CurrentYear() => Year; }
    internal static class MclslRuntimeSettings { internal static bool CoreEnabled = true; internal static bool AnnualBackpressureEnabled = true; }
    internal enum MclslActorChange { Traits }
    internal static class MclslRuntimeChanges
    {
        internal static void OnWrite(Actor _, string __) { }
        internal static void Publish(Actor _, MclslActorChange __) { }
    }
    internal static class MclslPerformanceProbe
    {
        internal static bool Enabled => false;
        internal static void MarkSaveFrame() { }
        internal static long Begin() => 0;
        internal static void End(string _, long __) { }
        internal static void RecordArchiveBytes(int _, int __) { }
    }
    internal static class MclslFrameDeadline
    {
        internal static bool Expired => RemainingMs <= 0;
        internal static double RemainingMs { get; set; } = double.MaxValue;
    }
    internal static class MclslDiagnostics
    {
        internal static void Error(string _, string __) { }
        internal static void Cultivation(string _, string __) { }
    }
}

namespace MySimulatedLongevityRoad.Data
{
    internal sealed class MclslRunEventRecord
    {
        public string EventType { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public int Importance { get; set; }
    }

    internal static class MclslActorDataKeys
    {
        internal const string Contribution = "contribution";
        internal const string SpiritStones = "spirit_stones";
        internal const string MindState = "mind";
        internal const string MortalMiasma = "miasma";
        internal const string Aptitude = "aptitude";
        internal const string WorldSoulEntityId = "world_soul_entity";
        internal const string Realm = "realm";
        internal const string CultivationSystem = "cultivation_system";
        internal const string LifespanStolenBonus = "lifespan_stolen_bonus";
        internal const string LifespanDrainedPenalty = "lifespan_drained_penalty";
        internal const string ConvertedLifespanFloor = "converted_lifespan_floor";
    }
    internal sealed class MclslAnnualBatchState
    {
        public byte WorldStage { get; set; }
        public byte WorldFailureStage { get; set; }
        public int WorldFailureCount { get; set; }
        public List<MclslAnnualClaimRecord> CaveClaims { get; set; } = new();
        public List<MclslAnnualClaimRecord> ChangeClaims { get; set; } = new();
        public List<MclslAnnualClaimRecord> AdventureCandidates { get; set; } = new();
        public List<string> TechniqueLineagePendingIds { get; set; } = new();
        public bool TechniqueLineagePendingInitialized { get; set; }
        public int SectLifecycleCursor { get; set; }
        public int SectLifecycleEmitted { get; set; }
        public int LatestRequestedYear { get; set; }
        public List<MclslAnnualFailureRecord> FailureRecords { get; set; } = new();
        public int ActiveYear { get; set; }
        public int LastCompletedYear { get; set; }
        public bool WorldBlocked { get; set; }
        public Dictionary<string, int> ModuleCompletedYears { get; set; } = new();
        public Dictionary<string, int> ModuleBlockedYears { get; set; } = new();
    }
    internal sealed class MclslWorldRunState
    {
        public MclslEconomyState Economy { get; set; } = new();
        public string RunId { get; set; } = string.Empty;
        public int CycleNumber { get; set; } = 1;
        public MclslAnnualBatchState AnnualBatch { get; set; } = new();
    }
    internal sealed class MclslAnnualFailureRecord
    {
        public int Year { get; set; }
        public string Scope { get; set; } = "test";
        public string Step { get; set; } = "test";
        public string Error { get; set; } = "injected failure";
    }
    internal sealed class MclslWorldArchiveBundle
    {
        public int Version { get; set; } = MclslSaveVersions.WorldArchive;
        public MclslWorldRunState? CurrentRun { get; set; } = new();
    }
    internal sealed class MclslOwnedItem
    {
        public string ItemId { get; set; } = string.Empty;
        public string InstanceId { get; set; } = string.Empty;
        public int Count { get; set; }
        public int Durability { get; set; }
        public int AcquiredYear { get; set; }
    }
}

namespace MySimulatedLongevityRoad.Systems
{
    internal static class MclslRuntimeCadence
    {
        internal static int Calls;
        internal static void Tick(int _) { Calls++; }
    }
    internal static class MclslScheduler
    {
        internal static int AnnualReadyCount => 0;
        internal static int FirstCultivationPendingCount => 0;
        internal static double FirstCultivationLongestWait => 0;
        internal static int AnnualWaitingCount => 0;
        internal static int NewestWaitingYear => 0;
    }
    internal static class MclslAnnouncementSystem
    {
        internal static int Count;
        internal static void Enqueue(string _) { Count++; }
        internal static void Enqueue(string _, string __, float ___ = 0, int ____ = 0) { Count++; }
    }
    internal static class MclslActorAccessor
    {
        internal static int GetInt(Actor? actor, string key, int fallback = 0)
            => actor?.Values.TryGetValue(key, out int value) == true ? value : fallback;
        internal static void Set(Actor actor, string key, int value) => actor.Values[key] = value;
        internal static long Id(Actor actor) => actor.Id;
        internal static bool Alive(Actor actor) => actor?.data != null;
        internal static string Realm(Actor actor) => actor?.Realm ?? string.Empty;
        internal static string GetString(Actor actor, string key, string fallback = "")
            => actor?.StringValues.TryGetValue(key, out string? value) == true ? value : fallback;
        internal static string DisplayName(Actor actor) => actor?.Name ?? string.Empty;
    }
    internal static class MclslRealmIds
    {
        internal const string LianQi = "lianqi";
        internal const string ZhuJi = "zhuji";
        internal const string JinDan = "jindan";
        internal const string YuanYing = "yuanying";
        internal const string HuaShen = "huashen";
        internal const string HeDao = "hedao";
        internal const string ChangSheng = "changsheng";
        internal static int Index(string realm) => Array.IndexOf(new[] { LianQi, ZhuJi, JinDan, YuanYing, HuaShen, HeDao, ChangSheng }, realm);
        internal static string Display(string realm) => realm;
    }
    internal static class MclslCultivationSystemIds { internal const string AncientLaw = "ancient"; }
    internal static class MclslCultivationActorMarker { internal static bool HasCultivationMarker(Actor _) => true; }
    internal static class MclslTraitGrantRouter
    {
        internal static void SuppressRouting(Action action) => action();
    }
    internal static class MclslSpiritualRootSystem
    {
        internal static bool EnsureForSpecialPhysique(Actor actor)
        {
            if (!MclslEligibility.CanOwnSpecialPhysique(actor)) return false;
            actor.Values.TryAdd(MclslActorDataKeys.Aptitude, 50);
            return true;
        }
    }
    internal static class MclslMindSystem
    {
        internal static int EnsureMindState(Actor actor)
        {
            int value = MclslActorAccessor.GetInt(actor, MclslActorDataKeys.MindState, 50);
            MclslActorAccessor.Set(actor, MclslActorDataKeys.MindState, value);
            return value;
        }
    }
    internal static class MclslWorldRunRepository
    {
        internal static void EnsureCurrentRun(int _) { if (Current.RunId.Length == 0) Current.RunId = "test-world"; }
        internal static MclslWorldRunState Current = new();
        internal static int ResetCount;
        internal static int ExportCount;
        internal static void ResetWorld() { Current = new(); ResetCount++; }
        internal static bool ImportArchive(MclslWorldArchiveBundle archive)
        {
            if (archive.Version != MclslSaveVersions.WorldArchive || archive.CurrentRun == null) return false;
            if (archive.CurrentRun?.RunId == "reject") throw new InvalidOperationException("invalid archive internals");
            Current = archive.CurrentRun ?? throw new InvalidOperationException("Missing run");
            return true;
        }
        internal static MclslWorldArchiveBundle ExportArchive()
        {
            ExportCount++;
            return new() { CurrentRun = Current };
        }
        internal static void RecordAnnualFailure(int _, string __, string ___, string ____, int _____,
            string ______, string _______) { }
    }
}

namespace MySimulatedLongevityRoad.Queries { }

namespace MySimulatedLongevityRoad.Traits
{
    internal static class MclslImmortalActorRegistration
    {
        internal const string BaiId = "mclsl_bai_xiansheng";
        internal const string ChuanfaId = "mclsl_chuanfa_tianzun";
    }
    internal static class MclslWorldSoulActorRegistration
    {
        internal static bool IsWorldSoulAssetId(string id) => id.StartsWith("mclsl_world_soul");
    }
    internal static class MclslLocalizationBridge
    { internal static void RegisterKey(string _, string __) { } }
    internal static class MclslTraitRegistration
    {
        internal const string PhysiqueGroupId = "MclslPhysiques";
        internal static bool LastAllowEntry;
        internal static readonly Dictionary<string, string> ProfessionTexts = new();
        internal static void SyncGiftTrait(Actor _, int __, bool allowEntry = true) => LastAllowEntry = allowEntry;
        internal static void RegisterProfessionText(string id, string __, string text) => ProfessionTexts[id] = text;
        internal static void RegisterTraitInfo(string _, string __, string ___) { }
        internal static void AddSpecial(string id, string _, bool __, bool ___, float ____, float _____, float ______, float _______)
            => AssetManager.traits.Values[id] = new ActorTrait();
        internal static void SetPhysiqueRarity(ActorTrait trait, bool legendary)
            => trait.Rarity = legendary ? 3 : 2;
        internal static string LegacyTraitIdForRealm(string _) => string.Empty;
        internal static bool SafeSetStat(BaseStats stats, string id, float amount)
        { stats.set(id, amount); return true; }
    }
}

namespace MySimulatedLongevityRoad.Systems.Death
{
    internal static class MclslDeathSystem
    {
        internal static void ExecuteScriptedDeath(Actor _, string __, string ___, string ____, bool _____) { }
    }
}

namespace MySimulatedLongevityRoad.Systems
{
    // The focused regression harness does not link the live WorldBox spawn runtime.
    internal static class MclslBeastSpawnSystem
    {
        internal static bool TickAnnual(int _) => true;
    }
}

namespace MySimulatedLongevityRoad.Traits
{
    internal static class MclslBeastActorRegistration
    {
        internal static bool IsBeast(Actor _) => false;
    }
}

namespace MySimulatedLongevityRoad.Modules
{
    internal static class FakeModuleSink
    {
        internal static readonly List<string> Events = new();
        internal static int ThrowTimelineYear;
        internal static bool ThrowRecovery;
        internal static Action? AnnualWork;
        internal static Action? FrameWork;
    }
    internal abstract class FakeAnnualModule : MclslModuleBase
    {
        internal override bool HasAnnualStep => true;
        internal override void TickAnnual(int year)
        {
            FakeModuleSink.Events.Add(Name + ":" + year);
            if (Name == "Timeline" && FakeModuleSink.ThrowTimelineYear == year)
                throw new InvalidOperationException("injected annual failure");
        }
    }
    internal sealed class MclslPersistenceModule : FakeAnnualModule
    { internal override string Name => "Persistence"; internal override int Order => 0; }
    internal sealed class MclslHuanzhenModule : FakeAnnualModule
    { internal override string Name => "Huanzhen"; internal override int Order => 10; }
    internal sealed class MclslTimelineModule : FakeAnnualModule
    { internal override string Name => "Timeline"; internal override int Order => 20; }
    internal sealed class MclslWorldEpochModule : FakeAnnualModule
    { internal override string Name => "WorldEpoch"; internal override int Order => 25; }
    internal sealed class MclslRuntimeCadenceModule : FakeAnnualModule
    {
        internal void TickAnnualWork(int frameCounter) => FakeModuleSink.AnnualWork?.Invoke();
        internal override void TickFrame(int frameCounter) => FakeModuleSink.FrameWork?.Invoke();
        internal override string Name => "RuntimeCadence";
        internal override int Order => 26;
        internal override bool HasLoadRecovery => true;
        internal override void TickLoadRecovery(int year)
        {
            FakeModuleSink.Events.Add("Recovery:" + year);
            if (FakeModuleSink.ThrowRecovery) throw new InvalidOperationException("injected recovery failure");
        }
    }
    internal sealed class MclslWorldSoulModule : MclslModuleBase { }
    internal sealed class MclslMapMarkerModule : MclslModuleBase { }
    internal sealed class MclslInverseTruthModule : MclslModuleBase { }
    internal sealed class MclslFactionMissionModule : MclslModuleBase { }
    internal sealed class MclslFactionPressureModule : MclslModuleBase { }
    internal sealed class MclslAnnouncementModule : MclslModuleBase { }
}
