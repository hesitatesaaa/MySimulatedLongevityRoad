using System;
using System.Text;
using MySimulatedLongevityRoad.Data;

namespace MySimulatedLongevityRoad.Systems;

/// <summary>
/// Stable, event-driven sources for crafting materials. Ordinary activity is checked
/// once in the existing annual actor pipeline; all other entry points are successful
/// reward events, never per-frame scans.
/// </summary>
internal static class MclslMaterialDiscovery
{
    private const int RollRange = 10000;
    private const int AnnualHuangChance = 100; // 1% per material
    private const int AnnualXuanChance = 30;   // 0.30% per material
    private const int RuinHuangChance = 800;  // 8% per material
    private const int RuinXuanChance = 300;  // 3% per material

    private static readonly string[] AllMaterialIds =
    {
        "A01", "A02", "A03", "A04", "A05", "A06", "A07", "A08", "A09", "F01"
    };

    internal static void TryAnnualActivity(Actor actor, int year)
    {
        if (!MclslActorAccessor.Alive(actor) || !MclslEligibility.CanCultivate(actor)) return;
        WorldTile tile = actor.current_tile;
        if (tile?.Type == null) return;

        string runId = CurrentRunId(year);
        string sourceId = "annual_activity";
        string sourceKey = BuildSourceKey(runId, actor, year, "annual", sourceId);
        string seed = sourceKey;
        string biome = tile.Type.biome_id ?? string.Empty;
        bool woodland = IsWoodlandBiome(biome);
        bool mountain = tile.Type.mountains || biome == "biome_rocklands" || biome == "biome_hill";
        bool water = tile.Type.liquid;
        int mask = 0;

        for (int i = 0; i < AllMaterialIds.Length; i++)
        {
            string materialId = AllMaterialIds[i];
            MclslItemDefinition item = MclslItemCatalog.Get(materialId);
            if (item == null || !HasSource(item, MclslMaterialSource.AnnualActivity)
                || item.MaterialTier == MclslMaterialTier.Di || item.MaterialTier == MclslMaterialTier.Tian)
                continue;

            int chance = item.MaterialTier == MclslMaterialTier.Huang ? AnnualHuangChance : AnnualXuanChance;
            if (MatchesHabitat(item, woodland, water, mountain)) chance *= 2;
            if (Passes(seed, materialId, chance)) mask |= 1 << i;
        }

        Commit(actor, year, "野外活动", sourceKey, mask, null, null);
    }

    /// <summary>Called only after an explorer survives a ruin expedition.</summary>
    internal static void TryDiscover(Actor actor, MclslSectRuinRecord exploration, int year, int order)
    {
        if (!MclslActorAccessor.Alive(actor) || exploration == null || string.IsNullOrWhiteSpace(exploration.Id)) return;
        string runId = CurrentRunId(year);
        string actorId = MclslActorAccessor.Id(actor).ToString(System.Globalization.CultureInfo.InvariantCulture);
        // This exact legacy key preserves deduplication for existing saves.
        string legacyKey = "material|" + actorId + "|material|" + exploration.Id + "|" + year + "|" + order;
        if (MclslWorldRunRepository.HasMaterialDiscoveryEventKey(exploration.Id, legacyKey)) return;

        string sourceKey = BuildSourceKey(runId, actor, year, "ruin", exploration.Id + "|" + order);
        int mask = 0;
        for (int i = 0; i < AllMaterialIds.Length; i++)
        {
            string materialId = AllMaterialIds[i];
            MclslItemDefinition item = MclslItemCatalog.Get(materialId);
            if (item == null || !HasSource(item, MclslMaterialSource.Ruin)) continue;

            int chance;
            switch (item.MaterialTier)
            {
                case MclslMaterialTier.Huang: chance = RuinHuangChance; break;
                case MclslMaterialTier.Xuan: chance = RuinXuanChance; break;
                case MclslMaterialTier.Di:
                    if (exploration.Quality < 3) continue;
                    chance = 150; // 1.5%
                    break;
                case MclslMaterialTier.Tian:
                    if (exploration.Quality < 4) continue;
                    chance = 50; // 0.5%
                    break;
                default: continue;
            }
            if (Passes(sourceKey, materialId, chance)) mask |= 1 << i;
        }

        Commit(actor, year, "遗迹探索", sourceKey, mask, exploration.Id, legacyKey);
    }

    /// <summary>One low-probability material reward for a genuine upward realm transition.</summary>
    internal static void TryDiscoverBreakthrough(Actor actor, string previousRealm, string newRealm, int year, string reason)
    {
        int previous = MclslRealmIds.Index(previousRealm);
        int current = MclslRealmIds.Index(newRealm);
        if (!MclslActorAccessor.Alive(actor) || previous < 0 || current <= previous || IsAdministrativeTransition(reason)) return;

        string sourceKey = BuildSourceKey(CurrentRunId(year), actor, year, "breakthrough", previousRealm + ">" + newRealm);
        if (StableRoll(sourceKey, "gate") % RollRange >= 600) return; // 6%

        MclslMaterialTier maxTier = current >= MclslRealmIds.Index(MclslRealmIds.YuanYing)
            ? MclslMaterialTier.Tian
            : current >= MclslRealmIds.Index(MclslRealmIds.JinDan)
                ? MclslMaterialTier.Di
                : MclslMaterialTier.Xuan;
        int mask = SelectSingleMaterial(sourceKey, MclslMaterialTier.Huang, maxTier, MclslMaterialSource.Breakthrough);
        Commit(actor, year, "突破机缘", sourceKey, mask, null, null);
    }

    /// <summary>One material roll attached to a successful named cave or world-change reward.</summary>
    internal static void TryDiscoverOpportunity(Actor actor, int year, string sourceType, string sourceId, int quality)
    {
        if (!MclslActorAccessor.Alive(actor) || string.IsNullOrWhiteSpace(sourceId)) return;
        string sourceKey = BuildSourceKey(CurrentRunId(year), actor, year, sourceType, sourceId);
        if (StableRoll(sourceKey, "gate") % RollRange >= 1800) return; // 18%

        MclslMaterialTier maxTier = quality >= 4
            ? MclslMaterialTier.Tian
            : quality >= 3 ? MclslMaterialTier.Di : MclslMaterialTier.Xuan;
        int mask = SelectSingleMaterial(sourceKey, MclslMaterialTier.Huang,
            maxTier, MclslMaterialSource.Opportunity);
        Commit(actor, year, "天地机缘", sourceKey, mask, null, null);
    }

    /// <summary>Faction commission resource rewards; ordinary stipend is intentionally excluded.</summary>
    internal static void TryDiscoverFactionReward(Actor actor, int year, string sourceId, bool highestQuality)
    {
        if (!MclslActorAccessor.Alive(actor) || string.IsNullOrWhiteSpace(sourceId)) return;
        string sourceKey = BuildSourceKey(CurrentRunId(year), actor, year, "faction_reward", sourceId);
        int mask = 0;
        if (StableRoll(sourceKey, "low_tier_gate") % RollRange < 800) // 8% Huang/Xuan chance
            mask |= SelectSingleMaterial(sourceKey + "|low_tier", MclslMaterialTier.Huang,
                MclslMaterialTier.Xuan, MclslMaterialSource.Faction);
        if (highestQuality && StableRoll(sourceKey, "di_tier_gate") % RollRange < 200) // 2% Di chance
            mask |= SelectSingleMaterial(sourceKey + "|di_tier", MclslMaterialTier.Di,
                MclslMaterialTier.Di, MclslMaterialSource.Faction);
        Commit(actor, year, "势力委托", sourceKey, mask, null, null);
    }

    private static bool IsAdministrativeTransition(string reason)
    {
        return !string.IsNullOrEmpty(reason)
            && (reason.Contains("手动") || reason.Contains("还真") || reason.Contains("恢复")
                || reason.Contains("修复") || reason.Contains("回载"));
    }

    private static string CurrentRunId(int year)
    {
        MclslWorldRunState run = MclslWorldRunRepository.Current;
        if (run == null || string.IsNullOrWhiteSpace(run.RunId))
        {
            MclslWorldRunRepository.EnsureCurrentRun(year);
            run = MclslWorldRunRepository.Current;
        }
        return run?.RunId ?? string.Empty;
    }

    private static string BuildSourceKey(string runId, Actor actor, int year, string sourceType, string sourceId)
    {
        return "material|" + runId + "|" + MclslActorAccessor.Id(actor).ToString(System.Globalization.CultureInfo.InvariantCulture)
            + "|" + year + "|" + sourceType + "|" + sourceId;
    }

    private static bool HasSource(MclslItemDefinition item, MclslMaterialSource source)
    {
        return item != null && (item.MaterialSources & source) != 0;
    }

    private static bool MatchesHabitat(MclslItemDefinition item, bool woodland, bool water, bool mountain)
    {
        MclslMaterialHabitat habitat = item?.PreferredHabitat ?? MclslMaterialHabitat.None;
        return (woodland && (habitat & MclslMaterialHabitat.Woodland) != 0)
            || (water && (habitat & MclslMaterialHabitat.Water) != 0)
            || (mountain && (habitat & MclslMaterialHabitat.Mountain) != 0);
    }

    private static bool IsWoodlandBiome(string biome)
    {
        return biome == "biome_grass" || biome == "biome_savanna" || biome == "biome_jungle"
            || biome == "biome_swamp" || biome == "biome_mushroom" || biome == "biome_birch"
            || biome == "biome_maple" || biome == "biome_flower";
    }

    private static int SelectSingleMaterial(string seed, MclslMaterialTier minTier,
        MclslMaterialTier maxTier, MclslMaterialSource source)
    {
        int totalWeight = 0;
        for (int i = 0; i < AllMaterialIds.Length; i++)
        {
            MclslItemDefinition item = MclslItemCatalog.Get(AllMaterialIds[i]);
            if (item != null && HasSource(item, source)
                && item.MaterialTier >= minTier && item.MaterialTier <= maxTier)
                totalWeight += TierWeight(item.MaterialTier);
        }
        if (totalWeight <= 0) return 0;

        int selected = StableRoll(seed, "selection") % totalWeight;
        for (int i = 0; i < AllMaterialIds.Length; i++)
        {
            MclslItemDefinition item = MclslItemCatalog.Get(AllMaterialIds[i]);
            if (item == null || !HasSource(item, source)
                || item.MaterialTier < minTier || item.MaterialTier > maxTier) continue;
            int weight = TierWeight(item.MaterialTier);
            if (selected < weight) return 1 << i;
            selected -= weight;
        }
        return 0;
    }

    private static int TierWeight(MclslMaterialTier tier) => tier switch
    {
        MclslMaterialTier.Huang => 60,
        MclslMaterialTier.Xuan => 35,
        MclslMaterialTier.Di => 18,
        MclslMaterialTier.Tian => 6,
        _ => 0
    };

    private static bool Passes(string seed, string materialId, int threshold)
        => StableRoll(seed, materialId) % RollRange < threshold;

    private static int StableRoll(string seed, string materialId)
    {
        uint hash = AppendString(2166136261, seed);
        hash = AppendChar(hash, '|');
        hash = AppendString(hash, materialId);
        return Avalanche(hash);
    }

    private static uint AppendString(uint hash, string value)
    {
        if (value == null) return hash;
        for (int i = 0; i < value.Length; i++) hash = AppendChar(hash, value[i]);
        return hash;
    }

    private static uint AppendChar(uint hash, char value)
    {
        hash ^= value;
        hash *= 16777619;
        return hash;
    }

    private static int Avalanche(uint hash)
    {
        unchecked
        {
            hash ^= hash >> 16;
            hash *= 0x7feb352d;
            hash ^= hash >> 15;
            hash *= 0x846ca68b;
            hash ^= hash >> 16;
            return (int)(hash & 0x7fffffff);
        }
    }

    private static void Commit(Actor actor, int year, string sourceName, string sourceKey, int mask,
        string legacyRuinId, string legacyRuinKey)
    {
        if (mask == 0 || !MclslActorAccessor.Alive(actor)) return;
        if (legacyRuinId != null && MclslWorldRunRepository.HasMaterialDiscoveryEventKey(legacyRuinId, legacyRuinKey)) return;
        if (!MclslWorldRunRepository.TryClaimMaterialAwardEventKey(sourceKey)) return;

        MclslBagState bag = MclslBagSystem.Read(actor);
        StringBuilder found = new(128);
        for (int i = 0; i < AllMaterialIds.Length; i++)
        {
            if ((mask & (1 << i)) == 0) continue;
            string itemId = AllMaterialIds[i];
            MclslItemDefinition item = MclslItemCatalog.Get(itemId);
            MclslBagSystem.Add(bag, itemId);
            if (found.Length > 0) found.Append('、');
            found.Append('【').Append(item?.Name ?? itemId).Append("】×1");
        }
        MclslBagSystem.Write(actor, bag);

        string actorName = MclslActorAccessor.DisplayName(actor);
        string title = actorName + "发现材料";
        string place = string.IsNullOrWhiteSpace(actor.city?.data?.name) ? "野外" : actor.city.data.name;
        string body = actorName + "在" + place + "通过" + sourceName + "获得" + found + "，已收入乾坤袋。";
        MclslWorldRunRepository.AddMaterialDiscoveryEvent(year, title, body, actor, sourceKey);
    }
}
