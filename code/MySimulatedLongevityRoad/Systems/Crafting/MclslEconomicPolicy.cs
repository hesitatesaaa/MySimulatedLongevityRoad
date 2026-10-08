using System;
using System.Collections.Generic;
using MySimulatedLongevityRoad.Data;

namespace MySimulatedLongevityRoad.Systems;

internal static class MclslEconomicPolicy
{
    internal const int StonePerContribution = 2;
    internal const int CommercialStockTarget = 2;
    internal const int MarketOrderYears = 10;
    private static readonly int[] ScrollPaper = { 1, 2, 3, 4, 5, 6, 8 };
    private static readonly string[] ScrollCore = { "A07", "A09", "A16", "A17", "A18", "A19", "A05" };

    internal static IReadOnlyDictionary<string, int> ScrollRecipe(int minRealm, string law)
    {
        int realm = Math.Clamp(minRealm, 0, 6);
        string extra = law switch
        {
            "金" => "M05", "木" => "A10", "水" => "A12", "火" => "A13", "土" => "F012",
            "风" => "F011", "雷" => "F014", "阴" => "F015", "阳" => "F017", "空间" => "F018",
            _ => string.Empty
        };
        Dictionary<string, int> recipe = new(StringComparer.Ordinal)
        {
            ["F01"] = ScrollPaper[realm], [ScrollCore[realm]] = 1
        };
        if (!string.IsNullOrEmpty(extra))
            recipe[extra] = recipe.TryGetValue(extra, out int count) ? count + 1 : 1;
        return recipe;
    }

    internal static int ReferenceStonePrice(int catalogPrice, int firstMaterialPrice, int secondMaterialPrice)
    {
        long materialCost = firstMaterialPrice > 0 && secondMaterialPrice > 0
            ? (long)firstMaterialPrice + secondMaterialPrice : 0;
        return ReferenceStonePrice(catalogPrice, materialCost);
    }

    internal static int ReferenceStonePrice(int catalogPrice, long materialCost)
    {
        long costFloor = (materialCost * 5 + 3) / 4;
        return (int)Math.Min(999999L, Math.Max(Math.Max(1, catalogPrice), costFloor));
    }

    // A craftable product is quoted from the ingredients it actually consumes.
    // Legacy catalog prices remain a fallback for non-craftable discoveries.
    internal static int CraftedStonePrice(long materialCost)
        => (int)Math.Min(999999L, Math.Max(1L, (Math.Max(0L, materialCost) * 5 + 3) / 4));

    internal static int CraftedStonePrice(string itemId, long materialCost)
        => Math.Max(CraftedStonePrice(materialCost), StrategicUsePriceFloor(itemId));

    internal static int StrategicUsePriceFloor(string itemId) => itemId switch
    {
        // A breakthrough item should require roughly two to five years of the
        // matching realm's ordinary disposable income. Material cost alone is
        // too small to express the scarcity and progression value.
        "D001" => 30,
        "D002" => 70,
        "D003" => 80,
        "D012" => 130,
        _ => 0
    };

    internal static int ContributionPrice(int stonePrice)
        => Math.Max(1, (stonePrice + StonePerContribution - 1) / StonePerContribution);

    internal static int AdjustMarketStonePrice(int referencePrice, int previousPrice, int fiveYearSupply,
        int fiveYearFundedDemand)
    {
        long reference = Math.Max(1, referencePrice);
        long previous = previousPrice > 0 ? previousPrice : reference;
        long target;
        if (fiveYearSupply <= 0 && fiveYearFundedDemand <= 0) target = reference;
        else
        {
            // Laplace smoothing keeps tiny markets stable. The final target is
            // bounded to 0.75..1.50 of production/reference cost.
            double ratio = (fiveYearFundedDemand + 1d) / (fiveYearSupply + 1d);
            double factor = Math.Clamp(0.75d + ratio * 0.25d, 0.75d, 1.50d);
            target = (long)Math.Round(reference * factor, MidpointRounding.AwayFromZero);
        }
        target = Math.Clamp(target, (reference * 3 + 3) / 4, (reference * 3 + 1) / 2);
        long annualMinimum = Math.Max(1, (previous * 9 + 9) / 10);
        long annualMaximum = Math.Max(1, previous * 11 / 10);
        return (int)Math.Min(999999L, Math.Clamp(target, annualMinimum, annualMaximum));
    }

    internal static bool ShouldProduce(int owned, int personalTarget, int marketStock)
        => owned < personalTarget || owned < CommercialStockTarget && marketStock < CommercialStockTarget;

    internal static int PracticeExperience(int craftExperience) => Math.Max(1, craftExperience / 2);

    internal static bool IsPriorityPurchase(string itemId)
        => itemId is "D001" or "D002" or "D003" or "D012" or "D011" or "D018"
            or "D006" or "D007" or "D010" or "D015" or "D019";

    internal static bool ListedThisYear(int listedYear, string listedItemIds, int year, string itemId)
        => listedYear == year && !string.IsNullOrEmpty(itemId)
            && !string.IsNullOrEmpty(listedItemIds)
            && listedItemIds.IndexOf("|" + itemId + "|", StringComparison.Ordinal) >= 0;

    internal static long SpendableForAutomaticPurchase(long funds, int realmIndex, bool contribution, bool priority)
    {
        if (priority || realmIndex < 2) return Math.Max(0, funds);
        int annualMaximum = contribution
            ? Math.Max(1, realmIndex - 1) + Math.Max(1, 1 + realmIndex / 2) - 1
                + (realmIndex >= 5 ? 4 : 0)
            : realmIndex * 4 + Math.Max(2, 6 + realmIndex * 2) - 1
                + (realmIndex >= 4 ? realmIndex * 6 : 0);
        return Math.Max(0, funds - annualMaximum * 2);
    }

    internal static bool OrderExpired(int currentYear, int listedYear)
        => listedYear > 0 && (long)currentYear - listedYear >= MarketOrderYears;

    internal static bool PublicOrdinaryStockExpired(int currentYear, int listedYear)
        => listedYear > 0 && (long)currentYear - listedYear >= MarketOrderYears * 2;

    internal static bool CanDiscardPublicStock(MclslItemDefinition item)
    {
        if (item == null) return false;
        if (item.Category is "Material" or "Plant" or "TalismanMaterial")
            return item.MaterialTier is MclslMaterialTier.Huang or MclslMaterialTier.Xuan;
        if (item.Category == "Talisman") return item.Grade <= 2;
        return item.Category == "Pill" && item.Grade <= 2
            && item.Id is not ("D001" or "D002" or "D003" or "D004" or "D011" or "D012" or "D018");
    }

    internal static int SaleServiceFee(int totalPrice, int previousRemainder, out int nextRemainder)
    {
        long hundredths = (long)Math.Max(0, totalPrice) * 2 + Math.Clamp(previousRemainder, 0, 99);
        nextRemainder = (int)(hundredths % 100);
        return (int)(hundredths / 100);
    }

    internal static int RecycleStonePrice(int referenceStonePrice)
        => Math.Max(1, (int)Math.Min(999999L, Math.Max(1L, referenceStonePrice) * 2 / 5));
}
