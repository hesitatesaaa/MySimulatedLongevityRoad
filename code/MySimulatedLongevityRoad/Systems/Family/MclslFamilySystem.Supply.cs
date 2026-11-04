using System;
using System.Collections.Generic;
using MySimulatedLongevityRoad.Core;
using MySimulatedLongevityRoad.Data;

namespace MySimulatedLongevityRoad.Systems;

/// <summary>
/// Result of the annual cross-actor priority aid check. Pending means the aid is
/// due but the frame budget is spent: the caller must keep Prepare step 5 for the
/// next operation instead of letting normal supply run first.
/// </summary>
internal enum MclslFamilyPriorityAid : byte
{
    Proceed = 0,
    Pending = 1
}

internal static partial class MclslFamilySystem
{
    // Work buffers for one supply / one aid call. Both are cleared at the start
    // and in the finally of the call that owns them: the demand list is never
    // read, saved or resumed across operations, so an exhausted budget always
    // recomputes from the real family store and the real bag.
    private static readonly List<MclslMarketPurchaseNeed> SupplyNeedsScratch = new();
    private static readonly List<MclslMarketPurchaseNeed> AidNeedsScratch = new();

    /// <summary>
    /// Long-term aid cadence in years. The family id decides 5 or 10 years
    /// deterministically, so the period survives focus changes and reloads.
    /// </summary>
    internal static int AidInterval(MclslFamilyRecord family)
    {
        if (family == null) return 5;
        return (MclslStableHash.Positive43x53(family.Id) & 1) == 0 ? 5 : 10;
    }

    /// <summary>
    /// Normal annual supply for the current actor, run at the head of Prepare
    /// step 5 before profession crafting consumes materials and before the market
    /// stage fills the remainder. Demand comes from the shared market demand core
    /// fed by the family store's runtime index, and the whole list is drawn in one
    /// <see cref="TransferNeeds"/> call: one bag work copy, one commit, one plan
    /// invalidation and one purchase-batch forget. Returns false only when the
    /// annual frame budget runs out (during demand preparation or during the
    /// transfer); the pipeline then keeps step 5 and recomputes the actual demand
    /// from the real stock and bag next operation. No persistent family task batch
    /// and no cross-operation needs list is created.
    /// </summary>
    internal static bool ProcessAnnualSupply(Actor actor, int year)
    {
        if (actor?.data == null || year <= 0 || !MclslActorAccessor.Alive(actor)) return true;
        MclslFamilyRecord family = FamilyOf(actor);
        if (family?.Items == null || family.Items.Count == 0) return true;
        if (MclslBagSystem.IsLocked(actor)) return true;
        if (MclslAnnualFrameBudget.Expired) return false;

        long sample = MclslPerformanceProbe.Begin();
        List<MclslMarketPurchaseNeed> needs = SupplyNeedsScratch;
        try
        {
            needs.Clear();
            long prepareSample = MclslPerformanceProbe.Begin();
            bool prepared;
            try { prepared = MclslTianxuanMarket.BuildFamilySupplyNeeds(family, actor, year, needs); }
            finally { MclslPerformanceProbe.End("family.supply.prepare", prepareSample); }
            // Budget ran out at a category boundary: the prepared demand is not
            // the whole demand, so keep step 5 instead of acting on a partial list.
            if (!prepared) return false;
            if (needs.Count == 0) return true;
            MclslFamilyTransferOutcome outcome = TransferNeeds(family, actor, needs, out int moved);
            if (moved > 0)
            {
                // One successful commit: drop the reserve memo and the cached
                // purchase batch exactly once, then reopen the scope so the
                // following profession crafting reads the real bag again.
                MclslAnnualEconomyPlan.Invalidate();
                MclslTianxuanMarket.ForgetPurchaseBatch(actor);
                MclslAnnualEconomyPlan.Begin(MclslActorAccessor.Id(actor), year);
            }
            return outcome == MclslFamilyTransferOutcome.Completed;
        }
        finally
        {
            needs.Clear();
            MclslPerformanceProbe.End("family.supply.annual", sample);
        }
    }

    /// <summary>
    /// Cross-actor priority support. The first family member to reach Prepare
    /// step 5 triggers at most one aid per year for the family focus: when the
    /// long-term interval is due and the year has not been attempted, the focus
    /// draws one currently-needed real item from the family store, in shared
    /// demand order. Success records LastAidYear and a history entry; no stock or
    /// no demand records only the attempt. It runs before the triggering actor's
    /// scope and bag transaction and opens/ends its own focus scope.
    /// Demand preparation and the one-item draw share the normal supply
    /// implementation; the draw is capped at one item.
    /// Returns <see cref="MclslFamilyPriorityAid.Pending"/> only when the aid is
    /// due but the frame budget is spent: the caller must then keep step 5 for the
    /// next operation (without running the normal supply first, which would
    /// otherwise draw the store ahead of the focus) and no attempt/success is
    /// recorded. Otherwise it returns <see cref="MclslFamilyPriorityAid.Proceed"/>.
    /// </summary>
    internal static MclslFamilyPriorityAid ProcessPriorityAid(Actor trigger, int year)
    {
        if (trigger?.data == null || year <= 0 || !MclslActorAccessor.Alive(trigger)) return MclslFamilyPriorityAid.Proceed;
        MclslFamilyRecord family = FamilyOf(trigger);
        if (family?.Items == null) return MclslFamilyPriorityAid.Proceed;
        if (family.LastAidAttemptYear == year) return MclslFamilyPriorityAid.Proceed;      // once per family per year
        int interval = AidInterval(family);
        long baseYear = family.LastAidYear >= 0 ? family.LastAidYear : family.FoundedYear;
        if (year < baseYear + interval) return MclslFamilyPriorityAid.Proceed;             // not due yet

        // Due now. A spent frame budget defers the whole step without recording
        // an attempt (and without consuming anything): retry next operation.
        if (MclslAnnualFrameBudget.Expired) return MclslFamilyPriorityAid.Pending;

        long focusId = EnsureFocus(family);
        if (focusId <= 0 || !MclslActorRegistry.ResolveKnownOrWorld(focusId, out Actor focus)
            || focus == null || !MclslActorAccessor.Alive(focus) || MclslBagSystem.IsLocked(focus))
            return MclslFamilyPriorityAid.Proceed;
        if (!MclslAnnualFrameBudget.TryConsumeOperation()) return MclslFamilyPriorityAid.Pending;

        bool success = false;
        string successItemId = null;
        bool transactionStarted = false;
        long sample = MclslPerformanceProbe.Begin();
        List<MclslMarketPurchaseNeed> needs = AidNeedsScratch;
        MclslAnnualEconomyPlan.Begin(focusId, year);
        try
        {
            needs.Clear();
            if (MclslBagSystem.PeekExisting(focus) != null)
            {
                MclslBagSystem.BeginTransaction(focus);
                transactionStarted = true;
            }
            long prepareSample = MclslPerformanceProbe.Begin();
            bool prepared;
            try { prepared = MclslTianxuanMarket.BuildFamilySupplyNeeds(family, focus, year, needs); }
            finally { MclslPerformanceProbe.End("family.supply.prepare", prepareSample); }
            // Budget spent while preparing the demand: no aid happened this
            // operation, so no attempt is recorded and step 5 is kept.
            if (!prepared) return MclslFamilyPriorityAid.Pending;
            // Selection rule unchanged: the first need in shared demand order that
            // the family store actually holds is the one the draw will take.
            MclslFamilyStock stock = FamilyStock(family);
            for (int i = 0; i < needs.Count; i++)
            {
                MclslMarketPurchaseNeed need = needs[i];
                if (need == null || string.IsNullOrEmpty(need.ItemId) || need.Remaining <= 0) continue;
                if (stock == null || stock.Count(need.ItemId) <= 0) continue;
                successItemId = need.ItemId;
                break;
            }
            if (successItemId != null)
            {
                // Capped at one item, so a Pending outcome means nothing was
                // committed (the budget was spent before the first item): keep
                // step 5 and record no attempt.
                if (TransferNeeds(family, focus, needs, out int moved, maxItems: 1)
                    == MclslFamilyTransferOutcome.Pending)
                    return MclslFamilyPriorityAid.Pending;
                success = moved > 0;
                // One successful commit: the focus bag changed, so drop its cached
                // purchase batch exactly once. The economy scope itself is closed
                // by the finally below.
                if (success) MclslTianxuanMarket.ForgetPurchaseBatch(focus);
            }
        }
        finally
        {
            needs.Clear();
            // The focus operation ends here: its economy scope is closed before
            // the bag transaction commits, and the triggering actor opens its own
            // scope in the existing pipeline.
            MclslAnnualEconomyPlan.Invalidate();
            if (transactionStarted) MclslBagSystem.EndTransaction(focus);
            MclslPerformanceProbe.End("family.supply.aid", sample);
        }

        if (success)
        {
            family.LastAidYear = year;
            AddHistory(family, year + "年 · 扶持" + SafeName(focus)
                + (MclslItemCatalog.Get(successItemId)?.Name ?? successItemId));
        }
        else
        {
            // No stock or no demand this year: record only the attempt and retry
            // next year while the interval remains unsatisfied.
            family.LastAidAttemptYear = year;
        }
        MclslWorldArchiveStore.MarkDirty();
        return MclslFamilyPriorityAid.Proceed;
    }
}
