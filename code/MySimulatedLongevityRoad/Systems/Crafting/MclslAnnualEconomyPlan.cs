using System.Collections.Generic;

namespace MySimulatedLongevityRoad.Systems;

/// <summary>
/// Per-annual-operation memo for the consumable reserve plan that profession
/// crafting, market demand planning and surplus listing all derive from the same
/// actor state. Before this memo one actor-year rebuilt the identical plan two
/// to four times: profession recipe selection, market batch construction and
/// surplus listing each called the policy separately, and the market batch also
/// called it again through the planned-recipe check.
///
/// Scope rules, both enforced here rather than by convention:
/// <list type="bullet">
/// <item>A plan is retained only between <see cref="Begin"/> and
/// <see cref="Invalidate"/>, i.e. inside one annual pipeline operation. Every
/// out-of-scope request rebuilds into a separate scratch list and retains
/// nothing, so a caller outside the pipeline can never poison the memo.</item>
/// <item><see cref="Invalidate"/> is called from the pipeline's <c>finally</c>,
/// so an operation that throws cannot leave a cached plan behind for the next
/// operation or the next year.</item>
/// </list>
/// The plan depends on live realm, status and mana state, which is why it must
/// not survive an operation boundary: progression inside a later operation can
/// change the realm (and therefore the breakthrough pill) before the market
/// stage asks again.
/// </summary>
internal static class MclslAnnualEconomyPlan
{
    private static readonly List<string> Plan = new(16);
    private static readonly List<string> UnscopedPlan = new(16);
    private static long _actorId = -1L;
    private static int _year = -1;
    private static bool _active;
    private static bool _cached;

    /// <summary>Opens one operation scope for a single actor and annual year.</summary>
    internal static void Begin(long actorId, int year)
    {
        _active = true;
        _cached = false;
        _actorId = actorId;
        _year = year;
    }

    /// <summary>Closes the operation scope and drops any retained plan.</summary>
    internal static void Invalidate()
    {
        _active = false;
        _cached = false;
        _actorId = -1L;
        _year = -1;
    }

    internal static bool HasScope => _active;

    /// <summary>
    /// Read-only reserve plan for one actor and year. Callers must not mutate the
    /// returned list, and it stays valid only until the next
    /// <see cref="ReservePlan"/> call in the same scope (the memo owns one list
    /// instance): consume it before asking for another plan.
    /// </summary>
    internal static IReadOnlyList<string> ReservePlan(Actor actor, int year)
    {
        long actorId = MclslActorAccessor.Id(actor);
        if (_active)
        {
            if (_cached && actorId == _actorId && year == _year) return Plan;
            MclslConsumableInventoryPolicy.BuildReservePlan(actor, Plan);
            _actorId = actorId;
            _year = year;
            _cached = true;
            return Plan;
        }
        // Outside a pipeline operation nothing may be retained.
        MclslConsumableInventoryPolicy.BuildReservePlan(actor, UnscopedPlan);
        return UnscopedPlan;
    }
}
