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

    internal static MclslTechniqueDefinition SelectStartingTechnique(Actor actor, string seed, int aptitude, bool ancient)
    {
        MclslTechniqueDefinition[] pool = MclslCultivationCatalog.StartingTechniquePool(seed, aptitude, ancient);
        if (pool.Length == 0) return SelectStartingTechnique(seed, aptitude, ancient);
        int total = 0;
        for (int i = 0; i < pool.Length; i++) total += MclslSpiritualRootSystem.TechniqueWeight(actor, pool[i]);
        int roll = StableHash((seed ?? string.Empty) + "|root_soft_match") % total;
        MclslTechniqueDefinition selected = pool[pool.Length - 1];
        for (int i = 0; i < pool.Length; i++)
        {
            roll -= MclslSpiritualRootSystem.TechniqueWeight(actor, pool[i]);
            if (roll >= 0) continue;
            selected = pool[i];
            break;
        }
        return ancient ? selected : PickLeastCrowdedVariant(selected, seed);
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
        double bestTie = double.MaxValue;

        IReadOnlyList<MclslTechniqueDefinition> pool = GetAnnualTechniquePool(year, realmIndex, aptitude);
        for (int i = 0; i < pool.Count; i++)
            ConsiderTechnique(actor, pool[i], seed + "|pool_tie|" + i, ref best, ref bestCount, ref bestTie);

        if (best == null) return false;
        if (best.Id == oldId && CountFor(oldId) <= 1) return true;
        string oldName = MclslActorAccessor.GetString(actor, MclslActorDataKeys.TechniqueName, string.Empty);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.TechniqueId, best.Id);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.TechniqueName, best.Name);
        MclslTechniqueRealmLimit.OnTechniqueAssigned(actor, best);
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

    private static double WeightedTie(Actor actor, MclslTechniqueDefinition candidate, string seed)
    {
        double sample = (StableHash(seed + "|" + candidate.Id) + 1d) / 2147483648d;
        return -Math.Log(sample) / MclslSpiritualRootSystem.TechniqueWeight(actor, candidate);
    }

    private static void ConsiderTechnique(Actor actor, MclslTechniqueDefinition candidate, string seed,
        ref MclslTechniqueDefinition best, ref int bestCount, ref double bestTie)
    {
        if (candidate == null || string.IsNullOrWhiteSpace(candidate.Id)) return;
        int count = CountFor(candidate.Id);
        double tie = WeightedTie(actor, candidate, seed);
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
            if (MclslActorRegistry.Resolve(id, out Actor candidate) && MclslActorAccessor.Alive(candidate)) return candidate;
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
            if (id == actorId || !MclslActorRegistry.Resolve(id, out Actor candidate) || !MclslActorAccessor.Alive(candidate)) continue;
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
        int contributionCost = MclslEconomicPolicy.ContributionPrice(cost);
        long stones = Math.Max(0, MclslActorAccessor.GetMoney(actor, MclslActorDataKeys.SpiritStones, 0));
        long contribution = Math.Max(0, MclslActorAccessor.GetMoney(actor, MclslActorDataKeys.Contribution, 0));
        if (stones < cost && contribution < contributionCost) return false;

        MclslTechniqueDefinition best = null;
        int bestCount = int.MaxValue;
        double tie = double.MaxValue;
        int aptitude = Math.Clamp(MclslActorAccessor.GetInt(actor, MclslActorDataKeys.Aptitude, 50), 1, 100);
        IReadOnlyList<MclslTechniqueDefinition> pool = GetAnnualTechniquePool(year, realmIndex, aptitude);
        for (int i = 0; i < pool.Count; i++)
        {
            MclslTechniqueDefinition candidate = pool[i];
            if (candidate == null || candidate.Id == oldTechniqueId) continue;
            int candidateCount = CountFor(candidate.Id);
            double candidateTie = WeightedTie(actor, candidate, MclslActorAccessor.Id(actor) + "|alternative_method|" + year);
            if (candidateCount < bestCount || (candidateCount == bestCount && candidateTie < tie))
            {
                best = candidate;
                bestCount = candidateCount;
                tie = candidateTie;
            }
        }

        // 改修后必须确实减少同修压力，否则不做无意义转法。
        if (best == null || bestCount + 1 >= oldCount) return false;
        string paymentKey = stones >= cost ? MclslActorDataKeys.SpiritStones : MclslActorDataKeys.Contribution;
        long payment = stones >= cost ? cost : contributionCost;
        string paymentSource = "technique-change/" + MclslEconomyCommands.Account(actor);
        if (MclslEconomyCommands.WasApplied(paymentSource, year)) return false;
        MclslTechniqueMutationSnapshot snapshot = MclslTechniqueMutationSnapshot.Capture(actor);
        try
        {
            MclslActorAccessor.Set(actor, MclslActorDataKeys.TechniqueId, best.Id);
            MclslActorAccessor.Set(actor, MclslActorDataKeys.TechniqueName, best.Name);
            MclslTechniqueRealmLimit.OnTechniqueAssigned(actor, best);
            MclslTechniqueStageSystem.AddProgress(actor, -12);
            bool ancientLaw = MclslActorAccessor.GetString(actor, MclslActorDataKeys.CultivationSystem, string.Empty)
                == MclslCultivationSystemIds.AncientLaw;
            MclslCultivationGrowthSystem.ApplyProgressSetback(actor, MclslActorAccessor.Realm(actor), 10f, ancientLaw);
            if (!MclslEconomyCommands.TryConsume(actor, paymentKey, payment,
                paymentSource, year))
                throw new InvalidOperationException("改修功法资金提交失败");
        }
        catch (Exception ex)
        {
            snapshot.Restore(actor);
            MclslDiagnostics.Error("technique-change", ex.Message);
            return false;
        }
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

    internal static void Forget(long actorId) => Unregister(actorId);

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

        PractitionerCounts[techniqueId] = PractitionerCounts.TryGetValue(techniqueId, out int count) ? count + 1 : 1;
        if (!PractitionerIdsByTechnique.TryGetValue(techniqueId, out List<long> ids)) PractitionerIdsByTechnique[techniqueId] = ids = new List<long>();
        ids.Add(actorId);
    }

    private static void Unregister(long actorId)
    {
        if (actorId <= 0L || !RegisteredTechniqueByActor.TryGetValue(actorId, out string techniqueId)) return;
        RegisteredTechniqueByActor.Remove(actorId);

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
