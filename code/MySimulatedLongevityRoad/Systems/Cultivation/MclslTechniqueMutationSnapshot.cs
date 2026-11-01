using MySimulatedLongevityRoad.Data;

namespace MySimulatedLongevityRoad.Systems;

/// <summary>
/// Rollback boundary for paid technique changes. The wallet commit happens only
/// after these native actor fields have been written successfully.
/// </summary>
internal sealed class MclslTechniqueMutationSnapshot
{
    private readonly string _affiliation;
    private readonly string _techniqueId;
    private readonly string _techniqueName;
    private readonly string _techniqueMaxRealm;
    private readonly int _comprehension;
    private readonly int _insight;
    private readonly int _trueEssence;
    private readonly int _cultivationProgress;
    private readonly float _trueEssenceRemainder;

    private MclslTechniqueMutationSnapshot(Actor actor)
    {
        _affiliation = MclslActorAccessor.GetString(actor, MclslActorDataKeys.FactionAffiliation, string.Empty);
        _techniqueId = MclslActorAccessor.GetString(actor, MclslActorDataKeys.TechniqueId, string.Empty);
        _techniqueName = MclslActorAccessor.GetString(actor, MclslActorDataKeys.TechniqueName, string.Empty);
        _techniqueMaxRealm = MclslActorAccessor.GetString(actor, MclslActorDataKeys.TechniqueMaxRealm, string.Empty);
        _comprehension = MclslActorAccessor.GetInt(actor, MclslActorDataKeys.AncientTechniqueComprehension, 0);
        _insight = MclslActorAccessor.GetInt(actor, MclslActorDataKeys.TechniqueInsight, 0);
        _trueEssence = MclslActorAccessor.GetInt(actor, MclslActorDataKeys.TrueEssence, 0);
        _cultivationProgress = MclslActorAccessor.GetInt(actor, MclslActorDataKeys.CultivationProgress, 0);
        _trueEssenceRemainder = MclslActorAccessor.GetFloat(actor, MclslActorDataKeys.TrueEssenceRemainder, 0f);
    }

    internal static MclslTechniqueMutationSnapshot Capture(Actor actor) => new(actor);

    internal void Restore(Actor actor)
    {
        MclslActorAccessor.Set(actor, MclslActorDataKeys.FactionAffiliation, _affiliation);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.TechniqueId, _techniqueId);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.TechniqueName, _techniqueName);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.TechniqueMaxRealm, _techniqueMaxRealm);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.AncientTechniqueComprehension, _comprehension);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.TechniqueInsight, _insight);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.TrueEssence, _trueEssence);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.TrueEssenceRemainder, _trueEssenceRemainder);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.CultivationProgress, _cultivationProgress);
    }
}
