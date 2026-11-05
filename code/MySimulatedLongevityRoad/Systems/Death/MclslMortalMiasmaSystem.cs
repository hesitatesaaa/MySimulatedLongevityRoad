using System;
using System.Reflection;
using MySimulatedLongevityRoad.Core;
using MySimulatedLongevityRoad.Data;
using MySimulatedLongevityRoad.Systems;

namespace MySimulatedLongevityRoad.Systems.Death;

internal static class MclslMortalMiasmaSystem
{
    private static readonly string[] KillerMemberNames =
    {
        "last_attacker", "lastAttacker", "_last_attacker", "attacked_by", "attackedBy", "killer", "last_hit_actor", "lastHitActor"
    };

    internal static void ObserveDeath(Actor victim, int year, AttackType attackType)
    {
        if (!MclslDeathSystem.IsCombatDeath(attackType)) return;
        if (!MclslRuntimeSettings.CoreEnabled || victim?.data == null) return;
        if (!MclslWorldEpochSystem.IsMortalMiasmaActive(year)) return;
        if (MclslActorAccessor.IsCultivator(victim)) return;
        if (!MclslEligibility.IsCivilizedActor(victim)) return;

        Actor killer = TryGetKiller(victim);
        if (!MclslActorAccessor.Alive(killer) || !MclslActorAccessor.IsCultivator(killer)) return;
        if (!string.IsNullOrWhiteSpace(MclslActorAccessor.GetString(killer, MclslActorDataKeys.WorldSoulEntityId, string.Empty))) return;

        string realm = MclslActorAccessor.Realm(killer);
        int gain = MiasmaGain(realm);
        if (gain > 0 && MclslInverseTruthSystem.IsTruthReversed("truth_mortal_miasma"))
            gain = Math.Max(gain + 1, (int)Math.Ceiling(gain * 1.5f));
        if (gain <= 0) return;

        int capacity = Capacity(realm);
        int current = Math.Clamp(MclslActorAccessor.GetInt(killer, MclslActorDataKeys.MortalMiasma, 0), 0, capacity);
        int next = Math.Clamp(current + gain, 0, capacity);
        MclslActorAccessor.Set(killer, MclslActorDataKeys.MortalMiasma, next);
        MclslActorAccessor.Set(killer, MclslActorDataKeys.LastBreakthroughResult, "误伤凡俗，仙凡瘴积累至" + next + "/" + capacity);
        if (next < capacity) return;

        string name = MclslActorAccessor.DisplayName(killer, realm);
        string detail = "修士“" + name + "”屡伤凡俗，仙凡瘴积满，瘴毒反噬修为与形神，终至还道于天。";
        MclslWorldRunRepository.AddEvent(year, "mortal_miasma_death", name + "为仙凡瘴反噬", detail, killer);
        MclslDeathSystem.ExecuteScriptedDeath(killer, "mortal_miasma", "仙凡瘴", detail, true);
    }

    private static int MiasmaGain(string realm) => realm switch
    {
        MclslRealmIds.LianQi => 2,
        MclslRealmIds.ZhuJi => 3,
        MclslRealmIds.JinDan => 5,
        MclslRealmIds.YuanYing => 8,
        MclslRealmIds.HuaShen => 12,
        MclslRealmIds.HeDao => 18,
        MclslRealmIds.ChangSheng => 25,
        _ => 0
    };

    internal static int Capacity(string realm) => realm switch
    {
        MclslRealmIds.LianQi => 50,
        MclslRealmIds.ZhuJi => 100,
        MclslRealmIds.JinDan => 200,
        MclslRealmIds.YuanYing => 400,
        MclslRealmIds.HuaShen => 800,
        MclslRealmIds.HeDao => 1600,
        MclslRealmIds.ChangSheng => 3200,
        _ => 50
    };

    private static Actor TryGetKiller(Actor victim)
    {
        try
        {
            Actor killer = victim.attackedBy as Actor;
            if (killer != null && killer != victim) return killer;
        }
        catch (System.Exception mclslEmptyCatchEx) { MySimulatedLongevityRoad.Core.MclslGuard.Swallowed("empty-catch-code-MySimulatedLongevityRoad-Systems-Death-MclslMortalMiasmaSystem-cs-1", mclslEmptyCatchEx, MySimulatedLongevityRoad.Core.MclslGuardScope.OptionalMember); }
        return TryGetActorMember(victim, KillerMemberNames);
    }

    private static Actor TryGetActorMember(object source, string[] memberNames)
        => MySimulatedLongevityRoad.Core.MclslReflectionMembers.TryGetActorMember(source, memberNames, "mortal-miasma-actor-member");
}
