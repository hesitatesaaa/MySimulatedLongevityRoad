using System;
using MySimulatedLongevityRoad.Core;
using MySimulatedLongevityRoad.Data;

namespace MySimulatedLongevityRoad.Systems;

internal static class MclslCultivationAgeSanity
{
    private const int CurrentTrueEssenceScaleVersion = 2;

    internal static bool RepairImpossibleYouthCultivation(Actor actor, int year)
    {
        if (actor?.data == null || !MclslActorAccessor.Alive(actor)) return false;

        int storedScaleVersion = MclslActorAccessor.GetInt(actor, MclslActorDataKeys.TrueEssenceScaleVersion, 0);
        int age = SafeAge(actor);
        string realm = MclslActorAccessor.Realm(actor);
        bool ancientLaw = MclslActorAccessor.GetString(actor, MclslActorDataKeys.CultivationSystem, string.Empty)
            == MclslCultivationSystemIds.AncientLaw;
        int currentEssence = MclslCultivationGrowthSystem.CurrentTrueEssence(actor);
        string allowedRealm = HighestRealmAllowedAtAge(age);
        bool changed = false;

        if (!string.IsNullOrWhiteSpace(realm)
            && MclslRealmIds.Index(realm) > MclslRealmIds.Index(allowedRealm))
        {
            string previousRealm = realm;
            if (string.IsNullOrWhiteSpace(allowedRealm))
            {
                MclslCultivationStateTransitions.ClearRealm(actor, year, "年岁尚浅，清理异常境界：" + MclslRealmIds.Display(previousRealm));
                realm = string.Empty;
            }
            else
            {
                MclslCultivationStateTransitions.TrySetRealm(
                    actor,
                    allowedRealm,
                    year,
                    "年岁尚浅，修复异常境界：" + MclslRealmIds.Display(previousRealm) + "回落至" + MclslRealmIds.Display(allowedRealm));
                realm = allowedRealm;
            }

            MclslDiagnostics.Cultivation(
                "age_sanity.realm_repair",
                "actor=" + MclslActorAccessor.Id(actor)
                + " year=" + year
                + " age=" + age
                + " previous=" + previousRealm
                + " allowed=" + allowedRealm);
            changed = true;
        }

        int cappedEssence = CapEssenceBeforeAgeGate(actor, realm, ancientLaw, age, currentEssence);
        if (cappedEssence < MclslCultivationGrowthSystem.CurrentTrueEssence(actor))
        {
            MclslCultivationGrowthSystem.SetTrueEssence(
                actor,
                realm,
                cappedEssence,
                ancientLaw,
                enforceRealmMinimum: !string.IsNullOrWhiteSpace(realm));
            MclslActorAccessor.Set(actor, MclslActorDataKeys.LastBreakthroughResult,
                "年岁尚浅，异常真元已压回当前年龄可承载范围");
            MclslDiagnostics.Cultivation(
                "age_sanity.essence_cap",
                "actor=" + MclslActorAccessor.Id(actor)
                + " year=" + year
                + " age=" + age
                + " realm=" + realm
                + " before=" + currentEssence
                + " after=" + cappedEssence);
            changed = true;
        }

        if (storedScaleVersion < CurrentTrueEssenceScaleVersion
            && MclslCultivationActorMarker.HasCultivationMarker(actor))
        {
            MclslActorAccessor.Set(actor, MclslActorDataKeys.TrueEssenceScaleVersion, CurrentTrueEssenceScaleVersion);
            MclslDiagnostics.Cultivation(
                "age_sanity.scale_version",
                "actor=" + MclslActorAccessor.Id(actor)
                + " year=" + year
                + " age=" + age
                + " oldVersion=" + storedScaleVersion
                + " newVersion=" + CurrentTrueEssenceScaleVersion
                + " realm=" + MclslActorAccessor.Realm(actor)
                + " essence=" + MclslCultivationGrowthSystem.CurrentTrueEssence(actor));
            changed = true;
        }

        return changed;
    }

    internal static bool CanAttemptNextRealm(Actor actor, string currentRealm, out string reason)
    {
        reason = string.Empty;
        if (actor?.data == null || string.IsNullOrWhiteSpace(currentRealm)) return true;

        int currentIndex = MclslRealmIds.Index(currentRealm);
        if (currentIndex < 0 || currentIndex >= MclslRealmIds.Ordered.Length - 1) return true;

        string nextRealm = MclslRealmIds.Ordered[currentIndex + 1];
        int requiredAge = MinimumAgeForRealm(nextRealm);
        if (requiredAge <= 0) return true;

        int age = SafeAge(actor);
        if (age >= requiredAge) return true;

        reason = "年岁尚浅，至少" + requiredAge + "岁方可尝试" + MclslRealmIds.Display(nextRealm);
        return false;
    }

    private static int CapEssenceBeforeAgeGate(Actor actor, string realm, bool ancientLaw, int age, int currentEssence)
    {
        int cap = int.MaxValue;
        if (string.IsNullOrWhiteSpace(realm))
        {
            if (age < MinimumAgeForRealm(MclslRealmIds.LianQi))
                cap = MclslRealmProgress.LianQiEntryMinimum - 1;
            else
                cap = MclslRealmProgress.NextRealmMinimum(MclslRealmIds.LianQi, ancientLaw) - 1;
        }
        else if (!CanAttemptNextRealm(actor, realm, out _))
        {
            int nextMinimum = MclslRealmProgress.NextRealmMinimum(realm, ancientLaw);
            if (nextMinimum > 0) cap = nextMinimum - 1;
        }

        if (cap == int.MaxValue) return currentEssence;
        return Math.Clamp(currentEssence, 0, Math.Max(0, cap));
    }

    private static string HighestRealmAllowedAtAge(int age)
    {
        string allowed = string.Empty;
        for (int i = 0; i < MclslRealmIds.Ordered.Length; i++)
        {
            string realm = MclslRealmIds.Ordered[i];
            if (age < MinimumAgeForRealm(realm)) break;
            allowed = realm;
        }
        return allowed;
    }

    private static int MinimumAgeForRealm(string realm) => realm switch
    {
        MclslRealmIds.LianQi => 7,
        MclslRealmIds.ZhuJi => 18,
        MclslRealmIds.JinDan => 60,
        MclslRealmIds.YuanYing => 150,
        MclslRealmIds.HuaShen => 500,
        MclslRealmIds.HeDao => 1000,
        MclslRealmIds.ChangSheng => 1600,
        _ => 0
    };

    private static int SafeAge(Actor actor)
    {
        try { return Math.Max(0, (int)Math.Floor(actor?.getAge() ?? 0f)); }
        catch { return 0; }
    }
}
