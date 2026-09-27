using System.Reflection;

namespace HarmonyLib
{
    public sealed class Harmony
    {
        public PatchProcessor CreateProcessor(MethodInfo original) => new();
    }

    public sealed class PatchProcessor
    {
        public void AddPrefix(MethodInfo method) { }
        public void AddPostfix(MethodInfo method) { }
        public void AddFinalizer(MethodInfo method) { }
        public void Patch() { }
    }
}

namespace MySimulatedLongevityRoad.Core
{
    internal static class MclslDiagnostics
    {
        internal static readonly List<string> Errors = new();
        internal static void Error(string key, string message) => Errors.Add(message);
    }
}
