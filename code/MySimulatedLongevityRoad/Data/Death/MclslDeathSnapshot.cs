namespace MySimulatedLongevityRoad.Data.Death;

internal readonly struct MclslDeathSnapshot
{
    internal static MclslDeathSnapshot Empty => default;

    internal readonly bool Found;
    internal readonly long ActorId;
    internal readonly string ActorName;
    internal readonly string RealmId;
    internal readonly int Year;
    internal readonly int Age;
    internal readonly string KingdomName;
    internal readonly string CityName;
    internal readonly int MapX;
    internal readonly int MapY;
    internal readonly string TechniqueName;
    internal readonly string CultivationSystem;
    internal readonly string AncientFoundationName;
    internal readonly string AncientCoreName;
    internal readonly string AncientDaoIntent;
    internal readonly string AncientNascentName;
    internal readonly string AncientDivineIntent;
    internal readonly string AncientDaoName;
    internal readonly string FoundationWonderName;
    internal readonly string GoldenCoreLaws;
    internal readonly string NascentCaveName;
    internal readonly string NascentEssenceName;
    internal readonly string DivineChangeName;
    internal readonly string DivineMarrowName;
    internal readonly string WorldSoulName;
    internal readonly string HeavenlyDuty;
    internal readonly string InverseTruthName;
    internal readonly bool HarmonyLeap;
    internal readonly string AttackTypeName;
    internal readonly string PendingCauseCode;
    internal readonly string PendingSource;
    internal readonly string PendingDetail;
    internal readonly string KillerName;
    internal readonly bool Important;

    internal MclslDeathSnapshot(bool found, long actorId, string actorName, string realmId, int year, int age,
        string kingdomName, string cityName, int mapX, int mapY, string techniqueName, string cultivationSystem,
        string ancientFoundationName, string ancientCoreName, string ancientDaoIntent, string ancientNascentName,
        string ancientDivineIntent, string ancientDaoName, string foundationWonderName, string goldenCoreLaws,
        string nascentCaveName, string nascentEssenceName, string divineChangeName, string divineMarrowName,
        string worldSoulName, string heavenlyDuty, string inverseTruthName, bool harmonyLeap,
        string attackTypeName, string pendingCauseCode, string pendingSource, string pendingDetail,
        string killerName, bool important)
    {
        Found = found; ActorId = actorId; ActorName = actorName ?? string.Empty; RealmId = realmId ?? string.Empty;
        Year = year < 0 ? 0 : year; Age = age < 0 ? 0 : age; KingdomName = kingdomName ?? string.Empty;
        CityName = cityName ?? string.Empty; MapX = mapX; MapY = mapY; TechniqueName = techniqueName ?? string.Empty;
        CultivationSystem = cultivationSystem ?? string.Empty; AncientFoundationName = ancientFoundationName ?? string.Empty;
        AncientCoreName = ancientCoreName ?? string.Empty; AncientDaoIntent = ancientDaoIntent ?? string.Empty;
        AncientNascentName = ancientNascentName ?? string.Empty; AncientDivineIntent = ancientDivineIntent ?? string.Empty;
        AncientDaoName = ancientDaoName ?? string.Empty; FoundationWonderName = foundationWonderName ?? string.Empty;
        GoldenCoreLaws = goldenCoreLaws ?? string.Empty; NascentCaveName = nascentCaveName ?? string.Empty;
        NascentEssenceName = nascentEssenceName ?? string.Empty; DivineChangeName = divineChangeName ?? string.Empty;
        DivineMarrowName = divineMarrowName ?? string.Empty; WorldSoulName = worldSoulName ?? string.Empty;
        HeavenlyDuty = heavenlyDuty ?? string.Empty; InverseTruthName = inverseTruthName ?? string.Empty;
        HarmonyLeap = harmonyLeap; AttackTypeName = attackTypeName ?? string.Empty;
        PendingCauseCode = pendingCauseCode ?? string.Empty; PendingSource = pendingSource ?? string.Empty;
        PendingDetail = pendingDetail ?? string.Empty; KillerName = killerName ?? string.Empty; Important = important;
    }
}
