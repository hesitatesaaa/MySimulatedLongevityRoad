using System;
using System.Collections.Generic;

namespace MySimulatedLongevityRoad.Systems;

internal static class MclslLocalizationBridge
{
    private static readonly Dictionary<string, string> RuntimeKeys = new(StringComparer.Ordinal);
    private static readonly Dictionary<string, string> TextByRuntimeKey = new(StringComparer.Ordinal);
    private static readonly HashSet<string> RegisteredKeys = new(StringComparer.Ordinal);

    internal static string RuntimeText(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return string.Empty;
        string value = text.Trim();
        if (RuntimeKeys.TryGetValue(value, out string cachedKey))
        {
            EnsureRegistered(cachedKey, value);
            return cachedKey;
        }
        string key = "mclsl_runtime_" + StableHash(value).ToString("X8");
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

    internal static void RegisterKey(string key, string text)
    {
        if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(text)) return;
        key = key.Trim();
        text = text.Trim();
        TextByRuntimeKey[key] = text;
        EnsureRegistered(key, text);
    }

    internal static void RetryRuntimeKeys()
    {
        foreach (KeyValuePair<string, string> pair in TextByRuntimeKey)
            EnsureRegistered(pair.Key, pair.Value);
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
        return TextByRuntimeKey.TryGetValue(key.Trim(), out text)
            && !string.IsNullOrWhiteSpace(text);
    }

    private static void EnsureRegistered(string key, string text)
    {
        if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(text)) return;
        key = key.Trim();
        text = text.Trim();
        if (!RegisteredKeys.Add(key)) return;
        try
        {
            LocalizedTextManager.add(key, text, false, string.Empty, true);
        }
        catch
        {
            RegisteredKeys.Remove(key);
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
