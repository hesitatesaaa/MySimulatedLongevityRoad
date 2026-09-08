using System;
using MySimulatedLongevityRoad.Data;

namespace MySimulatedLongevityRoad.Systems;

/// <summary>
/// 旧法功法参悟进度。它不是破境门槛，也不直接增加破境成功率；
/// 其唯一数值职责是决定角色运转当前功法时的年度真元效率。
/// </summary>
internal static class MclslTechniqueStageSystem
{
    internal static string StageName(int progress) => Math.Clamp(progress, 0, 100) switch
    {
        >= 85 => "圆满",
        >= 55 => "大成",
        >= 25 => "小成",
        _ => "入门"
    };

    internal static string Display(Actor actor)
    {
        int progress = Progress(actor);
        return StageName(progress) + " " + progress + "%（功法效率" + EfficiencyPercent(progress) + "%）";
    }

    /// <summary>
    /// 读取新字段；若是0.1.11及以前的旧档，则将旧字段中的0—100数值原样迁移为参悟进度。
    /// </summary>
    internal static int Progress(Actor actor)
    {
        if (actor?.data == null) return 0;
        int stored = MclslActorAccessor.GetInt(actor, MclslActorDataKeys.AncientTechniqueComprehension, -1);
        if (stored >= 0) return Math.Clamp(stored, 0, 100);

        int legacy = Math.Clamp(
            MclslActorAccessor.GetInt(actor, MclslActorDataKeys.LegacyAncientTechniqueProgressKey, 0),
            0,
            100);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.AncientTechniqueComprehension, legacy);
        return legacy;
    }

    internal static void SetProgress(Actor actor, int value)
    {
        if (actor?.data == null) return;
        MclslActorAccessor.Set(
            actor,
            MclslActorDataKeys.AncientTechniqueComprehension,
            Math.Clamp(value, 0, 100));
    }

    /// <summary>
    /// 每一点参悟都会改变功法效率；小成、大成、圆满是可见阶段，而非额外硬门槛。
    /// 入门0—24：80%—99%；小成25—54：100%—114%；
    /// 大成55—84：115%—129%；圆满85—100：130%—140%。
    /// </summary>
    internal static float AnnualMultiplier(Actor actor) => AnnualMultiplier(Progress(actor));

    internal static float AnnualMultiplier(int progress)
    {
        int value = Math.Clamp(progress, 0, 100);
        if (value >= 85) return 1.30f + (value - 85) * (0.10f / 15f);
        if (value >= 55) return 1.15f + (value - 55) * (0.15f / 30f);
        if (value >= 25) return 1.00f + (value - 25) * (0.15f / 30f);
        return 0.80f + value * (0.20f / 25f);
    }

    internal static int EfficiencyPercent(int progress) =>
        Math.Clamp((int)MathF.Round(AnnualMultiplier(progress) * 100f), 80, 140);

    /// <summary>
    /// 旧法功法参悟的年度基础增长。每名角色每年只结算一次，
    /// 复用既有年度修炼链，不增加扫描或独立状态机。
    /// </summary>
    internal static int AdvanceAnnualProgress(Actor actor, int aptitude)
    {
        if (actor?.data == null) return 0;
        int current = Progress(actor);
        if (current >= 100) return 0;

        int mind = MclslMindSystem.EnsureMindState(actor);
        int gain = 1;
        if (aptitude >= 60) gain++;
        if (aptitude >= 85) gain++;
        if (mind >= 80) gain++;

        int next = Math.Clamp(current + gain, 0, 100);
        SetProgress(actor, next);
        return next - current;
    }

    internal static void AddProgress(Actor actor, int delta)
    {
        if (actor?.data == null || delta == 0) return;
        SetProgress(actor, Progress(actor) + delta);
    }
}
