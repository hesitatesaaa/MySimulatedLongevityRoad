using System;
using System.Collections.Generic;

namespace MySimulatedLongevityRoad.Systems;

// A negative priority excludes a spell; otherwise higher priorities are drawn first.
internal static class MclslSpellAcquisitionPolicy
{
    internal static bool Succeeds(int roll) => roll < 50;

    internal static int RequestedCount(int roll) => 2 + (roll & 1);

    internal static int TeacherPriority(int studentRealm, int spellRealm, bool alreadyKnown)
        => studentRealm >= spellRealm && !alreadyKnown ? 0 : -1;

    internal static int RealmPriority(int currentRealm, int spellRealm, bool alreadyKnown,
        bool rootMatches, bool techniqueMatches)
        => currentRealm < spellRealm || alreadyKnown ? -1
            : (rootMatches ? 2 : 0) + (techniqueMatches ? 1 : 0);

    internal static int[] SelectIndices(IReadOnlyList<int> priorities, int requestedCount, Func<int, int> rollForSlot)
    {
        if (priorities == null || rollForSlot == null || requestedCount <= 0) return Array.Empty<int>();
        bool[] selected = new bool[priorities.Count];
        List<int> result = new(Math.Min(requestedCount, priorities.Count));
        for (int slot = 0; slot < requestedCount; slot++)
        {
            int bestPriority = -1, candidates = 0;
            for (int i = 0; i < priorities.Count; i++)
            {
                if (selected[i] || priorities[i] < 0) continue;
                if (priorities[i] > bestPriority) { bestPriority = priorities[i]; candidates = 1; }
                else if (priorities[i] == bestPriority) candidates++;
            }
            if (candidates == 0) break;
            int pick = (rollForSlot(slot) & int.MaxValue) % candidates;
            for (int i = 0; i < priorities.Count; i++)
            {
                if (selected[i] || priorities[i] != bestPriority) continue;
                if (pick-- != 0) continue;
                selected[i] = true;
                result.Add(i);
                break;
            }
        }
        return result.ToArray();
    }
}
