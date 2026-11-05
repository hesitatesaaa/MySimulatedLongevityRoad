using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Threading;

namespace MySimulatedLongevityRoad.Core;

/// <summary>
/// 通用成员反射缓存：<c>GetField/GetProperty</c> 结果按 (Type, name) 缓存，
/// 供 Traits/Death 等运行期路径替换每次调用都查找的 <c>GetType().GetField(...)</c>。
/// 与 <see cref="MclslReflectionNumbers"/> 同款缓存策略（进程级、线程安全）。
/// </summary>
internal static class MclslReflectionMembers
{
    private const BindingFlags Flags = BindingFlags.Instance | BindingFlags.Public
        | BindingFlags.NonPublic | BindingFlags.IgnoreCase;

    private readonly struct Accessor
    {
        internal readonly FieldInfo Field;
        internal readonly PropertyInfo Property;
        internal Accessor(FieldInfo field, PropertyInfo property) { Field = field; Property = property; }
    }

    private static readonly ConcurrentDictionary<(Type, string), Accessor> Cache = new();
    private static int _factoryCalls;

    /// <summary>实际执行过的反射查找次数（预热后应保持不变即证明命中缓存）。</summary>
    internal static int FactoryCalls => Volatile.Read(ref _factoryCalls);
    internal static int CacheCount => Cache.Count;

    internal static void ResetCache()
    {
        Cache.Clear();
        Volatile.Write(ref _factoryCalls, 0);
    }

    private static Accessor Resolve(Type type, string name)
        => Cache.GetOrAdd((type, name), static key =>
        {
            Interlocked.Increment(ref _factoryCalls);
            FieldInfo field = key.Item1.GetField(key.Item2, Flags);
            PropertyInfo property = field == null ? key.Item1.GetProperty(key.Item2, Flags) : null;
            return new Accessor(field, property);
        });

    internal static bool TryGet(object target, string name, out object value, string site = null)
    {
        value = null;
        if (target == null || string.IsNullOrWhiteSpace(name)) return false;
        try
        {
            Accessor accessor = Resolve(target.GetType(), name);
            if (accessor.Field != null) { value = accessor.Field.GetValue(target); return true; }
            if (accessor.Property != null && accessor.Property.CanRead)
            {
                value = accessor.Property.GetValue(target);
                return true;
            }
            return false;
        }
        catch (Exception ex)
        {
            MclslDiagnostics.Probe("reflection-get:" + (site ?? name), ex.Message);
            return false;
        }
    }

    internal static bool TryGet<T>(object target, string name, out T value, string site = null)
    {
        value = default;
        if (!TryGet(target, name, out object raw, site)) return false;
        if (raw is T typed) { value = typed; return true; }
        try { value = (T)Convert.ChangeType(raw, typeof(T)); return true; }
        catch (Exception ex)
        {
            MclslDiagnostics.Probe("reflection-get-cast:" + (site ?? name), ex.Message);
            return false;
        }
    }

    internal static bool TrySet(object target, string name, object value, string site = null)
    {
        if (target == null || string.IsNullOrWhiteSpace(name)) return false;
        try
        {
            Accessor accessor = Resolve(target.GetType(), name);
            if (accessor.Field != null) { accessor.Field.SetValue(target, value); return true; }
            if (accessor.Property != null && accessor.Property.CanWrite)
            {
                accessor.Property.SetValue(target, value);
                return true;
            }
            return false;
        }
        catch (Exception ex)
        {
            MclslDiagnostics.Probe("reflection-set:" + (site ?? name), ex.Message);
            return false;
        }
    }

    /// <summary>启动期预热：提前解析并缓存，成员缺失时可尽早暴露（此处不抛，交给调用方判断）。</summary>
    internal static void Prime(Type type, params string[] names)
    {
        if (type == null) return;
        foreach (string name in names ?? Array.Empty<string>())
            if (!string.IsNullOrWhiteSpace(name)) Resolve(type, name);
    }
}
