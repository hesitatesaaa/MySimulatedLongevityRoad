using System;
using System.Collections.Generic;
using System.Reflection;
using MySimulatedLongevityRoad.Core;
using MySimulatedLongevityRoad.Data;
using MySimulatedLongevityRoad.Queries;
using MySimulatedLongevityRoad.Traits;

namespace MySimulatedLongevityRoad.Systems;

internal static class MclslCultivationSystem
{
    private static readonly HashSet<string> ManualRealmGrantGuards = new(StringComparer.Ordinal);
    private const string ManualRealmAnnouncementPrefix = "mclsl.manual_realm_announcement.";

    internal static void ClearRuntimeOnly() { }

    internal static void ProcessAnnualFromScheduler(Actor actor, int year) => ProcessAnnual(actor, year);

    /// <summary>
    /// 修复读档、回档或世界年份重置后残留的未来年度标记。
    /// 年度真元写入由 MclslAnnualCultivationExecutor 统一发放；这里只校正游标。
    /// </summary>
    internal static int NormalizeLastCultivationYear(Actor actor, int year, int lastYear)
    {
        if (actor?.data == null || year <= 0 || lastYear <= year) return lastYear;

        int repairedYear = Math.Max(-1, year - 1);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.LastCultivationYear, repairedYear);
        return repairedYear;
    }

    private static void ProcessAnnual(Actor actor, int year)
    {
        if (!MclslEligibility.CanCultivate(actor)) return;
        ReconcileManualTrait(actor, year);
        string realm = MclslActorAccessor.Realm(actor);
        string cultivationSystem = MclslActorAccessor.GetString(actor, MclslActorDataKeys.CultivationSystem, string.Empty);
        if (cultivationSystem == MclslCultivationSystemIds.AncientLaw)
            return;
        if (MclslSensingQiSystem.ShouldReturnToSensingQi(actor, realm, cultivationSystem))
        {
            MclslSensingQiSystem.ReturnToSensingQi(actor, year);
            realm = string.Empty;
        }
        MclslActorAccessor.ApplyDisplayName(actor, realm);
        if (string.IsNullOrWhiteSpace(realm))
        {
            if (!MclslNewLawEntrySystem.TryBeginFromMortal(actor, year)) return;
            return;
        }
        MclslMindSystem.ProcessAnnual(actor, year, realm);
        if (string.IsNullOrWhiteSpace(MclslActorAccessor.GetString(actor, MclslActorDataKeys.CultivationSystem, string.Empty)))
            MclslCultivationStateTransitions.TrySetCultivationSystem(actor, MclslCultivationSystemIds.NewLaw);
        StabilizeCultivatorSurvival(actor, realm);
        MclslTraitRegistration.SyncNativeRealmTraits(actor, realm);

        bool directHarmonyLeap = MclslRealmIds.Index(realm) >= MclslRealmIds.Index(MclslRealmIds.HeDao)
            && MclslActorAccessor.GetInt(actor, MclslActorDataKeys.HarmonyLeap, 0) == 1;
        if (!directHarmonyLeap) EnsureTechnique(actor);
        MclslResourceSystem.EnsureActorResources(actor);
        ReconcileAptitudeGift(actor);
        MclslGeneratedObjectFactory.MigrateLegacyFoundationWonder(actor, year);
        MclslGeneratedObjectFactory.NormalizeActorRootText(actor);
        EnsureManualStageData(actor, realm, year);
        MclslTaishangSystem.ProcessAnnual(actor, year, realm);

        if (realm == MclslRealmIds.ChangSheng) return;
        if (MclslWorldEpochSystem.IsNewLawActive(year))
        {
            MclslResourceSystem.GrantAnnualStipend(actor, year);
            MclslResourceSystem.TryAutoSpend(actor, year);
        }

        bool ancientLaw = false;
        MclslCultivationGrowthSystem.Reconcile(actor, realm, ancientLaw);
        int aptitude = Math.Clamp(
            MclslActorAccessor.GetInt(actor, MclslActorDataKeys.Aptitude, 50),
            1,
            100);
        MclslNewLawEventSystem.ProcessAnnual(actor, year, realm, aptitude);
        MclslCultivationGrowthSystem.MaybeRecordSameLawPressure(actor, year);

        if (!MclslCultivationGrowthSystem.MeetsNextRealmMinimum(actor, realm, ancientLaw, out int essence, out int requiredEssence))
        {
            if (requiredEssence > 0)
                MclslActorAccessor.Set(actor, MclslActorDataKeys.LastBreakthroughResult,
                    "真元未达下境最低要求：" + essence + "/" + requiredEssence);
            return;
        }
        if (!MclslCultivationAgeSanity.CanAttemptNextRealm(actor, realm, out string ageReason))
        {
            MclslActorAccessor.Set(actor, MclslActorDataKeys.LastBreakthroughResult, ageReason);
            return;
        }
        if (realm == MclslRealmIds.LianQi)
        {
            if (!MclslTechniqueRealmLimit.CanReach(actor, MclslRealmIds.ZhuJi, out string reason)) { MclslActorAccessor.Set(actor, MclslActorDataKeys.LastBreakthroughResult, reason); return; }
            MclslNewLawBreakthroughSystem.TryFoundation(actor, year, aptitude);
        }
        else if (realm == MclslRealmIds.ZhuJi)
        {
            if (!MclslTechniqueRealmLimit.CanReach(actor, MclslRealmIds.JinDan, out string reason)) { MclslActorAccessor.Set(actor, MclslActorDataKeys.LastBreakthroughResult, reason); return; }
            MclslNewLawBreakthroughSystem.TryGoldenCore(actor, year, aptitude);
        }
        else if (realm == MclslRealmIds.JinDan)
        {
            if (!MclslTechniqueRealmLimit.CanReach(actor, MclslRealmIds.YuanYing, out string reason)) { MclslActorAccessor.Set(actor, MclslActorDataKeys.LastBreakthroughResult, reason); return; }
            MclslWorldCaveSystem.RegisterNascentClaim(actor, year);
        }
        else if (realm == MclslRealmIds.YuanYing)
        {
            if (!MclslTechniqueRealmLimit.CanReach(actor, MclslRealmIds.HuaShen, out string reason)) { MclslActorAccessor.Set(actor, MclslActorDataKeys.LastBreakthroughResult, reason); return; }
            MclslWorldChangeSystem.RegisterDivineClaim(actor, year);
        }
        // 合道由天地之魄的祭炼归属触发；当前以原生战斗击杀归属落地。
    }

    private static void StabilizeCultivatorSurvival(Actor actor, string realm)
    {
        if (!MclslActorAccessor.Alive(actor) || string.IsNullOrWhiteSpace(realm)) return;
        object data = actor.data;
        // Do not rewrite native lifespan counters every year. Some WorldBox versions
        // treat these fields as remaining-life state, so refreshing them annually makes
        // cultivators effectively immortal. Cultivation lifespan is enforced once in
        // the annual pipeline instead.
        ApplyNativeLevelFloor(actor, realm);
        if (MclslRealmIds.Index(realm) >= MclslRealmIds.Index(MclslRealmIds.YuanYing))
        {
            TrySetNumber(data, "hunger", 100f);
            TrySetNumber(data, "_hunger", 100f);
            TrySetNumber(data, "food", 100f);
            TrySetNumber(data, "nutrition", 100f);
        }
    }

    private static void ApplyNativeLevelFloor(Actor actor, string realm)
    {
        if (!MclslInverseTruthSystem.IsTruthReversed("truth_player_wounds_forge_body")) return;
        int realmIndex = Math.Max(0, MclslRealmIds.Index(realm) + 1);
        int targetLevel = realmIndex switch
        {
            1 => 2,
            2 => 4,
            3 => 7,
            4 => 10,
            5 => 14,
            6 => 18,
            _ => 22
        };
        object data = actor.data;
        int current = Math.Max(GetNumber(data, "level"), GetNumber(data, "_level"));
        if (current >= targetLevel) return;
        TrySetNumber(data, "level", targetLevel);
        TrySetNumber(data, "_level", targetLevel);
        TrySetNumber(data, "experience", Math.Max(GetNumber(data, "experience"), targetLevel * 100));
        TrySetNumber(data, "xp", Math.Max(GetNumber(data, "xp"), targetLevel * 100));
        try { actor.updateStats(); } catch (System.Exception mclslEmptyCatchEx) { MySimulatedLongevityRoad.Core.MclslDiagnostics.Error("empty-catch-code-MySimulatedLongevityRoad-Systems-Cultivation-MclslCultivationSystem-cs-2", "空 catch 捕获: code/MySimulatedLongevityRoad/Systems/Cultivation/MclslCultivationSystem.cs #2: " + mclslEmptyCatchEx.Message); }
    }

    private static int GetNumber(object target, string name)
    {
        if (target == null) return 0;
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.IgnoreCase;
        try
        {
            FieldInfo field = target.GetType().GetField(name, flags);
            if (field != null)
            {
                object value = field.GetValue(target);
                if (value is int i) return i;
                if (value is float f) return (int)f;
            }
            PropertyInfo property = target.GetType().GetProperty(name, flags);
            if (property != null)
            {
                object value = property.GetValue(target, null);
                if (value is int i) return i;
                if (value is float f) return (int)f;
            }
        }
        catch (System.Exception mclslEmptyCatchEx) { MySimulatedLongevityRoad.Core.MclslDiagnostics.Error("empty-catch-code-MySimulatedLongevityRoad-Systems-Cultivation-MclslCultivationSystem-cs-3", "空 catch 捕获: code/MySimulatedLongevityRoad/Systems/Cultivation/MclslCultivationSystem.cs #3: " + mclslEmptyCatchEx.Message); }
        return 0;
    }

    private static void TrySetNumber(object target, string name, float value)
    {
        if (target == null) return;
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.IgnoreCase;
        try
        {
            FieldInfo field = target.GetType().GetField(name, flags);
            if (field != null)
            {
                if (field.FieldType == typeof(float)) field.SetValue(target, value);
                else if (field.FieldType == typeof(int)) field.SetValue(target, (int)value);
                return;
            }
            PropertyInfo property = target.GetType().GetProperty(name, flags);
            if (property != null && property.CanWrite)
            {
                if (property.PropertyType == typeof(float)) property.SetValue(target, value);
                else if (property.PropertyType == typeof(int)) property.SetValue(target, (int)value);
            }
        }
        catch (System.Exception mclslEmptyCatchEx) { MySimulatedLongevityRoad.Core.MclslDiagnostics.Error("empty-catch-code-MySimulatedLongevityRoad-Systems-Cultivation-MclslCultivationSystem-cs-4", "空 catch 捕获: code/MySimulatedLongevityRoad/Systems/Cultivation/MclslCultivationSystem.cs #4: " + mclslEmptyCatchEx.Message); }
    }

    private static MclslAptitudeGiftDefinition Gift(Actor actor)
    {
        int aptitude = Math.Clamp(MclslActorAccessor.GetInt(actor, MclslActorDataKeys.Aptitude, 50), 1, 100);
        return MclslSpiritualRootSystem.GiftForCultivation(actor) ?? MclslAptitudeGiftCatalog.ForAptitude(aptitude);
    }

    private static void ReconcileAptitudeGift(Actor actor)
    {
        int manualAptitude = 0;
        foreach (MclslAptitudeGiftDefinition gift in MclslAptitudeGiftCatalog.Gifts)
        {
            try
            {
                if (actor.hasTrait(gift.TraitId))
                {
                    manualAptitude = Math.Max(manualAptitude, (gift.MinAptitude + gift.MaxAptitude) / 2);
                }
            }
            catch (System.Exception mclslEmptyCatchEx) { MySimulatedLongevityRoad.Core.MclslDiagnostics.Error("empty-catch-code-MySimulatedLongevityRoad-Systems-Cultivation-MclslCultivationSystem-cs-5", "空 catch 捕获: code/MySimulatedLongevityRoad/Systems/Cultivation/MclslCultivationSystem.cs #5: " + mclslEmptyCatchEx.Message); }
        }
        if (manualAptitude > 0) MclslActorAccessor.Set(actor, MclslActorDataKeys.Aptitude, manualAptitude);
        MclslTraitRegistration.SyncGiftTrait(actor, Math.Clamp(MclslActorAccessor.GetInt(actor, MclslActorDataKeys.Aptitude, manualAptitude > 0 ? manualAptitude : 50), 1, 100));
    }

    private static void ReconcileManualTrait(Actor actor, int year)
    {
        if (HasForbiddenAncientLongevity(actor, year))
        {
            RejectForbiddenAncientLongevity(actor, string.Empty, year);
            return;
        }

        string current = MclslActorAccessor.Realm(actor);
        for (int i = MclslRealmIds.Ordered.Length - 1; i >= 0; i--)
        {
            string realm = MclslRealmIds.Ordered[i];
            string traitId = MclslTraitRegistration.TraitIdForRealm(realm);
            string legacyTraitId = MclslTraitRegistration.LegacyTraitIdForRealm(realm);
            bool hasNewRealmTrait = actor.hasTrait(traitId);
            bool hasLegacyRealmTrait = !string.IsNullOrWhiteSpace(legacyTraitId) && actor.hasTrait(legacyTraitId);
            bool hasRealmTrait = hasNewRealmTrait || hasLegacyRealmTrait;
            if (!hasRealmTrait) continue;
            if (MclslRealmIds.Index(realm) > MclslRealmIds.Index(current))
            {
                ApplyManualRealmGrant(actor, hasNewRealmTrait ? traitId : legacyTraitId, realm, year, hasLegacyRealmTrait && !hasNewRealmTrait, hasNewRealmTrait);
            }
            return;
        }
    }

    internal static void ApplyManualRealmGrant(Actor actor, string traitId, string realm, int year, bool forceAncient = false, bool forceNewLaw = false)
    {
        if (actor?.data == null || string.IsNullOrWhiteSpace(realm)) return;
        string guard = MclslActorAccessor.Id(actor) + "|" + realm + "|" + traitId;
        if (!ManualRealmGrantGuards.Add(guard)) return;
        try
        {
            if (!MclslEligibility.CanCultivate(actor))
            {
                MclslTraitGrantRouter.SuppressRouting(() =>
                {
                    try { if (!string.IsNullOrWhiteSpace(traitId) && actor.hasTrait(traitId)) actor.removeTrait(traitId); } catch (System.Exception mclslEmptyCatchEx) { MySimulatedLongevityRoad.Core.MclslDiagnostics.Error("empty-catch-code-MySimulatedLongevityRoad-Systems-Cultivation-MclslCultivationSystem-cs-6", "空 catch 捕获: code/MySimulatedLongevityRoad/Systems/Cultivation/MclslCultivationSystem.cs #6: " + mclslEmptyCatchEx.Message); }
                });
                MclslCultivationStateTransitions.ClearRealm(actor, year, "角色不满足修炼资格");
                return;
            }

            if (IsLongevityForbiddenBeforeNewLaw(realm, year))
            {
                RejectForbiddenAncientLongevity(actor, traitId, year);
                return;
            }

            if (!MclslRealmSeatSystem.CanGrantRealm(actor, realm, out string seatReason))
            {
                MclslActorAccessor.Set(actor, MclslActorDataKeys.LastBreakthroughResult, "玩家手动赋予境界，已越过席位限制：" + seatReason);
            }

            MclslActorAccessor.Set(actor, MclslActorDataKeys.ManualGrant, 1);
            bool ancientManualGrant = realm != MclslRealmIds.ChangSheng && !forceNewLaw && (forceAncient || ShouldUseAncientManualGrant(actor, year));
            EnsureTechnique(actor, ancientManualGrant);
            if (ancientManualGrant)
            {
                int aptitude = Math.Clamp(MclslActorAccessor.GetInt(actor, MclslActorDataKeys.Aptitude, 50), 1, 100);
                MclslCultivationStateTransitions.TrySetCultivationSystem(actor, MclslCultivationSystemIds.AncientLaw);
                MclslActorAccessor.Set(actor, MclslActorDataKeys.AncientLawStatus, MclslWorldEpochSystem.IsNewLawActive(year) ? "旧法遗修" : "仙道正修");
                if (MclslActorAccessor.GetInt(actor, MclslActorDataKeys.AncientLineageStrength, 0) <= 0)
                    MclslActorAccessor.Set(actor, MclslActorDataKeys.AncientLineageStrength, 35 + aptitude / 2);
                if (MclslActorAccessor.GetInt(actor, MclslActorDataKeys.AncientLegacyPotential, 0) <= 0)
                    MclslActorAccessor.Set(actor, MclslActorDataKeys.AncientLegacyPotential, 20 + aptitude / 4);
                MclslAncientLawBreakthroughSystem.EnsureAncientStageData(actor, realm, year, aptitude);
                MclslTechniqueRealmLimit.EnsureAtLeastRealm(actor, realm);
            }
            else
            {
                MclslCultivationStateTransitions.TrySetCultivationSystem(actor, MclslCultivationSystemIds.NewLaw);
                MclslActorAccessor.Set(actor, MclslActorDataKeys.AncientLawStatus, string.Empty);
                EnsureManualStageData(actor, realm, year);
                if (realm == MclslRealmIds.ChangSheng) EnsureManualLongevityData(actor, year);
                if (!ManualStageSatisfied(actor, realm)) EnsureManualStageData(actor, realm, year);
            }

            SetRealm(actor, realm, year, "玩家手动赋予境界");
            if (!ancientManualGrant && MclslRealmIds.Index(realm) >= MclslRealmIds.Index(MclslRealmIds.HeDao))
                MclslWorldSoulSystem.EnsureManualHarmonyData(actor, year);
            if (!ancientManualGrant && realm == MclslRealmIds.ChangSheng) EnsureManualLongevityData(actor, year);
            MclslTraitRegistration.SyncRealmTraitFamily(actor, realm);
            MclslTraitRegistration.SyncNativeRealmTraits(actor, realm);
            if (realm == MclslRealmIds.ChangSheng) MclslTraitRegistration.EnsureNativeImmortal(actor);
            MclslTechniqueRealmLimit.EnsureAtLeastRealm(actor, realm);
            MclslActorAccessor.ApplyDisplayName(actor, realm);
            try
            {
                actor.updateStats();
                float max = actor.getMaxHealth();
                if (max > 0f) actor.data.health = Math.Max(1, (int)Math.Min((float)int.MaxValue, max));
            }
            catch (System.Exception mclslEmptyCatchEx) { MySimulatedLongevityRoad.Core.MclslDiagnostics.Error("empty-catch-code-MySimulatedLongevityRoad-Systems-Cultivation-MclslCultivationSystem-cs-7", "空 catch 捕获: code/MySimulatedLongevityRoad/Systems/Cultivation/MclslCultivationSystem.cs #7: " + mclslEmptyCatchEx.Message); }
            RegisterHighRealmBreakthrough(actor, realm, year);
            MclslWorldActorQuery.MarkDirty();
        }
        finally
        {
            ManualRealmGrantGuards.Remove(guard);
        }
    }

    private static void RegisterHighRealmBreakthrough(Actor actor, string realm, int year)
    {
        if (MclslRealmIds.Index(realm) < MclslRealmIds.Index(MclslRealmIds.YuanYing)) return;
        bool ancient = MclslActorAccessor.GetString(actor, MclslActorDataKeys.CultivationSystem, string.Empty) == MclslCultivationSystemIds.AncientLaw;
        if (ancient && realm == MclslRealmIds.ChangSheng) return;

        string key = ManualRealmAnnouncementPrefix + realm;
        if (MclslActorAccessor.GetInt(actor, key, -1) == year) return;
        MclslActorAccessor.Set(actor, key, year);
        string name = MclslActorAccessor.DisplayName(actor, realm);
        string title;
        string body;
        string type;
        string color;
        if (realm == MclslRealmIds.YuanYing)
        {
            string essence = MclslActorAccessor.GetString(actor, MclslActorDataKeys.NascentEssenceName, "天地之精");
            title = name + "成就元婴";
            body = ancient ? "丹破婴生，神魂成形。" : "吞“" + essence + "”之精，以成元婴。";
            type = ancient ? "ancient_law_breakthrough" : "nascent_soul";
            color = "#9CD7FF";
        }
        else if (realm == MclslRealmIds.HuaShen)
        {
            string marrow = MclslActorAccessor.GetString(actor, MclslActorDataKeys.DivineMarrowName, "天地之髓");
            title = name + "成就化神";
            body = ancient ? "神魂与自身大道相融，化神意成。" : "抽“" + marrow + "”之髓，以化其神。";
            type = ancient ? "ancient_law_breakthrough" : "divine_transformation";
            color = "#B7A7FF";
        }
        else if (realm == MclslRealmIds.HeDao)
        {
            string soul = MclslActorAccessor.GetString(actor, MclslActorDataKeys.WorldSoulName, "无名天地之魄");
            title = name + "合道";
            body = ancient ? "自身大道合于天地。" : "祭“" + soul + "”之魄，以身合道。";
            type = ancient ? "ancient_law_breakthrough" : "harmony_last_hit";
            color = "#FFD37A";
        }
        else if (realm == MclslRealmIds.ChangSheng)
        {
            string truth = MclslActorAccessor.GetString(actor, MclslActorDataKeys.InverseTruthName, "未名逆理");
            if (string.IsNullOrWhiteSpace(truth) || truth == "无名逆理") truth = "未名逆理";
            title = name + "证得长生";
            body = ancient ? "仙道根基积至极处，身与大道长契。" : "逆天地之理“" + truth + "”，以证长生。";
            type = ancient ? "ancient_law_breakthrough" : "longevity_achieved";
            color = "#B7A7FF";
        }
        else return;

        MclslWorldRunRepository.AddEvent(year, type, title, body, actor);
        bool popup = true;
        if (realm == MclslRealmIds.YuanYing) popup = MclslRuntimeSettings.NascentBreakthroughAnnouncementsEnabled;
        else if (realm == MclslRealmIds.HuaShen) popup = MclslRuntimeSettings.DivineBreakthroughAnnouncementsEnabled;
        if (popup) MclslAnnouncementSystem.Enqueue(title + "。", color, 9f, 1);
    }

    private static void EnsureManualLongevityData(Actor actor, int year)
    {
        if (!string.IsNullOrWhiteSpace(MclslActorAccessor.GetString(actor, MclslActorDataKeys.InverseTruthName, string.Empty)))
        {
            if (MclslActorAccessor.GetInt(actor, MclslActorDataKeys.InverseTruthProgress, 0) < 100)
                MclslActorAccessor.Set(actor, MclslActorDataKeys.InverseTruthProgress, 100);
            return;
        }

        MclslWorldRunRepository.EnsureNewLawCatalogs(year);
        MclslWorldRunState run = MclslWorldRunRepository.Current;
        MclslInverseTruthRecord truth = null;
        if (run?.InverseTruths != null)
        {
            foreach (MclslInverseTruthRecord candidate in run.InverseTruths)
            {
                if (candidate == null || !candidate.CountsTowardLongevity) continue;
                if (candidate.Reversed && candidate.ChallengerActorId != MclslActorAccessor.Id(actor)) continue;
                truth = candidate;
                break;
            }
        }

        if (truth != null)
        {
            truth.Reversed = true;
            truth.Progress = 100;
            truth.ChallengerActorId = MclslActorAccessor.Id(actor);
            MclslActorAccessor.Set(actor, MclslActorDataKeys.InverseTruthId, truth.Id);
            MclslActorAccessor.Set(actor, MclslActorDataKeys.InverseTruthName, truth.Name);
            MclslActorAccessor.Set(actor, MclslActorDataKeys.InverseTruthProgress, 100);
        }
        else
        {
            MclslActorAccessor.Set(actor, MclslActorDataKeys.InverseTruthId, "truth_manual_longevity");
            MclslActorAccessor.Set(actor, MclslActorDataKeys.InverseTruthName, "未名逆理");
            MclslActorAccessor.Set(actor, MclslActorDataKeys.InverseTruthProgress, 100);
        }
    }

    private static bool ShouldUseAncientManualGrant(Actor actor, int year)
    {
        string system = MclslActorAccessor.GetString(actor, MclslActorDataKeys.CultivationSystem, string.Empty);
        if (system == MclslCultivationSystemIds.AncientLaw) return true;
        if (system == MclslCultivationSystemIds.NewLaw) return false;
        return !MclslWorldEpochSystem.IsNewLawActive(year);
    }

    private static void EnsureTechnique(Actor actor, bool ancientPath = false)
    {
        if (!string.IsNullOrWhiteSpace(MclslActorAccessor.GetString(actor, MclslActorDataKeys.TechniqueId, string.Empty))) return;
        long id = MclslActorAccessor.Id(actor);
        int aptitude = MclslActorAccessor.GetInt(actor, MclslActorDataKeys.Aptitude, 0);
        if (aptitude <= 0) MclslActorAccessor.Set(actor, MclslActorDataKeys.Aptitude, 20 + PositiveHash(id + "|manual_apt") % 81);
        ReconcileAptitudeGift(actor);
        string system = MclslActorAccessor.GetString(actor, MclslActorDataKeys.CultivationSystem, string.Empty);
        bool ancient = ancientPath
            || system == MclslCultivationSystemIds.AncientLaw
            || (system != MclslCultivationSystemIds.NewLaw && !MclslWorldEpochSystem.IsNewLawActive(MclslRuntime.CurrentYear()));
        string techniqueSeed = id > 0L ? id.ToString() : MclslActorAccessor.DisplayName(actor) + "|" + MclslRuntime.CurrentYear();
        MclslTechniqueDefinition technique = MclslTechniqueOccupationSystem.SelectStartingTechnique(techniqueSeed, MclslActorAccessor.GetInt(actor, MclslActorDataKeys.Aptitude, 50), ancient);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.TechniqueId, ancient ? "spiritual_" + technique.Id : technique.Id);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.TechniqueName, technique.Name);
        MclslTechniqueRealmLimit.EnsureFromDefinition(actor, technique);
    }

    internal static void EnsureManualStageData(Actor actor, string realm, int year)
    {
        int target = MclslRealmIds.Index(realm);
        // 夺魄捡漏者可以从凡人或低境直接立地合道；不得为了补面板而伪造其筑基、金丹、元婴与化神经历。
        if (target >= MclslRealmIds.Index(MclslRealmIds.HeDao)
            && MclslActorAccessor.GetInt(actor, MclslActorDataKeys.HarmonyLeap, 0) == 1)
        {
            MclslWorldSoulSystem.EnsureManualHarmonyData(actor, year);
            return;
        }
        if (target >= MclslRealmIds.Index(MclslRealmIds.ZhuJi) && string.IsNullOrWhiteSpace(MclslActorAccessor.GetString(actor, MclslActorDataKeys.FoundationWonderName, string.Empty)))
        {
            MclslNewLawBreakthroughSystem.EnsureManualFoundationData(actor, year);
        }
        if (target >= MclslRealmIds.Index(MclslRealmIds.JinDan) && string.IsNullOrWhiteSpace(MclslActorAccessor.GetString(actor, MclslActorDataKeys.GoldenCoreLaws, string.Empty)))
            MclslNewLawBreakthroughSystem.BuildGoldenCoreData(actor, year, Math.Clamp(MclslActorAccessor.GetInt(actor, MclslActorDataKeys.Aptitude, 50), 1, 100));
        if (target >= MclslRealmIds.Index(MclslRealmIds.YuanYing) && string.IsNullOrWhiteSpace(MclslActorAccessor.GetString(actor, MclslActorDataKeys.NascentEssenceName, string.Empty)))
            MclslWorldCaveSystem.EnsureManualNascentData(actor, year);
        if (target >= MclslRealmIds.Index(MclslRealmIds.HuaShen) && string.IsNullOrWhiteSpace(MclslActorAccessor.GetString(actor, MclslActorDataKeys.DivineMarrowName, string.Empty)))
            MclslWorldChangeSystem.EnsureManualDivineData(actor, year);
        if (target >= MclslRealmIds.Index(MclslRealmIds.HeDao) && string.IsNullOrWhiteSpace(MclslActorAccessor.GetString(actor, MclslActorDataKeys.WorldSoulId, string.Empty)))
            MclslWorldSoulSystem.EnsureManualHarmonyData(actor, year);
    }

    internal static bool ManualStageSatisfied(Actor actor, string realm)
    {
        int target = MclslRealmIds.Index(realm);
        if (target >= MclslRealmIds.Index(MclslRealmIds.ZhuJi)
            && string.IsNullOrWhiteSpace(MclslActorAccessor.GetString(actor, MclslActorDataKeys.FoundationWonderName, string.Empty)))
            return false;
        if (target >= MclslRealmIds.Index(MclslRealmIds.JinDan)
            && string.IsNullOrWhiteSpace(MclslActorAccessor.GetString(actor, MclslActorDataKeys.GoldenCoreLaws, string.Empty)))
            return false;
        if (target >= MclslRealmIds.Index(MclslRealmIds.YuanYing)
            && string.IsNullOrWhiteSpace(MclslActorAccessor.GetString(actor, MclslActorDataKeys.NascentEssenceName, string.Empty)))
            return false;
        if (target >= MclslRealmIds.Index(MclslRealmIds.HuaShen)
            && string.IsNullOrWhiteSpace(MclslActorAccessor.GetString(actor, MclslActorDataKeys.DivineMarrowName, string.Empty)))
            return false;
        if (target >= MclslRealmIds.Index(MclslRealmIds.HeDao)
            && string.IsNullOrWhiteSpace(MclslActorAccessor.GetString(actor, MclslActorDataKeys.WorldSoulId, string.Empty)))
            return false;
        return true;
    }

    internal static void RestoreFromHuanzhen(Actor actor, MclslHuanzhenCultivationSnapshot snapshot, int anchorYear, int deathYear)
    {
        if (actor?.data == null || snapshot == null) return;
        string realm = snapshot.RealmId ?? string.Empty;
        string restoredSystem = snapshot.CultivationSystemId ?? string.Empty;
        if (restoredSystem == MclslCultivationSystemIds.AncientLaw
            || restoredSystem == MclslCultivationSystemIds.NewLaw)
            MclslCultivationStateTransitions.TrySetCultivationSystem(actor, restoredSystem);
        if (!string.IsNullOrWhiteSpace(realm)) SetRealm(actor, realm, anchorYear, "还真回溯后恢复" + deathYear + "年身死前修为");
        else
        {
            MclslCultivationStateTransitions.ClearRealm(
                actor,
                anchorYear,
                "还真回溯后恢复凡人或感气状态");
        }

        int restoredEssence = snapshot.TrueEssence > 0
            ? snapshot.TrueEssence
            : MclslRealmProgress.EssenceAtProgress(realm,
                MclslActorAccessor.GetString(actor, MclslActorDataKeys.CultivationSystem, string.Empty) == MclslCultivationSystemIds.AncientLaw,
                snapshot.CultivationProgress);
        MclslCultivationGrowthSystem.SetTrueEssence(actor, realm, restoredEssence,
            MclslActorAccessor.GetString(actor, MclslActorDataKeys.CultivationSystem, string.Empty) == MclslCultivationSystemIds.AncientLaw,
            enforceRealmMinimum: !string.IsNullOrWhiteSpace(realm));
        MclslActorAccessor.Set(actor, MclslActorDataKeys.Aptitude, snapshot.Aptitude);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.MindState, snapshot.MindState);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.HeartTemperingProgress, snapshot.HeartTemperingProgress);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.HeartMethodKnown, snapshot.HeartMethodKnown);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.MiasmaPoolCleansing, snapshot.MiasmaPoolCleansing);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.Contribution, snapshot.Contribution);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.RuinExperience, snapshot.RuinExperience);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.TechniqueInsight, snapshot.TechniqueInsight);
        SetString(actor, MclslActorDataKeys.TechniqueId, snapshot.TechniqueId);
        SetString(actor, MclslActorDataKeys.TechniqueName, snapshot.TechniqueName);
        SetString(actor, MclslActorDataKeys.TechniqueMaxRealm, snapshot.TechniqueMaxRealm);
        SetString(actor, MclslActorDataKeys.FoundationWonderId, snapshot.FoundationWonderId);
        SetString(actor, MclslActorDataKeys.FoundationWonderName, snapshot.FoundationWonderName);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.FoundationWonderQuality, snapshot.FoundationWonderQuality);
        SetString(actor, MclslActorDataKeys.FoundationWonderTags, snapshot.FoundationWonderTags);
        SetString(actor, MclslActorDataKeys.FoundationWonderDescription, snapshot.FoundationWonderDescription);
        SetString(actor, MclslActorDataKeys.FoundationWonderOrigin, snapshot.FoundationWonderOrigin);
        SetString(actor, MclslActorDataKeys.FoundationWonderEffects, snapshot.FoundationWonderEffects);
        SetString(actor, MclslActorDataKeys.GoldenCoreLaws, snapshot.GoldenCoreLaws);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.GoldenCorePurity, snapshot.GoldenCorePurity);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.GoldenCoreStability, snapshot.GoldenCoreStability);
        SetString(actor, MclslActorDataKeys.NascentCaveId, snapshot.NascentCaveId);
        SetString(actor, MclslActorDataKeys.NascentCaveName, snapshot.NascentCaveName);
        SetString(actor, MclslActorDataKeys.NascentCaveTags, snapshot.NascentCaveTags);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.NascentCaveCompatibility, snapshot.NascentCaveCompatibility);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.NascentCaveIntegrity, snapshot.NascentCaveIntegrity);
        SetString(actor, MclslActorDataKeys.NascentEssenceId, snapshot.NascentEssenceId);
        SetString(actor, MclslActorDataKeys.NascentEssenceName, snapshot.NascentEssenceName);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.NascentEssenceQuality, snapshot.NascentEssenceQuality);
        SetString(actor, MclslActorDataKeys.NascentEssenceTags, snapshot.NascentEssenceTags);
        SetString(actor, MclslActorDataKeys.NascentEssenceDescription, snapshot.NascentEssenceDescription);
        SetString(actor, MclslActorDataKeys.NascentEssenceEffects, snapshot.NascentEssenceEffects);
        SetString(actor, MclslActorDataKeys.DivineChangeId, snapshot.DivineChangeId);
        SetString(actor, MclslActorDataKeys.DivineChangeName, snapshot.DivineChangeName);
        SetString(actor, MclslActorDataKeys.DivineChangeTags, snapshot.DivineChangeTags);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.DivineChangeCompatibility, snapshot.DivineChangeCompatibility);
        SetString(actor, MclslActorDataKeys.DivineMarrowId, snapshot.DivineMarrowId);
        SetString(actor, MclslActorDataKeys.DivineMarrowName, snapshot.DivineMarrowName);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.DivineMarrow, snapshot.DivineMarrowCount);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.DivineMarrowQuality, snapshot.DivineMarrowQuality);
        SetString(actor, MclslActorDataKeys.DivineMarrowTags, snapshot.DivineMarrowTags);
        SetString(actor, MclslActorDataKeys.DivineMarrowDescription, snapshot.DivineMarrowDescription);
        SetString(actor, MclslActorDataKeys.DivineMarrowEffects, snapshot.DivineMarrowEffects);
        SetString(actor, MclslActorDataKeys.HuaShenHonorific, snapshot.HuaShenHonorific);
        SetString(actor, MclslActorDataKeys.HeDaoHonorific, snapshot.HeDaoHonorific);
        SetString(actor, MclslActorDataKeys.ChangShengHonorific, snapshot.ChangShengHonorific);
        SetString(actor, MclslActorDataKeys.AncientHuaShenHonorific, snapshot.AncientHuaShenHonorific);
        SetString(actor, MclslActorDataKeys.AncientHeDaoHonorific, snapshot.AncientHeDaoHonorific);
        SetString(actor, MclslActorDataKeys.AncientChangShengHonorific, snapshot.AncientChangShengHonorific);
        SetString(actor, MclslActorDataKeys.WorldSoulId, snapshot.WorldSoulId);
        SetString(actor, MclslActorDataKeys.WorldSoulName, snapshot.WorldSoulName);
        SetString(actor, MclslActorDataKeys.HeavenlyDuty, snapshot.HeavenlyDuty);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.HarmonyLeap, snapshot.HarmonyLeap);
        SetString(actor, MclslActorDataKeys.HarmonyOrigin, snapshot.HarmonyOrigin);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.HarmonyCompatibility, snapshot.HarmonyCompatibility);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.HarmonyStability, snapshot.HarmonyStability);
        SetString(actor, MclslActorDataKeys.InverseTruthId, snapshot.InverseTruthId);
        SetString(actor, MclslActorDataKeys.InverseTruthName, snapshot.InverseTruthName);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.InverseTruthProgress, snapshot.InverseTruthProgress);
        // Version 2 snapshots preserve roots, resources, ancient-law stages and
        // post-longevity state through an allow-listed extensible state bag.
        MclslHuanzhenSystem.RestoreSupplementalState(actor, snapshot);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.LastCultivationYear, anchorYear - 1);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.LastBreakthroughYear, deathYear);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.LastBreakthroughResult, "还真保留死前境界与全部修行根基");
        MclslActorAccessor.ApplyDisplayName(actor, realm);
    }

    internal static bool RestoreHuanzhenLegacyCategory(Actor actor, MclslHuanzhenCultivationSnapshot snapshot, string category, int anchorYear, int deathYear, out string message)
    {
        message = string.Empty;
        if (actor?.data == null || snapshot == null) { message = "人物或前世快照无效。"; return false; }
        switch (category)
        {
            case "cultivation":
                RestoreLegacyCultivation(actor, snapshot, anchorYear, deathYear);
                message = "已继承前世修为根基。";
                break;
            case "technique":
                RestoreLegacyTechnique(actor, snapshot);
                message = "已继承前世功法道统。";
                break;
            case "treasures":
                RestoreLegacyTreasures(actor, snapshot);
                message = "已继承前世突破造物。";
                break;
            case "dao":
                if ((!string.IsNullOrWhiteSpace(snapshot.WorldSoulId) || !string.IsNullOrWhiteSpace(snapshot.InverseTruthId))
                    && MclslRealmIds.Index(MclslActorAccessor.Realm(actor)) < MclslRealmIds.Index(MclslRealmIds.HeDao))
                {
                    message = "天地道果需要合道根基；请先选择“修为根基”。";
                    return false;
                }
                RestoreLegacyDao(actor, snapshot);
                MclslWorldSoulSystem.ReconcileRestoredHolder(actor, snapshot, MclslRuntime.CurrentYear());
                message = "已继承前世天地道果。";
                break;
            case "resources":
                RestoreLegacyResources(actor, snapshot);
                message = "已继承前世资源与心境。";
                break;
            default:
                message = "未知的前世遗产类别。";
                return false;
        }
        MclslActorAccessor.Set(actor, MclslActorDataKeys.LastBreakthroughResult, message);
        MclslActorAccessor.ApplyDisplayName(actor, MclslActorAccessor.Realm(actor));
        try { actor.updateStats(); } catch (Exception ex) { MclslDiagnostics.Error("huanzhen-legacy-stats", "刷新前世遗产属性失败: " + ex.Message); }
        MclslWorldActorQuery.MarkDirty();
        return true;
    }

    internal static bool RestoreHuanzhenLegacyOption(Actor actor, MclslHuanzhenCultivationSnapshot snapshot, string optionId, int anchorYear, int deathYear, out string message)
    {
        message = string.Empty;
        if (actor?.data == null || snapshot == null) { message = "人物或前世快照无效。"; return false; }
        switch (optionId ?? string.Empty)
        {
            case "cultivation": RestoreLegacyCultivation(actor, snapshot, anchorYear, deathYear); message = "已继承前世修为根基。"; break;
            case "technique": RestoreLegacyTechnique(actor, snapshot); message = "已继承前世功法道统。"; break;
            case "foundation_wonder": RestoreLegacyFoundationWonder(actor, snapshot); message = "已继承突破造物：" + snapshot.FoundationWonderName + "。"; break;
            case "golden_core": RestoreLegacyGoldenCore(actor, snapshot); message = "已继承金丹法则：" + snapshot.GoldenCoreLaws + "。"; break;
            case "nascent_cave": RestoreLegacyNascentCave(actor, snapshot); message = "已继承元婴洞天：" + snapshot.NascentCaveName + "。"; break;
            case "nascent_essence": RestoreLegacyNascentEssence(actor, snapshot); message = "已继承天地之精：" + snapshot.NascentEssenceName + "。"; break;
            case "divine_change": RestoreLegacyDivineChange(actor, snapshot); message = "已继承神通变化：" + snapshot.DivineChangeName + "。"; break;
            case "divine_marrow": RestoreLegacyDivineMarrow(actor, snapshot); message = "已继承天地之髓：" + snapshot.DivineMarrowName + "。"; break;
            case "world_soul": RestoreLegacyWorldSoul(actor, snapshot); message = "已继承天地之魄：" + snapshot.WorldSoulName + "。"; break;
            case "inverse_truth": RestoreLegacyInverseTruth(actor, snapshot); message = "已继承逆理：" + snapshot.InverseTruthName + "。"; break;
            case "aptitude": MclslActorAccessor.Set(actor, MclslActorDataKeys.Aptitude, snapshot.Aptitude); message = "已继承资质：" + snapshot.Aptitude + "。"; break;
            case "mind": RestoreLegacyMind(actor, snapshot); message = "已继承心境与心法进度。"; break;
            case "contribution": MclslActorAccessor.Set(actor, MclslActorDataKeys.Contribution, snapshot.Contribution); message = "已继承贡献值：" + snapshot.Contribution + "。"; break;
            case "spirit_stones": MclslActorAccessor.Set(actor, MclslActorDataKeys.SpiritStones, SnapshotInt(snapshot, MclslActorDataKeys.SpiritStones)); message = "已继承灵石：" + SnapshotInt(snapshot, MclslActorDataKeys.SpiritStones) + "。"; break;
            default: message = "未知或已失效的前世遗产。"; return false;
        }
        MclslActorAccessor.Set(actor, MclslActorDataKeys.LastBreakthroughResult, message);
        MclslActorAccessor.ApplyDisplayName(actor, MclslActorAccessor.Realm(actor));
        try { actor.updateStats(); } catch (Exception ex) { MclslDiagnostics.Error("huanzhen-legacy-option-stats", "刷新前世遗产属性失败: " + ex.Message); }
        MclslWorldActorQuery.MarkDirty();
        return true;
    }

    private static void RestoreLegacyFoundationWonder(Actor actor, MclslHuanzhenCultivationSnapshot s)
    {
        SetString(actor, MclslActorDataKeys.FoundationWonderId, s.FoundationWonderId);
        SetString(actor, MclslActorDataKeys.FoundationWonderName, s.FoundationWonderName);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.FoundationWonderQuality, s.FoundationWonderQuality);
        SetString(actor, MclslActorDataKeys.FoundationWonderTags, s.FoundationWonderTags);
        SetString(actor, MclslActorDataKeys.FoundationWonderDescription, s.FoundationWonderDescription);
        SetString(actor, MclslActorDataKeys.FoundationWonderOrigin, s.FoundationWonderOrigin);
        SetString(actor, MclslActorDataKeys.FoundationWonderEffects, s.FoundationWonderEffects);
        CopyIntState(actor, s, MclslActorDataKeys.FoundationWonderGrade, MclslActorDataKeys.FoundationWonderCompleteness, MclslActorDataKeys.FoundationWonderRuleStrength);
    }

    private static void RestoreLegacyGoldenCore(Actor actor, MclslHuanzhenCultivationSnapshot s)
    {
        SetString(actor, MclslActorDataKeys.GoldenCoreLaws, s.GoldenCoreLaws);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.GoldenCorePurity, s.GoldenCorePurity);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.GoldenCoreStability, s.GoldenCoreStability);
    }

    private static void RestoreLegacyNascentCave(Actor actor, MclslHuanzhenCultivationSnapshot s)
    {
        SetString(actor, MclslActorDataKeys.NascentCaveId, s.NascentCaveId); SetString(actor, MclslActorDataKeys.NascentCaveName, s.NascentCaveName); SetString(actor, MclslActorDataKeys.NascentCaveTags, s.NascentCaveTags);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.NascentCaveCompatibility, s.NascentCaveCompatibility); MclslActorAccessor.Set(actor, MclslActorDataKeys.NascentCaveIntegrity, s.NascentCaveIntegrity);
    }

    private static void RestoreLegacyNascentEssence(Actor actor, MclslHuanzhenCultivationSnapshot s)
    {
        SetString(actor, MclslActorDataKeys.NascentEssenceId, s.NascentEssenceId); SetString(actor, MclslActorDataKeys.NascentEssenceName, s.NascentEssenceName); MclslActorAccessor.Set(actor, MclslActorDataKeys.NascentEssenceQuality, s.NascentEssenceQuality);
        SetString(actor, MclslActorDataKeys.NascentEssenceTags, s.NascentEssenceTags); SetString(actor, MclslActorDataKeys.NascentEssenceDescription, s.NascentEssenceDescription); SetString(actor, MclslActorDataKeys.NascentEssenceEffects, s.NascentEssenceEffects);
    }

    private static void RestoreLegacyDivineChange(Actor actor, MclslHuanzhenCultivationSnapshot s)
    {
        SetString(actor, MclslActorDataKeys.DivineChangeId, s.DivineChangeId); SetString(actor, MclslActorDataKeys.DivineChangeName, s.DivineChangeName); SetString(actor, MclslActorDataKeys.DivineChangeTags, s.DivineChangeTags); MclslActorAccessor.Set(actor, MclslActorDataKeys.DivineChangeCompatibility, s.DivineChangeCompatibility);
    }

    private static void RestoreLegacyDivineMarrow(Actor actor, MclslHuanzhenCultivationSnapshot s)
    {
        SetString(actor, MclslActorDataKeys.DivineMarrowId, s.DivineMarrowId); SetString(actor, MclslActorDataKeys.DivineMarrowName, s.DivineMarrowName); MclslActorAccessor.Set(actor, MclslActorDataKeys.DivineMarrow, s.DivineMarrowCount); MclslActorAccessor.Set(actor, MclslActorDataKeys.DivineMarrowQuality, s.DivineMarrowQuality);
        SetString(actor, MclslActorDataKeys.DivineMarrowTags, s.DivineMarrowTags); SetString(actor, MclslActorDataKeys.DivineMarrowDescription, s.DivineMarrowDescription); SetString(actor, MclslActorDataKeys.DivineMarrowEffects, s.DivineMarrowEffects);
    }

    private static void RestoreLegacyWorldSoul(Actor actor, MclslHuanzhenCultivationSnapshot s)
    {
        SetString(actor, MclslActorDataKeys.WorldSoulId, s.WorldSoulId); SetString(actor, MclslActorDataKeys.WorldSoulName, s.WorldSoulName); SetString(actor, MclslActorDataKeys.HeavenlyDuty, s.HeavenlyDuty);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.HarmonyLeap, s.HarmonyLeap); SetString(actor, MclslActorDataKeys.HarmonyOrigin, s.HarmonyOrigin); MclslActorAccessor.Set(actor, MclslActorDataKeys.HarmonyCompatibility, s.HarmonyCompatibility); MclslActorAccessor.Set(actor, MclslActorDataKeys.HarmonyStability, s.HarmonyStability);
    }

    private static void RestoreLegacyInverseTruth(Actor actor, MclslHuanzhenCultivationSnapshot s)
    {
        SetString(actor, MclslActorDataKeys.InverseTruthId, s.InverseTruthId); SetString(actor, MclslActorDataKeys.InverseTruthName, s.InverseTruthName); MclslActorAccessor.Set(actor, MclslActorDataKeys.InverseTruthProgress, s.InverseTruthProgress);
    }

    private static void RestoreLegacyMind(Actor actor, MclslHuanzhenCultivationSnapshot s)
    {
        MclslActorAccessor.Set(actor, MclslActorDataKeys.MindState, s.MindState); MclslActorAccessor.Set(actor, MclslActorDataKeys.HeartTemperingProgress, s.HeartTemperingProgress); MclslActorAccessor.Set(actor, MclslActorDataKeys.HeartMethodKnown, s.HeartMethodKnown);
    }

    private static int SnapshotInt(MclslHuanzhenCultivationSnapshot snapshot, string key) => snapshot?.IntState != null && snapshot.IntState.TryGetValue(key, out int value) ? value : 0;

    private static void RestoreLegacyCultivation(Actor actor, MclslHuanzhenCultivationSnapshot s, int anchorYear, int deathYear)
    {
        string realm = s.RealmId ?? string.Empty;
        if (s.CultivationSystemId == MclslCultivationSystemIds.AncientLaw || s.CultivationSystemId == MclslCultivationSystemIds.NewLaw)
            MclslCultivationStateTransitions.TrySetCultivationSystem(actor, s.CultivationSystemId);
        if (!string.IsNullOrWhiteSpace(realm)) SetRealm(actor, realm, anchorYear, "还真空间继承" + deathYear + "年前世修为");
        bool ancient = s.CultivationSystemId == MclslCultivationSystemIds.AncientLaw;
        int essence = s.TrueEssence > 0 ? s.TrueEssence : MclslRealmProgress.EssenceAtProgress(realm, ancient, s.CultivationProgress);
        MclslCultivationGrowthSystem.SetTrueEssence(actor, realm, essence, ancient, !string.IsNullOrWhiteSpace(realm));
        MclslActorAccessor.Set(actor, MclslActorDataKeys.Aptitude, s.Aptitude);
        CopyStringState(actor, s, MclslActorDataKeys.SpiritualRootPrimary, MclslActorDataKeys.SpiritualRootAttributes);
        CopyIntState(actor, s, MclslActorDataKeys.ImmortalFate, MclslActorDataKeys.SpiritualRootCount);
        CopyFloatState(actor, s, MclslActorDataKeys.TrueEssenceRemainder);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.LastCultivationYear, anchorYear - 1);
    }

    private static void RestoreLegacyTechnique(Actor actor, MclslHuanzhenCultivationSnapshot s)
    {
        if (s.CultivationSystemId == MclslCultivationSystemIds.AncientLaw || s.CultivationSystemId == MclslCultivationSystemIds.NewLaw)
            MclslCultivationStateTransitions.TrySetCultivationSystem(actor, s.CultivationSystemId);
        SetString(actor, MclslActorDataKeys.TechniqueId, s.TechniqueId);
        SetString(actor, MclslActorDataKeys.TechniqueName, s.TechniqueName);
        SetString(actor, MclslActorDataKeys.TechniqueMaxRealm, s.TechniqueMaxRealm);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.TechniqueInsight, s.TechniqueInsight);
        CopyStringState(actor, s, MclslActorDataKeys.AncientLawStatus, MclslActorDataKeys.AncientFoundationName,
            MclslActorDataKeys.AncientCoreName, MclslActorDataKeys.AncientDaoIntent, MclslActorDataKeys.AncientNascentName, MclslActorDataKeys.AncientDaoName);
        CopyIntState(actor, s, MclslActorDataKeys.AncientLineageStrength, MclslActorDataKeys.AncientLegacyPotential,
            MclslActorDataKeys.AncientFoundationQuality, MclslActorDataKeys.AncientFoundationStability,
            MclslActorDataKeys.AncientCorePurity, MclslActorDataKeys.AncientSoulStrength,
            MclslActorDataKeys.AncientBodyFit, MclslActorDataKeys.AncientDivineIntent,
            MclslActorDataKeys.AncientSoulFusion, MclslActorDataKeys.AncientDaoCompatibility,
            MclslActorDataKeys.AncientTechniqueComprehension, MclslActorDataKeys.AncientHarmonyIntegrity,
            MclslActorDataKeys.AncientHeavenCompatibility);
    }

    private static void RestoreLegacyTreasures(Actor actor, MclslHuanzhenCultivationSnapshot s)
    {
        SetString(actor, MclslActorDataKeys.FoundationWonderId, s.FoundationWonderId); SetString(actor, MclslActorDataKeys.FoundationWonderName, s.FoundationWonderName);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.FoundationWonderQuality, s.FoundationWonderQuality);
        SetString(actor, MclslActorDataKeys.FoundationWonderTags, s.FoundationWonderTags); SetString(actor, MclslActorDataKeys.FoundationWonderDescription, s.FoundationWonderDescription);
        SetString(actor, MclslActorDataKeys.FoundationWonderOrigin, s.FoundationWonderOrigin); SetString(actor, MclslActorDataKeys.FoundationWonderEffects, s.FoundationWonderEffects);
        SetString(actor, MclslActorDataKeys.GoldenCoreLaws, s.GoldenCoreLaws); MclslActorAccessor.Set(actor, MclslActorDataKeys.GoldenCorePurity, s.GoldenCorePurity); MclslActorAccessor.Set(actor, MclslActorDataKeys.GoldenCoreStability, s.GoldenCoreStability);
        SetString(actor, MclslActorDataKeys.NascentCaveId, s.NascentCaveId); SetString(actor, MclslActorDataKeys.NascentCaveName, s.NascentCaveName); SetString(actor, MclslActorDataKeys.NascentCaveTags, s.NascentCaveTags);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.NascentCaveCompatibility, s.NascentCaveCompatibility); MclslActorAccessor.Set(actor, MclslActorDataKeys.NascentCaveIntegrity, s.NascentCaveIntegrity);
        SetString(actor, MclslActorDataKeys.NascentEssenceId, s.NascentEssenceId); SetString(actor, MclslActorDataKeys.NascentEssenceName, s.NascentEssenceName); MclslActorAccessor.Set(actor, MclslActorDataKeys.NascentEssenceQuality, s.NascentEssenceQuality);
        SetString(actor, MclslActorDataKeys.NascentEssenceTags, s.NascentEssenceTags); SetString(actor, MclslActorDataKeys.NascentEssenceDescription, s.NascentEssenceDescription); SetString(actor, MclslActorDataKeys.NascentEssenceEffects, s.NascentEssenceEffects);
        SetString(actor, MclslActorDataKeys.DivineChangeId, s.DivineChangeId); SetString(actor, MclslActorDataKeys.DivineChangeName, s.DivineChangeName); SetString(actor, MclslActorDataKeys.DivineChangeTags, s.DivineChangeTags); MclslActorAccessor.Set(actor, MclslActorDataKeys.DivineChangeCompatibility, s.DivineChangeCompatibility);
        SetString(actor, MclslActorDataKeys.DivineMarrowId, s.DivineMarrowId); SetString(actor, MclslActorDataKeys.DivineMarrowName, s.DivineMarrowName); MclslActorAccessor.Set(actor, MclslActorDataKeys.DivineMarrow, s.DivineMarrowCount); MclslActorAccessor.Set(actor, MclslActorDataKeys.DivineMarrowQuality, s.DivineMarrowQuality);
        SetString(actor, MclslActorDataKeys.DivineMarrowTags, s.DivineMarrowTags); SetString(actor, MclslActorDataKeys.DivineMarrowDescription, s.DivineMarrowDescription); SetString(actor, MclslActorDataKeys.DivineMarrowEffects, s.DivineMarrowEffects);
        CopyIntState(actor, s, MclslActorDataKeys.FoundationWonderGrade, MclslActorDataKeys.FoundationWonderCompleteness, MclslActorDataKeys.FoundationWonderRuleStrength);
    }

    private static void RestoreLegacyDao(Actor actor, MclslHuanzhenCultivationSnapshot s)
    {
        SetString(actor, MclslActorDataKeys.WorldSoulId, s.WorldSoulId); SetString(actor, MclslActorDataKeys.WorldSoulName, s.WorldSoulName); SetString(actor, MclslActorDataKeys.HeavenlyDuty, s.HeavenlyDuty);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.HarmonyLeap, s.HarmonyLeap); SetString(actor, MclslActorDataKeys.HarmonyOrigin, s.HarmonyOrigin); MclslActorAccessor.Set(actor, MclslActorDataKeys.HarmonyCompatibility, s.HarmonyCompatibility); MclslActorAccessor.Set(actor, MclslActorDataKeys.HarmonyStability, s.HarmonyStability);
        SetString(actor, MclslActorDataKeys.InverseTruthId, s.InverseTruthId); SetString(actor, MclslActorDataKeys.InverseTruthName, s.InverseTruthName); MclslActorAccessor.Set(actor, MclslActorDataKeys.InverseTruthProgress, s.InverseTruthProgress);
        SetString(actor, MclslActorDataKeys.HuaShenHonorific, s.HuaShenHonorific); SetString(actor, MclslActorDataKeys.HeDaoHonorific, s.HeDaoHonorific); SetString(actor, MclslActorDataKeys.ChangShengHonorific, s.ChangShengHonorific);
        SetString(actor, MclslActorDataKeys.AncientHuaShenHonorific, s.AncientHuaShenHonorific); SetString(actor, MclslActorDataKeys.AncientHeDaoHonorific, s.AncientHeDaoHonorific); SetString(actor, MclslActorDataKeys.AncientChangShengHonorific, s.AncientChangShengHonorific);
        CopyIntState(actor, s, MclslActorDataKeys.TaishangProgress, MclslActorDataKeys.LifespanStolenBonus, MclslActorDataKeys.LifespanDrainedPenalty, MclslActorDataKeys.HeavenlyDutyBacklash);
    }

    private static void RestoreLegacyResources(Actor actor, MclslHuanzhenCultivationSnapshot s)
    {
        MclslActorAccessor.Set(actor, MclslActorDataKeys.MindState, s.MindState); MclslActorAccessor.Set(actor, MclslActorDataKeys.HeartTemperingProgress, s.HeartTemperingProgress);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.HeartMethodKnown, s.HeartMethodKnown); MclslActorAccessor.Set(actor, MclslActorDataKeys.MiasmaPoolCleansing, s.MiasmaPoolCleansing);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.Contribution, s.Contribution); MclslActorAccessor.Set(actor, MclslActorDataKeys.RuinExperience, s.RuinExperience);
        CopyIntState(actor, s, MclslActorDataKeys.SpiritStones, MclslActorDataKeys.MortalMiasma,
            MclslActorDataKeys.FoundationChanceBonus, MclslActorDataKeys.CaveClaimBonus, MclslActorDataKeys.DivineClaimBonus);
        CopyStringState(actor, s, MclslActorDataKeys.FactionAffiliation);
    }

    private static void CopyStringState(Actor actor, MclslHuanzhenCultivationSnapshot snapshot, params string[] keys)
    {
        foreach (string key in keys) if (snapshot.StringState?.TryGetValue(key, out string value) == true) MclslActorAccessor.Set(actor, key, value ?? string.Empty);
    }

    private static void CopyIntState(Actor actor, MclslHuanzhenCultivationSnapshot snapshot, params string[] keys)
    {
        foreach (string key in keys) if (snapshot.IntState?.TryGetValue(key, out int value) == true) MclslActorAccessor.Set(actor, key, value);
    }

    private static void CopyFloatState(Actor actor, MclslHuanzhenCultivationSnapshot snapshot, params string[] keys)
    {
        foreach (string key in keys) if (snapshot.FloatState?.TryGetValue(key, out float value) == true) MclslActorAccessor.Set(actor, key, value);
    }

    private static void SetString(Actor actor, string key, string value) => MclslActorAccessor.Set(actor, key, value ?? string.Empty);

    internal static void SetRealm(Actor actor, string realm, int year, string reason)
    {
        if (actor?.data == null || string.IsNullOrWhiteSpace(realm)) return;
        if (IsLongevityForbiddenBeforeNewLaw(realm, year))
        {
            RejectForbiddenAncientLongevity(actor, string.Empty, year);
            return;
        }

        MclslCultivationStateTransitions.TrySetRealm(actor, realm, year, reason);
    }


    private static bool IsLongevityForbiddenBeforeNewLaw(string realm, int year) =>
        realm == MclslRealmIds.ChangSheng && !MclslWorldEpochSystem.IsNewLawActive(year);

    private static bool HasForbiddenAncientLongevity(Actor actor, int year)
    {
        if (actor?.data == null || MclslWorldEpochSystem.IsNewLawActive(year)) return false;
        string newTrait = MclslTraitRegistration.TraitIdForRealm(MclslRealmIds.ChangSheng);
        string ancientTrait = MclslTraitRegistration.LegacyTraitIdForRealm(MclslRealmIds.ChangSheng);
        try
        {
            return MclslActorAccessor.Realm(actor) == MclslRealmIds.ChangSheng
                || (!string.IsNullOrWhiteSpace(newTrait) && actor.hasTrait(newTrait))
                || (!string.IsNullOrWhiteSpace(ancientTrait) && actor.hasTrait(ancientTrait));
        }
        catch { return MclslActorAccessor.Realm(actor) == MclslRealmIds.ChangSheng; }
    }

    private static void RejectForbiddenAncientLongevity(Actor actor, string traitId, int year)
    {
        if (actor?.data == null) return;
        RemoveRealmTraitPair(actor, MclslRealmIds.ChangSheng);
        MclslTraitGrantRouter.SuppressRouting(() =>
        {
            try { if (!string.IsNullOrWhiteSpace(traitId) && actor.hasTrait(traitId)) actor.removeTrait(traitId); } catch (System.Exception mclslEmptyCatchEx) { MySimulatedLongevityRoad.Core.MclslDiagnostics.Error("empty-catch-code-MySimulatedLongevityRoad-Systems-Cultivation-MclslCultivationSystem-cs-11", "空 catch 捕获: code/MySimulatedLongevityRoad/Systems/Cultivation/MclslCultivationSystem.cs #11: " + mclslEmptyCatchEx.Message); }
        });

        string current = MclslActorAccessor.Realm(actor);
        string reason = "传法新法未立，旧法不得长生";
        if (current == MclslRealmIds.ChangSheng)
        {
            MclslCultivationStateTransitions.TrySetCultivationSystem(actor, MclslCultivationSystemIds.AncientLaw);
            MclslActorAccessor.Set(actor, MclslActorDataKeys.AncientLawStatus, "仙道正修");
            int aptitude = Math.Clamp(MclslActorAccessor.GetInt(actor, MclslActorDataKeys.Aptitude, 50), 1, 100);
            MclslAncientLawBreakthroughSystem.EnsureAncientStageData(actor, MclslRealmIds.HeDao, year, aptitude);
            SetRealm(actor, MclslRealmIds.HeDao, year, reason);
            return;
        }

        MclslActorAccessor.Set(actor, MclslActorDataKeys.LastBreakthroughResult, reason);
        if (!string.IsNullOrWhiteSpace(current))
            MclslTraitRegistration.SyncRealmTraitFamily(actor, current);
        MclslWorldActorQuery.MarkDirty();
    }

    private static void RemoveRealmTraitPair(Actor actor, string realm)
    {
        if (actor?.data == null || string.IsNullOrWhiteSpace(realm)) return;
        string newTrait = MclslTraitRegistration.TraitIdForRealm(realm);
        string ancientTrait = MclslTraitRegistration.LegacyTraitIdForRealm(realm);
        MclslTraitGrantRouter.SuppressRouting(() =>
        {
            try { if (!string.IsNullOrWhiteSpace(newTrait) && actor.hasTrait(newTrait)) actor.removeTrait(newTrait); } catch (System.Exception mclslEmptyCatchEx) { MySimulatedLongevityRoad.Core.MclslDiagnostics.Error("empty-catch-code-MySimulatedLongevityRoad-Systems-Cultivation-MclslCultivationSystem-cs-12", "空 catch 捕获: code/MySimulatedLongevityRoad/Systems/Cultivation/MclslCultivationSystem.cs #12: " + mclslEmptyCatchEx.Message); }
            try { if (!string.IsNullOrWhiteSpace(ancientTrait) && actor.hasTrait(ancientTrait)) actor.removeTrait(ancientTrait); } catch (System.Exception mclslEmptyCatchEx) { MySimulatedLongevityRoad.Core.MclslDiagnostics.Error("empty-catch-code-MySimulatedLongevityRoad-Systems-Cultivation-MclslCultivationSystem-cs-13", "空 catch 捕获: code/MySimulatedLongevityRoad/Systems/Cultivation/MclslCultivationSystem.cs #13: " + mclslEmptyCatchEx.Message); }
        });
    }

    private static int PositiveHash(string value)
    {
        unchecked { int hash = 23; foreach (char c in value ?? string.Empty) hash = hash * 37 + c; return hash & int.MaxValue; }
    }
}
