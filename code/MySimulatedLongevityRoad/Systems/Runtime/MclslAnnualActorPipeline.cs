using MySimulatedLongevityRoad.Core;
using MySimulatedLongevityRoad.Data;
using MySimulatedLongevityRoad.Queries;

namespace MySimulatedLongevityRoad.Systems;

internal enum MclslAnnualPipelineStage : byte
{
    Prepare = 0,
    Progression = 1,
    Finalize = 2,
    Market = 3
}

internal static class MclslAnnualActorPipeline
{
    internal static bool ProcessStage(Actor actor, int annualYear, MclslAnnualPipelineStage stage,
        int step, out MclslAnnualPipelineStage nextStage, out int nextStep)
    {
        nextStage = stage;
        nextStep = step;
        MclslDiagnostics.Cultivation(
            "pipeline.stage.enter",
            "actor=" + MclslActorAccessor.Id(actor)
            + " year=" + annualYear
            + " stage=" + stage
            + " alive=" + MclslActorAccessor.Alive(actor)
            + " realm=" + MclslActorAccessor.Realm(actor)
            + " system=" + MclslActorAccessor.GetString(actor, MclslActorDataKeys.CultivationSystem, string.Empty)
            + " essence=" + MclslCultivationGrowthSystem.CurrentTrueEssence(actor));
        if (actor?.data == null || !MclslActorAccessor.Alive(actor) || annualYear <= 0)
        {
            MclslDiagnostics.Cultivation(
                "pipeline.stage.skip",
                "actor=" + MclslActorAccessor.Id(actor)
                + " year=" + annualYear
                + " stage=" + stage
                + " reason=data-dead-year");
            return false;
        }

        bool bagStep = stage == MclslAnnualPipelineStage.Market
            || stage == MclslAnnualPipelineStage.Prepare && step is 0 or 1 or 3 or 5 or 6 or 7
            || stage == MclslAnnualPipelineStage.Progression && step is 1 or 2;
        if (bagStep) MclslBagSystem.BeginTransaction(actor);
        try
        {
            switch (stage)
            {
                case MclslAnnualPipelineStage.Prepare:
                    return ProcessPrepare(actor, annualYear, step, out nextStage, out nextStep);
                case MclslAnnualPipelineStage.Progression:
                    return ProcessProgressionStep(actor, annualYear, step, out nextStage, out nextStep);
                case MclslAnnualPipelineStage.Market:
                    return ProcessMarketStep(actor, annualYear, step, out nextStage, out nextStep);
                case MclslAnnualPipelineStage.Finalize:
                    ProcessFinalize(actor);
                    return false;
                default:
                    return false;
            }
        }
        finally { if (bagStep) MclslBagSystem.EndTransaction(actor); }
    }

    private static bool ProcessPrepare(Actor actor, int annualYear, int step,
        out MclslAnnualPipelineStage nextStage, out int nextStep)
    {
        nextStage = MclslAnnualPipelineStage.Prepare;
        nextStep = step + 1;
        MclslDiagnostics.Cultivation(
            "pipeline.prepare.enter",
            "actor=" + MclslActorAccessor.Id(actor)
            + " year=" + annualYear
            + " eligible=" + MclslEligibility.CanCultivate(actor)
            + " keepTracked=" + MclslCultivationActorMarker.ShouldKeepTracked(actor));
        if (!MclslEligibility.CanCultivate(actor))
        {
            MclslDiagnostics.Cultivation("pipeline.prepare.skip", "actor=" + MclslActorAccessor.Id(actor) + " year=" + annualYear + " reason=not-eligible");
            return false;
        }
        switch (step)
        {
            case 0: MclslArtifactSystem.OnActorInitialized(actor); break;
            case 1: MclslAncientMentorshipSystem.ProcessAnnual(actor, annualYear); break;
            case 2: MclslImmortalPathSystem.TryNaturalGrant(actor); break;
            case 3: MclslMaterialDiscovery.TryAnnualActivity(actor, annualYear); break;
            case 4:
                if (MclslChildhoodRootSystem.ShouldTrackChildhoodCandidate(actor))
                    MclslChildhoodRootSystem.TryProcessAgeFiveDeadline(actor, annualYear);
                break;
            case 5: MclslProfessionSystem.ProcessAnnual(actor, annualYear); break;
            case 6: MclslItemUseSystem.SyncPersistent(actor); break;
            case 7: MclslItemUseSystem.TryAutoCultivationConsumables(actor, annualYear); break;
            case 8: MclslCultivationAgeSanity.RepairImpossibleYouthCultivation(actor, annualYear); break;
            case 9: MclslMortalFateEventSystem.TryProcessAnnual(actor, annualYear); break;
            case 10:
                if (MclslCultivationActorMarker.ShouldKeepTracked(actor)) MclslWorldActorQuery.Track(actor);
                nextStage = MclslAnnualPipelineStage.Progression;
                nextStep = 0;
                break;
            default:
                nextStage = MclslAnnualPipelineStage.Progression;
                nextStep = 0;
                break;
        }
        return true;
    }

    private static bool ProcessProgression(
        Actor actor,
        int annualYear,
        out MclslAnnualPipelineStage nextStage)
    {
        nextStage = MclslAnnualPipelineStage.Market;
        long actorId = MclslActorAccessor.Id(actor);
        if (!MclslAnnualExecutionContext.TryEnter(actor, annualYear)) return false;

        try
        {
            if (MclslLongevityRules.TryExpireAtAnnualLimit(actor, annualYear))
            {
                MclslDiagnostics.Cultivation(
                    "pipeline.progress.skip",
                    "actor=" + actorId + " year=" + annualYear + " reason=expired-at-annual-limit");
                return false;
            }

            if (string.IsNullOrWhiteSpace(MclslActorAccessor.Realm(actor))
                && MclslSpiritualRootSystem.HasCultivationPotential(actor))
            {
                MclslSpiritualRootEntrySystem.TryEnterFromGiftTrait(actor, annualYear);
            }

            string system = MclslActorAccessor.GetString(
                actor,
                MclslActorDataKeys.CultivationSystem,
                string.Empty);
            string realm = MclslActorAccessor.Realm(actor);
            MclslDiagnostics.Cultivation(
                "pipeline.progress.before_executor",
                "actor=" + actorId
                + " year=" + annualYear
                + " system=" + system
                + " realm=" + realm
                + " essence=" + MclslCultivationGrowthSystem.CurrentTrueEssence(actor)
                + " lastCultYear=" + MclslActorAccessor.GetInt(actor, MclslActorDataKeys.LastCultivationYear, -999));

            long growthSample = MclslPerformanceProbe.Begin();
            bool didGrow;
            try { didGrow = MclslAnnualCultivationExecutor.TryApplyOneAnnualStep(actor, annualYear); }
            finally { MclslPerformanceProbe.End("年度角色.修炼增长", growthSample); }
            int committedCultivationYear = MclslCultivationSystem.NormalizeLastCultivationYear(
                actor,
                annualYear,
                MclslActorAccessor.GetInt(actor, MclslActorDataKeys.LastCultivationYear, -1));
            bool growthCommitted = didGrow || committedCultivationYear >= annualYear;
            if (!growthCommitted)
            {
                MclslDiagnostics.Cultivation(
                    "pipeline.progress.no_growth",
                    "actor=" + actorId
                    + " year=" + annualYear
                    + " essence=" + MclslCultivationGrowthSystem.CurrentTrueEssence(actor)
                    + " lastCultYear=" + committedCultivationYear);
                return false;
            }

            system = MclslActorAccessor.GetString(
                actor,
                MclslActorDataKeys.CultivationSystem,
                string.Empty);
            realm = MclslActorAccessor.Realm(actor);
            bool useNewLaw = system == MclslCultivationSystemIds.NewLaw
                || (string.IsNullOrWhiteSpace(system)
                    && MclslWorldEpochSystem.IsNewLawActive(annualYear));

            if (string.IsNullOrWhiteSpace(realm)
                && MclslSpiritualRootSystem.HasCultivationPotential(actor))
            {
                MclslDiagnostics.Cultivation(
                    "pipeline.progress.sensing",
                    "actor=" + actorId + " year=" + annualYear + " system=" + system);
                MclslSensingQiSystem.ProcessAnnual(
                    actor,
                    annualYear,
                    system == MclslCultivationSystemIds.AncientLaw);
            }
            else if (useNewLaw)
            {
                MclslDiagnostics.Cultivation(
                    "pipeline.progress.newlaw",
                    "actor=" + actorId + " year=" + annualYear + " realm=" + realm);
                long cultivationSample = MclslPerformanceProbe.Begin();
                try { MclslCultivationSystem.ProcessAnnualFromScheduler(actor, annualYear); }
                finally { MclslPerformanceProbe.End("年度角色.新法修炼", cultivationSample); }
            }
            else
            {
                MclslDiagnostics.Cultivation(
                    "pipeline.progress.ancient",
                    "actor=" + actorId + " year=" + annualYear + " realm=" + realm);
                long cultivationSample = MclslPerformanceProbe.Begin();
                try { MclslAncientLawSystem.ProcessAnnualFromScheduler(actor, annualYear); }
                finally { MclslPerformanceProbe.End("年度角色.旧法修炼", cultivationSample); }
            }

            bool alive = MclslActorAccessor.Alive(actor);
            bool hasRealm = alive && !string.IsNullOrWhiteSpace(MclslActorAccessor.Realm(actor));
            bool sensing = alive && !hasRealm && MclslSensingQiSystem.IsSensing(actor);
            if (hasRealm)
            {
                MclslSpellSystem.TryProgressAnnual(actor, annualYear);
                if (MclslArtifactSystem.EquippedArtifactId(actor, MclslArtifactEquipmentSlot.Amulet) == "B080")
                    actor.restoreHealthPercent(0.10f);
                long techniqueSample = MclslPerformanceProbe.Begin();
                try { MclslTechniqueOccupationSystem.TryResolveConflictAnnual(actor, annualYear); }
                finally { MclslPerformanceProbe.End("年度角色.功法占用", techniqueSample); }
                long adventureSample = MclslPerformanceProbe.Begin();
                try { MclslAdventureSystem.RegisterAnnual(actor, annualYear); }
                finally { MclslPerformanceProbe.End("年度角色.冒险登记", adventureSample); }
            }

            MclslDiagnostics.Cultivation(
                "pipeline.progress.done",
                "actor=" + actorId
                + " year=" + annualYear
                + " hasRealm=" + hasRealm
                + " sensing=" + sensing
                + " realm=" + MclslActorAccessor.Realm(actor)
                + " essence=" + MclslCultivationGrowthSystem.CurrentTrueEssence(actor));
            return hasRealm || sensing;
        }
        finally
        {
            MclslAnnualExecutionContext.Exit(actor, annualYear);
        }
    }

    private static bool ProcessMarket(Actor actor, int annualYear, out MclslAnnualPipelineStage nextStage)
    {
        nextStage = MclslAnnualPipelineStage.Finalize;
        if (string.IsNullOrWhiteSpace(MclslActorAccessor.Realm(actor))) return true;
        long sample = MclslPerformanceProbe.Begin();
        bool purchased;
        try { purchased = MclslTianxuanMarket.TryBuyNeeded(actor, annualYear); }
        finally { MclslPerformanceProbe.End("天玄镜.需求购买", sample); }
        if (purchased && MclslTianxuanMarket.PurchasesInYear(actor, annualYear) < MclslTianxuanMarket.MaxAnnualPurchases)
        {
            nextStage = MclslAnnualPipelineStage.Market;
            return true;
        }
        long sellSample = MclslPerformanceProbe.Begin();
        try { MclslTianxuanMarket.TryListSurplus(actor, annualYear); }
        finally { MclslPerformanceProbe.End("天玄镜.多余出售", sellSample); }
        return true;
    }

    private static bool ProcessProgressionStep(Actor actor, int year, int step,
        out MclslAnnualPipelineStage nextStage, out int nextStep)
    {
        nextStage = MclslAnnualPipelineStage.Progression;
        nextStep = step + 1;
        if (!MclslAnnualExecutionContext.TryEnter(actor, year)) return false;
        try
        {
            bool hasRealm = !string.IsNullOrWhiteSpace(MclslActorAccessor.Realm(actor));
            switch (step)
            {
                case 0:
                    if (MclslLongevityRules.TryExpireAtAnnualLimit(actor, year)) return false;
                    if (!hasRealm && MclslSpiritualRootSystem.HasCultivationPotential(actor))
                        MclslSpiritualRootEntrySystem.TryEnterFromGiftTrait(actor, year);
                    bool grew = MclslAnnualCultivationExecutor.TryApplyOneAnnualStep(actor, year);
                    return grew || MclslCultivationSystem.NormalizeLastCultivationYear(actor, year,
                        MclslActorAccessor.GetInt(actor, MclslActorDataKeys.LastCultivationYear, -1)) >= year;
                case 1:
                    string system = MclslActorAccessor.GetString(actor, MclslActorDataKeys.CultivationSystem);
                    if (!hasRealm && MclslSpiritualRootSystem.HasCultivationPotential(actor))
                        MclslSensingQiSystem.ProcessAnnual(actor, year, system == MclslCultivationSystemIds.AncientLaw);
                    else if (system == MclslCultivationSystemIds.NewLaw
                        || (string.IsNullOrWhiteSpace(system) && MclslWorldEpochSystem.IsNewLawActive(year)))
                        MclslCultivationSystem.ProcessAnnualFromScheduler(actor, year);
                    else
                        MclslAncientLawSystem.ProcessAnnualFromScheduler(actor, year);
                    break;
                case 2:
                    if (hasRealm) MclslSpellSystem.TryProgressAnnual(actor, year);
                    break;
                case 3:
                    if (hasRealm && MclslArtifactSystem.EquippedArtifactId(actor, MclslArtifactEquipmentSlot.Amulet) == "B080")
                        actor.restoreHealthPercent(0.10f);
                    break;
                case 4:
                    if (hasRealm) MclslTechniqueOccupationSystem.TryResolveConflictAnnual(actor, year);
                    break;
                case 5:
                    if (hasRealm) MclslAdventureSystem.RegisterAnnual(actor, year);
                    break;
                default:
                    bool sensing = !hasRealm && MclslSensingQiSystem.IsSensing(actor);
                    nextStage = MclslAnnualPipelineStage.Market;
                    nextStep = 0;
                    return hasRealm || sensing;
            }
            return true;
        }
        finally { MclslAnnualExecutionContext.Exit(actor, year); }
    }

    private static bool ProcessMarketStep(Actor actor, int year, int step,
        out MclslAnnualPipelineStage nextStage, out int nextStep)
    {
        nextStage = MclslAnnualPipelineStage.Market;
        nextStep = step + 1;
        if (string.IsNullOrWhiteSpace(MclslActorAccessor.Realm(actor)))
        {
            nextStage = MclslAnnualPipelineStage.Finalize;
            nextStep = 0;
            return true;
        }
        if (step == 0)
        {
            bool purchased = MclslTianxuanMarket.TryBuyNeeded(actor, year);
            if (purchased && MclslTianxuanMarket.PurchasesInYear(actor, year) < MclslTianxuanMarket.MaxAnnualPurchases)
            {
                nextStep = 0;
                return true;
            }
            nextStep = 1;
            return true;
        }
        MclslTianxuanMarket.TryListSurplus(actor, year);
        nextStage = MclslAnnualPipelineStage.Finalize;
        nextStep = 0;
        return true;
    }

    private static void ProcessFinalize(Actor actor)
    {
        MclslDiagnostics.Cultivation(
            "pipeline.finalize.enter",
            "actor=" + MclslActorAccessor.Id(actor)
            + " alive=" + MclslActorAccessor.Alive(actor)
            + " realm=" + MclslActorAccessor.Realm(actor)
            + " essence=" + MclslCultivationGrowthSystem.CurrentTrueEssence(actor));
        if (!MclslActorAccessor.Alive(actor))
        {
            MclslCultivatorCandidateIndex.Remove(MclslActorAccessor.Id(actor));
            return;
        }

        MclslWorldActorQuery.Track(actor);
    }
}
