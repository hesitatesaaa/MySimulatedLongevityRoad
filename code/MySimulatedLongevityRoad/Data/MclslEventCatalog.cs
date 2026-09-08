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
    internal const string DaoStruggle = "dao_struggle";
    internal const string Faction = "faction";
    internal const string Ruin = "ruin";
    internal const string Death = "death";
    internal const string Reincarnation = "reincarnation";
    internal const string NativeWorld = "native_world";
    internal const string AncientWorld = "ancient_world";

    internal static readonly MclslEventCategoryDefinition[] Categories =
    {
        C(All, "全部", "#CFC7B2", "ui/Icons/XuanHuangXianLu"),
        C(Cultivation, "修行大事", "#FFD37A", "trait/realm_3"),
        C(WorldResource, "天地资源", "#9CD7FF", "ui/Icons/DongTian"),
        C(DaoStruggle, "仙法不可同修", "#D8A7FF", "ui/Icons/JinDanWuFa"),
        C(Faction, "仙盟五老", "#B7A7FF", "ui/Icons/WanXianMeng"),
        C(Ruin, "宗门遗迹", "#A7E08A", "ui/Icons/ZongMenYiJi"),
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
        if (type.StartsWith("world_calamity_", StringComparison.Ordinal))
            return WorldResource;
        if (type.StartsWith("cave_", StringComparison.Ordinal) || type.StartsWith("world_change", StringComparison.Ordinal)
            || type.StartsWith("native_world_change", StringComparison.Ordinal) || type.StartsWith("marrow_", StringComparison.Ordinal)
            || type.StartsWith("world_soul", StringComparison.Ordinal))
            return WorldResource;
        if (type.StartsWith("faction_", StringComparison.Ordinal) || type == "epoch_wanxian_alliance_founded" || type == "epoch_five_elders_founded")
            return Faction;
        if (type.StartsWith("ruin_", StringComparison.Ordinal) || type.StartsWith("sect_lifecycle_", StringComparison.Ordinal) || type == "ancient_ruin_born")
            return Ruin;
        if (type.StartsWith("huanzhen_", StringComparison.Ordinal) || type.StartsWith("inverse_truth", StringComparison.Ordinal) || type == "reincarnation_unbroken" || type == "longevity_achieved" || type == "taishang_achieved" || type == "epoch_mortal_miasma_truth")
            return Reincarnation;
        return NativeWorld;
    }

    internal static bool ShouldMirrorToNativeHistory(string eventType)
    {
        string type = eventType ?? string.Empty;
        if (type.StartsWith("newlaw_", StringComparison.Ordinal)
            || type.StartsWith("era_cycle_", StringComparison.Ordinal)
            || type.StartsWith("sect_lifecycle_", StringComparison.Ordinal)
            || type.StartsWith("world_calamity_", StringComparison.Ordinal)
            || type.StartsWith("mortal_fate_", StringComparison.Ordinal)
            || type is "cycle_start" or "cultivation_entry" or "native_death_divert" or "cave_refine_failed" or "marrow_extract_failed"
            or "ancient_law_entry" or "ancient_root_manifest" or "ancient_closed_cultivation"
            or "ancient_sudden_insight" or "ancient_inner_demon" or "ancient_cultivators_gather"
            or "ancient_spiritual_convergence" or "ancient_spiritual_decline")
            return false;
        if (type == "cultivator_death")
            return false;
        if (type == "law_conflict_death")
            return false;
        if (type == "reincarnation_unbroken")
            return false;
        if (type.StartsWith("ancient_technique_", StringComparison.Ordinal)
            || type == "ancient_lineage_ruin_born")
            return false;
        return true;
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
