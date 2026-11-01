namespace MySimulatedLongevityRoad.Systems;

internal static class MclslItemAcquisitionHistoryPolicy
{
    internal static bool ShouldRecordMaterial(int materialTier, bool historyEnabled, bool lowGradeEnabled)
    {
        if (!historyEnabled || materialTier < 1 || materialTier > 4) return false;
        return materialTier >= 3 || lowGradeEnabled;
    }

    internal static bool ShouldRecordFinishedProduct(string category, int grade,
        bool historyEnabled, bool lowGradeEnabled)
    {
        if (!historyEnabled || grade < 0 || grade > 4
            || category is not ("Pill" or "Talisman" or "Artifact")) return false;
        return grade >= 3 || lowGradeEnabled;
    }
}
