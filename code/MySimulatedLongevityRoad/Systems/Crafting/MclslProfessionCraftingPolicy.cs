using System;

namespace MySimulatedLongevityRoad.Systems;

internal static class MclslProfessionCraftingPolicy
{
    internal static int MinimumRecipeGrade(int professionGrade)
    {
        if (professionGrade < 0 || professionGrade > 4) return -1;
        return Math.Max(0, professionGrade - 1);
    }

    internal static bool CanAttemptRecipe(int professionGrade, int recipeGrade)
    {
        int minimum = MinimumRecipeGrade(professionGrade);
        return minimum >= 0 && recipeGrade >= minimum && recipeGrade <= professionGrade;
    }

    internal static int ExperienceGain(int professionGrade, int recipeGrade)
        => professionGrade is >= 0 and <= 4 && recipeGrade == professionGrade ? 1 : 0;
}
