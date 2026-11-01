using System;
using System.Reflection;
using HarmonyLib;
// Only game/presentation identity boundaries are substituted. Transpilers and
// numeric decisions are linked directly from production; input IL is the game DLL.
namespace UnityEngine
{
    public struct Vector2 { public float x, y; }
    public struct Vector3 { public float x, y, z; }
}
public class BaseStats { public float Value; public float this[string key] => Value; }
public class BaseSimObject { public BaseStats stats = new(); public UnityEngine.Vector3 current_scale; }
public class Actor : BaseSimObject
{
    public bool IsMod;
    public float actor_scale;
    public float getScaleMod() => actor_scale / .1f;
}
public class EffectsLibrary { }
namespace MySimulatedLongevityRoad.Systems
{
    internal static class MclslActorPresentation
    {
        internal const float NativeAdultMarkerScale = .1f;
        internal static bool UsesVanillaMarker(BaseSimObject actor) => actor is Actor a && a.IsMod;
    }
}
namespace MySimulatedLongevityRoad.Patches
{
    internal static class MclslHarmonyPatchGuard
    {
        internal static bool TryPatch(Harmony h, string key, MethodInfo method, MethodInfo transpiler) => throw new NotSupportedException();
    }
}
