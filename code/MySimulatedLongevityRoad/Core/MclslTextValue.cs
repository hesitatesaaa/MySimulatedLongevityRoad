using System;
using System.Text;
using System.Collections.Generic;

namespace MySimulatedLongevityRoad.Core;

/// <summary>
/// A self-contained UI text value. Dynamic descriptions own their text instead
/// of adding a permanent entry to the mod or the game's localization tables.
/// Lowercase hexadecimal survives the game's locale-key normalization.
/// </summary>
internal static class MclslTextValue
{
    internal const string Prefix = "mclsl_runtime_value_";
    private const int CacheLimit = 128, MaxCachedKeyLength = 8192;
    private static readonly Dictionary<string, string> Decoded = new(StringComparer.Ordinal);
    private static readonly Queue<string> Order = new();
    private static readonly UTF8Encoding Utf8 = new(false, true);
    internal static int CacheCount => Decoded.Count;
    internal static void ClearRuntime() { Decoded.Clear(); Order.Clear(); }
    private const string Hex = "0123456789abcdef";
    internal static string Encode(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return string.Empty;
        byte[] bytes = Encoding.UTF8.GetBytes(text.Trim());
        char[] value = new char[Prefix.Length + bytes.Length * 2];
        Prefix.CopyTo(0, value, 0, Prefix.Length);
        for (int i = 0; i < bytes.Length; i++)
        { value[Prefix.Length + i * 2] = Hex[bytes[i] >> 4]; value[Prefix.Length + i * 2 + 1] = Hex[bytes[i] & 15]; }
        return new string(value);
    }
    internal static bool TryDecode(string key, out string text)
    {
        text = string.Empty;
        if (key == null || !key.StartsWith(Prefix, StringComparison.Ordinal)) return false;
        if (Decoded.TryGetValue(key, out text)) return true;
        text = string.Empty;
        int length = key.Length - Prefix.Length;
        if (length == 0 || (length & 1) != 0) return false;
        byte[] bytes = new byte[length / 2];
        for (int i = 0; i < bytes.Length; i++)
        {
            int high = Digit(key[Prefix.Length + i * 2]), low = Digit(key[Prefix.Length + i * 2 + 1]);
            if (high < 0 || low < 0) return false;
            bytes[i] = (byte)((high << 4) | low);
        }
        try { text = Utf8.GetString(bytes); }
        catch (DecoderFallbackException) { return false; }
        if (key.Length <= MaxCachedKeyLength)
        {
            if (Decoded.Count >= CacheLimit) Decoded.Remove(Order.Dequeue());
            Decoded.Add(key, text); Order.Enqueue(key);
        }
        return true;
    }
    private static int Digit(char value) => value >= '0' && value <= '9' ? value - '0'
        : value >= 'a' && value <= 'f' ? value - 'a' + 10 : -1;
}
