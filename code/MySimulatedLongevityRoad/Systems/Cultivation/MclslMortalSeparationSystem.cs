using MySimulatedLongevityRoad.Data;

namespace MySimulatedLongevityRoad.Systems;

internal static class MclslMortalSeparationSystem
{
    internal static bool EnsureJudged(Actor actor, int year)
    {
        if (actor?.data == null) return false;
        if (MclslActorAccessor.GetInt(actor, MclslActorDataKeys.MortalSeparationChecked, 0) == 1)
            return MclslActorAccessor.GetInt(actor, MclslActorDataKeys.ImmortalFate, 0) > 0;

        int fate = RollFate(actor, year);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.MortalSeparationChecked, 1);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.ImmortalFate, fate);
        if (fate <= 0)
        {
            MclslActorAccessor.Set(actor, MclslActorDataKeys.LastBreakthroughResult, "未得入门仙缘，此生难入新法。");
            return false;
        }
        MclslActorAccessor.Set(actor, MclslActorDataKeys.LastBreakthroughResult, "得入门仙缘：" + fate);
        return true;
    }

    private static int RollFate(Actor actor, int year)
    {
        long id = MclslActorAccessor.Id(actor);
        int hash = PositiveHash(id + "|mortal_separation|" + year);
        int threshold = 18 + PositiveHash(actor.asset?.id ?? string.Empty) % 8;
        try { if (actor.city != null) threshold += 4; } catch (System.Exception mclslEmptyCatchEx) { MySimulatedLongevityRoad.Core.MclslDiagnostics.Error("empty-catch-code-MySimulatedLongevityRoad-Systems-Cultivation-MclslMortalSeparationSystem-cs-1", "空 catch 捕获: code/MySimulatedLongevityRoad/Systems/Cultivation/MclslMortalSeparationSystem.cs #1: " + mclslEmptyCatchEx.Message); }
        if (hash % 100 >= threshold) return 0;
        return 20 + PositiveHash(id + "|immortal_fate") % 81;
    }

    private static int PositiveHash(string value)
    {
        unchecked { int hash = 31; foreach (char c in value ?? string.Empty) hash = hash * 37 + c; return hash & int.MaxValue; }
    }
}
