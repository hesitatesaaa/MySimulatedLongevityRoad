using System;
using System.Collections.Generic;
using MySimulatedLongevityRoad.Data;

namespace MySimulatedLongevityRoad.Systems;

internal static class MclslFactionExchangeSystem
{
    private const string WanXian = "wanxian";
    private const string FiveElders = "five_elders";
    private const int ExchangeAttemptIntervalYears = 5;

    internal static bool TryExchangeTechnique(Actor actor, string factionId, int year, int seed, out string summary)
    {
        summary = string.Empty;
        if (actor?.data == null || !MclslWorldEpochSystem.IsNewLawActive(year)) return false;
        factionId = NormalizeFaction(factionId);
        if (string.IsNullOrWhiteSpace(factionId)) return false;

        string affiliation = NormalizeFaction(MclslActorAccessor.GetString(actor, MclslActorDataKeys.FactionAffiliation, string.Empty));
        if (!string.IsNullOrWhiteSpace(affiliation) && affiliation != factionId) return false;

        string currentMax = MclslTechniqueRealmLimit.MaxRealm(actor);
        int currentMaxIndex = MclslRealmIds.Index(currentMax);
        int realmIndex = Math.Max(0, MclslRealmIds.Index(MclslActorAccessor.Realm(actor)));
        int targetIndex = Math.Clamp(Math.Max(currentMaxIndex, realmIndex) + 1, MclslRealmIds.Index(MclslRealmIds.ZhuJi), MclslRealmIds.Index(MclslRealmIds.HeDao));
        if (currentMaxIndex >= MclslRealmIds.Index(MclslRealmIds.HeDao)) return false;

        string targetRealm = MclslRealmIds.Ordered[targetIndex];
        int cost = TechniqueCost(targetRealm, factionId);
        int contribution = MclslActorAccessor.GetInt(actor, MclslActorDataKeys.Contribution, 0);
        if (contribution < cost) return false;
        if (!MclslDetectionGate.TryEnterActorAttempt(actor, "faction", MclslDetectionGate.FactionTechniqueExchange + "." + factionId, year, ExchangeAttemptIntervalYears))
            return false;

        int roll = PositiveHash(MclslActorAccessor.Id(actor) + "|" + factionId + "|exchange|" + year + "|" + seed);
        if (roll % 100 >= 34) return false;

        MclslTechniqueDefinition technique = PickTechnique(targetRealm, factionId, actor, year, roll);
        MclslTechniqueLineageRecord lineage = ResolveExchangeLineage(technique, targetRealm, year, factionId, roll);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.Contribution, Math.Max(0, contribution - cost));
        if (string.IsNullOrWhiteSpace(affiliation))
            MclslActorAccessor.Set(actor, MclslActorDataKeys.FactionAffiliation, factionId);
        ApplyTechniqueLineage(actor, technique, lineage);
        MclslTechniqueStageSystem.AddProgress(actor, 6 + targetIndex * 2);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.TechniqueInsight,
            MclslActorAccessor.GetInt(actor, MclslActorDataKeys.TechniqueInsight, 0) + 5 + targetIndex * 3);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.LastResourceSpendYear, year);

        string factionName = factionId == WanXian ? "万仙盟" : "五老会";
        string lineageName = lineage == null || string.IsNullOrWhiteSpace(lineage.Name) ? technique.Name : lineage.Name;
        summary = MclslActorAccessor.DisplayName(actor) + "耗贡献" + cost + "，自" + factionName + "换得《" + technique.Name + "》，承入《" + lineageName + "》法脉，最高可至" + MclslRealmIds.Display(technique.MaxRealm) + "。";
        MclslActorAccessor.Set(actor, MclslActorDataKeys.LastBreakthroughResult, summary);
        MclslWorldRunRepository.RegisterResourceSpend(new MclslResourceSpendRecord
        {
            Id = "spend_" + year + "_" + MclslWorldRunRepository.NextProceduralSequence(),
            Year = year,
            ActorId = MclslActorAccessor.Id(actor),
            ActorName = MclslActorAccessor.DisplayName(actor),
            RealmName = MclslRealmIds.Display(MclslActorAccessor.Realm(actor)),
            ItemName = factionName + "功法兑换",
            ContributionCost = cost,
            SpiritStoneCost = 0,
            EffectText = "换得《" + technique.Name + "》，承入《" + lineageName + "》，最高可至" + MclslRealmIds.Display(technique.MaxRealm),
            Summary = summary
        });
        return true;
    }

    private static void ApplyTechniqueLineage(Actor actor, MclslTechniqueDefinition technique, MclslTechniqueLineageRecord lineage)
    {
        string techniqueId = MclslCultivationCatalog.NormalizeTechniqueId(technique?.Id ?? string.Empty);
        string techniqueName = technique?.Name ?? string.Empty;
        string maxRealm = string.IsNullOrWhiteSpace(lineage?.MaxRealm) ? (technique?.MaxRealm ?? MclslRealmIds.ZhuJi) : lineage.MaxRealm;

        MclslActorAccessor.Set(actor, MclslActorDataKeys.TechniqueId, techniqueId);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.TechniqueName, techniqueName);
        MclslTechniqueRealmLimit.SetMaxRealm(actor, maxRealm);
    }

    private static MclslTechniqueLineageRecord ResolveExchangeLineage(MclslTechniqueDefinition technique, string targetRealm, int year, string factionId, int roll)
    {
        if (technique == null) return null;
        MclslWorldRunState run = MclslWorldRunRepository.Current;
        run?.TechniqueLineages?.RemoveAll(x => x == null);
        MclslTechniqueLineageRecord existing = FindExchangeLineage(run, technique, targetRealm, roll);
        if (existing != null) return existing;
        return CreateExchangeLineage(run, technique, year, factionId);
    }

    private static MclslTechniqueLineageRecord FindExchangeLineage(MclslWorldRunState run, MclslTechniqueDefinition technique, string targetRealm, int roll)
    {
        if (run?.TechniqueLineages == null || technique == null) return null;
        string sourceId = MclslCultivationCatalog.NormalizeTechniqueId(technique.Id);
        MclslTechniqueLineageRecord best = null;
        int bestScore = int.MaxValue;
        for (int i = 0; i < run.TechniqueLineages.Count; i++)
        {
            MclslTechniqueLineageRecord lineage = run.TechniqueLineages[i];
            if (!IsExchangeCandidate(lineage, sourceId, technique, targetRealm)) continue;
            int score = Math.Max(0, lineage.CurrentPractitioners) * 2;
            if (!string.Equals(lineage.SourceTechniqueId, sourceId, StringComparison.Ordinal)) score += 12;
            score += PositiveHash((lineage.Id ?? string.Empty) + "|exchange_pick|" + roll) % 7;
            if (score < bestScore)
            {
                best = lineage;
                bestScore = score;
            }
        }
        return best;
    }

    private static bool IsExchangeCandidate(MclslTechniqueLineageRecord lineage, string sourceId, MclslTechniqueDefinition technique, string targetRealm)
    {
        if (lineage == null) return false;
        if (!string.Equals(lineage.SystemId, MclslCultivationSystemIds.NewLaw, StringComparison.Ordinal)) return false;
        if (!RealmAtLeast(lineage.MaxRealm, targetRealm)) return false;
        if (string.Equals(lineage.SourceTechniqueId, sourceId, StringComparison.Ordinal)) return true;
        if (string.Equals(lineage.Id, sourceId, StringComparison.Ordinal)) return true;
        return false;
    }

    private static MclslTechniqueLineageRecord CreateExchangeLineage(MclslWorldRunState run, MclslTechniqueDefinition technique, int year, string factionId)
    {
        if (run?.TechniqueLineages == null || technique == null) return null;
        string sourceId = MclslCultivationCatalog.NormalizeTechniqueId(technique.Id);
        string id = UniqueLineageId(run, string.IsNullOrWhiteSpace(sourceId) ? "newlaw_technique" : sourceId);
        string factionName = factionId == WanXian ? "万仙盟" : "五老会";
        MclslTechniqueLineageRecord lineage = new()
        {
            Id = id,
            Name = technique.Name,
            SystemId = MclslCultivationSystemIds.NewLaw,
            LawTags = technique.LawPool == null ? string.Empty : string.Join(",", technique.LawPool),
            MaxRealm = technique.MaxRealm,
            FirstSeenYear = year,
            LastSeenYear = year,
            CurrentPractitioners = 0,
            PeakPractitioners = 0,
            PeakRealm = MclslRealmIds.Mortal,
            FounderName = factionName,
            FounderActorId = 0,
            State = "法脉初传",
            SourceTechniqueId = sourceId,
            BranchRootId = id,
            LifecycleState = "法脉初传",
            LifecycleYear = year,
            Summary = factionName + "收录此法，准许修士以贡献换取传承。"
        };
        run.TechniqueLineages.Add(lineage);
        MclslWorldArchiveStore.MarkDirty();
        return lineage;
    }

    private static string UniqueLineageId(MclslWorldRunState run, string baseId)
    {
        string id = baseId;
        int suffix = 1;
        while (LineageIdExists(run, id))
        {
            suffix++;
            id = baseId + "_line_" + suffix;
        }
        return id;
    }

    private static bool LineageIdExists(MclslWorldRunState run, string id)
    {
        if (run?.TechniqueLineages == null || string.IsNullOrWhiteSpace(id)) return false;
        for (int i = 0; i < run.TechniqueLineages.Count; i++)
        {
            if (string.Equals(run.TechniqueLineages[i]?.Id, id, StringComparison.Ordinal)) return true;
        }
        return false;
    }

    private static bool RealmAtLeast(string maxRealm, string targetRealm)
    {
        int max = MclslRealmIds.Index(maxRealm);
        int target = MclslRealmIds.Index(targetRealm);
        return max >= target && target >= 0;
    }

    private static int TechniqueCost(string targetRealm, string factionId)
    {
        int cost = targetRealm switch
        {
            MclslRealmIds.ZhuJi => 420,
            MclslRealmIds.JinDan => 1350,
            MclslRealmIds.YuanYing => 3400,
            MclslRealmIds.HuaShen => 7600,
            MclslRealmIds.HeDao => 14500,
            _ => 420
        };
        return factionId == FiveElders ? cost * 105 / 100 : cost;
    }

    private static MclslTechniqueDefinition PickTechnique(string targetRealm, string factionId, Actor actor, int year, int roll)
    {
        string roots = MclslActorAccessor.GetString(actor, MclslActorDataKeys.GoldenCoreLaws,
            MclslActorAccessor.GetString(actor, MclslActorDataKeys.FoundationWonderTags, string.Empty));
        if (roll % 100 < 28
            && MclslTechniqueLineageSystem.TryPickInheritedTechnique(year, MclslActorAccessor.Id(actor) + "|faction_exchange|" + roll, 100, out MclslTechniqueDefinition inherited)
            && IsUsableTechnique(inherited, targetRealm))
            return inherited;

        MclslTechniqueDefinition picked = PickFromCatalog(targetRealm, roots, exact: true, requireLawMatch: true, roll + FactionBias(factionId));
        if (picked != null) return picked;
        picked = PickFromCatalog(targetRealm, roots, exact: false, requireLawMatch: true, roll + FactionBias(factionId));
        if (picked != null) return picked;
        picked = PickFromCatalog(targetRealm, roots, exact: true, requireLawMatch: false, roll + FactionBias(factionId));
        if (picked != null) return picked;
        return PickFromCatalog(targetRealm, roots, exact: false, requireLawMatch: false, roll + FactionBias(factionId))
            ?? MclslCultivationCatalog.Techniques[0];
    }

    private static MclslTechniqueDefinition PickFromCatalog(string targetRealm, string roots, bool exact, bool requireLawMatch, int roll)
    {
        int count = 0;
        for (int i = 0; i < MclslCultivationCatalog.Techniques.Count; i++)
        {
            if (IsCatalogCandidate(MclslCultivationCatalog.Techniques[i], targetRealm, roots, exact, requireLawMatch))
                count++;
        }
        if (count <= 0) return null;

        int pick = Math.Abs(roll) % count;
        for (int i = 0; i < MclslCultivationCatalog.Techniques.Count; i++)
        {
            MclslTechniqueDefinition technique = MclslCultivationCatalog.Techniques[i];
            if (!IsCatalogCandidate(technique, targetRealm, roots, exact, requireLawMatch)) continue;
            if (pick-- == 0) return technique;
        }
        return null;
    }

    private static bool IsCatalogCandidate(MclslTechniqueDefinition technique, string targetRealm, string roots, bool exact, bool requireLawMatch)
    {
        if (technique == null) return false;
        int maxIndex = MclslRealmIds.Index(technique.MaxRealm);
        int targetIndex = MclslRealmIds.Index(targetRealm);
        if (maxIndex < 0 || targetIndex < 0) return false;
        if (exact ? maxIndex != targetIndex : maxIndex < targetIndex) return false;
        return !requireLawMatch || TechniqueMatchesRoot(technique, roots);
    }

    private static bool IsUsableTechnique(MclslTechniqueDefinition technique, string targetRealm)
    {
        if (technique == null) return false;
        int maxIndex = MclslRealmIds.Index(technique.MaxRealm);
        int targetIndex = MclslRealmIds.Index(targetRealm);
        return maxIndex >= targetIndex && targetIndex >= 0;
    }

    private static bool TechniqueMatchesRoot(MclslTechniqueDefinition technique, string roots)
    {
        if (technique?.LawPool == null || string.IsNullOrWhiteSpace(roots)) return false;
        for (int i = 0; i < technique.LawPool.Length; i++)
        {
            string tag = technique.LawPool[i];
            if (!string.IsNullOrWhiteSpace(tag) && roots.Contains(tag, StringComparison.Ordinal))
                return true;
        }
        return false;
    }

    private static int FactionBias(string factionId)
    {
        return factionId == FiveElders ? 13 : 0;
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

    private static int PositiveHash(string value)
    {
        unchecked
        {
            int hash = 29;
            foreach (char c in value ?? string.Empty) hash = hash * 37 + c;
            return hash & int.MaxValue;
        }
    }
}
