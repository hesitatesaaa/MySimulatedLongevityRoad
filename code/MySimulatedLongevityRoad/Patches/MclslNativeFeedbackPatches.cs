using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using MySimulatedLongevityRoad.Systems;
using UnityEngine;

namespace MySimulatedLongevityRoad.Patches;

// Only native feedback call sites use these values. Never change actor stats or
// the shared effect spawner: mod spell geometry and vanilla giants retain theirs.
internal static class MclslNativeFeedbackPatches
{
    internal static float FeedbackScale(Actor actor)
        => MclslActorPresentation.UsesVanillaMarker(actor) ? MclslActorPresentation.NativeAdultMarkerScale : actor.actor_scale;

    internal static float FeedbackScaleMod(Actor actor)
        => MclslActorPresentation.UsesVanillaMarker(actor) ? 1f : actor.getScaleMod();

    internal static float CastScale(Actor actor)
        => MclslActorPresentation.UsesVanillaMarker(actor) ? MclslActorPresentation.NativeAdultMarkerScale : actor.stats["scale"];

    internal static float CastOffsetScale(Actor actor)
        => MclslActorPresentation.UsesVanillaMarker(actor) ? MclslActorPresentation.NativeAdultMarkerScale : actor.current_scale.y;

    internal static int Apply(Harmony harmony)
    {
        int count = 0;
        count += Patch(harmony, typeof(Actor), "spawnSlash", new[] { typeof(Vector2), typeof(string), typeof(float), typeof(float), typeof(float), typeof(float?) }, nameof(ActorScaleTranspiler));
        count += Patch(harmony, typeof(Actor), "spawnSlashKick", new[] { typeof(Vector2) }, nameof(ActorScaleTranspiler));
        count += Patch(harmony, typeof(Actor), "getSlashPosition", new[] { typeof(Actor), typeof(Vector2), typeof(float), typeof(float), typeof(float) }, nameof(SlashPositionTranspiler));
        count += Patch(harmony, typeof(Actor), "doCastAnimation", Type.EmptyTypes, nameof(CastTranspiler));
        count += Patch(harmony, typeof(EffectsLibrary), "showMetaEventEffect", new[] { typeof(string), typeof(Actor) }, nameof(ActorScaleTranspiler));
        count += Patch(harmony, typeof(EffectsLibrary), "showMetaEventEffectConversion", new[] { typeof(string), typeof(Actor) }, nameof(ActorScaleTranspiler));
        return count;
    }

    private static int Patch(Harmony harmony, Type type, string name, Type[] args, string transpiler)
        => MclslHarmonyPatchGuard.TryPatch(harmony, type.Name + "." + name + ".nativeFeedback",
            AccessTools.Method(type, name, args), transpiler: AccessTools.Method(typeof(MclslNativeFeedbackPatches), transpiler)) ? 1 : 0;

    internal static IEnumerable<CodeInstruction> ActorScaleTranspiler(IEnumerable<CodeInstruction> source)
    {
        var code = new List<CodeInstruction>(source);
        FieldInfo field = AccessTools.Field(typeof(Actor), "actor_scale");
        int replaced = 0;
        foreach (CodeInstruction instruction in code)
            if (instruction.opcode == OpCodes.Ldfld && Equals(instruction.operand, field))
            {
                Set(instruction, OpCodes.Call, AccessTools.Method(typeof(MclslNativeFeedbackPatches), nameof(FeedbackScale)));
                replaced++;
            }
        Require(replaced, 1, "actor_scale");
        return code;
    }

    internal static IEnumerable<CodeInstruction> SlashPositionTranspiler(IEnumerable<CodeInstruction> source)
    {
        var code = new List<CodeInstruction>(source);
        MethodInfo original = AccessTools.Method(typeof(Actor), "getScaleMod", Type.EmptyTypes);
        int replaced = 0;
        foreach (CodeInstruction instruction in code)
            if ((instruction.opcode == OpCodes.Call || instruction.opcode == OpCodes.Callvirt) && Equals(instruction.operand, original))
            {
                Set(instruction, OpCodes.Call, AccessTools.Method(typeof(MclslNativeFeedbackPatches), nameof(FeedbackScaleMod)));
                replaced++;
            }
        Require(replaced, 1, "slash scale modifier");
        return code;
    }

    internal static IEnumerable<CodeInstruction> CastTranspiler(IEnumerable<CodeInstruction> source)
    {
        var code = new List<CodeInstruction>(source);
        FieldInfo stats = AccessTools.Field(typeof(BaseSimObject), "stats");
        FieldInfo scale = AccessTools.Field(typeof(BaseSimObject), "current_scale");
        FieldInfo y = AccessTools.Field(typeof(Vector3), "y");
        MethodInfo getter = AccessTools.PropertyGetter(typeof(BaseStats), "Item");
        int sizes = 0, offsets = 0;
        for (int i = 0; i < code.Count; i++)
        {
            if (i + 2 < code.Count && code[i].opcode == OpCodes.Ldfld && Equals(code[i].operand, stats)
                && code[i + 1].opcode == OpCodes.Ldstr && Equals(code[i + 1].operand, "scale")
                && code[i + 2].opcode == OpCodes.Callvirt && Equals(code[i + 2].operand, getter))
            {
                Set(code[i], OpCodes.Call, AccessTools.Method(typeof(MclslNativeFeedbackPatches), nameof(CastScale)));
                Set(code[i + 1], OpCodes.Nop, null);
                Set(code[i + 2], OpCodes.Nop, null);
                sizes++;
                i += 2;
            }
            else if (i + 1 < code.Count && code[i].opcode == OpCodes.Ldflda && Equals(code[i].operand, scale)
                && code[i + 1].opcode == OpCodes.Ldfld && Equals(code[i + 1].operand, y))
            {
                Set(code[i], OpCodes.Call, AccessTools.Method(typeof(MclslNativeFeedbackPatches), nameof(CastOffsetScale)));
                Set(code[i + 1], OpCodes.Nop, null);
                offsets++;
                i++;
            }
        }
        Require(sizes, 2, "cast effect sizes");
        Require(offsets, 1, "cast effect offset");
        return code;
    }

    // Mutate in place so Harmony branch labels and exception blocks survive.
    private static void Set(CodeInstruction instruction, OpCode opcode, object operand)
    {
        instruction.opcode = opcode;
        instruction.operand = operand;
    }

    private static void Require(int actual, int expected, string site)
    {
        if (actual != expected)
            throw new InvalidOperationException($"Native feedback IL mismatch ({site}): expected {expected}, found {actual}");
    }
}
