using MySimulatedLongevityRoad.Core;
using System.Text.Json;

string directory = Path.Combine(Path.GetTempPath(), "mclsl-atomic-" + Guid.NewGuid().ToString("N"));
Directory.CreateDirectory(directory);
string path = Path.Combine(directory, "state.json");
int checks = 0;
void Check(bool condition)
{
    if (!condition) throw new Exception("Atomic file assertion " + (checks + 1));
    checks++;
}
Dictionary<string, int> Parse(string json) => JsonSerializer.Deserialize<Dictionary<string, int>>(json);
try
{
    Check(MclslAtomicFile.Read(path, Parse, out _) == MclslArchiveReadResult.Missing);
    MclslAtomicFile.Write(path, "{\"value\":1}");
    MclslAtomicFile.Write(path, "{\"value\":2}");
    Check(MclslAtomicFile.Read(path, Parse, out var primary) == MclslArchiveReadResult.Primary && primary["value"] == 2);
    File.WriteAllText(path, "broken");
    Check(MclslAtomicFile.Read(path, Parse, out var recovered) == MclslArchiveReadResult.Backup && recovered["value"] == 1);
    MclslAtomicFile.Write(path, "{\"value\":3}", preserveBackup: true);
    Check(Parse(File.ReadAllText(path + ".bak"))["value"] == 1);
    using (var held = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.None))
    {
        bool failed = false;
        try { MclslAtomicFile.Write(path, "{\"value\":4}"); }
        catch (IOException) { failed = true; }
        Check(failed);
    }
    Check(Parse(File.ReadAllText(path))["value"] == 3);
    Check(Directory.GetFiles(directory, "*.tmp").Length == 0);
    File.WriteAllText(path, "broken");
    File.WriteAllText(path + ".bak", "broken");
    Check(MclslAtomicFile.Read(path, Parse, out _) == MclslArchiveReadResult.Invalid);
    Console.WriteLine(JsonSerializer.Serialize(new { checks, actualFileSystem = true, unityTest = false }));
}
finally
{
    foreach (string file in Directory.GetFiles(directory)) File.Delete(file);
    Directory.Delete(directory);
}
