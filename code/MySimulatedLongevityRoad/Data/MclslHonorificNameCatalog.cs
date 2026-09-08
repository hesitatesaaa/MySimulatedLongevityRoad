using System;
using System.Collections.Generic;
using MySimulatedLongevityRoad.Systems;

namespace MySimulatedLongevityRoad.Data;

/// <summary>
/// 修士姓名统一写回 WorldBox 原生姓名：低境为“姓名-境界”，
/// 高境为“尊号·姓名-境界”。个人姓名、尊号与境界后缀分别生成，
/// 每次刷新前都会剥离旧装饰，避免读档或重复同步后出现嵌套姓名。
/// </summary>
internal static class MclslHonorificNameCatalog
{
    internal const string SensingQiStage = "sensing_qi";

    private static readonly string[] Surnames =
    {
        "赵", "钱", "孙", "李", "周", "吴", "郑", "王", "冯", "陈", "褚", "卫",
        "蒋", "沈", "韩", "杨", "朱", "秦", "许", "何", "吕", "施", "张", "孔",
        "曹", "严", "华", "金", "魏", "陶", "姜", "谢", "邹", "喻", "柏", "章",
        "云", "苏", "潘", "葛", "范", "彭", "郎", "鲁", "韦", "昌", "马", "苗",
        "方", "俞", "任", "袁", "柳", "史", "唐", "费", "岑", "薛", "雷", "贺",
        "倪", "汤", "罗", "郝", "安", "常", "乐", "于", "傅", "齐", "康", "伍",
        "余", "顾", "孟", "黄", "穆", "萧", "尹", "姚", "邵", "汪", "毛", "戴",
        "宋", "熊", "舒", "祝", "董", "梁", "杜", "阮", "蓝", "季", "贾", "江",
        "郭", "梅", "林", "钟", "徐", "高", "夏", "蔡", "田", "胡", "凌", "霍",
        "卢", "莫", "房", "丁", "邓", "洪", "石", "崔", "程", "裴", "陆", "荣",
        "段", "侯", "宁", "武", "刘", "叶", "白", "卓", "乔", "谭", "温", "庄",
        "晏", "阎", "廖", "欧阳", "司马", "诸葛", "上官"
    };

    // 只使用常见、可作为人名的字，不再生成“客、老、祖、尊”等身份性字样。
    private static readonly string[] GivenFirst =
    {
        "子", "承", "景", "明", "清", "怀", "知", "守", "安", "宁", "修", "远",
        "行", "慎", "允", "元", "初", "昭", "晏", "云", "长", "维", "伯", "仲",
        "季", "书", "砚", "临", "望", "归", "听", "照", "观", "问", "澄", "谨",
        "定", "青", "玄", "真", "若", "兰", "松", "柏", "江", "川", "海", "岳",
        "霁", "寒", "秋", "春", "沐", "淮", "渊", "衡", "珩", "瑾", "瑜", "璟",
        "钧", "穆", "祺", "昀", "晟", "熙", "彦", "文", "思", "绍", "启", "弘"
    };

    private static readonly string[] GivenSecond =
    {
        "安", "宁", "和", "清", "明", "远", "修", "衡", "川", "渊", "澜", "舟",
        "行", "言", "微", "真", "玄", "白", "秋", "春", "霁", "昭", "晏", "景",
        "岳", "海", "云", "岑", "林", "松", "竹", "兰", "瑜", "瑾", "珩", "璟",
        "琛", "钧", "砚", "书", "辞", "知", "怀", "守", "慎", "恪", "穆", "允",
        "初", "元", "昀", "晟", "熙", "泽", "沅", "淮", "泓", "峻", "嵩", "桐",
        "柏", "楷", "祺", "彦", "文", "思", "绍", "启", "弘", "庭", "章", "礼"
    };

    private static readonly string[] InvalidMarkers =
    {
        "散人", "老人", "老祖", "真人", "真君", "神君", "道君", "道祖", "仙君", "仙尊", "天尊", "道人", "道长",
        "居士", "上人", "尊者", "先生", "魔君", "剑主", "山主", "府主", "宗主", "宫主",
        "法王", "帝君", "遗修", "旧修", "客", "修士", "感气", "炼气", "筑基", "金丹",
        "元婴", "化神", "合道", "长生", "太上"
    };


    // 旧法不借天地之变、天地之魄和逆理而成道。化神尊号以自身神意、
    // 道意和功法根基生成“神君”号；合道以自身大道生成“道君”号。
    // 旧法按正常规则止步合道，旧法长生“道祖”只用于异常旧档或手动数据兜底。
    private static readonly Dictionary<string, string[]> AncientHuaShenHonorificPools = new(StringComparer.Ordinal)
    {
        ["金"] = new[]
        {
            "太白神君", "庚元神君", "金阙神君", "白锋神君", "断岳神君",
            "玄钧神君", "鸣玉神君", "肃衡神君", "铸锋神君", "锐光神君"
        },
        ["木"] = new[]
        {
            "青玄神君", "长青神君", "建木神君", "扶桑神君", "荣枯神君",
            "苍梧神君", "生玄神君", "春生神君", "灵枢神君", "碧梧神君"
        },
        ["水"] = new[]
        {
            "沧海神君", "玄渊神君", "归潮神君", "寒潭神君", "天河神君",
            "镜水神君", "溯流神君", "澄波神君", "雨师神君", "流月神君"
        },
        ["火"] = new[]
        {
            "赤炎神君", "离火神君", "照夜神君", "朱陵神君", "焚霞神君",
            "烛龙神君", "丹霄神君", "烈阳神君", "烬明神君", "炎极神君"
        },
        ["土"] = new[]
        {
            "镇岳神君", "厚土神君", "山河神君", "坤元神君", "玄岩神君",
            "地衡神君", "万壑神君", "承天神君", "崇山神君", "镇脉神君"
        },
        ["风"] = new[]
        {
            "长风神君", "巽羽神君", "扶摇神君", "凌霄神君", "清岚神君",
            "追云神君", "天游神君", "流风神君", "御虚神君", "听涛神君"
        },
        ["雷"] = new[]
        {
            "玄霆神君", "玉枢神君", "震岳神君", "惊蛰神君", "雷泽神君",
            "神霄神君", "紫电神君", "天刑神君", "霹雳神君", "九雷神君"
        },
        ["阴"] = new[]
        {
            "幽冥神君", "玄阴神君", "藏月神君", "冥照神君", "太阴神君",
            "夜游神君", "寒魄神君", "隐渊神君", "玄夜神君", "幽明神君"
        },
        ["阳"] = new[]
        {
            "纯阳神君", "扶光神君", "曜真神君", "大明神君", "景曜神君",
            "洞阳神君", "曦和神君", "明玄神君", "朝元神君", "照真神君"
        },
        ["空间"] = new[]
        {
            "太虚神君", "界行神君", "空玄神君", "渡界神君", "无垠神君",
            "星门神君", "寰宇神君", "虚渡神君", "界海神君", "游空神君"
        }
    };

    private static readonly string[] GenericAncientHuaShenHonorifics =
    {
        "玄微神君", "洞真神君", "守一神君", "抱朴神君", "灵台神君",
        "玉景神君", "清虚神君", "道衡神君", "冲和神君", "无尘神君",
        "归元神君", "明道神君", "含章神君", "太和神君", "观妙神君",
        "澄玄神君", "灵虚神君", "知微神君", "元景神君", "照心神君"
    };

    private static readonly Dictionary<string, string[]> AncientHeDaoHonorificPools = new(StringComparer.Ordinal)
    {
        ["金"] = new[] { "庚天道君", "金衡道君", "太白道君", "玄钧道君", "断岳道君", "铸锋道君" },
        ["木"] = new[] { "青华道君", "建木道君", "荣枯道君", "长青道君", "扶桑道君", "生玄道君" },
        ["水"] = new[] { "沧溟道君", "玄澜道君", "归潮道君", "天河道君", "寒渊道君", "溯川道君" },
        ["火"] = new[] { "赤明道君", "离焰道君", "朱陵道君", "炎极道君", "照夜道君", "烬明道君" },
        ["土"] = new[] { "镇岳道君", "坤元道君", "厚载道君", "山河道君", "地衡道君", "承天道君" },
        ["风"] = new[] { "御虚道君", "长风道君", "扶摇道君", "天游道君", "清岚道君", "巽羽道君" },
        ["雷"] = new[] { "玄霄道君", "玉枢道君", "神霆道君", "天刑道君", "雷泽道君", "紫电道君" },
        ["阴"] = new[] { "幽玄道君", "玄阴道君", "藏月道君", "冥照道君", "太阴道君", "玄夜道君" },
        ["阳"] = new[] { "纯阳道君", "昭阳道君", "扶光道君", "曜真道君", "大明道君", "洞阳道君" },
        ["空间"] = new[] { "太虚道君", "界海道君", "渡界道君", "寰宇道君", "空玄道君", "无垠道君" }
    };

    private static readonly string[] GenericAncientHeDaoHonorifics =
    {
        "玄元道君", "太清道君", "洞玄道君", "无极道君", "冲虚道君",
        "归真道君", "玉宸道君", "太和道君", "道衡道君", "玄都道君",
        "守一道君", "抱朴道君", "观妙道君", "清微道君", "元始道君",
        "灵虚道君", "知微道君", "含章道君", "万象道君", "自然道君"
    };

    private static readonly Dictionary<string, string> AncientChangShengHonorificByCategory = new(StringComparer.Ordinal)
    {
        ["金"] = "太白道祖", ["木"] = "青玄道祖", ["水"] = "沧海道祖",
        ["火"] = "赤炎道祖", ["土"] = "镇岳道祖", ["风"] = "长风道祖",
        ["雷"] = "神霄道祖", ["阴"] = "幽冥道祖", ["阳"] = "纯阳道祖",
        ["空间"] = "太虚道祖"
    };

    private static readonly string[] GenericAncientChangShengHonorifics =
    {
        "玄元道祖", "洞真道祖", "无为道祖", "太初道祖", "自然道祖",
        "归一道祖", "万象道祖", "守一道祖", "鸿蒙道祖", "无名道祖"
    };


    // 化神尊号按角色真实法则根基分组。十类属性池共一百个专属尊号，
    // 再配二十个通用尊号；化神席位即使高度集中于同一属性，也不会轻易重名。
    private static readonly Dictionary<string, string[]> HuaShenHonorificPools = new(StringComparer.Ordinal)
    {
        ["金"] = new[]
        {
            "庚辰仙君", "金阙仙君", "白锋仙君", "玄钧仙君", "鸣玉仙君",
            "断岳仙君", "流光仙君", "肃衡仙君", "太白仙君", "锐光仙君"
        },
        ["木"] = new[]
        {
            "青华仙君", "长青仙君", "扶桑仙君", "碧落仙君", "生玄仙君",
            "灵枢仙君", "苍梧仙君", "荣枯仙君", "建木仙君", "春生仙君"
        },
        ["水"] = new[]
        {
            "沧溟仙君", "玄澜仙君", "归潮仙君", "寒渊仙君", "天河仙君",
            "镜海仙君", "流月仙君", "雨师仙君", "澄波仙君", "溯川仙君"
        },
        ["火"] = new[]
        {
            "赤明仙君", "离焰仙君", "照夜仙君", "炎极仙君", "朱陵仙君",
            "焚霞仙君", "烛龙仙君", "丹霄仙君", "烈阳仙君", "烬明仙君"
        },
        ["土"] = new[]
        {
            "镇岳仙君", "厚载仙君", "山河仙君", "坤元仙君", "玄岩仙君",
            "地衡仙君", "封疆仙君", "万壑仙君", "承天仙君", "崇山仙君"
        },
        ["风"] = new[]
        {
            "御虚仙君", "长风仙君", "巽羽仙君", "扶摇仙君", "凌霄仙君",
            "清岚仙君", "追云仙君", "天游仙君", "流风仙君", "听涛仙君"
        },
        ["雷"] = new[]
        {
            "玄霄仙君", "玉枢仙君", "震岳仙君", "惊蛰仙君", "雷泽仙君",
            "神霆仙君", "紫电仙君", "天刑仙君", "霹雳仙君", "震玄仙君"
        },
        ["阴"] = new[]
        {
            "幽玄仙君", "玄阴仙君", "藏月仙君", "冥照仙君", "太阴仙君",
            "夜游仙君", "寒魄仙君", "隐渊仙君", "玄夜仙君", "幽明仙君"
        },
        ["阳"] = new[]
        {
            "昭阳仙君", "扶光仙君", "曜真仙君", "大明仙君", "景曜仙君",
            "洞阳仙君", "曦和仙君", "明玄仙君", "朝元仙君", "照真仙君"
        },
        ["空间"] = new[]
        {
            "太虚仙君", "界行仙君", "空玄仙君", "渡界仙君", "无垠仙君",
            "星门仙君", "寰宇仙君", "虚渡仙君", "界海仙君", "游空仙君"
        }
    };

    private static readonly string[] GenericHuaShenHonorifics =
    {
        "玄微仙君", "洞真仙君", "守一仙君", "抱朴仙君", "灵台仙君",
        "玉景仙君", "清虚仙君", "道衡仙君", "冲和仙君", "无尘仙君",
        "归元仙君", "明道仙君", "含章仙君", "太和仙君", "观妙仙君",
        "澄玄仙君", "灵虚仙君", "知微仙君", "元景仙君", "照心仙君"
    };

    private static readonly Dictionary<string, string> HuaShenCategoryByTag = new(StringComparer.Ordinal)
    {
        ["金"] = "金", ["锋锐"] = "金", ["秩序"] = "金",
        ["木"] = "木", ["生机"] = "木", ["繁衍"] = "木", ["重生"] = "木",
        ["水"] = "水", ["流转"] = "水",
        ["火"] = "火", ["燃烧"] = "火", ["毁灭"] = "火", ["余烬"] = "火",
        ["土"] = "土", ["山"] = "土", ["稳定"] = "土", ["封镇"] = "土",
        ["风"] = "风", ["速度"] = "风",
        ["雷"] = "雷", ["惩戒"] = "雷", ["电"] = "雷",
        ["阴"] = "阴", ["暗"] = "阴", ["寒"] = "阴", ["隐匿"] = "阴",
        ["阳"] = "阳", ["光"] = "阳",
        ["空间"] = "空间", ["空"] = "空间", ["虚空"] = "空间", ["迁跃"] = "空间"
    };

    private static readonly Dictionary<string, string> HeDaoHonorificBySoulId = new(StringComparer.Ordinal)
    {
        ["soul_attr_metal"] = "金衡仙尊",
        ["soul_attr_wood"] = "青生仙尊",
        ["soul_attr_water"] = "玄澜仙尊",
        ["soul_attr_fire"] = "赤明仙尊",
        ["soul_attr_earth"] = "镇岳仙尊",
        ["soul_attr_wind"] = "御虚仙尊",
        ["soul_attr_thunder"] = "天刑仙尊",
        ["soul_attr_yin"] = "幽玄仙尊",
        ["soul_attr_yang"] = "昭阳仙尊",
        ["soul_attr_space"] = "太虚仙尊",
        ["soul_trace_ash"] = "烬劫仙尊",
        ["soul_trace_tide"] = "潮生仙尊",
        ["soul_trace_star"] = "陨星仙尊",
        ["soul_trace_shadow"] = "藏影仙尊",
        ["soul_trace_root"] = "归根仙尊"
    };

    private static readonly Dictionary<string, string> HeDaoHonorificBySoulName = new(StringComparer.Ordinal)
    {
        ["金魄"] = "金衡仙尊", ["木魄"] = "青生仙尊", ["水魄"] = "玄澜仙尊",
        ["火魄"] = "赤明仙尊", ["土魄"] = "镇岳仙尊", ["风魄"] = "御虚仙尊",
        ["雷魄"] = "天刑仙尊", ["阴魄"] = "幽玄仙尊", ["阳魄"] = "昭阳仙尊",
        ["空魄"] = "太虚仙尊", ["烬魄"] = "烬劫仙尊", ["潮魄"] = "潮生仙尊",
        ["陨魄"] = "陨星仙尊", ["影魄"] = "藏影仙尊", ["根魄"] = "归根仙尊"
    };

    private static readonly string[] GenericHeDaoHonorifics =
    {
        "玄元仙尊", "太清仙尊", "洞玄仙尊", "无极仙尊", "冲虚仙尊",
        "归真仙尊", "玉宸仙尊", "太和仙尊", "道衡仙尊", "玄都仙尊"
    };

    private static readonly Dictionary<string, string> ChangShengHonorificByTruthId = new(StringComparer.Ordinal)
    {
        ["truth_chuanfa_new_law"] = "传法天尊",
        ["truth_one_heart"] = "一心天尊",
        ["truth_wuyou"] = "无忧天尊",
        ["truth_wangsheng"] = "往生天尊",
        ["truth_mortal_miasma"] = "白先生",
        ["truth_human_will"] = "人道天尊",
        ["truth_true_unreal"] = "真实天尊",
        ["truth_player_many_paths"] = "殊途天尊",
        ["truth_player_failure_steps"] = "登阶天尊",
        ["truth_player_wounds_forge_body"] = "不坏天尊",
        ["truth_player_disaster_chance"] = "劫生天尊",
        ["truth_player_weak_not_fixed"] = "易势天尊",
        ["truth_player_trace_persistence"] = "留痕天尊",
        ["truth_player_lifespan_drain"] = "夺寿天尊",
        ["truth_player_duty_not_fixed"] = "自在天尊",
        ["truth_player_flawed_dao"] = "缺道天尊",
        ["truth_player_all_laws_one"] = "归一天尊",
        ["truth_player_reincarnation_unbroken"] = "轮回天尊"
    };

    private static readonly Dictionary<string, string> ChangShengHonorificByTruthName = new(StringComparer.Ordinal)
    {
        ["传法新法"] = "传法天尊",
        ["万众一心"] = "一心天尊",
        ["忘忧而乐"] = "无忧天尊",
        ["逆转死生"] = "往生天尊",
        ["仙凡瘴之理"] = "白先生",
        ["改天换地，人人可为"] = "人道天尊",
        ["真假之变"] = "真实天尊",
        ["法同异途"] = "殊途天尊",
        ["败痕为阶"] = "登阶天尊",
        ["伤可铸身"] = "不坏天尊",
        ["劫生灵机"] = "劫生天尊",
        ["强弱非定"] = "易势天尊",
        ["道痕常在"] = "留痕天尊",
        ["寿元可夺"] = "夺寿天尊",
        ["职非天定"] = "自在天尊",
        ["大道有缺"] = "缺道天尊",
        ["万法归一"] = "归一天尊",
        ["轮回不灭"] = "轮回天尊",
        ["未名逆理"] = "无名天尊",
        ["长生"] = "无名天尊"
    };

    private static readonly Dictionary<string, long> HuaShenHonorificOwnerByTitle = new(StringComparer.Ordinal);
    private static readonly Dictionary<string, long> AncientHuaShenHonorificOwnerByTitle = new(StringComparer.Ordinal);
    private static readonly Dictionary<string, long> AncientHeDaoHonorificOwnerByTitle = new(StringComparer.Ordinal);

    private static readonly Dictionary<long, string> NameByActorId = new();
    private static readonly Dictionary<string, long> ActorIdByName = new(StringComparer.Ordinal);

    internal static string Format(Actor actor, string realmId)
    {
        long actorId = SafeActorId(actor);
        string rawName;
        try { rawName = actor?.getName(); }
        catch { rawName = string.Empty; }
        string personalName = ResolvePersonalName(rawName, actorId);
        return Compose(personalName, ResolveActorHonorific(actor, realmId), realmId);
    }

    internal static string Format(string baseName, long actorId, string realmId)
    {
        return Compose(ResolvePersonalName(baseName, actorId), Honorific(realmId), realmId);
    }

    /// <summary>
    /// 感气至元婴只显示姓名与境界。新法化神、合道、长生分别使用
    /// 仙君、仙尊、天尊体系；旧法化神、合道分别使用神君、道君体系，
    /// 且两套尊号独立存档、互不污染。
    /// </summary>
    internal static string Honorific(string realmId) => realmId switch
    {
        MclslRealmIds.HuaShen => "仙君",
        MclslRealmIds.HeDao => "仙尊",
        MclslRealmIds.ChangSheng => "天尊",
        _ => string.Empty
    };

    internal static string WorldSoulHonorific(string soulId, string soulName)
    {
        string id = CleanToken(soulId);
        if (!string.IsNullOrWhiteSpace(id)
            && HeDaoHonorificBySoulId.TryGetValue(id, out string byId))
            return byId;

        string name = CleanToken(soulName);
        if (!string.IsNullOrWhiteSpace(name)
            && HeDaoHonorificBySoulName.TryGetValue(name, out string byName))
            return byName;

        if (string.IsNullOrWhiteSpace(name) || name.StartsWith("无名", StringComparison.Ordinal)) return string.Empty;
        if (name.EndsWith("仙尊", StringComparison.Ordinal)) return name;
        if (name.EndsWith("魄", StringComparison.Ordinal)) name = name[..^1];
        return string.IsNullOrWhiteSpace(name) ? string.Empty : name + "仙尊";
    }

    internal static string InverseTruthHonorific(string truthId, string truthName)
    {
        string id = CleanToken(truthId);
        if (!string.IsNullOrWhiteSpace(id)
            && ChangShengHonorificByTruthId.TryGetValue(id, out string byId))
            return byId;

        string name = CleanToken(truthName);
        if (!string.IsNullOrWhiteSpace(name)
            && ChangShengHonorificByTruthName.TryGetValue(name, out string byName))
            return byName;

        if (string.IsNullOrWhiteSpace(name)) return "无名天尊";
        if (name.EndsWith("天尊", StringComparison.Ordinal) || name.EndsWith("先生", StringComparison.Ordinal))
            return name;
        if (name.EndsWith("之理", StringComparison.Ordinal)) name = name[..^2];
        return string.IsNullOrWhiteSpace(name) ? "无名天尊" : name + "天尊";
    }

    private static string ResolveActorHonorific(Actor actor, string realmId)
    {
        if (actor?.data == null) return Honorific(realmId);
        bool ancientLaw = IsAncientLaw(actor);
        return realmId switch
        {
            MclslRealmIds.HuaShen => ancientLaw
                ? ResolveAncientHuaShenHonorific(actor)
                : ResolveHuaShenHonorific(actor),
            MclslRealmIds.HeDao => ancientLaw
                ? ResolveAncientHeDaoHonorific(actor)
                : ResolveHeDaoHonorific(actor),
            MclslRealmIds.ChangSheng => ancientLaw
                ? ResolveAncientChangShengHonorific(actor)
                : ResolveChangShengHonorific(actor),
            _ => string.Empty
        };
    }

    private static bool IsAncientLaw(Actor actor)
    {
        string system = ReadActorString(actor, MclslActorDataKeys.CultivationSystem);
        if (string.Equals(system, MclslCultivationSystemIds.AncientLaw, StringComparison.Ordinal)) return true;
        if (string.Equals(system, MclslCultivationSystemIds.NewLaw, StringComparison.Ordinal)) return false;

        // 仅用于极早期旧档：体系字段缺失时，优先依据旧法专属阶段数据判断。
        bool ancientEvidence = !string.IsNullOrWhiteSpace(ReadActorString(actor, MclslActorDataKeys.AncientDivineIntent))
            || !string.IsNullOrWhiteSpace(ReadActorString(actor, MclslActorDataKeys.AncientDaoName))
            || !string.IsNullOrWhiteSpace(ReadActorString(actor, MclslActorDataKeys.AncientLawStatus));
        bool newLawEvidence = !string.IsNullOrWhiteSpace(ReadActorString(actor, MclslActorDataKeys.DivineMarrowName))
            || !string.IsNullOrWhiteSpace(ReadActorString(actor, MclslActorDataKeys.WorldSoulId))
            || !string.IsNullOrWhiteSpace(ReadActorString(actor, MclslActorDataKeys.InverseTruthId));
        return ancientEvidence && !newLawEvidence;
    }

    private static string ResolveAncientHuaShenHonorific(Actor actor)
    {
        long actorId = SafeActorId(actor);
        string stored = CleanToken(ReadActorString(actor, MclslActorDataKeys.AncientHuaShenHonorific));
        if (IsFormalHonorific(stored, "神君")
            && TryClaimAncientHonorific(actorId, stored, AncientHuaShenHonorificOwnerByTitle))
            return stored;

        string category = ResolveAncientCategory(actor);
        string[] categoryPool = AncientHuaShenHonorificPools.TryGetValue(category, out string[] pool)
            ? pool
            : Array.Empty<string>();
        string selected = PickUnclaimedAncientHonorific(actorId, category + "|huashen", categoryPool,
            AncientHuaShenHonorificOwnerByTitle);
        if (string.IsNullOrWhiteSpace(selected))
            selected = PickUnclaimedAncientHonorific(actorId, "ancient|huashen|generic",
                GenericAncientHuaShenHonorifics, AncientHuaShenHonorificOwnerByTitle);
        if (string.IsNullOrWhiteSpace(selected)) selected = "无名神君";

        WriteActorString(actor, MclslActorDataKeys.AncientHuaShenHonorific, selected);
        return selected;
    }

    private static string ResolveAncientHeDaoHonorific(Actor actor)
    {
        long actorId = SafeActorId(actor);
        string stored = CleanToken(ReadActorString(actor, MclslActorDataKeys.AncientHeDaoHonorific));
        if (IsFormalHonorific(stored, "道君")
            && TryClaimAncientHonorific(actorId, stored, AncientHeDaoHonorificOwnerByTitle))
            return stored;

        string category = ResolveAncientCategory(actor);
        string[] categoryPool = AncientHeDaoHonorificPools.TryGetValue(category, out string[] pool)
            ? pool
            : Array.Empty<string>();
        string selected = PickUnclaimedAncientHonorific(actorId, category + "|hedao", categoryPool,
            AncientHeDaoHonorificOwnerByTitle);
        if (string.IsNullOrWhiteSpace(selected))
            selected = PickUnclaimedAncientHonorific(actorId, "ancient|hedao|generic",
                GenericAncientHeDaoHonorifics, AncientHeDaoHonorificOwnerByTitle);
        if (string.IsNullOrWhiteSpace(selected)) selected = "无名道君";

        WriteActorString(actor, MclslActorDataKeys.AncientHeDaoHonorific, selected);
        return selected;
    }

    private static string ResolveAncientChangShengHonorific(Actor actor)
    {
        string stored = CleanToken(ReadActorString(actor, MclslActorDataKeys.AncientChangShengHonorific));
        if (IsFormalHonorific(stored, "道祖")) return stored;

        string category = ResolveAncientCategory(actor);
        string selected = AncientChangShengHonorificByCategory.TryGetValue(category, out string mapped)
            ? mapped
            : GenericAncientChangShengHonorifics[
                StableHash(SafeActorId(actor) + "|ancient|changsheng") % GenericAncientChangShengHonorifics.Length];
        WriteActorString(actor, MclslActorDataKeys.AncientChangShengHonorific, selected);
        return selected;
    }

    private static string ResolveHuaShenHonorific(Actor actor)
    {
        long actorId = SafeActorId(actor);
        string stored = CleanToken(ReadActorString(actor, MclslActorDataKeys.HuaShenHonorific));
        if (IsFormalHonorific(stored, "仙君") && TryClaimHuaShenHonorific(actorId, stored))
            return stored;

        string category = ResolveHuaShenCategory(actor);
        string[] categoryPool = HuaShenHonorificPools.TryGetValue(category, out string[] pool)
            ? pool
            : Array.Empty<string>();
        string selected = PickUnclaimedHonorific(actorId, category, categoryPool);
        if (string.IsNullOrWhiteSpace(selected))
            selected = PickUnclaimedHonorific(actorId, category + "|generic", GenericHuaShenHonorifics);
        if (string.IsNullOrWhiteSpace(selected))
        {
            // 同一世界化神席位上限远低于可用尊号数；仅异常旧档可能到达此处。
            selected = "无名仙君";
            TryClaimHuaShenHonorific(actorId, selected);
        }

        WriteActorString(actor, MclslActorDataKeys.HuaShenHonorific, selected);
        return selected;
    }

    private static string ResolveHeDaoHonorific(Actor actor)
    {
        string soulId = ReadActorString(actor, MclslActorDataKeys.WorldSoulId);
        string soulName = ReadActorString(actor, MclslActorDataKeys.WorldSoulName);
        string expected = WorldSoulHonorific(soulId, soulName);
        if (string.IsNullOrWhiteSpace(expected))
        {
            string stored = CleanToken(ReadActorString(actor, MclslActorDataKeys.HeDaoHonorific));
            if (IsFormalHonorific(stored, "仙尊")) return stored;
            int index = StableHash(SafeActorId(actor) + "|hedao|"
                + ReadActorString(actor, MclslActorDataKeys.TechniqueName) + "|"
                + ReadActorString(actor, MclslActorDataKeys.GoldenCoreLaws))
                % GenericHeDaoHonorifics.Length;
            expected = GenericHeDaoHonorifics[index];
        }

        string current = CleanToken(ReadActorString(actor, MclslActorDataKeys.HeDaoHonorific));
        if (!string.Equals(current, expected, StringComparison.Ordinal))
            WriteActorString(actor, MclslActorDataKeys.HeDaoHonorific, expected);
        return expected;
    }

    private static string ResolveChangShengHonorific(Actor actor)
    {
        string truthId = ReadActorString(actor, MclslActorDataKeys.InverseTruthId);
        string truthName = ReadActorString(actor, MclslActorDataKeys.InverseTruthName);
        string expected = InverseTruthHonorific(truthId, truthName);
        string current = CleanToken(ReadActorString(actor, MclslActorDataKeys.ChangShengHonorific));
        if (!string.Equals(current, expected, StringComparison.Ordinal))
            WriteActorString(actor, MclslActorDataKeys.ChangShengHonorific, expected);
        return expected;
    }

    private static string ResolveHuaShenCategory(Actor actor)
    {
        string[] sources =
        {
            ReadActorString(actor, MclslActorDataKeys.DivineMarrowTags),
            ReadActorString(actor, MclslActorDataKeys.DivineChangeTags),
            ReadActorString(actor, MclslActorDataKeys.NascentEssenceTags),
            ReadActorString(actor, MclslActorDataKeys.GoldenCoreLaws),
            ReadActorString(actor, MclslActorDataKeys.SpiritualRootPrimary),
            ReadActorString(actor, MclslActorDataKeys.SpiritualRootAttributes)
        };

        for (int i = 0; i < sources.Length; i++)
        {
            string category = ResolveCategoryFromText(sources[i]);
            if (!string.IsNullOrWhiteSpace(category)) return category;
        }

        string techniqueId = ReadActorString(actor, MclslActorDataKeys.TechniqueId);
        if (MclslCultivationCatalog.TryTechnique(techniqueId, out MclslTechniqueDefinition technique))
        {
            for (int i = 0; i < technique.LawPool.Length; i++)
            {
                string category = ResolveCategoryFromText(technique.LawPool[i]);
                if (!string.IsNullOrWhiteSpace(category)) return category;
            }
        }
        return string.Empty;
    }

    private static string ResolveAncientCategory(Actor actor)
    {
        string[] sources =
        {
            ReadActorString(actor, MclslActorDataKeys.AncientDaoName),
            ReadActorString(actor, MclslActorDataKeys.AncientDivineIntent),
            ReadActorString(actor, MclslActorDataKeys.AncientDaoIntent),
            ReadActorString(actor, MclslActorDataKeys.AncientCoreName),
            ReadActorString(actor, MclslActorDataKeys.AncientFoundationName)
        };

        for (int i = 0; i < sources.Length; i++)
        {
            string category = ResolveCategoryFromText(sources[i]);
            if (!string.IsNullOrWhiteSpace(category)) return category;
        }

        string techniqueId = ReadActorString(actor, MclslActorDataKeys.TechniqueId);
        if (MclslCultivationCatalog.TryTechnique(techniqueId, out MclslTechniqueDefinition technique))
        {
            for (int i = 0; i < technique.LawPool.Length; i++)
            {
                string category = ResolveCategoryFromText(technique.LawPool[i]);
                if (!string.IsNullOrWhiteSpace(category)) return category;
            }
        }
        return string.Empty;
    }

    private static string ResolveCategoryFromText(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return string.Empty;
        string[] tokens = value.Split(new[] { ',', '，', '、', '|', ';', '；', '/', ' ' },
            StringSplitOptions.RemoveEmptyEntries);
        for (int i = 0; i < tokens.Length; i++)
        {
            string token = CleanToken(tokens[i]);
            if (HuaShenCategoryByTag.TryGetValue(token, out string exact)) return exact;
        }

        // 旧法道意可能是完整名称而非标签，按核心属性字作最后兜底。
        string compact = value.Replace(" ", string.Empty);
        string[] primary = { "空间", "虚空", "金", "木", "水", "火", "土", "风", "雷", "阴", "阳" };
        for (int i = 0; i < primary.Length; i++)
        {
            string token = primary[i];
            if (!compact.Contains(token, StringComparison.Ordinal)) continue;
            return token is "空间" or "虚空" ? "空间" : token;
        }
        return string.Empty;
    }

    private static string PickUnclaimedHonorific(long actorId, string salt, string[] pool)
    {
        if (pool == null || pool.Length == 0) return string.Empty;
        int start = StableHash(actorId + "|" + salt + "|honorific") % pool.Length;
        for (int offset = 0; offset < pool.Length; offset++)
        {
            string candidate = pool[(start + offset) % pool.Length];
            if (TryClaimHuaShenHonorific(actorId, candidate)) return candidate;
        }
        return string.Empty;
    }

    private static bool TryClaimHuaShenHonorific(long actorId, string title)
    {
        if (string.IsNullOrWhiteSpace(title)) return false;
        if (actorId <= 0L) return true;
        if (HuaShenHonorificOwnerByTitle.TryGetValue(title, out long owner) && owner != actorId) return false;
        HuaShenHonorificOwnerByTitle[title] = actorId;
        return true;
    }

    private static string PickUnclaimedAncientHonorific(long actorId, string salt, string[] pool,
        Dictionary<string, long> owners)
    {
        if (pool == null || pool.Length == 0) return string.Empty;
        int start = StableHash(actorId + "|" + salt + "|honorific") % pool.Length;
        for (int offset = 0; offset < pool.Length; offset++)
        {
            string candidate = pool[(start + offset) % pool.Length];
            if (TryClaimAncientHonorific(actorId, candidate, owners)) return candidate;
        }
        return string.Empty;
    }

    private static bool TryClaimAncientHonorific(long actorId, string title,
        Dictionary<string, long> owners)
    {
        if (string.IsNullOrWhiteSpace(title)) return false;
        if (actorId <= 0L) return true;
        if (owners.TryGetValue(title, out long owner) && owner != actorId) return false;
        owners[title] = actorId;
        return true;
    }

    private static bool IsFormalHonorific(string value, string suffix)
    {
        return !string.IsNullOrWhiteSpace(value)
            && value.Length > suffix.Length
            && value.EndsWith(suffix, StringComparison.Ordinal);
    }

    private static string CleanToken(string value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? string.Empty
            : value.Trim().Trim('【', '】', '·', '-', '—', '_');
    }

    private static string Compose(string personalName, string honorific, string realmId)
    {
        if (string.IsNullOrWhiteSpace(personalName)) return string.Empty;

        string stageName = realmId == SensingQiStage
            ? "感气"
            : MclslRealmIds.Index(realmId) >= 0
                ? MclslRealmIds.Display(realmId)
                : string.Empty;
        if (string.IsNullOrWhiteSpace(stageName)) return personalName;

        string decorated = string.IsNullOrWhiteSpace(honorific)
            ? personalName
            : honorific + "·" + personalName;
        return decorated + "-" + stageName;
    }

    private static string ReadActorString(Actor actor, string key)
    {
        if (actor?.data == null || string.IsNullOrWhiteSpace(key)) return string.Empty;
        try
        {
            ((BaseSystemData)actor.data).get(key, out string value, string.Empty);
            return value ?? string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }

    private static void WriteActorString(Actor actor, string key, string value)
    {
        if (actor?.data == null || string.IsNullOrWhiteSpace(key)) return;
        try
        {
            ((BaseSystemData)actor.data).set(key, value ?? string.Empty);
        }
        catch
        {
            // 尊号写入失败只影响显示，不得中断修炼链路。
        }
    }

    internal static void ClearRuntime()
    {
        NameByActorId.Clear();
        ActorIdByName.Clear();
        HuaShenHonorificOwnerByTitle.Clear();
        AncientHuaShenHonorificOwnerByTitle.Clear();
        AncientHeDaoHonorificOwnerByTitle.Clear();
    }

    private static string ResolvePersonalName(string value, long actorId)
    {
        if (actorId > 0L && NameByActorId.TryGetValue(actorId, out string cached)) return cached;

        string cleaned = StripLegacyDecoration(value);
        if (IsNaturalPersonalName(cleaned) && TryClaim(actorId, cleaned)) return cleaned;

        string preferredSurname = ExtractSurname(cleaned);
        string generated = GeneratePersonalName(actorId, cleaned, preferredSurname);
        TryClaim(actorId, generated);
        return generated;
    }

    private static string StripLegacyDecoration(string value)
    {
        string name = string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();

        // 旧版本格式为“尊号·姓名-境界”；可能经多次写回形成嵌套，故循环清理。
        int mark;
        while ((mark = name.IndexOf('·')) >= 0 && mark + 1 < name.Length)
            name = name[(mark + 1)..].Trim();

        bool removed;
        do
        {
            removed = false;
            const string sensingSuffix = "-感气";
            if (name.EndsWith(sensingSuffix, StringComparison.Ordinal))
            {
                name = name[..^sensingSuffix.Length].Trim();
                removed = true;
                continue;
            }

            foreach (string realm in MclslRealmIds.Ordered)
            {
                string suffix = "-" + MclslRealmIds.Display(realm);
                if (!name.EndsWith(suffix, StringComparison.Ordinal)) continue;
                name = name[..^suffix.Length].Trim();
                removed = true;
                break;
            }
        }
        while (removed);

        return name.Replace(" ", string.Empty).Trim('·', '-', '—', '_');
    }

    private static bool IsNaturalPersonalName(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return false;
        foreach (string marker in InvalidMarkers)
            if (name.Contains(marker, StringComparison.Ordinal)) return false;
        foreach (char c in name)
            if (c < '\u4e00' || c > '\u9fff') return false;

        string surname = ExtractSurname(name);
        if (string.IsNullOrWhiteSpace(surname)) return false;
        int givenLength = name.Length - surname.Length;
        return givenLength is 1 or 2;
    }

    private static string ExtractSurname(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return string.Empty;
        for (int i = Surnames.Length - 1; i >= 0; i--)
        {
            string surname = Surnames[i];
            if (name.StartsWith(surname, StringComparison.Ordinal)) return surname;
        }
        return string.Empty;
    }

    private static bool TryClaim(long actorId, string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return false;
        if (actorId <= 0L) return true;
        if (ActorIdByName.TryGetValue(name, out long owner) && owner != actorId) return false;
        ActorIdByName[name] = actorId;
        NameByActorId[actorId] = name;
        return true;
    }

    private static string GeneratePersonalName(long actorId, string salt, string preferredSurname)
    {
        int pairCount = GivenFirst.Length * GivenSecond.Length;
        int startPair = StableHash(actorId + "|" + salt + "|preferred") % Math.Max(1, pairCount);

        // 先尽量保留原姓氏；同姓重名过多时再进入完整姓名空间。
        if (!string.IsNullOrWhiteSpace(preferredSurname))
        {
            for (int offset = 0; offset < pairCount; offset++)
            {
                int pair = (startPair + offset) % pairCount;
                string first = GivenFirst[pair % GivenFirst.Length];
                string second = GivenSecond[(pair / GivenFirst.Length) % GivenSecond.Length];
                if (string.Equals(first, second, StringComparison.Ordinal)) continue;
                string candidate = preferredSurname + first + second;
                if (!ActorIdByName.TryGetValue(candidate, out long owner) || owner == actorId)
                    return candidate;
            }
        }

        int totalNames = Surnames.Length * pairCount;
        int startName = StableHash(actorId + "|" + salt + "|global") % Math.Max(1, totalNames);
        for (int offset = 0; offset < totalNames; offset++)
        {
            int index = (startName + offset) % totalNames;
            int pair = index / Surnames.Length;
            string first = GivenFirst[pair % GivenFirst.Length];
            string second = GivenSecond[(pair / GivenFirst.Length) % GivenSecond.Length];
            if (string.Equals(first, second, StringComparison.Ordinal)) continue;
            string candidate = Surnames[index % Surnames.Length] + first + second;
            if (!ActorIdByName.TryGetValue(candidate, out long owner) || owner == actorId)
                return candidate;
        }

        // 当前姓名空间远大于 WorldBox 可运行人口；仅在数据严重损坏时到达此处。
        int fallback = StableHash(actorId + "|" + salt + "|fallback");
        return Surnames[fallback % Surnames.Length]
            + GivenFirst[(fallback / Surnames.Length) % GivenFirst.Length]
            + GivenSecond[(fallback / Math.Max(1, Surnames.Length * GivenFirst.Length)) % GivenSecond.Length];
    }

    private static long SafeActorId(Actor actor)
    {
        try { return actor?.data == null ? 0L : ((BaseSystemData)actor.data).id; }
        catch { return 0L; }
    }

    private static int StableHash(string value)
    {
        unchecked
        {
            int hash = 31;
            foreach (char c in value ?? string.Empty) hash = hash * 41 + c;
            return hash & int.MaxValue;
        }
    }
}
