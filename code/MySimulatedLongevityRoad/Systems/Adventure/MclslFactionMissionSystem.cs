using System;
using System.Collections.Generic;
using MySimulatedLongevityRoad.Core;
using MySimulatedLongevityRoad.Data;

namespace MySimulatedLongevityRoad.Systems;

internal static class MclslFactionMissionSystem
{
    private const string WanXian = "wanxian";
    private const string FiveElders = "five_elders";

    internal static void TickAnnual(int year)
    {
        MclslWorldRunState run = MclslWorldRunRepository.Current;
        if (run == null || string.IsNullOrWhiteSpace(run.RunId)) return;
        Normalize(run, year);
        if (year < run.NextFactionMissionYear) return;
        if (!MclslDetectionGate.TryBeginAnnualJob(MclslDetectionGate.AnnualFactionMission, year)) return;

        List<Actor> candidates = new(MclslCultivatorCandidateIndex.SelectCultivators(
            80,
            MclslActorAccessor.Alive,
            MissionCandidateScore));
        if (candidates.Count == 0)
        {
            run.NextFactionMissionYear = year + 5;
            return;
        }

        int missionCount = Math.Clamp(1 + candidates.Count / 40, 1, 4);
        int seed = PositiveHash(run.RunId + "|faction_mission|" + year + "|" + run.FactionMissions.Count);
        for (int i = 0; i < missionCount; i++)
        {
            Actor actor = candidates[(seed + i * 17) % candidates.Count];
            if (!MclslActorAccessor.Alive(actor)) continue;
            string faction = PickFaction(run, actor, year, i);
            ResolveMission(run, actor, faction, year, i);
        }

        UpdatePolicies(run);
        run.NextFactionMissionYear = year + Math.Clamp(6 + PositiveHash(run.RunId + "|next_faction|" + year) % 7, 6, 12);
        MclslWorldArchiveStore.MarkDirty();
    }

    internal static void Clear()
    {
    }

    private static void ResolveMission(MclslWorldRunState run, Actor actor, string faction, int year, int order)
    {
        int realmIndex = Math.Max(0, MclslRealmIds.Index(MclslActorAccessor.Realm(actor)));
        int aptitude = Math.Clamp(MclslActorAccessor.GetInt(actor, MclslActorDataKeys.Aptitude, 50), 1, 100);
        int mind = MclslMindSystem.EnsureMindState(actor);
        int roll = PositiveHash(MclslActorAccessor.Id(actor) + "|" + faction + "|mission|" + year + "|" + order);
        int quality = Math.Clamp(1 + realmIndex / 2 + roll % 3, 1, 4);

        string missionName;
        string outcome;
        int contribution;
        int stones;
        int influenceDelta;
        if (faction == WanXian)
        {
            missionName = WanXianMissionName(roll);
            contribution = 18 + quality * 10 + realmIndex * 6 + mind / 18;
            stones = 5 + quality * 8 + realmIndex * 4;
            influenceDelta = InfluenceStep(roll, quality);
            ApplyWanXianReward(actor, realmIndex, quality, roll);
            run.BackgroundFactions.WanXianAllianceInfluence = Math.Clamp(run.BackgroundFactions.WanXianAllianceInfluence + influenceDelta, 0, 100);
            run.BackgroundFactions.FiveEldersSubversion = Math.Max(0, run.BackgroundFactions.FiveEldersSubversion - quality);
            outcome = "完成万仙盟委托，秩序记录加深";
        }
        else
        {
            missionName = FiveEldersMissionName(roll);
            contribution = 8 + quality * 7 + realmIndex * 4;
            stones = 28 + quality * 22 + aptitude / 6;
            influenceDelta = InfluenceStep(roll / 7, quality);
            ApplyFiveEldersReward(run, actor, realmIndex, quality, year, roll);
            run.BackgroundFactions.FiveEldersInfluence = Math.Clamp(run.BackgroundFactions.FiveEldersInfluence + influenceDelta, 0, 100);
            run.BackgroundFactions.AllianceOrderPressure = Math.Max(0, run.BackgroundFactions.AllianceOrderPressure - 1);
            outcome = "暗中完成五老会委托，潜伏痕迹加深";
        }

        MclslResourceSystem.GrantFactionReward(actor, contribution, stones);
        if (string.IsNullOrWhiteSpace(MclslActorAccessor.GetString(actor, MclslActorDataKeys.FactionAffiliation, string.Empty)))
            MclslActorAccessor.Set(actor, MclslActorDataKeys.FactionAffiliation, faction);
        MclslFactionExchangeSystem.TryExchangeTechnique(actor, faction, year, roll, out string exchangeSummary);
        MclslFactionMissionRecord record = new()
        {
            Id = "faction_" + year + "_" + MclslWorldRunRepository.NextProceduralSequence(),
            Year = year,
            FactionId = faction,
            FactionName = faction == WanXian ? "万仙盟" : "五老会",
            MissionName = missionName,
            ActorId = MclslActorAccessor.Id(actor),
            ActorName = SafeName(actor),
            RealmName = MclslRealmIds.Display(MclslActorAccessor.Realm(actor)),
            Outcome = outcome,
            ContributionReward = contribution,
            SpiritStoneReward = stones,
            InfluenceDelta = influenceDelta,
            Summary = SafeName(actor) + "受" + (faction == WanXian ? "万仙盟" : "五老会") + "委托“" + missionName + "”，得贡献" + contribution + "、灵石" + stones + "。"
        };
        if (!string.IsNullOrWhiteSpace(exchangeSummary))
            record.Summary += " " + exchangeSummary;
        MclslWorldRunRepository.RegisterFactionMission(record);
        if (MclslRuntimeSettings.FactionCommissionAnnouncementsEnabled)
            MclslWorldRunRepository.AddEvent(year, "faction_mission", record.FactionName + "委托", record.Summary + outcome + "。");
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

    private static int MissionCandidateScore(Actor actor)
    {
        int realm = Math.Max(0, MclslRealmIds.Index(MclslActorAccessor.Realm(actor)));
        int contribution = Math.Min(80, MclslActorAccessor.GetInt(actor, MclslActorDataKeys.Contribution, 0) / 5);
        int experience = Math.Min(60, MclslActorAccessor.GetInt(actor, MclslActorDataKeys.RuinExperience, 0));
        return realm * 50 + contribution + experience + MclslMindSystem.EnsureMindState(actor) / 3;
    }

    private static void Normalize(MclslWorldRunState run, int year)
    {
        run.BackgroundFactions ??= new MclslBackgroundFactionState();
        run.FactionMissions ??= new List<MclslFactionMissionRecord>();
        if (run.NextFactionMissionYear <= 0) run.NextFactionMissionYear = year + 3;
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

    private static string WanXianMissionName(int seed)
    {
        string[] names = { "巡查灵脉", "缉录散修", "护送仙册", "清点遗藏", "核验功法", "平息灵潮" };
        return names[(seed & int.MaxValue) % names.Length];
    }

    private static string FiveEldersMissionName(int seed)
    {
        string[] names = { "夺取残卷", "暗探遗迹", "扰乱灵脉", "遮掩洞天", "收买散修", "引动灾兆" };
        return names[(seed & int.MaxValue) % names.Length];
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
