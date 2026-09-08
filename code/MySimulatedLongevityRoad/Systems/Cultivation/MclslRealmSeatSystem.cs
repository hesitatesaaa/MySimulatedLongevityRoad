using System;
using System.Linq;
using MySimulatedLongevityRoad.Data;

namespace MySimulatedLongevityRoad.Systems;

internal static class MclslRealmSeatSystem
{
    internal const int DivineTransformationLimit = 20;
    internal const int LongevityLimit = 5;

    internal static int NascentSoulLimit()
    {
        MclslWorldRunState run = MclslWorldRunRepository.Current;
        return Math.Max(0, run?.WorldCaves?.Count(x => x != null) ?? 0);
    }

    internal static int DivineTransformationLimitCurrent() => DivineTransformationLimit;

    internal static int HarmonyLimit()
    {
        MclslWorldRunState run = MclslWorldRunRepository.Current;
        int soulCount = run?.WorldSouls?.Count(x => x != null) ?? 0;
        return soulCount > 0 ? Math.Min(15, soulCount) : 10;
    }

    internal static int LongevityLimitCurrent() => LongevityLimit;

    internal static bool CanAddNascentSoul(out string reason) =>
        CanAddAtLeast(MclslRealmIds.YuanYing, NascentSoulLimit(), "元婴席位", "洞天", out reason);

    internal static bool CanAddDivineTransformation(out string reason) =>
        CanAddAtLeast(MclslRealmIds.HuaShen, DivineTransformationLimitCurrent(), "化神席位", "天地之变承载上限", out reason);

    internal static bool CanAddHarmony(out string reason) =>
        CanAddAtLeast(MclslRealmIds.HeDao, HarmonyLimit(), "合道席位", "天地之魄", out reason);

    internal static bool CanAddLongevity(out string reason) =>
        CanAddAtLeast(MclslRealmIds.ChangSheng, LongevityLimitCurrent(), "长生席位", "已逆之理", out reason);

    internal static bool CanGrantRealm(Actor actor, string targetRealm, out string reason)
    {
        reason = string.Empty;
        int target = MclslRealmIds.Index(targetRealm);
        int current = MclslRealmIds.Index(MclslActorAccessor.Realm(actor));
        if (target < 0 || target <= current) return true;

        if (target >= MclslRealmIds.Index(MclslRealmIds.YuanYing)
            && current < MclslRealmIds.Index(MclslRealmIds.YuanYing)
            && !CanAddNascentSoul(out reason)) return false;

        if (target >= MclslRealmIds.Index(MclslRealmIds.HuaShen)
            && current < MclslRealmIds.Index(MclslRealmIds.HuaShen)
            && !CanAddDivineTransformation(out reason)) return false;

        if (target >= MclslRealmIds.Index(MclslRealmIds.HeDao)
            && current < MclslRealmIds.Index(MclslRealmIds.HeDao)
            && !CanAddHarmony(out reason)) return false;

        if (target >= MclslRealmIds.Index(MclslRealmIds.ChangSheng)
            && current < MclslRealmIds.Index(MclslRealmIds.ChangSheng)
            && !CanAddLongevity(out reason)) return false;

        return true;
    }

    private static bool CanAddAtLeast(string realm, int limit, string seatName, string sourceName, out string reason)
    {
        int used = MclslCultivatorCandidateIndex.CountRealmAtLeast(realm);
        if (used < limit)
        {
            reason = string.Empty;
            return true;
        }

        reason = seatName + "已满：" + sourceName + limit + "，当前" + MclslRealmIds.Display(realm) + "及以上" + used + "人";
        return false;
    }
}
