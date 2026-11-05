using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace MySimulatedLongevityRoad.Core;

/// <summary>
/// 边界异常统一守卫：把散落各处的 <c>catch (Exception) { Probe(...) }</c> 收敛到一处，
/// 按“可忽略 / 持久化 / 其它”分类处理并计数，便于指标与质量棘轮读取消异常数量。
/// </summary>
internal enum MclslGuardScope
{
    /// <summary>可忽略的可选成员读取（反射取不到就当默认值）。</summary>
    OptionalMember = 0,
    /// <summary>可忽略的反射探测。</summary>
    Reflection = 1,
    /// <summary>原生 UI/回调边界，异常应记录但不中断外层。</summary>
    NativeUi = 2,
    /// <summary>持久化边界，异常不可静默，必须 Fatal 并停写。</summary>
    Persistence = 3
}

internal static class MclslGuard
{
    private static int _swallowed;
    private static readonly ConcurrentDictionary<string, int> ByKey = new(StringComparer.Ordinal);

    /// <summary>累计被守卫吞掉的异常次数，供指标/棘轮观察是否在下降。</summary>
    internal static int SwallowedCount => Volatile.Read(ref _swallowed);
    internal static IReadOnlyDictionary<string, int> SwallowedByKey => ByKey;

    internal static bool Try(Action action, string key, MclslGuardScope scope = MclslGuardScope.NativeUi)
    {
        if (action == null) return false;
        try { action(); return true; }
        catch (Exception ex) { Handle(key, scope, ex); return false; }
    }

    internal static T Try<T>(Func<T> action, T fallback, string key, MclslGuardScope scope = MclslGuardScope.NativeUi)
    {
        if (action == null) return fallback;
        try { return action(); }
        catch (Exception ex) { Handle(key, scope, ex); return fallback; }
    }

    /// <summary>已在 catch 内、只想统一记录并计数的场景：替代散落的 Probe("empty-catch-code-…")。</summary>
    internal static void Swallowed(string key, Exception ex, MclslGuardScope scope = MclslGuardScope.OptionalMember)
    {
        string k = string.IsNullOrWhiteSpace(key) ? "swallowed" : key;
        ByKey.AddOrUpdate(k, 1, static (_, v) => v + 1);
        Interlocked.Increment(ref _swallowed);
        if (scope == MclslGuardScope.Persistence)
        {
            MclslDiagnostics.Fatal("guard-" + k, "持久化异常不可忽略: " + ex);
            return;
        }
        if (scope == MclslGuardScope.NativeUi)
        {
            // 原生 UI/回调边界：保持“始终可见”的错误级别，避免静默。
            MclslDiagnostics.Error("guard-" + k, "边界异常(" + scope + "): " + (ex?.Message ?? string.Empty));
            return;
        }
        MclslDiagnostics.Probe("guard-" + k, "可忽略异常(" + scope + "): " + (ex?.Message ?? string.Empty));
    }

    internal static void ResetCounters()
    {
        Interlocked.Exchange(ref _swallowed, 0);
        ByKey.Clear();
    }

    private static void Handle(string key, MclslGuardScope scope, Exception ex)
    {
        string k = string.IsNullOrWhiteSpace(key) ? "guard" : key;
        ByKey.AddOrUpdate(k, 1, static (_, v) => v + 1);
        Interlocked.Increment(ref _swallowed);
        switch (scope)
        {
            case MclslGuardScope.OptionalMember:
            case MclslGuardScope.Reflection:
                MclslDiagnostics.Probe("guard-" + k, "可忽略异常(" + scope + "): " + ex.Message);
                break;
            case MclslGuardScope.Persistence:
                MclslDiagnostics.Fatal("guard-" + k, "持久化异常不可忽略: " + ex);
                break;
            default:
                MclslDiagnostics.Error("guard-" + k, "边界异常: " + ex.Message);
                break;
        }
    }
}
