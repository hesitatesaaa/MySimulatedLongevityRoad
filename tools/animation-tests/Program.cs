using System.Reflection;
using MySimulatedLongevityRoad;
using MySimulatedLongevityRoad.Systems;
using MySimulatedLongevityRoad.Core;
using Newtonsoft.Json.Linq;

int checks = 0;
void Check(bool value, string scenario)
{
    if (!value) throw new Exception("FAILED: " + scenario);
    checks++;
}
void Reject(Action action, string scenario)
{
    bool rejected = false;
    try { action(); } catch (ArgumentException) { rejected = true; }
    Check(rejected, scenario);
}
var clip = new MclslAnimationClipData(new[] { "attack_0", "attack_2", "attack_10" },
    new[] { 0.125, 0.25, 0.125 }, false);
_ = MclslRuntimeChanges.MainThreadId;
World.world = new World { Time = 12.5 };
MclslActorAnimationTime.Clear();
Check(MclslActorAnimationTime.Now == 0 && World.world.Reads == 0, "clock getter only reads cached value");
MclslActorAnimationTime.Sample();
Check(MclslActorAnimationTime.Now == 12.5 && World.world.Reads == 1, "main thread samples simulation clock");
World.world.Time = 13;
var workerClockReads = Enumerable.Range(0, 64).Select(_ => Task.Run(() =>
{
    MclslActorAnimationTime.Sample();
    return MclslActorAnimationTime.Now;
})).ToArray();
Task.WaitAll(workerClockReads);
Check(workerClockReads.All(t => t.Result == 12.5) && World.world.Reads == 1,
    "parallel render workers never query world state");
MclslActorAnimationTime.Sample();
Check(MclslActorAnimationTime.Now == 13, "render prefix captures latest simulation step");
World.world.Time = double.NaN;
MclslActorAnimationTime.Sample();
Check(MclslActorAnimationTime.Now == 13, "invalid sample preserves previous time");
World.world.Time = double.PositiveInfinity;
MclslActorAnimationTime.Sample();
Check(MclslActorAnimationTime.Now == 13, "infinite sample preserves previous time");
World.world.Time = 1;
MclslActorAnimationTime.Sample();
Check(MclslActorAnimationTime.Now == 1, "world clock rollback remains visible to state reset");
World.world = null;
MclslActorAnimationTime.Sample();
Check(MclslActorAnimationTime.Now == 0, "missing world resets sampled time");
Check(clip.FrameIndex(0) == 0 && clip.FrameIndex(0.124) == 0, "first frame duration");
Check(clip.FrameIndex(0.125) == 1 && clip.FrameIndex(0.375) == 2, "unequal frame boundaries");
Check(clip.FrameIndex(100) == 2, "one-shot clamps to last frame");
Check(clip.FrameIndex(double.NaN) == 0 && clip.FrameIndex(double.PositiveInfinity) == 2, "invalid/overflow elapsed");
var loop = new MclslAnimationClipData(new[] { "walk_0", "walk_1" }, new[] { .125, .125 }, true);
Check(loop.FrameIndex(.25) == 0 && loop.FrameIndex(.375) == 1, "loop duration wraps");
foreach (int fps in new[] { 30, 60, 144 })
{
    for (int frame = 0; frame < fps * 2; frame++)
    {
        double time = (double)frame / fps;
        int expected = time < .125 ? 0 : time < .375 ? 1 : 2;
        Check(clip.FrameIndex(time) == expected, "frame-rate-independent timing " + fps);
    }
}
Reject(() => new MclslAnimationClipData(new[] { "idle_0" }, new[] { 0d }, true), "zero duration");
Reject(() => new MclslAnimationClipData(new[] { "idle_0" }, new[] { double.NaN }, true), "NaN duration");
Reject(() => new MclslAnimationClipData(new[] { "../idle_0" }, new[] { .1 }, true), "frame path traversal");
var state = new MclslActorAnimationState();
Check(state.Select(10, false, .6, .3, out var elapsed) == MclslActorAction.Idle && elapsed == 0, "idle starts at first frame");
Check(state.Select(10.2, true, .6, .3, out elapsed) == MclslActorAction.Walk && elapsed == 0, "walk transition restarts");
state.Attack(10.25);
Check(state.Select(10.35, true, .6, .3, out elapsed) == MclslActorAction.Attack && Math.Abs(elapsed - .1) < 1e-9, "attack overrides movement");
state.Attack(10.4);
Check(state.Select(10.4, false, .6, .3, out elapsed) == MclslActorAction.Attack && elapsed == 0, "repeated attack restarts");
state.Hit(10.5, .3);
Check(state.Select(10.6, true, .6, .3, out elapsed) == MclslActorAction.Hit, "hit interrupts attack");
Check(state.Select(10.81, true, .6, .3, out elapsed) == MclslActorAction.Walk && elapsed == 0, "hit completion returns to movement");
state.Attack(11);
Check(state.Select(11.2, false, .6, .3, out elapsed) == MclslActorAction.Attack, "active one-shot");
double pausedElapsed = elapsed;
Check(state.Select(11.2, false, .6, .3, out elapsed) == MclslActorAction.Attack && elapsed == pausedElapsed, "paused simulation clock freezes");
Check(state.Select(12, false, .6, .3, out elapsed) == MclslActorAction.Idle, "accelerated simulation completes one-shot");
state.Attack(20);
Check(state.Select(1, false, .6, .3, out elapsed) == MclslActorAction.Idle && elapsed == 0, "clock rollback resets stale events");
state.Hit(40.1, .3);
state.Attack(40.2);
Check(state.Select(40.3, true, .6, .3, out elapsed) == MclslActorAction.Hit, "attack during hit cannot preempt it");
Check(state.Select(40.41, true, .6, .3, out elapsed) == MclslActorAction.Walk && elapsed == 0, "hit discards lower-priority attack event");
state.Attack(40.5);
Check(state.Select(40.5, false, .6, .3, out elapsed) == MclslActorAction.Attack && elapsed == 0, "attack after hit starts at first frame");
state.Hit(41, .9);
state.Attack(41.6);
Check(state.Select(41.7, false, .6, .9, out elapsed) == MclslActorAction.Hit, "suppression follows variable hit duration");
Check(state.Select(41.91, false, .6, .9, out elapsed) == MclslActorAction.Idle, "long hit does not leave queued attack");

string workspace = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../../"));
string fixture = Path.Combine(workspace, "build/animation-test-fixtures");
Directory.CreateDirectory(Path.Combine(fixture, "resources"));
Directory.CreateDirectory(Path.Combine(fixture, "GameResources/actors/Test/main"));
MclslMod.Root = fixture;
void ResetCatalog()
{
    var type = typeof(MclslActorAnimationCatalog);
    type.GetField("_loaded", BindingFlags.NonPublic | BindingFlags.Static).SetValue(null, false);
    foreach (string name in new[] { "Forms", "Legacy" })
        ((System.Collections.IDictionary)type.GetField(name, BindingFlags.NonPublic | BindingFlags.Static).GetValue(null)).Clear();
    MclslDiagnostics.Errors.Clear();
}
string manifestPath = Path.Combine(fixture, "resources/actor-frames.manifest.json");
JObject Manifest(int version)
{
    var actions = new JObject();
    foreach (string action in MclslActorAnimationCatalog.ActionNames)
        actions[action] = new JObject { ["loop"] = action is "idle" or "walk", ["frames"] = new JArray(
            new JObject { ["name"] = action + "_0", ["durationMs"] = 125 },
            new JObject { ["name"] = action + "_10", ["durationMs"] = 250 },
            new JObject { ["name"] = action + "_2", ["durationMs"] = 125 }) };
    return new JObject { ["schemaVersion"] = version, ["groups"] = new JArray(new JObject {
        ["resourcePath"] = "actors/Test", ["canvas"] = new JArray(128, 128), ["anchor"] = new JArray(64, 112),
        ["approved"] = true, ["anchorReviewed"] = true, ["pixelsPerUnit"] = 1,
        ["calibration"] = new JObject { ["verifiedInGame"] = true, ["renderScale"] = .3 }, ["actions"] = actions }) };
}
File.WriteAllText(manifestPath, Manifest(2).ToString());
ResetCatalog();
var profile = MclslActorAnimationCatalog.Get("actors/Test/");
Check(profile.AnchorApproved && profile.ScaleApproved && profile.Scale == .3f, "schema 2 anchor and scale");
Check(MclslActorAnimationCatalog.Clip("actors/Test", "idle").Names[1] == "idle_10", "explicit authored order overrides filename sorting");
Check(profile.FrameProfiles.Count == 0, "existing schema 2 retains form-level geometry");
JObject previewScale = Manifest(2);
previewScale["groups"][0]["calibration"]["verifiedInGame"] = false;
previewScale["groups"][0]["calibration"]["previewScaleApproved"] = true;
File.WriteAllText(manifestPath, previewScale.ToString());
ResetCatalog();
Check(MclslActorAnimationCatalog.Get("actors/Test").ScaleApproved
    && !(bool)previewScale["groups"][0]["calibration"]["verifiedInGame"],
    "preview scaling works without falsely marking game acceptance");
previewScale["groups"][0]["approved"] = false;
File.WriteAllText(manifestPath, previewScale.ToString());
ResetCatalog();
Check(!MclslActorAnimationCatalog.Get("actors/Test").ScaleApproved,
    "unreviewed art cannot enable preview scaling");
previewScale["groups"][0]["integrationAccepted"] = true;
previewScale["groups"][0]["anchorReviewed"] = false;
previewScale["groups"][0]["geometryConfigured"] = true;
File.WriteAllText(manifestPath, previewScale.ToString());
ResetCatalog();
Check(MclslActorAnimationCatalog.Get("actors/Test").AnchorApproved
    && MclslActorAnimationCatalog.Get("actors/Test").ScaleApproved
    && !(bool)previewScale["groups"][0]["approved"]
    && !(bool)previewScale["groups"][0]["calibration"]["verifiedInGame"],
    "explicit integration accepts configured geometry without claiming visual or Unity acceptance");
JObject waivedCombat = Manifest(2);
waivedCombat["groups"][0]["actions"]["attack"] = new JObject {
    ["visualFallback"] = "idle", ["frames"] = new JArray(), ["loop"] = false, ["durationMs"] = 600 };
File.WriteAllText(manifestPath, waivedCombat.ToString());
ResetCatalog();
var waivedAttack = MclslActorAnimationCatalog.Clip("actors/Test", "attack");
Check(waivedAttack.Names.SequenceEqual(new[] { "idle_0" }) && !waivedAttack.Loop
    && Math.Abs(waivedAttack.Duration - .6) < 1e-9, "waived combat reuses a reviewed idle pose with its own single-shot duration");
Check(MclslActorAnimationCatalog.Clip("actors/Test", "idle").Names.Length == 3,
    "combat fallback leaves the original idle cycle unchanged");
waivedCombat["groups"][0]["actions"]["attack"]["loop"] = true;
File.WriteAllText(manifestPath, waivedCombat.ToString()); ResetCatalog();
Check(MclslActorAnimationCatalog.Get("actors/Test") == null, "looping waived combat rejected");
waivedCombat["groups"][0]["actions"]["attack"]["loop"] = false;
waivedCombat["groups"][0]["actions"]["attack"]["visualFallback"] = "walk";
File.WriteAllText(manifestPath, waivedCombat.ToString()); ResetCatalog();
Check(MclslActorAnimationCatalog.Get("actors/Test") == null, "unsupported combat fallback rejected");
JObject mixed = Manifest(2);
JToken native = mixed["groups"][0]["actions"]["idle"]["frames"][0];
native["canvas"] = new JArray(1254, 1254);
native["anchor"] = new JArray(627, 1120);
native["pixelsPerUnit"] = 10;
native["anchorReviewed"] = true;
File.WriteAllText(manifestPath, mixed.ToString());
ResetCatalog();
profile = MclslActorAnimationCatalog.Get("actors/Test");
var nativeProfile = profile.FrameProfiles["idle_0"];
Check(nativeProfile.Width == 1254 && nativeProfile.Height == 1254 && nativeProfile.PixelsPerUnit == 10,
    "native frame keeps independent canvas and PPU");
Check(nativeProfile.AnchorApproved && nativeProfile.AnchorX == 627 && nativeProfile.AnchorY == 1120,
    "native frame anchor remains explicit rather than using installed canvas");
Check(!profile.FrameProfiles.ContainsKey("idle_10") && profile.Width == 128,
    "mixed frame resolutions keep unchanged form fallback");
Check(Math.Abs(600f / nativeProfile.PixelsPerUnit - 60f) < .001f,
    "PPU maps native body pixels to existing world height without actor-scale changes");
JObject streamed = (JObject)mixed.DeepClone();
streamed["groups"][0]["nativeResourcePath"] = "actors/Test";
File.WriteAllText(manifestPath, streamed.ToString());
ResetCatalog();
Check(MclslActorAnimationCatalog.Get("actors/Test") == null,
    "streamed native form rejects incomplete per-frame geometry");
foreach (string action in MclslActorAnimationCatalog.ActionNames)
    foreach (JToken frame in streamed["groups"][0]["actions"][action]["frames"])
    {
        frame["canvas"] = new JArray(1254, 1254);
        frame["anchor"] = new JArray(627, 1120);
        frame["pixelsPerUnit"] = 10;
        frame["anchorReviewed"] = true;
    }
File.WriteAllText(manifestPath, streamed.ToString());
ResetCatalog();
Check(MclslActorAnimationCatalog.Get("actors/Test").NativeResourcePath == "actors/Test",
    "complete native form uses separate non-auto-imported resource root");
JObject directIntegration = (JObject)streamed.DeepClone();
directIntegration["groups"][0]["approved"] = false;
directIntegration["groups"][0]["anchorReviewed"] = false;
directIntegration["groups"][0]["integrationAccepted"] = true;
directIntegration["groups"][0]["geometryConfigured"] = true;
foreach (string action in MclslActorAnimationCatalog.ActionNames)
    foreach (JToken frame in directIntegration["groups"][0]["actions"][action]["frames"])
    {
        frame["anchorReviewed"] = false;
        frame["geometryConfigured"] = true;
    }
File.WriteAllText(manifestPath, directIntegration.ToString());
ResetCatalog();
Check(MclslActorAnimationCatalog.Get("actors/Test").FrameProfiles["idle_0"].AnchorApproved,
    "direct native integration accepts configured frame geometry without inventing review approval");
directIntegration["groups"][0]["actions"]["idle"]["frames"][0]["pixelsPerUnit"] = 0;
File.WriteAllText(manifestPath, directIntegration.ToString());
ResetCatalog();
Check(MclslActorAnimationCatalog.Get("actors/Test") == null,
    "direct integration still rejects invalid native geometry");
streamed["groups"][0]["nativeResourcePath"] = "../../outside";
File.WriteAllText(manifestPath, streamed.ToString());
ResetCatalog();
Check(MclslActorAnimationCatalog.Get("actors/Test") == null,
    "native resource path cannot escape the mod root");
File.WriteAllText(manifestPath, mixed.ToString());
ResetCatalog();
native["anchorReviewed"] = false;
File.WriteAllText(manifestPath, mixed.ToString());
ResetCatalog();
Check(!MclslActorAnimationCatalog.Get("actors/Test").FrameProfiles["idle_0"].AnchorApproved,
    "unreviewed frame anchor cannot inherit approval");
native["pixelsPerUnit"] = 0;
File.WriteAllText(manifestPath, mixed.ToString());
ResetCatalog();
Check(MclslActorAnimationCatalog.Get("actors/Test") == null && MclslDiagnostics.Errors.Count > 0,
    "zero native-frame PPU rejected");
native["pixelsPerUnit"] = 10;
((JObject)native).Remove("anchor");
File.WriteAllText(manifestPath, mixed.ToString());
ResetCatalog();
Check(MclslActorAnimationCatalog.Get("actors/Test") == null && MclslDiagnostics.Errors.Count > 0,
    "partial per-frame geometry rejected");
File.WriteAllText(manifestPath, Manifest(1).ToString());
ResetCatalog();
profile = MclslActorAnimationCatalog.Get("actors/Test");
Check(profile.AnchorApproved && profile.Actions.Count == 0, "schema 1 profile compatibility");
foreach (int n in new[] { 0, 10, 2 }) File.WriteAllBytes(Path.Combine(fixture, "GameResources/actors/Test/main/idle_" + n + ".png"), Array.Empty<byte>());
var legacy = MclslActorAnimationCatalog.Clip("actors/Test", "idle");
Check(string.Join(",", legacy.Names) == "idle_0,idle_2,idle_10", "legacy numeric sorting");
File.WriteAllText(manifestPath, Manifest(2).ToString().Replace("\"durationMs\": 125", "\"durationMs\": -1"));
ResetCatalog();
Check(MclslActorAnimationCatalog.Get("actors/Test") == null && MclslDiagnostics.Errors.Count > 0, "malformed form isolated and diagnosed");
Check(MclslActorAnimationCatalog.Clip("actors/Test", "idle").Names.Length == 3, "invalid manifest falls back to existing frames");
bool escaped = false;
try { MclslActorAnimationCatalog.ResolveFile("../../outside"); } catch (InvalidDataException) { escaped = true; }
Check(escaped, "resource root confinement");
File.WriteAllText(manifestPath, Manifest(2).ToString());
var nativeCache = new MclslActorFrameCache<string>(12, 5);
nativeCache.Register("frame-a", 8);
nativeCache.Register("frame-b", 8);
Check(nativeCache.Request("frame-a") == null && nativeCache.PendingCount == 1,
    "missing native frame queues without decoding on render thread");
var renderRequests = Enumerable.Range(0, 32).Select(_ => Task.Run(() => nativeCache.Request("frame-a"))).ToArray();
Task.WaitAll(renderRequests);
Check(nativeCache.PendingCount == 1 && renderRequests.All(t => t.Result == null),
    "parallel render requests deduplicate a frame load");
int loadThread = 0;
var released = new List<string>();
Check(nativeCache.Pump(0, 2, 1000, key => { loadThread = Environment.CurrentManagedThreadId; return key; }, released.Add) == 1
    && loadThread == Environment.CurrentManagedThreadId && nativeCache.Request("frame-a") == "frame-a",
    "native frame loads on owner thread and becomes shared cache entry");
nativeCache.Request("frame-b");
Check(nativeCache.Pump(1, 2, 1000, key => key, released.Add) == 0
    && nativeCache.PendingCount == 1 && nativeCache.ResidentBytes == 8,
    "hot frame cannot be evicted to exceed memory budget");
Check(nativeCache.Pump(6, 2, 1000, key => key, released.Add) == 1
    && released.SequenceEqual(new[] { "frame-a" }) && nativeCache.Request("frame-b") == "frame-b",
    "cold frame releases before replacement and cache stays within budget");
Check(nativeCache.ResidentBytes <= 12, "native cache budget is a hard cap");
bool workerPumpRejected = false;
try { Task.Run(() => nativeCache.Pump(7, 1, 1000, key => key, released.Add)).GetAwaiter().GetResult(); }
catch (InvalidOperationException) { workerPumpRejected = true; }
Check(workerPumpRejected, "render worker cannot decode or destroy Unity resources");
nativeCache.Clear(released.Add);
Check(nativeCache.ResidentBytes == 0 && released.Contains("frame-b"), "world reset releases resident native frames");
MclslMod.Root = workspace;
ResetCatalog();
JObject actualManifest = JObject.Parse(File.ReadAllText(Path.Combine(workspace, "resources/actor-frames.manifest.json")));
foreach (JToken group in actualManifest["groups"])
{
    string resource = (string)group["resourcePath"];
    var actual = MclslActorAnimationCatalog.Get(resource);
    Check(actual != null && actual.NativeResourcePath != null && actual.Actions.Count == 5,
        "current production native profile parses all actions: " + (string)group["actorId"]);
}
Console.WriteLine($"PASS: {checks} assertions; timing at 30/60/144 FPS, states, schema 1/2, confinement and native-cache thread/budget checks");
