using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using MySimulatedLongevityRoad.Core;
using Newtonsoft.Json;

namespace MySimulatedLongevityRoad.Systems;

internal static class MclslLocalizationBridge
{
    private static readonly Dictionary<string, string> TextByRuntimeKey = new(StringComparer.Ordinal);
    private static readonly HashSet<string> RegisteredKeys = new(StringComparer.Ordinal);
    private static readonly FieldInfo ActiveTextField = typeof(LocalizedTextManager).GetField(
        "_localized_text", BindingFlags.Instance | BindingFlags.NonPublic);
    private static bool _chineseLoaded;

    internal static void LoadChineseCatalog()
    {
        using var profileScope = MclslUnityProfiler.Sample("MCLS/Load/ChineseCatalog");
        if (_chineseLoaded) return;
        string file = Path.Combine(MclslMod.I.GetDeclaration().FolderPath, "Locales", "ch.json");
        Dictionary<string, string> entries = JsonConvert.DeserializeObject<Dictionary<string, string>>(
            File.ReadAllText(file));
        if (entries == null || !entries.ContainsKey("mclsl_mod_tab"))
            throw new InvalidDataException("简体中文目录缺少模组入口键: " + file);
        foreach (KeyValuePair<string, string> entry in entries)
        {
            if (string.IsNullOrWhiteSpace(entry.Value))
                throw new InvalidDataException("简体中文目录存在空翻译: " + entry.Key);
            RegisterKey(entry.Key, entry.Value);
        }
        _chineseLoaded = true;
    }

    internal static string RuntimeText(string text) => MclslTextValue.Encode(text);

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

    internal static bool IsRuntimeKey(string value)
    {
        return !string.IsNullOrWhiteSpace(value)
            && value.Trim().StartsWith("mclsl_runtime_", StringComparison.OrdinalIgnoreCase);
    }

    internal static bool TryResolveRuntimeKey(string key, out string text)
    {
        text = string.Empty;
        if (string.IsNullOrWhiteSpace(key)) return false;
        if (MclslTextValue.TryDecode(key, out text)) return true;
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
            else MclslDiagnostics.Error("locale-register:" + key, "简体中文键注册后未出现在当前语言表: " + key);
        }
        catch (Exception ex)
        {
            RegisteredKeys.Remove(key);
            MclslDiagnostics.Error("locale-register:" + key, "简体中文键注册失败: " + key + " " + ex);
        }
    }

}
