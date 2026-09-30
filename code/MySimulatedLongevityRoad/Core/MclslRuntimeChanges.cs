using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Threading;
using MySimulatedLongevityRoad.Data;
using MySimulatedLongevityRoad.Systems;
using MySimulatedLongevityRoad.UI;

namespace MySimulatedLongevityRoad.Core;

[Flags]
internal enum MclslActorChange : byte
{
    None = 0, Data = 1, Membership = 2, Inventory = 4, Traits = 8, Native = 16,
    Presentation = 32, NativeMetadata = 64, All = 127
}

/// <summary>Single bounded handoff from gameplay mutations to indexes and visible presentation.</summary>
internal static class MclslRuntimeChanges
{
    private static readonly MclslChangeQueue Pending = new();
    private static readonly MclslActorThreadHandoff<Actor> Deferred = new();
    private static int _mainThreadId;
    internal static void BindMainThread() => Volatile.Write(ref _mainThreadId, Thread.CurrentThread.ManagedThreadId);
    internal static bool IsMainThread => Thread.CurrentThread.ManagedThreadId == Volatile.Read(ref _mainThreadId);
    internal static void PublishNativeStats(Actor actor)
    {
        long id = MclslActorAccessor.Id(actor);
        Deferred.Publish(id, actor, (int)MclslActorChange.Native, repairStats: true);
    }
    private static readonly Dictionary<long, long> DataRevisions = new();
    internal static long DataRevision(long id) => DataRevisions.TryGetValue(id, out long value) ? value : 0;
    private static readonly Dictionary<long, NativeStamp> NativeStamps = new();
    private readonly struct NativeStamp
    {
        private readonly string _name, _kingdomName, _asset;
        private readonly long _kingdom, _city;
        private readonly float _damage, _health;
        private readonly int _age;
        private readonly bool _hasTile;
        internal NativeStamp(Actor actor)
        {
            _name = actor.data.name; _kingdomName = actor.kingdom?.data?.name;
            _asset = actor.asset?.id; _kingdom = actor.kingdom?.data?.id ?? 0;
            _city = actor.city?.data?.id ?? 0; _hasTile = actor.current_tile != null;
            _damage = actor.stats == null ? 0 : actor.stats["damage"];
            _health = actor.stats == null ? 0 : actor.stats["health"];
            _age = (int)actor.getAge();
        }
        internal MclslActorChange ChangesSince(NativeStamp before)
        {
            MclslActorChange flags = MclslActorChange.None;
            if (!_damage.Equals(before._damage) || !_health.Equals(before._health)) flags |= MclslActorChange.Native;
            if (_age != before._age) flags |= MclslActorChange.Presentation;
            if (_name != before._name || _kingdom != before._kingdom || _kingdomName != before._kingdomName
                || _asset != before._asset || _city != before._city || _hasTile != before._hasTile)
                flags |= MclslActorChange.NativeMetadata;
            return flags;
        }
    }
    internal static long Revision { get; private set; }
    internal static int Count => Pending.Count + Deferred.Count;
    internal static void Publish(Actor actor, MclslActorChange flags = MclslActorChange.Data)
    {
        long id = MclslActorAccessor.Id(actor);
        if (!IsMainThread)
        {
            Deferred.Publish(id, actor, (int)flags, repairStats: false);
            return;
        }
        if (id <= 0 || !MclslActorRegistry.Resolve(id, out _)) return;
        if (flags != MclslActorChange.Presentation) MclslActorRegistry.RefreshTargets(actor, forceEligibility: flags != MclslActorChange.Native);
        if ((flags & MclslActorChange.Traits) != 0) MclslActorRegistry.RefreshTraits(actor);
        if (flags == MclslActorChange.Native)
        {
            NativeStamp next = new(actor);
            flags = NativeStamps.TryGetValue(id, out NativeStamp before) ? next.ChangesSince(before)
                : MclslActorChange.Native | MclslActorChange.NativeMetadata | MclslActorChange.Presentation;
            NativeStamps[id] = next;
            if ((flags & MclslActorChange.NativeMetadata) != 0)
            { MclslActorRegistry.RefreshLocation(actor); MclslActorRegistry.RefreshTargets(actor, forceEligibility: true); }
            if ((flags & (MclslActorChange.Presentation | MclslActorChange.NativeMetadata)) != 0)
                MclslActorProjectionIndex.RefreshUninitiated(actor);
            if (flags == MclslActorChange.None) return;
            if (flags == MclslActorChange.Presentation && !MclslActorInfoPanel.HasOpenActor(actor)) return;
        }
        if ((flags & (MclslActorChange.Membership | MclslActorChange.Traits)) != 0)
            MclslActorProjectionIndex.RefreshUninitiated(actor);
        Pending.Publish(id, (int)flags);
        Revision++;
        if ((flags & (MclslActorChange.Data | MclslActorChange.Membership | MclslActorChange.Traits | MclslActorChange.Inventory | MclslActorChange.NativeMetadata)) != 0) DataRevisions[id] = Revision;
    }
    internal static void OnWrite(Actor actor, string key)
    {
        if (key == MclslActorDataKeys.HuanzhenIdentity) MclslActorRegistry.RefreshIdentity(actor);
        if (key == MclslActorDataKeys.NascentCaveId || key == MclslActorDataKeys.DivineChangeId)
            MclslActorRegistry.RefreshResourceReferences(actor);
        if (key == MclslActorDataKeys.QiankunBagBackup || key == MclslActorDataKeys.QiankunBagCorrupt) return;
        if (key == MclslActorDataKeys.AnnualStep || key == MclslActorDataKeys.AnnualActiveYear
            || key == MclslActorDataKeys.AnnualLatestRequestedYear || key == MclslActorDataKeys.AnnualStage
            || key == MclslActorDataKeys.AnnualLastCompletedYear || key == MclslActorDataKeys.LastCultivationYear
            || key == MclslActorDataKeys.ManaLastWorldTime || key == MclslActorDataKeys.ManaInitialized) return;
        if (key == MclslActorDataKeys.ManaCurrent)
        {
            if (MclslActorInfoPanel.HasOpenActor(actor)) Publish(actor, MclslActorChange.Presentation);
            return;
        }
        if (key == MclslActorDataKeys.Realm || key == MclslActorDataKeys.CultivationSystem)
            MclslAncientMentorshipSystem.OnCultivationChanged(actor);
        if (key == MclslActorDataKeys.ImmortalFate || key == MclslActorDataKeys.MindState)
            MclslActorProjectionIndex.RefreshUninitiated(actor);
        MclslActorChange flags = MclslActorChange.Data;
        if (key == MclslActorDataKeys.Realm || key == MclslActorDataKeys.CultivationSystem
            || key == MclslActorDataKeys.Profession || key == MclslActorDataKeys.SpiritualRootPrimary
            || key == MclslActorDataKeys.SpiritualRootAttributes || key == MclslActorDataKeys.SpiritualRootCount
            || key == MclslActorDataKeys.Aptitude || key == MclslActorDataKeys.TechniqueId)
            flags |= MclslActorChange.Membership;
        if (key == MclslActorDataKeys.QiankunBag) flags |= MclslActorChange.Inventory;
        Publish(actor, flags);
    }
    internal static void Remove(long id)
    {
        Deferred.Cancel(id);
        Pending.Cancel(id);
        NativeStamps.Remove(id);
        DataRevisions.Remove(id);
        MclslActorProjectionIndex.Remove(id);
        MclslRankSnapshotSource.Remove(id);
        Revision++;
    }
    internal static void Drain(int limit = 64)
    {
        long sample = MclslPerformanceProbe.Begin();
        long deadline = Stopwatch.GetTimestamp() + Stopwatch.Frequency / 2000;
        try
        {
            for (int i = 0; i < limit && !MclslFrameDeadline.Expired && Stopwatch.GetTimestamp() < deadline; i++)
            {
                if (!Deferred.TryTake(out long id, out Actor source, out int flags, out bool repairStats)) break;
                if (!MclslActorRegistry.Resolve(id, out Actor actor) || !ReferenceEquals(actor, source)
                    || !MclslActorAccessor.Alive(actor)) continue;
                try
                {
                    if (repairStats)
                    {
                        MclslArtifactSystem.ApplyNativeStatCaps(actor);
                        MclslLongevityRules.ApplyRuntimeLifespan(actor);
                    }
                    Publish(actor, (MclslActorChange)flags);
                }
                catch (Exception ex) { MclslDiagnostics.Error("runtime-worker-change", "人物并行变化交接失败: " + ex.Message); }
            }
            for (int i = 0; i < limit && !MclslFrameDeadline.Expired && Stopwatch.GetTimestamp() < deadline; i++)
            {
                if (!Pending.TryTake(out long id, out int flags)) break;
                if (!MclslActorRegistry.Resolve(id, out Actor actor) || !MclslActorAccessor.Alive(actor))
                { Remove(id); continue; }
                try
                {
                    if ((flags & (int)(MclslActorChange.Membership | MclslActorChange.Traits)) != 0)
                        MclslCultivatorCandidateIndex.Observe(actor);
                    if ((flags & (int)(MclslActorChange.Data | MclslActorChange.Membership | MclslActorChange.Traits | MclslActorChange.NativeMetadata)) != 0)
                        MclslActorProjectionIndex.Update(actor);
                    if ((flags & (int)(MclslActorChange.Data | MclslActorChange.Membership | MclslActorChange.Traits | MclslActorChange.Native | MclslActorChange.NativeMetadata)) != 0)
                        MclslRankSnapshotSource.UpdateActor(actor,
                            (flags & (int)(MclslActorChange.Membership | MclslActorChange.Traits | MclslActorChange.NativeMetadata)) != 0);
                    MclslActorInfoPanel.RefreshOpenForActor(actor);
                }
                catch (Exception ex) { MclslDiagnostics.Error("runtime-change", "人物变化分发失败: " + ex.Message); }
            }
        }
        finally { MclslPerformanceProbe.End("数据变化分发", sample); }
    }
    internal static void Clear()
    {
        Deferred.Clear();
        Pending.Clear();
        NativeStamps.Clear();
        DataRevisions.Clear();
        MclslActorProjectionIndex.Clear();
        MclslRankSnapshotSource.Clear();
        Revision++;
    }
}
