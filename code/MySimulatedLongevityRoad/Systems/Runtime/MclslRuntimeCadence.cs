using MySimulatedLongevityRoad.Core;
using UnityEngine;

namespace MySimulatedLongevityRoad.Systems;

internal static class MclslRuntimeCadence
{
    private const int FastCadenceFrames = 10;

    private static int _lastUnityFrame = -1;
    private static int _lastProcessYear = -1;

    internal static void Tick(int frameCounter)
    {
        int unityFrame = Time.frameCount;
        if (_lastUnityFrame == unityFrame) return;
        _lastUnityFrame = unityFrame;

        bool fastCadenceDue = frameCounter > 0 && frameCounter % FastCadenceFrames == 0;
        int currentYear = MclslRuntime.CurrentYear();
        bool isYearChange = currentYear > 0 && currentYear > _lastProcessYear;
        if (isYearChange)
        {
            MclslDiagnostics.Cultivation(
                "cadence.year_change",
                "year=" + currentYear
                + " lastProcessYear=" + _lastProcessYear
                + " backlog=" + MclslScheduler.AnnualActorBacklogCount
                + " stateCount=" + MclslScheduler.AnnualActorStateCount);
            _lastProcessYear = currentYear;
            MclslScheduler.ScheduleAnnualWorld(currentYear);
        }

        // 年度队列只在共享调度令牌允许时运行。旧实现让任何积压绕过帧压
        // 策略、每个渲染帧都进入调度器；新法初开的大量角色因此会反过来
        // 压低 FPS。队列状态会持久化，故延后一帧不会丢失修炼结算。
        bool annualBacklogDue = MclslScheduler.HasAnnualActorBacklog
            || MclslScheduler.HasAnnualCandidateBacklog;
        bool priorityFastDue = MclslScheduler.HasUrgentSimulationBacklog
            || (annualBacklogDue
                && MclslRuntimeWorkBudget.ShouldRunAnnualActorPriorityPass());
        bool processFast = (fastCadenceDue || priorityFastDue)
            && (priorityFastDue || MclslScheduler.HasFastWork)
            && MclslRuntimeWorkBudget.TryBeginFastSchedulerPass();
        if (processFast || isYearChange)
        {
            MclslDiagnostics.CultivationThrottle(
                "cadence.process",
                unityFrame,
                30,
                "year=" + currentYear
                + " processFast=" + processFast
                + " isYearChange=" + isYearChange
                + " backlog=" + MclslScheduler.AnnualActorBacklogCount
                + " stateCount=" + MclslScheduler.AnnualActorStateCount
                + " annualWorldPending=" + MclslScheduler.AnnualWorldWorkPending);
            MclslScheduler.ProcessAll(new MclslSchedulerContext(
                processFast: processFast,
                isYearChange: isYearChange,
                currentYear: currentYear));
        }
    }

    internal static void InitializeAfterLoad(int currentYear)
    {
        MclslDiagnostics.Cultivation("cadence.after_load", "year=" + currentYear);
        _lastUnityFrame = -1;
        _lastProcessYear = System.Math.Max(0, currentYear);
        MclslScheduler.InitializeAfterLoad(currentYear);
    }

    internal static void Clear()
    {
        _lastUnityFrame = -1;
        _lastProcessYear = -1;
        MclslScheduler.Clear();
    }
}
