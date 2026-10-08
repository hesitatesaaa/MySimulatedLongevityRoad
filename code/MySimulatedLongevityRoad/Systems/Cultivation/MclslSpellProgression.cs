using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using MySimulatedLongevityRoad.Core;
using MySimulatedLongevityRoad.Data;
using MySimulatedLongevityRoad.Traits;

namespace MySimulatedLongevityRoad.Systems;

/// <summary>Small, persisted per-actor spell state. Casts write at most once per spell and game year.</summary>
internal static class MclslSpellProgression
{
    private sealed class Mastery
    {
        internal int Level = 1;
        internal int Experience;
        internal int LastCastYear = -1;
    }

    private sealed class Cache
    {
        internal string Raw;
        internal long Revision;
        internal readonly Dictionary<string, Mastery> Spells = new(StringComparer.Ordinal);
    }

    private static ConditionalWeakTable<Actor, Cache> _cache = new();

    internal static void ClearRuntime() => _cache = new();

    internal static int Insight(Actor actor)
    {
        if (actor?.data == null || !MclslActorAccessor.HasCultivationPath(actor)) return 0;
        int stored = MclslActorAccessor.GetInt(actor, MclslActorDataKeys.SpellInsight, 0);
        if (stored >= 1 && stored <= 100) return Math.Min(100, stored + (actor.hasTrait(MclslTraitRegistration.HeavenFavorTraitId) ? 20 : 0));
        // Two independent stable rolls give most people ordinary insight without tying it to root aptitude.
        long id = MclslActorAccessor.Id(actor);
        int value = (StableRoll(id, 0x39b4a1) + StableRoll(id, 0x71df23)) / 2 + 1;
        MclslActorAccessor.Set(actor, MclslActorDataKeys.SpellInsight, value);
        return Math.Min(100, value + (actor.hasTrait(MclslTraitRegistration.HeavenFavorTraitId) ? 20 : 0));
    }

    private static int StableRoll(long id, int salt)
    {
        unchecked
        {
            ulong x = (ulong)id ^ (uint)salt;
            x ^= x >> 33; x *= 0xff51afd7ed558ccdUL;
            x ^= x >> 33; x *= 0xc4ceb9fe1a85ec53UL;
            x ^= x >> 33;
            return (int)(x % 100UL);
        }
    }

    internal static string StudyTarget(Actor actor)
        => MclslActorAccessor.GetString(actor, MclslActorDataKeys.SpellStudyTarget);

    internal static long Revision(Actor actor)
        => actor?.data != null && _cache.TryGetValue(actor, out Cache cache) ? cache.Revision : 0L;

    internal static bool SetStudyTarget(Actor actor, string spellId)
    {
        if (!MclslActorAccessor.Alive(actor)) return false;
        if (string.IsNullOrEmpty(spellId))
        {
            MclslActorAccessor.Set(actor, MclslActorDataKeys.SpellStudyTarget, string.Empty);
            MclslActorAccessor.Set(actor, MclslActorDataKeys.SpellStudyProgress, 0);
            Touch(actor);
            return true;
        }
        if (!MclslSpellSystem.TryGet(spellId, out MclslSpellDefinition spell)
            || (!MclslSpellSystem.Knows(actor, spellId) && MclslRealmIds.Index(MclslActorAccessor.Realm(actor)) < spell.MinRealm)
            || MclslSpellSystem.Knows(actor, spellId) && Level(actor, spellId) >= 10) return false;
        if (StudyTarget(actor) != spellId)
        {
            MclslActorAccessor.Set(actor, MclslActorDataKeys.SpellStudyTarget, spellId);
            MclslActorAccessor.Set(actor, MclslActorDataKeys.SpellStudyProgress, 0);
            Touch(actor);
        }
        return true;
    }

    internal static int StudyProgress(Actor actor)
        => Math.Max(0, MclslActorAccessor.GetInt(actor, MclslActorDataKeys.SpellStudyProgress, 0));

    internal static int StudyThreshold(MclslSpellDefinition spell)
        => spell == null ? 0 : 1000 + 200 * spell.MinRealm;

    internal static int Level(Actor actor, string spellId)
    {
        if (!MclslSpellSystem.Knows(actor, spellId)) return 0;
        return State(actor, spellId).Level;
    }

    internal static int Experience(Actor actor, string spellId)
        => MclslSpellSystem.Knows(actor, spellId) ? State(actor, spellId).Experience : 0;

    internal static int ExperienceThreshold(MclslSpellDefinition spell, int level)
        => spell == null || level >= 10 ? 0 : 12 + 4 * Math.Clamp(level, 1, 9) + 2 * spell.MinRealm;

    internal static float Strength(Actor actor, string spellId)
        => 1f + 0.05f * (Math.Max(1, Level(actor, spellId)) - 1);

    internal static void OnLearned(Actor actor, string spellId)
    {
        Cache cache = Read(actor);
        if (cache == null) return;
        if (!cache.Spells.ContainsKey(spellId)) cache.Spells[spellId] = new Mastery();
        if (MclslActorAccessor.GetInt(actor, MclslActorDataKeys.SpellPracticeCycleStart, -1) < 0)
        {
            MclslActorAccessor.Set(actor, MclslActorDataKeys.SpellPracticeCycleStart, MclslRuntime.CurrentYear());
            MclslActorAccessor.Set(actor, MclslActorDataKeys.SpellPracticeCycleSpent, 0);
            MclslActorAccessor.Set(actor, MclslActorDataKeys.SpellPracticeCycleBudget, 12 + Insight(actor) / 8);
        }
        if (StudyTarget(actor) == spellId)
            MclslActorAccessor.Set(actor, MclslActorDataKeys.SpellStudyProgress, 0);
        Save(actor, cache);
    }

    internal static void ProgressAnnual(Actor actor)
    {
        if (!MclslActorAccessor.Alive(actor)) return;
        SettleCycle(actor, MclslRuntime.CurrentYear());
        int insight = Insight(actor);
        if (insight <= 0) return;
        string id = StudyTarget(actor);
        if (!MclslSpellSystem.TryGet(id, out MclslSpellDefinition spell)
            || MclslRealmIds.Index(MclslActorAccessor.Realm(actor)) < spell.MinRealm) return;
        if (!MclslSpellSystem.Knows(actor, id))
        {
            int baseProgress = 50 + insight;
            int bonus = MclslPhysiqueSystem.StudyPercent(actor, spell.Law);
            int progress = Math.Min(StudyThreshold(spell), StudyProgress(actor) + (int)Math.Ceiling(baseProgress * (1d + bonus / 100d)));
            if (progress >= StudyThreshold(spell))
            {
                if (MclslSpellSystem.Learn(actor, id)) progress = 0;
            }
            MclslActorAccessor.Set(actor, MclslActorDataKeys.SpellStudyProgress, progress);
            Touch(actor);
            return;
        }
        // Known spells receive their remaining experience at the end of each complete decade.
    }

    internal static void RecordCast(Actor actor, MclslSpellDefinition spell, int year)
    {
        if (actor?.data == null || spell == null || year < 0) return;
        SettleCycle(actor, year);
        Mastery mastery = State(actor, spell.Id);
        if (mastery.LastCastYear == year) return;
        mastery.LastCastYear = year;
        int remaining = Math.Max(0, CycleBudget(actor) - MclslActorAccessor.GetInt(actor, MclslActorDataKeys.SpellPracticeCycleSpent, 0));
        int granted = mastery.Level < 10 ? Math.Min(remaining, 1 + Insight(actor) / 50) : 0;
        if (granted > 0)
        {
            AddExperience(mastery, spell, (int)Math.Ceiling(granted * (1d + MclslPhysiqueSystem.ExperiencePercent(actor, spell.Law, true) / 100d)));
            MclslActorAccessor.Set(actor, MclslActorDataKeys.SpellPracticeCycleSpent,
                MclslActorAccessor.GetInt(actor, MclslActorDataKeys.SpellPracticeCycleSpent, 0) + granted);
        }
        Save(actor, Read(actor));
    }

    private static int CycleBudget(Actor actor) => MclslActorAccessor.GetInt(actor,
        MclslActorDataKeys.SpellPracticeCycleBudget, 12 + Insight(actor) / 8);

    private static void SettleCycle(Actor actor, int year)
    {
        int start = MclslActorAccessor.GetInt(actor, MclslActorDataKeys.SpellPracticeCycleStart, -1);
        if (start < 0 || year < start + 10) return;
        int remaining = Math.Max(0, CycleBudget(actor) - MclslActorAccessor.GetInt(actor, MclslActorDataKeys.SpellPracticeCycleSpent, 0));
        List<MclslSpellDefinition> priorities = PracticePriorities(actor);
        if (remaining > 0 && priorities.Count > 0)
        {
            int[] weights = priorities.Count switch { 1 => new[] { 100 }, 2 => new[] { 63, 37 }, _ => new[] { 50, 30, 20 } };
            int distributed = 0;
            for (int i = 0; i < priorities.Count; i++)
            {
                int amount = i == priorities.Count - 1 ? remaining - distributed : remaining * weights[i] / 100;
                distributed += amount;
                GrantExperience(actor, priorities[i], amount);
            }
        }
        // A skipped decade never receives a second award on a later annual tick.
        int next = start + 10 * Math.Max(1, (year - start) / 10);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.SpellPracticeCycleStart, next);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.SpellPracticeCycleSpent, 0);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.SpellPracticeCycleBudget, 12 + Insight(actor) / 8);
    }

    internal static List<MclslSpellDefinition> PracticePriorities(Actor actor)
    {
        List<MclslSpellDefinition> known = MclslSpellSystem.Known(actor);
        string target = StudyTarget(actor);
        string[] roots = MclslSpiritualRootSystem.RootAttributes(actor);
        MclslTechniqueDefinition technique = MclslCultivationCatalog.Technique(
            MclslActorAccessor.GetString(actor, MclslActorDataKeys.TechniqueId));
        known.Sort((a, b) =>
        {
            int Score(MclslSpellDefinition spell)
            {
                int need = spell.Description.Contains("疗") || spell.Description.Contains("恢复") ? 12
                    : spell.Description.Contains("护") || spell.Description.Contains("御") ? 10 : 8;
                return (spell.Id == target ? 1000 : 0)
                    + (Array.IndexOf(roots, spell.Law) >= 0 ? 40 : 0)
                    + (Array.IndexOf(technique.LawPool, spell.Law) >= 0 ? 25 : 0)
                    + need + spell.MinRealm * 2 - Level(actor, spell.Id) * 3;
            }
            int compare = Score(b).CompareTo(Score(a));
            return compare != 0 ? compare : string.CompareOrdinal(a.Id, b.Id);
        });
        known.RemoveAll(spell => Level(actor, spell.Id) >= 10);
        if (known.Count > 3) known.RemoveRange(3, known.Count - 3);
        return known;
    }

    private static void GrantExperience(Actor actor, MclslSpellDefinition spell, int amount)
    {
        Mastery mastery = State(actor, spell.Id);
        if (mastery.Level >= 10)
        {
            if (StudyTarget(actor) == spell.Id) SetStudyTarget(actor, string.Empty);
            return;
        }
        AddExperience(mastery, spell, (int)Math.Ceiling(amount * (1d + MclslPhysiqueSystem.ExperiencePercent(actor, spell.Law, false) / 100d)));
        Save(actor, Read(actor));
        if (mastery.Level >= 10 && StudyTarget(actor) == spell.Id) SetStudyTarget(actor, string.Empty);
    }

    private static void AddExperience(Mastery mastery, MclslSpellDefinition spell, int amount)
    {
        mastery.Experience += Math.Max(0, amount);
        while (mastery.Level < 10 && mastery.Experience >= ExperienceThreshold(spell, mastery.Level))
        {
            mastery.Experience -= ExperienceThreshold(spell, mastery.Level);
            mastery.Level++;
        }
        if (mastery.Level >= 10) mastery.Experience = 0;
    }

    private static Mastery State(Actor actor, string id)
    {
        Cache cache = Read(actor);
        if (cache == null) return new Mastery();
        if (!cache.Spells.TryGetValue(id, out Mastery mastery))
        {
            mastery = new Mastery();
            cache.Spells[id] = mastery;
        }
        return mastery;
    }

    private static Cache Read(Actor actor)
    {
        if (actor?.data == null) return null;
        Cache cache = _cache.GetOrCreateValue(actor);
        string raw = MclslActorAccessor.GetString(actor, MclslActorDataKeys.SpellMastery);
        if (cache.Raw == raw) return cache;
        cache.Raw = raw;
        cache.Spells.Clear();
        if (!raw.StartsWith("v1:", StringComparison.Ordinal)) return cache;
        foreach (string record in raw.Substring(3).Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
        {
            string[] fields = record.Split(':');
            if (fields.Length != 4 || !MclslSpellSystem.TryGet(fields[0], out _)
                || !int.TryParse(fields[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out int level)
                || !int.TryParse(fields[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out int xp)
                || !int.TryParse(fields[3], NumberStyles.Integer, CultureInfo.InvariantCulture, out int year)) continue;
            cache.Spells[fields[0]] = new Mastery
            {
                Level = Math.Clamp(level, 1, 10), Experience = Math.Max(0, xp), LastCastYear = year
            };
        }
        return cache;
    }

    private static void Save(Actor actor, Cache cache)
    {
        if (cache == null) return;
        StringBuilder output = new("v1:");
        foreach (MclslSpellDefinition spell in MclslSpellSystem.All)
        {
            if (!cache.Spells.TryGetValue(spell.Id, out Mastery mastery)) continue;
            if (output.Length > 3) output.Append(',');
            output.Append(spell.Id).Append(':').Append(mastery.Level).Append(':')
                .Append(mastery.Experience).Append(':').Append(mastery.LastCastYear);
        }
        cache.Raw = output.ToString();
        MclslActorAccessor.Set(actor, MclslActorDataKeys.SpellMastery, cache.Raw);
        Touch(actor);
    }

    private static void Touch(Actor actor)
    {
        if (actor?.data == null) return;
        Cache cache = _cache.GetOrCreateValue(actor);
        cache.Revision++;
    }
}
