using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using MySimulatedLongevityRoad.Data;
using MySimulatedLongevityRoad.Systems;
using UnityEngine;

namespace MySimulatedLongevityRoad.Core;

/// <summary>Opt-in, bounded diagnostics available in the release package.</summary>
internal static class MclslPerformanceProbe
{
    private const int RecentLaneSamples = 1024;
    private const int FrameHistoryCapacity = 4096;
    private const int ContextRefreshFrames = 60;

    private sealed class SpeedFrames
    {
        internal long Count, Over25;
        internal double TotalMs;
        internal float MaximumMs;
        // 0.1 ms upper-bound buckets, last bucket is overflow (>= 100 ms).
        internal readonly long[] Histogram = new long[1001];
        internal void Add(float ms)
        {
            Count++; TotalMs += ms; if (ms > 25f) Over25++;
            MaximumMs = Math.Max(MaximumMs, ms);
            Histogram[Math.Min(1000, (int)Math.Ceiling(ms * 10d))]++;
        }
        internal string Percentile(double percentile)
        {
            long target = (long)Math.Ceiling(Count * percentile), total = 0;
            for (int i = 0; i < Histogram.Length; i++)
            {
                total += Histogram[i];
                if (total >= target) return i == 1000 ? ">=100" : (i / 10d).ToString("F1");
            }
            return "不可用";
        }
    }
    private static readonly Dictionary<(string Id, float Multiplier, int Ticks, bool Sonic), SpeedFrames> SpeedWindows = new();
    private static int _saveFrame = -2;
    private static long _excludedSaveFrames, _excludedPausedFrames;
    internal static void MarkSaveFrame() { if (Enabled) _saveFrame = Time.frameCount; }

    private sealed class Sample
    {
        internal long Ticks;
        internal long MaximumTicks;
        internal long CurrentFrameTicks;
        internal long MaximumFrameTicks;
        internal long AllocatedBytes, MaximumAllocatedBytes;
        internal int AllocationCalls;
        internal int Calls;
        internal readonly long[] RecentTicks = new long[RecentLaneSamples];
        internal int RecentCount;
        internal int RecentCursor;
        internal int QuantileCalls;
        internal long P95, P99;
        internal bool AcrossFrames;

        internal void Add(long ticks)
        {
            Ticks += ticks;
            if (ticks > MaximumTicks) MaximumTicks = ticks;
            if (!AcrossFrames) CurrentFrameTicks += ticks;
            Calls++;
            RecentTicks[RecentCursor] = ticks;
            RecentCursor = (RecentCursor + 1) % RecentTicks.Length;
            if (RecentCount < RecentTicks.Length) RecentCount++;
        }
    }

    private struct FrameContext
    {
        internal int Frame;
        internal int Year;
        internal float FrameMilliseconds;
        internal float ModMilliseconds;
        internal int Population;
        internal int Cultivators;
        internal int AnnualQueue;
        internal int AnnualStates;
        internal int CandidateRemaining;
        internal int TransitionRemaining;
        internal int MarketListings;
        internal int ChangeQueue;
        internal int InventoryWrites;
        internal int InventoryCache;
        internal int RankCache;
        internal int ProjectionCache;
        internal int DelayedHealing;
        internal int PendingListings;
        internal int DiagnosticCache;
        internal int LatestRequestedYear;
        internal int OldestPendingYear;
        internal string Era;
        internal string WorldStage;
        internal string TopLane;
        internal float TopLaneMilliseconds;
        internal float Speed;
        internal string NativeSpeedId;
        internal float NativeMultiplier, BackpressureFactor;
        internal int NativeTicks;
    }

    private static readonly Dictionary<string, Sample> Samples = new(StringComparer.Ordinal);
    private static readonly Dictionary<long, long> AllocationStarts = new(1024);
    private static readonly List<Sample> QuantileSamples = new(512);
    private static readonly long[] QuantileScratch = new long[RecentLaneSamples];
    private static readonly float[] FrameQuantileScratch = new float[FrameHistoryCapacity];
    private static int _quantileCursor;
    private static double _frameP95, _frameP99;
    private static long _lastScopeToken;
    private static readonly StringBuilder Buffer = new(4096);
    private static readonly float[] FrameMilliseconds = new float[FrameHistoryCapacity];
    private static readonly FrameContext[] FrameContexts = new FrameContext[FrameHistoryCapacity];
    private static Func<long> _allocatedBytesReader;
    private static bool _allocationReaderResolved;
    private static int _sampleStartFrame;
    private static int _sampleStartGc0;
    private static int _sampleStartGc1;
    private static int _lastSampledFrame = -1;
    private static long _lastAllocatedBytes = -1L;
    private static long _frameAllocationTotal;
    private static long _frameAllocationPeak;
    private static int _allocationFrameCount;
    private static int _frameHistoryCursor;
    private static long _frameTimeTotalTicks;
    private static long _validFrameSamples;
    private static float _minimumFps;
    private static float _maximumFrameMilliseconds;
    private static long _framesOver16_7;
    private static long _framesOver25;
    private static long _framesOver33;
    private static long _framesOver50;
    private static int _latestRequestedYear;
    private static int _oldestPendingYear;
    private static int _lastYearSpanRefreshFrame = -ContextRefreshFrames;
    private static string _summaryCache = string.Empty;
    private static long _currentFrameModTicks;
    private static long _combatSpellFastRejects;
    private static long _combatBagSingleScans;
    private static long _marketOffersInspected;
    private static long _cultivatorPollActors;
    private static int _archiveMainBytes;
    private static int _archiveBackupBytes;

    internal static bool Enabled { get; private set; }

    internal static void SetEnabled(bool enabled)
    {
        Enabled = enabled;
        Samples.Clear(); AllocationStarts.Clear(); _lastScopeToken = 0;
        SpeedWindows.Clear(); _saveFrame = -2; _excludedSaveFrames = _excludedPausedFrames = 0;
        QuantileSamples.Clear(); _quantileCursor = 0; _frameP95 = _frameP99 = 0;
        Array.Clear(FrameMilliseconds, 0, FrameMilliseconds.Length);
        Array.Clear(FrameContexts, 0, FrameContexts.Length);
        _sampleStartFrame = Time.frameCount;
        _sampleStartGc0 = GC.CollectionCount(0);
        _sampleStartGc1 = GC.CollectionCount(1);
        _lastSampledFrame = -1;
        _frameAllocationTotal = 0L;
        _frameAllocationPeak = 0L;
        _allocationFrameCount = 0;
        _frameHistoryCursor = 0;
        _frameTimeTotalTicks = 0L;
        _validFrameSamples = 0L;
        _framesOver16_7 = 0L;
        _framesOver25 = 0L;
        _framesOver33 = 0L;
        _framesOver50 = 0L;
        _minimumFps = float.PositiveInfinity;
        _maximumFrameMilliseconds = 0f;
        _latestRequestedYear = 0;
        _oldestPendingYear = 0;
        _lastYearSpanRefreshFrame = -ContextRefreshFrames;
        _summaryCache = string.Empty;
        _currentFrameModTicks = 0L;
        _combatSpellFastRejects = 0L;
        _combatBagSingleScans = 0L;
        _marketOffersInspected = 0L;
        _cultivatorPollActors = 0L;
        _archiveMainBytes = 0;
        _archiveBackupBytes = 0;
        _lastAllocatedBytes = enabled ? ReadAllocatedBytes() : -1L;
    }

    internal static void RecordCombatSpellFastReject()
    {
        if (Enabled) _combatSpellFastRejects++;
    }

    internal static void RecordCombatBagSingleScan()
    {
        if (Enabled) _combatBagSingleScans++;
    }

    internal static void RecordMarketOffersInspected(int count)
    {
        if (Enabled && count > 0) _marketOffersInspected += count;
    }

    internal static void RecordCultivatorPollActors(int count)
    {
        if (Enabled && count > 0) _cultivatorPollActors += count;
    }

    internal static void RecordArchiveBytes(int mainBytes, int backupBytes)
    {
        if (!Enabled) return;
        _archiveMainBytes = Math.Max(0, mainBytes);
        _archiveBackupBytes = Math.Max(0, backupBytes);
    }

    internal static long Begin()
    {
        if (!Enabled) return 0;
        long token = Math.Max(Stopwatch.GetTimestamp(), _lastScopeToken + 1);
        _lastScopeToken = token;
        long bytes = ReadAllocatedBytes();
        if (bytes >= 0)
        {
            // Faulted scopes cannot accumulate forever. Existing CPU durations
            // remain valid if an abandoned allocation scope is discarded.
            if (AllocationStarts.Count >= 1024) AllocationStarts.Clear();
            AllocationStarts[token] = bytes;
        }
        return token;
    }

    internal static void End(string lane, long started, bool acrossFrames = false)
    {
        if (!Enabled || started == 0L || string.IsNullOrWhiteSpace(lane)) return;
        if (!Samples.TryGetValue(lane, out Sample sample))
        {
            if (Samples.Count >= 512) { AllocationStarts.Remove(started); return; }
            Samples[lane] = sample = new Sample();
            QuantileSamples.Add(sample);
        }
        sample.AcrossFrames = acrossFrames;
        if (AllocationStarts.TryGetValue(started, out long before))
        {
            AllocationStarts.Remove(started);
            long current = ReadAllocatedBytes();
            if (!acrossFrames && current >= before)
            {
                long allocated = current - before;
                sample.AllocatedBytes += allocated; sample.AllocationCalls++;
                if (allocated > sample.MaximumAllocatedBytes) sample.MaximumAllocatedBytes = allocated;
            }
        }
        long elapsed = Math.Max(0L, Stopwatch.GetTimestamp() - started);
        sample.Add(elapsed);
        if (string.Equals(lane, "模组每帧CPU", StringComparison.Ordinal))
            _currentFrameModTicks += elapsed;
    }

    internal static void SampleFrame()
    {
        if (!Enabled) return;
        int frame = Time.frameCount;
        if (_lastSampledFrame == frame) return;
        _lastSampledFrame = frame;

        float delta = Time.unscaledDeltaTime;
        float milliseconds = delta > 0f && !float.IsNaN(delta) && !float.IsInfinity(delta)
            ? delta * 1000f
            : 0f;
        if (milliseconds > 0f)
        {
            if (frame == _saveFrame || frame == _saveFrame + 1) _excludedSaveFrames++;
            else if (Config.paused) _excludedPausedFrames++;
            else
            {
                var speed = Config.time_scale_asset;
                var key = (speed?.id ?? "未设置", speed?.multiplier ?? 0f, speed?.ticks ?? 0, speed?.sonic ?? false);
                if (!SpeedWindows.TryGetValue(key, out SpeedFrames window) && SpeedWindows.Count < 32)
                    SpeedWindows[key] = window = new SpeedFrames();
                window?.Add(milliseconds);
            }
            float fps = 1000f / milliseconds;
            if (fps < _minimumFps) _minimumFps = fps;
            if (milliseconds > _maximumFrameMilliseconds) _maximumFrameMilliseconds = milliseconds;
            _frameTimeTotalTicks += (long)(milliseconds * Stopwatch.Frequency / 1000f);
            _validFrameSamples++;
            if (milliseconds > 16.7f) _framesOver16_7++;
            if (milliseconds > 25f) _framesOver25++;
            if (milliseconds > 33f) _framesOver33++;
            if (milliseconds > 50f) _framesOver50++;
        }
        FrameMilliseconds[_frameHistoryCursor] = milliseconds;
        RefreshQuantiles();

        long allocated = ReadAllocatedBytes();
        if (_lastAllocatedBytes >= 0L && allocated >= _lastAllocatedBytes)
        {
            long frameAllocation = allocated - _lastAllocatedBytes;
            _frameAllocationTotal += frameAllocation;
            if (frameAllocation > _frameAllocationPeak) _frameAllocationPeak = frameAllocation;
            _allocationFrameCount++;
        }
        _lastAllocatedBytes = allocated;

        string topLane = string.Empty;
        long topLaneTicks = 0L;
        foreach (KeyValuePair<string, Sample> pair in Samples)
        {
            Sample sample = pair.Value;
            long frameTicks = sample.CurrentFrameTicks;
            sample.CurrentFrameTicks = 0L;
            if (frameTicks > sample.MaximumFrameTicks) sample.MaximumFrameTicks = frameTicks;
            if (!string.Equals(pair.Key, "模组每帧CPU", StringComparison.Ordinal) && frameTicks > topLaneTicks)
            {
                topLane = pair.Key;
                topLaneTicks = frameTicks;
            }
        }

        if (frame % ContextRefreshFrames == 0 || _lastYearSpanRefreshFrame < 0)
        {
            MclslScheduler.GetAnnualYearSpan(out _latestRequestedYear, out _oldestPendingYear);
            _lastYearSpanRefreshFrame = frame;
        }

        int year = MclslRuntime.CurrentYear();
        string era = ResolveEra(year);
        int population = -1;
        try { population = World.world?.units?.getSimpleList()?.Count ?? 0; } catch { }
        FrameContexts[_frameHistoryCursor] = new FrameContext
        {
            Frame = frame,
            Year = year,
            FrameMilliseconds = milliseconds,
            ModMilliseconds = _currentFrameModTicks * 1000f / Stopwatch.Frequency,
            Population = population,
            Cultivators = MclslCultivatorCandidateIndex.CultivatorCount,
            AnnualQueue = MclslScheduler.AnnualActorBacklogCount,
            AnnualStates = MclslScheduler.AnnualActorStateCount,
            CandidateRemaining = MclslScheduler.AnnualCandidateScanRemaining,
            TransitionRemaining = MclslWorldEpochSystem.PendingTransitionWorkCount,
            MarketListings = MclslTianxuanMarket.ActiveListingCount,
            ChangeQueue = MclslRuntimeChanges.Count,
            InventoryWrites = MclslBagSystem.PendingWriteCount,
            InventoryCache = MclslBagSystem.CacheCount,
            RankCache = MySimulatedLongevityRoad.UI.MclslRankSnapshotSource.Count,
            ProjectionCache = MclslActorProjectionIndex.Count,
            DelayedHealing = MclslItemEffectDriver.PendingCount,
            PendingListings = MclslTianxuanMarket.PendingListingCount,
            DiagnosticCache = MclslDiagnostics.CacheCount,
            LatestRequestedYear = _latestRequestedYear,
            OldestPendingYear = _oldestPendingYear,
            Era = era,
            WorldStage = MclslAnnualWorldRuntimeLane.CurrentStageName,
            TopLane = topLane,
            TopLaneMilliseconds = topLaneTicks * 1000f / Stopwatch.Frequency,
            Speed = Time.timeScale,
            NativeSpeedId = Config.time_scale_asset?.id ?? "未设置",
            NativeMultiplier = Config.time_scale_asset?.multiplier ?? 0,
            NativeTicks = Config.time_scale_asset?.ticks ?? 0,
            BackpressureFactor = MclslAnnualBackpressure.AppliedFactor
        };
        _frameHistoryCursor = (_frameHistoryCursor + 1) % FrameContexts.Length;
        _currentFrameModTicks = 0L;
    }

    internal static string Summary(bool forceRefresh = false)
    {
        if (!Enabled) return "性能采样未开启。";
        if (!forceRefresh) return "性能采样运行中；停止采样后生成详细报告。";
        _summaryCache = BuildSummary();
        return _summaryCache;
    }

    private static string BuildSummary()
    {
        Buffer.Length = 0;
        int frames = Math.Max(0, Time.frameCount - _sampleStartFrame);
        double frameAverage = _validFrameSamples > 0
            ? FrameMillisecondsAverage()
            : 0d;
        Buffer.Append("采样 ").Append(frames).Append(" 帧；当前 ")
            .Append(Mathf.RoundToInt(1f / Mathf.Max(0.0001f, Time.unscaledDeltaTime))).Append(" FPS；最低 ")
            .Append(float.IsPositiveInfinity(_minimumFps) ? 0f : _minimumFps).Append(" FPS；纪元/年份 ")
            .Append(ResolveEra(MclslRuntime.CurrentYear())).Append('/').Append(MclslRuntime.CurrentYear())
            .Append("；Unity时间倍率 ").Append(Time.timeScale.ToString("F2")).Append('x')
            .Append("；人口/修士 ").Append(ReadPopulationCount()).Append('/')
            .Append(MclslCultivatorCandidateIndex.CultivatorCount).Append("\n")
            .Append("年度队列/状态 ").Append(MclslScheduler.AnnualActorBacklogCount).Append('/')
            .Append(MclslScheduler.AnnualActorStateCount).Append("；请求年/最旧未完成年 ")
            .Append(_latestRequestedYear).Append('/').Append(_oldestPendingYear)
            .Append("；候选剩余 ");
        int candidateRemaining = MclslScheduler.AnnualCandidateScanRemaining;
        Buffer.Append(candidateRemaining < 0 ? "索引刷新中" : candidateRemaining.ToString())
            .Append("；新法转换剩余 ").Append(MclslWorldEpochSystem.PendingTransitionWorkCount)
            .Append("；年度世界阶段 ").Append(MclslAnnualWorldRuntimeLane.CurrentStageName)
            .Append("；天玄镜挂单 ").Append(MclslTianxuanMarket.ActiveListingCount).Append('\n')
            ;
        Buffer.Append("天玄镜已检查挂单 ").Append(_marketOffersInspected)
            .Append("；未完成采购批次 ").Append(MclslTianxuanMarket.PendingPurchaseBatchCount).Append('\n');
        Buffer
            .Append("战斗法术快速拒绝/符箓背包单扫/修士轮询人数 ")
            .Append(_combatSpellFastRejects).Append('/').Append(_combatBagSingleScans).Append('/')
            .Append(_cultivatorPollActors).Append("；主档/备份字节 ")
            .Append(_archiveMainBytes).Append('/').Append(_archiveBackupBytes).Append('\n')
            .Append("帧时 ms 全程平均/最近4096帧P95/P99/全程最大：").Append(frameAverage.ToString("F2"))
            .Append('/').Append(FramePercentile(0.95).ToString("F2"))
            .Append('/').Append(FramePercentile(0.99).ToString("F2"))
            .Append('/').Append(_maximumFrameMilliseconds.ToString("F2"))
            .Append("；卡顿帧 >16.7/25/33/50ms：").Append(_framesOver16_7).Append('/')
            .Append(_framesOver25).Append('/').Append(_framesOver33).Append('/').Append(_framesOver50).Append('\n')
            .Append("GC0/GC1 增量 ").Append(GC.CollectionCount(0) - _sampleStartGc0).Append('/')
            .Append(GC.CollectionCount(1) - _sampleStartGc1).Append("；GC Alloc 均值/峰值 ");
        if (_allocationFrameCount > 0)
            Buffer.Append((_frameAllocationTotal / _allocationFrameCount).ToString())
                .Append('/').Append(_frameAllocationPeak).Append(" B/帧");
        else Buffer.Append("不可用");
        Buffer.Append("\n原生速度档位 ").Append(Config.time_scale_asset?.id ?? "未设置")
            .Append("；multiplier/ticks/sonic ").Append(Config.time_scale_asset?.multiplier ?? 0).Append('/')
            .Append(Config.time_scale_asset?.ticks ?? 0).Append('/').Append(Config.time_scale_asset?.sonic ?? false)
            .Append("；模组世界速度系数 ").Append(MclslAnnualBackpressure.AppliedFactor.ToString("F2"))
            .Append("\n").Append(MclslAnnualBackpressure.MetricsText);
        Buffer.Append("\n按原生速度分组的全程帧时（P95/P99为0.1ms上界；排除已标记存档帧及后一帧、暂停帧）：\n");
        foreach (var pair in SpeedWindows)
        {
            SpeedFrames window = pair.Value;
            Buffer.Append(pair.Key.Id).Append(" multiplier=").Append(pair.Key.Multiplier)
                .Append(" ticks=").Append(pair.Key.Ticks).Append(" sonic=").Append(pair.Key.Sonic)
                .Append(" frames=").Append(window.Count)
                .Append(" averageFPS=").Append((window.TotalMs > 0 ? window.Count * 1000d / window.TotalMs : 0).ToString("F2"))
                .Append(" P95/P99ms=").Append(window.Percentile(.95)).Append('/').Append(window.Percentile(.99))
                .Append(" over25ms=").Append(window.Over25).Append(" maxMs=").Append(window.MaximumMs.ToString("F2")).Append('\n');
        }
        Buffer.Append("排除的存档相关/暂停帧：").Append(_excludedSaveFrames).Append('/').Append(_excludedPausedFrames)
            .Append("；其他原版加载/菜单长帧仍保留，实机验收应单独开始采样。\n");
        Buffer.Append("\n\n总耗时前十模块\n");

        Buffer.Append("\n变化队列/背包待写入 ").Append(MclslRuntimeChanges.Count).Append('/')
            .Append(MclslBagSystem.PendingWriteCount).Append("；缓存：人物/职业纪元/排行/背包 ")
            .Append(MclslActorRegistry.Count).Append('/').Append(MclslActorProjectionIndex.Count).Append('/')
            .Append(MySimulatedLongevityRoad.UI.MclslRankSnapshotSource.Count).Append('/').Append(MclslBagSystem.CacheCount)
            .Append("\n");
        List<KeyValuePair<string, Sample>> ranked = new(Samples.Count);
        foreach (KeyValuePair<string, Sample> pair in Samples)
            if (!pair.Value.AcrossFrames && !string.Equals(pair.Key, "模组每帧CPU", StringComparison.Ordinal)) ranked.Add(pair);
        ranked.Sort((left, right) => right.Value.Ticks.CompareTo(left.Value.Ticks));
        AppendLaneTable(ranked, useFramePeak: false);

        ranked.Sort((left, right) => right.Value.MaximumFrameTicks.CompareTo(left.Value.MaximumFrameTicks));
        Buffer.Append("\n最大单帧峰值前十模块\n");
        AppendLaneTable(ranked, useFramePeak: true);

        Buffer.Append("\n各模块：调用/总ms/均ms/单次最大ms/P95/P99/单帧峰值ms；GC Alloc 均值/单次峰值 B（含子调用）\n");
        ranked.Sort((left, right) => right.Value.Ticks.CompareTo(left.Value.Ticks));
        for (int i = 0; i < ranked.Count; i++)
        {
            KeyValuePair<string, Sample> pair = ranked[i];
            Sample sample = pair.Value;
            Buffer.Append(pair.Key).Append('：').Append(sample.Calls).Append('/')
                .Append(ToMilliseconds(sample.Ticks).ToString("F2")).Append('/')
                .Append((sample.Calls == 0 ? 0d : ToMilliseconds(sample.Ticks) / sample.Calls).ToString("F3")).Append('/')
                .Append(ToMilliseconds(sample.MaximumTicks).ToString("F2")).Append('/')
                .Append(ToMilliseconds(Percentile(sample, 0.95)).ToString("F2")).Append('/')
                .Append(ToMilliseconds(Percentile(sample, 0.99)).ToString("F2")).Append('/')
                .Append(ToMilliseconds(sample.MaximumFrameTicks).ToString("F2")).Append(" ms；GC Alloc ")
                .Append(sample.AllocationCalls > 0 ? (sample.AllocatedBytes / sample.AllocationCalls).ToString() : "不可用")
                .Append('/').Append(sample.AllocationCalls > 0 ? sample.MaximumAllocatedBytes.ToString() : "不可用").Append(" B\n");
        }

        if (Samples.TryGetValue("模组每帧CPU", out Sample modSample))
        {
            Buffer.Append("模组主循环CPU：").Append(modSample.Calls).Append(" 次，总/均/最大/P95/P99/单帧峰值 ")
                .Append(ToMilliseconds(modSample.Ticks).ToString("F2")).Append('/')
                .Append((modSample.Calls == 0 ? 0d : ToMilliseconds(modSample.Ticks) / modSample.Calls).ToString("F3")).Append('/')
                .Append(ToMilliseconds(modSample.MaximumTicks).ToString("F2")).Append('/')
                .Append(ToMilliseconds(Percentile(modSample, 0.95)).ToString("F2")).Append('/')
                .Append(ToMilliseconds(Percentile(modSample, 0.99)).ToString("F2")).Append('/')
                .Append(ToMilliseconds(modSample.MaximumFrameTicks).ToString("F2")).Append(" ms\n");
        }

        Buffer.Append("\n跨帧完成延迟（包含等待，不计入单帧CPU或模块GC）\n");
        foreach (KeyValuePair<string, Sample> pair in Samples)
        {
            Sample sample = pair.Value;
            if (!sample.AcrossFrames) continue;
            Buffer.Append(pair.Key).Append("：完成次数 ").Append(sample.Calls)
                .Append("；平均/最大/P95/P99 ")
                .Append((sample.Calls == 0 ? 0d : ToMilliseconds(sample.Ticks) / sample.Calls).ToString("F2")).Append('/')
                .Append(ToMilliseconds(sample.MaximumTicks).ToString("F2")).Append('/')
                .Append(ToMilliseconds(Percentile(sample, 0.95)).ToString("F2")).Append('/')
                .Append(ToMilliseconds(Percentile(sample, 0.99)).ToString("F2")).Append(" ms\n");
        }
        AppendWorstFrames();
        return Buffer.ToString();
    }

    private static void AppendLaneTable(List<KeyValuePair<string, Sample>> ranked, bool useFramePeak)
    {
        int count = Math.Min(10, ranked.Count);
        for (int i = 0; i < count; i++)
        {
            KeyValuePair<string, Sample> pair = ranked[i];
            long ticks = useFramePeak ? pair.Value.MaximumFrameTicks : pair.Value.Ticks;
            Buffer.Append(i + 1).Append(". ").Append(pair.Key).Append("：")
                .Append(ToMilliseconds(ticks).ToString("F2")).Append(" ms；调用 ")
                .Append(pair.Value.Calls).Append("；单次最大 ")
                .Append(ToMilliseconds(pair.Value.MaximumTicks).ToString("F2")).Append(" ms\n");
        }
        if (count == 0) Buffer.Append("暂无样本\n");
    }

    private static void AppendWorstFrames()
    {
        List<FrameContext> worst = new(10);
        for (int i = 0; i < FrameHistoryCapacity; i++)
        {
            FrameContext sample = FrameContexts[i];
            if (sample.Frame <= 0 || sample.FrameMilliseconds <= 0f) continue;
            int insert = worst.Count;
            while (insert > 0 && worst[insert - 1].FrameMilliseconds < sample.FrameMilliseconds) insert--;
            if (insert >= 10) continue;
            if (worst.Count == 10) worst.RemoveAt(9);
            worst.Insert(insert, sample);
        }
        Buffer.Append("\n最差帧上下文：帧/帧ms/模组CPUms/纪元年份/倍速/人口修士/队列状态/年度阶段/最耗时模块\n");
        for (int i = 0; i < worst.Count; i++)
        {
            FrameContext sample = worst[i];
            int yearGap = sample.LatestRequestedYear > 0 && sample.OldestPendingYear > 0
                ? Math.Max(0, sample.LatestRequestedYear - sample.OldestPendingYear)
                : 0;
            Buffer.Append(sample.Frame).Append('/').Append(sample.FrameMilliseconds.ToString("F2")).Append('/')
                .Append(sample.ModMilliseconds.ToString("F2")).Append('/')
                .Append(sample.Era).Append(' ').Append(sample.Year).Append('/')
                .Append(sample.Speed.ToString("F2")).Append("x native=").Append(sample.NativeSpeedId)
                .Append(' ').Append(sample.NativeMultiplier.ToString("F2")).Append('*').Append(sample.NativeTicks)
                .Append(" protect=").Append(sample.BackpressureFactor.ToString("F2")).Append('/')
                .Append(sample.Population).Append(' ').Append(sample.Cultivators).Append('/')
                .Append(sample.AnnualQueue).Append(' ').Append(sample.AnnualStates).Append(" gap=").Append(yearGap).Append('/')
                .Append("changes=").Append(sample.ChangeQueue).Append(" writes=").Append(sample.InventoryWrites)
                .Append(" bags=").Append(sample.InventoryCache).Append(" ranks=").Append(sample.RankCache)
                .Append(" projections=").Append(sample.ProjectionCache)
                .Append(" healing=").Append(sample.DelayedHealing).Append(" pendingListings=").Append(sample.PendingListings)
                .Append(" diagnostics=").Append(sample.DiagnosticCache).Append('/')
                .Append(sample.WorldStage).Append(" 候选=")
                .Append(sample.CandidateRemaining < 0 ? "刷新中" : sample.CandidateRemaining.ToString())
                .Append(" conv=").Append(sample.TransitionRemaining).Append(" listings=").Append(sample.MarketListings).Append('/')
                .Append(sample.TopLane).Append(' ').Append(sample.TopLaneMilliseconds.ToString("F2")).Append("ms\n");
        }
        if (worst.Count == 0) Buffer.Append("暂无帧样本\n");
    }

    private static double FrameMillisecondsAverage() => _validFrameSamples <= 0
        ? 0d
        : _frameTimeTotalTicks * 1000d / Stopwatch.Frequency / _validFrameSamples;

    private static void RefreshQuantiles()
    {
        // Dashboard reads cached percentiles. At most four lane rings are sorted
        // in one sampling frame, using the same persistent scratch allocation.
        long deadline = Stopwatch.GetTimestamp() + Stopwatch.Frequency * 3 / 10000;
        for (int i = 0; i < 4 && QuantileSamples.Count > 0 && Stopwatch.GetTimestamp() < deadline; i++)
        {
            if (_quantileCursor >= QuantileSamples.Count) _quantileCursor = 0;
            Sample sample = QuantileSamples[_quantileCursor++];
            int count = sample.RecentCount;
            if (count == 0 || sample.QuantileCalls == sample.Calls) continue;
            Array.Copy(sample.RecentTicks, QuantileScratch, count);
            Array.Sort(QuantileScratch, 0, count);
            sample.P95 = QuantileScratch[Math.Clamp((int)Math.Ceiling(.95 * count) - 1, 0, count - 1)];
            sample.P99 = QuantileScratch[Math.Clamp((int)Math.Ceiling(.99 * count) - 1, 0, count - 1)];
            sample.QuantileCalls = sample.Calls;
        }
        if (Time.frameCount % 120 != 0 && _frameP95 > 0) return;
        int frames = 0;
        for (int i = 0; i < FrameMilliseconds.Length; i++)
            if (FrameMilliseconds[i] > 0) FrameQuantileScratch[frames++] = FrameMilliseconds[i];
        if (frames == 0) return;
        Array.Sort(FrameQuantileScratch, 0, frames);
        _frameP95 = FrameQuantileScratch[Math.Clamp((int)Math.Ceiling(.95 * frames) - 1, 0, frames - 1)];
        _frameP99 = FrameQuantileScratch[Math.Clamp((int)Math.Ceiling(.99 * frames) - 1, 0, frames - 1)];
    }
    private static double FramePercentile(double percentile) => percentile >= .99 ? _frameP99 : _frameP95;
    private static long Percentile(Sample sample, double percentile) => sample.QuantileCalls == 0 ? sample.MaximumTicks
        : percentile >= .99 ? sample.P99 : sample.P95;

    private static double ToMilliseconds(long ticks) => ticks * 1000d / Stopwatch.Frequency;

    private static int ReadPopulationCount()
    {
        try { return World.world?.units?.getSimpleList()?.Count ?? 0; }
        catch { return -1; }
    }

    private static string ResolveEra(int year)
    {
        MclslWorldRunState run = MclslWorldRunRepository.Current;
        if (run == null || string.IsNullOrWhiteSpace(run.RunId)) return "未初始化";
        if (MclslWorldEpochSystem.IsNewLawActive(year)) return "新法时代";
        return run.CultivationEpoch switch
        {
            MclslWorldEpochSystem.AncientLawEpoch => "仙道纪元",
            MclslWorldEpochSystem.TransmissionTransitionEpoch => "传法过渡",
            MclslWorldEpochSystem.NewLawEpoch => "新法时代",
            _ => "纪元待定"
        };
    }

    private static long ReadAllocatedBytes()
    {
        if (!_allocationReaderResolved)
        {
            _allocationReaderResolved = true;
            try
            {
                MethodInfo method = typeof(GC).GetMethod(
                    "GetAllocatedBytesForCurrentThread",
                    BindingFlags.Public | BindingFlags.Static,
                    binder: null,
                    types: Type.EmptyTypes,
                    modifiers: null);
                if (method != null) _allocatedBytesReader = (Func<long>)method.CreateDelegate(typeof(Func<long>));
                // Some Unity/Mono builds expose this API but always return 0.
                // Calibrate once, never report that unsupported counter as zero allocation.
                if (_allocatedBytesReader != null)
                {
                    long before = _allocatedBytesReader();
                    byte[] calibration = new byte[1024];
                    long after = _allocatedBytesReader();
                    GC.KeepAlive(calibration);
                    if (after <= before) _allocatedBytesReader = null;
                }
            }
            catch { _allocatedBytesReader = null; }
        }

        try { return _allocatedBytesReader?.Invoke() ?? -1L; }
        catch { return -1L; }
    }
}
