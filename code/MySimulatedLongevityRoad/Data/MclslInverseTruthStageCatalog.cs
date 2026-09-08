namespace MySimulatedLongevityRoad.Data;

internal static class MclslInverseTruthStageCatalog
{
    internal static string StageName(int progress)
    {
        if (progress >= 100) return "逆转";
        if (progress >= 80) return "逆转前夕";
        if (progress >= 50) return "掌控";
        if (progress >= 25) return "熟悉";
        return "寻找";
    }
}
