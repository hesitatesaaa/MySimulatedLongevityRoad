// Scope / invalidation tests for the real MclslAnnualEconomyPlan type.
// Run: dotnet run --project EconomyPlanScopeTests.csproj
using System;
using System.Collections.Generic;
using MySimulatedLongevityRoad.Systems;

namespace EconomyPlanScopeTests
{
    internal static class Program
    {
        private static int _failures;

        private static void Check(bool condition, string name)
        {
            if (condition) Console.WriteLine("PASS " + name);
            else { Console.WriteLine("FAIL " + name); _failures++; }
        }

        private static int Main()
        {
            var a = new Actor(1);
            var b = new Actor(2);

            // 1. Outside any operation scope nothing may be retained: every call
            //    must rebuild, even for the same actor and year.
            MclslConsumableInventoryPolicy.Reset();
            MclslAnnualEconomyPlan.ReservePlan(a, 100);
            MclslAnnualEconomyPlan.ReservePlan(a, 100);
            Check(MclslConsumableInventoryPolicy.BuildCalls == 2,
                "out-of-scope requests are never cached (builds=" + MclslConsumableInventoryPolicy.BuildCalls + ")");

            // 2. Inside one operation the same actor/year plan is reused exactly once.
            MclslConsumableInventoryPolicy.Reset();
            MclslAnnualEconomyPlan.Begin(a.Id, 100);
            IReadOnlyList<string> first = MclslAnnualEconomyPlan.ReservePlan(a, 100);
            IReadOnlyList<string> second = MclslAnnualEconomyPlan.ReservePlan(a, 100);
            Check(MclslConsumableInventoryPolicy.BuildCalls == 1,
                "in-scope plan is reused (builds=" + MclslConsumableInventoryPolicy.BuildCalls + ")");
            Check(ReferenceEquals(first, second), "in-scope reuse returns the same instance");
            Check(first.Count == 1 && first[0] == "PLAN:1", "in-scope plan content is the built plan");

            // 3. A different year inside the scope invalidates the memo.
            MclslConsumableInventoryPolicy.Reset();
            MclslAnnualEconomyPlan.Begin(a.Id, 100);
            MclslAnnualEconomyPlan.ReservePlan(a, 100);
            MclslAnnualEconomyPlan.ReservePlan(a, 101);
            Check(MclslConsumableInventoryPolicy.BuildCalls == 2, "a different year rebuilds the plan");

            // 4. A different actor inside the scope invalidates the memo. The memo
            //    reuses one list instance, so A's content is captured before the
            //    switch (the returned list is only valid until the next request).
            MclslConsumableInventoryPolicy.Reset();
            MclslAnnualEconomyPlan.Begin(a.Id, 100);
            IReadOnlyList<string> planA = MclslAnnualEconomyPlan.ReservePlan(a, 100);
            string aContent = string.Join(",", planA);
            IReadOnlyList<string> planB = MclslAnnualEconomyPlan.ReservePlan(b, 100);
            Check(MclslConsumableInventoryPolicy.BuildCalls == 2, "a different actor rebuilds the plan");
            Check(aContent == "PLAN:1" && planB[0] == "PLAN:2", "each actor sees its own plan");

            // 5. Invalidate (called from the pipeline finally) drops the plan, which
            //    is how a realm/status/mana change between operations is observed.
            MclslConsumableInventoryPolicy.Reset();
            MclslAnnualEconomyPlan.Begin(a.Id, 100);
            MclslAnnualEconomyPlan.ReservePlan(a, 100);
            MclslAnnualEconomyPlan.Invalidate();
            Check(!MclslAnnualEconomyPlan.HasScope, "invalidated scope reports no scope");
            MclslAnnualEconomyPlan.ReservePlan(a, 100);
            Check(MclslConsumableInventoryPolicy.BuildCalls == 2, "post-invalidate request rebuilds the plan");

            // 6. A build that throws must not leave a cache entry behind.
            MclslConsumableInventoryPolicy.Reset();
            MclslAnnualEconomyPlan.Begin(a.Id, 100);
            MclslConsumableInventoryPolicy.ThrowOnBuild = true;
            bool threw = false;
            try { MclslAnnualEconomyPlan.ReservePlan(a, 100); }
            catch (InvalidOperationException) { threw = true; }
            MclslConsumableInventoryPolicy.ThrowOnBuild = false;
            Check(threw, "build failure propagates to the caller");
            Check(MclslAnnualEconomyPlan.HasScope, "scope stays open until the pipeline invalidates it");
            MclslAnnualEconomyPlan.ReservePlan(a, 100);
            Check(MclslConsumableInventoryPolicy.BuildCalls == 2,
                "after a failed build the next request rebuilds instead of serving a partial plan");

            // 7. Begin on a new operation resets the retained plan even without a
            //    preceding Invalidate (defensive against an abandoned scope).
            MclslConsumableInventoryPolicy.Reset();
            MclslAnnualEconomyPlan.Begin(a.Id, 100);
            MclslAnnualEconomyPlan.ReservePlan(a, 100);
            MclslAnnualEconomyPlan.Begin(a.Id, 100);
            MclslAnnualEconomyPlan.ReservePlan(a, 100);
            Check(MclslConsumableInventoryPolicy.BuildCalls == 2, "Begin starts a fresh operation");

            // 8. After the scope closes, other actors are unaffected (no retained
            //    plan leaks between actors across operations).
            MclslConsumableInventoryPolicy.Reset();
            MclslAnnualEconomyPlan.Begin(a.Id, 100);
            MclslAnnualEconomyPlan.ReservePlan(a, 100);
            MclslAnnualEconomyPlan.Invalidate();
            MclslAnnualEconomyPlan.Begin(b.Id, 100);
            IReadOnlyList<string> bPlan = MclslAnnualEconomyPlan.ReservePlan(b, 100);
            Check(MclslConsumableInventoryPolicy.BuildCalls == 2 && bPlan[0] == "PLAN:2",
                "next operation rebuilds for its own actor");

            Console.WriteLine(_failures == 0 ? "ALL TESTS PASSED" : (_failures + " TEST(S) FAILED"));
            return _failures == 0 ? 0 : 1;
        }
    }
}
