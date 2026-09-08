using System;
using System.Collections.Generic;
using MySimulatedLongevityRoad.Data;

namespace MySimulatedLongevityRoad.Systems;

internal static class MclslNewLawBreakthroughSystem
{
    internal static void TryFoundation(Actor actor, int year, int aptitude)
    {
        int storedBonus = Math.Max(0, MclslActorAccessor.GetInt(actor, MclslActorDataKeys.FoundationChanceBonus, 0));
        MclslAptitudeGiftDefinition gift = Gift(actor);
        int chance = 8 + aptitude / 8 + storedBonus + gift.BreakthroughBonus
            + MclslMindSystem.BreakthroughAdjustment(actor)
            + MclslInverseTruthSystem.NewLawBreakthroughChanceBonus()
            + MclslWorldStateModifierSystem.BreakthroughStabilityBonus(year);
        int roll = PositiveHash(MclslActorAccessor.Id(actor) + "|foundation|" + year) % 100;
        if (roll >= Math.Min(100, chance))
        {
            MclslActorAccessor.Set(actor, MclslActorDataKeys.LastBreakthroughResult, "尚未遇到合适的筑基奇物");
            return;
        }
        if (storedBonus > 0) MclslActorAccessor.Set(actor, MclslActorDataKeys.FoundationChanceBonus, 0);
        int qualityRoll = PositiveHash(MclslActorAccessor.Id(actor) + "|foundation_quality|" + year) % 100;
        int quality = qualityRoll < 4 ? 4 : qualityRoll < 18 ? 3 : qualityRoll < 58 ? 2 : 1;
        string techniqueId = MclslActorAccessor.GetString(actor, MclslActorDataKeys.TechniqueId, MclslCultivationCatalog.Techniques[0].Id);
        MclslTechniqueDefinition technique = MclslCultivationCatalog.Technique(techniqueId);
        string[] tags = BuildFoundationTags(actor, year, technique, quality);
        MclslGeneratedItemRecord wonder = MclslGeneratedObjectFactory.CreateFoundationWonder(actor, year, quality, tags);
        MclslGeneratedObjectFactory.ApplyFoundation(actor, wonder);
        ApplyFoundationCategoryBonus(actor, wonder);
        MclslCultivationSystem.SetRealm(actor, MclslRealmIds.ZhuJi, year, "以奇物“" + wonder.Name + "”筑基");
        string displayName = MclslActorAccessor.DisplayName(actor, MclslRealmIds.ZhuJi);
        string rank = MclslGeneratedObjectFactory.FoundationRankText(wonder);
        MclslWorldRunRepository.AddEvent(year, "foundation", displayName + "筑基", wonder.Origin + "，得" + rank + "“" + wonder.Name + "”，以“" + wonder.LawTags.Replace(",", "、") + "”筑成道基。", actor);
    }

    internal static void TryGoldenCore(Actor actor, int year, int aptitude)
    {
        BuildGoldenCoreData(actor, year, aptitude);
        string laws = MclslActorAccessor.GetString(actor, MclslActorDataKeys.GoldenCoreLaws, "灵");
        int lawCount = laws.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Length;
        MclslCultivationSystem.SetRealm(actor, MclslRealmIds.JinDan, year, "悟得" + lawCount + "法金丹：" + laws.Replace(",", "、"));
        string displayName = MclslActorAccessor.DisplayName(actor, MclslRealmIds.JinDan);
        MclslWorldRunRepository.AddEvent(year, "golden_core", displayName + "结成金丹", "由功法与动态筑基奇物共同映照，悟得“" + laws.Replace(",", "、") + "”之法。", actor);
    }

    internal static bool TryAcquireFoundationForConversion(Actor actor, int year)
    {
        if (!MclslActorAccessor.Alive(actor)) return false;
        if (!string.IsNullOrWhiteSpace(MclslActorAccessor.GetString(actor, MclslActorDataKeys.FoundationWonderName, string.Empty))) return true;
        MclslWorldRunState run = MclslWorldRunRepository.Current;
        if (run?.SectRuins == null || run.SectRuins.Count == 0) return false;

        MclslTechniqueDefinition technique = MclslCultivationCatalog.Technique(
            MclslActorAccessor.GetString(actor, MclslActorDataKeys.TechniqueId, string.Empty));
        string[] desiredTags = technique?.LawPool ?? MclslSpiritualRootSystem.RootAttributes(actor);
        MclslSectRuinRecord best = null;
        int bestScore = int.MinValue;
        for (int i = 0; i < run.SectRuins.Count; i++)
        {
            MclslSectRuinRecord ruin = run.SectRuins[i];
            if (ruin == null || ruin.RemainingValue <= 0 || ruin.State == "搜尽" || ruin.State == "封绝" || ruin.State == "崩毁") continue;
            int compatibility = MclslLawInteractionCatalog.CompatibilityScore(desiredTags,
                MclslGeneratedObjectFactory.SplitTags(ruin.LawTags), 10);
            int score = compatibility + ruin.Quality * 12 + ruin.RemainingValue * 6
                + PositiveHash(MclslActorAccessor.Id(actor) + "|conversion_foundation|" + ruin.Id + "|" + year) % 17;
            if (score <= bestScore) continue;
            best = ruin;
            bestScore = score;
        }
        if (best == null) return false;

        string[] tags = MclslGeneratedObjectFactory.SplitTags(best.LawTags);
        if (tags.Length == 0) tags = BuildFoundationTags(actor, year, technique, Math.Clamp(best.Quality, 1, 4));
        MclslGeneratedItemRecord wonder = MclslGeneratedObjectFactory.CreateFoundationWonder(actor, year, Math.Clamp(best.Quality, 1, 4), tags);
        wonder.Origin = "自遗迹“" + best.Name + "”寻得";
        wonder.SourceObjectId = best.Id;
        MclslGeneratedObjectFactory.ApplyFoundation(actor, wonder);
        ApplyFoundationCategoryBonus(actor, wonder);
        best.RemainingValue = Math.Max(0, best.RemainingValue - 1);
        best.LastExploredYear = year;
        best.LastExplorerNames = MclslActorAccessor.DisplayName(actor);
        if (best.RemainingValue <= 0) best.State = "搜尽";
        MclslActorAccessor.Set(actor, MclslActorDataKeys.LastBreakthroughResult, "已从“" + best.Name + "”寻得筑基奇物“" + wonder.Name + "”");
        MclslWorldRunRepository.AddEvent(year, "ancient_conversion_foundation", MclslActorAccessor.DisplayName(actor) + "寻得筑基奇物",
            MclslActorAccessor.DisplayName(actor) + "为转修新法探入“" + best.Name + "”，得“" + wonder.Name + "”。", actor);
        MclslWorldArchiveStore.MarkDirty();
        return true;
    }

    internal static void EnsureManualFoundationData(Actor actor, int year)
    {
        MclslTechniqueDefinition technique = MclslCultivationCatalog.Technique(MclslActorAccessor.GetString(actor, MclslActorDataKeys.TechniqueId, string.Empty));
        string[] tags = BuildFoundationTags(actor, year, technique, 2);
        MclslGeneratedItemRecord wonder = MclslGeneratedObjectFactory.CreateFoundationWonder(actor, year, 2, tags);
        MclslGeneratedObjectFactory.ApplyFoundation(actor, wonder);
        ApplyFoundationCategoryBonus(actor, wonder);
    }

    internal static void BuildGoldenCoreData(Actor actor, int year, int aptitude)
    {
        string techniqueId = MclslActorAccessor.GetString(actor, MclslActorDataKeys.TechniqueId, MclslCultivationCatalog.Techniques[0].Id);
        MclslTechniqueDefinition technique = MclslCultivationCatalog.Technique(techniqueId);
        string[] wonderTags = MclslGeneratedObjectFactory.SplitTags(MclslActorAccessor.GetString(actor, MclslActorDataKeys.FoundationWonderTags, string.Empty));
        string[] rootTags = MclslSpiritualRootSystem.RootAttributes(actor);
        List<string> pool = new();
        AddUniqueTags(pool, technique.LawPool);
        AddUniqueTags(pool, wonderTags);
        AddUniqueTags(pool, rootTags);
        if (pool.Count == 0) pool.Add("灵");
        int quality = MclslActorAccessor.GetInt(actor, MclslActorDataKeys.FoundationWonderQuality, 1);
        string category = MclslActorAccessor.GetString(actor, MclslActorDataKeys.FoundationWonderCategory, MclslGeneratedObjectFactory.FoundationHuman);
        string grade = MclslActorAccessor.GetString(actor, MclslActorDataKeys.FoundationWonderGrade, string.Empty);
        int completeness = MclslActorAccessor.GetInt(actor, MclslActorDataKeys.FoundationWonderCompleteness, 0);
        int ruleStrength = MclslActorAccessor.GetInt(actor, MclslActorDataKeys.FoundationWonderRuleStrength, 0);
        int countRoll = PositiveHash(MclslActorAccessor.Id(actor) + "|law_count|" + year) % 100;
        MclslAptitudeGiftDefinition gift = Gift(actor);
        int insight = Math.Max(0, MclslActorAccessor.GetInt(actor, MclslActorDataKeys.TechniqueInsight, 0));
        int rootInsight = MclslSpiritualRootSystem.MultiLawInsightBonus(actor);
        int effectiveInsight = gift.InsightBonus + rootInsight;
        int categoryLawBonus = category == MclslGeneratedObjectFactory.FoundationHeaven ? 2 : category == MclslGeneratedObjectFactory.FoundationEarth ? 1 : 0;
        int lawCount = countRoll < Math.Min(28, quality * 4 + effectiveInsight / 2 + insight / 12) ? 3
            : countRoll < Math.Min(72, 28 + quality * 6 + effectiveInsight + insight / 8) ? 2 : 1;
        lawCount += categoryLawBonus;
        lawCount = Math.Min(lawCount, pool.Count);
        List<string> laws = new();
        int start = PositiveHash(MclslActorAccessor.Id(actor) + "|law_start|" + year) % pool.Count;
        for (int i = 0; i < pool.Count && laws.Count < lawCount; i++)
        {
            string law = pool[(start + i) % pool.Count];
            if (!laws.Contains(law)) laws.Add(law);
        }
        int mindBonus = MclslMindSystem.StabilityBonus(actor);
        int purityBonus = category == MclslGeneratedObjectFactory.FoundationHeaven ? ruleStrength / 8
            : category == MclslGeneratedObjectFactory.FoundationEarth ? Math.Max(4, completeness / 12)
            : grade == "上品" ? 8 : grade == "中品" ? 4 : 1;
        int stabilityBonus = category == MclslGeneratedObjectFactory.FoundationHeaven ? -14 + completeness / 10
            : category == MclslGeneratedObjectFactory.FoundationEarth ? -6 + completeness / 20
            : grade == "上品" ? 10 : grade == "中品" ? 5 : 1;
        int purity = Math.Clamp(48 + aptitude / 4 + quality * 6 + purityBonus + gift.InsightBonus / 2 + mindBonus / 2 - (lawCount - 1) * 12, 25, 99);
        int stability = Math.Clamp(72 + quality * 5 + stabilityBonus + gift.LawHarmonyBonus
            + gift.BreakthroughBonus + mindBonus + MclslWorldStateModifierSystem.BreakthroughStabilityBonus(year)
            - (lawCount - 1) * 18, 25, 100);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.GoldenCoreLaws, string.Join(",", laws));
        MclslActorAccessor.Set(actor, MclslActorDataKeys.GoldenCorePurity, purity);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.GoldenCoreStability, stability);
    }

    private static void ApplyFoundationCategoryBonus(Actor actor, MclslGeneratedItemRecord wonder)
    {
        if (wonder == null) return;
        int insight = Math.Max(0, MclslActorAccessor.GetInt(actor, MclslActorDataKeys.TechniqueInsight, 0));
        if (wonder.Category == MclslGeneratedObjectFactory.FoundationHeaven)
        {
            MclslActorAccessor.Set(actor, MclslActorDataKeys.TechniqueInsight, insight + 25);
            MclslTechniqueRealmLimit.SetMaxRealm(actor, MclslRealmIds.HeDao);
        }
        else if (wonder.Category == MclslGeneratedObjectFactory.FoundationEarth)
        {
            MclslActorAccessor.Set(actor, MclslActorDataKeys.TechniqueInsight, insight + 10);
        }
    }

    private static void AddUniqueTags(List<string> target, IReadOnlyList<string> source)
    {
        if (target == null || source == null) return;
        for (int i = 0; i < source.Count; i++)
        {
            string tag = source[i];
            if (string.IsNullOrWhiteSpace(tag) || target.Contains(tag)) continue;
            target.Add(tag);
        }
    }

    private static string[] BuildFoundationTags(Actor actor, int year, MclslTechniqueDefinition technique, int quality)
    {
        int count = quality >= 3 ? 3 : 2;
        List<string> tags = new();
        int start = PositiveHash(MclslActorAccessor.Id(actor) + "|foundation_tag|" + year) % technique.LawPool.Length;
        for (int i = 0; i < technique.LawPool.Length && tags.Count < count; i++)
        {
            string tag = technique.LawPool[(start + i) % technique.LawPool.Length];
            if (!tags.Contains(tag)) tags.Add(tag);
        }
        if (quality >= 3 && PositiveHash(MclslActorAccessor.Id(actor) + "|foundation_variant|" + year) % 100 < 30)
        {
            string variant = MclslCultivationCatalog.AllLawTags[PositiveHash(MclslActorAccessor.Id(actor) + "|foundation_rare|" + year) % MclslCultivationCatalog.AllLawTags.Length];
            if (!tags.Contains(variant))
            {
                if (tags.Count >= 3) tags[tags.Count - 1] = variant;
                else tags.Add(variant);
            }
        }
        return tags.ToArray();
    }

    private static MclslAptitudeGiftDefinition Gift(Actor actor)
    {
        int aptitude = Math.Clamp(MclslActorAccessor.GetInt(actor, MclslActorDataKeys.Aptitude, 50), 1, 100);
        return MclslSpiritualRootSystem.GiftForCultivation(actor) ?? MclslAptitudeGiftCatalog.ForAptitude(aptitude);
    }

    private static int PositiveHash(string value)
    {
        unchecked { int hash = 23; foreach (char c in value ?? string.Empty) hash = hash * 37 + c; return hash & int.MaxValue; }
    }
}
