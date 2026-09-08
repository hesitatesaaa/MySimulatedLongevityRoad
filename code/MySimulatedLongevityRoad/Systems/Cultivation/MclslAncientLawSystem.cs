using System;
using System.Collections.Generic;
using MySimulatedLongevityRoad.Core;
using MySimulatedLongevityRoad.Data;
using MySimulatedLongevityRoad.Queries;
using MySimulatedLongevityRoad.Systems.Death;
using MySimulatedLongevityRoad.Traits;

namespace MySimulatedLongevityRoad.Systems;

internal static class MclslAncientLawSystem
{
    internal static void ClearRuntimeOnly() { }

    internal static void ProcessAnnualFromScheduler(Actor actor, int year) => ProcessAnnual(actor, year);

    private static void ProcessAnnual(Actor actor, int year)
    {
        if (!MclslEligibility.CanCultivate(actor)) return;
        string system = MclslActorAccessor.GetString(actor, MclslActorDataKeys.CultivationSystem, string.Empty);
        string realm = MclslActorAccessor.Realm(actor);
        if (!string.IsNullOrWhiteSpace(realm) && system != MclslCultivationSystemIds.AncientLaw) return;
        if (MclslSensingQiSystem.ShouldReturnToSensingQi(actor, realm, system))
        {
            MclslSensingQiSystem.ReturnToSensingQi(actor, year);
            realm = string.Empty;
        }
        MclslActorAccessor.ApplyDisplayName(actor, realm);

        if (string.IsNullOrWhiteSpace(realm))
        {
            if (!MclslAncientLawEntrySystem.TryBeginFromMortal(actor, year)) return;
            return;
        }

        if (MclslWorldEpochSystem.TryRetryAncientConversion(actor, year)) return;

        MclslMindSystem.ProcessAnnual(actor, year, realm);
        MclslResourceSystem.EnsureActorResources(actor);
        MaintainAncientLineage(actor, year, realm);
        StabilizeAncientSurvival(actor, realm);
        MclslTraitRegistration.SyncNativeRealmTraits(actor, realm);

        bool ancientLaw = true;
        MclslCultivationGrowthSystem.Reconcile(actor, realm, ancientLaw);
        int aptitude = Math.Clamp(
            MclslActorAccessor.GetInt(actor, MclslActorDataKeys.Aptitude, 50),
            1,
            100);
        MclslTechniqueStageSystem.AdvanceAnnualProgress(actor, aptitude);
        MclslAncientLawEventSystem.ProcessAnnual(actor, year, realm, aptitude);

        ProcessAncientSpiritStones(actor, year, realm, aptitude);
        MclslCultivationGrowthSystem.MaybeRecordSameLawPressure(actor, year);

        if (!MclslCultivationGrowthSystem.MeetsNextRealmMinimum(actor, realm, ancientLaw, out int essence, out int requiredEssence))
        {
            if (requiredEssence > 0)
                MclslActorAccessor.Set(actor, MclslActorDataKeys.LastBreakthroughResult,
                    SpiritualPathLabel(year) + "真元未达下境最低要求：" + essence + "/" + requiredEssence);
            return;
        }
        if (!MclslCultivationAgeSanity.CanAttemptNextRealm(actor, realm, out string ageReason))
        {
            MclslActorAccessor.Set(actor, MclslActorDataKeys.LastBreakthroughResult, SpiritualPathLabel(year) + ageReason);
            return;
        }

        MclslAncientLawBreakthroughSystem.TryBreakthrough(actor, realm, year, aptitude);
    }

    private static void MaintainAncientLineage(Actor actor, int year, string realm)
    {
        int strength = MclslActorAccessor.GetInt(actor, MclslActorDataKeys.AncientLineageStrength, -1);
        if (strength < 0)
        {
            int aptitude = Math.Clamp(MclslActorAccessor.GetInt(actor, MclslActorDataKeys.Aptitude, 50), 1, 100);
            strength = 25 + aptitude / 2 + Math.Max(0, MclslRealmIds.Index(realm)) * 8;
        }

        int mind = MclslMindSystem.EnsureMindState(actor);
        int delta = MclslWorldEpochSystem.IsNewLawActive(year)
            ? -Math.Max(1, 4 - Math.Max(0, MclslRealmIds.Index(realm)) / 2)
            : (mind >= 70 ? 2 : mind >= 50 ? 1 : 0);
        strength = Math.Clamp(strength + delta, 0, 100);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.AncientLineageStrength, strength);
        if (MclslActorAccessor.GetInt(actor, MclslActorDataKeys.AncientLegacyPotential, 0) <= 0)
            MclslActorAccessor.Set(actor, MclslActorDataKeys.AncientLegacyPotential, Math.Clamp(10 + strength / 2 + Math.Max(0, MclslRealmIds.Index(realm)) * 8, 5, 100));

        if (!MclslWorldEpochSystem.IsNewLawActive(year))
        {
            if (MclslRuntimeSettings.AncientLineageAnnouncementsEnabled
                && strength >= 85
                && PositiveHash(MclslActorAccessor.Id(actor) + "|ancient_lineage_notice|" + year) % 100 < 3)
                MclslWorldRunRepository.AddEvent(year, "ancient_lineage_flourish", MclslActorAccessor.DisplayName(actor) + "仙道法脉兴盛", "其所修《" + MclslActorAccessor.GetString(actor, MclslActorDataKeys.TechniqueName, "仙道功法") + "》渐成一支传承，声势日隆。");
            return;
        }

        string status = strength <= 10 ? "旧法传承将绝" : strength <= 35 ? "旧法衰微" : RemnantLabel(year);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.AncientLawStatus, status);
        if (strength <= 0 && PositiveHash(MclslActorAccessor.Id(actor) + "|ancient_lineage_ruin|" + year) % 100 < 12)
            MclslAdventureSystem.CreateAncientLawRuin(year, actor, MclslActorAccessor.GetString(actor, MclslActorDataKeys.TechniqueName, "旧法传承"));
        if (strength <= 8 && PositiveHash(MclslActorAccessor.Id(actor) + "|ancient_retire|" + year) % 1000 < 8)
        {
            MclslAdventureSystem.CreateAncientLawRuin(year, actor, MclslActorAccessor.GetString(actor, MclslActorDataKeys.TechniqueName, "旧法传承"));
            MclslActorAccessor.Set(actor, MclslActorDataKeys.LastBreakthroughResult, "旧法传承近绝，遗修闭死关前留下残存传承");
            MclslWorldRunRepository.AddEvent(year, "ancient_remnant_secluded", MclslActorAccessor.DisplayName(actor) + "旧法闭死关", "其所修旧法传承几近断绝，闭死关前将残篇、灵石与旧法遗物封入遗迹。");
        }
    }

    private static void ProcessAncientSpiritStones(Actor actor, int year, string realm, int aptitude)
    {
        int stones = Math.Max(0, MclslActorAccessor.GetInt(actor, MclslActorDataKeys.SpiritStones, 0));
        int realmIndex = Math.Max(0, MclslRealmIds.Index(realm));
        long actorId = MclslActorAccessor.Id(actor);
        int incomeChance = Math.Clamp(16 + realmIndex * 5 + aptitude / 14, 16, 55);
        if (PositiveHash(actorId + "|ancient_spirit_stone_income|" + year) % 100 < incomeChance)
        {
            int variance = PositiveHash(actorId + "|ancient_spirit_stone_amount|" + year) % Math.Max(2, 4 + realmIndex * 2);
            int income = Math.Max(1, 1 + realmIndex + aptitude / 38 + variance);
            stones = Math.Min(999999, stones + income);
        }

        int cost = 3 + realmIndex * 4;
        int spendChance = Math.Clamp(30 + aptitude / 4 + realmIndex * 3, 30, 72);
        if (stones >= cost && PositiveHash(actorId + "|ancient_spirit_stone_spend|" + year) % 100 < spendChance)
        {
            stones -= cost;
            MclslActorAccessor.Set(actor, MclslActorDataKeys.SpiritStones, stones);
            return;
        }

        MclslActorAccessor.Set(actor, MclslActorDataKeys.SpiritStones, stones);
    }

    internal static int AnnualFieldBonusPercent(Actor actor, string realm)
    {
        int realmIndex = Math.Max(0, MclslRealmIds.Index(realm));
        int lineage = MclslActorAccessor.GetInt(actor, MclslActorDataKeys.AncientLineageStrength, 0);
        int bonus = lineage / 12;
        if (realmIndex >= MclslRealmIds.Index(MclslRealmIds.ZhuJi))
            bonus += MclslActorAccessor.GetInt(actor, MclslActorDataKeys.AncientFoundationStability, 0) / 12
                + MclslActorAccessor.GetInt(actor, MclslActorDataKeys.AncientFoundationQuality, 0) * 2;
        if (realmIndex >= MclslRealmIds.Index(MclslRealmIds.JinDan))
            bonus += MclslActorAccessor.GetInt(actor, MclslActorDataKeys.AncientCorePurity, 0) / 13;
        if (realmIndex >= MclslRealmIds.Index(MclslRealmIds.YuanYing))
            bonus += MclslActorAccessor.GetInt(actor, MclslActorDataKeys.AncientSoulStrength, 0) / 16
                + MclslActorAccessor.GetInt(actor, MclslActorDataKeys.AncientBodyFit, 0) / 22;
        if (realmIndex >= MclslRealmIds.Index(MclslRealmIds.HuaShen))
            bonus += MclslActorAccessor.GetInt(actor, MclslActorDataKeys.AncientSoulFusion, 0) / 15
                + MclslActorAccessor.GetInt(actor, MclslActorDataKeys.AncientDaoCompatibility, 0) / 16;
        if (realmIndex >= MclslRealmIds.Index(MclslRealmIds.HeDao))
            bonus += MclslActorAccessor.GetInt(actor, MclslActorDataKeys.AncientHarmonyIntegrity, 0) / 15
                + MclslActorAccessor.GetInt(actor, MclslActorDataKeys.AncientHeavenCompatibility, 0) / 18;
        return Math.Clamp(bonus, 0, 60);
    }

    private static string SpiritualPathLabel(int year)
    {
        if (!MclslWorldEpochSystem.IsNewLawActive(year)) return "仙道";
        return MclslWorldEpochSystem.IsStableNewLawEra(year) ? "古法" : "旧法";
    }

    private static string RemnantLabel(int year) => MclslWorldEpochSystem.IsStableNewLawEra(year) ? "古法遗修" : "旧法遗修";

    private static void StabilizeAncientSurvival(Actor actor, string realm)
    {
        if (!MclslActorAccessor.Alive(actor) || string.IsNullOrWhiteSpace(realm)) return;
        try
        {
            // Native lifespan counters must not be reset each year. The shared annual
            // lifespan rule now handles both ancient-law and new-law cultivators.
            if (MclslRealmIds.Index(realm) >= MclslRealmIds.Index(MclslRealmIds.YuanYing))
            {
                TrySetNumber(actor.data, "hunger", 100f);
                TrySetNumber(actor.data, "_hunger", 100f);
                TrySetNumber(actor.data, "food", 100f);
                TrySetNumber(actor.data, "nutrition", 100f);
            }
        }
        catch (System.Exception mclslEmptyCatchEx) { MySimulatedLongevityRoad.Core.MclslDiagnostics.Error("empty-catch-code-MySimulatedLongevityRoad-Systems-Cultivation-MclslAncientLawSystem-cs-1", "空 catch 捕获: code/MySimulatedLongevityRoad/Systems/Cultivation/MclslAncientLawSystem.cs #1: " + mclslEmptyCatchEx.Message); }
    }

    private static void TrySetNumber(object target, string name, float value)
    {
        if (target == null) return;
        const System.Reflection.BindingFlags flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.IgnoreCase;
        try
        {
            System.Reflection.FieldInfo field = target.GetType().GetField(name, flags);
            if (field != null)
            {
                if (field.FieldType == typeof(float)) field.SetValue(target, value);
                else if (field.FieldType == typeof(int)) field.SetValue(target, (int)value);
                return;
            }
            System.Reflection.PropertyInfo property = target.GetType().GetProperty(name, flags);
            if (property != null && property.CanWrite)
            {
                if (property.PropertyType == typeof(float)) property.SetValue(target, value);
                else if (property.PropertyType == typeof(int)) property.SetValue(target, (int)value);
            }
        }
        catch (System.Exception mclslEmptyCatchEx) { MySimulatedLongevityRoad.Core.MclslDiagnostics.Error("empty-catch-code-MySimulatedLongevityRoad-Systems-Cultivation-MclslAncientLawSystem-cs-2", "空 catch 捕获: code/MySimulatedLongevityRoad/Systems/Cultivation/MclslAncientLawSystem.cs #2: " + mclslEmptyCatchEx.Message); }
    }

    private static int PositiveHash(string value)
    {
        unchecked { int hash = 79; foreach (char c in value ?? string.Empty) hash = hash * 43 + c; return hash & int.MaxValue; }
    }
}
