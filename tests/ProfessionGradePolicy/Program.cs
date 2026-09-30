using MySimulatedLongevityRoad.Systems;
using MySimulatedLongevityRoad.Data;
using System.Text.Json;

static void Equal(int expected, int actual, string caseName)
{
    if (actual != expected)
        throw new Exception($"{caseName}: expected {expected}, got {actual}");
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
    Equal(grade, MclslProfessionCraftingPolicy.MinimumRecipeGrade(grade), $"grade {grade} exact recipe floor");
    for (int recipe = 0; recipe <= 4; recipe++)
        if (MclslProfessionCraftingPolicy.CanAttemptRecipe(grade, recipe) != (grade == recipe))
            throw new Exception($"profession {grade} must craft only recipe {grade}, got {recipe}");
}
if (MclslProfessionCraftingPolicy.CanAttemptRecipe(-1, 0)
    || MclslProfessionCraftingPolicy.CanAttemptRecipe(5, 5))
    throw new Exception("invalid profession grades must not craft");

MclslMarketPurchaseBatch batch = new() { Year = 42, Profession = "alchemist", Grade = 2 };
for (int i = 0; i < 28; i++)
    batch.Needs.Add(new MclslMarketPurchaseNeed { ItemId = "item" + i, Kind = "Finished", Remaining = 1 });
if (!MclslMarketPurchasePolicy.ValidShape(batch, 100)) throw new Exception("valid 28-item batch rejected");
Equal(21, MclslMarketPurchaseSequence.AfterPurchase(20), "purchase count continues past old limit");
Equal(28, MclslMarketPurchaseSequence.CountInYear(42, 28, 42), "same-year purchase count is not clamped");
Equal(0, MclslMarketPurchaseSequence.CountInYear(42, 28, 43), "new-year purchase count resets");
if (MclslMarketPaymentPolicy.Select(5, 10, 1, 100, 100, 999999, 0)
    != MclslMarketPayment.SpiritStone)
    throw new Exception("seller contribution capacity must fall back to spirit stones");
if (MclslMarketPaymentPolicy.Select(5, 10, 1, 0, 0, 0, 0)
    != MclslMarketPayment.None)
    throw new Exception("unaffordable offer must be rejected");
if (MclslMarketPaymentPolicy.CanPay(MclslMarketPayment.Contribution, 600000, 2, int.MaxValue, 0))
    throw new Exception("seller capacity must reject oversized settlement");
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

Console.WriteLine("Profession grade and annual market purchase batch checks passed.");
