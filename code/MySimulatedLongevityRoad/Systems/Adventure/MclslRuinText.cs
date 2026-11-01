using System;

namespace MySimulatedLongevityRoad.Systems;

internal static class MclslRuinText
{
    internal static string DangerBand(int danger)
    {
        if (danger >= 85) return "绝地";
        if (danger >= 70) return "凶险";
        if (danger >= 55) return "险象";
        if (danger >= 40) return "多变";
        return "平稳";
    }

    internal static string DangerColor(int danger)
    {
        if (danger >= 85) return "#FF5A5A";
        if (danger >= 70) return "#FF8877";
        if (danger >= 55) return "#FFD37A";
        if (danger >= 40) return "#D8C778";
        return "#A7E08A";
    }

    internal static string DangerEventText(int danger)
    {
        string band = DangerBand(danger);
        return band switch
        {
            "绝地" => "此地凶机深伏",
            "凶险" => "此地险象已显",
            "险象" => "此地暗藏险象",
            "多变" => "此地灵机多变",
            _ => "此地尚称平稳"
        };
    }
}
