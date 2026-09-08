using MySimulatedLongevityRoad.Core;
using MySimulatedLongevityRoad.Data;
using MySimulatedLongevityRoad.Queries;

namespace MySimulatedLongevityRoad.Systems;

internal enum MclslAnnualPipelineStage : byte
{
    Prepare = 0,
    Progression = 1,
    Finalize = 2
}

internal static class MclslAnnualActorPipeline
{
    internal static bool ProcessStage(Actor actor, int annualYear, MclslAnnualPipelineStage stage, out MclslAnnualPipelineStage nextStage)
    {
        nextStage = stage;
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

        switch (stage)
        {
            case MclslAnnualPipelineStage.Prepare:
                return ProcessPrepare(actor, annualYear, out nextStage);
            case MclslAnnualPipelineStage.Progression:
                return ProcessProgression(actor, annualYear, out nextStage);
            case MclslAnnualPipelineStage.Finalize:
                ProcessFinalize(actor);
                return false;
            default:
                return false;
        }
    }

    private static bool ProcessPrepare(Actor actor, int annualYear, out MclslAnnualPipelineStage nextStage)
    {
        nextStage = MclslAnnualPipelineStage.Progression;
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
        if (MclslChildhoodRootSystem.ShouldTrackChildhoodCandidate(actor))
        {
            MclslChildhoodRootSystem.TryProcessAgeFiveDeadline(actor, annualYear);
        }
        MclslCultivationAgeSanity.RepairImpossibleYouthCultivation(actor, annualYear);

        MclslMortalFateEventSystem.TryProcessAnnual(actor, annualYear);
        if (MclslCultivationActorMarker.ShouldKeepTracked(actor))
        {
            MclslWorldActorQuery.Track(actor);
        }
        MclslDiagnostics.Cultivation("pipeline.prepare.done", "actor=" + MclslActorAccessor.Id(actor) + " year=" + annualYear + " next=" + nextStage);
        return true;
    }

    private static bool ProcessProgression(
        Actor actor,
        int annualYear,
        out MclslAnnualPipelineStage nextStage)
    {
        nextStage = MclslAnnualPipelineStage.Finalize;
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

            bool didGrow = MclslAnnualCultivationExecutor.TryApplyOneAnnualStep(actor, annualYear);
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
                MclslCultivationSystem.ProcessAnnualFromScheduler(actor, annualYear);
            }
            else
            {
                MclslDiagnostics.Cultivation(
                    "pipeline.progress.ancient",
                    "actor=" + actorId + " year=" + annualYear + " realm=" + realm);
                MclslAncientLawSystem.ProcessAnnualFromScheduler(actor, annualYear);
            }

            bool alive = MclslActorAccessor.Alive(actor);
            bool hasRealm = alive && !string.IsNullOrWhiteSpace(MclslActorAccessor.Realm(actor));
            bool sensing = alive && !hasRealm && MclslSensingQiSystem.IsSensing(actor);
            if (hasRealm)
            {
                MclslTechniqueOccupationSystem.TryResolveConflictAnnual(actor, annualYear);
                MclslAdventureSystem.RegisterAnnual(actor, annualYear);
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
