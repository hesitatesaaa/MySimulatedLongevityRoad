using System;
using System.Collections.Generic;
using System.Diagnostics;
using MySimulatedLongevityRoad.Core;
using MySimulatedLongevityRoad.Data;
using MySimulatedLongevityRoad.Queries;

namespace MySimulatedLongevityRoad.Systems;

internal readonly struct MclslSchedulerContext
{
    internal readonly bool ProcessFast;
    internal readonly bool IsYearChange;
    internal readonly int CurrentYear;

    internal MclslSchedulerContext(bool processFast, bool isYearChange, int currentYear)
    {
        ProcessFast = processFast;
        IsYearChange = isYearChange;
        CurrentYear = Math.Max(0, currentYear);
    }
}

internal static class MclslScheduler
{
    private sealed class AnnualActorState
    {
        internal int ActiveYear;
        internal int LatestRequestedYear;
        internal int LastCompletedYear;
        internal MclslAnnualPipelineStage Stage;
        internal bool Queued;
    }

    private const int CleanupIntervalTicks = 128;
    private const int CleanupRemoveBudget = 64;
    private const int AnnualActorStageBudget = 96;
    private const double AnnualActorTimeBudgetMs = 0.65d;

    private static readonly Queue<long> AnnualActorQueue = new();
    private static readonly Dictionary<long, AnnualActorState> AnnualActorStates = new();
    private static readonly Dictionary<long, int> DeferredAnnualCompletions = new();
    private static readonly List<Actor> LineageActors = new();
    private static readonly HashSet<long> LineageActorIds = new();
    private static int _tickCounter;
    private static int _activeAnnualYear = -1;
    private static bool _newLawActive;

    internal static bool HasFastWork => MclslWorldBootstrapLane.HasPending
        || AnnualActorQueue.Count > 0
        || MclslAnnualWorldRuntimeLane.HasPending
        || MclslWorldEpochSystem.HasPendingTransitionWork;
    internal static bool HasAnnualActorBacklog => AnnualActorQueue.Count > 0;
    internal static bool HasUrgentSimulationBacklog => AnnualActorQueue.Count > 2048;
    internal static int AnnualActorBacklogCount => AnnualActorQueue.Count;
    internal static int AnnualActorStateCount => AnnualActorStates.Count;
    internal static bool AnnualWorldWorkPending => MclslAnnualWorldRuntimeLane.HasPending;

    internal static void InitializeAfterLoad(int currentYear)
    {
        AnnualActorQueue.Clear();
        AnnualActorStates.Clear();
        DeferredAnnualCompletions.Clear();
        LineageActors.Clear();
        LineageActorIds.Clear();
        MclslDetectionGate.ClearRuntimeState();
        MclslHotPathPolicy.Clear();
        MclslLongevityRules.ClearRuntimeCache();
        MclslAnnualExecutionContext.Clear();
        MclslCultivatorCandidateIndex.Clear();
        MclslWorldActorQuery.ClearCache();
        _activeAnnualYear = -1;
        _newLawActive = MclslWorldEpochSystem.IsNewLawActive(Math.Max(0, currentYear));
        _tickCounter = 0;
        MclslNewLawPioneerSystem.Clear();
        MclslWorldEpochSystem.ClearDeferredTransitionWork();
        MclslWorldEpochSystem.ResumeDeferredTransitionWork(currentYear);
        MclslTechniqueOccupationSystem.Rebuild();
        MclslWorldBootstrapLane.ScheduleAfterLoad();
    }

    internal static void RegisterAndEnqueueAnnualActor(Actor actor)
    {
        if (actor?.data == null || !MclslActorAccessor.Alive(actor)) return;

        MclslActorRegistry.Register(actor, out long actorId);
        if (actorId <= 0L) return;
        if (!MclslEligibility.CanCultivate(actor))
        {
            MclslCultivatorCandidateIndex.MarkNonCultivator(actorId);
            return;
        }

        int year = ResolveAnnualRequestYear();
        if (year <= 0 || MclslAnnualExecutionContext.IsExecuting(actor, year)) return;

        // 与玄鉴一致：已进入修炼索引的角色，年度回调只做轻量入队，
        // 不再重复执行灵根、特质与修炼身份全套校正。
        if (MclslCultivatorCandidateIndex.IsCultivator(actorId))
        {
            CommitBaseAnnualCultivationFromNativeAge(actor, year);
            EnqueueAnnualActorCore(actor, year);
            return;
        }

        bool childhoodWindow = MclslChildhoodRootSystem.ShouldTrackChildhoodCandidate(actor);
        bool hasExplicitState = MclslCultivationActorMarker.HasCultivationMarker(actor)
            || MclslActorAccessor.GetInt(actor, MclslActorDataKeys.ManualGrant, 0) > 0
            || MclslActorAccessor.HasCultivationPath(actor);
        if (!childhoodWindow && !hasExplicitState)
        {
            MclslCultivatorCandidateIndex.MarkNonCultivator(actorId);
            return;
        }

        if (childhoodWindow)
            MclslChildhoodRootSystem.TryProcessAgeFiveDeadline(actor, year);

        MclslCultivationWake.ReconcileCultivationIdentity(
            actor,
            ensureEntryFromGift: !childhoodWindow);
        MclslCultivatorCandidateIndex.Observe(actor);
        MclslWorldActorQuery.MarkDirty();
        if (ShouldQueueAnnualActor(actor, year))
        {
            CommitBaseAnnualCultivationFromNativeAge(actor, year);
            EnqueueAnnualActorCore(actor, year);
        }
    }

    /// <summary>
    /// 玄鉴式年度保底：Actor.updateAge 是已经验证可用的原生年度入口。
    /// 基础真元增长计算轻量且不可丢失，因此在该入口同步提交；感气推进、突破、
    /// 事件及世界结算仍留在有界队列。队列稍后再次进入 Progression 时会识别
    /// LastCultivationYear，继续后续链路而不会重复发放真元。
    /// </summary>
    private static void CommitBaseAnnualCultivationFromNativeAge(Actor actor, int year)
    {
        if (actor?.data == null || year <= 0 || !MclslEligibility.CanCultivate(actor)) return;
        if (!MclslCultivationActorMarker.IsAnnualCandidate(actor)) return;
        if (MclslActorAccessor.Realm(actor) == MclslRealmIds.ChangSheng) return;

        int lastYear = MclslActorAccessor.GetInt(actor, MclslActorDataKeys.LastCultivationYear, -1);
        lastYear = MclslCultivationSystem.NormalizeLastCultivationYear(actor, year, lastYear);
        if (lastYear >= year) return;

        try
        {
            if (string.IsNullOrWhiteSpace(MclslActorAccessor.Realm(actor))
                && MclslSpiritualRootSystem.HasCultivationPotential(actor))
            {
                MclslSpiritualRootEntrySystem.TryEnterFromGiftTrait(actor, year);
            }

            if (MclslAnnualCultivationExecutor.TryApplyOneAnnualStep(actor, year))
            {
                MclslCultivatorCandidateIndex.Observe(actor);
            }
        }
        catch (Exception ex)
        {
            // 同步保底失败时仍保留年度队列，由标准 Progression 再尝试一次。
            MclslDiagnostics.Error(
                "annual-growth-native:" + MclslActorAccessor.Id(actor),
                "原生年度入口提交基础真元失败，已转入年度队列重试: " + ex.Message);
        }
    }


    internal static void WakeAnnualCultivationActor(Actor actor)
    {
        if (actor?.data == null || !MclslActorAccessor.Alive(actor)) return;

        MclslActorRegistry.Register(actor, out long actorId);
        if (actorId <= 0L) return;
        if (!MclslEligibility.CanCultivate(actor))
        {
            MclslCultivatorCandidateIndex.MarkNonCultivator(actorId);
            return;
        }

        int year = ResolveAnnualRequestYear();
        if (year <= 0 || MclslAnnualExecutionContext.IsExecuting(actor, year)) return;

        MclslCultivatorCandidateIndex.Observe(actor);
        if (!MclslCultivationActorMarker.IsAnnualCandidate(actor)) return;

        int previousYear = Math.Max(0, year - 1);
        int persistedCompleted = MclslActorAccessor.GetInt(actor, MclslActorDataKeys.AnnualLastCompletedYear, 0);
        if (persistedCompleted > year)
            MclslActorAccessor.Set(actor, MclslActorDataKeys.AnnualLastCompletedYear, previousYear);

        int lastCultivationYear = MclslActorAccessor.GetInt(actor, MclslActorDataKeys.LastCultivationYear, 0);
        if (lastCultivationYear > year)
            MclslActorAccessor.Set(actor, MclslActorDataKeys.LastCultivationYear, previousYear);

        // 不再在唤醒路径旁路补发真元。缺失年度统一交给 Progression 单步结算。
        EnqueueAnnualActorCore(actor, year);
    }


    internal static void ScheduleAnnualWorld(int year)
    {
        if (year <= 0 || _activeAnnualYear == year || !MclslDetectionGate.TryBeginAnnualJob(MclslDetectionGate.AnnualWorldPreparation, year))
        {
            return;
        }

        _activeAnnualYear = year;
        _newLawActive = MclslWorldEpochSystem.IsNewLawActive(year);
        bool newLawCultivationAvailable = MclslNewLawPioneerSystem.CanPracticeNewLaw(year);
        MclslNewLawPioneerSystem.ProcessAnnual(year);
        LineageActors.Clear();
        LineageActorIds.Clear();
        MclslWorldActorQuery.BeginAnnualDetection(year);
        TickBootstrapLane();
        // Adventure candidates register during the actor annual pipeline, so the
        // adventure year must be initialized before actors are enqueued.
        MclslAdventureSystem.BeginAnnual(year);
        EnqueueKnownAnnualCandidates(year);
        MclslAnnualWorldRuntimeLane.Schedule(year, _newLawActive, newLawCultivationAvailable);
    }

    internal static void ProcessAll(in MclslSchedulerContext context)
    {
        if (!MclslRuntimeSettings.CoreEnabled) return;
        if (MclslWorldBootstrapLane.HasPending && (context.ProcessFast || context.IsYearChange))
        {
            TickBootstrapLane();
        }

        // 传法变世的旧法修士转化是昂贵工作，必须独立于年度回调分帧消化。
        // 即使年度角色队列为空，也要保留这条轻量车道，直到持久化队列清空。
        if (MclslWorldEpochSystem.HasPendingTransitionWork)
        {
            MclslWorldEpochSystem.TickDeferredTransitionWork();
        }

        // 队列一旦存在就必须被消费；DetectionGate 只负责决定何时创建年度工作，
        // 不能再作为已创建语义队列的第二道开关。
        if (AnnualActorQueue.Count > 0)
        {
            TickAnnualActors(context);
        }

        if (MclslAnnualWorldRuntimeLane.HasPending && MclslHotPathPolicy.ShouldRunBackgroundWorldStep())
        {
            TryResolveAnnualWorld();
        }

        TickCleanup();
    }

    internal static void Clear()
    {
        AnnualActorQueue.Clear();
        AnnualActorStates.Clear();
        DeferredAnnualCompletions.Clear();
        LineageActors.Clear();
        LineageActorIds.Clear();
        _tickCounter = 0;
        _activeAnnualYear = -1;
        _newLawActive = false;
        MclslWorldEpochSystem.ClearDeferredTransitionWork();
        MclslCultivatorCandidateIndex.Clear();
        MclslAnnualWorldRuntimeLane.Clear();
        MclslWorldBootstrapLane.Clear();
        MclslDetectionGate.ClearRuntimeState();
        MclslCultivationSystem.ClearRuntimeOnly();
        MclslAncientLawSystem.ClearRuntimeOnly();
        MclslWorldCaveSystem.Clear();
        MclslWorldChangeSystem.Clear();
        MclslAdventureSystem.Clear();
        MclslTechniqueOccupationSystem.Clear();
        MclslMortalFateEventSystem.Clear();
        MclslNewLawPioneerSystem.Clear();
        MclslHotPathPolicy.Clear();
        MclslLongevityRules.ClearRuntimeCache();
        MclslAnnualExecutionContext.Clear();
        MclslWorldActorQuery.ClearCache();
    }

    private static bool ShouldQueueAnnualActor(Actor actor, int year)
    {
        if (actor?.data == null || year <= 0) return false;
        if (!MclslEligibility.CanCultivate(actor)) return false;
        string realm = MclslActorAccessor.Realm(actor);
        if (!string.IsNullOrWhiteSpace(realm)) return true;
        if (MclslSpiritualRootSystem.HasCultivationPotential(actor)) return true;

        string system = MclslActorAccessor.GetString(actor, MclslActorDataKeys.CultivationSystem, string.Empty);
        if (!string.IsNullOrWhiteSpace(system))
        {
            return true;
        }

        bool newLaw = MclslWorldEpochSystem.IsNewLawActive(year);
        return newLaw
            ? MclslNewLawEntrySystem.ShouldAttemptMortalEntry(actor, year)
            : MclslAncientLawEntrySystem.ShouldAttemptMortalEntry(actor, year);
    }



    private static void EnqueueAnnualActorCore(Actor actor, int requestedYear)
    {
        long actorId = MclslActorAccessor.Id(actor);
        MclslDiagnostics.Cultivation(
            "scheduler.enqueue.enter",
            "actor=" + actorId
            + " requested=" + requestedYear
            + " annualStateCount=" + AnnualActorStates.Count
            + " queueCount=" + AnnualActorQueue.Count);
        if (actorId <= 0L || requestedYear <= 0)
        {
            MclslDiagnostics.Cultivation("scheduler.enqueue.skip", "actor=" + actorId + " requested=" + requestedYear + " reason=id-or-year");
            return;
        }

        RepairImpossibleZeroProgressCursor(actor, requestedYear);
        if (!AnnualActorStates.TryGetValue(actorId, out AnnualActorState state))
        {
            if (!TryReadPersistedPendingAnnualState(actor, out state))
            {
                int lastCompletedYear = ReadInitialLastCompletedYear(actor, requestedYear);
                if (requestedYear <= lastCompletedYear)
                {
                    MclslDiagnostics.Cultivation(
                        "scheduler.enqueue.skip",
                        "actor=" + actorId
                        + " requested=" + requestedYear
                        + " reason=requested<=lastCompleted"
                        + " lastCompleted=" + lastCompletedYear);
                    return;
                }
                int activeYear = ResolveNextAnnualActiveYear(lastCompletedYear, requestedYear);
                if (activeYear <= 0)
                {
                    MclslDiagnostics.Cultivation(
                        "scheduler.enqueue.skip",
                        "actor=" + actorId
                        + " requested=" + requestedYear
                        + " reason=activeYear<=0"
                        + " lastCompleted=" + lastCompletedYear);
                    return;
                }
                state = new AnnualActorState
                {
                    ActiveYear = activeYear,
                    LatestRequestedYear = requestedYear,
                    LastCompletedYear = Math.Max(0, lastCompletedYear),
                    Stage = MclslAnnualPipelineStage.Prepare,
                    Queued = false
                };
                MclslDiagnostics.Cultivation(
                    "scheduler.enqueue.new_state",
                    "actor=" + actorId
                    + " active=" + state.ActiveYear
                    + " latest=" + state.LatestRequestedYear
                    + " completed=" + state.LastCompletedYear
                    + " stage=" + state.Stage);
            }
            else
            {
                MclslDiagnostics.Cultivation(
                    "scheduler.enqueue.persisted_state",
                    "actor=" + actorId
                    + " active=" + state.ActiveYear
                    + " latest=" + state.LatestRequestedYear
                    + " completed=" + state.LastCompletedYear
                    + " stage=" + state.Stage);
            }
            state.LatestRequestedYear = Math.Max(state.LatestRequestedYear, requestedYear);
            AnnualActorStates[actorId] = state;
        }
        else
        {
            state.LatestRequestedYear = Math.Max(state.LatestRequestedYear, requestedYear);
            if (requestedYear <= state.LastCompletedYear)
            {
                MclslDiagnostics.Cultivation(
                    "scheduler.enqueue.skip",
                    "actor=" + actorId
                    + " requested=" + requestedYear
                    + " reason=requested<=state.completed"
                    + " completed=" + state.LastCompletedYear);
                return;
            }
        }

        if (state.ActiveYear <= 0 || state.ActiveYear <= state.LastCompletedYear)
        {
            state.ActiveYear = ResolveNextAnnualActiveYear(state.LastCompletedYear, state.LatestRequestedYear);
            state.Stage = MclslAnnualPipelineStage.Prepare;
        }
        if (state.ActiveYear <= 0)
        {
            MclslDiagnostics.Cultivation("scheduler.enqueue.skip", "actor=" + actorId + " requested=" + requestedYear + " reason=state.active<=0");
            return;
        }
        MclslDiagnostics.Cultivation(
            "scheduler.enqueue.queue",
            "actor=" + actorId
            + " active=" + state.ActiveYear
            + " latest=" + state.LatestRequestedYear
            + " completed=" + state.LastCompletedYear
            + " stage=" + state.Stage
            + " queued=" + state.Queued);
        QueueExistingState(actorId, state);
    }

    private static void EnqueueKnownAnnualCandidates(int year)
    {
        MclslCultivatorCandidateIndex.RefreshAnnualCandidatesFromKnownActors(
            MclslRuntimeWorkBudget.ScaleCount(512, 8));
        IReadOnlyList<long> candidateIds = MclslCultivatorCandidateIndex.GetAnnualCandidateIds();
        for (int i = 0; i < candidateIds.Count; i++)
        {
            long actorId = candidateIds[i];
            if (!MclslCultivatorCandidateIndex.Resolve(actorId, out Actor actor)) continue;
            if (!ShouldQueueAnnualActor(actor, year)) continue;
            EnqueueAnnualActorCore(actor, year);
        }
    }

    private static void TickBootstrapLane()
    {
        MclslWorldBootstrapLane.Tick(
            MclslRuntimeWorkBudget.ScaleCount(96, 16),
            MclslRuntimeWorkBudget.ScaleMilliseconds(0.65d, 0.20d));
    }

    private static void TickAnnualActors(in MclslSchedulerContext context)
    {
        int stageBudget = MclslRuntimeWorkBudget.ScaleCount(Math.Max(12, AnnualActorStageBudget), 8);
        double timeBudgetMs = MclslRuntimeWorkBudget.ScaleMilliseconds(AnnualActorTimeBudgetMs, 0.20d);
        long started = Stopwatch.GetTimestamp();
        int processed = 0;
        while (AnnualActorQueue.Count > 0 && processed < stageBudget)
        {
            long actorId = AnnualActorQueue.Dequeue();
            if (!AnnualActorStates.TryGetValue(actorId, out AnnualActorState state))
            {
                MclslDiagnostics.Cultivation("scheduler.tick.missing_state", "actor=" + actorId);
                continue;
            }
            state.Queued = false;
            processed++;
            MclslDiagnostics.Cultivation(
                "scheduler.tick.dequeue",
                "actor=" + actorId
                + " active=" + state.ActiveYear
                + " latest=" + state.LatestRequestedYear
                + " completed=" + state.LastCompletedYear
                + " stage=" + state.Stage
                + " queueRemaining=" + AnnualActorQueue.Count);

            if (!MclslActorRegistry.Resolve(actorId, out Actor actor)
                || actor?.data == null
                || !MclslActorAccessor.Alive(actor))
            {
                MclslDiagnostics.Cultivation("scheduler.tick.remove", "actor=" + actorId + " reason=missing-or-dead");
                AnnualActorStates.Remove(actorId);
                MclslCultivatorCandidateIndex.Remove(actorId);
            }
            else if (!MclslEligibility.CanCultivate(actor)
                || !MclslCultivationActorMarker.IsAnnualCandidate(actor))
            {
                // 存活但已失去修炼资格/修炼标记的角色只退出修炼队列与索引，
                // 仍保留在完整人口注册表中，避免世界人口和凡人查询被误删。
                CompletePersistedAnnualState(
                    actor,
                    Math.Max(state.LastCompletedYear, state.LatestRequestedYear));
                MclslDiagnostics.Cultivation("scheduler.tick.retire", "actor=" + actorId + " reason=ineligible-or-no-marker");
                AnnualActorStates.Remove(actorId);
                MclslCultivatorCandidateIndex.MarkNonCultivator(actorId);
            }
            else
            {
                int activeYear = state.ActiveYear > 0
                    ? state.ActiveYear
                    : ResolveActiveYear(state, context.CurrentYear);
                state.ActiveYear = activeYear;

                bool hasNextStage;
                MclslAnnualPipelineStage nextStage;
                try
                {
                    MclslDiagnostics.Cultivation(
                        "scheduler.tick.stage_start",
                        "actor=" + actorId
                        + " active=" + activeYear
                        + " stage=" + state.Stage);
                    hasNextStage = MclslAnnualActorPipeline.ProcessStage(
                        actor,
                        activeYear,
                        state.Stage,
                        out nextStage);
                    MclslDiagnostics.Cultivation(
                        "scheduler.tick.stage_done",
                        "actor=" + actorId
                        + " active=" + activeYear
                        + " stage=" + state.Stage
                        + " hasNext=" + hasNextStage
                        + " next=" + nextStage
                        + " essence=" + MclslCultivationGrowthSystem.CurrentTrueEssence(actor)
                        + " lastCultYear=" + MclslActorAccessor.GetInt(actor, MclslActorDataKeys.LastCultivationYear, -999));
                }
                catch (Exception ex)
                {
                    // 可选维护、事件或突破逻辑不能让角色永久脱离年度队列。
                    // 基础真元增长位于 Progression 最前方；发生异常后推进到
                    // 下一阶段，下一年仍会继续正常修炼。
                    MclslDiagnostics.Error(
                        "annual-actor:" + actorId + ":" + state.Stage,
                        "角色年度结算异常，已跳过当前附加阶段: " + ex.Message);
                    hasNextStage = state.Stage != MclslAnnualPipelineStage.Finalize;
                    nextStage = state.Stage switch
                    {
                        MclslAnnualPipelineStage.Prepare => MclslAnnualPipelineStage.Progression,
                        MclslAnnualPipelineStage.Progression => MclslAnnualPipelineStage.Finalize,
                        _ => MclslAnnualPipelineStage.Finalize
                    };
                }

                if (MclslActorAccessor.Alive(actor)
                    && !string.IsNullOrWhiteSpace(MclslActorAccessor.Realm(actor)))
                {
                    AddLineageActor(actor);
                }

                if (hasNextStage)
                {
                    state.Stage = nextStage;
                    QueueExistingState(actorId, state);
                }
                else
                {
                    if (ShouldDeferIncompleteProgression(actor, state.Stage, activeYear))
                    {
                        state.LastCompletedYear = Math.Max(state.LastCompletedYear, Math.Max(0, activeYear - 1));
                        CompletePersistedAnnualState(actor, state.LastCompletedYear);
                        AnnualActorStates.Remove(actorId);
                        continue;
                    }

                    state.LastCompletedYear = Math.Max(state.LastCompletedYear, activeYear);
                    if (state.LatestRequestedYear > state.LastCompletedYear)
                    {
                        state.ActiveYear = ResolveNextAnnualActiveYear(
                            state.LastCompletedYear,
                            state.LatestRequestedYear);
                        state.Stage = MclslAnnualPipelineStage.Prepare;
                        QueueExistingState(actorId, state);
                    }
                    else
                    {
                        CompletePersistedAnnualState(actor, state.LastCompletedYear);
                        AnnualActorStates.Remove(actorId);
                    }
                }
            }

            if (HasExceededTimeBudget(started, timeBudgetMs))
            {
                break;
            }
        }
    }

    private static bool ShouldDeferIncompleteProgression(Actor actor, MclslAnnualPipelineStage stage, int activeYear)
    {
        if (stage != MclslAnnualPipelineStage.Progression
            && stage != MclslAnnualPipelineStage.Finalize) return false;
        if (actor?.data == null || activeYear <= 0) return false;
        if (!MclslEligibility.CanCultivate(actor)) return false;
        if (MclslActorAccessor.Realm(actor) == MclslRealmIds.ChangSheng) return false;
        if (!MclslSpiritualRootSystem.HasCultivationPotential(actor)) return false;
        int lastCultivationYear = MclslActorAccessor.GetInt(actor, MclslActorDataKeys.LastCultivationYear, -1);
        return MclslCultivationSystem.NormalizeLastCultivationYear(actor, activeYear, lastCultivationYear) < activeYear;
    }

    private static int ResolveActiveYear(AnnualActorState state, int fallbackYear)
    {
        int latest = Math.Max(0, state.LatestRequestedYear);
        int completed = Math.Max(0, state.LastCompletedYear);
        if (latest <= completed) return Math.Max(1, fallbackYear);
        return completed + 1;
    }

    private static void QueueExistingState(long actorId, AnnualActorState state)
    {
        if (state == null || state.Queued) return;
        state.Queued = true;
        AnnualActorQueue.Enqueue(actorId);
    }

    private static bool TryReadPersistedPendingAnnualState(Actor actor, out AnnualActorState state)
    {
        state = null;
        if (actor?.data == null) return false;

        int activeYear = MclslActorAccessor.GetInt(actor, MclslActorDataKeys.AnnualActiveYear, 0);
        int latestYear = MclslActorAccessor.GetInt(actor, MclslActorDataKeys.AnnualLatestRequestedYear, 0);
        int stageValue = MclslActorAccessor.GetInt(actor, MclslActorDataKeys.AnnualStage, -1);
        int lastCompletedYear = ReadEffectiveLastCompletedYear(actor);
        if (activeYear <= 0
            || latestYear < activeYear
            || activeYear <= lastCompletedYear
            || stageValue < (int)MclslAnnualPipelineStage.Prepare
            || stageValue > (int)MclslAnnualPipelineStage.Finalize)
        {
            return false;
        }

        int normalizedActiveYear = stageValue == (int)MclslAnnualPipelineStage.Prepare
            ? ResolveNextAnnualActiveYear(lastCompletedYear, latestYear)
            : activeYear;
        if (normalizedActiveYear <= 0) return false;

        state = new AnnualActorState
        {
            ActiveYear = normalizedActiveYear,
            LatestRequestedYear = latestYear,
            LastCompletedYear = Math.Max(0, lastCompletedYear),
            Stage = (MclslAnnualPipelineStage)stageValue,
            Queued = false
        };
        return true;
    }

    /// <summary>
    /// 0.1.4/0.1.5 的失败链可能写入“年度已完成”或“本年已修炼”，却没有任何真元。
    /// 对已经开始感气、跨过至少一个世界年且仍为 0 真元的角色，这组游标不可能
    /// 代表真实完成状态，必须回退到上一年，让 Progression 重新执行。
    /// </summary>
    private static void RepairImpossibleZeroProgressCursor(Actor actor, int requestedYear)
    {
        if (actor?.data == null || requestedYear <= 0) return;
        if (!MclslSpiritualRootSystem.HasCultivationPotential(actor)) return;
        if (MclslCultivationGrowthSystem.CurrentTrueEssence(actor) > 0) return;
        if (MclslActorAccessor.Realm(actor) == MclslRealmIds.ChangSheng) return;

        int startYear = MclslActorAccessor.GetInt(actor, MclslActorDataKeys.CultivationStartYear, 0);
        if (startYear <= 0 || startYear >= requestedYear) return;

        int previousYear = Math.Max(0, requestedYear - 1);
        int lastCultivationYear = MclslActorAccessor.GetInt(actor, MclslActorDataKeys.LastCultivationYear, 0);
        int lastCompletedYear = MclslActorAccessor.GetInt(actor, MclslActorDataKeys.AnnualLastCompletedYear, 0);
        if (lastCultivationYear < requestedYear && lastCompletedYear < requestedYear) return;

        MclslActorAccessor.Set(actor, MclslActorDataKeys.LastCultivationYear, previousYear);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.AnnualLastCompletedYear, previousYear);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.AnnualActiveYear, 0);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.AnnualLatestRequestedYear, 0);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.AnnualStage, (int)MclslAnnualPipelineStage.Prepare);
    }

    private static int ReadInitialLastCompletedYear(Actor actor, int requestedYear)
    {
        if (actor?.data == null) return 0;

        int requested = Math.Max(0, requestedYear);
        int completed = Math.Min(ReadEffectiveLastCompletedYear(actor), requested);
        bool expectsAnnualGrowth =
            MclslActorAccessor.Realm(actor) != MclslRealmIds.ChangSheng
            && MclslCultivationActorMarker.HasCultivationMarker(actor);

        if (!expectsAnnualGrowth) return Math.Max(0, completed);

        int cultivationYear = MclslActorAccessor.GetInt(
            actor,
            MclslActorDataKeys.LastCultivationYear,
            0);

        if (cultivationYear > 0)
        {
            cultivationYear = Math.Min(cultivationYear, requested);

            // 旧版本可能只推进“年度完成”而没有真正写入真元。
            // 只补最近十二个缺失年份，既修复卡死角色，也避免高年份旧档
            // 从出生年开始形成巨大补算队列。
            if (cultivationYear < completed)
                completed = Math.Max(cultivationYear, Math.Max(0, requested - 12));
            else if (cultivationYear >= requested && completed < requested)
            {
                // updateAge 保底已经提交了本年基础真元，但感气、突破和事件阶段
                // 仍需由年度队列完成，不能把 LastCultivationYear 误当整条流水线完成。
                completed = Math.Max(0, requested - 1);
            }
            else if (completed <= 0)
                completed = cultivationYear;
        }
        else
        {
            // 有修炼标记却没有真实修炼年份时，年度完成游标不可信。
            // 至少回退到上一年，让年度流水线重新尝试一次基础真元写入。
            int previousYear = Math.Max(0, requested - 1);
            int startYear = MclslActorAccessor.GetInt(
                actor,
                MclslActorDataKeys.CultivationStartYear,
                0);
            if (completed <= 0)
            {
                completed = startYear > 0
                    ? Math.Max(0, Math.Min(previousYear, startYear - 1))
                    : previousYear;
            }
            else
            {
                completed = Math.Min(completed, previousYear);
            }
        }

        return Math.Max(0, Math.Min(completed, requested));
    }

    private static int ReadEffectiveLastCompletedYear(Actor actor)
    {
        if (actor?.data == null) return 0;
        int persistedYear = MclslActorAccessor.GetInt(actor, MclslActorDataKeys.AnnualLastCompletedYear, 0);
        long actorId = MclslActorAccessor.Id(actor);
        if (actorId > 0L && DeferredAnnualCompletions.TryGetValue(actorId, out int deferredYear))
        {
            persistedYear = Math.Max(persistedYear, deferredYear);
        }
        return ClampFutureCompletedYear(actor, persistedYear);
    }

    private static int ClampFutureCompletedYear(Actor actor, int completedYear)
    {
        int normalizedYear = Math.Max(0, completedYear);
        int currentYear = MclslRuntime.CurrentYear();
        if (currentYear <= 0 || normalizedYear <= currentYear) return normalizedYear;

        int repairedYear = Math.Max(0, currentYear - 1);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.AnnualLastCompletedYear, repairedYear);
        return repairedYear;
    }

    private static int ResolveNextAnnualActiveYear(int lastCompletedYear, int latestRequestedYear)
    {
        int completed = Math.Max(0, lastCompletedYear);
        int requested = Math.Max(0, latestRequestedYear);
        if (requested <= completed) return 0;
        return completed + 1;
    }

    private static void PersistAnnualState(Actor actor, AnnualActorState state)
    {
        if (actor?.data == null || state == null) return;
        int lastCompletedYear = state.LastCompletedYear;
        long actorId = MclslActorAccessor.Id(actor);
        if (actorId > 0L && DeferredAnnualCompletions.TryGetValue(actorId, out int deferredYear))
        {
            lastCompletedYear = Math.Max(lastCompletedYear, deferredYear);
        }

        MclslActorAccessor.Set(actor, MclslActorDataKeys.AnnualActiveYear, Math.Max(0, state.ActiveYear));
        MclslActorAccessor.Set(actor, MclslActorDataKeys.AnnualLatestRequestedYear, Math.Max(0, state.LatestRequestedYear));
        MclslActorAccessor.Set(actor, MclslActorDataKeys.AnnualStage, (int)state.Stage);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.AnnualLastCompletedYear, Math.Max(0, lastCompletedYear));
    }

    private static void CompletePersistedAnnualState(Actor actor, int completedYear)
    {
        if (actor?.data == null) return;
        long actorId = MclslActorAccessor.Id(actor);
        int normalizedYear = Math.Max(0, completedYear);
        if (actorId <= 0L || normalizedYear <= 0) return;

        int persistedYear = MclslActorAccessor.GetInt(actor, MclslActorDataKeys.AnnualLastCompletedYear, 0);
        if (normalizedYear <= persistedYear && !DeferredAnnualCompletions.ContainsKey(actorId)) return;
        if (!DeferredAnnualCompletions.TryGetValue(actorId, out int previousYear) || normalizedYear > previousYear)
        {
            DeferredAnnualCompletions[actorId] = normalizedYear;
        }
    }

    internal static void FlushPendingAnnualStatesForSave()
    {
        foreach (KeyValuePair<long, int> pair in DeferredAnnualCompletions)
        {
            if (MclslCultivatorCandidateIndex.Resolve(pair.Key, out Actor actor) && actor?.data != null)
            {
                MclslActorAccessor.Set(actor, MclslActorDataKeys.AnnualLastCompletedYear, pair.Value);
            }
        }

        foreach (KeyValuePair<long, AnnualActorState> pair in AnnualActorStates)
        {
            if (pair.Value != null && MclslCultivatorCandidateIndex.Resolve(pair.Key, out Actor actor))
            {
                PersistAnnualState(actor, pair.Value);
            }
        }
        DeferredAnnualCompletions.Clear();
    }

    private static void TryResolveAnnualWorld()
    {
        if (_activeAnnualYear <= 0) return;
        if (MclslWorldBootstrapLane.HasPending) return;
        if (AnnualActorQueue.Count > 0 || AnnualActorStates.Count > 0) return;

        bool completed = MclslAnnualWorldRuntimeLane.Tick(LineageActors);
        if (completed)
        {
            MclslWorldActorQuery.EndAnnualDetection(_activeAnnualYear);
            LineageActors.Clear();
            LineageActorIds.Clear();
        }
    }

    private static void AddLineageActor(Actor actor)
    {
        long actorId = MclslActorAccessor.Id(actor);
        if (actorId <= 0L || !LineageActorIds.Add(actorId)) return;
        LineageActors.Add(actor);
    }

    private static void TickCleanup()
    {
        _tickCounter++;
        if (!MclslDetectionGate.ShouldRunCadence(MclslDetectionGate.RuntimeCandidateCleanup, _tickCounter, CleanupIntervalTicks))
        {
            return;
        }
        _tickCounter = 0;
        if (MclslCultivatorCandidateIndex.CleanupInvalid(CleanupRemoveBudget) > 0)
            MclslWorldActorQuery.MarkDirty();
    }

    private static int ResolveAnnualRequestYear()
    {
        int worldYear = MclslRuntime.CurrentYear();
        if (worldYear > 0) return worldYear;
        return 1;
    }

    private static bool HasExceededTimeBudget(long startedTimestamp, double budgetMilliseconds)
    {
        long elapsedTicks = Stopwatch.GetTimestamp() - startedTimestamp;
        return elapsedTicks > budgetMilliseconds * Stopwatch.Frequency / 1000.0;
    }
}
