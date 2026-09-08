using System;
using System.Collections.Generic;
using MySimulatedLongevityRoad.Data;
using MySimulatedLongevityRoad.Queries;
using MySimulatedLongevityRoad.Systems.Death;
using MySimulatedLongevityRoad.Core;

namespace MySimulatedLongevityRoad.Systems;

internal static class MclslAncientLawEventSystem
{
    internal static void ManualTriggerSpiritualDisaster(int year)
    {
        int sequence = MclslWorldRunRepository.NextProceduralSequence();
        int roll = PositiveHash("manual_ancient_disaster|" + year + "|" + sequence) % 3;
        if (roll == 0)
        {
            MclslWorldRunRepository.AddEvent(year, "ancient_spiritual_convergence", "灵气汇聚", "山川灵机一时汇拢，近地修士吐纳有得。");
            if (MclslRuntimeSettings.MinorWorldAnnouncementsEnabled) MclslAnnouncementSystem.Enqueue("灵气汇聚，山川有感。", "#A7E08A", 6f, 1);
            return;
        }
        if (roll == 1)
        {
            MclslWorldRunRepository.AddEvent(year, "ancient_earthfire", "地火涌动", "地火上涌，火、土二道修士遥观其势。");
            if (MclslRuntimeSettings.MinorWorldAnnouncementsEnabled) MclslAnnouncementSystem.Enqueue("地火涌动，赤脉出山。", "#FF8877", 6f, 1);
            return;
        }
        MclslWorldRunRepository.AddEvent(year, "ancient_meteor_stone", "天降星石", "星石坠野，金石火光中藏一线大道。");
        if (MclslRuntimeSettings.MinorWorldAnnouncementsEnabled) MclslAnnouncementSystem.Enqueue("天降星石，灵机入野。", "#FFD37A", 6f, 1);
    }

    internal static void ManualTriggerSecretRealm(int year)
    {
        Actor candidate = PickSecretRealmCandidate(year, "manual_secret_realm");
        if (MclslActorAccessor.Alive(candidate))
            ResolveAncientSecretRealm(candidate, year, MclslActorAccessor.DisplayName(candidate), true);
        else
            MclslWorldRunRepository.AddEvent(year, "ancient_secret_realm", "秘境开启", "云雾开合，秘境一线洞开，未有修士入内。");
        if (MclslRuntimeSettings.MinorWorldAnnouncementsEnabled) MclslAnnouncementSystem.Enqueue("秘境一线洞开。", "#B7A7FF", 6f, 1);
    }

    internal static void ManualTriggerAncientRuin(int year)
    {
        if (MclslAdventureSystem.CreateManualAncientLawRuin(year) != null) return;
        MclslWorldRunRepository.AddEvent(year, "ancient_high_cultivator_seclusion", "遗府现世", "洞府显于山河之间，残卷、灵石与遗物尚有余韵。");
        if (MclslRuntimeSettings.MinorWorldAnnouncementsEnabled) MclslAnnouncementSystem.Enqueue("遗府现世。", "#D8C778", 6f, 1);
    }

    internal static void ProcessAnnual(Actor actor, int year, string realm, int aptitude)
    {
        int realmIndex = Math.Max(0, MclslRealmIds.Index(realm));
        bool newLawActive = MclslWorldEpochSystem.IsNewLawActive(year);
        int gate = newLawActive ? 4 + realmIndex * 2 : 5 + realmIndex * 3;
        if (!MclslDetectionGate.DeterministicRoll(actor, "ancient_event", "gate", year, gate, 10000)) return;

        int eventSeed = PositiveHash(MclslActorAccessor.Id(actor) + "|ancient_event_kind|" + year);
        int kind = NormalizeEventKindForRealm(eventSeed % 22, realmIndex, eventSeed);
        string eventKey = AncientEventKey(kind);
        if (!MclslDetectionGate.TryEnterActorAttempt(actor, "ancient_event", eventKey, year, AncientEventInterval(kind, realmIndex))) return;
        string name = MclslActorAccessor.DisplayName(actor, realm);
        string technique = MclslActorAccessor.GetString(actor, MclslActorDataKeys.TechniqueName, "无名仙法");
        string place = string.IsNullOrWhiteSpace(actor.city?.data?.name) ? "荒野" : actor.city.data.name;
        string kingdom = string.IsNullOrWhiteSpace(actor.kingdom?.data?.name) ? "无国之地" : actor.kingdom.data.name;

        switch (kind)
        {
            case 0:
                AddClamped(actor, MclslActorDataKeys.AncientLegacyPotential, 4, 0, 100);
                MclslWorldRunRepository.AddEvent(year, "ancient_root_manifest", "灵根显现", name + "灵根与《" + technique + "》相应，法脉潜质稍增。", actor);
                break;
            case 1:
                MclslTechniqueStageSystem.AddProgress(actor, 5);
                AddClamped(actor, MclslActorDataKeys.RuinExperience, 1, 0, 9999);
                MclslWorldRunRepository.AddEvent(year, "ancient_found_method", "偶得仙法", name + "游历" + place + "时得前人手札，补足《" + technique + "》一角。", actor);
                break;
            case 2:
                if (realmIndex >= MclslRealmIds.Index(MclslRealmIds.ZhuJi))
                {
                    AddClamped(actor, MclslActorDataKeys.AncientLineageStrength, 5, 0, 100);
                    AddClamped(actor, MclslActorDataKeys.Contribution, 2 + realmIndex, 0, 999999);
                    if (MclslAncientMentorshipSystem.TryRecruitFromTeacher(actor, year, out Actor student))
                        MclslWorldRunRepository.AddEvent(year, "ancient_master_teaching", "仙师授法", name + "于" + kingdom + "收" + MclslActorAccessor.DisplayName(student) + "为徒，传下《" + technique + "》口诀。师徒关系已入仙道履历。", actor);
                    else
                        MclslWorldRunRepository.AddEvent(year, "ancient_master_teaching", "仙师授法", name + "于" + kingdom + "传下《" + technique + "》口诀，附近求道者渐知此法。", actor);
                }
                break;
            case 3:
                MclslTechniqueStageSystem.AddProgress(actor, 6 + realmIndex);
                AddClamped(actor, MclslActorDataKeys.TechniqueInsight, 2 + realmIndex, 0, 9999);
                MclslActorAccessor.Set(actor, MclslActorDataKeys.LastBreakthroughResult, "闭关修行，功法参悟精进");
                MclslWorldRunRepository.AddEvent(year, "ancient_closed_cultivation", "闭关修行", name + "闭关参悟《" + technique + "》，功法参悟有所增长。", actor);
                break;
            case 4:
                AddClamped(actor, MclslActorDataKeys.MindState, 8, 0, 100);
                MclslTechniqueStageSystem.AddProgress(actor, 4);
                AddClamped(actor, MclslActorDataKeys.AncientDaoCompatibility, 4, 0, 100);
                MclslWorldRunRepository.AddEvent(year, "ancient_sudden_insight", "心境顿悟", name + "观人间兴废而顿悟，道心更定，大道感悟加深。", actor);
                break;
            case 5:
                ApplyProgressSetback(actor, realm, 8f);
                AddClamped(actor, MclslActorDataKeys.MindState, -7, 0, 100);
                AddClamped(actor, MclslActorDataKeys.AncientLawTribulation, 5, 0, 100);
                MclslWorldRunRepository.AddEvent(year, "ancient_inner_demon", "心魔滋生", name + "久困关隘，心魔暗生，修行进度与心境皆受损。", actor);
                break;
            case 6:
                MclslTechniqueStageSystem.AddProgress(actor, 8);
                AddClamped(actor, MclslActorDataKeys.TechniqueInsight, 4, 0, 9999);
                AddClamped(actor, MclslActorDataKeys.Contribution, 4 + realmIndex * 2, 0, 999999);
                string deduction = name + "重校《" + technique + "》行气次第，使此法更契合自身根基。";
                if (aptitude >= 70 && PositiveHash(MclslActorAccessor.Id(actor) + "|raise_technique_limit|" + year) % 100 < 16
                    && MclslTechniqueRealmLimit.TryRaiseLimit(actor, out string raisedRealm))
                    deduction += "此番推演补足高境纲要，此法可修至" + MclslRealmIds.Display(raisedRealm) + "。";
                if (aptitude >= 82 || realmIndex >= MclslRealmIds.Index(MclslRealmIds.JinDan))
                    MclslTechniqueLineageSystem.MarkAncientTechniqueImprint(actor, year, "功法推演", 10 + realmIndex * 3);
                MclslWorldRunRepository.AddEvent(year, "ancient_technique_deduction", "功法推演", deduction, actor);
                break;
            case 7:
                OpenAncientMethodBranch(actor, year, name, technique);
                break;
            case 8:
                if (realmIndex >= MclslRealmIds.Index(MclslRealmIds.JinDan))
                {
                    MclslAdventureSystem.CreateAncientLawRuin(year, actor, technique);
                    MclslTechniqueLineageSystem.MarkAncientTechniqueImprint(actor, year, "高境坐化遗府", 16 + realmIndex * 3);
                    MclslWorldRunRepository.AddEvent(year, "ancient_high_cultivator_seclusion", "高境坐化遗府", name + "自知寿数终有尽时，预先封存功法与遗物，遗府由此成形。", actor);
                }
                break;
            case 9:
                if (!string.IsNullOrWhiteSpace(actor.kingdom?.data?.name))
                {
                    AddClamped(actor, MclslActorDataKeys.Contribution, 12 + realmIndex * 7, 0, 999999);
                    MclslWorldRunRepository.AddEvent(year, "ancient_country_immortal", "国中得仙", kingdom + "有修士" + name + "成就" + MclslRealmIds.Display(realm) + "，各方求道者渐聚其地。", actor);
                }
                break;
            case 10:
                if (!string.IsNullOrWhiteSpace(actor.kingdom?.data?.name))
                {
                    AddClamped(actor, MclslActorDataKeys.MindState, 3, 0, 100);
                    AddClamped(actor, MclslActorDataKeys.Contribution, 8 + realmIndex * 6, 0, 999999);
                    MclslWorldRunRepository.AddEvent(year, "ancient_protect_country", "仙修护国", name + "出手护持" + kingdom + "，未另立仙门，只以自身道行影响国势。", actor);
                }
                break;
            case 11:
                AddClamped(actor, MclslActorDataKeys.AncientLineageStrength, 4, 0, 100);
                MclslWorldRunRepository.AddEvent(year, "ancient_cultivators_gather", "修士云集", place + "修士往来渐密，《" + technique + "》论道次数增多，法脉声势稍盛。", actor);
                break;
            case 12:
                MclslTechniqueStageSystem.AddProgress(actor, 4);
                AddClamped(actor, MclslActorDataKeys.AncientLegacyPotential, 3, 0, 100);
                MclslWorldRunRepository.AddEvent(year, "ancient_spiritual_convergence", "灵气汇聚", place + "一带灵气短暂汇聚，" + name + "趁机吐纳，修行稍进。", actor);
                break;
            case 13:
                ApplyProgressSetback(actor, realm, 4f);
                AddClamped(actor, MclslActorDataKeys.AncientLineageStrength, -3, 0, 100);
                MclslWorldRunRepository.AddEvent(year, "ancient_spiritual_decline", "灵气衰退", place + "连年兵灾与修士聚集，使地脉灵机渐衰。", actor);
                break;
            case 14:
                AddClamped(actor, MclslActorDataKeys.AncientDaoCompatibility, 5, 0, 100);
                MclslWorldRunRepository.AddEvent(year, "ancient_earthfire", "地火涌动", place + "地火涌动，火、土二道修士得以旁观悟道。", actor);
                break;
            case 15:
                MclslTechniqueStageSystem.AddProgress(actor, 5);
                AddClamped(actor, MclslActorDataKeys.RuinExperience, 1, 0, 9999);
                MclslWorldRunRepository.AddEvent(year, "ancient_meteor_stone", "天降星石", "星石坠于" + place + "外，" + name + "从金石火光中得一线大道感悟。", actor);
                break;
            case 16:
                ResolveAncientSecretRealm(actor, year, name, false);
                break;
            case 18:
                ResolveGoldenCoreLecture(actor, year, name, technique, place);
                break;
            case 19:
                ResolveNascentSoulJourney(actor, year, name, technique, kingdom);
                break;
            case 20:
                ResolveSoulTransformationResonance(actor, year, name, technique, place);
                break;
            case 21:
                ResolveDaoIntegrationHarmony(actor, year, name, technique);
                break;
            default:
                if (realmIndex >= MclslRealmIds.Index(MclslRealmIds.YuanYing))
                {
                    AddClamped(actor, MclslActorDataKeys.AncientDaoCompatibility, 7, 0, 100);
                    AddClamped(actor, MclslActorDataKeys.AncientHeavenCompatibility, 4, 0, 100);
                    MclslWorldRunRepository.AddEvent(year, "ancient_heaven_earth_resonance", "山河道痕显现", name + "遥观山河异象，自天地气机流转中得一线大道感悟。", actor);
                }
                break;
        }
    }

    private static int NormalizeEventKindForRealm(int kind, int realmIndex, int seed)
    {
        if (kind == 18 && realmIndex < MclslRealmIds.Index(MclslRealmIds.JinDan)) return 3 + seed % 5;
        if (kind == 19 && realmIndex < MclslRealmIds.Index(MclslRealmIds.YuanYing)) return 3 + seed % 5;
        if (kind == 20 && realmIndex < MclslRealmIds.Index(MclslRealmIds.HuaShen)) return 3 + seed % 5;
        if (kind == 21 && realmIndex < MclslRealmIds.Index(MclslRealmIds.HeDao)) return 3 + seed % 5;
        return kind;
    }

    private static string AncientEventKey(int kind) => kind switch
    {
        0 => "root_resonance",
        1 => "found_method",
        2 => "master_teaching",
        3 => "closed_cultivation",
        4 => "insight",
        5 => "inner_demon",
        6 => "technique_deduction",
        7 => "new_method_branch",
        8 => "seclusion_ruin",
        9 => "country_immortal",
        10 => "protect_country",
        11 => "cultivators_gather",
        12 => "spiritual_convergence",
        13 => "spiritual_decline",
        14 => "earthfire",
        15 => "meteor",
        16 => "secret_realm",
        18 => "golden_core_lecture",
        19 => "nascent_soul_journey",
        20 => "soul_transformation_resonance",
        21 => "dao_integration_harmony",
        _ => "world_soul_observation"
    };

    private static int AncientEventInterval(int kind, int realmIndex) => kind switch
    {
        2 => Math.Max(8, 14 - realmIndex),
        3 => 8,
        4 => 14,
        5 => 18,
        6 => 18,
        7 => 42,
        8 => 70,
        9 or 10 or 11 => 24,
        12 or 13 or 14 or 15 => 32,
        16 => 48,
        17 => 60,
        18 => 42,
        19 => 52,
        20 => 64,
        21 => 80,
        _ => 16
    };

    private static void OpenAncientMethodBranch(Actor actor, int year, string name, string oldTechnique)
    {
        string[] tags = AncientLawTags(actor);
        int nameSeed = PositiveHash(MclslActorAccessor.Id(actor) + "|method_branch|" + year);
        string newName = MclslProceduralLexicon.TechniqueName(tags, nameSeed);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.TechniqueName, newName);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.TechniqueId, "spiritual_branch_" + MclslWorldRunRepository.NextProceduralSequence());
        MclslTechniqueRealmLimit.SetProceduralBranchLimit(actor, MclslActorAccessor.GetInt(actor, MclslActorDataKeys.Aptitude, 50));
        MclslTechniqueStageSystem.AddProgress(actor, 10);
        AddClamped(actor, MclslActorDataKeys.AncientLineageStrength, 8, 0, 100);
        MclslTechniqueLineageSystem.MarkAncientTechniqueImprint(actor, year, "开创新支", 14);
        MclslWorldRunRepository.AddEvent(year, "ancient_new_method_branch", "法门新开", name + "由《" + oldTechnique + "》推演出《" + newName + "》，一支新传承由此分出。", actor);
    }

    private static void ResolveAncientSecretRealm(Actor actor, int year, string name, bool manual)
    {
        if (!MclslDetectionGate.TryEnterActorAttempt(actor, "ancient_secret_realm", "explore", year, manual ? 12 : 48)) return;
        int roll = PositiveHash(MclslActorAccessor.Id(actor) + "|ancient_secret_realm|" + year) % 100;
        if (roll < 10)
        {
            MclslAdventureSystem.CreateAncientLawRuin(year, actor, MclslActorAccessor.GetString(actor, MclslActorDataKeys.TechniqueName, "上古秘法"));
            MclslDeathSystem.ExecuteScriptedDeath(actor, "ancient_secret_realm", "秘境", "深入秘境后失陷于残阵与地脉裂隙", true);
            return;
        }
        AddClamped(actor, MclslActorDataKeys.RuinExperience, 3, 0, 9999);
        MclslTechniqueStageSystem.AddProgress(actor, 6);
        AddClamped(actor, MclslActorDataKeys.Contribution, 6 + roll / 12, 0, 999999);
        AddClamped(actor, MclslActorDataKeys.SpiritStones, 15 + roll / 3, 0, 999999);
        if (roll >= 72)
        {
            MclslAdventureSystem.CreateAncientSecretRealmRuin(year, actor, MclslActorAccessor.GetString(actor, MclslActorDataKeys.TechniqueName, "仙道秘法"));
            MclslTechniqueLineageSystem.MarkAncientTechniqueImprint(actor, year, "秘境道统", 16);
        }
        MclslWorldRunRepository.AddEvent(year, "ancient_secret_realm", "秘境开启", name + "入秘境而还，得灵石、功法残章与大道感悟。", actor);
    }

    private static Actor PickSecretRealmCandidate(int year, string scope)
    {
        IReadOnlyList<Actor> candidates = MclslCultivatorCandidateIndex.SelectCultivators(
            96,
            actor => MclslEligibility.CanCultivate(actor)
                && MclslActorAccessor.GetString(actor, MclslActorDataKeys.CultivationSystem, string.Empty) == MclslCultivationSystemIds.AncientLaw,
            actor => Math.Max(0, MclslRealmIds.Index(MclslActorAccessor.Realm(actor))) * 80
                + MclslActorAccessor.GetInt(actor, MclslActorDataKeys.RuinExperience, 0) * 4
                + MclslActorAccessor.GetInt(actor, MclslActorDataKeys.Aptitude, 50)
                + PositiveHash(MclslActorAccessor.Id(actor) + "|secret_realm_score|" + year) % 50);
        return candidates.Count > 0 ? candidates[0] : null;
    }

    private static void ResolveGoldenCoreLecture(Actor actor, int year, string name, string technique, string place)
    {
        AddClamped(actor, MclslActorDataKeys.AncientCorePurity, 4, 0, 100);
        MclslTechniqueStageSystem.AddProgress(actor, 6);
        AddClamped(actor, MclslActorDataKeys.AncientLineageStrength, 5, 0, 100);
        MclslTechniqueLineageSystem.MarkAncientTechniqueImprint(actor, year, "金丹讲法", 18);
        MclslWorldRunRepository.AddEvent(year, "ancient_golden_core_lecture", "金丹讲法", name + "于" + place + "讲《" + technique + "》丹意，闻者各有所悟。", actor);
    }

    private static void ResolveNascentSoulJourney(Actor actor, int year, string name, string technique, string kingdom)
    {
        AddClamped(actor, MclslActorDataKeys.AncientSoulStrength, 5, 0, 100);
        AddClamped(actor, MclslActorDataKeys.AncientDaoCompatibility, 5, 0, 100);
        AddClamped(actor, MclslActorDataKeys.RuinExperience, 2, 0, 9999);
        MclslTechniqueLineageSystem.MarkAncientTechniqueImprint(actor, year, "元婴神游", 22);
        MclslWorldRunRepository.AddEvent(year, "ancient_nascent_soul_journey", "元婴神游", name + "元神游历" + kingdom + "山河，归后重订《" + technique + "》神魂篇。", actor);
    }

    private static void ResolveSoulTransformationResonance(Actor actor, int year, string name, string technique, string place)
    {
        AddClamped(actor, MclslActorDataKeys.AncientSoulFusion, 5, 0, 100);
        AddClamped(actor, MclslActorDataKeys.AncientDaoCompatibility, 6, 0, 100);
        AddClamped(actor, MclslActorDataKeys.AncientHeavenCompatibility, 4, 0, 100);
        MclslTechniqueLineageSystem.MarkAncientTechniqueImprint(actor, year, "化神映世", 26);
        MclslWorldRunRepository.AddEvent(year, "ancient_soul_transformation_resonance", "化神映世", name + "神意映照" + place + "，使《" + technique + "》高境法义渐成定本。", actor);
    }

    private static void ResolveDaoIntegrationHarmony(Actor actor, int year, string name, string technique)
    {
        AddClamped(actor, MclslActorDataKeys.AncientHarmonyIntegrity, 5, 0, 100);
        AddClamped(actor, MclslActorDataKeys.AncientHeavenCompatibility, 5, 0, 100);
        AddClamped(actor, MclslActorDataKeys.AncientLineageStrength, 7, 0, 100);
        MclslTechniqueLineageSystem.MarkAncientTechniqueImprint(actor, year, "合道合鸣", 32);
        MclslWorldRunRepository.AddEvent(year, "ancient_dao_integration_harmony", "大道合鸣", name + "以自身大道校订《" + technique + "》，此法自此有合道纲目。", actor);
    }

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

    private static void ApplyProgressSetback(Actor actor, string realm, float progress)
    {
        MclslCultivationGrowthSystem.ApplyProgressSetback(actor, realm, progress, ancientLaw: true);
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
