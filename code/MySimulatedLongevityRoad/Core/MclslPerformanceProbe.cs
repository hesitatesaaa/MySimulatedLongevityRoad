using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using MySimulatedLongevityRoad.Data;
using MySimulatedLongevityRoad.Systems;
using UnityEngine;

namespace MySimulatedLongevityRoad.Core;

/// <summary>Opt-in, bounded performance diagnostics for the developer package.</summary>
internal static class MclslPerformanceProbe
{
    private const int RecentLaneSamples = 1024;
    private const int FrameHistoryCapacity = 4096;
    private const int ContextRefreshFrames = 60;

    private sealed class Sample
    {
        internal long Ticks;
        internal long MaximumTicks;
        internal long CurrentFrameTicks;
        internal long MaximumFrameTicks;
        internal int Calls;
        internal readonly long[] RecentTicks = new long[RecentLaneSamples];
        internal int RecentCount;
        internal int RecentCursor;

        internal void Add(long ticks)
        {
            Ticks += ticks;
            if (ticks > MaximumTicks) MaximumTicks = ticks;
            CurrentFrameTicks += ticks;
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
        internal int LatestRequestedYear;
        internal int OldestPendingYear;
        internal string Era;
        internal string WorldStage;
        internal string TopLane;
        internal float TopLaneMilliseconds;
        internal float Speed;
    }

    private static readonly Dictionary<string, Sample> Samples = new(StringComparer.Ordinal);
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
    private static long _cultivatorPollActors;
    private static int _archiveMainBytes;
    private static int _archiveBackupBytes;

    internal static bool Enabled { get; private set; }

    internal static void SetEnabled(bool enabled)
    {
        Enabled = enabled;
        Samples.Clear();
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

    internal static long Begin() => Enabled ? Stopwatch.GetTimestamp() : 0L;

    internal static void End(string lane, long started)
    {
        if (!Enabled || started == 0L || string.IsNullOrWhiteSpace(lane)) return;
        if (!Samples.TryGetValue(lane, out Sample sample)) Samples[lane] = sample = new Sample();
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
            LatestRequestedYear = _latestRequestedYear,
            OldestPendingYear = _oldestPendingYear,
            Era = era,
            WorldStage = MclslAnnualWorldRuntimeLane.CurrentStageName,
            TopLane = topLane,
            TopLaneMilliseconds = topLaneTicks * 1000f / Stopwatch.Frequency,
            Speed = Time.timeScale
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
            .Append("；倍速 ").Append(Time.timeScale.ToString("F2")).Append('x')
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
        MclslStaggeredWorkPolicy.AppendSummary(Buffer);
        Buffer
            .Append("战斗法术快速拒绝/符箓背包单扫/修士轮询人数 ")
            .Append(_combatSpellFastRejects).Append('/').Append(_combatBagSingleScans).Append('/')
            .Append(_cultivatorPollActors).Append("；主档/备份字节 ")
            .Append(_archiveMainBytes).Append('/').Append(_archiveBackupBytes).Append('\n')
            .Append("帧时 ms 平均/P95/P99/最大：").Append(frameAverage.ToString("F2"))
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
        Buffer.Append("\n\n总耗时前十模块\n");

        List<KeyValuePair<string, Sample>> ranked = new(Samples.Count);
        foreach (KeyValuePair<string, Sample> pair in Samples)
            if (!string.Equals(pair.Key, "模组每帧CPU", StringComparison.Ordinal)) ranked.Add(pair);
        ranked.Sort((left, right) => right.Value.Ticks.CompareTo(left.Value.Ticks));
        AppendLaneTable(ranked, useFramePeak: false);

        ranked.Sort((left, right) => right.Value.MaximumFrameTicks.CompareTo(left.Value.MaximumFrameTicks));
        Buffer.Append("\n最大单帧峰值前十模块\n");
        AppendLaneTable(ranked, useFramePeak: true);

        Buffer.Append("\n各模块：调用/总ms/均ms/单次最大ms/P95/P99/单帧峰值ms\n");
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
                .Append(ToMilliseconds(sample.MaximumFrameTicks).ToString("F2")).Append(" ms\n");
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
                .Append(sample.Speed.ToString("F2")).Append("x/")
                .Append(sample.Population).Append(' ').Append(sample.Cultivators).Append('/')
                .Append(sample.AnnualQueue).Append(' ').Append(sample.AnnualStates).Append(" gap=").Append(yearGap).Append('/')
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

    private static double FramePercentile(double percentile)
    {
        int count = 0;
        for (int i = 0; i < FrameMilliseconds.Length; i++)
            if (FrameMilliseconds[i] > 0f) count++;
        if (count == 0) return 0d;
        float[] values = new float[count];
        int next = 0;
        for (int i = 0; i < FrameMilliseconds.Length; i++)
            if (FrameMilliseconds[i] > 0f) values[next++] = FrameMilliseconds[i];
        Array.Sort(values);
        int index = Math.Clamp((int)Math.Ceiling(percentile * count) - 1, 0, count - 1);
        return values[index];
    }

    private static long Percentile(Sample sample, double percentile)
    {
        int count = sample.RecentCount;
        if (count <= 0) return 0L;
        long[] values = new long[count];
        Array.Copy(sample.RecentTicks, values, count);
        Array.Sort(values);
        int index = Math.Clamp((int)Math.Ceiling(percentile * count) - 1, 0, count - 1);
        return values[index];
    }

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
            }
            catch { _allocatedBytesReader = null; }
        }

        try { return _allocatedBytesReader?.Invoke() ?? -1L; }
        catch { return -1L; }
    }
}
