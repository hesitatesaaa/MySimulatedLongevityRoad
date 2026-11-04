using System;
using System.Diagnostics;
using System.Threading;
using MySimulatedLongevityRoad.Data;
using MySimulatedLongevityRoad.Systems;
using MySimulatedLongevityRoad.UI;

namespace MySimulatedLongevityRoad.Core;

[Flags]
internal enum MclslActorChange : ushort
{
    None = 0, Data = 1, Membership = 2, Inventory = 4, Traits = 8, Native = 16,
    Presentation = 32, NativeMetadata = 64, Targets = 128, Position = 256, All = 127
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
        Deferred.Publish(id, actor, (int)MclslActorChange.Native, repairStats: true, MclslWorldContext.Generation);
    }
    internal static long DataRevision(long id) => MclslActorRegistry.ResolveEntry(id, out var entry) ? entry.DataRevision : 0;
    internal readonly struct NativeStamp
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
            if (_age >= 16 && before._age < 16) flags |= MclslActorChange.NativeMetadata;
            if (_name != before._name || _kingdom != before._kingdom || _kingdomName != before._kingdomName
                || _asset != before._asset || _city != before._city || _hasTile != before._hasTile)
                flags |= MclslActorChange.NativeMetadata;
            return flags;
        }
    }
    internal static void PublishTargets(Actor actor)
    {
        long id = MclslActorAccessor.Id(actor);
        if (MclslActorRegistry.ResolveEntry(id, out var entry) && ReferenceEquals(entry.Actor, actor)
            && entry.ObserveTargets()) Publish(actor, MclslActorChange.Targets);
    }
    internal static long Revision { get; private set; }
    internal static int Count => Pending.Count + Deferred.Count;
    internal static void Publish(Actor actor, MclslActorChange flags = MclslActorChange.Data)
    {
        long id = MclslActorAccessor.Id(actor);
        if (!IsMainThread)
        {
            Deferred.Publish(id, actor, (int)flags, repairStats: false, MclslWorldContext.Generation);
            return;
        }
        if (id <= 0 || !MclslActorRegistry.ResolveEntry(id, out var entry) || !ReferenceEquals(entry.Actor, actor)) return;
        if ((flags & MclslActorChange.Position) != 0)
        {
            MclslBeastPopulationIndex.Moved(actor);
            flags &= ~MclslActorChange.Position;
            if (flags == MclslActorChange.None) return;
        }
        if ((flags & MclslActorChange.Targets) != 0)
        {
            MclslActorRegistry.RefreshTargets(actor);
            MySimulatedLongevityRoad.Systems.Combat.MclslCareerCombat.Wake(actor);
            MclslBeastTalentSystem.Wake(actor);
            flags &= ~MclslActorChange.Targets;
            if (flags == MclslActorChange.None) return;
        }
        if ((flags & (MclslActorChange.Membership | MclslActorChange.Traits | MclslActorChange.Native | MclslActorChange.NativeMetadata)) != 0)
            MclslActorRegistry.RefreshTargets(actor, forceEligibility: flags != MclslActorChange.Native);
        if ((flags & MclslActorChange.Traits) != 0) MclslActorRegistry.RefreshTraits(actor);
        if (flags == MclslActorChange.Native)
        {
            NativeStamp next = new(actor);
            flags = entry.HasNativeStamp ? next.ChangesSince(entry.NativeStamp)
                : MclslActorChange.Native | MclslActorChange.NativeMetadata | MclslActorChange.Presentation;
            entry.NativeStamp = next; entry.HasNativeStamp = true;
            if ((flags & MclslActorChange.NativeMetadata) != 0)
            { MclslActorRegistry.RefreshLocation(actor); MclslActorRegistry.RefreshTargets(actor, forceEligibility: true); }
            if ((flags & (MclslActorChange.Presentation | MclslActorChange.NativeMetadata)) != 0)
                MclslActorProjectionIndex.RefreshUninitiated(actor);
            if (flags == MclslActorChange.None) return;
            if (flags == MclslActorChange.Presentation && !MclslActorInfoPanel.HasOpenActor(actor)) return;
        }
        if ((flags & (MclslActorChange.Membership | MclslActorChange.Traits)) != 0)
            MclslActorProjectionIndex.RefreshUninitiated(actor);
        if ((flags & (MclslActorChange.Membership | MclslActorChange.Traits | MclslActorChange.NativeMetadata)) != 0)
            Systems.Visual.MclslRealmHaloVisualSystem.RefreshQualification(actor);
        Pending.Publish(id, (int)flags);
        Revision++;
        if ((flags & (MclslActorChange.Data | MclslActorChange.Membership | MclslActorChange.Traits | MclslActorChange.Inventory | MclslActorChange.NativeMetadata)) != 0) entry.DataRevision = Revision;
    }
    internal static void OnWrite(Actor actor, string key)
    {
        if (key == MclslActorDataKeys.TaishangProgress && IsMainThread)
            Systems.Visual.MclslRealmHaloVisualSystem.ObserveTaishangProgress(actor);
        if (key == MclslActorDataKeys.LearnedSpells) MclslSpellSystem.ObserveSelfCaster(actor);
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
        if (key == MclslActorDataKeys.TaishangProgress && !IsMainThread)
            flags |= MclslActorChange.Membership;
        if (key == MclslActorDataKeys.Realm || key == MclslActorDataKeys.CultivationSystem
            || key == MclslActorDataKeys.Profession || key == MclslActorDataKeys.SpiritualRootPrimary
            || key == MclslActorDataKeys.SpiritualRootAttributes || key == MclslActorDataKeys.SpiritualRootCount
            || key == MclslActorDataKeys.Aptitude || key == MclslActorDataKeys.TechniqueId
            || key == MclslActorDataKeys.TechniqueName || key == MclslActorDataKeys.TechniqueMaxRealm)
            flags |= MclslActorChange.Membership;
        if (key == MclslActorDataKeys.QiankunBag) flags |= MclslActorChange.Inventory;
        Publish(actor, flags);
    }
    internal static void Remove(long id)
    {
        Systems.Visual.MclslRealmHaloVisualSystem.Forget(id);
        Deferred.Cancel(id);
        Pending.Cancel(id);
        MclslActorProjectionIndex.Remove(id);
        MclslRankSnapshotSource.Remove(id);
        Revision++;
    }
    internal static void Drain()
    {
        if (Count == 0 || MclslFrameDeadline.Expired) return;
        long sample = MclslPerformanceProbe.Begin();
        long deadline = Stopwatch.GetTimestamp() + Stopwatch.Frequency / 2000;
        int drained = 0;
        try
        {
            while (!MclslFrameDeadline.Expired && Stopwatch.GetTimestamp() < deadline)
            {
                bool worked = false;
                if (Deferred.TryTake(out long nativeId, out Actor source, out int nativeFlags, out bool repairStats, out long generation))
                {
                    worked = true;
                    drained++;
                    if (generation == MclslWorldContext.Generation
                        && MclslActorRegistry.Resolve(nativeId, out Actor actor) && ReferenceEquals(actor, source)
                        && MclslActorAccessor.Alive(actor))
                    {
                        try
                        {
                            if (repairStats)
                            {
                                long repairSample = MclslPerformanceProbe.Begin();
                                try
                                {
                                    MclslArtifactSystem.ApplyNativeStatCaps(actor);
                                    MclslBeastWorld.ApplyRankStats(actor);
                                    MclslLongevityRules.ApplyRuntimeLifespan(actor);
                                }
                                finally { MclslPerformanceProbe.End("变化分发.属性修复", repairSample, actorId: nativeId, flags: nativeFlags); }
                            }
                            long publishSample = MclslPerformanceProbe.Begin();
                            try { Publish(actor, (MclslActorChange)nativeFlags); }
                            finally { MclslPerformanceProbe.End("变化分发.原生交接", publishSample, actorId: nativeId, flags: nativeFlags); }
                        }
                        catch (Exception ex) { MclslDiagnostics.Error("runtime-worker-change", "人物并行变化交接失败: " + ex.Message); }
                    }
                }
                // Alternate native handoff and downstream notifications so neither
                // stream can starve the other. The shared deadline bounds work;
                // a fixed 16/32-actor cap no longer leaves usable time idle.
                if (Pending.TryTake(out long id, out int flags))
                {
                    worked = true;
                    drained++;
                    if (!MclslActorRegistry.Resolve(id, out Actor actor) || !MclslActorAccessor.Alive(actor)) Remove(id);
                    else
                    {
                        try
                        {
                            if ((flags & (int)(MclslActorChange.Membership | MclslActorChange.Traits | MclslActorChange.NativeMetadata)) != 0)
                            {
                                long relationsSample = MclslPerformanceProbe.Begin();
                                try
                                {
                                    Patches.MclslXuanhuangDirectionPatch.ObserveActor(actor);
                                    MclslActorRelations.Refresh(actor);
                                    MclslBeastPopulationIndex.Observe(actor);
                                    MclslFamilySystem.Observe(actor);
                                }
                                finally { MclslPerformanceProbe.End("变化分发.成员关系", relationsSample, actorId: id, flags: flags); }
                            }
                            if ((flags & (int)(MclslActorChange.Membership | MclslActorChange.Traits)) != 0)
                            {
                                long candidateSample = MclslPerformanceProbe.Begin();
                                try { MclslCultivatorCandidateIndex.Observe(actor); }
                                finally { MclslPerformanceProbe.End("变化分发.候选", candidateSample, actorId: id, flags: flags); }
                            }
                            if ((flags & (int)(MclslActorChange.Data | MclslActorChange.Membership | MclslActorChange.Traits | MclslActorChange.NativeMetadata)) != 0)
                            {
                                long projectionSample = MclslPerformanceProbe.Begin();
                                try { MclslActorProjectionIndex.Update(actor); }
                                finally { MclslPerformanceProbe.End("变化分发.投影", projectionSample, actorId: id, flags: flags); }
                            }
                            if ((flags & (int)(MclslActorChange.Data | MclslActorChange.Membership | MclslActorChange.Traits | MclslActorChange.Native | MclslActorChange.NativeMetadata)) != 0)
                            {
                                long rankSample = MclslPerformanceProbe.Begin();
                                try
                                {
                                    MclslRankSnapshotSource.QueueUpdate(actor,
                                        (flags & (int)(MclslActorChange.Membership | MclslActorChange.Traits | MclslActorChange.NativeMetadata)) != 0);
                                }
                                finally { MclslPerformanceProbe.End("变化分发.排名", rankSample, actorId: id, flags: flags); }
                            }
                            // Without an open native window there is no per-actor UI
                            // work and no window list walk to perform.
                            if (MclslActorInfoPanel.HasAnyOpenWindow)
                            {
                                long uiSample = MclslPerformanceProbe.Begin();
                                try { MclslActorInfoPanel.RefreshOpenForActor(actor); }
                                finally { MclslPerformanceProbe.End("变化分发.UI", uiSample, actorId: id, flags: flags); }
                            }
                        }
                        catch (Exception ex) { MclslDiagnostics.Error("runtime-change", "人物变化分发失败: " + ex.Message); }
                    }
                }
                if (!worked) break;
            }
        }
        finally { MclslPerformanceProbe.RecordChangeQueueDrained(drained); MclslPerformanceProbe.End("数据变化分发", sample); }
    }
    internal static void Clear()
    {
        Deferred.Clear();
        Pending.Clear();
        MclslActorProjectionIndex.Clear();
        MclslRankSnapshotSource.Clear();
        Revision++;
    }
}
