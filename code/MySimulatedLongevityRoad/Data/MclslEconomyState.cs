using System;
using System.Collections.Generic;

namespace MySimulatedLongevityRoad.Data;

internal enum MclslCurrency : byte { SpiritStone, Contribution }
internal enum MclslEconomicKind : byte { Issue, Transfer, Consume, Recycle, Destroy }
internal enum MclslEconomicResult : byte { Applied, AlreadyApplied, Invalid, InsufficientFunds, Overflow }

internal sealed class MclslWalletState
{
    public long SpiritStones { get; set; }
    public long Contribution { get; set; }

    internal long Balance(MclslCurrency currency)
        => currency == MclslCurrency.SpiritStone ? SpiritStones : Contribution;

    internal void Set(MclslCurrency currency, long value)
    {
        if (currency == MclslCurrency.SpiritStone) SpiritStones = value;
        else Contribution = value;
    }

    internal MclslWalletState Copy() => new() { SpiritStones = SpiritStones, Contribution = Contribution };
}

internal sealed class MclslCurrencyLedgerTotals
{
    public long Issued { get; set; }
    public long Transferred { get; set; }
    public long Consumed { get; set; }
    public long Recycled { get; set; }
    public long Destroyed { get; set; }

    internal bool IsValid() => Issued >= 0 && Transferred >= 0 && Consumed >= 0
        && Recycled >= 0 && Destroyed >= 0;

    internal MclslCurrencyLedgerTotals Copy() => new()
    {
        Issued = Issued, Transferred = Transferred, Consumed = Consumed,
        Recycled = Recycled, Destroyed = Destroyed
    };

    internal void Record(MclslEconomicKind kind, long amount)
    {
        checked
        {
            switch (kind)
            {
                case MclslEconomicKind.Issue: Issued += amount; break;
                case MclslEconomicKind.Transfer: Transferred += amount; break;
                case MclslEconomicKind.Consume: Consumed += amount; break;
                case MclslEconomicKind.Recycle: Recycled += amount; break;
                case MclslEconomicKind.Destroy: Destroyed += amount; break;
                default: throw new ArgumentOutOfRangeException(nameof(kind));
            }
        }
    }
}

internal sealed class MclslEconomicYearTotals
{
    public MclslCurrencyLedgerTotals SpiritStones { get; set; } = new();
    public MclslCurrencyLedgerTotals Contribution { get; set; } = new();

    internal MclslEconomicYearTotals Copy() => new()
    {
        SpiritStones = SpiritStones.Copy(), Contribution = Contribution.Copy()
    };

    internal void Record(MclslCurrency currency, MclslEconomicKind kind, long amount)
        => (currency == MclslCurrency.SpiritStone ? SpiritStones : Contribution).Record(kind, amount);
}

internal sealed class MclslEconomyState
{
    public int Version { get; set; } = 2;
    public long Revision { get; set; }
    public int LatestYear { get; set; }
    public MclslEconomicYearTotals Lifetime { get; set; } = new();
    public Dictionary<string, MclslWalletState> Wallets { get; set; } = new(StringComparer.Ordinal);
    // A cursor belongs to a command source, not to each historical transaction.
    // Annual sources retain one receipt per actor/step instead of one per year.
    public Dictionary<string, long> CompletedCursors { get; set; } = new(StringComparer.Ordinal);
    public Dictionary<int, MclslEconomicYearTotals> Years { get; set; } = new();

    internal long Balance(string account, MclslCurrency currency)
        => account != null && Wallets.TryGetValue(account, out MclslWalletState wallet) ? wallet.Balance(currency) : 0;

    internal bool IsValid()
    {
        if (Version != 2 || Revision < 0 || LatestYear < 0 || Wallets == null || CompletedCursors == null
            || Years == null || Lifetime?.SpiritStones == null || Lifetime.Contribution == null
            || !Lifetime.SpiritStones.IsValid() || !Lifetime.Contribution.IsValid()) return false;
        foreach (var pair in Wallets)
            if (string.IsNullOrWhiteSpace(pair.Key) || pair.Value == null
                || pair.Value.SpiritStones < 0 || pair.Value.Contribution < 0) return false;
        foreach (var pair in CompletedCursors)
            if (string.IsNullOrWhiteSpace(pair.Key) || pair.Value < 0) return false;
        foreach (var pair in Years)
            if (pair.Key < 0 || pair.Key > LatestYear || pair.Value?.SpiritStones == null || pair.Value.Contribution == null
                || !pair.Value.SpiritStones.IsValid() || !pair.Value.Contribution.IsValid()) return false;
        return true;
    }
}

internal readonly struct MclslEconomicOperation
{
    internal readonly MclslEconomicKind Kind;
    internal readonly MclslCurrency Currency;
    internal readonly string FromAccount;
    internal readonly string ToAccount;
    internal readonly long Amount;

    internal MclslEconomicOperation(MclslEconomicKind kind, MclslCurrency currency, long amount,
        string fromAccount = null, string toAccount = null)
    {
        Kind = kind; Currency = currency; Amount = amount;
        FromAccount = fromAccount; ToAccount = toAccount;
    }
}

internal static class MclslEconomicTransaction
{
    private const int RetainedYearCount = 20;
    internal static MclslEconomicResult Commit(MclslEconomyState state, int year,
        IReadOnlyList<MclslEconomicOperation> operations, string source = null, long cursor = 0)
    {
        if (state == null || state.Wallets == null || state.Years == null || state.CompletedCursors == null
            || state.Version != 2 || state.Revision < 0 || state.Lifetime?.SpiritStones == null
            || state.Lifetime.Contribution == null || operations == null || year < 0 || cursor < 0
            || source != null && string.IsNullOrWhiteSpace(source)) return MclslEconomicResult.Invalid;
        if (!string.IsNullOrEmpty(source) && state.CompletedCursors.TryGetValue(source, out long completed)
            && completed >= cursor) return MclslEconomicResult.AlreadyApplied;
        Dictionary<string, MclslWalletState> staged = new(StringComparer.Ordinal);
        bool hasTotals = state.Years.TryGetValue(year, out MclslEconomicYearTotals oldTotals);
        if (hasTotals && (oldTotals?.SpiritStones == null || oldTotals.Contribution == null
            || !oldTotals.SpiritStones.IsValid() || !oldTotals.Contribution.IsValid())) return MclslEconomicResult.Invalid;
        MclslEconomicYearTotals totals = hasTotals ? oldTotals.Copy() : new();
        MclslEconomicYearTotals lifetime = state.Lifetime.Copy();
        long revision;
        try
        {
            revision = checked(state.Revision + 1);
            foreach (MclslEconomicOperation operation in operations)
            {
                if (operation.Amount < 0 || operation.Currency > MclslCurrency.Contribution
                    || operation.Kind > MclslEconomicKind.Destroy) return MclslEconomicResult.Invalid;
                bool debit = operation.Kind is MclslEconomicKind.Transfer or MclslEconomicKind.Consume or MclslEconomicKind.Destroy;
                bool credit = operation.Kind is MclslEconomicKind.Transfer or MclslEconomicKind.Issue or MclslEconomicKind.Recycle;
                if (debit && string.IsNullOrWhiteSpace(operation.FromAccount)
                    || credit && string.IsNullOrWhiteSpace(operation.ToAccount)) return MclslEconomicResult.Invalid;
                if (debit && !ValidWallet(state, operation.FromAccount)
                    || credit && !ValidWallet(state, operation.ToAccount)) return MclslEconomicResult.Invalid;
                if (operation.Amount == 0) continue;
                if (debit)
                {
                    MclslWalletState wallet = Wallet(state, staged, operation.FromAccount);
                    long balance = wallet.Balance(operation.Currency);
                    if (balance < operation.Amount) return MclslEconomicResult.InsufficientFunds;
                    wallet.Set(operation.Currency, balance - operation.Amount);
                }
                if (credit)
                {
                    MclslWalletState wallet = Wallet(state, staged, operation.ToAccount);
                    wallet.Set(operation.Currency, checked(wallet.Balance(operation.Currency) + operation.Amount));
                }
                totals.Record(operation.Currency, operation.Kind, operation.Amount);
                lifetime.Record(operation.Currency, operation.Kind, operation.Amount);
            }
        }
        catch (OverflowException) { return MclslEconomicResult.Overflow; }
        // No game callbacks or serialization occur inside the commit boundary.
        foreach (var pair in staged) state.Wallets[pair.Key] = pair.Value;
        state.Lifetime = lifetime;
        int previousLatestYear = state.LatestYear;
        state.LatestYear = Math.Max(state.LatestYear, year);
        int oldest = state.LatestYear - RetainedYearCount + 1;
        if (year >= oldest) state.Years[year] = totals;
        if (state.LatestYear > previousLatestYear)
        {
            foreach (int historicalYear in new List<int>(state.Years.Keys))
                if (historicalYear < oldest) state.Years.Remove(historicalYear);
        }
        if (!string.IsNullOrEmpty(source)) state.CompletedCursors[source] = cursor;
        state.Revision = revision;
        return MclslEconomicResult.Applied;
    }

    private static bool ValidWallet(MclslEconomyState state, string account)
        => !state.Wallets.TryGetValue(account, out MclslWalletState wallet)
            || wallet != null && wallet.SpiritStones >= 0 && wallet.Contribution >= 0;

    private static MclslWalletState Wallet(MclslEconomyState state,
        Dictionary<string, MclslWalletState> staged, string account)
    {
        if (staged.TryGetValue(account, out MclslWalletState wallet)) return wallet;
        wallet = state.Wallets.TryGetValue(account, out MclslWalletState current) ? current.Copy() : new();
        staged[account] = wallet;
        return wallet;
    }
}
