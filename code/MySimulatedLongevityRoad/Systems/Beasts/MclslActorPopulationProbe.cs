using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using MySimulatedLongevityRoad.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Profiling;

namespace MySimulatedLongevityRoad.Systems;

// Opt-in paused render-pipeline stress test. It does not certify normal gameplay FPS.
internal static class MclslActorPopulationProbe
{
    private static readonly string Output = Environment.GetEnvironmentVariable("MCLSL_ACTOR_POPULATION_PROBE");
    private static readonly int[] Counts = { 100, 500, 1000 };
    private static readonly List<Actor> Actors = new();
    private static readonly List<object> Results = new();
    private static readonly List<double> Timings = new();
    private static string[] Ids;
    private static int _phase;
    private static double _mapAt = -1, _placedAt = -1;
    private static bool _done;
    internal static void Tick()
    {
        if (_done || string.IsNullOrWhiteSpace(Output)) return;
        try
        {
            if (World.world == null || SmoothLoader.isLoading()) return;
            double now = Time.realtimeSinceStartup;
            if (Ids == null)
            {
                var ids = new List<string>();
                string file = Path.Combine(MySimulatedLongevityRoad.MclslMod.I.GetDeclaration().FolderPath,
                    "resources", "actor-frames.manifest.json");
                foreach (JToken group in JObject.Parse(File.ReadAllText(file))["groups"])
                    if ((bool?)group["approved"] == true) ids.Add((string)group["actorId"]);
                Ids = ids.ToArray();
            }
            if (_mapAt < 0)
            {
                Actors.Clear(); Timings.Clear(); _placedAt = -1; _mapAt = now;
                World.world.generateNewMap(); return;
            }
            if (now - _mapAt < 10) return;
            Config.paused = true;
            if (_placedAt < 0)
            {
                for (int i = 0; i < Counts[_phase]; i++)
                {
                    string id = Ids[i % Ids.Length];
                    WorldTile tile = World.world.GetTile(32 + i % 40, 32 + i / 40);
                    Actor actor = World.world.units.createNewUnit(id, tile, false, 0f, null, null, true, false, false, false);
                    if (id.EndsWith("_yuan", StringComparison.Ordinal))
                        MclslActorAccessor.Set(actor, MclslActorDataKeys.Realm, MclslRealmIds.YuanYing);
                    Actors.Add(actor);
                }
                MoveCamera.instance?.focusOn(new UnityEngine.Vector3(52, 45, 0));
                _placedAt = now;
            }
            MoveCamera.instance?.forceZoom(35);
            foreach (Actor actor in Actors)
            {
                actor.is_visible = true;
                if (actor.current_tile?.zone != null) actor.current_tile.zone.visible = true;
            }
            var watch = Stopwatch.StartNew();
            World.world.units.calculateVisibleActors();
            watch.Stop();
            if (now - _placedAt < 10) return;
            Timings.Add(watch.Elapsed.TotalMilliseconds);
            if (now - _placedAt < 25) return;
            Timings.Sort();
            int visible = World.world.units.visible_units.count;
            Results.Add(new { requestedUnits = Counts[_phase], placedUnits = Actors.Count, visibleUnits = visible,
                samples = Timings.Count, warmupSeconds = 10, observationSeconds = 15,
                renderPipelineMedianMs = Timings[Timings.Count / 2],
                renderPipelineP95Ms = Timings[(int)((Timings.Count - 1) * .95)],
                unityAllocatedBytes = Profiler.GetTotalAllocatedMemoryLong(),
                unityReservedBytes = Profiler.GetTotalReservedMemoryLong(), managedBytes = GC.GetTotalMemory(false) });
            _phase++; _mapAt = -1;
            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(Output)));
            File.WriteAllText(Output, JsonConvert.SerializeObject(new {
                kind = "paused-forced-visible-render-pipeline-population-diagnostic", capturedAtUtc = DateTime.UtcNow,
                phaseCount = Results.Count, complete = _phase == Counts.Length, results = Results,
                visibilityForced = true, gamePaused = true, newUnsavedMapsOnly = true,
                fullGameplayFpsVerified = false, gpuMemoryVerified = false,
                limitation = "Pipeline call time excludes full game update and screen presentation; not a 30/60/144 FPS or accelerated-combat acceptance result."
            }, Formatting.Indented));
            if (_phase == Counts.Length) _done = true;
        }
        catch (Exception ex) { _done = true; UnityEngine.Debug.LogWarning("[MCLSL population probe] " + ex); }
    }
}
