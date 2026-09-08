using System;
using System.Collections.Generic;

namespace MySimulatedLongevityRoad.Data;

internal static class MclslProceduralLexicon
{
    internal static readonly string[] Prefixes =
    {
        "太玄", "玄冥", "青冥", "紫极", "太初", "古荒", "清微", "玉宸", "寂照", "归墟",
        "混元", "沉星", "元磁", "九幽", "上清", "厚载", "流光", "无相", "幽都", "天外",
        "灵台", "洞真", "元始", "离合", "空明", "太素", "冲虚", "玄微", "神霄", "云笈",
        "赤明", "苍梧", "白藏", "黑水", "黄庭", "扶桑", "望舒", "重渊", "阆风", "天衡",
        "紫霄", "元辰", "太乙", "少阳", "少阴", "北冥", "南离", "西极", "东华", "中岳"
    };

    internal static readonly string[] NatureWords =
    {
        "不灭", "无垢", "寂灭", "生生", "离明", "含章", "藏锋", "定岳", "流转", "归一",
        "破妄", "镇虚", "凝真", "化生", "玄照", "幽寂", "清灵", "赤明", "沉渊", "惊雷",
        "长曜", "太和", "封藏", "轮转", "飞玄", "昭彻", "潜真", "抱朴", "回风", "化极",
        "守一", "通幽", "承真", "照影", "敛息", "开劫", "复命", "衍法", "定神", "澄元",
        "伏藏", "转轮", "垂天", "观妙", "凝魄", "归藏", "镇劫", "明心", "藏真", "拓界"
    };

    internal static readonly string[] FoundationForms =
    {
        "髓", "胎", "珠", "魄", "晶", "露", "砂", "种", "蜕", "骨", "心", "液", "壤", "精", "核", "华", "英", "璧", "玦", "芽",
        "根", "枝", "叶", "石", "玉", "印", "环", "简", "符", "烬", "泉", "炉", "盘", "钧", "镜", "窍"
    };

    internal static readonly string[] CaveForms =
    {
        "洞天", "玄境", "福地", "真府", "灵墟", "秘藏", "天隙", "幽壑", "古界", "残境", "神藏", "内府", "玄宫", "灵域", "天府", "宝境",
        "小界", "道场", "元宫", "仙圃", "秘境", "灵山", "天渊", "虚府", "山界", "海藏", "云台", "星府"
    };

    internal static readonly string[] EssenceForms =
    {
        "元精", "真粹", "天精", "玄精", "道精", "元粹", "灵英", "精魄", "真英", "天华", "元华", "道华", "玄英", "灵粹",
        "灵髓", "道英", "真华", "元胎", "精核", "天英", "洞精", "界粹", "灵华", "玄胎"
    };

    internal static readonly string[] MarrowForms =
    {
        "天髓", "道髓", "劫髓", "玄髓", "真髓", "元髓", "灵髓", "神髓",
        "变髓", "界髓", "灾髓", "法髓", "天骨", "道骨", "劫华", "变华"
    };


    internal static readonly string[] RuinForms =
    {
        "宗遗址", "门故地", "古宗墟", "道统遗府", "山门残界", "仙门旧址", "宗门残境", "传承遗宫", "古派遗藏", "山门废墟",
        "洞府遗址", "法坛旧址", "仙府残墟", "经阁遗藏", "旧庭废址", "禁地残界", "遗脉山门", "祖庭残址"
    };

    internal static readonly string[] WorldChangeForms =
    {
        "天变", "劫潮", "地变", "界变", "灾潮", "异变", "天灾", "劫变", "灵潮", "天象",
        "界潮", "天裂", "地涌", "星坠", "海啸", "风劫", "雷灾", "火劫", "寒潮", "山崩", "虚震", "灵暴"
    };

    internal static readonly string[] TechniqueForms =
    {
        "诀", "经", "法", "篇", "录", "书", "章", "真经", "玄经", "道书", "秘典", "心法", "炼形篇", "养神录", "参同契", "归元法"
    };

    private static readonly Dictionary<string, string[]> AncientStageForms = new(StringComparer.Ordinal)
    {
        [MclslRealmIds.ZhuJi] = new[] { "道基", "玄基", "真基", "灵基", "仙基", "命基", "元基", "法基" },
        [MclslRealmIds.JinDan] = new[] { "本命丹", "真丹", "玄丹", "金丹", "元丹", "道丹", "灵丹", "命丹" },
        [MclslRealmIds.YuanYing] = new[] { "元婴", "真婴", "玄婴", "灵婴", "道婴", "命婴", "神婴", "玉婴" },
        [MclslRealmIds.HuaShen] = new[] { "神意", "真意", "玄意", "道意", "法意", "元神意", "化神意", "合神意" },
        [MclslRealmIds.HeDao] = new[] { "大道", "真道", "玄道", "本命道", "合天道", "归一道", "承天道", "证真道" }
    };

    private static readonly string[] WorldSoulManifestForms =
    {
        "魄影", "魄身", "魄相", "天魄", "法魄", "灵魄", "魄灵", "真魄", "劫魄", "魄主", "魄形", "魄灵显身"
    };

    internal static readonly string[] CaveOrigins =
    {
        "地脉交汇处", "古战场遗址", "海陆相接之隙", "高山绝巅", "深谷幽壑", "火山地心", "极寒荒原", "雷暴云眼",
        "空间薄弱处", "古城废墟", "大河源头", "密林腹地", "荒漠深处", "陨星坠地", "阴阳交替之地", "无主荒域"
    };

    private static readonly Dictionary<string, string[]> LawRoots = new(StringComparer.Ordinal)
    {
        ["火"] = new[] { "炎", "离", "焱", "烬", "朱", "赤", "曜", "燧" },
        ["燃烧"] = new[] { "焚", "燎", "熔", "灼", "炽", "焦", "煅", "烈" },
        ["光"] = new[] { "明", "曜", "晖", "阳", "照", "曦", "辉", "白" },
        ["水"] = new[] { "沧", "溟", "玄水", "潮", "渊", "澜", "泓", "泽" },
        ["寒"] = new[] { "霜", "冰", "凛", "朔", "凝", "玄寒", "雪", "冽" },
        ["流转"] = new[] { "回流", "周流", "旋", "环", "洄", "转", "行", "迁流" },
        ["木"] = new[] { "青木", "苍木", "森", "柯", "藤", "叶", "榕", "古木" },
        ["生机"] = new[] { "生", "春", "荣", "长青", "萌", "复苏", "灵生", "青华" },
        ["繁衍"] = new[] { "蕃", "滋", "化育", "孕", "衍", "万生", "孳", "育" },
        ["土"] = new[] { "坤", "岳", "岩", "壤", "山", "厚土", "地元", "玄黄" },
        ["稳定"] = new[] { "定", "镇", "固", "安", "衡", "不动", "沉", "持" },
        ["封镇"] = new[] { "封", "锁", "镇", "禁", "压", "伏", "玄关", "闭" },
        ["金"] = new[] { "金", "庚", "白金", "玄铁", "银", "锋", "锐", "精金" },
        ["锋锐"] = new[] { "锋", "刃", "锐", "斩", "裂", "断", "破甲", "剑" },
        ["秩序"] = new[] { "律", "序", "衡", "规", "法度", "正", "天章", "仪" },
        ["风"] = new[] { "风", "罡", "飙", "岚", "飒", "长风", "青风", "天风" },
        ["雷"] = new[] { "雷", "霆", "震", "霄", "电", "劫雷", "惊雷", "玉枢" },
        ["速度"] = new[] { "疾", "瞬", "迅", "飞", "奔", "逐电", "流星", "无影" },
        ["阴"] = new[] { "阴", "幽", "玄阴", "太阴", "晦", "冥", "夜", "月" },
        ["阳"] = new[] { "阳", "太阳", "昭", "白昼", "日", "明阳", "纯阳", "金乌" },
        ["转化"] = new[] { "化", "易", "变", "轮转", "互生", "阴阳", "反复", "蜕" },
        ["空间"] = new[] { "空", "虚", "界", "天隙", "乾坤", "太虚", "界门", "虚域" },
        ["隐匿"] = new[] { "隐", "藏", "匿", "无踪", "潜形", "幽影", "伏", "晦迹" },
        ["迁跃"] = new[] { "跃", "遁", "移", "穿界", "挪移", "飞渡", "横空", "越界" },
        ["净化"] = new[] { "净", "涤", "澄", "洗", "清", "无垢", "化浊", "荡秽" },
        ["毁灭"] = new[] { "灭", "劫", "破", "崩", "灾", "寂灭", "末", "碎界" },
        ["余烬"] = new[] { "余烬", "残火", "劫灰", "死焰", "灰烬", "焦土", "烬光", "余炎" },
        ["惩戒"] = new[] { "罚", "刑", "劫", "诛", "戒", "天谴", "雷罚", "肃" },
        ["重生"] = new[] { "涅生", "复起", "新生", "再荣", "还生", "返元", "回春", "不死" }
    };

    internal static string[] Roots(string law)
    {
        if (!string.IsNullOrWhiteSpace(law) && LawRoots.TryGetValue(law, out string[] values)) return values;
        return new[] { "灵", "玄", "真", "元", "道", "天", "幽", "清" };
    }

    internal static string[] Forms(string kind) => kind switch
    {
        MclslGeneratedKinds.WorldCave => CaveForms,
        MclslGeneratedKinds.HeavenEarthEssence => EssenceForms,
        MclslGeneratedKinds.WorldChangeMarrow => MarrowForms,
        MclslGeneratedKinds.SectRuin => RuinForms,
        MclslGeneratedKinds.WorldChange => WorldChangeForms,
        _ => FoundationForms
    };

    internal static string TechniqueName(IReadOnlyList<string> tags, int seed)
    {
        string primary = tags != null && tags.Count > 0 ? tags[0] : "灵";
        string secondary = tags != null && tags.Count > 1 ? tags[1] : primary;
        string[] roots1 = Roots(primary);
        string[] roots2 = Roots(secondary);
        int hash = PositiveHash(seed + "|technique|" + primary + "|" + secondary);
        string prefix = Pick(Prefixes, hash);
        string nature = Pick(NatureWords, hash / 7);
        string root1 = Pick(roots1, hash / 13);
        string root2 = Pick(roots2, hash / 23);
        if (root2 == root1 && roots2.Length > 1) root2 = roots2[((hash / 23) + 1) % roots2.Length];
        string form = Pick(TechniqueForms, hash / 31);
        string name = (hash % 7) switch
        {
            0 => prefix + root1 + form,
            1 => root1 + root2 + form,
            2 => nature + root1 + form,
            3 => prefix + nature + form,
            4 => root1 + "参" + root2 + form,
            5 => prefix + root1 + root2 + form,
            _ => root2 + nature + form
        };
        return Collapse(name);
    }

    internal static string AncientStageName(string realm, IReadOnlyList<string> tags, int seed)
    {
        string primary = tags != null && tags.Count > 0 ? tags[0] : "灵";
        string secondary = tags != null && tags.Count > 1 ? tags[1] : primary;
        string[] roots1 = Roots(primary);
        string[] roots2 = Roots(secondary);
        string[] forms = AncientStageForms.TryGetValue(realm ?? string.Empty, out string[] mapped) ? mapped : new[] { "根基" };
        int hash = PositiveHash(seed + "|ancient_stage|" + realm + "|" + primary + "|" + secondary);
        string root1 = Pick(roots1, hash / 11);
        string root2 = Pick(roots2, hash / 19);
        if (root2 == root1 && roots2.Length > 1) root2 = roots2[((hash / 19) + 1) % roots2.Length];
        string nature = Pick(NatureWords, hash / 29);
        string form = Pick(forms, hash / 37);
        string name = (hash % 5) switch
        {
            0 => root1 + form,
            1 => root1 + root2 + form,
            2 => nature + root1 + form,
            3 => root1 + nature + form,
            _ => root2 + form
        };
        return Collapse(name);
    }

    internal static string WorldSoulManifestName(string seatName, IReadOnlyList<string> tags, int seed)
    {
        string primary = tags != null && tags.Count > 0 ? tags[0] : "灵";
        string secondary = tags != null && tags.Count > 1 ? tags[1] : primary;
        string[] roots1 = Roots(primary);
        string[] roots2 = Roots(secondary);
        int hash = PositiveHash(seed + "|world_soul_manifest|" + seatName + "|" + primary + "|" + secondary);
        string root1 = Pick(roots1, hash / 13);
        string root2 = Pick(roots2, hash / 23);
        if (root2 == root1 && roots2.Length > 1) root2 = roots2[((hash / 23) + 1) % roots2.Length];
        string prefix = Pick(Prefixes, hash / 31);
        string form = Pick(WorldSoulManifestForms, hash / 43);
        string core = (hash % 4) switch
        {
            0 => prefix + root1,
            1 => root1 + root2,
            2 => root1 + Pick(NatureWords, hash / 47),
            _ => prefix + root1 + root2
        };
        return Collapse(core + form);
    }

    private static string Pick(string[] values, int hash) => values[(hash & int.MaxValue) % values.Length];
    private static int PositiveHash(string value)
    {
        unchecked { int hash = 37; foreach (char c in value ?? string.Empty) hash = hash * 41 + c; return hash & int.MaxValue; }
    }

    private static string Collapse(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return "无名玄法";
        return value
            .Replace("玄玄", "玄")
            .Replace("灵灵", "灵")
            .Replace("真真", "真")
            .Replace("道道", "道")
            .Replace("天天", "天")
            .Replace("空空", "空")
            .Replace("火火", "火")
            .Replace("水水", "水")
            .Replace("元元", "元");
    }
}
