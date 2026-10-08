using System;
using NeoModLoader.General;

namespace MySimulatedLongevityRoad.Data;

internal static class MclslMissionSettlementPolicy
{
    internal static bool CanReward(bool failed, bool died) => !failed && !died;
    internal static string ResultCode(bool failed, bool died) => died ? "dead" : failed ? "failed" : "success";
}

internal readonly struct MclslMissionDefinition
{
    internal readonly string Key;
    private readonly string _fallbackName;
    internal readonly int Stones;
    internal readonly int Tier;
    internal string Name
    {
        get
        {
            if (string.IsNullOrWhiteSpace(Key)) return _fallbackName;
            try
            {
                string translated = LM.Get(Key);
                return string.IsNullOrWhiteSpace(translated) || translated == Key ? _fallbackName : translated;
            }
            catch { return _fallbackName; }
        }
    }

    internal MclslMissionDefinition(string name, int stones, int tier, string key = "")
    {
        Key = key;
        _fallbackName = name;
        Stones = stones;
        Tier = tier;
    }

    internal MclslMissionDefinition WithKey(string key) => new(_fallbackName, Stones, Tier, key);
}

internal static class MclslFactionMissionCatalog
{
    private static MclslMissionDefinition M(string name, int stones, int tier) => new(name, stones, tier);

    // Array indices follow MclslFactionMissionSystem.AncientSects.
    private static readonly MclslMissionDefinition[][] Ancient =
    {
        new[] { M("编修道藏",12,0), M("收集散卷",15,0), M("校勘残卷",19,1), M("寻访古碑",22,1), M("护送经匣",26,2), M("镇守藏阁",30,2) },
        new[] { M("试剑守关",13,0), M("打磨剑胚",16,0), M("清剿剑冢",20,1), M("追寻失剑",23,1), M("护送剑胚",27,2), M("斩除剑煞",31,2) },
        new[] { M("护养灵兽",12,0), M("修缮兽栏",15,0), M("寻回幼兽",20,1), M("驯服凶兽",23,1), M("镇抚兽潮",28,2), M("镇压妖王",32,2) },
        new[] { M("巡查狱阵",14,0), M("搜捕潜逃",17,0), M("缉拿逃修",21,1), M("巡狱护法",24,1), M("押送重犯",29,2), M("封镇狱门",32,2) },
        new[] { M("观星测轨",12,0), M("校准星盘",15,0), M("修补星盘",19,1), M("测算天机",22,1), M("追索星图",27,2), M("镇守星台",31,2) },
        new[] { M("寻访本源",13,0), M("采录初炁",16,0), M("采集源息",21,1), M("溯查古井",24,1), M("守护源眼",28,2), M("稳固源脉",32,2) },
        new[] { M("誊录道经",12,0), M("整理讲义",15,0), M("辨析真伪",20,1), M("巡护法坛",23,1), M("护持讲经",27,2), M("镇压道劫",31,2) },
        new[] { M("推演阵图",13,0), M("刻绘阵纹",16,0), M("修补阵眼",21,1), M("查验阵脉",24,1), M("试炼大阵",29,2), M("封护禁阵",32,2) },
        new[] { M("培育灵植",12,0), M("照看药圃",15,0), M("修复灵脉",20,1), M("调配灵液",23,1), M("镇护灵泉",28,2), M("净化毒脉",32,2) },
        new[] { M("校衡地脉",13,0), M("巡察地眼",16,0), M("调理气脉",22,1), M("修补地阵",25,1), M("平复地动",29,2), M("镇定龙脉",32,2) }
    };

    private static readonly MclslMissionDefinition[] WanXian =
    {
        M("巡查灵脉",0,0), M("护送仙册",2,0), M("检视矿脉",4,0), M("护持灵舟",6,0),
        M("缉录散修",0,1), M("清点遗藏",2,1), M("清剿邪修",4,1), M("修补城阵",6,1),
        M("核验功法",0,2), M("平息灵潮",2,2), M("镇守洞天",4,2), M("追缉叛盟",6,2)
    };

    private static readonly MclslMissionDefinition[] FiveElders =
    {
        M("夺取残卷",0,0), M("收买散修",2,0), M("潜查丹坊",4,0), M("截取密信",6,0),
        M("暗探遗迹",0,1), M("扰乱灵脉",2,1), M("窃取阵图",4,1), M("护送暗使",6,1),
        M("遮掩洞天",0,2), M("引动灾兆",2,2), M("夺取天地之精",4,2), M("诛除盟使",6,2)
    };

    static MclslFactionMissionCatalog()
    {
        string[] sectIds = { "dadao", "tianjian", "yushou", "wuding", "tianshu", "yishi", "taishang", "taiyan", "zaohua", "xuanheng" };
        for (int sect = 0; sect < Ancient.Length; sect++)
            for (int index = 0; index < Ancient[sect].Length; index++)
                Ancient[sect][index] = Ancient[sect][index].WithKey("mclsl_mission_" + sectIds[sect] + "_" + index);
        for (int index = 0; index < WanXian.Length; index++)
            WanXian[index] = WanXian[index].WithKey("mclsl_mission_wanxian_" + index);
        for (int index = 0; index < FiveElders.Length; index++)
            FiveElders[index] = FiveElders[index].WithKey("mclsl_mission_five_elders_" + index);
    }

    internal static MclslMissionDefinition[] AncientFor(int sectIndex) => Ancient[Math.Clamp(sectIndex, 0, Ancient.Length - 1)];
    internal static MclslMissionDefinition[] NewLawFor(string faction)
        => faction == "five_elders" ? FiveElders : WanXian;

    internal static MclslMissionDefinition PickAncient(int sectIndex, int tier, int roll)
        => AncientFor(sectIndex)[Math.Clamp(tier, 0, 2) * 2 + roll % 2];

    internal static MclslMissionDefinition PickNewLaw(string faction, int tier, int roll)
        => NewLawFor(faction)[Math.Clamp(tier, 0, 2) * 4 + roll % 4];
}
