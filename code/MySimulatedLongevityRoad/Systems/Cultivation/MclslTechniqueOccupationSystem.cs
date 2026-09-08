using System;
using System.Collections.Generic;
using MySimulatedLongevityRoad.Core;
using MySimulatedLongevityRoad.Data;
using MySimulatedLongevityRoad.Systems.Death;

namespace MySimulatedLongevityRoad.Systems;

internal static class MclslTechniqueOccupationSystem
{
    private static readonly Dictionary<string, int> PractitionerCounts = new(StringComparer.Ordinal);
    private static readonly Dictionary<long, string> RegisteredTechniqueByActor = new();
    private static readonly Dictionary<long, Actor> RegisteredActors = new();
    private static readonly Dictionary<string, List<long>> PractitionerIdsByTechnique = new(StringComparer.Ordinal);
    private static readonly HashSet<string> ResolvedTechniquesThisYear = new(StringComparer.Ordinal);

    // 每部固定母法提供六十四个真实异法版本（含母法本身）。只改变功法ID与名称，
    // 法则、境界上限继续复用母法，不建立额外实体或年度生成器。
    private static readonly Dictionary<int, List<MclslTechniqueDefinition>> AnnualTechniquePools = new();
    private static int _techniquePoolYear = -1;
    private const int VariantsPerBaseTechnique = 64;
    private const int AnnualResolutionBudget = 8;
    private const int AnnualStruggleBudget = 5;
    private static int _resolutionBudgetYear = -1;
    private static int _annualResolutionCount;
    private static int _annualStruggleCount;

    internal static void Rebuild() => Rebuild(MclslCultivatorCandidateIndex.GetCultivatorActorsSnapshot());

    internal static void Rebuild(IReadOnlyList<Actor> units)
    {
        Clear();
        if (units == null) return;
        DistributeLegacyNewLawTechniques(units);
        Clear();
        for (int i = 0; i < units.Count; i++)
        {
            Actor actor = units[i];
            if (!MclslActorAccessor.Alive(actor) || !MclslActorAccessor.IsCultivator(actor)) continue;
            Register(actor, TechniqueId(actor));
        }
    }

    /// <summary>
    /// 旧版本只给大量新法修士分配固定母法，导致数百人挤在同一功法。
    /// 在既有功法索引重建时只迁移一次，把这些角色稳定分摊到同源异法中。
    /// 不处理旧法修士，也不额外扫描世界。
    /// </summary>
    private static void DistributeLegacyNewLawTechniques(IReadOnlyList<Actor> units)
    {
        MclslWorldRunState run = MclslWorldRunRepository.Current;
        const string migrationKey = "migration_newlaw_derived_methods_v1";
        if (run?.LawConflictEnabled != true || run.FiredHistoricalEvents?.Contains(migrationKey) == true) return;

        Dictionary<string, List<Actor>> groups = new(StringComparer.Ordinal);
        for (int i = 0; i < units.Count; i++)
        {
            Actor actor = units[i];
            if (!MclslActorAccessor.Alive(actor) || !MclslActorAccessor.IsCultivator(actor)) continue;
            if (MclslActorAccessor.GetString(actor, MclslActorDataKeys.CultivationSystem, string.Empty) != MclslCultivationSystemIds.NewLaw) continue;
            string id = TechniqueId(actor);
            if (!MclslCultivationCatalog.TryTechnique(id, out MclslTechniqueDefinition definition) || definition == null || definition.Id != id) continue;
            if (!groups.TryGetValue(id, out List<Actor> group)) groups[id] = group = new List<Actor>();
            group.Add(actor);
        }

        bool changed = false;
        foreach (KeyValuePair<string, List<Actor>> pair in groups)
        {
            List<Actor> group = pair.Value;
            if (group == null || group.Count <= 1) continue;
            group.Sort((a, b) => MclslActorAccessor.Id(a).CompareTo(MclslActorAccessor.Id(b)));
            MclslTechniqueDefinition baseTechnique = MclslCultivationCatalog.Technique(pair.Key);
            int start = StableHash((run.RunId ?? string.Empty) + "|legacy_method_distribution|" + pair.Key) % VariantsPerBaseTechnique;
            for (int i = 0; i < group.Count; i++)
            {
                int variant = (start + i) % VariantsPerBaseTechnique;
                MclslTechniqueDefinition assigned = VariantDefinition(baseTechnique, variant);
                Actor actor = group[i];
                if (assigned == null || TechniqueId(actor) == assigned.Id) continue;
                MclslActorAccessor.Set(actor, MclslActorDataKeys.TechniqueId, assigned.Id);
                MclslActorAccessor.Set(actor, MclslActorDataKeys.TechniqueName, assigned.Name);
                MclslTechniqueRealmLimit.EnsureFromDefinition(actor, assigned);
                changed = true;
            }
        }

        run.FiredHistoricalEvents ??= new List<string>();
        run.FiredHistoricalEvents.Add(migrationKey);
        if (changed)
            MclslWorldRunRepository.AddEvent(MclslRuntime.CurrentYear(), "newlaw_method_diversification", "新法诸功分流",
                "旧有固定功法记录已按修士差异分化为多部同源异法，仙法不可同修的计数由此恢复正常。");
        MclslWorldArchiveStore.MarkDirty();
    }

    internal static float CultivationMultiplier(Actor actor)
    {
        if (MclslWorldRunRepository.Current?.LawConflictEnabled != true) return 1f;
        if (!MclslActorAccessor.Alive(actor) || !MclslActorAccessor.IsCultivator(actor)) return 1f;
        int count = CountFor(TechniqueId(actor));
        return count <= 1 ? 1f : 1f / count;
    }

    internal static string ConflictDisplayText(Actor actor)
    {
        if (MclslWorldRunRepository.Current?.LawConflictEnabled != true) return string.Empty;
        if (!MclslActorAccessor.Alive(actor) || !MclslActorAccessor.IsCultivator(actor)) return string.Empty;
        int count = CountFor(TechniqueId(actor));
        if (count <= 1) return "同法修士：仅此一人";
        int efficiency = Math.Max(1, (int)Math.Round(100f / count));
        return "仙法不可同修：同修" + count + "人，修炼效率" + efficiency + "%";
    }

    internal static int CountFor(string techniqueId)
    {
        string normalized = MclslCultivationCatalog.NormalizeTechniqueId(techniqueId);
        return string.IsNullOrWhiteSpace(normalized) ? 0 : PractitionerCounts.TryGetValue(normalized, out int count) ? count : 0;
    }

    /// <summary>
    /// 新法入门时从母法的六十四个真实异法版本中选择当前同修最少者。
    /// 只在角色获得功法时扫描十二项，不增加年度或帧级全图开销。
    /// </summary>
    internal static MclslTechniqueDefinition SelectStartingTechnique(string seed, int aptitude, bool ancient)
    {
        MclslTechniqueDefinition baseTechnique = MclslCultivationCatalog.StartingTechnique(seed, aptitude, ancient);
        if (ancient || baseTechnique == null) return baseTechnique;
        return PickLeastCrowdedVariant(baseTechnique, seed);
    }

    internal static MclslTechniqueDefinition SelectTechniqueVariant(MclslTechniqueDefinition technique, string seed)
    {
        if (technique == null) return null;
        if (technique.Id.StartsWith("legacy_", StringComparison.Ordinal)) return technique;
        MclslTechniqueDefinition baseTechnique = MclslCultivationCatalog.Technique(MclslCultivationCatalog.BaseTechniqueId(technique.Id));
        return PickLeastCrowdedVariant(baseTechnique, seed);
    }

    internal static bool AssignLeastCrowdedTechniqueForRealm(Actor actor, int year, string realm, bool preserveProgress)
    {
        if (actor?.data == null) return false;
        int realmIndex = Math.Max(0, MclslRealmIds.Index(realm));
        int aptitude = Math.Clamp(MclslActorAccessor.GetInt(actor, MclslActorDataKeys.Aptitude, 50), 1, 100);
        string seed = MclslActorAccessor.Id(actor) + "|seek_method|" + year + "|" + realm;
        string oldId = TechniqueId(actor);
        MclslTechniqueDefinition best = null;
        int bestCount = int.MaxValue;
        int bestTie = int.MaxValue;

        IReadOnlyList<MclslTechniqueDefinition> pool = GetAnnualTechniquePool(year, realmIndex, aptitude);
        for (int i = 0; i < pool.Count; i++)
            ConsiderTechnique(pool[i], seed + "|pool_tie|" + i, ref best, ref bestCount, ref bestTie);

        if (best == null) return false;
        if (best.Id == oldId && CountFor(oldId) <= 1) return true;
        string oldName = MclslActorAccessor.GetString(actor, MclslActorDataKeys.TechniqueName, string.Empty);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.TechniqueId, best.Id);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.TechniqueName, best.Name);
        MclslTechniqueRealmLimit.EnsureFromDefinition(actor, best);
        if (!preserveProgress)
        {
            MclslTechniqueStageSystem.AddProgress(actor, -8);
            bool ancientLaw = MclslActorAccessor.GetString(actor, MclslActorDataKeys.CultivationSystem, string.Empty)
                == MclslCultivationSystemIds.AncientLaw;
            MclslCultivationGrowthSystem.ApplyProgressSetback(actor, realm, 6f, ancientLaw);
        }
        if (!string.Equals(oldId, best.Id, StringComparison.Ordinal))
            MclslWorldRunRepository.AddEvent(year, "law_conflict_change_method", MclslActorAccessor.DisplayName(actor) + "另寻功法",
                (string.IsNullOrWhiteSpace(oldName) ? "原法" : "《" + oldName + "》") + "同修者渐众，其转修《" + best.Name + "》以避仙法不可同修。", actor);
        return true;
    }

    private static void ConsiderTechnique(MclslTechniqueDefinition candidate, string seed,
        ref MclslTechniqueDefinition best, ref int bestCount, ref int bestTie)
    {
        if (candidate == null || string.IsNullOrWhiteSpace(candidate.Id)) return;
        int count = CountFor(candidate.Id);
        int tie = StableHash(seed + "|" + candidate.Id);
        if (best == null || count < bestCount || (count == bestCount && tie < bestTie))
        {
            best = candidate;
            bestCount = count;
            bestTie = tie;
        }
    }

    internal static void TryResolveConflictAnnual(Actor actor, int year)
    {
        if (MclslWorldRunRepository.Current?.LawConflictEnabled != true || !MclslActorAccessor.Alive(actor) || !MclslActorAccessor.IsCultivator(actor)) return;
        string techniqueId = TechniqueId(actor);
        int count = CountFor(techniqueId);
        if (count <= 1 || string.IsNullOrWhiteSpace(techniqueId)) return;

        EnsureAnnualResolutionBudget(year);
        if (_annualResolutionCount >= AnnualResolutionBudget || !ResolvedTechniquesThisYear.Add(techniqueId)) return;

        // 世界级小额抽样：每年只处理少量受困功法。
        if (StableHash(techniqueId + "|annual_resolution_group|" + year) % 1000 >= 350) return;
        Actor resolver = PickAnnualResolver(techniqueId, year);
        if (!MclslActorAccessor.Alive(resolver)) return;
        _annualResolutionCount++;

        int realmIndex = Math.Max(0, MclslRealmIds.Index(MclslActorAccessor.Realm(resolver)));
        int aptitude = Math.Clamp(MclslActorAccessor.GetInt(resolver, MclslActorDataKeys.Aptitude, 50), 1, 100);
        int insight = Math.Max(0, MclslActorAccessor.GetInt(resolver, MclslActorDataKeys.TechniqueInsight, 0));
        int seekChance = Math.Clamp(18 + count * 3 + aptitude / 5 + insight / 8 - realmIndex * 9, 8, 72);
        if (realmIndex <= MclslRealmIds.Index(MclslRealmIds.ZhuJi) || insight >= 55)
        {
            int roll = StableHash(MclslActorAccessor.Id(resolver) + "|seek_other_method|" + year) % 100;
            if (roll < seekChance && TryChangeToLessCrowdedTechnique(resolver, year, realmIndex, techniqueId, count)) return;
        }

        if (_annualStruggleCount >= AnnualStruggleBudget) return;
        int struggleChance = Math.Clamp(8 + count * 4 + realmIndex * 7, 12, 70);
        if (StableHash(MclslActorAccessor.Id(resolver) + "|seek_same_method_rival|" + year) % 100 >= struggleChance) return;
        Actor rival = PickNearbyRival(resolver, techniqueId, year);
        if (!MclslActorAccessor.Alive(rival)) return;
        try
        {
            resolver.setAttackTarget(rival);
            resolver.beh_actor_target = rival;
            _annualStruggleCount++;
            MclslWorldRunRepository.AddEvent(year, "law_conflict_hunt", MclslActorAccessor.DisplayName(resolver) + "寻同法者论生死",
                MclslActorAccessor.DisplayName(resolver) + "久受《" + MclslActorAccessor.GetString(resolver, MclslActorDataKeys.TechniqueName, "无名功法") + "》同修所阻，遂向附近的" + MclslActorAccessor.DisplayName(rival) + "发起法争。", resolver);
        }
        catch { }
    }

    private static void EnsureAnnualResolutionBudget(int year)
    {
        if (_resolutionBudgetYear == year) return;
        _resolutionBudgetYear = year;
        _annualResolutionCount = 0;
        _annualStruggleCount = 0;
        ResolvedTechniquesThisYear.Clear();
    }

    private static Actor PickAnnualResolver(string techniqueId, int year)
    {
        if (!PractitionerIdsByTechnique.TryGetValue(techniqueId, out List<long> ids) || ids == null || ids.Count == 0) return null;
        int start = StableHash(techniqueId + "|annual_resolver|" + year) % ids.Count;
        int checks = Math.Min(8, ids.Count);
        for (int i = 0; i < checks; i++)
        {
            long id = ids[(start + i) % ids.Count];
            if (RegisteredActors.TryGetValue(id, out Actor candidate) && MclslActorAccessor.Alive(candidate)) return candidate;
        }
        return null;
    }

    private static Actor PickNearbyRival(Actor actor, string techniqueId, int year)
    {
        if (!PractitionerIdsByTechnique.TryGetValue(techniqueId, out List<long> ids) || ids == null || ids.Count < 2) return null;
        long actorId = MclslActorAccessor.Id(actor);
        int start = StableHash(actorId + "|nearby_rival|" + year) % ids.Count;
        int checks = Math.Min(8, ids.Count);
        int ax = actor?.data?.x ?? 0;
        int ay = actor?.data?.y ?? 0;
        for (int i = 0; i < checks; i++)
        {
            long id = ids[(start + i) % ids.Count];
            if (id == actorId || !RegisteredActors.TryGetValue(id, out Actor candidate) || !MclslActorAccessor.Alive(candidate)) continue;
            bool sameCity = actor.city != null && actor.city == candidate.city;
            int dx = (candidate.data?.x ?? 0) - ax;
            int dy = (candidate.data?.y ?? 0) - ay;
            if (sameCity || dx * dx + dy * dy <= 96 * 96) return candidate;
        }
        return null;
    }

    private static bool TryChangeToLessCrowdedTechnique(Actor actor, int year, int realmIndex, string oldTechniqueId, int oldCount)
    {
        string oldTechniqueName = MclslActorAccessor.GetString(actor, MclslActorDataKeys.TechniqueName, "无名功法");
        int cost = 10 + realmIndex * 15;
        int stones = Math.Max(0, MclslActorAccessor.GetInt(actor, MclslActorDataKeys.SpiritStones, 0));
        int contribution = Math.Max(0, MclslActorAccessor.GetInt(actor, MclslActorDataKeys.Contribution, 0));
        if (stones < cost && contribution < cost) return false;

        MclslTechniqueDefinition best = null;
        int bestCount = int.MaxValue;
        int tie = int.MaxValue;
        int aptitude = Math.Clamp(MclslActorAccessor.GetInt(actor, MclslActorDataKeys.Aptitude, 50), 1, 100);
        IReadOnlyList<MclslTechniqueDefinition> pool = GetAnnualTechniquePool(year, realmIndex, aptitude);
        for (int i = 0; i < pool.Count; i++)
        {
            MclslTechniqueDefinition candidate = pool[i];
            if (candidate == null || candidate.Id == oldTechniqueId) continue;
            int candidateCount = CountFor(candidate.Id);
            int candidateTie = StableHash(MclslActorAccessor.Id(actor) + "|alternative_method|" + year + "|" + candidate.Id);
            if (candidateCount < bestCount || (candidateCount == bestCount && candidateTie < tie))
            {
                best = candidate;
                bestCount = candidateCount;
                tie = candidateTie;
            }
        }

        // 改修后必须确实减少同修压力，否则不做无意义转法。
        if (best == null || bestCount + 1 >= oldCount) return false;
        if (stones >= cost) MclslActorAccessor.Set(actor, MclslActorDataKeys.SpiritStones, stones - cost);
        else MclslActorAccessor.Set(actor, MclslActorDataKeys.Contribution, contribution - cost);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.TechniqueId, best.Id);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.TechniqueName, best.Name);
        MclslTechniqueRealmLimit.EnsureFromDefinition(actor, best);
        MclslTechniqueStageSystem.AddProgress(actor, -12);
        bool ancientLaw = MclslActorAccessor.GetString(actor, MclslActorDataKeys.CultivationSystem, string.Empty)
            == MclslCultivationSystemIds.AncientLaw;
        MclslCultivationGrowthSystem.ApplyProgressSetback(actor, MclslActorAccessor.Realm(actor), 10f, ancientLaw);
        MclslWorldRunRepository.AddEvent(year, "law_conflict_change_method", MclslActorAccessor.DisplayName(actor) + "改修避争",
            "《" + oldTechniqueName + "》原有同修" + oldCount + "人，其另寻《" + best.Name + "》改修，改修后同法压力降至" + (bestCount + 1) + "人。基础修为有所折损。", actor);
        return true;
    }

    private static IReadOnlyList<MclslTechniqueDefinition> GetAnnualTechniquePool(int year, int realmIndex, int aptitude)
    {
        if (_techniquePoolYear != year)
        {
            _techniquePoolYear = year;
            AnnualTechniquePools.Clear();
        }
        int aptitudeBand = Math.Clamp(aptitude / 20, 0, 5);
        int cacheKey = realmIndex * 10 + aptitudeBand;
        if (AnnualTechniquePools.TryGetValue(cacheKey, out List<MclslTechniqueDefinition> cached)) return cached;

        List<MclslTechniqueDefinition> pool = new(192);
        HashSet<string> ids = new(StringComparer.Ordinal);
        for (int i = 0; i < MclslCultivationCatalog.Techniques.Count; i++)
        {
            MclslTechniqueDefinition baseDefinition = MclslCultivationCatalog.Techniques[i];
            if (baseDefinition == null || MclslRealmIds.Index(baseDefinition.MaxRealm) < realmIndex) continue;
            List<MclslTechniqueDefinition> bestVariants = new(4);
            for (int variant = 0; variant < VariantsPerBaseTechnique; variant++)
            {
                MclslTechniqueDefinition candidate = VariantDefinition(baseDefinition, variant);
                int insertAt = bestVariants.Count;
                int candidateCount = CountFor(candidate.Id);
                for (int j = 0; j < bestVariants.Count; j++)
                {
                    MclslTechniqueDefinition existing = bestVariants[j];
                    int existingCount = CountFor(existing.Id);
                    if (candidateCount < existingCount || (candidateCount == existingCount && string.CompareOrdinal(candidate.Id, existing.Id) < 0))
                    {
                        insertAt = j;
                        break;
                    }
                }
                bestVariants.Insert(insertAt, candidate);
                if (bestVariants.Count > 4) bestVariants.RemoveAt(bestVariants.Count - 1);
            }
            for (int j = 0; j < bestVariants.Count; j++)
                if (ids.Add(bestVariants[j].Id)) pool.Add(bestVariants[j]);
        }

        string inheritedSeed = (MclslWorldRunRepository.Current?.RunId ?? string.Empty) + "|annual_method_pool|" + year + "|" + realmIndex;
        for (int i = 0; i < 24; i++)
        {
            if (!MclslTechniqueLineageSystem.TryPickInheritedTechnique(year, inheritedSeed + "|" + i, aptitude, out MclslTechniqueDefinition inherited)
                || inherited == null || MclslRealmIds.Index(inherited.MaxRealm) < realmIndex || !ids.Add(inherited.Id)) continue;
            pool.Add(inherited);
        }
        AnnualTechniquePools[cacheKey] = pool;
        return pool;
    }

    private static MclslTechniqueDefinition PickLeastCrowdedVariant(MclslTechniqueDefinition baseTechnique, string seed)
    {
        if (baseTechnique == null) return null;
        MclslTechniqueDefinition best = null;
        int bestCount = int.MaxValue;
        int start = StableHash((seed ?? string.Empty) + "|variant_start|" + baseTechnique.Id) % VariantsPerBaseTechnique;
        for (int offset = 0; offset < VariantsPerBaseTechnique; offset++)
        {
            int variant = (start + offset) % VariantsPerBaseTechnique;
            MclslTechniqueDefinition candidate = VariantDefinition(baseTechnique, variant);
            int count = CountFor(candidate.Id);
            if (best == null || count < bestCount)
            {
                best = candidate;
                bestCount = count;
            }
        }
        return best ?? baseTechnique;
    }

    private static MclslTechniqueDefinition VariantDefinition(MclslTechniqueDefinition baseTechnique, int variant)
    {
        if (baseTechnique == null || variant <= 0) return baseTechnique;
        int index = Math.Clamp(variant, 1, VariantsPerBaseTechnique - 1);
        int seed = StableHash(baseTechnique.Id + "|derived_method|" + index);
        string name = MclslProceduralLexicon.TechniqueName(baseTechnique.LawPool, seed);
        return new MclslTechniqueDefinition
        {
            Id = baseTechnique.Id + "_derived_" + index,
            Name = name,
            LawPool = baseTechnique.LawPool,
            MaxRealm = baseTechnique.MaxRealm
        };
    }

    internal static void Release(Actor actor)
    {
        if (actor?.data != null) Unregister(MclslActorAccessor.Id(actor));
    }

    internal static void OnTechniqueChanged(Actor actor, string oldTechniqueId, string newTechniqueId) => RefreshRegistration(actor, MclslCultivationCatalog.NormalizeTechniqueId(newTechniqueId));
    internal static void OnCultivationStateChanged(Actor actor) => RefreshRegistration(actor, TechniqueId(actor));

    internal static void RecordSameTechniqueKill(Actor victim, int year, AttackType attackType)
    {
        if (!MclslDeathSystem.IsCombatDeath(attackType)) return;
        if (MclslWorldRunRepository.Current?.LawConflictEnabled != true || victim?.data == null) return;
        Actor killer;
        try { killer = victim.attackedBy as Actor; }
        catch { killer = null; }
        if (!MclslActorAccessor.Alive(killer) || killer == victim || !MclslActorAccessor.IsCultivator(killer)) return;
        string victimTechnique = TechniqueId(victim);
        string killerTechnique = TechniqueId(killer);
        if (string.IsNullOrWhiteSpace(victimTechnique) || victimTechnique != killerTechnique) return;
        string techniqueName = MclslActorAccessor.GetString(victim, MclslActorDataKeys.TechniqueName, "无名功法");
        string killerName = MclslActorAccessor.DisplayName(killer);
        string victimName = MclslActorAccessor.DisplayName(victim);
        MclslWorldRunRepository.AddEvent(year, "law_conflict_death", killerName + "斩却同法阻道者",
            killerName + "斩杀同修《" + techniqueName + "》的" + victimName + "。阻道者既去，此法余修皆觉前路稍宽。", killer);
    }

    internal static void Clear()
    {
        PractitionerCounts.Clear();
        RegisteredTechniqueByActor.Clear();
        RegisteredActors.Clear();
        PractitionerIdsByTechnique.Clear();
        ResolvedTechniquesThisYear.Clear();
        _resolutionBudgetYear = -1;
        _annualResolutionCount = 0;
        _annualStruggleCount = 0;
        AnnualTechniquePools.Clear();
        _techniquePoolYear = -1;
    }

    private static void RefreshRegistration(Actor actor, string newTechniqueId)
    {
        if (actor?.data == null) return;
        long actorId = MclslActorAccessor.Id(actor);
        if (actorId <= 0L) return;
        if (RegisteredTechniqueByActor.TryGetValue(actorId, out string registeredTechnique))
        {
            if (MclslActorAccessor.Alive(actor) && MclslActorAccessor.IsCultivator(actor) && !string.IsNullOrWhiteSpace(newTechniqueId) && registeredTechnique == newTechniqueId)
            {
                RegisteredActors[actorId] = actor;
                return;
            }
            Unregister(actorId);
        }
        if (MclslActorAccessor.Alive(actor) && MclslActorAccessor.IsCultivator(actor)) Register(actor, newTechniqueId);
    }

    private static void Register(Actor actor, string techniqueId)
    {
        long actorId = MclslActorAccessor.Id(actor);
        if (actorId <= 0L || string.IsNullOrWhiteSpace(techniqueId) || RegisteredTechniqueByActor.ContainsKey(actorId)) return;
        RegisteredTechniqueByActor[actorId] = techniqueId;
        RegisteredActors[actorId] = actor;
        PractitionerCounts[techniqueId] = PractitionerCounts.TryGetValue(techniqueId, out int count) ? count + 1 : 1;
        if (!PractitionerIdsByTechnique.TryGetValue(techniqueId, out List<long> ids)) PractitionerIdsByTechnique[techniqueId] = ids = new List<long>();
        ids.Add(actorId);
    }

    private static void Unregister(long actorId)
    {
        if (actorId <= 0L || !RegisteredTechniqueByActor.TryGetValue(actorId, out string techniqueId)) return;
        RegisteredTechniqueByActor.Remove(actorId);
        RegisteredActors.Remove(actorId);
        Decrement(techniqueId);
        if (PractitionerIdsByTechnique.TryGetValue(techniqueId, out List<long> ids))
        {
            ids.Remove(actorId);
            if (ids.Count == 0) PractitionerIdsByTechnique.Remove(techniqueId);
        }
    }

    private static string TechniqueId(Actor actor) => MclslCultivationCatalog.NormalizeTechniqueId(MclslActorAccessor.GetString(actor, MclslActorDataKeys.TechniqueId, string.Empty));

    private static void Decrement(string techniqueId)
    {
        if (string.IsNullOrWhiteSpace(techniqueId) || !PractitionerCounts.TryGetValue(techniqueId, out int count)) return;
        if (count <= 1) PractitionerCounts.Remove(techniqueId);
        else PractitionerCounts[techniqueId] = count - 1;
    }

    private static int StableHash(string value)
    {
        unchecked
        {
            int hash = 61;
            foreach (char c in value ?? string.Empty) hash = hash * 43 + c;
            return hash & int.MaxValue;
        }
    }
}
