using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using MySimulatedLongevityRoad.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace MySimulatedLongevityRoad.Systems;

internal static class MclslLocalizationBridge
{
    private static readonly Dictionary<string, string> RuntimeKeys = new(StringComparer.Ordinal);
    private static readonly Dictionary<string, string> TextByRuntimeKey = new(StringComparer.Ordinal);
    private static readonly HashSet<string> RegisteredKeys = new(StringComparer.Ordinal);
    private static readonly FieldInfo ActiveTextField = typeof(LocalizedTextManager).GetField(
        "_localized_text", BindingFlags.Instance | BindingFlags.NonPublic);
    private static bool _chineseLoaded;

    internal static void LoadChineseCatalog()
    {
        if (_chineseLoaded) return;
        string json = FindChineseCatalog();
        if (string.IsNullOrWhiteSpace(json))
        {
            Debug.LogWarning("[模拟长生路] 未找到 Locales/ch.json，界面将使用已注册的运行时中文。");
            return;
        }
        try
        {
            Dictionary<string, string> entries = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
            if (entries == null || !entries.ContainsKey("mclsl_mod_tab")) throw new InvalidDataException("缺少模组入口键");
            foreach (KeyValuePair<string, string> entry in entries) RegisterKey(entry.Key, entry.Value);
            _chineseLoaded = true;
        }
        catch (Exception ex) { Debug.LogWarning("[模拟长生路] 简体中文目录加载失败: " + ex); }
    }

    private static string FindChineseCatalog()
    {
        try
        {
            string gameRoot = Path.GetDirectoryName(Application.dataPath);
            if (!string.IsNullOrWhiteSpace(gameRoot))
            {
                string modsRoot = Path.Combine(gameRoot, "Mods");
                if (Directory.Exists(modsRoot))
                {
                    foreach (string directory in Directory.EnumerateDirectories(modsRoot))
                    {
                        string manifest = Path.Combine(directory, "mod.json");
                        string locale = Path.Combine(directory, "Locales", "ch.json");
                        if (!File.Exists(manifest) || !File.Exists(locale)) continue;
                        JObject metadata = JObject.Parse(File.ReadAllText(manifest));
                        if (!string.Equals((string)metadata["GUID"], "MY_SIMULATED_LONGEVITY_ROAD", StringComparison.Ordinal)) continue;
                        return File.ReadAllText(locale);
                    }
                }
            }
            foreach (TextAsset asset in Resources.LoadAll<TextAsset>("locales/ch"))
            {
                if (asset?.text != null && asset.text.Contains("\"mclsl_mod_tab\"")) return asset.text;
            }
        }
        catch (Exception ex) { Debug.LogWarning("[模拟长生路] 定位简体中文目录失败: " + ex.Message); }
        return string.Empty;
    }

    internal static string RuntimeText(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return string.Empty;
        string value = text.Trim();
        if (RuntimeKeys.TryGetValue(value, out string cachedKey))
        {
            EnsureRegistered(cachedKey, value);
            return cachedKey;
        }
        // WorldBox normalizes keys when adding them. Keep generated keys in that
        // form so a tooltip lookup uses the exact key that was registered.
        string baseKey = "mclsl_runtime_" + StableHash(value).ToString("x8");
        string key = baseKey;
        int collision = 0;
        while (TextByRuntimeKey.TryGetValue(key, out string existing) && existing != value)
            key = baseKey + "_" + (++collision);
        TextByRuntimeKey[key] = value;
        EnsureRegistered(key, value);
        RuntimeKeys[value] = key;
        return key;
    }

    internal static void Register(params string[] texts)
    {
        if (texts == null) return;
        for (int i = 0; i < texts.Length; i++) RuntimeText(texts[i]);
    }

    internal static string RegisterKey(string key, string text)
    {
        if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(text)) return string.Empty;
        key = key.Trim().Underscore();
        text = text.Trim();
        TextByRuntimeKey[key] = text;
        EnsureRegistered(key, text);
        return key;
    }

    internal static void ReapplyAfterLanguageChange()
    {
        LoadChineseCatalog();
        RegisteredKeys.Clear();
        foreach (KeyValuePair<string, string> pair in TextByRuntimeKey)
            EnsureRegistered(pair.Key, pair.Value);
        LocalizedTextManager.updateTexts();
    }

    internal static void NormalizeLookupKey(ref string key)
    {
        if (string.IsNullOrWhiteSpace(key)) return;
        string normalized = key.Underscore();
        if (TextByRuntimeKey.ContainsKey(normalized)) key = normalized;
    }

    internal static bool ShouldSuppressMissingTextLog(object message)
    {
        string text = message?.ToString() ?? string.Empty;
        const string marker = "LocalizedTextManager: missing text:";
        int index = text.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
        if (index < 0) return false;
        string key = text.Substring(index + marker.Length).Trim();
        if (key.Length == 0) return false;
        return key.StartsWith("mclsl_", StringComparison.OrdinalIgnoreCase)
            || key.StartsWith("MCLSL_", StringComparison.Ordinal)
            || key.StartsWith("trait_Mclsl", StringComparison.OrdinalIgnoreCase)
            || key.StartsWith("Mclsl", StringComparison.Ordinal);
    }

    internal static bool IsRuntimeKey(string value)
    {
        return !string.IsNullOrWhiteSpace(value)
            && value.Trim().StartsWith("mclsl_runtime_", StringComparison.OrdinalIgnoreCase);
    }

    internal static bool TryResolveRuntimeKey(string key, out string text)
    {
        text = string.Empty;
        if (string.IsNullOrWhiteSpace(key)) return false;
        return TextByRuntimeKey.TryGetValue(key.Trim().Underscore(), out text)
            && !string.IsNullOrWhiteSpace(text);
    }

    internal static bool TryGetActiveText(string key, out string text)
    {
        text = string.Empty;
        if (string.IsNullOrWhiteSpace(key) || LocalizedTextManager.instance == null
            || ActiveTextField == null) return false;
        try
        {
            return ActiveTextField.GetValue(LocalizedTextManager.instance) is Dictionary<string, string> active
                && active.TryGetValue(key.Trim().Underscore(), out text);
        }
        catch { return false; }
    }

    private static void EnsureRegistered(string key, string text)
    {
        if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(text)) return;
        key = key.Trim();
        text = text.Trim();
        if (RegisteredKeys.Contains(key)) return;
        try
        {
            LocalizedTextManager.add(key, text, true, string.Empty, true);
            if (TryGetActiveText(key, out string registered) && registered == text)
                RegisteredKeys.Add(key);
            else MclslDiagnostics.Error("locale-register", "简体中文键注册后未出现在当前语言表；将使用模组文本兜底。");
        }
        catch
        {
            RegisteredKeys.Remove(key);
            MclslDiagnostics.Error("locale-register", "简体中文键注册失败；将使用模组文本兜底。");
        }
    }

    private static uint StableHash(string value)
    {
        unchecked
        {
            const uint offset = 2166136261u;
            const uint prime = 16777619u;
            uint hash = offset;
            for (int i = 0; i < value.Length; i++)
            {
                hash ^= value[i];
                hash *= prime;
            }
            return hash;
        }
    }
}
