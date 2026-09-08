using System;
using System.Collections;
using System.Globalization;
using System.Reflection;
using NeoModLoader.api;

namespace MySimulatedLongevityRoad.Core;

internal static class MclslRuntimeSettings
{
    private static bool _coreEnabled = true;
    private static bool _timelineEnabled = true;
    private static bool _autoSealTerminalCycle;
    private static int _timelineYearScalePercent = 100;
    private static int _carrySlotLimit = 8;
    private static int _annualActorBudget = 80;
    private static bool _deathAnnouncementsEnabled = true;
    private static bool _breakthroughFailureAnnouncementsEnabled = true;
    private static bool _nascentBreakthroughAnnouncementsEnabled = true;
    private static bool _divineBreakthroughAnnouncementsEnabled = true;
    private static bool _worldSoulAnnouncementsEnabled = true;
    private static int _deathPopupMinRealm = 3;
    private static bool _ruinDeathAnnouncementsEnabled;
    private static bool _worldAdventuresEnabled = true;
    private static bool _factionCommissionAnnouncementsEnabled = true;
    private static bool _ancientLineageAnnouncementsEnabled;
    private static bool _pioneerAnnouncementsEnabled = true;
    private static bool _minorWorldAnnouncementsEnabled;
    private static bool _ruinBirthAnnouncementsEnabled = true;
    private static bool _resourceBirthAnnouncementsEnabled = true;
    private static bool _factionPolicyAnnouncementsEnabled = true;
    private static bool _survivalAnnouncementsEnabled = true;
    private static bool _huanzhenEnabled;
    private static int _huanzhenAnchorIntervalYears = 100;
    private static int _huanzhenSafetyGapYears = 40;
    private static int _transmissionYear = 1000;
    private static bool _autoCollectYuanYing = true;
    private static bool _autoCollectHuaShen = true;
    private static bool _autoCollectHeDao = true;
    private static bool _autoCollectChangSheng = true;
    private static bool _autoCollectPureRoot = true;
    private static bool _autoCollectHeavenRoot = true;
    private static bool _diagnosticsEnabled;
    private static bool _debugToolsVisible;

    internal static bool CoreEnabled => _coreEnabled;
    internal static bool TimelineEnabled => _timelineEnabled;
    internal static bool AutoSealTerminalCycle => _autoSealTerminalCycle;
    internal static int CarrySlotLimit => _carrySlotLimit;
    internal static int AnnualActorBudget => _annualActorBudget;
    internal static bool DeathAnnouncementsEnabled => _deathAnnouncementsEnabled;
    internal static bool BreakthroughFailureAnnouncementsEnabled => _breakthroughFailureAnnouncementsEnabled;
    internal static bool NascentBreakthroughAnnouncementsEnabled => _nascentBreakthroughAnnouncementsEnabled;
    internal static bool DivineBreakthroughAnnouncementsEnabled => _divineBreakthroughAnnouncementsEnabled;
    internal static bool WorldSoulAnnouncementsEnabled => _worldSoulAnnouncementsEnabled;
    internal static int DeathPopupMinRealm => _deathPopupMinRealm;
    internal static bool RuinDeathAnnouncementsEnabled => _ruinDeathAnnouncementsEnabled;
    internal static bool WorldAdventuresEnabled => _worldAdventuresEnabled;
    internal static bool FactionCommissionAnnouncementsEnabled => _factionCommissionAnnouncementsEnabled;
    internal static bool AncientLineageAnnouncementsEnabled => _ancientLineageAnnouncementsEnabled;
    internal static bool PioneerAnnouncementsEnabled => _pioneerAnnouncementsEnabled;
    internal static bool MinorWorldAnnouncementsEnabled => _minorWorldAnnouncementsEnabled;
    internal static bool RuinBirthAnnouncementsEnabled => _ruinBirthAnnouncementsEnabled;
    internal static bool ResourceBirthAnnouncementsEnabled => _resourceBirthAnnouncementsEnabled;
    internal static bool FactionPolicyAnnouncementsEnabled => _factionPolicyAnnouncementsEnabled;
    internal static bool SurvivalAnnouncementsEnabled => _survivalAnnouncementsEnabled;
    internal static bool HuanzhenEnabled => _huanzhenEnabled;
    internal static int HuanzhenAnchorIntervalYears => _huanzhenAnchorIntervalYears;
    internal static int HuanzhenSafetyGapYears => _huanzhenSafetyGapYears;
    internal static int TransmissionYear => _transmissionYear;
    internal static bool AutoCollectYuanYing => _autoCollectYuanYing;
    internal static bool AutoCollectHuaShen => _autoCollectHuaShen;
    internal static bool AutoCollectHeDao => _autoCollectHeDao;
    internal static bool AutoCollectChangSheng => _autoCollectChangSheng;
    internal static bool AutoCollectPureRoot => _autoCollectPureRoot;
    internal static bool AutoCollectHeavenRoot => _autoCollectHeavenRoot;
    internal static bool DiagnosticsEnabled => _diagnosticsEnabled;
    internal static bool DebugToolsVisible => _debugToolsVisible;

    internal static void LoadFromModConfig(object config)
    {
        if (config == null) return;
        _coreEnabled = ReadBool(config, "MCLSL_config_enable_core", _coreEnabled);
        _timelineEnabled = ReadBool(config, "MCLSL_config_enable_timeline", _timelineEnabled);
        _autoSealTerminalCycle = ReadBool(config, "MCLSL_config_auto_seal_terminal", _autoSealTerminalCycle);
        _timelineYearScalePercent = Math.Clamp(ReadInt(config, "MCLSL_config_timeline_year_scale", _timelineYearScalePercent), 25, 400);
        _carrySlotLimit = Math.Clamp(ReadInt(config, "MCLSL_config_profile_carry_slots", _carrySlotLimit), 1, 20);
        _annualActorBudget = Math.Clamp(ReadInt(config, "MCLSL_config_annual_actor_budget", _annualActorBudget), 20, 400);
        _deathAnnouncementsEnabled = ReadBool(config, "MCLSL_config_enable_death_announcements", _deathAnnouncementsEnabled);
        _breakthroughFailureAnnouncementsEnabled = ReadBool(config, "MCLSL_config_enable_breakthrough_failure_announcements", _breakthroughFailureAnnouncementsEnabled);
        _nascentBreakthroughAnnouncementsEnabled = ReadBool(config, "MCLSL_config_enable_yuanying_breakthrough_announcements", _nascentBreakthroughAnnouncementsEnabled);
        _divineBreakthroughAnnouncementsEnabled = ReadBool(config, "MCLSL_config_enable_huashen_breakthrough_announcements", _divineBreakthroughAnnouncementsEnabled);
        _worldSoulAnnouncementsEnabled = ReadBool(config, "MCLSL_config_enable_world_soul_announcements", _worldSoulAnnouncementsEnabled);
        _deathPopupMinRealm = Math.Clamp(ReadInt(config, "MCLSL_config_death_popup_min_realm", _deathPopupMinRealm), 3, 7);
        _ruinDeathAnnouncementsEnabled = ReadBool(config, "MCLSL_config_enable_ruin_death_announcements", _ruinDeathAnnouncementsEnabled);
        _worldAdventuresEnabled = ReadBool(config, "MCLSL_config_enable_world_adventures", _worldAdventuresEnabled);
        _factionCommissionAnnouncementsEnabled = ReadBool(config, "MCLSL_config_enable_faction_commissions", _factionCommissionAnnouncementsEnabled);
        _ancientLineageAnnouncementsEnabled = ReadBool(config, "MCLSL_config_enable_ancient_lineage_announcements", _ancientLineageAnnouncementsEnabled);
        _pioneerAnnouncementsEnabled = ReadBool(config, "MCLSL_config_enable_pioneer_announcements", _pioneerAnnouncementsEnabled);
        _minorWorldAnnouncementsEnabled = ReadBool(config, "MCLSL_config_enable_minor_world_announcements", _minorWorldAnnouncementsEnabled);
        _ruinBirthAnnouncementsEnabled = ReadBool(config, "MCLSL_config_enable_ruin_birth_announcements", _ruinBirthAnnouncementsEnabled);
        _resourceBirthAnnouncementsEnabled = ReadBool(config, "MCLSL_config_enable_resource_birth_announcements", _resourceBirthAnnouncementsEnabled);
        _factionPolicyAnnouncementsEnabled = ReadBool(config, "MCLSL_config_enable_faction_policy_announcements", _factionPolicyAnnouncementsEnabled);
        _survivalAnnouncementsEnabled = ReadBool(config, "MCLSL_config_enable_survival_announcements", _survivalAnnouncementsEnabled);
        _huanzhenEnabled = ReadBool(config, "MCLSL_config_enable_huanzhen", _huanzhenEnabled);
        _huanzhenAnchorIntervalYears = Math.Clamp(ReadInt(config, "MCLSL_config_huanzhen_anchor_interval", _huanzhenAnchorIntervalYears), 25, 500);
        _huanzhenSafetyGapYears = Math.Clamp(ReadInt(config, "MCLSL_config_huanzhen_safety_gap", _huanzhenSafetyGapYears), 10, 200);
        _transmissionYear = Math.Clamp(ReadInt(config, "MCLSL_config_transmission_year", _transmissionYear), 1000, 3000);
        _autoCollectYuanYing = ReadBool(config, "MCLSL_config_auto_collect_yuanying", _autoCollectYuanYing);
        _autoCollectHuaShen = ReadBool(config, "MCLSL_config_auto_collect_huashen", _autoCollectHuaShen);
        _autoCollectHeDao = ReadBool(config, "MCLSL_config_auto_collect_hedao", _autoCollectHeDao);
        _autoCollectChangSheng = ReadBool(config, "MCLSL_config_auto_collect_changsheng", _autoCollectChangSheng);
        _autoCollectPureRoot = ReadBool(config, "MCLSL_config_auto_collect_pure_root", _autoCollectPureRoot);
        _autoCollectHeavenRoot = ReadBool(config, "MCLSL_config_auto_collect_heaven_root", _autoCollectHeavenRoot);
        _diagnosticsEnabled = ReadBool(config, "MCLSL_config_enable_diagnostics", _diagnosticsEnabled);
        _debugToolsVisible = ReadBool(config, "MCLSL_config_show_debug_tools", _debugToolsVisible);
    }

    internal static int ScaleTimelineYear(int year) => year <= 0 ? 0 : Math.Max(1, (int)Math.Round(year * (_timelineYearScalePercent / 100d)));

    private static bool ReadBool(object config, string id, bool fallback)
    {
        object item = Find(config, id, 0);
        if (item == null) return fallback;
        if (TryRead(item, "BoolVal", out object value) || TryRead(item, "Value", out value))
            return value is bool b ? b : bool.TryParse(Convert.ToString(value, CultureInfo.InvariantCulture), out bool p) ? p : fallback;
        return fallback;
    }

    private static int ReadInt(object config, string id, int fallback)
    {
        object item = Find(config, id, 0);
        if (item == null) return fallback;
        if (TryRead(item, "IntVal", out object value) || TryRead(item, "Value", out value))
            return value is int i ? i : int.TryParse(Convert.ToString(value, CultureInfo.InvariantCulture), out int p) ? p : fallback;
        return fallback;
    }

    private static object Find(object source, string id, int depth)
    {
        if (source == null || depth > 4) return null;
        if ((TryRead(source, "Id", out object ownId) || TryRead(source, "id", out ownId))
            && string.Equals(Convert.ToString(ownId), id, StringComparison.Ordinal)) return source;
        if (source is ModConfig typed)
        {
            try { return Find(typed["ConfigItems"], id, depth + 1); } catch (System.Exception mclslEmptyCatchEx) { MySimulatedLongevityRoad.Core.MclslDiagnostics.Error("empty-catch-code-MySimulatedLongevityRoad-Core-MclslRuntimeSettings-cs-1", "空 catch 捕获: code/MySimulatedLongevityRoad/Core/MclslRuntimeSettings.cs #1: " + mclslEmptyCatchEx.Message); }
        }
        if (source is IDictionary dictionary)
        {
            foreach (DictionaryEntry entry in dictionary)
            {
                if (string.Equals(Convert.ToString(entry.Key), id, StringComparison.Ordinal)) return entry.Value;
                object found = Find(entry.Value, id, depth + 1);
                if (found != null) return found;
            }
        }
        if (source is IEnumerable enumerable && source is not string)
        {
            foreach (object value in enumerable)
            {
                object found = Find(value, id, depth + 1);
                if (found != null) return found;
            }
        }
        return null;
    }

    private static bool TryRead(object source, string name, out object value)
    {
        value = null;
        if (source == null) return false;
        Type type = source.GetType();
        PropertyInfo property = type.GetProperty(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
        if (property?.GetMethod != null)
        {
            try { value = property.GetValue(source); return true; } catch (System.Exception mclslEmptyCatchEx) { MySimulatedLongevityRoad.Core.MclslDiagnostics.Error("empty-catch-code-MySimulatedLongevityRoad-Core-MclslRuntimeSettings-cs-2", "空 catch 捕获: code/MySimulatedLongevityRoad/Core/MclslRuntimeSettings.cs #2: " + mclslEmptyCatchEx.Message); }
        }
        FieldInfo field = type.GetField(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
        if (field != null)
        {
            try { value = field.GetValue(source); return true; } catch (System.Exception mclslEmptyCatchEx) { MySimulatedLongevityRoad.Core.MclslDiagnostics.Error("empty-catch-code-MySimulatedLongevityRoad-Core-MclslRuntimeSettings-cs-3", "空 catch 捕获: code/MySimulatedLongevityRoad/Core/MclslRuntimeSettings.cs #3: " + mclslEmptyCatchEx.Message); }
        }
        return false;
    }
}
