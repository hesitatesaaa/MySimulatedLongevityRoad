using System;
using MySimulatedLongevityRoad.Core;
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

    // 换法时用实际获得的传承定义校准，不能拿母法替代残缺/生成传承。
    // 已有残篇提升与角色境界只作下限，普通初始化仍不重置这些成果。
    internal static void OnTechniqueAssigned(Actor actor, MclslTechniqueDefinition technique)
    {
        if (actor?.data == null || technique == null) return;
        string previous = MclslActorAccessor.GetString(actor, MclslActorDataKeys.TechniqueMaxRealm, string.Empty);
        string target = MclslRealmIds.Index(previous) >= 0 ? previous : technique.MaxRealm;
        if (MclslRealmIds.Index(technique.MaxRealm) > MclslRealmIds.Index(target)) target = technique.MaxRealm;
        string realm = MclslActorAccessor.Realm(actor);
        if (MclslRealmIds.Index(realm) > MclslRealmIds.Index(target)) target = realm;
        SetMaxRealm(actor, target);
        MclslDiagnostics.Cultivation("technique.assigned_limit",
            "actor=" + MclslActorAccessor.Id(actor) + " technique=" + technique.Id
            + " realm=" + realm + " limit=" + previous + "->"
            + MclslActorAccessor.GetString(actor, MclslActorDataKeys.TechniqueMaxRealm, string.Empty));
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
        string current = StoredMaxRealm(actor);
        int currentIndex = MclslRealmIds.Index(current);
        if (currentIndex < target) SetMaxRealm(actor, MclslRealmIds.Ordered[target]);
    }

    internal static string MaxRealm(Actor actor)
    {
        if (actor?.data == null) return MclslRealmIds.ZhuJi;
        string effective = StoredMaxRealm(actor);
        string pathTarget = MclslImmortalPathSystem.HighestTargetRealm(actor);
        if (pathTarget == MclslRealmIds.ChangSheng) pathTarget = MclslRealmIds.HeDao;
        if (MclslRealmIds.Index(pathTarget) > MclslRealmIds.Index(effective)) effective = pathTarget;
        return effective;
    }

    private static string StoredMaxRealm(Actor actor)
    {
        string stored = MclslActorAccessor.GetString(actor, MclslActorDataKeys.TechniqueMaxRealm, string.Empty);
        string techniqueId = MclslActorAccessor.GetString(actor, MclslActorDataKeys.TechniqueId, string.Empty);
        string normalized = MclslCultivationCatalog.NormalizeTechniqueId(techniqueId);
        MclslTechniqueDefinition known = null;
        // legacy_* 的母法并非实际传承的上限；自创分支也必须保留独立数据。
        bool knownDefinition = !normalized.StartsWith("legacy_", StringComparison.Ordinal)
            && MclslCultivationCatalog.TryTechnique(techniqueId, out known);
        if (MclslRealmIds.Index(stored) >= 0)
        {
            if (knownDefinition && MclslRealmIds.Index(known.MaxRealm) > MclslRealmIds.Index(stored))
            {
                OnTechniqueAssigned(actor, known);
                return MclslActorAccessor.GetString(actor, MclslActorDataKeys.TechniqueMaxRealm, stored);
            }
            return stored;
        }
        string realm = MclslActorAccessor.Realm(actor);
        // 无有效缓存且无法确认实际定义时，只保证现有境界，不猜测母法上限。
        string fallback = MclslRealmIds.Index(realm) > MclslRealmIds.Index(MclslRealmIds.ZhuJi)
            ? realm : MclslRealmIds.ZhuJi;
        SetMaxRealm(actor, knownDefinition ? known.MaxRealm : fallback);
        return MclslActorAccessor.GetString(actor, MclslActorDataKeys.TechniqueMaxRealm, MclslRealmIds.ZhuJi);
    }

    internal static bool CanReach(Actor actor, string targetRealm, out string reason)
    {
        reason = string.Empty;
        if (actor?.data == null || string.IsNullOrWhiteSpace(targetRealm)) return true;
        if (targetRealm == MclslRealmIds.ChangSheng) return true;
        if (MclslImmortalPathSystem.CanFillTechniqueLimit(actor, targetRealm)) return true;
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
