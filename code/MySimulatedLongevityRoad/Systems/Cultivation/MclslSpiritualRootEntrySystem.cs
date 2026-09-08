using System;
using MySimulatedLongevityRoad.Core;
using MySimulatedLongevityRoad.Data;
using MySimulatedLongevityRoad.Traits;

namespace MySimulatedLongevityRoad.Systems;

internal static class MclslSpiritualRootEntrySystem
{
    internal static bool TryEnterFromGiftTrait(Actor actor, int year)
    {
        long actorId = MclslActorAccessor.Id(actor);
        MclslDiagnostics.Cultivation(
            "entry.enter",
            "actor=" + actorId
            + " year=" + year
            + " realm=" + MclslActorAccessor.Realm(actor)
            + " system=" + MclslActorAccessor.GetString(actor, MclslActorDataKeys.CultivationSystem, string.Empty)
            + " hasRoot=" + MclslSpiritualRootSystem.HasCultivationPotential(actor));
        if (actor?.data == null || !MclslEligibility.CanCultivate(actor))
        {
            MclslDiagnostics.Cultivation("entry.skip", "actor=" + actorId + " year=" + year + " reason=data-or-eligibility");
            return false;
        }
        if (MclslTraitRegistration.TryBestRealmFromTraits(actor, out _))
        {
            MclslTraitRegistration.ReconcileTraitState(actor);
            MclslDiagnostics.Cultivation("entry.skip", "actor=" + actorId + " year=" + year + " reason=realm-trait-present");
            return false;
        }
        if (!string.IsNullOrWhiteSpace(MclslActorAccessor.Realm(actor)))
        {
            MclslDiagnostics.Cultivation("entry.skip", "actor=" + actorId + " year=" + year + " reason=realm-present");
            return false;
        }

        MclslAptitudeGiftDefinition gift = MclslSpiritualRootSystem.GiftForCultivation(actor);
        if (gift == null)
        {
            MclslDiagnostics.Cultivation("entry.skip", "actor=" + actorId + " year=" + year + " reason=no-gift");
            return false;
        }

        MclslSpiritualRootProfile profile = MclslSpiritualRootSystem.Profile(actor);
        int purity = Math.Clamp(profile.Purity, gift.MinAptitude, gift.MaxAptitude);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.Aptitude, purity);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.ImmortalFate, Math.Clamp(purity, 20, 100));
        MclslMindSystem.EnsureMindState(actor);

        MclslActorAccessor.Set(actor, MclslActorDataKeys.ChildhoodRootChecked, 1);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.MortalSeparationChecked, 1);
        if (MclslWorldEpochSystem.IsNewLawActive(year))
        {
            MclslNewLawEntrySystem.BeginCultivation(actor, year, "灵根显现，开始感气");
            MclslDiagnostics.Cultivation("entry.newlaw", "actor=" + actorId + " year=" + year + " gift=" + gift.Name);
            return true;
        }
        if (MclslNewLawPioneerSystem.TryBeginMortalPioneer(actor, year))
        {
            MclslDiagnostics.Cultivation("entry.pioneer", "actor=" + actorId + " year=" + year + " gift=" + gift.Name);
            return true;
        }
        bool ancient = MclslAncientLawEntrySystem.TryBeginFromKnownRoot(actor, year);
        MclslDiagnostics.Cultivation(
            "entry.ancient",
            "actor=" + actorId
            + " year=" + year
            + " gift=" + gift.Name
            + " success=" + ancient
            + " systemAfter=" + MclslActorAccessor.GetString(actor, MclslActorDataKeys.CultivationSystem, string.Empty));
        return ancient;
    }
}
