using System;
using System.Collections.Generic;

namespace MySimulatedLongevityRoad.Data;

internal sealed class MclslEventCategoryDefinition
{
    internal string Id { get; set; } = string.Empty;
    internal string Name { get; set; } = string.Empty;
    internal string Color { get; set; } = "#CFC7B2";
    internal string IconPath { get; set; } = "ui/Icons/XuanHuangXianLu";
}

internal static class MclslEventCatalog
{
    internal const string All = "all";
    internal const string Cultivation = "cultivation";
    internal const string WorldResource = "world_resource";
    internal const string TreasureAcquisition = "treasure_acquisition";
    internal const string DaoStruggle = "dao_struggle";
    internal const string Faction = "faction";
    internal const string Ruin = "ruin";
    internal const string Lineage = "lineage";
    internal const string Death = "death";
    internal const string Reincarnation = "reincarnation";
    internal const string NativeWorld = "native_world";
    internal const string AncientWorld = "ancient_world";

    internal static readonly MclslEventCategoryDefinition[] Categories =
    {
        C(All, "全部", "#CFC7B2", "ui/Icons/XuanHuangXianLu"),
        C(Cultivation, "修行大事", "#FFD37A", "trait/realm_3"),
        C(WorldResource, "天地资源", "#9CD7FF", "ui/Icons/DongTian"),
        C(TreasureAcquisition, "仙缘宝录", "#E7C878", "ui/Icons/QiankunBagEntrance"),
        C(DaoStruggle, "仙法不可同修", "#D8A7FF", "ui/Icons/JinDanWuFa"),
        C(Faction, "仙盟五老", "#B7A7FF", "ui/Icons/WanXianMeng"),
        C(Ruin, "宗门遗迹", "#A7E08A", "ui/Icons/ZongMenYiJi"),
        C(Lineage, "仙道传承", "#D8C778", "trait/gifts_6"),
        C(Death, "修士生死", "#FF8877", "ui/Icons/SiWang"),
        C(Reincarnation, "还真逆理", "#7CCFD0", "ui/Icons/HuanZhen"),
        C(AncientWorld, "仙道纪元", "#D8C778", "trait/gifts_6"),
        C(NativeWorld, "人间世事", "#CFC7B2", "ui/Icons/XuanHuangXianLu")
    };

    internal static MclslEventCategoryDefinition Category(string id)
    {
        foreach (MclslEventCategoryDefinition category in Categories)
            if (string.Equals(category.Id, id, StringComparison.Ordinal)) return category;
        return Categories[0];
    }

    internal static string CategoryForType(string eventType)
    {
        string type = eventType ?? string.Empty;
        if (IsLineageEvent(type)) return Lineage;
        if (type is "ancient_ruin_born" or "ancient_lineage_ruin_born") return Ruin;
        if (type.StartsWith("ancient_", StringComparison.Ordinal)
            || type.StartsWith("era_cycle_ancient", StringComparison.Ordinal)
            || type is "ancient_law_breakthrough" or "ancient_convert_new_law" or "ancient_convert_foundation" or "ancient_lineage_flourish" or "ancient_remnant" or "ancient_remnant_secluded")
            return AncientWorld;
        if (type.StartsWith("mortal_fate_", StringComparison.Ordinal))
            return NativeWorld;
        if (type.StartsWith("newlaw_same_law", StringComparison.Ordinal))
            return DaoStruggle;
        if (type.StartsWith("newlaw_", StringComparison.Ordinal)
            || type.StartsWith("era_cycle_", StringComparison.Ordinal)
            || type is "cultivation_entry" or "foundation" or "golden_core" or "nascent_soul" or "divine_transformation" or "harmony_last_hit"
            or "epoch_ancient_law_end" or "epoch_chuanfa_dao" or "epoch_new_law_spread" or "epoch_law_conflict" or "epoch_ancient_remnants" or "epoch_new_law_era")
            return Cultivation;
        if (type.Contains("death", StringComparison.Ordinal) || type == "cultivator_death")
            return Death;
        if (type is "material_discovery" or "item_acquisition")
            return TreasureAcquisition;
        if (type.StartsWith("world_calamity_", StringComparison.Ordinal))
            return WorldResource;
        if (type.StartsWith("cave_", StringComparison.Ordinal) || type.StartsWith("world_change", StringComparison.Ordinal)
            || type.StartsWith("native_world_change", StringComparison.Ordinal) || type.StartsWith("marrow_", StringComparison.Ordinal)
            || type.StartsWith("world_soul", StringComparison.Ordinal))
            return WorldResource;
        if (type.StartsWith("faction_", StringComparison.Ordinal) || type == "epoch_wanxian_alliance_founded" || type == "epoch_five_elders_founded")
            return Faction;
        if (type.StartsWith("ruin_", StringComparison.Ordinal) || type == "ancient_ruin_born")
            return Ruin;
        if (type.StartsWith("huanzhen_", StringComparison.Ordinal) || type.StartsWith("inverse_truth", StringComparison.Ordinal) || type == "reincarnation_unbroken" || type == "longevity_achieved" || type == "taishang_achieved" || type == "epoch_mortal_miasma_truth")
            return Reincarnation;
        return NativeWorld;
    }

    internal static int ImportanceFor(string eventType, string title, string body, string realmId = null)
    {
        string type = eventType ?? string.Empty;
        if (type is "cycle_start" or "annual_settlement_failure") return 1;
        if (type.StartsWith("epoch_", StringComparison.Ordinal)
            || type is "harmony_last_hit" or "manual_harmony_soul_claimed" or "longevity_achieved" or "taishang_achieved") return 5;
        if (type is "ancient_law_breakthrough" or "cultivator_death")
            return RealmImportance(realmId, title, body);
        if (type is "golden_core" or "nascent_soul" or "divine_transformation"
            or "world_change" or "native_world_change" or "cave_born" or "ruin_born"
            or "world_soul_manifest" or "world_soul_claimed" or "world_soul_killed"
            or "ancient_found_method" or "ancient_new_method_branch"
            or "inverse_truth_reversed") return 4;
        if (type.StartsWith("sect_lifecycle_", StringComparison.Ordinal)
            || type.StartsWith("era_cycle_", StringComparison.Ordinal)) return 4;
        if (type.StartsWith("world_calamity_", StringComparison.Ordinal))
            return type.EndsWith("_pulse", StringComparison.Ordinal) || type.EndsWith("_cleared", StringComparison.Ordinal) ? 2 : 4;
        if (type is "foundation" or "ancient_master_teaching" or "ancient_famous_technique"
            or "ancient_lineage_flourish" or "ancient_secret_realm") return 3;
        if (type is "material_discovery" or "item_acquisition"
            || type.StartsWith("mortal_fate_", StringComparison.Ordinal)
            || type.StartsWith("ancient_technique_", StringComparison.Ordinal)) return 2;
        if (type is "ancient_breakthrough_failed" or "cave_refine_failed" or "marrow_extract_failed") return 2;
        return 2;
    }

    internal static int EffectiveImportance(MclslRunEventRecord record)
        => record == null ? 1 : record.Importance is >= 1 and <= 5
            ? record.Importance : ImportanceFor(record.EventType, record.Title, record.Body);

    internal static bool IsLineageEvent(string type)
        => type != null && (type.StartsWith("sect_lifecycle_", StringComparison.Ordinal)
            || type is "ancient_technique_recorded" or "ancient_technique_revived"
                or "ancient_technique_lost" or "ancient_technique_flourish"
                or "newlaw_technique_recorded" or "newlaw_technique_revived"
                or "newlaw_technique_lost" or "newlaw_technique_flourish"
                or "ancient_lineage_flourish" or "ancient_lineage_remembered"
                or "ancient_famous_technique");

    internal static bool ShouldMirrorToNativeHistory(int importance) => importance >= 4;

    internal static bool ShouldMirrorToNativeHistory(string type, int importance, bool lineageEnabled)
        => ShouldMirrorToNativeHistory(importance) && (lineageEnabled || !IsLineageEvent(type));

    // WorldBox 0.51 Date.getYear(time) = (int)(time / 60) + 1.
    internal static int NativeTimestampForYear(int year)
        => (int)Math.Min(int.MaxValue, Math.Max(0L, (long)year - 1L) * 60L);

    private static int RealmImportance(string realmId, string title, string body)
    {
        if (realmId is "changsheng" or "hedao") return 5;
        if (realmId is "jindan" or "yuanying" or "huashen") return 4;
        if (realmId == "zhuji") return 3;
        if (realmId == "lianqi" || realmId == "mortal") return 2;
        string text = (title ?? string.Empty) + " " + (body ?? string.Empty);
        if (text.Contains("长生", StringComparison.Ordinal)
            || text.Contains("合道", StringComparison.Ordinal)) return 5;
        if (text.Contains("金丹", StringComparison.Ordinal) || text.Contains("元婴", StringComparison.Ordinal)
            || text.Contains("化神", StringComparison.Ordinal)) return 4;
        if (text.Contains("筑基", StringComparison.Ordinal)) return 3;
        return 2;
    }

    internal static bool ShouldAnnounceEvent(string eventType)
    {
        string type = eventType ?? string.Empty;
        return type is "foundation" or "golden_core" or "nascent_soul" or "divine_transformation"
            or "harmony_last_hit" or "longevity_achieved" or "taishang_achieved" or "world_soul_manifest"
            or "world_soul_duty_complete" or "ruin_born" or "world_change" or "cave_born";
    }

    private static MclslEventCategoryDefinition C(string id, string name, string color, string icon) => new()
    {
        Id = id,
        Name = name,
        Color = color,
        IconPath = icon
    };
}
