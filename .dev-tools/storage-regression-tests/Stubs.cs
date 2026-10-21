namespace UnityEngine
{
    internal static class Application { internal static string persistentDataPath; }
    internal static class Debug
    {
        internal static readonly List<string> Errors = new();
        internal static void LogError(object message) => Errors.Add(message.ToString());
        internal static void LogWarning(object message) { }
    }
}
namespace MySimulatedLongevityRoad.Core
{
    internal static class MclslDiagnostics { internal static void Error(string key, string text) { } }
}
