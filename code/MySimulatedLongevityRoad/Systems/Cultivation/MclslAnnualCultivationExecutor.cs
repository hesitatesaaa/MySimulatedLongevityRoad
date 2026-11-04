using System;
using MySimulatedLongevityRoad.Core;
using MySimulatedLongevityRoad.Data;
using MySimulatedLongevityRoad.Queries;
using MySimulatedLongevityRoad.Traits;
using MySimulatedLongevityRoad.UI;

namespace MySimulatedLongevityRoad.Systems;

/// <summary>
/// 玄鉴式年度修炼单步：年度调度只负责把角色送入一次结算，
/// 本入口只根据修炼资质、境界和世界规则计算一次基础真元增长。
/// 任何事件、UI、突破资源与感气展示都不能替代或阻断基础增长。
/// </summary>
internal static class MclslAnnualCultivationExecutor
{
    internal static bool TryApplyInitialYear(Actor actor, int year)
    {
        if (actor?.data == null) return false;
        // Entry hooks may run during another actor's annual step. Always retain
        // the request, instead of dropping it because a global context is active.
        MclslScheduler.RequestFirstCultivation(actor);
        return false;
    }

    internal static bool TryApplyQueuedInitialYear(Actor actor)
    {
        if (actor?.data == null) return false;
        int startYear = MclslActorAccessor.GetInt(actor, MclslActorDataKeys.CultivationStartYear, 0);
        int lastYear = MclslActorAccessor.GetInt(actor, MclslActorDataKeys.LastCultivationYear, -1);
        int year = startYear;
        if (!MclslInitialCultivationPolicy.ShouldApply(
                year, startYear, lastYear, MclslAnnualExecutionContext.IsActive)
            || !MclslAnnualExecutionContext.TryEnter(actor, year)) return false;
        try { return TryApplyOneAnnualStep(actor, year); }
        finally { MclslAnnualExecutionContext.Exit(actor, year); }
    }

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
        if(MclslBeastActorRegistration.IsOrdinary(actor))
        {
            // 兽类分支与修士分支同属“修行推进”阶段，单独计时便于区分开销。
            long beastSample = MclslPerformanceProbe.Begin();
            bool pending=MclslActorAccessor.GetInt(actor,"mclsl.beast.growth_year",-1)<annualYear;
            MclslBeastCultivation.TickAnnual(actor,annualYear);
            MclslScheduler.CompleteFirstCultivation(actor);
            bool beastAlive = MclslActorAccessor.Alive(actor);
            MclslPerformanceProbe.End("年度角色.修行兽类", beastSample);
            return pending && beastAlive;
        }
        // 快照与纯规则判断是每次年度结算最前的固定开销。
        long snapshotSample = MclslPerformanceProbe.Begin();
        MclslCultivationAnnualSnapshot snapshot =
            MclslCultivationLocalCore.BuildAnnualSnapshot(actor, annualYear);
        MclslCultivationLocalCheckResult check =
            MclslCultivationLocalCore.CheckAnnualStep(snapshot);
        MclslPerformanceProbe.End("年度角色.修行快照", snapshotSample);
        if (!check.Passed)
        {
            failureReason = check.Reason;
            TraceFailure(snapshot.ActorId, annualYear, failureReason);
            return false;
        }

        // 不变式：CheckAnnualStep 通过意味着 snapshot.Gift != null。而 snapshot.Gift
        // 由 GiftForCultivation 得到，其非空或来自当前特质（GiftFromCurrentTrait 非空），
        // 或来自 aptitude>0；两者都令 HasCultivationRootArchive(actor)==true。
        // 因此此处 HasCultivationPotential(actor) 必为 true，无需再调用它触发一次
        // 完整的特质目录扫描（GiftFromCurrentTrait），直接进入 Profile 即可，结果不变。
        long rootSample = MclslPerformanceProbe.Begin();
        MclslSpiritualRootProfile rootProfile = MclslSpiritualRootSystem.Profile(actor);
        MclslPerformanceProbe.End("年度角色.灵根入门", rootSample);

        long gainSample = MclslPerformanceProbe.Begin();
        float rawGain = CalculateRawAnnualGain(
            actor,
            snapshot.Realm,
            annualYear,
            snapshot.AncientLaw,
            snapshot.Gift,
            rootProfile.CountCultivationMultiplier);
        MclslPerformanceProbe.End("年度角色.修行增长", gainSample);
        if (rawGain <= 0f)
        {
            failureReason = "年度真元计算结果为0";
            TraceFailure(snapshot.ActorId, annualYear, failureReason);
            return false;
        }

        // 先提交真元，再提交年度游标；事件、突破和 UI 均不能吞掉基础增长。
        long writeSample = MclslPerformanceProbe.Begin();
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
            applySameLaw: true,
            annualCultivation: true);
        int afterEssence = MclslCultivationGrowthSystem.CurrentTrueEssence(actor);
        float afterRemainder = MclslActorAccessor.GetFloat(
            actor,
            MclslActorDataKeys.TrueEssenceRemainder,
            0f);
        MclslPerformanceProbe.End("年度角色.修行写入", writeSample);
        bool wroteCultivation = gained > 0
            || afterEssence != beforeEssence
            || Math.Abs(afterRemainder - beforeRemainder) > 0.0001f;
        if (!wroteCultivation)
        {
            failureReason = "真元写入后没有整数或小数变化";
            TraceFailure(snapshot.ActorId, annualYear, failureReason);
            return false;
        }

        long finishSample = MclslPerformanceProbe.Begin();
        MclslActorAccessor.Set(actor, MclslActorDataKeys.LastCultivationYear, annualYear);
        if (MclslActorAccessor.GetInt(actor, MclslActorDataKeys.CultivationStartYear, 0) <= 0)
            MclslActorAccessor.Set(actor, MclslActorDataKeys.CultivationStartYear, annualYear);
        MclslScheduler.CompleteFirstCultivation(actor);
        if (string.IsNullOrWhiteSpace(snapshot.Realm))
            MclslActorAccessor.Set(actor, MclslActorDataKeys.LastBreakthroughResult,
                "感气积累真元：" + Math.Min(afterEssence, MclslRealmProgress.LianQiEntryMinimum)
                + "/" + MclslRealmProgress.LianQiEntryMinimum);
        RefreshCultivationRuntime(actor);
        MclslPerformanceProbe.End("年度角色.修行收尾", finishSample);
        return true;
    }

    internal static float CalculateRawAnnualGain(
        Actor actor,
        string realm,
        int year,
        bool ancientLaw)
    {
        int aptitude = Math.Clamp(
            MclslActorAccessor.GetInt(actor, MclslActorDataKeys.Aptitude, 0)
                + (actor.hasTrait(MclslTraitRegistration.HeavenFavorTraitId) ? 20 : 0),
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
        return CalculateRawAnnualGain(
            actor,
            realm,
            year,
            ancientLaw,
            gift,
            MclslSpiritualRootSystem.CountCultivationMultiplier(actor));
    }

    private static float CalculateRawAnnualGain(
        Actor actor,
        string realm,
        int year,
        bool ancientLaw,
        MclslAptitudeGiftDefinition gift,
        float countCultivationMultiplier)
    {
        float gain = gift.AnnualTrueEssence
            * MclslCultivationGrowthSystem.RealmGainScale(realm)
            * countCultivationMultiplier
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
        if (actor.hasTrait(MclslTraitRegistration.HeavenFavorTraitId)) gain *= 1.20f;
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
