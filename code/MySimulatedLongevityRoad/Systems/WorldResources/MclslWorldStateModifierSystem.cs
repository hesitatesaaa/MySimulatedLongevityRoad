using System;
using MySimulatedLongevityRoad.Data;

namespace MySimulatedLongevityRoad.Systems;

internal readonly struct MclslWorldStateModifiers
{
    internal readonly string Id;
    internal readonly string Name;
    internal readonly string Summary;
    internal readonly float CultivationRate;
    internal readonly float ResourceIncome;
    internal readonly float CaveBirthRate;
    internal readonly float WorldChangeBirthRate;
    internal readonly float RuinRevealRate;
    internal readonly float FactionPressureRate;
    internal readonly int BreakthroughStability;

    internal MclslWorldStateModifiers(
        string id,
        string name,
        string summary,
        float cultivationRate,
        float resourceIncome,
        float caveBirthRate,
        float worldChangeBirthRate,
        float ruinRevealRate,
        float factionPressureRate,
        int breakthroughStability)
    {
        Id = id;
        Name = name;
        Summary = summary;
        CultivationRate = cultivationRate;
        ResourceIncome = resourceIncome;
        CaveBirthRate = caveBirthRate;
        WorldChangeBirthRate = worldChangeBirthRate;
        RuinRevealRate = ruinRevealRate;
        FactionPressureRate = factionPressureRate;
        BreakthroughStability = breakthroughStability;
    }
}

internal static class MclslWorldStateModifierSystem
{
    internal const string Normal = "normal";
    internal const string SpiritTide = "spirit_tide";
    internal const string LockSpiritAfterwave = "lock_spirit_afterwave";
    internal const string BlackTideTribulation = "black_tide_tribulation";
    internal const string WhiteMistEncroachment = "white_mist_encroachment";
    internal const string EndDharmaTerminal = "end_dharma_terminal";
    internal const string XuanhuangTerminal = "xuanhuang_terminal";

    private static int _cachedYear = int.MinValue;
    private static string _cachedKey = string.Empty;
    private static MclslWorldStateModifiers _cached = NormalModifiers;

    private static readonly MclslWorldStateModifiers NormalModifiers = new(
        Normal,
        "天地平衡",
        "天地灵机循常流转。",
        1f,
        1f,
        1f,
        1f,
        1f,
        1f,
        0);

    internal static MclslWorldStateModifiers Current(int year)
    {
        string key = CacheKey();
        if (_cachedYear == year && string.Equals(_cachedKey, key, StringComparison.Ordinal)) return _cached;
        _cachedYear = year;
        _cachedKey = key;
        _cached = Resolve(year);
        return _cached;
    }

    internal static void ClearCache()
    {
        _cachedYear = int.MinValue;
        _cachedKey = string.Empty;
        _cached = NormalModifiers;
    }

    internal static int ScaleCaveInterval(int baseInterval, int year)
    {
        return ScaleInterval(baseInterval, Current(year).CaveBirthRate);
    }

    internal static int ScaleWorldChangeInterval(int baseInterval, int year)
    {
        return ScaleInterval(baseInterval, Current(year).WorldChangeBirthRate);
    }

    internal static int ScaleRuinInterval(int baseInterval, int year)
    {
        return ScaleInterval(baseInterval, Current(year).RuinRevealRate);
    }

    internal static int ScaleFactionPressureInterval(int baseInterval, int year)
    {
        return ScaleInterval(baseInterval, Current(year).FactionPressureRate);
    }

    internal static int ScaleResourceIncome(int amount, int year)
    {
        if (amount <= 0) return amount;
        return Math.Max(1, (int)MathF.Round(amount * Current(year).ResourceIncome));
    }

    internal static int ScaleFactionPressureDelta(int amount, int year)
    {
        if (amount <= 0) return amount;
        return Math.Max(1, (int)MathF.Round(amount * Current(year).FactionPressureRate));
    }

    internal static int BreakthroughStabilityBonus(int year)
    {
        return Current(year).BreakthroughStability;
    }

    internal static string CurrentName(int year)
    {
        return Current(year).Name;
    }

    internal static int EstimateCurrentStateEndYear(int year)
    {
        MclslWorldRunState run = MclslWorldRunRepository.Current;
        if (run == null || string.IsNullOrWhiteSpace(run.RunId)) return 0;
        MclslWorldStateModifiers state = Current(year);
        if (state.Id == Normal) return 0;
        if (state.Id != SpiritTide) return 0;

        int newLawAge = NewLawAge(run, year);
        int periodStartAge = newLawAge / 180 * 180;
        int periodEndAge = periodStartAge + 180;
        return Math.Max(year + 1, Math.Max(0, run.NewLawStartYear) + periodEndAge);
    }

    private static MclslWorldStateModifiers Resolve(int year)
    {
        MclslWorldRunState run = MclslWorldRunRepository.Current;
        if (run == null || string.IsNullOrWhiteSpace(run.RunId) || !MclslWorldEpochSystem.IsNewLawActive(year))
            return NormalModifiers;

        if (TimelineResolved(run, "anchor_xuanhuang_terminal") || HasDiscovery(run, "truth_xuanhuang_terminal"))
        {
            return new MclslWorldStateModifiers(
                XuanhuangTerminal,
                "玄黄终局",
                "本世渐近终局，万般修行皆受天地修正。",
                0.58f,
                0.48f,
                0.55f,
                1.35f,
                1.6f,
                1.55f,
                -8);
        }

        if (TimelineResolved(run, "anchor_end_dharma") || HasDiscovery(run, "truth_end_dharma"))
        {
            return new MclslWorldStateModifiers(
                EndDharmaTerminal,
                "末法终局",
                "灵机渐衰，求道、取材与破境皆更艰难。",
                0.68f,
                0.55f,
                0.62f,
                0.78f,
                1.22f,
                1.35f,
                -5);
        }

        if (TimelineResolved(run, "anchor_white_mist") || HasDiscovery(run, "truth_white_mist"))
        {
            return new MclslWorldStateModifiers(
                WhiteMistEncroachment,
                "白雾吞界",
                "白雾侵世，遗迹与天地之变更易显露。",
                0.95f,
                0.92f,
                1.08f,
                1.45f,
                1.35f,
                1.18f,
                -1);
        }

        if (TimelineResolved(run, "anchor_black_tide") || HasDiscovery(run, "truth_black_tide"))
        {
            return new MclslWorldStateModifiers(
                BlackTideTribulation,
                "黑潮大劫",
                "黑潮暗涌，传承易断，遗迹与劫后灵机更易显世。",
                0.82f,
                0.72f,
                0.9f,
                1.18f,
                1.42f,
                1.28f,
                -3);
        }

        if (TimelineResolved(run, "anchor_lock_spirit_plan") || HasDiscovery(run, "truth_lock_spirit_plan"))
        {
            return new MclslWorldStateModifiers(
                LockSpiritAfterwave,
                "锁灵余波",
                "灵机受束，修士更依赖贡献与势力资源。",
                0.86f,
                0.78f,
                0.82f,
                0.92f,
                1.12f,
                1.22f,
                -2);
        }

        int newLawAge = NewLawAge(run, year);
        if (newLawAge >= 120 && ((newLawAge / 180) % 5 == 1))
        {
            return new MclslWorldStateModifiers(
                SpiritTide,
                "灵潮复起",
                "灵潮流经玄黄，修行与天地资源略有增益。",
                1.12f,
                1.18f,
                1.24f,
                1.16f,
                1.08f,
                0.95f,
                2);
        }

        return NormalModifiers;
    }

    private static int NewLawAge(MclslWorldRunState run, int year)
    {
        int origin = Math.Max(0, run?.NewLawStartYear ?? 0);
        return Math.Max(0, year - origin);
    }

    private static string CacheKey()
    {
        MclslWorldRunState run = MclslWorldRunRepository.Current;
        if (run == null) return string.Empty;
        return (run.RunId ?? string.Empty)
            + "|" + (run.CultivationEpoch ?? string.Empty)
            + "|" + run.NewLawEnabled
            + "|" + run.NewLawStartYear
            + "|" + (run.Discoveries?.Count ?? 0)
            + "|" + (run.TimelineAnchors?.Count ?? 0)
            + "|" + ResolvedAnchorCount(run);
    }

    private static int ResolvedAnchorCount(MclslWorldRunState run)
    {
        if (run?.TimelineAnchors == null) return 0;
        int count = 0;
        for (int i = 0; i < run.TimelineAnchors.Count; i++)
        {
            if (run.TimelineAnchors[i]?.Resolved == true) count++;
        }
        return count;
    }

    private static bool TimelineResolved(MclslWorldRunState run, string anchorId)
    {
        if (run?.TimelineAnchors == null || string.IsNullOrWhiteSpace(anchorId)) return false;
        for (int i = 0; i < run.TimelineAnchors.Count; i++)
        {
            MclslTimelineAnchorState anchor = run.TimelineAnchors[i];
            if (anchor != null && anchor.Resolved && string.Equals(anchor.AnchorId, anchorId, StringComparison.Ordinal))
                return true;
        }
        return false;
    }

    private static bool HasDiscovery(MclslWorldRunState run, string knowledgeId)
    {
        if (run?.Discoveries == null || string.IsNullOrWhiteSpace(knowledgeId)) return false;
        for (int i = 0; i < run.Discoveries.Count; i++)
        {
            MclslKnowledgeDiscoveryRecord discovery = run.Discoveries[i];
            if (discovery != null && string.Equals(discovery.KnowledgeId, knowledgeId, StringComparison.Ordinal))
                return true;
        }
        return false;
    }

    private static int ScaleInterval(int baseInterval, float birthRate)
    {
        if (baseInterval <= 1) return baseInterval;
        float safeRate = Math.Clamp(birthRate, 0.35f, 2.5f);
        return Math.Max(1, (int)MathF.Round(baseInterval / safeRate));
    }
}
