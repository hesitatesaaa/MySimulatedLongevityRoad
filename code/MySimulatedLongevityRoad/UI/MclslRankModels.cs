using System;
using UnityEngine;

namespace MySimulatedLongevityRoad.UI;

internal sealed class MclslRankEntry
{
    internal Actor Actor;
    internal long ActorId;
    internal string Name = string.Empty;
    internal string RealmId = string.Empty;
    internal string RealmName = string.Empty;
    internal string GiftName = string.Empty;
    internal string RootText = string.Empty;
    internal string RootAttributes = string.Empty;
    internal string NormalizedSearchText = string.Empty;
    internal string ExtraText = string.Empty;
    internal double Power;
    internal int RealmIndex;
    internal int Aptitude;
    internal int TrueEssence;
    internal int Contribution;
    internal int SpiritStones;
    internal int MindState;
    internal int MortalMiasma;
    internal int MortalMiasmaLimit;
}

internal enum MclslRankFilterType
{
    And,
    Or,
    Not
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

    internal MclslRankFilterSetting(string id, string displayName, Sprite icon, Sprite innerIcon, Color iconColor, Color innerColor, Func<Actor, bool> filterFunc)
    {
        Id = id ?? string.Empty;
        DisplayName = string.IsNullOrWhiteSpace(displayName) ? Id : displayName;
        Icon = icon;
        InnerIcon = innerIcon;
        IconColor = iconColor;
        InnerColor = innerColor;
        FilterFunc = filterFunc;
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
            MclslRankFilterType.And => new Color(0.25f, 0.55f, 0.25f, 0.95f),
            MclslRankFilterType.Or => new Color(0.25f, 0.4f, 0.75f, 0.95f),
            MclslRankFilterType.Not => new Color(0.65f, 0.25f, 0.25f, 0.95f),
            _ => Color.gray
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
}
