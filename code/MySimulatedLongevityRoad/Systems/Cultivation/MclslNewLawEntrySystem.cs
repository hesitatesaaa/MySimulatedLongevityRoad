using System;
using MySimulatedLongevityRoad.Core;
using MySimulatedLongevityRoad.Data;
using MySimulatedLongevityRoad.Traits;

namespace MySimulatedLongevityRoad.Systems;

internal static class MclslNewLawEntrySystem
{
    private const string DefaultEntryReason = "感应天地灵气，开始感气";

    internal static bool ShouldAttemptMortalEntry(Actor actor, int year)
    {
        if (actor?.data == null) return false;
        if (!MclslEligibility.CanCultivate(actor)) return false;
        bool hasGift = MclslSpiritualRootSystem.HasCultivationPotential(actor);
        string system = MclslActorAccessor.GetString(actor, MclslActorDataKeys.CultivationSystem, string.Empty);
        if (system == MclslCultivationSystemIds.NewLaw && string.IsNullOrWhiteSpace(MclslActorAccessor.Realm(actor))) return hasGift;
        if (hasGift) return true;
        int age = SafeAgeYear(actor);
        if (age == 5
            && MclslActorAccessor.GetInt(actor, MclslActorDataKeys.ChildhoodRootChecked, 0) == 1
            && hasGift) return true;
        return false;
    }

    internal static bool TryBeginFromMortal(Actor actor, int year)
    {
        if (actor?.data == null) return false;
        if (!MclslEligibility.CanCultivate(actor)) return false;
        string system = MclslActorAccessor.GetString(actor, MclslActorDataKeys.CultivationSystem, string.Empty);
        bool hasGift = MclslSpiritualRootSystem.HasCultivationPotential(actor);
        if (system == MclslCultivationSystemIds.NewLaw && string.IsNullOrWhiteSpace(MclslActorAccessor.Realm(actor)))
        {
            if (!hasGift) return false;
            EnsureInitialized(actor, year, DefaultEntryReason, 0);
            return true;
        }

        if (hasGift)
        {
            MclslActorAccessor.Set(actor, MclslActorDataKeys.ChildhoodRootChecked, 1);
            MclslActorAccessor.Set(actor, MclslActorDataKeys.MortalSeparationChecked, 1);
            BeginCultivation(actor, year, "灵根显现，开始感气");
            return true;
        }
        int age = SafeAgeYear(actor);
        if (age == 5
            && MclslActorAccessor.GetInt(actor, MclslActorDataKeys.ChildhoodRootChecked, 0) == 1
            && hasGift)
        {
            BeginCultivation(actor, year);
            return true;
        }
        return false;
    }

    internal static void BeginCultivation(Actor actor, int year, string reason = DefaultEntryReason, int aptitudeBonus = 0)
    {
        if (!MclslEligibility.CanCultivate(actor)) return;
        EnsureInitialized(actor, year, reason, aptitudeBonus);
    }

    private static void EnsureInitialized(Actor actor, int year, string reason, int aptitudeBonus)
    {
        if (actor?.data == null) return;
        if (!MclslEligibility.CanCultivate(actor)) return;
        string existingSystem = MclslActorAccessor.GetString(actor, MclslActorDataKeys.CultivationSystem, string.Empty);
        bool alreadyInitialized = existingSystem == MclslCultivationSystemIds.NewLaw
            && !string.IsNullOrWhiteSpace(MclslActorAccessor.GetString(actor, MclslActorDataKeys.TechniqueId, string.Empty));
        if (alreadyInitialized)
        {
            MclslActorAccessor.ApplyDisplayName(actor, MclslActorAccessor.Realm(actor));
            return;
        }

        long id = MclslActorAccessor.Id(actor);
        int aptitude = MclslActorAccessor.GetInt(actor, MclslActorDataKeys.Aptitude, 0);
        if (aptitude <= 0) aptitude = 20 + PositiveHash(id + "|apt") % 81;
        int fate = Math.Max(1, MclslActorAccessor.GetInt(actor, MclslActorDataKeys.ImmortalFate, 20));
        if (MclslActorAccessor.GetInt(actor, MclslActorDataKeys.ChildhoodRootChecked, 0) == 1)
            aptitude = Math.Clamp(aptitude + aptitudeBonus, 1, 100);
        else
            aptitude = Math.Clamp((aptitude + fate) / 2 + aptitudeBonus, 1, 100);
        string techniqueSeed = id > 0L ? id.ToString() : MclslActorAccessor.DisplayName(actor) + "|" + year;
        bool inheritedTechnique = MclslTechniqueLineageSystem.TryPickInheritedTechnique(year, techniqueSeed, aptitude, out MclslTechniqueDefinition technique);
        if (technique == null)
            technique = MclslTechniqueOccupationSystem.SelectStartingTechnique(techniqueSeed, aptitude, false);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.Aptitude, aptitude);
        MclslCultivationStateTransitions.TrySetCultivationSystem(actor, MclslCultivationSystemIds.NewLaw);
        if (MclslActorAccessor.GetInt(actor, MclslActorDataKeys.CultivationStartYear, 0) <= 0)
            MclslActorAccessor.Set(actor, MclslActorDataKeys.CultivationStartYear, Math.Max(0, year));
        MclslMindSystem.EnsureMindState(actor);
        MclslTraitRegistration.SyncGiftTrait(actor, aptitude);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.TechniqueId, technique.Id);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.TechniqueName, technique.Name);
        MclslTechniqueRealmLimit.EnsureFromDefinition(actor, technique);
        if (inheritedTechnique)
        {
            MclslActorAccessor.Set(actor, MclslActorDataKeys.AncientLegacyPotential,
                Math.Min(100, MclslActorAccessor.GetInt(actor, MclslActorDataKeys.AncientLegacyPotential, 0) + 18));
            MclslActorAccessor.Set(actor, MclslActorDataKeys.TechniqueInsight,
                MclslActorAccessor.GetInt(actor, MclslActorDataKeys.TechniqueInsight, 0) + 8);
        }
        MclslActorAccessor.Set(actor, MclslActorDataKeys.Contribution, 20);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.LastBreakthroughResult, reason ?? DefaultEntryReason);
        MclslActorAccessor.ApplyDisplayName(actor, string.Empty);
    }

    private static int PositiveHash(string value)
    {
        unchecked { int hash = 23; foreach (char c in value ?? string.Empty) hash = hash * 37 + c; return hash & int.MaxValue; }
    }

    private static int SafeAgeYear(Actor actor)
    {
        try { return Math.Max(0, (int)Math.Floor(actor?.getAge() ?? 0f)); }
        catch { return 0; }
    }
}
