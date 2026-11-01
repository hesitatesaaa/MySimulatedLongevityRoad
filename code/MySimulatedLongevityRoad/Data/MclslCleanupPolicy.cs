namespace MySimulatedLongevityRoad.Data;

/// <summary>
/// Pure decision helpers for persistent archive cleanup (family history and
/// per-actor death history). Kept free of runtime state so the offline
/// verification project can compile them directly from the source tree.
/// </summary>
internal static class MclslCleanupPolicy
{
    /// <summary>
    /// New worlds initialise the marker to true; an existing v24 archive without
    /// the field deserialises to null and must stay off so an old save never
    /// starts deleting history retroactively.
    /// </summary>
    internal static bool ResolvePersistentCleanup(bool? marker) => marker == true;

    /// <summary>
    /// Removes an extinct family record only when its estate is fully settled and
    /// no member is still living or missing. The legacy per-family flag keeps
    /// pre-marker archives behaving exactly as they did before the switch existed.
    /// </summary>
    internal static bool ShouldRemoveExtinctFamily(
        bool persistentCleanupEnabled,
        bool legacyRemoveAfterLiquidation,
        bool extinct,
        bool estateLiquidated,
        bool hasLivingOrMissingMember,
        bool hasUnsettledDeath)
    {
        if (!extinct || hasLivingOrMissingMember || !estateLiquidated || hasUnsettledDeath) return false;
        return persistentCleanupEnabled || legacyRemoveAfterLiquidation;
    }
}
