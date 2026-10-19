using System;
using System.Collections.Generic;
using System.IO;
using MySimulatedLongevityRoad.Core;
using MySimulatedLongevityRoad.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace MySimulatedLongevityRoad.Systems;

// Replays the actual failed save, without changing its actors or archived bytes.
internal static class MclslBeastReloadProbe
{
    private static readonly string Output = Environment.GetEnvironmentVariable("MCLSL_BEAST_RELOAD_PROBE");
    private static JObject Expected;
    private static double LoadedAt = -1;
    private static bool Done;
    internal static void Tick()
    {
        if (Done || string.IsNullOrWhiteSpace(Output)) return;
        try
        {
            if (Time.realtimeSinceStartup < 90 || World.world == null || SmoothLoader.isLoading()) return;
            Config.paused = true;
            if (Expected == null)
            {
                Expected = JObject.Parse(File.ReadAllText(Path.Combine(Path.GetDirectoryName(Output), "save-reload-expected.json")));
                LoadedAt = Time.realtimeSinceStartup;
                World.world.save_manager.loadWorld((string)Expected["saveDirectory"], false);
                return;
            }
            if (Time.realtimeSinceStartup - LoadedAt < 20) return;
            var cases = new List<object>();
            int failed = 0;
            foreach (JToken item in Expected["actors"])
            {
                long id = (long)item["id"];
                bool found = MclslActorRegistry.ResolveKnownOrWorld(id, out Actor actor);
                string asset = actor?.asset?.id;
                string realm = MclslActorAccessor.Realm(actor);
                bool passed = found && asset == (string)item["asset"] && realm == (string)item["realm"];
                if (!passed) failed++;
                cases.Add(new { id, found, expectedAsset = (string)item["asset"], asset,
                    expectedRealm = (string)item["realm"], realm, passed });
            }
            File.WriteAllText(Output, JsonConvert.SerializeObject(new {
                source = "unity-runtime", kind = "actual-beast-save-reload-regression", unityVersion = Application.unityVersion,
                status = failed == 0 ? "reload-probe-completed" : "reload-probe-failed", failed, cases,
                sourceSaveSha256 = (string)Expected["sourceSaveSha256"], saveTestScope = "new-save-only",
                actualActorCount = World.world.units.Count, pausedAfterLoad = true,
                limitation = "Exact identity, form and cultivation realm recovery only; no population rendering or performance certification."
            }, Formatting.Indented));
            Done = true; Application.Quit();
        }
        catch (Exception error)
        {
            Done = true;
            File.WriteAllText(Output, JsonConvert.SerializeObject(new { source = "unity-runtime", status = "reload-probe-failed", error = error.ToString() }));
            Application.Quit();
        }
    }
}
