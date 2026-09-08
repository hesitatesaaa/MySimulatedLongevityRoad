using System;
using MySimulatedLongevityRoad.Core;
using MySimulatedLongevityRoad.Data;
using MySimulatedLongevityRoad.Queries;
using MySimulatedLongevityRoad.UI;

namespace MySimulatedLongevityRoad.Systems;

/// <summary>
/// 玄鉴式年度修炼单步：年度调度只负责把角色送入一次结算，
/// 本入口只根据修炼资质、境界和世界规则计算一次基础真元增长。
/// 任何事件、UI、突破资源与感气展示都不能替代或阻断基础增长。
/// </summary>
internal static class MclslAnnualCultivationExecutor
{
    internal static bool TryApplyOneAnnualStep(Actor actor, int annualYear)
    {
        return TryApplyOneAnnualStep(actor, annualYear, out _);
    }

    internal static bool TryApplyOneAnnualStep(
        Actor actor,
        int annualYear,
        out string failureReason)
    {
        failureReason = string.Empty;
        RepairLocalZeroProgressCursor(actor, annualYear);

        MclslCultivationAnnualSnapshot snapshot =
            MclslCultivationLocalCore.BuildAnnualSnapshot(actor, annualYear);
        MclslCultivationLocalCheckResult check =
            MclslCultivationLocalCore.CheckAnnualStep(snapshot);
        if (!check.Passed)
        {
            failureReason = check.Reason;
            TraceFailure(snapshot.ActorId, annualYear, failureReason);
            return false;
        }

        if (MclslSpiritualRootSystem.HasCultivationPotential(actor))
            _ = MclslSpiritualRootSystem.Profile(actor);

        float rawGain = CalculateRawAnnualGain(
            actor,
            snapshot.Realm,
            annualYear,
            snapshot.AncientLaw,
            snapshot.Gift);
        if (rawGain <= 0f)
        {
            failureReason = "年度真元计算结果为0";
            TraceFailure(snapshot.ActorId, annualYear, failureReason);
            return false;
        }

        // 先提交真元，再提交年度游标；事件、突破和 UI 均不能吞掉基础增长。
        int beforeEssence = MclslCultivationGrowthSystem.CurrentTrueEssence(actor);
        float beforeRemainder = MclslActorAccessor.GetFloat(
            actor,
            MclslActorDataKeys.TrueEssenceRemainder,
            0f);
        int gained = MclslCultivationGrowthSystem.GrantTrueEssence(
            actor,
            snapshot.Realm,
            rawGain,
            annualYear,
            snapshot.AncientLaw,
            applyWorldState: true,
            applySameLaw: true);
        int afterEssence = MclslCultivationGrowthSystem.CurrentTrueEssence(actor);
        float afterRemainder = MclslActorAccessor.GetFloat(
            actor,
            MclslActorDataKeys.TrueEssenceRemainder,
            0f);
        bool wroteCultivation = gained > 0
            || afterEssence != beforeEssence
            || Math.Abs(afterRemainder - beforeRemainder) > 0.0001f;
        if (!wroteCultivation)
        {
            failureReason = "真元写入后没有整数或小数变化";
            TraceFailure(snapshot.ActorId, annualYear, failureReason);
            return false;
        }

        MclslActorAccessor.Set(actor, MclslActorDataKeys.LastCultivationYear, annualYear);
        if (MclslActorAccessor.GetInt(actor, MclslActorDataKeys.CultivationStartYear, 0) <= 0)
            MclslActorAccessor.Set(actor, MclslActorDataKeys.CultivationStartYear, annualYear);
        RefreshCultivationRuntime(actor);
        return true;
    }

    private static void RepairLocalZeroProgressCursor(Actor actor, int annualYear)
    {
        if (actor?.data == null || annualYear <= 0) return;
        if (!MclslSpiritualRootSystem.HasCultivationPotential(actor)) return;
        if (MclslCultivationGrowthSystem.CurrentTrueEssence(actor) > 0) return;
        int startYear = MclslActorAccessor.GetInt(actor, MclslActorDataKeys.CultivationStartYear, 0);
        int lastYear = MclslActorAccessor.GetInt(actor, MclslActorDataKeys.LastCultivationYear, -1);
        if (startYear > 0 && startYear < annualYear && lastYear >= annualYear)
            MclslActorAccessor.Set(actor, MclslActorDataKeys.LastCultivationYear, annualYear - 1);
    }

    internal static float CalculateRawAnnualGain(
        Actor actor,
        string realm,
        int year,
        bool ancientLaw)
    {
        int aptitude = Math.Clamp(
            MclslActorAccessor.GetInt(actor, MclslActorDataKeys.Aptitude, 0),
            0,
            100);
        MclslAptitudeGiftDefinition gift =
            MclslSpiritualRootSystem.GiftForCultivation(actor)
            ?? (aptitude > 0 ? MclslAptitudeGiftCatalog.ForAptitude(aptitude) : null);
        return gift == null
            ? 0f
            : CalculateRawAnnualGain(actor, realm, year, ancientLaw, gift);
    }

    private static float CalculateRawAnnualGain(
        Actor actor,
        string realm,
        int year,
        bool ancientLaw,
        MclslAptitudeGiftDefinition gift)
    {
        float gain = gift.AnnualTrueEssence
            * MclslCultivationGrowthSystem.RealmGainScale(realm)
            * MclslSpiritualRootSystem.CountCultivationMultiplier(actor)
            * MclslMindSystem.CultivationMultiplier(actor);

        if (ancientLaw)
        {
            float field = 1f + MclslAncientLawSystem.AnnualFieldBonusPercent(actor, realm) / 100f;
            string lastResult = MclslActorAccessor.GetString(
                actor,
                MclslActorDataKeys.LastBreakthroughResult,
                string.Empty);
            float retreat = lastResult.IndexOf("闭关", StringComparison.Ordinal) >= 0
                ? 1.06f
                : 1f;
            float techniqueMultiplier = MclslTechniqueStageSystem.AnnualMultiplier(actor);
            if (techniqueMultiplier <= 0f) techniqueMultiplier = 1f;
            gain *= field
                * techniqueMultiplier
                * retreat;
        }
        else
        {
            int insight = Math.Max(
                0,
                MclslActorAccessor.GetInt(actor, MclslActorDataKeys.TechniqueInsight, 0));
            gain *= 1f + Math.Min(0.22f, insight / 600f);
        }

        float variance = 0.95f
            + (PositiveHash(MclslActorAccessor.Id(actor)
                + "|annual_cultivation|" + year) % 11) / 100f;
        return Math.Max(0.01f, gain * variance);
    }

    private static int PositiveHash(string value)
    {
        unchecked
        {
            int hash = 53;
            foreach (char c in value ?? string.Empty) hash = hash * 47 + c;
            return hash & int.MaxValue;
        }
    }

    private static void RefreshCultivationRuntime(Actor actor)
    {
        MclslWorldActorQuery.Track(actor);
        MclslWorldActorQuery.MarkDirty();
        MclslRankSnapshotSource.Invalidate();
    }

    private static void TraceFailure(long actorId, int year, string reason)
    {
        MclslDiagnostics.Cultivation(
            "executor.fail",
            "actor=" + actorId + " year=" + year + " reason=" + reason);
    }
}
