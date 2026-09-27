using System;
using System.Diagnostics;
using System.Text;
using MySimulatedLongevityRoad.Core;
using UnityEngine;

namespace MySimulatedLongevityRoad.Systems;

internal enum MclslWorkCategory : byte
{
    AnnualLight,
    Inventory,
    Progression,
    Market,
    WorldAggregate,
    Maintenance,
    Visual,
    Notification
}

/// <summary>
/// Allocation-free per-frame admission control for deferred simulation work.
/// Gameplay state is still owned and persisted by the existing annual pipeline.
/// </summary>
internal static class MclslStaggeredWorkPolicy
{
    private static readonly int[] Counts = new int[8];
    private static readonly long[] ElapsedTicks = new long[8];
    private static readonly long[] Enqueued = new long[8];
    private static readonly long[] Executed = new long[8];
    private static readonly long[] Deduplicated = new long[8];
    private static readonly long[] Canceled = new long[8];
    private static readonly long[] SlotAssignments = new long[120];
    private static int _frame = -1;
    private static long _frameStarted;

    internal static MclslWorkCategory Classify(MclslAnnualPipelineStage stage, int step)
    {
        return stage switch
        {
            MclslAnnualPipelineStage.Prepare when step is 3 or 5 or 6 or 7 => MclslWorkCategory.Inventory,
            MclslAnnualPipelineStage.Progression => MclslWorkCategory.Progression,
            MclslAnnualPipelineStage.Market => MclslWorkCategory.Market,
            _ => MclslWorkCategory.AnnualLight
        };
    }

    internal static int AssignDueFrame(long actorId, int logicalYear, MclslAnnualPipelineStage stage, int step)
    {
        int now = Time.frameCount;
        if (!MclslRuntimeSettings.StaggeredEventsEnabled) return now;
        int window = CurrentWindowFrames();
        long hash = unchecked(actorId * 31L + logicalYear * 131L + (int)stage * 17L + step * 7L);
        int slot = (int)((hash & long.MaxValue) % window);
        RecordEnqueued(Classify(stage, step), slot);
        return now + 1 + slot;
    }

    internal static int AssignVisualDueFrame(long actorId, string visualId)
    {
        int now = Time.frameCount;
        if (!MclslRuntimeSettings.StaggeredEventsEnabled) return now;
        int window = MclslRuntimeWorkBudget.StressTier switch
        {
            MclslRuntimeStressTier.Mild => 18,
            MclslRuntimeStressTier.Severe => 24,
            MclslRuntimeStressTier.Critical => 30,
            _ => 12
        };
        long hash = unchecked(actorId * 31L);
        if (visualId != null)
            for (int i = 0; i < visualId.Length; i++) hash = unchecked(hash * 33L + visualId[i]);
        int slot = (int)((hash & long.MaxValue) % window);
        RecordEnqueued(MclslWorkCategory.Visual, slot);
        return now + 1 + slot;
    }

    internal static bool TryBegin(MclslWorkCategory category, out long started)
    {
        BeginFrame();
        started = 0L;
        int index = (int)category;
        if (Counts[index] >= CountLimit(category)) return false;
        if (ElapsedTicks[index] >= MillisecondsToTicks(CategoryBudgetMs(category))) return false;
        if (Stopwatch.GetTimestamp() - _frameStarted >= MillisecondsToTicks(GlobalBudgetMs())) return false;
        Counts[index]++;
        Executed[index]++;
        started = Stopwatch.GetTimestamp();
        return true;
    }

    internal static void End(MclslWorkCategory category, long started)
    {
        if (started <= 0L) return;
        ElapsedTicks[(int)category] += Math.Max(0L, Stopwatch.GetTimestamp() - started);
    }

    internal static bool GlobalBudgetExhausted
    {
        get
        {
            BeginFrame();
            return Stopwatch.GetTimestamp() - _frameStarted >= MillisecondsToTicks(GlobalBudgetMs());
        }
    }

    internal static void Clear()
    {
        _frame = -1;
        _frameStarted = 0L;
        Array.Clear(Counts, 0, Counts.Length);
        Array.Clear(ElapsedTicks, 0, ElapsedTicks.Length);
        Array.Clear(Enqueued, 0, Enqueued.Length);
        Array.Clear(Executed, 0, Executed.Length);
        Array.Clear(Deduplicated, 0, Deduplicated.Length);
        Array.Clear(Canceled, 0, Canceled.Length);
        Array.Clear(SlotAssignments, 0, SlotAssignments.Length);
    }

    internal static void RecordDeduplicated(MclslWorkCategory category) => Deduplicated[(int)category]++;
    internal static void RecordCanceled(MclslWorkCategory category) => Canceled[(int)category]++;

    internal static void AppendSummary(StringBuilder buffer)
    {
        if (buffer == null) return;
        buffer.Append("错峰 开关/窗口/极限模式/年度上限 ")
            .Append(MclslRuntimeSettings.StaggeredEventsEnabled ? "开" : "关").Append('/')
            .Append(CurrentWindowFrames()).Append('/')
            .Append(MclslRuntimeSettings.AggressivePerformanceEnabled ? "开" : "关").Append('/')
            .Append(MclslRuntimeSettings.AnnualActorBudget).Append('\n');
        for (int i = 0; i < Enqueued.Length; i++)
        {
            buffer.Append("车道 ").Append((MclslWorkCategory)i)
                .Append(" 入队/去重/取消/执行 ")
                .Append(Enqueued[i]).Append('/').Append(Deduplicated[i]).Append('/')
                .Append(Canceled[i]).Append('/').Append(Executed[i]).Append('\n');
        }
        int window = Math.Min(CurrentWindowFrames(), SlotAssignments.Length);
        long minimum = long.MaxValue;
        long maximum = 0L;
        for (int i = 0; i < window; i++)
        {
            minimum = Math.Min(minimum, SlotAssignments[i]);
            maximum = Math.Max(maximum, SlotAssignments[i]);
        }
        buffer.Append("帧槽累计负载 最小/最大 ")
            .Append(minimum == long.MaxValue ? 0L : minimum).Append('/').Append(maximum).Append('\n');
    }

    private static void RecordEnqueued(MclslWorkCategory category, int slot)
    {
        Enqueued[(int)category]++;
        if (slot >= 0 && slot < SlotAssignments.Length) SlotAssignments[slot]++;
    }

    private static void BeginFrame()
    {
        int frame = Time.frameCount;
        if (_frame == frame) return;
        _frame = frame;
        _frameStarted = Stopwatch.GetTimestamp();
        Array.Clear(Counts, 0, Counts.Length);
        Array.Clear(ElapsedTicks, 0, ElapsedTicks.Length);
    }

    private static int CurrentWindowFrames()
    {
        int configured = MclslRuntimeSettings.EventStaggerFrames;
        if (!MclslRuntimeSettings.AggressivePerformanceEnabled) return configured;
        return MclslRuntimeWorkBudget.StressTier switch
        {
            MclslRuntimeStressTier.Mild => Math.Max(configured, 75),
            MclslRuntimeStressTier.Severe => Math.Max(configured, 90),
            MclslRuntimeStressTier.Critical => Math.Max(configured, 120),
            _ => configured
        };
    }

    private static int CountLimit(MclslWorkCategory category) => category switch
    {
        MclslWorkCategory.AnnualLight => 24,
        MclslWorkCategory.Inventory => 8,
        MclslWorkCategory.Progression => 4,
        MclslWorkCategory.Market => 4,
        MclslWorkCategory.WorldAggregate => 1,
        MclslWorkCategory.Maintenance => 16,
        MclslWorkCategory.Visual => 12,
        _ => 1
    };

    private static double CategoryBudgetMs(MclslWorkCategory category) => category switch
    {
        MclslWorkCategory.AnnualLight => 0.20d,
        MclslWorkCategory.Inventory => 0.25d,
        MclslWorkCategory.Progression => 0.40d,
        MclslWorkCategory.Market => 0.25d,
        MclslWorkCategory.WorldAggregate => 0.35d,
        MclslWorkCategory.Maintenance => 0.15d,
        MclslWorkCategory.Visual => 0.20d,
        _ => 0.10d
    };

    private static double GlobalBudgetMs() => MclslRuntimeWorkBudget.StressTier switch
    {
        MclslRuntimeStressTier.Mild => 0.55d,
        MclslRuntimeStressTier.Severe => 0.35d,
        MclslRuntimeStressTier.Critical => 0.20d,
        _ => 0.80d
    };

    private static long MillisecondsToTicks(double value)
        => Math.Max(1L, (long)(value * Stopwatch.Frequency / 1000d));
}
