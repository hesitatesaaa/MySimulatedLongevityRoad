using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using MySimulatedLongevityRoad.Data;
using MySimulatedLongevityRoad.Traits;

namespace MySimulatedLongevityRoad.Systems;

internal readonly struct MclslSpiritualRootProfile
{
    internal readonly string GradeName;
    internal readonly string AttributeText;
    internal readonly int Count;
    internal readonly int Purity;
    internal readonly float CountCultivationMultiplier;
    internal readonly int MultiLawInsightBonus;

    internal string CountName => Count switch
    {
        1 => "单灵根",
        2 => "双灵根",
        3 => "三灵根",
        4 => "四灵根",
        _ => "五灵根"
    };

    internal MclslSpiritualRootProfile(string gradeName, string attributeText, int count, int purity, float countCultivationMultiplier, int multiLawInsightBonus)
    {
        GradeName = gradeName ?? string.Empty;
        AttributeText = attributeText ?? string.Empty;
        Count = Math.Clamp(count, 1, 5);
        Purity = Math.Clamp(purity, 1, 100);
        CountCultivationMultiplier = countCultivationMultiplier;
        MultiLawInsightBonus = multiLawInsightBonus;
    }
}

internal static class MclslSpiritualRootSystem
{
    private sealed class RootCache
    {
        internal string Stored, Primary, Display;
        internal int Count;
        internal bool ValidStored;
        internal string[] Attributes;
    }
    private static ConditionalWeakTable<Actor, RootCache> RootCaches = new();
    internal static void ClearRuntime() => RootCaches = new();
    internal static void Forget(Actor actor) { if (actor != null) RootCaches.Remove(actor); }
    private static string AttributeDisplay(Actor actor, string[] attributes)
    {
        if (actor == null) return string.Join("、", attributes);
        RootCache cache = RootCaches.GetOrCreateValue(actor);
        return cache.Display ??= string.Join("、", attributes);
    }

    private static readonly string[] Elements = { "金", "木", "水", "火", "土", "风", "雷", "阴", "阳", "空间" };

    internal static MclslSpiritualRootProfile Profile(Actor actor)
    {
        int rawPurity = Math.Clamp(MclslActorAccessor.GetInt(actor, MclslActorDataKeys.Aptitude, 50)
            + (actor.hasTrait(MclslTraitRegistration.HeavenFavorTraitId) ? 20 : 0), 1, 100);
        MclslAptitudeGiftDefinition traitGrade = GiftFromCurrentTrait(actor);
        MclslAptitudeGiftDefinition grade = traitGrade ?? MclslAptitudeGiftCatalog.ForAptitude(rawPurity);
        int purity = traitGrade == null ? rawPurity : CoercePurityForTrait(actor, rawPurity, traitGrade);
        int count = StoredRootCount(actor, purity);
        return new MclslSpiritualRootProfile(
            grade.Name,
            AttributeDisplay(actor, EnsureRootAttributes(actor, count)),
            count,
            purity,
            CountCultivationMultiplier(count),
            MultiLawInsightBonus(count));
    }

    internal static bool EnsureForSpecialPhysique(Actor actor)
    {
        if (actor?.data == null || !MclslEligibility.CanOwnSpecialPhysique(actor)) return false;
        int aptitude = MclslActorAccessor.GetInt(actor, MclslActorDataKeys.Aptitude, 0);
        if (aptitude < 1 || aptitude > 100)
        {
            aptitude = MclslAptitudeGiftCatalog.RollAptitude(
                MclslActorAccessor.Id(actor) + "|physique_root");
            MclslActorAccessor.Set(actor, MclslActorDataKeys.Aptitude, aptitude);
        }
        // Profile completes and persists count, primary element and attributes.
        Profile(actor);
        return HasActualSpiritualRoot(actor);
    }

    internal static MclslSpiritualRootProfile ReadProfile(Actor actor)
    {
        int rawPurity = Math.Clamp(MclslActorAccessor.GetInt(actor, MclslActorDataKeys.Aptitude, 50), 1, 100);
        MclslAptitudeGiftDefinition traitGrade = GiftFromCurrentTrait(actor);
        MclslAptitudeGiftDefinition grade = traitGrade ?? MclslAptitudeGiftCatalog.ForAptitude(rawPurity);
        int purity = traitGrade == null ? rawPurity : CoercePurityForTrait(actor, rawPurity, traitGrade);
        int storedCount = MclslActorAccessor.GetInt(actor, MclslActorDataKeys.SpiritualRootCount, 0);
        int count = storedCount >= 1 && storedCount <= 5 ? storedCount : RootCount(actor, purity);
        return new MclslSpiritualRootProfile(
            grade.Name,
            AttributeDisplay(actor, ReadRootAttributes(actor, count)),
            count,
            purity,
            CountCultivationMultiplier(count),
            MultiLawInsightBonus(count));
    }

    internal static string[] RootAttributes(Actor actor)
    {
        int count = StoredRootCount(actor, EffectivePurity(actor));
        return EnsureRootAttributes(actor, count);
    }

    internal static int TechniqueWeight(Actor actor, MclslTechniqueDefinition technique)
    {
        if (actor?.data == null || technique?.LawPool == null) return 10;
        string[] attributes = RootAttributes(actor);
        if (attributes.Length == 0) return 10;
        int hits = 0;
        for (int i = 0; i < attributes.Length; i++)
            if (Array.IndexOf(technique.LawPool, attributes[i]) >= 0) hits++;
        string primary = MclslActorAccessor.GetString(actor, MclslActorDataKeys.SpiritualRootPrimary, string.Empty);
        int primaryBonus = !string.IsNullOrEmpty(primary) && Array.IndexOf(technique.LawPool, primary) >= 0 ? 24 : 0;
        return 10 + (int)Math.Round(36d * hits / attributes.Length) + primaryBonus;
    }

    internal static float CountCultivationMultiplier(Actor actor) => CountCultivationMultiplier(StoredRootCount(actor, EffectivePurity(actor)));

    internal static int MultiLawInsightBonus(Actor actor) => MultiLawInsightBonus(StoredRootCount(actor, EffectivePurity(actor)));

    internal static int LawHarmonyBonus(Actor actor)
    {
        int purity = EffectivePurity(actor);
        return (GiftFromCurrentTrait(actor) ?? MclslAptitudeGiftCatalog.ForAptitude(purity)).LawHarmonyBonus;
    }

    internal static int BreakthroughBonus(Actor actor)
    {
        int purity = EffectivePurity(actor);
        return (GiftFromCurrentTrait(actor) ?? MclslAptitudeGiftCatalog.ForAptitude(purity)).BreakthroughBonus;
    }

    internal static int InsightBonus(Actor actor)
    {
        int purity = EffectivePurity(actor);
        return (GiftFromCurrentTrait(actor) ?? MclslAptitudeGiftCatalog.ForAptitude(purity)).InsightBonus + MultiLawInsightBonus(actor);
    }

    internal static MclslAptitudeGiftDefinition GiftFromCurrentTrait(Actor actor)
    {
        if (actor?.data == null) return null;
        MclslAptitudeGiftDefinition best = null;
        foreach (MclslAptitudeGiftDefinition gift in MclslAptitudeGiftCatalog.Gifts)
        {
            try
            {
                if (!MclslTraitRegistration.HasTraitWithAlias(actor, gift.TraitId)) continue;
                if (best == null || gift.Level > best.Level) best = gift;
            }
            catch (System.Exception mclslEmptyCatchEx) { MySimulatedLongevityRoad.Core.MclslDiagnostics.Error("empty-catch-code-MySimulatedLongevityRoad-Systems-Cultivation-MclslSpiritualRootSystem-cs-1", "空 catch 捕获: code/MySimulatedLongevityRoad/Systems/Cultivation/MclslSpiritualRootSystem.cs #1: " + mclslEmptyCatchEx.Message); }
        }
        return best;
    }

    internal static bool HasCultivationRootArchive(Actor actor)
    {
        if (actor?.data == null) return false;
        if (GiftFromCurrentTrait(actor) != null) return true;

        if (MclslActorAccessor.GetInt(actor, MclslActorDataKeys.ManualGrant, 0) > 0) return true;
        if (MclslActorAccessor.GetInt(actor, MclslActorDataKeys.SpiritualRootCount, 0) > 0) return true;
        if (!string.IsNullOrWhiteSpace(MclslActorAccessor.GetString(actor, MclslActorDataKeys.SpiritualRootPrimary, string.Empty))) return true;
        if (!string.IsNullOrWhiteSpace(MclslActorAccessor.GetString(actor, MclslActorDataKeys.SpiritualRootAttributes, string.Empty))) return true;

        int aptitude = MclslActorAccessor.GetInt(actor, MclslActorDataKeys.Aptitude, 0);
        if (aptitude > 0) return true;

        bool checkedRoot = MclslActorAccessor.GetInt(actor, MclslActorDataKeys.ChildhoodRootChecked, 0) > 0
            || MclslActorAccessor.GetInt(actor, MclslActorDataKeys.MortalSeparationChecked, 0) > 0;
        return checkedRoot && MclslActorAccessor.GetInt(actor, MclslActorDataKeys.ImmortalFate, 0) > 0;
    }

    internal static bool HasActualSpiritualRoot(Actor actor)
    {
        if (actor?.data == null) return false;
        if (GiftFromCurrentTrait(actor) != null) return true;
        if (MclslActorAccessor.GetInt(actor, MclslActorDataKeys.SpiritualRootCount, 0) > 0) return true;
        if (!string.IsNullOrWhiteSpace(MclslActorAccessor.GetString(actor, MclslActorDataKeys.SpiritualRootPrimary, string.Empty))) return true;
        return !string.IsNullOrWhiteSpace(MclslActorAccessor.GetString(actor, MclslActorDataKeys.SpiritualRootAttributes, string.Empty));
    }

    internal static bool HasCultivationPotential(Actor actor)
    {
        return actor?.data != null && HasCultivationRootArchive(actor);
    }

    internal static MclslAptitudeGiftDefinition ReadGiftForCultivation(Actor actor)
    {
        MclslAptitudeGiftDefinition traitGift = GiftFromCurrentTrait(actor);
        if (traitGift != null) return traitGift;
        if (!HasCultivationPotential(actor)) return null;

        int storedAptitude = MclslActorAccessor.GetInt(actor, MclslActorDataKeys.Aptitude, 0);
        int aptitude = storedAptitude <= 0 ? 50 : Math.Clamp(storedAptitude, 1, 100);
        return MclslAptitudeGiftCatalog.ForAptitude(aptitude);
    }

    internal static MclslAptitudeGiftDefinition GiftForCultivation(Actor actor)
    {
        MclslAptitudeGiftDefinition gift = ReadGiftForCultivation(actor);
        if (gift == null) return null;
        if (MclslActorAccessor.GetInt(actor, MclslActorDataKeys.Aptitude, 0) <= 0)
            MclslActorAccessor.Set(actor, MclslActorDataKeys.Aptitude, 50);
        return gift;
    }

    private static int EffectivePurity(Actor actor)
    {
        int rawPurity = Math.Clamp(MclslActorAccessor.GetInt(actor, MclslActorDataKeys.Aptitude, 50), 1, 100);
        MclslAptitudeGiftDefinition traitGrade = GiftFromCurrentTrait(actor);
        return traitGrade == null ? rawPurity : CoercePurityForTrait(actor, rawPurity, traitGrade);
    }

    private static int CoercePurityForTrait(Actor actor, int rawPurity, MclslAptitudeGiftDefinition traitGrade)
    {
        if (traitGrade == null) return Math.Clamp(rawPurity, 1, 100);
        if (rawPurity >= traitGrade.MinAptitude && rawPurity <= traitGrade.MaxAptitude) return rawPurity;
        int span = Math.Max(1, traitGrade.MaxAptitude - traitGrade.MinAptitude + 1);
        int offset = PositiveHash(MclslActorAccessor.Id(actor) + "|" + traitGrade.TraitId + "|purity") % span;
        return Math.Clamp(traitGrade.MinAptitude + offset, 1, 100);
    }

    private static string[] ReadRootAttributes(Actor actor, int count)
    {
        string stored = MclslActorAccessor.GetString(actor, MclslActorDataKeys.SpiritualRootAttributes, string.Empty);
        string primary = MclslActorAccessor.GetString(actor, MclslActorDataKeys.SpiritualRootPrimary, string.Empty);
        RootCache cache = actor == null ? new RootCache() : RootCaches.GetOrCreateValue(actor);
        if (cache.Attributes != null && cache.Stored == stored && cache.Primary == primary && cache.Count == count)
            return cache.Attributes;
        string[] existing = (stored ?? string.Empty).Split(new[] { ',', '，', '、' }, StringSplitOptions.RemoveEmptyEntries);
        cache.Stored = stored; cache.Primary = primary; cache.Count = count; cache.Display = null;
        cache.ValidStored = existing.Length == count;
        if (cache.ValidStored) return cache.Attributes = existing;
        if (string.IsNullOrWhiteSpace(primary) || Array.IndexOf(Elements, primary) < 0)
            primary = Elements[PositiveHash(MclslActorAccessor.Id(actor) + "|spiritual_root_primary") % Elements.Length];
        string[] result = new string[Math.Clamp(count, 1, 5)];
        result[0] = primary;
        int written = 1, start = PositiveHash(MclslActorAccessor.Id(actor) + "|spiritual_root_attrs") % Elements.Length;
        for (int i = 0; i < Elements.Length && written < result.Length; i++)
        {
            string value = Elements[(start + i) % Elements.Length];
            if (Array.IndexOf(result, value, 0, written) < 0) result[written++] = value;
        }
        return cache.Attributes = result;
    }

    private static string[] EnsureRootAttributes(Actor actor, int count)
    {
        string[] attributes = ReadRootAttributes(actor, count);
        if (actor?.data == null) return attributes;
        RootCache cache = RootCaches.GetOrCreateValue(actor);
        if (cache.ValidStored) return attributes;
        string primary = attributes[0];
        string stored = string.Join(",", attributes);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.SpiritualRootPrimary, primary);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.SpiritualRootAttributes, stored);
        cache.Primary = primary; cache.Stored = stored; cache.ValidStored = true;
        return attributes;
    }

    private static int StoredRootCount(Actor actor, int purity)
    {
        int stored = MclslActorAccessor.GetInt(actor, MclslActorDataKeys.SpiritualRootCount, 0);
        if (stored >= 1 && stored <= 5) return stored;
        int generated = RootCount(actor, purity);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.SpiritualRootCount, generated);
        return generated;
    }

    private static int RootCount(Actor actor, int purity)
    {
        int roll = PositiveHash(MclslActorAccessor.Id(actor) + "|spiritual_root_count") % 100;
        if (purity >= 95) return roll < 70 ? 1 : roll < 92 ? 2 : 3;
        if (purity >= 85) return roll < 55 ? 1 : roll < 85 ? 2 : roll < 97 ? 3 : 4;
        if (purity >= 70) return roll < 30 ? 1 : roll < 68 ? 2 : roll < 90 ? 3 : 4;
        if (purity >= 55) return roll < 14 ? 1 : roll < 44 ? 2 : roll < 76 ? 3 : roll < 94 ? 4 : 5;
        if (purity >= 35) return roll < 8 ? 1 : roll < 26 ? 2 : roll < 56 ? 3 : roll < 84 ? 4 : 5;
        return roll < 4 ? 1 : roll < 14 ? 2 : roll < 36 ? 3 : roll < 70 ? 4 : 5;
    }

    private static float CountCultivationMultiplier(int count) => Math.Clamp(count, 1, 5) switch
    {
        1 => 1.20f,
        2 => 1.05f,
        3 => 0.90f,
        4 => 0.75f,
        _ => 0.60f
    };

    private static int MultiLawInsightBonus(int count) => Math.Clamp(count, 1, 5) switch
    {
        1 => -15,
        2 => 5,
        3 => 15,
        4 => 25,
        _ => 35
    };

    private static int PositiveHash(string value)
    {
        unchecked
        {
            int hash = 17;
            foreach (char c in value ?? string.Empty) hash = hash * 31 + c;
            return hash & int.MaxValue;
        }
    }
}
