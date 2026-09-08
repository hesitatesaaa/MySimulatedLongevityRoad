using System;
using System.Collections.Generic;
using System.Diagnostics;
using MySimulatedLongevityRoad.Core;
using MySimulatedLongevityRoad.Data;
using MySimulatedLongevityRoad.Queries;
using MySimulatedLongevityRoad.Systems.Death;
using MySimulatedLongevityRoad.Traits;

namespace MySimulatedLongevityRoad.Systems;

internal static class MclslWorldEpochSystem
{
    internal const string AncientLawEpoch = "ancient_law";
    internal const string TransmissionTransitionEpoch = "transmission_transition";
    internal const string NewLawEpoch = "new_law";
    internal static int AncientLawDurationYears => MclslRuntimeSettings.TransmissionYear;
    internal const int TransmissionDurationYears = 10;

    private const string EventAncientLawEnd = "epoch_ancient_law_end";
    private const string EventChuanfaDao = "epoch_chuanfa_dao";
    private const string EventNewLawSpread = "epoch_new_law_spread";
    private const string EventLawConflict = "epoch_law_conflict";
    private const string EventMortalMiasma = "epoch_mortal_miasma_truth";
    private const string EventWanxianFounded = "epoch_wanxian_alliance_founded";
    private const string EventAncientRemnants = "epoch_ancient_remnants";
    private const string EventFiveEldersFounded = "epoch_five_elders_founded";
    private const string EventNewLawEra = "epoch_new_law_era";
    // 旧法修士转化会修改特质、功法、世界资源及档案，不能在年度回调中集中执行。
    // 这些预算故意偏小：新法时代可以稍后数帧完全收敛，但不应抢占渲染主线程。
    private const int TransitionSeedBudget = 12;
    private const int TransitionConversionBudget = 6;
    private const double TransitionWorkBudgetMs = 0.45d;
    private static IReadOnlyList<Actor> _ancientQueueSeedActors = Array.Empty<Actor>();
    private static int _ancientQueueSeedCursor;

    internal static bool IsNewLawActive(int year)
    {
        MclslWorldRunState run = MclslWorldRunRepository.Current;
        return run != null && !string.IsNullOrWhiteSpace(run.RunId) && run.NewLawEnabled && year >= run.AncientLawEndYear;
    }

    internal static bool CanPracticeNewLaw(int year) => MclslNewLawPioneerSystem.CanPracticeNewLaw(year);

    internal static bool IsStableNewLawEra(int year)
    {
        MclslWorldRunState run = MclslWorldRunRepository.Current;
        return run != null && string.Equals(run.CultivationEpoch, NewLawEpoch, StringComparison.Ordinal) && year >= run.NewLawStartYear;
    }

    internal static bool IsMortalMiasmaActive(int year)
    {
        MclslWorldRunState run = MclslWorldRunRepository.Current;
        return run != null && run.ImmortalMortalMiasmaEnabled && IsNewLawActive(year);
    }

    internal static int YearsUntilNewLaw(int year)
    {
        MclslWorldRunState run = MclslWorldRunRepository.Current;
        if (run == null || string.IsNullOrWhiteSpace(run.RunId)) return AncientLawDurationYears;
        return Math.Max(0, run.AncientLawEndYear - year);
    }

    internal static void ForceTransmissionForTesting(int year)
    {
        MclslWorldRunRepository.EnsureCurrentRun(year);
        MclslWorldRunState run = MclslWorldRunRepository.Current;
        if (run == null || string.IsNullOrWhiteSpace(run.RunId)) return;

        NormalizeEra(run);
        run.AncientLawEndYear = year;
        run.TransmissionEndYear = year + TransmissionDurationYears;
        run.NewLawStartYear = run.TransmissionEndYear;
        run.CultivationEpoch = TransmissionTransitionEpoch;
        run.NewLawTransitionResolved = false;

        ProcessTransitionYear(run, year);
        MclslWorldRunRepository.EnsureNewLawCatalogs(year);
        MclslTraitRegistration.RefreshRealmTraitVisibility(year);
        MclslWorldArchiveStore.MarkDirty();
    }

    internal static void EnsureAnnualState(int year)
    {
        MclslWorldRunState run = MclslWorldRunRepository.Current;
        if (run == null || string.IsNullOrWhiteSpace(run.RunId)) return;
        NormalizeEra(run);

        if (year < run.AncientLawEndYear)
        {
            run.CultivationEpoch = AncientLawEpoch;
            MclslTraitRegistration.RefreshRealmTraitVisibility(year);
            return;
        }

        if (year < run.TransmissionEndYear)
        {
            run.CultivationEpoch = TransmissionTransitionEpoch;
            CatchUpTransitionYears(run, year);
            MclslTraitRegistration.RefreshRealmTraitVisibility(year);
            MclslWorldArchiveStore.MarkDirty();
            return;
        }

        CatchUpTransitionYears(run, run.TransmissionEndYear - 1);
        run.CultivationEpoch = NewLawEpoch;
        CompleteNewLawEra(run, year);
        MclslTraitRegistration.RefreshRealmTraitVisibility(year);
        MclslWorldArchiveStore.MarkDirty();
    }

    private static void NormalizeEra(MclslWorldRunState run)
    {
        run.FiredHistoricalEvents ??= new();
        run.PendingAncientCultivatorIds ??= new();
        if (run.EraOriginYear <= 0) run.EraOriginYear = Math.Max(0, run.StartYear);
        if (run.AncientLawEndYear <= 0) run.AncientLawEndYear = run.EraOriginYear + AncientLawDurationYears;
        if (run.TransmissionEndYear <= 0) run.TransmissionEndYear = run.AncientLawEndYear + TransmissionDurationYears;
        if (run.NewLawStartYear <= 0) run.NewLawStartYear = run.TransmissionEndYear;
        if (run.NewLawPioneerStartYear <= 0)
            run.NewLawPioneerStartYear = Math.Max(run.EraOriginYear + 80, run.AncientLawEndYear - MclslNewLawPioneerSystem.PioneerLeadYears);
        if (!run.FiredHistoricalEvents.Contains(EventChuanfaDao))
        {
            run.AncientLawEndYear = run.EraOriginYear + AncientLawDurationYears;
            run.TransmissionEndYear = run.AncientLawEndYear + TransmissionDurationYears;
            run.NewLawStartYear = run.TransmissionEndYear;
            run.NewLawPioneerStartYear = Math.Max(run.EraOriginYear + 80, run.AncientLawEndYear - MclslNewLawPioneerSystem.PioneerLeadYears);
        }
        if (string.IsNullOrWhiteSpace(run.CultivationEpoch)) run.CultivationEpoch = AncientLawEpoch;
    }

    private static void CatchUpTransitionYears(MclslWorldRunState run, int targetYear)
    {
        if (run == null || targetYear < run.AncientLawEndYear) return;
        int firstYear = run.TransmissionStepYear >= run.AncientLawEndYear
            ? run.TransmissionStepYear + 1
            : run.AncientLawEndYear;
        int lastYear = Math.Min(targetYear, run.TransmissionEndYear - 1);
        for (int transitionYear = firstYear; transitionYear <= lastYear; transitionYear++)
            ProcessTransitionYear(run, transitionYear);
    }

    private static void ProcessTransitionYear(MclslWorldRunState run, int year)
    {
        int offset = Math.Max(0, year - run.AncientLawEndYear);
        if (offset == 0)
        {
            Fire(run, year, EventAncientLawEnd, "仙道终纪", "千年仙道，至此风雨满楼。", "#CFC7B2");
            Fire(run, year, EventChuanfaDao, "传法天尊证道", "传法立道，天下始闻新法。", "#E2BE55");
            run.TransmissionProved = true;
            run.NewLawEnabled = true;
            run.WanxianAllianceFounded = true;
            run.BackgroundFactions ??= new MclslBackgroundFactionState();
            run.BackgroundFactions.WanXianAllianceInfluence = Math.Max(run.BackgroundFactions.WanXianAllianceInfluence, 80);
            run.BackgroundFactions.AllianceOrderPressure = Math.Max(run.BackgroundFactions.AllianceOrderPressure, 35);
            run.BackgroundFactions.AlliancePolicy = "传法立盟，整顿新法";
            Fire(run, year, EventWanxianFounded, "万仙盟立", "万仙盟传檄诸国，新法自此有统。", "#B7A7FF");
            ScheduleAncientCultivatorQueueSeed(run);
        }
        else if (offset == 1)
        {
            Fire(run, year, EventNewLawSpread, "新法传世", "各地求法者渐众，旧日山门风声鹤唳。", "#FFD37A");
        }
        else if (offset == 2)
        {
            run.LawConflictEnabled = true;
            // 功法占用索引会由年度世界车道在角色结算后统一重建；此处重建会在
            // 纪元切换当帧额外扫描全部修士。
            Fire(run, year, EventLawConflict, "法不可同修之劫", "同法相争，诸脉一夜生隙。", "#FF8877");
        }
        else if (offset == 3)
        {
            run.ImmortalMortalMiasmaEnabled = true;
            Fire(run, year, EventMortalMiasma, "仙凡瘴起", "白先生身陨，仙凡自此隔世。", "#C8E6D0");
        }
        else if (offset == 4)
        {
            Fire(run, year, EventAncientRemnants, "遗修入世", "前代仙修或隐或散，山河间旧影渐稀。", "#CFC7B2");
        }

        int transitionPressure = Math.Max(0, 100 - offset * 20);
        run.BackgroundFactions ??= new MclslBackgroundFactionState();
        run.BackgroundFactions.WanXianAllianceInfluence = Math.Max(run.BackgroundFactions.WanXianAllianceInfluence, 80 - Math.Max(0, offset - 1));
        run.BackgroundFactions.AllianceOrderPressure = Math.Max(run.BackgroundFactions.AllianceOrderPressure, 30 + transitionPressure / 3);
        run.TransmissionStepYear = year;
    }

    /// <summary>
    /// 在传法开始时只保存一个既有索引快照；实际筛选和转化由 TickDeferredTransitionWork
    /// 分帧完成。这样不会在 Actor.updateAge 的同一帧扫描世界或批量修改角色特质。
    /// </summary>
    private static void ScheduleAncientCultivatorQueueSeed(MclslWorldRunState run)
    {
        if (run == null || run.PendingAncientCultivatorIds == null) return;
        if (run.PendingAncientCultivatorIds.Count > 0 || _ancientQueueSeedCursor < _ancientQueueSeedActors.Count) return;
        _ancientQueueSeedActors = MclslCultivatorCandidateIndex.GetCultivatorActorsSnapshot();
        _ancientQueueSeedCursor = 0;
    }

    internal static bool HasPendingTransitionWork => _ancientQueueSeedCursor < _ancientQueueSeedActors.Count
        || (MclslWorldRunRepository.Current?.PendingAncientCultivatorIds?.Count ?? 0) > 0;

    /// <summary>
    /// 用数量和时间双预算消化旧法修士队列。队列数据保存在世界档案中，存档或读档
    /// 不会丢失未完成的转化工作。
    /// </summary>
    internal static void TickDeferredTransitionWork()
    {
        MclslWorldRunState run = MclslWorldRunRepository.Current;
        if (run == null || string.IsNullOrWhiteSpace(run.RunId)) return;
        run.PendingAncientCultivatorIds ??= new List<string>();
        if (!HasPendingTransitionWork) return;

        long started = Stopwatch.GetTimestamp();
        int seedBudget = MclslRuntimeWorkBudget.ScaleCount(TransitionSeedBudget, 2);
        int conversionBudget = MclslRuntimeWorkBudget.ScaleCount(TransitionConversionBudget, 1);
        int seeded = 0;
        while (_ancientQueueSeedCursor < _ancientQueueSeedActors.Count && seeded < seedBudget)
        {
            Actor actor = _ancientQueueSeedActors[_ancientQueueSeedCursor++];
            if (!MclslActorAccessor.Alive(actor)) continue;
            if (MclslActorAccessor.GetString(actor, MclslActorDataKeys.CultivationSystem, string.Empty) != MclslCultivationSystemIds.AncientLaw) continue;
            if (MclslActorAccessor.GetInt(actor, MclslActorDataKeys.AncientLawProcessed, 0) == 1) continue;
            long actorId = MclslActorAccessor.Id(actor);
            if (actorId > 0L) run.PendingAncientCultivatorIds.Add(actorId.ToString());
            seeded++;
            if (HasExceededTransitionBudget(started)) break;
        }

        if (_ancientQueueSeedCursor >= _ancientQueueSeedActors.Count)
        {
            _ancientQueueSeedActors = Array.Empty<Actor>();
            _ancientQueueSeedCursor = 0;
        }

        if (run.PendingAncientCultivatorIds.Count > 0 && !HasExceededTransitionBudget(started))
            ProcessAncientTransitionBatch(run, MclslRuntime.CurrentYear(), conversionBudget, 0);
    }

    internal static void ResumeDeferredTransitionWork(int year)
    {
        MclslWorldRunState run = MclslWorldRunRepository.Current;
        if (run == null || string.IsNullOrWhiteSpace(run.RunId) || !run.NewLawEnabled) return;
        // 读档时静态快照不存在。重新从索引中挑出尚未处理的旧法修士即可；
        // 已完成者有 AncientLawProcessed 标记，不会重复进入持久化队列。
        ScheduleAncientCultivatorQueueSeed(run);
    }

    internal static void ClearDeferredTransitionWork()
    {
        _ancientQueueSeedActors = Array.Empty<Actor>();
        _ancientQueueSeedCursor = 0;
    }

    private static void ProcessAncientTransitionBatch(MclslWorldRunState run, int year, int limit, int transitionOffset)
    {
        if (run == null || limit <= 0 || run.PendingAncientCultivatorIds == null) return;
        int processed = 0;
        while (run.PendingAncientCultivatorIds.Count > 0 && processed < limit)
        {
            int lastIndex = run.PendingAncientCultivatorIds.Count - 1;
            string raw = run.PendingAncientCultivatorIds[lastIndex];
            run.PendingAncientCultivatorIds.RemoveAt(lastIndex);
            if (!long.TryParse(raw, out long actorId)) continue;
            Actor actor = FindActor(actorId);
            if (!MclslActorAccessor.Alive(actor)) continue;
            if (MclslActorAccessor.GetInt(actor, MclslActorDataKeys.AncientLawProcessed, 0) == 1) continue;
            if (MclslActorAccessor.GetString(actor, MclslActorDataKeys.CultivationSystem, string.Empty) != MclslCultivationSystemIds.AncientLaw) continue;
            processed++;

            if (TryConvertLegacyAncientLongevity(actor, year)) continue;
            if (TryConvertAncientRemnant(actor, year, transitionOffset)) continue;
            MarkAncientRemnant(actor, year);
        }
    }

    private static bool TryConvertLegacyAncientLongevity(Actor actor, int year)
    {
        if (MclslActorAccessor.Realm(actor) != MclslRealmIds.ChangSheng) return false;

        MclslCultivationStateTransitions.TrySetCultivationSystem(actor, MclslCultivationSystemIds.NewLaw);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.AncientLawStatus, string.Empty);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.AncientLawProcessed, 1);
        MclslCultivationSystem.ApplyManualRealmGrant(
            actor,
            MclslTraitRegistration.TraitIdForRealm(MclslRealmIds.ChangSheng),
            MclslRealmIds.ChangSheng,
            year,
            forceAncient: false,
            forceNewLaw: true);
        return true;
    }

    private static bool TryConvertAncientRemnant(Actor actor, int year, int transitionOffset)
    {
        _ = transitionOffset;
        if (!MclslActorAccessor.Alive(actor)) return false;
        string realm = MclslActorAccessor.Realm(actor);
        int realmIndex = MclslRealmIds.Index(realm);
        if (realmIndex < 0 || realm == MclslRealmIds.ChangSheng) return false;

        MclslActorAccessor.Set(actor, MclslActorDataKeys.AncientLawStatus, "求取新法要素");
        if (!TryPrepareAncientConversion(actor, year, realm, realmIndex, out string missing))
        {
            MclslActorAccessor.Set(actor, MclslActorDataKeys.LastBreakthroughResult, "转修新法尚缺“" + missing + "”，将继续寻找");
            return false;
        }

        string oldTechniqueId = MclslActorAccessor.GetString(actor, MclslActorDataKeys.TechniqueId, string.Empty);
        string oldTechniqueName = MclslActorAccessor.GetString(actor, MclslActorDataKeys.TechniqueName, string.Empty);
        MclslCultivationStateTransitions.TrySetCultivationSystem(actor, MclslCultivationSystemIds.NewLaw);
        bool assigned = MclslTechniqueOccupationSystem.AssignLeastCrowdedTechniqueForRealm(actor, year, realm, true);
        if (!assigned && !string.IsNullOrWhiteSpace(oldTechniqueId))
        {
            string normalized = MclslCultivationCatalog.NormalizeTechniqueId(oldTechniqueId);
            MclslActorAccessor.Set(actor, MclslActorDataKeys.TechniqueId, normalized);
            MclslActorAccessor.Set(actor, MclslActorDataKeys.TechniqueName, oldTechniqueName);
        }

        if (realm == MclslRealmIds.LianQi)
            MclslActorAccessor.Set(actor, MclslActorDataKeys.FoundationChanceBonus,
                Math.Max(100, MclslActorAccessor.GetInt(actor, MclslActorDataKeys.FoundationChanceBonus, 0)));

        MclslActorAccessor.Set(actor, MclslActorDataKeys.AncientLawStatus, string.Empty);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.AncientLawProcessed, 1);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.LastBreakthroughResult, "旧法修为与已求得要素转入新法体系");
        MclslTraitRegistration.SyncRealmTraitFamily(actor, realm);
        MclslTraitRegistration.SyncNativeRealmTraits(actor, realm);
        string name = MclslActorAccessor.DisplayName(actor, realm);
        string techniqueName = MclslActorAccessor.GetString(actor, MclslActorDataKeys.TechniqueName, "无名功法");
        MclslWorldRunRepository.AddEvent(year, "ancient_convert_new_law", name + "转修新法",
            name + "保留原有" + MclslRealmIds.Display(realm) + "修为，集齐新法所需要素，并另择《" + techniqueName + "》继续修行。", actor);
        MclslCultivationWake.EnsureAwake(
            actor,
            ensureEntryFromGift: true,
            enqueueAnnual: true,
            refreshUi: false);
        return true;
    }

    private static bool TryPrepareAncientConversion(Actor actor, int year, string realm, int realmIndex, out string missing)
    {
        missing = string.Empty;
        if (realmIndex >= MclslRealmIds.Index(MclslRealmIds.ZhuJi)
            && string.IsNullOrWhiteSpace(MclslActorAccessor.GetString(actor, MclslActorDataKeys.FoundationWonderName, string.Empty))
            && !MclslNewLawBreakthroughSystem.TryAcquireFoundationForConversion(actor, year))
        {
            missing = "筑基奇物";
            return false;
        }
        if (realmIndex >= MclslRealmIds.Index(MclslRealmIds.JinDan)
            && string.IsNullOrWhiteSpace(MclslActorAccessor.GetString(actor, MclslActorDataKeys.GoldenCoreLaws, string.Empty)))
        {
            MclslNewLawBreakthroughSystem.BuildGoldenCoreData(actor, year,
                Math.Clamp(MclslActorAccessor.GetInt(actor, MclslActorDataKeys.Aptitude, 50), 1, 100));
        }
        if (realmIndex >= MclslRealmIds.Index(MclslRealmIds.YuanYing)
            && string.IsNullOrWhiteSpace(MclslActorAccessor.GetString(actor, MclslActorDataKeys.NascentEssenceName, string.Empty))
            && !MclslWorldCaveSystem.TryAcquireExistingCaveForConversion(actor, year))
        {
            missing = "洞天与天地之精";
            return false;
        }
        if (realmIndex >= MclslRealmIds.Index(MclslRealmIds.HuaShen)
            && string.IsNullOrWhiteSpace(MclslActorAccessor.GetString(actor, MclslActorDataKeys.DivineMarrowName, string.Empty))
            && !MclslWorldChangeSystem.TryAcquireExistingChangeForConversion(actor, year))
        {
            missing = "天地之变与天地之髓";
            return false;
        }
        if (realmIndex >= MclslRealmIds.Index(MclslRealmIds.HeDao)
            && !MclslWorldSoulSystem.TryAcquireExistingSoulForConversion(actor, year))
        {
            missing = "无主天地之魄";
            return false;
        }
        return MclslCultivationSystem.ManualStageSatisfied(actor, realm);
    }

    internal static bool TryConvertAncientPioneer(Actor actor, int year)
    {
        if (!MclslNewLawPioneerSystem.IsPioneerEra(year) || !MclslActorAccessor.Alive(actor)) return false;
        if (MclslActorAccessor.GetString(actor, MclslActorDataKeys.CultivationSystem, string.Empty) != MclslCultivationSystemIds.AncientLaw) return false;
        string realm = MclslActorAccessor.Realm(actor);
        int realmIndex = MclslRealmIds.Index(realm);
        if (realmIndex < MclslRealmIds.Index(MclslRealmIds.ZhuJi)
            || realmIndex > MclslRealmIds.Index(MclslRealmIds.YuanYing)) return false;

        MclslActorAccessor.Set(actor, MclslActorDataKeys.AncientLawStatus, "试修新法");
        if (!TryPrepareAncientConversion(actor, year, realm, realmIndex, out string missing))
        {
            MclslActorAccessor.Set(actor, MclslActorDataKeys.LastBreakthroughResult,
                "尝试转入早期新法，尚缺“" + missing + "”");
            return false;
        }

        string oldTechniqueId = MclslActorAccessor.GetString(actor, MclslActorDataKeys.TechniqueId, string.Empty);
        string oldTechniqueName = MclslActorAccessor.GetString(actor, MclslActorDataKeys.TechniqueName, string.Empty);
        MclslCultivationStateTransitions.TrySetCultivationSystem(actor, MclslCultivationSystemIds.NewLaw);
        bool assigned = MclslTechniqueOccupationSystem.AssignLeastCrowdedTechniqueForRealm(actor, year, realm, true);
        if (!assigned && !string.IsNullOrWhiteSpace(oldTechniqueId))
        {
            string normalized = MclslCultivationCatalog.NormalizeTechniqueId(oldTechniqueId);
            MclslActorAccessor.Set(actor, MclslActorDataKeys.TechniqueId, normalized);
            MclslActorAccessor.Set(actor, MclslActorDataKeys.TechniqueName, oldTechniqueName);
        }

        MclslActorAccessor.Set(actor, MclslActorDataKeys.AncientLawStatus, string.Empty);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.LastBreakthroughResult, "于传法证道前转试新法");
        MclslTraitRegistration.SyncRealmTraitFamily(actor, realm);
        MclslTraitRegistration.SyncNativeRealmTraits(actor, realm);
        string name = MclslActorAccessor.DisplayName(actor, realm);
        string techniqueName = MclslActorAccessor.GetString(actor, MclslActorDataKeys.TechniqueName, "无名功法");
        MclslWorldRunRepository.AddEvent(year, "newlaw_pioneer_conversion", name + "转试新法",
            name + "在新法尚未定世之时，集齐所需要素，保留" + MclslRealmIds.Display(realm)
            + "修为转入《" + techniqueName + "》。", actor);
        MclslCultivationWake.EnsureAwake(
            actor,
            ensureEntryFromGift: true,
            enqueueAnnual: true,
            refreshUi: false);
        MclslWorldArchiveStore.MarkDirty();
        return true;
    }

    internal static bool TryRetryAncientConversion(Actor actor, int year)
    {
        if (!IsStableNewLawEra(year) || !MclslActorAccessor.Alive(actor)) return false;
        if (MclslActorAccessor.GetString(actor, MclslActorDataKeys.CultivationSystem, string.Empty) != MclslCultivationSystemIds.AncientLaw) return false;
        string realm = MclslActorAccessor.Realm(actor);
        if (string.IsNullOrWhiteSpace(realm) || realm == MclslRealmIds.ChangSheng) return false;
        return TryConvertAncientRemnant(actor, year, 0);
    }

    private static void MarkAncientRemnant(Actor actor, int year)
    {
        string remnantLabel = IsStableNewLawEra(year) ? "古法遗修" : "旧法遗修";
        MclslActorAccessor.Set(actor, MclslActorDataKeys.AncientLawProcessed, 1);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.AncientLawStatus, remnantLabel);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.AncientLegacyPotential, Math.Max(MclslActorAccessor.GetInt(actor, MclslActorDataKeys.AncientLegacyPotential, 0), 20 + Math.Max(0, MclslRealmIds.Index(MclslActorAccessor.Realm(actor))) * 10));
        MclslActorAccessor.Set(actor, MclslActorDataKeys.LastBreakthroughResult, remnantLabel);
        if (StableHash(MclslActorAccessor.Id(actor) + "|ancient_remnant_notice|" + year) % 100 >= 8) return;
        MclslWorldRunRepository.AddEvent(year, "ancient_remnant", MclslActorAccessor.DisplayName(actor) + "隐为" + remnantLabel, "前代道影，渐入风尘。", actor);
    }

    private static void CompleteNewLawEra(MclslWorldRunState run, int year)
    {
        run.TransmissionProved = true;
        run.NewLawEnabled = true;
        run.LawConflictEnabled = true;
        // The annual world lane rebuilds the shared technique index once after actor processing.
        // Avoid a second full cultivator snapshot scan every stable new-law year.
        run.ImmortalMortalMiasmaEnabled = true;
        run.WanxianAllianceFounded = true;
        run.FiveEldersFounded = true;
        run.NewLawTransitionResolved = true;
        run.BackgroundFactions ??= new MclslBackgroundFactionState();
        run.BackgroundFactions.WanXianAllianceInfluence = Math.Max(run.BackgroundFactions.WanXianAllianceInfluence, 75);
        run.BackgroundFactions.FiveEldersInfluence = Math.Max(run.BackgroundFactions.FiveEldersInfluence, 35);
        run.BackgroundFactions.FiveEldersSubversion = Math.Max(run.BackgroundFactions.FiveEldersSubversion, 18);
        run.BackgroundFactions.AlliancePolicy = "维系新法秩序";
        run.BackgroundFactions.FiveEldersPolicy = "暗中渗透新法格局";
        Fire(run, year, EventFiveEldersFounded, "五天尊证道", "五老同席，暗潮入世。", "#B7A7FF");
        Fire(run, year, EventNewLawEra, "新法时代开启", "旧山河远去，新法定世。", "#FFD37A");
    }

    private static void Fire(MclslWorldRunState run, int year, string eventId, string title, string body, string color)
    {
        if (run.FiredHistoricalEvents.Contains(eventId)) return;
        run.FiredHistoricalEvents.Add(eventId);
        MclslWorldRunRepository.AddEvent(year, eventId, title, body);
        MclslAnnouncementSystem.Enqueue(title, color, 10f, 1);
    }

    private static Actor FindActor(long id)
    {
        if (id <= 0 || World.world?.units == null) return null;
        try { return World.world.units.get(id); }
        catch (Exception ex)
        {
            MclslDiagnostics.Error("world-epoch-find-actor", "纪元系统按 ID 查找角色失败: " + ex.Message);
            return null;
        }
    }

    private static int SafeAge(Actor actor)
    {
        try { return Math.Max(0, (int)Math.Floor(actor?.getAge() ?? 0f)); }
        catch { return 0; }
    }

    private static int StableHash(string value)
    {
        unchecked { int hash = 83; foreach (char c in value ?? string.Empty) hash = hash * 47 + c; return hash & int.MaxValue; }
    }

    private static bool HasExceededTransitionBudget(long startedTimestamp)
    {
        double budgetMs = MclslRuntimeWorkBudget.ScaleMilliseconds(TransitionWorkBudgetMs, 0.08d);
        return Stopwatch.GetTimestamp() - startedTimestamp > budgetMs * Stopwatch.Frequency / 1000d;
    }
}
