using System;
using System.Collections.Generic;
using MySimulatedLongevityRoad.Core;
using MySimulatedLongevityRoad.Data;
using MySimulatedLongevityRoad.Traits;

namespace MySimulatedLongevityRoad.Systems;

internal static class MclslBeastPopulationIndex
{
    private struct Entry { internal int Region; }
    private static readonly Dictionary<long, Entry> Automatic = new();
    private static readonly Dictionary<int, int> Regional = new();
    private static int _unlocked = 1;
    private static void Adjust(Dictionary<int, int> counts, int region, int delta)
    {
        int count = counts.GetValueOrDefault(region) + delta;
        if (count == 0) counts.Remove(region); else counts[region] = count;
    }
    internal static void Observe(Actor actor)
    {
        if (actor?.data == null) return;
        long id = actor.data.id;
        if (!MclslActorAccessor.Alive(actor)) { Forget(id); return; }
        if (!MclslBeastActorRegistration.IsOrdinary(actor))
        {
            Forget(id);
            if (MclslEligibility.IsCivilizedActor(actor)
                && !actor.asset.id.StartsWith("mclsl_named_", StringComparison.Ordinal)
                && !MclslImmortalActorRegistration.IsImmortal(actor))
                _unlocked = Math.Max(_unlocked, MclslRealmIds.Index(MclslActorAccessor.Realm(actor)) + 1);
            return;
        }
        string source = MclslActorAccessor.GetString(actor, MclslBeastWorld.SourceKey);
        if (source != "natural" && !source.StartsWith("wave:", StringComparison.Ordinal)) { Forget(id); return; }
        int region = MclslBeastSurfaceIndex.Region(actor.current_tile);
        if (Automatic.TryGetValue(id, out var before) && before.Region == region) return;
        Forget(id);
        Automatic[id] = new Entry { Region = region };
        Adjust(Regional, region, 1);
    }
    internal static void Moved(Actor actor)
    {
        // Native findCurrentTile also runs inside Parallel.ForEach. Never read or
        // mutate the main-thread dictionaries from that callback.
        if (!MclslRuntimeChanges.IsMainThread)
        {
            if (actor?.asset?.id?.StartsWith("mclsl_beast_", StringComparison.Ordinal) == true)
                MclslRuntimeChanges.Publish(actor, MclslActorChange.Position);
            return;
        }
        if (actor?.data == null || !Automatic.TryGetValue(actor.data.id, out var entry)) return;
        int region = MclslBeastSurfaceIndex.Region(actor.current_tile);
        if (region == entry.Region) return;
        Adjust(Regional, entry.Region, -1); Adjust(Regional, region, 1);
        entry.Region = region; Automatic[actor.data.id] = entry;
    }
    internal static int Count(int region)
        => region < 0 ? Automatic.Count : Regional.GetValueOrDefault(region);
    internal static void PublishProgress() => MclslBeastWorld.State.Unlocked = Math.Max(MclslBeastWorld.State.Unlocked, _unlocked);
    internal static void Forget(long id)
    {
        if (!Automatic.TryGetValue(id, out var entry)) return;
        Automatic.Remove(id); Adjust(Regional, entry.Region, -1);
    }
    // 惰性自校验：分摊式抽样，避免每帧全量拷贝字典键。
    // 只有走完一整轮（游标到末尾）时才重建一次快照，其余时间每帧只做 budget 次查询。
    private static readonly List<long> ValidateQueue = new();
    private static int _validateCursor;
    internal static void ValidateSome(int budget)
    {
        if (budget <= 0 || Automatic.Count == 0) return;
        if (_validateCursor >= ValidateQueue.Count)
        {
            ValidateQueue.Clear();
            ValidateQueue.AddRange(Automatic.Keys);
            _validateCursor = 0;
            if (ValidateQueue.Count == 0) return;
        }
        int checks = Math.Min(budget, ValidateQueue.Count - _validateCursor);
        for (int i = 0; i < checks; i++)
        {
            long id = ValidateQueue[_validateCursor++];
            if (!MclslActorRegistry.TryPeekAlive(id, out _)) Forget(id);
        }
    }
    internal static void Clear() { Automatic.Clear(); Regional.Clear(); _unlocked = 1; ValidateQueue.Clear(); _validateCursor = 0; }
}
