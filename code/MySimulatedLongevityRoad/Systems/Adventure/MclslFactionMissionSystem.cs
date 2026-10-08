using System;
using System.Collections.Generic;
using MySimulatedLongevityRoad.Core;
using MySimulatedLongevityRoad.Data;
using MySimulatedLongevityRoad.Systems.Death;

namespace MySimulatedLongevityRoad.Systems;

internal static class MclslFactionMissionSystem
{
    private const string WanXian = "wanxian";
    private const string FiveElders = "five_elders";
    internal static readonly (string Id, string Name, string Mission)[] AncientSects =
    {
        ("ancient_dadao", "大道宗", "编修道藏"), ("ancient_tianjian", "天剑宗", "试剑守关"),
        ("ancient_yushou", "御兽宗", "护养灵兽"), ("ancient_wuding", "无定狱", "缉拿逃修"),
        ("ancient_tianshu", "天枢宗", "观星测轨"), ("ancient_yishi", "一始宗", "寻访本源"),
        ("ancient_taishang", "太上宗", "誊录道经"), ("ancient_taiyan", "太衍宗", "推演阵图"),
        ("ancient_zaohua", "造化宗", "修复灵脉"), ("ancient_xuanheng", "玄衡宗", "校衡地脉")
    };

    internal static string DisplaySectText(string value) => string.IsNullOrEmpty(value)
        ? value : value.Replace("玄衡宗（模组原创）", "玄衡宗");

    internal static string CurrentSectName(Actor actor, int year)
    {
        if (actor?.data == null || MclslWorldEpochSystem.IsNewLawActive(year)) return string.Empty;
        string id = MclslActorAccessor.GetString(actor, MclslActorDataKeys.AncientSectAffiliation);
        foreach (var sect in AncientSects) if (sect.Id == id) return sect.Name;
        return DisplaySectText(MclslActorAccessor.GetString(actor, MclslActorDataKeys.AncientSectOrigin));
    }

    internal static int TierForRealm(int realmIndex) => realmIndex < 2 ? 0 : realmIndex < 4 ? 1 : 2;
    internal static string TierName(int tier) => tier switch { 0 => "低阶", 1 => "中阶", _ => "高阶" };
    internal static string RealmBand(int tier) => tier switch
    {
        0 => "炼气、筑基", 1 => "金丹、元婴", _ => "化神、合道、长生"
    };

    // Each value is basis points out of 10,000. Death is conditional on failure.
    internal static (int Fail, int DeathAfterFail) RiskForRealm(int realmIndex) => realmIndex switch
    {
        0 => (1000, 100), 1 => (700, 50), 2 => (2000, 400),
        3 => (1400, 200), 4 => (3000, 800), 5 => (2200, 500),
        6 => (1500, 300), _ => (0, 0)
    };

    internal static void TickAnnual(int year)
    {
        MclslWorldRunState run = MclslWorldRunRepository.Current;
        if (run == null || string.IsNullOrWhiteSpace(run.RunId)) return;
        bool ancient = year < run.AncientLawEndYear;
        if (!ancient && !run.WanxianAllianceFounded) return;
        Normalize(run, year);
        string missionEpoch = ancient ? "ancient" : "new_law";
        if (!string.Equals(run.LastFactionMissionEpoch, missionEpoch, StringComparison.Ordinal))
        {
            if (run.LastFactionMissionEpoch == "ancient" && !ancient)
                run.NextFactionMissionYear = year + 6 + PositiveHash(run.RunId + "|new_law_first|" + year) % 7;
            run.LastFactionMissionEpoch = missionEpoch;
            MclslWorldArchiveStore.MarkDirty();
        }
        if (year < run.NextFactionMissionYear) return;
        if (!MclslDetectionGate.TryBeginAnnualJob(MclslDetectionGate.AnnualFactionMission, year)) return;

        IReadOnlyList<Actor>[] tierCandidates =
        {
            MclslActorProjectionIndex.MissionCandidates(0, 80),
            MclslActorProjectionIndex.MissionCandidates(1, 80),
            MclslActorProjectionIndex.MissionCandidates(2, 80)
        };
        int candidateCount = tierCandidates[0].Count + tierCandidates[1].Count + tierCandidates[2].Count;
        if (candidateCount == 0)
        {
            run.NextFactionMissionYear = year + (ancient
                ? 10 + PositiveHash(run.RunId + "|next_empty_sect|" + year) % 7
                : 6 + PositiveHash(run.RunId + "|next_empty_faction|" + year) % 7);
            MclslWorldArchiveStore.MarkDirty();
            return;
        }

        int missionCount = Math.Clamp(1 + candidateCount / 40, 1, ancient ? 3 : 4);
        int seed = PositiveHash(run.RunId + "|faction_mission|" + year + "|" + run.FactionMissions.Count);
        HashSet<long> selected = new();
        for (int i = 0; i < missionCount; i++)
        {
            Actor actor = null;
            for (int offset = 0; offset < 3 && actor == null; offset++)
            {
                int tier = (seed + i + offset) % 3;
                IReadOnlyList<Actor> bucket = tierCandidates[tier];
                for (int scan = 0; scan < bucket.Count; scan++)
                {
                    Actor candidate = bucket[(seed + i * 17 + scan) % bucket.Count];
                    long id = MclslActorAccessor.Id(candidate);
                    if (id <= 0 || selected.Contains(id) || !MclslActorAccessor.Alive(candidate)
                        || MclslRealmIds.Index(MclslActorAccessor.Realm(candidate)) < 0
                        || TierForRealm(MclslRealmIds.Index(MclslActorAccessor.Realm(candidate))) != tier) continue;
                    selected.Add(id);
                    actor = candidate;
                    break;
                }
            }
            if (actor == null) continue;
            if (ancient) ResolveAncientMission(run, actor, year, i);
            else ResolveMission(run, actor, PickFaction(run, actor, year, i), year, i);
        }

        UpdatePolicies(run);
        run.NextFactionMissionYear = year + (ancient
            ? 10 + PositiveHash(run.RunId + "|next_sect|" + year) % 7
            : 6 + PositiveHash(run.RunId + "|next_faction|" + year) % 7);
        MclslWorldArchiveStore.MarkDirty();
    }

    private static void ResolveAncientMission(MclslWorldRunState run, Actor actor, int year, int order)
    {
        string affiliation = MclslActorAccessor.GetString(actor, MclslActorDataKeys.AncientSectAffiliation, string.Empty);
        int roll = PositiveHash(MclslActorAccessor.Id(actor) + "|ancient_sect|" + year + "|" + order);
        int index = Array.FindIndex(AncientSects, sect => sect.Id == affiliation);
        if (index < 0) index = roll % AncientSects.Length;
        var sect = AncientSects[index];
        if (string.IsNullOrWhiteSpace(affiliation))
        {
            MclslActorAccessor.Set(actor, MclslActorDataKeys.AncientSectAffiliation, sect.Id);
            MclslActorAccessor.Set(actor, MclslActorDataKeys.AncientSectOrigin, sect.Name);
        }
        int realm = MclslRealmIds.Index(MclslActorAccessor.Realm(actor));
        if (realm < 0) return;
        MclslMissionDefinition mission = MclslFactionMissionCatalog.PickAncient(index, TierForRealm(realm),
            PositiveHash(MclslActorAccessor.Id(actor) + "|sect_type|" + year + "|" + order));
        int stones = 0;
        string exchangeSummary = string.Empty;
        string actorName = SafeName(actor);
        long actorId = MclslActorAccessor.Id(actor);
        string realmName = MclslRealmIds.Display(MclslActorAccessor.Realm(actor));
        bool failed = Fails(run, actor, year, order, sect.Id, mission.Key, mission.Name, realm, out bool died);
        if (MclslMissionSettlementPolicy.CanReward(failed, died))
        {
            int baseStones = mission.Stones + 3 * realm + roll % 5;
            stones = MclslResourceSystem.GrantFactionReward(actor, 0, baseStones).SpiritStones;
            MclslMaterialDiscovery.TryDiscoverFactionReward(actor, year, sect.Id + "|" + order + "|" + roll, highestQuality: realm >= 4);
            exchangeSummary = TryExchangeAncientTechnique(actor, sect.Name, year, roll);
        }
        MclslFactionMissionRecord record = new()
        {
            Id = "sect_" + year + "_" + MclslWorldRunRepository.NextProceduralSequence(),
            Year = year, FactionId = sect.Id, FactionName = sect.Name, MissionName = mission.Name,
            ActorId = actorId, ActorName = actorName,
            RealmName = realmName,
            Outcome = failed ? died ? "任务中陨落" : "委托失败" : "完成旧法宗门委托",
            ResultCode = MclslMissionSettlementPolicy.ResultCode(failed, died),
            ContributionReward = 0, SpiritStoneReward = stones,
            Summary = actorName + "受" + sect.Name + "委托“" + mission.Name + "”，" +
                (failed ? died ? "任务失败并陨落，未获报酬。" : "任务失败，未获报酬。"
                    : "得灵石" + stones + "。" + exchangeSummary)
        };
        MclslWorldRunRepository.RegisterFactionMission(record);
        if (MclslRuntimeSettings.FactionCommissionAnnouncementsEnabled)
            MclslWorldRunRepository.AddEvent(year, "ancient_sect_mission", sect.Name + "委托", record.Summary);
    }

    private static bool Fails(MclslWorldRunState run, Actor actor, int year, int order,
        string faction, string missionKey, string missionName, int realm, out bool died)
    {
        died = false;
        (int failChance, int deathChance) = RiskForRealm(realm);
        string source = run.RunId + "|" + MclslActorAccessor.Id(actor) + "|" + year + "|" + order
            + "|" + faction + "|" + missionKey;
        if (PositiveHash(source + "|fail") % 10000 >= failChance) return false;
        if (PositiveHash(source + "|death") % 10000 < deathChance)
            died = MclslDeathSystem.ExecuteScriptedDeath(actor, "faction_mission", faction,
                SafeName(actor) + "执行“" + missionName + "”时失手陨落。", true);
        return true;
    }

    private static string TryExchangeAncientTechnique(Actor actor, string sectName, int year, int roll)
    {
        if (roll % 100 >= 34) return string.Empty;
        int current = MclslRealmIds.Index(MclslTechniqueRealmLimit.MaxRealm(actor));
        int target = Math.Clamp(current + 1, 1, MclslRealmIds.Index(MclslRealmIds.HeDao));
        if (target <= current) return string.Empty;
        int[] costs = { 0, 420, 1350, 3400, 7600, 14500 };
        int cost = costs[target];
        long stones = MclslActorAccessor.GetMoney(actor, MclslActorDataKeys.SpiritStones, 0);
        if (stones < cost) return string.Empty;
        List<MclslTechniqueDefinition> candidates = new();
        foreach (MclslTechniqueDefinition technique in MclslCultivationCatalog.Techniques)
            if (MclslRealmIds.Index(technique.MaxRealm) == target) candidates.Add(technique);
        if (candidates.Count == 0) return string.Empty;
        MclslTechniqueDefinition selected = candidates[roll % candidates.Count];
        string paymentSource = "ancient-technique-exchange/" + MclslEconomyCommands.Account(actor);
        if (MclslEconomyCommands.WasApplied(paymentSource, year)) return string.Empty;
        MclslTechniqueMutationSnapshot snapshot = MclslTechniqueMutationSnapshot.Capture(actor);
        try
        {
            MclslActorAccessor.Set(actor, MclslActorDataKeys.TechniqueId, selected.Id);
            MclslActorAccessor.Set(actor, MclslActorDataKeys.TechniqueName, selected.Name);
            MclslTechniqueRealmLimit.SetMaxRealm(actor, selected.MaxRealm);
            MclslTechniqueStageSystem.AddProgress(actor, 5 + target);
            if (!MclslEconomyCommands.TryConsume(actor, MclslActorDataKeys.SpiritStones, cost,
                paymentSource, year))
                throw new InvalidOperationException("宗门功法兑换资金提交失败");
        }
        catch (Exception ex)
        {
            snapshot.Restore(actor);
            MclslDiagnostics.Error("ancient-technique-exchange", ex.Message);
            return string.Empty;
        }
        string summary = "耗灵石" + cost + "，自" + sectName + "换得《" + selected.Name + "》，功法上限至"
            + MclslRealmIds.Display(selected.MaxRealm) + "。";
        MclslWorldRunRepository.RegisterResourceSpend(new MclslResourceSpendRecord
        {
            Id = "spend_" + year + "_" + MclslWorldRunRepository.NextProceduralSequence(),
            Year = year, ActorId = MclslActorAccessor.Id(actor), ActorName = SafeName(actor),
            RealmName = MclslRealmIds.Display(MclslActorAccessor.Realm(actor)),
            ItemName = sectName + "功法兑换", ContributionCost = 0, SpiritStoneCost = cost,
            EffectText = "换得《" + selected.Name + "》", Summary = summary
        });
        return summary;
    }

    private static void ResolveMission(MclslWorldRunState run, Actor actor, string faction, int year, int order)
    {
        int realmIndex = MclslRealmIds.Index(MclslActorAccessor.Realm(actor));
        if (realmIndex < 0) return;
        int aptitude = Math.Clamp(MclslActorAccessor.GetInt(actor, MclslActorDataKeys.Aptitude, 50), 1, 100);
        int mind = MclslMindSystem.EnsureMindState(actor);
        int roll = PositiveHash(MclslActorAccessor.Id(actor) + "|" + faction + "|mission|" + year + "|" + order);
        int quality = Math.Clamp(1 + realmIndex / 2 + roll % 3, 1, 4);
        MclslMissionDefinition mission = MclslFactionMissionCatalog.PickNewLaw(faction, TierForRealm(realmIndex),
            PositiveHash(MclslActorAccessor.Id(actor) + "|" + faction + "|mission_type|" + year + "|" + order));
        string factionName = faction == WanXian ? "万仙盟" : "五老会";
        string actorName = SafeName(actor);
        long actorId = MclslActorAccessor.Id(actor);
        string realmName = MclslRealmIds.Display(MclslActorAccessor.Realm(actor));
        if (string.IsNullOrWhiteSpace(MclslActorAccessor.GetString(actor, MclslActorDataKeys.FactionAffiliation, string.Empty)))
            MclslActorAccessor.Set(actor, MclslActorDataKeys.FactionAffiliation, faction);

        bool failed = Fails(run, actor, year, order, faction, mission.Key, mission.Name, realmIndex, out bool died);
        int contribution = 0, stones = 0, influenceDelta = 0;
        string outcome = failed ? died ? "任务中陨落" : "委托失败" :
            faction == WanXian ? "完成万仙盟委托，秩序记录加深" : "暗中完成五老会委托，潜伏痕迹加深";
        string exchangeSummary = string.Empty;
        if (MclslMissionSettlementPolicy.CanReward(failed, died))
        {
            int baseContribution, baseStones;
        if (faction == WanXian)
        {
            baseContribution = 18 + quality * 10 + realmIndex * 6 + mind / 18;
            baseStones = 5 + quality * 8 + realmIndex * 4 + mission.Stones;
            influenceDelta = InfluenceStep(roll, quality);
            ApplyWanXianReward(actor, realmIndex, quality, roll);
            run.BackgroundFactions.WanXianAllianceInfluence = Math.Clamp(run.BackgroundFactions.WanXianAllianceInfluence + influenceDelta, 0, 100);
            run.BackgroundFactions.FiveEldersSubversion = Math.Max(0, run.BackgroundFactions.FiveEldersSubversion - quality);
        }
        else
        {
            baseContribution = 8 + quality * 7 + realmIndex * 4;
            baseStones = 28 + quality * 22 + aptitude / 6 + mission.Stones;
            influenceDelta = InfluenceStep(roll / 7, quality);
            ApplyFiveEldersReward(run, actor, realmIndex, quality, year, roll);
            run.BackgroundFactions.FiveEldersInfluence = Math.Clamp(run.BackgroundFactions.FiveEldersInfluence + influenceDelta, 0, 100);
            run.BackgroundFactions.AllianceOrderPressure = Math.Max(0, run.BackgroundFactions.AllianceOrderPressure - 1);
        }

            (contribution, stones) = MclslResourceSystem.GrantFactionReward(actor, baseContribution, baseStones);
            MclslMaterialDiscovery.TryDiscoverFactionReward(actor, year,
                faction + "|" + order + "|" + roll, highestQuality: quality >= 4);
            MclslFactionExchangeSystem.TryExchangeTechnique(actor, faction, year, roll, out exchangeSummary);
        }
        MclslFactionMissionRecord record = new()
        {
            Id = "faction_" + year + "_" + MclslWorldRunRepository.NextProceduralSequence(),
            Year = year,
            FactionId = faction,
            FactionName = factionName,
            MissionName = mission.Name,
            ActorId = actorId,
            ActorName = actorName,
            RealmName = realmName,
            Outcome = outcome,
            ResultCode = MclslMissionSettlementPolicy.ResultCode(failed, died),
            ContributionReward = contribution,
            SpiritStoneReward = stones,
            InfluenceDelta = influenceDelta,
            Summary = actorName + "受" + factionName + "委托“" + mission.Name + "”，" +
                (failed ? died ? "任务失败并陨落，未获报酬。" : "任务失败，未获报酬。"
                    : "得贡献" + contribution + "、灵石" + stones + "。")
        };
        if (!string.IsNullOrWhiteSpace(exchangeSummary))
            record.Summary += " " + exchangeSummary;
        MclslWorldRunRepository.RegisterFactionMission(record);
        if (MclslRuntimeSettings.FactionCommissionAnnouncementsEnabled)
            MclslWorldRunRepository.AddEvent(year, "faction_mission", record.FactionName + "委托", record.Summary + outcome + "。");
        if (MclslActorAccessor.Alive(actor))
            MclslActorAccessor.Set(actor, MclslActorDataKeys.LastBreakthroughResult, record.Summary);
    }

    private static void ApplyWanXianReward(Actor actor, int realmIndex, int quality, int roll)
    {
        if (realmIndex <= 0)
        {
            int bonus = 4 + quality * 3;
            MclslActorAccessor.Set(actor, MclslActorDataKeys.FoundationChanceBonus, Math.Min(55, MclslActorAccessor.GetInt(actor, MclslActorDataKeys.FoundationChanceBonus, 0) + bonus));
            return;
        }
        if (realmIndex == MclslRealmIds.Index(MclslRealmIds.ZhuJi))
        {
            int insight = 4 + quality * 3;
            MclslActorAccessor.Set(actor, MclslActorDataKeys.TechniqueInsight, MclslActorAccessor.GetInt(actor, MclslActorDataKeys.TechniqueInsight, 0) + insight);
            return;
        }
        if (realmIndex == MclslRealmIds.Index(MclslRealmIds.JinDan))
        {
            int bonus = 5 + quality * 4;
            MclslActorAccessor.Set(actor, MclslActorDataKeys.CaveClaimBonus, Math.Min(75, MclslActorAccessor.GetInt(actor, MclslActorDataKeys.CaveClaimBonus, 0) + bonus));
            return;
        }
        int heart = 3 + quality * 2 + roll % 4;
        MclslActorAccessor.Set(actor, MclslActorDataKeys.HeartTemperingProgress, Math.Min(100, MclslActorAccessor.GetInt(actor, MclslActorDataKeys.HeartTemperingProgress, 0) + heart));
    }

    private static int InfluenceStep(int roll, int quality)
    {
        return roll % 100 < 30 + quality * 8 ? 1 : 0;
    }

    private static void ApplyFiveEldersReward(MclslWorldRunState run, Actor actor, int realmIndex, int quality, int year, int roll)
    {
        int ruin = 1 + quality + roll % 3;
        MclslActorAccessor.Set(actor, MclslActorDataKeys.RuinExperience, MclslActorAccessor.GetInt(actor, MclslActorDataKeys.RuinExperience, 0) + ruin);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.HeartMethodKnown, 1);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.HeartTemperingProgress, Math.Min(100, MclslActorAccessor.GetInt(actor, MclslActorDataKeys.HeartTemperingProgress, 0) + 4 + quality * 3));

        if (realmIndex >= MclslRealmIds.Index(MclslRealmIds.YuanYing) && roll % 100 < 18 + quality * 4)
        {
            string[] laws = MclslGeneratedObjectFactory.SplitTags(MclslActorAccessor.GetString(actor, MclslActorDataKeys.NascentEssenceTags,
                MclslActorAccessor.GetString(actor, MclslActorDataKeys.GoldenCoreLaws, "阴,转化")));
            MclslWorldChangeSystem.CreateFromTimeline(year, "五老会暗中搅动天地", laws);
        }
    }

    private static string PickFaction(MclslWorldRunState run, Actor actor, int year, int order)
    {
        if (!run.FiveEldersFounded) return WanXian;
        string affiliation = NormalizeFaction(MclslActorAccessor.GetString(actor, MclslActorDataKeys.FactionAffiliation, string.Empty));
        if (!string.IsNullOrWhiteSpace(affiliation)) return affiliation;

        int alliance = Math.Clamp(run.BackgroundFactions.WanXianAllianceInfluence, 0, 100);
        int elders = Math.Clamp(run.BackgroundFactions.FiveEldersInfluence, 0, 100);
        int realmIndex = Math.Max(0, MclslRealmIds.Index(MclslActorAccessor.Realm(actor)));
        int roll = PositiveHash(MclslActorAccessor.Id(actor) + "|pick_faction|" + year + "|" + order) % Math.Max(1, alliance + elders + 25);
        int allianceWeight = alliance + 8 + realmIndex * 2;
        return roll < allianceWeight ? WanXian : FiveElders;
    }

    private static string NormalizeFaction(string faction)
    {
        return faction switch
        {
            WanXian or "万仙盟" => WanXian,
            FiveElders or "五老会" => FiveElders,
            _ => string.Empty
        };
    }

    internal static int MissionCandidateScore(Actor actor)
    {
        int realm = Math.Max(0, MclslRealmIds.Index(MclslActorAccessor.Realm(actor)));
        int contribution = Math.Min(80, MclslResourceSystem.ContributionInfluence(actor, 5));
        int experience = Math.Min(60, MclslActorAccessor.GetInt(actor, MclslActorDataKeys.RuinExperience, 0));
        return realm * 50 + contribution + experience + MclslMindSystem.EnsureMindState(actor) / 3;
    }

    private static void Normalize(MclslWorldRunState run, int year)
    {
        run.BackgroundFactions ??= new MclslBackgroundFactionState();
        run.FactionMissions ??= new List<MclslFactionMissionRecord>();
        if (run.NextFactionMissionYear <= 0)
            run.NextFactionMissionYear = year + (year < run.AncientLawEndYear
                ? 10 + PositiveHash(run.RunId + "|initial_sect|" + year) % 7
                : 6 + PositiveHash(run.RunId + "|initial_faction|" + year) % 7);
        UpdatePolicies(run);
    }

    private static void UpdatePolicies(MclslWorldRunState run)
    {
        MclslBackgroundFactionState f = run.BackgroundFactions;
        f.AlliancePolicy = f.WanXianAllianceInfluence switch
        {
            >= 75 => "明令巡天",
            >= 45 => "监察诸修",
            >= 20 => "远观诸国",
            _ => "势弱收缩"
        };
        f.FiveEldersPolicy = f.FiveEldersInfluence switch
        {
            >= 70 => "潜伏成网",
            >= 40 => "暗布棋子",
            >= 18 => "潜伏暗流",
            _ => "蛰伏避锋"
        };
    }

    private static string SafeName(Actor actor)
    {
        try { return MclslActorAccessor.DisplayName(actor); }
        catch { return "无名者"; }
    }

    private static int PositiveHash(string value)
    {
        unchecked { int hash = 67; foreach (char c in value ?? string.Empty) hash = hash * 71 + c; return hash & int.MaxValue; }
    }
}
