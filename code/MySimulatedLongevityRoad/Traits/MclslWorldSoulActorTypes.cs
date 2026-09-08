using System;
using UnityEngine;

namespace MySimulatedLongevityRoad.Traits;

internal static class MclslWorldSoulActorDefinitions
{
    internal static readonly SoulActorDefinition[] All =
    {
        new("soul_attr_metal", "mclsl_world_soul_jin", "Jin_Soul", new[] { "金", "锋锐", "秩序" }),
        new("soul_attr_wood", "mclsl_world_soul_wood", "Wood_Soul", new[] { "木", "生机", "繁衍" }),
        new("soul_attr_water", "mclsl_world_soul_water", "Water_Soul", new[] { "水", "寒", "流转" }),
        new("soul_attr_fire", "mclsl_world_soul_fire", "Fire_Soul", new[] { "火", "燃烧", "毁灭", "余烬" }),
        new("soul_attr_earth", "mclsl_world_soul_earth", "Earth_Soul", new[] { "土", "山", "稳定", "封镇" }),
        new("soul_attr_wind", "mclsl_world_soul_wind", "Wind_Soul", new[] { "风", "速度" }),
        new("soul_attr_thunder", "mclsl_world_soul_thunder", "Thunder_Soul", new[] { "雷", "惩戒", "电" }),
        new("soul_attr_yin", "mclsl_world_soul_yin", "Yin_Soul", new[] { "阴", "暗", "隐匿", "寒" }),
        new("soul_attr_yang", "mclsl_world_soul_yang", "Yang_Soul", new[] { "阳", "光", "生机" }),
        new("soul_attr_space", "mclsl_world_soul_kong", "Kong_Soul", new[] { "空间", "迁跃", "虚空" })
    };
}

internal sealed class SoulActorDefinition
{
    internal readonly string SoulId;
    internal readonly string ActorId;
    internal readonly string Folder;
    private readonly string[] _keys;

    internal SoulActorDefinition(string soulId, string actorId, string folder, string[] keys)
    {
        SoulId = soulId;
        ActorId = actorId;
        Folder = folder;
        _keys = keys ?? Array.Empty<string>();
    }

    internal int MatchScore(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return 0;
        int score = 0;
        for (int i = 0; i < _keys.Length; i++)
            if (!string.IsNullOrWhiteSpace(_keys[i]) && text.Contains(_keys[i], StringComparison.Ordinal))
                score += 10 + i;
        return score;
    }
}

internal readonly struct SoulSpriteSet
{
    internal readonly Sprite[] Idle;
    internal readonly Sprite[] Run;
    internal readonly Sprite[] Attack;
    internal readonly Sprite[] Death;
    internal readonly bool UsesGenericFallback;

    internal SoulSpriteSet(Sprite[] idle, Sprite[] run, Sprite[] attack, Sprite[] death, bool usesGenericFallback)
    {
        Idle = idle ?? Array.Empty<Sprite>();
        Run = run ?? Array.Empty<Sprite>();
        Attack = attack ?? Array.Empty<Sprite>();
        Death = death ?? Array.Empty<Sprite>();
        UsesGenericFallback = usesGenericFallback;
    }
}
