using System;
using System.Collections.Generic;
using MySimulatedLongevityRoad.Core;
using MySimulatedLongevityRoad.Data;
using MySimulatedLongevityRoad.Systems;
using UnityEngine;

namespace MySimulatedLongevityRoad.Queries;

/// <summary>
/// 世界角色只读查询门面。人口来自完整运行时角色注册表，修士和境界统计来自专用索引；
/// 不再把“被修炼系统追踪的人数”误当成世界人口。
/// </summary>
internal static class MclslWorldActorQuery
{
    internal static int Revision { get; private set; }

    internal static int UnitCount()
    {
        // getSimpleList().Count 是原生单位容器的 O(1) 数量读取，不是全图扫描。
        // 它用于世界人口；修士人数仍严格来自修炼索引，二者不再混用。
        try
        {
            IReadOnlyList<Actor> units = World.world?.units?.getSimpleList();
            if (units != null) return Math.Max(0, units.Count);
        }
        catch (Exception ex)
        {
            MclslDiagnostics.Error("actor-query:unit-count", "读取世界人口失败: " + ex.Message);
        }

        return MclslActorRegistry.Count;
    }

    internal static void Track(Actor actor)
    {
        if (actor?.data == null) return;
        if (!MclslActorRegistry.Register(actor, out _, out bool isNew)) return;
        MclslCultivatorCandidateIndex.Observe(actor);
        if (isNew) MarkDirty();
    }

    internal static void TrackIfRelevant(Actor actor)
    {
        // 完整人口注册与修炼索引分离：凡人也进入角色注册表，但不会进入年度修炼候选集。
        Track(actor);
    }

    internal static void ClearCache() => Revision++;
    internal static void MarkDirty() => Revision++;
}
