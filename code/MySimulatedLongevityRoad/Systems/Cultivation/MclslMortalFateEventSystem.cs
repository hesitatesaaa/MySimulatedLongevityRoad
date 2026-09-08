using System;
using MySimulatedLongevityRoad.Core;
using MySimulatedLongevityRoad.Data;
using MySimulatedLongevityRoad.Queries;

namespace MySimulatedLongevityRoad.Systems;

internal static class MclslMortalFateEventSystem
{
    private const int MinAge = 8;
    private const int MaxAge = 70;
    private const int PerActorIntervalYears = 12;
    private const int MaxEventsPerYear = 4;

    private static int _activeYear;
    private static int _eventsThisYear;

    internal static void TryProcessAnnual(Actor actor, int year)
    {
        if (actor?.data == null || year <= 0) return;
        if (!MclslRuntimeSettings.CoreEnabled) return;
        if (!MclslActorAccessor.Alive(actor) || !MclslEligibility.CanCultivate(actor)) return;
        if (MclslActorAccessor.IsCultivator(actor)) return;

        int age = SafeAgeYear(actor);
        if (age < MinAge || age > MaxAge) return;

        EnsureYear(year);
        if (_eventsThisYear >= MaxEventsPerYear) return;

        int fate = Math.Clamp(MclslActorAccessor.GetInt(actor, MclslActorDataKeys.ImmortalFate, 0), 0, 100);
        int chance = Math.Clamp(8 + fate / 5 + (HasCity(actor) ? 3 : 0), 8, 34);
        if (!MclslDetectionGate.DeterministicRoll(actor, "mortal_fate", "gate", year, chance, 10000)) return;
        if (!MclslDetectionGate.TryEnterActorAttempt(actor, "mortal_fate", "event", year, PerActorIntervalYears)) return;

        int kind = PositiveHash(MclslActorAccessor.Id(actor) + "|mortal_fate_kind|" + year) % 4;
        Apply(actor, year, kind);
        _eventsThisYear++;
        MclslWorldActorQuery.Track(actor);
        MclslWorldActorQuery.MarkDirty();
    }

    internal static void Clear()
    {
        _activeYear = 0;
        _eventsThisYear = 0;
    }

    private static void Apply(Actor actor, int year, int kind)
    {
        string name = MclslActorAccessor.DisplayName(actor);
        switch (kind)
        {
            case 0:
                AddClamped(actor, MclslActorDataKeys.ImmortalFate, 3, 0, 100);
                AddClamped(actor, MclslActorDataKeys.MindState, 1, 0, 100);
                MclslActorAccessor.Set(actor, MclslActorDataKeys.LastBreakthroughResult, "遥见仙迹，心中求道之念未熄");
                MclslWorldRunRepository.AddEvent(year, "mortal_fate_glimpse", name + "遥见仙迹", name + "遥见云中剑光，自此常问仙路。", actor);
                return;
            case 1:
                int stones = 6 + PositiveHash(MclslActorAccessor.Id(actor) + "|mortal_stones|" + year) % 13;
                AddClamped(actor, MclslActorDataKeys.SpiritStones, stones, 0, 9999999);
                AddClamped(actor, MclslActorDataKeys.ImmortalFate, 1, 0, 100);
                MclslActorAccessor.Set(actor, MclslActorDataKeys.LastBreakthroughResult, "拾得灵石，仙缘暗藏");
                MclslWorldRunRepository.AddEvent(year, "mortal_fate_spirit_stone", name + "拾得灵石", name + "拾得半匣灵石，藏于旧衣箱中。", actor);
                return;
            case 2:
                AddClamped(actor, MclslActorDataKeys.RuinExperience, 2, 0, 9999);
                AddClamped(actor, MclslActorDataKeys.ImmortalFate, 1, 0, 100);
                MclslActorAccessor.Set(actor, MclslActorDataKeys.LastBreakthroughResult, "误入旧府外缘，记得残纹");
                MclslWorldRunRepository.AddEvent(year, "mortal_fate_ruin_edge", name + "误入旧府", name + "误入荒山旧府外缘，记下一角残纹。", actor);
                return;
            default:
                AddClamped(actor, MclslActorDataKeys.TechniqueInsight, 2, 0, 9999);
                if (MclslWorldEpochSystem.IsNewLawActive(year))
                    AddClamped(actor, MclslActorDataKeys.HeartTemperingProgress, 2, 0, 100);
                MclslActorAccessor.Set(actor, MclslActorDataKeys.LastBreakthroughResult, "闻得残句，默记心中");
                string body = MclslWorldEpochSystem.IsNewLawActive(year)
                    ? name + "闻游方修士谈玄黄新法，默记残句。"
                    : name + "闻老修讲古，默记一段吐纳残句。";
                MclslWorldRunRepository.AddEvent(year, "mortal_fate_fragment", name + "闻得残句", body, actor);
                return;
        }
    }

    private static void EnsureYear(int year)
    {
        if (_activeYear == year) return;
        _activeYear = year;
        _eventsThisYear = 0;
    }

    private static void AddClamped(Actor actor, string key, int delta, int min, int max)
    {
        if (actor?.data == null || string.IsNullOrWhiteSpace(key) || delta == 0) return;
        int current = MclslActorAccessor.GetInt(actor, key, 0);
        MclslActorAccessor.Set(actor, key, Math.Clamp(current + delta, min, max));
    }

    private static bool HasCity(Actor actor)
    {
        try { return actor?.city != null; }
        catch { return false; }
    }

    private static int SafeAgeYear(Actor actor)
    {
        try { return Math.Max(0, (int)Math.Floor(actor?.getAge() ?? 0f)); }
        catch { return 0; }
    }

    private static int PositiveHash(string value)
    {
        unchecked
        {
            int hash = 211;
            foreach (char c in value ?? string.Empty) hash = hash * 37 + c;
            return hash & int.MaxValue;
        }
    }
}
