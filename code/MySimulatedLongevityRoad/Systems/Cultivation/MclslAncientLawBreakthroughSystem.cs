using System;
using MySimulatedLongevityRoad.Core;
using MySimulatedLongevityRoad.Data;
using MySimulatedLongevityRoad.Systems.Death;

namespace MySimulatedLongevityRoad.Systems;

internal static class MclslAncientLawBreakthroughSystem
{
    internal static void TryBreakthrough(Actor actor, string realm, int year, int aptitude)
    {
        int index = MclslRealmIds.Index(realm);
        if (index < 0 || index >= MclslRealmIds.Ordered.Length - 2)
        {
            MclslActorAccessor.Set(actor, MclslActorDataKeys.LastBreakthroughResult, SpiritualPathLabel(year) + "止于此境，天地已不许旧法再上长生");
            return;
        }

        int nextIndex = index + 1;
        string nextRealm = MclslRealmIds.Ordered[nextIndex];
        if (!MclslTechniqueRealmLimit.CanReach(actor, nextRealm, out string limitReason))
        {
            if (!TryRaiseTechniqueLimitAtBottleneck(actor, realm, nextRealm, year, aptitude, limitReason, out string raisedRealm))
            {
                MclslActorAccessor.Set(actor, MclslActorDataKeys.LastBreakthroughResult, limitReason);
                return;
            }
            MclslActorAccessor.Set(actor, MclslActorDataKeys.LastBreakthroughResult,
                "久困瓶颈，反推功法后路，此法已可修至" + MclslRealmIds.Display(raisedRealm));
        }
        // 功法参悟只修正年度真元效率，不是破境硬门槛，也不直接叠加破境成功率。
        int fieldBonus = AncientBreakthroughBonus(actor, nextRealm);
        int chance = Math.Clamp(24 + aptitude / 4 + fieldBonus
            + MclslMindSystem.BreakthroughAdjustment(actor)
            + MclslWorldStateModifierSystem.BreakthroughStabilityBonus(year)
            - nextIndex * 8, 8, 88);
        if (MclslWorldEpochSystem.IsNewLawActive(year)) chance = Math.Max(3, chance / 2);
        int roll = PositiveHash(MclslActorAccessor.Id(actor) + "|ancient_break|" + realm + "|" + year) % 100;
        if (roll >= chance)
        {
            ResolveAncientBreakthroughFailure(actor, nextRealm, year, chance, roll);
            return;
        }

        EnsureAncientStageData(actor, nextRealm, year, aptitude);
        MclslCultivationStateTransitions.TrySetCultivationSystem(actor, MclslCultivationSystemIds.AncientLaw);
        MclslCultivationSystem.SetRealm(actor, nextRealm, year, AncientBreakthroughReason(nextRealm));
        MclslActorAccessor.Set(actor, MclslActorDataKeys.AncientLawStatus, MclslWorldEpochSystem.IsNewLawActive(year) ? RemnantLabel(year) : "仙道正修");
        MclslActorAccessor.Set(actor, MclslActorDataKeys.AncientLineageStrength, Math.Min(100, MclslActorAccessor.GetInt(actor, MclslActorDataKeys.AncientLineageStrength, 40) + 12));
        MclslActorAccessor.Set(actor, MclslActorDataKeys.AncientLegacyPotential, Math.Min(100, MclslActorAccessor.GetInt(actor, MclslActorDataKeys.AncientLegacyPotential, 30) + 10));
        string displayName = MclslActorAccessor.DisplayName(actor, nextRealm);
        MclslWorldRunRepository.AddEvent(year, "ancient_law_breakthrough", displayName + "仙道突破", "其不假外物权柄，仅凭灵根、功法、真元、神魂与大道感悟破入" + MclslRealmIds.Display(nextRealm) + "。", actor);
    }

    private static bool TryRaiseTechniqueLimitAtBottleneck(Actor actor, string currentRealm, string nextRealm, int year, int aptitude, string limitReason, out string raisedRealm)
    {
        raisedRealm = string.Empty;
        if (actor?.data == null) return false;

        string currentMax = MclslTechniqueRealmLimit.MaxRealm(actor);
        int currentIndex = MclslRealmIds.Index(currentRealm);
        int maxIndex = MclslRealmIds.Index(currentMax);
        int nextIndex = MclslRealmIds.Index(nextRealm);
        if (currentIndex < 0 || maxIndex < 0 || nextIndex < 0) return false;
        if (maxIndex >= nextIndex || currentIndex < maxIndex) return false;

        int lineage = MclslActorAccessor.GetInt(actor, MclslActorDataKeys.AncientLineageStrength, 0);
        int legacy = MclslActorAccessor.GetInt(actor, MclslActorDataKeys.AncientLegacyPotential, 0);
        int mind = MclslMindSystem.EnsureMindState(actor);
        int chance = Math.Clamp(5 + aptitude / 12 + lineage / 18 + legacy / 22 + mind / 25 + currentIndex * 2, 6, 48);
        if (MclslWorldEpochSystem.IsNewLawActive(year)) chance = Math.Max(3, chance / 2);

        int roll = PositiveHash(MclslActorAccessor.Id(actor) + "|ancient_limit_bottleneck|" + nextRealm + "|" + year) % 100;
        if (roll >= chance)
        {
            MclslActorAccessor.Set(actor, MclslActorDataKeys.LastBreakthroughResult,
                limitReason + "；尝试补全后路未成。");
            return false;
        }

        if (!MclslTechniqueRealmLimit.TryRaiseLimit(actor, out raisedRealm)) return false;
        if (MclslRealmIds.Index(raisedRealm) < nextIndex)
        {
            MclslTechniqueRealmLimit.EnsureAtLeastRealm(actor, nextRealm);
            raisedRealm = nextRealm;
        }

        MclslTechniqueStageSystem.AddProgress(actor, 4 + currentIndex);
        AddClamped(actor, MclslActorDataKeys.AncientLegacyPotential, 4 + currentIndex, 0, 100);
        AddClamped(actor, MclslActorDataKeys.AncientLineageStrength, 2 + Math.Max(0, currentIndex - 1), 0, 100);

        string technique = MclslActorAccessor.GetString(actor, MclslActorDataKeys.TechniqueName, "所修功法");
        string name = MclslActorAccessor.DisplayName(actor);
        MclslWorldRunRepository.AddEvent(year, "ancient_technique_limit_raised",
            name + "补全《" + technique + "》后路",
            "此法原止于" + MclslRealmIds.Display(currentMax) + "，经其推演，后路可至" + MclslRealmIds.Display(raisedRealm) + "。", actor);
        MclslTechniqueLineageSystem.MarkAncientTechniqueImprint(actor, year, "破境悟法", 12 + currentIndex * 4);
        return true;
    }

    private static int AncientBreakthroughBonus(Actor actor, string nextRealm)
    {
        int lineage = MclslActorAccessor.GetInt(actor, MclslActorDataKeys.AncientLineageStrength, 0);
        int legacy = MclslActorAccessor.GetInt(actor, MclslActorDataKeys.AncientLegacyPotential, 0);
        int bonus = lineage / 18 + legacy / 20;
        if (nextRealm == MclslRealmIds.JinDan)
            bonus += MclslActorAccessor.GetInt(actor, MclslActorDataKeys.AncientFoundationStability, 0) / 14;
        else if (nextRealm == MclslRealmIds.YuanYing)
            bonus += MclslActorAccessor.GetInt(actor, MclslActorDataKeys.AncientCorePurity, 0) / 12;
        else if (nextRealm == MclslRealmIds.HuaShen)
            bonus += MclslActorAccessor.GetInt(actor, MclslActorDataKeys.AncientSoulStrength, 0) / 14
                + MclslActorAccessor.GetInt(actor, MclslActorDataKeys.AncientBodyFit, 0) / 18;
        else if (nextRealm == MclslRealmIds.HeDao)
            bonus += MclslActorAccessor.GetInt(actor, MclslActorDataKeys.AncientSoulFusion, 0) / 14
                + MclslActorAccessor.GetInt(actor, MclslActorDataKeys.AncientDaoCompatibility, 0) / 14;
        return Math.Clamp(bonus, 0, 32);
    }

    private static void ResolveAncientBreakthroughFailure(Actor actor, string nextRealm, int year, int chance, int roll)
    {
        int nextIndex = Math.Max(0, MclslRealmIds.Index(nextRealm));
        float progressLoss = nextIndex <= 1 ? 14f : nextIndex == 2 ? 20f : nextIndex == 3 ? 28f : 35f;
        MclslCultivationGrowthSystem.ApplyProgressSetback(
            actor, MclslActorAccessor.Realm(actor), progressLoss, ancientLaw: true, minimumProgressPercent: 20f);
        AddClamped(actor, MclslActorDataKeys.MindState, -(2 + nextIndex), 0, 100);
        AddClamped(actor, MclslActorDataKeys.AncientLineageStrength, -Math.Max(1, nextIndex - 1), 0, 100);

        string result = nextRealm switch
        {
            MclslRealmIds.ZhuJi => "自筑道基失败，经脉震荡，修炼进度回落",
            MclslRealmIds.JinDan => "凝结本命金丹失败，丹意浑浊，心境受损",
            MclslRealmIds.YuanYing => "丹破婴生失败，金丹开裂，神魂受创",
            MclslRealmIds.HuaShen => "神融大道失败，元婴反噬，道途几近断绝",
            MclslRealmIds.HeDao => "以自身之道合天失败，天地不许，根基大损",
            _ => "仙道突破失败，修炼进度回落"
        };
        DamageAncientField(actor, nextRealm, nextIndex);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.LastBreakthroughResult, SpiritualPathLabel(year) + result);
        if (ShouldRecordBreakthroughFailure(actor, nextRealm))
            MclslWorldRunRepository.AddEvent(year, "ancient_breakthrough_failed", MclslActorAccessor.DisplayName(actor) + "仙道破境失败", result + "。", actor);

        if (nextIndex >= MclslRealmIds.Index(MclslRealmIds.HuaShen))
        {
            int deathChance = Math.Clamp(2 + nextIndex * 3 - MclslMindSystem.StabilityBonus(actor) / 8, 1, 18);
            int deathRoll = PositiveHash(MclslActorAccessor.Id(actor) + "|ancient_break_death|" + nextRealm + "|" + year) % 100;
            if (deathRoll < deathChance)
            {
                MclslAdventureSystem.CreateAncientLawRuin(year, actor, MclslActorAccessor.GetString(actor, MclslActorDataKeys.TechniqueName, "上古功法"));
                MclslDeathSystem.ExecuteScriptedDeath(actor, "ancient_breakthrough_failure", nextRealm, result + "，形神终被大道反噬", true);
            }
        }
    }

    private static bool ShouldRecordBreakthroughFailure(Actor actor, string nextRealm)
    {
        if (!MclslRuntimeSettings.BreakthroughFailureAnnouncementsEnabled) return false;
        int nextIndex = MclslRealmIds.Index(nextRealm);
        if (nextIndex >= MclslRealmIds.Index(MclslRealmIds.YuanYing)) return true;
        try { return actor?.data != null && ((BaseSystemData)actor.data).favorite; }
        catch { return false; }
    }

    private static void DamageAncientField(Actor actor, string nextRealm, int nextIndex)
    {
        if (nextRealm == MclslRealmIds.JinDan)
            AddClamped(actor, MclslActorDataKeys.AncientFoundationStability, -8, 0, 100);
        else if (nextRealm == MclslRealmIds.YuanYing)
            AddClamped(actor, MclslActorDataKeys.AncientCorePurity, -7, 0, 100);
        else if (nextRealm == MclslRealmIds.HuaShen)
        {
            AddClamped(actor, MclslActorDataKeys.AncientSoulStrength, -10, 0, 100);
            AddClamped(actor, MclslActorDataKeys.AncientBodyFit, -8, 0, 100);
        }
        else if (nextRealm == MclslRealmIds.HeDao)
        {
            AddClamped(actor, MclslActorDataKeys.AncientSoulFusion, -10, 0, 100);
            AddClamped(actor, MclslActorDataKeys.AncientDaoCompatibility, -10, 0, 100);
        }
    }

    internal static void EnsureAncientStageData(Actor actor, string targetRealm, int year, int aptitude)
    {
        int target = MclslRealmIds.Index(targetRealm);
        if (target < MclslRealmIds.Index(MclslRealmIds.ZhuJi)) return;
        for (int i = MclslRealmIds.Index(MclslRealmIds.ZhuJi); i <= target; i++)
        {
            string realm = MclslRealmIds.Ordered[i];
            if (NeedsAncientStageData(actor, realm)) ApplyAncientStageData(actor, realm, year, aptitude);
        }
    }

    private static bool NeedsAncientStageData(Actor actor, string realm) => realm switch
    {
        MclslRealmIds.ZhuJi => string.IsNullOrWhiteSpace(MclslActorAccessor.GetString(actor, MclslActorDataKeys.AncientFoundationName, string.Empty)),
        MclslRealmIds.JinDan => string.IsNullOrWhiteSpace(MclslActorAccessor.GetString(actor, MclslActorDataKeys.AncientCoreName, string.Empty)),
        MclslRealmIds.YuanYing => string.IsNullOrWhiteSpace(MclslActorAccessor.GetString(actor, MclslActorDataKeys.AncientNascentName, string.Empty)),
        MclslRealmIds.HuaShen => string.IsNullOrWhiteSpace(MclslActorAccessor.GetString(actor, MclslActorDataKeys.AncientDivineIntent, string.Empty)),
        MclslRealmIds.HeDao => string.IsNullOrWhiteSpace(MclslActorAccessor.GetString(actor, MclslActorDataKeys.AncientDaoName, string.Empty)),
        _ => false
    };

    private static void ApplyAncientStageData(Actor actor, string nextRealm, int year, int aptitude)
    {
        string[] tags = AncientLawTags(actor);
        string primary = tags.Length > 0 ? tags[0] : "灵";
        int nameSeed = PositiveHash(MclslActorAccessor.Id(actor) + "|ancient_root_word|" + nextRealm);
        string stageName = MclslProceduralLexicon.AncientStageName(nextRealm, tags, nameSeed);
        int mind = MclslMindSystem.EnsureMindState(actor);

        if (nextRealm == MclslRealmIds.ZhuJi)
        {
            MclslActorAccessor.Set(actor, MclslActorDataKeys.AncientFoundationName, stageName);
            MclslActorAccessor.Set(actor, MclslActorDataKeys.AncientFoundationQuality, Math.Clamp(1 + aptitude / 25 + mind / 40, 1, 4));
            MclslActorAccessor.Set(actor, MclslActorDataKeys.AncientFoundationStability, Math.Clamp(45 + aptitude / 3 + mind / 4, 35, 99));
            MclslActorAccessor.Set(actor, MclslActorDataKeys.AncientDaoIntent, primary);
            return;
        }
        if (nextRealm == MclslRealmIds.JinDan)
        {
            string dao = MclslActorAccessor.GetString(actor, MclslActorDataKeys.AncientDaoIntent, primary);
            MclslActorAccessor.Set(actor, MclslActorDataKeys.AncientCoreName, stageName);
            MclslActorAccessor.Set(actor, MclslActorDataKeys.AncientCorePurity, Math.Clamp(42 + aptitude / 3 + MclslActorAccessor.GetInt(actor, MclslActorDataKeys.AncientFoundationStability, 50) / 5, 35, 99));
            MclslActorAccessor.Set(actor, MclslActorDataKeys.AncientDaoIntent, dao);
            return;
        }
        if (nextRealm == MclslRealmIds.YuanYing)
        {
            MclslActorAccessor.Set(actor, MclslActorDataKeys.AncientNascentName, stageName);
            MclslActorAccessor.Set(actor, MclslActorDataKeys.AncientSoulStrength, Math.Clamp(45 + mind / 3 + MclslActorAccessor.GetInt(actor, MclslActorDataKeys.AncientCorePurity, 50) / 5, 35, 99));
            MclslActorAccessor.Set(actor, MclslActorDataKeys.AncientBodyFit, Math.Clamp(42 + aptitude / 4 + mind / 4, 30, 96));
            return;
        }
        if (nextRealm == MclslRealmIds.HuaShen)
        {
            MclslActorAccessor.Set(actor, MclslActorDataKeys.AncientDivineIntent, stageName);
            MclslActorAccessor.Set(actor, MclslActorDataKeys.AncientSoulFusion, Math.Clamp(45 + MclslActorAccessor.GetInt(actor, MclslActorDataKeys.AncientSoulStrength, 50) / 3 + mind / 4, 35, 99));
            MclslActorAccessor.Set(actor, MclslActorDataKeys.AncientDaoCompatibility, Math.Clamp(40 + aptitude / 4 + MclslActorAccessor.GetInt(actor, MclslActorDataKeys.AncientCorePurity, 50) / 5, 30, 98));
            return;
        }
        if (nextRealm == MclslRealmIds.HeDao)
        {
            MclslActorAccessor.Set(actor, MclslActorDataKeys.AncientDaoName, stageName);
            MclslActorAccessor.Set(actor, MclslActorDataKeys.AncientHarmonyIntegrity, Math.Clamp(45 + MclslActorAccessor.GetInt(actor, MclslActorDataKeys.AncientDaoCompatibility, 50) / 3 + mind / 5, 35, 96));
            MclslActorAccessor.Set(actor, MclslActorDataKeys.AncientHeavenCompatibility, Math.Clamp(35 + MclslActorAccessor.GetInt(actor, MclslActorDataKeys.AncientSoulFusion, 50) / 4 + aptitude / 5, 25, 92));
        }
    }

    private static string AncientBreakthroughReason(string realm) => realm switch
    {
        MclslRealmIds.ZhuJi => "自筑道基，破入筑基",
        MclslRealmIds.JinDan => "凝结本命金丹",
        MclslRealmIds.YuanYing => "丹破婴生，凝成本命元婴",
        MclslRealmIds.HuaShen => "神魂与自身大道相融",
        MclslRealmIds.HeDao => "以自身之道合于天地",
        _ => "仙道修行突破"
    };

    private static string SpiritualPathLabel(int year)
    {
        if (!MclslWorldEpochSystem.IsNewLawActive(year)) return "仙道";
        return MclslWorldEpochSystem.IsStableNewLawEra(year) ? "古法" : "旧法";
    }

    private static string RemnantLabel(int year) => MclslWorldEpochSystem.IsStableNewLawEra(year) ? "古法遗修" : "旧法遗修";

    private static string[] AncientLawTags(Actor actor)
    {
        string techniqueId = MclslActorAccessor.GetString(actor, MclslActorDataKeys.TechniqueId, string.Empty).Replace("spiritual_", string.Empty).Replace("ancient_", string.Empty);
        if (techniqueId.StartsWith("branch_", StringComparison.Ordinal))
        {
            string dao = MclslActorAccessor.GetString(actor, MclslActorDataKeys.AncientDaoIntent, string.Empty);
            if (!string.IsNullOrWhiteSpace(dao)) return MclslGeneratedObjectFactory.NormalizeTags(new[] { dao });
        }
        MclslTechniqueDefinition technique = MclslCultivationCatalog.Technique(techniqueId);
        return MclslGeneratedObjectFactory.NormalizeTags(technique.LawPool);
    }

    private static void AddClamped(Actor actor, string key, int delta, int min, int max)
    {
        int current = MclslActorAccessor.GetInt(actor, key, min);
        MclslActorAccessor.Set(actor, key, Math.Clamp(current + delta, min, max));
    }

    private static int PositiveHash(string value)
    {
        unchecked { int hash = 79; foreach (char c in value ?? string.Empty) hash = hash * 43 + c; return hash & int.MaxValue; }
    }
}
