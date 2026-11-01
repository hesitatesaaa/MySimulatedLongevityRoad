using System;
using System.Collections.Generic;
using System.Linq;
using MySimulatedLongevityRoad.Core;
using MySimulatedLongevityRoad.Data;

namespace MySimulatedLongevityRoad.Systems;

internal static partial class MclslFamilySystem
{
    internal static long Revision { get; private set; }
    private static bool _populationComplete;
    private static readonly Queue<long> PendingMembers = new();
    private static readonly HashSet<long> PendingMemberIds = new();
    private static readonly Queue<MclslFamilyRecord> ChangedFamilies = new();
    private static readonly HashSet<string> ChangedFamilyIds = new(StringComparer.Ordinal);
    private static readonly HashSet<string> StateChangedFamilyIds = new(StringComparer.Ordinal);
    private static readonly HashSet<string> ActiveFamilies = new(StringComparer.Ordinal);
    internal static bool HasWork => PendingMembers.Count != 0 || ChangedFamilies.Count != 0;
    internal static int ActiveFamilyCount => ActiveFamilies.Count;
    internal static long ReconciledMembers { get; private set; }
    internal static long FullPopulationPasses => 0;
    private static void NotifyChanged() { Revision++; MclslWorldArchiveStore.MarkDirty(); }
    private static void QueueFamily(MclslFamilyRecord family, bool stateChanged = true)
    {
        if (family == null) return;
        if (stateChanged) StateChangedFamilyIds.Add(family.Id);
        if (ChangedFamilyIds.Add(family.Id)) ChangedFamilies.Enqueue(family);
    }
    private static void QueueMember(long id)
    { if (id > 0 && PendingMemberIds.Add(id)) PendingMembers.Enqueue(id); }
    private static void QueueRelatives(MclslFamilyRecord family, long actorId)
    {
        if (family.NativeFamilyId > 0)
            foreach (long id in MclslActorRelations.Family(family.NativeFamilyId)) QueueMember(id);
        foreach (long id in MclslActorRelations.Descendants(actorId)) QueueMember(id);
    }
    internal static void InitializeAfterLoad() => EnsureIndex();
    private static void ResetReconciliation()
    {
        _populationComplete = false;
        PendingMembers.Clear(); PendingMemberIds.Clear();
        ChangedFamilies.Clear(); ChangedFamilyIds.Clear(); StateChangedFamilyIds.Clear(); ActiveFamilies.Clear();
        ReconciledMembers = 0; Revision++;
    }
    internal static void TickReconciliation()
    {
        if (MclslWorldArchiveStore.WriteLocked || !MclslWorldBootstrapLane.Completed) return;
        MclslWorldRunState run = EnsureIndex();
        _populationComplete = true;
        for (int work = 0; work < 64 && !MclslFrameDeadline.Expired && PendingMembers.Count > 0; work++)
        {
            long id = PendingMembers.Dequeue(); PendingMemberIds.Remove(id); ReconciledMembers++;
            if (MclslActorRegistry.ResolveKnownOrWorld(id, out Actor actor) && actor?.data != null)
            {
                if (MclslActorAccessor.Alive(actor)) Observe(actor);
                else OnDeath(actor);
            }
            else if (ByActor.TryGetValue(id, out var family))
            {
                var member = Member(family, id);
                if (member != null && member.Alive) { member.Alive = false; member.Missing = true; InvalidateMemberView(family); NotifyChanged(); QueueFamily(family); }
            }
        }
        if (PendingMembers.Count > 0) return;
        for (int work = 0; work < 16 && !MclslFrameDeadline.Expired && ChangedFamilies.Count > 0; work++)
        {
            var family = ChangedFamilies.Dequeue(); ChangedFamilyIds.Remove(family.Id);
            bool stateChanged = StateChangedFamilyIds.Remove(family.Id);
            if (!ById.ContainsKey(family.Id)) continue;
            RefreshQueuedTechniqueDetails(family);
            if (!stateChanged) continue;
            EnsureHead(family);
            long focusBefore = family.FocusActorId;
            bool manualBefore = family.FocusIsManual;
            EnsureFocus(family);
            if (family.FocusActorId != focusBefore || family.FocusIsManual != manualBefore) NotifyChanged();
            bool active = false, hasLiving = false;
            foreach (var member in family.Members)
            {
                if (member == null) continue;
                hasLiving |= member.Alive || member.Missing;
                if (member.Alive && !member.Missing && member.IsCultivator
                    && MclslActorRegistry.Resolve(member.ActorId, out var actor) && MclslEligibility.CanCultivate(actor)) active = true;
            }
            if (active) ActiveFamilies.Add(family.Id);
            else
            {
                ActiveFamilies.Remove(family.Id);
                if (family.CityInfluence.Count > 0) { family.CityInfluence.Clear(); InvalidateCityInfluenceViews(); NotifyChanged(); }
            }
            // Records are created with their founder, so an empty member list
            // means the last member left: settle the treasury as extinct.
            bool extinct = !hasLiving;
            if (family.Extinct != extinct) { family.Extinct = extinct; NotifyChanged(); }
            if (extinct) TryLiquidateEstate(run, family);
        }
    }
}
