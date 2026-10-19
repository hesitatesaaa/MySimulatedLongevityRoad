using System;
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
        // A failing domain cannot prevent later death consequences or registry
        // cleanup. No per-death delegates or closures are allocated here.
        for (int step = 0; step < 12; step++)
        {
            try
            {
                switch (step)
                {
                    case 0: MclslNativeKillStatisticsSystem.RecordCommittedDeath(actor); break;
                    case 1: MclslHuanzhenSystem.ObserveHostKill(actor, attackType); break;
                    case 2: MclslTechniqueOccupationSystem.RecordSameTechniqueKill(actor, MclslRuntime.CurrentYear(), attackType); break;
                    case 3: MclslMortalMiasmaSystem.ObserveDeath(actor, MclslRuntime.CurrentYear(), attackType); break;
                    case 4:
                        if (!state.WorldSoulDeath.Found) MclslWorldChangeSystem.ObserveNativeDeath(actor, attackType);
                        break;
                    case 5: MclslWorldSoulSystem.CommitDeath(actor, state.WorldSoulDeath); break;
                    case 6: MclslDeathSystem.Commit(actor, state.CultivatorDeath); break;
                    case 7: MclslWorldSoulSystem.ReleaseHolderOnDeath(actor); break;
                    case 8: MclslTechniqueOccupationSystem.Release(actor); break;
                    case 9: MclslHuanzhenSystem.CommitDeath(actor, state.HuanzhenDeath); break;
                    case 10: MclslFamilySystem.OnDeath(actor); break;
                    case 11:
                        MclslBeastWorld.ForgetRank(MclslActorAccessor.Id(actor));
                        if(state.HealthDepletedAtDeath)
                            MclslBeastLoot.Commit(actor,state.CultivatorDeath.PendingCauseCode,attackType);
                        break;
                }
            }
            catch (Exception ex) { MclslDiagnostics.Error("death-domain-" + step, "人物死亡阶段 " + step + " 失败：" + ex.Message); }
        }
    }
}
