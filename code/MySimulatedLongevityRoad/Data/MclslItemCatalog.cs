using System;
using System.Collections.Generic;

namespace MySimulatedLongevityRoad.Data;

[Flags]
internal enum MclslMaterialSource : ushort
{
    None = 0,
    AnnualActivity = 1 << 0,
    Ruin = 1 << 1,
    Breakthrough = 1 << 2,
    Opportunity = 1 << 3,
    Faction = 1 << 4
}

[Flags]
internal enum MclslMaterialHabitat : byte
{
    None = 0,
    Woodland = 1 << 0,
    Water = 1 << 1,
    Mountain = 1 << 2
}

internal enum MclslMaterialTier : byte
{
    None = 0,
    Huang = 1,
    Xuan = 2,
    Di = 3,
    Tian = 4
}

internal enum MclslArtifactEquipmentSlot : byte
{
    Weapon,
    Helmet,
    Armor,
    Boots,
    Ring,
    Amulet
}

internal sealed class MclslItemDefinition
{
    internal readonly string Id;
    internal readonly string Name;
    internal readonly string Category;
    internal readonly int Grade;
    internal string EffectText;
    internal readonly string IngredientA;
    internal readonly string IngredientB;
    internal readonly int Price;
    // Material rarity is independent from Grade, which remains the recipe / crafted-item grade.
    internal readonly MclslMaterialTier MaterialTier;
    internal readonly MclslMaterialSource MaterialSources;
    internal readonly MclslMaterialHabitat PreferredHabitat;
    internal readonly string IconPath;
    internal readonly MclslArtifactEquipmentSlot EquipmentSlot;
    internal (string Id, float Value)[] NativeStats;

    internal MclslItemDefinition(string id, string name, string category, int grade,
        string effectText, string ingredientA, string ingredientB, int price,
        MclslMaterialTier materialTier = MclslMaterialTier.None,
        MclslMaterialSource materialSources = MclslMaterialSource.None,
        MclslMaterialHabitat preferredHabitat = MclslMaterialHabitat.None,
        string iconPath = null,
        MclslArtifactEquipmentSlot equipmentSlot = MclslArtifactEquipmentSlot.Weapon,
        (string Id, float Value)[] nativeStats = null)
    {
        Id = id;
        Name = name;
        Category = category;
        Grade = grade;
        EffectText = effectText;
        IngredientA = ingredientA;
        IngredientB = ingredientB;
        Price = price;
        MaterialTier = materialTier;
        MaterialSources = materialSources;
        PreferredHabitat = preferredHabitat;
        IconPath = iconPath ?? "ui/Items/" + id;
        EquipmentSlot = equipmentSlot;
        NativeStats = nativeStats ?? Array.Empty<(string Id, float Value)>();
    }
}

internal static class MclslItemCatalog
{
    internal static readonly MclslItemDefinition[] All =
    {
        new("A01", "琉璃珠", "SpiritObject", 0, "启灵、悟性、突破辅助；原著明确用于炼制琉璃丹。", "", "", 12, MclslMaterialTier.Xuan, AllMaterialSources, MclslMaterialHabitat.Mountain),
        new("A02", "蓝血珊瑚", "SpiritObject", 0, "冰寒、镇定、净体类药性；原著明确作为炼丹材料。", "", "", 12, MclslMaterialTier.Xuan, AllMaterialSources, MclslMaterialHabitat.Water),
        new("A03", "灵虚草", "Plant", 0, "神魂恢复类；原著明确为蕴神丹必备原料。", "", "", 12, MclslMaterialTier.Xuan, AllMaterialSources, MclslMaterialHabitat.Woodland),
        new("A04", "长生青力", "SpiritObject", 0, "强生机、修复、重塑；药王宗长生丹核心炼制力量。", "", "", 24, MclslMaterialTier.Di,
            MclslMaterialSource.Ruin | MclslMaterialSource.Breakthrough | MclslMaterialSource.Opportunity | MclslMaterialSource.Faction),
        new("A05", "万灵琼浆", "SpiritObject", 0, "高阶大补之物，用作高阶突破/恢复的通用稀有材料。", "", "", 24, MclslMaterialTier.Tian,
            MclslMaterialSource.Ruin | MclslMaterialSource.Breakthrough | MclslMaterialSource.Opportunity),
        new("A06", "长生药材", "Plant", 0, "将长生谷中炼制长生丹所需药材统一压缩为一个游戏材料。", "", "", 6, MclslMaterialTier.Huang, AllMaterialSources, MclslMaterialHabitat.Woodland),
        new("A07", "聚灵髓", "SpiritObject", 0, "用于凝聚灵力、推动境界突破。", "", "", 6, MclslMaterialTier.Huang, AllMaterialSources, MclslMaterialHabitat.Mountain),
        new("A08", "护脉草", "Plant", 0, "用于保护经脉、稳定伤势与强行突破。", "", "", 6, MclslMaterialTier.Huang, AllMaterialSources, MclslMaterialHabitat.Woodland),
        new("A09", "洗髓液", "SpiritObject", 0, "用于洗髓、重塑根基与改善资质。", "", "", 12, MclslMaterialTier.Xuan, AllMaterialSources, MclslMaterialHabitat.Mountain),
        new("F01", "灵符纸", "TalismanMaterial", 0, "所有符箓的基础载体；每张符箓固定消耗 1 份灵符纸，并额外消耗 1 种丹药材料。", "", "", 6, MclslMaterialTier.Huang,
            MclslMaterialSource.AnnualActivity | MclslMaterialSource.Ruin | MclslMaterialSource.Faction),
        Mat("A10", "回灵草", "Plant", MclslMaterialTier.Huang, MclslMaterialHabitat.Woodland),
        Mat("A11", "清心花", "Plant", MclslMaterialTier.Huang, MclslMaterialHabitat.Woodland),
        Mat("A12", "玄霜露", "SpiritObject", MclslMaterialTier.Xuan, MclslMaterialHabitat.Water),
        Mat("A13", "赤阳芝", "Plant", MclslMaterialTier.Xuan, MclslMaterialHabitat.Mountain),
        Mat("A14", "玉髓莲", "Plant", MclslMaterialTier.Xuan, MclslMaterialHabitat.Water),
        Mat("A15", "地脉灵乳", "SpiritObject", MclslMaterialTier.Di, MclslMaterialHabitat.Mountain),
        Mat("A16", "星魂藤", "Plant", MclslMaterialTier.Di, MclslMaterialHabitat.Woodland),
        Mat("A17", "九转金液", "SpiritObject", MclslMaterialTier.Di, MclslMaterialHabitat.Mountain),
        Mat("A18", "天露琼华", "SpiritObject", MclslMaterialTier.Tian, MclslMaterialHabitat.Water),
        Mat("A19", "太虚灵泉", "SpiritObject", MclslMaterialTier.Tian, MclslMaterialHabitat.Water),
        Mat("F010", "朱砂灵墨", "TalismanMaterial", MclslMaterialTier.Huang, MclslMaterialHabitat.Mountain),
        Mat("F011", "轻风符砂", "TalismanMaterial", MclslMaterialTier.Huang, MclslMaterialHabitat.Mountain),
        Mat("F012", "坚岩符砂", "TalismanMaterial", MclslMaterialTier.Xuan, MclslMaterialHabitat.Mountain),
        Mat("F013", "水镜灵墨", "TalismanMaterial", MclslMaterialTier.Xuan, MclslMaterialHabitat.Water),
        Mat("F014", "雷纹晶粉", "TalismanMaterial", MclslMaterialTier.Xuan, MclslMaterialHabitat.Mountain),
        Mat("F015", "镇魂墨", "TalismanMaterial", MclslMaterialTier.Di, MclslMaterialHabitat.Woodland),
        Mat("F016", "青木符骨", "TalismanMaterial", MclslMaterialTier.Di, MclslMaterialHabitat.Woodland),
        Mat("F017", "辟邪金砂", "TalismanMaterial", MclslMaterialTier.Di, MclslMaterialHabitat.Mountain),
        Mat("F018", "星河墨", "TalismanMaterial", MclslMaterialTier.Tian, MclslMaterialHabitat.Water),
        Mat("F019", "玄黄符胆", "TalismanMaterial", MclslMaterialTier.Tian, MclslMaterialHabitat.Mountain),
        Mat("M01", "灵木器胚", "Material", MclslMaterialTier.Huang, MclslMaterialHabitat.Woodland),
        Mat("M02", "玄铁器胚", "Material", MclslMaterialTier.Xuan, MclslMaterialHabitat.Mountain),
        Mat("M03", "地脉器胚", "Material", MclslMaterialTier.Di, MclslMaterialHabitat.Mountain),
        Mat("M04", "天星器胚", "Material", MclslMaterialTier.Tian, MclslMaterialHabitat.Mountain),
        Mat("M05", "青铜灵砂", "Material", MclslMaterialTier.Huang, MclslMaterialHabitat.Mountain),
        Mat("M06", "赤纹矿", "Material", MclslMaterialTier.Huang, MclslMaterialHabitat.Mountain),
        Mat("M07", "灵木芯", "Material", MclslMaterialTier.Huang, MclslMaterialHabitat.Woodland),
        Mat("M08", "青岩晶", "Material", MclslMaterialTier.Huang, MclslMaterialHabitat.Mountain),
        Mat("M09", "云纹铜", "Material", MclslMaterialTier.Huang, MclslMaterialHabitat.Mountain),
        Mat("M10", "松魄木", "Material", MclslMaterialTier.Huang, MclslMaterialHabitat.Woodland),
        Mat("M11", "赤砂铁", "Material", MclslMaterialTier.Huang, MclslMaterialHabitat.Mountain),
        Mat("M12", "玄铁晶", "Material", MclslMaterialTier.Xuan, MclslMaterialHabitat.Mountain),
        Mat("M13", "寒水玉", "Material", MclslMaterialTier.Xuan, MclslMaterialHabitat.Water),
        Mat("M14", "紫电砂", "Material", MclslMaterialTier.Xuan, MclslMaterialHabitat.Mountain),
        Mat("M15", "月纹木", "Material", MclslMaterialTier.Xuan, MclslMaterialHabitat.Woodland),
        Mat("M16", "碧波银", "Material", MclslMaterialTier.Xuan, MclslMaterialHabitat.Water),
        Mat("M17", "幽光石", "Material", MclslMaterialTier.Xuan, MclslMaterialHabitat.Mountain),
        Mat("M18", "雷纹铜", "Material", MclslMaterialTier.Xuan, MclslMaterialHabitat.Mountain),
        Mat("M19", "地髓精金", "Material", MclslMaterialTier.Di, MclslMaterialHabitat.Mountain),
        Mat("M20", "幽冥骨", "Material", MclslMaterialTier.Di, MclslMaterialHabitat.Mountain),
        Mat("M21", "赤炎晶", "Material", MclslMaterialTier.Di, MclslMaterialHabitat.Mountain),
        Mat("M22", "沧海魄", "Material", MclslMaterialTier.Di, MclslMaterialHabitat.Water),
        Mat("M23", "山河石髓", "Material", MclslMaterialTier.Di, MclslMaterialHabitat.Mountain),
        Mat("M24", "玄冥寒铁", "Material", MclslMaterialTier.Di, MclslMaterialHabitat.Mountain),
        Mat("M25", "朱雀火羽", "Material", MclslMaterialTier.Di, MclslMaterialHabitat.Woodland),
        Mat("M26", "星陨神铁", "Material", MclslMaterialTier.Tian, MclslMaterialHabitat.Mountain),
        Mat("M27", "太虚晶", "Material", MclslMaterialTier.Tian, MclslMaterialHabitat.Mountain),
        Mat("M28", "九霄雷髓", "Material", MclslMaterialTier.Tian, MclslMaterialHabitat.Mountain),
        Mat("M29", "造化玉", "Material", MclslMaterialTier.Tian, MclslMaterialHabitat.Mountain),
        Mat("M30", "天河银砂", "Material", MclslMaterialTier.Tian, MclslMaterialHabitat.Water),
        Mat("M31", "混元金髓", "Material", MclslMaterialTier.Tian, MclslMaterialHabitat.Mountain),
        Mat("M32", "乾坤灵木", "Material", MclslMaterialTier.Tian, MclslMaterialHabitat.Woodland),
        new("D019", "粗炼回春丸", "Pill", 0, "立即恢复 10% 最大生命。", "R07", "R08", 4),
        new("D020", "引气散", "Pill", 0, "立即恢复 8% 最大灵力。", "R07", "R09", 4),
        new("F026", "微力符", "Talisman", 0, "伤害 +5%，持续 15 秒。", "R01", "R08", 3),
        new("F027", "轻身符", "Talisman", 0, "移动速度 +5%，持续 15 秒。", "R01", "R09", 3),
        new("B082", "凡铁短剑", "Artifact", 0, "固定伤害 +30。", "R01", "R03", 8,
            nativeStats: Stats(("damage", 30f))),
        new("B083", "素木护心佩", "Artifact", 0, "最大生命 +50。", "R01", "R02", 8,
            equipmentSlot: MclslArtifactEquipmentSlot.Amulet, nativeStats: Stats(("health", 50f))),
        new("D001", "筑基丹", "Pill", 1, "仅炼气圆满可使用。筑基突破成功率 +25%；突破失败时修为损失 -50%；若以低于正常要求的状态强行突破，成功后有 20% 概率获得“根基受损”5 年：最大生命 -8%、修炼速度 -5%。", "A07", "A08", 20),
        new("D006", "活气抱元丹", "Pill", 1, "立即恢复 30% 最大生命；之后 20 秒内每 2 秒恢复 2% 最大生命（额外共 20%）；解除“轻伤”。", "A08", "A04", 20),
        new("D009", "净体拂尘丹", "Pill", 1, "立即清除中毒、虚弱、传送不适、轻度减速等普通负面状态；恢复 15% 最大生命；30 秒内负面状态持续时间 -30%。", "A02", "A08", 20),
        new("D002", "紫韵炼道丹", "Pill", 2, "仅筑基圆满突破金丹时生效：金丹突破成功率 +30%；失败时生命损失与修为倒退量 -40%；平时服用不提供修炼增益。", "A01", "A09", 60),
        new("D004", "太上奠基丹", "Pill", 2, "首次完整服用永久获得：最大生命 +10%、修炼速度 +8%、大境界突破成功率 +5%；同时移除 1 个“根基受损”类状态。重复服用只恢复 20% 生命，不再叠加永久属性。", "A09", "A04", 60),
        new("D008", "补魂丹", "Pill", 2, "恢复 60% 神魂/精神值并清除轻、中度神魂损伤。若 Mod 未设置独立神魂值，则改为恢复 30% 最大生命，并解除由神魂损伤导致的攻击、攻速、移速惩罚。", "A03", "A05", 60),
        new("D010", "天愈丹", "Pill", 2, "立即恢复 55% 最大生命；清除轻伤、中伤；10 秒内受到伤害 -20%。不能解除濒死、神魂破碎等最高级伤势。", "A06", "A05", 60),
        new("D003", "思悟丹", "Pill", 3, "仅金丹圆满突破元婴时生效：元婴突破成功率 +35%；突破过程中修炼/灵力运转效率 +20%；失败时修为倒退量 -50%。", "A01", "A05", 180),
        new("D005", "补缺丹", "Pill", 3, "立即获得当前境界升级所需修为的 35%；若正处瓶颈，额外增加 25% 突破进度；最高辅助至化神前。30 年内连续服用时，第二颗仅 70% 效果、第三颗 40%、第四颗及以后 10%。", "A07", "A05", 180),
        new("D007", "百愈丹", "Pill", 3, "立即恢复 85% 最大生命；移除轻伤、中伤、重伤；15 秒内生命恢复速度 +100%。不能复活已死亡单位。", "A06", "A04", 180),
        new("D011", "造化紫金丹", "Pill", 4, "可主动服用或设置自动保命：生命首次低于 10% 时立即恢复至 80% 最大生命，清除普通伤势与重伤，并获得 5 秒 90% 减伤。每个单位 50 年最多触发 1 次。", "A05", "A04", 540),
        new("D012", "化神丹", "Pill", 4, "仅元婴圆满冲击化神时可使用：化神突破成功率 +40%；突破失败时 80% 概率避免境界跌落；反噬伤害 -70%；失败后保留 50% 突破积累。不提供常驻修炼加成。", "A05", "A07", 540),
        new("D013", "回灵丹", "Pill", 1, "恢复最大灵力的25%；同类回灵物品冷却30秒。", "A10", "A07", 20),
        new("D014", "清心丹", "Pill", 1, "解除普通眩晕和迟缓，短时定神。", "A11", "A08", 20),
        new("D015", "玄霜养脉丹", "Pill", 2, "恢复35%生命，20秒内减少35%火焰伤害。", "A12", "A09", 60),
        new("D016", "赤阳培元丹", "Pill", 2, "一段时间内提高灵力恢复。", "A13", "A10", 60),
        new("D017", "星魂定魄丹", "Pill", 3, "镇定神魂并恢复40%灵力。", "A16", "A15", 180),
        new("D018", "太虚续命丹", "Pill", 4, "濒危时恢复50%生命，每五十年只触发一次。", "A19", "A18", 540),
        new("F002", "巨力符", "Talisman", 1, "攻击力 +15%，持续 30 秒。", "A06", "F01", 12),
        new("F003", "神行符", "Talisman", 1, "移动速度 +15%，持续 30 秒。", "A07", "F01", 12),
        new("F001", "护体符箓", "Talisman", 2, "护甲 +15%，受到伤害 -8%，持续 30 秒。", "A08", "F01", 36),
        new("F004", "迅击符", "Talisman", 2, "攻击速度 +12%，持续 30 秒。", "A01", "F01", 36),
        new("F005", "健体符", "Talisman", 3, "最大生命 +20%，并立即按新增上限同比补充生命，持续 45 秒。", "A04", "F01", 108),
        new("F006", "凝神符", "Talisman", 3, "暴击率 +8%，命中/感知类属性 +10%，持续 45 秒。", "A03", "F01", 108),
        new("F007", "聚灵符", "Talisman", 4, "修炼效率 +15%，灵力恢复效率 +15%，持续 60 秒。", "A05", "F01", 324),
        new("F020", "回灵符", "Talisman", 1, "立即恢复15%灵力；同类回灵物品冷却30秒。", "F010", "F01", 12),
        new("F021", "御风符", "Talisman", 1, "三十秒内行动更迅捷。", "F011", "F01", 12),
        new("F022", "镇岩符", "Talisman", 2, "25秒内护甲+15，所受击退力减半。", "F012", "F01", 36),
        new("F023", "水镜符", "Talisman", 2, "20秒内抵挡下一次法术伤害的25%。", "F013", "F01", 36),
        new("F024", "雷引符", "Talisman", 3, "小范围雷击，最多命中3名敌人。", "F014", "F01", 108),
        new("F025", "天幕符", "Talisman", 4, "15秒内自身和附近友军所受伤害减少15%。", "F019", "F01", 324),
        new("B002", "万头幡", "Artifact", 2, "固定伤害 +750；伤害倍率 +300%；生命 +500；护甲 +20。", "M02", "M12", 90, nativeStats: Stats(("damage", 750f), ("multiplier_damage", 3f), ("health", 500f), ("armor", 20f))),
        new("B004", "混元紫金葫芦", "Artifact", 2, "固定伤害 +650；伤害倍率 +250%；攻速倍率 +300%；射程 +5。", "M02", "M13", 90, nativeStats: Stats(("damage", 650f), ("multiplier_damage", 2.5f), ("multiplier_attack_speed", 3f), ("range", 5f))),
        new("B009", "定海神剑", "Artifact", 2, "固定伤害 +900；伤害倍率 +350%；命中 +5；攻速倍率 +150%。", "M02", "M14", 90, nativeStats: Stats(("damage", 900f), ("multiplier_damage", 3.5f), ("accuracy", 5f), ("multiplier_attack_speed", 1.5f))),
        new("B003", "裂界神兵", "Artifact", 3, "固定伤害 +2200；伤害倍率 +900%；暴击率 +40%；暴击伤害倍率 +200%。", "M03", "M19", 270, nativeStats: Stats(("damage", 2200f), ("multiplier_damage", 9f), ("critical_chance", 0.4f), ("critical_damage_multiplier", 2f))),
        new("B005", "墨染恶书", "Artifact", 3, "固定伤害 +1800；伤害倍率 +700%；生命 +2000；生命倍率 +400%。", "M03", "M20", 270, nativeStats: Stats(("damage", 1800f), ("multiplier_damage", 7f), ("health", 2000f), ("multiplier_health", 4f))),
        new("B006", "连山杖", "Artifact", 4, "固定伤害 +5500；伤害倍率 +1800%；护甲 +50；击退 +10。", "M04", "M26", 810, nativeStats: Stats(("damage", 5500f), ("multiplier_damage", 18f), ("armor", 50f), ("knockback", 10f))),
        new("B007", "归海铲", "Artifact", 4, "固定伤害 +5000；伤害倍率 +1600%；速度倍率 +300%；射程 +10。", "M04", "M27", 810, nativeStats: Stats(("damage", 5000f), ("multiplier_damage", 16f), ("multiplier_speed", 3f), ("range", 10f))),
        new("B008", "众生棍", "Artifact", 4, "固定伤害 +6500；伤害倍率 +2000%；命中 +9；暴击率 +80%；暴击伤害倍率 +500%。", "M04", "M28", 810, nativeStats: Stats(("damage", 6500f), ("multiplier_damage", 20f), ("accuracy", 9f), ("critical_chance", 0.8f), ("critical_damage_multiplier", 5f))),
        new("B010", "青锋灵剑", "Artifact", 1, "固定伤害 +250；伤害倍率 +100%；攻速倍率 +100%。", "M01", "M05", 30, iconPath: "ui/Items/B010", nativeStats: Stats(("damage", 250f), ("multiplier_damage", 1f), ("multiplier_attack_speed", 1f))),
        new("B011", "玄铁镇岳刀", "Artifact", 1, "固定伤害 +300；伤害倍率 +120%；护甲 +15。", "M01", "M06", 30, iconPath: "ui/Items/B011", nativeStats: Stats(("damage", 300f), ("multiplier_damage", 1.2f), ("armor", 15f))),
        new("B012", "青木护心甲", "Artifact", 1, "护甲 +35；生命 +500；生命倍率 +100%。", "M01", "M07", 30, equipmentSlot: MclslArtifactEquipmentSlot.Armor, nativeStats: Stats(("armor", 35f), ("health", 500f), ("multiplier_health", 1f))),
        new("B013", "玄水流光甲", "Artifact", 2, "护甲 +60；生命 +1500；生命倍率 +300%。", "M02", "M15", 90, equipmentSlot: MclslArtifactEquipmentSlot.Armor, nativeStats: Stats(("armor", 60f), ("health", 1500f), ("multiplier_health", 3f))),
        new("B014", "地煞玄罡甲", "Artifact", 3, "护甲 +85；生命 +4000；生命倍率 +800%。", "M03", "M21", 270, equipmentSlot: MclslArtifactEquipmentSlot.Armor, nativeStats: Stats(("armor", 85f), ("health", 4000f), ("multiplier_health", 8f))),
        new("B015", "九曜天辰甲", "Artifact", 4, "护甲 +99；生命 +10000；生命倍率 +1800%。", "M04", "M29", 810, equipmentSlot: MclslArtifactEquipmentSlot.Armor, nativeStats: Stats(("armor", 99f), ("health", 10000f), ("multiplier_health", 18f))),
        new("B016", "清心玉冠", "Artifact", 1, "命中 +3；攻速倍率 +100%；暴击率 +10%。", "M01", "M08", 30, equipmentSlot: MclslArtifactEquipmentSlot.Helmet, nativeStats: Stats(("accuracy", 3f), ("multiplier_attack_speed", 1f), ("critical_chance", 0.1f))),
        new("B017", "玄霜定神冠", "Artifact", 2, "命中 +5；攻速倍率 +300%；暴击率 +25%。", "M02", "M16", 90, equipmentSlot: MclslArtifactEquipmentSlot.Helmet, nativeStats: Stats(("accuracy", 5f), ("multiplier_attack_speed", 3f), ("critical_chance", 0.25f))),
        new("B018", "地魂镇念盔", "Artifact", 3, "命中 +8；攻速倍率 +800%；暴击率 +50%；暴击伤害倍率 +300%。", "M03", "M22", 270, equipmentSlot: MclslArtifactEquipmentSlot.Helmet, nativeStats: Stats(("accuracy", 8f), ("multiplier_attack_speed", 8f), ("critical_chance", 0.5f), ("critical_damage_multiplier", 3f))),
        new("B019", "太虚观天冕", "Artifact", 4, "命中 +9；攻速倍率 +1800%；暴击率 +100%；暴击伤害倍率 +800%。", "M04", "M30", 810, equipmentSlot: MclslArtifactEquipmentSlot.Helmet, nativeStats: Stats(("accuracy", 9f), ("multiplier_attack_speed", 18f), ("critical_chance", 1f), ("critical_damage_multiplier", 8f))),
        new("B020", "踏风履", "Artifact", 1, "速度倍率 +100%；体力 +500；体力倍率 +100%。", "M01", "M09", 30, equipmentSlot: MclslArtifactEquipmentSlot.Boots, nativeStats: Stats(("multiplier_speed", 1f), ("stamina", 500f), ("multiplier_stamina", 1f))),
        new("B021", "玄影追云靴", "Artifact", 2, "速度倍率 +300%；体力 +1500；体力倍率 +300%。", "M02", "M17", 90, equipmentSlot: MclslArtifactEquipmentSlot.Boots, nativeStats: Stats(("multiplier_speed", 3f), ("stamina", 1500f), ("multiplier_stamina", 3f))),
        new("B022", "地脉挪移靴", "Artifact", 3, "速度倍率 +800%；体力 +4000；体力倍率 +800%。", "M03", "M23", 270, equipmentSlot: MclslArtifactEquipmentSlot.Boots, nativeStats: Stats(("multiplier_speed", 8f), ("stamina", 4000f), ("multiplier_stamina", 8f))),
        new("B023", "天涯咫尺履", "Artifact", 4, "速度倍率 +1800%；体力 +10000；体力倍率 +1800%。", "M04", "M31", 810, equipmentSlot: MclslArtifactEquipmentSlot.Boots, nativeStats: Stats(("multiplier_speed", 18f), ("stamina", 10000f), ("multiplier_stamina", 18f))),
        new("B024", "聚灵戒", "Artifact", 1, "伤害倍率 +120%；暴击率 +15%；暴击伤害倍率 +100%。", "M01", "M10", 30, equipmentSlot: MclslArtifactEquipmentSlot.Ring, nativeStats: Stats(("multiplier_damage", 1.2f), ("critical_chance", 0.15f), ("critical_damage_multiplier", 1f))),
        new("B025", "玄火战戒", "Artifact", 2, "伤害倍率 +350%；暴击率 +30%；暴击伤害倍率 +200%。", "M02", "M18", 90, equipmentSlot: MclslArtifactEquipmentSlot.Ring, nativeStats: Stats(("multiplier_damage", 3.5f), ("critical_chance", 0.3f), ("critical_damage_multiplier", 2f))),
        new("B026", "地元归真戒", "Artifact", 3, "伤害倍率 +900%；暴击率 +60%；暴击伤害倍率 +500%。", "M03", "M24", 270, equipmentSlot: MclslArtifactEquipmentSlot.Ring, nativeStats: Stats(("multiplier_damage", 9f), ("critical_chance", 0.6f), ("critical_damage_multiplier", 5f))),
        new("B027", "周天星辰戒", "Artifact", 4, "伤害倍率 +2000%；暴击率 +100%；暴击伤害倍率 +1000%。", "M04", "M32", 810, equipmentSlot: MclslArtifactEquipmentSlot.Ring, nativeStats: Stats(("multiplier_damage", 20f), ("critical_chance", 1f), ("critical_damage_multiplier", 10f))),
        new("B028", "护脉玉符", "Artifact", 1, "生命 +300；生命倍率 +100%；寿命倍率 +100%；护甲 +15。", "M01", "M11", 30, equipmentSlot: MclslArtifactEquipmentSlot.Amulet, nativeStats: Stats(("health", 300f), ("multiplier_health", 1f), ("multiplier_lifespan", 1f), ("armor", 15f))),
        new("B029", "玄龟护身佩", "Artifact", 2, "生命 +1000；生命倍率 +300%；寿命倍率 +300%；护甲 +35。", "M12", "M13", 90, equipmentSlot: MclslArtifactEquipmentSlot.Amulet, nativeStats: Stats(("health", 1000f), ("multiplier_health", 3f), ("multiplier_lifespan", 3f), ("armor", 35f))),
        new("B030", "地藏回生坠", "Artifact", 3, "生命 +3000；生命倍率 +800%；寿命倍率 +800%；护甲 +65。", "M03", "M25", 270, equipmentSlot: MclslArtifactEquipmentSlot.Amulet, nativeStats: Stats(("health", 3000f), ("multiplier_health", 8f), ("multiplier_lifespan", 8f), ("armor", 65f))),
        new("B031", "长生护道符", "Artifact", 4, "生命 +8000；生命倍率 +1800%；寿命倍率 +1800%；护甲 +90。", "M26", "M27", 810, equipmentSlot: MclslArtifactEquipmentSlot.Amulet, nativeStats: Stats(("health", 8000f), ("multiplier_health", 18f), ("multiplier_lifespan", 18f), ("armor", 90f))),
        Art("B032", "青羽灵弓", 1, "弦动如风。", "bow", "M05", "M06"), Art("B033", "逐月玄弓", 2, "月影随箭。", "bow", "M12", "M14"),
        Art("B034", "破云地弓", 3, "一矢穿云。", "bow", "M19", "M20"), Art("B035", "天河星弓", 4, "星河为弦。", "bow", "M26", "M28"),
        Art("B036", "开山灵斧", 1, "劈石见径。", "axe", "M05", "M07"), Art("B037", "断岳玄斧", 2, "重锋撼山。", "axe", "M12", "M15"),
        Art("B038", "裂海地斧", 3, "斧落分潮。", "axe", "M19", "M21"), Art("B039", "乾坤天斧", 4, "一击定乾坤。", "axe", "M26", "M29"),
        Art("B040", "镇军灵钺", 1, "威仪初显。", "yue", "M05", "M08"), Art("B041", "玄章法钺", 2, "符章护刃。", "yue", "M12", "M16"),
        Art("B042", "地阙王钺", 3, "厚土为锋。", "yue", "M19", "M22"), Art("B043", "天律圣钺", 4, "法度随身。", "yue", "M26", "M30"),
        Art("B044", "索影灵钩", 1, "牵敌失位。", "hook", "M05", "M09"), Art("B045", "玄丝月钩", 2, "弧光无声。", "hook", "M12", "M17"),
        Art("B046", "地缚锁钩", 3, "锁势难逃。", "hook", "M19", "M23"), Art("B047", "天罗星钩", 4, "钩起星痕。", "hook", "M26", "M31"),
        Art("B048", "分潮灵叉", 1, "水势开路。", "fork", "M05", "M10"), Art("B049", "三元玄叉", 2, "三锋齐进。", "fork", "M12", "M18"),
        Art("B050", "镇渊地叉", 3, "定浪沉渊。", "fork", "M19", "M24"), Art("B051", "四海天叉", 4, "四海同鸣。", "fork", "M26", "M32"),
        Art("B052", "霜纹灵刀", 1, "刃映薄霜。", "blade", "M05", "M11"), Art("B053", "赤霞玄刀", 2, "霞色炽烈。", "blade", "M13", "M14"),
        Art("B054", "断念地刀", 3, "锋尽杂念。", "blade", "M19", "M25"), Art("B055", "归寂天刀", 4, "静中藏锋。", "blade", "M27", "M28"),
        Art("B056", "点星灵枪", 1, "一点寒星。", "spear", "M06", "M07"), Art("B057", "游龙玄枪", 2, "枪势如龙。", "spear", "M13", "M15"),
        Art("B058", "贯岳地枪", 3, "直取山心。", "spear", "M20", "M21"), Art("B059", "太虚天枪", 4, "虚实莫测。", "spear", "M27", "M29"),
        Art("B060", "清光灵剑", 1, "一线清辉。", "sword", "M06", "M08"), Art("B061", "碧霄玄剑", 2, "青虹破空。", "sword", "M13", "M16"),
        Art("B062", "镇魄地剑", 3, "剑意定神。", "sword", "M20", "M22"), Art("B063", "万法天剑", 4, "万法归锋。", "sword", "M27", "M30"),
        Art("B064", "横云灵戟", 1, "横扫云头。", "halberd", "M06", "M09"), Art("B065", "逐雷玄戟", 2, "电光随刃。", "halberd", "M13", "M17"),
        Art("B066", "破阵地戟", 3, "阵纹俱碎。", "halberd", "M20", "M23"), Art("B067", "周天神戟", 4, "环天而动。", "halberd", "M27", "M31"),
        Art("B068", "护心灵钟", 1, "微鸣护体。", "bell", "M06", "M10"), Art("B069", "定神玄钟", 2, "钟声清念。", "bell", "M13", "M18"),
        Art("B070", "镇海地钟", 3, "音沉如海。", "bell", "M20", "M24"), Art("B071", "无量天钟", 4, "余响不绝。", "bell", "M27", "M32"),
        Art("B072", "聚元灵印", 1, "凝元成纹。", "seal", "M06", "M11"), Art("B073", "伏岳玄印", 2, "印落如岳。", "seal", "M14", "M15"),
        Art("B074", "山河地印", 3, "山川入掌。", "seal", "M20", "M25"), Art("B075", "玄黄天印", 4, "一印定界。", "seal", "M28", "M29"),
        Art("B076", "避雨灵伞", 1, "灵雨不侵。", "umbrella", "M07", "M08"), Art("B077", "流云玄伞", 2, "伞下云行。", "umbrella", "M14", "M16"),
        Art("B078", "万象地伞", 3, "诸相成幕。", "umbrella", "M21", "M22"), Art("B079", "天幕宝伞", 4, "撑开一方天。", "umbrella", "M28", "M30"),
        new("B080", "济世药匣", "Artifact", 4, "一匣药香，护尽有缘人。白先生专属法宝。", "", "", 999999,
            equipmentSlot: MclslArtifactEquipmentSlot.Amulet, nativeStats: Stats(("health", 500000f), ("armor", 80f))),
        new("B081", "传法玉简", "Artifact", 4, "万法由此传诸世间。传法天尊专属法宝。", "", "", 999999,
            equipmentSlot: MclslArtifactEquipmentSlot.Amulet, nativeStats: Stats(("health", 500000f), ("multiplier_damage", 10f))),
        new("S001_SCROLL", "金芒剑气卷", "SpellScroll", 1, "使用后学会金芒剑气。", "F01", "A07", 24, iconPath: "ui/Spells/S001"),
        new("S002_SCROLL", "青木回春卷", "SpellScroll", 1, "使用后学会青木回春。", "F01", "A07", 24, iconPath: "ui/Spells/S002"),
        new("S003_SCROLL", "水镜护身卷", "SpellScroll", 1, "使用后学会水镜护身。", "F01", "A07", 24, iconPath: "ui/Spells/S003"),
        new("S004_SCROLL", "赤焰术卷", "SpellScroll", 1, "使用后学会赤焰术。", "F01", "A07", 24, iconPath: "ui/Spells/S004"),
        new("S005_SCROLL", "厚土壁卷", "SpellScroll", 2, "使用后学会厚土壁。", "F01", "A07", 72, iconPath: "ui/Spells/S005"),
        new("S006_SCROLL", "御风行卷", "SpellScroll", 2, "使用后学会御风行。", "F01", "A07", 72, iconPath: "ui/Spells/S006"),
        new("S007_SCROLL", "雷引诀卷", "SpellScroll", 2, "使用后学会雷引诀。", "F01", "A07", 72, iconPath: "ui/Spells/S007"),
        new("S008_SCROLL", "玄阴缚卷", "SpellScroll", 3, "使用后学会玄阴缚。", "F01", "A07", 216, iconPath: "ui/Spells/S008"),
        new("S009_SCROLL", "阳华破障卷", "SpellScroll", 3, "使用后学会阳华破障。", "F01", "A07", 216, iconPath: "ui/Spells/S009"),
        new("S010_SCROLL", "五行轮转卷", "SpellScroll", 4, "使用后学会五行轮转。", "F01", "A07", 648, iconPath: "ui/Spells/S010"),
        new("S011_SCROLL", "咫尺遁光卷", "SpellScroll", 4, "使用后学会咫尺遁光。", "F01", "A07", 648, iconPath: "ui/Spells/S011"),
        new("S012_SCROLL", "万象归一卷", "SpellScroll", 4, "使用后学会万象归一。", "F01", "A07", 648, iconPath: "ui/Spells/S012"),
        new("S013_SCROLL", "霜针诀卷", "SpellScroll", 1, "使用后学会霜针诀。", "F01", "A07", 24, iconPath: "ui/Spells/S013"),
        new("S014_SCROLL", "纳灵术卷", "SpellScroll", 1, "使用后学会纳灵术。", "F01", "A07", 24, iconPath: "ui/Spells/S014"),
        new("S015_SCROLL", "巽风刃卷", "SpellScroll", 2, "使用后学会巽风刃。", "F01", "A07", 72, iconPath: "ui/Spells/S015"),
        new("S016_SCROLL", "青藤缚卷", "SpellScroll", 2, "使用后学会青藤缚。", "F01", "A07", 72, iconPath: "ui/Spells/S016"),
        new("S017_SCROLL", "紫霄破甲卷", "SpellScroll", 3, "使用后学会紫霄破甲。", "F01", "A07", 216, iconPath: "ui/Spells/S017"),
        new("S018_SCROLL", "灵泉归元卷", "SpellScroll", 3, "使用后学会灵泉归元。", "F01", "A07", 216, iconPath: "ui/Spells/S018"),
        new("S019_SCROLL", "离火轮卷", "SpellScroll", 4, "使用后学会离火轮。", "F01", "A07", 648, iconPath: "ui/Spells/S019"),
        new("S020_SCROLL", "山岳护体卷", "SpellScroll", 4, "使用后学会山岳护体。", "F01", "A07", 648, iconPath: "ui/Spells/S020"),
        new("S021_SCROLL", "太阴摄魂卷", "SpellScroll", 4, "使用后学会太阴摄魂。", "F01", "A07", 648, iconPath: "ui/Spells/S021"),
        new("S022_SCROLL", "星移剑阵卷", "SpellScroll", 4, "使用后学会星移剑阵。", "F01", "A07", 648, iconPath: "ui/Spells/S022"),
        new("S023_SCROLL", "乾坤镇域卷", "SpellScroll", 4, "使用后学会乾坤镇域。", "F01", "A07", 648, iconPath: "ui/Spells/S023"),
        new("S024_SCROLL", "五行化生卷", "SpellScroll", 4, "使用后学会五行化生。", "F01", "A07", 648, iconPath: "ui/Spells/S024"),
        new("S025_SCROLL", "万灵回天卷", "SpellScroll", 4, "使用后学会万灵回天。", "F01", "A07", 648, iconPath: "ui/Spells/S025"),
        new("S026_SCROLL", "太初寂光卷", "SpellScroll", 4, "使用后学会太初寂光。", "F01", "A07", 648, iconPath: "ui/Spells/S026"),
    };

    private const MclslMaterialSource AllMaterialSources = MclslMaterialSource.AnnualActivity
        | MclslMaterialSource.Ruin | MclslMaterialSource.Breakthrough
        | MclslMaterialSource.Opportunity | MclslMaterialSource.Faction;

    private static MclslItemDefinition Mat(string id, string name, string category, MclslMaterialTier tier,
        MclslMaterialHabitat habitat)
    {
        MclslMaterialSource sources = tier switch
        {
            MclslMaterialTier.Tian => MclslMaterialSource.Ruin | MclslMaterialSource.Breakthrough | MclslMaterialSource.Opportunity,
            MclslMaterialTier.Di => MclslMaterialSource.Ruin | MclslMaterialSource.Breakthrough
                | MclslMaterialSource.Opportunity | MclslMaterialSource.Faction,
            _ => AllMaterialSources
        };
        int price = tier switch { MclslMaterialTier.Huang => 6, MclslMaterialTier.Xuan => 12,
            MclslMaterialTier.Di => 24, _ => 48 };
        return new MclslItemDefinition(id, name, category, 0, "用于仙道制作的" + name + "。", "", "", price,
            tier, sources, habitat);
    }

    private static MclslItemDefinition Art(string id, string name, int grade, string introduction, string family,
        string first, string second)
    {
        bool accessory = family is "bell" or "seal" or "umbrella";
        int price = grade switch { 1 => 30, 2 => 90, 3 => 270, _ => 810 };
        int damage = grade switch { 1 => 250, 2 => 900, 3 => 2200, _ => 5500 };
        int health = grade switch { 1 => 300, 2 => 1000, 3 => 3000, _ => 8000 };
        (string Id, float Value)[] stats = accessory
            ? family == "bell" ? Stats(("health", health), ("armor", 10f + grade * 15f))
                : family == "seal" ? Stats(("health", health), ("multiplier_damage", grade * 0.8f))
                : Stats(("health", health), ("multiplier_speed", grade * 0.4f))
            : family == "bow" ? Stats(("damage", damage), ("range", 3f + grade * 2f))
                : family is "spear" or "halberd" or "fork" ? Stats(("damage", damage), ("range", 1f + grade))
                : family is "hook" or "sword" ? Stats(("damage", damage), ("multiplier_attack_speed", grade * 0.35f))
                : Stats(("damage", damage), ("multiplier_damage", grade * 0.7f));
        return new MclslItemDefinition(id, name, "Artifact", grade, introduction, first, second, price,
            equipmentSlot: accessory ? MclslArtifactEquipmentSlot.Amulet : MclslArtifactEquipmentSlot.Weapon,
            nativeStats: stats);
    }

    private static (string Id, float Value)[] Stats(params (string Id, float Value)[] values) => values;

    private static readonly Dictionary<string, MclslItemDefinition> ById = BuildIndex();

    private static readonly Dictionary<string, string> DisplayNameIds = BuildDisplayNames();

    internal static MclslItemDefinition Get(string id)
    {
        if (id != null && ById.TryGetValue(id, out MclslItemDefinition direct)) return direct;
        string canonicalId = NormalizeId(id);
        return canonicalId.Length > 0 && ById.TryGetValue(canonicalId, out MclslItemDefinition item) ? item : null;
    }

    internal static string IngredientDisplayName(string id) => Get(id)?.Name ?? id switch
    {
        "R01" => "木材",
        "R02" => "石料",
        "R03" or "R04" => "常见金属",
        "R05" => "秘银",
        "R06" => "精金",
        "R07" => "草药",
        "R08" => "浆果",
        "R09" => "小麦",
        _ => id ?? string.Empty
    };

    internal static string NormalizeId(string id)
    {
        if (string.IsNullOrWhiteSpace(id)) return string.Empty;
        if (ById.ContainsKey(id)) return id;
        string value = id.Trim();
        if (value.EndsWith(" Description", StringComparison.OrdinalIgnoreCase))
            value = value.Substring(0, value.Length - " Description".Length).Trim();
        else if (value.EndsWith("_description", StringComparison.OrdinalIgnoreCase))
            value = value.Substring(0, value.Length - "_description".Length).Trim();
        if (value.StartsWith("mclsl_artifact_", StringComparison.OrdinalIgnoreCase))
            value = value.Substring("mclsl_artifact_".Length);
        else if (value.StartsWith("artifact_", StringComparison.OrdinalIgnoreCase))
            value = value.Substring("artifact_".Length);
        else if (value.StartsWith("item_", StringComparison.OrdinalIgnoreCase))
            value = value.Substring("item_".Length);

        string upper = value.ToUpperInvariant();
        if (ById.ContainsKey(upper)) return upper;
        if (DisplayNameIds.TryGetValue(value, out string canonical)
            || DisplayNameIds.TryGetValue(upper, out canonical)) return canonical;
        return string.Empty;
    }

    private static Dictionary<string, string> BuildDisplayNames()
    {
        Dictionary<string, string> names = new(StringComparer.Ordinal);
        foreach (MclslItemDefinition item in All) names[item.Name] = item.Id;
        return names;
    }

    private static Dictionary<string, MclslItemDefinition> BuildIndex()
    {
        Dictionary<string, MclslItemDefinition> index = new(StringComparer.Ordinal);
        foreach (MclslItemDefinition item in All) index.Add(item.Id, item);
        return index;
    }
}
