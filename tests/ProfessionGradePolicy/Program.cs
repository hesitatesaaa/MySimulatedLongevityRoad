using MySimulatedLongevityRoad.Systems;
using MySimulatedLongevityRoad.Data;
using System.Text.Json;

static void Equal(long expected, long actual, string caseName)
{
    if (actual != expected)
        throw new Exception($"{caseName}: expected {expected}, got {actual}");
}

static int IngredientPrice(string id)
{
    if (string.IsNullOrWhiteSpace(id)) return 0;
    return MclslItemCatalog.Get(id)?.Price ?? (id.StartsWith("R", StringComparison.Ordinal) ? 2 : 0);
}

static int AuditedStonePrice(MclslItemDefinition item, int first, int second)
{
    if (item.Category == "SpellScroll") return 0; // Runtime recipe has more than two ingredients.
    if (first > 0 && second > 0 && item.Category is "Pill" or "Talisman" or "Artifact")
        return MclslEconomicPolicy.CraftedStonePrice((long)first + second);
    return MclslEconomicPolicy.ReferenceStonePrice(item.Price, first, second);
}

if (args.Contains("--price-table"))
{
    Console.WriteLine("| ID | 商品 | 类别 | 原目录价 | 材料成本 | 参考灵石价 | 贡献度价 |");
    Console.WriteLine("|---|---|---|---:|---:|---:|---:|");
    foreach (MclslItemDefinition item in MclslItemCatalog.All.OrderBy(x => x.Id, StringComparer.Ordinal))
    {
        int first = IngredientPrice(item.IngredientA), second = IngredientPrice(item.IngredientB);
        int reference = AuditedStonePrice(item, first, second);
        Console.WriteLine($"| {item.Id} | {item.Name} | {item.Category} | {item.Price} | {first + second} | "
            + $"{(reference > 0 ? reference.ToString() : "按完整制卷配方") } | "
            + $"{(reference > 0 ? MclslEconomicPolicy.ContributionPrice(reference).ToString() : "按完整制卷配方")} |");
    }
    return;
}

// Realm indices follow MclslRealmIds.Ordered: Lian Qi through Chang Sheng.
int[] expectedCaps = { 0, 1, 2, 3, 4, 4, 4 };
string[] realms = { "练气", "筑基", "金丹", "元婴", "化神", "合道", "长生" };
for (int realm = 0; realm < expectedCaps.Length; realm++)
{
    int cap = expectedCaps[realm];
    Equal(cap, MclslProfessionCraftingPolicy.MaximumGrade(realm), realms[realm] + " cap");
    Equal(cap, MclslProfessionCraftingPolicy.ClampGrade(4, realm), realms[realm] + " old save repair");
    Equal(cap, MclslProfessionCraftingPolicy.PromotedGrade(0, 1000, realm), realms[realm] + " high experience");
    Equal(cap, MclslProfessionCraftingPolicy.PromotedGrade(cap, 1000, realm), realms[realm] + " cannot exceed cap");
}

Equal(0, MclslProfessionCraftingPolicy.MaximumGrade(-1), "unknown realm cap");
Equal(0, MclslProfessionCraftingPolicy.ClampGrade(-3, 4), "negative grade repair");
Equal(4, MclslProfessionCraftingPolicy.ClampGrade(99, 6), "oversized grade repair");

int[] thresholds = { 10, 30, 80, 160 };
for (int grade = 0; grade < thresholds.Length; grade++)
{
    int realm = grade + 1;
    Equal(grade, MclslProfessionCraftingPolicy.PromotedGrade(grade, thresholds[grade] - 1, realm),
        $"grade {grade} below experience threshold");
    Equal(grade + 1, MclslProfessionCraftingPolicy.PromotedGrade(grade, thresholds[grade], realm),
        $"grade {grade} reaches experience threshold");
    Equal(grade, MclslProfessionCraftingPolicy.PromotedGrade(grade, 1000, grade),
        $"grade {grade} blocked before realm breakthrough");
    Equal(grade + 1, MclslProfessionCraftingPolicy.PromotedGrade(grade, 1000, realm),
        $"grade {grade} unlocked after realm breakthrough");
}

for (int grade = 0; grade <= 4; grade++)
{
    int minimum = grade == 0 ? 0 : Math.Max(1, grade - 1);
    Equal(minimum, MclslProfessionCraftingPolicy.MinimumRecipeGrade(grade), $"grade {grade} recipe floor");
    int[] experienceByRecipe = { 1, 3, 6, 10, 15 };
    for (int recipe = 0; recipe <= 4; recipe++)
    {
        bool allowed = recipe >= minimum && recipe <= grade;
        if (MclslProfessionCraftingPolicy.CanAttemptRecipe(grade, recipe) != allowed)
            throw new Exception($"profession {grade} recipe {recipe} eligibility mismatch");
        Equal(allowed ? experienceByRecipe[recipe] : 0,
            MclslProfessionCraftingPolicy.ExperienceGain(grade, recipe),
            $"profession {grade} recipe {recipe} experience");
    }
}
if (MclslProfessionCraftingPolicy.CanAttemptRecipe(-1, 0)
    || MclslProfessionCraftingPolicy.CanAttemptRecipe(5, 5))
    throw new Exception("invalid profession grades must not craft");
Equal(0, MclslProfessionCraftingPolicy.ExperienceGain(-1, 0), "invalid profession earns no experience");

MclslMarketPurchaseBatch batch = new() { Year = 42, Profession = "alchemist", Grade = 2 };
for (int i = 0; i < 28; i++)
    batch.Needs.Add(new MclslMarketPurchaseNeed { ItemId = "item" + i, Kind = "Finished", Remaining = 1 });
if (!MclslMarketPurchasePolicy.ValidShape(batch, 100)) throw new Exception("valid 28-item batch rejected");
Equal(21, MclslMarketPurchaseSequence.AfterPurchase(20), "purchase count continues past old limit");
Equal(28, MclslMarketPurchaseSequence.CountInYear(42, 28, 42), "same-year purchase count is not clamped");
Equal(0, MclslMarketPurchaseSequence.CountInYear(42, 28, 43), "new-year purchase count resets");
if (MclslMarketPaymentPolicy.Select(5, 10, 1, 100, 100, long.MaxValue, 0)
    != MclslMarketPayment.SpiritStone)
    throw new Exception("seller contribution capacity must fall back to spirit stones");
if (MclslMarketPaymentPolicy.Select(5, 10, 1, 0, 0, 0, 0)
    != MclslMarketPayment.None)
    throw new Exception("unaffordable offer must be rejected");
if (MclslMarketPaymentPolicy.CanPay(MclslMarketPayment.Contribution, 600000, 2, long.MaxValue, long.MaxValue - 1))
    throw new Exception("seller capacity must reject overflowing settlement");
if (!MclslMarketPaymentPolicy.CanPay(MclslMarketPayment.SpiritStone, 10, 1, 100,
        1000000) || !MclslMarketPaymentPolicy.CanPay(MclslMarketPayment.Contribution, 10, 1, 100, 1000000))
    throw new Exception("both currencies support 64-bit balances without legacy wallet caps");
if (MclslMarketPaymentPolicy.CanPay(MclslMarketPayment.SpiritStone, 1, 1, 10, -1))
    throw new Exception("negative wallet data cannot enter a settlement");
if (MclslMarketPaymentPolicy.CanPay((MclslMarketPayment)255, 1, 1, 10, 0))
    throw new Exception("undefined currencies cannot enter settlement");
if (MclslMarketPaymentPolicy.Select(5, 10, 1, long.MaxValue, long.MaxValue, 0, 0)
    != MclslMarketPayment.Contribution)
    throw new Exception("currency surplus comparison must not overflow at Int64 maximum");
for (int i = 0; i < 21; i++) MclslMarketPurchasePolicy.Advance(batch, true);
Equal(21, batch.Cursor, "purchase batch exceeds old 20-order limit");
string saved = JsonSerializer.Serialize(batch);
MclslMarketPurchaseBatch restored = JsonSerializer.Deserialize<MclslMarketPurchaseBatch>(saved)!;
while (!MclslMarketPurchasePolicy.IsComplete(restored)) MclslMarketPurchasePolicy.Advance(restored, true);
Equal(28, restored.Cursor, "restored batch completes without repeating purchases");
MclslMarketPurchaseBatch pair = new() { Year = 43,
    Needs = new() { new MclslMarketPurchaseNeed { ItemId = "material", Kind = "Craft", Remaining = 2 } } };
MclslMarketPurchasePolicy.Advance(pair, true);
Equal(1, pair.Needs[0].Remaining, "same-material recipe needs second unit");
Equal(0, pair.Cursor, "same-material recipe stays on current need");
MclslMarketPurchasePolicy.Advance(pair, false);
Equal(1, pair.Cursor, "unavailable material advances without a loop");

Equal(38, MclslEconomicPolicy.ReferenceStonePrice(20, 6, 24),
    "crafted item cannot be priced below materials and work");
Equal(38, MclslEconomicPolicy.CraftedStonePrice(30), "craft quote includes twenty-five percent work");
Equal(23, MclslEconomicPolicy.CraftedStonePrice(18), "craft quote does not inherit inflated catalog prices");
var finalScrollRecipe = MclslEconomicPolicy.ScrollRecipe(6, "木");
Equal(8, finalScrollRecipe["F01"], "high realm scroll consumes eight sheets of paper");
Equal(1, finalScrollRecipe["A05"], "high realm scroll consumes its core");
Equal(1, finalScrollRecipe["A10"], "wood scroll consumes its third ingredient");
long finalScrollCost = finalScrollRecipe.Sum(x => (long)IngredientPrice(x.Key) * x.Value);
Equal(98, MclslEconomicPolicy.CraftedStonePrice(finalScrollCost),
    "full scroll recipe is quoted from all ingredients rather than two catalog fields");
Equal(19, MclslEconomicPolicy.ContributionPrice(38), "single dual-currency quote");
Equal(3, MclslEconomicPolicy.ContributionPrice(5), "odd stone quotes round up");
Equal(40, MclslEconomicPolicy.RecycleStonePrice(100), "system recycle pays forty percent of reference");
Equal(90, MclslEconomicPolicy.AdjustMarketStonePrice(100, 100, 100, 0),
    "oversupply price falls by at most ten percent per year");
Equal(110, MclslEconomicPolicy.AdjustMarketStonePrice(100, 100, 0, 100),
    "funded demand raises price by at most ten percent per year");
Equal(100, MclslEconomicPolicy.AdjustMarketStonePrice(100, 100, 0, 0),
    "market without samples returns to reference price");
int boundedHigh = 100;
for (int i = 0; i < 20; i++) boundedHigh = MclslEconomicPolicy.AdjustMarketStonePrice(100, boundedHigh, 0, 1000);
Equal(150, boundedHigh, "demand multiplier is capped at one and a half");
int boundedLow = 100;
for (int i = 0; i < 20; i++) boundedLow = MclslEconomicPolicy.AdjustMarketStonePrice(100, boundedLow, 1000, 0);
Equal(75, boundedLow, "supply multiplier is capped at three quarters");
Equal(1, MclslEconomicPolicy.PracticeExperience(3), "practice preserves progression");
Equal(0, MclslEconomicPolicy.SpendableForAutomaticPurchase(25, 2, false, false),
    "ordinary purchases keep two years of maintenance");
Equal(25, MclslEconomicPolicy.SpendableForAutomaticPurchase(25, 2, false, true),
    "life-saving purchases may use maintenance reserve");
if (!MclslEconomicPolicy.IsPriorityPurchase("D001")
    || MclslEconomicPolicy.IsPriorityPurchase("S001_SCROLL"))
    throw new Exception("priority purchases must distinguish breakthrough from scroll stock");
if (!MclslEconomicPolicy.ListedThisYear(12, "|A01|F01|", 12, "F01")
    || MclslEconomicPolicy.ListedThisYear(12, "|A01|F01|", 13, "F01")
    || MclslEconomicPolicy.ListedThisYear(12, "|A01|F01|", 12, "A0"))
    throw new Exception("same-year sale must not create a stale automatic repurchase need");
if (MclslEconomicPolicy.OrderExpired(10, 1) || !MclslEconomicPolicy.OrderExpired(11, 1)
    || MclslEconomicPolicy.PublicOrdinaryStockExpired(20, 1)
    || !MclslEconomicPolicy.PublicOrdinaryStockExpired(21, 1))
    throw new Exception("seller orders expire after ten years; public stock after twice that duration");
if (MclslEconomicPolicy.CanDiscardPublicStock(MclslItemCatalog.Get("D012"))
    || MclslEconomicPolicy.CanDiscardPublicStock(MclslItemCatalog.Get("B001"))
    || MclslEconomicPolicy.CanDiscardPublicStock(MclslItemCatalog.Get("S025_SCROLL"))
    || MclslEconomicPolicy.CanDiscardPublicStock(MclslItemCatalog.Get("M19"))
    || !MclslEconomicPolicy.CanDiscardPublicStock(MclslItemCatalog.Get("F001")))
    throw new Exception("important stock must not be destroyed by public inventory cleanup");
int feeCarry = 0, accumulatedFee = 0;
for (int sale = 0; sale < 50; sale++)
    accumulatedFee += MclslEconomicPolicy.SaleServiceFee(1, feeCarry, out feeCarry);
Equal(1, accumulatedFee, "split sales cannot evade two percent fee");
Equal(0, feeCarry, "fee fraction clears after fifty one-unit sales");
Equal(2, MclslEconomicPolicy.SaleServiceFee(100, 0, out _), "seller fee is two percent");
foreach (MclslItemDefinition item in MclslItemCatalog.All)
{
    int first = IngredientPrice(item.IngredientA), second = IngredientPrice(item.IngredientB);
    int reference = AuditedStonePrice(item, first, second);
    if (item.Category == "SpellScroll") continue;
    bool crafted = first > 0 && second > 0 && item.Category is "Pill" or "Talisman" or "Artifact";
    if (crafted && reference != MclslEconomicPolicy.CraftedStonePrice((long)first + second)
        || !crafted && reference < item.Price)
        throw new Exception($"{item.Id} quote disagrees with its recipe or non-craftable catalog price");
}
if (MclslMarketPaymentPolicy.Select(10, 15, 1, 100, 100, 0, 0)
    != MclslMarketPayment.SpiritStone)
    throw new Exception("automatic purchase must prefer cheaper actual stone cost");
int stock = 0, practiceYears = 0;
for (int year = 1; year <= 100; year++)
{
    if (MclslEconomicPolicy.ShouldProduce(stock, 2, 2)) stock++;
    else practiceYears++;
}
Equal(2, stock, "100-year production stays within personal reserve");
Equal(98, practiceYears, "idle production becomes bounded practice");

Console.WriteLine("Profession grade and annual market purchase batch checks passed.");
