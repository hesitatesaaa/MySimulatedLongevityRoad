using System;
using System.Collections.Generic;
using MySimulatedLongevityRoad.Core;
using MySimulatedLongevityRoad.Data;
using MySimulatedLongevityRoad.Traits;

namespace MySimulatedLongevityRoad.Systems;

internal static class MclslChildhoodRootSystem
{
    private const int RootCheckAge = 5;
    private const int RootCheckRecoveryAge = 6;

    private static SortedSet<(int Hash, long Id, int Year)> _pending = new();
    private static SortedSet<(int Hash, long Id, int Year)> _batch = new();
    private static readonly HashSet<long> Queued = new();
    internal static int Population { get; private set; }
    internal static int RootPopulation { get; private set; }
    internal static int TargetPopulation => (int)((long)Population * MclslRuntimeSettings.ChildhoodRootChancePercent / 100);
    internal static bool HasPending => Queued.Count > 0;

    private static byte ReadPopulation(Actor actor, out int age)
    {
        age = MclslActorAccessor.Alive(actor) ? MclslActorAccessor.Age(actor) : -1;
        if (age < RootCheckAge || MclslBeastActorRegistration.IsBeast(actor)
            || !actor.isSapient() || !MclslEligibility.CanCultivate(actor)) return 0;
        return MclslSpiritualRootSystem.HasActualSpiritualRoot(actor) ? (byte)3 : (byte)1;
    }

    private static void UpdatePopulation(MclslActorRegistry.Entry entry, byte state, int age)
    {
        Population += (state & 1) - (entry.RootPopulationState & 1);
        RootPopulation += ((state & 2) - (entry.RootPopulationState & 2)) / 2;
        entry.RootPopulationState = state;
        entry.RootAgeEligible = age >= RootCheckAge;
    }

    internal static void ObservePopulation(Actor actor)
    {
        long id = MclslActorAccessor.Id(actor);
        if (!MclslActorRegistry.ResolveEntry(id, out var entry)) return;
        byte state = ReadPopulation(actor, out int age);
        UpdatePopulation(entry, state, age);
        if (state != 0 && IsRootCheckAgeDeadlineYear(age)
            && MclslActorAccessor.GetInt(actor, MclslActorDataKeys.ChildhoodRootChecked, 0) != 1 && Queued.Add(id))
        {
            int year = MclslRuntime.CurrentYear();
            int hash = MclslStableHash.Positive43x53(MapBox.current_world_seed_id + "|childhood_quota|" + year + "|" + id);
            _pending.Add((hash, id, year));
        }
    }

    internal static void ForgetPopulation(long id)
    {
        if (!MclslActorRegistry.ResolveEntry(id, out var entry)) return;
        Population -= entry.RootPopulationState & 1;
        RootPopulation -= (entry.RootPopulationState & 2) / 2;
        entry.RootPopulationState = 0;
    }

    internal static void ClearPopulation()
    {
        Population = RootPopulation = 0;
        _pending.Clear(); _batch.Clear(); Queued.Clear();
    }

    internal static void TickQuota()
    {
        if (!MclslWorldBootstrapLane.Completed) return;
        if (_batch.Count == 0) { var batch = _batch; _batch = _pending; _pending = batch; }
        int processed = 0;
        while (_batch.Count > 0 && processed < 32 && MclslAnnualFrameBudget.TryConsumeOperation())
        {
            var candidate = _batch.Min;
            _batch.Remove(candidate);
            processed++;
            MclslAnnualBackpressure.RecordProgress();
            if (candidate.Year != MclslRuntime.CurrentYear())
            {
                int year = MclslRuntime.CurrentYear();
                int hash = MclslStableHash.Positive43x53(MapBox.current_world_seed_id + "|childhood_quota|" + year + "|" + candidate.Id);
                _pending.Add((hash, candidate.Id, year));
                continue;
            }
            try
            {
                if (!MclslActorRegistry.ResolveEntry(candidate.Id, out var entry)) continue;
                Actor actor = entry.Actor;
                byte state = ReadPopulation(actor, out int age);
                UpdatePopulation(entry, state, age);
                if (state == 0 || !IsRootCheckAgeDeadlineYear(age)
                    || MclslActorAccessor.GetInt(actor, MclslActorDataKeys.ChildhoodRootChecked, 0) == 1) continue;
                ApplyCheck(actor, (state & 2) != 0, RootPopulation < TargetPopulation);
                ObservePopulation(actor);
                if ((entry.RootPopulationState & 2) != 0)
                {
                    MclslSpiritualRootEntrySystem.TryEnterFromGiftTrait(actor, MclslRuntime.CurrentYear());
                    MclslScheduler.WakeAnnualCultivationActor(actor);
                }
            }
            finally { Queued.Remove(candidate.Id); }
        }
    }

    internal static void ProcessAnnual(Actor actor, int year)
    {
        if (!MclslBeastActorRegistration.IsBeast(actor)) return;
        // 妖兽保留原判定，不消耗智慧人口的自然灵根配额。
        CompleteCheck(actor, year, HasSpiritualRoot(actor, year));
    }

    private static void CompleteCheck(Actor actor, int year, bool grant)
    {
        if (actor?.data == null) return;
        if (!MclslEligibility.CanCultivate(actor)) return;
        if (MclslActorAccessor.GetInt(actor, MclslActorDataKeys.ChildhoodRootChecked, 0) == 1) return;

        int age = MclslActorAccessor.Age(actor);
        if (!IsRootCheckAgeDeadlineYear(age)) return;

        ApplyCheck(actor, MclslSpiritualRootSystem.HasActualSpiritualRoot(actor), grant);
    }

    private static void ApplyCheck(Actor actor, bool hasRoot, bool grant)
    {
        MclslActorAccessor.Set(actor, MclslActorDataKeys.ChildhoodRootChecked, 1);
        if (!hasRoot && !grant)
        {
            MclslActorAccessor.Set(actor, MclslActorDataKeys.MortalSeparationChecked, 1);
            MclslActorAccessor.Set(actor, MclslActorDataKeys.ImmortalFate, 0);
            MclslActorAccessor.Set(actor, MclslActorDataKeys.LastBreakthroughResult, "幼年灵根未显。");
            return;
        }

        int aptitude = MclslActorAccessor.GetInt(actor, MclslActorDataKeys.Aptitude, 0);
        if (aptitude < 1 || aptitude > 100) aptitude = RollAptitude(actor);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.Aptitude, aptitude);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.ImmortalFate, Math.Clamp(aptitude, 20, 100));
        MclslActorAccessor.Set(actor, MclslActorDataKeys.MortalSeparationChecked, 1);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.LastBreakthroughResult, "幼年灵根显现：" + MclslAptitudeGiftCatalog.ForAptitude(aptitude).Name);
        MclslTraitRegistration.SyncGiftTrait(actor, aptitude);
        MclslMindSystem.EnsureMindState(actor);
    }

    internal static bool TryProcessAgeFiveDeadline(Actor actor, int year)
    {
        if (actor?.data == null || !MclslEligibility.CanCultivate(actor)) return false;
        if (!IsRootCheckAgeDeadline(actor)) return false;
        if (MclslActorAccessor.GetInt(actor, MclslActorDataKeys.ChildhoodRootChecked, 0) == 1) return true;

        ProcessAnnual(actor, year);
        if (MclslActorAccessor.GetInt(actor, MclslActorDataKeys.ChildhoodRootChecked, 0) != 1) return false;
        if (MclslActorAccessor.GetInt(actor, MclslActorDataKeys.Aptitude, 0) <= 0) return true;

        MclslSpiritualRootEntrySystem.TryEnterFromGiftTrait(actor, year);
        return true;
    }

    internal static bool ShouldTrackChildhoodCandidate(Actor actor)
    {
        if (actor?.data == null) return false;
        if (!MclslEligibility.CanCultivate(actor)) return false;
        if (!MclslBeastActorRegistration.IsBeast(actor) && !actor.isSapient()) return false;
        if (MclslActorAccessor.GetInt(actor, MclslActorDataKeys.ChildhoodRootChecked, 0) == 1) return false;
        return IsRootCheckAgeDeadline(actor);
    }

    internal static bool IsRootCheckAge(Actor actor)
    {
        return IsRootCheckAgeYear(MclslActorAccessor.Age(actor));
    }

    internal static bool IsRootCheckAgeDeadline(Actor actor)
    {
        return IsRootCheckAgeDeadlineYear(MclslActorAccessor.Age(actor));
    }

    internal static int ExistingOrRollAptitude(Actor actor, string salt)
    {
        int current = MclslActorAccessor.GetInt(actor, MclslActorDataKeys.Aptitude, 0);
        if (current > 0) return Math.Clamp(current, 1, 100);
        int aptitude = RollAptitude(actor, salt);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.Aptitude, aptitude);
        MclslTraitRegistration.SyncGiftTrait(actor, aptitude);
        return aptitude;
    }

    private static bool HasSpiritualRoot(Actor actor, int year)
    {
        const int ageFiveSeed = 5;
        return MclslAptitudeGiftCatalog.RollQualification(MclslActorAccessor.Id(actor) + "|childhood_root|" + ageFiveSeed,
            MySimulatedLongevityRoad.Core.MclslRuntimeSettings.ChildhoodRootChancePercent);
    }

    private static bool IsRootCheckAgeYear(int ageYear)
    {
        return ageYear == RootCheckAge;
    }

    private static bool IsRootCheckAgeDeadlineYear(int ageYear)
    {
        // WorldBox 高倍速下 updateAge 可能直接从四岁跨到六岁。
        // 六岁只作为五岁判定的补处理窗口。
        return ageYear == RootCheckAge || ageYear == RootCheckRecoveryAge;
    }

    private static int RollAptitude(Actor actor, string salt = "childhood_apt")
    {
        long id = MclslActorAccessor.Id(actor);
        return MclslAptitudeGiftCatalog.RollAptitude(id + "|" + salt);
    }
}
