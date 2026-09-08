using System;
using System.Collections.Generic;
using System.Linq;
using MySimulatedLongevityRoad.Data;

namespace MySimulatedLongevityRoad.Systems;

internal static class MclslCultivationLineage
{
    internal static string[] RootTags(Actor actor)
    {
        if (actor?.data == null) return Array.Empty<string>();
        List<string> tags = new();
        Add(tags, MclslActorAccessor.GetString(actor, MclslActorDataKeys.FoundationWonderTags, string.Empty));
        Add(tags, MclslActorAccessor.GetString(actor, MclslActorDataKeys.GoldenCoreLaws, string.Empty));
        Add(tags, MclslActorAccessor.GetString(actor, MclslActorDataKeys.NascentEssenceTags, string.Empty));
        Add(tags, MclslActorAccessor.GetString(actor, MclslActorDataKeys.DivineMarrowTags, string.Empty));
        return tags.Distinct(StringComparer.Ordinal).ToArray();
    }

    internal static int Compatibility(Actor actor, string targetTags)
    {
        string[] roots = RootTags(actor);
        string[] target = Split(targetTags);
        if (roots.Length == 0 || target.Length == 0) return 0;
        int overlap = roots.Count(x => target.Contains(x, StringComparer.Ordinal));
        int score = overlap * 28;
        if (overlap > 0 && roots[0] == target[0]) score += 16;
        score += MclslLawInteractionCatalog.InteractionScore(roots, target);
        return Math.Clamp(score, 0, 100);
    }

    internal static int Integrity(Actor actor)
    {
        if (actor?.data == null) return 0;
        int value = 0;
        if (!string.IsNullOrWhiteSpace(MclslActorAccessor.GetString(actor, MclslActorDataKeys.FoundationWonderName, string.Empty)))
            value += 8 + Math.Clamp(MclslActorAccessor.GetInt(actor, MclslActorDataKeys.FoundationWonderQuality, 1), 1, 4) * 3;
        if (!string.IsNullOrWhiteSpace(MclslActorAccessor.GetString(actor, MclslActorDataKeys.GoldenCoreLaws, string.Empty)))
            value += 10 + Math.Clamp(MclslActorAccessor.GetInt(actor, MclslActorDataKeys.GoldenCorePurity, 0), 0, 100) / 10 + Math.Clamp(MclslActorAccessor.GetInt(actor, MclslActorDataKeys.GoldenCoreStability, 0), 0, 100) / 12;
        if (!string.IsNullOrWhiteSpace(MclslActorAccessor.GetString(actor, MclslActorDataKeys.NascentEssenceName, string.Empty)))
            value += 14 + Math.Clamp(MclslActorAccessor.GetInt(actor, MclslActorDataKeys.NascentEssenceQuality, 1), 1, 4) * 3 + Math.Clamp(MclslActorAccessor.GetInt(actor, MclslActorDataKeys.NascentCaveCompatibility, 0), 0, 100) / 20;
        if (!string.IsNullOrWhiteSpace(MclslActorAccessor.GetString(actor, MclslActorDataKeys.DivineMarrowName, string.Empty)))
            value += 18 + Math.Clamp(MclslActorAccessor.GetInt(actor, MclslActorDataKeys.DivineMarrowQuality, 1), 1, 4) * 3 + Math.Clamp(MclslActorAccessor.GetInt(actor, MclslActorDataKeys.DivineChangeCompatibility, 0), 0, 100) / 20;
        return Math.Clamp(value, 0, 100);
    }

    internal static string ChainSummary(Actor actor)
    {
        string[] tags = RootTags(actor);
        if (tags.Length == 0) return "未形成完整新法根基";
        return string.Join("、", tags.Take(6));
    }

    private static void Add(List<string> tags, string csv)
    {
        foreach (string tag in Split(csv))
            if (!string.IsNullOrWhiteSpace(tag)) tags.Add(tag);
    }

    private static string[] Split(string csv) => (csv ?? string.Empty)
        .Split(new[] { ',', '、', '，' }, StringSplitOptions.RemoveEmptyEntries)
        .Select(x => x.Trim())
        .Where(x => x.Length > 0)
        .ToArray();

}
