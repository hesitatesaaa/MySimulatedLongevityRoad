using System;
using MySimulatedLongevityRoad.Data;
using MySimulatedLongevityRoad.Traits;

namespace MySimulatedLongevityRoad.Systems;

/// <summary>
/// 感气是进入炼气前的真实阶段：真元未满炼气门槛时不授予炼气境界。
/// 只在角色年度修炼时结算，不增加帧检测或额外世界扫描。
/// </summary>
internal static class MclslSensingQiSystem
{
    internal static bool IsSensing(Actor actor)
    {
        if (actor?.data == null || !MclslActorAccessor.Alive(actor) || !MclslEligibility.CanCultivate(actor)) return false;
        if (!string.IsNullOrWhiteSpace(MclslActorAccessor.Realm(actor))) return false;

        return MclslSpiritualRootSystem.HasCultivationPotential(actor);
    }

    internal static bool ProcessAnnual(Actor actor, int year, bool ancientLaw)
    {
        if (actor?.data == null) return false;
        MclslActorAccessor.ApplyDisplayName(actor, string.Empty);

        int currentEssence = MclslCultivationGrowthSystem.CurrentTrueEssence(actor);
        MySimulatedLongevityRoad.Core.MclslDiagnostics.Cultivation(
            "sensing.process",
            "actor=" + MclslActorAccessor.Id(actor)
            + " year=" + year
            + " ancient=" + ancientLaw
            + " essence=" + currentEssence
            + " requirement=" + MclslRealmProgress.LianQiEntryMinimum);
        if (currentEssence < MclslRealmProgress.LianQiEntryMinimum)
        {
            MclslActorAccessor.Set(
                actor,
                MclslActorDataKeys.LastBreakthroughResult,
                "感气积累真元："
                + Math.Min(currentEssence, MclslRealmProgress.LianQiEntryMinimum)
                + "/"
                + MclslRealmProgress.LianQiEntryMinimum);
            _ = year;
            _ = ancientLaw;
            return true;
        }

        MclslCultivationSystem.SetRealm(
            actor,
            MclslRealmIds.LianQi,
            year,
            ancientLaw
                ? "感气圆满，灵根吐纳而入仙道炼气"
                : "感气圆满，正式踏入炼气境");
        MySimulatedLongevityRoad.Core.MclslDiagnostics.Cultivation(
            "sensing.breakthrough",
            "actor=" + MclslActorAccessor.Id(actor)
            + " year=" + year
            + " ancient=" + ancientLaw
            + " essence=" + currentEssence);
        return true;
    }

    internal static bool ShouldReturnToSensingQi(Actor actor, string realm, string cultivationSystem)
    {
        if (actor?.data == null || realm != MclslRealmIds.LianQi) return false;
        if (cultivationSystem != MclslCultivationSystemIds.NewLaw
            && cultivationSystem != MclslCultivationSystemIds.AncientLaw
            && !string.IsNullOrWhiteSpace(cultivationSystem)) return false;
        return MclslActorAccessor.GetInt(actor, MclslActorDataKeys.TrueEssence, 0)
            < MclslRealmProgress.LianQiEntryMinimum;
    }

    internal static void ReturnToSensingQi(Actor actor, int year)
    {
        if (actor?.data == null) return;
        MclslCultivationStateTransitions.ClearRealm(
            actor,
            year,
            "旧档修为校正：真元未满" + MclslRealmProgress.LianQiEntryMinimum + "，回归感气阶段");
        MclslCultivationGrowthSystem.SyncProgress(
            actor,
            string.Empty,
            MclslActorAccessor.GetString(actor, MclslActorDataKeys.CultivationSystem, string.Empty)
                == MclslCultivationSystemIds.AncientLaw);
    }


}
