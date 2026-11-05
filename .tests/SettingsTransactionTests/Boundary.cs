using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
namespace NeoModLoader.api
{
    public enum ConfigItemType { SWITCH, INT_SLIDER, SELECT, SLIDER, TEXT }
    public class ModConfigItem
    {
        public string Id;
        public ConfigItemType Type;
        public bool BoolVal;
        public int IntVal;
        public float FloatVal;
        public string TextVal = "";
        [JsonIgnore] public bool FailSet, Clamp;
        public void SetValue(object value, bool callback)
        {
            if (callback) throw new Exception("callbacks must be suppressed");
            if (FailSet) throw new InvalidOperationException("setter injected");
            switch (Type)
            {
                case ConfigItemType.SWITCH: BoolVal = (bool)value; break;
                case ConfigItemType.INT_SLIDER: case ConfigItemType.SELECT: IntVal = Clamp && (int)value == 5 ? 1 : (int)value; break;
                case ConfigItemType.SLIDER: FloatVal = (float)value; break;
                case ConfigItemType.TEXT: TextVal = (string)value; break;
            }
        }
        public void SetIntRange(int min, int max) { }
    }
    public class ModConfig
    {
        readonly Dictionary<string, ModConfigItem> items = new();
        public Dictionary<string, ModConfigItem> this[string key] => items;
        public bool FailSave;
        public int Saves;
        public ModConfigItem AddConfigItem(string section, string id, ConfigItemType type, object value, object x, string callback)
        {
            var item = new ModConfigItem { Id = id, Type = type }; item.SetValue(value, false); items.Add(id, item); return item;
        }
        public void Save(string path)
        {
            Saves++;
            if (path != null) File.WriteAllText(path, JsonConvert.SerializeObject(new { ConfigItems = items.Values }));
            if (FailSave) throw new IOException("save injected after temporary write");
        }
    }
}
namespace UnityEngine { static class Application { public static string dataPath = ""; public static string persistentDataPath = ""; } }
namespace MySimulatedLongevityRoad
{
    static class MclslMod { public static object GetModConfigSafe() => null; }
}
namespace MySimulatedLongevityRoad.Core
{
    static class MclslDiagnostics
    {
        public static void Error(string k, string s) { }
        public static void Alert(string k, string s) { }
        public static void Fatal(string k, string s) { }
        public static void Probe(string k, string s) { }
    }
    static class MclslFpsOverlay { public static bool Visible; public static void SetVisible(bool value) => Visible = value; }
}
namespace MySimulatedLongevityRoad.Data
{
    class MclslBeastDefinition { public string ConfigId; public int ChancePerMillion = 0; }
    static class MclslBeastCatalog
    {
        public static MclslBeastDefinition[] All = { new() { ConfigId = "legacy_beast" } };
        public static MclslBeastDefinition[] Named = Array.Empty<MclslBeastDefinition>();
    }
}
namespace MySimulatedLongevityRoad.Systems
{
    static class MclslBeastWorld
    {
        public static bool Enabled;
        public static int Interval, MaximumWaveCount, MaximumLevel, AutomaticLimit, NaturalLimit;
        public static float CountMultiplier;
    }
    static class MclslSpellEffect
    {
        public static bool Enabled, TalentEnabled, Fail;
        public static int Applies;
        public static Action BeforeApply;
        public static void SetEnabled(bool value) { BeforeApply?.Invoke(); Applies++; if (Fail) throw new Exception("effect injected"); Enabled = value; }
        public static void SetTalentEnabled(bool value) { Applies++; TalentEnabled = value; }
    }
    static class MclslLocalizationBridge { public static void RegisterKey(string key, string value) { } }
    static class MclslAnnouncementSystem
    {
        public static string Last;
        public static void Enqueue(string text, string color, float duration, int priority) => Last = text;
    }
}
