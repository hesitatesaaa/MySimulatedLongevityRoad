using System;
using System.Collections.Generic;
using MySimulatedLongevityRoad.Core;
using MySimulatedLongevityRoad.Data;

namespace MySimulatedLongevityRoad.Systems;

internal static partial class MclslWorldRunRepository
{
    // 年度结算一旦因异常被永久阻塞，就会出现“时间在走、世界不动”的死锁：
    // 世界时间继续推进，但年度世界（时代、兽潮、宗门、家族……）永远不再结算。
    // 这里给出有界自愈：异常后按指数退避重试，避免每帧重试造成卡顿；
    // 连续失败超过上限后保持阻塞，不会无限刷。
    private const int MaxAnnualAutoRecoverAttempts = 8;
    private static double _annualRecoverAt;

    internal static void ResetAnnualBlockOnLoad()
    {
        MclslAnnualBatchState batch = Current?.AnnualBatch;
        if (batch == null) return;
        _annualRecoverAt = 0d;
        if (!batch.WorldBlocked && batch.WorldFailureStage == 0 && batch.WorldFailureCount == 0) return;
        batch.WorldBlocked = false;
        batch.WorldFailureStage = 0;
        batch.WorldFailureCount = 0;
        MclslWorldArchiveStore.MarkDirty();
        MclslDiagnostics.Alert("annual-block-reset-on-load", "读档/进入世界后已清除上一年度结算阻塞状态，重新尝试结算。");
    }

    /// <summary>返回 true 表示可以继续年度结算；false 表示本轮仍应等待。now 为调用方的秒级时钟。</summary>
    internal static bool TryRecoverBlockedAnnual(double now)
    {
        MclslAnnualBatchState batch = Current?.AnnualBatch;
        if (batch == null || !batch.WorldBlocked) { _annualRecoverAt = 0d; return true; }
        if (batch.WorldFailureCount > MaxAnnualAutoRecoverAttempts) return false;
        if (now < _annualRecoverAt) return false;
        int attempt = Math.Max(1, batch.WorldFailureCount);
        double delay = Math.Min(120d, 5d * Math.Pow(2d, Math.Min(6, attempt - 1)));
        _annualRecoverAt = now + delay;
        batch.WorldBlocked = false;
        MclslWorldArchiveStore.MarkDirty();
        MclslDiagnostics.Alert("annual-auto-recover",
            "年度结算异常后自动重试（第 " + attempt + " 次，退避 " + delay.ToString("F0") + " 秒）。");
        return true;
    }

    // One boundary owns the blocked flag, durable failure record, and revision.
    internal static void BlockAnnualSettlement(int year, string scope, string targetId,
        string step, int attempts, string error)
    {
        Current.AnnualBatch.WorldBlocked = true;
        try { RecordAnnualFailure(year, scope, targetId, step, attempts, "未结算", error); }
        finally
        {
            MclslDiagnostics.Fatal("annual-blocked:" + scope + ":" + step,
                year + " 年年度结算已暂停：" + scope + "/" + step + "（" + targetId + "）：" + error);
        }
    }

    internal static void BlockWorldAnnualSettlement(int year, byte stage, string step, string error)
    {
        MclslAnnualBatchState batch = Current.AnnualBatch;
        batch.WorldFailureStage = stage;
        batch.WorldFailureCount++;
        BlockAnnualSettlement(year, "世界", string.Empty, step, batch.WorldFailureCount, error);
    }

    internal static void RecordAnnualFailure(int year, string scope, string targetId,
        string step, int attempts, string result, string error)
    {
        scope ??= string.Empty;
        targetId ??= string.Empty;
        step ??= string.Empty;
        MclslAnnualBatchState batch = Current.AnnualBatch;
        batch.FailureRecords ??= new List<MclslAnnualFailureRecord>();
        MclslAnnualFailureRecord record = batch.FailureRecords.Find(x => x.Year == year
            && x.Scope == scope && x.TargetId == targetId && x.Step == step);
        if (record == null)
        {
            record = new MclslAnnualFailureRecord { Year = year, Scope = scope,
                TargetId = targetId, Step = step };
            batch.FailureRecords.Add(record);
            if (batch.FailureRecords.Count > 512) batch.FailureRecords.RemoveAt(0);
        }
        record.Attempts = attempts;
        record.Result = result ?? string.Empty;
        record.Error = error?.Length > 300 ? error.Substring(0, 300) : error ?? string.Empty;
        MclslWorldArchiveStore.MarkDirty();
        if (result is "跳过" or "部分失败" or "未结算")
        {
            MclslDiagnostics.Error("annual-failure:" + scope + ":" + step + ":" + targetId,
                year + " 年 " + scope + "/" + step + " 结算" + result + "：" + record.Error);
            AddEvent(year, "annual_settlement_failure", "年度结算异常",
                scope + "/" + step + "（" + targetId + "）" + result + "：" + record.Error);
        }
    }

}
