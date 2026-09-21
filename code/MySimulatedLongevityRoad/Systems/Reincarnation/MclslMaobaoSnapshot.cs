using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using MySimulatedLongevityRoad.Data;

namespace MySimulatedLongevityRoad.Systems;

/// <summary>
/// 猫宝跨世界快照的 custom_data 部分。
/// 这是鬼谷时光长河快照器的同等实现，只把数据命名空间换成 mclsl.*。
/// </summary>
internal static class MclslMaobaoSnapshot
{
    private const string Prefix = "mclsl.";
    private const string LegacyPrefix = "xiuxian.standalone.";
    private const string PlacedKey = "mclsl.maobao.placed";

    internal static Dictionary<string, string> Capture(Actor actor)
    {
        return actor?.data is BaseSystemData data
            ? CaptureFromBaseData(data)
            : new Dictionary<string, string>(StringComparer.Ordinal);
    }

    internal static Dictionary<string, string> CaptureFromActorData(ActorData actorData)
    {
        return actorData is BaseSystemData data
            ? CaptureFromBaseData(data)
            : new Dictionary<string, string>(StringComparer.Ordinal);
    }

    internal static void Apply(Actor actor, Dictionary<string, string> snapshot)
    {
        if (actor?.data is not BaseSystemData data || snapshot == null) return;
        foreach (KeyValuePair<string, string> pair in snapshot)
        {
            if (!ShouldCapture(pair.Key) || string.Equals(pair.Key, PlacedKey, StringComparison.Ordinal)) continue;
            TryWriteEntry(data, pair.Key, pair.Value ?? string.Empty);
        }
    }

    internal static void MarkPlaced(Actor actor)
    {
        if (actor?.data == null) return;
        try { actor.data.addFlag(PlacedKey); } catch { }
        try { actor.data.set(PlacedKey, 1); } catch { }
    }

    private static Dictionary<string, string> CaptureFromBaseData(BaseSystemData data)
    {
        Dictionary<string, string> result = new(StringComparer.Ordinal);
        if (data == null) return result;

        TryCaptureFromCustomDataDictionary(data, result);
        FieldInfo[] fields = typeof(MclslActorDataKeys).GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
        for (int i = 0; i < fields.Length; i++)
        {
            FieldInfo field = fields[i];
            if (field.FieldType != typeof(string)) continue;
            string key = field.GetValue(null) as string;
            if (!ShouldCapture(key) || result.ContainsKey(key)) continue;
            if (TryReadEntry(data, key, out string value)) result[key] = value;
        }
        return result;
    }

    private static void TryCaptureFromCustomDataDictionary(BaseSystemData data, Dictionary<string, string> result)
    {
        try
        {
            object custom = ReadMember(data, "custom_data") ?? ReadMember(data, "customData");
            object dict = ReadMember(custom, "dict") ?? ReadMember(custom, "_dict") ?? ReadMember(custom, "data");
            if (dict is not IDictionary map) return;
            foreach (DictionaryEntry entry in map)
            {
                string key = Convert.ToString(entry.Key, CultureInfo.InvariantCulture) ?? string.Empty;
                if (!ShouldCapture(key) || result.ContainsKey(key)) continue;
                string value = Convert.ToString(entry.Value, CultureInfo.InvariantCulture) ?? string.Empty;
                if (value.Length > 0) result[key] = value;
            }
        }
        catch { }
    }

    private static bool ShouldCapture(string key)
    {
        return !string.IsNullOrWhiteSpace(key)
            && (key.StartsWith(Prefix, StringComparison.Ordinal)
                || key.StartsWith(LegacyPrefix, StringComparison.Ordinal));
    }

    private static bool TryReadEntry(BaseSystemData data, string key, out string value)
    {
        value = string.Empty;
        try
        {
            data.get(key, out string s, string.Empty);
            if (!string.IsNullOrEmpty(s)) { value = s; return true; }
        }
        catch { }
        try
        {
            data.get(key, out int i, int.MinValue);
            if (i != int.MinValue) { value = i.ToString(CultureInfo.InvariantCulture); return true; }
        }
        catch { }
        try
        {
            data.get(key, out float f, float.MinValue);
            if (Math.Abs(f - float.MinValue) > 0.0001f)
            {
                value = f.ToString(CultureInfo.InvariantCulture);
                return true;
            }
        }
        catch { }
        return false;
    }

    private static void TryWriteEntry(BaseSystemData data, string key, string raw)
    {
        if (data == null || string.IsNullOrWhiteSpace(key)) return;
        string value = raw.Trim();
        if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int integer)
            && !value.Contains('.', StringComparison.Ordinal)
            && !value.Contains('e', StringComparison.OrdinalIgnoreCase))
        {
            data.set(key, integer);
            return;
        }
        if (float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out float number))
        {
            data.set(key, number);
            return;
        }
        if (value.Equals("true", StringComparison.OrdinalIgnoreCase)) { data.set(key, 1); return; }
        if (value.Equals("false", StringComparison.OrdinalIgnoreCase)) { data.set(key, 0); return; }
        data.set(key, value);
    }

    private static object ReadMember(object target, string name)
    {
        if (target == null || string.IsNullOrWhiteSpace(name)) return null;
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        Type type = target.GetType();
        FieldInfo field = type.GetField(name, flags);
        if (field != null) return field.GetValue(target);
        PropertyInfo property = type.GetProperty(name, flags);
        return property != null && property.CanRead && property.GetIndexParameters().Length == 0
            ? property.GetValue(target, null)
            : null;
    }
}
