using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace MySimulatedLongevityRoad.Core;

/// <summary>
/// Developer-only static-state audit (M6-A). When the diagnostics switch is on,
/// enumerates the mod assembly's <c>Mclsl*</c> types and reports static fields that
/// still hold data after a world teardown. It never changes behaviour and is fully
/// silent while diagnostics are disabled, so it can be wired into the normal
/// lifecycle to prove that <see cref="MclslRuntime.ClearWorldState"/> leaves no
/// residual state before a new architecture (M6-B) is attempted.
/// </summary>
internal static class MclslWorldStateAudit
{
    // Process-wide caches and configuration deliberately survive world reloads.
    private static readonly HashSet<string> IgnoredTypeNames = new(StringComparer.Ordinal)
    {
        "MclslDiagnostics", "MclslReflectionNumbers", "MclslStableHash", "MclslRuntimeSettings",
        "MclslConfig", "MclslConfigLocalization", "MclslWorldStateAudit", "MclslPerformanceProbe",
        "MclslFpsOverlay", "MclslStartupTiming", "MclslDeveloperBridge", "MclslMemoryMaintenance"
    };

    internal static void ReportResidual(string phase)
    {
        if (!MclslDiagnostics.Enabled) return;
        Type[] types;
        try { types = typeof(MclslRuntime).Assembly.GetTypes(); }
        catch (ReflectionTypeLoadException ex) { types = ex.Types; }
        catch { return; }

        StringBuilder detail = new();
        int residual = 0;
        foreach (Type type in types)
        {
            if (type == null || !type.Name.StartsWith("Mclsl", StringComparison.Ordinal)) continue;
            if (IgnoredTypeNames.Contains(type.Name)) continue;
            FieldInfo[] fields;
            try { fields = type.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic); }
            catch { continue; }
            foreach (FieldInfo field in fields)
            {
                if (field.IsLiteral) continue;
                string measured = Measure(field);
                if (measured == null) continue;
                residual++;
                if (detail.Length < 1500)
                    detail.Append('\n').Append(type.Name).Append('.').Append(field.Name).Append('=').Append(measured);
            }
        }
        if (residual > 0)
            MclslDiagnostics.Alert("worldstate.residual." + phase,
                "[模拟长生路][诊断] " + phase + " 后仍有 " + residual + " 个静态状态非空:" + detail);
    }

    private static string Measure(FieldInfo field)
    {
        try
        {
            object value = field.GetValue(null);
            if (value is bool b) return b ? "true" : null;
            if (value is int i) return i != 0 ? i.ToString() : null;
            if (value is long l) return l != 0 ? l.ToString() : null;
            if (value is IDictionary dict) return dict.Count > 0 ? dict.Count.ToString() : null;
            if (value is ICollection collection) return collection.Count > 0 ? collection.Count.ToString() : null;
            return null;
        }
        catch { return null; }
    }
}
