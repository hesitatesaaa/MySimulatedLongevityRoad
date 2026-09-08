using System;
using System.Collections.Generic;
using System.Diagnostics;
using MySimulatedLongevityRoad.Core;
using MySimulatedLongevityRoad.Data;
using MySimulatedLongevityRoad.Queries;

namespace MySimulatedLongevityRoad.Systems;

/// <summary>
/// 参考玄鉴仙族的冷路径启动重建：只在读档后拿一次世界单位快照，
/// 分帧恢复修炼特质、运行索引和年度队列；不在热路径全图扫描。
/// </summary>
internal static class MclslWorldBootstrapLane
{
    private static IReadOnlyList<Actor> _actors = Array.Empty<Actor>();
    private static int _cursor;

    internal static bool HasPending => _cursor < _actors.Count;

    internal static void ScheduleAfterLoad()
    {
        _cursor = 0;
        try
        {
            IReadOnlyList<Actor> actors = World.world?.units?.getSimpleList();
            if (actors == null || actors.Count == 0)
            {
                _actors = Array.Empty<Actor>();
            }
            else
            {
                Actor[] snapshot = new Actor[actors.Count];
                for (int i = 0; i < actors.Count; i++) snapshot[i] = actors[i];
                _actors = snapshot;
            }
        }
        catch (Exception ex)
        {
            MclslDiagnostics.Error("world-bootstrap:snapshot", "读取世界角色快照失败: " + ex.Message);
            _actors = Array.Empty<Actor>();
        }
    }

    internal static void Tick(int budget, double maxMilliseconds)
    {
        if (budget <= 0 || !HasPending) return;

        long started = Stopwatch.GetTimestamp();
        int processed = 0;
        while (_cursor < _actors.Count && processed < budget)
        {
            Actor actor = _actors[_cursor++];
            processed++;
            if (actor?.data == null || !MclslActorAccessor.Alive(actor)) continue;

            // 完整人口只恢复角色引用；修炼身份校正仅作用于有明确修炼标记的角色。
            MclslCultivatorCandidateIndex.Observe(actor);
            if (!MclslEligibility.CanCultivate(actor)) continue;

            bool hasCultivationState = MclslCultivationActorMarker.HasCultivationMarker(actor)
                || MclslActorAccessor.HasCultivationPath(actor)
                || MclslChildhoodRootSystem.ShouldTrackChildhoodCandidate(actor);
            if (!hasCultivationState) continue;
            if (!MclslCultivationWake.ReconcileCultivationIdentity(actor, ensureEntryFromGift: false)) continue;

            string realm = MclslActorAccessor.Realm(actor);
            MclslActorAccessor.ApplyDisplayName(actor, realm);
            if (!string.IsNullOrWhiteSpace(realm)
                && MclslActorAccessor.GetInt(actor, MclslActorDataKeys.RealmEnteredYear, 0) <= 0)
            {
                int enteredYear = MclslActorAccessor.GetInt(
                    actor,
                    MclslActorDataKeys.LastBreakthroughYear,
                    MclslActorAccessor.GetInt(actor, MclslActorDataKeys.CultivationStartYear, MclslRuntime.CurrentYear()));
                MclslActorAccessor.Set(actor, MclslActorDataKeys.RealmEnteredYear, Math.Max(0, enteredYear));
            }

            MclslCultivatorCandidateIndex.Observe(actor);
            MclslScheduler.WakeAnnualCultivationActor(actor);

            if ((processed & 15) == 0
                && maxMilliseconds > 0d
                && Stopwatch.GetTimestamp() - started > maxMilliseconds * Stopwatch.Frequency / 1000d)
            {
                break;
            }
        }

        if (_cursor < _actors.Count) return;
        MclslTechniqueOccupationSystem.Rebuild();
        MclslWorldActorQuery.MarkDirty();
        Clear();
    }

    internal static void Clear()
    {
        _actors = Array.Empty<Actor>();
        _cursor = 0;
    }
}
