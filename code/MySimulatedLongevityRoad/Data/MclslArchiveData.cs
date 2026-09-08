using System.Collections.Generic;

namespace MySimulatedLongevityRoad.Data;

internal sealed class MclslWorldArchiveBundle
{
    public int Version { get; set; } = MclslSaveVersions.WorldArchive;
    public MclslWorldRunState CurrentRun { get; set; } = new();
}

internal sealed class MclslWorldRunState
{
    public string RunId { get; set; } = string.Empty;
    public int CycleNumber { get; set; } = 1;
    public int StartYear { get; set; }
    public int LastProcessedYear { get; set; }
    public int RunTruthValue { get; set; }
    public string CultivationEpoch { get; set; } = "ancient_law";
    public int EraOriginYear { get; set; }
    public int AncientLawEndYear { get; set; }
    public int TransmissionEndYear { get; set; }
    public int TransmissionStepYear { get; set; }
    public int NewLawStartYear { get; set; }
    public bool NewLawTransitionResolved { get; set; }
    public bool TransmissionProved { get; set; }
    public bool NewLawEnabled { get; set; }
    public bool LawConflictEnabled { get; set; }
    public bool ImmortalMortalMiasmaEnabled { get; set; }
    public bool WanxianAllianceFounded { get; set; }
    public bool FiveEldersFounded { get; set; }
    public int NewLawPioneerStartYear { get; set; }
    public int LastNewLawPioneerYear { get; set; }
    public int LastNewLawPioneerConversionYear { get; set; }
    public int NewLawPioneerCount { get; set; }
    public bool IsTerminal { get; set; }
    public string TerminalReason { get; set; } = string.Empty;
    public bool UsesNativeKingdoms { get; set; } = true;
    public string CurrentBackgroundEraId { get; set; } = string.Empty;
    public string CurrentBackgroundEraName { get; set; } = string.Empty;
    public int CurrentBackgroundEraStartYear { get; set; }
    public int CurrentBackgroundEraEndYear { get; set; }
    public string CurrentWorldCalamityId { get; set; } = string.Empty;
    public string CurrentWorldCalamityName { get; set; } = string.Empty;
    public int CurrentWorldCalamityStartYear { get; set; }
    public int CurrentWorldCalamityEndYear { get; set; }
    public int LastSectLifecycleYear { get; set; }
    public MclslBackgroundFactionState BackgroundFactions { get; set; } = new();
    public List<string> InheritedKnowledgeIds { get; set; } = new();
    public List<MclslKnowledgeDiscoveryRecord> Discoveries { get; set; } = new();
    public List<MclslTimelineAnchorState> TimelineAnchors { get; set; } = new();
    public List<MclslRunEventRecord> Events { get; set; } = new();
    public List<MclslDeathRecord> DeathRecords { get; set; } = new();
    public List<MclslFactionMissionRecord> FactionMissions { get; set; } = new();
    public List<MclslFactionPressureRecord> FactionPressureEvents { get; set; } = new();
    public List<MclslResourceSpendRecord> ResourceSpendEvents { get; set; } = new();
    public List<MclslTechniqueLineageRecord> TechniqueLineages { get; set; } = new();
    public List<MclslSectRuinRecord> SectRuins { get; set; } = new();
    public List<MclslRuinExplorationRecord> RuinExplorations { get; set; } = new();
    public List<MclslWorldCaveRecord> WorldCaves { get; set; } = new();
    public List<MclslWorldChangeRecord> WorldChanges { get; set; } = new();
    public List<MclslWorldSoulRecord> WorldSouls { get; set; } = new();
    public List<MclslInverseTruthRecord> InverseTruths { get; set; } = new();
    public List<MclslActorReincarnationRecord> ReincarnationRecords { get; set; } = new();
    public List<string> UsedGeneratedNames { get; set; } = new();
    public List<string> FiredHistoricalEvents { get; set; } = new();
    public List<string> PendingAncientCultivatorIds { get; set; } = new();
    public List<MclslGeneratedItemRecord> GeneratedItems { get; set; } = new();
    public int ProceduralSequence { get; set; }
    public int NextCaveBirthYear { get; set; }
    public int NextWorldChangeYear { get; set; }
    public int NextRuinBirthYear { get; set; }
    public int NextFactionMissionYear { get; set; }
    public int NextFactionPressureYear { get; set; }
    public int NativeKillStatisticsRepairVersion { get; set; }
}

internal sealed class MclslBackgroundFactionState
{
    public int WanXianAllianceInfluence { get; set; } = 15;
    public int FiveEldersInfluence { get; set; } = 10;
    public int AllianceOrderPressure { get; set; }
    public int FiveEldersSubversion { get; set; }
    public string AlliancePolicy { get; set; } = "远观诸国";
    public string FiveEldersPolicy { get; set; } = "潜伏暗流";
}

internal sealed class MclslKnowledgeDiscoveryRecord
{
    public string KnowledgeId { get; set; } = string.Empty;
    public int DiscoveryYear { get; set; }
    public string SourceAnchorId { get; set; } = string.Empty;
    public int TruthValue { get; set; }
}

internal sealed class MclslTimelineAnchorState
{
    public string AnchorId { get; set; } = string.Empty;
    public int ScheduledYear { get; set; }
    public bool Resolved { get; set; }
    public bool Succeeded { get; set; }
    public int ResolvedYear { get; set; }
    public string OutcomeCode { get; set; } = string.Empty;
}


internal sealed class MclslRunEventRecord
{
    public int Year { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public long ActorId { get; set; }
    public string ActorName { get; set; } = string.Empty;
    public int MapX { get; set; } = -1;
    public int MapY { get; set; } = -1;
    public string LocationName { get; set; } = string.Empty;
    public string KingdomName { get; set; } = string.Empty;
    public bool NativeLogged { get; set; }
}

internal sealed class MclslDeathRecord
{
    public long ActorId { get; set; }
    public string ActorName { get; set; } = string.Empty;
    public string RealmId { get; set; } = string.Empty;
    public string RealmName { get; set; } = string.Empty;
    public int Year { get; set; }
    public int Age { get; set; }
    public string CauseCode { get; set; } = string.Empty;
    public string CauseText { get; set; } = string.Empty;
    public string SourceName { get; set; } = string.Empty;
    public string KillerName { get; set; } = string.Empty;
    public string KingdomName { get; set; } = string.Empty;
    public string CityName { get; set; } = string.Empty;
    public int MapX { get; set; } = -1;
    public int MapY { get; set; } = -1;
    public string TechniqueName { get; set; } = string.Empty;
    public string CultivationSystem { get; set; } = string.Empty;
    public string AncientFoundationName { get; set; } = string.Empty;
    public string AncientCoreName { get; set; } = string.Empty;
    public string AncientDaoIntent { get; set; } = string.Empty;
    public string AncientNascentName { get; set; } = string.Empty;
    public string AncientDivineIntent { get; set; } = string.Empty;
    public string AncientDaoName { get; set; } = string.Empty;
    public string FoundationWonderName { get; set; } = string.Empty;
    public string GoldenCoreLaws { get; set; } = string.Empty;
    public string NascentCaveName { get; set; } = string.Empty;
    public string NascentEssenceName { get; set; } = string.Empty;
    public string DivineChangeName { get; set; } = string.Empty;
    public string DivineMarrowName { get; set; } = string.Empty;
    public string WorldSoulName { get; set; } = string.Empty;
    public string HeavenlyDuty { get; set; } = string.Empty;
    public string InverseTruthName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Announcement { get; set; } = string.Empty;
    public bool Surfaced { get; set; }
}

internal sealed class MclslActorReincarnationRecord
{
    public string Id { get; set; } = string.Empty;
    public long SourceActorId { get; set; }
    public string SourceActorName { get; set; } = string.Empty;
    public string SourceRealmId { get; set; } = string.Empty;
    public string SourceRealmName { get; set; } = string.Empty;
    public string SourceTechniqueName { get; set; } = string.Empty;
    public string RaceKey { get; set; } = string.Empty;
    public int DeathYear { get; set; }
    public int RootPurityFloor { get; set; }
    public long TargetActorId { get; set; }
    public string TargetActorName { get; set; } = string.Empty;
    public int AppliedYear { get; set; }
    public string Status { get; set; } = "待转";
}

internal sealed class MclslFactionMissionRecord
{
    public string Id { get; set; } = string.Empty;
    public int Year { get; set; }
    public string FactionId { get; set; } = string.Empty;
    public string FactionName { get; set; } = string.Empty;
    public string MissionName { get; set; } = string.Empty;
    public long ActorId { get; set; }
    public string ActorName { get; set; } = string.Empty;
    public string RealmName { get; set; } = string.Empty;
    public string Outcome { get; set; } = string.Empty;
    public int ContributionReward { get; set; }
    public int SpiritStoneReward { get; set; }
    public int InfluenceDelta { get; set; }
    public string Summary { get; set; } = string.Empty;
}

internal sealed class MclslFactionPressureRecord
{
    public string Id { get; set; } = string.Empty;
    public int Year { get; set; }
    public string FactionId { get; set; } = string.Empty;
    public string FactionName { get; set; } = string.Empty;
    public string PolicyName { get; set; } = string.Empty;
    public int InfluenceAtTrigger { get; set; }
    public int PressureDelta { get; set; }
    public string ActorNames { get; set; } = string.Empty;
    public string WorldEffect { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
}

internal sealed class MclslResourceSpendRecord
{
    public string Id { get; set; } = string.Empty;
    public int Year { get; set; }
    public long ActorId { get; set; }
    public string ActorName { get; set; } = string.Empty;
    public string RealmName { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public int ContributionCost { get; set; }
    public int SpiritStoneCost { get; set; }
    public string EffectText { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
}

internal sealed class MclslTechniqueLineageRecord
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string SystemId { get; set; } = string.Empty;
    public string LawTags { get; set; } = string.Empty;
    public string MaxRealm { get; set; } = string.Empty;
    public int Completeness { get; set; } = 100;
    public int FirstSeenYear { get; set; }
    public int LastSeenYear { get; set; }
    public int CurrentPractitioners { get; set; }
    public int PeakPractitioners { get; set; }
    public string PeakRealm { get; set; } = string.Empty;
    public string FounderName { get; set; } = string.Empty;
    public long FounderActorId { get; set; }
    public string State { get; set; } = "流传";
    public int LostYear { get; set; }
    public int RevivedYear { get; set; }
    public int BranchCount { get; set; }
    public string SourceTechniqueId { get; set; } = string.Empty;
    public string ParentLineageId { get; set; } = string.Empty;
    public string BranchRootId { get; set; } = string.Empty;
    public string LinkedRuinId { get; set; } = string.Empty;
    public string SectDisplayName { get; set; } = string.Empty;
    public string LifecycleState { get; set; } = string.Empty;
    public int LifecycleYear { get; set; }
    public int LastLifecycleEventYear { get; set; }
    public string Summary { get; set; } = string.Empty;
}

internal sealed class MclslSectRuinRecord
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string LawTags { get; set; } = string.Empty;
    public int Quality { get; set; } = 1;
    public int Danger { get; set; } = 30;
    public int BornYear { get; set; }
    public string LocationName { get; set; } = "无主荒域";
    public string NativeKingdomName { get; set; } = "无主";
    public int Depth { get; set; } = 4;
    public int ExplorationProgress { get; set; }
    public int RemainingValue { get; set; } = 4;
    public int ExpeditionCount { get; set; }
    public int CasualtyCount { get; set; }
    public string State { get; set; } = "显世";
    public int LastExploredYear { get; set; }
    public string LastExplorerNames { get; set; } = string.Empty;
    public string SourceTechniqueId { get; set; } = string.Empty;
    public string SourceTechniqueName { get; set; } = string.Empty;
    public string LinkedLineageId { get; set; } = string.Empty;
    public int SourceTechniqueLostYear { get; set; }
    public int SourceTechniqueRevivedYear { get; set; }
}

internal sealed class MclslRuinExplorationRecord
{
    public string Id { get; set; } = string.Empty;
    public int Year { get; set; }
    public string RuinId { get; set; } = string.Empty;
    public string RuinName { get; set; } = string.Empty;
    public long ActorId { get; set; }
    public string ActorName { get; set; } = string.Empty;
    public string RealmName { get; set; } = string.Empty;
    public bool Survived { get; set; }
    public string OutcomeCode { get; set; } = string.Empty;
    public string RewardText { get; set; } = string.Empty;
    public int ProgressGained { get; set; }
    public string Summary { get; set; } = string.Empty;
    public string RevivedTechniqueId { get; set; } = string.Empty;
    public string RevivedTechniqueName { get; set; } = string.Empty;
    public string LinkedLineageId { get; set; } = string.Empty;
}

internal sealed class MclslGeneratedItemRecord
{
    public string Id { get; set; } = string.Empty;
    public string Kind { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Origin { get; set; } = string.Empty;
    public string LawTags { get; set; } = string.Empty;
    public string AttributeText { get; set; } = string.Empty;
    public int Quality { get; set; }
    public string Category { get; set; } = string.Empty;
    public string Grade { get; set; } = string.Empty;
    public int Completeness { get; set; }
    public int RuleStrength { get; set; }
    public int CreatedYear { get; set; }
    public long HolderActorId { get; set; }
    public string HolderName { get; set; } = string.Empty;
    public bool Consumed { get; set; }
    public string SourceObjectId { get; set; } = string.Empty;
}

internal sealed class MclslWorldCaveRecord
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string LawTags { get; set; } = string.Empty;
    public int Quality { get; set; } = 1;
    public int BornYear { get; set; }
    public string LocationName { get; set; } = "无主荒域";
    public string NativeKingdomName { get; set; } = "无主";
    public int MapX { get; set; } = -1;
    public int MapY { get; set; } = -1;
    public int EssenceCapacity { get; set; } = 2;
    public int RemainingEssence { get; set; } = 2;
    public int Integrity { get; set; } = 100;
    public string State { get; set; } = "活跃";
    public int LastContestedYear { get; set; }
    public long LastRefinedByActorId { get; set; }
    public string LastRefinedByActorName { get; set; } = string.Empty;
    public int RefinedCount { get; set; }
}

internal sealed class MclslWorldChangeRecord
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Origin { get; set; } = string.Empty;
    public string LawTags { get; set; } = string.Empty;
    public int Quality { get; set; } = 1;
    public int StartYear { get; set; }
    public int EndYear { get; set; }
    public int MarrowCapacity { get; set; } = 3;
    public int RemainingMarrow { get; set; } = 3;
    public int Intensity { get; set; } = 100;
    public string State { get; set; } = "活跃";
    public string LocationName { get; set; } = "天地之间";
    public string NativeKingdomName { get; set; } = "无主";
    public int MapX { get; set; } = -1;
    public int MapY { get; set; } = -1;
    public string SourceType { get; set; } = "world_event";
    public string NativeTerrainEffect { get; set; } = string.Empty;
    public int ExtractedCount { get; set; }
    public long LastExtractedByActorId { get; set; }
    public string LastExtractedByActorName { get; set; } = string.Empty;
}

internal sealed class MclslWorldSoulRecord
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string HeavenlyDuty { get; set; } = string.Empty;
    public string LawTags { get; set; } = string.Empty;
    public int Quality { get; set; } = 3;
    public string State { get; set; } = "沉寂";
    public int NextManifestYear { get; set; }
    public int ManifestYear { get; set; }
    public int ManifestCombatStartedYear { get; set; }
    public int LastHunterDispatchYear { get; set; }
    public int DeathYear { get; set; }
    public long ManifestActorId { get; set; }
    public string ManifestActorName { get; set; } = string.Empty;
    public string LocationName { get; set; } = "天地之间";
    public string NativeKingdomName { get; set; } = "无主";
    public int MapX { get; set; } = -1;
    public int MapY { get; set; } = -1;
    public string NativeTerrainEffect { get; set; } = string.Empty;
    public long HolderActorId { get; set; }
    public string HolderActorName { get; set; } = string.Empty;
    public int HolderYear { get; set; }
    public string HolderOrigin { get; set; } = string.Empty;
    public int DutyProgress { get; set; }
    public int DutyBacklash { get; set; }
    public int LastDutyEventYear { get; set; }
    public int DutyCompletedYear { get; set; }
    public int ManifestCount { get; set; }
}

internal sealed class MclslInverseTruthRecord
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string RuleDescription { get; set; } = string.Empty;
    public string Category { get; set; } = "player";
    public bool CountsTowardLongevity { get; set; } = true;
    public long ChallengerActorId { get; set; }
    public int Progress { get; set; }
    public bool Reversed { get; set; }
}

internal sealed class MclslReincarnationProfile
{
    public int Version { get; set; } = MclslSaveVersions.ReincarnationProfile;
    public int Revision { get; set; }
    public int CurrentCycle { get; set; } = 1;
    public int CompletedCycles { get; set; }
    public int TotalTruthValue { get; set; }
    public int CarrySlotLimit { get; set; } = 8;
    public string LastCompletedRunId { get; set; } = string.Empty;
    public int LastCompletedYear { get; set; }
    public string LastTerminalReason { get; set; } = string.Empty;
    public List<string> KnownKnowledgeIds { get; set; } = new();
    public List<string> KnownTechniqueIds { get; set; } = new();
    public List<string> KnownTimelineAnchorIds { get; set; } = new();
}
