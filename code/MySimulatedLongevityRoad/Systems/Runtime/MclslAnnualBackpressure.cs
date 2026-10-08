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
    private static long _lastObservedProgress;
    private static float _lastProgressAt;
    private static bool _fallbackToSlow;
    internal static string StatusText { get; private set; } = string.Empty;
    internal static string MetricsText { get; private set; } = string.Empty;
    internal static float AppliedFactor => _worldBound && _patchAvailable && MclslRuntimeSettings.CoreEnabled
        && MclslRuntimeSettings.AnnualBackpressureEnabled ? EffectiveFactor : 1f;
    private static float EffectiveFactor => Policy.Level == 3 && _fallbackToSlow ? 0.25f : Policy.Factor;
    internal static bool IsBlocked => MclslWorldRunRepository.Current.AnnualBatch.WorldBlocked;
    internal static int LagYears => _worldBound ? Lag(out _, out _) : 0;
    internal static bool ShowStatus => MclslDeveloperBridge.IsAvailable && _worldBound
        && (Policy.Level > 0 || (!_patchAvailable && MclslRuntimeSettings.AnnualBackpressureEnabled));
    internal static void SetPatchAvailable(bool available)
    {
        _patchAvailable = available;
        if (!available) MclslDiagnostics.Error("annual-backpressure-patch", Text("MCLSL_annual_backlog_unavailable"));
    }
    internal static void InitializeAfterLoad() { Clear(); _worldBound = true; _rateStart = _lastProgressAt = Time.unscaledTime; }
    internal static void Clear()
    {
        _worldBound = false; Policy.Reset(); _announcedLevel = 0; _nextStatus = 0f;
        _reportedUnavailable = false; _steps = 0; _years = 0; _firstEssenceWaitMax = 0;
        _progressSerial = _lastObservedProgress = 0; _lastProgressAt = 0f; _fallbackToSlow = false;
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
        return Math.Max(0, Math.Max(current, batch.LatestRequestedYear) - completed);
    }
    internal static bool Apply(ref float elapsed)
    {
        bool enabled = _worldBound && _patchAvailable && MclslRuntimeSettings.CoreEnabled
            && MclslRuntimeSettings.AnnualBackpressureEnabled;
        Policy.Update(enabled ? Lag(out _, out _) : 0, Time.unscaledTime, enabled);
        if (!enabled) return true;
        if (EffectiveFactor == 0f) return false;
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
        bool enabled = MclslRuntimeSettings.CoreEnabled && MclslRuntimeSettings.AnnualBackpressureEnabled;
        int lag = Lag(out int current, out int completed);
        // Also permits recovery while the native simulation is paused by a window.
        Policy.Update(lag, Time.unscaledTime, enabled && _patchAvailable);
        UpdateRecoveryWatchdog(lag);
        if (!enabled) { _announcedLevel = 0; _fallbackToSlow = false; StatusText = string.Empty; return; }
        if (!_patchAvailable && !_reportedUnavailable)
        {
            _reportedUnavailable = true;
            if (MclslDeveloperBridge.IsAvailable)
                MclslAnnouncementSystem.Enqueue(Text("MCLSL_annual_backlog_unavailable"));
        }
        int announcementLevel = _fallbackToSlow && Policy.Level == 3 ? 4 : Policy.Level;
        bool changed = _announcedLevel != announcementLevel;
        if (Time.unscaledTime < _nextStatus && !changed) return;
        _nextStatus = Time.unscaledTime + 1f;
        StatusText = !_patchAvailable ? Text("MCLSL_annual_backlog_unavailable")
            : string.Format(Text("MCLSL_annual_backlog_status"), current, completed, lag,
                MclslScheduler.AnnualReadyCount, MclslScheduler.AnnualWaitingCount, EffectiveFactor);
        if (changed)
        {
            _announcedLevel = announcementLevel;
            string key = EffectiveFactor == 0f ? "MCLSL_annual_backlog_paused"
                : Policy.Level == 0 ? "MCLSL_annual_backlog_normal" : "MCLSL_annual_backlog_slow";
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
        }
        _steps = 0; _years = 0; _rateStart = Time.unscaledTime;
    }
    private static void UpdateRecoveryWatchdog(int lag)
    {
        float now = Time.unscaledTime;
        if (Policy.Level < 3)
        {
            _lastObservedProgress = _progressSerial;
            _lastProgressAt = now;
            if (lag <= 1 && Policy.Level == 0) _fallbackToSlow = false;
            return;
        }
        if (_lastObservedProgress != _progressSerial)
        {
            _lastObservedProgress = _progressSerial;
            _lastProgressAt = now;
        }
        if (_fallbackToSlow) return;
        string reason = IsBlocked ? "年度任务异常阻断" : now - _lastProgressAt >= 10f ? "连续10秒未结算" : null;
        if (reason == null) return;
        _fallbackToSlow = true;
        MclslDiagnostics.Error("annual-backpressure-fallback", reason + "，本轮积压由零速改为0.25倍兜底速度");
    }
    private static string Text(string key) => LocalizedTextManager.getText(key);
}
