using System;
using System.Collections.Generic;
using MySimulatedLongevityRoad.Data;

namespace MySimulatedLongevityRoad.Systems;

internal static class MclslMapNodeSystem
{
    internal const string Cave = "cave";
    internal const string Ruin = "ruin";
    internal const string WorldChange = "world_change";

    private static readonly List<MclslMapNodeRecord> Snapshot = new();
    private static readonly Dictionary<long, List<MclslMapNodeRecord>> SpatialIndex = new();
    private const int SpatialBucketSize = 16;
    private static int _lastSyncYear = -1;

    internal static void OnWorldLoaded(int year)
    {
        _lastSyncYear = -1;
        Sync(year);
    }

    internal static void TickAnnual(int year)
    {
        Sync(year);
    }

    internal static void Clear()
    {
        Snapshot.Clear();
        SpatialIndex.Clear();
        _lastSyncYear = -1;
    }

    internal static IReadOnlyList<MclslMapNodeRecord> SnapshotNodes()
    {
        Snapshot.Clear();
        List<MclslMapNodeRecord> nodes = MclslWorldRunRepository.Current?.MapNodes;
        if (nodes == null) return Snapshot;
        for (int i = 0; i < nodes.Count; i++)
            if (nodes[i] != null) Snapshot.Add(nodes[i]);
        return Snapshot;
    }

    internal static MclslMapNodeRecord Find(string nodeId)
    {
        if (string.IsNullOrWhiteSpace(nodeId)) return null;
        List<MclslMapNodeRecord> nodes = MclslWorldRunRepository.Current?.MapNodes;
        if (nodes == null) return null;
        for (int i = 0; i < nodes.Count; i++)
            if (nodes[i] != null && string.Equals(nodes[i].Id, nodeId, StringComparison.Ordinal)) return nodes[i];
        return null;
    }

    internal static MclslMapNodeRecord FindBySource(string sourceType, string sourceId)
    {
        if (string.IsNullOrWhiteSpace(sourceType) || string.IsNullOrWhiteSpace(sourceId)) return null;
        List<MclslMapNodeRecord> nodes = MclslWorldRunRepository.Current?.MapNodes;
        if (nodes == null) return null;
        for (int i = 0; i < nodes.Count; i++)
        {
            MclslMapNodeRecord node = nodes[i];
            if (node != null && string.Equals(node.SourceType, sourceType, StringComparison.Ordinal)
                && string.Equals(node.SourceRecordId, sourceId, StringComparison.Ordinal)) return node;
        }
        return null;
    }

    internal static void MarkContested(string nodeId, int year)
    {
        MclslMapNodeRecord node = Find(nodeId);
        if (node == null) return;
        node.LifecycleState = "contested";
        node.LastChangedYear = year;
        MclslWorldArchiveStore.MarkDirty();
    }

    internal static void MarkControlled(string nodeId, string ownerId, string ownerName, int year)
    {
        MclslMapNodeRecord node = Find(nodeId);
        if (node == null) return;
        node.OwnerKind = string.IsNullOrWhiteSpace(ownerId) ? "none" : ownerId.StartsWith("sect:", StringComparison.Ordinal) ? "sect" : "actor";
        node.OwnerId = ownerId ?? string.Empty;
        node.OwnerNameSnapshot = ownerName ?? string.Empty;
        node.LifecycleState = "controlled";
        node.LastChangedYear = year;
        MclslWorldArchiveStore.MarkDirty();
    }

    internal static int LocalInfluence(int x, int y)
    {
        if (x < 0 || y < 0) return 0;
        int influence = 0;
        int bucketX = x / SpatialBucketSize;
        int bucketY = y / SpatialBucketSize;
        for (int dxBucket = -1; dxBucket <= 1; dxBucket++)
        for (int dyBucket = -1; dyBucket <= 1; dyBucket++)
        {
            if (!SpatialIndex.TryGetValue(BucketKey(bucketX + dxBucket, bucketY + dyBucket), out List<MclslMapNodeRecord> bucket)) continue;
            for (int i = 0; i < bucket.Count; i++)
            {
                MclslMapNodeRecord node = bucket[i];
                int dx = node.MapX - x;
                int dy = node.MapY - y;
                if (dx * dx + dy * dy > 18 * 18) continue;
                if (node.LifecycleState == "controlled") influence += 4;
                else if (node.LifecycleState == "contested") influence -= 5;
                else if (node.LifecycleState == "collapsed") influence -= 8;
            }
        }
        return Math.Clamp(influence, -20, 20);
    }

    private static void Sync(int year)
    {
        if (year <= 0 || _lastSyncYear == year) return;
        _lastSyncYear = year;
        MclslWorldRunState run = MclslWorldRunRepository.Current;
        if (run == null) return;
        run.MapNodes ??= new List<MclslMapNodeRecord>();
        bool changed = false;
        if (run.WorldCaves != null)
        {
            for (int i = 0; i < run.WorldCaves.Count; i++)
            {
                MclslWorldCaveRecord cave = run.WorldCaves[i];
                if (cave == null) continue;
                changed |= Upsert(run, "node_" + Cave + "_" + cave.Id, Cave, cave.Id, cave.Name, cave.MapX, cave.MapY,
                    cave.Quality, cave.LawTags, 0, cave.RemainingEssence, cave.BornYear, cave.LocationName, cave.NativeKingdomName,
                    cave.RemainingEssence > 0 && cave.Integrity > 20 ? "active" : "depleted");
            }
        }
        if (run.WorldChanges != null)
        {
            for (int i = 0; i < run.WorldChanges.Count; i++)
            {
                MclslWorldChangeRecord change = run.WorldChanges[i];
                if (change == null) continue;
                changed |= Upsert(run, "node_" + WorldChange + "_" + change.Id, WorldChange, change.Id, change.Name, change.MapX, change.MapY,
                    change.Quality, change.LawTags, change.Intensity, change.RemainingMarrow, change.StartYear, change.LocationName,
                    change.NativeKingdomName, change.RemainingMarrow > 0 ? "active" : "depleted");
            }
        }
        if (run.SectRuins != null)
        {
            for (int i = 0; i < run.SectRuins.Count; i++)
            {
                MclslSectRuinRecord ruin = run.SectRuins[i];
                if (ruin == null) continue;
                changed |= Upsert(run, "node_" + Ruin + "_" + ruin.Id, Ruin, ruin.Id, ruin.Name, ruin.MapX, ruin.MapY,
                    ruin.Quality, ruin.LawTags, ruin.Danger, ruin.RemainingValue, ruin.BornYear, ruin.LocationName,
                    ruin.NativeKingdomName, ruin.RemainingValue > 0 ? "active" : "depleted");
            }
        }
        RebuildSpatialIndex(run.MapNodes);
        if (changed) MclslWorldArchiveStore.MarkDirty();
    }

    private static bool Upsert(
        MclslWorldRunState run,
        string id,
        string type,
        string sourceId,
        string name,
        int x,
        int y,
        int quality,
        string lawTags,
        int danger,
        int remaining,
        int bornYear,
        string location,
        string kingdom,
        string lifecycle)
    {
        MclslMapNodeRecord node = FindIn(run.MapNodes, id);
        bool changed = false;
        if (node == null)
        {
            if (run.MapNodes.Count >= MclslWorldRunRepository.MaxMapNodeRecords) return false;
            node = new MclslMapNodeRecord { Id = id, MarkerKey = "mclsl_node_" + id };
            run.MapNodes.Add(node);
            changed = true;
        }
        changed |= Set(node.NodeType, type, value => node.NodeType = value);
        changed |= Set(node.SourceType, type, value => node.SourceType = value);
        changed |= Set(node.SourceRecordId, sourceId, value => node.SourceRecordId = value);
        changed |= Set(node.Name, name, value => node.Name = value);
        changed |= Set(node.LawTags, lawTags, value => node.LawTags = value);
        changed |= Set(node.LocationName, location, value => node.LocationName = value);
        changed |= Set(node.NativeKingdomNameSnapshot, kingdom, value => node.NativeKingdomNameSnapshot = value);
        string preservedLifecycle = node.LifecycleState == "controlled" && lifecycle != "depleted"
            ? "controlled"
            : node.LifecycleState == "contested" && lifecycle == "active"
                ? "contested"
                : lifecycle;
        changed |= Set(node.LifecycleState, preservedLifecycle, value => node.LifecycleState = value);
        if (node.MapX != x) { node.MapX = x; changed = true; }
        if (node.MapY != y) { node.MapY = y; changed = true; }
        if (node.Quality != Math.Max(1, quality)) { node.Quality = Math.Max(1, quality); changed = true; }
        if (node.Danger != Math.Max(0, danger)) { node.Danger = Math.Max(0, danger); changed = true; }
        if (node.RemainingValue != Math.Max(0, remaining)) { node.RemainingValue = Math.Max(0, remaining); changed = true; }
        if (node.BornYear <= 0 && bornYear > 0) { node.BornYear = bornYear; changed = true; }
        string visibility = x >= 0 && y >= 0 ? "discovered" : "lost";
        if (!string.Equals(node.VisibilityState, visibility, StringComparison.Ordinal)) { node.VisibilityState = visibility; changed = true; }
        return changed;
    }

    private static MclslMapNodeRecord FindIn(List<MclslMapNodeRecord> nodes, string id)
    {
        for (int i = 0; i < nodes.Count; i++)
            if (nodes[i] != null && string.Equals(nodes[i].Id, id, StringComparison.Ordinal)) return nodes[i];
        return null;
    }

    private static bool Set(string target, string value, Action<string> setter)
    {
        string next = value ?? string.Empty;
        if (string.Equals(target, next, StringComparison.Ordinal)) return false;
        setter(next);
        return true;
    }

    private static void RebuildSpatialIndex(List<MclslMapNodeRecord> nodes)
    {
        SpatialIndex.Clear();
        if (nodes == null) return;
        for (int i = 0; i < nodes.Count; i++)
        {
            MclslMapNodeRecord node = nodes[i];
            if (node == null || node.MapX < 0 || node.MapY < 0) continue;
            long key = BucketKey(node.MapX / SpatialBucketSize, node.MapY / SpatialBucketSize);
            if (!SpatialIndex.TryGetValue(key, out List<MclslMapNodeRecord> bucket))
                SpatialIndex[key] = bucket = new List<MclslMapNodeRecord>();
            bucket.Add(node);
        }
    }

    private static long BucketKey(int x, int y) => ((long)x << 32) ^ (uint)y;
}
