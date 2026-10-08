using MySimulatedLongevityRoad.Modules;
using MySimulatedLongevityRoad.Systems;

internal static class PriorityAnnualWorkTests
{
    internal static void Run()
    {
        MclslWorldRunRepository.Current = new();
        MclslModuleHub.Init();
        MclslModuleHub.OnWorldLoaded(1, true);
        FakeModuleSink.Events.Clear();
        MclslRuntimeCadence.Calls = 0;
        MclslModuleHub.TickAnnual(2, true);
        MclslModuleHub.TickPriorityAnnualWork(1, true);
        if (FakeModuleSink.Events.Count == 0 || MclslRuntimeCadence.Calls != 1)
            throw new Exception("annual module work and scheduler must run in the priority phase");
        MclslModuleHub.TickFrame(1, true);
        if (MclslRuntimeCadence.Calls != 1)
            throw new Exception("the normal frame phase must not run the scheduler twice");
        MclslModuleHub.Clear();
    }
}
