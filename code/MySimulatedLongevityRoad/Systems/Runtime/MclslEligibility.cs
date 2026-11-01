using System;
using System.Collections.Generic;
using MySimulatedLongevityRoad.Data;
using MySimulatedLongevityRoad.Traits;

namespace MySimulatedLongevityRoad.Systems;

internal static class MclslEligibility
{
    internal const string RequiredBrainTraitPrefrontalCortex = "prefrontal_cortex";
    internal const string RequiredBrainTraitAdvancedHippocampus = "advanced_hippocampus";

    private static readonly Dictionary<ActorAsset, bool> CivilizedAssetCache = new();
    private static readonly Dictionary<ActorAsset, bool> AssetBrainCache = new();
    private static readonly Dictionary<Subspecies, (bool Sapient, bool RequiredBrain)> SubspeciesBrainCache = new();
    private static readonly HashSet<string> ExplicitCivilizedSpecies = new(StringComparer.OrdinalIgnoreCase)
    {
        "unit_human", "human",
        "unit_elf", "elf",
        "unit_dwarf", "dwarf",
        "unit_orc", "orc", "mclsl_xuanhuang"
    };

    private static readonly string[] LockedIdTokens =
    {
        "boat", "ship", "building", "house", "tower",
        "ghost", "skeleton", "zombie", "necromancer", "tumor",
        "demon", "dragon", "angel", "ufo", "robot",
        "crabzilla", "snowman", "evil_mage", "plague",
        "mclsl_world_soul", "world_soul"
    };

    internal static bool CanCultivate(Actor actor)
    {
        if (!MclslActorAccessor.Alive(actor) || actor?.asset == null) return false;
        string id = actor.asset.id ?? string.Empty;
        if (MclslBeastActorRegistration.IsBeast(actor)) return true;
        if (id is MclslImmortalActorRegistration.BaiId or MclslImmortalActorRegistration.ChuanfaId) return true;
        if (IsLockedAsset(actor.asset, id)) return false;
        return IsCivilizedActor(actor) || HasRequiredCultivationBrain(actor);
    }

    internal static bool CanOwnSpecialPhysique(Actor actor)
        => TryGetSpecialPhysiqueEligibility(actor, out bool eligible) && eligible;

    internal static bool TryGetSpecialPhysiqueEligibility(Actor actor, out bool eligible)
    {
        eligible = false;
        if (actor?.data == null || actor.asset == null || MclslActorAccessor.Id(actor) <= 0L) return false;
        string id = actor.asset.id ?? string.Empty;
        if (id != "mclsl_xuanhuang" && id.StartsWith("mclsl_", StringComparison.OrdinalIgnoreCase)
            || MclslWorldSoulActorRegistration.IsWorldSoulAssetId(id)) return true;
        // Native Actor.isSapient returns false before a subspecies is attached.
        // Treat that state as pending, not as a completed negative birth roll.
        if (actor.subspecies == null) return false;
        eligible = actor.isSapient();
        return true;
    }

    // Death routers call this after the native actor has already died, so this check must not depend on isAlive().
    internal static bool IsCivilizedActor(Actor actor)
    {
        if (actor?.asset == null) return false;
        if (MclslBeastActorRegistration.IsOrdinary(actor)) return false;
        if (CivilizedAssetCache.TryGetValue(actor.asset, out bool cached)) return cached && BrainState(actor.subspecies).Sapient;
        string id = actor.asset.id ?? string.Empty;
        if (id is MclslImmortalActorRegistration.BaiId or MclslImmortalActorRegistration.ChuanfaId) return true;
        bool result = !IsLockedAsset(actor.asset, id)
            && (ExplicitCivilizedSpecies.Contains(id) || !string.IsNullOrWhiteSpace(actor.asset.kingdom_id_civilization));
        CivilizedAssetCache[actor.asset] = result;
        return result && BrainState(actor.subspecies).Sapient;
    }

    internal static bool CanClaimWorldSoul(Actor actor)
    {
        if (MclslBeastActorRegistration.IsOrdinary(actor)) return false;
        if (!CanCultivate(actor)) return false;
        if (!string.IsNullOrWhiteSpace(MclslActorAccessor.GetString(actor, MclslActorDataKeys.WorldSoulEntityId, string.Empty))) return false;
        int realm = MclslRealmIds.Index(MclslActorAccessor.Realm(actor));
        if (realm >= MclslRealmIds.Index(MclslRealmIds.HeDao)) return false;
        return true;
    }

    internal static bool HasRequiredCultivationBrain(Actor actor)
        => actor?.asset != null && (actor.subspecies != null
            ? BrainState(actor.subspecies).RequiredBrain : AssetHasRequiredCultivationBrain(actor.asset));

    internal static bool Refresh(Subspecies subspecies)
    {
        var next = ReadBrainState(subspecies);
        bool changed = !SubspeciesBrainCache.TryGetValue(subspecies, out var previous) || previous != next;
        SubspeciesBrainCache[subspecies] = next;
        return changed;
    }

    internal static void Clear()
    {
        CivilizedAssetCache.Clear();
        AssetBrainCache.Clear();
        SubspeciesBrainCache.Clear();
    }

    private static bool AssetHasRequiredCultivationBrain(ActorAsset asset)
    {
        if (asset == null) return false;
        if (AssetBrainCache.TryGetValue(asset, out bool cached)) return cached;
        bool result =
            (HasTraitId(asset.traits, RequiredBrainTraitPrefrontalCortex)
                || HasTraitId(asset.default_subspecies_traits, RequiredBrainTraitPrefrontalCortex))
            && (HasTraitId(asset.traits, RequiredBrainTraitAdvancedHippocampus)
                || HasTraitId(asset.default_subspecies_traits, RequiredBrainTraitAdvancedHippocampus));
        AssetBrainCache[asset] = result;
        return result;
    }

    private static (bool Sapient, bool RequiredBrain) BrainState(Subspecies subspecies)
    {
        if (subspecies == null) return default;
        if (!SubspeciesBrainCache.TryGetValue(subspecies, out var state))
            SubspeciesBrainCache[subspecies] = state = ReadBrainState(subspecies);
        return state;
    }

    private static (bool Sapient, bool RequiredBrain) ReadBrainState(Subspecies subspecies)
        => (subspecies.isSapient(), subspecies.hasTrait(RequiredBrainTraitPrefrontalCortex)
            && subspecies.hasTrait(RequiredBrainTraitAdvancedHippocampus));

    private static bool HasTraitId(List<string> traits, string traitId)
    {
        if (traits == null || string.IsNullOrWhiteSpace(traitId)) return false;
        for (int i = 0; i < traits.Count; i++)
            if (string.Equals(traits[i], traitId, StringComparison.OrdinalIgnoreCase))
                return true;
        return false;
    }

    private static bool IsLockedAsset(ActorAsset asset, string id)
    {
        if (asset == null) return true;
        if (asset.is_boat) return true;
        for (int i = 0; i < LockedIdTokens.Length; i++)
            if (id.IndexOf(LockedIdTokens[i], StringComparison.OrdinalIgnoreCase) >= 0)
                return true;
        return false;
    }

}
