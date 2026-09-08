using System;
using MySimulatedLongevityRoad.Core;
using MySimulatedLongevityRoad.Data;
using MySimulatedLongevityRoad.Systems;
using MySimulatedLongevityRoad.Systems.Death;

namespace MySimulatedLongevityRoad.Queries;

internal sealed class MclslActorCultivationView
{
    internal Actor Actor { get; set; }
    internal bool Alive { get; set; }
    internal string Name { get; set; } = string.Empty;
    internal string RealmId { get; set; } = string.Empty;
    internal string RealmName { get; set; } = "凡俗";
    internal bool IsSpiritualRootPath { get; set; }
    internal string CultivationSystemName { get; set; } = string.Empty;
    internal string AncientLawStatus { get; set; } = string.Empty;
    internal int AncientLineageStrength { get; set; }
    internal int AncientLegacyPotential { get; set; }
    internal string AncientFoundationName { get; set; } = string.Empty;
    internal int AncientFoundationQuality { get; set; }
    internal int AncientFoundationStability { get; set; }
    internal string AncientCoreName { get; set; } = string.Empty;
    internal int AncientCorePurity { get; set; }
    internal string AncientDaoIntent { get; set; } = string.Empty;
    internal string AncientNascentName { get; set; } = string.Empty;
    internal int AncientSoulStrength { get; set; }
    internal int AncientBodyFit { get; set; }
    internal string AncientDivineIntent { get; set; } = string.Empty;
    internal int AncientSoulFusion { get; set; }
    internal int AncientDaoCompatibility { get; set; }
    internal int TechniqueComprehensionProgress { get; set; }
    internal string AncientDaoName { get; set; } = string.Empty;
    internal int AncientHarmonyIntegrity { get; set; }
    internal int AncientHeavenCompatibility { get; set; }
    internal string TechniqueName { get; set; } = string.Empty;
    internal int ImmortalFate { get; set; }
    internal int Aptitude { get; set; }
    internal string GiftName { get; set; } = string.Empty;
    internal string SpiritualRootGrade { get; set; } = string.Empty;
    internal string SpiritualRootAttributes { get; set; } = string.Empty;
    internal string SpiritualRootCountName { get; set; } = string.Empty;
    internal int SpiritualRootPurity { get; set; }
    internal int MindState { get; set; }
    internal int MortalMiasma { get; set; }
    internal int MortalMiasmaLimit { get; set; }
    internal int HeartTemperingProgress { get; set; }
    internal string HeartMethodText { get; set; } = string.Empty;
    internal float CultivationProgress { get; set; }
    internal int TrueEssence { get; set; }
    internal int NextRealmMinimum { get; set; }
    internal int Contribution { get; set; }
    internal int SpiritStones { get; set; }
    internal string FactionAffiliation { get; set; } = string.Empty;
    internal string LastResult { get; set; } = string.Empty;
    internal int LineageIntegrity { get; set; }
    internal string LineageSummary { get; set; } = string.Empty;
    internal string FoundationWonderName { get; set; } = string.Empty;
    internal int FoundationWonderQuality { get; set; }
    internal string FoundationWonderTags { get; set; } = string.Empty;
    internal string FoundationWonderCategory { get; set; } = string.Empty;
    internal string FoundationWonderGrade { get; set; } = string.Empty;
    internal int FoundationWonderCompleteness { get; set; }
    internal int FoundationWonderRuleStrength { get; set; }
    internal string GoldenCoreLaws { get; set; } = string.Empty;
    internal int GoldenCorePurity { get; set; }
    internal int GoldenCoreStability { get; set; }
    internal string NascentCaveName { get; set; } = string.Empty;
    internal int NascentCaveCompatibility { get; set; }
    internal int NascentCaveIntegrity { get; set; }
    internal string NascentEssenceName { get; set; } = string.Empty;
    internal string DivineChangeName { get; set; } = string.Empty;
    internal int DivineChangeCompatibility { get; set; }
    internal string DivineMarrowName { get; set; } = string.Empty;
    internal string WorldSoulName { get; set; } = string.Empty;
    internal string HeavenlyDuty { get; set; } = string.Empty;
    internal int HeavenlyDutyProgress { get; set; }
    internal int HeavenlyDutyBacklash { get; set; }
    internal int HarmonyStability { get; set; }
    internal int HarmonyLeap { get; set; }
    internal string InverseTruthName { get; set; } = string.Empty;
    internal int InverseTruthProgress { get; set; }
    internal int TaishangProgress { get; set; }
    internal int HuanzhenAnchorYear { get; set; }
    internal int HuanzhenRestoreCount { get; set; }
    internal bool EligibleForCultivation { get; set; }
    internal bool HasCultivationData { get; set; }
    internal bool HasSpiritualRoot { get; set; }
}

internal static class MclslActorCultivationQuery
{
    internal static MclslActorCultivationView Build(Actor actor)
    {
        if (!MclslActorAccessor.Alive(actor))
            return new MclslActorCultivationView { Actor = actor, Alive = false };

        bool eligible = MclslEligibility.CanCultivate(actor);
        MySimulatedLongevityRoad.Core.MclslDiagnostics.Cultivation(
            "query.build",
            "actor=" + MclslActorAccessor.Id(actor)
            + " eligible=" + eligible
            + " realm=" + MclslActorAccessor.Realm(actor)
            + " system=" + MclslActorAccessor.GetString(actor, MclslActorDataKeys.CultivationSystem, string.Empty)
            + " essence=" + MclslCultivationGrowthSystem.CurrentTrueEssence(actor));
        // 角色信息查询必须是纯读取。修炼身份初始化、年度入队和索引维护
        // 只能由角色生命周期、读档迁移或年度调度入口执行；否则排行榜/仙录
        // 在构建快照时会反向唤醒角色并使自身缓存失效。
        string storedRealm = MclslActorAccessor.Realm(actor);
        string systemId = MclslActorAccessor.GetString(actor, MclslActorDataKeys.CultivationSystem, string.Empty);
        int aptitude = Math.Clamp(MclslActorAccessor.GetInt(actor, MclslActorDataKeys.Aptitude, 0), 0, 100);
        int trueEssence = MclslActorAccessor.GetInt(actor, MclslActorDataKeys.TrueEssence, 0);
        float storedProgress = Math.Clamp(MclslActorAccessor.GetFloat(actor, MclslActorDataKeys.CultivationProgress, 0f), 0f, 100f);
        MclslAptitudeGiftDefinition realGift = eligible ? MclslSpiritualRootSystem.ReadGiftForCultivation(actor) : null;
        bool hasRootArchive = eligible && MclslSpiritualRootSystem.HasCultivationPotential(actor);
        bool hasSystem = systemId == MclslCultivationSystemIds.AncientLaw || systemId == MclslCultivationSystemIds.NewLaw;
        bool hasStoredRealm = !string.IsNullOrWhiteSpace(storedRealm);

        bool belowLianQiEntry = MclslSensingQiSystem.ShouldReturnToSensingQi(actor, storedRealm, systemId);
        string realm = belowLianQiEntry ? string.Empty : storedRealm;
        MclslAptitudeGiftDefinition gift = realGift
            ?? (hasRootArchive
                ? MclslAptitudeGiftCatalog.ForAptitude(Math.Clamp(aptitude <= 0 ? 50 : aptitude, 1, 100))
                : null);
        string worldSoulId = MclslActorAccessor.GetString(actor, MclslActorDataKeys.WorldSoulId, string.Empty);
        MclslWorldSoulRecord worldSoul = string.IsNullOrWhiteSpace(worldSoulId)
            ? null
            : MclslWorldRunRepository.Current?.WorldSouls?.Find(x => x != null && x.Id == worldSoulId);
        MclslSpiritualRootProfile root = hasRootArchive
            ? MclslSpiritualRootSystem.ReadProfile(actor)
            : new MclslSpiritualRootProfile(string.Empty, string.Empty, 1, 0, 1f, 0);
        bool ancientPath = systemId == MclslCultivationSystemIds.AncientLaw;
        int nextRealmMinimum = MclslRealmProgress.NextRealmMinimum(realm, ancientPath);
        float progress = MclslRealmProgress.ProgressForRealm(realm, trueEssence, ancientPath, storedProgress);
        int taishangProgress = Math.Clamp(MclslActorAccessor.GetInt(actor, MclslActorDataKeys.TaishangProgress, 0), 0, 100);
        bool hasCultivationData = eligible && (realGift != null || hasRootArchive || hasSystem || hasStoredRealm || trueEssence > 0 || storedProgress > 0f);
        MclslActorCultivationView view = new()
        {
            Actor = actor,
            Alive = true,
            Name = MclslActorAccessor.DisplayName(actor, realm),
            RealmId = realm,
            RealmName = RealmDisplay(realm, trueEssence, progress, taishangProgress),
            IsSpiritualRootPath = ancientPath,
            CultivationSystemName = SystemDisplay(actor, systemId),
            AncientLawStatus = MclslActorAccessor.GetString(actor, MclslActorDataKeys.AncientLawStatus, string.Empty),
            AncientLineageStrength = MclslActorAccessor.GetInt(actor, MclslActorDataKeys.AncientLineageStrength, 0),
            AncientLegacyPotential = MclslActorAccessor.GetInt(actor, MclslActorDataKeys.AncientLegacyPotential, 0),
            AncientFoundationName = MclslActorAccessor.GetString(actor, MclslActorDataKeys.AncientFoundationName, string.Empty),
            AncientFoundationQuality = MclslActorAccessor.GetInt(actor, MclslActorDataKeys.AncientFoundationQuality, 0),
            AncientFoundationStability = MclslActorAccessor.GetInt(actor, MclslActorDataKeys.AncientFoundationStability, 0),
            AncientCoreName = MclslActorAccessor.GetString(actor, MclslActorDataKeys.AncientCoreName, string.Empty),
            AncientCorePurity = MclslActorAccessor.GetInt(actor, MclslActorDataKeys.AncientCorePurity, 0),
            AncientDaoIntent = MclslActorAccessor.GetString(actor, MclslActorDataKeys.AncientDaoIntent, string.Empty),
            AncientNascentName = MclslActorAccessor.GetString(actor, MclslActorDataKeys.AncientNascentName, string.Empty),
            AncientSoulStrength = MclslActorAccessor.GetInt(actor, MclslActorDataKeys.AncientSoulStrength, 0),
            AncientBodyFit = MclslActorAccessor.GetInt(actor, MclslActorDataKeys.AncientBodyFit, 0),
            AncientDivineIntent = MclslActorAccessor.GetString(actor, MclslActorDataKeys.AncientDivineIntent, string.Empty),
            AncientSoulFusion = MclslActorAccessor.GetInt(actor, MclslActorDataKeys.AncientSoulFusion, 0),
            AncientDaoCompatibility = MclslActorAccessor.GetInt(actor, MclslActorDataKeys.AncientDaoCompatibility, 0),
            TechniqueComprehensionProgress = MclslTechniqueStageSystem.Progress(actor),
            AncientDaoName = MclslActorAccessor.GetString(actor, MclslActorDataKeys.AncientDaoName, string.Empty),
            AncientHarmonyIntegrity = MclslActorAccessor.GetInt(actor, MclslActorDataKeys.AncientHarmonyIntegrity, 0),
            AncientHeavenCompatibility = MclslActorAccessor.GetInt(actor, MclslActorDataKeys.AncientHeavenCompatibility, 0),
            TechniqueName = MclslActorAccessor.GetString(actor, MclslActorDataKeys.TechniqueName, "未定"),
            ImmortalFate = MclslActorAccessor.GetInt(actor, MclslActorDataKeys.ImmortalFate, 0),
            Aptitude = aptitude,
            GiftName = gift?.Name ?? string.Empty,
            SpiritualRootGrade = root.GradeName,
            SpiritualRootAttributes = root.AttributeText,
            SpiritualRootCountName = root.CountName,
            SpiritualRootPurity = root.Purity,
            MindState = MclslMindSystem.ReadMindState(actor),
            MortalMiasmaLimit = MclslMortalMiasmaSystem.Capacity(realm),
            MortalMiasma = Math.Clamp(MclslActorAccessor.GetInt(actor, MclslActorDataKeys.MortalMiasma, 0), 0, MclslMortalMiasmaSystem.Capacity(realm)),
            HeartTemperingProgress = Math.Clamp(MclslActorAccessor.GetInt(actor, MclslActorDataKeys.HeartTemperingProgress, 0), 0, 100),
            HeartMethodText = MclslMindSystem.MethodText(actor),
            CultivationProgress = progress,
            TrueEssence = trueEssence,
            NextRealmMinimum = nextRealmMinimum,
            Contribution = MclslActorAccessor.GetInt(actor, MclslActorDataKeys.Contribution, 0),
            SpiritStones = MclslActorAccessor.GetInt(actor, MclslActorDataKeys.SpiritStones, 0),
            FactionAffiliation = MclslActorAccessor.GetString(actor, MclslActorDataKeys.FactionAffiliation, string.Empty),
            LastResult = MclslActorAccessor.GetString(actor, MclslActorDataKeys.LastBreakthroughResult, "无"),
            LineageIntegrity = MclslCultivationLineage.Integrity(actor),
            LineageSummary = MclslCultivationLineage.ChainSummary(actor),
            FoundationWonderName = ancientPath ? string.Empty : MclslActorAccessor.GetString(actor, MclslActorDataKeys.FoundationWonderName, string.Empty),
            FoundationWonderQuality = ancientPath ? 0 : MclslActorAccessor.GetInt(actor, MclslActorDataKeys.FoundationWonderQuality, 1),
            FoundationWonderTags = ancientPath ? string.Empty : MclslActorAccessor.GetString(actor, MclslActorDataKeys.FoundationWonderTags, string.Empty),
            FoundationWonderCategory = ancientPath ? string.Empty : MclslActorAccessor.GetString(actor, MclslActorDataKeys.FoundationWonderCategory, string.Empty),
            FoundationWonderGrade = ancientPath ? string.Empty : MclslActorAccessor.GetString(actor, MclslActorDataKeys.FoundationWonderGrade, string.Empty),
            FoundationWonderCompleteness = ancientPath ? 0 : MclslActorAccessor.GetInt(actor, MclslActorDataKeys.FoundationWonderCompleteness, 0),
            FoundationWonderRuleStrength = ancientPath ? 0 : MclslActorAccessor.GetInt(actor, MclslActorDataKeys.FoundationWonderRuleStrength, 0),
            GoldenCoreLaws = ancientPath ? string.Empty : MclslActorAccessor.GetString(actor, MclslActorDataKeys.GoldenCoreLaws, string.Empty),
            GoldenCorePurity = ancientPath ? 0 : MclslActorAccessor.GetInt(actor, MclslActorDataKeys.GoldenCorePurity, 0),
            GoldenCoreStability = ancientPath ? 0 : MclslActorAccessor.GetInt(actor, MclslActorDataKeys.GoldenCoreStability, 0),
            NascentCaveName = ancientPath ? string.Empty : MclslActorAccessor.GetString(actor, MclslActorDataKeys.NascentCaveName, string.Empty),
            NascentCaveCompatibility = ancientPath ? 0 : MclslActorAccessor.GetInt(actor, MclslActorDataKeys.NascentCaveCompatibility, 0),
            NascentCaveIntegrity = ancientPath ? 0 : MclslActorAccessor.GetInt(actor, MclslActorDataKeys.NascentCaveIntegrity, 0),
            NascentEssenceName = ancientPath ? string.Empty : MclslActorAccessor.GetString(actor, MclslActorDataKeys.NascentEssenceName, string.Empty),
            DivineChangeName = ancientPath ? string.Empty : MclslActorAccessor.GetString(actor, MclslActorDataKeys.DivineChangeName, string.Empty),
            DivineChangeCompatibility = ancientPath ? 0 : MclslActorAccessor.GetInt(actor, MclslActorDataKeys.DivineChangeCompatibility, 0),
            DivineMarrowName = ancientPath ? string.Empty : MclslActorAccessor.GetString(actor, MclslActorDataKeys.DivineMarrowName, string.Empty),
            WorldSoulName = ancientPath ? string.Empty : MclslActorAccessor.GetString(actor, MclslActorDataKeys.WorldSoulName, string.Empty),
            HeavenlyDuty = ancientPath ? string.Empty : MclslActorAccessor.GetString(actor, MclslActorDataKeys.HeavenlyDuty, string.Empty),
            HeavenlyDutyProgress = ancientPath ? 0 : Math.Clamp(worldSoul?.DutyProgress ?? 0, 0, 100),
            HeavenlyDutyBacklash = ancientPath ? 0 : Math.Clamp(worldSoul?.DutyBacklash ?? MclslActorAccessor.GetInt(actor, MclslActorDataKeys.HeavenlyDutyBacklash, 0), 0, 100),
            HarmonyStability = ancientPath ? 0 : MclslActorAccessor.GetInt(actor, MclslActorDataKeys.HarmonyStability, 0),
            HarmonyLeap = ancientPath ? 0 : MclslActorAccessor.GetInt(actor, MclslActorDataKeys.HarmonyLeap, 0),
            InverseTruthName = ancientPath ? string.Empty : MclslActorAccessor.GetString(actor, MclslActorDataKeys.InverseTruthName, string.Empty),
            InverseTruthProgress = ancientPath ? 0 : MclslActorAccessor.GetInt(actor, MclslActorDataKeys.InverseTruthProgress, 0),
            TaishangProgress = ancientPath ? 0 : taishangProgress,
            HuanzhenAnchorYear = MclslActorAccessor.GetInt(actor, MclslActorDataKeys.HuanzhenAnchorYear, -1),
            HuanzhenRestoreCount = MclslActorAccessor.GetInt(actor, MclslActorDataKeys.HuanzhenRestoreCount, 0),
            EligibleForCultivation = eligible,
            HasCultivationData = hasCultivationData,
            HasSpiritualRoot = realGift != null || hasRootArchive
        };

        return view;
    }

    private static string RealmDisplay(string realm, int trueEssence, float progress, int taishangProgress)
    {
        if (string.IsNullOrWhiteSpace(realm))
        {
            int requirement = MclslRealmProgress.LianQiEntryMinimum;
            if (requirement <= 0) return "未入道";
            if (trueEssence <= 0) return "感气：未起";
            int tenths = Math.Clamp((int)Math.Floor(trueEssence * 10f / requirement), 1, 10);
            return "感气" + ChengText(tenths);
        }
        if (realm == MclslRealmIds.ChangSheng && taishangProgress >= 100) return "太上";
        return MclslMinorRealmCatalog.Display(realm, progress);
    }

    private static string ChengText(int tenths) => tenths switch
    {
        1 => "一成",
        2 => "二成",
        3 => "三成",
        4 => "四成",
        5 => "五成",
        6 => "六成",
        7 => "七成",
        8 => "八成",
        9 => "九成",
        _ => "十成"
    };

    private static string SystemDisplay(Actor actor, string system)
    {
        if (system == MclslCultivationSystemIds.NewLaw
            && MclslActorAccessor.GetInt(actor, MclslActorDataKeys.NewLawPioneer, 0) == 1
            && !MclslWorldEpochSystem.IsNewLawActive(MclslRuntime.CurrentYear()))
            return "先行新法";
        return system switch
        {
            MclslCultivationSystemIds.AncientLaw => SpiritualPathDisplay(),
            MclslCultivationSystemIds.NewLaw => "新法",
            _ => "未入道"
        };
    }

    private static string SpiritualPathDisplay()
    {
        string era = MclslWorldRunRepository.Current?.CultivationEpoch ?? MclslWorldEpochSystem.AncientLawEpoch;
        return era switch
        {
            MclslWorldEpochSystem.AncientLawEpoch => "仙道",
            MclslWorldEpochSystem.TransmissionTransitionEpoch => "旧法",
            MclslWorldEpochSystem.NewLawEpoch => "古法",
            _ => "仙道"
        };
    }
}
