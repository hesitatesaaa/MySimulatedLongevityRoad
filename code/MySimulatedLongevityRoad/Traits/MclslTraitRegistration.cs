using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MySimulatedLongevityRoad.Core;
using MySimulatedLongevityRoad.Data;
using MySimulatedLongevityRoad.Queries;
using MySimulatedLongevityRoad.Systems;
using MySimulatedLongevityRoad.UI;
using UnityEngine;

namespace MySimulatedLongevityRoad.Traits;

internal static class MclslTraitRegistration
{
    private const string RealmGroupId = "MclslRealms";
    private const string AncientRealmGroupId = "MclslAncientRealms";
    private const string GiftGroupId = "MclslGifts";
    private const string SpecialGroupId = "MclslSpecial";
    internal const string NativeImmortalTraitId = "immortal";
    internal const string HuanzhenTraitId = "MclslHuanzhen";
    internal const string WorldSoulEntityTraitId = "MclslWorldSoulEntity";
    private static bool _initialized;
    private static bool _reconciling;
    private static bool? _lastRealmEditorNewLawState;
    private static readonly Dictionary<string, string[][]> NativeTraitsByRealm = new()
    {
        [MclslRealmIds.LianQi] = new[] { A("eagle_eyed", "eagle_eye", "eagle_eyes"), A("wise") },
        [MclslRealmIds.ZhuJi] = new[] { A("lucky") },
        [MclslRealmIds.JinDan] = new[] { A("tough"), A("fast") },
        [MclslRealmIds.YuanYing] = new[] { A("regeneration"), A("immune"), A("agile", "dodge", "dodgy"), A("energized", "vitality"), A("fast", "speedy", "sprinter", "dash") },
        [MclslRealmIds.HuaShen] = new[] { A("sunny", "sun_blessed", "solar"), A("poison_immune", "immune_poison", "poison_immunity"), A("fire_proof", "fireproof"), A("freeze_proof", "cold_proof", "frost_proof"), A("flash", "blink", "teleport") },
        [MclslRealmIds.HeDao] = new[] { A("shiny"), A("light_lamp", "light", "glowing"), A("blessed"), A("deflect_projectiles", "projectile_deflection", "projectile_deflect"), A("shield", "shielded", "block") },
        [MclslRealmIds.ChangSheng] = new[] { A(NativeImmortalTraitId) }
    };
    private static readonly string[] LowRealmInvalidHealingTraitIds =
    {
        "regeneration",
        "regenerate",
        "accelerated_healing"
    };

    private static readonly Dictionary<string, string> RealmTraits = new()
    {
        [MclslRealmIds.LianQi] = "realm_1",
        [MclslRealmIds.ZhuJi] = "realm_2",
        [MclslRealmIds.JinDan] = "realm_3",
        [MclslRealmIds.YuanYing] = "realm_4",
        [MclslRealmIds.HuaShen] = "realm_5",
        [MclslRealmIds.HeDao] = "realm_6",
        [MclslRealmIds.ChangSheng] = "realm_7"
    };
    private static readonly Dictionary<string, string> LegacyRealmTraits = new()
    {
        [MclslRealmIds.LianQi] = "MclslRealmLianQi",
        [MclslRealmIds.ZhuJi] = "MclslRealmZhuJi",
        [MclslRealmIds.JinDan] = "MclslRealmJinDan",
        [MclslRealmIds.YuanYing] = "MclslRealmYuanYing",
        [MclslRealmIds.HuaShen] = "MclslRealmHuaShen",
        [MclslRealmIds.HeDao] = "MclslRealmHeDao",
        [MclslRealmIds.ChangSheng] = "MclslRealmChangSheng"
    };

    internal static string TraitIdForRealm(string realm) => RealmTraits.TryGetValue(realm ?? "", out string id) ? id : RealmTraits[MclslRealmIds.LianQi];
    internal static string LegacyTraitIdForRealm(string realm) => LegacyRealmTraits.TryGetValue(realm ?? "", out string id) ? id : string.Empty;
    internal static bool IsAncientRealmTrait(string traitId)
    {
        traitId = NormalizeTraitId(traitId);
        if (string.IsNullOrWhiteSpace(traitId)) return false;
        foreach (string id in LegacyRealmTraits.Values)
            if (string.Equals(id, traitId, StringComparison.Ordinal)) return true;
        return false;
    }

    internal static bool IsNewLawRealmTrait(string traitId)
    {
        traitId = NormalizeTraitId(traitId);
        if (string.IsNullOrWhiteSpace(traitId)) return false;
        foreach (string id in RealmTraits.Values)
            if (string.Equals(id, traitId, StringComparison.Ordinal)) return true;
        return false;
    }

    internal static bool IsEraRealmTrait(string traitId) => IsNewLawRealmTrait(traitId) || IsAncientRealmTrait(traitId);

    internal static bool ShouldShowRealmTraitInEditor(string traitId, int year)
    {
        bool newLawActive = MclslWorldEpochSystem.IsNewLawActive(year);
        if (IsNewLawRealmTrait(traitId)) return newLawActive;
        if (IsAncientRealmTrait(traitId))
            return !newLawActive && !string.Equals(traitId, LegacyTraitIdForRealm(MclslRealmIds.ChangSheng), StringComparison.Ordinal);
        return true;
    }

    internal static string TraitIdForRealm(Actor actor, string realm)
    {
        if (UsesAncientRealmTrait(actor) && realm != MclslRealmIds.ChangSheng) return LegacyTraitIdForRealm(realm);
        return TraitIdForRealm(realm);
    }

    internal static bool UsesAncientRealmTrait(Actor actor)
    {
        if (actor?.data == null) return false;
        string system = MclslActorAccessor.GetString(actor, MclslActorDataKeys.CultivationSystem, string.Empty);
        if (system == MclslCultivationSystemIds.AncientLaw) return true;
        if (system == MclslCultivationSystemIds.NewLaw) return false;
        return !MclslWorldEpochSystem.IsNewLawActive(MclslRuntime.CurrentYear());
    }

    internal static bool TryRealmForTrait(string traitId, out string realm)
    {
        traitId = NormalizeTraitId(traitId);
        foreach (KeyValuePair<string, string> pair in RealmTraits)
        {
            if (pair.Value == traitId)
            {
                realm = pair.Key;
                return true;
            }
        }
        foreach (KeyValuePair<string, string> pair in LegacyRealmTraits)
        {
            if (pair.Value == traitId)
            {
                realm = pair.Key;
                return true;
            }
        }
        realm = string.Empty;
        return false;
    }

    internal static bool TryBestRealmFromTraits(Actor actor, out string bestRealm)
    {
        bestRealm = string.Empty;
        if (actor?.data == null) return false;
        foreach (string realm in MclslRealmIds.Ordered)
        {
            string newTrait = TraitIdForRealm(realm);
            string ancientTrait = LegacyTraitIdForRealm(realm);
            if (HasTraitWithAlias(actor, newTrait) || (!string.IsNullOrWhiteSpace(ancientTrait) && HasTraitWithAlias(actor, ancientTrait)))
                bestRealm = realm;
        }

        return !string.IsNullOrWhiteSpace(bestRealm);
    }

    internal static bool IsGiftTrait(string traitId)
    {
        traitId = NormalizeTraitId(traitId);
        if (string.IsNullOrWhiteSpace(traitId)) return false;
        foreach (MclslAptitudeGiftDefinition gift in MclslAptitudeGiftCatalog.Gifts)
            if (string.Equals(gift.TraitId, traitId, StringComparison.Ordinal)) return true;
        return false;
    }

    internal static void RemoveOtherGiftTraits(Actor actor, string keepTraitId)
    {
        keepTraitId = NormalizeTraitId(keepTraitId);
        if (actor?.data == null || string.IsNullOrWhiteSpace(keepTraitId)) return;
        MclslTraitGrantRouter.SuppressRouting(() =>
        {
            foreach (MclslAptitudeGiftDefinition gift in MclslAptitudeGiftCatalog.Gifts)
            {
                if (string.Equals(gift.TraitId, keepTraitId, StringComparison.Ordinal)) continue;
                try { if (actor.hasTrait(gift.TraitId)) actor.removeTrait(gift.TraitId); } catch (System.Exception mclslEmptyCatchEx) { MySimulatedLongevityRoad.Core.MclslDiagnostics.Error("empty-catch-code-MySimulatedLongevityRoad-Traits-MclslTraitRegistration-cs-1", "空 catch 捕获: code/MySimulatedLongevityRoad/Traits/MclslTraitRegistration.cs #1: " + mclslEmptyCatchEx.Message); }
                string alias = TraitAliasId(gift.TraitId);
                try { if (actor.hasTrait(alias)) actor.removeTrait(alias); } catch (System.Exception mclslEmptyCatchEx) { MySimulatedLongevityRoad.Core.MclslDiagnostics.Error("empty-catch-code-MySimulatedLongevityRoad-Traits-MclslTraitRegistration-cs-1-alias", "空 catch 捕获: code/MySimulatedLongevityRoad/Traits/MclslTraitRegistration.cs #1-alias: " + mclslEmptyCatchEx.Message); }
                string slashAlias = TraitPathAliasId(gift.TraitId);
                try { if (actor.hasTrait(slashAlias)) actor.removeTrait(slashAlias); } catch (System.Exception mclslEmptyCatchEx) { MySimulatedLongevityRoad.Core.MclslDiagnostics.Error("empty-catch-code-MySimulatedLongevityRoad-Traits-MclslTraitRegistration-cs-1-path-alias", "空 catch 捕获: code/MySimulatedLongevityRoad/Traits/MclslTraitRegistration.cs #1-path-alias: " + mclslEmptyCatchEx.Message); }
            }
        });
    }

    internal static void ReconcileTraitState(Actor actor)
    {
        if (actor?.data == null) return;
        if (_reconciling) return;
        try
        {
            _reconciling = true;
            ReconcileGiftTraits(actor);
            ReconcileRealmTraits(actor);
        }
        finally
        {
            _reconciling = false;
        }
    }

    internal static void TryAutoCollectTrait(Actor actor, string traitId)
    {
        traitId = NormalizeTraitId(traitId);
        if (actor?.data == null || string.IsNullOrWhiteSpace(traitId)) return;
        if (TryRealmForTrait(traitId, out string realm))
        {
            TryAutoCollectRealm(actor, MclslRealmIds.Index(realm));
            return;
        }
        TryAutoCollectGift(actor, traitId);
    }

    internal static void Init()
    {
        if (_initialized) return;
        _initialized = true;
        EnsureGroup(RealmGroupId, "新法境界", "#8FD4C8");
        EnsureGroup(AncientRealmGroupId, "仙道境界", "#A6D8D1");
        EnsureGroup(GiftGroupId, "灵根品阶", "#C8D48F");
        EnsureGroup(SpecialGroupId, "长生路特殊", "#D8C778");

        AddRealm(MclslRealmIds.LianQi, "trait/realm_1", 1, 200f, 100f, 20f, 0.2f, 0f, false);
        AddRealm(MclslRealmIds.ZhuJi, "trait/realm_2", 2, 300f, 10000f, 1000f, 1.2f, 70f, false);
        AddRealm(MclslRealmIds.JinDan, "trait/realm_3", 3, 600f, 100000f, 10000f, 1.5f, 99f, false);
        AddRealm(MclslRealmIds.YuanYing, "trait/realm_4", 4, 1000f, 1000000f, 100000f, 2.0f, 198f, false);
        AddRealm(MclslRealmIds.HuaShen, "trait/realm_5", 5, 2000f, 1800000f, 180000f, 2.2f, 220f, false);
        AddRealm(MclslRealmIds.HeDao, "trait/realm_6", 6, 3000f, 3200000f, 320000f, 2.5f, 260f, false);
        AddRealm(MclslRealmIds.ChangSheng, "trait/realm_7", 7, 0f, 5200000f, 520000f, 3.0f, 320f, false);

        AddRealm(MclslRealmIds.LianQi, "trait/realm_1", 1, 240f, 90f, 18f, 0.18f, 0f, true);
        AddRealm(MclslRealmIds.ZhuJi, "trait/realm_2", 2, 360f, 9000f, 900f, 1.08f, 63f, true);
        AddRealm(MclslRealmIds.JinDan, "trait/realm_3", 3, 720f, 90000f, 9000f, 1.35f, 89f, true);
        AddRealm(MclslRealmIds.YuanYing, "trait/realm_4", 4, 1200f, 900000f, 90000f, 1.8f, 178f, true);
        AddRealm(MclslRealmIds.HuaShen, "trait/realm_5", 5, 2400f, 1620000f, 162000f, 1.98f, 198f, true);
        AddRealm(MclslRealmIds.HeDao, "trait/realm_6", 6, 3600f, 2880000f, 288000f, 2.25f, 234f, true);
        AddRealm(MclslRealmIds.ChangSheng, "trait/realm_7", 7, 0f, 4680000f, 468000f, 2.7f, 288f, true);

        foreach (MclslAptitudeGiftDefinition gift in MclslAptitudeGiftCatalog.Gifts.OrderBy(x => x.Level))
            AddGift(gift);

        AddSpecial(HuanzhenTraitId, "ui/Icons/HuanZhen", false, true, 0f, 0f, 0f, 0f);
        AddSpecial(WorldSoulEntityTraitId, "trait/TianDiZhiPo", false, false, 2500f, 2500000f, 250000f, 2.35f, 240f);
        MclslWorldSoulActorRegistration.Init();
        RefreshRealmTraitVisibility(MclslRuntime.CurrentYear(), true);
    }

    internal static string GiftTraitIdForAptitude(int aptitude) => MclslAptitudeGiftCatalog.ForAptitude(aptitude).TraitId;

    internal static void SyncNativeRealmTraits(Actor actor, string realm)
    {
        if (actor?.data == null) return;
        int targetIndex = MclslRealmIds.Index(realm);
        if (targetIndex < 0) return;
        TryAutoCollectRealm(actor, targetIndex);
        RemoveInvalidLowRealmHealingTraits(actor, targetIndex);

        MclslTraitGrantRouter.SuppressRouting(() =>
        {
            for (int i = 0; i <= targetIndex && i < MclslRealmIds.Ordered.Length; i++)
            {
                string realmId = MclslRealmIds.Ordered[i];
                if (!NativeTraitsByRealm.TryGetValue(realmId, out string[][] groups)) continue;
                for (int g = 0; g < groups.Length; g++) AddFirstExistingNativeTrait(actor, groups[g]);
            }
        });
    }

    private static void RemoveInvalidLowRealmHealingTraits(Actor actor, int targetIndex)
    {
        if (actor?.data == null) return;
        if (targetIndex >= MclslRealmIds.Index(MclslRealmIds.YuanYing)) return;

        MclslTraitGrantRouter.SuppressRouting(() =>
        {
            for (int i = 0; i < LowRealmInvalidHealingTraitIds.Length; i++)
            {
                string traitId = LowRealmInvalidHealingTraitIds[i];
                try
                {
                    if (actor.hasTrait(traitId)) actor.removeTrait(traitId);
                }
                catch (Exception ex)
                {
                    MySimulatedLongevityRoad.Core.MclslDiagnostics.Error(
                        "remove-invalid-low-realm-healing:" + traitId,
                        "\u6e05\u7406\u5143\u5a74\u4ee5\u4e0b\u5f02\u5e38\u56de\u8840\u7279\u8d28\u5931\u8d25: " + ex.Message);
                }
            }
        });
    }

    internal static void EnsureNativeImmortal(Actor actor)
    {
        if (actor?.data == null) return;
        try { if (actor.hasTrait(NativeImmortalTraitId)) return; } catch (System.Exception mclslEmptyCatchEx) { MySimulatedLongevityRoad.Core.MclslDiagnostics.Error("empty-catch-code-MySimulatedLongevityRoad-Traits-MclslTraitRegistration-cs-2", "空 catch 捕获: code/MySimulatedLongevityRoad/Traits/MclslTraitRegistration.cs #2: " + mclslEmptyCatchEx.Message); }
        try
        {
            ActorTrait immortal = AssetManager.traits.get(NativeImmortalTraitId);
            MclslTraitGrantRouter.SuppressRouting(() =>
            {
                if (immortal != null) actor.addTrait(immortal, true);
                else actor.addTrait(NativeImmortalTraitId);
            });
        }
        catch (Exception ex)
        {
            Debug.LogWarning("[模拟长生路] 长生不朽特质授予失败: " + ex.Message);
        }
    }

    internal static void EnsureTaishangStats(Actor actor)
    {
        if (actor?.data == null) return;
        try
        {
            actor.updateStats();
            float max = actor.getMaxHealth();
            if (max > 0f) actor.data.health = Math.Max(actor.data.health, (int)Math.Min((float)int.MaxValue, max));
        }
        catch (System.Exception mclslEmptyCatchEx) { MySimulatedLongevityRoad.Core.MclslDiagnostics.Error("empty-catch-code-MySimulatedLongevityRoad-Traits-MclslTraitRegistration-cs-3", "空 catch 捕获: code/MySimulatedLongevityRoad/Traits/MclslTraitRegistration.cs #3: " + mclslEmptyCatchEx.Message); }
    }

    private static string[] A(params string[] ids) => ids;

    private static void AddFirstExistingNativeTrait(Actor actor, string[] candidateIds)
    {
        if (actor?.data == null || candidateIds == null || candidateIds.Length == 0) return;
        for (int i = 0; i < candidateIds.Length; i++)
        {
            string id = candidateIds[i];
            if (string.IsNullOrWhiteSpace(id)) continue;
            try { if (actor.hasTrait(id)) return; } catch (System.Exception mclslEmptyCatchEx) { MySimulatedLongevityRoad.Core.MclslDiagnostics.Error("empty-catch-code-MySimulatedLongevityRoad-Traits-MclslTraitRegistration-cs-4", "空 catch 捕获: code/MySimulatedLongevityRoad/Traits/MclslTraitRegistration.cs #4: " + mclslEmptyCatchEx.Message); }
        }

        for (int i = 0; i < candidateIds.Length; i++)
        {
            string id = candidateIds[i];
            if (string.IsNullOrWhiteSpace(id)) continue;
            try
            {
                ActorTrait trait = AssetManager.traits.get(id);
                if (trait == null) continue;
                actor.addTrait(trait, true);
                return;
            }
            catch (System.Exception mclslEmptyCatchEx) { MySimulatedLongevityRoad.Core.MclslDiagnostics.Error("empty-catch-code-MySimulatedLongevityRoad-Traits-MclslTraitRegistration-cs-5", "空 catch 捕获: code/MySimulatedLongevityRoad/Traits/MclslTraitRegistration.cs #5: " + mclslEmptyCatchEx.Message); }
        }
    }

    internal static void SyncGiftTrait(Actor actor, int aptitude)
    {
        if (actor?.data == null) return;
        string target = GiftTraitIdForAptitude(aptitude);
        MclslTraitGrantRouter.SuppressRouting(() =>
        {
            foreach (MclslAptitudeGiftDefinition gift in MclslAptitudeGiftCatalog.Gifts)
            {
                try { if (gift.TraitId != target && actor.hasTrait(gift.TraitId)) actor.removeTrait(gift.TraitId); } catch (System.Exception mclslEmptyCatchEx) { MySimulatedLongevityRoad.Core.MclslDiagnostics.Error("empty-catch-code-MySimulatedLongevityRoad-Traits-MclslTraitRegistration-cs-6", "空 catch 捕获: code/MySimulatedLongevityRoad/Traits/MclslTraitRegistration.cs #6: " + mclslEmptyCatchEx.Message); }
                string alias = TraitAliasId(gift.TraitId);
                try { if (gift.TraitId != target && actor.hasTrait(alias)) actor.removeTrait(alias); } catch (System.Exception mclslEmptyCatchEx) { MySimulatedLongevityRoad.Core.MclslDiagnostics.Error("empty-catch-code-MySimulatedLongevityRoad-Traits-MclslTraitRegistration-cs-6-alias", "空 catch 捕获: code/MySimulatedLongevityRoad/Traits/MclslTraitRegistration.cs #6-alias: " + mclslEmptyCatchEx.Message); }
                string slashAlias = TraitPathAliasId(gift.TraitId);
                try { if (gift.TraitId != target && actor.hasTrait(slashAlias)) actor.removeTrait(slashAlias); } catch (System.Exception mclslEmptyCatchEx) { MySimulatedLongevityRoad.Core.MclslDiagnostics.Error("empty-catch-code-MySimulatedLongevityRoad-Traits-MclslTraitRegistration-cs-6-path-alias", "空 catch 捕获: code/MySimulatedLongevityRoad/Traits/MclslTraitRegistration.cs #6-path-alias: " + mclslEmptyCatchEx.Message); }
            }
            try { if (!HasTraitWithAlias(actor, target)) actor.addTrait(target); } catch (System.Exception mclslEmptyCatchEx) { MySimulatedLongevityRoad.Core.MclslDiagnostics.Error("empty-catch-code-MySimulatedLongevityRoad-Traits-MclslTraitRegistration-cs-7", "空 catch 捕获: code/MySimulatedLongevityRoad/Traits/MclslTraitRegistration.cs #7: " + mclslEmptyCatchEx.Message); }
        });
        TryAutoCollectGift(actor, target);
        MclslCultivationWake.EnsureAwake(
            actor,
            ensureEntryFromGift: true,
            enqueueAnnual: true,
            refreshUi: true);
    }

    private static void ReconcileGiftTraits(Actor actor)
    {
        MclslAptitudeGiftDefinition best = null;
        foreach (MclslAptitudeGiftDefinition gift in MclslAptitudeGiftCatalog.Gifts)
        {
            if (!HasTraitWithAlias(actor, gift.TraitId)) continue;
            if (best == null || gift.Level > best.Level) best = gift;
        }
        if (best == null) return;
        RemoveOtherGiftTraits(actor, best.TraitId);
        int midpoint = Math.Clamp((best.MinAptitude + best.MaxAptitude) / 2, 1, 100);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.Aptitude, midpoint);
        TryAutoCollectGift(actor, best.TraitId);
        MclslCultivationWake.EnsureAwake(
            actor,
            ensureEntryFromGift: true,
            enqueueAnnual: true,
            refreshUi: true);
    }

    private static void ReconcileRealmTraits(Actor actor)
    {
        if (MclslCultivationStateTransitions.IsProjectionSyncActive) return;
        string storedRealm = MclslActorAccessor.Realm(actor);
        if (!TryBestRealmFromTraits(actor, out string bestRealm))
        {
            string legacyStored = MclslTraitRegistration.LegacyTraitIdForRealm(storedRealm);
            if (!string.IsNullOrWhiteSpace(storedRealm)
                && !HasTraitWithAlias(actor, TraitIdForRealm(storedRealm))
                && (string.IsNullOrWhiteSpace(legacyStored) || !HasTraitWithAlias(actor, legacyStored)))
                MclslCultivationStateTransitions.ClearRealm(
                    actor,
                    MclslRuntime.CurrentYear(),
                    "境界特质已移除，清理境界身份");
            return;
        }

        string system = MclslActorAccessor.GetString(actor, MclslActorDataKeys.CultivationSystem, string.Empty);
        if (MclslSensingQiSystem.ShouldReturnToSensingQi(actor, bestRealm, system))
        {
            MclslSensingQiSystem.ReturnToSensingQi(actor, MclslRuntime.CurrentYear());
            return;
        }

        int minimumTrueEssence = MclslRealmProgress.EntryMinimum(bestRealm);
        if (minimumTrueEssence > 0
            && MclslActorAccessor.GetInt(actor, MclslActorDataKeys.TrueEssence, 0) < minimumTrueEssence)
        {
            bool ancientLaw = MclslActorAccessor.GetString(actor, MclslActorDataKeys.CultivationSystem, string.Empty)
                == MclslCultivationSystemIds.AncientLaw;
            MclslCultivationGrowthSystem.SetTrueEssence(
                actor, bestRealm, minimumTrueEssence, ancientLaw, enforceRealmMinimum: true);
        }

        MclslTraitGrantRouter.SuppressRouting(() =>
        {
            foreach (string realm in MclslRealmIds.Ordered)
            {
                if (realm == bestRealm) continue;
                string trait = TraitIdForRealm(realm);
                string legacyTrait = LegacyTraitIdForRealm(realm);
                try { if (HasTrait(actor, trait)) actor.removeTrait(trait); } catch (System.Exception mclslEmptyCatchEx) { MySimulatedLongevityRoad.Core.MclslDiagnostics.Error("empty-catch-code-MySimulatedLongevityRoad-Traits-MclslTraitRegistration-cs-8", "空 catch 捕获: code/MySimulatedLongevityRoad/Traits/MclslTraitRegistration.cs #8: " + mclslEmptyCatchEx.Message); }
                try { if (!string.IsNullOrWhiteSpace(legacyTrait) && HasTrait(actor, legacyTrait)) actor.removeTrait(legacyTrait); } catch (System.Exception mclslEmptyCatchEx) { MySimulatedLongevityRoad.Core.MclslDiagnostics.Error("empty-catch-code-MySimulatedLongevityRoad-Traits-MclslTraitRegistration-cs-9", "空 catch 捕获: code/MySimulatedLongevityRoad/Traits/MclslTraitRegistration.cs #9: " + mclslEmptyCatchEx.Message); }
            }
        });

        SyncRealmTraitFamily(actor, bestRealm);

        if (!string.Equals(storedRealm, bestRealm, StringComparison.Ordinal))
        {
            MclslCultivationStateTransitions.TrySetRealm(
                actor,
                bestRealm,
                MclslRuntime.CurrentYear(),
                "境界特质同步");
            TryAutoCollectRealm(actor, MclslRealmIds.Index(bestRealm));
        }
    }

    internal static string NormalizeTraitId(string traitId)
    {
        if (string.IsNullOrWhiteSpace(traitId)) return string.Empty;
        string normalized = traitId.Trim().Replace('\\', '/');
        while (true)
        {
            if (normalized.StartsWith("trait_", StringComparison.OrdinalIgnoreCase))
            {
                normalized = normalized.Substring("trait_".Length);
                continue;
            }

            if (normalized.StartsWith("trait/", StringComparison.OrdinalIgnoreCase))
            {
                normalized = normalized.Substring("trait/".Length);
                continue;
            }

            if (normalized.StartsWith("traits/", StringComparison.OrdinalIgnoreCase))
            {
                normalized = normalized.Substring("traits/".Length);
                continue;
            }

            break;
        }
        return normalized;
    }

    internal static string TraitAliasId(string traitId)
    {
        string normalized = NormalizeTraitId(traitId);
        return string.IsNullOrWhiteSpace(normalized) ? string.Empty : "trait_" + normalized;
    }

    internal static string TraitPathAliasId(string traitId)
    {
        string normalized = NormalizeTraitId(traitId);
        return string.IsNullOrWhiteSpace(normalized) ? string.Empty : "trait/" + normalized;
    }

    internal static bool HasTraitWithAlias(Actor actor, string traitId)
    {
        string raw = traitId?.Trim();
        string normalized = NormalizeTraitId(traitId);
        if (string.IsNullOrWhiteSpace(normalized)) return false;
        if (!string.IsNullOrWhiteSpace(raw) && HasTrait(actor, raw)) return true;
        if (HasTrait(actor, normalized)) return true;
        if (HasTrait(actor, TraitAliasId(normalized))) return true;
        return HasTrait(actor, TraitPathAliasId(normalized));
    }

    private static bool HasTrait(Actor actor, string traitId)
    {
        if (actor?.data == null || string.IsNullOrWhiteSpace(traitId)) return false;
        try { return actor.hasTrait(traitId); } catch { return false; }
    }

    private static void EnsureGroup(string id, string name, string color)
    {
        try
        {
            ActorTraitGroupAsset group = AssetManager.trait_groups.get(id);
            if (group == null)
            {
                group = new ActorTraitGroupAsset { id = id };
                AssetManager.trait_groups.add(group);
            }
            group.name = name;
            group.color = color;
        }
        catch (Exception ex) { Debug.LogWarning("[模拟长生路] 特质分组注册失败: " + ex.Message); }
    }

    internal static void SyncRealmTraitFamily(Actor actor, string realm)
    {
        if (actor?.data == null || string.IsNullOrWhiteSpace(realm)) return;
        bool useAncient = UsesAncientRealmTrait(actor);
        MclslTraitGrantRouter.SuppressRouting(() =>
        {
            foreach (string oldRealm in MclslRealmIds.Ordered)
            {
                string newTrait = TraitIdForRealm(oldRealm);
                string ancientTrait = LegacyTraitIdForRealm(oldRealm);
                bool keepNew = !useAncient && oldRealm == realm;
                bool keepAncient = useAncient && oldRealm == realm && oldRealm != MclslRealmIds.ChangSheng;
                try { if (!keepNew && HasTrait(actor, newTrait)) actor.removeTrait(newTrait); } catch (System.Exception mclslEmptyCatchEx) { MySimulatedLongevityRoad.Core.MclslDiagnostics.Error("empty-catch-code-MySimulatedLongevityRoad-Traits-MclslTraitRegistration-cs-10", "空 catch 捕获: code/MySimulatedLongevityRoad/Traits/MclslTraitRegistration.cs #10: " + mclslEmptyCatchEx.Message); }
                try { if (!keepAncient && !string.IsNullOrWhiteSpace(ancientTrait) && HasTrait(actor, ancientTrait)) actor.removeTrait(ancientTrait); } catch (System.Exception mclslEmptyCatchEx) { MySimulatedLongevityRoad.Core.MclslDiagnostics.Error("empty-catch-code-MySimulatedLongevityRoad-Traits-MclslTraitRegistration-cs-11", "空 catch 捕获: code/MySimulatedLongevityRoad/Traits/MclslTraitRegistration.cs #11: " + mclslEmptyCatchEx.Message); }
            }
            string target = useAncient && realm != MclslRealmIds.ChangSheng ? LegacyTraitIdForRealm(realm) : TraitIdForRealm(realm);
            try { if (!string.IsNullOrWhiteSpace(target) && !actor.hasTrait(target)) actor.addTrait(target); } catch (System.Exception mclslEmptyCatchEx) { MySimulatedLongevityRoad.Core.MclslDiagnostics.Error("empty-catch-code-MySimulatedLongevityRoad-Traits-MclslTraitRegistration-cs-12", "空 catch 捕获: code/MySimulatedLongevityRoad/Traits/MclslTraitRegistration.cs #12: " + mclslEmptyCatchEx.Message); }
        });
    }

    internal static void RefreshRealmTraitVisibility(int year, bool force = false)
    {
        bool showNewLaw = MclslWorldEpochSystem.IsNewLawActive(year);
        if (!force && _lastRealmEditorNewLawState.HasValue && _lastRealmEditorNewLawState.Value == showNewLaw)
            return;
        _lastRealmEditorNewLawState = showNewLaw;

        foreach (string realm in MclslRealmIds.Ordered)
        {
            SetTraitEditorVisibility(TraitIdForRealm(realm), showNewLaw);
            SetTraitEditorVisibility(LegacyTraitIdForRealm(realm), !showNewLaw && realm != MclslRealmIds.ChangSheng);
        }
    }

    private static void SetTraitEditorVisibility(string traitId, bool visible)
    {
        if (string.IsNullOrWhiteSpace(traitId)) return;
        try
        {
            ActorTrait trait = AssetManager.traits.get(traitId);
            if (trait == null) return;
            trait.show_in_meta_editor = visible;
            trait.can_be_given = visible;
        }
        catch (System.Exception mclslEmptyCatchEx) { MySimulatedLongevityRoad.Core.MclslDiagnostics.Error("empty-catch-code-MySimulatedLongevityRoad-Traits-MclslTraitRegistration-cs-13", "空 catch 捕获: code/MySimulatedLongevityRoad/Traits/MclslTraitRegistration.cs #13: " + mclslEmptyCatchEx.Message); }
    }

    private static void AddRealm(string realm, string icon, int order, float lifespan, float health, float damage, float speedMultiplier, float armor, bool ancient)
    {
        string id = ancient ? LegacyTraitIdForRealm(realm) : TraitIdForRealm(realm);
        ActorTrait trait = null;
        try { trait = AssetManager.traits.get(id); } catch (System.Exception mclslEmptyCatchEx) { MySimulatedLongevityRoad.Core.MclslDiagnostics.Error("empty-catch-code-MySimulatedLongevityRoad-Traits-MclslTraitRegistration-cs-14", "空 catch 捕获: code/MySimulatedLongevityRoad/Traits/MclslTraitRegistration.cs #14: " + mclslEmptyCatchEx.Message); }
        bool add = trait == null;
        trait ??= new ActorTrait { id = id };
        trait.group_id = ancient ? AncientRealmGroupId : RealmGroupId;
        trait.path_icon = icon;
        string localeKey = id;
        TrySetString(trait, "name", localeKey);
        TrySetString(trait, "name_locale", localeKey);
        TrySetString(trait, "description", localeKey + " Description");
        TrySetString(trait, "description_locale", localeKey + " Description");
        trait.needs_to_be_explored = false;
        trait.rate_birth = 0;
        trait.rate_inherit = 0;
        trait.rate_acquire_grow_up = 0;
        bool canGive = ancient && realm != MclslRealmIds.ChangSheng;
        trait.can_be_given = canGive;
        trait.show_in_meta_editor = canGive;
        ApplyNativeOrderingAndRarity(trait, order, RealmRarity(order));
        trait.base_stats = new BaseStats();
        ApplyRealmStats(trait.base_stats, order, lifespan, health, damage, speedMultiplier, armor, ancient ? 0.9f : 1f);
        if (add) AssetManager.traits.add(trait);
    }

    private static void ApplyRealmStats(BaseStats statsBlock, int order, float lifespan, float health, float damage, float speedMultiplier, float armor, float extraStatMultiplier = 1f)
    {
        if (statsBlock == null) return;
        SafeSetStat(statsBlock, "lifespan", lifespan);
        SafeSetStat(statsBlock, "health", health);
        SafeSetStat(statsBlock, "damage", damage);
        SafeSetStat(statsBlock, "multiplier_speed", speedMultiplier);
        if (armor != 0f) SafeSetStat(statsBlock, "armor", armor);
        void Set(string statId, float amount) => SafeSetStat(statsBlock, statId, amount * extraStatMultiplier);

        switch (order)
        {
            case 1:
                Set("mass", 10f);
                Set("accuracy", 3f);
                Set("stamina", 30f);
                Set("Dodge", 50f);
                Set("Accuracy", 50f);
                Set("resist", 1.5f);
                break;
            case 2:
                Set("resist", 10f);
                Set("warfare", 15f);
                Set("mass", 18f);
                Set("speed", 12f);
                Set("targets", 3f);
                Set("critical_chance", 0.4f);
                Set("accuracy", 25f);
                Set("attack_speed", 6f);
                Set("stamina", 80f);
                Set("Dodge", 120f);
                Set("Accuracy", 120f);
                Set("multiplier_health", 2f);
                Set("multiplier_damage", 2f);
                break;
            case 3:
                Set("resist", 128f);
                Set("warfare", 50f);
                Set("mass", 140f);
                Set("speed", 30f);
                Set("area_of_effect", 4f);
                Set("targets", 12f);
                Set("critical_chance", 0.8f);
                Set("accuracy", 60f);
                Set("attack_speed", 6f);
                Set("stamina", 400f);
                Set("range", 6f);
                Set("multiplier_health", 2f);
                Set("multiplier_damage", 2f);
                Set("Dodge", 500f);
                Set("Accuracy", 500f);
                break;
            case 4:
                Set("resist", 256f);
                Set("warfare", 100f);
                Set("mass", 180f);
                Set("speed", 40f);
                Set("area_of_effect", 5f);
                Set("targets", 16f);
                Set("critical_chance", 0.9f);
                Set("accuracy", 80f);
                Set("attack_speed", 8f);
                Set("stamina", 500f);
                Set("range", 8f);
                Set("multiplier_health", 3f);
                Set("multiplier_damage", 3f);
                Set("Dodge", 1000f);
                Set("Accuracy", 1000f);
                break;
            case 5:
                Set("resist", 512f);
                Set("warfare", 200f);
                Set("mass", 360f);
                Set("speed", 80f);
                Set("area_of_effect", 30f);
                Set("targets", 96f);
                Set("critical_chance", 1.2f);
                Set("accuracy", 160f);
                Set("attack_speed", 10f);
                Set("stamina", 1000f);
                Set("range", 48f);
                Set("multiplier_health", 5f);
                Set("multiplier_damage", 5f);
                Set("Dodge", 10000f);
                Set("Accuracy", 10000f);
                break;
            case 6:
                Set("resist", 768f);
                Set("warfare", 300f);
                Set("mass", 520f);
                Set("speed", 100f);
                Set("area_of_effect", 42f);
                Set("targets", 128f);
                Set("critical_chance", 1.5f);
                Set("accuracy", 240f);
                Set("attack_speed", 12f);
                Set("stamina", 1600f);
                Set("range", 64f);
                Set("scale", 0.05f);
                Set("multiplier_health", 8f);
                Set("multiplier_damage", 8f);
                Set("Dodge", 20000f);
                Set("Accuracy", 20000f);
                break;
            default:
                Set("resist", 1024f);
                Set("warfare", 500f);
                Set("mass", 700f);
                Set("speed", 120f);
                Set("area_of_effect", 60f);
                Set("targets", 180f);
                Set("critical_chance", 1.8f);
                Set("accuracy", 360f);
                Set("attack_speed", 15f);
                Set("stamina", 2400f);
                Set("range", 88f);
                Set("scale", 0.1f);
                Set("multiplier_health", 10f);
                Set("multiplier_damage", 10f);
                Set("Dodge", 40000f);
                Set("Accuracy", 40000f);
                break;
        }
    }

    private static bool SafeSetStat(BaseStats statsBlock, string statId, float amount)
    {
        if (statsBlock == null || string.IsNullOrWhiteSpace(statId)) return false;
        try
        {
            if (AssetManager.base_stats_library == null || AssetManager.base_stats_library.get(statId) == null)
                return false;
            statsBlock.set(statId, amount);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static void AddGift(MclslAptitudeGiftDefinition gift)
    {
        ActorTrait trait = null;
        try { trait = AssetManager.traits.get(gift.TraitId); } catch (System.Exception mclslEmptyCatchEx) { MySimulatedLongevityRoad.Core.MclslDiagnostics.Error("empty-catch-code-MySimulatedLongevityRoad-Traits-MclslTraitRegistration-cs-15", "空 catch 捕获: code/MySimulatedLongevityRoad/Traits/MclslTraitRegistration.cs #15: " + mclslEmptyCatchEx.Message); }
        bool add = trait == null;
        trait ??= new ActorTrait { id = gift.TraitId };
        trait.group_id = GiftGroupId;
        trait.path_icon = "trait/" + gift.TraitId;
        TrySetString(trait, "name", gift.TraitId);
        TrySetString(trait, "name_locale", gift.TraitId);
        TrySetString(trait, "description", gift.TraitId + " Description");
        TrySetString(trait, "description_locale", gift.TraitId + " Description");
        trait.needs_to_be_explored = false;
        trait.rate_birth = 0;
        trait.rate_inherit = 0;
        trait.rate_acquire_grow_up = 0;
        trait.can_be_given = true;
        trait.show_in_meta_editor = true;
        ApplyNativeOrderingAndRarity(trait, gift.Level, GiftRarity(gift.Level));
        trait.base_stats = new BaseStats();
        ApplyGiftStats(trait.base_stats, gift.Level);
        if (add) AssetManager.traits.add(trait);
    }

    private static void ApplyGiftStats(BaseStats statsBlock, int level)
    {
        if (statsBlock == null) return;
        int value = Math.Clamp(level, 1, 6);
        float[] lifespan = { 0f, 10f, 20f, 35f, 60f, 100f, 160f };
        float[] health = { 0f, 30f, 60f, 120f, 220f, 360f, 600f };
        float[] damage = { 0f, 3f, 6f, 12f, 22f, 36f, 60f };
        float[] speed = { 0f, 0.00f, 0.02f, 0.04f, 0.06f, 0.08f, 0.10f };
        float[] accuracy = { 0f, 3f, 6f, 12f, 20f, 32f, 50f };
        float[] stamina = { 0f, 20f, 35f, 60f, 90f, 140f, 220f };
        SafeSetStat(statsBlock, "lifespan", lifespan[value]);
        SafeSetStat(statsBlock, "health", health[value]);
        SafeSetStat(statsBlock, "damage", damage[value]);
        SafeSetStat(statsBlock, "multiplier_speed", speed[value]);
        SafeSetStat(statsBlock, "accuracy", accuracy[value]);
        SafeSetStat(statsBlock, "stamina", stamina[value]);
        SafeSetStat(statsBlock, "critical_chance", value >= 5 ? (value == 6 ? 0.25f : 0.15f) : 0f);
        SafeSetStat(statsBlock, "Dodge", value * 20f);
        SafeSetStat(statsBlock, "Accuracy", value * 20f);
    }

    private static void ApplyNativeOrderingAndRarity(ActorTrait trait, int order, int rarity)
    {
        int displayOrder = 100 - Math.Clamp(order, 1, 99);
        TrySetRarity(trait, rarity);
        TrySet(trait, "rank", displayOrder);
        TrySet(trait, "order", displayOrder);
        TrySet(trait, "sort_order", displayOrder);
        TrySet(trait, "priority", displayOrder);
    }

    private static int RealmRarity(int order) => order <= 3 ? 1 : order <= 5 ? 2 : 3;

    private static int GiftRarity(int level) => level switch
    {
        >= 5 => 3,
        >= 3 => 2,
        _ => 1
    };

    private static void TrySet(object target, string name, int value)
    {
        if (target == null) return;
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        try
        {
            FieldInfo field = target.GetType().GetField(name, flags);
            if (field != null)
            {
                if (field.FieldType == typeof(int)) field.SetValue(target, value);
                else if (field.FieldType == typeof(float)) field.SetValue(target, (float)value);
                return;
            }
            PropertyInfo property = target.GetType().GetProperty(name, flags);
            if (property != null && property.CanWrite)
            {
                if (property.PropertyType == typeof(int)) property.SetValue(target, value);
                else if (property.PropertyType == typeof(float)) property.SetValue(target, (float)value);
            }
        }
        catch (System.Exception mclslEmptyCatchEx) { MySimulatedLongevityRoad.Core.MclslDiagnostics.Error("empty-catch-code-MySimulatedLongevityRoad-Traits-MclslTraitRegistration-cs-16", "空 catch 捕获: code/MySimulatedLongevityRoad/Traits/MclslTraitRegistration.cs #16: " + mclslEmptyCatchEx.Message); }
    }

    private static void TrySetRarity(ActorTrait trait, int rarity)
    {
        if (trait == null || rarity < 0) return;
        try { ((BaseTrait<ActorTrait>)(object)trait).rarity = (Rarity)rarity; } catch (System.Exception mclslEmptyCatchEx) { MySimulatedLongevityRoad.Core.MclslDiagnostics.Error("empty-catch-code-MySimulatedLongevityRoad-Traits-MclslTraitRegistration-cs-17", "空 catch 捕获: code/MySimulatedLongevityRoad/Traits/MclslTraitRegistration.cs #17: " + mclslEmptyCatchEx.Message); }
        TrySet(trait, "rarity", rarity);
        TrySet(trait, "quality", rarity);
    }

    private static void TrySetString(object target, string name, string value)
    {
        if (target == null) return;
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        try
        {
            FieldInfo field = target.GetType().GetField(name, flags);
            if (field != null && field.FieldType == typeof(string))
            {
                field.SetValue(target, value ?? string.Empty);
                return;
            }
            PropertyInfo property = target.GetType().GetProperty(name, flags);
            if (property != null && property.CanWrite && property.PropertyType == typeof(string))
                property.SetValue(target, value ?? string.Empty);
        }
        catch (System.Exception mclslEmptyCatchEx) { MySimulatedLongevityRoad.Core.MclslDiagnostics.Error("empty-catch-code-MySimulatedLongevityRoad-Traits-MclslTraitRegistration-cs-18", "空 catch 捕获: code/MySimulatedLongevityRoad/Traits/MclslTraitRegistration.cs #18: " + mclslEmptyCatchEx.Message); }
    }

    private static void AddSpecial(string id, string icon, bool canBeGiven, bool showInEditor, float lifespan, float health, float damage, float speedMultiplier, float armor = 0f)
    {
        ActorTrait trait = null;
        try { trait = AssetManager.traits.get(id); } catch (System.Exception mclslEmptyCatchEx) { MySimulatedLongevityRoad.Core.MclslDiagnostics.Error("empty-catch-code-MySimulatedLongevityRoad-Traits-MclslTraitRegistration-cs-19", "空 catch 捕获: code/MySimulatedLongevityRoad/Traits/MclslTraitRegistration.cs #19: " + mclslEmptyCatchEx.Message); }
        bool add = trait == null;
        trait ??= new ActorTrait { id = id };
        trait.group_id = SpecialGroupId;
        trait.path_icon = icon;
        TrySetString(trait, "name", id);
        TrySetString(trait, "name_locale", id);
        TrySetString(trait, "description", id + " Description");
        TrySetString(trait, "description_locale", id + " Description");
        trait.needs_to_be_explored = false;
        trait.rate_birth = 0;
        trait.rate_inherit = 0;
        trait.rate_acquire_grow_up = 0;
        trait.can_be_given = canBeGiven;
        trait.show_in_meta_editor = showInEditor;
        trait.base_stats = new BaseStats();
        if (lifespan != 0f) SafeSetStat(trait.base_stats, "lifespan", lifespan);
        if (health != 0f) SafeSetStat(trait.base_stats, "health", health);
        if (damage != 0f) SafeSetStat(trait.base_stats, "damage", damage);
        if (speedMultiplier != 0f) SafeSetStat(trait.base_stats, "multiplier_speed", speedMultiplier);
        if (armor != 0f) SafeSetStat(trait.base_stats, "armor", armor);
        if (string.Equals(id, WorldSoulEntityTraitId, StringComparison.Ordinal))
            ApplyWorldSoulEntityStats(trait.base_stats);
        if (add) AssetManager.traits.add(trait);
    }

    private static void ApplyWorldSoulEntityStats(BaseStats statsBlock)
    {
        if (statsBlock == null) return;
        const float factor = 1.15f;
        SafeSetStat(statsBlock, "resist", 512f * factor);
        SafeSetStat(statsBlock, "warfare", 200f * factor);
        SafeSetStat(statsBlock, "mass", 360f * factor);
        SafeSetStat(statsBlock, "speed", 80f * factor);
        SafeSetStat(statsBlock, "area_of_effect", 30f * factor);
        SafeSetStat(statsBlock, "targets", 96f * factor);
        SafeSetStat(statsBlock, "critical_chance", 1.2f * factor);
        SafeSetStat(statsBlock, "accuracy", 160f * factor);
        SafeSetStat(statsBlock, "attack_speed", 10f * factor);
        SafeSetStat(statsBlock, "stamina", 1000f * factor);
        SafeSetStat(statsBlock, "range", 48f * factor);
        SafeSetStat(statsBlock, "multiplier_health", 5f * factor);
        SafeSetStat(statsBlock, "multiplier_damage", 5f * factor);
        SafeSetStat(statsBlock, "Dodge", 10000f * factor);
        SafeSetStat(statsBlock, "Accuracy", 10000f * factor);
    }

    private static void TryAutoCollectRealm(Actor actor, int realmIndex)
    {
        if (ShouldAutoCollectRealm(realmIndex)) TryMarkFavorite(actor);
    }

    private static void TryAutoCollectGift(Actor actor, string traitId)
    {
        if (ShouldAutoCollectGift(traitId)) TryMarkFavorite(actor);
    }

    private static bool ShouldAutoCollectRealm(int realmIndex)
    {
        if (realmIndex == MclslRealmIds.Index(MclslRealmIds.YuanYing)) return MclslRuntimeSettings.AutoCollectYuanYing;
        if (realmIndex == MclslRealmIds.Index(MclslRealmIds.HuaShen)) return MclslRuntimeSettings.AutoCollectHuaShen;
        if (realmIndex == MclslRealmIds.Index(MclslRealmIds.HeDao)) return MclslRuntimeSettings.AutoCollectHeDao;
        if (realmIndex == MclslRealmIds.Index(MclslRealmIds.ChangSheng)) return MclslRuntimeSettings.AutoCollectChangSheng;
        return false;
    }

    private static bool ShouldAutoCollectGift(string traitId)
    {
        if (string.Equals(traitId, "gifts_5", StringComparison.Ordinal)) return MclslRuntimeSettings.AutoCollectPureRoot;
        if (string.Equals(traitId, "gifts_6", StringComparison.Ordinal)) return MclslRuntimeSettings.AutoCollectHeavenRoot;
        return false;
    }

    private static void TryMarkFavorite(Actor actor)
    {
        try
        {
            if (actor?.data == null) return;
            BaseSystemData data = (BaseSystemData)actor.data;
            data.favorite = true;
        }
        catch (System.Exception mclslEmptyCatchEx) { MySimulatedLongevityRoad.Core.MclslDiagnostics.Error("empty-catch-code-MySimulatedLongevityRoad-Traits-MclslTraitRegistration-cs-20", "空 catch 捕获: code/MySimulatedLongevityRoad/Traits/MclslTraitRegistration.cs #20: " + mclslEmptyCatchEx.Message); }
    }
}
