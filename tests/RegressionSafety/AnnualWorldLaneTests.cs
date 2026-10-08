using System.Diagnostics;
using MySimulatedLongevityRoad.Core;
using MySimulatedLongevityRoad.Data;
using MySimulatedLongevityRoad.Systems;

internal static class AnnualWorldLaneTests
{
    private static long _clock;
    private static void Check(bool ok, string why) { if (!ok) throw new Exception(why); }
    private static void Reset()
    {
        MclslAnnualWorldRuntimeLane.Clear();
        MclslWorldRunRepository.Current = new();
        WorldLaneSink.Events.Clear();
        WorldLaneSink.Action = _ => true;
        MclslAnnualWorkMetrics.Clear();
        MclslFrameDeadline.RemainingMs = double.MaxValue;
        _clock = 0;
        UnityEngine.Time.frameCount = 0;
    }
    private static void Begin() => MclslAnnualFrameBudget.Begin(2.5);
    internal static void Run()
    {
        var oldClock = MclslAnnualFrameBudget.Timestamp;
        MclslAnnualFrameBudget.Timestamp = () => _clock;
        try
        {
            Reset();
            Actor[] actors = Enumerable.Range(1, 1500).Select(id => new Actor { Id = id }).ToArray();
            MclslActorRegistry.Actors.Clear();
            foreach (Actor actor in actors) MclslActorRegistry.Actors.Add(actor.Id, actor);
            Begin();
            var builder = MclslAnnualWorldSnapshot.BeginBuild(actors);
            Check(builder.Tick(int.MaxValue) && builder.Complete().LineageActors.Count == 1500,
                "actual snapshot must scan 1500 actors without a 256-item limit");
            Check(MclslAnnualFrameBudget.Operations == 1500, "snapshot accounts once per actor");
            MclslAnnualFrameBudget.End();
            Reset();
            Begin();
            MclslAnnualFrameBudget.ConsumeOperations(4090);
            builder = MclslAnnualWorldSnapshot.BeginBuild(actors);
            Check(!builder.Tick(int.MaxValue), "snapshot must stop at the shared operation ceiling");
            MclslAnnualFrameBudget.End();
            Begin();
            Check(builder.Tick(int.MaxValue) && builder.Complete().LineageActors.Select(a => a.Id)
                .SequenceEqual(actors.Select(a => a.Id)), "snapshot resumes without omission or duplication");
            MclslAnnualFrameBudget.End();

            // Production world-stage dispatcher and snapshot; gameplay effects are stubbed.
            Reset();
            Begin();
            MclslAnnualWorldRuntimeLane.Schedule(100, true, true);
            Check(MclslAnnualWorldRuntimeLane.Tick(actors), "cheap stages should finish in one frame");
            string[] expected = WorldLaneSink.Events.ToArray();
            Check(expected.Distinct().Count() == expected.Length, "stage side effects must run once");
            MclslAnnualFrameBudget.End();
            foreach (byte savedStage in new byte[] { 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 15, 14 })
            {
                Reset();
                MclslWorldRunRepository.Current.AnnualBatch.WorldStage = savedStage;
                Begin();
                MclslAnnualWorldRuntimeLane.Schedule(100, true, true);
                Check(MclslAnnualWorldRuntimeLane.Tick(actors), "saved world stage must resume");
                string[] trace = WorldLaneSink.Events.ToArray();
                Check(trace.Length == 0 || expected.Skip(expected.Length - trace.Length).SequenceEqual(trace),
                    "resume must preserve event order without replaying earlier stages");
                Check(!trace.Contains("Publish"), "restored stage must not repeat annual initialization");
                MclslAnnualFrameBudget.End();
            }

            Reset();
            WorldLaneSink.Action = name => name != "Cave";
            Begin();
            MclslAnnualWorldRuntimeLane.Schedule(100, true, true);
            Check(!MclslAnnualWorldRuntimeLane.Tick(actors)
                && WorldLaneSink.Events.Count(x => x == "Cave") == 1,
                "waiting must yield without busy retry");
            Check(MclslAnnualWorkMetrics.Report().Contains("exit.Waiting=1"), "wait must be reported");
            MclslAnnualFrameBudget.End();
            WorldLaneSink.Action = _ => true;
            Begin();
            Check(MclslAnnualWorldRuntimeLane.Tick(actors), "resolved dependency resumes");
            MclslAnnualFrameBudget.End();

            Reset();
            int partial = 0;
            WorldLaneSink.Action = name =>
            {
                if (name != "Cave" || ++partial == 1500) return true;
                MclslAnnualFrameBudget.ReportProgress();
                return false;
            };
            Begin();
            MclslAnnualWorldRuntimeLane.Schedule(100, true, true);
            Check(MclslAnnualWorldRuntimeLane.Tick(actors) && partial == 1500,
                "explicit partial progress must continue within budget without 64/8 caps");
            MclslAnnualFrameBudget.End();

            Reset();
            WorldLaneSink.Action = name =>
            {
                if (name == "Cave") { MclslAnnualFrameBudget.ReportProgress(); return false; }
                return true;
            };
            Begin();
            MclslAnnualWorldRuntimeLane.Schedule(100, true, true);
            Check(!MclslAnnualWorldRuntimeLane.Tick(actors) && MclslAnnualFrameBudget.RemainingOperations == 0,
                "progress loop must stop at 4096 operations");
            MclslAnnualFrameBudget.End();

            Reset();
            WorldLaneSink.Action = name => name == "Cave" ? throw new Exception("after write") : true;
            Begin();
            MclslAnnualWorldRuntimeLane.Schedule(100, true, true);
            Check(!MclslAnnualWorldRuntimeLane.Tick(actors), "injected failure blocks");
            byte cursor = MclslWorldRunRepository.Current.AnnualBatch.WorldStage;
            MclslAnnualFrameBudget.End();
            Begin();
            Check(!MclslAnnualWorldRuntimeLane.Tick(actors)
                && WorldLaneSink.Events.Count(x => x == "Cave") == 1
                && MclslWorldRunRepository.Current.AnnualBatch.WorldStage == cursor,
                "failed side effects retain cursor and are never replayed");
            MclslAnnualFrameBudget.End();

            Reset();
            WorldLaneSink.Action = _ => { _clock += Stopwatch.Frequency * 3 / 1000; return true; };
            Begin();
            MclslAnnualWorldRuntimeLane.Schedule(100, true, true);
            Check(!MclslAnnualWorldRuntimeLane.Tick(actors) && !WorldLaneSink.Events.Contains("Era"),
                "elapsed deadline must prevent the next stage");
            MclslAnnualFrameBudget.End();
            Check(MclslAnnualFrameBudget.LastTimeExhausted, "slow indivisible stage is reported");

            // Compare fixed-input stage/RNG traces across frame slicing and runtime reload.
            string RunTrace(bool slice, bool reload)
            {
                Reset();
                var random = new Random(314159);
                var trace = new List<string>();
                WorldLaneSink.Action = name =>
                {
                    trace.Add(name + ":" + random.Next());
                    if (slice) _clock += Stopwatch.Frequency * 3 / 1000;
                    return true;
                };
                for (int year = 100; year < 110; year++)
                {
                    MclslWorldRunRepository.Current.AnnualBatch.WorldStage = 0;
                    bool complete = false;
                    for (int frame = 0; !complete && frame < 100; frame++)
                    {
                        Begin();
                        MclslAnnualWorldRuntimeLane.Schedule(year, true, true);
                        complete = MclslAnnualWorldRuntimeLane.Tick(actors);
                        MclslAnnualFrameBudget.End();
                        if (reload && !complete) MclslAnnualWorldRuntimeLane.Clear();
                    }
                    Check(complete, "multi-year world workload stalled");
                }
                return string.Join("|", trace);
            }
            Check(RunTrace(false, false) == RunTrace(true, false), "frame slicing must preserve stage/RNG order");
            Check(RunTrace(false, false) == RunTrace(true, true), "runtime reload must preserve stage/RNG order");
            Console.WriteLine("Actual annual world lane: 1500 snapshot, continuous stages, waits, partial progress, deadlines, 4096 ceiling, failure and multi-year reload traces passed (gameplay effects stubbed).");
        }
        finally
        {
            MclslAnnualFrameBudget.End();
            MclslAnnualFrameBudget.Timestamp = oldClock;
            Reset();
            MclslActorRegistry.Actors.Clear();
        }
    }
}
