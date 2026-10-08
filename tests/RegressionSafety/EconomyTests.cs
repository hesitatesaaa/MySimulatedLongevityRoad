using MySimulatedLongevityRoad.Data;
using MySimulatedLongevityRoad.Systems;
using Newtonsoft.Json;

internal static class EconomyTests
{
    internal static void Run()
    {
        static void Check(bool condition, string message)
        {
            if (!condition) throw new Exception(message);
        }
        const MclslCurrency stone = MclslCurrency.SpiritStone;
        const MclslCurrency contribution = MclslCurrency.Contribution;
        MclslEconomyState state = new();
        Check(MclslEconomicTransaction.Commit(state, 1, new[] {
            new MclslEconomicOperation(MclslEconomicKind.Issue, stone, 5_000_000_000L, toAccount: "buyer"),
            new MclslEconomicOperation(MclslEconomicKind.Issue, contribution, 10, toAccount: "buyer")
        }, "initial/buyer", 1) == MclslEconomicResult.Applied, "64-bit issuance");
        string before = JsonConvert.SerializeObject(state);
        MclslEconomicOperation[] sale = {
            new(MclslEconomicKind.Transfer, stone, 98, "buyer", "seller"),
            new(MclslEconomicKind.Consume, stone, 2, "buyer")
        };
        Check(MclslEconomicTransaction.Commit(state, 1, new[] {
            sale[0], new MclslEconomicOperation(MclslEconomicKind.Consume, contribution, 11, "buyer")
        }, "failed", 1) == MclslEconomicResult.InsufficientFunds
            && before == JsonConvert.SerializeObject(state), "failed second debit must roll back all wallets and receipts");
        Check(MclslEconomicTransaction.Commit(state, 1, sale, "sale/1", 1) == MclslEconomicResult.Applied
            && state.Balance("buyer", stone) == 4_999_999_900L
            && state.Balance("seller", stone) == 98, "atomic sale and fee");
        before = JsonConvert.SerializeObject(state);
        Check(MclslEconomicTransaction.Commit(state, 1, sale, "sale/1", 1) == MclslEconomicResult.AlreadyApplied
            && before == JsonConvert.SerializeObject(state), "duplicate transaction receipt");
        state = JsonConvert.DeserializeObject<MclslEconomyState>(before)!;
        Check(MclslEconomicTransaction.Commit(state, 1, sale, "sale/1", 1) == MclslEconomicResult.AlreadyApplied,
            "receipt survives save and load");
        Check(MclslEconomicTransaction.Commit(state, 1, new[] {
            new MclslEconomicOperation(MclslEconomicKind.Issue, stone, long.MaxValue, toAccount: "buyer")
        }) == MclslEconomicResult.Overflow && before == JsonConvert.SerializeObject(state), "wallet overflow rolls back");
        MclslEconomyState fullLedger = new();
        fullLedger.Years[1] = new() { SpiritStones = new() { Issued = long.MaxValue } };
        before = JsonConvert.SerializeObject(fullLedger);
        Check(MclslEconomicTransaction.Commit(fullLedger, 1, new[] {
            new MclslEconomicOperation(MclslEconomicKind.Issue, stone, 1, toAccount: "buyer")
        }) == MclslEconomicResult.Overflow && before == JsonConvert.SerializeObject(fullLedger),
            "ledger overflow cannot partially credit wallet");
        fullLedger.Years[1].SpiritStones.Issued = -1;
        Check(!fullLedger.IsValid(), "negative ledger rejected");
        Check(state.IsValid() && state.Years[1].SpiritStones.Issued == 5_000_000_000L
            && state.Years[1].SpiritStones.Transferred == 98 && state.Years[1].SpiritStones.Consumed == 2,
            "issuance, transfers and sinks are separately accounted");
        Check(state.Lifetime.SpiritStones.Issued == 5_000_000_000L
            && state.Lifetime.SpiritStones.Consumed == 2, "lifetime totals survive a save");
        MclslWorldRunRepository.Current = new();
        Actor actor = new() { Id = 42 };
        MclslEconomyCommands.RestoreBalances(actor, 4_000_000_000L, 5_000_000_000L);
        Check(MclslEconomyCommands.Balance(actor, MclslActorDataKeys.Contribution) == 4_000_000_000L
            && MclslEconomyCommands.Balance(actor, MclslActorDataKeys.SpiritStones) == 5_000_000_000L,
            "runtime adapter stores both currencies in the world ledger");
        long revision = MclslWorldRunRepository.Current.Economy.Revision;
        MclslEconomyCommands.RestoreBalances(actor, 4_000_000_000L, 5_000_000_000L);
        Check(MclslWorldRunRepository.Current.Economy.Revision == revision, "reapplying identical snapshot issues no currency");
        before = JsonConvert.SerializeObject(MclslWorldRunRepository.Current.Economy);
        bool overflow = false;
        try { MclslEconomyCommands.RestoreBalances(actor, 4_000_000_010L, long.MaxValue); }
        catch (InvalidOperationException) { overflow = true; }
        // The second issue reaches exactly long.MaxValue and remains legal.
        Check(!overflow && MclslEconomyCommands.Balance(actor, MclslActorDataKeys.SpiritStones) == long.MaxValue,
            "64-bit maximum balance remains exact");
        MclslEconomyCommands.SetBalance(actor, MclslActorDataKeys.SpiritStones, long.MaxValue - 1);
        before = JsonConvert.SerializeObject(MclslWorldRunRepository.Current.Economy);
        try { MclslEconomyCommands.RestoreBalances(actor, 4_000_000_020L, long.MaxValue); }
        catch (InvalidOperationException) { overflow = true; }
        Check(overflow && JsonConvert.SerializeObject(MclslWorldRunRepository.Current.Economy) == before,
            "overflow of yearly issuance rolls back the other currency during restoration");
        MclslWorldRunRepository.ResetWorld();
        Check(MclslAnnualExecutionContext.TryEnter(actor, 8), "enter explicit settlement year");
        try { MclslEconomyCommands.SetBalance(actor, MclslActorDataKeys.Contribution, 20); }
        finally { MclslAnnualExecutionContext.Exit(actor, 8); }
        Check(MclslWorldRunRepository.Current.Economy.Years.ContainsKey(8)
            && !MclslWorldRunRepository.Current.Economy.Years.ContainsKey(1),
            "catch-up income is booked to settlement year instead of native current year");
        MclslWorldRunRepository.ResetWorld();
        Check(MclslEconomyCommands.Balance(actor, MclslActorDataKeys.SpiritStones) == 0,
            "actor ID reused in another world must not inherit the old wallet");
        MclslEconomyCommands.SetBalance(actor, MclslActorDataKeys.SpiritStones, 5_000_000_000L);
        string account = MclslEconomyCommands.Account(actor);
        Check(MclslEconomyCommands.Commit(1, new[] {
            new MclslEconomicOperation(MclslEconomicKind.Transfer, stone, 4_000_000_000L, account, "family/one")
        }) == MclslEconomicResult.Applied
            && MclslWorldRunRepository.Current.Economy.Balance("family/one", stone) == 4_000_000_000L,
            "family transfer shares the ledger and supports balances beyond Int32");
        Check(MclslEconomyCommands.Commit(1, new[] {
            new MclslEconomicOperation(MclslEconomicKind.Transfer, stone, 15, "family/one", account)
        }) == MclslEconomicResult.Applied
            && MclslWorldRunRepository.Current.Economy.Balance("family/one", stone)
                + MclslEconomyCommands.Balance(actor, MclslActorDataKeys.SpiritStones) == 5_000_000_000L,
            "family withdrawal does not issue currency");
        Actor nextLife = new() { Id = 42 };
        nextLife.data.created_time = 100;
        Check(MclslEconomyCommands.Account(nextLife) != account
            && MclslEconomyCommands.Balance(nextLife, MclslActorDataKeys.SpiritStones) == 0,
            "same native ID in a later lifecycle must have a separate wallet");
        MclslWorldRunRepository.ResetWorld();
    }

    // Ledger-only stress test. This does not simulate recipes, demand, wars,
    // inventory or eras and must not be reported as the full economy simulation.
    internal static void RunLongLedgerCheck()
    {
        for (int seed = 1; seed <= 100; seed++)
        {
            MclslEconomyState state = new();
            long expectedSupply = 0;
            uint random = (uint)seed;
            for (int year = 1; year <= 1000; year++)
            {
                random = unchecked(random * 1664525U + 1013904223U);
                long income = 20 + random % 80;
                long fee = income / 10;
                long transfer = income / 2;
                string actor = "actor/" + random % 8;
                var operations = new[]
                {
                    new MclslEconomicOperation(MclslEconomicKind.Issue, MclslCurrency.SpiritStone, income, toAccount: actor),
                    new MclslEconomicOperation(MclslEconomicKind.Transfer, MclslCurrency.SpiritStone, transfer, actor, "family/test"),
                    new MclslEconomicOperation(MclslEconomicKind.Consume, MclslCurrency.SpiritStone, fee, actor)
                };
                if (MclslEconomicTransaction.Commit(state, year, operations, "annual", year) != MclslEconomicResult.Applied)
                    throw new Exception("long ledger run failed to commit");
                expectedSupply += income - fee;
                long revision = state.Revision;
                if (MclslEconomicTransaction.Commit(state, year, operations, "annual", year) != MclslEconomicResult.AlreadyApplied
                    || state.Revision != revision || state.Wallets.Values.Sum(wallet => wallet.SpiritStones) != expectedSupply
                    || state.Lifetime.SpiritStones.Issued - state.Lifetime.SpiritStones.Consumed != expectedSupply
                    || state.Years.Count > 20)
                    throw new Exception("long ledger run violated conservation or replay protection");
                if (year % 250 == 0)
                {
                    state = JsonConvert.DeserializeObject<MclslEconomyState>(JsonConvert.SerializeObject(state))!;
                    if (!state.IsValid() || state.CompletedCursors["annual"] != year)
                        throw new Exception("long ledger run lost state across save/load");
                }
            }
        }
    }
}
