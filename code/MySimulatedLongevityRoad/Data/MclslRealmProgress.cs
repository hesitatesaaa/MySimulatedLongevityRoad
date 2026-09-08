using System;

namespace MySimulatedLongevityRoad.Data;

/// <summary>
/// 真元门槛采用“累计最低要求”语义，而不是境界真元上限。
/// 角色进入更高境界后会保留既有真元，真元也不会因达到门槛而被截断。
/// </summary>
internal static class MclslRealmProgress
{
    internal const int LianQiEntryMinimum = 800;
    internal const int ZhuJiEntryMinimum = 4000;
    internal const int JinDanEntryMinimum = 24000;
    internal const int YuanYingEntryMinimum = 120000;
    internal const int HuaShenEntryMinimum = 600000;

    // 新法合道依赖天地之魄；旧法化神则继续以真元累积合道。
    internal const int AncientHeDaoEntryMinimum = 2400000;

    internal static int EntryMinimum(string realmId) => realmId switch
    {
        MclslRealmIds.LianQi => LianQiEntryMinimum,
        MclslRealmIds.ZhuJi => ZhuJiEntryMinimum,
        MclslRealmIds.JinDan => JinDanEntryMinimum,
        MclslRealmIds.YuanYing => YuanYingEntryMinimum,
        MclslRealmIds.HuaShen => HuaShenEntryMinimum,
        MclslRealmIds.HeDao => 0,
        MclslRealmIds.ChangSheng => 0,
        _ => 0
    };

    /// <summary>
    /// 兼容UI与排行榜的旧调用名。当前语义与EntryMinimum一致：
    /// 返回进入该境界所需的累计最低真元，而不是境界真元上限。
    /// </summary>
    internal static int MinimumForRealm(string realmId) => EntryMinimum(realmId);

    internal static int NextRealmMinimum(string realmId, bool ancientLaw)
    {
        return realmId switch
        {
            null or "" or MclslRealmIds.Mortal => LianQiEntryMinimum,
            MclslRealmIds.LianQi => ZhuJiEntryMinimum,
            MclslRealmIds.ZhuJi => JinDanEntryMinimum,
            MclslRealmIds.JinDan => YuanYingEntryMinimum,
            MclslRealmIds.YuanYing => HuaShenEntryMinimum,
            MclslRealmIds.HuaShen when ancientLaw => AncientHeDaoEntryMinimum,
            _ => 0
        };
    }

    internal static int RealmSpan(string realmId, bool ancientLaw)
    {
        int entry = EntryMinimum(realmId);
        int next = NextRealmMinimum(realmId, ancientLaw);
        return next > entry ? next - entry : 0;
    }

    internal static float ProgressForRealm(string realmId, int trueEssence, bool ancientLaw, float fallback = 0f)
    {
        int safeEssence = Math.Max(0, trueEssence);
        if (string.IsNullOrWhiteSpace(realmId) || realmId == MclslRealmIds.Mortal)
            return Math.Clamp(safeEssence * 100f / LianQiEntryMinimum, 0f, 100f);

        int entry = EntryMinimum(realmId);
        int next = NextRealmMinimum(realmId, ancientLaw);
        if (next <= entry) return Math.Clamp(fallback, 0f, 100f);
        return Math.Clamp((safeEssence - entry) * 100f / Math.Max(1, next - entry), 0f, 100f);
    }

    internal static int EssenceAtProgress(string realmId, bool ancientLaw, float progress)
    {
        int entry = EntryMinimum(realmId);
        int span = RealmSpan(realmId, ancientLaw);
        if (span <= 0) return Math.Max(0, entry);
        return Math.Max(entry, entry + (int)MathF.Round(span * Math.Clamp(progress, 0f, 100f) / 100f));
    }
}
