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
        internal int Step;
        internal bool Queued;
        internal int DueFrame;
    }

    private const int CleanupIntervalTicks = 128;
    private const int CleanupRemoveBudget = 64;
    private const double AnnualActorTimeBudgetMs = 0.65d;
    private const int AnnualCandidateRefreshBudget = 16;
    private const int AnnualCandidateEnqueueBudget = 16;
    private const double AnnualCandidateTimeBudgetMs = 0.15d;

    private static readonly Queue<long> AnnualActorQueue = new();
    private static readonly Dictionary<long, AnnualActorState> AnnualActorStates = new();
    private static readonly Dictionary<long, int> DeferredAnnualCompletions = new();
    private static readonly List<Actor> LineageActors = new();
    private static readonly HashSet<long> LineageActorIds = new();
    private static readonly HashSet<long> PersistedLineageIds = new();
    private static int _tickCounter;
    private static int _activeAnnualYear = -1;
    private static bool _newLawActive;
    private static int _worldLaneScheduleFailures;
    private static int _worldLaneRetryAfterFrame;
    private static bool _annualCandidateRefreshPending;
    private static bool _annualCandidateScanPending;
    private static IReadOnlyList<long> _annualCandidateScanIds = Array.Empty<long>();
    private static int _annualCandidateScanCursor;
    private static long _annualCycleStarted;

    internal static bool HasFastWork => MclslWorldBootstrapLane.HasPending
        || AnnualActorQueue.Count > 0
        || _annualCandidateRefreshPending
        || _annualCandidateScanPending
        || MclslAnnualWorldRuntimeLane.HasPending
        || MclslWorldEpochSystem.HasPendingTransitionWork;
    internal static bool HasAnnualActorBacklog => AnnualActorQueue.Count > 0;
    internal static bool HasAnnualCandidateBacklog => _annualCandidateRefreshPending || _annualCandidateScanPending;
    private static int AnnualUrgentBacklogThreshold => Math.Min(2048,
        Math.Max(256, MclslCultivatorCandidateIndex.CultivatorCount / 2));
    internal static bool HasUrgentSimulationBacklog => AnnualActorQueue.Count > AnnualUrgentBacklogThreshold;
    internal static int AnnualActorBacklogCount => AnnualActorQueue.Count;
    internal static int AnnualActorStateCount => AnnualActorStates.Count;
    internal static bool AnnualWorldWorkPending => MclslAnnualWorldRuntimeLane.HasPending;
    internal static int AnnualCandidateScanRemaining => _annualCandidateRefreshPending
        ? -1
        : _annualCandidateScanPending
            ? Math.Max(0, _annualCandidateScanIds.Count - _annualCandidateScanCursor)
            : 0;

    internal static void GetAnnualYearSpan(out int latestRequestedYear, out int oldestPendingYear)
    {
        latestRequestedYear = 0;
        oldestPendingYear = 0;
        foreach (AnnualActorState state in AnnualActorStates.Values)
        {
            if (state == null || state.ActiveYear <= 0) continue;
            latestRequestedYear = Math.Max(latestRequestedYear, state.LatestRequestedYear);
            if (oldestPendingYear == 0 || state.ActiveYear < oldestPendingYear)
                oldestPendingYear = state.ActiveYear;
        }
        MclslAnnualBatchState batch = MclslWorldRunRepository.Current.AnnualBatch;
        latestRequestedYear = Math.Max(latestRequestedYear, batch.LatestRequestedYear);
        if (batch.ActiveYear > 0 && (oldestPendingYear == 0 || batch.ActiveYear < oldestPendingYear))
            oldestPendingYear = batch.ActiveYear;
    }

    internal static void InitializeAfterLoad(int currentYear)
    {
        AnnualActorQueue.Clear();
        AnnualActorStates.Clear();
        DeferredAnnualCompletions.Clear();
        LineageActors.Clear();
        LineageActorIds.Clear();
        PersistedLineageIds.Clear();
        MclslDetectionGate.ClearRuntimeState();
        MclslStaggeredWorkPolicy.Clear();
        MclslHotPathPolicy.Clear();
        MclslLongevityRules.ClearRuntimeCache();
        MclslAnnualExecutionContext.Clear();
        MclslCultivatorCandidateIndex.Clear();
        MclslWorldActorQuery.ClearCache();
        _activeAnnualYear = -1;
        _annualCandidateRefreshPending = false;
        _annualCandidateScanPending = false;
        _annualCandidateScanIds = Array.Empty<long>();
        _annualCandidateScanCursor = 0;
        _annualCycleStarted = 0L;
        _newLawActive = MclslWorldEpochSystem.IsNewLawActive(Math.Max(0, currentYear));
        _tickCounter = 0;
        MclslAnnualWorldRuntimeLane.Clear();
        MclslWorldCaveSystem.Clear();
        MclslWorldChangeSystem.Clear();
        MclslAdventureSystem.Clear();
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
            EnqueueAnnualActorCore(actor, year);
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
        if (year <= 0) return;
        MclslAnnualBatchState batch = MclslWorldRunRepository.Current.AnnualBatch;
        if (year > batch.LatestRequestedYear)
        {
            batch.LatestRequestedYear = year;
            MclslWorldArchiveStore.MarkDirty();
        }
        if (_activeAnnualYear > 0)
        {
            EnsureWorldLaneScheduled();
            return;
        }
        int startYear = batch.ActiveYear > 0 ? batch.ActiveYear
            : batch.LastCompletedYear > 0 ? batch.LastCompletedYear + 1 : year;
        if (startYear > batch.LatestRequestedYear) return;
        StartAnnualWorld(startYear);
    }

    private static void StartAnnualWorld(int year)
    {
        MclslAnnualBatchState batch = MclslWorldRunRepository.Current.AnnualBatch;
        bool resuming = batch.ActiveYear == year && batch.EraSnapshotRecorded;
        _activeAnnualYear = year;
        batch.ActiveYear = year;
        _annualCycleStarted = MclslPerformanceProbe.Begin();
        RunAnnualStartStep(year, "MarketPrune", MclslTianxuanMarket.PruneDeadSellers);
        _newLawActive = resuming ? batch.NewLawEraActive : RunAnnualStartQuery(year,
            "NewLawEra", () => MclslWorldEpochSystem.IsNewLawActive(year));
        bool newLawCultivationAvailable = resuming ? batch.NewLawCultivationAvailable : RunAnnualStartQuery(year,
            "NewLawCultivation", () => MclslNewLawPioneerSystem.CanPracticeNewLaw(year));
        batch.NewLawEraActive = _newLawActive;
        batch.NewLawCultivationAvailable = newLawCultivationAvailable;
        batch.EraSnapshotRecorded = true;
        RunAnnualStartStep(year, "NewLawPioneer", () => MclslNewLawPioneerSystem.ProcessAnnual(year));
        LineageActors.Clear();
        LineageActorIds.Clear();
        PersistedLineageIds.Clear();
        foreach (long actorId in batch.LineageActorIds) PersistedLineageIds.Add(actorId);
        RunAnnualStartStep(year, "ActorDetection", () => MclslWorldActorQuery.BeginAnnualDetection(year));
        RunAnnualStartStep(year, "Bootstrap", TickBootstrapLane);
        // Adventure candidates register during the actor annual pipeline, so the
        // adventure year must be initialized before actors are enqueued.
        RunAnnualStartStep(year, "AdventureBegin", () => MclslAdventureSystem.BeginAnnual(year));
        RunAnnualStartStep(year, "CandidateScan", BeginAnnualCandidateScan);
        EnsureWorldLaneScheduled();
        MclslWorldArchiveStore.MarkDirty();
    }

    private static void EnsureWorldLaneScheduled()
    {
        if (_activeAnnualYear <= 0 || MclslAnnualWorldRuntimeLane.HasPending
            || UnityEngine.Time.frameCount < _worldLaneRetryAfterFrame) return;
        int year = _activeAnnualYear;
        try
        {
            MclslAnnualWorldRuntimeLane.Schedule(year, _newLawActive,
                MclslWorldRunRepository.Current.AnnualBatch.NewLawCultivationAvailable);
            _worldLaneScheduleFailures = 0;
            _worldLaneRetryAfterFrame = 0;
        }
        catch (Exception ex)
        {
            _worldLaneScheduleFailures++;
            bool retry = MclslAnnualResiliencePolicy.ShouldRetry(_worldLaneScheduleFailures, false);
            MclslWorldRunRepository.RecordAnnualFailure(year, "年度启动", string.Empty,
                "WorldLaneSchedule", _worldLaneScheduleFailures, retry ? "待重试" : "未结算", ex.Message);
            if (retry)
            {
                _worldLaneRetryAfterFrame = MclslAnnualResiliencePolicy.RetryAtFrame(UnityEngine.Time.frameCount);
                return;
            }
            MclslAnnualBatchState batch = MclslWorldRunRepository.Current.AnnualBatch;
            RunAnnualStartStep(year, "ActorDetectionEnd", () => MclslWorldActorQuery.EndAnnualDetection(year));
            batch.LastCompletedYear = Math.Max(batch.LastCompletedYear, year);
            batch.ActiveYear = 0;
            batch.WorldStage = 0;
            batch.EraSnapshotRecorded = false;
            batch.LineageActorIds.Clear();
            _activeAnnualYear = -1;
            _worldLaneScheduleFailures = 0;
            _worldLaneRetryAfterFrame = 0;
            MclslAnnualWorldRuntimeLane.Clear();
            MclslWorldArchiveStore.MarkDirty();
            if (batch.LatestRequestedYear > year) StartAnnualWorld(year + 1);
        }
    }

    private static void RunAnnualStartStep(int year, string step, Action action)
    {
        try { action(); }
        catch (Exception ex)
        {
            MclslWorldRunRepository.RecordAnnualFailure(year, "年度启动", string.Empty,
                step, 1, "部分失败", ex.Message);
        }
    }

    private static bool RunAnnualStartQuery(int year, string step, Func<bool> query)
    {
        try { return query(); }
        catch (Exception ex)
        {
            MclslWorldRunRepository.RecordAnnualFailure(year, "年度启动", string.Empty,
                step, 1, "跳过", ex.Message);
            return false;
        }
    }

    internal static void ProcessAll(in MclslSchedulerContext context)
    {
        if (!MclslRuntimeSettings.CoreEnabled) return;
        if (_activeAnnualYear > 0 && !MclslAnnualWorldRuntimeLane.HasPending)
            EnsureWorldLaneScheduled();
        if (MclslWorldBootstrapLane.HasPending && (context.ProcessFast || context.IsYearChange))
        {
            TickBootstrapLane();
        }

        if (_annualCandidateRefreshPending || _annualCandidateScanPending)
        {
            if (MclslStaggeredWorkPolicy.TryBegin(MclslWorkCategory.Maintenance, out long maintenanceStarted))
            {
                long candidateSample = MclslPerformanceProbe.Begin();
                try
                {
                    using (MclslUnityProfiler.Sample("MCLS/Annual/CandidateScan"))
                        TickAnnualCandidateScan();
                }
                finally
                {
                    MclslPerformanceProbe.End("年度候选扫描", candidateSample);
                    MclslStaggeredWorkPolicy.End(MclslWorkCategory.Maintenance, maintenanceStarted);
                }
            }
        }

        // 传法变世的旧法修士转化是昂贵工作，必须独立于年度回调分帧消化。
        // 即使年度角色队列为空，也要保留这条轻量车道，直到持久化队列清空。
        long transitionSample = MclslPerformanceProbe.Begin();
        if (MclslWorldEpochSystem.HasPendingTransitionWork)
        {
            using (MclslUnityProfiler.Sample("MCLS/NewLaw/DeferredTransition"))
                MclslWorldEpochSystem.TickDeferredTransitionWork();
        }
        MclslPerformanceProbe.End("新法转化", transitionSample);

        // 队列一旦存在就必须被消费；DetectionGate 只负责决定何时创建年度工作，
        // 不能再作为已创建语义队列的第二道开关。
        long actorSample = MclslPerformanceProbe.Begin();
        if (AnnualActorQueue.Count > 0)
        {
            TickAnnualActors(context);
        }
        MclslPerformanceProbe.End("年度角色", actorSample);

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
        PersistedLineageIds.Clear();
        _tickCounter = 0;
        _activeAnnualYear = -1;
        _newLawActive = false;
        _worldLaneScheduleFailures = 0;
        _worldLaneRetryAfterFrame = 0;
        _annualCandidateRefreshPending = false;
        _annualCandidateScanPending = false;
        _annualCandidateScanIds = Array.Empty<long>();
        _annualCandidateScanCursor = 0;
        _annualCycleStarted = 0L;
        MclslWorldEpochSystem.ClearDeferredTransitionWork();
        MclslCultivatorCandidateIndex.Clear();
        MclslAnnualWorldRuntimeLane.Clear();
        MclslWorldBootstrapLane.Clear();
        MclslDetectionGate.ClearRuntimeState();
        MclslStaggeredWorkPolicy.Clear();
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
            state.Step = 0;
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

    private static void BeginAnnualCandidateScan()
    {
        _annualCandidateRefreshPending = true;
        _annualCandidateScanPending = false;
        _annualCandidateScanIds = Array.Empty<long>();
        _annualCandidateScanCursor = 0;
        MclslCultivatorCandidateIndex.BeginAnnualCandidateRefresh();
    }

    private static void TickAnnualCandidateScan()
    {
        long started = Stopwatch.GetTimestamp();
        double timeBudgetMs = MclslRuntimeWorkBudget.ScaleMilliseconds(AnnualCandidateTimeBudgetMs, 0.10d);
        if (_annualCandidateRefreshPending)
        {
            bool complete;
            using (MclslUnityProfiler.Sample("MCLS/Annual/CandidateIndexRefresh"))
            {
                complete = MclslCultivatorCandidateIndex.RefreshAnnualCandidatesFromKnownActors(
                    MclslRuntimeWorkBudget.ScaleCount(AnnualCandidateRefreshBudget, 16));
            }
            if (!complete)
            {
                MclslPerformanceProbe.End("年度候选.索引刷新", started);
                return;
            }

            _annualCandidateRefreshPending = false;
            _annualCandidateScanIds = MclslCultivatorCandidateIndex.GetAnnualCandidateIds();
            _annualCandidateScanCursor = 0;
            _annualCandidateScanPending = _annualCandidateScanIds.Count > 0;
            if (!_annualCandidateScanPending)
            {
                MclslPerformanceProbe.End("年度候选.索引刷新", started);
                return;
            }
        }

        int processed = 0;
        int budget = MclslRuntimeWorkBudget.ScaleCount(AnnualCandidateEnqueueBudget, 16);
        using (MclslUnityProfiler.Sample("MCLS/Annual/CandidateEnqueue"))
        {
            while (_annualCandidateScanCursor < _annualCandidateScanIds.Count && processed < budget)
            {
                long actorId = _annualCandidateScanIds[_annualCandidateScanCursor++];
                processed++;
                if (MclslCultivatorCandidateIndex.Resolve(actorId, out Actor actor)
                    && ShouldQueueAnnualActor(actor, _activeAnnualYear))
                {
                    EnqueueAnnualActorCore(actor, _activeAnnualYear);
                }

                if ((processed & 15) == 0
                    && Stopwatch.GetTimestamp() - started > timeBudgetMs * Stopwatch.Frequency / 1000d)
                    break;
            }
        }

        if (_annualCandidateScanCursor >= _annualCandidateScanIds.Count)
        {
            _annualCandidateScanPending = false;
            _annualCandidateScanIds = Array.Empty<long>();
            _annualCandidateScanCursor = 0;
        }
        MclslPerformanceProbe.End("年度候选.入队", started);
    }

    private static void TickBootstrapLane()
    {
        MclslWorldBootstrapLane.Tick(
            MclslRuntimeWorkBudget.ScaleCount(96, 16),
            MclslRuntimeWorkBudget.ScaleMilliseconds(0.65d, 0.20d));
    }

    private static void TickAnnualActors(in MclslSchedulerContext context)
    {
        int stageBudget = MclslRuntimeWorkBudget.ScaleCount(MclslRuntimeSettings.AnnualActorBudget, 8);
        double timeBudgetMs = MclslRuntimeWorkBudget.ScaleMilliseconds(AnnualActorTimeBudgetMs, 0.20d);
        long started = Stopwatch.GetTimestamp();
        int processed = 0;
        int examined = 0;
        int examineBudget = Math.Min(AnnualActorQueue.Count, Math.Max(64, stageBudget * 4));
        while (AnnualActorQueue.Count > 0 && processed < stageBudget && examined < examineBudget)
        {
            long actorId = AnnualActorQueue.Dequeue();
            examined++;
            if (!AnnualActorStates.TryGetValue(actorId, out AnnualActorState state))
            {
                MclslDiagnostics.Cultivation("scheduler.tick.missing_state", "actor=" + actorId);
                continue;
            }
            state.Queued = false;
            if (state.DueFrame > UnityEngine.Time.frameCount)
            {
                RequeuePreservingDueFrame(actorId, state);
                continue;
            }
            if (_activeAnnualYear > 0 && state.ActiveYear > _activeAnnualYear)
            {
                RequeuePreservingDueFrame(actorId, state);
                continue;
            }

            MclslWorkCategory category = MclslStaggeredWorkPolicy.Classify(state.Stage, state.Step);
            if (!MclslStaggeredWorkPolicy.TryBegin(category, out long categoryStarted))
            {
                RequeuePreservingDueFrame(actorId, state);
                if (MclslStaggeredWorkPolicy.GlobalBudgetExhausted) break;
                continue;
            }
            processed++;
            MclslDiagnostics.Cultivation(
                "scheduler.tick.dequeue",
                "actor=" + actorId
                + " active=" + state.ActiveYear
                + " latest=" + state.LatestRequestedYear
                + " completed=" + state.LastCompletedYear
                + " stage=" + state.Stage
                + " queueRemaining=" + AnnualActorQueue.Count);

            try
            {
                if (!MclslActorRegistry.Resolve(actorId, out Actor actor)
                    || actor?.data == null
                    || !MclslActorAccessor.Alive(actor))
                {
                    MclslDiagnostics.Cultivation("scheduler.tick.remove", "actor=" + actorId + " reason=missing-or-dead");
                    AnnualActorStates.Remove(actorId);
                    MclslCultivatorCandidateIndex.Remove(actorId);
                    continue;
                }
                if (!MclslEligibility.CanCultivate(actor)
                    || !MclslCultivationActorMarker.IsAnnualCandidate(actor))
                {
                    CompletePersistedAnnualState(
                        actor,
                        Math.Max(state.LastCompletedYear, state.LatestRequestedYear));
                    MclslDiagnostics.Cultivation("scheduler.tick.retire", "actor=" + actorId + " reason=ineligible-or-no-marker");
                    AnnualActorStates.Remove(actorId);
                    MclslCultivatorCandidateIndex.MarkNonCultivator(actorId);
                    continue;
                }

                int activeYear = state.ActiveYear > 0
                    ? state.ActiveYear
                    : ResolveActiveYear(state, context.CurrentYear);
                state.ActiveYear = activeYear;

                bool hasNextStage;
                MclslAnnualPipelineStage nextStage;
                int nextStep;
                long annualStageSample = MclslPerformanceProbe.Begin();
                long annualStageStarted = Stopwatch.GetTimestamp();
                try
                {
                    MclslDiagnostics.Cultivation(
                        "scheduler.tick.stage_start",
                        "actor=" + actorId
                        + " active=" + activeYear
                        + " stage=" + state.Stage);
                    string profilerStage = state.Stage switch
                    {
                        MclslAnnualPipelineStage.Prepare => "MCLS/Annual/ActorPrepare",
                        MclslAnnualPipelineStage.Progression => "MCLS/Annual/ActorProgression",
                        MclslAnnualPipelineStage.Market => "MCLS/Annual/ActorMarket",
                        _ => "MCLS/Annual/ActorFinalize"
                    };
                    using (MclslUnityProfiler.Sample(profilerStage))
                    {
                        hasNextStage = MclslAnnualActorPipeline.ProcessStage(
                            actor,
                            activeYear,
                            state.Stage,
                            state.Step,
                            out nextStage,
                            out nextStep);
                    }
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
                    // 单步可能已产生副作用，不能重放整段年度流水线。
                    MclslWorldRunRepository.RecordAnnualFailure(activeYear, "角色", actorId.ToString(),
                        state.Stage + "." + state.Step, 1, "部分失败", ex.Message);
                    hasNextStage = state.Stage != MclslAnnualPipelineStage.Finalize;
                    nextStage = state.Stage;
                    nextStep = state.Step + 1;
                    if (state.Stage == MclslAnnualPipelineStage.Progression && state.Step == 0)
                        nextStep = 2; // 基础增长未确认，不能继续突破。
                    if (state.Stage == MclslAnnualPipelineStage.Prepare && nextStep > 10)
                    {
                        nextStage = MclslAnnualPipelineStage.Progression;
                        nextStep = 0;
                    }
                    if (state.Stage == MclslAnnualPipelineStage.Progression && nextStep > 6)
                    {
                        nextStage = MclslAnnualPipelineStage.Market;
                        nextStep = 0;
                    }
                    if (state.Stage == MclslAnnualPipelineStage.Market)
                    {
                        nextStage = MclslAnnualPipelineStage.Finalize;
                        nextStep = 0;
                    }
                }
                finally
                {
                    MclslPerformanceProbe.End(AnnualStageProbeName(state.Stage), annualStageSample);
                    double stageMs = (Stopwatch.GetTimestamp() - annualStageStarted) * 1000d / Stopwatch.Frequency;
                    if (stageMs > timeBudgetMs)
                        MclslDiagnostics.Error("annual-actor-overbudget:" + state.Stage + "." + state.Step,
                            "角色年度单步超预算：角色=" + actorId + " 年=" + activeYear
                            + " 步骤=" + state.Stage + "." + state.Step
                            + " 耗时=" + stageMs.ToString("F2") + "ms 预算=" + timeBudgetMs.ToString("F2") + "ms");
                }

                if (MclslActorAccessor.Alive(actor)
                    && !string.IsNullOrWhiteSpace(MclslActorAccessor.Realm(actor)))
                {
                    AddLineageActor(actor);
                }

                if (hasNextStage)
                {
                    state.Stage = nextStage;
                    state.Step = nextStep;
                    state.DueFrame = 0;
                    PersistAnnualState(actor, state);
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
                        state.Step = 0;
                        state.DueFrame = 0;
                        QueueExistingState(actorId, state);
                    }
                    else
                    {
                        CompletePersistedAnnualState(actor, state.LastCompletedYear);
                        AnnualActorStates.Remove(actorId);
                    }
                }
            }
            finally
            {
                MclslStaggeredWorkPolicy.End(category, categoryStarted);
            }

            if (HasExceededTimeBudget(started, timeBudgetMs) || MclslStaggeredWorkPolicy.GlobalBudgetExhausted)
            {
                break;
            }
        }
    }

    private static bool ShouldDeferIncompleteProgression(Actor actor, MclslAnnualPipelineStage stage, int activeYear)
    {
        if (stage != MclslAnnualPipelineStage.Progression
            && stage != MclslAnnualPipelineStage.Market
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
        if (state == null) return;
        if (state.Queued)
        {
            MclslStaggeredWorkPolicy.RecordDeduplicated(MclslStaggeredWorkPolicy.Classify(state.Stage, state.Step));
            return;
        }
        if (state.DueFrame <= UnityEngine.Time.frameCount)
            state.DueFrame = MclslStaggeredWorkPolicy.AssignDueFrame(actorId, state.ActiveYear, state.Stage, state.Step);
        state.Queued = true;
        AnnualActorQueue.Enqueue(actorId);
    }

    private static void RequeuePreservingDueFrame(long actorId, AnnualActorState state)
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
        int stepValue = MclslActorAccessor.GetInt(actor, MclslActorDataKeys.AnnualStep, 0);
        int lastCompletedYear = ReadEffectiveLastCompletedYear(actor);
        if (activeYear <= 0
            || latestYear < activeYear
            || activeYear <= lastCompletedYear
            || stageValue < (int)MclslAnnualPipelineStage.Prepare
            || stageValue > (int)MclslAnnualPipelineStage.Market)
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
            Step = Math.Clamp(stepValue, 0, 10),
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
        MclslActorAccessor.Set(actor, MclslActorDataKeys.AnnualStep, Math.Max(0, state.Step));
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
        if (_annualCandidateRefreshPending || _annualCandidateScanPending) return;
        foreach (AnnualActorState state in AnnualActorStates.Values)
            if (state.ActiveYear > 0 && state.ActiveYear <= _activeAnnualYear) return;

        RestoreLineageActors();
        if (!MclslStaggeredWorkPolicy.TryBegin(MclslWorkCategory.WorldAggregate, out long worldStarted)) return;
        bool completed;
        try { completed = MclslAnnualWorldRuntimeLane.Tick(LineageActors); }
        finally { MclslStaggeredWorkPolicy.End(MclslWorkCategory.WorldAggregate, worldStarted); }
        if (completed)
        {
            int completedYear = _activeAnnualYear;
            MclslWorldActorQuery.EndAnnualDetection(completedYear);
            LineageActors.Clear();
            LineageActorIds.Clear();
            PersistedLineageIds.Clear();
            MclslPerformanceProbe.End("年度周期总耗时", _annualCycleStarted);
            _annualCycleStarted = 0L;
            MclslAnnualBatchState batch = MclslWorldRunRepository.Current.AnnualBatch;
            batch.LastCompletedYear = Math.Max(batch.LastCompletedYear, completedYear);
            batch.ActiveYear = 0;
            batch.WorldStage = 0;
            batch.EraSnapshotRecorded = false;
            batch.LineageActorIds.Clear();
            _activeAnnualYear = -1;
            MclslWorldArchiveStore.MarkDirty();
            if (batch.LatestRequestedYear > completedYear) StartAnnualWorld(completedYear + 1);
        }
    }

    private static void RestoreLineageActors()
    {
        foreach (long actorId in MclslWorldRunRepository.Current.AnnualBatch.LineageActorIds)
            if (!LineageActorIds.Contains(actorId)
                && MclslActorRegistry.ResolveKnownOrWorld(actorId, out Actor actor)) AddLineageActor(actor);
    }

    private static void AddLineageActor(Actor actor)
    {
        long actorId = MclslActorAccessor.Id(actor);
        if (actorId <= 0L || !LineageActorIds.Add(actorId)) return;
        LineageActors.Add(actor);
        MclslAnnualBatchState batch = MclslWorldRunRepository.Current.AnnualBatch;
        if (PersistedLineageIds.Add(actorId))
        {
            batch.LineageActorIds.Add(actorId);
            MclslWorldArchiveStore.MarkDirty();
        }
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

    private static string AnnualStageProbeName(MclslAnnualPipelineStage stage) => stage switch
    {
        MclslAnnualPipelineStage.Prepare => "年度角色.准备",
        MclslAnnualPipelineStage.Progression => "年度角色.修行推进",
        MclslAnnualPipelineStage.Market => "年度角色.天玄镜交易",
        MclslAnnualPipelineStage.Finalize => "年度角色.收尾",
        _ => "年度角色.未知阶段"
    };

    private static bool HasExceededTimeBudget(long startedTimestamp, double budgetMilliseconds)
    {
        long elapsedTicks = Stopwatch.GetTimestamp() - startedTimestamp;
        return elapsedTicks > budgetMilliseconds * Stopwatch.Frequency / 1000.0;
    }
}
