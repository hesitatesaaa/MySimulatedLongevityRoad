using System;
using MySimulatedLongevityRoad.Data;

namespace MySimulatedLongevityRoad.Systems;

internal static class MclslTechniqueRealmLimit
{
    internal static void EnsureFromDefinition(Actor actor, MclslTechniqueDefinition technique)
    {
        if (actor?.data == null || technique == null) return;
        string current = MclslActorAccessor.GetString(actor, MclslActorDataKeys.TechniqueMaxRealm, string.Empty);
        if (!string.IsNullOrWhiteSpace(current) && MclslRealmIds.Index(current) >= 0) return;
        SetMaxRealm(actor, technique.MaxRealm);
    }

    internal static void SetMaxRealm(Actor actor, string realm)
    {
        if (actor?.data == null) return;
        string value = MclslRealmIds.Index(realm) >= 0 ? realm : MclslRealmIds.ZhuJi;
        if (value == MclslRealmIds.ChangSheng) value = MclslRealmIds.HeDao;
        MclslActorAccessor.Set(actor, MclslActorDataKeys.TechniqueMaxRealm, value);
    }

    internal static void EnsureAtLeastRealm(Actor actor, string realm)
    {
        if (actor?.data == null) return;
        if (realm == MclslRealmIds.ChangSheng) realm = MclslRealmIds.HeDao;
        int target = MclslRealmIds.Index(realm);
        if (target < 0) return;
        string current = MaxRealm(actor);
        int currentIndex = MclslRealmIds.Index(current);
        if (currentIndex < target) SetMaxRealm(actor, MclslRealmIds.Ordered[target]);
    }

    internal static string MaxRealm(Actor actor)
    {
        if (actor?.data == null) return MclslRealmIds.ZhuJi;
        string stored = MclslActorAccessor.GetString(actor, MclslActorDataKeys.TechniqueMaxRealm, string.Empty);
        if (MclslRealmIds.Index(stored) >= 0) return stored;
        string techniqueId = MclslActorAccessor.GetString(actor, MclslActorDataKeys.TechniqueId, string.Empty);
        MclslTechniqueDefinition definition = MclslCultivationCatalog.Technique(techniqueId);
        SetMaxRealm(actor, definition.MaxRealm);
        return definition.MaxRealm;
    }

    internal static bool CanReach(Actor actor, string targetRealm, out string reason)
    {
        reason = string.Empty;
        if (actor?.data == null || string.IsNullOrWhiteSpace(targetRealm)) return true;
        if (targetRealm == MclslRealmIds.ChangSheng) return true;
        string maxRealm = MaxRealm(actor);
        if (MclslRealmIds.Index(targetRealm) <= MclslRealmIds.Index(maxRealm)) return true;
        string technique = MclslActorAccessor.GetString(actor, MclslActorDataKeys.TechniqueName, "所修功法");
        reason = "主修《" + technique + "》止于" + MclslRealmIds.Display(maxRealm) + "，需另得更高传承";
        return false;
    }

    internal static void SetProceduralBranchLimit(Actor actor, int aptitude)
    {
        if (actor?.data == null) return;
        int current = Math.Max(0, MclslRealmIds.Index(MclslActorAccessor.Realm(actor)));
        int bonus = aptitude >= 95 ? 2 : aptitude >= 70 ? 1 : 0;
        int target = Math.Clamp(current + 1 + bonus, MclslRealmIds.Index(MclslRealmIds.ZhuJi), MclslRealmIds.Index(MclslRealmIds.HeDao));
        SetMaxRealm(actor, MclslRealmIds.Ordered[target]);
    }

    internal static bool TryRaiseLimit(Actor actor, out string newMaxRealm)
    {
        newMaxRealm = string.Empty;
        if (actor?.data == null) return false;
        string current = MaxRealm(actor);
        int index = MclslRealmIds.Index(current);
        if (index < 0 || index >= MclslRealmIds.Index(MclslRealmIds.HeDao)) return false;
        index++;
        newMaxRealm = MclslRealmIds.Ordered[index];
        SetMaxRealm(actor, newMaxRealm);
        return true;
    }

    internal static string DisplayMaxRealm(Actor actor)
    {
        if (actor?.data == null) return string.Empty;
        if (string.IsNullOrWhiteSpace(MclslActorAccessor.GetString(actor, MclslActorDataKeys.TechniqueId, string.Empty))
            && string.IsNullOrWhiteSpace(MclslActorAccessor.GetString(actor, MclslActorDataKeys.TechniqueName, string.Empty)))
            return string.Empty;
        string realm = MaxRealm(actor);
        return MclslRealmIds.Index(realm) >= 0 ? MclslRealmIds.Display(realm) : string.Empty;
    }
}
