using System;
using System.Collections.Generic;
using System.Linq;

namespace MySimulatedLongevityRoad.Data;

internal static class MclslLawInteractionCatalog
{
    private static readonly Dictionary<string, string> Generates = new(StringComparer.Ordinal)
    {
        ["木"] = "火",
        ["火"] = "土",
        ["土"] = "金",
        ["金"] = "水",
        ["水"] = "木",
        ["空间"] = "风",
        ["风"] = "雷",
        ["雷"] = "阳",
        ["阳"] = "火",
        ["阴"] = "水"
    };

    private static readonly Dictionary<string, string> Restrains = new(StringComparer.Ordinal)
    {
        ["木"] = "土",
        ["土"] = "水",
        ["水"] = "火",
        ["火"] = "金",
        ["金"] = "木",
        ["风"] = "土",
        ["雷"] = "木",
        ["阴"] = "阳",
        ["阳"] = "阴",
        ["空间"] = "风"
    };

    private static readonly Dictionary<string, string[]> Supports = new(StringComparer.Ordinal)
    {
        ["火"] = new[] { "燃烧", "光", "毁灭", "阳" },
        ["水"] = new[] { "寒", "流转", "阴", "净化" },
        ["木"] = new[] { "生机", "繁衍", "重生" },
        ["土"] = new[] { "稳定", "封镇" },
        ["金"] = new[] { "锋锐", "秩序", "终结" },
        ["风"] = new[] { "速度", "流转", "迁跃" },
        ["雷"] = new[] { "惩戒", "毁灭", "速度" },
        ["阴"] = new[] { "寒", "隐匿", "转化" },
        ["阳"] = new[] { "光", "生机", "燃烧" },
        ["空间"] = new[] { "隐匿", "迁跃", "封镇" }
    };

    internal static int InteractionScore(IReadOnlyList<string> source, IReadOnlyList<string> target)
    {
        if (source == null || target == null || source.Count == 0 || target.Count == 0) return 0;
        int score = 0;
        foreach (string s in Clean(source))
        {
            foreach (string t in Clean(target))
            {
                if (s == t) score += 16;
                if (Generates.TryGetValue(s, out string born) && born == t) score += 10;
                if (Generates.TryGetValue(t, out string feeds) && feeds == s) score += 6;
                if (Restrains.TryGetValue(s, out string restrained) && restrained == t) score -= 18;
                if (Restrains.TryGetValue(t, out string controller) && controller == s) score -= 12;
                if (Supports.TryGetValue(s, out string[] supports) && supports.Contains(t, StringComparer.Ordinal)) score += 8;
                if (Supports.TryGetValue(t, out string[] reverse) && reverse.Contains(s, StringComparer.Ordinal)) score += 4;
            }
        }
        return Math.Clamp(score, -40, 36);
    }

    internal static string Brief(IReadOnlyList<string> source, IReadOnlyList<string> target)
    {
        int score = InteractionScore(source, target);
        if (score >= 24) return "法则同源";
        if (score >= 10) return "法则相生";
        if (score <= -24) return "法则大克";
        if (score <= -10) return "法则相克";
        return "法则平衡";
    }

    internal static int CompatibilityScore(IReadOnlyList<string> source, IReadOnlyList<string> target, int minInteractionForNoOverlap = 18)
    {
        List<string> sourceTags = Clean(source ?? Array.Empty<string>()).ToList();
        List<string> targetTags = Clean(target ?? Array.Empty<string>()).ToList();
        if (sourceTags.Count == 0 || targetTags.Count == 0) return 0;

        int overlap = sourceTags.Count(x => targetTags.Contains(x, StringComparer.Ordinal));
        int interaction = InteractionScore(sourceTags, targetTags);
        if (overlap <= 0 && interaction < minInteractionForNoOverlap) return 0;

        int score = overlap > 0 ? 35 + overlap * 25 : 28 + interaction;
        if (sourceTags[0] == targetTags[0]) score += 10;
        score += interaction;
        score -= Math.Abs(sourceTags.Count - targetTags.Count) * 4;
        return Math.Clamp(score, 0, 100);
    }

    internal static string Detail(IReadOnlyList<string> source, IReadOnlyList<string> target)
    {
        List<string> sourceTags = Clean(source ?? Array.Empty<string>()).ToList();
        List<string> targetTags = Clean(target ?? Array.Empty<string>()).ToList();
        if (sourceTags.Count == 0 || targetTags.Count == 0) return "法则未明";

        string brief = Brief(sourceTags, targetTags);
        int score = CompatibilityScore(sourceTags, targetTags);
        List<string> common = sourceTags.Where(x => targetTags.Contains(x, StringComparer.Ordinal)).ToList();
        string commonText = common.Count == 0 ? string.Empty : "，同源：" + string.Join("、", common);
        return brief + "，适配" + score + "%" + commonText;
    }

    private static IEnumerable<string> Clean(IEnumerable<string> values) => values
        .Where(x => !string.IsNullOrWhiteSpace(x))
        .Select(x => x.Trim())
        .Distinct(StringComparer.Ordinal);
}
