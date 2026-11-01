using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using MySimulatedLongevityRoad.Core;
using MySimulatedLongevityRoad.Systems;
using NeoModLoader.api;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace MySimulatedLongevityRoad.UI;

internal static class MclslSettingsReset
{
    internal static bool Restore()
    {
        if (MySimulatedLongevityRoad.MclslMod.GetModConfigSafe() is not ModConfig config) return false;
        var original = new List<(ModConfigItem Item, object Value)>();
        string stage = "定位默认配置";
        string pendingFile = null;
        bool valuesApplied = false;
        try
        {
            string path = Path.Combine(FindModDirectory(), "default_config.json");
            if (!File.Exists(path)) throw new FileNotFoundException("未找到 default_config.json", path);
            stage = "校验设置";
            JArray items = JObject.Parse(File.ReadAllText(path))["ConfigItems"] as JArray
                ?? throw new InvalidDataException("默认设置缺少 ConfigItems");
            var staged = new List<(ModConfigItem Item, object Value)>();
            var seen = new HashSet<string>(StringComparer.Ordinal);
            foreach (JObject source in items)
            {
                string id = (string)source["Id"];
                if (string.IsNullOrWhiteSpace(id) || !seen.Add(id))
                    throw new InvalidDataException("默认设置存在空白或重复 ID");
                if (!config["ConfigItems"].TryGetValue(id, out ModConfigItem target))
                    throw new InvalidDataException("默认设置含未知配置项: " + id);
                if (!string.Equals((string)source["Type"], target.Type.ToString(), StringComparison.Ordinal))
                    throw new InvalidDataException("默认设置类型不匹配: " + id);
                string field = target.Type switch
                {
                    ConfigItemType.SWITCH => "BoolVal",
                    ConfigItemType.INT_SLIDER or ConfigItemType.SELECT => "IntVal",
                    ConfigItemType.SLIDER => "FloatVal",
                    ConfigItemType.TEXT => "TextVal",
                    _ => null
                };
                if (field == null || source[field] == null || source[field].Type == JTokenType.Null)
                    throw new InvalidDataException("默认设置缺少有效值: " + id);
                object value = target.Type switch
                {
                    ConfigItemType.SWITCH => source[field].ToObject<bool>(),
                    ConfigItemType.INT_SLIDER or ConfigItemType.SELECT => source[field].ToObject<int>(),
                    ConfigItemType.SLIDER => source[field].ToObject<float>(),
                    ConfigItemType.TEXT => source[field].ToObject<string>(),
                    _ => null
                };
                object before = ReadValue(target, field);
                if (value == null || before == null) throw new InvalidDataException("无法读取设置项: " + id);
                staged.Add((target, value));
                original.Add((target, before));
            }
            if (staged.Count == 0 || staged.Count != config["ConfigItems"].Count)
                throw new InvalidDataException("默认设置未覆盖全部当前配置项");
            stage = "写入设置";
            valuesApplied = true;
            foreach (var entry in staged) entry.Item.SetValue(entry.Value, false);
            foreach (var entry in staged)
            {
                string field = entry.Item.Type switch
                {
                    ConfigItemType.SWITCH => "BoolVal",
                    ConfigItemType.INT_SLIDER or ConfigItemType.SELECT => "IntVal",
                    ConfigItemType.SLIDER => "FloatVal",
                    _ => "TextVal"
                };
                if (!Equals(ReadValue(entry.Item, field), entry.Value))
                    throw new InvalidDataException("设置项写入后校验失败");
            }
            MclslRuntimeSettings.LoadFromModConfig(config);
            MclslBeastConfig.Load(config);
            MclslFpsOverlay.SetVisible(MclslRuntimeSettings.ShowFps);
            stage = "保存配置";
            string configDirectory = Path.Combine(Application.persistentDataPath, "mods_config");
            Directory.CreateDirectory(configDirectory);
            string configFile = Path.Combine(configDirectory, "MY_SIMULATED_LONGEVITY_ROAD.config");
            pendingFile = Path.Combine(configDirectory, "MY_SIMULATED_LONGEVITY_ROAD.config." + Guid.NewGuid().ToString("N") + ".tmp");
            config.Save(pendingFile);
            if (File.Exists(configFile)) File.Replace(pendingFile, configFile, null);
            else File.Move(pendingFile, configFile);
            pendingFile = null;
        }
        catch (Exception ex)
        {
            if (valuesApplied) foreach (var entry in original)
            {
                try { entry.Item.SetValue(entry.Value, false); }
                catch (Exception rollbackError) { MclslDiagnostics.Error("settings-reset-rollback", rollbackError.Message); }
            }
            if (pendingFile != null) try { if (File.Exists(pendingFile)) File.Delete(pendingFile); }
                catch (Exception cleanupError) { MclslDiagnostics.Error("settings-reset-cleanup", cleanupError.Message); }
            MclslRuntimeSettings.LoadFromModConfig(config);
            MclslFpsOverlay.SetVisible(MclslRuntimeSettings.ShowFps);
            MclslDiagnostics.Error("settings-reset", stage + "：" + ex.GetType().Name + "：" + ex.Message);
            MclslAnnouncementSystem.Enqueue("恢复默认设置失败，请查看模组日志。", "#D79A8E", 6f, 1);
            return false;
        }
        MclslAnnouncementSystem.Enqueue("模拟长生路设置已恢复默认。", "#9FD8BE", 5f, 1);
        return true;
    }

    private static string FindModDirectory()
    {
        string gameRoot = Path.GetDirectoryName(Application.dataPath);
        if (string.IsNullOrWhiteSpace(gameRoot)) throw new InvalidDataException("无法定位游戏目录");
        string modsRoot = Path.Combine(gameRoot, "Mods");
        if (!Directory.Exists(modsRoot)) throw new DirectoryNotFoundException("未找到 Mods 目录");
        foreach (string directory in Directory.EnumerateDirectories(modsRoot))
        {
            string manifest = Path.Combine(directory, "mod.json");
            if (!File.Exists(manifest)) continue;
            try
            {
                JObject metadata = JObject.Parse(File.ReadAllText(manifest));
                if (string.Equals((string)metadata["GUID"], "MY_SIMULATED_LONGEVITY_ROAD", StringComparison.Ordinal))
                    return directory;
            }
            catch (Exception ex) { MclslDiagnostics.Error("settings-reset-manifest", directory + "：" + ex.Message); }
        }
        throw new DirectoryNotFoundException("未找到模拟长生路安装目录");
    }

    private static object ReadValue(ModConfigItem item, string name)
    {
        const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
        Type type = item.GetType();
        return type.GetField(name, flags)?.GetValue(item)
            ?? type.GetProperty(name, flags)?.GetValue(item);
    }
}
