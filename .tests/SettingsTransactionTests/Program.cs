using System;
using System.IO;
using System.Linq;
using MySimulatedLongevityRoad.Core;
using MySimulatedLongevityRoad.Systems;
using MySimulatedLongevityRoad.UI;
using NeoModLoader.api;
using Newtonsoft.Json.Linq;
static class Program
{
    static int checks;
    static string directory, defaults, target;
    static void Check(bool ok, string name) { checks++; if (!ok) throw new Exception(name); }
    static ModConfig Fresh()
    {
        var config = new ModConfig();
        foreach (string id in new[] { "enable_core", "show_fps", "beast_natural_spawn", "beast_waves", "spell_effects_enabled", "beast_talent_effects_enabled" })
            config.AddConfigItem("ConfigItems", "MCLSL_config_" + id, ConfigItemType.SWITCH, true, null, "");
        config.AddConfigItem("ConfigItems", "MCLSL_config_beast_wave_interval", ConfigItemType.INT_SLIDER, 17, null, "");
        MclslRuntimeSettings.LoadFromModConfig(config); MclslBeastConfig.Load(config); MclslFpsOverlay.SetVisible(true);
        MclslSpellEffect.Applies = 0; MclslSpellEffect.Fail = false; MclslSpellEffect.BeforeApply = null;
        var items = new JArray(config["ConfigItems"].Values.Select(i => new JObject { ["Id"] = i.Id, ["Type"] = i.Type.ToString(), ["BoolVal"] = false, ["IntVal"] = 5 }));
        File.WriteAllText(defaults, new JObject { ["ConfigItems"] = items }.ToString()); File.WriteAllText(target, "original-file");
        return config;
    }
    static bool Restore(ModConfig c) => MclslSettingsReset.Restore(c, defaults, target);
    static void Intact(ModConfig c, string scenario)
    {
        Check(c["ConfigItems"].Values.Where(i => i.Type == ConfigItemType.SWITCH).All(i => i.BoolVal), scenario + " config switches restored");
        Check(c["ConfigItems"]["MCLSL_config_beast_wave_interval"].IntVal == 17, scenario + " config slider restored");
        Check(MclslRuntimeSettings.CoreEnabled && MclslRuntimeSettings.ShowFps && MclslFpsOverlay.Visible, scenario + " ordinary runtime unchanged");
        Check(MclslBeastConfig.NaturalSpawnEnabled && MclslBeastWorld.Enabled && MclslBeastWorld.Interval == 17, scenario + " beast runtime unchanged");
        Check(MclslSpellEffect.Enabled && MclslSpellEffect.TalentEnabled && MclslSpellEffect.Applies == 0, scenario + " effects untouched");
        Check(File.ReadAllText(target) == "original-file" && Directory.GetFiles(directory, "*.tmp").Length == 0, scenario + " file preserved, temporary cleaned");
    }
    static void Main()
    {
        directory = Path.Combine(Path.GetTempPath(), "mclsl-settings-" + Guid.NewGuid().ToString("N")); Directory.CreateDirectory(directory);
        defaults = Path.Combine(directory, "defaults.json"); target = Path.Combine(directory, "current.config");
        try
        {
            foreach (string invalid in new[] { "missing", "duplicate", "unknown", "type", "incomplete", "null" })
            {
                var c = Fresh(); var json = JObject.Parse(File.ReadAllText(defaults)); var items = (JArray)json["ConfigItems"];
                switch (invalid)
                {
                    case "missing": File.Delete(defaults); break;
                    case "duplicate": items.Add(items[0].DeepClone()); break;
                    case "unknown": items[0]["Id"] = "unknown"; break;
                    case "type": items[0]["Type"] = "TEXT"; break;
                    case "incomplete": items.RemoveAt(0); break;
                    case "null": items[0]["BoolVal"] = JValue.CreateNull(); break;
                }
                if (invalid != "missing") File.WriteAllText(defaults, json.ToString());
                Check(!Restore(c), invalid + " rejected"); Intact(c, invalid);
            }
            var config = Fresh(); config["ConfigItems"]["MCLSL_config_show_fps"].FailSet = true;
            Check(!Restore(config), "partial setter failure"); Intact(config, "setter failure");
            config = Fresh(); config["ConfigItems"]["MCLSL_config_beast_wave_interval"].Clamp = true;
            Check(!Restore(config) && MclslSpellEffect.Applies == 0 && File.ReadAllText(target) == "original-file", "clamped value rejected before commit"); Intact(config, "clamped setter");
            config = Fresh(); config.FailSave = true;
            Check(!Restore(config), "save failure"); Intact(config, "save failure");
            config = Fresh(); File.Delete(target); Directory.CreateDirectory(target);
            Check(!Restore(config) && MclslSpellEffect.Applies == 0 && Directory.GetFiles(directory, "*.tmp").Length == 0, "commit failure has no runtime effects");
            Directory.Delete(target);
            config = Fresh();
            MclslSpellEffect.BeforeApply = () => Check(File.ReadAllText(target) != "original-file", "file committed before effects");
            Check(Restore(config), "existing file replacement succeeds");
            Check(!MclslRuntimeSettings.CoreEnabled && !MclslRuntimeSettings.ShowFps && !MclslFpsOverlay.Visible, "ordinary runtime applied");
            Check(!MclslBeastConfig.NaturalSpawnEnabled && !MclslBeastWorld.Enabled && MclslBeastWorld.Interval == 5 && !MclslSpellEffect.Enabled && !MclslSpellEffect.TalentEnabled, "beast/effects applied");
            Check(config.Saves == 1 && Directory.GetFiles(directory, "*.tmp").Length == 0, "single save without migration writes");
            config = Fresh(); File.Delete(target); Check(Restore(config) && File.Exists(target), "first config commit succeeds");
            config = Fresh(); config.AddConfigItem("ConfigItems", "legacy_beast", ConfigItemType.SWITCH, true, null, "");
            var snapshot = MclslBeastConfig.Read(config);
            Check(config.Saves == 0 && config["ConfigItems"].ContainsKey("legacy_beast") && MclslSpellEffect.Applies == 0, "read is side-effect free");
            MclslBeastConfig.Load(config); Check(config.Saves == 0 && config["ConfigItems"].ContainsKey("legacy_beast"), "load no longer migrates or saves");
            config = Fresh(); MclslSpellEffect.Fail = true;
            Check(!Restore(config) && !config["ConfigItems"]["MCLSL_config_enable_core"].BoolVal && File.ReadAllText(target) != "original-file", "postcommit failure keeps durable config consistent");
            Check(MclslAnnouncementSystem.Last.Contains("已保存"), "postcommit failure reports saved state accurately");
            Console.WriteLine("PASS settings transaction: " + checks + " checks");
        }
        finally { Directory.Delete(directory, true); }
    }
}
