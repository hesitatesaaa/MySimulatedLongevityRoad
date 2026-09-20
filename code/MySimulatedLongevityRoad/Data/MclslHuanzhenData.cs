using System.Collections.Generic;

namespace MySimulatedLongevityRoad.Data;

internal sealed class MclslHuanzhenExternalState
{
    public int Version { get; set; } = MclslSaveVersions.HuanzhenExternalState;
    public string WorldRunId { get; set; } = string.Empty;
    public string OriginalSavePath { get; set; } = string.Empty;
    public string HostIdentity { get; set; } = string.Empty;
    public long HostLastActorId { get; set; }
    public string HostName { get; set; } = string.Empty;
    public int LastAnchorYear { get; set; } = -1;
    public int LastRestoreYear { get; set; } = -1;
    public int ConsecutiveLoopDeaths { get; set; }
    public int AnchorSequence { get; set; }
    public int NextNaturalArrivalYear { get; set; } = -1;
    public int NaturalArrivalYear { get; set; } = -1;
    // long keeps the value effectively uncapped while remaining compatible with
    // the old JSON int representation used by v0.1.8 saves.
    public long SpaceEssenceBase { get; set; }
    public int SpaceEssenceUpdatedYear { get; set; } = -1;
    public List<MclslHuanzhenAnchorRecord> Anchors { get; set; } = new();
    public MclslHuanzhenPendingRestore PendingRestore { get; set; } = new();
    public List<MclslHuanzhenHistoryRecord> History { get; set; } = new();
    public List<MclslHuanzhenEssenceRecord> EssenceHistory { get; set; } = new();
    public List<MclslHuanzhenLegacyRecord> Legacies { get; set; } = new();
}

internal sealed class MclslHuanzhenLegacyRecord
{
    public string Id { get; set; } = string.Empty;
    public int DeathYear { get; set; }
    public int AnchorYear { get; set; }
    public string HostName { get; set; } = string.Empty;
    public string RealmId { get; set; } = string.Empty;
    public int CarryLimit { get; set; } = 3;
    public List<string> ClaimedChoices { get; set; } = new();
    public MclslHuanzhenCultivationSnapshot Snapshot { get; set; } = new();
}

internal sealed class MclslHuanzhenEssenceRecord
{
    public int Year { get; set; }
    public string Source { get; set; } = string.Empty;
    public string Detail { get; set; } = string.Empty;
    public long Amount { get; set; }
    public long Balance { get; set; }
}

internal sealed class MclslHuanzhenAnchorRecord
{
    public string RelativeSavePath { get; set; } = string.Empty;
    public string WorldRunId { get; set; } = string.Empty;
    public int Year { get; set; }
    public int Sequence { get; set; }
    public string HostIdentity { get; set; } = string.Empty;
    public string HostName { get; set; } = string.Empty;
    public string RealmIdAtAnchor { get; set; } = string.Empty;
    public MclslHuanzhenCultivationSnapshot Snapshot { get; set; } = new();
}

internal sealed class MclslHuanzhenPendingRestore
{
    public bool Active { get; set; }
    public string RelativeSavePath { get; set; } = string.Empty;
    public string OriginalSavePath { get; set; } = string.Empty;
    public string WorldRunId { get; set; } = string.Empty;
    public string HostIdentity { get; set; } = string.Empty;
    public string HostName { get; set; } = string.Empty;
    public int AnchorYear { get; set; }
    public int DeathYear { get; set; }
    public int LoopDepth { get; set; }
    public int LoadAttempts { get; set; }
    public string Trigger { get; set; } = "death";
    // Added in v0.1.9. The bool distinguishes old pending restores, where the
    // field did not exist, from a real pre-restore snapshot of zero.
    public bool HasSpaceEssenceBeforeRestore { get; set; }
    public long SpaceEssenceBeforeRestore { get; set; }
    public MclslHuanzhenCultivationSnapshot Cultivation { get; set; } = new();
}

internal sealed class MclslHuanzhenLegacyOption
{
    public string Id { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
}

internal sealed class MclslHuanzhenHistoryRecord
{
    public int DeathYear { get; set; }
    public int AnchorYear { get; set; }
    public string HostName { get; set; } = string.Empty;
    public string RealmId { get; set; } = string.Empty;
    public int LoopDepth { get; set; }
    public string Result { get; set; } = string.Empty;
}

internal sealed class MclslHuanzhenCultivationSnapshot
{
    // Version 2 supplements the original fixed fields with allow-listed bags. This
    // keeps new cultivation subsystems from requiring another large DTO rewrite.
    public Dictionary<string, string> StringState { get; set; } = new();
    public Dictionary<string, int> IntState { get; set; } = new();
    public Dictionary<string, float> FloatState { get; set; } = new();
    public List<string> TraitIds { get; set; } = new();
    public string CultivationSystemId { get; set; } = string.Empty;
    public string RealmId { get; set; } = string.Empty;
    public float CultivationProgress { get; set; }
    public int TrueEssence { get; set; }
    public int Aptitude { get; set; }
    public int MindState { get; set; }
    public int HeartTemperingProgress { get; set; }
    public int HeartMethodKnown { get; set; }
    public int MiasmaPoolCleansing { get; set; }
    public int Contribution { get; set; }
    public int RuinExperience { get; set; }
    public int TechniqueInsight { get; set; }
    public string TechniqueId { get; set; } = string.Empty;
    public string TechniqueName { get; set; } = string.Empty;
    public string TechniqueMaxRealm { get; set; } = string.Empty;
    public string FoundationWonderId { get; set; } = string.Empty;
    public string FoundationWonderName { get; set; } = string.Empty;
    public int FoundationWonderQuality { get; set; }
    public string FoundationWonderTags { get; set; } = string.Empty;
    public string FoundationWonderDescription { get; set; } = string.Empty;
    public string FoundationWonderOrigin { get; set; } = string.Empty;
    public string FoundationWonderEffects { get; set; } = string.Empty;
    public string GoldenCoreLaws { get; set; } = string.Empty;
    public int GoldenCorePurity { get; set; }
    public int GoldenCoreStability { get; set; }
    public string NascentCaveId { get; set; } = string.Empty;
    public string NascentCaveName { get; set; } = string.Empty;
    public string NascentCaveTags { get; set; } = string.Empty;
    public int NascentCaveCompatibility { get; set; }
    public int NascentCaveIntegrity { get; set; }
    public string NascentEssenceId { get; set; } = string.Empty;
    public string NascentEssenceName { get; set; } = string.Empty;
    public int NascentEssenceQuality { get; set; }
    public string NascentEssenceTags { get; set; } = string.Empty;
    public string NascentEssenceDescription { get; set; } = string.Empty;
    public string NascentEssenceEffects { get; set; } = string.Empty;
    public string DivineChangeId { get; set; } = string.Empty;
    public string DivineChangeName { get; set; } = string.Empty;
    public string DivineChangeTags { get; set; } = string.Empty;
    public int DivineChangeCompatibility { get; set; }
    public string DivineMarrowId { get; set; } = string.Empty;
    public string DivineMarrowName { get; set; } = string.Empty;
    public int DivineMarrowCount { get; set; }
    public int DivineMarrowQuality { get; set; }
    public string DivineMarrowTags { get; set; } = string.Empty;
    public string DivineMarrowDescription { get; set; } = string.Empty;
    public string DivineMarrowEffects { get; set; } = string.Empty;
    public string HuaShenHonorific { get; set; } = string.Empty;
    public string HeDaoHonorific { get; set; } = string.Empty;
    public string ChangShengHonorific { get; set; } = string.Empty;
    public string AncientHuaShenHonorific { get; set; } = string.Empty;
    public string AncientHeDaoHonorific { get; set; } = string.Empty;
    public string AncientChangShengHonorific { get; set; } = string.Empty;
    public string WorldSoulId { get; set; } = string.Empty;
    public string WorldSoulName { get; set; } = string.Empty;
    public string HeavenlyDuty { get; set; } = string.Empty;
    public int HarmonyLeap { get; set; }
    public string HarmonyOrigin { get; set; } = string.Empty;
    public int HarmonyCompatibility { get; set; }
    public int HarmonyStability { get; set; }
    public string InverseTruthId { get; set; } = string.Empty;
    public string InverseTruthName { get; set; } = string.Empty;
    public int InverseTruthProgress { get; set; }
}

internal readonly struct MclslHuanzhenDeathSnapshot
{
    // Actor.die is a hot path. Reuse one empty snapshot instead of allocating a
    // cultivation DTO for every non-Huanzhen death while the feature is disabled.
    internal static readonly MclslHuanzhenDeathSnapshot Empty = new(false, string.Empty, 0L, string.Empty, 0, new MclslHuanzhenCultivationSnapshot());

    internal readonly bool Found;
    internal readonly string Identity;
    internal readonly long ActorId;
    internal readonly string ActorName;
    internal readonly int DeathYear;
    internal readonly MclslHuanzhenCultivationSnapshot Cultivation;

    internal MclslHuanzhenDeathSnapshot(bool found, string identity, long actorId, string actorName, int deathYear, MclslHuanzhenCultivationSnapshot cultivation)
    {
        Found = found;
        Identity = identity ?? string.Empty;
        ActorId = actorId;
        ActorName = actorName ?? string.Empty;
        DeathYear = deathYear;
        Cultivation = cultivation ?? new MclslHuanzhenCultivationSnapshot();
    }
}

internal readonly struct MclslWorldSoulDeathSnapshot
{
    internal static MclslWorldSoulDeathSnapshot Empty => new(false, string.Empty, 0L, 0L, string.Empty);

    internal readonly bool Found;
    internal readonly string SoulId;
    internal readonly long VictimActorId;
    internal readonly long KillerActorId;
    internal readonly string KillerName;

    internal MclslWorldSoulDeathSnapshot(bool found, string soulId, long victimActorId, long killerActorId, string killerName)
    {
        Found = found;
        SoulId = soulId ?? string.Empty;
        VictimActorId = victimActorId;
        KillerActorId = killerActorId;
        KillerName = killerName ?? string.Empty;
    }
}

internal readonly struct MclslDeathPatchState
{
    internal readonly Death.MclslDeathSnapshot CultivatorDeath;
    internal readonly MclslHuanzhenDeathSnapshot HuanzhenDeath;
    internal readonly MclslWorldSoulDeathSnapshot WorldSoulDeath;

    internal MclslDeathPatchState(Death.MclslDeathSnapshot cultivatorDeath, MclslHuanzhenDeathSnapshot huanzhenDeath, MclslWorldSoulDeathSnapshot worldSoulDeath)
    {
        CultivatorDeath = cultivatorDeath;
        HuanzhenDeath = huanzhenDeath;
        WorldSoulDeath = worldSoulDeath;
    }
}
