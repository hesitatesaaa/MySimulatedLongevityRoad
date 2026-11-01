using System;
using MySimulatedLongevityRoad.Data;

namespace MySimulatedLongevityRoad.Systems;

internal readonly struct MclslCultivationAnnualSnapshot
{
    internal readonly long ActorId;
    internal readonly int Year;
    internal readonly string CultivationSystem;
    internal readonly string Realm;
    internal readonly int Aptitude;
    internal readonly int LastCultivationYear;
    internal readonly bool Alive;
    internal readonly bool Eligible;
    internal readonly bool HasCultivationGift;
    internal readonly bool AncientLaw;
    internal readonly bool IsLongevity;
    internal readonly MclslAptitudeGiftDefinition Gift;

    internal MclslCultivationAnnualSnapshot(
        long actorId,
        int year,
        string cultivationSystem,
        string realm,
        int aptitude,
        int lastCultivationYear,
        bool alive,
        bool eligible,
        MclslAptitudeGiftDefinition gift)
    {
        ActorId = actorId;
        Year = Math.Max(0, year);
        CultivationSystem = cultivationSystem ?? string.Empty;
        Realm = realm ?? string.Empty;
        Aptitude = Math.Clamp(aptitude, 0, 100);
        LastCultivationYear = lastCultivationYear;
        Alive = alive;
        Eligible = eligible;
        Gift = gift;
        HasCultivationGift = gift != null;
        AncientLaw = string.Equals(CultivationSystem, MclslCultivationSystemIds.AncientLaw, StringComparison.Ordinal);
        IsLongevity = string.Equals(Realm, MclslRealmIds.ChangSheng, StringComparison.Ordinal);
    }
}

internal readonly struct MclslCultivationLocalCheckResult
{
    internal readonly bool Passed;
    internal readonly string Reason;

    internal MclslCultivationLocalCheckResult(bool passed, string reason)
    {
        Passed = passed;
        Reason = reason ?? string.Empty;
    }
}

/// <summary>
/// 玄鉴式本地修炼核心：一次读取角色事实快照，再进行纯规则判断。
/// 调度器、UI 与事件系统不各自猜测角色是否应增长或能否进入下一境。
/// </summary>
internal static class MclslCultivationLocalCore
{
    internal static MclslCultivationAnnualSnapshot BuildAnnualSnapshot(Actor actor, int year)
    {
        long actorId = MclslActorAccessor.Id(actor);
        bool alive = MclslActorAccessor.Alive(actor);
        bool eligible = alive && MclslEligibility.CanCultivate(actor);
        string system = MclslActorAccessor.GetString(actor, MclslActorDataKeys.CultivationSystem, string.Empty);
        string realm = MclslActorAccessor.Realm(actor);
        int aptitude = Math.Clamp(MclslActorAccessor.GetInt(actor, MclslActorDataKeys.Aptitude, 0), 0, 100);
        MclslAptitudeGiftDefinition gift = MclslSpiritualRootSystem.GiftForCultivation(actor)
            ?? (aptitude > 0 ? MclslAptitudeGiftCatalog.ForAptitude(aptitude) : null);
        int lastYear = MclslCultivationSystem.NormalizeLastCultivationYear(
            actor,
            year,
            MclslActorAccessor.GetInt(actor, MclslActorDataKeys.LastCultivationYear, -1));
        return new MclslCultivationAnnualSnapshot(
            actorId,
            year,
            system,
            realm,
            aptitude,
            lastYear,
            alive,
            eligible,
            gift);
    }

    internal static MclslCultivationLocalCheckResult CheckAnnualStep(in MclslCultivationAnnualSnapshot snapshot)
    {
        if (snapshot.ActorId <= 0L) return new MclslCultivationLocalCheckResult(false, "角色数据为空");
        if (snapshot.Year <= 0) return new MclslCultivationLocalCheckResult(false, "年度为0，调度器没有拿到有效世界年份");
        if (!snapshot.Alive) return new MclslCultivationLocalCheckResult(false, "角色不是存活状态");
        if (!snapshot.Eligible) return new MclslCultivationLocalCheckResult(false, "角色不满足修炼资格");
        if (!snapshot.HasCultivationGift) return new MclslCultivationLocalCheckResult(false, "未识别到修炼特质或灵根档案");
        if (snapshot.IsLongevity) return new MclslCultivationLocalCheckResult(false, "长生境不再进行年度基础修炼");
        if (snapshot.LastCultivationYear >= snapshot.Year)
            return new MclslCultivationLocalCheckResult(false, "本逻辑年份已经写入过真元");
        return new MclslCultivationLocalCheckResult(true, "Ok");
    }

    internal static string ResolveNextRealmId(string currentRealm)
    {
        if (string.IsNullOrWhiteSpace(currentRealm)) return MclslRealmIds.LianQi;
        int index = MclslRealmIds.Index(currentRealm);
        if (index < 0 || index + 1 >= MclslRealmIds.Ordered.Length) return string.Empty;
        return MclslRealmIds.Ordered[index + 1];
    }
}
