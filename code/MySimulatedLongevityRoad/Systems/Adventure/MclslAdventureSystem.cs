using System;
using System.Collections.Generic;
using MySimulatedLongevityRoad.Core;
using MySimulatedLongevityRoad.Data;
using MySimulatedLongevityRoad.Queries;
using MySimulatedLongevityRoad.Systems.Death;

namespace MySimulatedLongevityRoad.Systems;

internal static class MclslAdventureSystem
{
    private sealed class RuinCandidate
    {
        internal Actor Actor;
        internal int Score;
    }

    private sealed class LocationSeed
    {
        internal string Location = "无主荒域";
        internal string Kingdom = "无主";
    }

    private static readonly List<Actor> AnnualCultivators = new();
    private static readonly HashSet<long> AnnualActorIds = new();
    private static readonly Dictionary<string, List<RuinCandidate>> RuinCandidates = new(StringComparer.Ordinal);
    private static int _year = -1;

    private static readonly string[] PrivateRuinCategories =
    {
        "散修遗府", "洞府残藏", "闭关旧府", "坐化洞府",
        "山居遗府", "水府遗藏", "地脉遗府", "云崖旧府",
        "星岩遗府", "荒谷洞府", "霜林遗府", "火脉旧府",
        "石室遗藏", "灵泉洞府", "孤峰遗府", "海隅旧府"
    };

    private static readonly string[] SecretRealmCategories =
    {
        "宗门秘境", "山门秘境", "祖庭秘境", "讲法秘境",
        "试炼秘境", "藏经秘境", "丹阁秘境", "剑台秘境",
        "符阵秘境", "灵圃秘境", "问道秘境", "掌教秘境",
        "护山秘境", "星宫秘境", "法坛秘境", "旧宗秘境"
    };

    internal static void BeginAnnual(int year)
    {
        if (_year == year) return;
        _year = year;
        AnnualCultivators.Clear();
        AnnualActorIds.Clear();
        RuinCandidates.Clear();
        if (!MclslRuntimeSettings.WorldAdventuresEnabled) return;
        EnsureSectRuins(year);
    }

    internal static void RegisterAnnual(Actor actor, int year)
    {
        if (!MclslRuntimeSettings.WorldAdventuresEnabled || _year != year || !MclslActorAccessor.Alive(actor) || !MclslActorAccessor.IsCultivator(actor)) return;
        long id = MclslActorAccessor.Id(actor);
        if (id <= 0L || !AnnualActorIds.Add(id)) return;
        EnsureActorAdventureData(actor);
        AnnualCultivators.Add(actor);

        int realmIndex = Math.Max(0, MclslRealmIds.Index(MclslActorAccessor.Realm(actor)));
        int participationChance = Math.Clamp(7 + realmIndex * 3 + MclslActorAccessor.GetInt(actor, MclslActorDataKeys.RuinExperience, 0) / 15, 5, 32);
        if (PositiveHash(id + "|ruin_participate|" + year) % 100 >= participationChance) return;
        MclslSectRuinRecord ruin = PickRuinForActor(actor, year, realmIndex);
        if (ruin == null) return;
        int aptitude = MclslActorAccessor.GetInt(actor, MclslActorDataKeys.Aptitude, 50);
        int experience = MclslActorAccessor.GetInt(actor, MclslActorDataKeys.RuinExperience, 0);
        int contribution = MclslActorAccessor.GetInt(actor, MclslActorDataKeys.Contribution, 0);
        int score = realmIndex * 45 + aptitude + Math.Min(70, experience) + Math.Min(30, contribution / 4)
            + PositiveHash(id + "|ruin_score|" + ruin.Id + "|" + year) % 51;
        if (!RuinCandidates.TryGetValue(ruin.Id, out List<RuinCandidate> list)) RuinCandidates[ruin.Id] = list = new List<RuinCandidate>();
        list.Add(new RuinCandidate { Actor = actor, Score = score });
    }

    internal static void ResolveAnnual(int year)
    {
        if (!MclslRuntimeSettings.WorldAdventuresEnabled || _year != year) return;
        ResolveRuinExpeditions(year);
        AnnualCultivators.Clear();
        AnnualActorIds.Clear();
        RuinCandidates.Clear();
        MclslWorldArchiveStore.MarkDirty();
    }

    internal static void Clear()
    {
        AnnualCultivators.Clear();
        AnnualActorIds.Clear();
        RuinCandidates.Clear();
        _year = -1;
    }

    internal static MclslSectRuinRecord CreateAncientLawRuin(int year, Actor source, string techniqueName)
    {
        if (source?.data == null) return null;
        MclslWorldRunState run = MclslWorldRunRepository.Current;
        if (run?.SectRuins == null) return null;
        int sequence = MclslWorldRunRepository.NextProceduralSequence();
        bool newLaw = MclslWorldEpochSystem.IsNewLawActive(year);
        string location = string.IsNullOrWhiteSpace(source.city?.data?.name) ? (newLaw ? "旧宗废墟" : "洞府旧址") : source.city.data.name + "附近";
        string kingdom = string.IsNullOrWhiteSpace(source.kingdom?.data?.name) ? "无主" : source.kingdom.data.name;
        string techniqueId = MclslActorAccessor.GetString(source, MclslActorDataKeys.TechniqueId, string.Empty).Replace("spiritual_", string.Empty).Replace("ancient_", string.Empty);
        MclslTechniqueDefinition technique = PromoteLegacyTechnique(MclslCultivationCatalog.Technique(techniqueId), sequence + year, MclslRealmIds.JinDan);
        int quality = Math.Clamp(2 + MclslRealmIds.Index(MclslActorAccessor.Realm(source)) / 2, 2, 4);
        string category = newLaw ? PickSecretRealmCategory(sequence + year) : PickPrivateRuinCategory(sequence + year);
        MclslSectRuinRecord ruin = MclslGeneratedObjectFactory.CreateSectRuin(year, sequence, location, kingdom, technique.LawPool, quality, category);
        string displayTechniqueName = string.IsNullOrWhiteSpace(techniqueName) ? technique.Name : techniqueName;
        AttachTechniqueSource(ruin, technique.Id, displayTechniqueName, string.Empty, year);
        ruin.Description = newLaw
            ? "传法变世后，旧日宗门法脉《" + displayTechniqueName + "》受法不可同修之劫冲击后崩毁，残余灵石、功法断章与门中遗物沉积成秘境。"
            : "修士旧藏《" + displayTechniqueName + "》余脉沉入山河，残余灵石、功法断章与前人遗物汇为遗府。";
        ruin.Danger = Math.Clamp(ruin.Danger + 10, 20, 95);
        if (!MclslWorldRunRepository.TryRegisterSectRuin(ruin)) return null;
        MclslWorldRunRepository.AddEvent(year, "ancient_ruin_born", ruin.Name + (newLaw ? "秘境显世" : "遗府显世"), ruin.Description);
        if (MclslRuntimeSettings.RuinBirthAnnouncementsEnabled) MclslAnnouncementSystem.Enqueue((newLaw ? "秘境“" : "遗府“") + ruin.Name + "”显世。", newLaw ? "#B7A7FF" : "#D8C778", 8f, 1);
        return ruin;
    }

    internal static MclslSectRuinRecord CreateManualAncientLawRuin(int year)
    {
        MclslWorldRunState run = MclslWorldRunRepository.Current;
        if (run?.SectRuins == null) return null;
        int sequence = MclslWorldRunRepository.NextProceduralSequence();
        int hash = PositiveHash((run.RunId ?? string.Empty) + "|manual_ancient_ruin|" + sequence + "|" + year);
        MclslTechniqueDefinition technique = PickTechniqueAtLeast(hash, MclslRealmIds.JinDan);
        int quality = 2 + PositiveHash(hash + "|quality") % 3;
        int tagCount = quality >= 3 ? 3 : 2;
        List<string> tags = new();
        int start = PositiveHash(hash + "|tag") % technique.LawPool.Length;
        for (int i = 0; i < technique.LawPool.Length && tags.Count < tagCount; i++)
        {
            string tag = technique.LawPool[(start + i) % technique.LawPool.Length];
            if (!tags.Contains(tag)) tags.Add(tag);
        }
        LocationSeed location = PickLocation(hash);
        MclslSectRuinRecord ruin = MclslGeneratedObjectFactory.CreateSectRuin(year, sequence, location.Location, location.Kingdom, tags, quality, PickPrivateRuinCategory(hash));
        AttachTechniqueSource(ruin, technique.Id, technique.Name, string.Empty, 0);
        ruin.Description = "旧日洞府显于山河之间，残卷、灵石与遗物尚有余韵。";
        if (!MclslWorldRunRepository.TryRegisterSectRuin(ruin)) return null;
        MclslWorldRunRepository.AddEvent(year, "ancient_high_cultivator_seclusion", ruin.Name + "现世", ruin.Description + MclslRuinText.DangerEventText(ruin.Danger) + "，尚存" + ruin.RemainingValue + "份主要机缘。");
        if (MclslRuntimeSettings.RuinBirthAnnouncementsEnabled) MclslAnnouncementSystem.Enqueue("遗府“" + ruin.Name + "”现世。", "#D8C778", 8f, 1);
        return ruin;
    }

    internal static MclslSectRuinRecord CreateAncientSecretRealmRuin(int year, Actor source, string techniqueName)
    {
        if (source?.data == null) return null;
        MclslWorldRunState run = MclslWorldRunRepository.Current;
        if (run?.SectRuins == null) return null;
        int sequence = MclslWorldRunRepository.NextProceduralSequence();
        int hash = PositiveHash(MclslActorAccessor.Id(source) + "|ancient_secret_lineage|" + sequence + "|" + year);
        MclslTechniqueDefinition technique = PromoteLegacyTechnique(MclslCultivationCatalog.Technique(MclslActorAccessor.GetString(source, MclslActorDataKeys.TechniqueId, string.Empty)), hash, MclslRealmIds.JinDan);
        int quality = Math.Clamp(2 + MclslRealmIds.Index(MclslActorAccessor.Realm(source)) / 2, 2, 4);
        string location = string.IsNullOrWhiteSpace(source.city?.data?.name) ? "山河秘境" : source.city.data.name + "附近";
        string kingdom = string.IsNullOrWhiteSpace(source.kingdom?.data?.name) ? "无主" : source.kingdom.data.name;
        MclslSectRuinRecord ruin = MclslGeneratedObjectFactory.CreateSectRuin(year, sequence, location, kingdom, technique.LawPool, quality, PickSecretRealmCategory(hash));
        string displayTechniqueName = string.IsNullOrWhiteSpace(techniqueName) ? technique.Name : techniqueName;
        AttachTechniqueSource(ruin, technique.Id, displayTechniqueName, string.Empty, year);
        ruin.Description = "旧日宗门秘境重开，门中《" + displayTechniqueName + "》残章、讲法石刻与试炼余痕尚存。";
        ruin.Danger = Math.Clamp(ruin.Danger + quality * 6, 25, 95);
        if (!MclslWorldRunRepository.TryRegisterSectRuin(ruin)) return null;
        MclslWorldRunRepository.AddEvent(year, "ancient_secret_realm_ruin", ruin.Name + "显世", ruin.Description);
        return ruin;
    }

    private static void ResolveRuinExpeditions(int year)
    {
        foreach (KeyValuePair<string, List<RuinCandidate>> pair in RuinCandidates)
        {
            MclslSectRuinRecord ruin = MclslWorldRunRepository.FindSectRuin(pair.Key);
            if (!IsRuinAvailable(ruin)) continue;
            int capacity = Math.Clamp(1 + ruin.Quality, 2, 5);
            List<RuinCandidate> expedition = PickExpedition(pair.Value, capacity);
            if (expedition.Count == 0) continue;
            ruin.State = "探索中";
            ruin.LastExploredYear = year;
            ruin.ExpeditionCount++;
            ruin.LastExplorerNames = JoinExplorerNames(expedition);
            for (int i = 0; i < expedition.Count; i++) ResolveSingleExplorer(expedition[i].Actor, ruin, year, i);
            if (ruin.RemainingValue <= 0 || ruin.ExplorationProgress >= ruin.Depth * 100 || ruin.ExpeditionCount >= RuinMaxExpeditions(ruin))
            {
                ruin.RemainingValue = 0;
                ruin.State = "搜尽";
                MclslWorldRunRepository.AddEvent(year, "ruin_exhausted", ruin.Name + "探索告终", "历经" + ruin.ExpeditionCount + "次探索、折损" + ruin.CasualtyCount + "名修士后，此地有价值的传承已被搜尽。");
                int collapseRoll = PositiveHash(ruin.Id + "|collapse|" + year) % 100;
                if (collapseRoll < 25 + ruin.Quality * 8) MclslWorldChangeSystem.CreateFromRuinCollapse(year, ruin);
            }
            else if (ruin.Danger >= 85 && ruin.CasualtyCount >= 3) ruin.State = "封绝";
            else ruin.State = "残破";
        }
    }

    private static void ResolveSingleExplorer(Actor actor, MclslSectRuinRecord ruin, int year, int order)
    {
        if (!MclslActorAccessor.Alive(actor)) return;
        int realmIndex = Math.Max(0, MclslRealmIds.Index(MclslActorAccessor.Realm(actor)));
        int aptitude = MclslActorAccessor.GetInt(actor, MclslActorDataKeys.Aptitude, 50);
        int experience = MclslActorAccessor.GetInt(actor, MclslActorDataKeys.RuinExperience, 0);
        int lawCompatibility = RuinLawCompatibility(actor, ruin);
        int survival = Math.Clamp(72 + realmIndex * 9 + aptitude / 6 + experience / 8 + lawCompatibility / 8 - ruin.Danger, 8, 98);
        int roll = PositiveHash(MclslActorAccessor.Id(actor) + "|ruin_survival|" + ruin.Id + "|" + year + "|" + order) % 100;
        int progressGain = Math.Clamp(12 + realmIndex * 8 + aptitude / 8 + PositiveHash(actor.getName() + "|ruin_progress|" + year) % 25, 10, 70);
        if (roll >= survival)
        {
            ruin.CasualtyCount++;
            ruin.ExplorationProgress += Math.Max(3, progressGain / 4);
            string danger = BuildRuinDeathDetail(ruin, actor, year);
            MclslRuinExplorationRecord deathRecord = BuildExplorationRecord(actor, ruin, year, false, "death", "无", Math.Max(3, progressGain / 4), danger + "。");
            MclslWorldRunRepository.RegisterRuinExploration(deathRecord);
            MclslWorldRunRepository.AddEvent(year, "ruin_death", MclslActorAccessor.DisplayName(actor) + "殒于" + ruin.Name, danger + "。", actor);
            MclslDeathSystem.ExecuteScriptedDeath(actor, "ruin_exploration", ruin.Name, danger, true);
            return;
        }

        ruin.ExplorationProgress += progressGain;
        int valueLossChance = MclslInverseTruthSystem.IsTruthReversed("truth_player_trace_persistence") ? 24 : 45;
        ruin.RemainingValue = Math.Max(0, ruin.RemainingValue - (PositiveHash(MclslActorAccessor.Id(actor) + "|ruin_value|" + year) % 100 < valueLossChance ? 1 : 0));
        MclslActorAccessor.Set(actor, MclslActorDataKeys.RuinExperience, experience + 1 + progressGain / 15);
        string reward = GrantRuinReward(actor, ruin, year, realmIndex, out string revivedTechniqueId, out string revivedTechniqueName, out string linkedLineageId);
        bool ancientPath = MclslActorAccessor.GetString(actor, MclslActorDataKeys.CultivationSystem, string.Empty)
            == MclslCultivationSystemIds.AncientLaw;
        string relation = ancientPath
            ? "旧法气机与自身传承相互印证"
            : MclslLawInteractionCatalog.Detail(ActorLawTags(actor), MclslGeneratedObjectFactory.SplitTags(ruin.LawTags));
        string summary = MclslActorAccessor.DisplayName(actor) + "从“" + ruin.Name + "”中生还，推进探索" + progressGain + "点，" + relation + "，所得：" + reward + "。";
        MclslWorldRunRepository.RegisterRuinExploration(BuildExplorationRecord(actor, ruin, year, true, "survived", reward, progressGain, summary, revivedTechniqueId, revivedTechniqueName, linkedLineageId));
        MclslWorldRunRepository.AddEvent(year, "ruin_explore", MclslActorAccessor.DisplayName(actor) + "探得" + ruin.Name, "生还。所得：" + reward + "。", actor);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.LastBreakthroughResult, summary);
    }

    private static string GrantRuinReward(Actor actor, MclslSectRuinRecord ruin, int year, int realmIndex, out string revivedTechniqueId, out string revivedTechniqueName, out string linkedLineageId)
    {
        revivedTechniqueId = string.Empty;
        revivedTechniqueName = string.Empty;
        linkedLineageId = string.Empty;
        int rewardRoll = PositiveHash(MclslActorAccessor.Id(actor) + "|ruin_reward|" + ruin.Id + "|" + year) % 100;
        int quality = Math.Clamp(ruin.Quality, 1, 4);
        bool newLawReward = MclslWorldEpochSystem.IsNewLawActive(year)
            && MclslActorAccessor.GetString(actor, MclslActorDataKeys.CultivationSystem, string.Empty) != MclslCultivationSystemIds.AncientLaw;
        if (rewardRoll < 20)
        {
            int amount = 8 + quality * 5 + PositiveHash(actor.getName() + "|contribution|" + year) % 16;
            if (!newLawReward)
            {
                GrantAncientPracticeNote(actor, amount);
                return "遗府手札，功法参悟+" + amount;
            }
            MclslResourceSystem.GrantRuinContribution(actor, amount);
            return "遗物折算贡献" + amount;
        }
        if (rewardRoll < 36)
        {
            int amount = 10 + quality * 9 + PositiveHash(actor.getName() + "|spirit_stones|" + year) % 24;
            MclslResourceSystem.GrantRuinSpiritStones(actor, amount);
            return "搜得灵石" + amount;
        }
        if (rewardRoll < 52)
        {
            int insight = 6 + quality * 4;
            MclslActorAccessor.Set(actor, MclslActorDataKeys.TechniqueInsight, MclslActorAccessor.GetInt(actor, MclslActorDataKeys.TechniqueInsight, 0) + insight);
            MclslTechniqueDefinition technique = DiscoverRuinTechnique(actor, ruin, year, quality);
            if (technique != null)
            {
                MclslActorAccessor.Set(actor, MclslActorDataKeys.TechniqueId, technique.Id);
                MclslActorAccessor.Set(actor, MclslActorDataKeys.TechniqueName, technique.Name);
                MclslTechniqueRealmLimit.EnsureFromDefinition(actor, technique);
                int completeness = Math.Clamp(80 + quality * 3, 83, 92);
                MclslTechniqueStageSystem.SetProgress(
                    actor,
                    Math.Max(MclslTechniqueStageSystem.Progress(actor), completeness));
                MclslTechniqueOccupationSystem.OnCultivationStateChanged(actor);
                revivedTechniqueId = technique.Id;
                revivedTechniqueName = technique.Name;
                linkedLineageId = MclslTechniqueLineageSystem.RecordRuinTechniqueRevival(year, actor, ruin, technique);
                MclslWorldRunRepository.AddEvent(year, "ruin_technique_found", "《" + technique.Name + "》自" + ruin.Name + "复现", MclslActorAccessor.DisplayName(actor) + "于遗迹中得完整传承，功法完整度" + completeness + "% ，可直接修至" + MclslRealmIds.Display(technique.MaxRealm) + "。", actor);
                return "得《" + technique.Name + "》传承，完整度" + completeness + "% ，可直接修至" + MclslRealmIds.Display(technique.MaxRealm);
            }
            return "功法残篇，功法感悟+" + insight;
        }
        if (rewardRoll < 66)
        {
            int bonus = 8 + quality * 4;
            if (newLawReward && realmIndex == 0)
            {
                MclslActorAccessor.Set(actor, MclslActorDataKeys.FoundationChanceBonus,
                    Math.Min(40, MclslActorAccessor.GetInt(actor, MclslActorDataKeys.FoundationChanceBonus, 0) + bonus));
                return "筑基奇物线索，奇遇概率+" + bonus + "%";
            }
            GrantAncientPracticeNote(actor, bonus);
            return "前人修行札记，功法参悟+" + bonus;
        }
        if (rewardRoll < 80)
        {
            int bonus = 8 + quality * 5;
            if (newLawReward && realmIndex >= 2)
            {
                MclslActorAccessor.Set(actor, MclslActorDataKeys.CaveClaimBonus,
                    Math.Min(60, MclslActorAccessor.GetInt(actor, MclslActorDataKeys.CaveClaimBonus, 0) + bonus));
                return "洞天残图，下一次洞天争夺强度+" + bonus;
            }
            GrantAncientPracticeNote(actor, bonus);
            return "旧法讲义残页，功法参悟+" + bonus;
        }
        if (rewardRoll < 88 && ruin.RemainingValue > 0)
        {
            if (newLawReward)
            {
                MclslWorldCaveRecord cave = MclslWorldCaveSystem.GenerateCaveFromDiscovery(year, actor, MclslGeneratedObjectFactory.SplitTags(ruin.LawTags), ruin.Name);
                return cave == null ? "古老洞天线索" : "发现洞天“" + cave.Name + "”";
            }
            int bonus = 10 + quality * 5;
            GrantAncientPracticeNote(actor, bonus);
            return "前辈闭关石刻，功法参悟+" + bonus;
        }
        if (rewardRoll < 94)
        {
            int heart = 10 + quality * 5;
            MclslActorAccessor.Set(actor, MclslActorDataKeys.HeartMethodKnown, 1);
            MclslActorAccessor.Set(actor, MclslActorDataKeys.HeartTemperingProgress, Math.Min(100, MclslActorAccessor.GetInt(actor, MclslActorDataKeys.HeartTemperingProgress, 0) + heart));
            MclslActorAccessor.Set(actor, MclslActorDataKeys.MindState, Math.Min(100, MclslMindSystem.EnsureMindState(actor) + quality));
            return "《" + MclslMindSystem.MethodText(actor) + "》残篇，炼心进度+" + heart + "%";
        }
        int rewardInsight = 6 + quality * 4;
        MclslActorAccessor.Set(actor, MclslActorDataKeys.TechniqueInsight,
            MclslActorAccessor.GetInt(actor, MclslActorDataKeys.TechniqueInsight, 0) + rewardInsight);
        return "修行感悟，悟法能力+" + rewardInsight;
    }

    private static void GrantAncientPracticeNote(Actor actor, int amount)
    {
        int bonus = Math.Max(0, amount);
        MclslTechniqueStageSystem.AddProgress(actor, bonus);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.TechniqueInsight,
            MclslActorAccessor.GetInt(actor, MclslActorDataKeys.TechniqueInsight, 0) + Math.Max(1, bonus / 2));
    }

    private static MclslRuinExplorationRecord BuildExplorationRecord(Actor actor, MclslSectRuinRecord ruin, int year, bool survived, string outcome, string reward, int progress, string summary, string revivedTechniqueId = "", string revivedTechniqueName = "", string linkedLineageId = "")
    {
        return new MclslRuinExplorationRecord
        {
            Id = "explore_" + year + "_" + MclslWorldRunRepository.NextProceduralSequence(),
            Year = year,
            RuinId = ruin.Id,
            RuinName = ruin.Name,
            ActorId = MclslActorAccessor.Id(actor),
            ActorName = MclslActorAccessor.DisplayName(actor),
            RealmName = MclslRealmIds.Display(MclslActorAccessor.Realm(actor)),
            Survived = survived,
            OutcomeCode = outcome,
            RewardText = reward,
            ProgressGained = progress,
            Summary = summary,
            RevivedTechniqueId = revivedTechniqueId ?? string.Empty,
            RevivedTechniqueName = revivedTechniqueName ?? string.Empty,
            LinkedLineageId = linkedLineageId ?? string.Empty
        };
    }

    private static void AttachTechniqueSource(MclslSectRuinRecord ruin, string techniqueId, string techniqueName, string lineageId, int lostYear)
    {
        if (ruin == null) return;
        if (!string.IsNullOrWhiteSpace(techniqueId))
            ruin.SourceTechniqueId = MclslCultivationCatalog.NormalizeTechniqueId(techniqueId);
        if (!string.IsNullOrWhiteSpace(techniqueName))
            ruin.SourceTechniqueName = techniqueName;
        if (!string.IsNullOrWhiteSpace(lineageId))
            ruin.LinkedLineageId = lineageId;
        if (lostYear > 0)
            ruin.SourceTechniqueLostYear = lostYear;
    }

    private static string BuildRuinDeathDetail(MclslSectRuinRecord ruin, Actor actor, int year)
    {
        string[] dangers =
        {
            "触动残存杀阵，形神被禁制绞碎",
            "误入错乱空间，再未寻得归路",
            "遭古宗护法残念袭杀，元神寂灭",
            "夺取遗物时引发遗迹坍塌，被埋入地脉",
            "被遗迹中失控的法则同化，肉身与神魂俱散",
            "踏入伪装成传承地的死关，未能破阵而出",
            "与同入遗迹的修士争夺机缘，重伤后困死其中"
        };
        int index = PositiveHash(ruin.Id + "|death_detail|" + MclslActorAccessor.Id(actor) + "|" + year) % dangers.Length;
        int layer = Math.Clamp(1 + ruin.ExplorationProgress / 100, 1, ruin.Depth);
        return "深入宗门遗迹“" + ruin.Name + "”第" + layer + "重后，" + dangers[index];
    }

    private static void EnsureSectRuins(int year)
    {
        MclslWorldRunState run = MclslWorldRunRepository.Current;
        int available = CountAvailableRuins(run);
        int population = MclslWorldActorQuery.UnitCount();
        int desired = Math.Clamp(2 + population / 4000, 2, 5);
        if (run.SectRuins.Count == 0)
        {
            while (run.SectRuins.Count < desired && GenerateRuin(year, false)) { }
            available = CountAvailableRuins(run);
        }
        if (run.NextRuinBirthYear <= 0) run.NextRuinBirthYear = year + NextRuinInterval(run, year);
        if (year >= run.NextRuinBirthYear && available < MclslWorldRunRepository.SectRuinRecoveryFloor)
        {
            GenerateRuin(year, true);
            run.NextRuinBirthYear = year + NextRuinInterval(run, year);
        }
    }

    private static int NextRuinInterval(MclslWorldRunState run, int year)
    {
        int baseInterval = 90 + PositiveHash((run?.RunId ?? string.Empty) + "|next_ruin|" + year + "|" + (run?.ProceduralSequence ?? 0)) % 91;
        return MclslWorldStateModifierSystem.ScaleRuinInterval(baseInterval, year);
    }

    private static bool GenerateRuin(int year, bool announce)
    {
        MclslWorldRunState run = MclslWorldRunRepository.Current;
        int sequence = MclslWorldRunRepository.NextProceduralSequence();
        int hash = PositiveHash(run.RunId + "|ruin_profile|" + sequence + "|" + year);
        MclslTechniqueDefinition technique = PickTechniqueAtLeast(hash, MclslRealmIds.JinDan);
        int qualityRoll = PositiveHash(hash + "|quality") % 100;
        int quality = qualityRoll < 7 ? 4 : qualityRoll < 27 ? 3 : qualityRoll < 68 ? 2 : 1;
        int tagCount = quality >= 3 ? 3 : 2;
        List<string> tags = new();
        int start = PositiveHash(hash + "|tag") % technique.LawPool.Length;
        for (int i = 0; i < technique.LawPool.Length && tags.Count < tagCount; i++)
            if (!tags.Contains(technique.LawPool[(start + i) % technique.LawPool.Length])) tags.Add(technique.LawPool[(start + i) % technique.LawPool.Length]);
        string category = PositiveHash(hash + "|category_kind") % 100 < 55
            ? PickPrivateRuinCategory(hash)
            : PickSecretRealmCategory(hash);
        LocationSeed location = PickLocation(hash);
        MclslSectRuinRecord ruin = MclslGeneratedObjectFactory.CreateSectRuin(year, sequence, location.Location, location.Kingdom, tags, quality, category);
        AttachTechniqueSource(ruin, technique.Id, technique.Name, string.Empty, 0);
        if (!MclslWorldRunRepository.TryRegisterSectRuin(ruin)) return false;
        MclslWorldRunRepository.AddEvent(year, "ruin_born", ruin.Name + "显世", MclslRuinText.DangerEventText(ruin.Danger) + "，尚存" + ruin.RemainingValue + "份主要机缘。");
        if (announce && MclslRuntimeSettings.RuinBirthAnnouncementsEnabled)
        {
            MclslAnnouncementSystem.Enqueue("宗门遗迹“" + ruin.Name + "”于" + ruin.LocationName + "显世。", "#8AB58C", 8f, 1);
        }
        return true;
    }

    private static MclslTechniqueDefinition DiscoverRuinTechnique(Actor actor, MclslSectRuinRecord ruin, int year, int quality)
    {
        string currentId = MclslCultivationCatalog.NormalizeTechniqueId(MclslActorAccessor.GetString(actor, MclslActorDataKeys.TechniqueId, string.Empty));
        int currentLimit = MclslRealmIds.Index(MclslActorAccessor.GetString(actor, MclslActorDataKeys.TechniqueMaxRealm, string.Empty));
        string[] ruinTags = MclslGeneratedObjectFactory.SplitTags(ruin.LawTags);
        MclslTechniqueDefinition source = PromoteLegacyTechnique(FindCatalogTechnique(ruin.SourceTechniqueId), PositiveHash(ruin.Id + "|source"), MclslRealmIds.JinDan);
        if (source != null && !string.Equals(source.Id, currentId, StringComparison.Ordinal))
            return source;
        int poolCount = CountRuinTechniquePool(currentId, currentLimit, quality, ruinTags, true);
        bool requireLawMatch = poolCount > 0;
        if (poolCount == 0) poolCount = CountRuinTechniquePool(currentId, currentLimit, quality, ruinTags, false);
        if (poolCount == 0) return null;
        int chance = Math.Clamp(18 + quality * 8, 18, 55);
        if (PositiveHash(MclslActorAccessor.Id(actor) + "|ruin_technique|" + ruin.Id + "|" + year) % 100 >= chance) return null;
        int pickIndex = PositiveHash(ruin.Id + "|technique_pick|" + MclslActorAccessor.Id(actor) + "|" + year) % poolCount;
        return PickRuinTechniqueFromPool(currentId, currentLimit, quality, ruinTags, requireLawMatch, pickIndex);
    }

    private static MclslTechniqueDefinition FindCatalogTechnique(string id)
    {
        string normalized = MclslCultivationCatalog.NormalizeTechniqueId(id);
        if (string.IsNullOrWhiteSpace(normalized)) return null;
        for (int i = 0; i < MclslCultivationCatalog.Techniques.Count; i++)
        {
            MclslTechniqueDefinition technique = MclslCultivationCatalog.Techniques[i];
            if (technique != null && string.Equals(technique.Id, normalized, StringComparison.Ordinal)) return technique;
        }
        return null;
    }

    private static int CountRuinTechniquePool(string currentId, int currentLimit, int quality, IReadOnlyList<string> ruinTags, bool requireLawMatch)
    {
        int count = 0;
        for (int i = 0; i < MclslCultivationCatalog.Techniques.Count; i++)
            if (IsRuinTechniqueCandidate(MclslCultivationCatalog.Techniques[i], currentId, currentLimit, quality, ruinTags, requireLawMatch)) count++;
        return count;
    }

    private static MclslTechniqueDefinition PickRuinTechniqueFromPool(string currentId, int currentLimit, int quality, IReadOnlyList<string> ruinTags, bool requireLawMatch, int pickIndex)
    {
        int seen = 0;
        for (int i = 0; i < MclslCultivationCatalog.Techniques.Count; i++)
        {
            MclslTechniqueDefinition technique = MclslCultivationCatalog.Techniques[i];
            if (!IsRuinTechniqueCandidate(technique, currentId, currentLimit, quality, ruinTags, requireLawMatch)) continue;
            if (seen == pickIndex) return technique;
            seen++;
        }
        return null;
    }

    private static bool IsRuinTechniqueCandidate(MclslTechniqueDefinition technique, string currentId, int currentLimit, int quality, IReadOnlyList<string> ruinTags, bool requireLawMatch)
    {
        if (technique == null || technique.Id == currentId) return false;
        int maxIndex = MclslRealmIds.Index(technique.MaxRealm);
        if (maxIndex < MclslRealmIds.Index(MclslRealmIds.JinDan)) return false;
        if (maxIndex < Math.Max(0, currentLimit)) return false;
        if (requireLawMatch)
        {
            if (quality < 3 && maxIndex > MclslRealmIds.Index(MclslRealmIds.JinDan)) return false;
            if (!HasAnyLawTag(technique.LawPool, ruinTags)) return false;
        }
        return true;
    }

    private static string PickPrivateRuinCategory(int seed)
    {
        return PrivateRuinCategories[PositiveHash(seed + "|private_ruin_category") % PrivateRuinCategories.Length];
    }

    private static string PickSecretRealmCategory(int seed)
    {
        return SecretRealmCategories[PositiveHash(seed + "|secret_realm_category") % SecretRealmCategories.Length];
    }

    private static MclslTechniqueDefinition PickTechniqueAtLeast(int seed, string minRealm)
    {
        int minIndex = MclslRealmIds.Index(minRealm);
        int count = 0;
        for (int i = 0; i < MclslCultivationCatalog.Techniques.Count; i++)
        {
            MclslTechniqueDefinition technique = MclslCultivationCatalog.Techniques[i];
            if (technique != null && MclslRealmIds.Index(technique.MaxRealm) >= minIndex) count++;
        }
        if (count <= 0) return MclslCultivationCatalog.Techniques[PositiveHash(seed + "|any_technique") % MclslCultivationCatalog.Techniques.Count];
        int pick = PositiveHash(seed + "|min_realm_technique") % count;
        int seen = 0;
        for (int i = 0; i < MclslCultivationCatalog.Techniques.Count; i++)
        {
            MclslTechniqueDefinition technique = MclslCultivationCatalog.Techniques[i];
            if (technique == null || MclslRealmIds.Index(technique.MaxRealm) < minIndex) continue;
            if (seen == pick) return technique;
            seen++;
        }
        return MclslCultivationCatalog.Techniques[0];
    }

    private static MclslTechniqueDefinition PromoteLegacyTechnique(MclslTechniqueDefinition technique, int seed, string minRealm)
    {
        if (technique != null && MclslRealmIds.Index(technique.MaxRealm) >= MclslRealmIds.Index(minRealm)) return technique;
        if (technique?.LawPool != null)
        {
            MclslTechniqueDefinition matched = PickTechniqueWithAnyLawAtLeast(technique.LawPool, seed, minRealm);
            if (matched != null) return matched;
        }
        return PickTechniqueAtLeast(seed, minRealm);
    }

    private static MclslTechniqueDefinition PickTechniqueWithAnyLawAtLeast(IReadOnlyList<string> lawPool, int seed, string minRealm)
    {
        int minIndex = MclslRealmIds.Index(minRealm);
        int count = 0;
        for (int i = 0; i < MclslCultivationCatalog.Techniques.Count; i++)
        {
            MclslTechniqueDefinition technique = MclslCultivationCatalog.Techniques[i];
            if (technique != null && MclslRealmIds.Index(technique.MaxRealm) >= minIndex && HasAnyLawTag(technique.LawPool, lawPool)) count++;
        }
        if (count <= 0) return null;
        int pick = PositiveHash(seed + "|law_match_technique") % count;
        int seen = 0;
        for (int i = 0; i < MclslCultivationCatalog.Techniques.Count; i++)
        {
            MclslTechniqueDefinition technique = MclslCultivationCatalog.Techniques[i];
            if (technique == null || MclslRealmIds.Index(technique.MaxRealm) < minIndex || !HasAnyLawTag(technique.LawPool, lawPool)) continue;
            if (seen == pick) return technique;
            seen++;
        }
        return null;
    }

    private static bool HasAnyLawTag(IReadOnlyList<string> left, IReadOnlyList<string> right)
    {
        if (left == null || right == null) return false;
        for (int i = 0; i < left.Count; i++)
        {
            string tag = left[i];
            if (string.IsNullOrWhiteSpace(tag)) continue;
            for (int j = 0; j < right.Count; j++)
                if (string.Equals(tag, right[j], StringComparison.Ordinal)) return true;
        }
        return false;
    }

    private static MclslSectRuinRecord PickRuinForActor(Actor actor, int year, int realmIndex)
    {
        MclslSectRuinRecord best = null;
        int bestScore = int.MinValue;
        foreach (MclslSectRuinRecord ruin in MclslWorldRunRepository.Current.SectRuins)
        {
            if (!IsRuinAvailable(ruin)) continue;
            int tolerance = 35 + realmIndex * 14 + MclslActorAccessor.GetInt(actor, MclslActorDataKeys.RuinExperience, 0) / 3;
            if (ruin.Danger > tolerance + 30) continue;
            int compatibility = RuinLawCompatibility(actor, ruin);
            int score = compatibility + ruin.RemainingValue * 8 + ruin.Quality * 12 - Math.Max(0, ruin.Danger - tolerance) * 2
                + PositiveHash(MclslActorAccessor.Id(actor) + "|pick_ruin|" + ruin.Id + "|" + year) % 31;
            if (score <= bestScore) continue;
            bestScore = score;
            best = ruin;
        }
        return best;
    }

    private static int RuinLawCompatibility(Actor actor, MclslSectRuinRecord ruin)
    {
        return MclslLawInteractionCatalog.CompatibilityScore(ActorLawTags(actor), MclslGeneratedObjectFactory.SplitTags(ruin.LawTags), 12);
    }

    private static List<RuinCandidate> PickExpedition(List<RuinCandidate> candidates, int capacity)
    {
        List<RuinCandidate> result = new(Math.Clamp(capacity, 0, 5));
        if (candidates == null || capacity <= 0) return result;
        for (int i = 0; i < candidates.Count; i++)
        {
            RuinCandidate candidate = candidates[i];
            if (!IsValidRuinCandidate(candidate)) continue;
            InsertExpeditionCandidate(result, candidate, capacity);
        }
        return result;
    }

    private static void InsertExpeditionCandidate(List<RuinCandidate> result, RuinCandidate candidate, int capacity)
    {
        int insertAt = result.Count;
        for (int i = 0; i < result.Count; i++)
        {
            if (CompareRuinCandidate(candidate, result[i]) < 0)
            {
                insertAt = i;
                break;
            }
        }
        if (insertAt >= capacity) return;
        result.Insert(insertAt, candidate);
        if (result.Count > capacity) result.RemoveAt(result.Count - 1);
    }

    private static int CompareRuinCandidate(RuinCandidate left, RuinCandidate right)
    {
        if (left == right) return 0;
        if (left == null) return 1;
        if (right == null) return -1;
        int score = right.Score.CompareTo(left.Score);
        if (score != 0) return score;
        return MclslActorAccessor.Id(left.Actor).CompareTo(MclslActorAccessor.Id(right.Actor));
    }

    private static bool IsValidRuinCandidate(RuinCandidate candidate)
    {
        return candidate != null && MclslActorAccessor.Alive(candidate.Actor);
    }

    private static string JoinExplorerNames(List<RuinCandidate> expedition)
    {
        if (expedition == null || expedition.Count == 0) return string.Empty;
        List<string> names = new(expedition.Count);
        for (int i = 0; i < expedition.Count; i++)
            if (IsValidRuinCandidate(expedition[i])) names.Add(MclslActorAccessor.DisplayName(expedition[i].Actor));
        return string.Join("、", names);
    }

    private static int CountAvailableRuins(MclslWorldRunState run)
    {
        if (run?.SectRuins == null) return 0;
        int count = 0;
        for (int i = 0; i < run.SectRuins.Count; i++)
            if (IsRuinAvailable(run.SectRuins[i])) count++;
        return count;
    }

    private static string[] ActorLawTags(Actor actor)
    {
        string laws = MclslActorAccessor.GetString(actor, MclslActorDataKeys.DivineMarrowTags,
            MclslActorAccessor.GetString(actor, MclslActorDataKeys.NascentEssenceTags,
            MclslActorAccessor.GetString(actor, MclslActorDataKeys.GoldenCoreLaws,
            MclslActorAccessor.GetString(actor, MclslActorDataKeys.FoundationWonderTags, string.Empty))));
        return MclslGeneratedObjectFactory.SplitTags(laws);
    }

    private static void EnsureActorAdventureData(Actor actor)
    {
        if (MclslActorAccessor.GetInt(actor, MclslActorDataKeys.Contribution, -1) < 0)
            MclslActorAccessor.Set(actor, MclslActorDataKeys.Contribution, 20);
    }

    private static LocationSeed PickLocation(int hash)
    {
        List<LocationSeed> locations = new();
        IReadOnlyList<Actor> units = MclslCultivatorCandidateIndex.GetKnownActorsSnapshot();
        if (units != null)
        {
            HashSet<string> seen = new(StringComparer.Ordinal);
            for (int i = 0; i < units.Count && locations.Count < 64; i++)
            {
                Actor actor = units[i];
                if (actor?.city == null) continue;
                string city = actor.city?.data?.name;
                if (string.IsNullOrWhiteSpace(city) || !seen.Add(city)) continue;
                string kingdom = string.IsNullOrWhiteSpace(actor.kingdom?.data?.name) ? "无主" : actor.kingdom.data.name;
                locations.Add(new LocationSeed { Location = city + "附近", Kingdom = kingdom });
            }
        }
        if (locations.Count > 0) return locations[(hash & int.MaxValue) % locations.Count];
        return new LocationSeed { Location = MclslProceduralLexicon.CaveOrigins[(hash & int.MaxValue) % MclslProceduralLexicon.CaveOrigins.Length], Kingdom = "无主" };
    }

    private static bool IsRuinAvailable(MclslSectRuinRecord ruin) =>
        ruin != null
        && ruin.RemainingValue > 0
        && ruin.ExpeditionCount < RuinMaxExpeditions(ruin)
        && ruin.State != "搜尽"
        && ruin.State != "封绝";

    internal static int RuinMaxExpeditions(MclslSectRuinRecord ruin)
    {
        if (ruin == null || string.IsNullOrWhiteSpace(ruin.Id)) return 5;
        return 5 + PositiveHash(ruin.Id + "|max_expeditions") % 6;
    }

    private static int PositiveHash(string value)
    {
        unchecked { int result = 43; foreach (char c in value ?? string.Empty) result = result * 53 + c; return result & int.MaxValue; }
    }
}
