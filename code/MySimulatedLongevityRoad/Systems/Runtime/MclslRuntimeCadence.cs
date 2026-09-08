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

        // 年度修炼队列是语义工作，不能被帧压策略永久饿死。队列内部已有
        // 数量与毫秒双预算，因此只要存在积压，每个渲染帧都允许一次有界消费。
        bool annualBacklogDue = MclslScheduler.HasAnnualActorBacklog;
        bool priorityFastDue = MclslScheduler.HasUrgentSimulationBacklog
            || (annualBacklogDue
                && MclslRuntimeWorkBudget.ShouldRunAnnualActorPriorityPass());
        bool processFast = annualBacklogDue
            || ((fastCadenceDue || priorityFastDue)
                && (priorityFastDue || MclslScheduler.HasFastWork)
                && MclslRuntimeWorkBudget.TryBeginFastSchedulerPass());
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
