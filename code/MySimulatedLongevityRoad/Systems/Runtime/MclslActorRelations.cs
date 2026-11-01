using System;
using System.Collections.Generic;

namespace MySimulatedLongevityRoad.Systems;

// Membership is maintained by native lifecycle notifications. No consumer has
// to scan the world when a cultivator founds a family or a subspecies evolves.
internal static class MclslActorRelations
{
    private readonly struct Relation
    {
        internal readonly long Family, First, Second;
        internal readonly Subspecies Species;
        internal Relation(Actor actor)
        {
            Family = actor.data.family > 0 ? actor.data.family : actor.data.ancestor_family;
            First = actor.data.parent_id_1; Second = actor.data.parent_id_2; Species = actor.subspecies;
        }
    }
    private static readonly Dictionary<long, Relation> ByActor = new();
    private static readonly Dictionary<long, HashSet<long>> Families = new(), Children = new();
    private static readonly Dictionary<Subspecies, HashSet<long>> Species = new();
    internal static IEnumerable<long> Family(long id) => Families.TryGetValue(id, out var ids) ? ids : Array.Empty<long>();
    internal static IEnumerable<long> Descendants(long id) => Children.TryGetValue(id, out var ids) ? ids : Array.Empty<long>();
    internal static IEnumerable<long> SubspeciesMembers(Subspecies species)
        => species != null && Species.TryGetValue(species, out var ids) ? ids : Array.Empty<long>();
    private static void Add(Dictionary<long, HashSet<long>> map, long key, long id)
    { if (key <= 0) return; if (!map.TryGetValue(key, out var ids)) map[key] = ids = new(); ids.Add(id); }
    private static void Remove(Dictionary<long, HashSet<long>> map, long key, long id)
    { if (!map.TryGetValue(key, out var ids)) return; ids.Remove(id); if (ids.Count == 0) map.Remove(key); }
    internal static void Refresh(Actor actor)
    {
        long id = actor.data.id;
        var next = new Relation(actor);
        if (ByActor.TryGetValue(id, out var old) && old.Family == next.Family && old.First == next.First
            && old.Second == next.Second && ReferenceEquals(old.Species, next.Species)) return;
        Forget(id);
        ByActor[id] = next;
        Add(Families, next.Family, id); Add(Children, next.First, id); Add(Children, next.Second, id);
        if (next.Species != null)
        { if (!Species.TryGetValue(next.Species, out var ids)) Species[next.Species] = ids = new(); ids.Add(id); }
    }
    internal static void Forget(long id)
    {
        if (!ByActor.TryGetValue(id, out var old)) return;
        Remove(Families, old.Family, id); Remove(Children, old.First, id); Remove(Children, old.Second, id);
        if (old.Species != null && Species.TryGetValue(old.Species, out var ids))
        { ids.Remove(id); if (ids.Count == 0) Species.Remove(old.Species); }
        ByActor.Remove(id);
    }
    internal static void Clear() { ByActor.Clear(); Families.Clear(); Children.Clear(); Species.Clear(); }
}
