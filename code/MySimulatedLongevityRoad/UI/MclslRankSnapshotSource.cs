using System;
using System.Collections.Generic;
using MySimulatedLongevityRoad.Core;
using MySimulatedLongevityRoad.Data;
using MySimulatedLongevityRoad.Queries;
using MySimulatedLongevityRoad.Systems;
using MySimulatedLongevityRoad.Systems.Death;
using NeoModLoader.General;
using UnityEngine;

namespace MySimulatedLongevityRoad.UI;

internal static class MclslRankSnapshotSource
{
    private struct RankValues
    {
        internal string RealmId;
        internal string Name;
        internal string RealmName;
        internal string GiftName;
        internal string SpiritualRootAttributes;
        internal int TaishangProgress;
        internal float CultivationProgress;
        internal int Aptitude;
        internal int TrueEssence;
        internal long Contribution;
        internal long SpiritStones;
        internal int MindState;
        internal int MortalMiasma;
        internal int MortalMiasmaLimit;
        internal int HarmonyStability;
        internal int InverseTruthProgress;
        internal int GoldenCorePurity;
        internal int NascentCaveIntegrity;
        internal int DivineChangeCompatibility;
        internal int NextRealmMinimum;
    }
    private static readonly List<MclslRankEntry> CachedEntries = new();
    private static readonly Dictionary<long, int> Positions = new();
    private static readonly List<MclslRankKingdomFilterChoice> CachedKingdomChoices = new();
    private static readonly List<MclslRankAssetFilterChoice> CachedAssetChoices = new();
    private static readonly List<MclslRankTraitFilterChoice> CachedTraitChoices = new();
    private static readonly Dictionary<long, FilterProjection> Filters = new();
    private static readonly Dictionary<long, int> KingdomUsers = new();
    private static readonly Dictionary<string, int> AssetUsers = new(StringComparer.Ordinal);
    private static readonly Dictionary<string, int> TraitUsers = new(StringComparer.Ordinal);
    private static readonly List<MclslRankKingdomFilterChoice> KingdomScratch = new();
    private static readonly List<MclslRankAssetFilterChoice> AssetScratch = new();
    private static readonly List<MclslRankTraitFilterChoice> TraitScratch = new();
    private static readonly HashSet<long> KingdomIds = new();
    private static readonly HashSet<string> AssetIds = new(StringComparer.Ordinal);
    private static readonly HashSet<string> TraitIds = new(StringComparer.Ordinal);
    private sealed class FilterProjection
    {
        internal long Kingdom;
        internal string Asset;
        internal readonly List<string> Traits = new();
    }
    internal static long Revision { get; private set; }
    internal static int Count => CachedEntries.Count;
    private static MclslOrderedIdIndex<MclslRankEntry> Ordered = new((a, b) => b.Power.CompareTo(a.Power));
    internal static IReadOnlyList<MclslRankEntry> MembershipEntries => CachedEntries;
    internal static IReadOnlyList<MclslRankEntry> EntriesSnapshot(bool forceRebuild = false) => Ordered;
    internal static void UpdateActor(Actor actor, bool refreshFilters = false)
    {
        long id = MclslActorAccessor.Id(actor);
        if (!MclslCultivatorCandidateIndex.IsCultivator(id) || !MclslActorAccessor.Alive(actor))
        { Remove(id); return; }
        MclslRankEntry entry = BuildEntrySafely(actor, refreshFilters || !Positions.ContainsKey(id));
        if (entry == null) return;
        Ordered.Upsert(id, entry);
        if (Positions.TryGetValue(id, out int pos)) CachedEntries[pos] = entry;
        else { Positions[id] = CachedEntries.Count; CachedEntries.Add(entry); refreshFilters = true; }
        if (refreshFilters) UpdateFilters(actor, id);
        Revision++;
        MclslCodexPopulationIndex.Update(entry);
        MclslRankWindow.OnEntryChanged(id, entry);
        MclslCodexWindow.OnEntryChanged(id, entry);
    }
    private static void UpdateFilters(Actor actor, long id)
    {
        RemoveFilters(id);
        KingdomScratch.Clear(); AssetScratch.Clear(); TraitScratch.Clear();
        KingdomIds.Clear(); AssetIds.Clear(); TraitIds.Clear();
        CaptureFilterChoices(actor, KingdomScratch, AssetScratch, TraitScratch, KingdomIds, AssetIds, TraitIds);
        FilterProjection projection = new();
        foreach (MclslRankKingdomFilterChoice choice in KingdomScratch)
        {
            projection.Kingdom = choice.KingdomId;
            if (!KingdomUsers.TryGetValue(choice.KingdomId, out int count)) CachedKingdomChoices.Add(choice);
            KingdomUsers[choice.KingdomId] = count + 1;
        }
        foreach (MclslRankAssetFilterChoice choice in AssetScratch)
        {
            projection.Asset = choice.AssetId;
            if (!AssetUsers.TryGetValue(choice.AssetId, out int count)) CachedAssetChoices.Add(choice);
            AssetUsers[choice.AssetId] = count + 1;
        }
        foreach (MclslRankTraitFilterChoice choice in TraitScratch)
        {
            projection.Traits.Add(choice.TraitId);
            if (!TraitUsers.TryGetValue(choice.TraitId, out int count)) CachedTraitChoices.Add(choice);
            TraitUsers[choice.TraitId] = count + 1;
        }
        Filters[id] = projection;
    }
    private static void RemoveFilters(long id)
    {
        if (!Filters.TryGetValue(id, out FilterProjection p)) return;
        Filters.Remove(id);
        if (p.Kingdom != 0 && KingdomUsers.TryGetValue(p.Kingdom, out int kingdoms))
        {
            if (kingdoms > 1) KingdomUsers[p.Kingdom] = kingdoms - 1;
            else { KingdomUsers.Remove(p.Kingdom); CachedKingdomChoices.RemoveAll(x => x.KingdomId == p.Kingdom); }
        }
        if (p.Asset != null && AssetUsers.TryGetValue(p.Asset, out int assets))
        {
            if (assets > 1) AssetUsers[p.Asset] = assets - 1;
            else { AssetUsers.Remove(p.Asset); CachedAssetChoices.RemoveAll(x => x.AssetId == p.Asset); }
        }
        foreach (string trait in p.Traits)
        {
            int count = TraitUsers[trait];
            if (count > 1) TraitUsers[trait] = count - 1;
            else { TraitUsers.Remove(trait); CachedTraitChoices.RemoveAll(x => x.TraitId == trait); }
        }
    }
    internal static void Remove(long id)
    {
        if (!Positions.TryGetValue(id, out int pos)) return;
        int last = CachedEntries.Count - 1;
        if (pos != last)
        {
            CachedEntries[pos] = CachedEntries[last]; Positions[CachedEntries[pos].ActorId] = pos;
            MclslRankWindow.OnEntryChanged(CachedEntries[pos].ActorId, CachedEntries[pos]);
            MclslCodexWindow.OnEntryChanged(CachedEntries[pos].ActorId, CachedEntries[pos]);
        }
        Ordered.Remove(id);
        MclslCodexPopulationIndex.Remove(id);
        MclslRankWindow.OnEntryChanged(id, null);
        MclslCodexWindow.OnEntryChanged(id, null);
        CachedEntries.RemoveAt(last); Positions.Remove(id); RemoveFilters(id); Revision++;
    }
    internal static void Clear()
    {
        CachedEntries.Clear(); Positions.Clear(); Filters.Clear();
        MclslCodexPopulationIndex.Clear();
        CachedKingdomChoices.Clear(); CachedAssetChoices.Clear(); CachedTraitChoices.Clear();
        KingdomUsers.Clear(); AssetUsers.Clear(); TraitUsers.Clear(); Revision++;
        Ordered.Clear();
        MclslRankWindow.ClearRuntime();
        MclslCodexWindow.ClearBiographyRuntime();
        Ordered = new MclslOrderedIdIndex<MclslRankEntry>((a, b) => b.Power.CompareTo(a.Power));
    }

    private static MclslRankEntry BuildEntrySafely(Actor actor, bool metadataChanged)
    {
        try
        {
            return BuildEntry(actor, metadataChanged);
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

    internal static void Invalidate() => Revision++;

    private static MclslRankEntry BuildEntry(Actor actor, bool metadataChanged)
    {
        MclslRankEntry prior = Positions.TryGetValue(MclslActorAccessor.Id(actor), out int position)
            ? CachedEntries[position] : null;
        string realm = MclslActorAccessor.Realm(actor);
        string system = MclslActorAccessor.GetString(actor, MclslActorDataKeys.CultivationSystem);
        if (MclslSensingQiSystem.ShouldReturnToSensingQi(actor, realm, system)) realm = string.Empty;
        int essence = MclslActorAccessor.GetInt(actor, MclslActorDataKeys.TrueEssence);
        float progress = ReadRankProgress(actor, realm, essence);
        int taishang = Math.Clamp(MclslActorAccessor.GetInt(actor, MclslActorDataKeys.TaishangProgress), 0, 100);
        string roots = prior?.RootAttributes ?? string.Empty;
        string gift = prior?.GiftName ?? string.Empty;
        if (metadataChanged)
        {
            MclslSpiritualRootProfile root = MclslSpiritualRootSystem.ReadProfile(actor);
            roots = root.AttributeText;
            gift = MclslSpiritualRootSystem.ReadGiftForCultivation(actor)?.Name ?? root.GradeName;
        }
        RankValues view = new()
        {
            Name = metadataChanged || prior == null ? MclslActorAccessor.DisplayName(actor, realm) : prior.Name,
            RealmId = realm,
            RealmName = MclslActorCultivationQuery.RealmDisplay(realm, essence, progress, taishang),
            SpiritualRootAttributes = roots, GiftName = gift,
            CultivationProgress = progress, TrueEssence = essence,
            Aptitude = Math.Clamp(MclslActorAccessor.GetInt(actor, MclslActorDataKeys.Aptitude), 0, 100),
            MindState = MclslMindSystem.ReadMindState(actor),
            MortalMiasmaLimit = MclslMortalMiasmaSystem.Capacity(realm),
            MortalMiasma = Math.Clamp(MclslActorAccessor.GetInt(actor, MclslActorDataKeys.MortalMiasma), 0,
                MclslMortalMiasmaSystem.Capacity(realm)),
            Contribution = MclslActorAccessor.GetMoney(actor, MclslActorDataKeys.Contribution),
            SpiritStones = MclslActorAccessor.GetMoney(actor, MclslActorDataKeys.SpiritStones),
            TaishangProgress = taishang,
            HarmonyStability = MclslActorAccessor.GetInt(actor, MclslActorDataKeys.HarmonyStability),
            InverseTruthProgress = MclslActorAccessor.GetInt(actor, MclslActorDataKeys.InverseTruthProgress),
            GoldenCorePurity = MclslActorAccessor.GetInt(actor, MclslActorDataKeys.GoldenCorePurity),
            NascentCaveIntegrity = MclslActorAccessor.GetInt(actor, MclslActorDataKeys.NascentCaveIntegrity),
            DivineChangeCompatibility = MclslActorAccessor.GetInt(actor, MclslActorDataKeys.DivineChangeCompatibility),
            NextRealmMinimum = MclslRealmProgress.NextRealmMinimum(realm, system == MclslCultivationSystemIds.AncientLaw)
        };
        int realmIndex = string.IsNullOrWhiteSpace(view.RealmId) ? -1 : MclslRealmIds.Index(view.RealmId);
        double power = CalculatePower(actor, view, realmIndex);
        string kingdomName = KingdomName(actor);
        return new MclslRankEntry
        {
            Actor = actor,
            ActorId = MclslActorAccessor.Id(actor),
            Name = view.Name,
            RealmId = view.RealmId,
            RealmName = view.RealmName,
            GiftName = view.GiftName,
            RootText = !metadataChanged && prior != null ? prior.RootText : FirstRoot(view.SpiritualRootAttributes, view.GiftName),
            RootAttributes = view.SpiritualRootAttributes,
            NormalizedSearchText = prior != null && prior.Name == view.Name && prior.RealmName == view.RealmName
                && prior.RootAttributes == roots && prior.KingdomName == kingdomName ? prior.NormalizedSearchText
                : NormalizeSearch(view.Name) + NormalizeSearch(view.RealmName) + NormalizeSearch(roots) + NormalizeSearch(kingdomName),
            ExtraData = ExtraValues(view),
            KingdomName = kingdomName,
            ProfessionId = ResolveProfessionId(actor),
            ProfessionGrade = MclslProfessionSystem.GetGrade(actor),
            ProfessionExperience = Math.Max(0, MclslActorAccessor.GetInt(actor, MclslActorDataKeys.ProfessionExperience, 0)),
            Power = power,
            RealmIndex = MclslMinorRealmCatalog.RankRealmIndex(view.RealmId, view.TaishangProgress),
            MinorRealmIndex = MclslMinorRealmCatalog.RankMinorIndex(view.RealmId, view.CultivationProgress),
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
        float progress = ReadRankProgress(actor, realm, essence);
        int taishangProgress = MclslActorAccessor.GetInt(actor, MclslActorDataKeys.TaishangProgress, 0);
        string realmName = MclslActorCultivationQuery.RealmDisplay(realm, essence, progress, taishangProgress);
        string kingdomName = KingdomName(actor);
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
            NormalizedSearchText = NormalizeSearch(name) + NormalizeSearch(realmName) + NormalizeSearch(rootAttributes) + NormalizeSearch(kingdomName),
            ExtraText = string.IsNullOrWhiteSpace(realm)
                ? essence + "/" + MclslRealmProgress.LianQiEntryMinimum
                : string.Empty,
            KingdomName = kingdomName,
            ProfessionId = ResolveProfessionId(actor),
            ProfessionGrade = MclslProfessionSystem.GetGrade(actor),
            ProfessionExperience = Math.Max(0, MclslActorAccessor.GetInt(actor, MclslActorDataKeys.ProfessionExperience, 0)),
            Power = Math.Max(1d, realmIndex + 1),
            RealmIndex = MclslMinorRealmCatalog.RankRealmIndex(realm, taishangProgress),
            MinorRealmIndex = MclslMinorRealmCatalog.RankMinorIndex(realm, progress),
            Aptitude = Math.Max(0, MclslActorAccessor.GetInt(actor, MclslActorDataKeys.Aptitude, 0)),
            TrueEssence = essence,
            Contribution = Math.Max(0, MclslActorAccessor.GetMoney(actor, MclslActorDataKeys.Contribution, 0)),
            SpiritStones = Math.Max(0, MclslActorAccessor.GetMoney(actor, MclslActorDataKeys.SpiritStones, 0)),
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
        int essence = 0;
        try { essence = MclslActorAccessor.GetInt(actor, MclslActorDataKeys.TrueEssence, 0); } catch { }
        float progress = 0f;
        int taishangProgress = 0;
        try { progress = ReadRankProgress(actor, realm, essence); } catch { }
        try { taishangProgress = MclslActorAccessor.GetInt(actor, MclslActorDataKeys.TaishangProgress, 0); } catch { }
        string realmName = MclslActorCultivationQuery.RealmDisplay(realm, essence, progress, taishangProgress);
        string kingdomName = KingdomName(actor);
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
            NormalizedSearchText = NormalizeSearch(name) + NormalizeSearch(realmName) + NormalizeSearch(kingdomName),
            ExtraText = string.IsNullOrWhiteSpace(realm)
                ? Math.Max(0, essence) + "/" + MclslRealmProgress.LianQiEntryMinimum
                : string.Empty,
            KingdomName = kingdomName,
            ProfessionId = ResolveProfessionId(actor),
            ProfessionGrade = MclslProfessionSystem.GetGrade(actor),
            ProfessionExperience = Math.Max(0, MclslActorAccessor.GetInt(actor, MclslActorDataKeys.ProfessionExperience, 0)),
            Power = 1d,
            RealmIndex = MclslMinorRealmCatalog.RankRealmIndex(realm, taishangProgress),
            MinorRealmIndex = MclslMinorRealmCatalog.RankMinorIndex(realm, progress),
            TrueEssence = Math.Max(0, essence)
        };
    }

    private static float ReadRankProgress(Actor actor, string realm, int essence)
    {
        bool ancientPath = MclslActorAccessor.GetString(actor, MclslActorDataKeys.CultivationSystem, string.Empty)
            == MclslCultivationSystemIds.AncientLaw;
        return MclslRealmProgress.ProgressForRealm(realm, essence, ancientPath,
            MclslActorAccessor.GetFloat(actor, MclslActorDataKeys.CultivationProgress, 0f));
    }

    private static string KingdomName(Actor actor)
    {
        try
        {
            string name = actor?.kingdom?.data?.name;
            return string.IsNullOrWhiteSpace(name) ? "无归属" : name;
        }
        catch { return "无归属"; }
    }

    private static string ResolveProfessionId(Actor actor)
    {
        string traitProfession = MclslProfessionSystem.FromTrait(actor);
        return string.IsNullOrWhiteSpace(traitProfession)
            ? MclslActorAccessor.GetString(actor, MclslActorDataKeys.Profession, string.Empty)
            : traitProfession;
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

    private static double CalculatePower(Actor actor, RankValues view, int realmIndex)
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

    private static MclslRankExtraData ExtraValues(RankValues view) => view.RealmId switch
    {
        MclslRealmIds.JinDan => new(1, view.GoldenCorePurity),
        MclslRealmIds.YuanYing => new(2, view.NascentCaveIntegrity),
        MclslRealmIds.HuaShen => new(3, view.DivineChangeCompatibility),
        MclslRealmIds.HeDao => new(4, view.HarmonyStability),
        MclslRealmIds.ChangSheng => new(5, view.TaishangProgress),
        _ when string.IsNullOrWhiteSpace(view.RealmId) && view.TrueEssence > 0
            => new(6, view.TrueEssence, MclslRealmProgress.LianQiEntryMinimum),
        _ => view.NextRealmMinimum > 0 ? new(7, progress: view.CultivationProgress) : default
    };

    private static string FirstRoot(string value, string fallback)
    {
        if (string.IsNullOrWhiteSpace(value)) return fallback;
        int separator = value.IndexOf('、');
        return separator < 0 ? value : value.Substring(0, separator);
    }
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
        return "未知特征";
    }

    private static bool ShouldHideTraitFilterChoice(ActorTrait trait)
    {
        if (trait == null || string.IsNullOrWhiteSpace(trait.id)) return true;
        if (MclslLocalizationBridge.IsRuntimeKey(trait.id)) return true;
        if (trait.id.StartsWith("mclsl_runtime_", StringComparison.OrdinalIgnoreCase)) return true;
        string displayName = GetTraitDisplayName(trait);
        return MclslLocalizationBridge.IsRuntimeKey(displayName);
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
