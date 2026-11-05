using MySimulatedLongevityRoad.Core;
using UnityEngine;

namespace MySimulatedLongevityRoad.Systems;

internal static class MclslRuntimeCadence
{

    private static int _lastUnityFrame = -1;

    internal static void Tick(int frameCounter)
    {
        int unityFrame = Time.frameCount;
        if (_lastUnityFrame == unityFrame) return;
        _lastUnityFrame = unityFrame;
        int currentYear = MclslRuntime.CurrentYear();
        // 年度工作已在本帧优先取用预算；这里不再用整帧 FrameDeadline 作为门控，
        // 否则前面的低优先级工作一旦吃满预算，年度就会整帧不跑，积压追不动。
        bool processFast = MclslScheduler.HasFastWork;
        if (processFast)
        {
            MclslDiagnostics.CultivationThrottle(
                "cadence.process",
                unityFrame,
                30,
                "year=" + currentYear
                + " processFast=" + processFast
                + " backlog=" + MclslScheduler.AnnualActorBacklogCount
                + " stateCount=" + MclslScheduler.AnnualActorStateCount
                + " annualWorldPending=" + MclslScheduler.AnnualWorldWorkPending);
            MclslScheduler.ProcessAll(new MclslSchedulerContext(
                processFast: processFast,
                isYearChange: false,
                currentYear: currentYear));
        }
        MclslSpellSystem.TickSelfCasting(frameCounter);
        MclslAncientLawEventSystem.TickManual();
        if (frameCounter > 0 && frameCounter % 30 == 0 && MclslTianxuanMarket.PendingListingCount > 0)
            MclslTianxuanMarket.PublishPendingArtifactListings(8);
    }

    internal static void InitializeAfterLoad(int currentYear)
    {
        MclslDiagnostics.Cultivation("cadence.after_load", "year=" + currentYear);
        _lastUnityFrame = -1;
        MclslScheduler.InitializeAfterLoad(currentYear);
    }

    internal static void Clear()
    {
        _lastUnityFrame = -1;
        MclslScheduler.Clear();
        MclslAncientLawEventSystem.ClearManual();
    }
}
