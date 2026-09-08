namespace MySimulatedLongevityRoad.Data;

internal sealed class MclslAptitudeGiftDefinition
{
    internal int Level { get; set; }
    internal string TraitId { get; set; } = string.Empty;
    internal string Name { get; set; } = string.Empty;
    internal int MinAptitude { get; set; }
    internal int MaxAptitude { get; set; }
    internal float CultivationEfficiency { get; set; }
    internal int AnnualTrueEssence { get; set; }
    internal int InsightBonus { get; set; }
    internal int BreakthroughBonus { get; set; }
    internal int LawHarmonyBonus { get; set; }
}

internal static class MclslAptitudeGiftCatalog
{
    private const int RollDenominator = 1000000;
    private const int QualificationDenominator = 10000;
    private const int QualificationChance = 5000;

    internal static readonly MclslAptitudeGiftDefinition[] Gifts =
    {
        G(6, "gifts_6", "天赐灵根", 95, 100, 1.80f, 180, 30, 20, 20),
        G(5, "gifts_5", "纯一灵根", 85, 94, 1.50f, 150, 20, 14, 14),
        G(4, "gifts_4", "上乘灵根", 70, 84, 1.30f, 120, 12, 9, 9),
        G(3, "gifts_3", "中正灵根", 55, 69, 1.15f, 90, 6, 4, 4),
        G(2, "gifts_2", "驳杂灵根", 35, 54, 1.00f, 60, 0, 0, -5),
        G(1, "gifts_1", "残缺灵根", 1, 34, 0.75f, 30, -10, -8, -12)
    };

    internal static int RollAptitude(string seed)
    {
        string safeSeed = seed ?? string.Empty;
        int roll = PositiveHash(safeSeed + "|aptitude_tier") % RollDenominator;

        // 从低到高：31 / 60 / 5.58 / 2.33 / 1.076 / 0.014。
        if (roll < 310000) return RollWithin(safeSeed, "broken", 1, 34);
        if (roll < 910000) return RollWithin(safeSeed, "mixed", 35, 54);
        if (roll < 965800) return RollWithin(safeSeed, "middle", 55, 69);
        if (roll < 989100) return RollWithin(safeSeed, "upper", 70, 84);
        if (roll < 999860) return RollWithin(safeSeed, "pure", 85, 94);
        return RollWithin(safeSeed, "heaven", 95, 100);
    }

    internal static bool RollQualification(string seed)
    {
        return PositiveHash((seed ?? string.Empty) + "|cultivation_qualification") % QualificationDenominator < QualificationChance;
    }

    internal static MclslAptitudeGiftDefinition ForAptitude(int aptitude)
    {
        int value = System.Math.Clamp(aptitude, 1, 100);
        foreach (MclslAptitudeGiftDefinition gift in Gifts)
            if (value >= gift.MinAptitude && value <= gift.MaxAptitude) return gift;
        return Gifts[^1];
    }

    internal static MclslAptitudeGiftDefinition ForTrait(string traitId)
    {
        foreach (MclslAptitudeGiftDefinition gift in Gifts)
            if (gift.TraitId == traitId) return gift;
        return null;
    }

    private static MclslAptitudeGiftDefinition G(
        int level,
        string traitId,
        string name,
        int min,
        int max,
        float efficiency,
        int annualTrueEssence,
        int insight,
        int breakthrough,
        int harmony) => new()
    {
        Level = level,
        TraitId = traitId,
        Name = name,
        MinAptitude = min,
        MaxAptitude = max,
        CultivationEfficiency = efficiency,
        AnnualTrueEssence = annualTrueEssence,
        InsightBonus = insight,
        BreakthroughBonus = breakthrough,
        LawHarmonyBonus = harmony
    };

    private static int RollWithin(string seed, string salt, int min, int max)
    {
        int span = System.Math.Max(1, max - min + 1);
        return min + PositiveHash(seed + "|aptitude_" + salt) % span;
    }

    private static int PositiveHash(string value)
    {
        unchecked
        {
            int hash = 31;
            foreach (char c in value ?? string.Empty) hash = hash * 43 + c;
            return hash & int.MaxValue;
        }
    }
}
