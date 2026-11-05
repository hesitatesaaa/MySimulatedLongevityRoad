using System;
using MySimulatedLongevityRoad.Core;

// MclslGuard / MclslReflectionMembers 的行为测试（补齐评估报告里缺失的单测）。
internal static class GuardReflectionTests
{
    private static void Check(bool ok, string name) { if (!ok) throw new Exception(name); }

    internal static void Run()
    {
        // ---- MclslGuard ----
        MclslGuard.ResetCounters();
        int baseCount = MclslGuard.SwallowedCount;
        bool handled = MclslGuard.Try(() => throw new InvalidOperationException("boom"), "g1");
        Check(!handled, "Guard.Try returns false when action throws");
        Check(MclslGuard.SwallowedCount == baseCount + 1, "Guard.Try counts the swallowed exception");
        Check(MclslGuard.Try(() => { }, "g2"), "Guard.Try returns true when no throw");
        int fallback = MclslGuard.Try<int>(() => throw new Exception("x"), 42, "g3");
        Check(fallback == 42, "Guard.Try<T> returns fallback on throw");
        MclslGuard.Swallowed("g4", new Exception("e"), MclslGuardScope.OptionalMember);
        Check(MclslGuard.SwallowedByKey.ContainsKey("g4"), "Guard.Swallowed records the key");

        // ---- MclslReflectionMembers ----
        MclslReflectionMembers.ResetCache();
        var target = new Sample { Num = 5, Text = "hi" };
        Check(MclslReflectionMembers.TryGet(target, "Num", out object boxed) && (int)boxed == 5, "members TryGet reads field");
        Check(MclslReflectionMembers.TryGet(target, "Text", out string text) && text == "hi", "members TryGet<T> reads field");
        Check(MclslReflectionMembers.TrySet(target, "Num", 9) && target.Num == 9, "members TrySet writes field");
        int factory = MclslReflectionMembers.FactoryCalls;
        Check(MclslReflectionMembers.TryGet(target, "Num", out _) && MclslReflectionMembers.FactoryCalls == factory, "members caches per (type,name)");
        Check(!MclslReflectionMembers.TryGet(target, "Missing", out _), "members missing returns false");
        Check(MclslReflectionMembers.ResolveAccessor(typeof(Sample), "Num", out var field, out var property) && field != null && property == null, "ResolveAccessor returns field");
        Check(!MclslReflectionMembers.ResolveAccessor(typeof(Sample), "Missing", out var noField, out var noProperty) && noField == null && noProperty == null, "ResolveAccessor missing returns false");

        Console.WriteLine("PASS guard and reflection members");
    }

    private sealed class Sample
    {
        internal int Num;
        internal string Text = string.Empty;
    }
}

// 生产文件 MclslReflectionMembers.TryGetActorMember 引用全局 Actor 类型；
// 离线回归只需一个最小替身（不做任何游戏行为）。
public sealed class Actor { }
