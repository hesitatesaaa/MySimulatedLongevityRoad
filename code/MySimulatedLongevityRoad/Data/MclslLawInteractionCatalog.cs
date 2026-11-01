using System;
using System.Collections.Generic;
using System.Text;

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
        for (int i = 0; i < source.Count; i++)
        {
            if (!UniqueTag(source, i, out string s)) continue;
            for (int j = 0; j < target.Count; j++)
            {
                if (!UniqueTag(target, j, out string t)) continue;
                if (s == t) score += 16;
                if (Generates.TryGetValue(s, out string born) && born == t) score += 10;
                if (Generates.TryGetValue(t, out string feeds) && feeds == s) score += 6;
                if (Restrains.TryGetValue(s, out string restrained) && restrained == t) score -= 18;
                if (Restrains.TryGetValue(t, out string controller) && controller == s) score -= 12;
                if (Supports.TryGetValue(s, out string[] supports) && ContainsTag(supports, t)) score += 8;
                if (Supports.TryGetValue(t, out string[] reverse) && ContainsTag(reverse, s)) score += 4;
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
        int sourceCount = CountTags(source, out string firstSource);
        int targetCount = CountTags(target, out string firstTarget);
        if (sourceCount == 0 || targetCount == 0) return 0;
        int overlap = 0;
        for (int i = 0; i < source.Count; i++)
            if (UniqueTag(source, i, out string tag) && ContainsTag(target, tag)) overlap++;
        int interaction = InteractionScore(source, target);
        if (overlap <= 0 && interaction < minInteractionForNoOverlap) return 0;
        int score = overlap > 0 ? 35 + overlap * 25 : 28 + interaction;
        if (firstSource == firstTarget) score += 10;
        score += interaction;
        score -= Math.Abs(sourceCount - targetCount) * 4;
        return Math.Clamp(score, 0, 100);
    }

    internal static string Detail(IReadOnlyList<string> source, IReadOnlyList<string> target)
    {
        if (CountTags(source, out _) == 0 || CountTags(target, out _) == 0) return "法则未明";
        var text = new StringBuilder(Brief(source, target));
        text.Append("，适配").Append(CompatibilityScore(source, target)).Append('%');
        bool first = true;
        for (int i = 0; i < source.Count; i++)
        {
            if (!UniqueTag(source, i, out string tag) || !ContainsTag(target, tag)) continue;
            text.Append(first ? "，同源：" : "、").Append(tag);
            first = false;
        }
        return text.ToString();
    }

    // Law tag sets are small. Compare in place, preserving first occurrence,
    // trimming and ordinal semantics without allocating lists or hash sets.
    private static bool UniqueTag(IReadOnlyList<string> tags, int index, out string tag)
    {
        tag = tags[index]?.Trim();
        if (string.IsNullOrEmpty(tag)) return false;
        for (int i = 0; i < index; i++)
            if (string.Equals(tags[i]?.Trim(), tag, StringComparison.Ordinal)) return false;
        return true;
    }

    private static bool ContainsTag(IReadOnlyList<string> tags, string tag)
    {
        if (tags == null) return false;
        for (int i = 0; i < tags.Count; i++)
            if (string.Equals(tags[i]?.Trim(), tag, StringComparison.Ordinal)) return true;
        return false;
    }

    private static int CountTags(IReadOnlyList<string> tags, out string first)
    {
        first = null;
        if (tags == null) return 0;
        int count = 0;
        for (int i = 0; i < tags.Count; i++)
        {
            if (!UniqueTag(tags, i, out string tag)) continue;
            if (count++ == 0) first = tag;
        }
        return count;
    }
}
