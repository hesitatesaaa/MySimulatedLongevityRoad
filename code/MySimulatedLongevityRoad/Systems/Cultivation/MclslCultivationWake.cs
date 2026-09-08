using System;
using MySimulatedLongevityRoad.Core;
using MySimulatedLongevityRoad.Data;
using MySimulatedLongevityRoad.Queries;
using MySimulatedLongevityRoad.Traits;
using MySimulatedLongevityRoad.UI;

namespace MySimulatedLongevityRoad.Systems;

/// <summary>
/// 玄鉴式修炼唤醒入口。特质、入道、转世和手动赋予只负责改变数据，
/// 统一由这里把角色接回候选索引、年度队列、排行榜和右侧信息框。
/// </summary>
internal static class MclslCultivationWake
{
    private static int _entryDepth;

    internal static bool ReconcileCultivationIdentity(
        Actor actor,
        bool ensureEntryFromGift = false)
    {
        long actorId = MclslActorAccessor.Id(actor);
        MclslDiagnostics.Cultivation(
            "wake.reconcile.enter",
            "actor=" + actorId
            + " ensureEntry=" + ensureEntryFromGift
            + " alive=" + MclslActorAccessor.Alive(actor)
            + " eligible=" + MclslEligibility.CanCultivate(actor)
            + " realm=" + MclslActorAccessor.Realm(actor)
            + " system=" + MclslActorAccessor.GetString(actor, MclslActorDataKeys.CultivationSystem, string.Empty)
            + " essence=" + MclslCultivationGrowthSystem.CurrentTrueEssence(actor));
        if (actor?.data == null
            || !MclslActorAccessor.Alive(actor)
            || !MclslEligibility.CanCultivate(actor))
        {
            MclslDiagnostics.Cultivation("wake.reconcile.skip", "actor=" + actorId + " reason=data-alive-eligibility");
            return false;
        }

        bool repairedGiftArchive = NormalizeGiftArchive(actor);
        MclslDiagnostics.Cultivation(
            "wake.reconcile.archive",
            "actor=" + actorId
            + " repairedGiftArchive=" + repairedGiftArchive
            + " hasRoot=" + MclslSpiritualRootSystem.HasCultivationPotential(actor)
            + " gift=" + (MclslSpiritualRootSystem.GiftForCultivation(actor)?.Name ?? ""));
        if (MclslTraitRegistration.TryBestRealmFromTraits(actor, out _))
        {
            MclslTraitRegistration.ReconcileTraitState(actor);
            MclslDiagnostics.Cultivation("wake.reconcile.realm_trait", "actor=" + actorId + " reconciledRealmTrait=true");
        }
        MclslCultivationAgeSanity.RepairImpossibleYouthCultivation(actor, MclslRuntime.CurrentYear());

        if (ensureEntryFromGift
            && _entryDepth == 0
            && string.IsNullOrWhiteSpace(MclslActorAccessor.Realm(actor))
            && MclslSpiritualRootSystem.HasCultivationPotential(actor))
        {
            _entryDepth++;
            try
            {
                bool entered = MclslSpiritualRootEntrySystem.TryEnterFromGiftTrait(actor, MclslRuntime.CurrentYear());
                MclslDiagnostics.Cultivation(
                    "wake.reconcile.entry",
                    "actor=" + actorId
                    + " entered=" + entered
                    + " year=" + MclslRuntime.CurrentYear()
                    + " systemAfter=" + MclslActorAccessor.GetString(actor, MclslActorDataKeys.CultivationSystem, string.Empty)
                    + " realmAfter=" + MclslActorAccessor.Realm(actor));
            }
            finally
            {
                _entryDepth = Math.Max(0, _entryDepth - 1);
            }
        }

        bool relevant = repairedGiftArchive
            || MclslCultivationActorMarker.ShouldKeepTracked(actor)
            || MclslActorAccessor.HasCultivationPath(actor);
        MclslDiagnostics.Cultivation(
            "wake.reconcile.done",
            "actor=" + actorId
            + " relevant=" + relevant
            + " keepTracked=" + MclslCultivationActorMarker.ShouldKeepTracked(actor)
            + " hasPath=" + MclslActorAccessor.HasCultivationPath(actor));
        return relevant;
    }

    internal static void EnsureAwake(
        Actor actor,
        bool ensureEntryFromGift = false,
        bool enqueueAnnual = true,
        bool refreshUi = true)
    {
        long actorId = MclslActorAccessor.Id(actor);
        MclslDiagnostics.Cultivation(
            "wake.ensure.enter",
            "actor=" + actorId
            + " ensureEntry=" + ensureEntryFromGift
            + " enqueue=" + enqueueAnnual
            + " refreshUi=" + refreshUi
            + " realm=" + MclslActorAccessor.Realm(actor)
            + " system=" + MclslActorAccessor.GetString(actor, MclslActorDataKeys.CultivationSystem, string.Empty)
            + " essence=" + MclslCultivationGrowthSystem.CurrentTrueEssence(actor));
        if (actor?.data == null
            || !MclslActorAccessor.Alive(actor)
            || !MclslEligibility.CanCultivate(actor))
        {
            MclslDiagnostics.Cultivation("wake.ensure.skip", "actor=" + actorId + " reason=data-alive-eligibility");
            return;
        }

        ReconcileCultivationIdentity(actor, ensureEntryFromGift);

        bool shouldKeepTracked = MclslCultivationActorMarker.ShouldKeepTracked(actor);
        MclslCultivatorCandidateIndex.Observe(actor);
        if (shouldKeepTracked)
            MclslActorAccessor.ApplyDisplayName(actor, MclslActorAccessor.Realm(actor));

        if (enqueueAnnual
            && shouldKeepTracked
            && !MclslAnnualExecutionContext.IsExecutingActor(actor))
        {
            MclslDiagnostics.Cultivation("wake.ensure.enqueue", "actor=" + actorId + " enqueueAnnual=true keepTracked=true");
            MclslScheduler.WakeAnnualCultivationActor(actor);
        }
        else
        {
            MclslDiagnostics.Cultivation(
                "wake.ensure.no_enqueue",
                "actor=" + actorId + " enqueueAnnual=" + enqueueAnnual + " keepTracked=" + shouldKeepTracked);
        }

        MclslWorldActorQuery.MarkDirty();
        MclslRankSnapshotSource.Invalidate();
        if (refreshUi)
        {
            MclslActorInfoPanel.RefreshOpenForActor(actor);
        }
    }

    private static bool NormalizeGiftArchive(Actor actor)
    {
        MclslAptitudeGiftDefinition gift = MclslSpiritualRootSystem.GiftFromCurrentTrait(actor);
        if (gift == null) return false;

        bool changed = false;
        int aptitude = Math.Clamp(
            MclslActorAccessor.GetInt(actor, MclslActorDataKeys.Aptitude, 0),
            0,
            100);
        if (aptitude < gift.MinAptitude || aptitude > gift.MaxAptitude)
        {
            aptitude = Math.Clamp((gift.MinAptitude + gift.MaxAptitude) / 2, 1, 100);
            MclslActorAccessor.Set(actor, MclslActorDataKeys.Aptitude, aptitude);
            changed = true;
        }

        int fate = Math.Clamp(
            MclslActorAccessor.GetInt(actor, MclslActorDataKeys.ImmortalFate, 0),
            0,
            100);
        if (fate <= 0)
        {
            MclslActorAccessor.Set(actor, MclslActorDataKeys.ImmortalFate, Math.Clamp(aptitude, 20, 100));
            changed = true;
        }

        if (MclslActorAccessor.GetInt(actor, MclslActorDataKeys.ManualGrant, 0) <= 0)
        {
            MclslActorAccessor.Set(actor, MclslActorDataKeys.ManualGrant, 1);
            changed = true;
        }
        if (MclslActorAccessor.GetInt(actor, MclslActorDataKeys.ChildhoodRootChecked, 0) <= 0)
        {
            MclslActorAccessor.Set(actor, MclslActorDataKeys.ChildhoodRootChecked, 1);
            changed = true;
        }
        if (MclslActorAccessor.GetInt(actor, MclslActorDataKeys.MortalSeparationChecked, 0) <= 0)
        {
            MclslActorAccessor.Set(actor, MclslActorDataKeys.MortalSeparationChecked, 1);
            changed = true;
        }
        MclslMindSystem.EnsureMindState(actor);

        // 修炼特质本身即代表灵根档案存在。这里立即生成/修复灵根资料，
        // 避免后续 UI、排行榜和年度候选只看到特质却读不到修炼档案。
        _ = MclslSpiritualRootSystem.Profile(actor);
        return changed;
    }
}
