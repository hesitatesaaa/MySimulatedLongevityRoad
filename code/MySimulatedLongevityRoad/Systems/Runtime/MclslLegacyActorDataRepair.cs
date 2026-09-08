using System;
using MySimulatedLongevityRoad.Data;

namespace MySimulatedLongevityRoad.Systems;

/// <summary>
/// Repairs only identifiers written by the abandoned 0.1.2 law-seat prototype.
/// Normal actors pay one string comparison and are left untouched.
/// </summary>
internal static class MclslLegacyActorDataRepair
{
    internal static void RepairTechnique(Actor actor)
    {
        if (actor?.data == null) return;
        string storedId = MclslActorAccessor.GetString(actor, MclslActorDataKeys.TechniqueId, string.Empty);
        if (string.IsNullOrWhiteSpace(storedId)) return;

        string normalizedId = MclslCultivationCatalog.NormalizeTechniqueId(storedId);
        if (string.Equals(storedId, normalizedId, StringComparison.Ordinal)) return;

        MclslActorAccessor.Set(actor, MclslActorDataKeys.TechniqueId, normalizedId);
        if (!MclslCultivationCatalog.TryTechnique(normalizedId, out MclslTechniqueDefinition definition)) return;
        MclslActorAccessor.Set(actor, MclslActorDataKeys.TechniqueName, definition.Name);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.TechniqueMaxRealm, definition.MaxRealm);
    }
}
