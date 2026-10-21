using System.Reflection;
using System.Collections.Generic;
using HarmonyLib;
using MySimulatedLongevityRoad.Core;

namespace MySimulatedLongevityRoad.Patches;

internal static class MclslHarmonyPatchGuard
{
    private static readonly HashSet<string> FailedRequiredPatches = new();
    internal static int FailedRequiredCount => FailedRequiredPatches.Count;
    internal static string FailedRequiredSummary => string.Join("、", FailedRequiredPatches);
    internal static bool TryPatch(
        Harmony harmony,
        string key,
        MethodInfo original,
        MethodInfo prefix = null,
        MethodInfo postfix = null,
        MethodInfo finalizer = null,
        MethodInfo transpiler = null,
        bool required = true)
    {
        if (harmony == null || original == null)
        {
            if (required) FailedRequiredPatches.Add(key ?? "unknown");
            MclslDiagnostics.Error("harmony-missing:" + key, "关键补丁目标缺失: " + key);
            return false;
        }

        try
        {
            PatchProcessor processor = harmony.CreateProcessor(original);
            if (prefix != null) processor.AddPrefix(prefix);
            if (postfix != null) processor.AddPostfix(postfix);
            if (finalizer != null) processor.AddFinalizer(finalizer);
            if (transpiler != null) processor.AddTranspiler(transpiler);
            processor.Patch();
            FailedRequiredPatches.Remove(key ?? "unknown");
            return true;
        }
        catch (System.Exception ex)
        {
            if (required) FailedRequiredPatches.Add(key ?? "unknown");
            MclslDiagnostics.Error("harmony-failed:" + key, "补丁挂载失败 " + key + ": " + ex.Message);
            return false;
        }
    }
}
