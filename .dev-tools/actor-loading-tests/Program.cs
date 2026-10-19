using MySimulatedLongevityRoad.Core;
using MySimulatedLongevityRoad.Systems;

int assertions = 0;
void Check(bool result, string scenario)
{
    if (!result) throw new Exception("FAILED: " + scenario);
    assertions++;
}
_ = MclslRuntimeChanges.Owner;
MclslActorAnimationCatalog.Root = Path.Combine(Directory.GetCurrentDirectory(), "build/actor-loading-fixture");
string Register(string formPath, string name, MclslActorAnimationCatalog.FrameProfile profile = null)
{
    var form = new MclslActorAnimationCatalog.Form { NativeResourcePath = formPath };
    form.FrameProfiles[name] = profile ?? new();
    string key = formPath + "/main/" + name;
    string file = MclslActorAnimationCatalog.ResolveNativeFile(key);
    Directory.CreateDirectory(Path.GetDirectoryName(file));
    File.WriteAllBytes(file, new byte[] { 1 }); // Decode is stubbed; real PNGs are checked by the Python validator.
    MclslNativeActorFrames.Register(key, form);
    return key;
}
string beast = Register("actors/Test/Beast", "idle_0");
string human = Register("actors/Test/Human", "idle_0");
string walk = Register("actors/Test/Human", "walk_0");
var beastInitial = MclslNativeActorFrames.Initial(beast);
var humanInitial = MclslNativeActorFrames.Initial(human);
Check(beastInitial != null && humanInitial != null && beastInitial != humanInitial, "forms own distinct current-art initial sprites");
Check(SpriteTextureLoader._cached_sprite_list["actors/Test/Human/main"].Length == 2
    && SpriteTextureLoader._cached_sprites[walk].name == "walk_0"
    && ReferenceEquals(SpriteTextureLoader._cached_sprites[walk].texture, humanInitial.texture),
    "all animation metadata uses distinct names and one shared current waiting texture");
Check(ReferenceEquals(beastInitial, SpriteTextureLoader._cached_sprites[beast])
    && ReferenceEquals(humanInitial, SpriteTextureLoader._cached_sprite_list["actors/Test/Human/main"][0]),
    "engine initialization shares the same current sprites without legacy files");
Check(ReferenceEquals(beastInitial, MclslNativeActorFrames.Initial(beast)) && MclslActorPixelResources.Loads[beast] == 1,
    "initial frame is decoded only once");
var parallel = Enumerable.Range(0, 32).Select(_ => Task.Run(() => MclslNativeActorFrames.Request(human))).ToArray();
Task.WaitAll(parallel);
Check(parallel.All(task => ReferenceEquals(task.Result, humanInitial)), "render workers reuse the current human initial frame");
Check(MclslNativeActorFrames.Request(walk) == null && MclslNativeActorFrames.Request(human) == humanInitial,
    "pending animation uses the current-art waiting pose");
MclslNativeActorFrames.Tick();
Check(MclslNativeActorFrames.Request(walk) != null && MclslActorPixelResources.Loads[walk] == 1,
    "main-thread pump loads the pending current frame");
MclslNativeActorFrames.Clear();
Check(MclslNativeActorFrames.Request(beast) == beastInitial && MclslNativeActorFrames.Request(human) == humanInitial,
    "world reset preserves engine-held current initial sprites");
MclslNativeActorFrames.Request(walk);
UnityEngine.Time.realtimeSinceStartup = 1;
MclslNativeActorFrames.Tick();
Check(MclslActorPixelResources.Loads[walk] == 2 && MclslActorPixelResources.Loads[human] == 1,
    "only evictable animation frames reload after reset");
string failed = Register("actors/Test/Failed", "idle_0");
MclslActorPixelResources.Failed = failed;
bool rejected = false;
try { MclslNativeActorFrames.Initial(failed); } catch (InvalidDataException) { rejected = true; }
Check(rejected && !SpriteTextureLoader._cached_sprites.ContainsKey(failed), "failed current art cannot fall back to an old sprite");
bool workerRejected = false;
try { Task.Run(() => MclslNativeActorFrames.Initial(beast)).GetAwaiter().GetResult(); }
catch (InvalidOperationException) { workerRejected = true; }
Check(workerRejected, "render worker cannot initialize Unity sprites");
string oversized = Register("actors/Test/Oversized", "idle_0", new() { Width = 9000, Height = 9000 });
bool budgetRejected = false;
try { MclslNativeActorFrames.Initial(oversized); } catch (InvalidDataException) { budgetRejected = true; }
Check(budgetRejected && !MclslActorPixelResources.Loads.ContainsKey(oversized), "reserved initial budget is enforced before decoding");
Console.WriteLine($"PASS: {assertions} offline loading assertions; Unity texture decoding is stubbed.");
