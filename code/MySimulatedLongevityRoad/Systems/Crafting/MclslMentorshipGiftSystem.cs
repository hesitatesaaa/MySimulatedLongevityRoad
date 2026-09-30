using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using MySimulatedLongevityRoad.Data;

namespace MySimulatedLongevityRoad.Systems;

internal static class MclslMentorshipGiftSystem
{
    private const string ArtifactGiftYearKey = "mclsl.architecture.v1.actor.mentor_artifact_gift_year";
    private const string ConsumableGiftYearKey = "mclsl.architecture.v1.actor.mentor_consumable_gift_year";
    private const string RecipeGiftYearKey = "mclsl.architecture.v1.actor.mentor_recipe_gift_year";

    internal static void TryGift(Actor student, Actor teacher, int year)
    {
        if (!MclslActorAccessor.Alive(student) || !MclslActorAccessor.Alive(teacher)) return;
        if (year - MclslActorAccessor.GetInt(student, ArtifactGiftYearKey, -1000) >= 10)
            TryGiftArtifact(student, teacher, year);
        if (year - MclslActorAccessor.GetInt(student, ConsumableGiftYearKey, -1000) >= 5)
            TryGiftConsumable(student, teacher, year);
        if (year - MclslActorAccessor.GetInt(student, RecipeGiftYearKey, -1000) >= 10)
            TryTeachRecipe(student, teacher, year);
    }

    private static void TryGiftArtifact(Actor student, Actor teacher, int year)
    {
        MclslBagState source = MclslBagSystem.Read(teacher);
        MclslBagState destination = MclslBagSystem.Read(student);
        MclslOwnedItem gift = null;
        foreach (MclslOwnedItem held in source.Items)
        {
            MclslItemDefinition item = MclslItemCatalog.Get(held?.ItemId);
            if (item?.Category != "Artifact" || item.Id is "B080" or "B081"
                || MclslInventoryDataRules.IsEquipped(source, held) || MclslBagSystem.Count(destination, item.Id) > 0) continue;
            if (gift == null || BetterRecipe(item, MclslItemCatalog.Get(gift.ItemId))) gift = held;
        }
        if (gift == null) return;
        gift = MclslInventoryDataRules.MaterializeArtifact(source, gift);
        source.Items.Remove(gift);
        destination.Items.Add(new MclslOwnedItem
        {
            ItemId = gift.ItemId, InstanceId = gift.InstanceId, Count = 1,
            Durability = gift.Durability, AcquiredYear = year
        });
        MclslBagSystem.Write(teacher, source);
        MclslBagSystem.Write(student, destination);
        MclslActorAccessor.Set(student, ArtifactGiftYearKey, year);
        MclslArtifactSystem.OnArtifactAcquired(student);
        MclslRecipeKnowledge.LearnFromOwnedItem(student, gift.ItemId);
        MclslWorldRunRepository.AddItemAcquisitionEvent(year, student, gift.ItemId, 1, "师徒赠予");
    }

    private static void TryGiftConsumable(Actor student, Actor teacher, int year)
    {
        MclslBagState source = MclslBagSystem.Read(teacher);
        MclslBagState destination = MclslBagSystem.Read(student);
        MclslOwnedItem gift = null;
        foreach (MclslOwnedItem held in source.Items)
        {
            MclslItemDefinition item = MclslItemCatalog.Get(held?.ItemId);
            if (held?.Count <= 1 || item?.Category is not ("Pill" or "Talisman")
                || MclslBagSystem.Count(destination, item.Id) > 0) continue;
            if (gift == null || BetterRecipe(item, MclslItemCatalog.Get(gift.ItemId))) gift = held;
        }
        if (gift == null || !MclslBagSystem.Remove(source, gift.ItemId)) return;
        MclslBagSystem.Add(destination, gift.ItemId, acquiredYear: year);
        MclslBagSystem.Write(teacher, source);
        MclslBagSystem.Write(student, destination);
        MclslActorAccessor.Set(student, ConsumableGiftYearKey, year);
        MclslRecipeKnowledge.LearnFromOwnedItem(student, gift.ItemId);
        MclslWorldRunRepository.AddItemAcquisitionEvent(year, student, gift.ItemId, 1, "师徒赠予");
    }

    private static void TryTeachRecipe(Actor student, Actor teacher, int year)
    {
        string profession = MclslActorAccessor.GetString(student, MclslActorDataKeys.Profession);
        string category = MclslRecipeKnowledge.CategoryForProfession(profession);
        if (category.Length == 0 || category != MclslRecipeKnowledge.CategoryForProfession(
            MclslActorAccessor.GetString(teacher, MclslActorDataKeys.Profession))) return;
        int grade = MclslProfessionSystem.GetGrade(student);
        MclslItemDefinition recipe = null;
        foreach (MclslItemDefinition item in MclslItemCatalog.All)
        {
            if (item.Category != category || item.Grade > grade || string.IsNullOrEmpty(item.IngredientA)
                || !MclslRecipeKnowledge.Knows(teacher, item.Id) || MclslRecipeKnowledge.Knows(student, item.Id)) continue;
            if (recipe == null || BetterRecipe(item, recipe)) recipe = item;
        }
        if (recipe == null || !MclslRecipeKnowledge.Learn(student, recipe.Id)) return;
        MclslActorAccessor.Set(student, RecipeGiftYearKey, year);
    }
    private static bool BetterRecipe(MclslItemDefinition left, MclslItemDefinition right)
        => left.Grade > right.Grade || left.Grade == right.Grade && string.CompareOrdinal(left.Id, right.Id) < 0;
}

internal static class MclslRecipeKnowledge
{
    private sealed class RecipeCache
    {
        internal string Raw;
        internal HashSet<string> Ids = new(StringComparer.Ordinal);
        internal string CandidateRaw;
        internal readonly Dictionary<(string Category, int Grade), MclslItemDefinition[]> Candidates = new();
        internal string CraftRaw, CraftCategory;
        internal int CraftGrade = -1;
        internal MclslItemDefinition[] CraftRecipes = Array.Empty<MclslItemDefinition>();
    }
    private static ConditionalWeakTable<Actor, RecipeCache> Cache = new();
    internal static void Forget(Actor actor) { if (actor != null) Cache.Remove(actor); }
    internal static void ClearRuntime() => Cache = new();
    private static readonly Dictionary<(string Category, int Grade), MclslItemDefinition[]> RecipeGroups = BuildGroups();
    private static (string Category, int Grade) GroupKey(string category, int grade) => (category, grade);
    private static Dictionary<(string Category, int Grade), MclslItemDefinition[]> BuildGroups()
    {
        return MclslItemCatalog.All.Where(x => x.Category is "Pill" or "Artifact" or "Talisman"
            && !string.IsNullOrWhiteSpace(x.IngredientA) && !string.IsNullOrWhiteSpace(x.IngredientB)
            && MclslProfessionSystem.RecipeFitsGrade(x, x.Grade))
            .GroupBy(x => GroupKey(x.Category, x.Grade))
            .ToDictionary(x => x.Key, x => x.OrderBy(item => item.Id, StringComparer.Ordinal).ToArray());
    }
    internal static MclslItemDefinition[] Recipes(string category, int grade)
        => RecipeGroups.TryGetValue(GroupKey(category, grade), out MclslItemDefinition[] recipes)
            ? recipes : Array.Empty<MclslItemDefinition>();
    internal static MclslItemDefinition[] CraftCandidates(Actor actor, string category, int grade)
    {
        string raw = MclslActorAccessor.GetString(actor, MclslActorDataKeys.KnownRecipes);
        MclslItemDefinition[] group = Recipes(category, grade);
        if (!raw.StartsWith("v1:", StringComparison.Ordinal)) return Array.Empty<MclslItemDefinition>();
        RecipeCache cache = Cache.GetOrCreateValue(actor);
        if (!string.Equals(cache.CandidateRaw, raw, StringComparison.Ordinal))
        {
            cache.CandidateRaw = raw;
            cache.Candidates.Clear();
        }
        var key = GroupKey(category, grade);
        if (!cache.Candidates.TryGetValue(key, out MclslItemDefinition[] known))
            cache.Candidates[key] = known = group.Where(x => Knows(actor, x.Id)).ToArray();
        return known;
    }

    internal static IReadOnlyList<MclslItemDefinition> KnownCraftRecipes(Actor actor, string category, int grade)
    {
        if (category.Length == 0 || grade < 0) return Array.Empty<MclslItemDefinition>();
        RecipeCache cache = Cache.GetOrCreateValue(actor);
        string raw = MclslActorAccessor.GetString(actor, MclslActorDataKeys.KnownRecipes);
        if (cache.CraftRaw == raw && cache.CraftCategory == category && cache.CraftGrade == grade) return cache.CraftRecipes;
        List<MclslItemDefinition> recipes = new();
        foreach (MclslItemDefinition item in MclslItemCatalog.All)
        {
            if (item.Category == category && MclslProfessionCraftingPolicy.CanAttemptRecipe(grade, item.Grade)
                && MclslProfessionSystem.RecipeFitsGrade(item, item.Grade)
                && !string.IsNullOrWhiteSpace(item.IngredientA) && !string.IsNullOrWhiteSpace(item.IngredientB)
                && Knows(actor, item.Id)) recipes.Add(item);
        }
        recipes.Sort((a, b) => { int order = b.Grade.CompareTo(a.Grade); return order != 0 ? order : string.CompareOrdinal(a.Id, b.Id); });
        cache.CraftRaw = raw; cache.CraftCategory = category; cache.CraftGrade = grade;
        return cache.CraftRecipes = recipes.ToArray();
    }
    internal static string CategoryForProfession(string profession) => profession switch
    {
        MclslProfessionSystem.Alchemist => "Pill",
        MclslProfessionSystem.Refiner => "Artifact",
        MclslProfessionSystem.TalismanMaker => "Talisman",
        _ => string.Empty
    };

    internal static bool Knows(Actor actor, string itemId)
    {
        string stored = MclslActorAccessor.GetString(actor, MclslActorDataKeys.KnownRecipes);
        if (!stored.StartsWith("v1:", StringComparison.Ordinal)) return false;
        RecipeCache cache = Cache.GetOrCreateValue(actor);
        if (!string.Equals(cache.Raw, stored, StringComparison.Ordinal))
        {
            cache.Raw = stored;
            cache.Ids = new HashSet<string>(stored.Substring(3).Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries), StringComparer.Ordinal);
        }
        return cache.Ids.Contains(itemId);
    }

    internal static bool Learn(Actor actor, string itemId)
    {
        MclslItemDefinition item = MclslItemCatalog.Get(itemId);
        if (actor?.data == null || item == null || string.IsNullOrEmpty(item.IngredientA) || Knows(actor, item.Id)) return false;
        string stored = MclslActorAccessor.GetString(actor, MclslActorDataKeys.KnownRecipes);
        if (stored.Length == 0) stored = "v1:";
        if (!stored.StartsWith("v1:", StringComparison.Ordinal)) return false;
        MclslActorAccessor.Set(actor, MclslActorDataKeys.KnownRecipes, stored + (stored == "v1:" ? "" : ",") + item.Id);
        return true;
    }

    internal static void SeedNewProfession(Actor actor, string profession)
    {
        if (MclslActorAccessor.GetString(actor, MclslActorDataKeys.KnownRecipes).Length != 0) return;
        MclslActorAccessor.Set(actor, MclslActorDataKeys.KnownRecipes, "v1:");
        SeedGrade(actor, profession, 0);
        SeedGrade(actor, profession, 1);
    }

    internal static void SeedGrade(Actor actor, string profession, int grade)
    {
        if (!MclslActorAccessor.GetString(actor, MclslActorDataKeys.KnownRecipes).StartsWith("v1:", StringComparison.Ordinal)) return;
        string category = CategoryForProfession(profession);
        MclslItemDefinition[] recipes = Recipes(category, grade);
        if (recipes.Length == 0) return;
        int index = (int)((Math.Max(0L, MclslActorAccessor.Id(actor)) + grade) % recipes.Length);
        Learn(actor, recipes[index].Id);
    }

    internal static void EnsureUsableRecipe(Actor actor, string profession, int grade)
    {
        string known = MclslActorAccessor.GetString(actor, MclslActorDataKeys.KnownRecipes);
        if (known.Length == 0)
        {
            SeedNewProfession(actor, profession);
            known = MclslActorAccessor.GetString(actor, MclslActorDataKeys.KnownRecipes);
        }
        if (grade < 0 || !known.StartsWith("v1:", StringComparison.Ordinal)) return;
        string category = CategoryForProfession(profession);
        if (CraftCandidates(actor, category, grade).Length > 0) return;
        // A changed profession needs a recipe for its current ingredient grade.
        SeedGrade(actor, profession, grade);
    }

    internal static void LearnFromOwnedItem(Actor actor, string itemId)
    {
        MclslItemDefinition item = MclslItemCatalog.Get(itemId);
        if (item == null || item.Category != CategoryForProfession(MclslActorAccessor.GetString(actor, MclslActorDataKeys.Profession))
            || item.Grade > MclslProfessionSystem.GetGrade(actor)) return;
        Learn(actor, itemId);
    }
}
