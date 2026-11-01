using System;

namespace MySimulatedLongevityRoad.Data;

internal readonly struct MclslCurrencyPair
{
    internal readonly int Contribution;
    internal readonly int SpiritStones;

    internal MclslCurrencyPair(int contribution, int spiritStones)
    {
        Contribution = Math.Max(0, contribution);
        SpiritStones = Math.Max(0, spiritStones);
    }

    internal long StoneValue => checked((long)Contribution * 2L + SpiritStones);
}

internal static class MclslCultivatorIncomePolicy
{
    internal static MclslCurrencyPair NewLawIncome(int realmIndex, bool hasSettlement, int seed)
    {
        int realm = Math.Clamp(realmIndex, 0, 6);
        int contributionBase = realm switch
        {
            0 => 2, 1 => 4, 2 => 8, 3 => 14, 4 => 24, 5 => 38, _ => 52
        };
        int contribution = contributionBase + (hasSettlement ? 1 + realm : 0)
            + Positive(seed) % Math.Max(2, 3 + realm * 2);
        int stones = (realm + 1) * (realm + 1)
            + Positive(seed) % Math.Max(2, 4 + realm * 3);
        if (realm >= 4) stones += 12 + realm * 5;
        return new MclslCurrencyPair(contribution, stones);
    }

    internal static MclslCurrencyPair NewLawMaintenance(int realmIndex, int seed,
        long availableContribution, long availableStones)
    {
        int realm = Math.Clamp(realmIndex, 0, 6);
        if (realm < 2) return new MclslCurrencyPair(0, 0);
        int contribution = Math.Max(1, realm - 1) + Positive(seed) % Math.Max(1, 1 + realm / 2);
        int stones = realm * 4 + Positive(seed) % Math.Max(2, 6 + realm * 2);
        if (realm >= 4) stones += realm * 6;
        if (realm >= 5) contribution += 4;
        return new MclslCurrencyPair(
            (int)Math.Min(contribution, Math.Max(0L, availableContribution / 3L)),
            (int)Math.Min(stones, Math.Max(0L, availableStones / 3L)));
    }

    internal static int AncientIncomeChance(int realmIndex, int aptitude)
        => Math.Clamp(40 + Math.Clamp(realmIndex, 0, 6) * 6 + Math.Clamp(aptitude, 1, 100) / 10, 40, 85);

    internal static int AncientIncome(int realmIndex, int aptitude, int amountRoll)
    {
        int realm = Math.Clamp(realmIndex, 0, 6);
        return Math.Max(1, 18 + realm * 5 + realm * realm * 3 + Math.Clamp(aptitude, 1, 100) / 20
            + Positive(amountRoll) % Math.Max(2, 4 + realm * 2));
    }

    internal static int AncientSpendChance(int realmIndex, int aptitude)
        => Math.Clamp(30 + Math.Clamp(aptitude, 1, 100) / 4 + Math.Clamp(realmIndex, 0, 6) * 3, 30, 72);

    internal static int AncientSpendCost(int realmIndex) => 2 + Math.Clamp(realmIndex, 0, 6) * 2;

    private static int Positive(int value) => value == int.MinValue ? int.MaxValue : Math.Abs(value);
}
