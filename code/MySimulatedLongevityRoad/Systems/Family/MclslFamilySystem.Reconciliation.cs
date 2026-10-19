using System;
using System.Collections.Generic;
using System.Linq;
using MySimulatedLongevityRoad.Core;
using MySimulatedLongevityRoad.Data;
using UnityEngine;

namespace MySimulatedLongevityRoad.Systems;

internal static partial class MclslFamilySystem
{
    internal static long Revision { get; private set; }
    private static bool _populationComplete;
    private static int _reconcilePhase, _reconcileActor, _reconcileFamily, _reconcileMember;
    private static float _nextReconcile;
    private static IReadOnlyList<Actor> _reconcileActors = Array.Empty<Actor>();
    private static readonly Dictionary<long, MclslDeathRecord> ReconcileDeaths = new();
    private static readonly Queue<Actor> PendingDeaths = new();
    private static readonly HashSet<long> PendingDeathIds = new();

    private static void NotifyChanged()
    {
        Revision++;
        MclslWorldArchiveStore.MarkDirty();
    }

    private static void ResetReconciliation()
    {
        _populationComplete = false;
        _reconcilePhase = _reconcileActor = _reconcileFamily = _reconcileMember = 0;
        _nextReconcile = 0;
        _reconcileActors = Array.Empty<Actor>();
        ReconcileDeaths.Clear();
        PendingDeaths.Clear(); PendingDeathIds.Clear();
        Revision++;
    }

    // Use the existing population index, only after load bootstrap finishes.
    // A second actor pass picks up mortals encountered before their cultivating founder.
    internal static void TickReconciliation()
    {
        if (MclslWorldArchiveStore.WriteLocked || !MclslWorldBootstrapLane.Completed) return;
        MclslWorldRunState run = EnsureIndex();
        for (int retry = 0, limit = Math.Min(2, PendingDeaths.Count); retry < limit; retry++)
        {
            Actor dead = PendingDeaths.Dequeue();
            PendingDeathIds.Remove(MclslActorAccessor.Id(dead));
            if (dead?.data != null) OnDeath(dead);
        }
        if (run == null || string.IsNullOrWhiteSpace(run.RunId) || Time.unscaledTime < _nextReconcile) return;
        for (int work = 0; work < 12 && !MclslFrameDeadline.Expired; work++)
        {
            if (_reconcilePhase == 0)
            {
                if (_reconcileFamily < run.Families.Count)
                {
                    MclslFamilyRecord family = run.Families[_reconcileFamily++];
                    if (family == null || !string.IsNullOrEmpty(family.MergedIntoId) || family.NativeFamilyId <= 0) continue;
                    if (ByNative.TryGetValue(family.NativeFamilyId, out MclslFamilyRecord canonical) && canonical != family)
                        MergeFamily(run, family, canonical);
                    continue;
                }
                if (run.FamilyMembershipVersion < 1) { run.FamilyMembershipVersion = 1; NotifyChanged(); }
                _reconcileFamily = 0;
                _reconcileActors = MclslActorRegistry.Snapshot();
                _reconcilePhase = 1;
            }
            if (_reconcilePhase is 1 or 2)
            {
                if (_reconcileActor < _reconcileActors.Count)
                {
                    Actor actor = _reconcileActors[_reconcileActor++];
                    if (MclslActorAccessor.Alive(actor)) Observe(actor);
                    continue;
                }
                _reconcileActor = 0;
                if (_reconcilePhase++ == 1) continue;
                _populationComplete = true;
                ReconcileDeaths.Clear();
                foreach (MclslDeathRecord death in run.DeathRecords)
                    if (death != null) ReconcileDeaths[death.ActorId] = death;
            }
            if (_reconcileFamily >= run.Families.Count)
            {
                _reconcilePhase = _reconcileFamily = _reconcileMember = 0;
                _nextReconcile = Time.unscaledTime + 5f;
                return;
            }
            MclslFamilyRecord current = run.Families[_reconcileFamily];
            if (current == null || !string.IsNullOrEmpty(current.MergedIntoId))
            { _reconcileFamily++; _reconcileMember = 0; continue; }
            if (_reconcileMember < current.Members.Count)
            {
                MclslFamilyMemberRecord member = current.Members[_reconcileMember++];
                if (member == null) continue;
                if (MclslActorRegistry.ResolveKnownOrWorld(member.ActorId, out Actor actor) && actor?.data != null)
                {
                    if (MclslActorAccessor.Alive(actor)) Observe(actor);
                    else if (!member.DeathSettled) OnDeath(actor);
                }
                else if (member.Alive || member.Missing)
                {
                    bool confirmed = ReconcileDeaths.TryGetValue(member.ActorId, out MclslDeathRecord death);
                    bool changed = member.Alive || member.Missing == confirmed;
                    member.Alive = false;
                    member.Missing = !confirmed;
                    if (confirmed) member.DeathYear = death.Year;
                    if (changed) NotifyChanged();
                }
                continue;
            }
            EnsureHead(current);
            if (Member(current, current.FocusActorId) is not { Alive: true, Missing: false, IsCultivator: true })
            {
                if (current.FocusActorId != 0) { current.FocusActorId = 0; NotifyChanged(); }
            }
            bool extinct = current.Members.Count > 0 && !current.Members.Any(x => x != null && (x.Alive || x.Missing));
            if (current.Extinct != extinct) { current.Extinct = extinct; NotifyChanged(); }
            if (extinct) TryLiquidateEstate(run, current);
            _reconcileFamily++;
            _reconcileMember = 0;
        }
    }

    private static void RetryDeathLater(Actor actor)
    {
        long id = MclslActorAccessor.Id(actor);
        if (id > 0 && PendingDeathIds.Add(id)) PendingDeaths.Enqueue(actor);
    }

    private static void MergeFamily(MclslWorldRunState run, MclslFamilyRecord source, MclslFamilyRecord target)
    {
        // Stage all collections before the currency commit. The tombstone and cursor
        // make the transfer idempotent across repeated loads and interrupted sweeps.
        var members = target.Members.Concat(source.Members).Where(x => x != null)
            .GroupBy(x => x.ActorId).Select(g => g.OrderByDescending(x => x.DeathSettled)
                .ThenByDescending(x => x.Alive).First()).ToList();
        var items = target.Items.Concat(source.Items).Where(x => x != null).ToList();
        var techniques = target.TechniqueIds.Concat(source.TechniqueIds).Distinct(StringComparer.Ordinal).ToList();
        var history = target.History.Concat(source.History).Distinct(StringComparer.Ordinal).ToList();
        long balance = TreasuryStones(source);
        MclslEconomicResult result = MclslEconomyCommands.Commit(Math.Max(0, MclslRuntime.CurrentYear()), new[]
        {
            new MclslEconomicOperation(MclslEconomicKind.Transfer, MclslCurrency.SpiritStone, balance,
                TreasuryAccount(source), TreasuryAccount(target))
        }, "family-merge/" + source.Id, 1);
        if (result is not (MclslEconomicResult.Applied or MclslEconomicResult.AlreadyApplied))
            throw new InvalidOperationException("同谱家族资金合并失败：" + result);
        target.Members = members; target.Items = items; target.TechniqueIds = techniques; target.History = history;
        target.HistoryCapacity = Math.Max(target.HistoryCapacity, history.Count);
        target.LastAidYear = Math.Max(target.LastAidYear, source.LastAidYear);
        target.Extinct = !members.Any(x => x.Alive || x.Missing);
        if (target.PreferredHeadActorId == 0) target.PreferredHeadActorId = source.PreferredHeadActorId;
        if (target.FocusActorId == 0) target.FocusActorId = source.FocusActorId;
        foreach (var city in source.CityInfluence)
            target.CityInfluence[city.Key] = target.CityInfluence.GetValueOrDefault(city.Key) + city.Value;
        source.MergedIntoId = target.Id;
        source.Members.Clear(); source.Items.Clear(); source.TechniqueIds.Clear(); source.CityInfluence.Clear();
        source.Extinct = true; source.EstateLiquidated = true;
        VisibleFamilies.Remove(source); ById.Remove(source.Id);
        foreach (MclslFamilyMemberRecord member in members) ByActor[member.ActorId] = target;
        NotifyChanged();
    }
}
