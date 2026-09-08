using System;
using MySimulatedLongevityRoad.Data;
using MySimulatedLongevityRoad.Queries;
using MySimulatedLongevityRoad.Traits;
using MySimulatedLongevityRoad.UI;

namespace MySimulatedLongevityRoad.Systems;

/// <summary>
/// 修炼身份的唯一权威写入口。Realm 字段是事实源，可见特质、真元下限、名称、
/// 统计索引和 UI 快照均是该事实源的投影；投影同步不得反向触发年度修炼。
/// </summary>
internal static class MclslCultivationStateTransitions
{
    private static int _projectionSyncDepth;

    internal static bool IsProjectionSyncActive => _projectionSyncDepth > 0;

    internal static bool TrySetCultivationSystem(Actor actor, string cultivationSystem)
    {
        if (actor?.data == null) return false;
        bool valid = string.IsNullOrWhiteSpace(cultivationSystem)
            || cultivationSystem == MclslCultivationSystemIds.AncientLaw
            || cultivationSystem == MclslCultivationSystemIds.NewLaw;
        if (!valid) return false;

        string normalized = string.IsNullOrWhiteSpace(cultivationSystem)
            ? string.Empty
            : cultivationSystem;
        string previous = MclslActorAccessor.GetString(
            actor,
            MclslActorDataKeys.CultivationSystem,
            string.Empty);

        MclslActorRegistry.Register(actor, out _);
        if (string.Equals(previous, normalized, StringComparison.Ordinal))
        {
            MclslCultivatorCandidateIndex.Observe(actor);
            return true;
        }

        MclslActorAccessor.Set(actor, MclslActorDataKeys.CultivationSystem, normalized);
        if (string.Equals(previous, MclslCultivationSystemIds.AncientLaw, StringComparison.Ordinal)
            && string.Equals(normalized, MclslCultivationSystemIds.NewLaw, StringComparison.Ordinal))
        {
            // 0.1.9 以前旧法高境也曾误用新法尊号键。转修新法时清空，
            // 让角色依据新法法则、天地之魄与逆理重新获得对应尊号。
            MclslActorAccessor.Set(actor, MclslActorDataKeys.HuaShenHonorific, string.Empty);
            MclslActorAccessor.Set(actor, MclslActorDataKeys.HeDaoHonorific, string.Empty);
            MclslActorAccessor.Set(actor, MclslActorDataKeys.ChangShengHonorific, string.Empty);
        }
        string realm = MclslActorAccessor.Realm(actor);
        if (!string.IsNullOrWhiteSpace(realm))
        {
            EnterProjectionSync();
            try
            {
                MclslTraitRegistration.SyncRealmTraitFamily(actor, realm);
            }
            finally
            {
                ExitProjectionSync();
            }
        }
        MclslActorAccessor.ApplyDisplayName(actor, realm);
        RefreshIndexes(actor);
        return true;
    }

    internal static bool TrySetRealm(Actor actor, string realm, int year, string reason)
    {
        if (actor?.data == null
            || string.IsNullOrWhiteSpace(realm)
            || MclslRealmIds.Index(realm) < 0) return false;

        string previousRealm = MclslActorAccessor.Realm(actor);
        bool realmChanged = !string.Equals(previousRealm, realm, StringComparison.Ordinal);
        int normalizedYear = Math.Max(0, year);

        MclslActorRegistry.Register(actor, out _);
        if (MclslActorAccessor.GetInt(actor, MclslActorDataKeys.CultivationStartYear, 0) <= 0)
            MclslActorAccessor.Set(actor, MclslActorDataKeys.CultivationStartYear, normalizedYear);

        EnterProjectionSync();
        try
        {
            MclslTraitGrantRouter.SuppressRouting(() =>
            {
                for (int i = 0; i < MclslRealmIds.Ordered.Length; i++)
                {
                    string oldRealm = MclslRealmIds.Ordered[i];
                    string trait = MclslTraitRegistration.TraitIdForRealm(oldRealm);
                    if (!string.IsNullOrWhiteSpace(trait) && actor.hasTrait(trait)) actor.removeTrait(trait);
                    string legacyTrait = MclslTraitRegistration.LegacyTraitIdForRealm(oldRealm);
                    if (!string.IsNullOrWhiteSpace(legacyTrait) && actor.hasTrait(legacyTrait)) actor.removeTrait(legacyTrait);
                }

                string targetTrait = MclslTraitRegistration.TraitIdForRealm(actor, realm);
                if (!string.IsNullOrWhiteSpace(targetTrait) && !actor.hasTrait(targetTrait)) actor.addTrait(targetTrait);
            });

            MclslActorAccessor.Set(actor, MclslActorDataKeys.Realm, realm);
            MclslTraitRegistration.SyncRealmTraitFamily(actor, realm);
            MclslTraitRegistration.SyncNativeRealmTraits(actor, realm);
            if (realm == MclslRealmIds.ChangSheng) MclslTraitRegistration.EnsureNativeImmortal(actor);
        }
        finally
        {
            ExitProjectionSync();
        }

        if (realmChanged)
        {
            MclslActorAccessor.Set(actor, MclslActorDataKeys.RealmEnteredYear, normalizedYear);
            MclslActorAccessor.Set(actor, MclslActorDataKeys.LastBreakthroughYear, normalizedYear);
            MclslActorAccessor.Set(actor, MclslActorDataKeys.LastBreakthroughResult, reason ?? string.Empty);
        }
        else if (string.IsNullOrWhiteSpace(MclslActorAccessor.GetString(
                     actor,
                     MclslActorDataKeys.LastBreakthroughResult,
                     string.Empty))
                 && !string.IsNullOrWhiteSpace(reason))
        {
            // 旧档若完全缺失结果可补一次；普通同境界投影同步不覆盖真实突破记录。
            MclslActorAccessor.Set(actor, MclslActorDataKeys.LastBreakthroughResult, reason);
        }

        int trueEssence = Math.Max(
            MclslRealmProgress.EntryMinimum(realm),
            MclslCultivationGrowthSystem.CurrentTrueEssence(actor));
        bool ancientPath = MclslActorAccessor.GetString(actor, MclslActorDataKeys.CultivationSystem, string.Empty)
            == MclslCultivationSystemIds.AncientLaw;
        MclslCultivationGrowthSystem.SetTrueEssence(
            actor,
            realm,
            trueEssence,
            ancientPath,
            enforceRealmMinimum: true,
            resetFractionalRemainder: false);
        MclslTechniqueRealmLimit.EnsureAtLeastRealm(actor, realm);
        MclslActorAccessor.ApplyDisplayName(actor, realm);
        if (realmChanged) RestoreHealthAfterPromotion(actor);
        RefreshIndexes(actor);
        return true;
    }

    internal static bool ClearRealm(Actor actor, int year, string reason)
    {
        if (actor?.data == null) return false;
        string previousRealm = MclslActorAccessor.Realm(actor);

        EnterProjectionSync();
        try
        {
            MclslTraitGrantRouter.SuppressRouting(() =>
            {
                for (int i = 0; i < MclslRealmIds.Ordered.Length; i++)
                {
                    string realm = MclslRealmIds.Ordered[i];
                    string trait = MclslTraitRegistration.TraitIdForRealm(realm);
                    if (!string.IsNullOrWhiteSpace(trait) && actor.hasTrait(trait)) actor.removeTrait(trait);
                    string legacyTrait = MclslTraitRegistration.LegacyTraitIdForRealm(realm);
                    if (!string.IsNullOrWhiteSpace(legacyTrait) && actor.hasTrait(legacyTrait)) actor.removeTrait(legacyTrait);
                }
            });
            MclslActorAccessor.Set(actor, MclslActorDataKeys.Realm, string.Empty);
        }
        finally
        {
            ExitProjectionSync();
        }

        if (!string.IsNullOrWhiteSpace(previousRealm))
        {
            MclslActorAccessor.Set(actor, MclslActorDataKeys.RealmEnteredYear, 0);
            MclslActorAccessor.Set(actor, MclslActorDataKeys.LastBreakthroughYear, Math.Max(0, year));
        }
        MclslActorAccessor.Set(actor, MclslActorDataKeys.LastBreakthroughResult, reason ?? string.Empty);
        MclslActorAccessor.ApplyDisplayName(actor, string.Empty);
        RefreshIndexes(actor);
        return true;
    }

    private static void RefreshIndexes(Actor actor)
    {
        MclslCultivatorCandidateIndex.Observe(actor);
        MclslWorldActorQuery.MarkDirty();
        MclslRankSnapshotSource.Invalidate();
    }

    private static void RestoreHealthAfterPromotion(Actor actor)
    {
        try
        {
            actor.updateStats();
            float max = actor.getMaxHealth();
            if (max > 0f) actor.data.health = Math.Max(1, (int)Math.Min((float)int.MaxValue, max));
        }
        catch (Exception ex)
        {
            MySimulatedLongevityRoad.Core.MclslDiagnostics.Error(
                "cultivation-transition:health",
                "境界写入后刷新生命失败: " + ex.Message);
        }
    }

    private static void EnterProjectionSync()
    {
        _projectionSyncDepth++;
    }

    private static void ExitProjectionSync()
    {
        if (_projectionSyncDepth > 0) _projectionSyncDepth--;
    }
}
