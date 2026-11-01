using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Mono.Cecil;
using MySimulatedLongevityRoad.Patches;
using F = MySimulatedLongevityRoad.Patches.MclslNativeFeedbackPatches;

static class Program
{
    static int checks;
    static void Check(bool value, string name) { if (!value) throw new Exception(name); checks++; }
    static void Reject(Action action, string name)
    {
        try { action(); } catch (InvalidOperationException) { checks++; return; }
        throw new Exception("Expected IL mismatch: " + name);
    }
    static readonly Dictionary<string, OpCode> Opcodes = typeof(OpCodes).GetFields(BindingFlags.Static | BindingFlags.Public)
        .Where(f => f.FieldType == typeof(OpCode)).Select(f => (OpCode)f.GetValue(null)).ToDictionary(o => o.Name);
    static object Operand(object value)
    {
        if (value is FieldReference f)
        {
            Type type = f.DeclaringType.FullName switch
            {
                "Actor" => typeof(Actor), "BaseSimObject" => typeof(BaseSimObject),
                "UnityEngine.Vector3" => typeof(UnityEngine.Vector3), _ => null
            };
            if (type != null && AccessTools.Field(type, f.Name) is FieldInfo field) return field;
        }
        if (value is MethodReference m)
        {
            if (m.DeclaringType.FullName == "Actor" && m.Name == "getScaleMod") return AccessTools.Method(typeof(Actor), m.Name);
            if (m.DeclaringType.FullName == "BaseStats" && m.Name == "get_Item") return AccessTools.PropertyGetter(typeof(BaseStats), "Item");
        }
        return value;
    }
    static void Main()
    {
        using var module = ModuleDefinition.ReadModule(System.IO.Path.Combine(AppContext.BaseDirectory, "GameInput.dll"));
        var cases = new[] {
            ("Actor", "spawnSlash", 6, 1), ("Actor", "spawnSlashKick", 1, 1),
            ("Actor", "getSlashPosition", 5, 1), ("Actor", "doCastAnimation", 0, 3),
            ("EffectsLibrary", "showMetaEventEffect", 2, 1), ("EffectsLibrary", "showMetaEventEffectConversion", 2, 1)
        };
        foreach (var (type, name, args, replacements) in cases)
        {
            var method = module.Types.Single(t => t.Name == type).Methods.Single(m => m.Name == name && m.Parameters.Count == args);
            var input = method.Body.Instructions.Select(i => new CodeInstruction(Opcodes[i.OpCode.Name], Operand(i.Operand))).ToList();
            var op = input.Select(i => i.opcode).ToArray();
            var operands = input.Select(i => i.operand).ToArray();
            // Preserve labels/exception metadata attached by Harmony/other patches.
            var label = new DynamicMethod("labels", typeof(void), Type.EmptyTypes).GetILGenerator().DefineLabel();
            foreach (var instruction in input)
            {
                instruction.labels.Add(label);
                instruction.blocks.Add(new ExceptionBlock(ExceptionBlockType.BeginExceptionBlock));
            }
            Func<IEnumerable<CodeInstruction>, IEnumerable<CodeInstruction>> patch = name == "doCastAnimation" ? F.CastTranspiler
                : name == "getSlashPosition" ? F.SlashPositionTranspiler : F.ActorScaleTranspiler;
            var result = patch(input).ToList();
            Check(result.Count == input.Count, name + " instruction count");
            Check(result.All(i => i.labels.Count == 1 && i.labels[0].Equals(label)), name + " labels");
            Check(result.All(i => i.blocks.Count == 1 && i.blocks[0].blockType == ExceptionBlockType.BeginExceptionBlock), name + " exception metadata");
            Check(result.Count(i => i.operand is MethodInfo mi && mi.DeclaringType == typeof(F)) == replacements, name + " replacement count");
            for (int i = 0; i < result.Count; i++)
            {
                bool changed = result[i].opcode != op[i] || !Equals(result[i].operand, operands[i]);
                if (changed) Check(result[i].opcode == OpCodes.Nop || result[i].operand is MethodInfo mi && mi.DeclaringType == typeof(F), name + " bounded edit");
            }
            Check(result.Where((_, i) => op[i].FlowControl == FlowControl.Branch || op[i].FlowControl == FlowControl.Cond_Branch)
                .All(i => Equals(i.operand, operands[input.IndexOf(i)])), name + " branch targets");
            Check(result.Where((_, i) => operands[i] is MethodReference mr && mr.DeclaringType.Name == "EffectsLibrary")
                .All(i => Equals(i.operand, operands[input.IndexOf(i)])), name + " original effect calls");
            Reject(() => patch(result).ToList(), name + " already normalized");
            Reject(() => patch(Array.Empty<CodeInstruction>()).ToList(), name + " missing");
            Reject(() => patch(method.Body.Instructions.Concat(method.Body.Instructions).Select(i => new CodeInstruction(Opcodes[i.OpCode.Name], Operand(i.Operand)))).ToList(), name + " duplicate");
        }
        foreach (float scale in new[] { .025f, .1f, .3f, 1.27515447f, 4f })
        {
            var actor = new Actor { actor_scale = scale, current_scale = new() { y = scale + .01f }, stats = new() { Value = scale + .02f } };
            Check(F.FeedbackScale(actor) == scale && F.FeedbackScaleMod(actor) == actor.getScaleMod()
                && F.CastScale(actor) == actor.stats["scale"] && F.CastOffsetScale(actor) == actor.current_scale.y, "vanilla exact passthrough");
            actor.IsMod = true;
            Check(F.FeedbackScale(actor) == .1f && F.FeedbackScaleMod(actor) == 1f && F.CastScale(actor) == .1f && F.CastOffsetScale(actor) == .1f, "mod normalization");
            Check(actor.actor_scale == scale && actor.stats["scale"] == scale + .02f && actor.current_scale.y == scale + .01f, "state untouched");
        }
        var mod = new Actor { IsMod = true };
        for (int i = 0; i < 10000; i++) Read(mod);
        long before = GC.GetAllocatedBytesForCurrentThread();
        float sum = 0;
        for (int i = 0; i < 100000; i++) sum += Read(mod);
        long allocated = GC.GetAllocatedBytesForCurrentThread() - before;
        Check(allocated == 0 && sum > 0, "numeric hot path allocation");
        Console.WriteLine($"NativeFeedbackTests: {checks} checks passed; game MVID={module.Mvid}; 100000 reads allocated={allocated} bytes (identity boundary stubbed).");
    }
    static float Read(Actor a) => F.FeedbackScale(a) + F.FeedbackScaleMod(a) + F.CastScale(a) + F.CastOffsetScale(a);
}
