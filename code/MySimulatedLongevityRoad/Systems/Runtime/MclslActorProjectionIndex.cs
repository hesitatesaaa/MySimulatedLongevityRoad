using System;
using System.Collections.Generic;
using MySimulatedLongevityRoad.Core;
using MySimulatedLongevityRoad.Data;

namespace MySimulatedLongevityRoad.Systems;

/// <summary>Incremental projections shared by economy, professions and annual world rules.</summary>
internal static class MclslActorProjectionIndex
{
    private readonly struct Row
    {
        internal readonly long Id;
        internal readonly string Profession, Era, HighTechnique;
        internal readonly int MissionScore, PressureScore, Realm, StudentScore;
        internal readonly long CityId, KingdomId, MaobaoScore;
        internal readonly bool AvailableStudent;
        internal readonly bool IsHigh;
        internal readonly bool QualifiedHunter, ManifestTarget;
        internal readonly int ManifestScore;
        internal Row(Actor actor)
        {
            Id = MclslActorAccessor.Id(actor);
            Profession = MclslActorAccessor.GetString(actor, MclslActorDataKeys.Profession, string.Empty);
            Era = MclslActorAccessor.GetString(actor, MclslActorDataKeys.CultivationSystem, string.Empty);
            int realm = Math.Max(0, MclslRealmIds.Index(MclslActorAccessor.Realm(actor)));
            Realm = realm;
            MaobaoScore = realm * 10000000L + Math.Max(0, MclslActorAccessor.GetInt(actor, MclslActorDataKeys.TrueEssence));
            CityId = actor.city?.data?.id ?? 0;
            KingdomId = actor.kingdom?.data?.id ?? 0;
            AvailableStudent = Era == MclslCultivationSystemIds.AncientLaw
                && MclslRealmIds.Index(MclslActorAccessor.Realm(actor)) >= 0
                && !MclslAncientMentorshipSystem.HasLivingTeacher(actor);
            StudentScore = AvailableStudent ? MclslAncientMentorshipSystem.ScoreStudent(actor) : 0;
            IsHigh = realm >= MclslRealmIds.Index(MclslRealmIds.JinDan);
            HighTechnique = IsHigh
                ? MclslActorAccessor.GetString(actor, MclslActorDataKeys.TechniqueId, string.Empty) : string.Empty;
            MissionScore = MclslFactionMissionSystem.MissionCandidateScore(actor);
            PressureScore = MclslFactionPressureSystem.PressureCandidateScore(actor);
            QualifiedHunter = MclslWorldSoulSystem.IsQualifiedHunter(actor);
            ManifestTarget = QualifiedHunter && actor.current_tile != null;
            ManifestScore = ManifestTarget ? MclslWorldSoulSystem.ManifestTargetScore(actor) : 0;
        }
    }
    private static readonly Dictionary<long, Row> Rows = new();
    private static readonly Dictionary<string, MclslIdSlots> Professions = new(StringComparer.Ordinal);
    private static readonly Dictionary<string, MclslIdSlots> Eras = new(StringComparer.Ordinal);
    private static readonly Dictionary<string, int> HighTechniqueCounts = new(StringComparer.Ordinal);
    private static readonly MclslOrderedIdIndex<Row> MissionOrder = new((a, b) => b.MissionScore.CompareTo(a.MissionScore));
    private static readonly MclslOrderedIdIndex<Row> PressureOrder = new((a, b) => b.PressureScore.CompareTo(a.PressureScore));
    private static readonly Dictionary<(long Location, int Realm), MclslOrderedIdIndex<Row>> CityStudents = new();
    private static readonly Dictionary<(long Location, int Realm), MclslOrderedIdIndex<Row>> KingdomStudents = new();
    private static readonly Comparison<Row> CompareStudent = (a, b) => b.StudentScore.CompareTo(a.StudentScore);
    private static readonly MclslOrderedIdIndex<Row> MaobaoOrder = new((a, b) => b.MaobaoScore.CompareTo(a.MaobaoScore));
    private static int _sharedHighTechniques;
    private static int _highCultivators;
    private static int _qualifiedHunters;
    private static readonly MclslOrderedIdIndex<Row> ManifestOrder = new((a, b) => b.ManifestScore.CompareTo(a.ManifestScore));
    private readonly struct MortalStudent
    {
        internal readonly long Id, City, Kingdom;
        internal readonly int Score;
        internal MortalStudent(Actor actor)
        {
            Id = MclslActorAccessor.Id(actor); City = actor.city?.data?.id ?? 0;
            Kingdom = actor.kingdom?.data?.id ?? 0;
            Score = MclslAncientMentorshipSystem.ScoreUninitiatedStudent(actor);
        }
    }
    private static readonly Dictionary<long, MortalStudent> MortalStudents = new();
    private static readonly Dictionary<long, MclslOrderedIdIndex<MortalStudent>> MortalCity = new(), MortalKingdom = new();
    private static readonly Comparison<MortalStudent> MortalCompare = (a, b) => b.Score.CompareTo(a.Score);
    internal static void RefreshUninitiated(Actor actor)
    {
        long id = MclslActorAccessor.Id(actor);
        if (id <= 0) return;
        if (!MclslAncientMentorshipSystem.IsAvailableUninitiatedStudent(actor)) { RemoveMortalStudent(id); return; }
        MortalStudent next = new(actor);
        if (MortalStudents.TryGetValue(id, out MortalStudent old))
        {
            if (old.City == next.City && old.Kingdom == next.Kingdom && old.Score == next.Score) return;
            RemoveMortalStudent(id);
        }
        MortalStudents[id] = next;
        PutMortal(MortalCity, next.City, next); PutMortal(MortalKingdom, next.Kingdom, next);
    }
    private static void PutMortal(Dictionary<long, MclslOrderedIdIndex<MortalStudent>> buckets, long location, MortalStudent row)
    {
        if (location <= 0) return;
        if (!buckets.TryGetValue(location, out var order)) buckets[location] = order = new(MortalCompare);
        order.Upsert(row.Id, row);
    }
    private static void RemoveMortalStudent(long id)
    {
        if (!MortalStudents.TryGetValue(id, out MortalStudent row)) return;
        RemoveMortal(MortalCity, row.City, id); RemoveMortal(MortalKingdom, row.Kingdom, id);
        MortalStudents.Remove(id);
    }
    private static void RemoveMortal(Dictionary<long, MclslOrderedIdIndex<MortalStudent>> buckets, long location, long id)
    {
        if (!buckets.TryGetValue(location, out var order)) return;
        order.Remove(id); if (order.Count == 0) buckets.Remove(location);
    }
    internal static Actor PickUninitiatedStudent(Actor teacher)
    {
        Actor best = null; int score = int.MinValue;
        ConsiderMortal(MortalCity, teacher.city?.data?.id ?? 0, teacher, 1_000_000, ref best, ref score);
        ConsiderMortal(MortalKingdom, teacher.kingdom?.data?.id ?? 0, teacher, 0, ref best, ref score);
        return best;
    }
    private static void ConsiderMortal(Dictionary<long, MclslOrderedIdIndex<MortalStudent>> buckets,
        long location, Actor teacher, int bonus, ref Actor best, ref int bestScore)
    {
        if (location <= 0 || !buckets.TryGetValue(location, out var order)) return;
        // Native location/age changes join the same bounded audit as every other
        // index. Queries repair stale heads, never inspect the world population.
        while (order.Count > 0 && !MclslFrameDeadline.Expired)
        {
            MortalStudent row = order[0];
            if (!MclslActorRegistry.Resolve(row.Id, out Actor actor)
                || !MclslAncientMentorshipSystem.IsValidUninitiatedStudent(teacher, actor))
            {
                RemoveMortalStudent(row.Id);
                if (actor != null) RefreshUninitiated(actor);
                if (order.Count > 0 && order[0].Id == row.Id) break;
                continue;
            }
            int score = row.Score + bonus;
            if (score > bestScore || score == bestScore && (best == null || row.Id < MclslActorAccessor.Id(best)))
            { best = actor; bestScore = score; }
            break;
        }
    }
    internal static int Count => Rows.Count;
    internal static int HighCultivators => _highCultivators;
    internal static int QualifiedHunters => _qualifiedHunters;
    internal static int SharedHighTechniqueGroups => _sharedHighTechniques;

    internal static void Update(Actor actor)
    {
        long id = MclslActorAccessor.Id(actor);
        if (!MclslActorAccessor.Alive(actor) || !MclslCultivatorCandidateIndex.IsCultivator(id))
        { Remove(id); RefreshUninitiated(actor); return; }
        Row next = new(actor);
        bool existed = Rows.TryGetValue(id, out Row old);
        Rows[id] = next;
        if (!existed || old.Profession != next.Profession)
        { if (existed) RemoveBucket(Professions, old.Profession, id); AddBucket(Professions, next.Profession, id); }
        if (!existed || old.Era != next.Era)
        { if (existed) RemoveBucket(Eras, old.Era, id); AddBucket(Eras, next.Era, id); }
        _highCultivators += (next.IsHigh ? 1 : 0) - (existed && old.IsHigh ? 1 : 0);
        _qualifiedHunters += (next.QualifiedHunter ? 1 : 0) - (existed && old.QualifiedHunter ? 1 : 0);
        if (!existed || old.HighTechnique != next.HighTechnique)
        { if (existed) RemoveTechnique(old.HighTechnique); AddTechnique(next.HighTechnique); }
        if (existed && old.AvailableStudent
            && (!next.AvailableStudent || old.CityId != next.CityId || old.KingdomId != next.KingdomId || old.Realm != next.Realm))
            RemoveStudent(old);
        if (next.AvailableStudent)
        {
            PutStudent(CityStudents, next.CityId, next);
            PutStudent(KingdomStudents, next.KingdomId, next);
        }
        MaobaoOrder.Upsert(id, next);
        MissionOrder.Upsert(id, next);
        PressureOrder.Upsert(id, next);
        if (next.ManifestTarget) ManifestOrder.Upsert(id, next); else ManifestOrder.Remove(id);
    }
    internal static void Remove(long id)
    {
        RemoveMortalStudent(id);
        if (!Rows.TryGetValue(id, out Row old)) return;
        RemoveMembership(old); Rows.Remove(id);
        MaobaoOrder.Remove(id);
        MissionOrder.Remove(id); PressureOrder.Remove(id);
        ManifestOrder.Remove(id);
        if (old.QualifiedHunter) _qualifiedHunters--;
    }
    private static void PutStudent(Dictionary<(long, int), MclslOrderedIdIndex<Row>> buckets, long location, Row row)
    {
        if (location <= 0) return;
        var key = (location, row.Realm);
        if (!buckets.TryGetValue(key, out var order)) buckets[key] = order = new(CompareStudent);
        order.Upsert(row.Id, row);
    }
    private static void RemoveStudent(Row row)
    {
        if (!row.AvailableStudent) return;
        RemoveStudentBucket(CityStudents, row.CityId, row);
        RemoveStudentBucket(KingdomStudents, row.KingdomId, row);
    }
    private static void RemoveStudentBucket(Dictionary<(long, int), MclslOrderedIdIndex<Row>> buckets, long location, Row row)
    {
        var key = (location, row.Realm);
        if (!buckets.TryGetValue(key, out var order)) return;
        order.Remove(row.Id);
        if (order.Count == 0) buckets.Remove(key);
    }
    internal static Actor PickMentorshipStudent(Actor teacher)
    {
        Actor best = null;
        int bestScore = int.MinValue;
        int teacherRealm = MclslRealmIds.Index(MclslActorAccessor.Realm(teacher));
        long city = teacher.city?.data?.id ?? 0, kingdom = teacher.kingdom?.data?.id ?? 0;
        for (int realm = 0; realm < teacherRealm; realm++)
        {
            ConsiderStudent(CityStudents, city, realm, teacher, 1_000_000, ref best, ref bestScore);
            ConsiderStudent(KingdomStudents, kingdom, realm, teacher, 0, ref best, ref bestScore);
        }
        return best;
    }
    private static void ConsiderStudent(Dictionary<(long, int), MclslOrderedIdIndex<Row>> buckets,
        long location, int realm, Actor teacher, int bonus, ref Actor best, ref int bestScore)
    {
        if (location <= 0 || !buckets.TryGetValue((location, realm), out var order)) return;
        // Ownership changes and deaths remove students immediately. Revalidate the
        // first row to cover external native changes awaiting their event dispatch.
        while (order.Count > 0 && !MclslFrameDeadline.Expired)
        {
            Row row = order[0];
            if (!MclslActorRegistry.Resolve(row.Id, out Actor actor)
                || !MclslAncientMentorshipSystem.IsValidStudent(teacher, actor))
            {
                order.Remove(row.Id);
                if (actor != null) Update(actor);
                if (order.Count > 0 && order[0].Id == row.Id) break;
                continue;
            }
            int score = row.StudentScore + bonus;
            if (score > bestScore || score == bestScore && (best == null || row.Id < MclslActorAccessor.Id(best)))
            { best = actor; bestScore = score; }
            break;
        }
    }
    private static void AddTechnique(string technique)
    {
        if (technique.Length == 0) return;
        HighTechniqueCounts.TryGetValue(technique, out int count);
        HighTechniqueCounts[technique] = count + 1;
        if (count == 1) _sharedHighTechniques++;
    }
    private static void RemoveMembership(Row row)
    {
        RemoveStudent(row);
        RemoveBucket(Professions, row.Profession, row.Id);
        RemoveBucket(Eras, row.Era, row.Id);
        if (row.IsHigh) _highCultivators--;
        RemoveTechnique(row.HighTechnique);
    }
    private static void RemoveTechnique(string technique)
    {
        if (technique.Length == 0 || !HighTechniqueCounts.TryGetValue(technique, out int count)) return;
        if (count == 2) _sharedHighTechniques--;
        if (count == 1) HighTechniqueCounts.Remove(technique);
        else HighTechniqueCounts[technique] = count - 1;
    }
    private static void AddBucket(Dictionary<string, MclslIdSlots> buckets, string key, long id)
    {
        if (string.IsNullOrEmpty(key)) return;
        if (!buckets.TryGetValue(key, out MclslIdSlots ids)) buckets[key] = ids = new();
        ids.Add(id);
    }
    private static void RemoveBucket(Dictionary<string, MclslIdSlots> buckets, string key, long id)
    {
        if (string.IsNullOrEmpty(key) || !buckets.TryGetValue(key, out MclslIdSlots ids)) return;
        ids.Remove(id); if (ids.LiveCount == 0) buckets.Remove(key);
    }
    internal static IReadOnlyList<Actor> ProfessionActors(string profession) => BucketActors(Professions, profession);
    internal static IReadOnlyList<Actor> EraActors(string era) => BucketActors(Eras, era);
    internal static IReadOnlyList<Actor> TraitActors(string trait) => MclslActorRegistry.TraitActors(trait);
    private static IReadOnlyList<Actor> BucketActors(Dictionary<string, MclslIdSlots> buckets, string key)
        => key != null && buckets.TryGetValue(key, out MclslIdSlots ids)
            ? MclslActorRegistry.CreateView(ids) : Array.Empty<Actor>();

    internal static IReadOnlyList<Actor> MaobaoCandidates(int limit) => new RankedView(MaobaoOrder, limit);
    internal static IReadOnlyList<Actor> MissionCandidates(int limit) => new RankedView(MissionOrder, limit);
    internal static IReadOnlyList<Actor> PressureCandidates(int limit) => new RankedView(PressureOrder, limit);
    internal static IReadOnlyList<Actor> ManifestCandidates(int limit) => new RankedView(ManifestOrder, limit);
    private sealed class RankedView : IReadOnlyList<Actor>
    {
        private readonly MclslOrderedIdIndex<Row> _source;
        private readonly int _limit;
        internal RankedView(MclslOrderedIdIndex<Row> source, int limit) { _source = source; _limit = limit; }
        public int Count => Math.Min(_limit, _source.Count);
        public Actor this[int index] => MclslActorRegistry.Resolve(_source[index].Id, out Actor actor) ? actor : null;
        public IEnumerator<Actor> GetEnumerator() { for (int i = 0; i < Count; i++) yield return this[i]; }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
    internal static void Clear()
    {
        Rows.Clear(); Professions.Clear(); Eras.Clear(); HighTechniqueCounts.Clear();
        CityStudents.Clear(); KingdomStudents.Clear(); MaobaoOrder.Clear();
        MortalStudents.Clear(); MortalCity.Clear(); MortalKingdom.Clear();
        MissionOrder.Clear(); PressureOrder.Clear(); _highCultivators = _sharedHighTechniques = 0;
        ManifestOrder.Clear(); _qualifiedHunters = 0;
    }
}
