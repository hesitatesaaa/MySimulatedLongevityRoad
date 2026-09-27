using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using MySimulatedLongevityRoad.Data;

namespace MySimulatedLongevityRoad.Systems;

internal static class MclslMentorshipGiftSystem
{
    private const string ArtifactGiftYearKey = "mclsl.v020.mentor_artifact_gift_year";
    private const string ConsumableGiftYearKey = "mclsl.v020.mentor_consumable_gift_year";
    private const string RecipeGiftYearKey = "mclsl.recipes.mentor_last_year";

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
        MclslOwnedItem gift = source.Items
            .Where(x => x != null && MclslItemCatalog.Get(x.ItemId)?.Category == "Artifact"
                && x.ItemId is not "B080" and not "B081"
                && !string.IsNullOrWhiteSpace(x.InstanceId)
                && !(source.EquippedArtifactSlots?.Values.Contains(x.InstanceId) ?? false)
                && MclslBagSystem.Count(destination, x.ItemId) == 0)
            .OrderByDescending(x => MclslItemCatalog.Get(x.ItemId).Grade)
            .ThenBy(x => x.ItemId, StringComparer.Ordinal)
            .FirstOrDefault();
        if (gift == null) return;
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
        MclslOwnedItem gift = source.Items
            .Where(x => x?.Count > 1 && MclslItemCatalog.Get(x.ItemId)?.Category is "Pill" or "Talisman"
                && MclslBagSystem.Count(destination, x.ItemId) == 0)
            .OrderByDescending(x => MclslItemCatalog.Get(x.ItemId).Grade)
            .ThenBy(x => x.ItemId, StringComparer.Ordinal)
            .FirstOrDefault();
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
        int grade = MclslActorAccessor.GetInt(student, MclslActorDataKeys.ProfessionGrade);
        MclslItemDefinition recipe = MclslItemCatalog.All
            .Where(x => x.Category == category && x.Grade <= grade && !string.IsNullOrEmpty(x.IngredientA)
                && MclslRecipeKnowledge.Knows(teacher, x.Id) && !MclslRecipeKnowledge.Knows(student, x.Id))
            .OrderByDescending(x => x.Grade).ThenBy(x => x.Id, StringComparer.Ordinal).FirstOrDefault();
        if (recipe == null || !MclslRecipeKnowledge.Learn(student, recipe.Id)) return;
        MclslActorAccessor.Set(student, RecipeGiftYearKey, year);
    }
}

internal static class MclslRecipeKnowledge
{
    private sealed class RecipeCache
    {
        internal string Raw;
        internal HashSet<string> Ids = new(StringComparer.Ordinal);
    }
    private static readonly ConditionalWeakTable<Actor, RecipeCache> Cache = new();
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
        if (stored.Length == 0) return true; // old professions keep their established recipes
        if (!stored.StartsWith("v1:", StringComparison.Ordinal)) return true;
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
        MclslItemDefinition[] recipes = MclslItemCatalog.All
            .Where(x => x.Category == category && x.Grade == grade
                && !string.IsNullOrEmpty(x.IngredientA) && MclslProfessionSystem.RecipeFitsGrade(x, grade))
            .OrderBy(x => x.Id, StringComparer.Ordinal).ToArray();
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
        if (MclslItemCatalog.All.Any(x => x.Category == category && x.Grade == grade
            && MclslProfessionSystem.RecipeFitsGrade(x, grade) && Knows(actor, x.Id))) return;
        // Older saves may know only a recipe whose ingredients exceed their
        // profession grade. Add one usable recipe without removing knowledge.
        SeedGrade(actor, profession, grade);
    }

    internal static void LearnFromOwnedItem(Actor actor, string itemId)
    {
        MclslItemDefinition item = MclslItemCatalog.Get(itemId);
        if (item == null || item.Category != CategoryForProfession(MclslActorAccessor.GetString(actor, MclslActorDataKeys.Profession))
            || item.Grade > MclslActorAccessor.GetInt(actor, MclslActorDataKeys.ProfessionGrade)) return;
        Learn(actor, itemId);
    }
}
