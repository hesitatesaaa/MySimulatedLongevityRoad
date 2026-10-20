namespace MySimulatedLongevityRoad
{
    internal sealed class MclslMod
    {
        internal static readonly MclslMod I = new();
        internal static string Root;
        internal Declaration GetDeclaration() => new() { FolderPath = Root };
        internal sealed class Declaration { internal string FolderPath; }
    }
}
namespace MySimulatedLongevityRoad.Core
{
    internal static class MclslRuntimeChanges
    {
        internal static readonly int MainThreadId = System.Threading.Thread.CurrentThread.ManagedThreadId;
        internal static bool IsMainThread => System.Threading.Thread.CurrentThread.ManagedThreadId == MainThreadId;
    }
    internal static class MclslDiagnostics
    {
        internal static readonly System.Collections.Generic.List<string> Errors = new();
        internal static void Error(string key, string message) => Errors.Add(key + ": " + message);
        internal static void Once(string key, string message) => Errors.Add(key + ": " + message);
    }
    internal static class MclslUnityProfiler
    {
        internal static Scope Sample(string name) => default;
        internal readonly struct Scope : System.IDisposable { public void Dispose() { } }
    }
}

internal sealed class World
{
    internal static World world;
    internal double Time;
    internal int Reads;
    internal double getCurWorldTime()
    {
        if (!MySimulatedLongevityRoad.Core.MclslRuntimeChanges.IsMainThread)
            throw new System.InvalidOperationException("Worker queried world clock");
        Reads++;
        return Time;
    }
}
