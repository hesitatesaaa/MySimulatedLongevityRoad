using System;
using MySimulatedLongevityRoad.Data;
using MySimulatedLongevityRoad.Traits;

namespace MySimulatedLongevityRoad.Systems;

internal static class MclslMindSystem
{
    private static readonly string[] HeartPrefixes = { "清微", "守玄", "照心", "澄神", "观澜", "抱一", "明真", "定念", "归藏", "含光" };
    private static readonly string[] HeartRoots = { "炼心", "养神", "定魄", "澄念", "照妄", "守真", "凝神", "明心", "问道", "洗尘" };
    private static readonly string[] HeartSuffixes = { "诀", "经", "篇", "法", "章" };

    internal static int ReadMindState(Actor actor)
    {
        if (actor?.data == null) return 50;
        int current = MclslActorAccessor.GetInt(actor, MclslActorDataKeys.MindState, -1);
        if (current >= 1) return Math.Clamp(current, 1, 100);

        long id = MclslActorAccessor.Id(actor);
        int fate = Math.Clamp(MclslActorAccessor.GetInt(actor, MclslActorDataKeys.ImmortalFate, 0), 0, 100);
        int aptitude = Math.Clamp(MclslActorAccessor.GetInt(actor, MclslActorDataKeys.Aptitude, 45), 1, 100);
        int seed = PositiveHash(id + "|mind_seed");
        return Math.Clamp(20 + fate / 4 + aptitude / 6 + seed % 31, 10, 100);
    }

    internal static int EnsureMindState(Actor actor)
    {
        int value = ReadMindState(actor);
        if (actor?.data == null) return value;
        int current = MclslActorAccessor.GetInt(actor, MclslActorDataKeys.MindState, -1);
        if (current < 1) MclslActorAccessor.Set(actor, MclslActorDataKeys.MindState, value);
        return value;
    }

    internal static void ProcessAnnual(Actor actor, int year, string realm)
    {
        if (actor?.data == null || year <= 0) return;
        int lastYear = MclslActorAccessor.GetInt(actor, MclslActorDataKeys.LastHeartTemperingYear, -1);
        if (lastYear >= year) return;
        MclslActorAccessor.Set(actor, MclslActorDataKeys.LastHeartTemperingYear, year);

        int mind = EnsureMindState(actor);
        bool cultivator = !string.IsNullOrWhiteSpace(realm);
        TryDiscoverHeartMethod(actor, year, mind, cultivator);

        int progress = Math.Clamp(MclslActorAccessor.GetInt(actor, MclslActorDataKeys.HeartTemperingProgress, 0), 0, 100);
        bool knowsMethod = MclslActorAccessor.GetInt(actor, MclslActorDataKeys.HeartMethodKnown, 0) == 1;
        int fate = Math.Clamp(MclslActorAccessor.GetInt(actor, MclslActorDataKeys.ImmortalFate, 0), 0, 100);
        int realmIndex = Math.Max(0, MclslRealmIds.Index(realm));
        int gain = knowsMethod
            ? 1 + mind / 35 + fate / 70 + realmIndex / 2
            : (mind >= 70 ? 1 : 0) + (fate >= 55 ? 1 : 0);
        if (HasHuanzhen(actor)) gain += 1;
        if (PositiveHash(MclslActorAccessor.Id(actor) + "|heart_gain|" + year) % 100 < 18 + mind / 4) gain += 1;
        if (gain > 0)
        {
            progress = Math.Clamp(progress + gain, 0, 100);
            MclslActorAccessor.Set(actor, MclslActorDataKeys.HeartTemperingProgress, progress);
        }

        if (cultivator && progress >= 60 && PositiveHash(MclslActorAccessor.Id(actor) + "|mind_refine|" + year) % 100 < 12 + realmIndex * 2)
            MclslActorAccessor.Set(actor, MclslActorDataKeys.MindState, Math.Clamp(mind + 1, 1, 100));
    }

    internal static bool CanBeginByHeartMethod(Actor actor)
    {
        if (actor?.data == null) return false;
        return MclslActorAccessor.GetInt(actor, MclslActorDataKeys.HeartMethodKnown, 0) == 1
            && MclslActorAccessor.GetInt(actor, MclslActorDataKeys.HeartTemperingProgress, 0) >= 100;
    }

    internal static bool TryMiasmaPoolEntry(Actor actor, int year)
    {
        if (actor?.data == null || MclslActorAccessor.GetInt(actor, MclslActorDataKeys.MiasmaPoolCleansing, 0) > 0) return false;
        int mind = EnsureMindState(actor);
        int progress = Math.Clamp(MclslActorAccessor.GetInt(actor, MclslActorDataKeys.HeartTemperingProgress, 0), 0, 100);
        int fate = Math.Clamp(MclslActorAccessor.GetInt(actor, MclslActorDataKeys.ImmortalFate, 0), 0, 100);
        int chance = (progress >= 60 ? 4 : progress >= 35 ? 2 : 0) + (mind >= 75 ? 2 : 0) + (fate >= 50 ? 1 : 0);
        if (chance <= 0) return false;
        int roll = PositiveHash(MclslActorAccessor.Id(actor) + "|miasma_pool|" + year) % 1000;
        if (roll >= chance) return false;
        MclslActorAccessor.Set(actor, MclslActorDataKeys.MiasmaPoolCleansing, 1);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.HeartMethodKnown, 1);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.HeartTemperingProgress, 100);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.MindState, Math.Clamp(mind + 8, 1, 100));
        MclslActorAccessor.Set(actor, MclslActorDataKeys.LastBreakthroughResult, "误入洗瘴池，洗去凡浊，得以踏入新法。");
        return true;
    }

    internal static float CultivationMultiplier(Actor actor)
    {
        int mind = EnsureMindState(actor);
        int progress = Math.Clamp(MclslActorAccessor.GetInt(actor, MclslActorDataKeys.HeartTemperingProgress, 0), 0, 100);
        float multiplier = 0.78f + mind / 250f + progress / 600f;
        return Math.Clamp(multiplier, 0.70f, 1.28f);
    }

    internal static int BreakthroughAdjustment(Actor actor)
    {
        int mind = EnsureMindState(actor);
        int progress = Math.Clamp(MclslActorAccessor.GetInt(actor, MclslActorDataKeys.HeartTemperingProgress, 0), 0, 100);
        return Math.Clamp((mind - 50) / 8 + progress / 25, -8, 14);
    }

    internal static int ClaimStrengthBonus(Actor actor)
    {
        int mind = EnsureMindState(actor);
        int progress = Math.Clamp(MclslActorAccessor.GetInt(actor, MclslActorDataKeys.HeartTemperingProgress, 0), 0, 100);
        return Math.Clamp(mind / 2 + progress / 3, 5, 85);
    }

    internal static int StabilityBonus(Actor actor)
    {
        int mind = EnsureMindState(actor);
        int progress = Math.Clamp(MclslActorAccessor.GetInt(actor, MclslActorDataKeys.HeartTemperingProgress, 0), 0, 100);
        return Math.Clamp((mind - 50) / 5 + progress / 20, -10, 25);
    }

    internal static string MethodText(Actor actor)
    {
        if (actor?.data == null) return "无";
        if (MclslActorAccessor.GetInt(actor, MclslActorDataKeys.MiasmaPoolCleansing, 0) == 1)
            return IsAncientEpoch() ? GeneratedHeartMethodName(actor) : "洗瘴池";
        return MclslActorAccessor.GetInt(actor, MclslActorDataKeys.HeartMethodKnown, 0) == 1 ? GeneratedHeartMethodName(actor) : "未得法";
    }

    private static void TryDiscoverHeartMethod(Actor actor, int year, int mind, bool cultivator)
    {
        if (MclslActorAccessor.GetInt(actor, MclslActorDataKeys.HeartMethodKnown, 0) == 1) return;
        int fate = Math.Clamp(MclslActorAccessor.GetInt(actor, MclslActorDataKeys.ImmortalFate, 0), 0, 100);
        int ruin = Math.Clamp(MclslActorAccessor.GetInt(actor, MclslActorDataKeys.RuinExperience, 0), 0, 100);
        int chance = cultivator ? 20 + mind / 8 + ruin / 5 : 2 + mind / 20 + fate / 25;
        if (HasHuanzhen(actor)) chance += 25;
        if (PositiveHash(MclslActorAccessor.Id(actor) + "|heart_method|" + year) % 100 >= Math.Clamp(chance, 1, 85)) return;
        MclslActorAccessor.Set(actor, MclslActorDataKeys.HeartMethodKnown, 1);
        string method = GeneratedHeartMethodName(actor);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.LastBreakthroughResult, cultivator ? "参得《" + method + "》，心境修持入门。" : "机缘得《" + method + "》，凡心开始受炼。");
    }

    private static string GeneratedHeartMethodName(Actor actor)
    {
        long id = MclslActorAccessor.Id(actor);
        int seed = PositiveHash(id + "|heart_method_name");
        string prefix = HeartPrefixes[seed % HeartPrefixes.Length];
        string root = HeartRoots[(seed / HeartPrefixes.Length) % HeartRoots.Length];
        string suffix = HeartSuffixes[(seed / HeartPrefixes.Length / HeartRoots.Length) % HeartSuffixes.Length];
        return prefix + root + suffix;
    }

    private static bool IsAncientEpoch()
    {
        MclslWorldRunState run = MclslWorldRunRepository.Current;
        return run != null && string.Equals(run.CultivationEpoch, MclslWorldEpochSystem.AncientLawEpoch, StringComparison.Ordinal);
    }

    private static bool HasHuanzhen(Actor actor)
    {
        try { return actor != null && actor.hasTrait(MclslTraitRegistration.HuanzhenTraitId); }
        catch { return false; }
    }

    private static int PositiveHash(string value)
    {
        unchecked { int hash = 47; foreach (char c in value ?? string.Empty) hash = hash * 59 + c; return hash & int.MaxValue; }
    }
}
