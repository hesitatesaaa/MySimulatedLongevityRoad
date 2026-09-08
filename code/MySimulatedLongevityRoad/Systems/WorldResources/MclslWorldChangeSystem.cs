using System;
using System.Collections.Generic;
using MySimulatedLongevityRoad.Core;
using MySimulatedLongevityRoad.Data;
using MySimulatedLongevityRoad.Queries;
using MySimulatedLongevityRoad.Systems.Death;

namespace MySimulatedLongevityRoad.Systems;

internal static class MclslWorldChangeSystem
{
    private sealed class ChangeClaim
    {
        internal Actor Actor;
        internal string ChangeId = string.Empty;
        internal int Compatibility;
        internal int Strength;
    }

    private sealed class LocationSeed
    {
        internal string Location = "天地之间";
        internal string Kingdom = "无主";
        internal int X = -1;
        internal int Y = -1;
    }

    private static readonly Dictionary<string, List<ChangeClaim>> Claims = new(StringComparer.Ordinal);
    private static readonly Dictionary<string, int> NativeDeathSignals = new(StringComparer.Ordinal);
    private static readonly HashSet<string> EmittedNativeChanges = new(StringComparer.Ordinal);
    private static int _claimYear = -1;
    private static int _nativeSignalYear = -1;


    internal static void ObserveNativeDeath(Actor actor, AttackType attackType)
    {
        if (!MclslRuntimeSettings.CoreEnabled || actor?.data == null || string.IsNullOrWhiteSpace(MclslWorldRunRepository.Current?.RunId)) return;
        if (!MclslWorldEpochSystem.IsNewLawActive(MclslRuntime.CurrentYear())) return;
        if (!string.IsNullOrWhiteSpace(MclslActorAccessor.GetString(actor, MclslActorDataKeys.PendingDeathCode, string.Empty))) return;
        int year = MclslRuntime.CurrentYear();
        if (_nativeSignalYear != year)
        {
            _nativeSignalYear = year;
            NativeDeathSignals.Clear();
            EmittedNativeChanges.Clear();
        }
        string cause = NativeCause(actor, attackType.ToString());
        if (string.IsNullOrWhiteSpace(cause)) return;
        int count = NativeDeathSignals.TryGetValue(cause, out int current) ? current + 1 : 1;
        NativeDeathSignals[cause] = count;
        int threshold = cause switch
        {
            "fire" => 8,
            "frost" => 8,
            "thunder" => 6,
            "storm" => 10,
            "meteor" => 3,
            "quake" => 4,
            "explosion" => 5,
            "disease" => 10,
            "drowning" => 8,
            "starvation" => 15,
            "battle" => 20,
            _ => 12
        };
        if (MclslInverseTruthSystem.IsTruthReversed("truth_player_disaster_chance"))
            threshold = Math.Max(2, (int)Math.Ceiling(threshold * 0.72f));
        string emittedKey = cause + "|" + year;
        if (count < threshold || !EmittedNativeChanges.Add(emittedKey)) return;
        if (CountAvailableChanges(MclslWorldRunRepository.Current) >= 14) return;

        string[] laws = cause switch
        {
            "fire" => new[] { "火", "燃烧", "毁灭" },
            "frost" => new[] { "水", "冻结", "沉寂" },
            "thunder" => new[] { "雷", "速度", "毁灭" },
            "storm" => new[] { "风", "流转", "毁灭" },
            "meteor" => new[] { "土", "金", "毁灭" },
            "quake" => new[] { "土", "稳定", "毁灭" },
            "explosion" => new[] { "火", "金", "毁灭" },
            "disease" => new[] { "阴", "转化", "毁灭" },
            "drowning" => new[] { "水", "流转", "毁灭" },
            "starvation" => new[] { "土", "生机", "毁灭" },
            "battle" => new[] { "金", "锋锐", "毁灭" },
            _ => new[] { "毁灭", "余烬" }
        };
        string origin = cause switch
        {
            "fire" => "人间大火死伤汇聚",
            "frost" => "人间寒灾死伤汇聚",
            "thunder" => "人间雷灾死伤汇聚",
            "storm" => "人间风灾死伤汇聚",
            "meteor" => "天外陨落死伤汇聚",
            "quake" => "地脉震裂死伤汇聚",
            "explosion" => "爆裂灾厄死伤汇聚",
            "disease" => "人间疫病死伤汇聚",
            "drowning" => "人间水患死伤汇聚",
            "starvation" => "人间饥荒死伤汇聚",
            "battle" => "人间大战死伤汇聚",
            _ => "人间灾厄死伤汇聚"
        };
        string location = string.IsNullOrWhiteSpace(actor.city?.data?.name) ? "灾厄波及之地" : actor.city.data.name + "附近";
        string kingdom = string.IsNullOrWhiteSpace(actor.kingdom?.data?.name) ? "无主" : actor.kingdom.data.name;
        int sequence = MclslWorldRunRepository.NextProceduralSequence();
        int quality = count >= threshold * 2 ? 3 : 2;
        if (MclslInverseTruthSystem.IsTruthReversed("truth_player_disaster_chance") && quality < 4) quality++;
        MclslWorldChangeRecord change = MclslGeneratedObjectFactory.CreateWorldChange(year, sequence, location, kingdom, laws, quality, origin, "native_" + cause, actor.data.x, actor.data.y);
        MclslWorldRunRepository.Current.WorldChanges.Add(change);
        MclslWorldRunRepository.AddEvent(year, "native_world_change", change.Name + "应劫而生", origin + "，" + change.Name + "现于" + change.LocationName + "。", change.MapX, change.MapY, change.LocationName, change.NativeKingdomName);
        if (MclslRuntimeSettings.ResourceBirthAnnouncementsEnabled) MclslAnnouncementSystem.Enqueue(change.Name + "现于" + change.LocationName + "。", "#B579D6", 9f, 1);
        MclslWorldArchiveStore.MarkDirty();
    }

    internal static void BeginAnnual(int year)
    {
        if (!MclslNewLawPioneerSystem.CanUseWorldChanges(year)) return;
        if (_claimYear == year) return;
        Claims.Clear();
        _claimYear = year;
        EnsureWorldChanges(year);
    }

    internal static void EnsureWorldChanges(int year)
    {
        if (!MclslNewLawPioneerSystem.CanUseWorldChanges(year)) return;
        MclslWorldRunState run = MclslWorldRunRepository.Current;
        NormalizeLegacyChanges(run, year);
        int active = CountAvailableChanges(run);
        int population = MclslWorldActorQuery.UnitCount();
        bool pioneerEra = MclslNewLawPioneerSystem.IsPioneerEra(year);
        int desiredInitial = pioneerEra
            ? Math.Clamp(2 + population / 5000, 2, 4)
            : Math.Clamp(5 + population / 2200, 5, 14);
        int minimumActive = pioneerEra ? Math.Min(2, desiredInitial) : Math.Min(5, desiredInitial);
        while (run.WorldChanges.Count == 0 || active < minimumActive)
        {
            GeneratePeriodic(year, false);
            active++;
        }
        if (run.NextWorldChangeYear <= 0)
        {
            int initialInterval = pioneerEra ? 210 : 55;
            run.NextWorldChangeYear = year + MclslWorldStateModifierSystem.ScaleWorldChangeInterval(initialInterval, year);
        }
        int activeCap = pioneerEra ? 5 : 16;
        if (year >= run.NextWorldChangeYear && active < activeCap)
        {
            GeneratePeriodic(year, true);
            int seq = Math.Max(1, run.ProceduralSequence);
            int baseInterval = pioneerEra
                ? 150 + PositiveHash(run.RunId + "|next_pioneer_change|" + seq) % 111
                : 26 + PositiveHash(run.RunId + "|next_change|" + seq) % 53;
            run.NextWorldChangeYear = year + MclslWorldStateModifierSystem.ScaleWorldChangeInterval(baseInterval, year);
            MclslWorldArchiveStore.MarkDirty();
        }
    }

    internal static MclslWorldChangeRecord CreateFromTimeline(int year, string originName, IReadOnlyList<string> lawTags)
    {
        if (!MclslWorldEpochSystem.IsNewLawActive(year)) return null;
        LocationSeed location = PickLocation(PositiveHash(MclslWorldRunRepository.Current.RunId + "|timeline_change|" + year + "|" + originName));
        int sequence = MclslWorldRunRepository.NextProceduralSequence();
        MclslWorldChangeRecord change = MclslGeneratedObjectFactory.CreateWorldChange(year, sequence, location.Location, location.Kingdom, lawTags, 4, originName, "timeline", location.X, location.Y);
        MclslWorldRunRepository.Current.WorldChanges.Add(change);
        MclslWorldRunRepository.AddEvent(year, "world_change", change.Name + "爆发", originName + "余势未尽，" + change.Name + "由此成形。", change.MapX, change.MapY, change.LocationName, change.NativeKingdomName);
        if (MclslRuntimeSettings.ResourceBirthAnnouncementsEnabled) MclslAnnouncementSystem.Enqueue("天地有变：" + change.Name + "爆发。", "#B579D6", 9f, 1);
        MclslWorldArchiveStore.MarkDirty();
        return change;
    }

    internal static MclslWorldChangeRecord CreateFromRuinCollapse(int year, MclslSectRuinRecord ruin)
    {
        if (!MclslWorldEpochSystem.IsNewLawActive(year)) return null;
        if (ruin == null) return null;
        int sequence = MclslWorldRunRepository.NextProceduralSequence();
        int quality = Math.Clamp(ruin.Quality, 1, 4);
        MclslWorldChangeRecord change = MclslGeneratedObjectFactory.CreateWorldChange(year, sequence, ruin.LocationName, ruin.NativeKingdomName, MclslGeneratedObjectFactory.SplitTags(ruin.LawTags), quality, ruin.Name + "崩解", "ruin_collapse");
        MclslWorldRunRepository.Current.WorldChanges.Add(change);
        MclslWorldRunRepository.AddEvent(year, "world_change", ruin.Name + "崩毁成变", ruin.Name + "崩，" + change.Name + "生。");
        MclslWorldArchiveStore.MarkDirty();
        return change;
    }

    internal static void RegisterDivineClaim(Actor actor, int year)
    {
        if (!MclslNewLawPioneerSystem.CanUseWorldChanges(year)) return;
        if (!MclslActorAccessor.Alive(actor) || MclslActorAccessor.Realm(actor) != MclslRealmIds.YuanYing) return;
        BeginAnnual(year);
        if (!MclslRealmSeatSystem.CanAddDivineTransformation(out string seatReason))
        {
            MclslActorAccessor.Set(actor, MclslActorDataKeys.LastBreakthroughResult, seatReason);
            return;
        }
        string[] laws = MclslGeneratedObjectFactory.SplitTags(MclslActorAccessor.GetString(actor, MclslActorDataKeys.NascentEssenceTags,
            MclslActorAccessor.GetString(actor, MclslActorDataKeys.GoldenCoreLaws, string.Empty)));
        MclslWorldChangeRecord best = null;
        MclslWorldChangeRecord nearBest = null;
        int bestCompatibility = 0;
        int nearCompatibility = 0;
        int bestScore = int.MinValue;
        foreach (MclslWorldChangeRecord change in MclslWorldRunRepository.Current.WorldChanges)
        {
            if (!IsAvailable(change)) continue;
            string[] changeTags = MclslGeneratedObjectFactory.SplitTags(change.LawTags);
            int compatibility = Compatibility(actor, laws, changeTags);
            if (compatibility > nearCompatibility)
            {
                nearBest = change;
                nearCompatibility = compatibility;
            }
            if (compatibility < 50) continue;
            int score = compatibility * 10 + change.Intensity + change.RemainingMarrow * 25
                + PositiveHash(MclslActorAccessor.Id(actor) + "|choose_change|" + change.Id + "|" + year) % 41;
            if (score <= bestScore) continue;
            best = change;
            bestCompatibility = compatibility;
            bestScore = score;
        }
        if (best == null)
        {
            string detail = nearBest == null
                ? "本世暂无可抽髓的天地之变"
                : nearBest.Name + "仅" + MclslLawInteractionCatalog.Detail(laws, MclslGeneratedObjectFactory.SplitTags(nearBest.LawTags));
            MclslActorAccessor.Set(actor, MclslActorDataKeys.LastBreakthroughResult, "元婴圆满，却未遇到法则相合的天地之变；" + detail);
            return;
        }
        MclslAptitudeGiftDefinition gift = MclslAptitudeGiftCatalog.ForAptitude(MclslActorAccessor.GetInt(actor, MclslActorDataKeys.Aptitude, 50));
        int essenceQuality = MclslActorAccessor.GetInt(actor, MclslActorDataKeys.NascentEssenceQuality, 1);
        int ruinExperience = MclslActorAccessor.GetInt(actor, MclslActorDataKeys.RuinExperience, 0);
        int claimBonus = Math.Max(0, MclslActorAccessor.GetInt(actor, MclslActorDataKeys.DivineClaimBonus, 0));
        int strength = essenceQuality * 20 + bestCompatibility + claimBonus + (gift.BreakthroughBonus + gift.LawHarmonyBonus) * 2 + Math.Min(40, ruinExperience / 3)
            + MclslMindSystem.ClaimStrengthBonus(actor)
            + PositiveHash(MclslActorAccessor.Id(actor) + "|change_claim|" + best.Id + "|" + year) % 61;
        if (claimBonus > 0) MclslActorAccessor.Set(actor, MclslActorDataKeys.DivineClaimBonus, 0);
        if (!Claims.TryGetValue(best.Id, out List<ChangeClaim> list)) Claims[best.Id] = list = new List<ChangeClaim>();
        list.Add(new ChangeClaim { Actor = actor, ChangeId = best.Id, Compatibility = bestCompatibility, Strength = strength });
        MclslActorAccessor.Set(actor, MclslActorDataKeys.LastBreakthroughResult, "正在等待“" + best.Name + "”演化至可抽髓之机；" + MclslLawInteractionCatalog.Detail(laws, MclslGeneratedObjectFactory.SplitTags(best.LawTags)));
    }

    internal static void ResolveAnnual(int year)
    {
        if (_claimYear != year || Claims.Count == 0) return;
        foreach (KeyValuePair<string, List<ChangeClaim>> pair in Claims)
        {
            MclslWorldChangeRecord change = MclslWorldRunRepository.FindWorldChange(pair.Key);
            if (!IsAvailable(change)) continue;
            ChangeClaim winner = PickWinningClaim(pair.Value);
            if (winner == null) continue;
            int contenderCount = CountValidClaims(pair.Value);
            MclslAptitudeGiftDefinition gift = MclslAptitudeGiftCatalog.ForAptitude(MclslActorAccessor.GetInt(winner.Actor, MclslActorDataKeys.Aptitude, 50));
            int essenceQuality = MclslActorAccessor.GetInt(winner.Actor, MclslActorDataKeys.NascentEssenceQuality, 1);
            int chance = Math.Clamp(18 + winner.Compatibility / 2 + gift.BreakthroughBonus + gift.LawHarmonyBonus / 2 + essenceQuality * 6 + change.Intensity / 12 + MclslMindSystem.BreakthroughAdjustment(winner.Actor) - change.Quality * 4, 28, 94);
            int roll = PositiveHash(change.Id + "|extract|" + MclslActorAccessor.Id(winner.Actor) + "|" + year) % 100;
            if (roll < chance)
            {
                if (!MclslRealmSeatSystem.CanAddDivineTransformation(out string seatReason))
                {
                    MclslActorAccessor.Set(winner.Actor, MclslActorDataKeys.LastBreakthroughResult, seatReason);
                    continue;
                }
                CompleteExtraction(winner.Actor, change, year, winner.Compatibility, contenderCount);
                for (int i = 0; i < pair.Value.Count; i++)
                {
                    ChangeClaim claim = pair.Value[i];
                    if (claim == winner || !IsValidClaim(claim)) continue;
                    MclslActorAccessor.Set(claim.Actor, MclslActorDataKeys.LastBreakthroughResult, "争夺“" + change.Name + "”天地之髓失利");
                }
            }
            else
            {
                ResolveFailedExtraction(winner.Actor, change, year, winner.Compatibility);
                for (int i = 0; i < pair.Value.Count; i++)
                {
                    ChangeClaim claim = pair.Value[i];
                    if (claim == winner || !IsValidClaim(claim)) continue;
                    MclslActorAccessor.Set(claim.Actor, MclslActorDataKeys.LastBreakthroughResult, "未能接近“" + change.Name + "”核心");
                }
            }
        }
        Claims.Clear();
        MclslWorldArchiveStore.MarkDirty();
    }

    internal static bool TryAcquireExistingChangeForConversion(Actor actor, int year)
    {
        if (!MclslActorAccessor.Alive(actor)) return false;
        if (!string.IsNullOrWhiteSpace(MclslActorAccessor.GetString(actor, MclslActorDataKeys.DivineMarrowName, string.Empty))) return true;
        string[] laws = MclslGeneratedObjectFactory.SplitTags(MclslActorAccessor.GetString(actor, MclslActorDataKeys.NascentEssenceTags,
            MclslActorAccessor.GetString(actor, MclslActorDataKeys.GoldenCoreLaws, "灵")));
        MclslWorldChangeRecord change = PickBestAvailableChange(actor, laws);
        if (change == null) return false;
        int compatibility = Compatibility(actor, laws, MclslGeneratedObjectFactory.SplitTags(change.LawTags));
        if (compatibility < 50) return false;
        CompleteExtraction(actor, change, year, compatibility, 1, false);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.LastBreakthroughResult, "已寻得天地之变“" + change.Name + "”并抽得天地之髓");
        MclslWorldRunRepository.AddEvent(year, "ancient_conversion_change", MclslActorAccessor.DisplayName(actor) + "抽得天地之髓",
            MclslActorAccessor.DisplayName(actor) + "为转修新法寻至“" + change.Name + "”，抽得其中天地之髓。", actor);
        return true;
    }

    internal static void EnsureManualDivineData(Actor actor, int year)
    {
        if (!MclslActorAccessor.Alive(actor)) return;
        if (!MclslNewLawPioneerSystem.CanUseWorldChanges(year))
        {
            MclslActorAccessor.Set(actor, MclslActorDataKeys.LastBreakthroughResult, "新法高阶资源尚未显世，无法补全天地之变与天地之髓");
            return;
        }
        if (MclslRealmIds.Index(MclslActorAccessor.Realm(actor)) < MclslRealmIds.Index(MclslRealmIds.HuaShen)
            && !MclslRealmSeatSystem.CanAddDivineTransformation(out string seatReason))
        {
            MclslActorAccessor.Set(actor, MclslActorDataKeys.LastBreakthroughResult, seatReason);
            return;
        }
        EnsureWorldChanges(year);
        string[] laws = MclslGeneratedObjectFactory.SplitTags(MclslActorAccessor.GetString(actor, MclslActorDataKeys.NascentEssenceTags,
            MclslActorAccessor.GetString(actor, MclslActorDataKeys.GoldenCoreLaws, "灵")));
        MclslWorldChangeRecord change = PickBestAvailableChange(actor, laws);
        if (change == null || Compatibility(actor, laws, MclslGeneratedObjectFactory.SplitTags(change.LawTags)) < 50)
        {
            int sequence = MclslWorldRunRepository.NextProceduralSequence();
            string city = string.IsNullOrWhiteSpace(actor.city?.data?.name) ? "玩家敕定之地" : actor.city.data.name + "附近";
            string kingdom = string.IsNullOrWhiteSpace(actor.kingdom?.data?.name) ? "无主" : actor.kingdom.data.name;
            change = MclslGeneratedObjectFactory.CreateWorldChange(year, sequence, city, kingdom, laws, 2, "玩家手动补全", "manual", actor.data.x, actor.data.y);
            MclslWorldRunRepository.Current.WorldChanges.Add(change);
        }
        int compatibility = Math.Max(75, Compatibility(actor, laws, MclslGeneratedObjectFactory.SplitTags(change.LawTags)));
        CompleteExtraction(actor, change, year, compatibility, 1);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.LastBreakthroughResult, "玩家手动赋予境界，并补全天地之变与天地之髓");
    }

    internal static void Clear()
    {
        Claims.Clear();
        NativeDeathSignals.Clear();
        EmittedNativeChanges.Clear();
        _claimYear = -1;
        _nativeSignalYear = -1;
    }

    private static void CompleteExtraction(Actor actor, MclslWorldChangeRecord change, int year, int compatibility, int contenderCount, bool advanceRealm = true)
    {
        NormalizeChangeUses(change);
        MclslGeneratedItemRecord marrow = MclslGeneratedObjectFactory.CreateHeavenEarthMarrow(actor, change, year, compatibility);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.DivineChangeId, change.Id);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.DivineChangeName, change.Name);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.DivineChangeTags, change.LawTags);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.DivineChangeCompatibility, compatibility);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.DivineMarrowId, marrow.Id);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.DivineMarrowName, marrow.Name);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.DivineMarrow, 1);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.DivineMarrowQuality, marrow.Quality);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.DivineMarrowTags, marrow.LawTags);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.DivineMarrowDescription, marrow.Description);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.DivineMarrowEffects, marrow.AttributeText);

        change.RemainingMarrow = Math.Max(0, change.RemainingMarrow - 1);
        change.ExtractedCount++;
        change.LastExtractedByActorId = MclslActorAccessor.Id(actor);
        change.LastExtractedByActorName = MclslActorAccessor.DisplayName(actor, MclslRealmIds.HuaShen);
        change.Intensity = Math.Max(0, change.Intensity - (18 + change.Quality * 5));
        if (change.RemainingMarrow > 0 && change.Intensity > 20)
        {
            change.State = "衰减";
            if (change.EndYear <= year) change.EndYear = 0;
        }
        else
        {
            change.State = "平息";
            change.EndYear = year;
        }

        if (advanceRealm)
        {
            MclslCultivationSystem.SetRealm(actor, MclslRealmIds.HuaShen, year, "自“" + change.Name + "”抽得天地之髓“" + marrow.Name + "”，成就化神");
            MclslResourceSystem.GrantWorldChangeReward(actor, change.Quality, compatibility, contenderCount);
            string contest = contenderCount > 1 ? "压过" + (contenderCount - 1) + "名元婴后，" : string.Empty;
            string relation = MclslLawInteractionCatalog.Detail(
                MclslGeneratedObjectFactory.SplitTags(MclslActorAccessor.GetString(actor, MclslActorDataKeys.NascentEssenceTags,
                    MclslActorAccessor.GetString(actor, MclslActorDataKeys.GoldenCoreLaws, string.Empty))),
                MclslGeneratedObjectFactory.SplitTags(change.LawTags));
            string displayName = MclslActorAccessor.DisplayName(actor, MclslRealmIds.HuaShen);
            MclslWorldRunRepository.AddEvent(year, "divine_transformation", displayName + "成就化神", contest + displayName + "抽得“" + marrow.Name + "”，由此化神。", actor);
            if (MclslRuntimeSettings.DivineBreakthroughAnnouncementsEnabled)
                MclslAnnouncementSystem.Enqueue(displayName + "抽髓化神。", "#B579D6", 9f, 1);
        }
    }

    private static void ResolveFailedExtraction(Actor actor, MclslWorldChangeRecord change, int year, int compatibility)
    {
        int deathChance = Math.Clamp(6 + change.Quality * 5 + change.Intensity / 10 - compatibility / 8 - MclslMindSystem.StabilityBonus(actor) / 2 - MclslSpiritualRootSystem.LawHarmonyBonus(actor) / 8, 4, 35);
        bool failureStep = MclslInverseTruthSystem.IsTruthReversed("truth_player_failure_steps");
        if (failureStep) deathChance = Math.Max(2, deathChance - 8);
        int deathRoll = PositiveHash(change.Id + "|backlash_death|" + MclslActorAccessor.Id(actor) + "|" + year) % 100;
        change.Intensity = Math.Max(1, change.Intensity - 5);
        if (deathRoll < deathChance)
        {
            string relation = MclslLawInteractionCatalog.Detail(
                MclslGeneratedObjectFactory.SplitTags(MclslActorAccessor.GetString(actor, MclslActorDataKeys.NascentEssenceTags,
                    MclslActorAccessor.GetString(actor, MclslActorDataKeys.GoldenCoreLaws, string.Empty))),
                MclslGeneratedObjectFactory.SplitTags(change.LawTags));
            string detail = MclslActorAccessor.DisplayName(actor) + "强抽“" + change.Name + "”，洞天崩毁。";
            MclslWorldRunRepository.AddEvent(year, "world_change_death", MclslActorAccessor.DisplayName(actor) + "抽髓身陨", detail, actor);
            MclslDeathSystem.ExecuteScriptedDeath(actor, "world_change_backlash", change.Name, detail, true);
            return;
        }
        if (failureStep)
            MclslActorAccessor.Set(actor, MclslActorDataKeys.DivineClaimBonus,
                Math.Min(100, MclslActorAccessor.GetInt(actor, MclslActorDataKeys.DivineClaimBonus, 0) + 8));
        MclslCultivationGrowthSystem.ApplyProgressSetback(
            actor,
            MclslActorAccessor.Realm(actor),
            failureStep ? 10f : 18f,
            ancientLaw: false,
            minimumProgressPercent: failureStep ? 72f : 55f);
        int caveIntegrity = Math.Clamp(MclslActorAccessor.GetInt(actor, MclslActorDataKeys.NascentCaveIntegrity, 70), 0, 100);
        int integrityLoss = Math.Clamp(6 + change.Quality * 3 + Math.Max(0, 65 - compatibility) / 8, 6, 22);
        int newIntegrity = Math.Max(0, caveIntegrity - integrityLoss);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.NascentCaveIntegrity, newIntegrity);
        string failedRelation = MclslLawInteractionCatalog.Detail(
            MclslGeneratedObjectFactory.SplitTags(MclslActorAccessor.GetString(actor, MclslActorDataKeys.NascentEssenceTags,
                MclslActorAccessor.GetString(actor, MclslActorDataKeys.GoldenCoreLaws, string.Empty))),
            MclslGeneratedObjectFactory.SplitTags(change.LawTags));
        string consequence = "元婴洞天受损" + integrityLoss + "%，完整度降至" + newIntegrity + "%，修炼进度回落";
        if (failureStep) consequence += "，败痕转为下一次抽髓补正";
        MclslActorAccessor.Set(actor, MclslActorDataKeys.LastBreakthroughResult, "抽取“" + change.Name + "”之髓失败，" + consequence + "；" + failedRelation);
        MclslWorldRunRepository.AddEvent(year, "marrow_extract_failed", change.Name + "抽髓未成", MclslActorAccessor.DisplayName(actor) + "败退，洞天震荡。", actor);
    }

    private static void GeneratePeriodic(int year, bool announce)
    {
        MclslWorldRunState run = MclslWorldRunRepository.Current;
        int sequence = MclslWorldRunRepository.NextProceduralSequence();
        int hash = PositiveHash(run.RunId + "|change_profile|" + sequence + "|" + year);
        MclslTechniqueDefinition technique = MclslCultivationCatalog.Techniques[hash % MclslCultivationCatalog.Techniques.Count];
        int qualityRoll = PositiveHash(hash + "|quality") % 100;
        int quality = qualityRoll < 6 ? 4 : qualityRoll < 26 ? 3 : qualityRoll < 68 ? 2 : 1;
        int tagCount = quality >= 3 ? 3 : 2;
        List<string> tags = new();
        int start = PositiveHash(hash + "|tag") % technique.LawPool.Length;
        for (int i = 0; i < technique.LawPool.Length && tags.Count < tagCount; i++)
            if (!tags.Contains(technique.LawPool[(start + i) % technique.LawPool.Length])) tags.Add(technique.LawPool[(start + i) % technique.LawPool.Length]);
        if (quality == 4 && PositiveHash(hash + "|rare") % 100 < 45) tags[tags.Count - 1] = "毁灭";
        LocationSeed location = PickLocation(hash);
        string[] origins = { "地脉骤变", "灵潮逆流", "洞天根源震荡", "天象异变", "界域交错", "山河震荡" };
        string origin = origins[hash % origins.Length];
        MclslWorldChangeRecord change = MclslGeneratedObjectFactory.CreateWorldChange(year, sequence, location.Location, location.Kingdom, tags, quality, origin, "periodic", location.X, location.Y);
        run.WorldChanges.Add(change);
        if (announce)
        {
            MclslWorldRunRepository.AddEvent(year, "world_change", change.Name + "爆发", origin + "，" + change.Name + "现于" + change.LocationName + "。", change.MapX, change.MapY, change.LocationName, change.NativeKingdomName);
            if (MclslRuntimeSettings.ResourceBirthAnnouncementsEnabled) MclslAnnouncementSystem.Enqueue("天地有变：" + change.Name + "爆发。", "#B579D6", 8f, 1);
        }
        MclslWorldArchiveStore.MarkDirty();
    }

    private static void NormalizeLegacyChanges(MclslWorldRunState run, int year)
    {
        for (int i = 0; i < run.WorldChanges.Count; i++)
        {
            MclslWorldChangeRecord change = run.WorldChanges[i];
            if (change == null) continue;
            if (string.IsNullOrWhiteSpace(change.Description)) change.Description = "旧世记录中的天地之变，仍可供元婴修士抽髓。";
            if (string.IsNullOrWhiteSpace(change.Origin)) change.Origin = "旧世历史锚点";
            if (change.Quality <= 0) change.Quality = 3;
            if (change.MarrowCapacity <= 0) change.MarrowCapacity = Math.Max(1, change.RemainingMarrow);
            if (change.Intensity <= 0 && change.RemainingMarrow > 0) change.Intensity = 80;
            if (string.IsNullOrWhiteSpace(change.State)) change.State = change.RemainingMarrow > 0 ? "活跃" : "平息";
            if (string.IsNullOrWhiteSpace(change.LocationName)) change.LocationName = "天地之间";
            if (string.IsNullOrWhiteSpace(change.NativeKingdomName)) change.NativeKingdomName = "无主";
            if (string.IsNullOrWhiteSpace(change.NativeTerrainEffect))
            {
                change.NativeTerrainEffect = MclslNativeTerrainProfileCatalog
                    .ForTags(MclslGeneratedObjectFactory.SplitTags(change.LawTags), change.SourceType)
                    .Summary;
            }
            if (change.StartYear <= 0) change.StartYear = year;
            if ((change.Name == "白雾吞界" || change.Id.StartsWith("change_white_mist_", StringComparison.Ordinal)) && change.SourceType != "timeline_white_mist")
            {
                change.Origin = "白雾吞界";
                change.Name = MclslGeneratedObjectFactory.GenerateUniqueName(
                    MclslGeneratedKinds.WorldChange,
                    MclslGeneratedObjectFactory.SplitTags(change.LawTags),
                    PositiveHash(change.Id + "|migrate_name|" + year),
                    change.Origin);
                change.SourceType = "timeline_white_mist";
            }
            if (!string.IsNullOrWhiteSpace(change.Name) && !ContainsOrdinal(run.UsedGeneratedNames, change.Name)) run.UsedGeneratedNames.Add(change.Name);
            NormalizeChangeUses(change);
        }
    }

    private static bool ContainsOrdinal(IReadOnlyList<string> values, string target)
    {
        if (values == null || string.IsNullOrWhiteSpace(target)) return false;
        for (int i = 0; i < values.Count; i++)
        {
            if (string.Equals(values[i], target, StringComparison.Ordinal)) return true;
        }
        return false;
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
                locations.Add(new LocationSeed { Location = city + "附近", Kingdom = kingdom, X = actor.data.x, Y = actor.data.y });
            }
        }
        if (locations.Count > 0) return locations[(hash & int.MaxValue) % locations.Count];
        return new LocationSeed { Location = MclslProceduralLexicon.CaveOrigins[(hash & int.MaxValue) % MclslProceduralLexicon.CaveOrigins.Length], Kingdom = "无主" };
    }

    private static int Compatibility(IReadOnlyList<string> laws, IReadOnlyList<string> changeTags)
    {
        return MclslLawInteractionCatalog.CompatibilityScore(laws, changeTags);
    }

    private static int Compatibility(Actor actor, IReadOnlyList<string> laws, IReadOnlyList<string> changeTags)
    {
        return Math.Clamp(Compatibility(laws, changeTags) + MclslSpiritualRootSystem.LawHarmonyBonus(actor), 0, 100);
    }

    private static int CountAvailableChanges(MclslWorldRunState run)
    {
        if (run?.WorldChanges == null) return 0;
        int count = 0;
        for (int i = 0; i < run.WorldChanges.Count; i++)
            if (IsAvailable(run.WorldChanges[i])) count++;
        return count;
    }

    private static ChangeClaim PickWinningClaim(List<ChangeClaim> claims)
    {
        if (claims == null || claims.Count == 0) return null;
        ChangeClaim best = null;
        long bestId = long.MaxValue;
        for (int i = 0; i < claims.Count; i++)
        {
            ChangeClaim claim = claims[i];
            if (!IsValidClaim(claim)) continue;
            long actorId = MclslActorAccessor.Id(claim.Actor);
            if (best == null || claim.Strength > best.Strength ||
                (claim.Strength == best.Strength && actorId < bestId))
            {
                best = claim;
                bestId = actorId;
            }
        }
        return best;
    }

    private static int CountValidClaims(List<ChangeClaim> claims)
    {
        if (claims == null) return 0;
        int count = 0;
        for (int i = 0; i < claims.Count; i++)
            if (IsValidClaim(claims[i])) count++;
        return count;
    }

    private static bool IsValidClaim(ChangeClaim claim)
    {
        return claim != null && MclslActorAccessor.Alive(claim.Actor) && MclslActorAccessor.Realm(claim.Actor) == MclslRealmIds.YuanYing;
    }

    private static MclslWorldChangeRecord PickBestAvailableChange(Actor actor, IReadOnlyList<string> laws)
    {
        MclslWorldRunState run = MclslWorldRunRepository.Current;
        if (run?.WorldChanges == null) return null;
        MclslWorldChangeRecord best = null;
        int bestCompatibility = int.MinValue;
        for (int i = 0; i < run.WorldChanges.Count; i++)
        {
            MclslWorldChangeRecord change = run.WorldChanges[i];
            if (!IsAvailable(change)) continue;
            int compatibility = Compatibility(actor, laws, MclslGeneratedObjectFactory.SplitTags(change.LawTags));
            if (compatibility <= bestCompatibility) continue;
            best = change;
            bestCompatibility = compatibility;
        }
        return best;
    }

    private static bool IsAvailable(MclslWorldChangeRecord change)
    {
        NormalizeChangeUses(change);
        return change != null && change.RemainingMarrow > 0 && change.Intensity > 20 && !string.Equals(change.State, "平息", StringComparison.Ordinal);
    }

    private static void NormalizeChangeUses(MclslWorldChangeRecord change)
    {
        if (change == null) return;
        int capacity = MclslInverseTruthSystem.IsTruthReversed("truth_player_trace_persistence") ? 2 : 1;
        int used = Math.Max(0, change.ExtractedCount);
        change.MarrowCapacity = capacity;
        int expectedRemaining = Math.Max(0, capacity - used);
        change.RemainingMarrow = Math.Clamp(change.RemainingMarrow, 0, capacity);
        if (change.RemainingMarrow < expectedRemaining && change.Intensity > 20)
            change.RemainingMarrow = expectedRemaining;
        if (change.RemainingMarrow > 0 && change.Intensity > 20 && string.Equals(change.State, "平息", StringComparison.Ordinal))
            change.State = used > 0 ? "衰减" : "活跃";
    }

    private static string NativeCause(Actor actor, string attackType)
    {
        string attack = (attackType ?? string.Empty).ToLowerInvariant();
        if (attack.Contains("fire")) return "fire";
        if (attack.Contains("cold") || attack.Contains("frost") || attack.Contains("freeze") || attack.Contains("snow")) return "frost";
        if (attack.Contains("lightning") || attack.Contains("thunder") || attack.Contains("electric")) return "thunder";
        if (attack.Contains("tornado") || attack.Contains("wind") || attack.Contains("storm")) return "storm";
        if (attack.Contains("meteor") || attack.Contains("asteroid")) return "meteor";
        if (attack.Contains("quake") || attack.Contains("earthquake") || attack.Contains("earth") || attack.Contains("crack")) return "quake";
        if (attack.Contains("bomb") || attack.Contains("explosion") || attack.Contains("explode")) return "explosion";
        if (attack.Contains("infection") || attack.Contains("plague") || attack.Contains("fever") || attack.Contains("tumor") || attack.Contains("poison")) return "disease";
        if (attack.Contains("drown") || attack == "water") return "drowning";
        if (attack.Contains("hunger") || attack.Contains("starv")) return "starvation";
        if (attack.Contains("age") || attack.Contains("old") || attack == "none" || attack == "0" || attack.Contains("other")) return string.Empty;
        if (IsExplicitBattleDeath(actor, attack)) return "battle";
        return string.Empty;
    }

    private static bool IsExplicitBattleDeath(Actor actor, string attack)
    {
        if (actor?.kingdom == null || actor.kingdom.isNeutral()) return false;
        return attack.Contains("weapon")
            || attack.Contains("sword")
            || attack.Contains("arrow")
            || attack.Contains("bow")
            || attack.Contains("attack")
            || attack.Contains("war")
            || attack.Contains("melee")
            || attack.Contains("projectile");
    }

    private static int PositiveHash(string value)
    {
        unchecked { int result = 41; foreach (char c in value ?? string.Empty) result = result * 47 + c; return result & int.MaxValue; }
    }
}
