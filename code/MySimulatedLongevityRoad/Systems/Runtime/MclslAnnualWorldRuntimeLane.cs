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
    private static bool _newLawEraActive;
    private static bool _newLawCultivationAvailable;
    private static MclslAnnualWorldSnapshot _snapshot;
    private static MclslAnnualWorldSnapshot.Builder _snapshotBuilder;
    private static int _retryAfterFrame;

    internal static bool HasPending => _stage != Stage.None;
    internal static string CurrentStageName => _stage switch
    {
        Stage.None => "无",
        Stage.Prepare => "准备",
        Stage.LegacyTechniqueMigration => "旧功法分流",
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

    internal static bool Tick(IReadOnlyList<Actor> lineageActors)
    {
        if (_stage == Stage.None || _activeYear <= 0) return true;
        if (Time.frameCount < _retryAfterFrame) return false;

        Stage sampledStage = _stage;
        long sample = MclslPerformanceProbe.Begin();
        long stageStarted = Stopwatch.GetTimestamp();
        bool sideEffectsStarted = _snapshot != null;
        try
        {
            using (MclslUnityProfiler.Sample(ProfilerName(sampledStage)))
            {
                if (_stage != Stage.Prepare && _snapshot == null)
                {
                    _snapshotBuilder ??= MclslAnnualWorldSnapshot.BeginBuild(lineageActors);
                    if (!_snapshotBuilder.Tick(MclslRuntimeWorkBudget.ScaleCount(256, 16))) return false;
                    _snapshot = _snapshotBuilder.Complete();
                    _snapshotBuilder = null;
                }
                sideEffectsStarted = true;
                bool completed = TickStage(lineageActors);
                MclslAnnualBatchState batch = MclslWorldRunRepository.Current.AnnualBatch;
                batch.WorldFailureStage = 0;
                batch.WorldFailureCount = 0;
                if (batch.WorldStage != (byte)_stage)
                {
                    batch.WorldStage = (byte)_stage;
                    MclslWorldArchiveStore.MarkDirty();
                }
                return completed;
            }
        }
        catch (Exception ex)
        {
            MclslAnnualBatchState batch = MclslWorldRunRepository.Current.AnnualBatch;
            int failures = batch.WorldFailureStage == (byte)sampledStage ? batch.WorldFailureCount + 1 : 1;
            batch.WorldFailureStage = (byte)sampledStage;
            batch.WorldFailureCount = failures;
            if (MclslAnnualResiliencePolicy.ShouldRetry(failures, sideEffectsStarted))
            {
                _snapshot = null;
                _snapshotBuilder = null;
                _retryAfterFrame = MclslAnnualResiliencePolicy.RetryAtFrame(Time.frameCount);
                MclslWorldRunRepository.RecordAnnualFailure(_activeYear, "世界", string.Empty,
                    sampledStage.ToString(), failures, "待重试", ex.Message);
                return false;
            }
            MclslWorldRunRepository.RecordAnnualFailure(_activeYear, "世界", string.Empty,
                sampledStage.ToString(), failures, sideEffectsStarted ? "部分失败" : "跳过", ex.Message);
            if (!sideEffectsStarted)
            {
                _snapshotBuilder = null;
                _snapshot = MclslAnnualWorldSnapshot.BeginBuild(Array.Empty<Actor>()).Complete();
            }
            SkipFailedStage(sampledStage);
            batch.WorldStage = (byte)_stage;
            batch.WorldFailureStage = 0;
            batch.WorldFailureCount = 0;
            MclslWorldArchiveStore.MarkDirty();
            return false;
        }
        finally
        {
            MclslPerformanceProbe.End(ProbeName(sampledStage), sample);
            double elapsedMs = (Stopwatch.GetTimestamp() - stageStarted) * 1000d / Stopwatch.Frequency;
            double budgetMs = MclslRuntimeWorkBudget.ScaleMilliseconds(0.65d, 0.20d);
            if (elapsedMs > budgetMs)
                MclslDiagnostics.Error("annual-world-overbudget:" + sampledStage,
                    "世界年度单步超预算：年=" + _activeYear + " 阶段=" + sampledStage
                    + " 耗时=" + elapsedMs.ToString("F2") + "ms 预算=" + budgetMs.ToString("F2") + "ms");
        }
    }

    private static void SkipFailedStage(Stage failed)
    {
        MclslAnnualBatchState batch = MclslWorldRunRepository.Current.AnnualBatch;
        List<MclslAnnualClaimRecord> pendingClaims = failed switch
        {
            Stage.Cave => batch.CaveClaims,
            Stage.WorldChange => batch.ChangeClaims,
            Stage.Adventure => batch.AdventureCandidates,
            _ => null
        };
        if (pendingClaims != null)
        {
            HashSet<string> recordedTargets = new(StringComparer.Ordinal);
            foreach (MclslAnnualClaimRecord claim in pendingClaims)
            {
                if (claim.Year != _activeYear || !recordedTargets.Add(claim.TargetId)) continue;
                MclslWorldRunRepository.RecordAnnualFailure(_activeYear, "世界", claim.TargetId,
                    failed.ToString(), 1, "未结算", "年度阶段隔离，申请未执行");
            }
        }
        switch (failed)
        {
            case Stage.Cave:
                batch.CaveClaims.RemoveAll(x => x.Year == _activeYear);
                break;
            case Stage.WorldChange:
                batch.ChangeClaims.RemoveAll(x => x.Year == _activeYear);
                break;
            case Stage.Adventure:
                batch.AdventureCandidates.RemoveAll(x => x.Year == _activeYear);
                break;
            case Stage.TechniqueLineage:
                foreach (string targetId in batch.TechniqueLineagePendingIds)
                    MclslWorldRunRepository.RecordAnnualFailure(_activeYear, "世界", targetId,
                        "TechniqueLineage", 1, "未结算", "年度阶段隔离，传承记录未执行");
                batch.TechniqueLineagePendingIds.Clear();
                batch.TechniqueLineagePendingInitialized = false;
                MclslTechniqueLineageSystem.ClearRuntime();
                break;
            case Stage.SectLifecycle:
                batch.SectLifecycleCursor = 0;
                batch.SectLifecycleEmitted = 0;
                break;
        }
        _stage = failed switch
        {
            Stage.Prepare or Stage.LegacyTechniqueMigration => Stage.EraCycle,
            Stage.EraCycle => Stage.WorldCalamity,
            Stage.WorldCalamity => Stage.Cave,
            Stage.Cave => Stage.WorldChange,
            Stage.WorldChange => Stage.WorldSoul,
            Stage.WorldSoul => Stage.InverseTruth,
            Stage.InverseTruth => Stage.Adventure,
            Stage.Adventure => Stage.TechniqueLineage,
            Stage.TechniqueLineage => Stage.SectLifecycle,
            Stage.SectLifecycle => Stage.FactionMission,
            Stage.FactionMission => Stage.FactionPressure,
            _ => Stage.Complete
        };
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
                if (!_newLawCultivationAvailable) { SkipFailedStage(Stage.Cave); return false; }
                if (!MclslWorldCaveSystem.TickResolveAnnual(_activeYear)) return false;
                _stage = Stage.WorldChange;
                return false;
            case Stage.WorldChange:
                if (!_newLawCultivationAvailable) { SkipFailedStage(Stage.WorldChange); return false; }
                if (!MclslWorldChangeSystem.TickResolveAnnual(_activeYear)) return false;
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
                if (!MclslAdventureSystem.TickResolveAnnual(_activeYear)) return false;
                _stage = Stage.TechniqueLineage;
                return false;
            case Stage.TechniqueLineage:
                if (!MclslTechniqueLineageSystem.TickResolveAnnual(_activeYear,
                    _snapshot?.LineageActors ?? lineageActors)) return false;
                _stage = Stage.SectLifecycle;
                return false;
            case Stage.SectLifecycle:
                if (!MclslSectLifecycleSystem.TickResolveAnnual(_activeYear)) return false;
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
        Stage.LegacyTechniqueMigration => "年度世界.旧功法分流",
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
        Stage.Complete => "年度世界.完成",
        _ => "年度世界.无"
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
        _newLawEraActive = false;
        _newLawCultivationAvailable = false;
        _snapshot = null;
        _snapshotBuilder = null;
        _retryAfterFrame = 0;
        MclslTechniqueLineageSystem.ClearRuntime();
        MclslTechniqueOccupationSystem.CancelLegacyTechniqueMigration();
    }

    private static void CompleteActiveYear()
    {
        Clear();
    }
}
