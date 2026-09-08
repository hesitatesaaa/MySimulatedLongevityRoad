using System;
using System.Collections.Generic;
using MySimulatedLongevityRoad.Core;
using MySimulatedLongevityRoad.Data;
using MySimulatedLongevityRoad.Queries;
using MySimulatedLongevityRoad.Systems;
using NeoModLoader.General;
using UnityEngine;

namespace MySimulatedLongevityRoad.UI;

internal static class MclslRankSnapshotSource
{
    private const int CacheFrames = 30;
    private static readonly List<MclslRankEntry> CachedEntries = new();
    private static readonly List<MclslRankKingdomFilterChoice> CachedKingdomChoices = new();
    private static readonly List<MclslRankAssetFilterChoice> CachedAssetChoices = new();
    private static readonly List<MclslRankTraitFilterChoice> CachedTraitChoices = new();
    private static int _cachedYear = -1;
    private static int _cachedActorCount = -1;
    private static long _cachedIndexRevision = -1L;
    private static int _cachedFrame = -100000;
    private static bool _isBuilding;
    private static bool _invalidateAfterBuild;

    internal static IReadOnlyList<MclslRankEntry> EntriesSnapshot(bool forceRebuild = false)
    {
        IReadOnlyList<Actor> actors = MclslCultivatorCandidateIndex.GetCultivatorActorsSnapshot();
        int year = MclslRuntime.CurrentYear();
        long indexRevision = MclslCultivatorCandidateIndex.Revision;
        if (!forceRebuild
            && _cachedYear == year
            && _cachedActorCount == actors.Count
            && _cachedIndexRevision == indexRevision
            && Time.frameCount - _cachedFrame < CacheFrames)
            return CachedEntries;

        // 排行榜采用“局部构建，完成后一次性提交”。任何角色读取异常或
        // 构建期间发生的缓存失效都不能清空已完成的前序角色。
        List<MclslRankEntry> nextEntries = new(actors.Count);
        List<MclslRankKingdomFilterChoice> nextKingdomChoices = new();
        List<MclslRankAssetFilterChoice> nextAssetChoices = new();
        List<MclslRankTraitFilterChoice> nextTraitChoices = new();
        HashSet<long> nextKingdomIds = new();
        HashSet<string> nextAssetIds = new(StringComparer.Ordinal);
        HashSet<string> nextTraitIds = new(StringComparer.Ordinal);

        _isBuilding = true;
        _invalidateAfterBuild = false;
        try
        {
            for (int i = 0; i < actors.Count; i++)
            {
                Actor actor = actors[i];
                if (actor?.data == null || !MclslActorAccessor.Alive(actor)) continue;

                MclslRankEntry entry = BuildEntrySafely(actor);
                if (entry == null) continue;
                nextEntries.Add(entry);

                try
                {
                    CaptureFilterChoices(
                        actor,
                        nextKingdomChoices,
                        nextAssetChoices,
                        nextTraitChoices,
                        nextKingdomIds,
                        nextAssetIds,
                        nextTraitIds);
                }
                catch (Exception ex)
                {
                    MclslDiagnostics.Error(
                        "rank-filter-choice:" + MclslActorAccessor.Id(actor),
                        "排行榜筛选项构建已跳过单个异常角色: " + ex.Message);
                }
            }

            nextTraitChoices.Sort((left, right) =>
            {
                int order = TraitFilterOrder(left?.TraitId).CompareTo(TraitFilterOrder(right?.TraitId));
                return order != 0 ? order : string.Compare(left?.TraitId, right?.TraitId, StringComparison.Ordinal);
            });

            CachedEntries.Clear();
            CachedEntries.AddRange(nextEntries);
            CachedKingdomChoices.Clear();
            CachedKingdomChoices.AddRange(nextKingdomChoices);
            CachedAssetChoices.Clear();
            CachedAssetChoices.AddRange(nextAssetChoices);
            CachedTraitChoices.Clear();
            CachedTraitChoices.AddRange(nextTraitChoices);

            _cachedYear = year;
            _cachedActorCount = actors.Count;
            _cachedIndexRevision = indexRevision;
            _cachedFrame = Time.frameCount;
        }
        finally
        {
            _isBuilding = false;
            if (_invalidateAfterBuild)
            {
                // 保留本次已完成的原子快照供当前调用使用，只把缓存标为过期；
                // 下一次读取会基于最新索引重新构建。
                _cachedYear = -1;
                _cachedActorCount = -1;
                _cachedIndexRevision = -1L;
                _cachedFrame = -100000;
                _invalidateAfterBuild = false;
            }
        }

        if (CachedEntries.Count != actors.Count)
        {
            MclslDiagnostics.Error(
                "rank-snapshot:count-mismatch",
                "排行榜快照人数与修士索引不一致: entries=" + CachedEntries.Count
                + " actors=" + actors.Count);
        }
        return CachedEntries;
    }

    private static MclslRankEntry BuildEntrySafely(Actor actor)
    {
        try
        {
            return BuildEntry(actor);
        }
        catch (Exception ex)
        {
            MclslDiagnostics.Error(
                "rank-entry:" + MclslActorAccessor.Id(actor),
                "排行榜角色快照降级构建: " + ex.Message);
        }

        try
        {
            return BuildFallbackEntry(actor);
        }
        catch (Exception ex)
        {
            MclslDiagnostics.Error(
                "rank-entry-fallback:" + MclslActorAccessor.Id(actor),
                "排行榜角色快照最低降级构建: " + ex.Message);
            return BuildMinimalEntry(actor);
        }
    }

    internal static IReadOnlyList<MclslRankKingdomFilterChoice> KingdomFilterChoicesSnapshot()
    {
        EntriesSnapshot();
        return CachedKingdomChoices;
    }

    internal static IReadOnlyList<MclslRankAssetFilterChoice> AssetFilterChoicesSnapshot()
    {
        EntriesSnapshot();
        return CachedAssetChoices;
    }

    internal static IReadOnlyList<MclslRankTraitFilterChoice> TraitFilterChoicesSnapshot()
    {
        EntriesSnapshot();
        return CachedTraitChoices;
    }

    internal static void Invalidate()
    {
        if (_isBuilding)
        {
            _invalidateAfterBuild = true;
            return;
        }

        _cachedYear = -1;
        _cachedActorCount = -1;
        _cachedIndexRevision = -1L;
        _cachedFrame = -100000;
        CachedEntries.Clear();
        CachedKingdomChoices.Clear();
        CachedAssetChoices.Clear();
        CachedTraitChoices.Clear();
    }

    private static MclslRankEntry BuildEntry(Actor actor)
    {
        MclslActorCultivationView view = MclslActorCultivationQuery.Build(actor);
        int realmIndex = string.IsNullOrWhiteSpace(view.RealmId) ? -1 : MclslRealmIds.Index(view.RealmId);
        double power = CalculatePower(actor, view, realmIndex);
        return new MclslRankEntry
        {
            Actor = actor,
            ActorId = MclslActorAccessor.Id(actor),
            Name = view.Name,
            RealmId = view.RealmId,
            RealmName = view.RealmName,
            GiftName = view.GiftName,
            RootText = string.IsNullOrWhiteSpace(view.SpiritualRootAttributes) ? view.GiftName : view.SpiritualRootAttributes.Split('、')[0],
            RootAttributes = view.SpiritualRootAttributes,
            NormalizedSearchText = NormalizeSearch(view.Name) + NormalizeSearch(view.RealmName) + NormalizeSearch(view.SpiritualRootAttributes),
            ExtraText = Extra(view),
            Power = power,
            RealmIndex = realmIndex,
            Aptitude = view.Aptitude,
            TrueEssence = view.TrueEssence,
            Contribution = view.Contribution,
            SpiritStones = view.SpiritStones,
            MindState = view.MindState,
            MortalMiasma = view.MortalMiasma,
            MortalMiasmaLimit = view.MortalMiasmaLimit
        };
    }

    private static MclslRankEntry BuildFallbackEntry(Actor actor)
    {
        string realm = MclslActorAccessor.Realm(actor);
        string realmName = string.IsNullOrWhiteSpace(realm) ? "感气" : MclslRealmIds.Display(realm);
        string rootAttributes = MclslActorAccessor.GetString(
            actor,
            MclslActorDataKeys.SpiritualRootAttributes,
            string.Empty);
        if (string.IsNullOrWhiteSpace(rootAttributes))
        {
            rootAttributes = MclslActorAccessor.GetString(
                actor,
                MclslActorDataKeys.SpiritualRootPrimary,
                "未明灵根");
        }
        string rootText = rootAttributes.Split('、')[0];
        string name;
        try { name = MclslActorAccessor.DisplayName(actor); }
        catch { name = "修士" + MclslActorAccessor.Id(actor); }
        int realmIndex = string.IsNullOrWhiteSpace(realm) ? -1 : MclslRealmIds.Index(realm);
        int essence = MclslCultivationGrowthSystem.CurrentTrueEssence(actor);
        return new MclslRankEntry
        {
            Actor = actor,
            ActorId = MclslActorAccessor.Id(actor),
            Name = string.IsNullOrWhiteSpace(name) ? "无名修士" : name,
            RealmId = realm,
            RealmName = realmName,
            GiftName = rootText,
            RootText = rootText,
            RootAttributes = rootAttributes,
            NormalizedSearchText = NormalizeSearch(name) + NormalizeSearch(realmName) + NormalizeSearch(rootAttributes),
            ExtraText = string.IsNullOrWhiteSpace(realm)
                ? essence + "/" + MclslRealmProgress.LianQiEntryMinimum
                : string.Empty,
            Power = Math.Max(1d, realmIndex + 1),
            RealmIndex = realmIndex,
            Aptitude = Math.Max(0, MclslActorAccessor.GetInt(actor, MclslActorDataKeys.Aptitude, 0)),
            TrueEssence = essence,
            Contribution = Math.Max(0, MclslActorAccessor.GetInt(actor, MclslActorDataKeys.Contribution, 0)),
            SpiritStones = Math.Max(0, MclslActorAccessor.GetInt(actor, MclslActorDataKeys.SpiritStones, 0)),
            MindState = Math.Max(0, MclslActorAccessor.GetInt(actor, MclslActorDataKeys.MindState, 0)),
            MortalMiasma = Math.Max(0, MclslActorAccessor.GetInt(actor, MclslActorDataKeys.MortalMiasma, 0)),
            MortalMiasmaLimit = 0
        };
    }

    private static MclslRankEntry BuildMinimalEntry(Actor actor)
    {
        long actorId = 0L;
        try { actorId = MclslActorAccessor.Id(actor); } catch { }
        string name = "修士" + actorId;
        try
        {
            string candidate = actor?.getName();
            if (!string.IsNullOrWhiteSpace(candidate)) name = candidate.Trim();
        }
        catch { }
        string realm = string.Empty;
        try { realm = MclslActorAccessor.Realm(actor); } catch { }
        string realmName = string.IsNullOrWhiteSpace(realm) ? "感气" : MclslRealmIds.Display(realm);
        int essence = 0;
        try { essence = MclslActorAccessor.GetInt(actor, MclslActorDataKeys.TrueEssence, 0); } catch { }
        return new MclslRankEntry
        {
            Actor = actor,
            ActorId = actorId,
            Name = name,
            RealmId = realm,
            RealmName = realmName,
            GiftName = "灵根",
            RootText = "未明",
            RootAttributes = "未明",
            NormalizedSearchText = NormalizeSearch(name) + NormalizeSearch(realmName),
            ExtraText = string.IsNullOrWhiteSpace(realm)
                ? Math.Max(0, essence) + "/" + MclslRealmProgress.LianQiEntryMinimum
                : string.Empty,
            Power = 1d,
            RealmIndex = string.IsNullOrWhiteSpace(realm) ? -1 : MclslRealmIds.Index(realm),
            TrueEssence = Math.Max(0, essence)
        };
    }

    private static void CaptureFilterChoices(
        Actor actor,
        List<MclslRankKingdomFilterChoice> kingdomChoices,
        List<MclslRankAssetFilterChoice> assetChoices,
        List<MclslRankTraitFilterChoice> traitChoices,
        HashSet<long> kingdomIds,
        HashSet<string> assetIds,
        HashSet<string> traitIds)
    {
        Kingdom kingdom = actor?.kingdom;
        if (kingdom?.data != null && kingdomIds.Add(kingdom.data.id))
        {
            kingdomChoices.Add(new MclslRankKingdomFilterChoice
            {
                Kingdom = kingdom,
                KingdomId = kingdom.data.id,
                DisplayName = string.IsNullOrWhiteSpace(kingdom.data.name) ? "未知国家" : kingdom.data.name
            });
        }

        ActorAsset asset = actor?.asset;
        if (asset != null && !string.IsNullOrWhiteSpace(asset.id) && assetIds.Add(asset.id))
        {
            assetChoices.Add(new MclslRankAssetFilterChoice
            {
                Asset = asset,
                AssetId = asset.id,
                DisplayName = GetAssetDisplayName(asset)
            });
        }

        if (actor?.traits == null) return;
        foreach (ActorTrait trait in actor.traits)
        {
            if (ShouldHideTraitFilterChoice(trait) || !traitIds.Add(trait.id)) continue;
            traitChoices.Add(new MclslRankTraitFilterChoice
            {
                TraitId = trait.id,
                DisplayName = GetTraitDisplayName(trait)
            });
        }
    }

    private static double CalculatePower(Actor actor, MclslActorCultivationView view, int realmIndex)
    {
        if (!MclslActorAccessor.Alive(actor)) return 0d;
        float attack = GetStatSafe(actor, "damage");
        float health = GetStatSafe(actor, "health");
        double baseStats = Math.Max(1d, attack + health);
        double power = RealmPowerWeight(view.RealmId, realmIndex) * baseStats;
        power *= 1d + Math.Clamp(view.MindState, 0, 100) / 500d;
        if (view.RealmId == MclslRealmIds.HeDao)
            power *= 1d + Math.Clamp(view.HarmonyStability, 0, 100) / 400d;
        if (view.RealmId == MclslRealmIds.ChangSheng)
        {
            power *= 1d + Math.Clamp(view.InverseTruthProgress, 0, 100) / 300d;
            if (view.TaishangProgress >= 100) power *= 1.2d;
        }
        if (double.IsNaN(power) || power <= 0d) return 0d;
        return Math.Min(float.MaxValue, power);
    }

    private static float GetStatSafe(Actor actor, string statId)
    {
        try { return actor?.stats == null ? 0f : Math.Max(0f, actor.stats[statId]); }
        catch { return 0f; }
    }

    private static int RealmPowerWeight(string realmId, int realmIndex)
    {
        return realmId switch
        {
            MclslRealmIds.LianQi => MclslRealmProgress.MinimumForRealm(MclslRealmIds.LianQi),
            MclslRealmIds.ZhuJi => MclslRealmProgress.MinimumForRealm(MclslRealmIds.ZhuJi),
            MclslRealmIds.JinDan => MclslRealmProgress.MinimumForRealm(MclslRealmIds.JinDan),
            MclslRealmIds.YuanYing => MclslRealmProgress.MinimumForRealm(MclslRealmIds.YuanYing),
            MclslRealmIds.HuaShen => MclslRealmProgress.MinimumForRealm(MclslRealmIds.HuaShen),
            MclslRealmIds.HeDao => 129600,
            MclslRealmIds.ChangSheng => 259200,
            _ => Math.Max(1, realmIndex + 1)
        };
    }

    private static string Extra(MclslActorCultivationView view) => view.RealmId switch
    {
        MclslRealmIds.JinDan => view.GoldenCorePurity > 0 ? view.GoldenCorePurity + "纯" : "悟法",
        MclslRealmIds.YuanYing => view.NascentCaveIntegrity > 0 ? view.NascentCaveIntegrity + "洞" : "洞天",
        MclslRealmIds.HuaShen => view.DivineChangeCompatibility > 0 ? view.DivineChangeCompatibility + "髓" : "抽髓",
        MclslRealmIds.HeDao => view.HarmonyStability > 0 ? view.HarmonyStability + "稳" : "祭魄",
        MclslRealmIds.ChangSheng => view.TaishangProgress >= 100 ? "太上" : view.TaishangProgress + "太上",
        _ when string.IsNullOrWhiteSpace(view.RealmId) && view.TrueEssence > 0
            => view.TrueEssence + "/" + MclslRealmProgress.LianQiEntryMinimum,
        _ => view.NextRealmMinimum > 0 ? view.CultivationProgress.ToString("0", System.Globalization.CultureInfo.InvariantCulture) + "%" : string.Empty
    };

    private static string NormalizeSearch(string value) => (value ?? string.Empty).Trim().Replace(" ", string.Empty).Replace("　", string.Empty);

    private static string GetAssetDisplayName(ActorAsset asset)
    {
        if (asset == null) return "未知生物";
        try
        {
            string name = asset.getLocalizedName();
            if (!string.IsNullOrWhiteSpace(name)) return name;
        }
        catch (System.Exception mclslEmptyCatchEx) { MySimulatedLongevityRoad.Core.MclslDiagnostics.Error("empty-catch-code-MySimulatedLongevityRoad-UI-MclslRankSnapshotSource-cs-1", "空 catch 捕获: code/MySimulatedLongevityRoad/UI/MclslRankSnapshotSource.cs #1: " + mclslEmptyCatchEx.Message); }
        try
        {
            string name = LM.Get(asset.name_locale);
            if (!string.IsNullOrWhiteSpace(name)) return name;
        }
        catch (System.Exception mclslEmptyCatchEx) { MySimulatedLongevityRoad.Core.MclslDiagnostics.Error("empty-catch-code-MySimulatedLongevityRoad-UI-MclslRankSnapshotSource-cs-2", "空 catch 捕获: code/MySimulatedLongevityRoad/UI/MclslRankSnapshotSource.cs #2: " + mclslEmptyCatchEx.Message); }
        return "未知生物";
    }

    private static string GetTraitDisplayName(ActorTrait trait)
    {
        if (trait == null || string.IsNullOrWhiteSpace(trait.id)) return "未知特质";
        if (MclslLocalizationBridge.TryResolveRuntimeKey(trait.id, out string runtimeText)) return runtimeText;
        try
        {
            string localized = LM.Get("trait_" + trait.id);
            if (!string.IsNullOrWhiteSpace(localized)
                && !string.Equals(localized, "trait_" + trait.id, StringComparison.Ordinal)
                && !MclslLocalizationBridge.IsRuntimeKey(localized))
                return localized;
        }
        catch (System.Exception mclslEmptyCatchEx) { MySimulatedLongevityRoad.Core.MclslDiagnostics.Error("empty-catch-code-MySimulatedLongevityRoad-UI-MclslRankSnapshotSource-cs-3", "空 catch 捕获: code/MySimulatedLongevityRoad/UI/MclslRankSnapshotSource.cs #3: " + mclslEmptyCatchEx.Message); }
        try
        {
            string localized = LM.Get(trait.id);
            if (!string.IsNullOrWhiteSpace(localized)
                && !string.Equals(localized, trait.id, StringComparison.Ordinal)
                && !MclslLocalizationBridge.IsRuntimeKey(localized))
                return localized;
        }
        catch (System.Exception mclslEmptyCatchEx) { MySimulatedLongevityRoad.Core.MclslDiagnostics.Error("empty-catch-code-MySimulatedLongevityRoad-UI-MclslRankSnapshotSource-cs-4", "空 catch 捕获: code/MySimulatedLongevityRoad/UI/MclslRankSnapshotSource.cs #4: " + mclslEmptyCatchEx.Message); }
        return trait.id;
    }

    private static bool ShouldHideTraitFilterChoice(ActorTrait trait)
    {
        if (trait == null || string.IsNullOrWhiteSpace(trait.id)) return true;
        if (MclslLocalizationBridge.IsRuntimeKey(trait.id)) return true;
        if (trait.id.StartsWith("mclsl_runtime_", StringComparison.OrdinalIgnoreCase)) return true;
        string displayName = GetTraitDisplayName(trait);
        return MclslLocalizationBridge.IsRuntimeKey(displayName);
    }

    private static int TraitFilterOrder(string traitId)
    {
        if (string.IsNullOrWhiteSpace(traitId)) return int.MaxValue;
        if (traitId.StartsWith("gifts_", StringComparison.Ordinal))
        {
            int rank = ParseTrailingNumber(traitId);
            return rank > 0 ? 100 + rank : 199;
        }
        if (traitId.StartsWith("realm_", StringComparison.Ordinal))
        {
            int rank = ParseTrailingNumber(traitId);
            return rank > 0 ? 200 + rank : 299;
        }
        if (traitId.StartsWith("trait_Mclsl", StringComparison.Ordinal)
            || traitId.StartsWith("MCLSL", StringComparison.OrdinalIgnoreCase)
            || traitId.StartsWith("mclsl", StringComparison.OrdinalIgnoreCase))
            return 300;
        return 1000;
    }

    private static int ParseTrailingNumber(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return -1;
        int end = value.Length - 1;
        while (end >= 0 && !char.IsDigit(value[end])) end--;
        if (end < 0) return -1;
        int start = end;
        while (start >= 0 && char.IsDigit(value[start])) start--;
        string digits = value.Substring(start + 1, end - start);
        return int.TryParse(digits, out int result) ? result : -1;
    }
}

internal sealed class MclslRankKingdomFilterChoice
{
    internal Kingdom Kingdom;
    internal long KingdomId;
    internal string DisplayName = string.Empty;
}

internal sealed class MclslRankAssetFilterChoice
{
    internal ActorAsset Asset;
    internal string AssetId = string.Empty;
    internal string DisplayName = string.Empty;
}

internal sealed class MclslRankTraitFilterChoice
{
    internal string TraitId = string.Empty;
    internal string DisplayName = string.Empty;
}
