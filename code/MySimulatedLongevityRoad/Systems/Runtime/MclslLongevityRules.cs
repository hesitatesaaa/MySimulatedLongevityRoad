using System;
using System.Collections.Generic;
using MySimulatedLongevityRoad.Data;
using MySimulatedLongevityRoad.Traits;

namespace MySimulatedLongevityRoad.Systems;

internal static class MclslLongevityRules
{
    private const int MortalBaseLifespan = 80;
    private static readonly HashSet<long> RepairingActorIds = new();

    internal static void ClearRuntimeCache()
    {
        RepairingActorIds.Clear();
    }

    internal static int RealmLifespanBonus(string realm) => RealmLifespanBonus(realm, false);

    internal static int RealmLifespanBonus(string realm, bool ancient)
    {
        int bonus = realm switch
        {
            MclslRealmIds.LianQi => 200,
            MclslRealmIds.ZhuJi => 300,
            MclslRealmIds.JinDan => 600,
            MclslRealmIds.YuanYing => 1000,
            MclslRealmIds.HuaShen => 2000,
            MclslRealmIds.HeDao => 3000,
            _ => 0
        };
        return ancient && bonus > 0 ? (int)Math.Round(bonus * 1.2f) : bonus;
    }

    internal static int ExpectedLifespan(string realm)
    {
        return ExpectedLifespan(realm, false);
    }

    private static int ExpectedLifespan(string realm, bool ancient)
    {
        if (realm == MclslRealmIds.ChangSheng) return 1000000;
        return MortalBaseLifespan + RealmLifespanBonus(realm, ancient);
    }

    internal static int ExpectedLifespan(Actor actor, string realm)
    {
        bool ancient = UsesAncientLifespan(actor, realm);
        int baseLifespan = ExpectedLifespan(realm, ancient);
        if (actor?.data == null || realm == MclslRealmIds.ChangSheng) return baseLifespan;
        int maxBonus = MaxTransferBonus(realm, ancient);
        int bonus = Math.Clamp(MclslActorAccessor.GetInt(actor, MclslActorDataKeys.LifespanStolenBonus, 0), 0, maxBonus);
        int penalty = Math.Clamp(MclslActorAccessor.GetInt(actor, MclslActorDataKeys.LifespanDrainedPenalty, 0), 0, Math.Max(0, baseLifespan - 20));
        return Math.Max(20, baseLifespan + bonus - penalty);
    }

    internal static int MaxTransferBonus(string realm) => Math.Max(20, ExpectedLifespan(realm) / 2);

    private static int MaxTransferBonus(string realm, bool ancient) => Math.Max(20, ExpectedLifespan(realm, ancient) / 2);

    internal static void ApplyRuntimeLifespan(Actor actor)
    {
        if (actor?.data == null || actor.stats == null) return;
        if (!MclslCultivationActorMarker.HasCultivationMarker(actor)) return;

        string realm = MclslActorAccessor.Realm(actor);
        if (string.IsNullOrWhiteSpace(realm)) return;
        int expected = ExpectedLifespan(actor, realm);
        if (expected <= MortalBaseLifespan) return;

        float current = GetStatSafe(actor, "lifespan", 0f);
        if (current + 0.01f >= expected) return;

        long actorId = MclslActorAccessor.Id(actor);
        if (actorId > 0 && !RepairingActorIds.Add(actorId)) return;
        try
        {
            actor.stats["lifespan"] = expected;
        }
        catch
        {
        }
        finally
        {
            if (actorId > 0) RepairingActorIds.Remove(actorId);
        }
    }

    internal static bool TryDrainYears(Actor receiver, Actor donor, int requestedYears, out int transferred)
    {
        transferred = 0;
        if (receiver?.data == null || donor?.data == null || receiver == donor) return false;
        string receiverRealm = MclslActorAccessor.Realm(receiver);
        if (string.IsNullOrWhiteSpace(receiverRealm) || receiverRealm == MclslRealmIds.ChangSheng) return false;
        string donorRealm = MclslActorAccessor.Realm(donor);
        bool receiverAncient = UsesAncientLifespan(receiver, receiverRealm);
        int receiverBase = ExpectedLifespan(receiverRealm, receiverAncient);
        int receiverCap = MaxTransferBonus(receiverRealm, receiverAncient);
        int receiverBonus = Math.Clamp(MclslActorAccessor.GetInt(receiver, MclslActorDataKeys.LifespanStolenBonus, 0), 0, receiverCap);
        int room = Math.Max(0, receiverCap - receiverBonus);
        if (room <= 0) return false;

        int donorExpected = ExpectedLifespan(donor, donorRealm);
        int donorAge;
        try { donorAge = Math.Max(0, (int)donor.getAge()); } catch { donorAge = 0; }
        int donorAvailable = Math.Max(0, donorExpected - donorAge - 8);
        int loss = Math.Clamp(requestedYears, 0, Math.Min(room, donorAvailable));
        if (loss <= 0) return false;

        int receiverGain = Math.Max(1, (int)Math.Floor(loss * 0.82f));
        receiverGain = Math.Min(receiverGain, room);
        if (receiverGain <= 0) return false;

        MclslActorAccessor.Set(receiver, MclslActorDataKeys.LifespanStolenBonus, receiverBonus + receiverGain);
        int donorPenalty = Math.Max(0, MclslActorAccessor.GetInt(donor, MclslActorDataKeys.LifespanDrainedPenalty, 0));
        MclslActorAccessor.Set(donor, MclslActorDataKeys.LifespanDrainedPenalty, donorPenalty + loss);
        transferred = receiverGain;
        return true;
    }

    internal static bool TryExpireAtAnnualLimit(Actor actor, int year)
    {
        if (!MclslActorAccessor.Alive(actor)) return true;
        string realm = MclslActorAccessor.Realm(actor);
        if (string.IsNullOrWhiteSpace(realm) || realm == MclslRealmIds.ChangSheng) return false;
        int age;
        try { age = Math.Max(0, (int)Math.Floor((double)actor.getAge())); }
        catch { return false; }
        int limit = ExpectedLifespan(actor, realm);
        if (age < limit) return false;

        string name = MclslActorAccessor.DisplayName(actor);
        string detail = name + "寿至" + age + "岁，已过" + MclslRealmIds.Display(realm) + "寿限" + limit + "岁，寿元耗尽而终。";
        MySimulatedLongevityRoad.Systems.Death.MclslDeathSystem.ExecuteScriptedDeath(
            actor,
            "old_age",
            "寿元耗尽",
            detail,
            MclslRealmIds.Index(realm) >= MclslRealmIds.Index(MclslRealmIds.YuanYing));
        return true;
    }

    internal static bool HasReachedLifespanLimit(Actor actor, string realm)
    {
        if (realm == MclslRealmIds.ChangSheng) return false;
        if (actor?.data == null) return true;
        try { return actor.getAge() >= ExpectedLifespan(actor, realm) - 3; }
        catch { return true; }
    }

    private static bool UsesAncientLifespan(Actor actor, string realm)
    {
        if (actor?.data == null || string.IsNullOrWhiteSpace(realm)) return false;
        string systemId = MclslActorAccessor.GetString(actor, MclslActorDataKeys.CultivationSystem, string.Empty);
        if (systemId == MclslCultivationSystemIds.AncientLaw) return true;

        string legacyTraitId = MclslTraitRegistration.LegacyTraitIdForRealm(realm);
        if (string.IsNullOrWhiteSpace(legacyTraitId)) return false;
        try { return actor.hasTrait(legacyTraitId); }
        catch { return false; }
    }

    private static float GetStatSafe(Actor actor, string statId, float fallback)
    {
        try { return actor?.stats == null ? fallback : actor.stats[statId]; }
        catch { return fallback; }
    }
}
