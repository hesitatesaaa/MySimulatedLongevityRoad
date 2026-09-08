using System;
using MySimulatedLongevityRoad.Core;
using MySimulatedLongevityRoad.Data;

namespace MySimulatedLongevityRoad.Systems;

/// <summary>
/// 修炼主链的唯一真元写入口。
/// 年度修炼、事件收益与修为损失都先改真元，再由真元派生修炼进度。
/// </summary>
internal static class MclslCultivationGrowthSystem
{
    internal static float RealmGainScale(string realm) => realm switch
    {
        MclslRealmIds.ZhuJi => 1.4f,
        MclslRealmIds.JinDan => 2.8f,
        MclslRealmIds.YuanYing => 4.5f,
        MclslRealmIds.HuaShen => 6.2f,
        _ => 1f
    };

    internal static int CurrentTrueEssence(Actor actor) =>
        Math.Max(0, MclslActorAccessor.GetInt(actor, MclslActorDataKeys.TrueEssence, 0));

    internal static int GrantTrueEssence(
        Actor actor,
        string realm,
        float unscaledGain,
        int year,
        bool ancientLaw,
        bool applyWorldState = true,
        bool applySameLaw = true)
    {
        long actorId = MclslActorAccessor.Id(actor);
        if (actor?.data == null || unscaledGain <= 0f)
        {
            MclslDiagnostics.Cultivation(
                "growth.skip",
                "actor=" + actorId
                + " year=" + year
                + " realm=" + (realm ?? "")
                + " unscaled=" + unscaledGain
                + " reason=" + (actor?.data == null ? "actor-data-null" : "unscaled<=0"));
            return 0;
        }

        float gain = unscaledGain;
        // 天地状态和仙法不可同修可以减速，但不能把一个具有真实修炼资质的
        // 角色永久压成 0 增长。基础年度修炼至少保留 5% 世界效率、1% 同法效率，
        // 并保证本次年度写入至少产生 1 点真元。
        float worldRate = applyWorldState
            ? Math.Max(0.05f, MclslWorldStateModifierSystem.Current(year).CultivationRate)
            : 1f;
        float sameLawRate = applySameLaw
            ? Math.Max(0.01f, MclslTechniqueOccupationSystem.CultivationMultiplier(actor))
            : 1f;
        if (applyWorldState) gain *= worldRate;
        if (applySameLaw) gain *= sameLawRate;
        gain = Math.Max(1f, gain);

        float remainder = Math.Clamp(
            MclslActorAccessor.GetFloat(actor, MclslActorDataKeys.TrueEssenceRemainder, 0f),
            0f,
            0.999999f);
        float total = remainder + gain;
        int whole = Math.Max(0, (int)Math.Floor(total + 0.000001f));
        MclslActorAccessor.Set(actor, MclslActorDataKeys.TrueEssenceRemainder,
            Math.Clamp(total - whole, 0f, 0.999999f));

        if (whole > 0)
            MclslActorAccessor.Set(actor, MclslActorDataKeys.TrueEssence, CurrentTrueEssence(actor) + whole);

        SyncProgress(actor, realm, ancientLaw);
        MclslDiagnostics.Cultivation(
            "growth.write",
            "actor=" + actorId
            + " year=" + year
            + " realm=" + (realm ?? "")
            + " ancient=" + ancientLaw
            + " unscaled=" + unscaledGain
            + " worldRate=" + worldRate
            + " sameLawRate=" + sameLawRate
            + " finalGain=" + gain
            + " oldRemainder=" + remainder
            + " whole=" + whole
            + " newEssence=" + CurrentTrueEssence(actor)
            + " newRemainder=" + MclslActorAccessor.GetFloat(actor, MclslActorDataKeys.TrueEssenceRemainder, 0f));
        return whole;
    }

    internal static int ApplyProgressSetback(
        Actor actor,
        string realm,
        float progressLossPercent,
        bool ancientLaw,
        float minimumProgressPercent = 0f)
    {
        if (actor?.data == null || progressLossPercent <= 0f) return CurrentTrueEssence(actor);
        int span = MclslRealmProgress.RealmSpan(realm, ancientLaw);
        if (span <= 0) return CurrentTrueEssence(actor);

        int current = CurrentTrueEssence(actor);
        int entry = MclslRealmProgress.EntryMinimum(realm);
        int floor = MclslRealmProgress.EssenceAtProgress(realm, ancientLaw, minimumProgressPercent);
        int loss = Math.Max(1, (int)MathF.Round(span * progressLossPercent / 100f));
        int next = Math.Max(Math.Max(entry, floor), current - loss);
        SetTrueEssence(actor, realm, next, ancientLaw, enforceRealmMinimum: true);
        return next;
    }

    internal static void SetTrueEssence(
        Actor actor,
        string realm,
        int value,
        bool ancientLaw,
        bool enforceRealmMinimum,
        bool resetFractionalRemainder = true)
    {
        if (actor?.data == null) return;
        int next = Math.Max(0, value);
        if (enforceRealmMinimum && !string.IsNullOrWhiteSpace(realm))
            next = Math.Max(next, MclslRealmProgress.EntryMinimum(realm));
        MclslActorAccessor.Set(actor, MclslActorDataKeys.TrueEssence, next);
        if (resetFractionalRemainder)
            MclslActorAccessor.Set(actor, MclslActorDataKeys.TrueEssenceRemainder, 0f);
        SyncProgress(actor, realm, ancientLaw);
    }

    internal static void Reconcile(Actor actor, string realm, bool ancientLaw)
    {
        if (actor?.data == null) return;
        int essence = CurrentTrueEssence(actor);
        if (!string.IsNullOrWhiteSpace(realm))
        {
            int minimum = MclslRealmProgress.EntryMinimum(realm);
            if (minimum > 0 && essence < minimum)
            {
                essence = minimum;
                MclslActorAccessor.Set(actor, MclslActorDataKeys.TrueEssence, essence);
                MclslActorAccessor.Set(actor, MclslActorDataKeys.TrueEssenceRemainder, 0f);
            }
        }
        SyncProgress(actor, realm, ancientLaw);
    }

    internal static void SyncProgress(Actor actor, string realm, bool ancientLaw)
    {
        if (actor?.data == null) return;
        float fallback = MclslActorAccessor.GetFloat(actor, MclslActorDataKeys.CultivationProgress, 0f);
        float progress = MclslRealmProgress.ProgressForRealm(realm, CurrentTrueEssence(actor), ancientLaw, fallback);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.CultivationProgress, progress);
    }

    internal static bool MeetsNextRealmMinimum(
        Actor actor,
        string realm,
        bool ancientLaw,
        out int current,
        out int required)
    {
        current = CurrentTrueEssence(actor);
        required = MclslRealmProgress.NextRealmMinimum(realm, ancientLaw);
        return required > 0 && current >= required;
    }

    internal static void MaybeRecordSameLawPressure(Actor actor, int year)
    {
        float multiplier = MclslTechniqueOccupationSystem.CultivationMultiplier(actor);
        if (multiplier >= 0.7f) return;
        if (PositiveHash(MclslActorAccessor.Id(actor) + "|same_law_notice|" + year) % 100 >= 18) return;
        string techniqueName = MclslActorAccessor.GetString(actor, MclslActorDataKeys.TechniqueName, "此法");
        int count = MclslTechniqueOccupationSystem.CountFor(
            MclslActorAccessor.GetString(actor, MclslActorDataKeys.TechniqueId, string.Empty));
        MclslActorAccessor.Set(actor, MclslActorDataKeys.LastBreakthroughResult,
            "仙法不可同修：《" + techniqueName + "》同修" + count + "人，真元增长降为原来的" + Math.Max(1, (int)MathF.Round(multiplier * 100f)) + "%");
    }

    private static int PositiveHash(string value)
    {
        unchecked
        {
            int hash = 47;
            foreach (char c in value ?? string.Empty) hash = hash * 53 + c;
            return hash & int.MaxValue;
        }
    }
}
