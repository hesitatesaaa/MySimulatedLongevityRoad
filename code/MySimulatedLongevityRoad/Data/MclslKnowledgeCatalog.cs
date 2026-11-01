using System;
using System.Collections.Generic;

namespace MySimulatedLongevityRoad.Data;

internal sealed class MclslKnowledgeDefinition
{
    internal string Id { get; set; } = string.Empty;
    internal string Name { get; set; } = string.Empty;
    internal string Description { get; set; } = string.Empty;
    internal int TruthValue { get; set; }
}

internal sealed class MclslTimelineAnchorDefinition
{
    internal string Id { get; set; } = string.Empty;
    internal string Name { get; set; } = string.Empty;
    internal string Description { get; set; } = string.Empty;
    internal int EarliestYear { get; set; }
    internal int LatestYear { get; set; }
    internal string KnowledgeId { get; set; } = string.Empty;
    internal bool Terminal { get; set; }
}

internal static class MclslKnowledgeCatalog
{
    private static readonly Dictionary<string, MclslKnowledgeDefinition> Knowledge = new(StringComparer.Ordinal)
    {
        ["truth_mortal_separation"] = K("truth_mortal_separation", "仙凡隔绝", "传法新法与白先生逆理落地后，修仙文明与凡俗社会被更高层规则分开。", 2),
        ["truth_alliance_rise"] = K("truth_alliance_rise", "万仙盟成形", "新法传播后，万仙盟只在背景层面塑造秩序，不替代地图中的原生国家。", 3),
        ["truth_five_elders_current"] = K("truth_five_elders_current", "五老会暗流", "新法格局成形后，五老会以背景渗透与事件干预影响诸国。", 3),
        ["truth_lock_spirit_plan"] = K("truth_lock_spirit_plan", "锁灵大计", "新法繁盛后，世界灵气秩序被大型背景工程重新约束。", 5),
        ["truth_black_tide"] = K("truth_black_tide", "黑潮大劫", "新法深处暗潮翻涌，修士、传承与遗迹皆受其扰。", 6),
        ["truth_white_mist"] = K("truth_white_mist", "白雾异变", "玄黄界出现可被化神修士抽髓的世界异变。", 6),
        ["truth_end_dharma"] = K("truth_end_dharma", "末法终局", "天地灵机渐衰，资源与破境之路愈发艰涩。", 8),
        ["truth_xuanhuang_terminal"] = K("truth_xuanhuang_terminal", "玄黄终局", "本世情报与真值被封存。", 10)
    };

    internal static readonly IReadOnlyList<MclslTimelineAnchorDefinition> TimelineAnchors = new[]
    {
        A("anchor_mortal_separation", "仙凡隔绝", "传法新法与白先生逆理落地后，修士与凡俗的生存秩序开始分化。", 1030, 1120, "truth_mortal_separation"),
        A("anchor_alliance_rise", "万仙盟成形", "万仙盟在新法世界背景中建立跨国秩序。", 1220, 1450, "truth_alliance_rise"),
        A("anchor_five_elders_current", "五老会暗流", "五老会开始在诸国间制造渗透与对抗。", 1500, 1760, "truth_five_elders_current"),
        A("anchor_lock_spirit_plan", "锁灵大计", "锁灵体系逐渐改变世界灵气分布。", 1900, 2300, "truth_lock_spirit_plan"),
        A("anchor_black_tide", "黑潮大劫", "黑潮扰动新法秩序，传承与遗迹皆现裂痕。", 2350, 2550, "truth_black_tide"),
        A("anchor_white_mist", "白雾异变", "大规模天地之变出现。", 2600, 3000, "truth_white_mist"),
        A("anchor_end_dharma", "末法终局", "灵机衰迟，天地渐入末法。", 3300, 3700, "truth_end_dharma"),
        A("anchor_xuanhuang_terminal", "玄黄终局", "本世走向终局。", 4300, 4800, "truth_xuanhuang_terminal", true)
    };

    internal static bool TryGetKnowledge(string id, out MclslKnowledgeDefinition definition) => Knowledge.TryGetValue(id ?? string.Empty, out definition);
    internal static MclslTimelineAnchorDefinition GetAnchor(string id)
    {
        foreach (var anchor in TimelineAnchors) if (anchor.Id == id) return anchor;
        return null;
    }

    private static MclslKnowledgeDefinition K(string id, string name, string description, int value) => new() { Id = id, Name = name, Description = description, TruthValue = value };
    private static MclslTimelineAnchorDefinition A(string id, string name, string description, int min, int max, string knowledge, bool terminal = false) => new() { Id = id, Name = name, Description = description, EarliestYear = min, LatestYear = max, KnowledgeId = knowledge, Terminal = terminal };
}
