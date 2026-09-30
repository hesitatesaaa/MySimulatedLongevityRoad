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
        internal double FirstEssenceQueuedAt = UnityEngine.Time.unscaledTime;
    }

    private const int CleanupIntervalTicks = 128;
    private const int CleanupRemoveBudget = 64;
    private const int AnnualCandidateEnqueueBudget = 64;

    private static readonly MclslAnnualReadyQueue AnnualActorQueue = new();
    private static readonly Dictionary<long, AnnualActorState> AnnualActorStates = new();
    private static readonly List<long> LineageActorSlots = new();
    private static readonly IReadOnlyList<Actor> LineageActors = MclslActorRegistry.CreateView(LineageActorSlots);
    private static readonly HashSet<long> LineageActorIds = new();
    private static readonly HashSet<long> PersistedLineageIds = new();
    private static int _tickCounter;
    private static int _activeAnnualYear = -1;
    private static bool _newLawActive;
    private static int _worldLaneScheduleFailures;
    private static int _worldLaneRetryAfterFrame;
    private static bool _annualStartPending;
    private static int _startLineageCursor;
    private static int _restoreLineageCursor;
    private static int _startRuntimeCursor;
    private static bool _annualCandidateScanPending;
    private static IReadOnlyList<long> _annualCandidateScanIds = Array.Empty<long>();
    private static int _annualCandidateScanCursor;
    private static long _annualCycleStarted;

    internal static bool HasFastWork => _activeAnnualYear > 0
        || MclslWorldBootstrapLane.HasPending
        || AnnualActorQueue.Count > 0
        || _annualStartPending
        || _annualCandidateScanPending
        || MclslAnnualWorldRuntimeLane.HasPending
        || MclslWorldEpochSystem.HasPendingTransitionWork;
    internal static bool HasAnnualActorBacklog => AnnualActorQueue.Count > 0;
    internal static bool HasAnnualCandidateBacklog => _annualStartPending || _annualCandidateScanPending;
    private static int AnnualUrgentBacklogThreshold => Math.Min(2048,
        Math.Max(256, MclslCultivatorCandidateIndex.CultivatorCount / 2));
    internal static bool HasUrgentSimulationBacklog => AnnualActorQueue.Count > AnnualUrgentBacklogThreshold;
    internal static int AnnualActorBacklogCount => AnnualActorQueue.Count;
    internal static int AnnualActorStateCount => AnnualActorStates.Count;
    internal static int AnnualReadyCount => AnnualActorQueue.ReadyCount;
    internal static int AnnualWaitingCount => AnnualActorQueue.WaitingCount;
    internal static int OldestWaitingYear => AnnualActorQueue.OldestWaitingYear;
    internal static int NewestWaitingYear => AnnualActorQueue.NewestWaitingYear;
    internal static bool AnnualWorldWorkPending => MclslAnnualWorldRuntimeLane.HasPending;
    internal static int AnnualCandidateScanRemaining => _annualStartPending
        ? -1
        : _annualCandidateScanPending
            ? Math.Max(0, _annualCandidateScanIds.Count - _annualCandidateScanCursor)
            : 0;

    internal static void GetAnnualYearSpan(out int latestRequestedYear, out int oldestPendingYear)
    {
        MclslAnnualBatchState batch = MclslWorldRunRepository.Current.AnnualBatch;
        latestRequestedYear = batch.LatestRequestedYear;
        oldestPendingYear = AnnualActorQueue.OldestWaitingYear;
        if (batch.ActiveYear > 0 && (oldestPendingYear == 0 || batch.ActiveYear < oldestPendingYear))
            oldestPendingYear = batch.ActiveYear;
    }

    internal static void InitializeAfterLoad(int currentYear)
    {
        AnnualActorQueue.Clear();
        AnnualActorStates.Clear();
        LineageActorSlots.Clear();
        LineageActorIds.Clear();
        PersistedLineageIds.Clear();
        MclslDetectionGate.ClearRuntimeState();
        MclslLongevityRules.ClearRuntimeCache();
        MclslAnnualExecutionContext.Clear();
        MclslCultivatorCandidateIndex.Clear();
        MclslWorldActorQuery.ClearCache();
        _activeAnnualYear = -1;
        _annualStartPending = false;
        _startLineageCursor = 0;
        _restoreLineageCursor = 0;
        _startRuntimeCursor = 0;
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
        MclslTechniqueOccupationSystem.Clear();
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
        _activeAnnualYear = year;
        batch.ActiveYear = year;
        _annualCycleStarted = MclslPerformanceProbe.Begin();
        _annualStartPending = true;
        _startLineageCursor = 0;
        LineageActorSlots.Clear();
        LineageActorIds.Clear();
        PersistedLineageIds.Clear();

    }

    private static void ContinueAnnualStart()
    {
        MclslAnnualBatchState batch = MclslWorldRunRepository.Current.AnnualBatch;
        while (_startLineageCursor < batch.LineageActorIds.Count && !AnnualBudgetExpired())
            PersistedLineageIds.Add(batch.LineageActorIds[_startLineageCursor++]);
        if (AnnualBudgetExpired()) return;
        int year = _activeAnnualYear;
        while (batch.StartCursor < 6 && !AnnualBudgetExpired())
        {
            switch (batch.StartCursor)
            {
                case 0: RunAnnualStartStep(year, "MarketPruneBegin", () => MclslTianxuanMarket.BeginAnnualPrune(year)); break;
                case 1:
                    if (!batch.EraSnapshotRecorded)
                        batch.NewLawEraActive = RunAnnualStartQuery(year, "NewLawEra", () => MclslWorldEpochSystem.IsNewLawActive(year));
                    break;
                case 2:
                    if (!batch.EraSnapshotRecorded)
                        batch.NewLawCultivationAvailable = RunAnnualStartQuery(year, "NewLawCultivation", () => MclslNewLawPioneerSystem.CanPracticeNewLaw(year));
                    break;
                case 3: batch.EraSnapshotRecorded = true; break;
                case 4:
                    // Keep the persisted startup cursor here until the read-only
                    // candidate query finishes; no population-wide work in one frame.
                    if (!MclslNewLawPioneerSystem.ProcessAnnual(year)) return;
                    break;
                case 5: RunAnnualStartStep(year, "AdventureBegin", () => MclslAdventureSystem.BeginAnnual(year)); break;
            }
            batch.StartCursor++;

        }
        if (batch.StartCursor < 6) return;
        // Saved startup cursors describe persistent effects. Runtime registries
        // must be restored independently after a load; both begin methods are idempotent.
        while (_startRuntimeCursor < 1 && !AnnualBudgetExpired())
        {
            RunAnnualStartStep(year, "AdventureRestore", () => MclslAdventureSystem.BeginAnnual(year));
            _startRuntimeCursor++;
        }
        if (_startRuntimeCursor < 1) return;
        _newLawActive = batch.NewLawEraActive;
        // Runtime candidate membership is rebuilt even when a persisted startup is complete.
        BeginAnnualCandidateScan();
        _annualStartPending = false;
    }

    private static void EnsureWorldLaneScheduled()
    {
        if (_annualStartPending || _activeAnnualYear <= 0 || MclslAnnualWorldRuntimeLane.HasPending
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
            batch.LastCompletedYear = Math.Max(batch.LastCompletedYear, year);
            batch.ActiveYear = 0;
            batch.WorldStage = 0;
            batch.EraSnapshotRecorded = false;
            batch.StartCursor = 0;
            batch.LineageActorIds.Clear();
            batch.InverseWork = null;
            _activeAnnualYear = -1;
            _worldLaneScheduleFailures = 0;
            _worldLaneRetryAfterFrame = 0;
            MclslAnnualWorldRuntimeLane.Clear();

            if (batch.LatestRequestedYear > year) StartAnnualWorld(year + 1);
        }
    }

    private static void RunAnnualStartStep(int year, string step, Action action)
    {
        long sample = MclslPerformanceProbe.Begin();
        try { action(); }
        catch (Exception ex)
        {
            MclslWorldRunRepository.RecordAnnualFailure(year, "年度启动", string.Empty,
                step, 1, "部分失败", ex.Message);
        }
        finally { if (sample != 0L) MclslPerformanceProbe.End("年度启动." + step, sample); }
    }

    private static bool RunAnnualStartQuery(int year, string step, Func<bool> query)
    {
        long sample = MclslPerformanceProbe.Begin();
        try { return query(); }
        catch (Exception ex)
        {
            MclslWorldRunRepository.RecordAnnualFailure(year, "年度启动", string.Empty,
                step, 1, "跳过", ex.Message);
            return false;
        }
        finally { if (sample != 0L) MclslPerformanceProbe.End("年度启动." + step, sample); }
    }

    internal static void ProcessAll(in MclslSchedulerContext context)
    {
        if (!MclslRuntimeSettings.CoreEnabled) return;
        MclslAnnualFrameBudget.Begin(MclslAnnualFrameBudget.ForPressure((int)MclslRuntimeWorkBudget.StressTier));
        try
        {
            if (_activeAnnualYear > 0 || HasAnnualActorBacklog || HasAnnualCandidateBacklog)
            if (MclslWorldBootstrapLane.HasPending) TickBootstrapLane();
            if (AnnualBudgetExpired()) return;
            if (_annualStartPending) ContinueAnnualStart();
            if (_annualStartPending || AnnualBudgetExpired()) return;
            if (_activeAnnualYear > 0)
            {
                EnsureWorldLaneScheduled();
                if (AnnualBudgetExpired()) return;
                long sample = MclslPerformanceProbe.Begin();
                bool pruned;
                try { pruned = MclslTianxuanMarket.TickAnnualPrune(); }
                finally { MclslPerformanceProbe.End("年度启动.MarketPrune", sample); }
                if (!pruned || AnnualBudgetExpired()) return;
            }
            if (_annualCandidateScanPending)
            {
                long sample = MclslPerformanceProbe.Begin();
                try { TickAnnualCandidateScan(); }
                finally { MclslPerformanceProbe.End("年度候选扫描", sample); }
            }
            AnnualActorQueue.Release(_activeAnnualYear, AnnualCandidateEnqueueBudget, AnnualBudgetExpired);
            if (!AnnualBudgetExpired() && MclslWorldEpochSystem.HasPendingTransitionWork)
            {
                long sample = MclslPerformanceProbe.Begin();
                try { MclslWorldEpochSystem.TickDeferredTransitionWork(); }
                finally { MclslPerformanceProbe.End("新法转化", sample); }
            }
            if (!AnnualBudgetExpired() && AnnualActorQueue.ReadyCount > 0)
            {
                long sample = MclslPerformanceProbe.Begin();
                try { TickAnnualActors(context); }
                finally { MclslPerformanceProbe.End("年度角色", sample); }
            }
            if (!AnnualBudgetExpired() && MclslAnnualWorldRuntimeLane.HasPending)
                TryResolveAnnualWorld();
            if (!AnnualBudgetExpired()) TickCleanup();
        }
        finally { MclslAnnualFrameBudget.End(); }
    }

    private static bool AnnualBudgetExpired() => MclslAnnualFrameBudget.Expired;

    internal static void ForgetAnnualActor(long actorId)
    {
        AnnualActorQueue.Remove(actorId);
        AnnualActorStates.Remove(actorId);
        MclslTianxuanMarket.ForgetPurchaseBatch(actorId);
    }

    internal static void Clear()
    {
        AnnualActorQueue.Clear();
        AnnualActorStates.Clear();
        LineageActorSlots.Clear();
        LineageActorIds.Clear();
        PersistedLineageIds.Clear();
        _tickCounter = 0;
        _activeAnnualYear = -1;
        _newLawActive = false;
        _worldLaneScheduleFailures = 0;
        _worldLaneRetryAfterFrame = 0;
        _annualStartPending = false;
        _startLineageCursor = 0;
        _annualCandidateScanPending = false;
        _annualCandidateScanIds = Array.Empty<long>();
        _annualCandidateScanCursor = 0;
        _annualCycleStarted = 0L;
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
        _annualCandidateScanIds = MclslCultivatorCandidateIndex.GetAnnualCandidateIds();
        _annualCandidateScanCursor = 0;
        _annualCandidateScanPending = _annualCandidateScanIds.Count > 0;
    }

    private static void TickAnnualCandidateScan()
    {
        int processed = 0;
        while (_annualCandidateScanCursor < _annualCandidateScanIds.Count
            && processed < AnnualCandidateEnqueueBudget && !AnnualBudgetExpired())
        {
            long actorId = _annualCandidateScanIds[_annualCandidateScanCursor++];
            processed++;
            if (MclslCultivatorCandidateIndex.Resolve(actorId, out Actor actor)
                && ShouldQueueAnnualActor(actor, _activeAnnualYear))
                EnqueueAnnualActorCore(actor, _activeAnnualYear);
        }
        if (_annualCandidateScanCursor < _annualCandidateScanIds.Count) return;
        _annualCandidateScanPending = false;
        _annualCandidateScanIds = Array.Empty<long>();
        _annualCandidateScanCursor = 0;
    }

    private static void TickBootstrapLane()
    {
        MclslWorldBootstrapLane.Tick(MclslRuntimeWorkBudget.ScaleCount(96, 16),
            MclslAnnualFrameBudget.RemainingMs);
    }

    private static void TickAnnualActors(in MclslSchedulerContext context)
    {
        int stageBudget = MclslRuntimeWorkBudget.ScaleCount(MclslRuntimeSettings.AnnualActorBudget, 8);
        double timeBudgetMs = MclslAnnualFrameBudget.RemainingMs;
        long started = Stopwatch.GetTimestamp();
        int processed = 0;
        int examined = 0;
        int examineBudget = Math.Max(64, stageBudget * 4);
        while (AnnualActorQueue.ReadyCount > 0 && processed < stageBudget && examined < examineBudget
            && !AnnualBudgetExpired())
        {
            if (!AnnualActorQueue.TryDequeue(out long actorId)) break;
            examined++;
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

            {
                if (!MclslActorRegistry.Resolve(actorId, out Actor actor)
                    || actor?.data == null
                    || !MclslActorAccessor.Alive(actor))
                {
                    MclslDiagnostics.Cultivation("scheduler.tick.remove", "actor=" + actorId + " reason=missing-or-dead");
                    AnnualActorStates.Remove(actorId);
                    MclslTianxuanMarket.ForgetPurchaseBatch(actorId);
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
                    MclslTianxuanMarket.ForgetPurchaseBatch(actorId);
                    MclslCultivatorCandidateIndex.MarkNonCultivator(actorId);
                    continue;
                }

                int activeYear = state.ActiveYear > 0
                    ? state.ActiveYear
                    : ResolveActiveYear(state, context.CurrentYear);
                state.ActiveYear = activeYear;

                bool awaitingFirstEssence = MclslPerformanceProbe.Enabled
                    && MclslCultivationGrowthSystem.CurrentTrueEssence(actor) <= 0;
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
                        + " stage=" + state.Stage
                        + " lagYears=" + Math.Max(0, context.CurrentYear - activeYear));
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
                        MclslTianxuanMarket.ForgetPurchaseBatch(actorId);
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

                if (awaitingFirstEssence && MclslCultivationGrowthSystem.CurrentTrueEssence(actor) > 0)
                    MclslAnnualBackpressure.RecordFirstEssenceWait(UnityEngine.Time.unscaledTime - state.FirstEssenceQueuedAt);
                MclslAnnualBackpressure.RecordStep();
                if (!MclslActorAccessor.Alive(actor) || !AnnualActorStates.ContainsKey(actorId))
                {
                    ForgetAnnualActor(actorId);
                    continue;
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
                        PersistAnnualState(actor, state);
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
        if (state.Queued) return;
        state.Queued = true;
        AnnualActorQueue.Enqueue(actorId, state.ActiveYear, _activeAnnualYear);
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

    private static int ReadInitialLastCompletedYear(Actor actor, int requestedYear)
    {
        if (actor?.data == null) return 0;
        int requested = Math.Max(0, requestedYear);
        int completed = Math.Min(ReadEffectiveLastCompletedYear(actor), requested);
        if (completed > 0) return completed;
        // The authoritative annual cursor is independent of essence spent on gameplay.
        // New actors never receive work that predates their cultivation start.
        int start = MclslActorAccessor.GetInt(actor, MclslActorDataKeys.CultivationStartYear, requested);
        return Math.Max(0, Math.Min(requested - 1, start > 0 ? start - 1 : requested - 1));
    }

    private static int ReadEffectiveLastCompletedYear(Actor actor)
    {
        if (actor?.data == null) return 0;
        int persistedYear = MclslActorAccessor.GetInt(actor, MclslActorDataKeys.AnnualLastCompletedYear, 0);
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
        MclslTianxuanMarket.ForgetPurchaseBatch(actorId);

        int persistedYear = MclslActorAccessor.GetInt(actor, MclslActorDataKeys.AnnualLastCompletedYear, 0);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.AnnualLastCompletedYear, Math.Max(persistedYear, normalizedYear));
        MclslActorAccessor.Set(actor, MclslActorDataKeys.AnnualActiveYear, 0);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.AnnualStage, 0);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.AnnualStep, 0);
    }

    private static void TryResolveAnnualWorld()
    {
        if (_activeAnnualYear <= 0) return;
        if (MclslWorldBootstrapLane.HasPending) return;
        if (_annualStartPending || _annualCandidateScanPending) return;
        if (AnnualActorQueue.ReadyCount > 0
            || (AnnualActorQueue.OldestWaitingYear > 0 && AnnualActorQueue.OldestWaitingYear <= _activeAnnualYear)) return;

        if (!RestoreLineageActors()) return;
        bool completed = MclslAnnualWorldRuntimeLane.Tick(LineageActors);
        if (completed)
        {
            int completedYear = _activeAnnualYear;
            MclslAnnualBackpressure.RecordYear();
            LineageActorSlots.Clear();
            LineageActorIds.Clear();
            PersistedLineageIds.Clear();
            MclslPerformanceProbe.End("年度周期总耗时", _annualCycleStarted, acrossFrames: true);
            _annualCycleStarted = 0L;
            MclslAnnualBatchState batch = MclslWorldRunRepository.Current.AnnualBatch;
            batch.LastCompletedYear = Math.Max(batch.LastCompletedYear, completedYear);
            batch.ActiveYear = 0;
            batch.WorldStage = 0;
            batch.EraSnapshotRecorded = false;
            batch.StartCursor = 0;
            batch.LineageActorIds.Clear();
            batch.InverseWork = null;
            _activeAnnualYear = -1;

            if (batch.LatestRequestedYear > completedYear) StartAnnualWorld(completedYear + 1);
        }
    }

    private static bool RestoreLineageActors()
    {
        List<long> ids = MclslWorldRunRepository.Current.AnnualBatch.LineageActorIds;
        while (_restoreLineageCursor < ids.Count && !AnnualBudgetExpired())
        {
            long actorId = ids[_restoreLineageCursor++];
            if (!LineageActorIds.Contains(actorId)
                && MclslActorRegistry.ResolveKnownOrWorld(actorId, out Actor actor)) AddLineageActor(actor);
        }
        return _restoreLineageCursor >= ids.Count;
    }

    private static void AddLineageActor(Actor actor)
    {
        long actorId = MclslActorAccessor.Id(actor);
        if (actorId <= 0L || !LineageActorIds.Add(actorId)) return;
        LineageActorSlots.Add(actorId);
        MclslAnnualBatchState batch = MclslWorldRunRepository.Current.AnnualBatch;
        if (PersistedLineageIds.Add(actorId))
        {
            batch.LineageActorIds.Add(actorId);

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
