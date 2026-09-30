using System;
using System.Collections.Generic;

namespace MySimulatedLongevityRoad.Data;

internal static class MclslMarketPurchaseSequence
{
    internal static int CountInYear(int purchaseYear, int count, int year) =>
        year > 0 && purchaseYear == year ? Math.Max(0, count) : 0;
    // Historical telemetry only; saturation never blocks a transaction.
    internal static int AfterPurchase(int count) => count >= int.MaxValue ? int.MaxValue : Math.Max(0, count) + 1;
}

internal enum MclslMarketPayment : byte { None, Contribution, SpiritStone }

internal static class MclslMarketPaymentPolicy
{
    internal static bool CanPay(MclslMarketPayment currency, int unitPrice, int count,
        int buyerFunds, int sellerFunds)
    {
        if (currency == MclslMarketPayment.None || unitPrice <= 0 || count <= 0) return false;
        long total = (long)unitPrice * count;
        return total <= buyerFunds && sellerFunds <= 999999L - total;
    }

    internal static MclslMarketPayment Select(int contributionUnit, int stoneUnit, int count,
        int buyerContribution, int buyerStones, int sellerContribution, int sellerStones)
    {
        bool contribution = CanPay(MclslMarketPayment.Contribution, contributionUnit, count,
            buyerContribution, sellerContribution);
        bool stones = CanPay(MclslMarketPayment.SpiritStone, stoneUnit, count,
            buyerStones, sellerStones);
        if (!contribution && !stones) return MclslMarketPayment.None;
        if (contribution && (!stones || (double)contributionUnit * count / Math.Max(1, buyerContribution)
            <= (double)stoneUnit * count / Math.Max(1, buyerStones))) return MclslMarketPayment.Contribution;
        return MclslMarketPayment.SpiritStone;
    }
}

internal sealed class MclslMarketPurchaseNeed
{
    public string ItemId { get; set; } = string.Empty;
    public string Kind { get; set; } = string.Empty;
    public int Remaining { get; set; }
}

internal sealed class MclslMarketPurchaseBatch
{
    public int Year { get; set; }
    public string Profession { get; set; } = string.Empty;
    public int Grade { get; set; } = -1;
    public int Cursor { get; set; }
    public List<MclslMarketPurchaseNeed> Needs { get; set; } = new();
}

internal static class MclslMarketPurchasePolicy
{
    internal static bool IsComplete(MclslMarketPurchaseBatch batch) => batch.Cursor >= batch.Needs.Count;

    internal static void Advance(MclslMarketPurchaseBatch batch, bool purchased)
    {
        MclslMarketPurchaseNeed need = batch.Needs[batch.Cursor];
        if (purchased && need.Remaining > 0) need.Remaining--;
        if (!purchased || need.Remaining <= 0) batch.Cursor++;
    }

    internal static bool ValidShape(MclslMarketPurchaseBatch batch, int maxNeeds) =>
        batch != null && batch.Year > 0 && batch.Needs != null
        && batch.Needs.Count <= maxNeeds && batch.Cursor >= 0 && batch.Cursor <= batch.Needs.Count;
}
