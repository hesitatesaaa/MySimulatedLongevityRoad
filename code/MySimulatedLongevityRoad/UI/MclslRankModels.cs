using System;
using System.Collections.Generic;
using MySimulatedLongevityRoad.Data;
using MySimulatedLongevityRoad.Systems;
using UnityEngine;

namespace MySimulatedLongevityRoad.UI;

internal sealed class MclslRankEntry
{
    internal Actor Actor
    {
        get => MclslActorRegistry.Resolve(ActorId, out Actor actor) ? actor : null;
        set { long id = MclslActorAccessor.Id(value); if (id > 0) ActorId = id; }
    }
    internal long ActorId;
    internal string Name = string.Empty;
    internal string RealmId = string.Empty;
    internal string RealmName = string.Empty;
    internal string GiftName = string.Empty;
    internal string RootText = string.Empty;
    internal string RootAttributes = string.Empty;
    internal string NormalizedSearchText = string.Empty;
    internal MclslRankExtraData ExtraData;
    private string _extraText;
    internal string ExtraText { get => _extraText ??= ExtraData.Format(); set => _extraText = value; }
    internal string KingdomName = string.Empty;
    internal string ProfessionId = string.Empty;
    internal int ProfessionGrade;
    internal int ProfessionExperience;
    // 战斗职业（剑修/武修/术修/符修/阵修/灵契师）：-1 表示无战斗职业。
    internal int CombatCareer = -1;
    internal int CombatCareerRank;
    internal int CombatCareerExperience;
    internal double Power;
    internal int RealmIndex;
    internal int MinorRealmIndex;
    internal int RealmSortRank => (RealmIndex + 1) * 5 + MinorRealmIndex;
    internal int Aptitude;
    internal int TrueEssence;
    internal long Contribution;
    internal long SpiritStones;
    internal int MindState;
    internal int MortalMiasma;
    internal int MortalMiasmaLimit;
    // 神兽榜专用：普通妖兽身份、物种与独立本命神通等级。
    // 共享快照同时保留修士和普通妖兽，两个榜单各自按此区分筛选。
    internal bool IsOrdinaryBeast;
    internal string SpeciesId = string.Empty;
    internal string SpeciesName = string.Empty;
    internal string TalentName = string.Empty;
    internal int TalentLevel;
}

/// <summary>Numeric presentation data; text is allocated only for displayed rows.</summary>
internal readonly struct MclslRankExtraData
{
    internal readonly byte Kind;
    internal readonly int Value, Limit;
    internal readonly float Progress;
    internal MclslRankExtraData(byte kind, int value = 0, int limit = 0, float progress = 0)
    { Kind = kind; Value = value; Limit = limit; Progress = progress; }
    internal string Format() => Kind switch
    {
        1 => Value > 0 ? Value + "纯" : "悟法",
        2 => Value > 0 ? Value + "洞" : "洞天",
        3 => Value > 0 ? Value + "髓" : "抽髓",
        4 => Value > 0 ? Value + "稳" : "祭魄",
        5 => Value >= 100 ? "太上" : Value + "太上",
        6 => Value + "/" + Limit,
        7 => Progress.ToString("0", System.Globalization.CultureInfo.InvariantCulture) + "%",
        _ => string.Empty
    };
}

internal enum MclslRankFilterType
{
    And,
    Or,
    Not
}

internal struct MclslRankFilterArgs
{
    internal string Id;
    internal string DisplayName;
    internal Sprite Icon;
    internal Sprite InnerIcon;
    internal Color IconColor;
    internal Color InnerColor;
    internal Func<Actor, bool> FilterFunc;
}

internal sealed class MclslRankFilterSetting
{
    internal readonly string Id;
    internal readonly string DisplayName;
    internal readonly Sprite Icon;
    internal readonly Sprite InnerIcon;
    internal readonly Color IconColor;
    internal readonly Color InnerColor;
    internal readonly Func<Actor, bool> FilterFunc;
    internal MclslRankFilterType Type;
    internal bool ToBeRemoved;

    internal MclslRankFilterSetting(MclslRankFilterArgs a)
    {
        Id = a.Id ?? string.Empty;
        DisplayName = string.IsNullOrWhiteSpace(a.DisplayName) ? Id : a.DisplayName;
        Icon = a.Icon;
        InnerIcon = a.InnerIcon;
        IconColor = a.IconColor;
        InnerColor = a.InnerColor;
        FilterFunc = a.FilterFunc;
        Type = MclslRankFilterType.And;
    }

    internal void CycleType()
    {
        Type = Type switch
        {
            MclslRankFilterType.And => MclslRankFilterType.Or,
            MclslRankFilterType.Or => MclslRankFilterType.Not,
            _ => MclslRankFilterType.And
        };
    }

    internal string GetTypeText()
    {
        return Type switch
        {
            MclslRankFilterType.And => "与",
            MclslRankFilterType.Or => "或",
            MclslRankFilterType.Not => "非",
            _ => "与"
        };
    }

    internal Color GetTypeColor()
    {
        return Type switch
        {
            MclslRankFilterType.And => new Color(0.28f, 0.45f, 0.56f, 0.95f),
            MclslRankFilterType.Or => new Color(0.39f, 0.56f, 0.64f, 0.95f),
            MclslRankFilterType.Not => new Color(0.48f, 0.31f, 0.31f, 0.95f),
            _ => new Color(0.30f, 0.41f, 0.48f, 0.95f)
        };
    }
}

internal sealed class MclslRankSortDef
{
    internal readonly string Id;
    internal readonly string Name;
    internal readonly string IconPath;
    internal readonly Func<MclslRankEntry, float> GetValue;
    internal readonly Func<MclslRankEntry, string> GetDisplay;

    internal MclslRankSortDef(string id, string name, string iconPath, Func<MclslRankEntry, float> getValue, Func<MclslRankEntry, string> getDisplay)
    {
        Id = id;
        Name = name;
        IconPath = iconPath;
        GetValue = getValue;
        GetDisplay = getDisplay;
    }
}

internal sealed class MclslRankSortKey
{
    internal readonly MclslRankSortDef Def;
    internal bool Ascending;

    internal MclslRankSortKey(MclslRankSortDef def)
    {
        Def = def;
    }

    internal void Toggle() => Ascending = !Ascending;
    internal int Compare(MclslRankEntry left, MclslRankEntry right) =>
        (Ascending ? 1 : -1) * Def.GetValue(left).CompareTo(Def.GetValue(right));
}

internal sealed class MclslKingdomCodexEntry
{
    internal string Name = string.Empty;
    internal int TotalCultivators;
    internal readonly Dictionary<string, int> RealmCounts = new(StringComparer.Ordinal);
    internal IReadOnlyList<MclslKingdomCultivatorEntry> Cultivators = Array.Empty<MclslKingdomCultivatorEntry>();
    internal readonly Dictionary<string, IReadOnlyList<MclslKingdomCultivatorEntry>> CultivatorsByRealm = new(StringComparer.Ordinal);

    internal IReadOnlyList<MclslKingdomCultivatorEntry> CultivatorsForRealm(string realmId)
    {
        if (string.IsNullOrWhiteSpace(realmId) || string.Equals(realmId, MclslEventCatalog.All, StringComparison.Ordinal))
            return Cultivators;
        return CultivatorsByRealm.TryGetValue(realmId, out IReadOnlyList<MclslKingdomCultivatorEntry> list)
            ? list
            : Array.Empty<MclslKingdomCultivatorEntry>();
    }
}

internal sealed class MclslKingdomCultivatorEntry
{
    internal long ActorId;
    internal string Name = string.Empty;
    internal string RealmId = string.Empty;
    internal string RealmName = string.Empty;
    internal int RealmIndex;
    internal int TrueEssence;
    internal long Contribution;
}
