using System;
using System.Collections.Generic;
using System.Linq;

namespace MySimulatedLongevityRoad.Data;

internal sealed class MclslNativeTerrainProfile
{
    internal string Name { get; set; } = "原生地形扰动";
    internal string NativeTiles { get; set; } = "草地、泥土、浅水、山地等原生地块";
    internal string Scope { get; set; } = "小范围地块替换";
    internal string Limitation { get; set; } = "仅允许使用 WorldBox 原生地形、火焰、熔岩、水域、冰雪、沙地、沼泽、腐败、森林、山地、矿石等已有效果，不创建自定义领域或隐藏数值。";

    internal string Summary => Name + "：" + NativeTiles + "；" + Scope + "。" + Limitation;
}

internal static class MclslNativeTerrainProfileCatalog
{
    internal static MclslNativeTerrainProfile ForTags(IReadOnlyList<string> lawTags, string sourceType = "")
    {
        HashSet<string> tags = new((lawTags ?? Array.Empty<string>()).Where(x => !string.IsNullOrWhiteSpace(x)), StringComparer.Ordinal);
        string source = sourceType ?? string.Empty;

        if (source.Contains("white_mist", StringComparison.Ordinal) || tags.Contains("空间") || tags.Contains("隐匿"))
            return P("界域错位", "浅水、冰雪、沙地、荒地、山地边界", "沿现有陆水/山地边界做有限替换");
        if (tags.Contains("封镇"))
            return P("封镇残痕", "山地、岩石、冰雪、浅水边界、矿石", "以不可通行或难通行原生地形表现镇压");
        if (tags.Contains("火") || tags.Contains("燃烧"))
            return P("火脉外泄", "火焰、焦土、熔岩、灰烬、山地", "围绕核心点形成小片灼烧或熔岩地形");
        if (tags.Contains("血") || tags.Contains("腐蚀"))
            return P("血煞蚀地", "腐败地、焦土、沼泽、浅水、枯萎森林", "以腐败和灼蚀痕迹表现血煞侵蚀");
        if (tags.Contains("雷"))
            return P("雷击裂地", "焦土、火焰、山地、裸露岩石", "零散破坏地表并引发短时燃烧");
        if (tags.Contains("水") || tags.Contains("流转"))
            return P("水脉改道", "浅水、深水、沼泽、冰雪、沙地", "把低洼边缘转为水域或湿地");
        if (tags.Contains("阴") || tags.Contains("暗") || tags.Contains("幽暗") || tags.Contains("转化"))
            return P("秽气蚀地", "腐败地、沼泽、枯萎森林、浅水", "以腐败或沼泽覆盖少量可通行地块");
        if (tags.Contains("光") || tags.Contains("迷幻"))
            return P("迷光留痕", "冰雪、浅水、沙地、草地边缘", "以亮色原生地块和水面边界表现迷光残留");
        if (tags.Contains("土"))
            return P("地脉隆沉", "泥土、山地、岩石、矿石、沙地", "轻微改变山地/荒地/矿脉分布");
        if (tags.Contains("木") || tags.Contains("生机"))
            return P("生机暴长", "森林、草地、肥沃土壤、浅水边缘", "在陆地与水边生成原生植被地形");
        if (tags.Contains("金") || tags.Contains("锋锐") || tags.Contains("剑") || tags.Contains("晶"))
            return P("金石出露", "山地、岩石、矿石、沙地", "把部分平地抬为岩石或矿脉地块");
        if (tags.Contains("寒"))
            return P("寒潮封境", "冰雪、冻土、浅水结冰边界", "局部替换为冰雪类原生地形");
        if (tags.Contains("毁灭"))
            return P("灾厄残痕", "焦土、灰烬、沙地、岩石、浅水坑", "以破坏后的原生地块表现剧变余波");

        return P("灵脉扰动", "草地、泥土、森林、浅水、山地", "按周边原生地貌做小范围替换");
    }

    internal static string ForSoul(IReadOnlyList<string> lawTags, string duty)
    {
        MclslNativeTerrainProfile profile = ForTags(lawTags, "world_soul");
        string dutyText = string.IsNullOrWhiteSpace(duty) ? "天职显化" : duty;
        return "天地之魄显化时只以敌对实体与原生地形痕迹表现“" + dutyText + "”："
            + profile.NativeTiles + "；不添加额外攻击速度、移动速度、爆发威能或法则权重。";
    }

    private static MclslNativeTerrainProfile P(string name, string tiles, string scope) => new()
    {
        Name = name,
        NativeTiles = tiles,
        Scope = scope
    };
}
