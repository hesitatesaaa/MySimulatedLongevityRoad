using System.Collections.Generic;

namespace MySimulatedLongevityRoad.Systems;

internal static class MclslAnnualWorldRuntimeLane
{
    private enum Stage : byte
    {
        None = 0,
        Prepare = 1,
        EraCycle = 2,
        WorldCalamity = 3,
        Cave = 4,
        WorldChange = 5,
        WorldSoul = 6,
        InverseTruth = 7,
        Adventure = 8,
        TechniqueLineage = 9,
        SectLifecycle = 10,
        FactionMission = 11,
        FactionPressure = 12,
        Complete = 13
    }

    private static Stage _stage;
    private static int _activeYear;
    private static int _latestRequestedYear;
    private static bool _newLawEraActive;
    private static bool _newLawCultivationAvailable;
    private static MclslAnnualWorldSnapshot _snapshot;

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

        switch (_stage)
        {
            case Stage.Prepare:
                _snapshot = MclslAnnualWorldSnapshot.Build(lineageActors);
                MclslTechniqueOccupationSystem.Rebuild(_snapshot.LineageActors);
                if (_newLawCultivationAvailable)
                {
                    MclslWorldCaveSystem.BeginAnnual(_activeYear);
                    MclslWorldChangeSystem.BeginAnnual(_activeYear);
                }
                MclslAdventureSystem.BeginAnnual(_activeYear);
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

    internal static void Clear()
    {
        _stage = Stage.None;
        _activeYear = 0;
        _latestRequestedYear = 0;
        _newLawEraActive = false;
        _newLawCultivationAvailable = false;
        _snapshot = null;
    }

    private static void CompleteActiveYear()
    {
        if (_latestRequestedYear > _activeYear)
        {
            _activeYear = _latestRequestedYear;
            _newLawEraActive = MclslWorldEpochSystem.IsNewLawActive(_activeYear);
            _newLawCultivationAvailable = MclslNewLawPioneerSystem.CanPracticeNewLaw(_activeYear);
            _snapshot = null;
            _stage = Stage.Prepare;
            return;
        }

        Clear();
    }
}
