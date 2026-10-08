using MySimulatedLongevityRoad.Data;
using MySimulatedLongevityRoad.Systems;

const int seeds = 100;
const int years = 1000;
long totalTrades = 0, totalCrafts = 0, totalPractice = 0, totalRecycled = 0;
for (int seed = 1; seed <= seeds; seed++)
{
    Simulation simulation = new(seed, seed % 5);
    simulation.Run(years);
    totalTrades += simulation.Trades;
    totalCrafts += simulation.Crafts;
    totalPractice += simulation.Practice;
    totalRecycled += simulation.Recycled;
}
Console.WriteLine($"Economy simulation passed: {seeds} seeds x {years} years; "
    + $"trades={totalTrades}, crafts={totalCrafts}, practice={totalPractice}, recycled={totalRecycled}.");
IncomeBalanceAudit.Run(seeds, years);

internal static class IncomeBalanceAudit
{
    private static readonly string[] RealmNames = { "炼气", "筑基", "金丹", "元婴", "化神", "合道", "长生" };
    private static readonly string[] BreakthroughItems = { "D001", "D002", "D003", "D012" };

    internal static void Run(int seeds, int years)
    {
        double[] newLawNet = new double[RealmNames.Length];
        double[] ancientGross = new double[RealmNames.Length];
        double[] ancientNet = new double[RealmNames.Length];
        for (int realm = 0; realm < RealmNames.Length; realm++)
        {
            long newValue = 0, oldGross = 0, oldNet = 0;
            for (int seed = 1; seed <= seeds; seed++)
            {
                long contributionWallet = 0, stoneWallet = 0, ancientWallet = 0;
                int aptitude = 35 + seed % 51;
                for (int year = 1; year <= years; year++)
                {
                    int roll = Stable(seed, realm, year, 17);
                    MclslCurrencyPair income = MclslCultivatorIncomePolicy.NewLawIncome(realm, seed % 5 != 0, roll);
                    contributionWallet += income.Contribution;
                    stoneWallet += income.SpiritStones;
                    MclslCurrencyPair maintenance = MclslCultivatorIncomePolicy.NewLawMaintenance(
                        realm, roll, contributionWallet, stoneWallet);
                    contributionWallet -= maintenance.Contribution;
                    stoneWallet -= maintenance.SpiritStones;

                    int incomeRoll = Stable(seed, realm, year, 31) % 100;
                    int oldIncome = incomeRoll < MclslCultivatorIncomePolicy.AncientIncomeChance(realm, aptitude)
                        ? MclslCultivatorIncomePolicy.AncientIncome(realm, aptitude, Stable(seed, realm, year, 47)) : 0;
                    ancientWallet += oldIncome;
                    oldGross += oldIncome;
                    int cost = MclslCultivatorIncomePolicy.AncientSpendCost(realm);
                    int spendRoll = Stable(seed, realm, year, 59) % 100;
                    if (ancientWallet >= (long)cost * 3L
                        && spendRoll < MclslCultivatorIncomePolicy.AncientSpendChance(realm, aptitude))
                        ancientWallet -= cost;
                }
                newValue += contributionWallet * MclslEconomicPolicy.StonePerContribution + stoneWallet;
                oldNet += ancientWallet;
            }
            double denominator = seeds * (double)years;
            newLawNet[realm] = newValue / denominator;
            ancientGross[realm] = oldGross / denominator;
            ancientNet[realm] = oldNet / denominator;
            Check(newLawNet[realm] > 0 && ancientGross[realm] > 0, "positive real income");
            if (realm > 0) Check(newLawNet[realm] >= newLawNet[realm - 1], "monotonic new-law income");
            double eraRatio = ancientNet[realm] / newLawNet[realm];
            Check(eraRatio >= 0.55d && eraRatio <= 1.25d, "old/new disposable income ratio");
        }

        List<string> yearsToBuy = new();
        for (int realm = 0; realm < BreakthroughItems.Length; realm++)
        {
            MclslItemDefinition item = MclslItemCatalog.Get(BreakthroughItems[realm]);
            int price = ReferencePrice(item);
            double purchaseYears = price / Math.Max(0.01d, newLawNet[realm]);
            double ancientPurchaseYears = price / Math.Max(0.01d, ancientNet[realm]);
            Check(purchaseYears >= 2d && purchaseYears <= 5d, item.Name + " purchase years");
            Check(ancientPurchaseYears >= 2d && ancientPurchaseYears <= 5d, item.Name + " ancient purchase years");
            yearsToBuy.Add(item.Name + "=新" + purchaseYears.ToString("0.00")
                + "/旧" + ancientPurchaseYears.ToString("0.00") + "年");
        }
        Console.WriteLine("Real income audit (stone value/year): new-law="
            + string.Join(",", newLawNet.Select((x, i) => RealmNames[i] + ":" + x.ToString("0.00")))
            + "; ancient gross=" + string.Join(",", ancientGross.Select((x, i) => RealmNames[i] + ":" + x.ToString("0.00")))
            + "; ancient retained=" + string.Join(",", ancientNet.Select((x, i) => RealmNames[i] + ":" + x.ToString("0.00")))
            + "; breakthrough=" + string.Join(",", yearsToBuy) + ".");
    }

    private static int ReferencePrice(MclslItemDefinition item)
    {
        int first = IngredientPrice(item.IngredientA);
        int second = IngredientPrice(item.IngredientB);
        if (first > 0 && second > 0 && item.Category is "Pill" or "Talisman" or "Artifact")
            return MclslEconomicPolicy.CraftedStonePrice(item.Id, (long)first + second);
        return MclslEconomicPolicy.ReferenceStonePrice(item.Price, first, second);
    }

    private static int IngredientPrice(string id)
    {
        if (string.IsNullOrWhiteSpace(id)) return 0;
        MclslItemDefinition item = MclslItemCatalog.Get(id);
        return item == null ? id.StartsWith("R", StringComparison.Ordinal) ? 2 : 0 : Math.Max(1, item.Price);
    }

    private static int Stable(int seed, int realm, int year, int salt)
    {
        unchecked { return (seed * 73856093 ^ realm * 19349663 ^ year * 83492791 ^ salt * 265443576) & int.MaxValue; }
    }

    private static void Check(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
}

internal sealed class Simulation
{
    private const int ActorCount = 48;
    private const int ProductKinds = 6;
    private readonly int _scenario;
    private readonly MclslEconomyState _economy = new();
    private readonly ActorState[] _actors = new ActorState[ActorCount];
    private readonly List<Order> _orders = new();
    private uint _random;
    private long _materialCreated;
    private long _materialConsumed;
    private long _productsCreated;
    private long _productsConsumed;
    private long _productsDestroyed;
    internal long Trades { get; private set; }
    internal long Crafts { get; private set; }
    internal long Practice { get; private set; }
    internal long Recycled { get; private set; }

    internal Simulation(int seed, int scenario)
    {
        _random = (uint)seed;
        _scenario = scenario;
        for (int i = 0; i < _actors.Length; i++)
            _actors[i] = new ActorState { Account = "actor/" + i, Profession = i % ProductKinds };
    }

    internal void Run(int years)
    {
        for (int year = 1; year <= years; year++) Step(year);
        long wallets = _economy.Wallets.Values.Sum(x => x.SpiritStones);
        long expected = _economy.Lifetime.SpiritStones.Issued + _economy.Lifetime.SpiritStones.Recycled
            - _economy.Lifetime.SpiritStones.Consumed - _economy.Lifetime.SpiritStones.Destroyed;
        Check(wallets == expected, "currency conservation");
        Check(_materialCreated == _materialConsumed + _actors.Sum(x => x.Materials), "material conservation");
        Check(_productsCreated == _productsConsumed + _productsDestroyed + _orders.Sum(x => (long)x.Count),
            "product conservation");
        Check(_orders.Sum(x => x.Count) <= ActorCount * ProductKinds * 4, "bounded inventory");
        Check(_economy.Years.Count <= 20 && _economy.IsValid(), "bounded valid ledger");
    }

    private void Step(int year)
    {
        bool newLaw = year >= 500;
        bool war = _scenario == 1 && year % 23 < 7;
        bool poor = _scenario == 2;
        bool professionImbalance = _scenario == 3;
        bool transitionShock = _scenario == 4 && year is >= 480 and <= 540;
        long issuedThisYear = 0;
        foreach (ActorState actor in _actors)
        {
            long income = 8 + Next(10) + (newLaw ? 4 : 0);
            if (war) income = Math.Max(2, income - 4);
            Commit(year, new[] { new MclslEconomicOperation(MclslEconomicKind.Issue,
                MclslCurrency.SpiritStone, income, toAccount: actor.Account) });
            issuedThisYear += income;
            int gathered = poor ? Next(2) : 1 + Next(3);
            if (war) gathered = Math.Max(0, gathered - 1);
            actor.Materials += gathered;
            _materialCreated += gathered;
        }

        foreach (ActorState actor in _actors)
        {
            int kind = professionImbalance ? 0 : actor.Profession;
            int stock = _orders.Where(x => x.Kind == kind).Sum(x => x.Count);
            if (actor.Materials >= 2 && stock < MclslEconomicPolicy.CommercialStockTarget)
            {
                actor.Materials -= 2;
                _materialConsumed += 2;
                _productsCreated++;
                Crafts++;
                _orders.Add(new Order { Kind = kind, Count = 1, ListedYear = year,
                    Seller = actor.Account, UnitPrice = MarketPrice(kind, year) });
            }
            else if (actor.Materials >= 2 && Next(100) < 35)
            {
                actor.Materials -= 2;
                _materialConsumed += 2;
                Practice++;
            }
        }

        foreach (ActorState buyer in _actors)
        {
            int demandAttempts = war ? 2 : 1;
            if (transitionShock) demandAttempts++;
            for (int attempt = 0; attempt < demandAttempts; attempt++)
            {
                int kind = Next(ProductKinds);
                if (_scenario == 2 && kind == ProductKinds - 1) continue;
                Order order = _orders.Where(x => x.Kind == kind && x.Count > 0 && x.Seller != buyer.Account)
                    .OrderBy(x => x.UnitPrice).ThenBy(x => x.ListedYear).FirstOrDefault();
                if (order == null) continue;
                long funds = _economy.Balance(buyer.Account, MclslCurrency.SpiritStone);
                if (funds < order.UnitPrice) continue;
                long fee = Math.Max(0, order.UnitPrice * 2 / 100);
                Commit(year, new[]
                {
                    new MclslEconomicOperation(MclslEconomicKind.Transfer, MclslCurrency.SpiritStone,
                        order.UnitPrice - fee, buyer.Account, order.Seller),
                    new MclslEconomicOperation(MclslEconomicKind.Consume, MclslCurrency.SpiritStone,
                        fee, buyer.Account)
                });
                order.Count--;
                Trades++;
                _productsConsumed++;
            }
        }
        _orders.RemoveAll(x => x.Count <= 0);

        long recycleBudget = issuedThisYear / 10;
        foreach (Order order in _orders.Where(x => year - x.ListedYear >= MclslEconomicPolicy.MarketOrderYears)
                     .OrderBy(x => x.ListedYear).ToArray())
        {
            long recyclePrice = MclslEconomicPolicy.RecycleStonePrice(order.UnitPrice);
            if (recyclePrice <= recycleBudget)
            {
                Commit(year, new[] { new MclslEconomicOperation(MclslEconomicKind.Recycle,
                    MclslCurrency.SpiritStone, recyclePrice, toAccount: order.Seller) });
                recycleBudget -= recyclePrice;
                Recycled++;
                order.Seller = "public";
                order.ListedYear = year;
            }
        }
        foreach (Order order in _orders.Where(x => x.Seller == "public"
                     && year - x.ListedYear >= MclslEconomicPolicy.MarketOrderYears * 2).ToArray())
        {
            _productsDestroyed += order.Count;
            _orders.Remove(order);
        }
        Check(_orders.All(x => x.Count > 0 && x.UnitPrice > 0), "valid orders");
    }

    private int MarketPrice(int kind, int year)
    {
        int reference = 10 + kind * 5;
        int supply = _orders.Where(x => x.Kind == kind).Sum(x => x.Count);
        int demand = _scenario == 1 ? 4 : _scenario == 3 && kind > 0 ? 6 : 2;
        return MclslEconomicPolicy.AdjustMarketStonePrice(reference, reference, supply, demand);
    }

    private void Commit(int year, IReadOnlyList<MclslEconomicOperation> operations)
    {
        MclslEconomicResult result = MclslEconomicTransaction.Commit(_economy, year, operations);
        Check(result == MclslEconomicResult.Applied, "transaction " + result);
    }

    private int Next(int maximum)
    {
        _random = unchecked(_random * 1664525U + 1013904223U);
        return (int)(_random % (uint)Math.Max(1, maximum));
    }

    private static void Check(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }

    private sealed class ActorState
    {
        internal string Account = string.Empty;
        internal int Profession;
        internal int Materials;
    }

    private sealed class Order
    {
        internal int Kind;
        internal int Count;
        internal int ListedYear;
        internal string Seller = string.Empty;
        internal int UnitPrice;
    }
}
