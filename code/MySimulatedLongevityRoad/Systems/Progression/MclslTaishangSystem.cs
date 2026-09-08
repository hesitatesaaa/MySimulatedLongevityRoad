using System;
using MySimulatedLongevityRoad.Data;
using MySimulatedLongevityRoad.Traits;

namespace MySimulatedLongevityRoad.Systems;

internal static class MclslTaishangSystem
{
    internal static void ProcessAnnual(Actor actor, int year, string realm)
    {
        if (actor?.data == null || realm != MclslRealmIds.ChangSheng) return;
        int current = Math.Clamp(MclslActorAccessor.GetInt(actor, MclslActorDataKeys.TaishangProgress, 0), 0, 100);
        if (current >= 100)
        {
            MclslTraitRegistration.EnsureTaishangStats(actor);
            MclslActorAccessor.Set(actor, MclslActorDataKeys.LastBreakthroughResult, "太上已成");
            return;
        }

        int truth = Math.Clamp(MclslActorAccessor.GetInt(actor, MclslActorDataKeys.InverseTruthProgress, 0), 0, 100);
        int mind = Math.Clamp(MclslMindSystem.EnsureMindState(actor), 0, 100);
        int gain = 1;
        if (truth >= 100) gain++;
        if (mind >= 80) gain++;
        if ((PositiveHash(MclslActorAccessor.Id(actor) + "|taishang|" + year) & 3) == 0) gain++;

        int next = Math.Min(100, current + gain);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.TaishangProgress, next);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.LastBreakthroughResult, next >= 100 ? "积岁证太上" : "太上积累：" + next + "/100");
        if (next >= 100) Complete(actor, year, true);
    }

    private static void Complete(Actor actor, int year, bool announce)
    {
        if (actor?.data == null) return;
        int current = Math.Clamp(MclslActorAccessor.GetInt(actor, MclslActorDataKeys.TaishangProgress, 0), 0, 100);
        bool firstTime = current < 100;
        MclslActorAccessor.Set(actor, MclslActorDataKeys.TaishangProgress, 100);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.LastBreakthroughResult, "积岁证太上");
        MclslTraitRegistration.EnsureTaishangStats(actor);
        MclslActorAccessor.ApplyDisplayName(actor, MclslRealmIds.ChangSheng);
        if (!announce || !firstTime) return;

        string name = MclslActorAccessor.DisplayName(actor, MclslRealmIds.ChangSheng);
        string truth = MclslActorAccessor.GetString(actor, MclslActorDataKeys.InverseTruthName, "未名逆理");
        if (string.IsNullOrWhiteSpace(truth)) truth = "未名逆理";
        string title = name + "证得太上";
        string body = "长生者“" + name + "”磨洗逆理“" + truth + "”，道果圆足，太上位成。";
        MclslWorldRunRepository.AddEvent(year, "taishang_achieved", title, body, actor);
        MclslAnnouncementSystem.Enqueue(title + "。", "#D8C778", 9f, 1);
    }

    private static int PositiveHash(string text)
    {
        unchecked
        {
            int hash = 23;
            string value = text ?? string.Empty;
            for (int i = 0; i < value.Length; i++)
                hash = hash * 31 + value[i];
            return hash & int.MaxValue;
        }
    }
}
