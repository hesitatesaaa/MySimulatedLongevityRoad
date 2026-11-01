using System;

namespace MySimulatedLongevityRoad.Systems;

internal static class MclslReincarnationService
{
    internal static bool SealCurrentCycle(string reason, int year)
    {
        var run = MclslWorldRunRepository.Current;
        var profile = MclslReincarnationProfileStore.Current;
        if (string.IsNullOrWhiteSpace(run.RunId) || profile.LastCompletedRunId == run.RunId) return false;
        foreach (var discovery in run.Discoveries)
            if (!profile.KnownKnowledgeIds.Contains(discovery.KnowledgeId)) profile.KnownKnowledgeIds.Add(discovery.KnowledgeId);
        profile.TotalTruthValue += Math.Max(0, run.RunTruthValue);
        profile.CompletedCycles++;
        profile.CurrentCycle = Math.Max(profile.CurrentCycle + 1, run.CycleNumber + 1);
        profile.LastCompletedRunId = run.RunId;
        profile.LastCompletedYear = Math.Max(0, year);
        profile.LastTerminalReason = reason ?? string.Empty;
        profile.Revision++;
        MclslReincarnationProfileStore.Flush();
        return true;
    }
}
