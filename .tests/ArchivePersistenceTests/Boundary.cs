using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

class SaveCustomData
{
    public readonly Dictionary<string, string> Values = new();
    public readonly List<string> Writes = new();
    public string FailRead, FailWrite;
    public void get(string key, out string value, string fallback)
    {
        if (key == FailRead) throw new InvalidOperationException("read injected");
        value = Values.TryGetValue(key, out var found) ? found : fallback;
    }
    public void set(string key, string value)
    {
        if (key == FailWrite) throw new InvalidOperationException("write injected");
        Values[key] = value; Writes.Add(key);
    }
}
class MapStats { public SaveCustomData custom_data; }
class MapBox
{
    public static bool FailSeed;
    public static int current_world_seed_id => FailSeed ? throw new InvalidOperationException("identity injected") : 1;
    public MapStats map_stats = new();
}
static class World { public static MapBox world; }
namespace MySimulatedLongevityRoad.Core
{
    static class MclslWorldContext { public static long Generation; }
    static class MclslDiagnostics
    {
        public static void Log(string k, string m) { }
        public static void Alert(string k, string m) { }
        public static void Fatal(string k, string m) { }
    }
    static class MclslPerformanceProbe
    {
        public static int Open;
        public static long Begin() { Open++; return 1; }
        public static void End(string s, long n) { Open--; }
        public static void RecordArchiveBytes(int a, int b) { }
    }
}
namespace MySimulatedLongevityRoad.Data
{
    class MclslWorldRunState { public string RunId; }
    class MclslWorldArchiveBundle
    {
        public int Version = 1; public MclslWorldRunState CurrentRun;
        public static int Serializations, Deserializations;
        [OnSerializing] void Serializing(StreamingContext context) => Serializations++;
        [OnDeserializing] void Deserializing(StreamingContext context) => Deserializations++;
    }
    static class MclslSaveVersions { public const int WorldArchive = 1; }
}
namespace MySimulatedLongevityRoad.Systems
{
    // Repository is an import/export boundary here, not a second persistence implementation.
    static class MclslWorldRunRepository
    {
        public static Data.MclslWorldRunState Current;
        public static bool FailExport;
        public static int Imports;
        public static void ResetWorld() => Current = new() { RunId = "fresh" };
        public static bool ImportArchive(Data.MclslWorldArchiveBundle b)
        {
            if (b.Version != 1 || string.IsNullOrWhiteSpace(b.CurrentRun?.RunId)) throw new InvalidOperationException("invalid archive");
            Current = b.CurrentRun; Imports++; return true;
        }
        public static Data.MclslWorldArchiveBundle ExportArchive()
            => FailExport ? throw new InvalidOperationException("export injected") : new() { CurrentRun = Current };
    }
}
