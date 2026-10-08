using System;
using System.Collections.Generic;
using System.Linq;
using MySimulatedLongevityRoad.Core;
using MySimulatedLongevityRoad.Data;
using MySimulatedLongevityRoad.Queries;
using MySimulatedLongevityRoad.Systems;

namespace MySimulatedLongevityRoad.Modules;

internal static class MclslModuleHub
{
    private sealed class AnnualWorkItem
    {
        internal int NextYear;
        internal int LatestYear;
        internal bool CoreEnabled;
        internal int Failures;
        internal int RetryAfterFrame;
    }
    private sealed class LoadRecoveryWorkItem
    {
        internal MclslModuleBase Module;
        internal int Failures;
        internal int RetryAfterFrame;
    }
    private static readonly List<MclslModuleBase> Modules = new();
    private static readonly Dictionary<MclslModuleBase, AnnualWorkItem> PendingAnnualWork = new();
    private static readonly Dictionary<MclslModuleBase, int> CompletedAnnualYears = new();
    private static readonly Queue<LoadRecoveryWorkItem> LoadRecoveryQueue = new();
    private static bool _initialized;
    private static int _annualModuleCursor;
    private static int _loadRecoveryYear;
    private static bool _loadRecoveryCoreEnabled;
    private static int _frameCounter;
    private static int _priorityFrame = -1;

    internal static int AnnualModuleBacklogCount => PendingAnnualWork.Count;
    internal static int LoadRecoveryBacklogCount => LoadRecoveryQueue.Count;

    internal static void Init()
    {
        if (_initialized) return;
        _initialized = true;
        Modules.Clear();
        Register(new MclslPersistenceModule());
        Register(new MclslHuanzhenModule());
        Register(new MclslTimelineModule());
        Register(new MclslWorldEpochModule());
        Register(new MclslWorldSoulModule());
        Register(new MclslMapMarkerModule());
        Register(new MclslInverseTruthModule());
        Register(new MclslFactionMissionModule());
        Register(new MclslFactionPressureModule());
        Register(new MclslRuntimeCadenceModule());
        Register(new MclslAnnouncementModule());
        Modules.Sort((a, b) => a.Order != b.Order ? a.Order.CompareTo(b.Order) : string.CompareOrdinal(a.Name, b.Name));
        ForEach(true, module => module.Init(), "Init");
    }

    internal static void OnWorldLoaded(int year, bool coreEnabled)
    {
        PendingAnnualWork.Clear();
        CompletedAnnualYears.Clear();
        _annualModuleCursor = 0;
        _priorityFrame = -1;
        LoadRecoveryQueue.Clear();
        ForEach(coreEnabled, module => module.OnWorldLoaded(year), "OnWorldLoaded");
        MclslAnnualBatchState batch = MclslWorldRunRepository.Current.AnnualBatch;
        foreach (MclslModuleBase module in Modules)
        {
            if (!module.HasAnnualStep || !batch.ModuleCompletedYears.TryGetValue(module.Name, out int completed)) continue;
            CompletedAnnualYears[module] = completed;
            if (completed < year) DeferredAnnualStep(module, year, coreEnabled);
        }
        ScheduleLoadRecovery(year, coreEnabled);
    }

    private static void ScheduleLoadRecovery(int year, bool coreEnabled)
    {
        LoadRecoveryQueue.Clear();
        _loadRecoveryYear = year;
        _loadRecoveryCoreEnabled = coreEnabled;
        for (int i = 0; i < Modules.Count; i++)
        {
            MclslModuleBase module = Modules[i];
            if (!module.HasLoadRecovery) continue;
            if (!coreEnabled && !module.RunsWhenCoreDisabled) continue;
            DeferredLoadRecoveryStep(module);
        }
    }

    internal static void TickRealtime(bool coreEnabled) => ForEach(coreEnabled, module => module.TickRealtime(), "TickRealtime");
    internal static void TickPriorityAnnualWork(int frameCounter, bool coreEnabled)
    {
        if (_priorityFrame == frameCounter) return;
        _priorityFrame = frameCounter;
        _frameCounter = frameCounter;
        DrainLoadRecoveryQueue();
        DrainAnnualQueue();
        if (coreEnabled) MclslRuntimeCadence.Tick(frameCounter);
    }
    internal static void TickFrame(int frameCounter, bool coreEnabled)
    {
        TickPriorityAnnualWork(frameCounter, coreEnabled);
        ForEach(coreEnabled, module => module.TickFrame(frameCounter), "TickFrame");
    }

    internal static void TickAnnual(int year, bool coreEnabled)
    {
        for (int i = 0; i < Modules.Count; i++)
        {
            MclslModuleBase module = Modules[i];
            if (!module.HasAnnualStep) continue;
            if (!coreEnabled && !module.RunsWhenCoreDisabled) continue;
            DeferredAnnualStep(module, year, coreEnabled);
        }
    }
    internal static void PrepareForSave() => ForEach(true, module => module.PrepareForSave(), "PrepareForSave");
    internal static void Clear()
    {
        PendingAnnualWork.Clear();
        CompletedAnnualYears.Clear();
        _annualModuleCursor = 0;
        _priorityFrame = -1;
        LoadRecoveryQueue.Clear();
        ForEach(true, module => module.Clear(), "Clear");
    }

    private static void Register(MclslModuleBase module)
    {
        if (module != null) Modules.Add(module);
    }

    private static void ForEach(bool coreEnabled, Action<MclslModuleBase> action, string hook)
    {
        for (int i = 0; i < Modules.Count; i++)
        {
            MclslModuleBase module = Modules[i];
            if (!coreEnabled && !module.RunsWhenCoreDisabled) continue;
            try { action(module); }
            catch (Exception ex) { MclslDiagnostics.Error("module:" + module.Name + ":" + hook, module.Name + "." + hook + " 失败: " + ex.Message); }
        }
    }

    private static void DeferredAnnualStep(MclslModuleBase module, int year, bool coreEnabled)
    {
        if (module == null) return;
        if (PendingAnnualWork.TryGetValue(module, out AnnualWorkItem pending))
        {
            pending.LatestYear = Math.Max(pending.LatestYear, year);
            pending.CoreEnabled = coreEnabled;
            return;
        }
        int nextYear = CompletedAnnualYears.TryGetValue(module, out int completed)
            ? Math.Min(year, completed + 1) : year;
        PendingAnnualWork[module] = new AnnualWorkItem
        {
            NextYear = nextYear,
            LatestYear = year,
            CoreEnabled = coreEnabled
        };
    }

    private static void DeferredLoadRecoveryStep(MclslModuleBase module)
    {
        if (module != null) LoadRecoveryQueue.Enqueue(new LoadRecoveryWorkItem { Module = module });
    }

    private static void DrainAnnualQueue()
    {
        const int annualModuleBudgetPerFrame = 2;
        for (int step = 0; step < annualModuleBudgetPerFrame && PendingAnnualWork.Count > 0 && !MclslFrameDeadline.Expired; step++)
        {
            MclslModuleBase module = null;
            for (int i = 0; i < Modules.Count; i++)
            {
                int index = (_annualModuleCursor + i) % Modules.Count;
                MclslModuleBase candidate = Modules[index];
                if (PendingAnnualWork.TryGetValue(candidate, out AnnualWorkItem candidateWork)
                    && candidateWork.RetryAfterFrame <= _frameCounter)
                {
                    module = candidate;
                    _annualModuleCursor = (index + 1) % Modules.Count;
                    break;
                }
            }
            if (module == null || !PendingAnnualWork.TryGetValue(module, out AnnualWorkItem work)) break;
            PendingAnnualWork.Remove(module);
            if (!work.CoreEnabled && !module.RunsWhenCoreDisabled) continue;
            if (!SafeAnnualStep(module, work.NextYear))
            {
                work.Failures++;
                // Annual module callbacks can cross multiple writes; without a commit cursor
                // the whole callback cannot be safely replayed after an exception.
                bool safeToRetry = false;
                if (MclslAnnualResiliencePolicy.ShouldRetry(work.Failures, !safeToRetry))
                {
                    work.RetryAfterFrame = MclslAnnualResiliencePolicy.RetryAtFrame(_frameCounter);
                    PendingAnnualWork[module] = work;
                }
                else
                {
                    MclslWorldRunRepository.RecordAnnualFailure(work.NextYear, "模块", module.Name,
                        "TickAnnual", work.Failures, safeToRetry ? "跳过" : "部分失败", "年度模块执行异常；详见诊断日志");
                    CompletedAnnualYears[module] = work.NextYear;
                    MclslWorldRunRepository.Current.AnnualBatch.ModuleCompletedYears[module.Name] = work.NextYear;
                    MclslWorldArchiveStore.MarkDirty();
                    if (work.NextYear < work.LatestYear)
                    {
                        work.NextYear++;
                        work.Failures = 0;
                        work.RetryAfterFrame = MclslAnnualResiliencePolicy.RetryAtFrame(_frameCounter);
                        PendingAnnualWork[module] = work;
                    }
                }
                continue;
            }
            work.Failures = 0;
            MclslAnnualBackpressure.RecordProgress();
            CompletedAnnualYears[module] = work.NextYear;
            MclslWorldRunRepository.Current.AnnualBatch.ModuleCompletedYears[module.Name] = work.NextYear;
            MclslWorldArchiveStore.MarkDirty();
            if (work.NextYear < work.LatestYear)
            {
                work.NextYear++;
                PendingAnnualWork[module] = work;
            }
        }
    }

    private static void DrainLoadRecoveryQueue()
    {
        const int loadRecoveryModuleBudgetPerFrame = 1;
        for (int i = 0; i < loadRecoveryModuleBudgetPerFrame && LoadRecoveryQueue.Count > 0 && !MclslFrameDeadline.Expired; i++)
        {
            LoadRecoveryWorkItem work = LoadRecoveryQueue.Dequeue();
            if (work.RetryAfterFrame > _frameCounter)
            {
                LoadRecoveryQueue.Enqueue(work);
                continue;
            }
            MclslModuleBase module = work.Module;
            if (!_loadRecoveryCoreEnabled && !module.RunsWhenCoreDisabled) continue;
            if (!SafeLoadRecoveryStep(module, _loadRecoveryYear))
            {
                work.Failures++;
                bool safeToRetry = false;
                if (MclslAnnualResiliencePolicy.ShouldRetry(work.Failures, !safeToRetry))
                {
                    work.RetryAfterFrame = MclslAnnualResiliencePolicy.RetryAtFrame(_frameCounter);
                    LoadRecoveryQueue.Enqueue(work);
                }
                else
                    MclslWorldRunRepository.RecordAnnualFailure(_loadRecoveryYear, "模块", module.Name,
                        "TickLoadRecovery", work.Failures, safeToRetry ? "跳过" : "部分失败",
                        "读档恢复失败；详见诊断日志");
            }
        }
    }

    private static bool SafeAnnualStep(MclslModuleBase module, int year)
    {
        if (!MclslPerformanceProbe.Enabled)
        {
            try { module?.TickAnnual(year); return true; }
            catch (Exception ex) { MclslDiagnostics.Error("module:" + module?.Name + ":TickAnnual", module?.Name + ".TickAnnual 失败，年度任务已保留: " + ex); return false; }
        }

        long sample = MclslPerformanceProbe.Begin();
        try { module?.TickAnnual(year); return true; }
        catch (Exception ex) { MclslDiagnostics.Error("module:" + module?.Name + ":TickAnnual", module?.Name + ".TickAnnual 失败，年度任务已保留: " + ex); return false; }
        finally { MclslPerformanceProbe.End("年度模块." + module?.Name, sample); }
    }

    private static bool SafeLoadRecoveryStep(MclslModuleBase module, int year)
    {
        try { module?.TickLoadRecovery(year); return true; }
        catch (Exception ex) { MclslDiagnostics.Error("module:" + module?.Name + ":TickLoadRecovery", module?.Name + ".TickLoadRecovery 失败: " + ex.Message); return false; }
    }

    internal static string DebugModuleList() => string.Join(" -> ", Modules.Select(x => x.Name));
}
