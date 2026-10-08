using System;
using System.Collections.Generic;
using System.Globalization;
using MySimulatedLongevityRoad.Core;
using MySimulatedLongevityRoad.Data;

namespace MySimulatedLongevityRoad.Systems;

internal static class MclslEconomyCommands
{
    internal static int YearFor(Actor actor)
        => MclslAnnualExecutionContext.ResolveYear(actor, MclslRuntime.CurrentYear());

    internal static bool WasApplied(string source, long cursor)
        => MclslWorldRunRepository.Current.Economy.CompletedCursors.TryGetValue(source, out long completed) && completed >= cursor;
    internal static bool IsCurrencyKey(string key)
        => key == MclslActorDataKeys.SpiritStones || key == MclslActorDataKeys.Contribution;

    internal static MclslCurrency Currency(string key) => key switch
    {
        MclslActorDataKeys.SpiritStones => MclslCurrency.SpiritStone,
        MclslActorDataKeys.Contribution => MclslCurrency.Contribution,
        _ => throw new ArgumentException("非货币字段", nameof(key))
    };

    internal static string Account(Actor actor)
    {
        long id = MclslActorAccessor.Id(actor);
        if (id <= 0) throw new InvalidOperationException("货币命令缺少人物身份");
        // Native IDs can be reused after a load or actor replacement. Birth time is
        // persisted by WorldBox and separates the lifecycle without a per-frame map.
        return "actor/" + id.ToString(CultureInfo.InvariantCulture) + "/"
            + Convert.ToString(actor.data.created_time, CultureInfo.InvariantCulture);
    }

    internal static long Balance(Actor actor, string key)
        => actor?.data == null || MclslActorAccessor.Id(actor) <= 0 ? 0
            : MclslWorldRunRepository.Current.Economy.Balance(Account(actor), Currency(key));

    internal static void SetBalance(Actor actor, string key, long value)
    {
        if (value < 0) throw new ArgumentOutOfRangeException(nameof(value), "余额不能为负数");
        MclslCurrency currency = Currency(key);
        long current = Balance(actor, key);
        if (current == value) return;
        string account = Account(actor);
        var operation = value > current
            ? new MclslEconomicOperation(MclslEconomicKind.Issue, currency, value - current, toAccount: account)
            : new MclslEconomicOperation(MclslEconomicKind.Consume, currency, current - value, fromAccount: account);
        MclslEconomicResult result = Commit(YearFor(actor), new[] { operation });
        if (result != MclslEconomicResult.Applied) throw new InvalidOperationException("余额提交失败：" + result);
    }

    internal static void Issue(Actor actor, string key, long amount, string source = null, long cursor = 0)
    {
        if (amount < 0) throw new ArgumentOutOfRangeException(nameof(amount));
        MclslEconomicResult result = Commit(YearFor(actor), new[]
        {
            new MclslEconomicOperation(MclslEconomicKind.Issue, Currency(key), amount,
                toAccount: Account(actor))
        }, source, cursor);
        if (result is not (MclslEconomicResult.Applied or MclslEconomicResult.AlreadyApplied))
            throw new InvalidOperationException("货币发行失败：" + result);
        NotifyWallet(actor);
    }

    internal static bool TryConsume(Actor actor, string key, long amount, string source = null, long cursor = 0)
    {
        if (amount < 0) return false;
        MclslEconomicResult result = Commit(YearFor(actor), new[]
        {
            new MclslEconomicOperation(MclslEconomicKind.Consume, Currency(key), amount,
                fromAccount: Account(actor))
        }, source, cursor);
        if (result is not (MclslEconomicResult.Applied or MclslEconomicResult.AlreadyApplied)) return false;
        NotifyWallet(actor);
        return true;
    }

    internal static void RestoreBalances(Actor actor, long contribution, long stones)
    {
        if (contribution < 0 || stones < 0) throw new InvalidOperationException("快照余额不能为负数");
        string account = Account(actor);
        var operations = new List<MclslEconomicOperation>(2);
        AddBalanceChange(operations, account, MclslCurrency.Contribution,
            Balance(actor, MclslActorDataKeys.Contribution), contribution);
        AddBalanceChange(operations, account, MclslCurrency.SpiritStone,
            Balance(actor, MclslActorDataKeys.SpiritStones), stones);
        if (operations.Count == 0) return;
        MclslEconomicResult result = Commit(YearFor(actor), operations);
        if (result != MclslEconomicResult.Applied) throw new InvalidOperationException("快照余额提交失败：" + result);
        NotifyWallet(actor);
    }

    internal static void CloseDeadActorWallet(Actor actor)
    {
        string account = Account(actor);
        string source = "death-wallet/" + account;
        if (WasApplied(source, 1)) return;
        long contribution = Balance(actor, MclslActorDataKeys.Contribution);
        long stones = Balance(actor, MclslActorDataKeys.SpiritStones);
        MclslEconomicResult result = Commit(YearFor(actor), new[]
        {
            new MclslEconomicOperation(MclslEconomicKind.Destroy, MclslCurrency.Contribution,
                contribution, fromAccount: account),
            new MclslEconomicOperation(MclslEconomicKind.Destroy, MclslCurrency.SpiritStone,
                stones, fromAccount: account)
        }, source, 1);
        if (result is not (MclslEconomicResult.Applied or MclslEconomicResult.AlreadyApplied))
            throw new InvalidOperationException("死亡钱包清算失败：" + result);
        NotifyWallet(actor);
    }

    private static void AddBalanceChange(List<MclslEconomicOperation> operations, string account,
        MclslCurrency currency, long current, long desired)
    {
        if (current == desired) return;
        operations.Add(desired > current
            ? new MclslEconomicOperation(MclslEconomicKind.Issue, currency, desired - current, toAccount: account)
            : new MclslEconomicOperation(MclslEconomicKind.Consume, currency, current - desired, fromAccount: account));
    }

    internal static MclslEconomicResult Commit(int year, IReadOnlyList<MclslEconomicOperation> operations,
        string source = null, long cursor = 0)
    {
        MclslWorldRunRepository.EnsureCurrentRun(year);
        MclslEconomicResult result = MclslEconomicTransaction.Commit(
            MclslWorldRunRepository.Current.Economy, year, operations, source, cursor);
        if (result == MclslEconomicResult.Applied) MclslWorldArchiveStore.MarkDirty();
        return result;
    }

    internal static void NotifyWallet(Actor actor)
    {
        if (actor?.data == null) return;
        MclslRuntimeChanges.OnWrite(actor, MclslActorDataKeys.Contribution);
        MclslRuntimeChanges.OnWrite(actor, MclslActorDataKeys.SpiritStones);
    }
}
