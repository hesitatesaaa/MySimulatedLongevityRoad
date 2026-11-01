using System;

namespace MySimulatedLongevityRoad.Systems;

internal static class MclslProfessionCraftingPolicy
{
    private static readonly int[] PromotionThresholds = { 10, 30, 80, 160 };

    internal static bool CanCraftInYear(int year, int lastCraftYear) => year > 0 && year != lastCraftYear;

    // Realm indices start at zero for Lian Qi. Every profession shares this cap.
    internal static int MaximumGrade(int realmIndex) => Math.Clamp(realmIndex, 0, 4);
    internal static int ClampGrade(int grade, int realmIndex) =>
        Math.Clamp(grade, 0, MaximumGrade(realmIndex));

    internal static int PromotedGrade(int grade, int experience, int realmIndex)
    {
        int maximum = MaximumGrade(realmIndex);
        grade = ClampGrade(grade, realmIndex);
        while (grade < maximum && experience >= PromotionThresholds[grade]) grade++;
        return grade;
    }

    internal static int MinimumRecipeGrade(int professionGrade)
    {
        if (professionGrade < 0 || professionGrade > 4) return -1;
        return professionGrade == 0 ? 0 : Math.Max(1, professionGrade - 1);
    }

    internal static bool CanAttemptRecipe(int professionGrade, int recipeGrade)
    {
        int minimum = MinimumRecipeGrade(professionGrade);
        return minimum >= 0 && recipeGrade >= minimum && recipeGrade <= professionGrade;
    }

    internal static int ExperienceGain(int professionGrade, int recipeGrade)
        => CanAttemptRecipe(professionGrade, recipeGrade) ? recipeGrade switch
        {
            0 => 1,
            1 => 3,
            2 => 6,
            3 => 10,
            4 => 15,
            _ => 0
        } : 0;
}
