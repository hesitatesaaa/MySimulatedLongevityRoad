// Minimal stubs so the real MclslAnnualEconomyPlan.cs can be compiled and
// exercised outside Unity. Only the members that type actually touches exist.
using System;
using System.Collections.Generic;

namespace MySimulatedLongevityRoad.Systems
{
    internal sealed class Actor
    {
        internal long Id;
        internal Actor(long id) { Id = id; }
        public override string ToString() => "Actor(" + Id + ")";
    }

    internal static class MclslActorAccessor
    {
        internal static long Id(Actor actor) => actor == null ? 0L : actor.Id;
    }

    internal static class MclslConsumableInventoryPolicy
    {
        internal static int BuildCalls;
        internal static long LastBuiltActor;
        // Set to make the next build throw, to exercise the exception path.
        internal static bool ThrowOnBuild;

        internal static void BuildReservePlan(Actor actor, List<string> itemIds)
        {
            BuildCalls++;
            LastBuiltActor = actor == null ? 0L : actor.Id;
            if (ThrowOnBuild) throw new InvalidOperationException("stub build failure");
            itemIds.Clear();
            itemIds.Add("PLAN:" + (actor == null ? 0L : actor.Id));
        }

        internal static void Reset()
        {
            BuildCalls = 0;
            LastBuiltActor = 0L;
            ThrowOnBuild = false;
        }
    }
}
