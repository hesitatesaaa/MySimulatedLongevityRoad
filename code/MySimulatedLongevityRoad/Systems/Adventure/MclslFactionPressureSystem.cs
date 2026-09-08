using System;
using System.Collections.Generic;
using MySimulatedLongevityRoad.Data;
using MySimulatedLongevityRoad.Core;

namespace MySimulatedLongevityRoad.Systems;

internal static class MclslFactionPressureSystem
{
    private const string WanXian = "wanxian";
    private const string FiveElders = "five_elders";
    private const int CandidateLimit = 100;
    private const int AffectedActorLimit = 4;

    internal static void TickAnnual(int year)
    {
        MclslWorldRunState run = MclslWorldRunRepository.Current;
        if (run == null || string.IsNullOrWhiteSpace(run.RunId)) return;
        Normalize(run, year);
        if (year < run.NextFactionPressureYear) return;
        if (!MclslDetectionGate.TryBeginAnnualJob(MclslDetectionGate.AnnualFactionPressure, year)) return;

        int allianceLevel = PressureLevel(run.BackgroundFactions.WanXianAllianceInfluence);
        int elderLevel = PressureLevel(run.BackgroundFactions.FiveEldersInfluence);
        if (allianceLevel <= 0 && elderLevel <= 0)
        {
            run.NextFactionPressureYear = NextYear(run, year, 8, 13);
            return;
        }

        List<Actor> candidates = new(MclslCultivatorCandidateIndex.SelectCultivators(
            CandidateLimit,
            MclslActorAccessor.Alive,
            PressureCandidateScore));

        int seed = PositiveHash(run.RunId + "|faction_pressure|" + year + "|" + run.FactionPressureEvents.Count);
        if (allianceLevel > 0) ResolveWanXianPressure(run, candidates, year, allianceLevel, seed);
        if (elderLevel > 0) ResolveFiveEldersPressure(run, candidates, year, elderLevel, seed / 7 + 31);

        UpdatePolicies(run);
        run.NextFactionPressureYear = NextYear(run, year, 6, 11);
        MclslWorldArchiveStore.MarkDirty();
    }

    internal static void Clear()
    {
    }

    private static void ResolveWanXianPressure(MclslWorldRunState run, List<Actor> candidates, int year, int level, int seed)
    {
        string policy = level switch { >= 3 => "明令巡天", 2 => "仙盟清册", _ => "监察诸修" };
        List<Actor> affected = PickActors(candidates, year, seed, preferOrder: true);
        foreach (Actor actor in affected) ApplyWanXianPressure(actor, level, year, policy);

        MclslBackgroundFactionState factions = run.BackgroundFactions;
        int pressureDelta = MclslWorldStateModifierSystem.ScaleFactionPressureDelta(3 + level * 3, year);
        factions.AllianceOrderPressure = Math.Clamp(factions.AllianceOrderPressure + pressureDelta, 0, 100);
        factions.FiveEldersSubversion = Math.Max(0, factions.FiveEldersSubversion - (2 + level * 2));
        if (level >= 3) factions.FiveEldersInfluence = Math.Max(0, factions.FiveEldersInfluence - 1);

        string names = ActorNames(affected);
        string effect = level >= 3
            ? "巡天令压过暗线，五老会影响略降"
            : "仙盟清册扩张，登记修士获得贡献与修行线索";
        Register(run, year, WanXian, "万仙盟", policy, factions.WanXianAllianceInfluence, pressureDelta, names, effect,
            "万仙盟推行“" + policy + "”，" + effect + (string.IsNullOrWhiteSpace(names) ? "。" : "，牵涉：" + names + "。"));
    }

    private static void ResolveFiveEldersPressure(MclslWorldRunState run, List<Actor> candidates, int year, int level, int seed)
    {
        string policy = level switch { >= 3 => "潜伏成网", 2 => "遗迹设伏", _ => "暗布棋子" };
        List<Actor> affected = PickActors(candidates, year, seed, preferOrder: false);
        foreach (Actor actor in affected) ApplyFiveEldersPressure(actor, level, year, policy);

        MclslBackgroundFactionState factions = run.BackgroundFactions;
        int pressureDelta = MclslWorldStateModifierSystem.ScaleFactionPressureDelta(3 + level * 4, year);
        factions.FiveEldersSubversion = Math.Clamp(factions.FiveEldersSubversion + pressureDelta, 0, 100);
        factions.AllianceOrderPressure = Math.Max(0, factions.AllianceOrderPressure - (1 + level));
        if (level >= 3) factions.WanXianAllianceInfluence = Math.Max(0, factions.WanXianAllianceInfluence - 1);

        string worldEffect = IntensifyRuin(run, seed, level);
        if (level >= 2 && PositiveHash(run.RunId + "|elder_change|" + year + "|" + seed) % 100 < 22 + level * 8)
        {
            string[] laws = PickDisturbanceLaws(seed);
            MclslWorldChangeRecord change = MclslWorldChangeSystem.CreateFromTimeline(year, "五老会暗线搅动天地", laws);
            if (change != null) worldEffect += (string.IsNullOrWhiteSpace(worldEffect) ? string.Empty : "；") + "引动天地之变“" + change.Name + "”";
        }

        string names = ActorNames(affected);
        if (string.IsNullOrWhiteSpace(worldEffect)) worldEffect = "暗线渗透加深，尚未形成显性天地异动";
        Register(run, year, FiveElders, "五老会", policy, factions.FiveEldersInfluence, pressureDelta, names, worldEffect,
            "五老会推行“" + policy + "”，" + worldEffect + (string.IsNullOrWhiteSpace(names) ? "。" : "，牵涉：" + names + "。"));
    }

    private static void ApplyWanXianPressure(Actor actor, int level, int year, string policy)
    {
        if (!MclslActorAccessor.Alive(actor)) return;
        int realmIndex = Math.Max(0, MclslRealmIds.Index(MclslActorAccessor.Realm(actor)));
        MclslResourceSystem.GrantFactionReward(actor, 10 + level * 8 + realmIndex * 3, 3 + level * 4);
        if (realmIndex <= MclslRealmIds.Index(MclslRealmIds.ZhuJi))
        {
            int bonus = 2 + level * 3;
            MclslActorAccessor.Set(actor, MclslActorDataKeys.FoundationChanceBonus,
                Math.Min(65, MclslActorAccessor.GetInt(actor, MclslActorDataKeys.FoundationChanceBonus, 0) + bonus));
        }
        else if (realmIndex == MclslRealmIds.Index(MclslRealmIds.JinDan))
        {
            int bonus = 4 + level * 4;
            MclslActorAccessor.Set(actor, MclslActorDataKeys.CaveClaimBonus,
                Math.Min(80, MclslActorAccessor.GetInt(actor, MclslActorDataKeys.CaveClaimBonus, 0) + bonus));
        }
        else
        {
            int insight = 3 + level * 3;
            MclslActorAccessor.Set(actor, MclslActorDataKeys.TechniqueInsight,
                MclslActorAccessor.GetInt(actor, MclslActorDataKeys.TechniqueInsight, 0) + insight);
        }
        MclslActorAccessor.Set(actor, MclslActorDataKeys.LastBreakthroughResult, "万仙盟" + policy + "登记入册，得贡献与修行线索");
    }

    private static void ApplyFiveEldersPressure(Actor actor, int level, int year, string policy)
    {
        if (!MclslActorAccessor.Alive(actor)) return;
        int realmIndex = Math.Max(0, MclslRealmIds.Index(MclslActorAccessor.Realm(actor)));
        MclslResourceSystem.GrantFactionReward(actor, 2 + level * 3, 14 + level * 12 + realmIndex * 4);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.RuinExperience,
            MclslActorAccessor.GetInt(actor, MclslActorDataKeys.RuinExperience, 0) + 2 + level * 3);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.HeartMethodKnown, 1);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.HeartTemperingProgress,
            Math.Min(100, MclslActorAccessor.GetInt(actor, MclslActorDataKeys.HeartTemperingProgress, 0) + 3 + level * 4));
        MclslActorAccessor.Set(actor, MclslActorDataKeys.LastBreakthroughResult, "五老会" + policy + "牵引暗线，得灵石、遗迹阅历与炼心残篇");
    }

    private static List<Actor> PickActors(List<Actor> candidates, int year, int seed, bool preferOrder)
    {
        List<Actor> result = new(AffectedActorLimit);
        if (candidates == null || candidates.Count == 0) return result;
        for (int i = 0; i < candidates.Count; i++)
        {
            Actor actor = candidates[i];
            if (!MclslActorAccessor.Alive(actor)) continue;
            InsertActorCandidate(result, actor, year, seed, preferOrder);
        }
        return result;
    }

    private static int FactionFit(Actor actor, bool preferOrder)
    {
        int realm = Math.Max(0, MclslRealmIds.Index(MclslActorAccessor.Realm(actor)));
        int contribution = Math.Min(80, MclslActorAccessor.GetInt(actor, MclslActorDataKeys.Contribution, 0) / 5);
        int ruin = Math.Min(80, MclslActorAccessor.GetInt(actor, MclslActorDataKeys.RuinExperience, 0));
        int mind = MclslMindSystem.EnsureMindState(actor) / 3;
        return realm * 45 + mind + (preferOrder ? contribution : ruin);
    }

    private static int PressureCandidateScore(Actor actor)
    {
        int realm = Math.Max(0, MclslRealmIds.Index(MclslActorAccessor.Realm(actor)));
        int essence = Math.Min(120, MclslActorAccessor.GetInt(actor, MclslActorDataKeys.TrueEssence, 0) / 80);
        int contribution = Math.Min(60, MclslActorAccessor.GetInt(actor, MclslActorDataKeys.Contribution, 0) / 8);
        int ruin = Math.Min(60, MclslActorAccessor.GetInt(actor, MclslActorDataKeys.RuinExperience, 0));
        return realm * 60 + essence + contribution + ruin;
    }

    private static string IntensifyRuin(MclslWorldRunState run, int seed, int level)
    {
        MclslSectRuinRecord ruin = PickPressureRuin(run, seed);
        if (ruin == null) return string.Empty;
        ruin.Danger = Math.Clamp(ruin.Danger + 5 + level * 5, 20, 98);
        ruin.RemainingValue = Math.Clamp(ruin.RemainingValue + (level >= 2 ? 1 : 0), 1, 12);
        if (ruin.State == "残破") ruin.State = "显世";
        return "遗迹“" + ruin.Name + "”被暗中设伏，已成" + MclslRuinText.DangerBand(ruin.Danger) + "之地";
    }

    private static void Register(MclslWorldRunState run, int year, string factionId, string factionName, string policy, int influence, int pressureDelta, string actors, string worldEffect, string summary)
    {
        MclslFactionPressureRecord record = new()
        {
            Id = "pressure_" + year + "_" + MclslWorldRunRepository.NextProceduralSequence(),
            Year = year,
            FactionId = factionId,
            FactionName = factionName,
            PolicyName = policy,
            InfluenceAtTrigger = Math.Clamp(influence, 0, 100),
            PressureDelta = pressureDelta,
            ActorNames = actors ?? string.Empty,
            WorldEffect = worldEffect ?? string.Empty,
            Summary = summary ?? string.Empty
        };
        MclslWorldRunRepository.RegisterFactionPressure(record);
        MclslWorldRunRepository.AddEvent(year, "faction_pressure", factionName + "势力压力", record.Summary);
        if (MclslRuntimeSettings.FactionPolicyAnnouncementsEnabled) MclslAnnouncementSystem.Enqueue(factionName + "推行“" + policy + "”，" + worldEffect + "。", factionId == WanXian ? "#9CD7FF" : "#B7A7FF", 8f, 1);
    }

    private static void Normalize(MclslWorldRunState run, int year)
    {
        run.BackgroundFactions ??= new MclslBackgroundFactionState();
        run.FactionPressureEvents ??= new List<MclslFactionPressureRecord>();
        if (run.NextFactionPressureYear <= 0) run.NextFactionPressureYear = year + 8;
        UpdatePolicies(run);
    }

    private static void UpdatePolicies(MclslWorldRunState run)
    {
        MclslBackgroundFactionState f = run.BackgroundFactions;
        f.AlliancePolicy = f.WanXianAllianceInfluence switch
        {
            >= 90 => "明令巡天",
            >= 70 => "仙盟清册",
            >= 45 => "监察诸修",
            >= 20 => "远观诸国",
            _ => "势弱收缩"
        };
        f.FiveEldersPolicy = f.FiveEldersInfluence switch
        {
            >= 90 => "潜伏成网",
            >= 70 => "遗迹设伏",
            >= 40 => "暗布棋子",
            >= 18 => "潜伏暗流",
            _ => "蛰伏避锋"
        };
    }

    private static int PressureLevel(int influence) => Math.Clamp(influence, 0, 100) switch
    {
        >= 90 => 3,
        >= 70 => 2,
        >= 45 => 1,
        _ => 0
    };

    private static string ActorNames(List<Actor> actors)
    {
        if (actors == null || actors.Count == 0) return string.Empty;
        List<string> names = new(AffectedActorLimit);
        for (int i = 0; i < actors.Count && names.Count < AffectedActorLimit; i++)
        {
            Actor actor = actors[i];
            if (!MclslActorAccessor.Alive(actor)) continue;
            names.Add(SafeName(actor));
        }
        return string.Join("、", names);
    }

    private static void InsertActorCandidate(List<Actor> result, Actor actor, int year, int seed, bool preferOrder)
    {
        int insertAt = result.Count;
        for (int i = 0; i < result.Count; i++)
        {
            if (CompareActorCandidate(actor, result[i], year, seed, preferOrder) < 0)
            {
                insertAt = i;
                break;
            }
        }
        if (insertAt >= AffectedActorLimit) return;
        result.Insert(insertAt, actor);
        if (result.Count > AffectedActorLimit) result.RemoveAt(result.Count - 1);
    }

    private static int CompareActorCandidate(Actor left, Actor right, int year, int seed, bool preferOrder)
    {
        int fit = FactionFit(right, preferOrder).CompareTo(FactionFit(left, preferOrder));
        if (fit != 0) return fit;
        int leftHash = PositiveHash(MclslActorAccessor.Id(left) + "|pressure_pick|" + year + "|" + seed);
        int rightHash = PositiveHash(MclslActorAccessor.Id(right) + "|pressure_pick|" + year + "|" + seed);
        return leftHash.CompareTo(rightHash);
    }

    private static MclslSectRuinRecord PickPressureRuin(MclslWorldRunState run, int seed)
    {
        if (run?.SectRuins == null) return null;
        MclslSectRuinRecord best = null;
        int bestHash = int.MaxValue;
        for (int i = 0; i < run.SectRuins.Count; i++)
        {
            MclslSectRuinRecord ruin = run.SectRuins[i];
            if (ruin == null || ruin.RemainingValue <= 0 || ruin.State == "搜尽" || ruin.State == "封绝") continue;
            int hash = PositiveHash(ruin.Id + "|elder_pressure|" + seed);
            if (best == null || ruin.Quality > best.Quality || (ruin.Quality == best.Quality && hash < bestHash))
            {
                best = ruin;
                bestHash = hash;
            }
        }
        return best;
    }

    private static string[] PickDisturbanceLaws(int seed)
    {
        string[][] pools =
        {
            new[] { "阴", "隐匿", "腐蚀" },
            new[] { "火", "毁灭", "杀戮" },
            new[] { "水", "转化", "迷离" },
            new[] { "土", "封禁", "稳定" },
            new[] { "雷", "惩戒", "速度" },
            new[] { "金", "秩序", "锋锐" }
        };
        return pools[PositiveHash(seed + "|laws") % pools.Length];
    }

    private static int NextYear(MclslWorldRunState run, int year, int min, int span)
    {
        int baseInterval = min + PositiveHash(run.RunId + "|next_pressure|" + year + "|" + run.FactionPressureEvents.Count) % Math.Max(1, span);
        return year + MclslWorldStateModifierSystem.ScaleFactionPressureInterval(baseInterval, year);
    }

    private static string SafeName(Actor actor)
    {
        try { return MclslActorAccessor.DisplayName(actor); }
        catch { return "无名者"; }
    }

    private static int PositiveHash(string value)
    {
        unchecked { int hash = 79; foreach (char c in value ?? string.Empty) hash = hash * 73 + c; return hash & int.MaxValue; }
    }
}
