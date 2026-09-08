using System;
using System.Collections.Generic;
using MySimulatedLongevityRoad.Data;

namespace MySimulatedLongevityRoad.Systems;

internal static class MclslInverseTruthSystem
{
    internal static void TickAnnual(int year)
    {
        if (!MclslDetectionGate.TryBeginAnnualJob(MclslDetectionGate.AnnualInverseTruth, year)) return;
        MclslWorldRunState run = MclslWorldRunRepository.Current;
        if (run?.InverseTruths == null || run.InverseTruths.Count == 0) return;
        bool effectsChanged = ApplyReversedTruthEffects(run, year);

        IReadOnlyList<Actor> highUnits = MclslCultivatorCandidateIndex.SelectRealmAtLeast(
            MclslRealmIds.JinDan,
            0);
        List<Actor> candidates = new(MclslCultivatorCandidateIndex.SelectRealm(
            MclslRealmIds.HeDao,
            0,
            MclslActorAccessor.Alive,
            ChallengeStrength));
        if (candidates.Count == 0)
        {
            if (effectsChanged) MclslWorldArchiveStore.MarkDirty();
            return;
        }

        MclslInverseTruthContext context = BuildContext(run, highUnits, year);
        foreach (Actor actor in candidates)
        {
            if (MclslActorAccessor.Realm(actor) != MclslRealmIds.HeDao) continue;
            MclslInverseTruthRecord truth = EnsureChallenge(actor, run, context, year);
            if (truth == null || truth.Reversed) continue;

            int gain = ProgressGain(actor, truth, context);
            if (gain <= 0) continue;
            truth.Progress = Math.Clamp(truth.Progress + gain, 0, 100);
            truth.ChallengerActorId = MclslActorAccessor.Id(actor);
            MclslActorAccessor.Set(actor, MclslActorDataKeys.InverseTruthProgress, truth.Progress);
            MclslActorAccessor.Set(actor, MclslActorDataKeys.LastBreakthroughResult, "正在逆反天地之理“" + truth.Name + "”，进度" + truth.Progress + "%");

            if (truth.Progress >= 100) Complete(actor, truth, year);
        }
        MclslWorldArchiveStore.MarkDirty();
    }

    internal static void Clear()
    {
    }

    internal static bool IsTruthReversed(string truthId)
    {
        if (string.IsNullOrWhiteSpace(truthId)) return false;
        MclslWorldRunState run = MclslWorldRunRepository.Current;
        return HasReversed(run, truthId);
    }

    internal static int NewLawBreakthroughChanceBonus()
    {
        return IsTruthReversed("truth_player_flawed_dao") ? 12 : 0;
    }

    private static MclslInverseTruthRecord EnsureChallenge(Actor actor, MclslWorldRunState run, MclslInverseTruthContext context, int year)
    {
        string currentId = MclslActorAccessor.GetString(actor, MclslActorDataKeys.InverseTruthId, string.Empty);
        MclslInverseTruthRecord current = FindTruthById(run, currentId);
        if (current != null && !current.Reversed) return current;

        MclslInverseTruthRecord selected = PickChallenge(actor, run, context);
        if (selected == null) return null;

        selected.ChallengerActorId = MclslActorAccessor.Id(actor);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.InverseTruthId, selected.Id);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.InverseTruthName, selected.Name);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.InverseTruthProgress, selected.Progress);
        string displayName = MclslActorAccessor.DisplayName(actor, MclslRealmIds.HeDao);
        MclslWorldRunRepository.AddEvent(year, "inverse_truth_begin", displayName + "触及天地之理", "合道者“" + displayName + "”承接天职后，开始逆反“" + selected.Name + "”。" + selected.RuleDescription, actor);
        return selected;
    }

    private static int ChallengeScore(Actor actor, MclslInverseTruthRecord truth, MclslInverseTruthContext context)
    {
        int backlash = DutyBacklash(actor);
        int baseScore = DutyProgress(actor) - backlash / 2 + MclslActorAccessor.GetInt(actor, MclslActorDataKeys.Contribution, 0) / 20;
        string soul = MclslActorAccessor.GetString(actor, MclslActorDataKeys.WorldSoulName, string.Empty);
        string duty = MclslActorAccessor.GetString(actor, MclslActorDataKeys.HeavenlyDuty, string.Empty);
        string laws = MclslActorAccessor.GetString(actor, MclslActorDataKeys.DivineMarrowTags,
            MclslActorAccessor.GetString(actor, MclslActorDataKeys.GoldenCoreLaws, string.Empty));
        int lineage = MclslCultivationLineage.Integrity(actor);
        int harmony = MclslActorAccessor.GetInt(actor, MclslActorDataKeys.HarmonyCompatibility, 0);
        int unstable = 100 - MclslActorAccessor.GetInt(actor, MclslActorDataKeys.HarmonyStability, 50);

        return truth.Id switch
        {
            "truth_chuanfa_new_law" => baseScore + context.Cultivators / 12 + context.SharedHighTechniqueGroups * 12 + lineage / 3,
            "truth_one_heart" => baseScore + context.SharedHighTechniqueGroups * 24 + context.FiveEldersPressure / 2 + (laws.Contains("转化") || laws.Contains("阴") ? 15 : 0),
            "truth_wuyou" => baseScore + unstable / 2 + Math.Max(0, 60 - MclslActorAccessor.GetInt(actor, MclslActorDataKeys.MindState, 50)) + context.DeathRecords / 12,
            "truth_wangsheng" => baseScore + context.DeathRecords / 8 + context.HuanzhenReturns * 42 + (context.SpecialDeathRecords > 0 ? 20 : 0),
            "truth_mortal_miasma" => baseScore + context.MiasmaDeaths * 35 + MclslActorAccessor.GetInt(actor, MclslActorDataKeys.MortalMiasma, 0) + (duty.Contains("凡") || truth.Name.Contains("瘴") ? 15 : 0),
            "truth_human_will" => baseScore + MclslActorAccessor.GetInt(actor, MclslActorDataKeys.Contribution, 0) / 10 + context.CompletedCycles * 20 + context.Cultivators / 20,
            "truth_true_unreal" => baseScore + context.FutureKnowledge * 14 + context.HuanzhenReturns * 25 + (soul.Contains("幻") || laws.Contains("空间") || laws.Contains("隐匿") ? 20 : 0),
            "truth_player_many_paths" => baseScore + context.SharedHighTechniqueGroups * 30 + context.Cultivators / 10 + MclslActorAccessor.GetInt(actor, MclslActorDataKeys.TechniqueInsight, 0) / 5,
            "truth_player_failure_steps" => baseScore + context.SpecialDeathRecords * 24 + context.AvailableCaves * 5 + context.AvailableWorldChanges * 5 + unstable / 2,
            "truth_player_wounds_forge_body" => baseScore + context.DeathRecords / 12 + SafeAge(actor) / 8 + MclslRealmIds.Index(MclslActorAccessor.Realm(actor)) * 12,
            "truth_player_disaster_chance" => baseScore + context.DeathRecords / 10 + context.SpecialDeathRecords * 18 + context.AvailableWorldChanges * 8,
            "truth_player_weak_not_fixed" => baseScore + context.MiasmaDeaths * 14 + context.DeathRecords / 18 + Math.Max(0, 80 - lineage) + (leapLike(actor) ? 30 : 0),
            "truth_player_trace_persistence" => baseScore + context.AvailableCaves * 8 + context.AvailableWorldChanges * 8 + context.SpecialDeathRecords * 14 + context.HarmonyAndAbove * 8,
            "truth_player_lifespan_drain" => baseScore + SafeAge(actor) / 6 + Math.Max(0, 70 - MclslMindSystem.EnsureMindState(actor)) + context.Cultivators / 16,
            "truth_player_duty_not_fixed" => baseScore + DutyProgress(actor) + Math.Max(0, 80 - DutyBacklash(actor)) + context.HarmonyAndAbove * 12,
            "truth_player_flawed_dao" => baseScore + context.SpecialDeathRecords * 20 + unstable / 2 + context.AvailableCaves * 4 + context.AvailableWorldChanges * 4,
            "truth_player_all_laws_one" => baseScore + context.SharedHighTechniqueGroups * 28 + context.Cultivators / 12 + MclslActorAccessor.GetInt(actor, MclslActorDataKeys.TechniqueInsight, 0) / 4,
            "truth_player_reincarnation_unbroken" => baseScore + context.CompletedCycles * 45 + context.FutureKnowledge * 16 + context.HuanzhenReturns * 18,
            _ => 0
        };
    }

    private static int ProgressGain(Actor actor, MclslInverseTruthRecord truth, MclslInverseTruthContext context)
    {
        int duty = Math.Clamp(DutyProgress(actor) / 20, 1, 5);
        int backlash = Math.Clamp(DutyBacklash(actor) / 25, 0, 4);
        MclslAptitudeGiftDefinition gift = MclslAptitudeGiftCatalog.ForAptitude(MclslActorAccessor.GetInt(actor, MclslActorDataKeys.Aptitude, 50));
        int aptitude = Math.Clamp((int)Math.Round(gift.CultivationEfficiency * 2f) + (gift.InsightBonus + MclslSpiritualRootSystem.MultiLawInsightBonus(actor)) / 18 + gift.LawHarmonyBonus / 18, 1, 5);
        int stability = Math.Clamp(MclslActorAccessor.GetInt(actor, MclslActorDataKeys.HarmonyStability, 50), 0, 100);
        int unstable = 100 - stability;
        int mind = Math.Clamp(MclslMindSystem.StabilityBonus(actor) / 8, -1, 3);
        int baseGain = Math.Max(0, duty + aptitude + Math.Clamp(stability / 35, 0, 2) + mind - backlash);
        return truth.Id switch
        {
            "truth_chuanfa_new_law" => context.Cultivators >= 30 ? baseGain + Math.Clamp(context.Cultivators / 80, 1, 7) : 0,
            "truth_one_heart" => context.SharedHighTechniqueGroups > 0 || context.FiveEldersPressure >= 25 ? baseGain + context.SharedHighTechniqueGroups * 2 + context.FiveEldersPressure / 30 : 0,
            "truth_wuyou" => context.DeathRecords >= 8 || MclslActorAccessor.GetInt(actor, MclslActorDataKeys.MindState, 50) < 45 ? baseGain + Math.Min(8, context.DeathRecords / 70 + unstable / 18) : 0,
            "truth_wangsheng" => context.DeathRecords >= 10 || context.HuanzhenReturns > 0 ? baseGain + Math.Min(8, context.SpecialDeathRecords + context.DeathRecords / 80 + context.HuanzhenReturns * 4) : 0,
            "truth_mortal_miasma" => context.MiasmaDeaths > 0 || MclslActorAccessor.GetInt(actor, MclslActorDataKeys.MortalMiasma, 0) >= 40 ? baseGain + context.MiasmaDeaths * 4 + MclslActorAccessor.GetInt(actor, MclslActorDataKeys.MortalMiasma, 0) / 20 : 0,
            "truth_human_will" => MclslActorAccessor.GetInt(actor, MclslActorDataKeys.Contribution, 0) >= 200 || context.CompletedCycles > 0 ? baseGain + Math.Min(7, MclslActorAccessor.GetInt(actor, MclslActorDataKeys.Contribution, 0) / 180 + context.CompletedCycles * 3) : 0,
            "truth_true_unreal" => context.FutureKnowledge > 0 || context.HuanzhenReturns > 0 ? baseGain + context.FutureKnowledge + context.HuanzhenReturns * 3 : 0,
            "truth_player_many_paths" => context.SharedHighTechniqueGroups > 0 ? baseGain + Math.Min(8, context.SharedHighTechniqueGroups * 2 + context.Cultivators / 90) : 0,
            "truth_player_failure_steps" => context.SpecialDeathRecords > 0 || context.AvailableWorldChanges > 0 ? baseGain + Math.Min(7, context.SpecialDeathRecords * 2 + context.AvailableWorldChanges / 4 + unstable / 25) : 0,
            "truth_player_wounds_forge_body" => context.DeathRecords >= 8 ? baseGain + Math.Min(7, context.DeathRecords / 90 + SafeAge(actor) / 80) : 0,
            "truth_player_disaster_chance" => context.DeathRecords >= 10 || context.AvailableWorldChanges > 0 ? baseGain + Math.Min(8, context.DeathRecords / 80 + context.SpecialDeathRecords * 2 + context.AvailableWorldChanges / 4) : 0,
            "truth_player_weak_not_fixed" => context.MiasmaDeaths > 0 || leapLike(actor) ? baseGain + Math.Min(7, context.MiasmaDeaths * 2 + Math.Max(0, 80 - MclslCultivationLineage.Integrity(actor)) / 18) : 0,
            "truth_player_trace_persistence" => context.AvailableCaves > 0 || context.AvailableWorldChanges > 0 ? baseGain + Math.Min(8, (context.AvailableCaves + context.AvailableWorldChanges) / 5 + context.SpecialDeathRecords) : 0,
            "truth_player_lifespan_drain" => SafeAge(actor) >= MclslLongevityRules.ExpectedLifespan(actor, MclslActorAccessor.Realm(actor)) / 2 ? baseGain + Math.Min(7, SafeAge(actor) / 90 + context.Cultivators / 180) : 0,
            "truth_player_duty_not_fixed" => DutyProgress(actor) >= 80 ? baseGain + Math.Min(8, DutyProgress(actor) / 18 + context.HarmonyAndAbove) : 0,
            "truth_player_flawed_dao" => context.SpecialDeathRecords > 0 || context.AvailableCaves > 0 || context.AvailableWorldChanges > 0 ? baseGain + Math.Min(7, context.SpecialDeathRecords * 2 + unstable / 24 + (context.AvailableCaves + context.AvailableWorldChanges) / 6) : 0,
            "truth_player_all_laws_one" => context.SharedHighTechniqueGroups > 0 ? baseGain + Math.Min(8, context.SharedHighTechniqueGroups * 2 + context.Cultivators / 100) : 0,
            "truth_player_reincarnation_unbroken" => context.CompletedCycles > 0 || context.FutureKnowledge > 0 || context.HuanzhenReturns > 0 ? baseGain + Math.Min(8, context.CompletedCycles * 4 + context.FutureKnowledge + context.HuanzhenReturns * 2) : 0,
            _ => 0
        };
    }

    private static void Complete(Actor actor, MclslInverseTruthRecord truth, int year)
    {
        if (MclslActorAccessor.Realm(actor) != MclslRealmIds.ChangSheng
            && !MclslRealmSeatSystem.CanAddLongevity(out string seatReason))
        {
            truth.Progress = 99;
            MclslActorAccessor.Set(actor, MclslActorDataKeys.InverseTruthProgress, 99);
            MclslActorAccessor.Set(actor, MclslActorDataKeys.LastBreakthroughResult, seatReason);
            return;
        }
        if (!truth.CountsTowardLongevity)
        {
            MclslActorAccessor.Set(actor, MclslActorDataKeys.LastBreakthroughResult, "此逆理为原著既有天地规则，不属于本世可证长生席位");
            return;
        }
        truth.Reversed = true;
        truth.Progress = 100;
        MclslActorAccessor.Set(actor, MclslActorDataKeys.InverseTruthId, truth.Id);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.InverseTruthName, truth.Name);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.InverseTruthProgress, 100);
        MclslCultivationStateTransitions.TrySetCultivationSystem(actor, MclslCultivationSystemIds.NewLaw);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.AncientLawStatus, string.Empty);
        MclslCultivationSystem.SetRealm(actor, MclslRealmIds.ChangSheng, year, "逆天地之理“" + truth.Name + "”而证长生");
        try
        {
            actor.updateStats();
            float max = actor.getMaxHealth();
            if (max > 0f) actor.data.health = Math.Max(1, (int)Math.Min((float)int.MaxValue, max));
        }
        catch (System.Exception mclslEmptyCatchEx) { MySimulatedLongevityRoad.Core.MclslDiagnostics.Error("empty-catch-code-MySimulatedLongevityRoad-Systems-Progression-MclslInverseTruthSystem-cs-1", "空 catch 捕获: code/MySimulatedLongevityRoad/Systems/Progression/MclslInverseTruthSystem.cs #1: " + mclslEmptyCatchEx.Message); }
        string displayName = MclslActorAccessor.DisplayName(actor, MclslRealmIds.ChangSheng);
        MclslWorldRunRepository.AddEvent(year, "longevity_achieved", displayName + "证得长生", "其以合道天职为根，完成逆理工程，令“" + truth.Name + "”在本世出现可被仙鉴承认的例外。", actor);
        MclslAnnouncementSystem.Enqueue(displayName + "逆反“" + truth.Name + "”，证得长生。", "#D8C778", 12f, 1);
    }

    private static bool ApplyReversedTruthEffects(MclslWorldRunState run, int year)
    {
        bool changed = false;
        IReadOnlyList<Actor> actors = MclslCultivatorCandidateIndex.GetKnownActorsSnapshot();
        if (actors == null || actors.Count == 0) return false;

        if (HasReversed(run, "truth_chuanfa_new_law"))
            changed |= ApplyLimited(actors, year, "truth_chuanfa_new_law", 80,
                actor => !MclslActorAccessor.IsCultivator(actor) && MclslEligibility.CanCultivate(actor),
                actor => AddClamped(actor, MclslActorDataKeys.ImmortalFate, 2, 0, 100));

        if (HasReversed(run, "truth_one_heart"))
            changed |= ApplyLimited(actors, year, "truth_one_heart", 80,
                actor => MclslActorAccessor.IsCultivator(actor),
                actor => AddClamped(actor, MclslActorDataKeys.TechniqueInsight, 1, 0, 9999));

        if (HasReversed(run, "truth_wuyou"))
            changed |= ApplyLimited(actors, year, "truth_wuyou", 80,
                actor => MclslActorAccessor.IsCultivator(actor),
                actor => AddClamped(actor, MclslActorDataKeys.MindState, 2, 0, 100));

        if (HasReversed(run, "truth_wangsheng"))
            changed |= ApplyLimited(actors, year, "truth_wangsheng", 50,
                actor => MclslActorAccessor.IsCultivator(actor),
                actor =>
                {
                    AddClamped(actor, MclslActorDataKeys.MindState, 1, 0, 100);
                    AddClamped(actor, MclslActorDataKeys.HeartTemperingProgress, 1, 0, 100);
                });

        if (HasReversed(run, "truth_human_will"))
            changed |= ApplyLimited(actors, year, "truth_human_will", 80,
                actor => MclslEligibility.CanCultivate(actor),
                actor =>
                {
                    if (MclslActorAccessor.IsCultivator(actor)) AddClamped(actor, MclslActorDataKeys.Contribution, 2, 0, 999999);
                    else AddClamped(actor, MclslActorDataKeys.ImmortalFate, 1, 0, 100);
                });

        if (HasReversed(run, "truth_true_unreal"))
            changed |= ApplyLimited(actors, year, "truth_true_unreal", 60,
                actor => MclslActorAccessor.IsCultivator(actor),
                actor => AddClamped(actor, MclslActorDataKeys.TechniqueInsight, 2, 0, 9999));

        if (HasReversed(run, "truth_player_failure_steps"))
            changed |= ApplyLimited(actors, year, "truth_player_failure_steps", 80,
                actor => MclslActorAccessor.Realm(actor) == MclslRealmIds.JinDan,
                actor => AddClamped(actor, MclslActorDataKeys.CaveClaimBonus, 1, 0, 100));

        if (HasReversed(run, "truth_player_failure_steps"))
            changed |= ApplyLimited(actors, year, "truth_player_failure_steps_divine", 80,
                actor => MclslActorAccessor.Realm(actor) == MclslRealmIds.YuanYing,
                actor => AddClamped(actor, MclslActorDataKeys.DivineClaimBonus, 1, 0, 100));

        if (HasReversed(run, "truth_player_weak_not_fixed"))
            changed |= ApplyLimited(actors, year, "truth_player_weak_not_fixed", 100,
                actor => !MclslActorAccessor.IsCultivator(actor) && MclslEligibility.CanClaimWorldSoul(actor),
                actor => AddClamped(actor, MclslActorDataKeys.ImmortalFate, 1, 0, 100));

        if (HasReversed(run, "truth_player_duty_not_fixed"))
            changed |= ApplyLimited(actors, year, "truth_player_duty_not_fixed", 20,
                actor => MclslRealmIds.Index(MclslActorAccessor.Realm(actor)) >= MclslRealmIds.Index(MclslRealmIds.HeDao),
                actor => AddClamped(actor, MclslActorDataKeys.HarmonyStability, 1, 0, 100));

        if (HasReversed(run, "truth_player_flawed_dao"))
            changed |= ApplyLimited(actors, year, "truth_player_flawed_dao", 80,
                actor => MclslActorAccessor.GetString(actor, MclslActorDataKeys.CultivationSystem, string.Empty) == MclslCultivationSystemIds.NewLaw,
                actor =>
                {
                    AddClamped(actor, MclslActorDataKeys.FoundationChanceBonus, 1, 0, 90);
                    AddClamped(actor, MclslActorDataKeys.CaveClaimBonus, 1, 0, 95);
                    AddClamped(actor, MclslActorDataKeys.DivineClaimBonus, 1, 0, 95);
                });

        if (HasReversed(run, "truth_player_all_laws_one"))
        {
            changed |= MclslTechniqueLineageSystem.TryFuseNewLawLineage(year);
            changed |= ApplyLimited(actors, year, "truth_player_all_laws_one", 80,
                actor => MclslActorAccessor.GetString(actor, MclslActorDataKeys.CultivationSystem, string.Empty) == MclslCultivationSystemIds.NewLaw,
                actor => AddClamped(actor, MclslActorDataKeys.TechniqueInsight, 2, 0, 9999));
        }

        if (HasReversed(run, "truth_player_reincarnation_unbroken"))
            changed |= MclslActorReincarnationSystem.TryApplyAnnual(year, actors);

        if (HasReversed(run, "truth_player_lifespan_drain"))
            changed |= ApplyLifespanDrain(actors, year);

        return changed;
    }

    private static bool ApplyLifespanDrain(IReadOnlyList<Actor> actors, int year)
    {
        int changed = 0;
        for (int i = 0; i < actors.Count && changed < 24; i++)
        {
            Actor receiver = actors[i];
            if (!MclslActorAccessor.Alive(receiver) || !MclslActorAccessor.IsCultivator(receiver)) continue;
            string realm = MclslActorAccessor.Realm(receiver);
            int realmIndex = MclslRealmIds.Index(realm);
            if (realmIndex < MclslRealmIds.Index(MclslRealmIds.JinDan) || realm == MclslRealmIds.ChangSheng) continue;
            if (StableHash(MclslActorAccessor.Id(receiver) + "|lifespan_drain|" + year) % 100 >= 18) continue;
            Actor donor = PickLifespanDonor(actors, receiver, year);
            if (donor == null) continue;
            int requested = 8 + realmIndex * 3 + StableHash(MclslActorAccessor.Id(receiver) + "|lifespan_years|" + year) % 10;
            if (!MclslLongevityRules.TryDrainYears(receiver, donor, requested, out int gained)) continue;
            MclslActorAccessor.Set(receiver, MclslActorDataKeys.LastBreakthroughResult, "寿元可夺：从" + MclslActorAccessor.DisplayName(donor) + "身上夺得寿元" + gained + "年");
            changed++;
        }
        return changed > 0;
    }

    private static Actor PickLifespanDonor(IReadOnlyList<Actor> actors, Actor receiver, int year)
    {
        if (actors == null || actors.Count == 0 || receiver == null) return null;
        int start = StableHash(MclslActorAccessor.Id(receiver) + "|donor|" + year) % actors.Count;
        int receiverRealm = MclslRealmIds.Index(MclslActorAccessor.Realm(receiver));
        for (int i = 0; i < actors.Count; i++)
        {
            Actor donor = actors[(start + i) % actors.Count];
            if (!MclslActorAccessor.Alive(donor) || donor == receiver) continue;
            if (!MclslEligibility.CanCultivate(donor)) continue;
            int donorRealm = MclslRealmIds.Index(MclslActorAccessor.Realm(donor));
            if (donorRealm >= receiverRealm && donorRealm >= 0) continue;
            return donor;
        }
        return null;
    }

    private static bool HasReversed(MclslWorldRunState run, string truthId)
    {
        if (run?.InverseTruths == null || string.IsNullOrWhiteSpace(truthId)) return false;
        for (int i = 0; i < run.InverseTruths.Count; i++)
        {
            MclslInverseTruthRecord truth = run.InverseTruths[i];
            if (truth != null && truth.Reversed && truth.Id == truthId) return true;
        }
        return false;
    }

    private static bool ApplyLimited(IReadOnlyList<Actor> actors, int year, string truthId, int limit, Func<Actor, bool> filter, Action<Actor> apply)
    {
        int changed = 0;
        for (int i = 0; i < actors.Count && changed < limit; i++)
        {
            Actor actor = actors[i];
            if (!MclslActorAccessor.Alive(actor) || !filter(actor)) continue;
            if (StableHash(MclslActorAccessor.Id(actor) + "|" + truthId + "|" + year) % 100 >= 35) continue;
            apply(actor);
            changed++;
        }
        return changed > 0;
    }

    private static void AddClamped(Actor actor, string key, int delta, int min, int max)
    {
        int current = MclslActorAccessor.GetInt(actor, key, min);
        MclslActorAccessor.Set(actor, key, Math.Clamp(current + delta, min, max));
    }

    private static MclslInverseTruthContext BuildContext(MclslWorldRunState run, IReadOnlyList<Actor> units, int year)
    {
        Dictionary<string, int> highTechniqueCounts = new(StringComparer.Ordinal);
        for (int i = 0; i < units.Count; i++)
        {
            Actor actor = units[i];
            string technique = MclslActorAccessor.GetString(actor, MclslActorDataKeys.TechniqueId, string.Empty);
            if (string.IsNullOrWhiteSpace(technique)) continue;
            highTechniqueCounts[technique] = highTechniqueCounts.TryGetValue(technique, out int count) ? count + 1 : 1;
        }

        int terminalPressure = run.IsTerminal ? 80 : 0;
        MclslTimelineAnchorState terminal = FindTimelineAnchor(run, "anchor_xuanhuang_terminal");
        if (terminal != null)
        {
            if (terminal.Resolved) terminalPressure = Math.Max(terminalPressure, 70 + Math.Max(0, year - terminal.ResolvedYear) / 10);
            else if (terminal.ScheduledYear > 0) terminalPressure = Math.Max(terminalPressure, Math.Clamp(40 - Math.Abs(terminal.ScheduledYear - year) / 20, 0, 40));
        }

        return new MclslInverseTruthContext
        {
            DeathRecords = run.DeathRecords?.Count ?? 0,
            SpecialDeathRecords = CountDeathRecords(run, "ruin_exploration", "world_change_backlash"),
            MiasmaDeaths = CountDeathRecords(run, "mortal_miasma"),
            SharedHighTechniqueGroups = CountSharedHighTechniqueGroups(highTechniqueCounts),
            Cultivators = units?.Count ?? 0,
            FiveEldersPressure = run.BackgroundFactions?.FiveEldersSubversion ?? 0,
            HuanzhenReturns = CountHuanzhenReturns(),
            FutureKnowledge = (run.InheritedKnowledgeIds?.Count ?? 0) + CountAnchorDiscoveries(run),
            CompletedCycles = MclslReincarnationProfileStore.Current.CompletedCycles,
            TerminalPressure = Math.Clamp(terminalPressure, 0, 100),
            AvailableCaves = CountAvailableCaves(run),
            AvailableWorldChanges = CountAvailableWorldChanges(run),
            NascentAndAbove = MclslCultivatorCandidateIndex.CountRealmAtLeast(MclslRealmIds.YuanYing),
            HarmonyAndAbove = MclslCultivatorCandidateIndex.CountRealmAtLeast(MclslRealmIds.HeDao)
        };
    }

    private static int ChallengeStrength(Actor actor) =>
        DutyProgress(actor) + MclslActorAccessor.GetInt(actor, MclslActorDataKeys.Contribution, 0) / 10 + SafeAge(actor) / 5;

    private static int DutyProgress(Actor actor)
    {
        string soulId = MclslActorAccessor.GetString(actor, MclslActorDataKeys.WorldSoulId, string.Empty);
        if (string.IsNullOrWhiteSpace(soulId)) return 0;
        MclslWorldSoulRecord soul = FindWorldSoulById(soulId);
        return Math.Clamp(soul?.DutyProgress ?? 0, 0, 100);
    }

    private static int DutyBacklash(Actor actor)
    {
        string soulId = MclslActorAccessor.GetString(actor, MclslActorDataKeys.WorldSoulId, string.Empty);
        if (string.IsNullOrWhiteSpace(soulId)) return 0;
        MclslWorldSoulRecord soul = FindWorldSoulById(soulId);
        return Math.Clamp(soul?.DutyBacklash ?? MclslActorAccessor.GetInt(actor, MclslActorDataKeys.HeavenlyDutyBacklash, 0), 0, 100);
    }

    private static MclslInverseTruthRecord FindTruthById(MclslWorldRunState run, string id)
    {
        if (run?.InverseTruths == null || string.IsNullOrWhiteSpace(id)) return null;
        for (int i = 0; i < run.InverseTruths.Count; i++)
        {
            MclslInverseTruthRecord truth = run.InverseTruths[i];
            if (truth != null && truth.Id == id) return truth;
        }
        return null;
    }

    private static MclslInverseTruthRecord PickChallenge(Actor actor, MclslWorldRunState run, MclslInverseTruthContext context)
    {
        if (run?.InverseTruths == null) return null;
        long actorId = MclslActorAccessor.Id(actor);
        MclslInverseTruthRecord best = null;
        int bestScore = 0;
        for (int i = 0; i < run.InverseTruths.Count; i++)
        {
            MclslInverseTruthRecord truth = run.InverseTruths[i];
            if (truth == null || !truth.CountsTowardLongevity || truth.Reversed) continue;
            if (truth.ChallengerActorId > 0 && truth.ChallengerActorId != actorId) continue;
            int score = ChallengeScore(actor, truth, context);
            if (score <= bestScore) continue;
            best = truth;
            bestScore = score;
        }
        return best;
    }

    private static MclslTimelineAnchorState FindTimelineAnchor(MclslWorldRunState run, string anchorId)
    {
        if (run?.TimelineAnchors == null || string.IsNullOrWhiteSpace(anchorId)) return null;
        for (int i = 0; i < run.TimelineAnchors.Count; i++)
        {
            MclslTimelineAnchorState anchor = run.TimelineAnchors[i];
            if (anchor != null && anchor.AnchorId == anchorId) return anchor;
        }
        return null;
    }

    private static int CountDeathRecords(MclslWorldRunState run, params string[] causeCodes)
    {
        if (run?.DeathRecords == null || causeCodes == null || causeCodes.Length == 0) return 0;
        int count = 0;
        for (int i = 0; i < run.DeathRecords.Count; i++)
        {
            string cause = run.DeathRecords[i]?.CauseCode;
            if (string.IsNullOrWhiteSpace(cause)) continue;
            for (int j = 0; j < causeCodes.Length; j++)
            {
                if (cause == causeCodes[j])
                {
                    count++;
                    break;
                }
            }
        }
        return count;
    }

    private static int CountSharedHighTechniqueGroups(Dictionary<string, int> highTechniqueCounts)
    {
        if (highTechniqueCounts == null || highTechniqueCounts.Count == 0) return 0;
        int count = 0;
        foreach (KeyValuePair<string, int> pair in highTechniqueCounts)
            if (pair.Value >= 2) count++;
        return count;
    }

    private static int CountHuanzhenReturns()
    {
        List<MclslHuanzhenHistoryRecord> history = MclslHuanzhenSystem.Current?.History;
        if (history == null) return 0;
        int count = 0;
        for (int i = 0; i < history.Count; i++)
            if (history[i]?.Result == "还真成功") count++;
        return count;
    }

    private static int CountAnchorDiscoveries(MclslWorldRunState run)
    {
        if (run?.Discoveries == null) return 0;
        int count = 0;
        for (int i = 0; i < run.Discoveries.Count; i++)
        {
            string source = run.Discoveries[i]?.SourceAnchorId;
            if (!string.IsNullOrWhiteSpace(source) && source.Contains("anchor")) count++;
        }
        return count;
    }

    private static int CountAvailableCaves(MclslWorldRunState run)
    {
        if (run?.WorldCaves == null) return 0;
        int count = 0;
        for (int i = 0; i < run.WorldCaves.Count; i++)
        {
            MclslWorldCaveRecord cave = run.WorldCaves[i];
            if (cave != null && cave.RemainingEssence > 0 && cave.Integrity > 15) count++;
        }
        return count;
    }

    private static int CountAvailableWorldChanges(MclslWorldRunState run)
    {
        if (run?.WorldChanges == null) return 0;
        int count = 0;
        for (int i = 0; i < run.WorldChanges.Count; i++)
        {
            MclslWorldChangeRecord change = run.WorldChanges[i];
            if (change != null && change.RemainingMarrow > 0 && change.Intensity > 20 && change.State != "平息") count++;
        }
        return count;
    }

    private static MclslWorldSoulRecord FindWorldSoulById(string soulId)
    {
        MclslWorldRunState run = MclslWorldRunRepository.Current;
        if (run?.WorldSouls == null || string.IsNullOrWhiteSpace(soulId)) return null;
        for (int i = 0; i < run.WorldSouls.Count; i++)
        {
            MclslWorldSoulRecord soul = run.WorldSouls[i];
            if (soul != null && soul.Id == soulId) return soul;
        }
        return null;
    }

    private static int SafeAge(Actor actor)
    {
        try { return Math.Max(0, (int)actor.getAge()); } catch { return 0; }
    }

    private static bool leapLike(Actor actor) => MclslActorAccessor.GetInt(actor, MclslActorDataKeys.HarmonyLeap, 0) == 1;

    private static int StableHash(string value)
    {
        unchecked { int hash = 67; foreach (char c in value ?? string.Empty) hash = hash * 41 + c; return hash & int.MaxValue; }
    }

    private sealed class MclslInverseTruthContext
    {
        internal int DeathRecords;
        internal int SpecialDeathRecords;
        internal int MiasmaDeaths;
        internal int SharedHighTechniqueGroups;
        internal int Cultivators;
        internal int FiveEldersPressure;
        internal int HuanzhenReturns;
        internal int FutureKnowledge;
        internal int CompletedCycles;
        internal int TerminalPressure;
        internal int AvailableCaves;
        internal int AvailableWorldChanges;
        internal int NascentAndAbove;
        internal int HarmonyAndAbove;
    }
}
