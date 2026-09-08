using System.Reflection;
using HarmonyLib;
using MySimulatedLongevityRoad.Core;

namespace MySimulatedLongevityRoad.Patches;

internal static class MclslHarmonyPatchGuard
{
    internal static bool TryPatch(
        Harmony harmony,
        string key,
        MethodInfo original,
        MethodInfo prefix = null,
        MethodInfo postfix = null,
        MethodInfo finalizer = null)
    {
        if (harmony == null || original == null)
        {
            MclslDiagnostics.Once("harmony-missing:" + key, "跳过缺失补丁目标: " + key);
            return false;
        }

        try
        {
            PatchProcessor processor = harmony.CreateProcessor(original);
            if (prefix != null) processor.AddPrefix(prefix);
            if (postfix != null) processor.AddPostfix(postfix);
            if (finalizer != null) processor.AddFinalizer(finalizer);
            processor.Patch();
            return true;
        }
        catch (System.Exception ex)
        {
            MclslDiagnostics.Error("harmony-failed:" + key, "补丁挂载失败 " + key + ": " + ex.Message);
            return false;
        }
    }
}
