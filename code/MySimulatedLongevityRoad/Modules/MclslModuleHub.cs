using System;
using System.Collections.Generic;
using System.Linq;
using MySimulatedLongevityRoad.Core;
using MySimulatedLongevityRoad.Queries;

namespace MySimulatedLongevityRoad.Modules;

internal static class MclslModuleHub
{
    private sealed class AnnualWorkItem
    {
        internal int Year;
        internal bool CoreEnabled;
    }
    private static readonly List<MclslModuleBase> Modules = new();
    private static readonly Dictionary<MclslModuleBase, AnnualWorkItem> PendingAnnualWork = new();
    private static readonly Queue<MclslModuleBase> LoadRecoveryQueue = new();
    private static bool _initialized;
    private static int _loadRecoveryYear;
    private static bool _loadRecoveryCoreEnabled;

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
        Register(new MclslInverseTruthModule());
        Register(new MclslFactionMissionModule());
        Register(new MclslFactionPressureModule());
        Register(new MclslRuntimeCadenceModule());
        Register(new MclslAnnouncementModule());
        Register(new MclslArchiveMaintenanceModule());
        Modules.Sort((a, b) => a.Order != b.Order ? a.Order.CompareTo(b.Order) : string.CompareOrdinal(a.Name, b.Name));
        ForEach(true, module => module.Init(), "Init");
    }

    internal static void OnWorldLoaded(int year, bool coreEnabled)
    {
        PendingAnnualWork.Clear();
        LoadRecoveryQueue.Clear();
        ForEach(coreEnabled, module => module.OnWorldLoaded(year), "OnWorldLoaded");
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
    internal static void TickFrame(int frameCounter, bool coreEnabled)
    {
        ForEach(coreEnabled, module => module.TickFrame(frameCounter), "TickFrame");
        DrainLoadRecoveryQueue();
        DrainAnnualQueue();
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
            pending.Year = Math.Max(pending.Year, year);
            pending.CoreEnabled = coreEnabled;
            return;
        }
        PendingAnnualWork[module] = new AnnualWorkItem
        {
            Year = year,
            CoreEnabled = coreEnabled
        };
    }

    private static void DeferredLoadRecoveryStep(MclslModuleBase module)
    {
        if (module != null) LoadRecoveryQueue.Enqueue(module);
    }

    private static void DrainAnnualQueue()
    {
        const int annualModuleBudgetPerFrame = 2;
        for (int step = 0; step < annualModuleBudgetPerFrame && PendingAnnualWork.Count > 0; step++)
        {
            MclslModuleBase module = null;
            for (int i = 0; i < Modules.Count; i++)
            {
                MclslModuleBase candidate = Modules[i];
                if (PendingAnnualWork.ContainsKey(candidate))
                {
                    module = candidate;
                    break;
                }
            }
            if (module == null || !PendingAnnualWork.TryGetValue(module, out AnnualWorkItem work)) break;
            PendingAnnualWork.Remove(module);
            if (!work.CoreEnabled && !module.RunsWhenCoreDisabled) continue;
            SafeAnnualStep(module, work.Year);
        }
    }

    private static void DrainLoadRecoveryQueue()
    {
        const int loadRecoveryModuleBudgetPerFrame = 1;
        for (int i = 0; i < loadRecoveryModuleBudgetPerFrame && LoadRecoveryQueue.Count > 0; i++)
        {
            MclslModuleBase module = LoadRecoveryQueue.Dequeue();
            if (!_loadRecoveryCoreEnabled && !module.RunsWhenCoreDisabled) continue;
            SafeLoadRecoveryStep(module, _loadRecoveryYear);
        }
    }

    private static void SafeAnnualStep(MclslModuleBase module, int year)
    {
        try { module?.TickAnnual(year); }
        catch (Exception ex) { MclslDiagnostics.Error("module:" + module?.Name + ":TickAnnual", module?.Name + ".TickAnnual 失败: " + ex.Message); }
    }

    private static void SafeLoadRecoveryStep(MclslModuleBase module, int year)
    {
        try { module?.TickLoadRecovery(year); }
        catch (Exception ex) { MclslDiagnostics.Error("module:" + module?.Name + ":TickLoadRecovery", module?.Name + ".TickLoadRecovery 失败: " + ex.Message); }
    }

    internal static string DebugModuleList() => string.Join(" -> ", Modules.Select(x => x.Name));
}
