using System;
using System.Collections.Generic;
using System.Linq;

namespace MySimulatedLongevityRoad.Data;

internal static class MclslRealmIds
{
    internal const string Mortal = "mortal";
    internal const string LianQi = "lianqi";
    internal const string ZhuJi = "zhuji";
    internal const string JinDan = "jindan";
    internal const string YuanYing = "yuanying";
    internal const string HuaShen = "huashen";
    internal const string HeDao = "hedao";
    internal const string ChangSheng = "changsheng";

    internal static readonly string[] Ordered = { LianQi, ZhuJi, JinDan, YuanYing, HuaShen, HeDao, ChangSheng };

    internal static string Display(string id) => id switch
    {
        LianQi => "炼气",
        ZhuJi => "筑基",
        JinDan => "金丹",
        YuanYing => "元婴",
        HuaShen => "化神",
        HeDao => "合道",
        ChangSheng => "长生",
        _ => "凡俗"
    };

    internal static int Index(string id) => Array.IndexOf(Ordered, id ?? string.Empty);
}

internal sealed class MclslTechniqueDefinition
{
    internal string Id { get; set; } = string.Empty;
    internal string Name { get; set; } = string.Empty;
    internal string[] LawPool { get; set; } = Array.Empty<string>();
    internal string MaxRealm { get; set; } = MclslRealmIds.HeDao;
}

internal static class MclslCultivationCatalog
{
    internal static readonly IReadOnlyList<MclslTechniqueDefinition> Techniques = new[]
    {
        // 筑基法门
        T("gongfa_chihuo", "赤火炼息诀", MclslRealmIds.ZhuJi, "火", "燃烧", "光"),
        T("gongfa_hanquan", "寒泉纳气诀", MclslRealmIds.ZhuJi, "水", "寒", "流转"),
        T("gongfa_qingye", "青叶培元功", MclslRealmIds.ZhuJi, "木", "生机", "繁衍"),
        T("gongfa_shayan", "砂岩固脉法", MclslRealmIds.ZhuJi, "土", "稳定", "封镇"),
        T("gongfa_jingang", "金罡炼体篇", MclslRealmIds.ZhuJi, "金", "锋锐", "秩序"),
        T("gongfa_xunfeng", "巽风吐纳篇", MclslRealmIds.ZhuJi, "风", "速度", "流转"),
        T("gongfa_yinming", "阴冥守神诀", MclslRealmIds.ZhuJi, "阴", "寒", "隐匿"),
        T("gongfa_yanghe", "阳和养息经", MclslRealmIds.ZhuJi, "阳", "光", "生机"),
        T("gongfa_zilei", "紫雷引息诀", MclslRealmIds.ZhuJi, "雷", "速度", "惩戒"),
        T("gongfa_huangting", "黄庭抱元法", MclslRealmIds.ZhuJi, "土", "阳", "稳定"),
        T("gongfa_xinghe", "星河导气篇", MclslRealmIds.ZhuJi, "水", "光", "流转"),
        T("gongfa_xuanguang", "玄光守一诀", MclslRealmIds.ZhuJi, "光", "秩序", "隐匿"),

        // 金丹法门
        T("gongfa_xuanshui", "玄水涵灵经", MclslRealmIds.JinDan, "水", "寒", "流转"),
        T("gongfa_qingmu", "青木养元功", MclslRealmIds.JinDan, "木", "生机", "繁衍"),
        T("gongfa_lihuo", "离火丹书", MclslRealmIds.JinDan, "火", "光", "转化"),
        T("gongfa_gengjin", "庚金化元录", MclslRealmIds.JinDan, "金", "锋锐", "转化"),
        T("gongfa_huitian", "巽风回天篇", MclslRealmIds.JinDan, "风", "速度", "生机"),
        T("gongfa_taiyin", "太阴养神法", MclslRealmIds.JinDan, "阴", "寒", "隐匿"),
        T("gongfa_taiyang", "太阳照真经", MclslRealmIds.JinDan, "阳", "光", "燃烧"),
        T("gongfa_shanhe", "山河定脉篇", MclslRealmIds.JinDan, "土", "封镇", "稳定"),
        T("gongfa_leize", "雷泽洗心诀", MclslRealmIds.JinDan, "雷", "水", "惩戒"),
        T("gongfa_canglang", "沧浪玄功", MclslRealmIds.JinDan, "水", "风", "流转"),

        // 元婴法门
        T("gongfa_houtu", "厚土镇脉法", MclslRealmIds.YuanYing, "土", "稳定", "封镇"),
        T("gongfa_jinmang", "金芒锻神录", MclslRealmIds.YuanYing, "金", "锋锐", "秩序"),
        T("gongfa_chiming", "赤明离火经", MclslRealmIds.YuanYing, "火", "阳", "光"),
        T("gongfa_youxuan", "太阴玄照录", MclslRealmIds.YuanYing, "阴", "寒", "隐匿"),
        T("gongfa_dongzhen", "阳和洞真篇", MclslRealmIds.YuanYing, "阳", "生机", "转化"),
        T("gongfa_qingdi", "青帝长生功", MclslRealmIds.YuanYing, "木", "生机", "繁衍"),
        T("gongfa_tianhe", "天河周流经", MclslRealmIds.YuanYing, "水", "空间", "流转"),
        T("gongfa_leixiao", "雷霄炼神法", MclslRealmIds.YuanYing, "雷", "光", "惩戒"),

        // 化神法门
        T("gongfa_fenglei", "风雷引气篇", MclslRealmIds.HuaShen, "风", "雷", "速度"),
        T("gongfa_taixu", "太虚照神经", MclslRealmIds.HuaShen, "空间", "光", "隐匿"),
        T("gongfa_shanhai", "山海归一典", MclslRealmIds.HuaShen, "水", "土", "稳定"),
        T("gongfa_jiuyang", "九阳化神篇", MclslRealmIds.HuaShen, "阳", "火", "光"),
        T("gongfa_youming", "幽冥炼魂书", MclslRealmIds.HuaShen, "阴", "寒", "转化"),

        // 合道法门
        T("gongfa_yinyang", "阴阳周天功", MclslRealmIds.HeDao, "阴", "阳", "转化"),
        T("gongfa_xukong", "虚空感应篇", MclslRealmIds.HeDao, "空间", "隐匿", "迁跃"),
        T("gongfa_wuxing", "五行归真道书", MclslRealmIds.HeDao, "金", "木", "水", "火", "土"),
        T("gongfa_wanxiang", "周天万象经", MclslRealmIds.HeDao, "光", "阴", "阳", "空间")
    };

    internal static readonly string[] AllLawTags = Techniques
        .SelectMany(x => x.LawPool)
        .Concat(new[] { "毁灭", "余烬", "惩戒", "重生" })
        .Distinct(StringComparer.Ordinal)
        .ToArray();

    internal static MclslTechniqueDefinition Technique(string id)
    {
        return TryTechnique(id, out MclslTechniqueDefinition definition) ? definition : Techniques[0];
    }

    internal static bool TryTechnique(string id, out MclslTechniqueDefinition definition)
    {
        string normalized = NormalizeTechniqueId(id);
        definition = Techniques.FirstOrDefault(x => x.Id == normalized);
        if (definition != null) return true;

        // 新法大人口下使用轻量异法ID分流。异法只改变功法ID与名称，
        // 法则池和境界上限仍读取对应母法，避免为每个异法创建静态表项。
        if (TryStripNumericSuffix(normalized, "_derived_", out string baseId))
            definition = Techniques.FirstOrDefault(x => x.Id == baseId);
        if (definition == null && TryLegacySourceId(normalized, out string legacySourceId))
            definition = Techniques.FirstOrDefault(x => x.Id == legacySourceId);
        return definition != null;
    }

    internal static string BaseTechniqueId(string id)
    {
        string normalized = NormalizeTechniqueId(id);
        if (TryStripNumericSuffix(normalized, "_derived_", out string baseId)) return baseId;
        return TryLegacySourceId(normalized, out string legacySourceId) ? legacySourceId : normalized;
    }

    private static bool TryLegacySourceId(string value, out string sourceId)
    {
        sourceId = string.Empty;
        if (string.IsNullOrWhiteSpace(value) || !value.StartsWith("legacy_", StringComparison.Ordinal)) return false;
        int separator = value.LastIndexOf("__", StringComparison.Ordinal);
        if (separator <= 0 || separator + 2 >= value.Length) return false;
        sourceId = value.Substring(separator + 2);
        return !string.IsNullOrWhiteSpace(sourceId);
    }

    internal static MclslTechniqueDefinition StartingTechnique(long actorId, int aptitude, bool ancient)
    {
        return StartingTechnique(actorId.ToString(), aptitude, ancient);
    }

    internal static MclslTechniqueDefinition StartingTechnique(string seed, int aptitude, bool ancient)
    {
        if (ancient)
        {
            return StartingAncientTechnique(seed, aptitude);
        }

        int targetIndex = aptitude >= 95 ? MclslRealmIds.Index(MclslRealmIds.HeDao)
            : aptitude >= 82 ? MclslRealmIds.Index(MclslRealmIds.HuaShen)
            : aptitude >= 65 ? MclslRealmIds.Index(MclslRealmIds.YuanYing)
            : aptitude >= 38 ? MclslRealmIds.Index(MclslRealmIds.JinDan)
            : MclslRealmIds.Index(MclslRealmIds.ZhuJi);
        int roll = PositiveHash((seed ?? string.Empty) + "|starting_tier|" + aptitude + "|" + (ancient ? "ancient" : "new")) % 100;
        int selectedIndex = roll < 68 ? targetIndex : roll < 92 ? Math.Max(1, targetIndex - 1) : -1;
        MclslTechniqueDefinition[] pool = selectedIndex >= 1
            ? Techniques.Where(x => MclslRealmIds.Index(x.MaxRealm) == selectedIndex).ToArray()
            : Techniques.Where(x => MclslRealmIds.Index(x.MaxRealm) >= 1 && MclslRealmIds.Index(x.MaxRealm) <= targetIndex).ToArray();
        if (pool.Length == 0) pool = Techniques.Where(x => x.MaxRealm == MclslRealmIds.ZhuJi).ToArray();
        int index = PositiveHash((seed ?? string.Empty) + "|starting_technique|" + (ancient ? "ancient" : "new")) % Math.Max(1, pool.Length);
        return pool.Length == 0 ? Techniques[0] : pool[index];
    }

    private static MclslTechniqueDefinition StartingAncientTechnique(string seed, int aptitude)
    {
        // 旧法时代的功法来源是前人传承，不应只由入门者资质决定上限。
        // 资质只略微影响获得高阶传承的概率，保证仙道纪元早期自然存在金丹、元婴法。
        int roll = PositiveHash((seed ?? string.Empty) + "|ancient_starting_realm|" + aptitude) % 1000;
        int aptitudeShift = aptitude >= 95 ? 180
            : aptitude >= 85 ? 130
            : aptitude >= 70 ? 80
            : aptitude >= 55 ? 35
            : aptitude <= 25 ? -60
            : 0;
        int score = Math.Clamp(roll + aptitudeShift, 0, 999);
        string maxRealm = score < 260 ? MclslRealmIds.ZhuJi
            : score < 620 ? MclslRealmIds.JinDan
            : score < 860 ? MclslRealmIds.YuanYing
            : score < 960 ? MclslRealmIds.HuaShen
            : MclslRealmIds.HeDao;

        MclslTechniqueDefinition[] pool = Techniques
            .Where(x => x.MaxRealm == maxRealm)
            .ToArray();
        if (pool.Length == 0) pool = Techniques.Where(x => x.MaxRealm == MclslRealmIds.JinDan).ToArray();
        if (pool.Length == 0) pool = Techniques.Where(x => x.MaxRealm == MclslRealmIds.ZhuJi).ToArray();
        int index = PositiveHash((seed ?? string.Empty) + "|ancient_starting_technique|" + maxRealm) % Math.Max(1, pool.Length);
        return pool.Length == 0 ? Techniques[0] : pool[index];
    }

    internal static string NormalizeTechniqueId(string id)
    {
        string value = id ?? string.Empty;
        bool changed;
        do
        {
            changed = false;
            if (value.StartsWith("spiritual_", StringComparison.Ordinal))
            {
                value = value.Substring("spiritual_".Length);
                changed = true;
            }
            if (value.StartsWith("ancient_", StringComparison.Ordinal))
            {
                value = value.Substring("ancient_".Length);
                changed = true;
            }
        }
        while (changed);

        // 0.1.2测试版曾把同一功法改写为 _line_N / _branch_N。
        // 这里只剥离明确的数字后缀，不创建新功法，也不改变正常动态功法ID。
        bool stripped;
        do
        {
            stripped = TryStripNumericSuffix(value, "_line_", out string lineBase)
                || TryStripNumericSuffix(value, "_branch_", out lineBase);
            if (stripped) value = lineBase;
        }
        while (stripped);
        return value;
    }

    private static bool TryStripNumericSuffix(string value, string marker, out string baseId)
    {
        baseId = value ?? string.Empty;
        int index = baseId.LastIndexOf(marker, StringComparison.Ordinal);
        if (index <= 0) return false;
        string suffix = baseId.Substring(index + marker.Length);
        if (!int.TryParse(suffix, out _)) return false;
        baseId = baseId.Substring(0, index);
        return !string.IsNullOrWhiteSpace(baseId);
    }

    private static MclslTechniqueDefinition T(string id, string name, string maxRealm, params string[] laws) => new() { Id = id, Name = name, MaxRealm = maxRealm, LawPool = laws };

    private static int PositiveHash(string value)
    {
        unchecked
        {
            int hash = 31;
            foreach (char c in value ?? string.Empty) hash = hash * 37 + c;
            return hash & int.MaxValue;
        }
    }
}

internal static class MclslGeneratedKinds
{
    internal const string FoundationWonder = "foundation_wonder";
    internal const string WorldCave = "world_cave";
    internal const string HeavenEarthEssence = "heaven_earth_essence";
    internal const string WorldChangeMarrow = "world_change_marrow";
    internal const string SectRuin = "sect_ruin";
    internal const string WorldChange = "world_change";
}
