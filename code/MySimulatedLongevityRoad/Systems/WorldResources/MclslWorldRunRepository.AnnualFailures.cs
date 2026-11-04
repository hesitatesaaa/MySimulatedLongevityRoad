using System.Collections.Generic;
using MySimulatedLongevityRoad.Core;
using MySimulatedLongevityRoad.Data;

namespace MySimulatedLongevityRoad.Systems;

internal static partial class MclslWorldRunRepository
{
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
