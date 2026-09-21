using System;
using MySimulatedLongevityRoad.Systems;
using MySimulatedLongevityRoad.Data;
using MySimulatedLongevityRoad.UI;
using MySimulatedLongevityRoad.Traits;

namespace MySimulatedLongevityRoad.Core;

public static class MclslDeveloperApi
{
    private static Actor _selectedActor;

    internal static Actor SelectedActor => _selectedActor;

    internal static void SetSelectedActor(Actor actor)
    {
        if (!MclslRuntimeSettings.DebugToolsVisible || actor?.data == null) return;
        _selectedActor = actor;
    }

    internal static void OpenSelectedActorEditor(Actor actor = null)
    {
        if (actor != null) SetSelectedActor(actor);
        if (_selectedActor?.data == null) return;
        MclslDeveloperActorEditor.Show(_selectedActor);
    }

    internal static bool SetSpaceEssence(long value, out string message) => MclslHuanzhenSystem.SetSpaceEssenceForDeveloper(value, out message);

    internal static bool GrantHuanzhenToSelected(out string message)
    {
        message = string.Empty;
        Actor actor = _selectedActor;
        if (actor?.data == null) { message = "尚未选中人物。"; return false; }
        try
        {
            ActorTrait trait = AssetManager.traits.get(MclslTraitRegistration.HuanzhenTraitId);
            if (trait == null || actor.hasTrait(MclslTraitRegistration.HuanzhenTraitId)) { message = "人物已经拥有还真，或还真特质缺失。"; return false; }
            if (!actor.addTrait(trait, true)) { message = "还真特质授予失败。"; return false; }
            message = "已将还真授予当前人物。";
            return true;
        }
        catch (Exception ex) { message = "授予还真失败：" + ex.Message; return false; }
    }

    internal static void RefreshSelectedActor()
    {
        if (_selectedActor != null) MclslActorInfoPanel.RefreshOpenForActor(_selectedActor);
    }

    public static string GetCurrentRunSummary()
    {
        var run = MclslWorldRunRepository.Current;
        return $"第{run.CycleNumber}世｜真值{run.RunTruthValue}｜事件{run.Events.Count}｜死亡{run.DeathRecords.Count}｜遗迹{run.SectRuins.Count}｜洞天{run.WorldCaves.Count}｜天地之变{run.WorldChanges.Count}｜动态物品{run.GeneratedItems.Count}｜原生国家模式={run.UsesNativeKingdoms}";
    }

    public static bool SealCurrentCycle() => MclslReincarnationService.SealCurrentCycle("手动封存本世", MclslRuntime.CurrentYear());
    public static string GetHuanzhenStatus() => MclslHuanzhenSystem.StatusText();
    public static void EnterNewLawEraImmediately() => MclslWorldEpochSystem.ForceNewLawNow(MclslRuntime.CurrentYear());
}
