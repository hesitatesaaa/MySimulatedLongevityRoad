using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using MySimulatedLongevityRoad.Data;

namespace MySimulatedLongevityRoad.Systems;

// Creates an unsaved, fresh diagnostic map only with an explicit environment opt-in.
internal static class MclslActorSizeProbe
{
    private static readonly string Output = Environment.GetEnvironmentVariable("MCLSL_ACTOR_SIZE_PROBE");
    private static string[] Ids;
    private static readonly HashSet<string> NativeIds = new();
    private static readonly Dictionary<string, object> Samples = new();
    private static readonly Dictionary<string, object> Observed = new();
    private static readonly List<Actor> Placed = new();
    private static double _requested = -1, _spawned = -1;
    private static bool _done;
    internal static void Tick()
    {
        if (string.IsNullOrWhiteSpace(Output)) return;
        if (_done) { MclslActorPopulationProbe.Tick(); return; }
        try
        {
            double now = Time.realtimeSinceStartup;
            if (now < 90 || World.world == null || SmoothLoader.isLoading()) return;
            if (Ids == null)
            {
                string manifest = Path.Combine(MySimulatedLongevityRoad.MclslMod.I.GetDeclaration().FolderPath,
                    "resources", "actor-frames.manifest.json");
                var groups = JObject.Parse(File.ReadAllText(manifest))["groups"];
                var ids = new List<string>();
                foreach (JToken group in groups)
                {
                    if ((bool?)group["approved"] != true) continue;
                    string id = (string)group["actorId"];
                    ids.Add(id);
                    if (group["nativeResourcePath"] != null) NativeIds.Add(id);
                }
                foreach (JToken group in groups)
                {
                    if ((bool?)group["approved"] != true) continue;
                    string reference = (string)group["calibration"]?["reference"];
                    if (!string.IsNullOrEmpty(reference) && !ids.Contains(reference)) ids.Add(reference);
                }
                Ids = ids.ToArray();
            }
            if (_requested < 0)
            {
                _requested = now;
                World.world.generateNewMap();
                Debug.Log("[MCLSL size probe] requested fresh unsaved map");
                return;
            }
            if (now - _requested < 10) return;
            if (_spawned < 0)
            {
                Config.paused = true;
                for (int i = 0; i < Ids.Length; i++)
                {
                    if (AssetManager.actor_library.get(Ids[i]) == null)
                        throw new InvalidDataException("Size probe actor missing: " + Ids[i]);
                    WorldTile tile = World.world.GetTile(Math.Min(MapBox.width - 3, 32 + (i % 6) * 10),
                        Math.Min(MapBox.height - 3, 32 + (i / 6) * 10));
                    if (tile == null) throw new InvalidDataException("Size probe tile missing");
                    Actor spawned = World.world.units.createNewUnit(Ids[i], tile, false, 0f, null, null, true, false, false, false);
                    if (Ids[i].EndsWith("_yuan", StringComparison.Ordinal))
                        MclslActorAccessor.Set(spawned, MclslActorDataKeys.Realm, MclslRealmIds.YuanYing);
                    Placed.Add(spawned);
                }
                Camera camera = Camera.main;
                if (camera != null)
                {
                    Vector3 position = camera.transform.position;
                    camera.transform.position = new Vector3(57, 47, position.z);
                    MoveCamera.instance?.focusOn(new Vector3(57, 47, position.z));
                    MoveCamera.instance?.forceZoom(35);
                }
                _spawned = now;
                Debug.Log("[MCLSL size probe] placed " + Ids.Length + " units in fresh paused map");
                return;
            }
            Config.paused = true;
            MoveCamera.instance?.forceZoom(35);
            foreach (Actor actor in Placed)
            {
                actor.is_visible = true;
                if (actor.current_tile?.zone != null) actor.current_tile.zone.visible = true;
            }
            var manager = World.world.units;
            // Batch mode may omit the normal draw pass; run the game's own
            // synchronous render-data pipeline on the actual placed actors.
            manager.calculateVisibleActors();
            var data = manager.render_data;
            var visible = manager.visible_units;
            if (data != null && visible?.array != null)
                for (int i = 0; i < visible.count && i < visible.array.Length && i < data.main_sprites.Length; i++)
                {
                    Actor actor = visible.array[i];
                    string id = actor?.asset?.id;
                    if (id != null) Observed[id] = new { alive = actor.isAlive(), main = data.main_sprites[i]?.name,
                        colored = data.main_sprite_colored[i]?.name,
                        scale = data.scales[i].ToString() };
                    if (id == null || Array.IndexOf(Ids, id) < 0 || Samples.ContainsKey(id)) continue;
                    if (!actor.isAlive()) continue;
                    Sprite s = data.main_sprite_colored[i] ?? data.main_sprites[i];
                    if (s == null) continue;
                    if (NativeIds.Contains(id) && !s.texture.name.StartsWith("mclsl_native_")) continue;
                    Vector3 scale = data.scales[i];
                    Samples[id] = new { actorId = id, sprite = s.name, texture = s.texture.name,
                        alive = actor.isAlive(), authoredBaseScale = actor.asset.base_stats["scale"],
                        texturePath = actor.asset.texture_asset.texture_path_main,
                        canvas = new[] { s.texture.width, s.texture.height }, ppu = s.pixelsPerUnit,
                        filter = s.texture.filterMode.ToString(), mipmaps = s.texture.mipmapCount,
                        readable = s.texture.isReadable, textureFormat = s.texture.format.ToString(),
                        rect = new[] { s.rect.x, s.rect.y, s.rect.width, s.rect.height },
                        spriteBounds = new[] { s.bounds.size.x, s.bounds.size.y },
                        actualRenderScale = new[] { scale.x, scale.y, scale.z },
                        actualCanvasWorldSize = new[] { s.bounds.size.x * Math.Abs(scale.x), s.bounds.size.y * Math.Abs(scale.y) } };
                }
            if (Samples.Count < Ids.Length && now - _spawned < 45) return;
            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(Output)));
            File.WriteAllText(Output, JsonConvert.SerializeObject(new
            {
                kind = "worldbox-placed-unit-size-probe", saveTestScope = "new-unsaved-map",
                geometrySource = "ActorManager.render_data.main_sprite_colored/main_sprites + scales", samples = Samples, expectedUnits = Ids.Length,
                renderDataPipelineExecuted = true, observed = Observed,
                visibilityForcedForDiagnostic = true, pausedFromPlacement = true,
                capturedUnits = Samples.Count, capturedAtUtc = DateTime.UtcNow,
                cameraSize = Camera.main == null ? 0 : Camera.main.orthographicSize,
                cameraPosition = Camera.main == null ? "unavailable" : Camera.main.transform.position.ToString(),
                visibleUnitCount = visible == null ? 0 : visible.count,
                sizeComparisonPassed = false, populationPerformanceVerified = false
            }, Formatting.Indented));
            _done = true;
            Debug.Log("[MCLSL size probe] saved " + Output);
        }
        catch (Exception ex) { _done = true; Debug.LogWarning("[MCLSL size probe] " + ex); }
    }
}
