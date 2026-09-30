using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using MySimulatedLongevityRoad.Data;

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
        if (stored >= 1 && stored <= 100) return stored;
        // Two independent stable rolls give most people ordinary insight without tying it to root aptitude.
        long id = MclslActorAccessor.Id(actor);
        int value = (StableRoll(id, 0x39b4a1) + StableRoll(id, 0x71df23)) / 2 + 1;
        MclslActorAccessor.Set(actor, MclslActorDataKeys.SpellInsight, value);
        return value;
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
            || MclslRealmIds.Index(MclslActorAccessor.Realm(actor)) < spell.MinRealm
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
        if (StudyTarget(actor) == spellId)
            MclslActorAccessor.Set(actor, MclslActorDataKeys.SpellStudyProgress, 0);
        Save(actor, cache);
    }

    internal static void ProgressAnnual(Actor actor)
    {
        if (!MclslActorAccessor.Alive(actor)) return;
        int insight = Insight(actor);
        if (insight <= 0) return;
        string id = StudyTarget(actor);
        if (!MclslSpellSystem.TryGet(id, out MclslSpellDefinition spell)
            || MclslRealmIds.Index(MclslActorAccessor.Realm(actor)) < spell.MinRealm) return;
        if (!MclslSpellSystem.Knows(actor, id))
        {
            int progress = Math.Min(StudyThreshold(spell), StudyProgress(actor) + 50 + insight);
            if (progress >= StudyThreshold(spell))
            {
                if (MclslSpellSystem.Learn(actor, id)) progress = 0;
            }
            MclslActorAccessor.Set(actor, MclslActorDataKeys.SpellStudyProgress, progress);
            Touch(actor);
            return;
        }
        GrantExperience(actor, spell, 5 + insight / 20);
    }

    internal static void RecordCast(Actor actor, MclslSpellDefinition spell, int year)
    {
        if (actor?.data == null || spell == null || year < 0) return;
        Mastery mastery = State(actor, spell.Id);
        if (mastery.LastCastYear == year) return;
        mastery.LastCastYear = year;
        if (mastery.Level < 10) AddExperience(mastery, spell, 1 + Insight(actor) / 50);
        Save(actor, Read(actor));
    }

    private static void GrantExperience(Actor actor, MclslSpellDefinition spell, int amount)
    {
        Mastery mastery = State(actor, spell.Id);
        if (mastery.Level >= 10)
        {
            if (StudyTarget(actor) == spell.Id) SetStudyTarget(actor, string.Empty);
            return;
        }
        AddExperience(mastery, spell, amount);
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
