using System;
using System.Collections.Generic;
using MySimulatedLongevityRoad.Core;
using MySimulatedLongevityRoad.Data;
using MySimulatedLongevityRoad.Queries;

namespace MySimulatedLongevityRoad.Systems;

internal static class MclslWorldCaveSystem
{
    private sealed class CaveClaim
    {
        internal Actor Actor;
        internal string CaveId = string.Empty;
        internal int Compatibility;
        internal int Strength;
    }

    private sealed class LocationSeed
    {
        internal string Location = "无主荒域";
        internal string Kingdom = "无主";
        internal int X = -1;
        internal int Y = -1;
    }

    private static readonly Dictionary<string, List<CaveClaim>> Claims = new(StringComparer.Ordinal);
    private static int _claimYear = -1;

    internal static void BeginAnnual(int year)
    {
        if (!MclslNewLawPioneerSystem.CanUseNewLawResources(year)) return;
        if (_claimYear == year) return;
        Claims.Clear();
        _claimYear = year;
        EnsureWorldCaves(year);
    }

    internal static void EnsureWorldCaves(int year)
    {
        if (!MclslNewLawPioneerSystem.CanUseNewLawResources(year)) return;
        MclslWorldRunState run = MclslWorldRunRepository.Current;
        int active = CountAvailableCaves(run);
        int population = MclslWorldActorQuery.UnitCount();
        bool pioneerEra = MclslNewLawPioneerSystem.IsPioneerEra(year);
        int desiredInitial = pioneerEra
            ? Math.Clamp(4 + population / 3000, 4, 8)
            : Math.Clamp(10 + population / 700, 10, 34);
        int minimumActive = pioneerEra ? Math.Min(4, desiredInitial) : Math.Min(10, desiredInitial);
        while (run.WorldCaves.Count == 0 || active < minimumActive)
        {
            GenerateCave(year, false);
            active++;
        }
        if (!pioneerEra && run.WorldCaves.Count < desiredInitial && run.LastProcessedYear <= run.StartYear + 1)
        {
            while (run.WorldCaves.Count < desiredInitial) GenerateCave(year, false);
        }
        if (run.NextCaveBirthYear <= 0)
        {
            int initialInterval = pioneerEra ? 150 : 80;
            run.NextCaveBirthYear = year + MclslWorldStateModifierSystem.ScaleCaveInterval(initialInterval, year);
        }
        int activeCap = pioneerEra ? 8 : 40;
        if (year >= run.NextCaveBirthYear && active < activeCap)
        {
            GenerateCave(year, true);
            int sequence = Math.Max(1, run.ProceduralSequence);
            int baseInterval = pioneerEra
                ? 120 + PositiveHash(run.RunId + "|next_pioneer_cave|" + sequence) % 81
                : 28 + PositiveHash(run.RunId + "|next_cave|" + sequence) % 49;
            run.NextCaveBirthYear = year + MclslWorldStateModifierSystem.ScaleCaveInterval(baseInterval, year);
            MclslWorldArchiveStore.MarkDirty();
        }
    }

    internal static void RegisterNascentClaim(Actor actor, int year)
    {
        if (!MclslNewLawPioneerSystem.CanUseNewLawResources(year)) return;
        if (!MclslActorAccessor.Alive(actor) || MclslActorAccessor.Realm(actor) != MclslRealmIds.JinDan) return;
        BeginAnnual(year);
        if (!MclslRealmSeatSystem.CanAddNascentSoul(out string seatReason))
        {
            MclslActorAccessor.Set(actor, MclslActorDataKeys.LastBreakthroughResult, seatReason);
            return;
        }
        string[] laws = MclslGeneratedObjectFactory.SplitTags(MclslActorAccessor.GetString(actor, MclslActorDataKeys.GoldenCoreLaws, string.Empty));
        MclslWorldCaveRecord best = null;
        MclslWorldCaveRecord nearBest = null;
        int bestCompatibility = 0;
        int nearCompatibility = 0;
        int bestScore = int.MinValue;
        foreach (MclslWorldCaveRecord cave in MclslWorldRunRepository.Current.WorldCaves)
        {
            if (!IsAvailable(cave)) continue;
            string[] caveTags = MclslGeneratedObjectFactory.SplitTags(cave.LawTags);
            int compatibility = Compatibility(actor, laws, caveTags);
            if (compatibility > nearCompatibility)
            {
                nearBest = cave;
                nearCompatibility = compatibility;
            }
            if (compatibility < 50) continue;
            int score = compatibility * 10 + cave.Integrity + cave.RemainingEssence * 20
                + PositiveHash(MclslActorAccessor.Id(actor) + "|choose_cave|" + cave.Id + "|" + year) % 31;
            if (score <= bestScore) continue;
            best = cave;
            bestCompatibility = compatibility;
            bestScore = score;
        }
        if (best == null)
        {
            string detail = nearBest == null
                ? "本世暂无可争洞天"
                : nearBest.Name + "仅" + MclslLawInteractionCatalog.Detail(laws, MclslGeneratedObjectFactory.SplitTags(nearBest.LawTags));
            MclslActorAccessor.Set(actor, MclslActorDataKeys.LastBreakthroughResult, "金丹圆满，却未寻到法则相合的洞天；" + detail);
            return;
        }
        MclslAptitudeGiftDefinition gift = MclslAptitudeGiftCatalog.ForAptitude(MclslActorAccessor.GetInt(actor, MclslActorDataKeys.Aptitude, 50));
        int purity = MclslActorAccessor.GetInt(actor, MclslActorDataKeys.GoldenCorePurity, 50);
        int stability = MclslActorAccessor.GetInt(actor, MclslActorDataKeys.GoldenCoreStability, 50);
        int claimBonus = Math.Max(0, MclslActorAccessor.GetInt(actor, MclslActorDataKeys.CaveClaimBonus, 0));
        int strength = purity + stability + bestCompatibility + claimBonus + (gift.BreakthroughBonus + gift.LawHarmonyBonus) * 2
            + MclslMindSystem.ClaimStrengthBonus(actor)
            + PositiveHash(MclslActorAccessor.Id(actor) + "|cave_claim|" + best.Id + "|" + year) % 51;
        if (claimBonus > 0) MclslActorAccessor.Set(actor, MclslActorDataKeys.CaveClaimBonus, 0);
        if (!Claims.TryGetValue(best.Id, out List<CaveClaim> list)) Claims[best.Id] = list = new List<CaveClaim>();
        list.Add(new CaveClaim { Actor = actor, CaveId = best.Id, Compatibility = bestCompatibility, Strength = strength });
        MclslActorAccessor.Set(actor, MclslActorDataKeys.LastBreakthroughResult, "正在争夺“" + best.Name + "”的天地之精；" + MclslLawInteractionCatalog.Detail(laws, MclslGeneratedObjectFactory.SplitTags(best.LawTags)));
    }

    internal static void ResolveAnnual(int year)
    {
        if (_claimYear != year || Claims.Count == 0) return;
        foreach (KeyValuePair<string, List<CaveClaim>> pair in Claims)
        {
            MclslWorldCaveRecord cave = MclslWorldRunRepository.FindCave(pair.Key);
            if (!IsAvailable(cave)) continue;
            CaveClaim winner = PickWinningClaim(pair.Value);
            if (winner == null) continue;
            int contenderCount = CountValidClaims(pair.Value);
            cave.LastContestedYear = year;
            int purity = MclslActorAccessor.GetInt(winner.Actor, MclslActorDataKeys.GoldenCorePurity, 50);
            int stability = MclslActorAccessor.GetInt(winner.Actor, MclslActorDataKeys.GoldenCoreStability, 50);
            MclslAptitudeGiftDefinition gift = MclslAptitudeGiftCatalog.ForAptitude(MclslActorAccessor.GetInt(winner.Actor, MclslActorDataKeys.Aptitude, 50));
            int chance = Math.Clamp(24 + winner.Compatibility / 2 + purity / 6 + stability / 10 + cave.Integrity / 18 + gift.BreakthroughBonus + gift.LawHarmonyBonus / 2 + MclslMindSystem.BreakthroughAdjustment(winner.Actor) - cave.Quality * 3, 35, 96);
            int roll = PositiveHash(cave.Id + "|refine|" + MclslActorAccessor.Id(winner.Actor) + "|" + year) % 100;
            if (roll < chance)
            {
                if (!MclslRealmSeatSystem.CanAddNascentSoul(out string seatReason))
                {
                    MclslActorAccessor.Set(winner.Actor, MclslActorDataKeys.LastBreakthroughResult, seatReason);
                    continue;
                }
                CompleteRefinement(winner.Actor, cave, year, winner.Compatibility, contenderCount);
                for (int i = 0; i < pair.Value.Count; i++)
                {
                    CaveClaim claim = pair.Value[i];
                    if (claim == winner || !IsValidClaim(claim)) continue;
                    Actor loser = claim.Actor;
                    MclslActorAccessor.Set(loser, MclslActorDataKeys.LastBreakthroughResult, "争夺“" + cave.Name + "”失利，天地之精已被" + MclslActorAccessor.DisplayName(winner.Actor) + "先行炼化");
                }
            }
            else
            {
                cave.Integrity = Math.Max(1, cave.Integrity - 3);
                if (MclslInverseTruthSystem.IsTruthReversed("truth_player_failure_steps"))
                    MclslActorAccessor.Set(winner.Actor, MclslActorDataKeys.CaveClaimBonus,
                        Math.Min(100, MclslActorAccessor.GetInt(winner.Actor, MclslActorDataKeys.CaveClaimBonus, 0) + 6));
                string relation = MclslLawInteractionCatalog.Detail(
                    MclslGeneratedObjectFactory.SplitTags(MclslActorAccessor.GetString(winner.Actor, MclslActorDataKeys.GoldenCoreLaws, string.Empty)),
                    MclslGeneratedObjectFactory.SplitTags(cave.LawTags));
                string scar = MclslInverseTruthSystem.IsTruthReversed("truth_player_failure_steps") ? "；败痕为阶，下一次洞天争夺获得补正" : string.Empty;
                MclslActorAccessor.Set(winner.Actor, MclslActorDataKeys.LastBreakthroughResult, "炼化“" + cave.Name + "”失败，道基未损，洞天根源轻微动荡" + scar + "；" + relation);
                for (int i = 0; i < pair.Value.Count; i++)
                {
                    CaveClaim claim = pair.Value[i];
                    if (claim == winner || !IsValidClaim(claim)) continue;
                    MclslActorAccessor.Set(claim.Actor, MclslActorDataKeys.LastBreakthroughResult, "争夺“" + cave.Name + "”未果");
                }
                MclslWorldRunRepository.AddEvent(year, "cave_refine_failed", cave.Name + "炼化未成", MclslActorAccessor.DisplayName(winner.Actor) + "压过诸修，却未能夺得其中天地之精；" + relation + "。洞天完整度降至" + cave.Integrity + "%。", winner.Actor);
            }
        }
        Claims.Clear();
        MclslWorldArchiveStore.MarkDirty();
    }

    internal static MclslWorldCaveRecord GenerateCaveFromDiscovery(int year, Actor discoverer, IReadOnlyList<string> lawTags, string sourceName)
    {
        if (!MclslNewLawPioneerSystem.CanUseNewLawResources(year)) return null;
        if (!MclslActorAccessor.Alive(discoverer)) return null;
        int sequence = MclslWorldRunRepository.NextProceduralSequence();
        string location = string.IsNullOrWhiteSpace(discoverer.city?.data?.name) ? "无主荒域" : discoverer.city.data.name + "附近";
        string kingdom = string.IsNullOrWhiteSpace(discoverer.kingdom?.data?.name) ? "无主" : discoverer.kingdom.data.name;
        int quality = Math.Clamp(1 + PositiveHash(MclslActorAccessor.Id(discoverer) + "|ruin_cave_quality|" + year + "|" + sourceName) % 3, 1, 3);
        MclslWorldCaveRecord cave = MclslGeneratedObjectFactory.CreateWorldCave(year, sequence, location, kingdom, lawTags, quality, discoverer.data.x, discoverer.data.y);
        MclslWorldRunRepository.Current.WorldCaves.Add(cave);
        string displayName = MclslActorAccessor.DisplayName(discoverer);
        MclslWorldRunRepository.AddEvent(year, "cave_discovered", displayName + "发现" + cave.Name, "自宗门遗迹“" + sourceName + "”所得残图指向一处洞天“" + cave.Name + "”。洞天仍附着于原生世界，不改变当地国家归属。", discoverer);
        if (MclslRuntimeSettings.ResourceBirthAnnouncementsEnabled) MclslAnnouncementSystem.Enqueue(displayName + "循遗迹残图发现洞天“" + cave.Name + "”。", "#79B6B0", 8f, 1);
        MclslWorldArchiveStore.MarkDirty();
        return cave;
    }

    internal static bool TryAcquireExistingCaveForConversion(Actor actor, int year)
    {
        if (!MclslActorAccessor.Alive(actor)) return false;
        if (!string.IsNullOrWhiteSpace(MclslActorAccessor.GetString(actor, MclslActorDataKeys.NascentEssenceName, string.Empty))) return true;
        string[] laws = MclslGeneratedObjectFactory.SplitTags(MclslActorAccessor.GetString(actor, MclslActorDataKeys.GoldenCoreLaws, "灵"));
        MclslWorldCaveRecord cave = PickBestAvailableCave(actor, laws);
        if (cave == null) return false;
        int compatibility = Compatibility(actor, laws, MclslGeneratedObjectFactory.SplitTags(cave.LawTags));
        if (compatibility < 50) return false;
        CompleteRefinement(actor, cave, year, compatibility, 1, false);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.LastBreakthroughResult, "已寻得洞天“" + cave.Name + "”并炼得天地之精");
        MclslWorldRunRepository.AddEvent(year, "ancient_conversion_cave", MclslActorAccessor.DisplayName(actor) + "炼得天地之精",
            MclslActorAccessor.DisplayName(actor) + "为转修新法寻至“" + cave.Name + "”，炼得其中天地之精。", actor);
        return true;
    }

    internal static void EnsureManualNascentData(Actor actor, int year)
    {
        if (!MclslActorAccessor.Alive(actor)) return;
        if (!MclslNewLawPioneerSystem.CanUseNewLawResources(year))
        {
            MclslActorAccessor.Set(actor, MclslActorDataKeys.LastBreakthroughResult, "新法尚未出现，无法补全洞天与天地之精");
            return;
        }
        if (MclslRealmIds.Index(MclslActorAccessor.Realm(actor)) < MclslRealmIds.Index(MclslRealmIds.YuanYing)
            && !MclslRealmSeatSystem.CanAddNascentSoul(out string seatReason))
        {
            MclslActorAccessor.Set(actor, MclslActorDataKeys.LastBreakthroughResult, seatReason);
            return;
        }
        EnsureWorldCaves(year);
        string[] laws = MclslGeneratedObjectFactory.SplitTags(MclslActorAccessor.GetString(actor, MclslActorDataKeys.GoldenCoreLaws, "灵"));
        MclslWorldCaveRecord cave = PickBestAvailableCave(actor, laws);
        if (cave == null || Compatibility(actor, laws, MclslGeneratedObjectFactory.SplitTags(cave.LawTags)) < 50)
        {
            int sequence = MclslWorldRunRepository.NextProceduralSequence();
            string location = string.IsNullOrWhiteSpace(actor.city?.data?.name) ? "玩家敕定之地" : actor.city.data.name + "附近";
            string kingdom = string.IsNullOrWhiteSpace(actor.kingdom?.data?.name) ? "无主" : actor.kingdom.data.name;
            cave = MclslGeneratedObjectFactory.CreateWorldCave(year, sequence, location, kingdom, laws, 2, actor.data.x, actor.data.y);
            MclslWorldRunRepository.Current.WorldCaves.Add(cave);
        }
        int compatibility = Math.Max(70, Compatibility(actor, laws, MclslGeneratedObjectFactory.SplitTags(cave.LawTags)));
        CompleteRefinement(actor, cave, year, compatibility, 1);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.LastBreakthroughResult, "玩家手动赋予境界，并补全动态洞天与天地之精");
    }

    internal static void Clear()
    {
        Claims.Clear();
        _claimYear = -1;
    }

    private static void CompleteRefinement(Actor actor, MclslWorldCaveRecord cave, int year, int compatibility, int contenderCount, bool advanceRealm = true)
    {
        NormalizeCaveUses(cave);
        MclslGeneratedItemRecord essence = MclslGeneratedObjectFactory.CreateHeavenEarthEssence(actor, cave, year, compatibility);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.NascentCaveId, cave.Id);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.NascentCaveName, cave.Name);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.NascentCaveTags, cave.LawTags);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.NascentCaveCompatibility, compatibility);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.NascentCaveIntegrity, cave.Integrity);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.NascentEssenceId, essence.Id);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.NascentEssenceName, essence.Name);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.NascentEssenceQuality, essence.Quality);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.NascentEssenceTags, essence.LawTags);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.NascentEssenceDescription, essence.Description);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.NascentEssenceEffects, essence.AttributeText);

        cave.RemainingEssence = Math.Max(0, cave.RemainingEssence - 1);
        cave.RefinedCount++;
        cave.LastRefinedByActorId = MclslActorAccessor.Id(actor);
        cave.LastRefinedByActorName = MclslActorAccessor.DisplayName(actor, MclslRealmIds.YuanYing);
        cave.Integrity = Math.Max(0, cave.Integrity - (18 + cave.Quality * 6));
        cave.State = cave.RemainingEssence > 0 && cave.Integrity > 20 ? "衰退" : "枯竭";

        if (advanceRealm)
        {
            MclslCultivationSystem.SetRealm(actor, MclslRealmIds.YuanYing, year, "炼化“" + cave.Name + "”中的“" + essence.Name + "”，夺天地之精以成元婴");
            MclslResourceSystem.GrantCaveRefinementReward(actor, cave.Quality, compatibility, contenderCount);
            string contest = contenderCount > 1 ? "力压" + (contenderCount - 1) + "名同道后，" : string.Empty;
            string relation = MclslLawInteractionCatalog.Detail(
                MclslGeneratedObjectFactory.SplitTags(MclslActorAccessor.GetString(actor, MclslActorDataKeys.GoldenCoreLaws, string.Empty)),
                MclslGeneratedObjectFactory.SplitTags(cave.LawTags));
            string displayName = MclslActorAccessor.DisplayName(actor, MclslRealmIds.YuanYing);
            string essenceState = cave.RemainingEssence > 0 ? "余精尚存" + cave.RemainingEssence + "份" : "余精已尽";
            MclslWorldRunRepository.AddEvent(year, "nascent_soul", displayName + "成就元婴", contest + "于“" + cave.Name + "”炼得天地之精“" + essence.Name + "”；" + relation + "。" + essenceState + "，完整度" + cave.Integrity + "%（" + cave.State + "）。", actor);
            if (MclslRuntimeSettings.NascentBreakthroughAnnouncementsEnabled)
                MclslAnnouncementSystem.Enqueue(displayName + "炼得天地之精“" + essence.Name + "”，成就元婴。", "#79B6B0", 9f, 1);
        }
    }

    private static void GenerateCave(int year, bool announce)
    {
        MclslWorldRunState run = MclslWorldRunRepository.Current;
        int sequence = MclslWorldRunRepository.NextProceduralSequence();
        int hash = PositiveHash(run.RunId + "|cave_profile|" + sequence + "|" + year);
        MclslTechniqueDefinition technique = MclslCultivationCatalog.Techniques[hash % MclslCultivationCatalog.Techniques.Count];
        int qualityRoll = PositiveHash(hash + "|quality") % 100;
        int quality = qualityRoll < 8 ? 4 : qualityRoll < 30 ? 3 : qualityRoll < 70 ? 2 : 1;
        int tagCount = quality >= 3 ? 3 : 2;
        List<string> tags = new();
        int start = PositiveHash(hash + "|tag") % technique.LawPool.Length;
        for (int i = 0; i < technique.LawPool.Length && tags.Count < tagCount; i++)
            if (!tags.Contains(technique.LawPool[(start + i) % technique.LawPool.Length])) tags.Add(technique.LawPool[(start + i) % technique.LawPool.Length]);
        if (quality == 4 && PositiveHash(hash + "|rare") % 100 < 35 && !tags.Contains("毁灭")) tags[tags.Count - 1] = "毁灭";
        LocationSeed location = PickLocation(hash);
        MclslWorldCaveRecord cave = MclslGeneratedObjectFactory.CreateWorldCave(year, sequence, location.Location, location.Kingdom, tags, quality, location.X, location.Y);
        run.WorldCaves.Add(cave);
        if (announce)
        {
            MclslWorldRunRepository.AddEvent(year, "cave_born", cave.Name + "显化", cave.Description + "洞天内孕有" + cave.RemainingEssence + "份天地之精。其存在不改变当地原生国家归属。", cave.MapX, cave.MapY, cave.LocationName, cave.NativeKingdomName);
            if (MclslRuntimeSettings.ResourceBirthAnnouncementsEnabled) MclslAnnouncementSystem.Enqueue("洞天“" + cave.Name + "”于" + cave.LocationName + "显化。", "#79B6B0", 8f, 1);
        }
        MclslWorldArchiveStore.MarkDirty();
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

    private static int Compatibility(IReadOnlyList<string> laws, IReadOnlyList<string> caveTags)
    {
        return MclslLawInteractionCatalog.CompatibilityScore(laws, caveTags);
    }

    private static int Compatibility(Actor actor, IReadOnlyList<string> laws, IReadOnlyList<string> caveTags)
    {
        return Math.Clamp(Compatibility(laws, caveTags) + MclslSpiritualRootSystem.LawHarmonyBonus(actor), 0, 100);
    }

    private static int CountAvailableCaves(MclslWorldRunState run)
    {
        if (run?.WorldCaves == null) return 0;
        int count = 0;
        for (int i = 0; i < run.WorldCaves.Count; i++)
            if (IsAvailable(run.WorldCaves[i])) count++;
        return count;
    }

    private static CaveClaim PickWinningClaim(List<CaveClaim> claims)
    {
        if (claims == null || claims.Count == 0) return null;
        CaveClaim best = null;
        long bestId = long.MaxValue;
        for (int i = 0; i < claims.Count; i++)
        {
            CaveClaim claim = claims[i];
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

    private static int CountValidClaims(List<CaveClaim> claims)
    {
        if (claims == null) return 0;
        int count = 0;
        for (int i = 0; i < claims.Count; i++)
            if (IsValidClaim(claims[i])) count++;
        return count;
    }

    private static bool IsValidClaim(CaveClaim claim)
    {
        return claim != null && MclslActorAccessor.Alive(claim.Actor) && MclslActorAccessor.Realm(claim.Actor) == MclslRealmIds.JinDan;
    }

    private static MclslWorldCaveRecord PickBestAvailableCave(Actor actor, IReadOnlyList<string> laws)
    {
        MclslWorldRunState run = MclslWorldRunRepository.Current;
        if (run?.WorldCaves == null) return null;
        MclslWorldCaveRecord best = null;
        int bestCompatibility = int.MinValue;
        for (int i = 0; i < run.WorldCaves.Count; i++)
        {
            MclslWorldCaveRecord cave = run.WorldCaves[i];
            if (!IsAvailable(cave)) continue;
            int compatibility = Compatibility(actor, laws, MclslGeneratedObjectFactory.SplitTags(cave.LawTags));
            if (compatibility <= bestCompatibility) continue;
            best = cave;
            bestCompatibility = compatibility;
        }
        return best;
    }

    private static bool IsAvailable(MclslWorldCaveRecord cave)
    {
        NormalizeCaveUses(cave);
        return cave != null && cave.RemainingEssence > 0 && cave.Integrity > 20 && !string.Equals(cave.State, "枯竭", StringComparison.Ordinal);
    }

    private static void NormalizeCaveUses(MclslWorldCaveRecord cave)
    {
        if (cave == null) return;
        int capacity = MclslInverseTruthSystem.IsTruthReversed("truth_player_trace_persistence") ? 2 : 1;
        int used = Math.Max(0, cave.RefinedCount);
        cave.EssenceCapacity = capacity;
        int expectedRemaining = Math.Max(0, capacity - used);
        cave.RemainingEssence = Math.Clamp(cave.RemainingEssence, 0, capacity);
        if (cave.RemainingEssence < expectedRemaining && cave.Integrity > 20)
            cave.RemainingEssence = expectedRemaining;
        if (cave.RemainingEssence > 0 && cave.Integrity > 20 && string.Equals(cave.State, "枯竭", StringComparison.Ordinal))
            cave.State = used > 0 ? "衰退" : "活跃";
    }

    private static int PositiveHash(string value)
    {
        unchecked { int result = 37; foreach (char c in value ?? string.Empty) result = result * 43 + c; return result & int.MaxValue; }
    }
}
