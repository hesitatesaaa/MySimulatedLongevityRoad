using System;
using MySimulatedLongevityRoad.Core;
using MySimulatedLongevityRoad.Data;
using UnityEngine;

namespace MySimulatedLongevityRoad.Systems;

internal static class MclslAnnualBackpressure
{
    private static readonly MclslAnnualSpeedPolicy Policy = new();
    private static bool _worldBound;
    private static bool _patchAvailable;
    private static bool _reportedUnavailable;
    private static int _announcedLevel;
    private static float _nextStatus;
    private static float _rateStart;
    private static long _steps;
    private static long _years;
    private static double _firstEssenceWaitMax;
    private static long _progressSerial;
    private static double _overrunSeconds;
    internal static string StatusText { get; private set; } = string.Empty;
    internal static string MetricsText { get; private set; } = string.Empty;
    internal static float AppliedFactor => _worldBound && _patchAvailable && MclslRuntimeSettings.CoreEnabled
        && MclslRuntimeSettings.AnnualBackpressureEnabled && !IsBlocked ? EffectiveFactor : 1f;
    private static float EffectiveFactor => Policy.Factor;
    internal static bool IsBlocked => MclslWorldRunRepository.Current?.AnnualBatch?.WorldBlocked ?? false;
    internal static bool IsPauseLevel => EffectiveFactor <= 0f;
    internal static int LagYears => _worldBound ? Lag(out _, out _) : 0;
    internal static bool ShowStatus => MclslDeveloperBridge.IsAvailable && _worldBound
        && (Policy.Level > 0 || (!_patchAvailable && MclslRuntimeSettings.AnnualBackpressureEnabled));
    internal static void SetPatchAvailable(bool available)
    {
        _patchAvailable = available;
        if (!available) MclslDiagnostics.Error("annual-backpressure-patch", Text("MCLSL_annual_backlog_unavailable"));
    }
    internal static void InitializeAfterLoad() { Clear(); _worldBound = true; _rateStart = Time.unscaledTime; }
    internal static void Clear()
    {
        _worldBound = false; Policy.Reset(); _announcedLevel = 0; _nextStatus = 0f;
        _reportedUnavailable = false; _steps = 0; _years = 0; _firstEssenceWaitMax = 0;
        _progressSerial = 0; _overrunSeconds = 0;
        StatusText = string.Empty; MetricsText = string.Empty;
    }
    private static int Lag(out int current, out int completed)
    {
        current = MclslRuntime.CurrentYear();
        MclslAnnualBatchState batch = MclslWorldRunRepository.Current.AnnualBatch;
        completed = batch.LastCompletedYear;
        // A new architecture run has no completed annual cursor.
        if (completed == 0 && batch.ActiveYear == 0 && batch.LatestRequestedYear == 0)
            completed = Math.Max(0, current - 1);
        // Lag is measured against the world clock only. A stale "requested" year
        // left behind by a previous, later world (anchor / save rollback) must not
        // report a phantom backlog that keeps the world paused forever.
        return MclslAnnualYearReconcilePolicy.LagYears(current, completed, batch.ActiveYear, batch.LatestRequestedYear);
    }
    // 年差口径
    private static int LevelByLag(int lag) => lag >= 10 ? 3 : lag >= 6 ? 2 : lag >= 3 ? 1 : 0;
    // 耗时口径（持续超预算秒数）：仅辅助到 0.25x，永不触发暂停（暂停只由年差 lag≥10 触发）
    private static int LevelByOverrun(double s) => s >= 30d ? 2 : s >= 12d ? 1 : 0;
    internal static bool Apply(ref float elapsed)
    {
        // 档位统一在 TickStatus 里按“年差+耗时”算好，这里只按当前档位缩放模拟时间。
        // 年度管线被阻塞（WorldBlocked）时绝不能再把世界时间乘成 0：那会让一个
        // 已经停止结算的世界同时被冻住，形成只能读档才能解除的二次死锁。
        if (!(_worldBound && _patchAvailable && MclslRuntimeSettings.CoreEnabled
              && MclslRuntimeSettings.AnnualBackpressureEnabled && !IsBlocked)) return true;
        elapsed *= EffectiveFactor;
        return true;
    }
    internal static void RecordProgress() => _progressSerial++;
    internal static void RecordStep() { RecordProgress(); if (MclslPerformanceProbe.Enabled) _steps++; }
    internal static void RecordYear() { RecordProgress(); if (MclslPerformanceProbe.Enabled) _years++; }
    internal static void RecordFirstEssenceWait(double seconds)
    {
        if (MclslPerformanceProbe.Enabled) _firstEssenceWaitMax = Math.Max(_firstEssenceWaitMax, seconds);
    }
    internal static void TickStatus()
    {
        if (!_worldBound) return;
        bool enabled = MclslRuntimeSettings.CoreEnabled && MclslRuntimeSettings.AnnualBackpressureEnabled && !IsBlocked;
        int lag = Lag(out int current, out int completed);
        // 混合触发：年差 + 持续超预算耗时；耗时仅在 lag≥2 计入，且最高只到 0.25x，绝不暂停。
        bool underPressure = enabled && lag >= 2
            && (MclslScheduler.HasAnnualActorBacklog || MclslScheduler.HasAnnualCandidateBacklog)
            && (MclslAnnualFrameBudget.LastTimeExhausted || MclslAnnualFrameBudget.LastOperationsExhausted);
        _overrunSeconds = underPressure
            ? _overrunSeconds + Time.unscaledDeltaTime
            : Math.Max(0d, _overrunSeconds - Time.unscaledDeltaTime * 2d);
        int level = Math.Max(LevelByLag(lag), LevelByOverrun(_overrunSeconds));
        Policy.Update(level, enabled && _patchAvailable);
        if (!enabled) { _announcedLevel = 0; StatusText = string.Empty; return; }
        if (!_patchAvailable && !_reportedUnavailable)
        {
            _reportedUnavailable = true;
            if (MclslDeveloperBridge.IsAvailable)
                MclslAnnouncementSystem.Enqueue(Text("MCLSL_annual_backlog_unavailable"));
        }
        int announcementLevel = Policy.Level;
        bool changed = _announcedLevel != announcementLevel;
        if (Time.unscaledTime < _nextStatus && !changed) return;
        _nextStatus = Time.unscaledTime + 1f;
        StatusText = !_patchAvailable ? Text("MCLSL_annual_backlog_unavailable")
            : string.Format(Text("MCLSL_annual_backlog_status"), current, completed, lag,
                MclslScheduler.AnnualReadyCount, MclslScheduler.AnnualWaitingCount, EffectiveFactor);
        if (changed)
        {
            _announcedLevel = announcementLevel;
            string key = Policy.Level == 0 ? "MCLSL_annual_backlog_normal" : "MCLSL_annual_backlog_slow";
            if (MclslDeveloperBridge.IsAvailable)
                MclslAnnouncementSystem.Enqueue(Text(key) + "。 " + StatusText, "#E2C078");
        }
        if (MclslPerformanceProbe.Enabled && Time.unscaledTime > _rateStart)
        {
            double seconds = Time.unscaledTime - _rateStart;
            MetricsText = "annualSteps/s=" + (_steps / seconds).ToString("F1")
                + " annualYears/s=" + (_years / seconds).ToString("F3")
                + " ready=" + MclslScheduler.AnnualReadyCount + " waiting=" + MclslScheduler.AnnualWaitingCount
                + " longestWaitingYearGap=" + Math.Max(0, MclslScheduler.NewestWaitingYear - completed)
                + " firstEssenceWaitMaxSeconds=" + _firstEssenceWaitMax.ToString("F1");
            MclslAnnualBatchState batch = MclslWorldRunRepository.Current.AnnualBatch;
            MetricsText += " world/active/requested/completed=" + current + "/" + batch.ActiveYear
                + "/" + batch.LatestRequestedYear + "/" + batch.LastCompletedYear
                + " rawYearGap=" + Math.Max(0, current - batch.LastCompletedYear)
                + " completedCursorValid=" + (batch.LastCompletedYear > 0)
                + " policyLag=" + lag + " level=" + Policy.Level
                + " lagLevel=" + LevelByLag(lag) + " overrunLevel=" + LevelByOverrun(_overrunSeconds)
                + " overrunSeconds=" + _overrunSeconds.ToString("F2")
                + " budgetTime/ops=" + MclslAnnualFrameBudget.LastTimeExhausted + "/" + MclslAnnualFrameBudget.LastOperationsExhausted
                + " transitionSeed/actors=" + MclslWorldEpochSystem.PendingTransitionSeedCount + "/" + MclslWorldEpochSystem.PendingTransitionActorCount;
        }
        _steps = 0; _years = 0; _rateStart = Time.unscaledTime;
    }
    private static string Text(string key) => LocalizedTextManager.getText(key);
}
