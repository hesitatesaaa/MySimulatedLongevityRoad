using MySimulatedLongevityRoad.Systems;
using MySimulatedLongevityRoad.Data;
using Newtonsoft.Json;

string root = Environment.GetEnvironmentVariable("MCLSL_STORAGE_FIXTURE")
    ?? Path.Combine(Directory.GetCurrentDirectory(), "build/storage-regression-" + Guid.NewGuid().ToString("N"));
Directory.CreateDirectory(root);
int checks = 0;
void Check(bool value, string label)
{
    if (!value) throw new Exception("FAILED: " + label);
    checks++;
}
string PathFor(string scenario)
{
    string folder = Path.Combine(root, scenario);
    string state = Path.Combine(folder, "MySimulatedLongevityRoad/ArchitectureV1/HuanzhenState.json");
    Directory.CreateDirectory(Path.GetDirectoryName(state)!);
    MclslHuanzhenSystem.Reset(folder);
    return state;
}
string path = PathFor("missing");
File.WriteAllText(path + ".tmp", "uncommitted evidence");
MclslHuanzhenSystem.EnsureLoaded();
Check(!MclslHuanzhenSystem.Locked && !File.Exists(path), "missing state starts fresh without promoting temp");
Check(File.ReadAllText(path + ".tmp") == "uncommitted evidence", "uncommitted evidence is preserved");

path = PathFor("invalid");
File.WriteAllText(path, "broken primary");
File.WriteAllText(path + ".bak", "broken backup");
MclslHuanzhenSystem.EnsureLoaded();
int errors = UnityEngine.Debug.Errors.Count;
Check(MclslHuanzhenSystem.Locked && MclslHuanzhenSystem.Failure.Contains("Json"), "parse failure retains its cause");
for (int i=0;i<8;i++) MclslHuanzhenSystem.PrepareForNewWorld();
Check(UnityEngine.Debug.Errors.Count == errors, "new-world requests do not throw or spam storage errors");
Check(File.ReadAllText(path) == "broken primary" && File.ReadAllText(path + ".bak") == "broken backup", "invalid generations are never overwritten");

path = PathFor("version");
File.WriteAllText(path, "{\"Version\":999}");
MclslHuanzhenSystem.EnsureLoaded();
Check(MclslHuanzhenSystem.Locked && MclslHuanzhenSystem.Failure.Contains("格式"), "unsupported version is diagnosed");

path = PathFor("backup");
File.WriteAllText(path, "broken");
string backup = JsonConvert.SerializeObject(new MclslHuanzhenExternalState { SpaceEssenceBase=200 });
File.WriteAllText(path + ".bak", backup);
MclslHuanzhenSystem.EnsureLoaded();
Check(!MclslHuanzhenSystem.Locked && MclslHuanzhenSystem.State.SpaceEssenceBase == 200, "valid backup restores balance");
MclslHuanzhenSystem.State.SpaceEssenceBase = 250;
Check(MclslHuanzhenSystem.Save() && File.ReadAllText(path + ".bak") == backup, "recovery commit preserves the good backup");

path = PathFor("write-failure");
MclslHuanzhenSystem.EnsureLoaded();
MclslHuanzhenSystem.State.SpaceEssenceBase = 200;
Check(MclslHuanzhenSystem.Save(), "initial checkpoint commits");
using (var held = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.None))
{
    MclslHuanzhenSystem.State.SpaceEssenceBase -= 80;
    Check(!MclslHuanzhenSystem.Save(), "failed persistence is reported to the caller");
    Check(MclslHuanzhenSystem.State.SpaceEssenceBase == 200 && MclslHuanzhenSystem.Locked, "failed debit rolls back and disables further operations");
}
Check(JsonConvert.DeserializeObject<MclslHuanzhenExternalState>(File.ReadAllText(path))!.SpaceEssenceBase == 200, "committed balance is intact");

path = PathFor("new-world-transaction");
MclslHuanzhenSystem.EnsureLoaded();
string anchor = Path.Combine(Path.GetDirectoryName(path)!, "HuanzhenAnchors/mclsl_huanzhen_test_1");
Directory.CreateDirectory(anchor);
File.WriteAllText(Path.Combine(anchor, "world.wbox"), "original anchor");
MclslHuanzhenSystem.State.Anchors.Add(new() { RelativeSavePath="mclsl_huanzhen_test_1" });
MclslHuanzhenSystem.State.WorldRunId="original";
Check(MclslHuanzhenSystem.Save(), "anchor metadata commits");
using (var held = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.None))
    MclslHuanzhenSystem.PrepareForNewWorld();
Check(Directory.Exists(anchor) && MclslHuanzhenSystem.State.WorldRunId == "original", "new-world failure preserves old anchors and state");
Check(MclslHuanzhenSystem.State.Anchors.Count == 1, "failed reset does not lose anchor metadata");

path = PathFor("unreadable");
Directory.CreateDirectory(path);
MclslHuanzhenSystem.EnsureLoaded();
Check(MclslHuanzhenSystem.Locked && !string.IsNullOrEmpty(MclslHuanzhenSystem.Failure), "unreadable path is not treated as missing");
Console.WriteLine($"PASS: {checks} storage assertions using production method bodies and real filesystem; Unity stubbed.");
