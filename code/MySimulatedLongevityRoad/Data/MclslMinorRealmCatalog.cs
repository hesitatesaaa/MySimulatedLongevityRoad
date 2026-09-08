using System;

namespace MySimulatedLongevityRoad.Data;

internal enum MclslMinorRealmStage
{
    None = 0,
    Early = 1,
    Middle = 2,
    Late = 3,
    Perfect = 4
}

internal static class MclslMinorRealmCatalog
{
    internal static bool UsesMinorRealm(string realmId)
    {
        if (string.IsNullOrWhiteSpace(realmId)) return false;
        return realmId != MclslRealmIds.ChangSheng && MclslRealmIds.Index(realmId) >= 0;
    }

    internal static MclslMinorRealmStage Stage(float progress)
    {
        progress = Math.Clamp(progress, 0f, 100f);
        if (progress >= 75f) return MclslMinorRealmStage.Perfect;
        if (progress >= 50f) return MclslMinorRealmStage.Late;
        if (progress >= 25f) return MclslMinorRealmStage.Middle;
        return MclslMinorRealmStage.Early;
    }

    internal static string StageName(float progress) => StageName(Stage(progress));

    internal static string StageName(MclslMinorRealmStage stage) => stage switch
    {
        MclslMinorRealmStage.Early => "初期",
        MclslMinorRealmStage.Middle => "中期",
        MclslMinorRealmStage.Late => "后期",
        MclslMinorRealmStage.Perfect => "圆满",
        _ => string.Empty
    };

    internal static string ChronicleStageName(float progress)
    {
        return Stage(progress) == MclslMinorRealmStage.Perfect ? "大圆满" : StageName(progress);
    }

    internal static string Display(string realmId, float progress)
    {
        if (string.IsNullOrWhiteSpace(realmId)) return "凡俗";
        if (realmId == MclslRealmIds.ChangSheng) return "长生天尊";
        string realm = MclslRealmIds.Display(realmId);
        return UsesMinorRealm(realmId) ? realm + StageName(progress) : realm;
    }
}
