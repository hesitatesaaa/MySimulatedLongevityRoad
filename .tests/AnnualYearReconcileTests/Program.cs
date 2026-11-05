using System;
using MySimulatedLongevityRoad.Core;

namespace AnnualYearReconcileTests;

// Regression for the "world paused forever after an anchor/save rollback" defect.
// The shipped performance report showed world 1302 / completed 1302 / request 1378
// and 2262 actors waiting at year 1303, with the backpressure factor pinned at 0.
public static class Program
{
    static int Checks;
    static void Check(bool ok, string name)
    {
        if (!ok) throw new Exception(name);
        Checks++;
    }

    public static void Main()
    {
        // --- Lag: a stale request year must not inflate a caught-up world ---
        Check(MclslAnnualYearReconcilePolicy.LagYears(1302, 1302, 1303, 1378) == 0,
            "caught-up world with stale request year reports zero lag");
        Check(MclslAnnualYearReconcilePolicy.LagYears(1302, 1302, 0, 1302) == 0,
            "caught-up world reports zero lag");
        Check(MclslAnnualYearReconcilePolicy.LagYears(1300, 1250, 1251, 1300) == 50,
            "genuine backlog reports 50 years");
        Check(MclslAnnualYearReconcilePolicy.LagYears(1300, 1240, 1241, 9999) == 60,
            "stale huge request uses world clock only");
        Check(MclslAnnualYearReconcilePolicy.LagYears(1302, 0, 0, 0) == 1,
            "new run fallback is one year");
        Check(MclslAnnualYearReconcilePolicy.LagYears(0, 1302, 1303, 1378) == 76,
            "world 0 falls back to request frontier");
        Check(MclslAnnualYearReconcilePolicy.LagYears(1300, 1400, 0, 1300) == 0,
            "completed ahead of world clamps to zero");

        // --- Batch clamp ---
        MclslAnnualYearReconcilePolicy.BatchYears reset =
            MclslAnnualYearReconcilePolicy.ClampBatch(1378, 1302, 1303, 1302);
        Check(reset.LatestRequestedYear == 1302, "batch: latest clamped to world");
        Check(reset.LastCompletedYear == 1302, "batch: completed kept at world");
        Check(reset.ActiveYear == 0, "batch: future active reset");
        Check(reset.ResetWorldStage, "batch: future active flags stage reset");

        MclslAnnualYearReconcilePolicy.BatchYears normal =
            MclslAnnualYearReconcilePolicy.ClampBatch(1300, 1299, 1300, 1300);
        Check(normal.LatestRequestedYear == 1300 && normal.LastCompletedYear == 1299
            && normal.ActiveYear == 1300 && !normal.ResetWorldStage, "batch: normal play unchanged");

        MclslAnnualYearReconcilePolicy.BatchYears loading =
            MclslAnnualYearReconcilePolicy.ClampBatch(1378, 1302, 1303, 0);
        Check(loading.LatestRequestedYear == 1378 && loading.ActiveYear == 1303
            && !loading.ResetWorldStage, "batch: world 0 leaves cursors untouched");

        MclslAnnualYearReconcilePolicy.BatchYears completedAhead =
            MclslAnnualYearReconcilePolicy.ClampBatch(1300, 1400, 0, 1300);
        Check(completedAhead.LastCompletedYear == 1300, "batch: completed clamped to world (not world-1)");

        // --- Per-actor restore ---
        Check(!MclslAnnualYearReconcilePolicy.TryRestorePending(1303, 1378, 1302, 1302,
                out _, out _, out _), "restore: active beyond world is dropped");
        Check(!MclslAnnualYearReconcilePolicy.TryRestorePending(0, 1378, 1302, 1302,
                out _, out _, out _), "restore: no active year dropped");

        Check(MclslAnnualYearReconcilePolicy.TryRestorePending(1251, 1300, 1250, 1300,
                out int a1, out int l1, out int c1), "restore: genuine backlog accepted");
        Check(a1 == 1251 && l1 == 1300 && c1 == 1250, "restore: genuine backlog years kept");

        Check(MclslAnnualYearReconcilePolicy.TryRestorePending(1302, 1378, 1301, 1302,
                out int a2, out int l2, out int c2), "restore: in-progress world year accepted");
        Check(a2 == 1302 && l2 == 1302 && c2 == 1301, "restore: request year clamped to world");

        Check(!MclslAnnualYearReconcilePolicy.TryRestorePending(1300, 1290, 1250, 1302,
                out _, out _, out _), "restore: latest below active rejected");
        Check(!MclslAnnualYearReconcilePolicy.TryRestorePending(1299, 1300, 1299, 1302,
                out _, out _, out _), "restore: active not ahead of completed rejected");

        Console.WriteLine("PASS annual year reconcile policy: " + Checks + " checks");
    }
}
