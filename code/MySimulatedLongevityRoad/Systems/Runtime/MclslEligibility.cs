using System;
using System.Collections.Generic;
using MySimulatedLongevityRoad.Data;

namespace MySimulatedLongevityRoad.Systems;

internal static class MclslEligibility
{
    internal const string RequiredBrainTraitPrefrontalCortex = "prefrontal_cortex";
    internal const string RequiredBrainTraitAdvancedHippocampus = "advanced_hippocampus";

    private static readonly Dictionary<ActorAsset, bool> CivilizedAssetCache = new();
    private static readonly Dictionary<ActorAsset, bool> AssetBrainCache = new();
    private static readonly Dictionary<Subspecies, bool> SubspeciesBrainCache = new();
    private static readonly HashSet<string> ExplicitCivilizedSpecies = new(StringComparer.OrdinalIgnoreCase)
    {
        "unit_human", "human",
        "unit_elf", "elf",
        "unit_dwarf", "dwarf",
        "unit_orc", "orc"
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
        if (IsLockedAsset(actor.asset, id)) return false;
        return IsCivilizedActor(actor) || HasRequiredCultivationBrain(actor);
    }

    // Death routers call this after the native actor has already died, so this check must not depend on isAlive().
    internal static bool IsCivilizedActor(Actor actor)
    {
        if (actor?.asset == null) return false;
        if (CivilizedAssetCache.TryGetValue(actor.asset, out bool cached)) return cached;
        string id = actor.asset.id ?? string.Empty;
        bool result = !IsLockedAsset(actor.asset, id)
            && (ExplicitCivilizedSpecies.Contains(id) || IsCivilizedSapient(actor));
        CivilizedAssetCache[actor.asset] = result;
        return result;
    }

    internal static bool CanClaimWorldSoul(Actor actor)
    {
        if (!CanCultivate(actor)) return false;
        if (!string.IsNullOrWhiteSpace(MclslActorAccessor.GetString(actor, MclslActorDataKeys.WorldSoulEntityId, string.Empty))) return false;
        int realm = MclslRealmIds.Index(MclslActorAccessor.Realm(actor));
        if (realm >= MclslRealmIds.Index(MclslRealmIds.HeDao)) return false;
        return true;
    }

    internal static bool HasRequiredCultivationBrain(Actor actor)
    {
        return actor?.asset != null
            && (AssetHasRequiredCultivationBrain(actor.asset)
                || SubspeciesHasRequiredCultivationBrain(actor.subspecies));
    }

    internal static void Clear()
    {
        CivilizedAssetCache.Clear();
        AssetBrainCache.Clear();
        SubspeciesBrainCache.Clear();
    }

    private static bool IsCivilizedSapient(Actor actor)
    {
        try
        {
            return actor != null
                && actor.isSapient()
                && !string.IsNullOrWhiteSpace(actor.asset?.kingdom_id_civilization);
        }
        catch (System.Exception mclslEmptyCatchEx)
        {
            MySimulatedLongevityRoad.Core.MclslDiagnostics.Error(
                "mclsl-eligibility-sapient",
                "判断文明物种修炼资格失败: " + mclslEmptyCatchEx.Message);
            return false;
        }
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

    private static bool SubspeciesHasRequiredCultivationBrain(Subspecies subspecies)
    {
        if (subspecies == null) return false;
        if (SubspeciesBrainCache.TryGetValue(subspecies, out bool cached)) return cached;
        bool result = false;
        try
        {
            result =
                (HasTraitId(subspecies.default_traits, RequiredBrainTraitPrefrontalCortex)
                    || HasTraitId(subspecies.saved_traits, RequiredBrainTraitPrefrontalCortex))
                && (HasTraitId(subspecies.default_traits, RequiredBrainTraitAdvancedHippocampus)
                    || HasTraitId(subspecies.saved_traits, RequiredBrainTraitAdvancedHippocampus));
        }
        catch (System.Exception mclslEmptyCatchEx)
        {
            MySimulatedLongevityRoad.Core.MclslDiagnostics.Error(
                "mclsl-eligibility-subspecies-brain",
                "判断亚种修炼资格失败: " + mclslEmptyCatchEx.Message);
        }

        SubspeciesBrainCache[subspecies] = result;
        return result;
    }

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
        try { if (asset.is_boat) return true; } catch (System.Exception mclslEmptyCatchEx) { MySimulatedLongevityRoad.Core.MclslDiagnostics.Error("empty-catch-code-MySimulatedLongevityRoad-Systems-Runtime-MclslEligibility-cs-1", "空 catch 捕获: code/MySimulatedLongevityRoad/Systems/Runtime/MclslEligibility.cs #1: " + mclslEmptyCatchEx.Message); }
        for (int i = 0; i < LockedIdTokens.Length; i++)
            if (id.IndexOf(LockedIdTokens[i], StringComparison.OrdinalIgnoreCase) >= 0)
                return true;
        return false;
    }

}
