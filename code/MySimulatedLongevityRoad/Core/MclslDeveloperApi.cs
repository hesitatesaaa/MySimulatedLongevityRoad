using MySimulatedLongevityRoad.Systems;

namespace MySimulatedLongevityRoad.Core;

public static class MclslDeveloperApi
{
    public static string GetCurrentRunSummary()
    {
        var run = MclslWorldRunRepository.Current;
        return $"第{run.CycleNumber}世｜真值{run.RunTruthValue}｜事件{run.Events.Count}｜死亡{run.DeathRecords.Count}｜遗迹{run.SectRuins.Count}｜洞天{run.WorldCaves.Count}｜天地之变{run.WorldChanges.Count}｜动态物品{run.GeneratedItems.Count}｜原生国家模式={run.UsesNativeKingdoms}";
    }

    public static bool SealCurrentCycle() => MclslReincarnationService.SealCurrentCycle("手动封存本世", MclslRuntime.CurrentYear());
    public static string GetHuanzhenStatus() => MclslHuanzhenSystem.StatusText();
}
