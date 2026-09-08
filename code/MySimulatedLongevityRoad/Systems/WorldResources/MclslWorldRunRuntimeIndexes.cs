using System;
using System.Collections.Generic;
using MySimulatedLongevityRoad.Data;

namespace MySimulatedLongevityRoad.Systems;

internal static class MclslWorldRunRuntimeIndexes
{
    private static MclslWorldRunState _indexedRun;
    private static readonly Dictionary<string, MclslWorldCaveRecord> CavesById = new(StringComparer.Ordinal);
    private static readonly Dictionary<string, MclslWorldChangeRecord> ChangesById = new(StringComparer.Ordinal);
    private static readonly Dictionary<string, MclslSectRuinRecord> RuinsById = new(StringComparer.Ordinal);
    private static readonly HashSet<string> GeneratedItemIds = new(StringComparer.Ordinal);
    private static readonly HashSet<string> UsedGeneratedNames = new(StringComparer.Ordinal);

    internal static void Invalidate()
    {
        _indexedRun = null;
        CavesById.Clear();
        ChangesById.Clear();
        RuinsById.Clear();
        GeneratedItemIds.Clear();
        UsedGeneratedNames.Clear();
    }

    internal static MclslWorldCaveRecord FindCave(MclslWorldRunState run, string id)
    {
        if (string.IsNullOrWhiteSpace(id)) return null;
        Ensure(run);
        return CavesById.TryGetValue(id, out MclslWorldCaveRecord cave) ? cave : null;
    }

    internal static MclslWorldChangeRecord FindWorldChange(MclslWorldRunState run, string id)
    {
        if (string.IsNullOrWhiteSpace(id)) return null;
        Ensure(run);
        return ChangesById.TryGetValue(id, out MclslWorldChangeRecord change) ? change : null;
    }

    internal static MclslSectRuinRecord FindSectRuin(MclslWorldRunState run, string id)
    {
        if (string.IsNullOrWhiteSpace(id)) return null;
        Ensure(run);
        return RuinsById.TryGetValue(id, out MclslSectRuinRecord ruin) ? ruin : null;
    }

    internal static bool HasGeneratedItem(MclslWorldRunState run, string id)
    {
        if (string.IsNullOrWhiteSpace(id)) return false;
        Ensure(run);
        return GeneratedItemIds.Contains(id);
    }

    internal static bool TryReserveGeneratedName(MclslWorldRunState run, string name)
    {
        if (run == null || string.IsNullOrWhiteSpace(name)) return false;
        run.UsedGeneratedNames ??= new List<string>();
        Ensure(run);
        if (!UsedGeneratedNames.Add(name)) return false;
        run.UsedGeneratedNames.Add(name);
        return true;
    }

    internal static void RegisterGeneratedName(MclslWorldRunState run, string name)
    {
        if (run == null || string.IsNullOrWhiteSpace(name)) return;
        run.UsedGeneratedNames ??= new List<string>();
        Ensure(run);
        if (!UsedGeneratedNames.Add(name)) return;
        run.UsedGeneratedNames.Add(name);
    }

    private static void Ensure(MclslWorldRunState run)
    {
        if (run == null)
        {
            Invalidate();
            return;
        }
        if (ReferenceEquals(_indexedRun, run)) return;

        Invalidate();
        _indexedRun = run;

        if (run.WorldCaves != null)
        {
            foreach (MclslWorldCaveRecord cave in run.WorldCaves)
                if (cave != null && !string.IsNullOrWhiteSpace(cave.Id))
                    CavesById[cave.Id] = cave;
        }

        if (run.WorldChanges != null)
        {
            foreach (MclslWorldChangeRecord change in run.WorldChanges)
                if (change != null && !string.IsNullOrWhiteSpace(change.Id))
                    ChangesById[change.Id] = change;
        }

        if (run.SectRuins != null)
        {
            foreach (MclslSectRuinRecord ruin in run.SectRuins)
                if (ruin != null && !string.IsNullOrWhiteSpace(ruin.Id))
                    RuinsById[ruin.Id] = ruin;
        }

        if (run.GeneratedItems != null)
        {
            foreach (MclslGeneratedItemRecord item in run.GeneratedItems)
                if (item != null && !string.IsNullOrWhiteSpace(item.Id))
                    GeneratedItemIds.Add(item.Id);
        }

        if (run.UsedGeneratedNames != null)
        {
            foreach (string name in run.UsedGeneratedNames)
                if (!string.IsNullOrWhiteSpace(name))
                    UsedGeneratedNames.Add(name);
        }
    }
}
