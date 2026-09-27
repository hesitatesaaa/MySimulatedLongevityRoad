using System;
using MySimulatedLongevityRoad.Data;
using MySimulatedLongevityRoad.Traits;

namespace MySimulatedLongevityRoad.Systems;

internal static class MclslImmortalPathSystem
{
    private const int NaturalChanceOutOf10000 = 50;

    internal static string HighestTargetRealm(Actor actor)
    {
        int target = HighestTargetIndex(actor);
        return target < 0 ? string.Empty : MclslRealmIds.Ordered[target];
    }

    internal static bool AllowsBreakthrough(Actor actor, string targetRealm)
    {
        int target = MclslRealmIds.Index(targetRealm);
        int current = MclslRealmIds.Index(MclslActorAccessor.Realm(actor));
        return current >= 0 && target > current && target <= HighestTargetIndex(actor);
    }

    internal static bool IsBreakthroughProtected(Actor actor)
    {
        int current = MclslRealmIds.Index(MclslActorAccessor.Realm(actor));
        return current >= 0 && HighestTargetIndex(actor) > current;
    }

    internal static bool CanFillTechniqueLimit(Actor actor, string targetRealm)
        => AllowsBreakthrough(actor, targetRealm);

    internal static void TryNaturalGrant(Actor actor)
    {
        if (actor?.data == null || !MclslActorAccessor.Alive(actor)) return;

        // addTrait runs for normal grants, but traits restored from old saves may
        // already be present before Harmony observes the mutation. Reconcile the
        // strongest saved path once during the existing annual actor pass.
        string existingPath = HighestPathTrait(actor);
        if (!string.IsNullOrEmpty(existingPath))
        {
            OnTraitGranted(actor, existingPath);
            return;
        }

        if (!MclslEligibility.CanCultivate(actor)
            || MclslActorAccessor.GetInt(actor, MclslActorDataKeys.ImmortalPathChecked, 0) != 0) return;

        MclslActorAccessor.Set(actor, MclslActorDataKeys.ImmortalPathChecked, 1);
        int roll = PositiveHash(MclslActorAccessor.Id(actor) + "|immortal_path|first_qualification") % 10000;
        if (roll >= NaturalChanceOutOf10000) return;
        ActorTrait trait = AssetManager.traits.get(MclslTraitRegistration.PathHuaShenTraitId);
        if (trait != null && !actor.hasTrait(MclslTraitRegistration.PathHuaShenTraitId)) actor.addTrait(trait, true);
    }

    internal static void OnTraitGranted(Actor actor, string traitId)
    {
        if (actor?.data == null || TargetIndexForTrait(traitId) < 0) return;
        int highestOwned = HighestTargetIndex(actor);
        string[] pathTraits =
        {
            MclslTraitRegistration.PathHuaShenTraitId,
            MclslTraitRegistration.PathHeDaoTraitId,
            MclslTraitRegistration.PathChangShengTraitId
        };
        MclslTraitGrantRouter.SuppressRouting(() =>
        {
            foreach (string pathTrait in pathTraits)
            {
                if (!actor.hasTrait(pathTrait)) continue;
                int ownedTarget = TargetIndexForTrait(pathTrait);
                if (ownedTarget != highestOwned) actor.removeTrait(pathTrait);
            }
        });

        EnsureLinkedSpiritualRoot(actor, traitId);

        MclslActorAccessor.Set(actor, MclslActorDataKeys.ImmortalPathChecked, 1);
    }

    private static string HighestPathTrait(Actor actor)
    {
        if (actor?.data == null) return string.Empty;
        string highest = string.Empty;
        string[] pathTraits =
        {
            MclslTraitRegistration.PathHuaShenTraitId,
            MclslTraitRegistration.PathHeDaoTraitId,
            MclslTraitRegistration.PathChangShengTraitId
        };
        foreach (string pathTrait in pathTraits)
            if (actor.hasTrait(pathTrait)
                && (highest.Length == 0 || TargetIndexForTrait(pathTrait) > TargetIndexForTrait(highest)))
                highest = pathTrait;
        return highest;
    }

    private static void EnsureLinkedSpiritualRoot(Actor actor, string pathTrait)
    {
        int aptitude = pathTrait == MclslTraitRegistration.PathHuaShenTraitId ? 85 : 95;
        string rootTraitId = MclslTraitRegistration.GiftTraitIdForAptitude(aptitude);
        if (string.IsNullOrEmpty(rootTraitId) || actor.hasTrait(rootTraitId)) return;
        ActorTrait rootTrait = AssetManager.traits.get(rootTraitId);
        if (rootTrait != null) actor.addTrait(rootTrait, true);
    }

    internal static bool IsPathTrait(string traitId) => TargetIndexForTrait(traitId) >= 0;

    internal static int TargetIndexForTrait(string traitId)
    {
        if (traitId == MclslTraitRegistration.PathHuaShenTraitId) return MclslRealmIds.Index(MclslRealmIds.HuaShen);
        if (traitId == MclslTraitRegistration.PathHeDaoTraitId) return MclslRealmIds.Index(MclslRealmIds.HeDao);
        if (traitId == MclslTraitRegistration.PathChangShengTraitId) return MclslRealmIds.Index(MclslRealmIds.ChangSheng);
        return -1;
    }

    private static int HighestTargetIndex(Actor actor)
    {
        int target = -1;
        if (actor?.data == null) return target;
        string[] pathTraits =
        {
            MclslTraitRegistration.PathHuaShenTraitId,
            MclslTraitRegistration.PathHeDaoTraitId,
            MclslTraitRegistration.PathChangShengTraitId
        };
        foreach (string traitId in pathTraits)
            if (actor.hasTrait(traitId)) target = Math.Max(target, TargetIndexForTrait(traitId));
        return target;
    }

    private static int PositiveHash(string value)
    {
        unchecked
        {
            uint hash = 2166136261;
            foreach (char c in value ?? string.Empty) { hash ^= c; hash *= 16777619; }
            hash ^= hash >> 16;
            hash *= 0x7feb352d;
            hash ^= hash >> 15;
            hash *= 0x846ca68b;
            hash ^= hash >> 16;
            return (int)(hash & 0x7fffffff);
        }
    }
}
