using MySimulatedLongevityRoad.Data;
using MySimulatedLongevityRoad.Core;
using MySimulatedLongevityRoad.Systems;
using MySimulatedLongevityRoad.Systems.Death;

namespace MySimulatedLongevityRoad.Modules;

internal static class MclslDeathEventRouter
{
    internal static void CommitActorDeath(Actor actor, AttackType attackType, MclslDeathPatchState state)
    {
        // Patches only capture the native event; domain modules own all consequences.
        MclslNativeKillStatisticsSystem.RecordCommittedDeath(actor);
        MclslTechniqueOccupationSystem.RecordSameTechniqueKill(actor, MclslRuntime.CurrentYear(), attackType);
        MclslMortalMiasmaSystem.ObserveDeath(actor, MclslRuntime.CurrentYear(), attackType);
        if (!state.WorldSoulDeath.Found) MclslWorldChangeSystem.ObserveNativeDeath(actor, attackType);
        MclslWorldSoulSystem.CommitDeath(actor, state.WorldSoulDeath);
        MclslDeathSystem.Commit(actor, state.CultivatorDeath);
        MclslWorldSoulSystem.ReleaseHolderOnDeath(actor);
        MclslTechniqueOccupationSystem.Release(actor);
        MclslHuanzhenSystem.CommitDeath(actor, state.HuanzhenDeath);
    }
}
