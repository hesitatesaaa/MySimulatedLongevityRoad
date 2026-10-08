using System.Diagnostics;
using MySimulatedLongevityRoad.Core;
using MySimulatedLongevityRoad.Data;
using MySimulatedLongevityRoad.Modules;
using MySimulatedLongevityRoad.Systems;

internal static class AnnualBackpressureTests
{
    private static void Check(bool condition, string message)
    {
        if (!condition) throw new Exception(message);
    }

    internal static void Run()
    {
        long clock = 0;
        MclslAnnualFrameBudget.Timestamp = () => clock;
        try
        {
            Check(MclslAnnualFrameBudget.ForLag(0) == 1.5 && MclslAnnualFrameBudget.ForLag(2) == 1.5
                && MclslAnnualFrameBudget.ForLag(3) == 2.5 && MclslAnnualFrameBudget.ForLag(300) == 2.5,
                "annual budget must stay within 1.5/2.5ms regardless of lag");
            MclslAnnualFrameBudget.Begin(1.5);
            clock = Stopwatch.Frequency * 15 / 10000;
            Check(!MclslAnnualFrameBudget.TryConsumeOperation(), "deadline must stop before another step begins");
            MclslAnnualFrameBudget.End();
            Check(MclslAnnualFrameBudget.LastTimeExhausted, "time exhaustion must be reported");
            MclslAnnualFrameBudget.Begin(2.5);
            MclslFrameDeadline.RemainingMs = 0;
            Check(!MclslAnnualFrameBudget.TryConsumeOperation(), "global frame deadline must dominate annual budget");
            MclslAnnualFrameBudget.End();
            MclslFrameDeadline.RemainingMs = double.MaxValue;
            MclslAnnualFrameBudget.Begin(2.5);
            var ready = new MclslAnnualReadyQueue();
            for (int id = 1; id <= 1500; id++) ready.Enqueue(id, 2, 1);
            int released = ready.Release(2, MclslAnnualFrameBudget.RemainingOperations,
                () => MclslAnnualFrameBudget.Expired);
            MclslAnnualFrameBudget.ConsumeOperations(released);
            Check(released == 1500, "waiting release must not retain the old 64-actor cap");
            int turns = 0;
            while (MclslAnnualFrameBudget.TryConsumeOperation()) turns++;
            Check(turns == 4096 - 1500 && turns > 80,
                "scan, release and actor work must share one ceiling, without the old 80-step cap");
            MclslAnnualFrameBudget.End();
            Check(MclslAnnualFrameBudget.LastOperationsExhausted, "operation ceiling must be reported");
            Check(!MclslAnnualFrameBudget.Expired, "finished annual budget must not block unrelated frame work");

            MclslModuleHub.Clear();
            MclslWorldArchiveStore.Clear();
            MclslWorldRunRepository.Current = new();
            var batch = MclslWorldRunRepository.Current.AnnualBatch;
            batch.LastCompletedYear = 20;
            MclslRuntime.Year = 320;
            MclslRuntimeSettings.CoreEnabled = true;
            UnityEngine.Time.unscaledTime = 1;
            MclslAnnualBackpressure.InitializeAfterLoad();
            foreach (bool playerPaused in new[] { false, true })
            {
                Config.paused = playerPaused;
                UnityEngine.Time.timeScale = playerPaused ? 0f : 1.25f;
                foreach (bool blocked in new[] { false, true })
                {
                    batch.WorldBlocked = blocked;
                    MclslAnnualBackpressure.TickStatus();
                    Check(MclslAnnualBackpressure.AppliedFactor == 1f && Config.paused == playerPaused
                        && UnityEngine.Time.timeScale == (playerPaused ? 0f : 1.25f)
                        && batch.LastCompletedYear == 20,
                        "backlog and failure observation must never change native speed, pause or saved progress");
                }
            }
            Check(MclslAnnualBackpressure.StatusText == "MCLSL_annual_backlog_blocked",
                "blocked annual work must remain visible");
            MclslAnnualBackpressure.Clear();
            Check(Config.paused && UnityEngine.Time.timeScale == 0f, "cleanup must preserve player pause");
            Config.paused = false;
            UnityEngine.Time.timeScale = 1f;
            batch = new MclslAnnualBatchState { LastCompletedYear = 20 };
            MclslWorldRunRepository.Current.AnnualBatch = batch;
            MclslRuntime.Year = 21;
            int failedAttempts = 0;
            FakeModuleSink.AnnualWork = () => { failedAttempts++; throw new Exception("injected worker failure"); };
            MclslModuleHub.OnWorldLoaded(21, true);
            for (int frame = 1; frame <= 10; frame++) MclslModuleHub.TickFrame(frame, true);
            Check(batch.WorldBlocked && failedAttempts == 1 && batch.LastCompletedYear == 20
                && !Config.paused && UnityEngine.Time.timeScale == 1f,
                "failed annual work must retain its cursor, stop replay and leave world speed alone");
            FakeModuleSink.AnnualWork = null;
            batch = new MclslAnnualBatchState { LastCompletedYear = 20 };
            MclslWorldRunRepository.Current.AnnualBatch = batch;
            MclslModuleHub.Clear();
            MclslModuleHub.OnWorldLoaded(21, true);
            bool annualRan = false;
            FakeModuleSink.AnnualWork = () =>
            {
                Check(!MclslAnnualFrameBudget.Expired, "annual work must receive budget before noncritical frame work");
                annualRan = true;
            };
            FakeModuleSink.FrameWork = () =>
            {
                Check(annualRan, "frame work ran before the annual phase");
                clock += Stopwatch.Frequency / 1000 * 3;
            };
            MclslModuleHub.TickFrame(11, true);
            FakeModuleSink.AnnualWork = null;
            FakeModuleSink.FrameWork = null;
            int baselineLag = RunLoad(4, 80, "old-count-cap");
            int optimizedLag = RunLoad(4, int.MaxValue, "deadline-only");
            int overloadedLag = RunLoad(30, int.MaxValue, "insufficient-service");
            int integratedLag = RunLoad(4, int.MaxValue, "actual-world-lane", true);
            Check(integratedLag <= 2, "integrated world-lane load should keep up at the specified synthetic cost");
            Check(RunLoad(30, int.MaxValue, "actual-world-lane-overload", true) > 2,
                "integrated overload must honestly retain backlog");
            Check(optimizedLag <= 2 && optimizedLag < baselineLag && overloadedLag > 12,
                "sufficient service must catch up, while overload must be reported without hiding it by slowing time");
        }
        finally
        {
            FakeModuleSink.AnnualWork = null;
            FakeModuleSink.FrameWork = null;
            MclslAnnualFrameBudget.End();
            MclslAnnualFrameBudget.Timestamp = Stopwatch.GetTimestamp;
            MclslFrameDeadline.RemainingMs = double.MaxValue;
            MclslAnnualBackpressure.Clear();
            MclslModuleHub.Clear();
            MclslRuntime.Year = 1;
        }
    }

    private static int RunLoad(int stepMicroseconds, int stepCap, string label, bool worldLane = false)
    {
        // Synthetic work cost; production queue, budget, module hub and monitor.
        // This is not a Unity/gameplay CPU benchmark.
        long clock = 0;
        long cost = Math.Max(1, (long)Math.Round(Stopwatch.Frequency * stepMicroseconds / 1000000d));
        MclslAnnualFrameBudget.Timestamp = () => clock;
        var batch = new MclslAnnualBatchState { LastCompletedYear = 99 };
        MclslWorldRunRepository.Current.AnnualBatch = batch;
        MclslModuleHub.Clear();
        FakeModuleSink.Events.Clear();
        MclslRuntime.Year = 100;
        UnityEngine.Time.unscaledTime = 0;
        MclslAnnualBackpressure.InitializeAfterLoad();
        var queue = new MclslAnnualReadyQueue();
        MclslAnnualWorldRuntimeLane.Clear();
        Actor[] actors = Enumerable.Range(1, 1500).Select(id => new Actor { Id = id }).ToArray();
        MclslActorRegistry.Actors.Clear();
        foreach (Actor actor in actors) MclslActorRegistry.Actors[actor.Id] = actor;
        WorldLaneSink.Events.Clear();
        WorldLaneSink.Action = _ => { clock += cost; return true; };
        var steps = new int[1500];
        var cultivationYear = Enumerable.Repeat(99, 1500).ToArray();
        var grants = new int[1500];
        // The first year's entry benefit and later pipeline share the same cursor.
        for (int id = 0; id < 1500; id++)
            for (int attempt = 0; attempt < 2; attempt++)
                if (MclslInitialCultivationPolicy.ShouldApply(100, 100, cultivationYear[id], false))
                { cultivationYear[id] = 100; grants[id]++; }
        int eventCursor = 0, activeYear = 0, previousYear = 99, maxLag = 0, maxStepsPerFrame = 0;
        int annualPasses = 0;
        FakeModuleSink.AnnualWork = () =>
        {
            annualPasses++;
            while (eventCursor < FakeModuleSink.Events.Count)
            {
                string entry = FakeModuleSink.Events[eventCursor++];
                if (!entry.StartsWith("RuntimeCadence:")) continue;
                activeYear = int.Parse(entry.Split(':')[1]);
                Check(queue.Count == 0 && activeYear == previousYear + 1,
                    "new annual work must start after previous completion, exactly once and in order");
                previousYear = activeYear;
                batch.ActiveYear = activeYear;
                batch.WorldStage = 0;
                Array.Clear(steps);
                for (int id = 1; id <= 1500; id++) queue.Enqueue(id, activeYear, activeYear);
            }
            int processed = 0;
            while (queue.ReadyCount > 0 && processed < stepCap && MclslAnnualFrameBudget.TryConsumeOperation())
            {
                queue.TryDequeue(out long actorId);
                int id = (int)actorId - 1;
                // FIFO round robin must preserve actor order across every stage.
                Check(id == processedActor % 1500, "round-robin actor order changed");
                processedActor++;
                steps[id]++;
                if (steps[id] == 8 && cultivationYear[id] < activeYear)
                { cultivationYear[id] = activeYear; grants[id]++; }
                if (steps[id] < 24) queue.Enqueue(actorId, activeYear, activeYear);
                clock += cost;
                processed++;
                MclslAnnualBackpressure.RecordStep();
            }
            maxStepsPerFrame = Math.Max(maxStepsPerFrame, processed);
            if (activeYear > batch.LastCompletedYear && queue.Count == 0)
            {
                if (worldLane)
                {
                    MclslAnnualWorldRuntimeLane.Schedule(activeYear, true, true);
                    if (!MclslAnnualWorldRuntimeLane.Tick(actors)) return;
                }
                Check(steps.All(x => x == 24) && grants.All(x => x == activeYear - 99),
                    "every actor must finish all annual steps and get exactly one cultivation benefit per year");
                batch.LastCompletedYear = activeYear;
                batch.ActiveYear = 0;
                MclslAnnualBackpressure.RecordYear();
            }
        };
        processedActor = 0;
        MclslModuleHub.OnWorldLoaded(100, true);
        for (int frame = 1; frame <= 9000; frame++)
        {
            // x20 native time is unconditional: neither lag nor monitor can change it.
            int year = 100 + frame / 150;
            UnityEngine.Time.unscaledTime = frame / 60f;
            if (year != MclslRuntime.Year) MclslModuleHub.TickAnnual(year, true);
            MclslRuntime.Year = year;
            int before = annualPasses;
            MclslModuleHub.TickFrame(frame, true);
            Check(annualPasses - before == 1, "annual consumption must run once per rendered frame");
            MclslAnnualBackpressure.TickStatus();
            maxLag = Math.Max(maxLag, year - batch.LastCompletedYear);
            Check(MclslAnnualBackpressure.AppliedFactor == 1f && !Config.paused
                && UnityEngine.Time.timeScale == 1f, "load test must never throttle native time");
        }
        int lag = MclslRuntime.Year - batch.LastCompletedYear;
        if (stepCap > 80 && stepMicroseconds == 4) Check(maxStepsPerFrame > 80, "new budget must use cheap-step capacity");
        Console.WriteLine($"Annual 1500 actors [{label}]: world={MclslRuntime.Year}, completed={batch.LastCompletedYear}, lag={lag}, maxLag={maxLag}, maxSteps/frame={maxStepsPerFrame}, nativeFactor=1.");
        FakeModuleSink.AnnualWork = null;
        MclslAnnualWorldRuntimeLane.Clear();
        WorldLaneSink.Events.Clear();
        WorldLaneSink.Action = _ => true;
        MclslActorRegistry.Actors.Clear();
        return lag;
    }

    private static long processedActor;
}
