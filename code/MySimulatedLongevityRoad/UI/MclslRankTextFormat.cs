using System;
using MySimulatedLongevityRoad.Core;
using MySimulatedLongevityRoad.Systems;
using NeoModLoader.General;
using UnityEngine;
using UnityEngine.UI;

namespace MySimulatedLongevityRoad.UI;

/// <summary>
/// Shared localized text for rank/codex rows. The rank snapshot source and the
/// rank window carried byte-identical copies of both lookups; the resolution
/// order (runtime locale, localized name, locale key, explicit fallback) and the
/// swallowed-exception diagnostics are preserved exactly.
/// </summary>
internal static class MclslRankTextFormat
{
    internal const string UnknownAsset = "未知生物";
    internal const string UnknownTrait = "未知特质";
    internal const string UnknownTraitFallback = "未知特征";

    internal static string AssetDisplayName(ActorAsset asset)
    {
        if (asset == null) return UnknownAsset;
        try
        {
            string name = asset.getLocalizedName();
            if (!string.IsNullOrWhiteSpace(name)) return name;
        }
        catch (Exception ex)
        {
            MclslGuard.Swallowed("empty-catch-code-MySimulatedLongevityRoad-UI-MclslRankTextFormat-cs-1", ex, MclslGuardScope.OptionalMember);
        }
        try
        {
            string name = LM.Get(asset.name_locale);
            if (!string.IsNullOrWhiteSpace(name)) return name;
        }
        catch (Exception ex)
        {
            MclslGuard.Swallowed("empty-catch-code-MySimulatedLongevityRoad-UI-MclslRankTextFormat-cs-2", ex, MclslGuardScope.OptionalMember);
        }
        return UnknownAsset;
    }

    internal static string TraitDisplayName(ActorTrait trait)
    {
        if (trait == null || string.IsNullOrWhiteSpace(trait.id)) return UnknownTrait;
        if (MclslLocalizationBridge.TryResolveRuntimeKey(trait.id, out string runtimeText)) return runtimeText;
        try
        {
            string localized = LM.Get("trait_" + trait.id);
            if (!string.IsNullOrWhiteSpace(localized)
                && !string.Equals(localized, "trait_" + trait.id, StringComparison.Ordinal)
                && !MclslLocalizationBridge.IsRuntimeKey(localized))
                return localized;
        }
        catch (Exception ex)
        {
            MclslGuard.Swallowed("empty-catch-code-MySimulatedLongevityRoad-UI-MclslRankTextFormat-cs-3", ex, MclslGuardScope.OptionalMember);
        }
        try
        {
            string localized = LM.Get(trait.id);
            if (!string.IsNullOrWhiteSpace(localized)
                && !string.Equals(localized, trait.id, StringComparison.Ordinal)
                && !MclslLocalizationBridge.IsRuntimeKey(localized))
                return localized;
        }
        catch (Exception ex)
        {
            MclslGuard.Swallowed("empty-catch-code-MySimulatedLongevityRoad-UI-MclslRankTextFormat-cs-4", ex, MclslGuardScope.OptionalMember);
        }
        return UnknownTraitFallback;
    }
    /// <summary>
    /// Search normalization shared by the three rank surfaces. The beast rank
    /// window, the rank snapshot source and the rank window carried identical
    /// private copies (ASCII space and the full-width ideographic space are both
    /// stripped, leading/trailing whitespace is trimmed first).
    /// </summary>
    internal static string NormalizeSearch(string value)
        => (value ?? string.Empty).Trim().Replace(" ", string.Empty).Replace("　", string.Empty);
    internal static void SetText(Transform root, string childName, string value, Color color)
    {
        Text text = root.Find(childName)?.GetComponent<Text>();
        if (text == null) return;
        text.text = value ?? string.Empty;
        text.color = color;
    }

    internal static string FormatPower(double power)
    {
        if (power <= 0d) return "凡人";
        if (power >= 1000000000000d) return (power / 1000000000000d).ToString("F2") + "万亿";
        if (power >= 100000000d) return (power / 100000000d).ToString("F2") + "亿";
        if (power >= 10000d) return (power / 10000d).ToString("F2") + "万";
        return power.ToString("F0");
    }
}
