using System;
using MySimulatedLongevityRoad.Core;
using MySimulatedLongevityRoad.Data;
using MySimulatedLongevityRoad.Traits;

namespace MySimulatedLongevityRoad.Systems;

internal static class MclslAncientLawEntrySystem
{
    internal static bool ShouldAttemptMortalEntry(Actor actor, int year)
    {
        if (actor?.data == null) return false;
        if (!MclslEligibility.CanCultivate(actor)) return false;
        bool hasGift = MclslSpiritualRootSystem.HasCultivationPotential(actor);
        string system = MclslActorAccessor.GetString(actor, MclslActorDataKeys.CultivationSystem, string.Empty);
        if (system == MclslCultivationSystemIds.AncientLaw && string.IsNullOrWhiteSpace(MclslActorAccessor.Realm(actor))) return hasGift;
        if (MclslWorldEpochSystem.IsNewLawActive(year)) return false;
        if (hasGift) return true;
        int age = SafeAgeYear(actor);
        if (age != 5) return false;
        return MclslActorAccessor.GetInt(actor, MclslActorDataKeys.ChildhoodRootChecked, 0) == 1
            && hasGift;
    }

    internal static bool TryBeginFromMortal(Actor actor, int year)
    {
        if (!ShouldAttemptMortalEntry(actor, year)) return false;
        if (MclslNewLawPioneerSystem.TryBeginMortalPioneer(actor, year)) return true;
        BeginAncientCultivation(actor, year);
        return true;
    }

    internal static bool TryBeginFromKnownRoot(Actor actor, int year)
    {
        if (actor?.data == null) return false;
        if (!MclslEligibility.CanCultivate(actor)) return false;
        if (MclslWorldEpochSystem.IsNewLawActive(year)) return false;
        if (!string.IsNullOrWhiteSpace(MclslActorAccessor.Realm(actor))) return false;
        if (!MclslSpiritualRootSystem.HasCultivationPotential(actor)) return false;
        if (MclslNewLawPioneerSystem.TryBeginMortalPioneer(actor, year)) return true;
        BeginAncientCultivation(actor, year);
        return true;
    }

    private static void BeginAncientCultivation(Actor actor, int year)
    {
        if (!MclslEligibility.CanCultivate(actor)) return;
        string existingSystem = MclslActorAccessor.GetString(actor, MclslActorDataKeys.CultivationSystem, string.Empty);
        bool alreadyInitialized = existingSystem == MclslCultivationSystemIds.AncientLaw
            && !string.IsNullOrWhiteSpace(MclslActorAccessor.GetString(actor, MclslActorDataKeys.TechniqueId, string.Empty));
        if (!alreadyInitialized)
        {
            long id = MclslActorAccessor.Id(actor);
            int aptitude = Math.Clamp(MclslActorAccessor.GetInt(actor, MclslActorDataKeys.Aptitude, 0), 0, 100);
            if (aptitude <= 0) aptitude = RollAncientAptitude(id);
            string techniqueSeed = id > 0L ? id.ToString() : MclslActorAccessor.DisplayName(actor) + "|" + year;
            MclslTechniqueDefinition technique = MclslCultivationCatalog.StartingTechnique(techniqueSeed, aptitude, true);
            MclslCultivationStateTransitions.TrySetCultivationSystem(actor, MclslCultivationSystemIds.AncientLaw);
            if (MclslActorAccessor.GetInt(actor, MclslActorDataKeys.CultivationStartYear, 0) <= 0)
                MclslActorAccessor.Set(actor, MclslActorDataKeys.CultivationStartYear, Math.Max(0, year));
            MclslActorAccessor.Set(actor, MclslActorDataKeys.AncientLawStatus, "仙道正修");
            MclslActorAccessor.Set(actor, MclslActorDataKeys.AncientLineageStrength, 35 + aptitude / 2);
            MclslActorAccessor.Set(actor, MclslActorDataKeys.AncientLegacyPotential, 20 + aptitude / 4);
            MclslActorAccessor.Set(actor, MclslActorDataKeys.Aptitude, aptitude);
            MclslActorAccessor.Set(actor, MclslActorDataKeys.TechniqueId, "spiritual_" + technique.Id);
            MclslActorAccessor.Set(actor, MclslActorDataKeys.TechniqueName, technique.Name);
            MclslTechniqueRealmLimit.EnsureFromDefinition(actor, technique);
            MclslMindSystem.EnsureMindState(actor);
            MclslTechniqueStageSystem.SetProgress(actor, Math.Clamp(10 + aptitude / 8, 10, 28));
            MclslTraitRegistration.SyncGiftTrait(actor, aptitude);
            MclslActorAccessor.Set(actor, MclslActorDataKeys.LastBreakthroughResult, "灵根感应，开始感气");
        }
        MclslActorAccessor.ApplyDisplayName(actor, string.Empty);
    }

    private static int RollAncientAptitude(long actorId)
    {
        return MclslAptitudeGiftCatalog.RollAptitude(actorId + "|ancient_apt");
    }

    private static int SafeAgeYear(Actor actor)
    {
        try { return Math.Max(0, (int)Math.Floor(actor?.getAge() ?? 0f)); }
        catch { return 0; }
    }
}
