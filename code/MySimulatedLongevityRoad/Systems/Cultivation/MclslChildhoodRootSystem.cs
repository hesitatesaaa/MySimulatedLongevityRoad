using System;
using MySimulatedLongevityRoad.Data;
using MySimulatedLongevityRoad.Traits;

namespace MySimulatedLongevityRoad.Systems;

internal static class MclslChildhoodRootSystem
{
    private const int RootCheckAge = 5;
    private const int RootCheckRecoveryAge = 6;

    internal static void ProcessAnnual(Actor actor, int year)
    {
        if (actor?.data == null) return;
        if (!MclslEligibility.CanCultivate(actor)) return;
        if (MclslActorAccessor.GetInt(actor, MclslActorDataKeys.ChildhoodRootChecked, 0) == 1) return;

        int age = SafeAgeYear(actor);
        if (!IsRootCheckAgeDeadlineYear(age)) return;

        MclslActorAccessor.Set(actor, MclslActorDataKeys.ChildhoodRootChecked, 1);
        if (!HasSpiritualRoot(actor, year))
        {
            MclslActorAccessor.Set(actor, MclslActorDataKeys.MortalSeparationChecked, 1);
            MclslActorAccessor.Set(actor, MclslActorDataKeys.ImmortalFate, 0);
            MclslActorAccessor.Set(actor, MclslActorDataKeys.LastBreakthroughResult, "幼年灵根未显。");
            return;
        }

        int aptitude = RollAptitude(actor);
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
        if (MclslActorAccessor.GetInt(actor, MclslActorDataKeys.ChildhoodRootChecked, 0) == 1) return false;
        return IsRootCheckAgeDeadline(actor);
    }

    internal static bool IsRootCheckAge(Actor actor)
    {
        return IsRootCheckAgeYear(SafeAgeYear(actor));
    }

    internal static bool IsRootCheckAgeDeadline(Actor actor)
    {
        return IsRootCheckAgeDeadlineYear(SafeAgeYear(actor));
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
        return MclslAptitudeGiftCatalog.RollQualification(MclslActorAccessor.Id(actor) + "|childhood_root|" + ageFiveSeed);
    }

    private static bool IsRootCheckAgeYear(int ageYear)
    {
        return ageYear == RootCheckAge;
    }

    private static bool IsRootCheckAgeDeadlineYear(int ageYear)
    {
        // WorldBox 高倍速下 updateAge 可能直接从四岁跨到六岁。
        // 六岁只作为五岁判定的容错窗口，随机种子仍固定用五岁。
        return ageYear == RootCheckAge || ageYear == RootCheckRecoveryAge;
    }

    private static int RollAptitude(Actor actor, string salt = "childhood_apt")
    {
        long id = MclslActorAccessor.Id(actor);
        return MclslAptitudeGiftCatalog.RollAptitude(id + "|" + salt);
    }

    private static int SafeAgeYear(Actor actor)
    {
        try { return Math.Max(0, (int)Math.Floor(actor?.getAge() ?? 0f)); }
        catch { return 0; }
    }

    private static int PositiveHash(string value)
    {
        unchecked
        {
            int hash = 29;
            foreach (char c in value ?? string.Empty) hash = hash * 43 + c;
            return hash & int.MaxValue;
        }
    }
}
