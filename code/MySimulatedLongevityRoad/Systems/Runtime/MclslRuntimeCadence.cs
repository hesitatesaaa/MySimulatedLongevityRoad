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
        MclslSpellSystem.TickSelfCasting(frameCounter);
        MclslAncientLawEventSystem.TickManual();

        int currentYear = MclslRuntime.CurrentYear();
        if (frameCounter > 0 && frameCounter % 30 == 0 && MclslTianxuanMarket.PendingListingCount > 0)
            MclslTianxuanMarket.PublishPendingArtifactListings(8);
        if (frameCounter > 0 && frameCounter % 120 == 0)
            MclslCultivatorCandidateIndex.AuditRegisteredActors();
        bool processFast = MclslScheduler.HasFastWork
            && MclslRuntimeWorkBudget.TryBeginFastSchedulerPass();
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
