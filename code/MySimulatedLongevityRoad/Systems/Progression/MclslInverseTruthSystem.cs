using System;
using System.Collections.Generic;
using MySimulatedLongevityRoad.Data;
using MySimulatedLongevityRoad.Core;

namespace MySimulatedLongevityRoad.Systems;

internal static class MclslInverseTruthSystem
{
    private readonly struct Challenger
    {
        internal readonly long Id;
        internal readonly int Strength;
        internal Challenger(Actor actor) { Id = MclslActorAccessor.Id(actor); Strength = ChallengeStrength(actor); }
    }
    private static readonly string[] AnnualEffects =
    {
        "truth_chuanfa_new_law", "truth_one_heart", "truth_wuyou", "truth_wangsheng",
        "truth_human_will", "truth_true_unreal", "truth_player_failure_steps",
        "truth_player_weak_not_fixed", "truth_player_duty_not_fixed", "truth_player_flawed_dao",
        "truth_player_all_laws_one", "truth_player_reincarnation_unbroken", "truth_player_lifespan_drain"
    };
    private static readonly MclslOrderedIdIndex<Challenger> ChallengerOrder = new((a, b) => b.Strength.CompareTo(a.Strength));
    private static MclslInverseAnnualState _orderingState;
    private static MclslInverseAnnualState _captureState;

    internal static bool TickAnnual(int year)
    {
        MclslWorldRunState run = MclslWorldRunRepository.Current;
        if (run?.InverseTruths == null || run.InverseTruths.Count == 0) return true;
        MclslAnnualBatchState batch = run.AnnualBatch;
        MclslInverseAnnualState work = batch.InverseWork;
        if (work == null || work.Year != year)
        {
            if (!MclslDetectionGate.TryBeginAnnualJob(MclslDetectionGate.AnnualInverseTruth, year)) return true;
            work = batch.InverseWork = new MclslInverseAnnualState
            { Year = year, SourceCount = MclslActorRegistry.Snapshot().Count };
            for (int i = 0; i < AnnualEffects.Length; i++) work.Effects[i] = HasReversed(run, AnnualEffects[i]);
        }
        MclslWorldArchiveStore.MarkDirty();
        IReadOnlyList<Actor> actors = MclslActorRegistry.CreateView(work.ActorIds);
        switch (work.Phase)
        {
            case 0:
                // Registry slots are rebuilt on load. Restart the read-only capture
                // so a saved slot cursor cannot duplicate or omit current actors.
                if (!ReferenceEquals(_captureState, work))
                {
                    _captureState = work; work.Cursor = 0; work.ActorIds.Clear();
                    work.SourceCount = MclslActorRegistry.Snapshot().Count;
                }
                IReadOnlyList<Actor> source = MclslActorRegistry.Snapshot();
                for (int n = 0; n < 64 && work.Cursor < work.SourceCount && !MclslAnnualFrameBudget.Expired; n++)
                    work.ActorIds.Add(work.Cursor < source.Count ? MclslActorAccessor.Id(source[work.Cursor++]) : AdvanceEmpty(work));
                if (work.Cursor < work.SourceCount) return false;
                work.Cursor = 0; work.Phase = 1;
                if (work.Effects[10]) MclslTechniqueLineageSystem.TryFuseNewLawLineage(year);
                return false;
            case 1:
                if (!TickLimitedEffects(run, work, actors, year)) return false;
                work.Cursor = 0; work.Phase = 2; return false;
            case 2:
                if (work.Effects[11] && !MclslActorReincarnationSystem.TickApplyAnnual(work, actors, year)) return false;
                work.Cursor = 0; work.Phase = 3; return false;
            case 3:
                if (work.Effects[12] && !TickLifespanDrain(work, actors, year)) return false;
                work.Cursor = 0; work.Phase = 4; return false;
            case 4:
            case 5:
                // Selection has no gameplay effects. A reload may safely restart
                // its partial order, whereas completed effects keep their saved cursors.
                if (!ReferenceEquals(_orderingState, work))
                {
                    ChallengerOrder.Clear(); _orderingState = work;
                    work.Phase = 4; work.Cursor = 0; work.ChallengerIds.Clear();
                }
                if (work.Phase == 4)
                {
                    for (int n = 0; n < 64 && work.Cursor < actors.Count && !MclslAnnualFrameBudget.Expired; n++)
                    {
                        Actor actor = actors[work.Cursor++];
                        if (MclslActorAccessor.Alive(actor) && MclslActorAccessor.Realm(actor) == MclslRealmIds.HeDao)
                            ChallengerOrder.Upsert(MclslActorAccessor.Id(actor), new Challenger(actor));
                    }
                    if (work.Cursor < actors.Count) return false;
                    work.Phase = 5; work.Cursor = 0;
                    work.Context = BuildContext(run, year);
                }
                for (int n = 0; n < 64 && work.Cursor < ChallengerOrder.Count && !MclslAnnualFrameBudget.Expired; n++)
                    work.ChallengerIds.Add(ChallengerOrder[work.Cursor++].Id);
                if (work.Cursor < ChallengerOrder.Count) return false;
                work.Phase = 6; work.Cursor = 0; ChallengerOrder.Clear(); return false;
            case 6:
                MclslInverseTruthContext context = work.Context;
                for (int n = 0; n < 8 && work.Cursor < work.ChallengerIds.Count && !MclslAnnualFrameBudget.Expired; n++)
                {
                    if (!MclslActorRegistry.Resolve(work.ChallengerIds[work.Cursor++], out Actor actor)
                        || !MclslActorAccessor.Alive(actor)) continue;
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
                if (work.Cursor < work.ChallengerIds.Count) return false;
                batch.InverseWork = null; Clear(); return true;
            default: throw new InvalidOperationException("Unknown inverse settlement phase");
        }
    }
    private static long AdvanceEmpty(MclslInverseAnnualState state) { state.Cursor++; return 0; }
    internal static void Clear() { ChallengerOrder.Clear(); _orderingState = _captureState = null; }

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

    private static bool TickLimitedEffects(MclslWorldRunState run, MclslInverseAnnualState state, IReadOnlyList<Actor> actors, int year)
    {
        bool chuanfa = state.Effects[0];
        bool oneHeart = state.Effects[1];
        bool wuyou = state.Effects[2];
        bool wangsheng = state.Effects[3];
        bool humanWill = state.Effects[4];
        bool trueUnreal = state.Effects[5];
        bool failureSteps = state.Effects[6];
        bool weakNotFixed = state.Effects[7];
        bool dutyNotFixed = state.Effects[8];
        bool flawedDao = state.Effects[9];
        bool allLawsOne = state.Effects[10];
        int chuanfaCount = state.EffectCounts[0];
        int oneHeartCount = state.EffectCounts[1];
        int wuyouCount = state.EffectCounts[2];
        int wangshengCount = state.EffectCounts[3];
        int humanWillCount = state.EffectCounts[4];
        int trueUnrealCount = state.EffectCounts[5];
        int failureCount = state.EffectCounts[6];
        int failureDivineCount = state.EffectCounts[7];
        int weakCount = state.EffectCounts[8];
        int dutyCount = state.EffectCounts[9];
        int flawedCount = state.EffectCounts[10];
        int allLawsCount = state.EffectCounts[11];
        bool changed = false;
        for (int n = 0; n < 32 && state.Cursor < actors.Count && !MclslAnnualFrameBudget.Expired; n++)
        {
            Actor actor = actors[state.Cursor++];
            if (!MclslActorAccessor.Alive(actor)) continue;
            long actorId = MclslActorAccessor.Id(actor);
            bool cultivator = (chuanfa || oneHeart || wuyou || wangsheng || humanWill || trueUnreal || weakNotFixed)
                && MclslActorAccessor.IsCultivator(actor);
            bool needsCultivationEligibility = humanWill || (!cultivator && (chuanfa || weakNotFixed));
            bool canCultivate = needsCultivationEligibility && MclslEligibility.CanCultivate(actor);

            if (chuanfa && chuanfaCount < 80 && !cultivator && canCultivate
                && PassesLimitedRoll(actorId, "truth_chuanfa_new_law", year))
            {
                AddClamped(actor, MclslActorDataKeys.ImmortalFate, 2, 0, 100);
                chuanfaCount++;
                changed = true;
            }
            if (oneHeart && oneHeartCount < 80 && cultivator
                && PassesLimitedRoll(actorId, "truth_one_heart", year))
            {
                AddClamped(actor, MclslActorDataKeys.TechniqueInsight, 1, 0, 9999);
                oneHeartCount++;
                changed = true;
            }
            if (wuyou && wuyouCount < 80 && cultivator
                && PassesLimitedRoll(actorId, "truth_wuyou", year))
            {
                AddClamped(actor, MclslActorDataKeys.MindState, 2, 0, 100);
                wuyouCount++;
                changed = true;
            }
            if (wangsheng && wangshengCount < 50 && cultivator
                && PassesLimitedRoll(actorId, "truth_wangsheng", year))
            {
                AddClamped(actor, MclslActorDataKeys.MindState, 1, 0, 100);
                AddClamped(actor, MclslActorDataKeys.HeartTemperingProgress, 1, 0, 100);
                wangshengCount++;
                changed = true;
            }
            if (humanWill && humanWillCount < 80 && canCultivate
                && PassesLimitedRoll(actorId, "truth_human_will", year))
            {
                if (cultivator) AddClamped(actor, MclslActorDataKeys.Contribution, 2, 0, 999999);
                else AddClamped(actor, MclslActorDataKeys.ImmortalFate, 1, 0, 100);
                humanWillCount++;
                changed = true;
            }
            if (trueUnreal && trueUnrealCount < 60 && cultivator
                && PassesLimitedRoll(actorId, "truth_true_unreal", year))
            {
                AddClamped(actor, MclslActorDataKeys.TechniqueInsight, 2, 0, 9999);
                trueUnrealCount++;
                changed = true;
            }

            if (failureSteps || dutyNotFixed)
            {
                string realm = MclslActorAccessor.Realm(actor);
                if (failureSteps && failureCount < 80 && realm == MclslRealmIds.JinDan
                    && PassesLimitedRoll(actorId, "truth_player_failure_steps", year))
                {
                    AddClamped(actor, MclslActorDataKeys.CaveClaimBonus, 1, 0, 100);
                    failureCount++;
                    changed = true;
                }
                if (failureSteps && failureDivineCount < 80 && realm == MclslRealmIds.YuanYing
                    && PassesLimitedRoll(actorId, "truth_player_failure_steps_divine", year))
                {
                    AddClamped(actor, MclslActorDataKeys.DivineClaimBonus, 1, 0, 100);
                    failureDivineCount++;
                    changed = true;
                }
                int realmIndex = dutyNotFixed ? MclslRealmIds.Index(realm) : -1;
                if (dutyNotFixed && dutyCount < 20
                    && realmIndex >= MclslRealmIds.Index(MclslRealmIds.HeDao)
                    && PassesLimitedRoll(actorId, "truth_player_duty_not_fixed", year))
                {
                    AddClamped(actor, MclslActorDataKeys.HarmonyStability, 1, 0, 100);
                    dutyCount++;
                    changed = true;
                }
            }
            if (weakNotFixed && weakCount < 100 && !cultivator && MclslEligibility.CanClaimWorldSoul(actor)
                && PassesLimitedRoll(actorId, "truth_player_weak_not_fixed", year))
            {
                AddClamped(actor, MclslActorDataKeys.ImmortalFate, 1, 0, 100);
                weakCount++;
                changed = true;
            }
            if (flawedDao && flawedCount < 80
                && MclslActorAccessor.GetString(actor, MclslActorDataKeys.CultivationSystem, string.Empty) == MclslCultivationSystemIds.NewLaw
                && PassesLimitedRoll(actorId, "truth_player_flawed_dao", year))
            {
                AddClamped(actor, MclslActorDataKeys.FoundationChanceBonus, 1, 0, 90);
                AddClamped(actor, MclslActorDataKeys.CaveClaimBonus, 1, 0, 95);
                AddClamped(actor, MclslActorDataKeys.DivineClaimBonus, 1, 0, 95);
                flawedCount++;
                changed = true;
            }
            if (allLawsOne && allLawsCount < 80
                && MclslActorAccessor.GetString(actor, MclslActorDataKeys.CultivationSystem, string.Empty) == MclslCultivationSystemIds.NewLaw
                && PassesLimitedRoll(actorId, "truth_player_all_laws_one", year))
            {
                AddClamped(actor, MclslActorDataKeys.TechniqueInsight, 2, 0, 9999);
                allLawsCount++;
                changed = true;
            }

        }
        state.EffectCounts[0] = chuanfaCount;
        state.EffectCounts[1] = oneHeartCount;
        state.EffectCounts[2] = wuyouCount;
        state.EffectCounts[3] = wangshengCount;
        state.EffectCounts[4] = humanWillCount;
        state.EffectCounts[5] = trueUnrealCount;
        state.EffectCounts[6] = failureCount;
        state.EffectCounts[7] = failureDivineCount;
        state.EffectCounts[8] = weakCount;
        state.EffectCounts[9] = dutyCount;
        state.EffectCounts[10] = flawedCount;
        state.EffectCounts[11] = allLawsCount;
        if (changed) MclslWorldArchiveStore.MarkDirty();
        return state.Cursor >= actors.Count;
    }

    private static bool TickLifespanDrain(MclslInverseAnnualState work, IReadOnlyList<Actor> actors, int year)
    {
        for (int n = 0; n < 64 && !MclslAnnualFrameBudget.Expired; n++)
        {
            if (work.DrainCount >= 24 || (work.DrainReceiverId == 0 && work.Cursor >= actors.Count)) return true;
            if (work.DrainReceiverId == 0)
            {
                Actor receiver = actors[work.Cursor++];
                if (!MclslActorAccessor.Alive(receiver) || !MclslActorAccessor.IsCultivator(receiver)) continue;
                string realm = MclslActorAccessor.Realm(receiver);
                if (MclslRealmIds.Index(realm) < MclslRealmIds.Index(MclslRealmIds.JinDan) || realm == MclslRealmIds.ChangSheng) continue;
                long receiverId = MclslActorAccessor.Id(receiver);
                if (StableHash(receiverId, "lifespan_drain", year) % 100 >= 18) continue;
                work.DrainReceiverId = receiverId; work.DrainDonorCursor = 0;
            }
            if (!MclslActorRegistry.Resolve(work.DrainReceiverId, out Actor target) || !MclslActorAccessor.Alive(target)
                || work.DrainDonorCursor >= actors.Count)
            { work.DrainReceiverId = 0; continue; }
            int start = StableHash(work.DrainReceiverId, "donor", year) % actors.Count;
            Actor donor = actors[(int)(((long)start + work.DrainDonorCursor++) % actors.Count)];
            if (!MclslActorAccessor.Alive(donor) || donor == target || !MclslEligibility.CanCultivate(donor)) continue;
            int receiverRealm = MclslRealmIds.Index(MclslActorAccessor.Realm(target));
            int donorRealm = MclslRealmIds.Index(MclslActorAccessor.Realm(donor));
            if (donorRealm >= receiverRealm && donorRealm >= 0) continue;
            int requested = 8 + receiverRealm * 3 + StableHash(work.DrainReceiverId, "lifespan_years", year) % 10;
            if (MclslLongevityRules.TryDrainYears(target, donor, requested, out int gained))
            {
                MclslActorAccessor.Set(target, MclslActorDataKeys.LastBreakthroughResult, "寿元可夺：从" + MclslActorAccessor.DisplayName(donor) + "身上夺得寿元" + gained + "年");
                work.DrainCount++;
            }
            work.DrainReceiverId = 0;
        }
        return false;
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

    private static bool PassesLimitedRoll(long actorId, string truthId, int year) => StableHash(actorId, truthId, year) % 100 < 35;

    private static void AddClamped(Actor actor, string key, int delta, int min, int max)
    {
        int current = MclslActorAccessor.GetInt(actor, key, min);
        MclslActorAccessor.Set(actor, key, Math.Clamp(current + delta, min, max));
    }

    private static MclslInverseTruthContext BuildContext(MclslWorldRunState run, int year)
    {
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
            SharedHighTechniqueGroups = MclslActorProjectionIndex.SharedHighTechniqueGroups,
            Cultivators = MclslActorProjectionIndex.HighCultivators,
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

    private static int StableHash(long actorId, string middle, int year)
    {
        unchecked
        {
            int hash = 67;
            hash = AppendPositiveNumber(hash, actorId);
            hash = hash * 41 + '|';
            for (int i = 0; i < middle.Length; i++) hash = hash * 41 + middle[i];
            hash = hash * 41 + '|';
            hash = AppendPositiveNumber(hash, year);
            return hash & int.MaxValue;
        }
    }

    private static int AppendPositiveNumber(int hash, long value)
    {
        if (value <= 0L) return hash * 41 + '0';
        long divisor = 1L;
        while (value / divisor >= 10L && divisor <= long.MaxValue / 10L) divisor *= 10L;
        do
        {
            hash = hash * 41 + (int)('0' + value / divisor % 10L);
            divisor /= 10L;
        } while (divisor > 0L);
        return hash;
    }

}
