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

internal sealed class MclslItemDefinition
{
    internal readonly string Id;
    internal readonly string Name;
    internal readonly string Category;
    internal readonly int Grade;
    internal readonly string EffectText;
    internal readonly string IngredientA;
    internal readonly string IngredientB;
    internal readonly int Price;
    // Material rarity is independent from Grade, which remains the recipe / crafted-item grade.
    internal readonly MclslMaterialTier MaterialTier;
    internal readonly MclslMaterialSource MaterialSources;
    internal readonly MclslMaterialHabitat PreferredHabitat;
    internal string IconPath => "ui/Items/" + Id;

    internal MclslItemDefinition(string id, string name, string category, int grade,
        string effectText, string ingredientA, string ingredientB, int price,
        MclslMaterialTier materialTier = MclslMaterialTier.None,
        MclslMaterialSource materialSources = MclslMaterialSource.None,
        MclslMaterialHabitat preferredHabitat = MclslMaterialHabitat.None)
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
        new("F002", "巨力符", "Talisman", 1, "攻击力 +15%，持续 30 秒。", "A06", "F01", 12),
        new("F003", "神行符", "Talisman", 1, "移动速度 +15%，持续 30 秒。", "A07", "F01", 12),
        new("F001", "护体符箓", "Talisman", 2, "护甲 +15%，受到伤害 -8%，持续 30 秒。", "A08", "F01", 36),
        new("F004", "迅击符", "Talisman", 2, "攻击速度 +12%，持续 30 秒。", "A01", "F01", 36),
        new("F005", "健体符", "Talisman", 3, "最大生命 +20%，并立即按新增上限同比补充生命，持续 45 秒。", "A04", "F01", 108),
        new("F006", "凝神符", "Talisman", 3, "暴击率 +8%，命中/感知类属性 +10%，持续 45 秒。", "A03", "F01", 108),
        new("F007", "聚灵符", "Talisman", 4, "修炼效率 +15%，灵力恢复效率 +15%，持续 60 秒。", "A05", "F01", 324),
        new("B001", "神武雷", "Artifact", 1, "投向目标位置，1.5 秒后爆炸；中心造成 300 点 + 使用者攻击力×250% 伤害，半径 3 格，外围最低 50% 伤害；对建筑额外 +50% 伤害。使用时消耗耐久。", "R03", "R04", 30),
        new("B002", "万头幡", "Artifact", 2, "主动释放魂煞，半径 6 格；造成攻击力×180% +150 伤害；敌人恐惧 4 秒，并在 12 秒内攻击力 -15%、护甲 -10%；冷却 30 秒。", "R01", "R05", 90),
        new("B004", "混元紫金葫芦", "Artifact", 2, "每次发动释放 5 柄混元飞刀，优先攻击 5 个不同目标；目标不足时可重复命中。每刀造成攻击力×120% +80 伤害；索敌距离 12 格；冷却 12 秒。", "R01", "R05", 90),
        new("B009", "定海神剑", "Artifact", 2, "普通攻击伤害 +35%；每第 4 次攻击触发“定海”，额外造成攻击力×100% 伤害并定身 2 秒；对首领/高境界单位定身缩短为 0.8 秒。", "R04", "R06", 90),
        new("B003", "裂界神兵", "Artifact", 3, "普通攻击伤害 +60%；主动斩击单体造成攻击力×400% +500 伤害并无视 35% 护甲；对护盾、建筑、空间类目标额外 +100% 伤害；冷却 20 秒。", "R05", "R06", 270),
        new("B005", "墨染恶书", "Artifact", 3, "墨光命中造成攻击力×220% +300 伤害；目标获得“墨染”15 秒：攻击力 -15%、护甲 -15%、装备/法宝效果 -25%；再次命中仅刷新持续时间；冷却 18 秒。", "R01", "R06", 270),
        new("B006", "连山杖", "Artifact", 4, "召唤山岳冲击半径 8 格区域；中心造成攻击力×450% +800 伤害，外围最低 50%；击退 4 格并眩晕 2 秒；对建筑额外 +100% 伤害；冷却 35 秒。", "R02", "R01", 810),
        new("B007", "归海铲", "Artifact", 4, "向前释放宽 6 格、长度 14 格海潮；造成攻击力×350% +650 伤害并击退；减速 50% 持续 8 秒；水域目标额外受到 25% 伤害；冷却 30 秒。", "R04", "R05", 810),
        new("B008", "众生棍", "Artifact", 4, "锁定 12 格内目标，命中率最低 95%；普通攻击伤害 +70%；主动“众生一击”造成攻击力×550% +1000 单体伤害并眩晕 1.5 秒；同一目标第 3 次连续命中额外造成最大生命 5% 伤害（Boss 上限 2000）；冷却 25 秒。", "R01", "R06", 810),
    };

    private const MclslMaterialSource AllMaterialSources = MclslMaterialSource.AnnualActivity
        | MclslMaterialSource.Ruin | MclslMaterialSource.Breakthrough
        | MclslMaterialSource.Opportunity | MclslMaterialSource.Faction;

    private static readonly Dictionary<string, MclslItemDefinition> ById = BuildIndex();

    internal static MclslItemDefinition Get(string id) =>
        !string.IsNullOrWhiteSpace(id) && ById.TryGetValue(id, out var item) ? item : null;

    private static Dictionary<string, MclslItemDefinition> BuildIndex()
    {
        Dictionary<string, MclslItemDefinition> index = new(StringComparer.Ordinal);
        foreach (MclslItemDefinition item in All) index.Add(item.Id, item);
        return index;
    }
}
