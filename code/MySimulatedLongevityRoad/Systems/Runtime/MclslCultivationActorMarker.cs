using MySimulatedLongevityRoad.Data;
using MySimulatedLongevityRoad.Traits;

namespace MySimulatedLongevityRoad.Systems;

internal static class MclslCultivationActorMarker
{
    internal static bool ShouldKeepTracked(Actor actor)
    {
        if (actor?.data == null) return false;
        if (!MclslActorAccessor.Alive(actor)) return false;
        if (HasWorldSoulMarker(actor)) return true;
        if (!MclslEligibility.CanCultivate(actor)) return false;
        return HasCultivationMarker(actor)
            || MclslChildhoodRootSystem.ShouldTrackChildhoodCandidate(actor)
            || HasManualTraitMarker(actor);
    }

    internal static bool IsAnnualCandidate(Actor actor)
    {
        if (actor?.data == null) return false;
        if (!MclslActorAccessor.Alive(actor)) return false;
        if (!MclslEligibility.CanCultivate(actor)) return false;
        bool hasGift = MclslSpiritualRootSystem.HasCultivationPotential(actor);
        return HasCultivationMarker(actor)
            || MclslChildhoodRootSystem.ShouldTrackChildhoodCandidate(actor)
            || hasGift
            || HasManualTraitMarker(actor);
    }

    internal static bool HasWorldSoulMarker(Actor actor)
    {
        if (!string.IsNullOrWhiteSpace(MclslActorAccessor.GetString(actor, MclslActorDataKeys.WorldSoulEntityId, string.Empty))) return true;
        try { return actor.hasTrait(MclslTraitRegistration.WorldSoulEntityTraitId); }
        catch { return false; }
    }

    internal static bool HasCultivationMarker(Actor actor)
    {
        if (actor?.data == null) return false;
        if (!string.IsNullOrWhiteSpace(MclslActorAccessor.Realm(actor))) return true;
        if (MclslSpiritualRootSystem.HasCultivationPotential(actor)) return true;
        if (MclslActorAccessor.GetInt(actor, MclslActorDataKeys.ManualGrant, 0) > 0) return true;
        return false;
    }

    private static bool HasManualTraitMarker(Actor actor)
    {
        try
        {
            return actor.hasTrait(MclslTraitRegistration.HuanzhenTraitId)
                || actor.hasTrait(MclslTraitRegistration.WorldSoulEntityTraitId);
        }
        catch
        {
            return false;
        }
    }
}
