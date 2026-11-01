using System;
using MySimulatedLongevityRoad.Data;
using MySimulatedLongevityRoad.Systems.Death;

namespace MySimulatedLongevityRoad.Systems;

internal static partial class MclslWorldSoulSystem
{
    private static int DutyGain(Actor holder, MclslWorldSoulRecord soul)
    {
        int stability = Math.Clamp(MclslActorAccessor.GetInt(holder, MclslActorDataKeys.HarmonyStability, 35), 0, 100);
        int compatibility = Math.Clamp(MclslActorAccessor.GetInt(holder, MclslActorDataKeys.HarmonyCompatibility, MclslCultivationLineage.Compatibility(holder, soul.LawTags)), 0, 100);
        int gain = 1 + stability / 35 + compatibility / 50;
        if (MclslActorAccessor.GetInt(holder, MclslActorDataKeys.HarmonyLeap, 0) == 1 && stability < 45) gain = Math.Max(1, gain - 1);
        if (MclslInverseTruthSystem.IsTruthReversed("truth_player_duty_not_fixed")) gain++;
        return Math.Clamp(gain, 1, 5);
    }

    private static void ResolveDutyOutcome(Actor holder, MclslWorldSoulRecord soul, int year, int dutyGain)
    {
        if (!MclslActorAccessor.Alive(holder)) return;
        if (soul.DutyProgress >= 100 && soul.DutyCompletedYear <= 0)
        {
            CompleteDuty(holder, soul, year);
            return;
        }

        int interval = year - soul.LastDutyEventYear;
        if (interval < 8) return;
        int stability = Math.Clamp(MclslActorAccessor.GetInt(holder, MclslActorDataKeys.HarmonyStability, 40), 0, 100);
        int compatibility = Math.Clamp(MclslActorAccessor.GetInt(holder, MclslActorDataKeys.HarmonyCompatibility, 40), 0, 100);
        int mind = Math.Clamp(MclslMindSystem.StabilityBonus(holder), -20, 35);
        bool leap = MclslActorAccessor.GetInt(holder, MclslActorDataKeys.HarmonyLeap, 0) == 1;
        int pressure = 14 + soul.Quality * 5 + (leap ? 18 : 0)
            + Math.Max(0, 58 - stability) / 2
            + Math.Max(0, 48 - compatibility) / 3
            + Math.Max(0, 65 - soul.DutyProgress) / 5
            + soul.DutyBacklash / 8
            - mind / 3
            - dutyGain;
        pressure = Math.Clamp(pressure, 3, 68);
        if (MclslInverseTruthSystem.IsTruthReversed("truth_player_duty_not_fixed"))
            pressure = Math.Max(2, pressure - 10);
        int roll = StableHash(soul.Id + "|duty_backlash|" + MclslActorAccessor.Id(holder) + "|" + year) % 100;
        if (roll >= pressure) return;

        soul.LastDutyEventYear = year;
        int backlashGain = Math.Clamp(5 + pressure / 5 + (leap ? 3 : 0), 4, 22);
        soul.DutyBacklash = Math.Clamp(soul.DutyBacklash + backlashGain, 0, 100);
        int stabilityLoss = Math.Clamp(2 + pressure / 14, 2, 8);
        int newStability = Math.Max(0, stability - stabilityLoss);
        MclslActorAccessor.Set(holder, MclslActorDataKeys.HarmonyStability, newStability);
        MclslActorAccessor.Set(holder, MclslActorDataKeys.MindState, Math.Max(0, MclslMindSystem.EnsureMindState(holder) - 1));
        SyncHolderDutyData(holder, soul);

        string detail = "合道者“" + SafeName(holder) + "”承接天地之魄·" + soul.Name + "后，天职“" + soul.HeavenlyDuty + "”反噬形神，合道稳定降至" + newStability + "%，反噬积累" + soul.DutyBacklash + "%。";
        MclslActorAccessor.Set(holder, MclslActorDataKeys.LastBreakthroughResult, detail);
        MclslWorldRunRepository.AddEvent(year, "world_soul_duty_backlash", "天地之魄·" + soul.Name + "天职反噬", detail);
        if (soul.DutyBacklash >= 82 && newStability <= 18)
        {
            int deathRoll = StableHash(soul.Id + "|fatal_backlash|" + MclslActorAccessor.Id(holder) + "|" + year) % 100;
            int deathChance = Math.Clamp(10 + soul.DutyBacklash / 3 + (leap ? 12 : 0) - MclslMindSystem.StabilityBonus(holder) / 3, 8, 55);
            if (deathRoll < deathChance)
            {
                MclslDeathSystem.ExecuteScriptedDeath(holder, "world_soul_backlash", "天地之魄·" + soul.Name, "承接天职“" + soul.HeavenlyDuty + "”失败，魄核反噬形神，合道根基崩解。", true);
                return;
            }
        }
        if (soul.DutyBacklash >= 50 || newStability <= 30)
            MclslAnnouncementSystem.Enqueue(SafeName(holder) + "承接天地之魄·" + soul.Name + "时遭天职反噬。", "#D0B067", 8f, 1);
    }

    private static void CompleteDuty(Actor holder, MclslWorldSoulRecord soul, int year)
    {
        soul.DutyCompletedYear = year;
        soul.LastDutyEventYear = year;
        soul.DutyBacklash = Math.Max(0, soul.DutyBacklash - 35);
        int stability = Math.Clamp(MclslActorAccessor.GetInt(holder, MclslActorDataKeys.HarmonyStability, 40) + 10 + soul.Quality, 0, 100);
        int inverse = Math.Min(100, MclslActorAccessor.GetInt(holder, MclslActorDataKeys.InverseTruthProgress, 0) + 6 + soul.Quality);
        MclslActorAccessor.Set(holder, MclslActorDataKeys.HarmonyStability, stability);
        MclslActorAccessor.Set(holder, MclslActorDataKeys.InverseTruthProgress, inverse);
        MclslResourceSystem.AddContribution(holder, 80 + soul.Quality * 30);
        SyncHolderDutyData(holder, soul);
        string detail = SafeName(holder) + "完成天地之魄·" + soul.Name + "的天职“" + soul.HeavenlyDuty + "”，合道稳定升至" + stability + "%，逆理进度+" + (6 + soul.Quality) + "%。";
        MclslActorAccessor.Set(holder, MclslActorDataKeys.LastBreakthroughResult, detail);
        MclslWorldRunRepository.AddEvent(year, "world_soul_duty_complete", "天地之魄·" + soul.Name + "天职圆满", detail);
        AnnounceWorldSoul(SafeName(holder) + "完成天地之魄·" + soul.Name + "天职，逆理根基更进一步。", "#E2BE55", 9f);
    }

    private static void SyncHolderDutyData(Actor holder, MclslWorldSoulRecord soul)
    {
        if (!MclslActorAccessor.Alive(holder) || soul == null) return;
        MclslActorAccessor.Set(holder, MclslActorDataKeys.HeavenlyDutyBacklash, Math.Clamp(soul.DutyBacklash, 0, 100));
    }
}
