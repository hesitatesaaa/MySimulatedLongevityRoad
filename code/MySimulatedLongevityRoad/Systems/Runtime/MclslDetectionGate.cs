using System;
using System.Collections.Generic;
using MySimulatedLongevityRoad.Data;

namespace MySimulatedLongevityRoad.Systems;

internal static class MclslDetectionGate
{
    private static readonly Dictionary<string, int> LastAnnualJobYearByKey = new(StringComparer.Ordinal);
    private static readonly HashSet<string> AnnualJobKeysThisYear = new(StringComparer.Ordinal);
    private static int _activeAnnualYear;

    internal const string AnnualWorldPreparation = "annual.world.preparation";
    internal const string AnnualFactionMission = "annual.faction_mission";
    internal const string AnnualFactionPressure = "annual.faction_pressure";
    internal const string AnnualInverseTruth = "annual.inverse_truth";
    internal const string RuntimeCandidateCleanup = "runtime.candidate.cleanup";
    internal const string ResourceAutoSpend = "resource.auto_spend";
    internal const string FactionTechniqueExchange = "faction.technique_exchange";

    internal static bool TryBeginAnnualJob(string key, int year, int intervalYears = 1)
    {
        if (string.IsNullOrWhiteSpace(key) || year <= 0) return false;
        if (_activeAnnualYear != year)
        {
            _activeAnnualYear = year;
            AnnualJobKeysThisYear.Clear();
        }

        if (!AnnualJobKeysThisYear.Add(key)) return false;
        intervalYears = Math.Max(1, intervalYears);
        if (LastAnnualJobYearByKey.TryGetValue(key, out int lastYear) && year - lastYear < intervalYears)
            return false;

        LastAnnualJobYearByKey[key] = year;
        return true;
    }

    internal static bool ShouldProcessAnnualActors(in MclslSchedulerContext context)
    {
        return context.IsYearChange || context.ProcessFast;
    }

    internal static bool ShouldRunCadence(string key, int tickCounter, int intervalTicks)
    {
        return !string.IsNullOrWhiteSpace(key)
            && intervalTicks > 0
            && tickCounter > 0
            && tickCounter % intervalTicks == 0;
    }

    internal static bool TryEnterActorAttempt(Actor actor, string scope, string key, int year, int intervalYears, bool mark = true)
    {
        if (!CanRunActorAttempt(actor, scope, key, year, intervalYears)) return false;
        if (mark) MarkActorAttempt(actor, scope, key, year);
        return true;
    }

    internal static bool CanRunActorAttempt(Actor actor, string scope, string key, int year, int intervalYears)
    {
        if (actor?.data == null || string.IsNullOrWhiteSpace(scope) || string.IsNullOrWhiteSpace(key)) return false;
        int last = MclslActorAccessor.GetInt(actor, AttemptKey(scope, key), -999999);
        return last < 0 || year - last >= Math.Max(1, intervalYears);
    }

    internal static void MarkActorAttempt(Actor actor, string scope, string key, int year)
    {
        if (actor?.data == null || string.IsNullOrWhiteSpace(scope) || string.IsNullOrWhiteSpace(key)) return;
        MclslActorAccessor.Set(actor, AttemptKey(scope, key), Math.Max(0, year));
    }

    internal static bool DeterministicRoll(Actor actor, string scope, string key, int year, int chance, int denominator = 1000)
    {
        if (actor?.data == null || chance <= 0) return false;
        if (chance >= denominator) return true;
        int roll = PositiveHash(MclslActorAccessor.Id(actor) + "|" + scope + "|" + key + "|" + year) % Math.Max(1, denominator);
        return roll < chance;
    }

    private static string AttemptKey(string scope, string key)
    {
        return MclslActorDataKeys.DetectionLastAttemptPrefix + Normalize(scope) + "." + Normalize(key);
    }

    private static string Normalize(string value)
    {
        return (value ?? string.Empty).Trim().Replace(' ', '_').Replace('|', '_').Replace(':', '_');
    }

    private static int PositiveHash(string value)
    {
        unchecked
        {
            int hash = 97;
            foreach (char c in value ?? string.Empty) hash = hash * 41 + c;
            return hash & int.MaxValue;
        }
    }

    internal static void ClearRuntimeState()
    {
        LastAnnualJobYearByKey.Clear();
        AnnualJobKeysThisYear.Clear();
        _activeAnnualYear = 0;
    }
}
