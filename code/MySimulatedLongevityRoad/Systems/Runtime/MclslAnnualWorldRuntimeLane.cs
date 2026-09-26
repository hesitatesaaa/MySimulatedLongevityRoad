using System.Collections.Generic;
using MySimulatedLongevityRoad.Core;

namespace MySimulatedLongevityRoad.Systems;

internal static class MclslAnnualWorldRuntimeLane
{
    private enum Stage : byte
    {
        None = 0,
        Prepare = 1,
        LegacyTechniqueMigration = 2,
        EraCycle = 3,
        WorldCalamity = 4,
        Cave = 5,
        WorldChange = 6,
        WorldSoul = 7,
        InverseTruth = 8,
        Adventure = 9,
        TechniqueLineage = 10,
        SectLifecycle = 11,
        FactionMission = 12,
        FactionPressure = 13,
        Complete = 14
    }

    private static Stage _stage;
    private static int _activeYear;
    private static int _latestRequestedYear;
    private static bool _newLawEraActive;
    private static bool _newLawCultivationAvailable;
    private static MclslAnnualWorldSnapshot _snapshot;
    private static MclslAnnualWorldSnapshot.Builder _snapshotBuilder;

    internal static bool HasPending => _stage != Stage.None;

    internal static void Schedule(int year, bool newLawEraActive, bool newLawCultivationAvailable)
    {
        if (year <= 0) return;
        _latestRequestedYear = year;
        _newLawEraActive = newLawEraActive;
        _newLawCultivationAvailable = newLawCultivationAvailable;
        if (_stage == Stage.None)
        {
            _activeYear = year;
            _stage = Stage.Prepare;
        }
    }

    internal static bool Tick(IReadOnlyList<Actor> lineageActors)
    {
        if (_stage == Stage.None || _activeYear <= 0) return true;

        Stage sampledStage = _stage;
        long sample = MclslPerformanceProbe.Begin();
        try
        {
            using (MclslUnityProfiler.Sample(ProfilerName(sampledStage)))
                return TickStage(lineageActors);
        }
        finally
        {
            MclslPerformanceProbe.End(ProbeName(sampledStage), sample);
        }
    }

    private static bool TickStage(IReadOnlyList<Actor> lineageActors)
    {
        switch (_stage)
        {
            case Stage.Prepare:
                if (_snapshot == null)
                {
                    _snapshotBuilder ??= MclslAnnualWorldSnapshot.BeginBuild(lineageActors);
                    if (!_snapshotBuilder.Tick(MclslRuntimeWorkBudget.ScaleCount(256, 16))) return false;
                    _snapshot = _snapshotBuilder.Complete();
                    _snapshotBuilder = null;
                }
                if (!MclslTechniqueOccupationSystem.TickLegacyTechniqueMigration(_snapshot.LineageActors,
                    MclslRuntimeWorkBudget.ScaleCount(192, 16)))
                {
                    _stage = Stage.LegacyTechniqueMigration;
                    return false;
                }
                BeginAnnualWorldSystems();
                _stage = Stage.EraCycle;
                return false;
            case Stage.LegacyTechniqueMigration:
                if (!MclslTechniqueOccupationSystem.TickLegacyTechniqueMigration(_snapshot?.LineageActors ?? lineageActors,
                    MclslRuntimeWorkBudget.ScaleCount(192, 16))) return false;
                BeginAnnualWorldSystems();
                _stage = Stage.EraCycle;
                return false;
            case Stage.EraCycle:
                MclslWorldEraCycleSystem.TickAnnual(_activeYear);
                _stage = Stage.WorldCalamity;
                return false;
            case Stage.WorldCalamity:
                MclslWorldCalamitySystem.TickAnnual(_activeYear);
                _stage = Stage.Cave;
                return false;
            case Stage.Cave:
                if (_newLawCultivationAvailable) MclslWorldCaveSystem.ResolveAnnual(_activeYear);
                _stage = Stage.WorldChange;
                return false;
            case Stage.WorldChange:
                if (_newLawCultivationAvailable) MclslWorldChangeSystem.ResolveAnnual(_activeYear);
                _stage = Stage.WorldSoul;
                return false;
            case Stage.WorldSoul:
                MclslWorldSoulSystem.TickAnnual(_activeYear);
                _stage = Stage.InverseTruth;
                return false;
            case Stage.InverseTruth:
                if (_newLawEraActive) MclslInverseTruthSystem.TickAnnual(_activeYear);
                _stage = Stage.Adventure;
                return false;
            case Stage.Adventure:
                MclslAdventureSystem.ResolveAnnual(_activeYear);
                _stage = Stage.TechniqueLineage;
                return false;
            case Stage.TechniqueLineage:
                MclslTechniqueLineageSystem.ResolveAnnual(_activeYear, _snapshot?.LineageActors ?? lineageActors);
                _stage = Stage.SectLifecycle;
                return false;
            case Stage.SectLifecycle:
                MclslSectLifecycleSystem.ResolveAnnual(_activeYear);
                _stage = Stage.FactionMission;
                return false;
            case Stage.FactionMission:
                if (_newLawEraActive) MclslFactionMissionSystem.TickAnnual(_activeYear);
                _stage = Stage.FactionPressure;
                return false;
            case Stage.FactionPressure:
                if (_newLawEraActive) MclslFactionPressureSystem.TickAnnual(_activeYear);
                _stage = Stage.Complete;
                return false;
            case Stage.Complete:
                CompleteActiveYear();
                return _stage == Stage.None;
            default:
                Clear();
                return true;
        }
    }

    private static void BeginAnnualWorldSystems()
    {
        if (_newLawCultivationAvailable)
        {
            MclslWorldCaveSystem.BeginAnnual(_activeYear);
            MclslWorldChangeSystem.BeginAnnual(_activeYear);
        }
        MclslAdventureSystem.BeginAnnual(_activeYear);
    }

    private static string ProbeName(Stage stage) => stage switch
    {
        Stage.Prepare => "年度世界.Prepare",
        Stage.LegacyTechniqueMigration => "年度世界.旧功法分流",
        Stage.EraCycle => "年度世界.EraCycle",
        Stage.WorldCalamity => "年度世界.WorldCalamity",
        Stage.Cave => "年度世界.Cave",
        Stage.WorldChange => "年度世界.WorldChange",
        Stage.WorldSoul => "年度世界.WorldSoul",
        Stage.InverseTruth => "年度世界.InverseTruth",
        Stage.Adventure => "年度世界.Adventure",
        Stage.TechniqueLineage => "年度世界.TechniqueLineage",
        Stage.SectLifecycle => "年度世界.SectLifecycle",
        Stage.FactionMission => "年度世界.FactionMission",
        Stage.FactionPressure => "年度世界.FactionPressure",
        Stage.Complete => "年度世界.Complete",
        _ => "年度世界.None"
    };

    private static string ProfilerName(Stage stage) => stage switch
    {
        Stage.Prepare => "MCLS/AnnualWorld/Prepare",
        Stage.LegacyTechniqueMigration => "MCLS/AnnualWorld/LegacyTechniqueMigration",
        Stage.EraCycle => "MCLS/AnnualWorld/EraCycle",
        Stage.WorldCalamity => "MCLS/AnnualWorld/WorldCalamity",
        Stage.Cave => "MCLS/AnnualWorld/Cave",
        Stage.WorldChange => "MCLS/AnnualWorld/WorldChange",
        Stage.WorldSoul => "MCLS/AnnualWorld/WorldSoul",
        Stage.InverseTruth => "MCLS/AnnualWorld/InverseTruth",
        Stage.Adventure => "MCLS/AnnualWorld/Adventure",
        Stage.TechniqueLineage => "MCLS/AnnualWorld/TechniqueLineage",
        Stage.SectLifecycle => "MCLS/AnnualWorld/SectLifecycle",
        Stage.FactionMission => "MCLS/AnnualWorld/FactionMission",
        Stage.FactionPressure => "MCLS/AnnualWorld/FactionPressure",
        Stage.Complete => "MCLS/AnnualWorld/Complete",
        _ => "MCLS/AnnualWorld/None"
    };

    internal static void Clear()
    {
        _stage = Stage.None;
        _activeYear = 0;
        _latestRequestedYear = 0;
        _newLawEraActive = false;
        _newLawCultivationAvailable = false;
        _snapshot = null;
        _snapshotBuilder = null;
        MclslTechniqueOccupationSystem.CancelLegacyTechniqueMigration();
    }

    private static void CompleteActiveYear()
    {
        if (_latestRequestedYear > _activeYear)
        {
            _activeYear = _latestRequestedYear;
            _newLawEraActive = MclslWorldEpochSystem.IsNewLawActive(_activeYear);
            _newLawCultivationAvailable = MclslNewLawPioneerSystem.CanPracticeNewLaw(_activeYear);
            _snapshot = null;
            _snapshotBuilder = null;
            _stage = Stage.Prepare;
            return;
        }

        Clear();
    }
}
