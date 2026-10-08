using MySimulatedLongevityRoad.Core;
using MySimulatedLongevityRoad.Data;
using MySimulatedLongevityRoad.Systems;

static void Check(bool value, string message) { if (!value) throw new Exception(message); }
foreach (string system in new[] { MclslCultivationSystemIds.AncientLaw, MclslCultivationSystemIds.NewLaw })
{
    for (int id = 1; id <= 5000; id++)
    {
        Actor actor = new() { Id = id, Scale = id % 2 == 0 ? 0.001f : 1f };
        MclslActorAccessor.Set(actor, "system", system);
        MclslActorAccessor.Set(actor, "start", 100);
        MclslActorAccessor.Set(actor, "last", 99);
        // An entry created inside another actor's step must retain its request.
        Actor other = new() { Id = id + 10000 };
        Check(MclslAnnualExecutionContext.TryEnter(other, 90), "enter outer annual context");
        MclslAnnualCultivationExecutor.TryApplyInitialYear(actor, 100);
        Check(!MclslAnnualCultivationExecutor.TryApplyQueuedInitialYear(actor), "no nested growth");
        Check(MclslScheduler.Pending.Count == 1, "entry request survives active context");
        MclslAnnualExecutionContext.Exit(other, 90);
        Check(MclslAnnualCultivationExecutor.TryApplyQueuedInitialYear(actor), "first growth succeeds");
        Check(MclslActorAccessor.GetInt(actor, "last") == 100 && MclslScheduler.Pending.Count == 0,
            "successful first growth commits cursor and removes pending entry");
        Check(!MclslAnnualCultivationExecutor.TryApplyQueuedInitialYear(actor), "no duplicate first-year grant");
        if (id % 2 == 0)
            Check(MclslCultivationGrowthSystem.CurrentTrueEssence(actor) == 0
                && MclslActorAccessor.GetFloat(actor, "fraction") > 0, "fractional growth counts as completion");
        // Simulate all integer essence being consumed by a breakthrough.
        MclslActorAccessor.Set(actor, "essence", 0);
        Check(!MclslAnnualCultivationExecutor.TryApplyOneAnnualStep(actor, 100),
            "zero essence must never rewind a committed year");
        Check(!MclslAnnualCultivationExecutor.TryApplyOneAnnualStep(actor, 90),
            "lagging annual work must not replay a newer first-year commit");
        Check(MclslAnnualCultivationExecutor.TryApplyOneAnnualStep(actor, 101), "next annual year still progresses");
    }
}
Actor failed = new() { Id = 20001 };
MclslActorAccessor.Set(failed, "start", 100);
MclslCultivationExecutorFailureTest(failed);
static void MclslCultivationExecutorFailureTest(Actor actor)
{
    MclslCultivationGrowthSystem.ThrowOnGrant = true;
    try { MclslAnnualCultivationExecutor.TryApplyQueuedInitialYear(actor); throw new Exception("missing failure"); }
    catch (InvalidOperationException) { }
    finally { MclslCultivationGrowthSystem.ThrowOnGrant = false; }
    Check(!MclslAnnualExecutionContext.IsActive && MclslActorAccessor.GetInt(actor, "last", -1) == -1,
        "failed grant must release execution context without committing a year");
}
Console.WriteLine("Production cultivation executor: 5000 actors per law, nested entry, fractional growth, consumed essence, lagging years and failure passed (Unity/growth dependencies stubbed; not FPS validation).");
HistoryTests.Run();
