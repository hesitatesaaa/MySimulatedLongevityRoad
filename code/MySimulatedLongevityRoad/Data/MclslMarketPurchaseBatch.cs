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
        long buyerFunds, long sellerFunds)
    {
        if (currency is not (MclslMarketPayment.Contribution or MclslMarketPayment.SpiritStone) || unitPrice <= 0 || count <= 0
            || buyerFunds < 0 || sellerFunds < 0) return false;
        long total = (long)unitPrice * count;
        long walletLimit = long.MaxValue;
        return total <= buyerFunds && sellerFunds <= walletLimit - total;
    }

    internal static MclslMarketPayment Select(int contributionUnit, int stoneUnit, int count,
        long buyerContribution, long buyerStones, long sellerContribution, long sellerStones)
    {
        bool contribution = CanPay(MclslMarketPayment.Contribution, contributionUnit, count,
            buyerContribution, sellerContribution);
        bool stones = CanPay(MclslMarketPayment.SpiritStone, stoneUnit, count,
            buyerStones, sellerStones);
        if (!contribution && !stones) return MclslMarketPayment.None;
        // First compare the actual price in stones. Only equal-cost offers use
        // the buyer's relative currency surplus as a tie breaker.
        if (contribution && (!stones || (long)contributionUnit * 2 < stoneUnit
            || (long)contributionUnit * 2 == stoneUnit
                && buyerContribution >= buyerStones / 2 + buyerStones % 2)) return MclslMarketPayment.Contribution;
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
