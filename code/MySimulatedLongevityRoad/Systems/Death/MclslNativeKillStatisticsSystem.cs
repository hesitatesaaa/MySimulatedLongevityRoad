using System;
using System.Collections.Generic;
using System.Reflection;
using MySimulatedLongevityRoad.Core;
using MySimulatedLongevityRoad.Data;
using MySimulatedLongevityRoad.Systems;

namespace MySimulatedLongevityRoad.Systems.Death;

/// <summary>
/// Keeps WorldBox's native kill counter aligned with deaths that really completed.
/// The game can credit a lethal hit before Actor.die is diverted by a survival rule;
/// in that case the credited kill must be rolled back.
/// </summary>
internal static class MclslNativeKillStatisticsSystem
{
    private const int RepairVersion = 1;
    private static readonly Dictionary<long, int> ObservedKillCounts = new();
    private static readonly string[] KillMemberNames =
    {
        "kills", "_kills", "kill_count", "killCount", "units_killed", "unitsKilled"
    };
    private static readonly string[] KillerMemberNames =
    {
        "last_attacker", "lastAttacker", "_last_attacker", "attacked_by", "attackedBy", "killer", "last_hit_actor", "lastHitActor"
    };
    private static readonly string[] WorldDeathMemberNames =
    {
        "deaths", "_deaths", "total_deaths", "totalDeaths", "death_count", "deathCount", "units_died", "unitsDied", "creatures_died", "creaturesDied"
    };

    private static int _loadedYear;

    internal static void OnWorldLoaded(int year)
    {
        ObservedKillCounts.Clear();
        _loadedYear = Math.Max(0, year);
    }

    internal static void Observe(Actor actor)
    {
        if (actor?.data == null) return;
        long actorId = MclslActorAccessor.Id(actor);
        if (actorId <= 0L || ObservedKillCounts.ContainsKey(actorId)) return;
        if (TryReadNativeKills(actor, out int kills)) ObservedKillCounts[actorId] = Math.Max(0, kills);
    }

    internal static void RollbackDivertedDeath(Actor victim)
    {
        Actor killer = TryGetKiller(victim);
        if (killer?.data == null || killer == victim) return;
        long killerId = MclslActorAccessor.Id(killer);
        if (killerId <= 0L || !TryReadNativeKills(killer, out int current)) return;

        if (!ObservedKillCounts.TryGetValue(killerId, out int confirmed))
        {
            // No trustworthy baseline yet. Keep the current value and begin tracking;
            // the one-time save repair handles counters polluted by older versions.
            ObservedKillCounts[killerId] = Math.Max(0, current);
            return;
        }

        if (current > confirmed)
            TryWriteNativeKills(killer, confirmed);
        else if (current < confirmed)
            ObservedKillCounts[killerId] = Math.Max(0, current);
    }

    internal static void RecordCommittedDeath(Actor victim)
    {
        long victimId = MclslActorAccessor.Id(victim);
        if (victimId > 0L) ObservedKillCounts.Remove(victimId);
        RefreshKiller(TryGetKiller(victim));
    }

    internal static MclslNativeKillAttemptState CapturePotentialDivertedHit(Actor victim, BaseSimObject attacker)
    {
        if (!(attacker is Actor killer) || killer?.data == null || killer == victim)
            return MclslNativeKillAttemptState.Empty;
        if (!MclslDeathSystem.HasPotentialCombatDeathDiversion(victim))
            return MclslNativeKillAttemptState.Empty;
        if (!TryReadNativeKills(killer, out int baseline))
            return MclslNativeKillAttemptState.Empty;

        long killerId = MclslActorAccessor.Id(killer);
        if (killerId <= 0L) return MclslNativeKillAttemptState.Empty;
        ObservedKillCounts[killerId] = Math.Max(0, baseline);
        return new MclslNativeKillAttemptState(true, killer, Math.Max(0, baseline));
    }

    internal static void CompletePotentialDivertedHit(Actor victim, in MclslNativeKillAttemptState state)
    {
        if (!state.Found || state.Killer?.data == null) return;
        bool victimAlive;
        try { victimAlive = victim?.data != null && victim.isAlive(); }
        catch { victimAlive = false; }

        if (!victimAlive)
        {
            RefreshKiller(state.Killer);
            return;
        }

        if (!TryReadNativeKills(state.Killer, out int current)) return;
        if (current > state.BaselineKills)
            TryWriteNativeKills(state.Killer, state.BaselineKills);
        RefreshKiller(state.Killer);
    }

    internal static void TickAnnualRepair(int year)
    {
        MclslWorldRunState run = MclslWorldRunRepository.Current;
        if (run == null || run.NativeKillStatisticsRepairVersion >= RepairVersion) return;
        if (year <= _loadedYear) return;

        IReadOnlyList<Actor> actors = MclslCultivatorCandidateIndex.GetKnownActorsSnapshot();
        if (actors == null || actors.Count == 0) return;
        long totalDeaths = ReadNativeWorldDeathTotal();
        if (totalDeaths <= 0L) return;

        List<KillEntry> entries = new(actors.Count);
        long totalLivingKills = 0L;
        for (int i = 0; i < actors.Count; i++)
        {
            Actor actor = actors[i];
            if (!MclslActorAccessor.Alive(actor) || !TryReadNativeKills(actor, out int kills) || kills <= 0) continue;
            entries.Add(new KillEntry(actor, kills));
            totalLivingKills = SaturatingAdd(totalLivingKills, kills);
        }

        bool changed = false;
        if (totalLivingKills > totalDeaths && entries.Count > 0)
        {
            double scale = totalDeaths / (double)totalLivingKills;
            for (int i = 0; i < entries.Count; i++)
            {
                KillEntry entry = entries[i];
                int corrected = Math.Max(0, (int)Math.Floor(entry.Kills * scale));
                if (corrected == entry.Kills) continue;
                TryWriteNativeKills(entry.Actor, corrected);
                changed = true;
            }
        }

        ObservedKillCounts.Clear();
        for (int i = 0; i < actors.Count; i++) Observe(actors[i]);
        run.NativeKillStatisticsRepairVersion = RepairVersion;
        if (changed)
        {
            MclslWorldRunRepository.AddEvent(
                year,
                "native_kill_statistics_repaired",
                "击杀统计归正",
                "旧版本中被免死或救回的致死判定曾重复计入击杀，本世现存角色的原生击杀数已按世界真实死亡总量等比例归正。");
        }
        MclslWorldArchiveStore.MarkDirty();
    }

    internal static Actor DetachScriptedDeathAttacker(Actor actor)
    {
        if (actor == null) return null;
        Actor prior = null;
        try { prior = actor.attackedBy as Actor; actor.attackedBy = null; }
        catch (Exception ex) { MclslDiagnostics.Error("kill-stat-detach-scripted-attacker", "清理脚本死亡攻击者引用失败: " + ex.Message); }
        return prior;
    }

    internal static void RestoreScriptedDeathAttacker(Actor actor, Actor prior)
    {
        if (actor == null || prior == null) return;
        try { actor.attackedBy = prior; }
        catch (Exception ex) { MclslDiagnostics.Error("kill-stat-restore-scripted-attacker", "恢复脚本死亡攻击者引用失败: " + ex.Message); }
    }

    internal static void Clear()
    {
        ObservedKillCounts.Clear();
        _loadedYear = 0;
    }

    private static void RefreshKiller(Actor killer)
    {
        if (killer?.data == null) return;
        long killerId = MclslActorAccessor.Id(killer);
        if (killerId <= 0L || !TryReadNativeKills(killer, out int current)) return;
        ObservedKillCounts[killerId] = Math.Max(0, current);
    }

    private static Actor TryGetKiller(Actor victim)
    {
        if (victim == null) return null;
        try
        {
            Actor direct = victim.attackedBy as Actor;
            if (direct != null && direct != victim) return direct;
        }
        catch { }

        object target = victim;
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.IgnoreCase;
        for (int i = 0; i < KillerMemberNames.Length; i++)
        {
            string name = KillerMemberNames[i];
            try
            {
                FieldInfo field = target.GetType().GetField(name, flags);
                if (field?.GetValue(target) is Actor fieldActor && fieldActor != victim) return fieldActor;
                PropertyInfo property = target.GetType().GetProperty(name, flags);
                if (property?.CanRead == true && property.GetValue(target, null) is Actor propertyActor && propertyActor != victim) return propertyActor;
            }
            catch { }
        }
        return null;
    }

    private static bool TryReadNativeKills(Actor actor, out int value)
    {
        value = 0;
        object data = actor?.data;
        if (data == null) return false;
        for (int i = 0; i < KillMemberNames.Length; i++)
        {
            if (TryReadNumericMember(data, KillMemberNames[i], out long numeric))
            {
                value = (int)Math.Clamp(numeric, 0L, int.MaxValue);
                return true;
            }
        }
        return false;
    }

    private static bool TryWriteNativeKills(Actor actor, int value)
    {
        object data = actor?.data;
        if (data == null) return false;
        int safe = Math.Max(0, value);
        for (int i = 0; i < KillMemberNames.Length; i++)
        {
            if (TryWriteNumericMember(data, KillMemberNames[i], safe)) return true;
        }
        return false;
    }

    private static long ReadNativeWorldDeathTotal()
    {
        object world = World.world;
        if (world == null) return 0L;
        long best = 0L;
        try { best = Math.Max(best, ReadDeathTotalFromObject(World.world.map_stats)); }
        catch { }
        best = Math.Max(best, ReadDeathTotalFromObject(world));
        object worldData = TryReadObjectMember(world, "data");
        best = Math.Max(best, ReadDeathTotalFromObject(worldData));
        return best;
    }

    private static long ReadDeathTotalFromObject(object target)
    {
        if (target == null) return 0L;
        long best = 0L;
        for (int i = 0; i < WorldDeathMemberNames.Length; i++)
        {
            if (TryReadNumericMember(target, WorldDeathMemberNames[i], out long value))
                best = Math.Max(best, value);
        }
        return Math.Max(0L, best);
    }

    private static object TryReadObjectMember(object target, string name)
    {
        if (target == null || string.IsNullOrWhiteSpace(name)) return null;
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.IgnoreCase;
        try
        {
            FieldInfo field = target.GetType().GetField(name, flags);
            if (field != null) return field.GetValue(target);
            PropertyInfo property = target.GetType().GetProperty(name, flags);
            if (property?.CanRead == true) return property.GetValue(target, null);
        }
        catch { }
        return null;
    }

    private static bool TryReadNumericMember(object target, string name, out long value)
    {
        value = 0L;
        if (target == null || string.IsNullOrWhiteSpace(name)) return false;
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.IgnoreCase;
        try
        {
            FieldInfo field = target.GetType().GetField(name, flags);
            if (field != null && TryConvertNumeric(field.GetValue(target), out value)) return true;
            PropertyInfo property = target.GetType().GetProperty(name, flags);
            if (property?.CanRead == true && TryConvertNumeric(property.GetValue(target, null), out value)) return true;
        }
        catch { }
        return false;
    }

    private static bool TryWriteNumericMember(object target, string name, long value)
    {
        if (target == null || string.IsNullOrWhiteSpace(name)) return false;
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.IgnoreCase;
        try
        {
            FieldInfo field = target.GetType().GetField(name, flags);
            if (field != null && TryConvertForType(value, field.FieldType, out object convertedField))
            {
                field.SetValue(target, convertedField);
                return true;
            }
            PropertyInfo property = target.GetType().GetProperty(name, flags);
            if (property?.CanWrite == true && TryConvertForType(value, property.PropertyType, out object convertedProperty))
            {
                property.SetValue(target, convertedProperty, null);
                return true;
            }
        }
        catch { }
        return false;
    }

    private static bool TryConvertNumeric(object raw, out long value)
    {
        value = 0L;
        if (raw == null) return false;
        try
        {
            switch (raw)
            {
                case byte v: value = v; return true;
                case sbyte v: value = Math.Max(0L, (long)v); return true;
                case short v: value = Math.Max(0L, (long)v); return true;
                case ushort v: value = v; return true;
                case int v: value = Math.Max(0, v); return true;
                case uint v: value = v; return true;
                case long v: value = Math.Max(0L, v); return true;
                case ulong v: value = v > long.MaxValue ? long.MaxValue : (long)v; return true;
                case float v when !float.IsNaN(v): value = Math.Max(0L, (long)v); return true;
                case double v when !double.IsNaN(v): value = Math.Max(0L, (long)v); return true;
                default: return false;
            }
        }
        catch { return false; }
    }

    private static bool TryConvertForType(long value, Type type, out object converted)
    {
        converted = null;
        long safe = Math.Max(0L, value);
        if (type == typeof(int)) converted = (int)Math.Min(int.MaxValue, safe);
        else if (type == typeof(long)) converted = safe;
        else if (type == typeof(uint)) converted = (uint)Math.Min(uint.MaxValue, (ulong)safe);
        else if (type == typeof(ulong)) converted = (ulong)safe;
        else if (type == typeof(short)) converted = (short)Math.Min(short.MaxValue, safe);
        else if (type == typeof(ushort)) converted = (ushort)Math.Min(ushort.MaxValue, safe);
        else if (type == typeof(byte)) converted = (byte)Math.Min(byte.MaxValue, safe);
        else if (type == typeof(float)) converted = (float)safe;
        else if (type == typeof(double)) converted = (double)safe;
        else return false;
        return true;
    }

    private static long SaturatingAdd(long left, long right)
    {
        if (right <= 0L) return left;
        return left > long.MaxValue - right ? long.MaxValue : left + right;
    }

    internal readonly struct MclslNativeKillAttemptState
    {
        internal static readonly MclslNativeKillAttemptState Empty = new(false, null, 0);
        internal readonly bool Found;
        internal readonly Actor Killer;
        internal readonly int BaselineKills;

        internal MclslNativeKillAttemptState(bool found, Actor killer, int baselineKills)
        {
            Found = found;
            Killer = killer;
            BaselineKills = baselineKills;
        }
    }

    private readonly struct KillEntry
    {
        internal readonly Actor Actor;
        internal readonly int Kills;

        internal KillEntry(Actor actor, int kills)
        {
            Actor = actor;
            Kills = kills;
        }
    }
}
