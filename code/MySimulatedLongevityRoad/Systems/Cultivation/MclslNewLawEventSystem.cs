using System;
using MySimulatedLongevityRoad.Core;
using MySimulatedLongevityRoad.Data;

namespace MySimulatedLongevityRoad.Systems;

internal static class MclslNewLawEventSystem
{
    internal static void ProcessAnnual(Actor actor, int year, string realm, int aptitude)
    {
        if (!MclslActorAccessor.Alive(actor) || string.IsNullOrWhiteSpace(realm)) return;
        int realmIndex = Math.Max(0, MclslRealmIds.Index(realm));
        int chance = 18 + realmIndex * 7 + Math.Min(22, Math.Max(0, aptitude - 50) / 4);
        if (!MclslDetectionGate.DeterministicRoll(actor, "new_law_event", "gate", year, chance, 10000)) return;

        int rawKind = PositiveHash(MclslActorAccessor.Id(actor) + "|new_law_event_kind|" + year);
        bool pioneerEra = MclslNewLawPioneerSystem.IsPioneerEra(year) && !MclslWorldEpochSystem.IsNewLawActive(year);
        int kind = pioneerEra ? PioneerEventKind(rawKind) : rawKind % 20;
        string key = EventKey(kind);
        if (!MclslDetectionGate.TryEnterActorAttempt(actor, "new_law_event", key, year, EventInterval(kind, realmIndex))) return;

        string name = MclslActorAccessor.DisplayName(actor, realm);
        string technique = MclslActorAccessor.GetString(actor, MclslActorDataKeys.TechniqueName, "无名功法");
        string place = string.IsNullOrWhiteSpace(actor.city?.data?.name) ? "荒野" : actor.city.data.name;
        int seed = PositiveHash(MclslActorAccessor.Id(actor) + "|new_law_event|" + year + "|" + kind);

        switch (kind)
        {
            case 0:
                AddClamped(actor, MclslActorDataKeys.TechniqueInsight, 4 + realmIndex + seed % 3, 0, 9999);
                Event(actor, year, "newlaw_closed_cultivation", name + "闭关行功", name + "闭关梳理《" + technique + "》，功法感悟稍进。");
                break;
            case 1:
                AddClamped(actor, MclslActorDataKeys.TechniqueInsight, 4 + realmIndex, 0, 9999);
                AddClamped(actor, MclslActorDataKeys.MindState, 1, 0, 100);
                Event(actor, year, "newlaw_law_insight", name + "偶得法悟", name + "于" + place + "观天象地气，补足《" + technique + "》一处关窍。");
                break;
            case 2:
                ResolveSameLawPressure(actor, year, name, technique);
                break;
            case 3:
                MclslResourceSystem.GrantFactionReward(actor, 4 + realmIndex * 3, 4 + realmIndex * 4);
                Event(actor, year, "newlaw_resource_gain", name + "得修行资粮", name + "参与人间事务，换得少量灵石与贡献，可供后续购买修行资粮。");
                break;
            case 4:
                if (realm == MclslRealmIds.LianQi)
                {
                    AddClamped(actor, MclslActorDataKeys.FoundationChanceBonus, 4 + seed % 7, 0, 90);
                    Event(actor, year, "newlaw_foundation_clue", name + "获筑基线索", name + "寻得一则筑基奇物传闻，后续筑基机缘略增。");
                }
                break;
            case 5:
                if (realm == MclslRealmIds.ZhuJi)
                {
                    AddClamped(actor, MclslActorDataKeys.TechniqueInsight, 6 + seed % 6, 0, 9999);
                    Event(actor, year, "newlaw_golden_core_preparation", name + "参悟金丹契机", name + "反照筑基根基与主修功法，为结丹悟法预作准备。");
                }
                break;
            case 6:
                if (realm == MclslRealmIds.JinDan)
                {
                    AddClamped(actor, MclslActorDataKeys.CaveClaimBonus, 5 + seed % 8, 0, 95);
                    Event(actor, year, "newlaw_cave_clue", name + "得洞天残图", name + "搜得洞天残图一角，争夺天地之精时把握略增。");
                }
                break;
            case 7:
                if (realm == MclslRealmIds.YuanYing)
                {
                    AddClamped(actor, MclslActorDataKeys.DivineClaimBonus, 5 + seed % 8, 0, 95);
                    Event(actor, year, "newlaw_world_change_omen", name + "察天地之变", name + "感知远方地脉有变，抽取天地之髓时承受力略增。");
                }
                break;
            case 8:
                AddClamped(actor, MclslActorDataKeys.HeartTemperingProgress, 4 + seed % 5, 0, 100);
                AddClamped(actor, MclslActorDataKeys.MindState, 1, 0, 100);
                Event(actor, year, "newlaw_heart_tempering", name + "炼心有得", name + "以玄黄炼心法门自照心神，心境与炼心进度稍进。");
                break;
            case 9:
                if (realmIndex >= MclslRealmIds.Index(MclslRealmIds.HuaShen))
                {
                    AddClamped(actor, MclslActorDataKeys.RuinExperience, 3 + realmIndex, 0, 9999);
                    AddClamped(actor, MclslActorDataKeys.InverseTruthProgress, realmIndex >= MclslRealmIds.Index(MclslRealmIds.HeDao) ? 1 : 0, 0, 100);
                    Event(actor, year, "newlaw_high_realm_deduction", name + "推演高境道途", name + "借遗迹残章与天地异动推演后路，高境履历与逆理感悟略有积累。");
                }
                break;
            case 10:
                if (actor.kingdom != null)
                {
                    AddClamped(actor, MclslActorDataKeys.Contribution, 3 + realmIndex * 3, 0, 999999);
                    Event(actor, year, "newlaw_native_involvement", name + "涉入人间国事", name + "在" + actor.kingdom.data.name + "中显露手段，凡俗国度因其存在而生出求法传闻。");
                }
                break;
            case 11:
                AddClamped(actor, MclslActorDataKeys.TechniqueInsight, 5 + realmIndex * 2, 0, 9999);
                if (seed % 100 < 18) MclslTechniqueRealmLimit.TryRaiseLimit(actor, out _);
                Event(actor, year, "newlaw_old_fragment", name + "得旧法残篇", name + "得旧法残篇一卷，化入《" + technique + "》。");
                break;
            case 12:
                if (MclslTechniqueOccupationSystem.CountFor(MclslActorAccessor.GetString(actor, MclslActorDataKeys.TechniqueId, string.Empty)) > 1)
                {
                    AddClamped(actor, MclslActorDataKeys.TechniqueInsight, 7 + realmIndex * 2, 0, 9999);
                    Event(actor, year, "newlaw_same_law_stagnation", name + "同法受阻", name + "同修《" + technique + "》者渐众，前路滞涩，唯有反复推敲本法关窍。");
                }
                break;
            case 13:
                if (realm == MclslRealmIds.LianQi)
                {
                    AddClamped(actor, MclslActorDataKeys.FoundationChanceBonus, 8 + seed % 10, 0, 90);
                    AddClamped(actor, MclslActorDataKeys.Contribution, -Math.Min(18, 6 + seed % 13), 0, 999999);
                    Event(actor, year, "newlaw_foundation_trade", name + "换得筑基秘闻", name + "以贡献换得筑基秘闻，筑基机缘渐明。");
                }
                break;
            case 14:
                if (realm == MclslRealmIds.JinDan)
                {
                    AddClamped(actor, MclslActorDataKeys.CaveClaimBonus, 8 + seed % 12, 0, 95);
                    AddClamped(actor, MclslActorDataKeys.SpiritStones, -Math.Min(40, 12 + seed % 29), 0, 999999);
                    Event(actor, year, "newlaw_cave_trade", name + "购得洞天线索", name + "得洞天线索一则。");
                }
                break;
            case 15:
                if (MclslActorAccessor.GetInt(actor, MclslActorDataKeys.MortalMiasma, 0) > 0)
                {
                    AddClamped(actor, MclslActorDataKeys.MortalMiasma, -(12 + seed % 18), 0, 10000);
                    AddClamped(actor, MclslActorDataKeys.MiasmaPoolCleansing, 5 + seed % 8, 0, 100);
                    Event(actor, year, "newlaw_miasma_cleansing", name + "洗去凡瘴", name + "入洗瘴池，凡瘴稍退。");
                }
                break;
            case 16:
                if (MclslActorAccessor.GetString(actor, MclslActorDataKeys.FactionAffiliation, string.Empty) is "wanxian" or "万仙盟")
                {
                    AddClamped(actor, MclslActorDataKeys.TechniqueInsight, 6 + realmIndex * 2, 0, 9999);
                    AddClamped(actor, MclslActorDataKeys.Contribution, -Math.Min(30, 10 + realmIndex * 4), 0, 999999);
                    Event(actor, year, "newlaw_alliance_archive", name + "阅仙盟档案", name + "查阅万仙盟旧档，功法关窍渐明。");
                }
                break;
            case 17:
                if (MclslActorAccessor.GetString(actor, MclslActorDataKeys.FactionAffiliation, string.Empty) is "five_elders" or "五老会")
                {
                    AddClamped(actor, MclslActorDataKeys.RuinExperience, 5 + realmIndex, 0, 9999);
                    AddClamped(actor, MclslActorDataKeys.HeartTemperingProgress, 4 + seed % 9, 0, 100);
                    Event(actor, year, "newlaw_elder_secret", name + "受五老暗授", name + "得五老会暗授残法。");
                }
                break;
            case 18:
                if (realmIndex >= MclslRealmIds.Index(MclslRealmIds.ZhuJi))
                {
                    AddClamped(actor, MclslActorDataKeys.RuinExperience, 4 + realmIndex * 2, 0, 9999);
                    AddClamped(actor, MclslActorDataKeys.TechniqueInsight, 3 + realmIndex, 0, 9999);
                    Event(actor, year, "newlaw_ruin_second_clue", name + "得遗迹续线", name + "旧迹未尽，另得一条余线。");
                }
                break;
            case 19:
                if (realmIndex >= MclslRealmIds.Index(MclslRealmIds.YuanYing))
                {
                    AddClamped(actor, MclslActorDataKeys.DivineClaimBonus, 8 + seed % 12, 0, 95);
                    Event(actor, year, "newlaw_world_change_afterglow", name + "遇天地余韵", name + "天地余韵入感，抽髓把握略增。");
                }
                break;
            default:
                AddClamped(actor, MclslActorDataKeys.TechniqueInsight, 1 + realmIndex, 0, 9999);
                Event(actor, year, "newlaw_minor_opportunity", name + "遇小机缘", name + "偶得一段灵机，虽不足以破境，却可助长功法理解。");
                break;
        }
    }

    private static void ResolveSameLawPressure(Actor actor, int year, string name, string technique)
    {
        string techniqueId = MclslActorAccessor.GetString(actor, MclslActorDataKeys.TechniqueId, string.Empty);
        int count = MclslTechniqueOccupationSystem.CountFor(techniqueId);
        if (count <= 1) return;
        float multiplier = MclslTechniqueOccupationSystem.CultivationMultiplier(actor);
        if (multiplier >= 0.85f) return;
        MclslActorAccessor.Set(actor, MclslActorDataKeys.LastBreakthroughResult, "法不可同修：《" + technique + "》同修" + count + "人，修行受天地压制");
        Event(actor, year, "newlaw_same_law_pressure", name + "受法压所困", "《" + technique + "》同修已达" + count + "人，天地反制使同法修行迟滞，心境亦受牵动。");
    }


    private static int PioneerEventKind(int seed)
    {
        // Before the transmission proof there is no alliance contribution economy,
        // same-law heavenly suppression, immortal-mortal miasma or Five Elders.
        // Keep only cultivation, foundation, cave/change clues and old-text study.
        int[] kinds = { 0, 1, 4, 5, 6, 7, 8, 11, 18, 19 };
        return kinds[(seed & int.MaxValue) % kinds.Length];
    }

    private static string EventKey(int kind) => kind switch
    {
        0 => "closed_cultivation",
        1 => "law_insight",
        2 => "same_law_pressure",
        3 => "resource_gain",
        4 => "foundation_clue",
        5 => "golden_core_preparation",
        6 => "cave_clue",
        7 => "world_change_omen",
        8 => "heart_tempering",
        9 => "high_realm_deduction",
        10 => "native_involvement",
        11 => "old_fragment",
        12 => "same_law_stagnation",
        13 => "foundation_trade",
        14 => "cave_trade",
        15 => "miasma_cleansing",
        16 => "alliance_archive",
        17 => "elder_secret",
        18 => "ruin_second_clue",
        19 => "world_change_afterglow",
        _ => "minor_opportunity"
    };

    private static int EventInterval(int kind, int realmIndex) => kind switch
    {
        2 => 10,
        4 or 5 or 6 or 7 => 14,
        11 or 12 => 18,
        13 or 14 => 20,
        15 => 12,
        16 or 17 => 18,
        18 or 19 => 20,
        8 => 8,
        9 => Math.Max(10, 18 - realmIndex),
        10 => 16,
        _ => 6
    };

    private static void AddClamped(Actor actor, string key, int delta, int min, int max)
    {
        int current = MclslActorAccessor.GetInt(actor, key, min);
        MclslActorAccessor.Set(actor, key, Math.Clamp(current + delta, min, max));
    }

    private static void Event(Actor actor, int year, string type, string title, string body)
    {
        MclslWorldRunRepository.AddEvent(year, type, title, body, actor);
    }

    private static int PositiveHash(string value)
    {
        unchecked { int hash = 83; foreach (char c in value ?? string.Empty) hash = hash * 47 + c; return hash & int.MaxValue; }
    }
}
