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
    // Saved cursors keep their original operation IDs. Empty operations are not
    // scheduled; remaining work still commits a cursor after each operation.
    internal static bool ProcessStage(Actor actor, int annualYear, MclslAnnualPipelineStage stage,
        int step, out MclslAnnualPipelineStage nextStage, out int nextStep)
    {
        nextStage = stage;
        nextStep = step;
        if (actor?.data == null || !MclslActorAccessor.Alive(actor) || annualYear <= 0)
        {
            if (stage == MclslAnnualPipelineStage.Market) MclslTianxuanMarket.ForgetPurchaseBatch(actor);
            return false;
        }
        if (MySimulatedLongevityRoad.Traits.MclslBeastActorRegistration.IsOrdinary(actor))
        {
            if (stage == MclslAnnualPipelineStage.Prepare && step == 0)
                MclslBeastCultivation.TickAnnual(actor, annualYear);
            MclslWorldActorQuery.Track(actor);
            MclslActorProjectionIndex.Update(actor);
            return false;
        }

        // One economy scope per pipeline operation. Profession crafting, market
        // demand planning and surplus listing all ask for the same reserve plan
        // inside this single operation. The prepare skip-scan runs before the
        // scope so the step-5 gate below sees the effective step (a step can jump
        // past several skipped operations); it has no side effects that need the
        // scope, and a throw here leaves no scope open.
        if (stage == MclslAnnualPipelineStage.Prepare) step = NextPrepareStep(actor, annualYear, step);
        // Cross-actor priority support for the family focus must run before this
        // actor's annual scope and bag transaction; it opens and ends its own
        // focus scope/transaction when it acts. When it is due but the frame
        // budget is spent it returns Pending: keep Prepare step 5 for the next
        // operation and do not run normal supply first (which would draw the
        // family store ahead of the focus).
        if (stage == MclslAnnualPipelineStage.Prepare && step == 5 && MclslEligibility.CanCultivate(actor)
            && MclslFamilySystem.ProcessPriorityAid(actor, annualYear) == MclslFamilyPriorityAid.Pending)
        {
            nextStage = MclslAnnualPipelineStage.Prepare;
            nextStep = 5;
            return true;
        }

        bool transactionStarted = false;
        bool completed;
        MclslAnnualEconomyPlan.Begin(MclslActorAccessor.Id(actor), annualYear);
        try
        {
            bool bagStep = stage == MclslAnnualPipelineStage.Market
                || stage == MclslAnnualPipelineStage.Prepare && step is 0 or 1 or 3 or 5 or 6 or 7
                || stage == MclslAnnualPipelineStage.Progression && step is 1 or 2;
            // Creating a transaction must not create an empty bag just to discover
            // that no work applies. Reward paths create their bag when needed.
            bagStep = bagStep && MclslBagSystem.PeekExisting(actor) != null;
            if (bagStep)
            {
                MclslBagSystem.BeginTransaction(actor);
                // Set only after the depth increment returned: a throw inside
                // BeginTransaction must not make EndTransaction report an
                // unbalanced transaction from this finally.
                transactionStarted = true;
            }
            switch (stage)
            {
                case MclslAnnualPipelineStage.Prepare:
                    completed = ProcessPrepare(actor, annualYear, step, out nextStage, out nextStep);
                    break;
                case MclslAnnualPipelineStage.Progression:
                    completed = ProcessProgressionStep(actor, annualYear, step, out nextStage, out nextStep);
                    break;
                case MclslAnnualPipelineStage.Market:
                    completed = ProcessMarketStep(actor, annualYear, step, out nextStage, out nextStep);
                    break;
                case MclslAnnualPipelineStage.Finalize:
                    ProcessFinalize(actor);
                    completed = false;
                    break;
                default:
                    throw new System.InvalidOperationException("Unknown annual actor stage: " + stage);
            }
        }
        finally
        {
            // Invalidate first: the inventory commit below publishes change flags
            // and never reads the plan, and a throw there must not keep the scope
            // (and therefore a stale plan) alive for the next operation.
            MclslAnnualEconomyPlan.Invalidate();
            if (transactionStarted) MclslBagSystem.EndTransaction(actor);
        }
        return completed;
    }

    private static int NextPrepareStep(Actor actor, int year, int step)
    {
        while (step < 10)
        {
            bool skip = step switch
            {
                0 => !MclslArtifactSystem.NeedsAnnualPass(actor),
                // Old-law actors without a teacher may still recruit students.
                1 => MclslActorAccessor.GetString(actor, MclslActorDataKeys.CultivationSystem)
                    != MclslCultivationSystemIds.AncientLaw,
                3 => ((MclslActorAccessor.Id(actor) ^ year) & 1L) != 0,
                4 => !MclslChildhoodRootSystem.ShouldTrackChildhoodCandidate(actor),
                7 => !MclslItemUseSystem.HasCultivationConsumables(actor),
                // Once every realm is age-legal the expensive repair is a no-op.
                8 => !MclslCultivationAgeSanity.NeedsAgeRepair(actor),
                _ => false
            };
            if (!skip) break;
            step++;
        }
        return step;
    }

    private static bool ProcessPrepare(Actor actor, int year, int step,
        out MclslAnnualPipelineStage nextStage, out int nextStep)
    {
        nextStage = MclslAnnualPipelineStage.Prepare;
        nextStep = step + 1;
        if (!MclslEligibility.CanCultivate(actor)) return false;
        switch (step)
        {
            case 0: MclslArtifactSystem.OnActorInitialized(actor); break;
            case 1: MclslAncientMentorshipSystem.ProcessAnnual(actor, year); break;
            case 2:
                long grantSample = MclslPerformanceProbe.Begin();
                try { MclslImmortalPathSystem.TryNaturalGrant(actor); }
                finally { MclslPerformanceProbe.End("年度角色.自然赠予", grantSample); }
                break;
            case 3: MclslMaterialDiscovery.TryAnnualActivity(actor, year); break;
            case 4: MclslChildhoodRootSystem.TryProcessAgeFiveDeadline(actor, year); break;
            case 5:
                long careerSample = MclslPerformanceProbe.Begin();
                try
                {
                    // Family store first. If the frame budget runs out mid-supply,
                    // keep step 5 so the next operation recomputes from the actual
                    // bag; crafting then consumes materials and the market stage
                    // fills whatever gap remains.
                    if (!MclslFamilySystem.ProcessAnnualSupply(actor, year))
                    {
                        nextStage = MclslAnnualPipelineStage.Prepare;
                        nextStep = 5;
                        return true;
                    }
                    MclslProfessionSystem.ProcessAnnual(actor, year);
                    Combat.MclslCareerSupply.CraftAnnual(actor, year);
                    Combat.MclslCombatCareerSystem.ProcessAnnual(actor, year);
                }
                finally { MclslPerformanceProbe.End("年度角色.职业判定制作", careerSample); }
                break;
            case 6:
                MclslBagSystem.CompactMatureStacks(actor, year);
                MclslItemUseSystem.SyncPersistent(actor);
                break;
            case 7: MclslItemUseSystem.TryAutoCultivationConsumables(actor, year); break;
            case 8: MclslCultivationAgeSanity.RepairImpossibleYouthCultivation(actor, year); break;
            case 9: MclslMortalFateEventSystem.TryProcessAnnual(actor, year); break;
            case 10:
                if (MclslCultivationActorMarker.ShouldKeepTracked(actor)) MclslWorldActorQuery.Track(actor);
                nextStage = MclslAnnualPipelineStage.Progression;
                nextStep = 0;
                return true;
            default: throw new System.InvalidOperationException("Unknown annual prepare operation: " + step);
        }
        nextStep = NextPrepareStep(actor, year, nextStep);
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
            // Read at each committed operation, since the preceding one can
            // initialize cultivation, change law or break through a realm.
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
                    else MclslAncientLawSystem.ProcessAnnualFromScheduler(actor, year);
                    break;
                case 2: if (hasRealm) MclslSpellSystem.TryProgressAnnual(actor, year); break;
                case 3:
                    if (hasRealm && MclslArtifactSystem.EquippedArtifactId(actor, MclslArtifactEquipmentSlot.Amulet) == "B080")
                        actor.restoreHealthPercent(0.10f);
                    break;
                case 4: if (hasRealm) MclslTechniqueOccupationSystem.TryResolveConflictAnnual(actor, year); break;
                case 5: if (hasRealm) MclslAdventureSystem.RegisterAnnual(actor, year); break;
                case 6:
                    nextStage = MclslAnnualPipelineStage.Market;
                    nextStep = 0;
                    return hasRealm || MclslSensingQiSystem.IsSensing(actor);
                default: throw new System.InvalidOperationException("Unknown annual progression operation: " + step);
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
            MclslTianxuanMarket.ForgetPurchaseBatch(actor);
            nextStage = MclslAnnualPipelineStage.Finalize;
            nextStep = 0;
            return true;
        }
        if (step == 0)
        {
            long sample = MclslPerformanceProbe.Begin();
            MclslMarketPurchaseStep purchase;
            try { purchase = MclslTianxuanMarket.ProcessPurchaseBatch(actor, year); }
            finally { MclslPerformanceProbe.End("天玄镜.需求购买", sample); }
            nextStep = purchase == MclslMarketPurchaseStep.Pending ? 0 : 1;
            return true;
        }
        long sellSample = MclslPerformanceProbe.Begin();
        try { MclslTianxuanMarket.TryListSurplus(actor, year); }
        finally { MclslPerformanceProbe.End("天玄镜.多余出售", sellSample); }
        nextStage = MclslAnnualPipelineStage.Finalize;
        nextStep = 0;
        return true;
    }

    private static void ProcessFinalize(Actor actor)
    {
        if (!MclslActorAccessor.Alive(actor))
        {
            MclslCultivatorCandidateIndex.Remove(MclslActorAccessor.Id(actor));
            return;
        }
        MclslWorldActorQuery.Track(actor);
        MclslActorProjectionIndex.Update(actor);
    }
}
