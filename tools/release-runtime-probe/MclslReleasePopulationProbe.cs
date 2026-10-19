using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using MySimulatedLongevityRoad.Core;
using MySimulatedLongevityRoad.Data;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Profiling;

namespace MySimulatedLongevityRoad.Systems;

// Inject only into the isolated test checkout. Normal AI and rendering remain enabled.
internal static class MclslReleasePopulationProbe
{
    private static readonly string Output = Environment.GetEnvironmentVariable("MCLSL_RELEASE_POPULATION_PROBE");
    private static readonly bool Baseline = Environment.GetEnvironmentVariable("MCLSL_RELEASE_BASELINE") == "1";
    private static readonly int[] Counts = { 100, 500, 1000 }, Fps = { 30, 60, 144 };
    private const double WarmupSeconds = 60;
    private static readonly List<Actor> Actors = new();
    private static readonly List<long> PlacedIds = new();
    private static readonly List<object> Results = new();
    private static readonly List<double> FrameTimes = new();
    private static readonly HashSet<string> Species = new();
    private static readonly Dictionary<string, object> Buttons = new();
    private static readonly Dictionary<int, string> FixtureHashes = new();
    private static double Started = -1, MapAt = -1, PlacedAt = -1, LastTick = -1, ObservedSeconds, LastWrite;
    private static int Phase, MinimumAlive = int.MaxValue, MinimumVisible = int.MaxValue;
    private static int MaximumVisible;
    private static bool Done, Saved, WaitingReload;
    private static long SavedActor;
    private static string SavedAsset, SaveFolder;
    private static double WorldTimeStart, LastCoverageAt = -1, LastCoverage, MinimumCoverage = 1;
    private static int CoverageSamples;
    private static double FirstFullCoverage = -1;

    private static void ObserveButtons()
    {
        foreach (PowerButton button in UnityEngine.Resources.FindObjectsOfTypeAll<PowerButton>())
        {
            var icon = button?.icon;
            string texture = icon?.sprite?.texture?.name;
            const string marker = "ui/ActorIcons/";
            int index = texture == null ? -1 : texture.IndexOf(marker, StringComparison.Ordinal);
            if (index < 0 || button.transform is not RectTransform rect || rect.rect.width <= 0 || rect.rect.height <= 0) continue;
            string id = texture.Substring(index + marker.Length);
            float width = icon.rectTransform.rect.width * Math.Abs(icon.rectTransform.lossyScale.x);
            float height = icon.rectTransform.rect.height * Math.Abs(icon.rectTransform.lossyScale.y);
            float outerWidth = rect.rect.width * Math.Abs(rect.lossyScale.x);
            float outerHeight = rect.rect.height * Math.Abs(rect.lossyScale.y);
            Buttons[id] = new { button = button.name, active = button.gameObject.activeInHierarchy,
                icon = icon.sprite.name, texture, canvas = new[] { icon.sprite.texture.width, icon.sprite.texture.height },
                preserveAspect = icon.preserveAspect, iconWidth = width, iconHeight = height,
                buttonWidth = outerWidth, buttonHeight = outerHeight,
                fits = width > 0 && height > 0 && width <= outerWidth + .01f && height <= outerHeight + .01f };
        }
    }

    internal static void Tick()
    {
        if (Done || string.IsNullOrEmpty(Output)) return;
        try
        {
            Application.runInBackground = true;
            QualitySettings.vSyncCount = 0;
            if (World.world == null || SmoothLoader.isLoading() || Time.realtimeSinceStartup < 90)
            { LastTick = -1; return; }
            Config.paused = PlacedAt < 0 && !WaitingReload;
            double now = Time.realtimeSinceStartup;
            Config.show_console_on_start = false;
            World.world.console?.Hide();
            Application.targetFrameRate = Fps[Math.Min(Phase, 2)];
            // Benchmark the full unit renderer at an overview zoom, rather than
            // letting the automatic minimap replace the unit-render workload.
            World.world.quality_changer._main_zoom = 1000f;
            World.world.quality_changer.setZoomOrthographic(Camera.main.orthographicSize);
            if (Started < 0) Started = now;
            if (LastTick >= 0 && !Config.paused) ObservedSeconds += now - LastTick;
            LastTick = Config.paused ? -1 : now;
            if (WaitingReload)
            {
                if (now - MapAt < 15) return;
                Saved = File.Exists(Path.Combine(SaveFolder, "map.wbox"))
                    && MclslActorRegistry.ResolveKnownOrWorld(SavedActor, out Actor restored)
                    && restored != null && restored.asset.id == SavedAsset;
                Done = true;
                Write(Saved ? "completed" : "save-reload-failed");
                Application.Quit(); return;
            }
            if (MapAt < 0)
            {
                Application.targetFrameRate = Fps[Math.Min(Phase, 2)];
                string world = Path.Combine(Path.GetDirectoryName(Path.GetFullPath(Output)), "visible-population-map-" + Phase);
                if (File.Exists(Path.Combine(world, "map.wbox"))) World.world.save_manager.loadWorld(world, false);
                else if (Baseline) World.world.generateNewMap();
                else
                {
                    throw new FileNotFoundException("Matching baseline map required", world);
                }
                MapAt = now; PlacedAt = -1;
                Actors.Clear(); PlacedIds.Clear(); FrameTimes.Clear();
                MinimumAlive = MinimumVisible = int.MaxValue; MaximumVisible = 0;
                LastCoverageAt = -1; LastCoverage = 0; MinimumCoverage = 1; CoverageSamples = 0;
                FirstFullCoverage = -1;
                Write("new-map-requested"); return;
            }
            if (now - MapAt < 10) return;
            if (PlacedAt < 0)
            {
                if (Baseline)
                {
                    string world = Path.Combine(Path.GetDirectoryName(Path.GetFullPath(Output)), "visible-population-map-" + Phase);
                    if (!File.Exists(Path.Combine(world, "map.wbox")))
                    {
                        Directory.CreateDirectory(world);
                        SaveManager.saveWorldToDirectory(world, true, true);
                    }
                }
                string fixture = Path.Combine(Path.GetDirectoryName(Path.GetFullPath(Output)), "visible-population-map-" + Phase, "map.wbox");
                using (SHA256 hash = SHA256.Create()) FixtureHashes[Phase] = BitConverter.ToString(hash.ComputeHash(File.ReadAllBytes(fixture))).Replace("-", "").ToLowerInvariant();
                var land = new List<WorldTile>(); var water = new List<WorldTile>();
                for (int y = 4; y < MapBox.height - 4; y++)
                    for (int x = 4; x < MapBox.width - 4; x++)
                    {
                        WorldTile tile = World.world.GetTile(x, y);
                        if (tile?.Type == null) continue;
                        (tile.Type.liquid ? water : land).Add(tile);
                    }
                if (land.Count == 0 || water.Count == 0) throw new InvalidOperationException("Both land and water required");
                for (int i = 0; i < Counts[Math.Min(Phase, 2)]; i++)
                {
                    MclslBeastDefinition beast = MclslBeastCatalog.All[i % MclslBeastCatalog.All.Length];
                    List<WorldTile> tiles = beast.AnimalTemplate == "piranha" ? water : land;
                    Actor actor = World.world.units.createNewUnit(beast.BeastAssetId, tiles[(i * 37) % tiles.Count],
                        false, 0f, null, null, true, false, false, false);
                    if (actor?.data == null) throw new InvalidOperationException("Placement failed: " + beast.Id);
                    Actors.Add(actor); PlacedIds.Add(MclslActorAccessor.Id(actor)); Species.Add(beast.Id);
                }
                MoveCamera.instance?.focusOn(new Vector3(MapBox.width / 2f, MapBox.height / 2f, 0));
                MoveCamera.instance?.forceZoom(Math.Max(MapBox.width, MapBox.height) * .58f);
                PlacedAt = Time.realtimeSinceStartup;
                LastTick = PlacedAt;
                WorldTimeStart = Convert.ToDouble(World.world.getCurWorldTime());
                Write("units-placed"); return;
            }
            int alive = Actors.Count(a => a != null && a.isAlive());
            int visible = World.world.units.visible_units.count;
            if (visible == 0 && now - PlacedAt > 20)
                throw new InvalidOperationException("No naturally visible units; renderGameplay=" + MapBox.isRenderGameplay()
                    + " focused=" + Application.isFocused + " camera=" + Camera.main?.transform.position
                    + " zoom=" + Camera.main?.orthographicSize + " lowRes=" + World.world.quality_changer.isLowRes());
            bool measuring = now - PlacedAt >= WarmupSeconds;
            if (measuring)
            {
                MinimumAlive = Math.Min(MinimumAlive, alive);
                MinimumVisible = Math.Min(MinimumVisible, visible);
                MaximumVisible = Math.Max(MaximumVisible, visible);
            }
            if (now - LastCoverageAt >= 1)
            {
                LastCoverageAt = now;
                int rendered = 0, native = 0;
                var manager = World.world.units;
                for (int i = 0; i < manager.visible_units.count && i < manager.visible_units.array.Length
                    && i < manager.render_data.main_sprites.Length; i++)
                {
                    Actor actor = manager.visible_units.array[i];
                    if (actor == null || !actor.isAlive() || MclslBeastCatalog.ForAsset(actor.asset.id) == null) continue;
                    rendered++;
                    Sprite sprite = manager.render_data.main_sprite_colored[i] ?? manager.render_data.main_sprites[i];
                    if (sprite?.texture?.name?.StartsWith("mclsl_native_", StringComparison.Ordinal) == true) native++;
                }
                LastCoverage = rendered == 0 ? 0 : (double)native / rendered;
                if (LastCoverage == 1 && FirstFullCoverage < 0) FirstFullCoverage = now - PlacedAt;
                if (measuring) { MinimumCoverage = Math.Min(MinimumCoverage, LastCoverage); CoverageSamples++; }
            }
            if (!measuring)
            {
                if (now - LastWrite >= 10) { LastWrite = now; Write("warming-renderer"); }
                return;
            }
            if (Phase < 3)
            {
                FrameTimes.Add(Time.unscaledDeltaTime * 1000d);
                if (now - PlacedAt >= WarmupSeconds + 120)
                {
                    double[] sorted = FrameTimes.OrderBy(x => x).ToArray();
                    Results.Add(new {
                        units = Counts[Phase], targetFps = Fps[Phase], durationSeconds = now - PlacedAt - WarmupSeconds,
                        warmupSeconds = WarmupSeconds, firstFullNativeCoverageSeconds = FirstFullCoverage,
                        placedActorIds = PlacedIds.ToArray(), placedCount = PlacedIds.Count,
                        minimumAlive = MinimumAlive, minimumVisible = MinimumVisible, maximumVisible = MaximumVisible,
                        samples = sorted.Length, averageFps = 1000d / FrameTimes.Average(),
                        frameP95Ms = sorted[(int)((sorted.Length - 1) * .95)],
                        cpuMemoryBytes = Profiler.GetTotalAllocatedMemoryLong(),
                        gpuMemoryBytes = Profiler.GetAllocatedMemoryForGraphicsDriver() > 0
                            ? (long?)Profiler.GetAllocatedMemoryForGraphicsDriver() : null,
                        gpuMemoryExternalCounterRequired = Profiler.GetAllocatedMemoryForGraphicsDriver() <= 0,
                        gamePaused = Config.paused, forcedVisibility = false, forcedRenderPipeline = false,
                        renderGameplay = MapBox.isRenderGameplay(), applicationFocused = Application.isFocused,
                        cameraPosition = Camera.main?.transform.position.ToString(), cameraSize = Camera.main?.orthographicSize,
                        lowResolution = World.world.quality_changer.isLowRes(), effectiveTargetFps = Application.targetFrameRate,
                        worldTimeStart = WorldTimeStart, worldTimeEnd = Convert.ToDouble(World.world.getCurWorldTime()),
                        nativeCoverageLast = LastCoverage, nativeCoverageMinimum = MinimumCoverage, nativeCoverageSamples = CoverageSamples
                    });
                    Phase++; Write("scenario-completed");
                    if (Phase < 3) MapAt = -1;
                    return;
                }
            }
            if (Phase >= 3 && (Baseline || ObservedSeconds >= 1200))
            {
                Actor survivor = Actors.FirstOrDefault(a => a != null && a.isAlive());
                if (survivor == null) throw new InvalidOperationException("No surviving actor for new-save test");
                SavedActor = MclslActorAccessor.Id(survivor); SavedAsset = survivor.asset.id;
                SaveFolder = Path.Combine(Path.GetDirectoryName(Path.GetFullPath(Output)), Baseline ? "baseline-save" : "release-save");
                Directory.CreateDirectory(SaveFolder);
                SaveManager.saveWorldToDirectory(SaveFolder, true, true);
                WaitingReload = true; MapAt = now;
                Write("new-save-written"); World.world.save_manager.loadWorld(SaveFolder, false); return;
            }
            if (now - LastWrite >= 10) { LastWrite = now; if (Buttons.Count < 38) ObserveButtons(); Write("observing-normal-gameplay"); }
        }
        catch (Exception error)
        { Done = true; Write("failed", error.ToString()); Application.Quit(); }
    }

    private static void Write(string status, string error = null)
    {
        string manifest = Path.Combine(MySimulatedLongevityRoad.MclslMod.I.GetDeclaration().FolderPath,
            "resources/actor-frames.manifest.json");
        string digest;
        using (SHA256 hash = SHA256.Create()) digest = BitConverter.ToString(hash.ComputeHash(File.ReadAllBytes(manifest))).Replace("-", "").ToLowerInvariant();
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(Output)));
        File.WriteAllText(Output, JsonConvert.SerializeObject(new {
            source = "unity-runtime", kind = "unpaused-native-gameplay-population-observation", status, error,
            capturedAtUtc = DateTime.UtcNow, unityVersion = Application.unityVersion, baseline = Baseline,
            graphicsDevice = SystemInfo.graphicsDeviceName, manifestSha256 = digest,
            continuousRunSeconds = ObservedSeconds, phase = Phase, currentUnits = Counts[Math.Min(Phase, 2)],
            currentTargetFps = Fps[Math.Min(Phase, 2)], scenarios = Results,
            renderResolution = new[] { Screen.width, Screen.height },
            currentVisibleUnits = World.world.units.visible_units.count,
            currentAliveUnits = Actors.Count(a => a != null && a.isAlive()),
            currentNativeCoverage = LastCoverage, renderGameplay = MapBox.isRenderGameplay(),
            placedOrdinarySpeciesIds = Species.OrderBy(x => x).ToArray(),
            placementButtons = Buttons,
            newSaveCompatibilityPassed = Saved, savedActorId = SavedActor, savedAsset = SavedAsset,
            saveTestScope = "new-save-only", actualAIEnabled = true, forcedVisibility = false,
            matchingBaselineWorlds = true, pausedOnlyDuringMapPreparation = true,
            fixtureSaveHashes = FixtureHashes,
            detailMode = "full unit renderer; automatic minimap threshold raised only in diagnostic checkout",
            logOverlayHiddenForBenchmark = true,
            observationClock = "unpaused gameplay only; loading and paused preparation excluded",
            gpuZeroMeansUnavailable = true, gamePaused = Config.paused,
            gameplayCasesPassed = false,
            limitation = "Population observation and disk reload only; combine with current controlled gameplay cases and external per-process GPU counters before release."
        }, Formatting.Indented));
    }
}
