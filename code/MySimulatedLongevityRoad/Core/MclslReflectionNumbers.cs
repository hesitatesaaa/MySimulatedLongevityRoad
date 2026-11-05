using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Threading;

namespace MySimulatedLongevityRoad.Core;

/// <summary>
/// Shared reflection accessor for optional native numeric members. Five systems
/// carried private copies of this lookup (cultivation, ancient law, world soul
/// and death). The <c>site</c> argument keeps the per-file diagnostic identity;
/// <paramref name="diagnosticKey"/> and <paramref name="messagePrefix"/> let a
/// caller that already had a different log line keep it without keeping a second
/// implementation.
///
/// Results are cached per (Type, member name): repeated per-actor/per-year lookups
/// no longer re-run GetField/GetProperty. The cache is process-wide (type metadata
/// does not change with the world) and is thread-safe because native callbacks can
/// reach this from worker threads.
/// </summary>
internal static class MclslReflectionNumbers
{
    private const BindingFlags Flags = BindingFlags.Instance | BindingFlags.Public
        | BindingFlags.NonPublic | BindingFlags.IgnoreCase;

    private readonly struct MemberAccessor
    {
        internal readonly FieldInfo Field;
        internal readonly PropertyInfo Property;
        internal MemberAccessor(FieldInfo field, PropertyInfo property) { Field = field; Property = property; }
    }

    private static readonly ConcurrentDictionary<(Type, string), MemberAccessor> Cache = new();
    private static int _factoryCalls;

    internal static int FactoryCalls => Volatile.Read(ref _factoryCalls);
    internal static int CacheCount => Cache.Count;
    internal static void ResetCacheCounters() { Cache.Clear(); Volatile.Write(ref _factoryCalls, 0); }

    private static MemberAccessor Resolve(Type type, string name)
        => Cache.GetOrAdd((type, name), static key =>
        {
            Interlocked.Increment(ref _factoryCalls);
            FieldInfo field = key.Item1.GetField(key.Item2, Flags);
            PropertyInfo property = field == null ? key.Item1.GetProperty(key.Item2, Flags) : null;
            return new MemberAccessor(field, property);
        });

    internal static void TrySetNumber(object target, string name, float value, string site,
        string diagnosticKey = null, string messagePrefix = null)
    {
        if (target == null || string.IsNullOrWhiteSpace(name)) return;
        try
        {
            MemberAccessor accessor = Resolve(target.GetType(), name);
            if (accessor.Field != null)
            {
                if (accessor.Field.FieldType == typeof(float)) accessor.Field.SetValue(target, value);
                else if (accessor.Field.FieldType == typeof(int)) accessor.Field.SetValue(target, (int)value);
                return;
            }
            PropertyInfo property = accessor.Property;
            if (property != null && property.CanWrite)
            {
                if (property.PropertyType == typeof(float)) property.SetValue(target, value);
                else if (property.PropertyType == typeof(int)) property.SetValue(target, (int)value);
            }
        }
        catch (Exception ex)
        {
            MclslDiagnostics.Error(diagnosticKey ?? ("reflection-set-number:" + site),
                (messagePrefix ?? ("空 catch 捕获: code/MySimulatedLongevityRoad/" + site + ": ")) + ex.Message);
        }
    }

    /// <summary>
    /// Cached read counterpart of <see cref="TrySetNumber"/>. Repeated per-actor /
    /// per-year numeric reads no longer re-run GetField/GetProperty. Returns false
    /// when the member is missing or not numeric; callers fall back to their default.
    /// </summary>
    internal static bool TryGetNumber(object target, string name, out int value,
        string site, string diagnosticKey = null, string messagePrefix = null)
    {
        value = 0;
        if (target == null || string.IsNullOrWhiteSpace(name)) return false;
        try
        {
            MemberAccessor accessor = Resolve(target.GetType(), name);
            object raw = accessor.Field != null
                ? accessor.Field.GetValue(target)
                : (accessor.Property != null && accessor.Property.CanRead ? accessor.Property.GetValue(target) : null);
            if (raw is int i) { value = i; return true; }
            if (raw is float f) { value = (int)f; return true; }
            if (raw is long l) { value = (int)l; return true; }
            return false;
        }
        catch (Exception ex)
        {
            MclslDiagnostics.Error(diagnosticKey ?? ("reflection-get-number:" + site),
                (messagePrefix ?? ("反射读取数值失败: " + site + ": ")) + ex.Message);
            return false;
        }
    }
}
