using System;
using System.Collections.Generic;
using System.Diagnostics;
using MySimulatedLongevityRoad.Core;
using MySimulatedLongevityRoad.Data;
using UnityEngine;

namespace MySimulatedLongevityRoad.Systems;

internal static class MclslAnnualWorldRuntimeLane
{
    private enum Stage : byte
    {
        None = 0,
        Prepare = 1,
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
        Family = 14,
        Complete = 16,
        BeastWave = 15
    }

    private static Stage _stage;
    private static int _activeYear;
    private static bool _newLawEraActive;
    private static bool _newLawCultivationAvailable;

    internal static bool HasPending => _stage != Stage.None;
    internal static string CurrentStageName => _stage switch
    {
        Stage.None => "无",
        Stage.Prepare => "准备",
        Stage.EraCycle => "时代轮转",
        Stage.WorldCalamity => "天地灾变",
        Stage.Cave => "洞天",
        Stage.WorldChange => "天地变",
        Stage.WorldSoul => "天地之魄",
        Stage.InverseTruth => "逆理",
        Stage.Adventure => "探索",
        Stage.TechniqueLineage => "功法传承",
        Stage.SectLifecycle => "宗门周期",
        Stage.FactionMission => "仙盟委托",
        Stage.FactionPressure => "势力施压",
        Stage.Family => "家族经营",
        Stage.BeastWave => "兽潮判定",
        Stage.Complete => "完成",
        _ => "未知"
    };

    internal static void Schedule(int year, bool newLawEraActive, bool newLawCultivationAvailable)
    {
        if (year <= 0) return;
        if (_stage == Stage.None)
        {
            _activeYear = year;
            _newLawEraActive = newLawEraActive;
            _newLawCultivationAvailable = newLawCultivationAvailable;
            byte savedStage = MclslWorldRunRepository.Current.AnnualBatch.WorldStage;
            _stage = savedStage >= (byte)Stage.Prepare && savedStage <= (byte)Stage.Complete
                ? (Stage)savedStage : Stage.Prepare;
        }
    }

    internal static bool TickWithinBudget(IReadOnlyList<Actor> lineageActors)
    {
        while (!MclslWorldRunRepository.Current.AnnualBatch.WorldBlocked
            && MclslAnnualFrameBudget.TryConsumeOperation())
        {
            Stage stage = _stage;
            long progress = MclslAnnualFrameBudget.ProgressVersion;
            if (Tick(lineageActors)) return true;
            // A committed target is progress even when the stage stays the same.
            // Waiting work must yield rather than spin on an unchanged cursor.
            if (stage == _stage && progress == MclslAnnualFrameBudget.ProgressVersion) break;
        }
        return false;
    }

    internal static bool Tick(IReadOnlyList<Actor> lineageActors)
    {
        if (_stage == Stage.None || _activeYear <= 0) return true;

        if (MclslAnnualFrameBudget.Expired) return false;
        double frameBudgetMs = MclslAnnualFrameBudget.RemainingMs;
        Stage sampledStage = _stage;
        long sample = MclslPerformanceProbe.Begin();
        long stageStarted = Stopwatch.GetTimestamp();
        try
        {
            using (MclslUnityProfiler.Sample(ProfilerName(sampledStage)))
            {
                bool completed = TickStage(lineageActors);
                MclslAnnualBatchState batch = MclslWorldRunRepository.Current.AnnualBatch;
                batch.WorldFailureStage = 0;
                batch.WorldFailureCount = 0;
                if (batch.WorldStage != (byte)_stage)
                {
                    batch.WorldStage = (byte)_stage;
                    MclslAnnualBackpressure.RecordProgress();
                    MclslWorldArchiveStore.MarkDirty();
                }
                return completed;
            }
        }
        catch (Exception ex)
        {
            MclslWorldRunRepository.BlockWorldAnnualSettlement(_activeYear, (byte)sampledStage,
                sampledStage.ToString(), ex.ToString());
            return false;
        }
        finally
        {
            MclslPerformanceProbe.End(ProbeName(sampledStage), sample);
            double elapsedMs = (Stopwatch.GetTimestamp() - stageStarted) * 1000d / Stopwatch.Frequency;
            double budgetMs = frameBudgetMs;
            if (elapsedMs > budgetMs)
                MclslDiagnostics.Error("annual-world-overbudget:" + sampledStage,
                    "世界年度单步超预算：年=" + _activeYear + " 阶段=" + sampledStage
                    + " 耗时=" + elapsedMs.ToString("F2") + "ms 预算=" + budgetMs.ToString("F2") + "ms");
        }
    }

    private static bool TickStage(IReadOnlyList<Actor> lineageActors)
    {
        switch (_stage)
        {
            case Stage.Prepare:
                if (MclslAnnualFrameBudget.Expired) return false;
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
                if (!_newLawCultivationAvailable) { _stage = Stage.WorldChange; return false; }
                if (!MclslWorldCaveSystem.TickResolveAnnual(_activeYear)) return false;
                _stage = Stage.WorldChange;
                return false;
            case Stage.WorldChange:
                if (!_newLawCultivationAvailable) { _stage = Stage.WorldSoul; return false; }
                if (!MclslWorldChangeSystem.TickResolveAnnual(_activeYear)) return false;
                _stage = Stage.WorldSoul;
                return false;
            case Stage.WorldSoul:
                if (!MclslWorldSoulSystem.TickAnnual(_activeYear)) return false;
                _stage = Stage.InverseTruth;
                return false;
            case Stage.InverseTruth:
                if (_newLawEraActive && !MclslInverseTruthSystem.TickAnnual(_activeYear)) return false;
                _stage = Stage.Adventure;
                return false;
            case Stage.Adventure:
                if (!MclslAdventureSystem.TickResolveAnnual(_activeYear)) return false;
                _stage = Stage.TechniqueLineage;
                return false;
            case Stage.TechniqueLineage:
                if (!MclslTechniqueLineageSystem.TickResolveAnnual(_activeYear,
                    lineageActors)) return false;
                _stage = Stage.SectLifecycle;
                return false;
            case Stage.SectLifecycle:
                if (!MclslSectLifecycleSystem.TickResolveAnnual(_activeYear)) return false;
                _stage = Stage.FactionMission;
                return false;
            case Stage.FactionMission:
                MclslFactionMissionSystem.TickAnnual(_activeYear);
                _stage = Stage.FactionPressure;
                return false;
            case Stage.FactionPressure:
                if (_newLawEraActive) MclslFactionPressureSystem.TickAnnual(_activeYear);
                _stage = Stage.Family;
                return false;
            case Stage.Family:
                if (!MclslFamilySystem.TickAnnual(_activeYear)) return false;
                _stage = Stage.BeastWave;
                return false;
            case Stage.BeastWave:
                if (!MclslBeastSurfaceIndex.Refresh()) return false;
                MclslBeastWorld.TickAnnual(_activeYear);
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
        MclslTianxuanMarket.PublishPendingArtifactListings(8);
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
        Stage.EraCycle => "年度世界.时代轮转",
        Stage.WorldCalamity => "年度世界.天地灾变",
        Stage.Cave => "年度世界.洞天",
        Stage.WorldChange => "年度世界.天地变",
        Stage.WorldSoul => "年度世界.天地之魄",
        Stage.InverseTruth => "年度世界.逆理",
        Stage.Adventure => "年度世界.探索",
        Stage.TechniqueLineage => "年度世界.功法传承",
        Stage.SectLifecycle => "年度世界.宗门周期",
        Stage.FactionMission => "年度世界.仙盟委托",
        Stage.FactionPressure => "年度世界.势力施压",
        Stage.Family => "年度世界.家族经营",
        Stage.BeastWave => "年度世界.兽潮判定",
        Stage.Complete => "年度世界.完成",
        _ => "年度世界.无"
    };

    private static string ProfilerName(Stage stage) => stage switch
    {
        Stage.Prepare => "MCLS/AnnualWorld/Prepare",
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
        Stage.Family => "MCLS/AnnualWorld/Family",
        Stage.BeastWave => "MCLS/AnnualWorld/BeastWave",
        Stage.Complete => "MCLS/AnnualWorld/Complete",
        _ => "MCLS/AnnualWorld/None"
    };

    internal static void Clear()
    {
        _stage = Stage.None;
        _activeYear = 0;
        _newLawEraActive = false;
        _newLawCultivationAvailable = false;
        MclslTechniqueLineageSystem.ClearRuntime();
    }

    private static void CompleteActiveYear()
    {
        Clear();
    }
}
