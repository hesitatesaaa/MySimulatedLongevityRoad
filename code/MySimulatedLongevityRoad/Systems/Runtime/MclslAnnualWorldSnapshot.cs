using System.Collections.Generic;
using MySimulatedLongevityRoad.Data;

namespace MySimulatedLongevityRoad.Systems;

internal sealed class MclslAnnualWorldSnapshot
{
    private static readonly Actor[] EmptyActors = System.Array.Empty<Actor>();

    internal IReadOnlyList<Actor> LineageActors { get; }

    private MclslAnnualWorldSnapshot(IReadOnlyList<Actor> lineageActors)
    {
        LineageActors = lineageActors ?? EmptyActors;
    }

    internal static MclslAnnualWorldSnapshot Build(IReadOnlyList<Actor> lineageActors)
    {
        return new MclslAnnualWorldSnapshot(BuildLineageSnapshot(lineageActors));
    }

    private static List<Actor> BuildLineageSnapshot(IReadOnlyList<Actor> actors)
    {
        if (actors == null || actors.Count == 0) return new List<Actor>(0);

        List<Actor> result = new(actors.Count);
        HashSet<long> seen = new();
        for (int i = 0; i < actors.Count; i++)
        {
            Actor actor = actors[i];
            if (!MclslHotPathPolicy.IsActorHotPathSafe(actor)) continue;
            if (!MclslEligibility.CanCultivate(actor)) continue;
            long id = MclslActorAccessor.Id(actor);
            if (id <= 0L || !seen.Add(id)) continue;
            result.Add(actor);
        }

        return result;
    }
}
