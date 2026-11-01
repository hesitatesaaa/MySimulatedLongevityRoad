using System;
using MySimulatedLongevityRoad.Data;

namespace MySimulatedLongevityRoad.Systems;

internal readonly struct MclslBackgroundEraInfo
{
    internal readonly string Id;
    internal readonly string Name;
    internal readonly string Summary;
    internal readonly int StartYear;
    internal readonly int EndYear;

    internal MclslBackgroundEraInfo(string id, string name, string summary, int startYear, int endYear)
    {
        Id = id;
        Name = name;
        Summary = summary;
        StartYear = Math.Max(0, startYear);
        EndYear = Math.Max(0, endYear);
    }
}

internal static class MclslWorldEraCycleSystem
{
    internal const string AncientSeed = "ancient_seed";
    internal const string AncientFlourish = "ancient_flourish";
    internal const string AncientGreatCultivators = "ancient_great_cultivators";
    internal const string AncientPeak = "ancient_peak";
    internal const string TransmissionChange = "transmission_change";
    internal const string NewLawOrder = "new_law_order";
    internal const string NewLawDeepening = "new_law_deepening";

    internal static MclslBackgroundEraInfo Current(int year)
    {
        MclslWorldRunState run = MclslWorldRunRepository.Current;
        return Resolve(run, Math.Max(0, year));
    }

    internal static void TickAnnual(int year)
    {
        MclslWorldRunState run = MclslWorldRunRepository.Current;
        if (run == null || string.IsNullOrWhiteSpace(run.RunId)) return;

        MclslBackgroundEraInfo era = Resolve(run, year);
        if (string.Equals(run.CurrentBackgroundEraId, era.Id, StringComparison.Ordinal)
            && run.CurrentBackgroundEraStartYear == era.StartYear
            && run.CurrentBackgroundEraEndYear == era.EndYear)
        {
            return;
        }

        string previous = run.CurrentBackgroundEraId ?? string.Empty;
        run.CurrentBackgroundEraId = era.Id;
        run.CurrentBackgroundEraName = era.Name;
        run.CurrentBackgroundEraStartYear = era.StartYear;
        run.CurrentBackgroundEraEndYear = era.EndYear;

        if (!string.IsNullOrWhiteSpace(previous) && year > run.EraOriginYear + 1)
            MclslWorldRunRepository.AddEvent(year, "era_cycle_" + era.Id, era.Name, era.Summary);

        MclslWorldArchiveStore.MarkDirty();
    }

    private static MclslBackgroundEraInfo Resolve(MclslWorldRunState run, int year)
    {
        if (run == null || string.IsNullOrWhiteSpace(run.RunId))
            return new MclslBackgroundEraInfo(AncientSeed, "仙道纪元", "灵机初萌，众生始问仙途。", 0, 0);

        int origin = Math.Max(0, run.EraOriginYear);
        int ancientEnd = Math.Max(origin + 1, run.AncientLawEndYear);
        int transmissionEnd = Math.Max(ancientEnd + 1, run.TransmissionEndYear);
        int newLawStart = Math.Max(transmissionEnd, run.NewLawStartYear);

        if (year < ancientEnd)
        {
            int duration = Math.Max(1, ancientEnd - origin);
            int first = origin + Math.Max(1, duration / 5);
            int second = origin + Math.Max(2, duration / 2);
            int third = origin + Math.Max(3, duration * 4 / 5);

            if (year < first)
                return new MclslBackgroundEraInfo(AncientSeed, "仙法初传", "灵根初显，吐纳成法，零星传承始入人间。", origin, first);
            if (year < second)
                return new MclslBackgroundEraInfo(AncientFlourish, "功法繁盛", "师徒相授，诸法并起，仙道渐成世间显学。", first, second);
            if (year < third)
                return new MclslBackgroundEraInfo(AncientGreatCultivators, "大修并起", "金丹元婴相继现世，高修开始左右诸国兴衰。", second, third);
            return new MclslBackgroundEraInfo(AncientPeak, "仙道极盛", "仙道走至鼎盛，诸法传承亦埋下变世之因。", third, ancientEnd);
        }

        if (year < transmissionEnd)
        {
            return new MclslBackgroundEraInfo(TransmissionChange, "传法变世", "传法立道，新旧二途并行，旧日秩序渐崩。", ancientEnd, transmissionEnd);
        }

        int newLawOrderDuration = ScaledNewLawOrderDuration(origin, ancientEnd);
        if (year < newLawStart + newLawOrderDuration)
            return new MclslBackgroundEraInfo(NewLawOrder, "新法立序", "新法成为世间主流，万仙盟与五老会分据暗明。", newLawStart, newLawStart + newLawOrderDuration);

        return new MclslBackgroundEraInfo(NewLawDeepening, "长生求索", "诸修逐层夺天地之资，长生与逆理成为本世焦点。", newLawStart + newLawOrderDuration, 0);
    }

    private static int ScaledNewLawOrderDuration(int origin, int ancientEnd)
    {
        int ancientDuration = Math.Max(1, ancientEnd - origin);
        return Math.Clamp(ancientDuration / 2, 500, 1500);
    }
}
