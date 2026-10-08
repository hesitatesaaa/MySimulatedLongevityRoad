using MySimulatedLongevityRoad.Core;
using MySimulatedLongevityRoad.Data;
using MySimulatedLongevityRoad.Systems;

internal static class HistoryTests
{
    internal static void Run()
    {
        static void Check(bool ok, string message) { if (!ok) throw new Exception(message); }
        MclslRunEventRecord record = new() { EventType = "sect_lifecycle_rise", Importance = 4,
            Category = "ruin", Year = 863, Title = "寒泉道统渐兴", Body = "传承渐广" };
        MclslRuntimeSettings.LineageNativeHistoryEnabled = false;
        Check(!MclslNativeHistoryBridge.Add(record) && WorldLogMessageExtensions.Messages.Count == 0,
            "disabled native lineage history must not write any message");
        MclslRuntimeSettings.LineageNativeHistoryEnabled = true;
        Check(WorldLogMessageExtensions.Messages.Count == 0, "enabling alone must not replay history");
        Check(MclslNativeHistoryBridge.Add(record), "enabled lineage history must write eligible event");
        WorldLogMessage added = WorldLogMessageExtensions.Messages.Single();
        Check(added.asset_id == "mclsl_history_lineage" && added.timestamp / 60 + 1 == 863,
            "native bridge must fix classification and convert years to timestamps");
        MclslRuntimeSettings.LineageNativeHistoryEnabled = false;
        Check(WorldLogMessageExtensions.Messages.Count == 1, "disabling must retain existing history");
        MclslNativeHistoryBridge.BeginLegacyClassification(MclslWorldRunRepository.Current);
        record.NativeLogged = true;
        MclslNativeHistoryBridge.RegisterLegacyClassification(record);
        WorldLogMessage legacy = new() { asset_id = "mclsl_history_ruin", timestamp = 863,
            special1 = "宗门遗迹", special2 = record.Title, special3 = record.Body };
        string text = "";
        AssetManager.world_log_library.get(legacy.asset_id).text_replacer(legacy, ref text);
        Check(text.StartsWith("[玄黄·仙道传承]") && legacy.timestamp / 60 + 1 == 863,
            "exact retained legacy record must display correct lineage classification");
        WorldLogMessage unrelated = new() { asset_id = "mclsl_history_ruin", timestamp = 863,
            special1 = "宗门遗迹", special2 = record.Title, special3 = "different body" };
        AssetManager.world_log_library.get(unrelated.asset_id).text_replacer(unrelated, ref text);
        Check(unrelated.asset_id == "mclsl_history_ruin", "title alone must not migrate unrelated history");
        MclslRuntimeSettings.LineageNativeHistoryEnabled = true;
        record.Importance = 2;
        Check(!MclslNativeHistoryBridge.Add(record), "switch must not bypass importance policy");
        Console.WriteLine("Production native history bridge: switch, no replay/deletion, classification, timestamp and exact legacy matching passed (native API stubbed).");
    }
}

internal sealed class WorldLogMessage
{
    internal string asset_id, special1, special2, special3;
    internal int timestamp;
}
internal delegate void TextReplacer(WorldLogMessage message, ref string text);
internal sealed class HistoryGroupAsset { internal string id, icon_path; }
internal sealed class WorldLogAsset
{
    internal string id, group, locale_id, path_icon;
    internal UnityEngine.Color color;
    internal TextReplacer text_replacer;
}
internal sealed class Library<T> where T : class
{
    private readonly Dictionary<string, T> _values = new();
    internal T get(string id) => _values.TryGetValue(id, out T value) ? value : null;
    internal void add(T value) => _values.Add(value is WorldLogAsset log ? log.id : ((HistoryGroupAsset)(object)value).id, value);
}
internal static class AssetManager
{
    internal static readonly Library<HistoryGroupAsset> history_groups = new();
    internal static readonly Library<WorldLogAsset> world_log_library = new();
}
internal static class WorldLogMessageExtensions
{
    internal static readonly List<WorldLogMessage> Messages = new();
    internal static void add(WorldLogMessage message) => Messages.Add(message);
}
namespace UnityEngine
{
    internal struct Color { internal Color(float r, float g, float b) { } }
    internal static class ColorUtility
    {
        internal static bool TryParseHtmlString(string value, out Color color) { color = default; return true; }
    }
    internal static class Debug { internal static void LogWarning(string message) => throw new Exception(message); }
}
namespace MySimulatedLongevityRoad.Core
{
    internal static class MclslRuntimeSettings { internal static bool LineageNativeHistoryEnabled = true; }
}
namespace MySimulatedLongevityRoad.Data
{
    internal sealed class MclslWorldRunState { }
    internal sealed class MclslRunEventRecord
    {
        internal string EventType, Category, Title, Body;
        internal int Year, Importance;
        internal bool NativeLogged;
    }
}
namespace MySimulatedLongevityRoad.Systems
{
    internal static class MclslWorldRunRepository { internal static MclslWorldRunState Current = new(); }
}
