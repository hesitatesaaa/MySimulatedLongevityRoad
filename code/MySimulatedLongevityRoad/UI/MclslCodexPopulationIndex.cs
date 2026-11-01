using System;
using System.Collections;
using System.Collections.Generic;
using MySimulatedLongevityRoad.Core;
using MySimulatedLongevityRoad.Data;

namespace MySimulatedLongevityRoad.UI;

/// <summary>Population presentation is a projection of actor changes, never an open-window scan.</summary>
internal static class MclslCodexPopulationIndex
{
    private readonly struct Membership
    {
        internal readonly string Kingdom, Realm;
        internal Membership(string kingdom, string realm) { Kingdom = kingdom; Realm = realm; }
    }
    private readonly struct KingdomOrder
    {
        internal readonly Bucket Bucket;
        internal readonly int Count;
        internal KingdomOrder(Bucket bucket) { Bucket = bucket; Count = bucket.Entry.TotalCultivators; }
    }
    private sealed class Bucket
    {
        internal readonly long Id;
        internal readonly MclslKingdomCodexEntry Entry;
        internal readonly MclslOrderedIdIndex<MclslKingdomCultivatorEntry> Actors = new(CompareActors);
        internal readonly Dictionary<string, MclslOrderedIdIndex<MclslKingdomCultivatorEntry>> Realms = new(StringComparer.Ordinal);
        internal Bucket(long id, string name)
        { Id = id; Entry = new MclslKingdomCodexEntry { Name = name, Cultivators = Actors }; }
    }
    private static readonly Dictionary<long, Membership> Members = new();
    private static readonly Dictionary<string, Bucket> Kingdoms = new(StringComparer.Ordinal);
    private static readonly Dictionary<string, int> Counts = new(StringComparer.Ordinal);
    private static readonly MclslOrderedIdIndex<KingdomOrder> Order = new((a, b) =>
    {
        int count = b.Count.CompareTo(a.Count);
        return count != 0 ? count : string.CompareOrdinal(a.Bucket.Entry.Name, b.Bucket.Entry.Name);
    });
    private static long _nextKingdomId;
    internal static int SensingQi { get; private set; }
    internal static int Count => Members.Count;
    internal static IReadOnlyDictionary<string, int> RealmCounts => Counts;
    internal static readonly IReadOnlyList<MclslKingdomCodexEntry> Entries = new KingdomView();
    private sealed class KingdomView : IReadOnlyList<MclslKingdomCodexEntry>
    {
        public int Count => Order.Count;
        public MclslKingdomCodexEntry this[int index] => Order[index].Bucket.Entry;
        public IEnumerator<MclslKingdomCodexEntry> GetEnumerator() { for (int i = 0; i < Count; i++) yield return this[i]; }
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
    private static int CompareActors(MclslKingdomCultivatorEntry a, MclslKingdomCultivatorEntry b)
    {
        int order = b.RealmIndex.CompareTo(a.RealmIndex);
        if (order != 0) return order;
        order = b.TrueEssence.CompareTo(a.TrueEssence);
        if (order != 0) return order;
        order = b.Contribution.CompareTo(a.Contribution);
        return order != 0 ? order : string.CompareOrdinal(a.Name, b.Name);
    }
    internal static void Update(MclslRankEntry actor)
    {
        long id = actor.ActorId;
        string realm = actor.RealmId ?? string.Empty;
        string kingdom = string.IsNullOrWhiteSpace(actor.KingdomName) || actor.KingdomName == "无归属" ? "无国散修" : actor.KingdomName;
        bool same = Members.TryGetValue(id, out Membership old) && old.Realm == realm && old.Kingdom == kingdom;
        if (!same)
        {
            Remove(id);
            Members[id] = new Membership(kingdom, realm);
            if (realm.Length == 0) { SensingQi++; return; }
            Counts.TryGetValue(realm, out int count); Counts[realm] = count + 1;
        }
        if (realm.Length == 0) return;
        if (!Kingdoms.TryGetValue(kingdom, out Bucket bucket)) Kingdoms[kingdom] = bucket = new(++_nextKingdomId, kingdom);
        if (!same)
        {
            bucket.Entry.TotalCultivators++;
            bucket.Entry.RealmCounts.TryGetValue(realm, out int count); bucket.Entry.RealmCounts[realm] = count + 1;
            Order.Upsert(bucket.Id, new KingdomOrder(bucket));
        }
        if (!bucket.Realms.TryGetValue(realm, out MclslOrderedIdIndex<MclslKingdomCultivatorEntry> realmOrder))
        {
            bucket.Realms[realm] = realmOrder = new(CompareActors);
            bucket.Entry.CultivatorsByRealm[realm] = realmOrder;
        }
        MclslKingdomCultivatorEntry entry = new()
        {
            ActorId = id, Name = actor.Name, RealmId = realm, RealmName = MclslRealmIds.Display(realm),
            RealmIndex = MclslRealmIds.Index(realm), TrueEssence = actor.TrueEssence, Contribution = actor.Contribution
        };
        bucket.Actors.Upsert(id, entry); realmOrder.Upsert(id, entry);
    }
    internal static void Remove(long id)
    {
        if (!Members.TryGetValue(id, out Membership old)) return;
        Members.Remove(id);
        if (old.Realm.Length == 0) { SensingQi--; return; }
        Decrement(Counts, old.Realm);
        if (!Kingdoms.TryGetValue(old.Kingdom, out Bucket bucket)) return;
        bucket.Actors.Remove(id); Decrement(bucket.Entry.RealmCounts, old.Realm);
        if (bucket.Realms.TryGetValue(old.Realm, out MclslOrderedIdIndex<MclslKingdomCultivatorEntry> realm))
        {
            realm.Remove(id);
            if (realm.Count == 0) { bucket.Realms.Remove(old.Realm); bucket.Entry.CultivatorsByRealm.Remove(old.Realm); }
        }
        bucket.Entry.TotalCultivators--;
        if (bucket.Entry.TotalCultivators == 0) { Kingdoms.Remove(old.Kingdom); Order.Remove(bucket.Id); }
        else Order.Upsert(bucket.Id, new KingdomOrder(bucket));
    }
    private static void Decrement(Dictionary<string, int> counts, string key)
    {
        if (!counts.TryGetValue(key, out int count)) return;
        if (count == 1) counts.Remove(key); else counts[key] = count - 1;
    }
    internal static MclslKingdomCodexEntry FindKingdom(string name) => name != null && Kingdoms.TryGetValue(name, out Bucket bucket) ? bucket.Entry : null;
    internal static void Clear()
    { Members.Clear(); Kingdoms.Clear(); Counts.Clear(); Order.Clear(); _nextKingdomId = 0; SensingQi = 0; }
}
