using System;
using MySimulatedLongevityRoad.Core;
using MySimulatedLongevityRoad.Data;
using MySimulatedLongevityRoad.Queries;
using MySimulatedLongevityRoad.Traits;
using MySimulatedLongevityRoad.UI;

namespace MySimulatedLongevityRoad.Systems;

internal static class MclslTraitGrantRouter
{
    private static int _suppressionDepth;
    // Small set of actor ids currently holding Heaven's Favor. Kept so uniqueness
    // enforcement iterates only over current favor holders instead of the whole
    // population (World.world.units.getSimpleList()). The set is the source of
    // truth for "who already has the trait": members are added/removed on trait
    // add/remove via the same routing hooks that apply the favor. The trait id is
    // authoritative; if the set and the world ever drift (e.g. a different code
    // path removes the trait), the scan fallback below still reconciles.
    private static readonly System.Collections.Generic.HashSet<long> HeavenFavorHolders
        = new System.Collections.Generic.HashSet<long>();

    internal static bool IsSuppressed => _suppressionDepth > 0;

    internal static void SuppressRouting(Action action)
    {
        if (action == null) return;
        _suppressionDepth++;
        try { action(); }
        finally { _suppressionDepth = Math.Max(0, _suppressionDepth - 1); }
    }

    internal static void HandleAddedTrait(Actor actor, string traitId)
    {
        if (IsSuppressed) return;
        if (MclslHeavenGrantActions.HandleAdded(actor, traitId)) return;
        if (actor?.data == null || string.IsNullOrWhiteSpace(traitId)) return;
        traitId = MclslTraitRegistration.NormalizeTraitId(traitId);
        if (!LooksLikeMclslTraitId(traitId)) return;

        if (Combat.MclslCombatCareerSystem.HandleAdded(actor, traitId)) { MarkAndRefresh(actor); return; }

        bool isHuanzhen = traitId == MclslTraitRegistration.HuanzhenTraitId;
        bool isGift = MclslTraitRegistration.IsGiftTrait(traitId);
        bool isRealm = MclslTraitRegistration.TryRealmForTrait(traitId, out string realm);
        bool isWorldSoulEntity = traitId == MclslTraitRegistration.WorldSoulEntityTraitId;
        bool isProfession = MclslProfessionSystem.IsProfessionTrait(traitId);
        bool isImmortalPath = MclslImmortalPathSystem.IsPathTrait(traitId);
        bool isHeavenFavor = traitId == MclslTraitRegistration.HeavenFavorTraitId;
        bool isPhysique = MclslPhysiqueSystem.IsPhysique(traitId);
        if (!isHuanzhen && !isGift && !isRealm && !isWorldSoulEntity && !isProfession && !isImmortalPath && !isHeavenFavor && !isPhysique) return;

        MclslWorldActorQuery.Track(actor);
        if (isPhysique)
        {
            MclslPhysiqueSystem.OnGranted(actor, traitId);
            if (MclslRuntimeSettings.AutoCollectPhysique) MclslTraitRegistration.TryMarkFavorite(actor);
            MarkAndRefresh(actor);
            return;
        }
        if (isHeavenFavor)
        {
            long recipientId = MclslActorAccessor.Id(actor);
            EnforceUniqueHeavenFavor(recipientId);
            if (recipientId > 0L) HeavenFavorHolders.Add(recipientId);
            MclslTraitRegistration.TryMarkFavorite(actor);
            MclslTechniqueStageSystem.AddProgress(actor, 10);
            MarkAndRefresh(actor);
            return;
        }
        if (isProfession)
        {
            MclslProfessionSystem.OnTraitGranted(actor, traitId);
            MarkAndRefresh(actor);
            return;
        }
        if (isImmortalPath)
        {
            MclslImmortalPathSystem.OnTraitGranted(actor, traitId);
            MclslTraitRegistration.TryMarkFavorite(actor);
            MarkAndRefresh(actor);
            MclslActorInfoPanel.RefreshOpenForActor(actor);
            return;
        }
        if (isRealm || isGift) MclslTraitRegistration.TryAutoCollectTrait(actor, traitId);

        if (isHuanzhen)
        {
            HandleHuanzhenGrant(actor);
            return;
        }

        if (isGift)
        {
            HandleGiftGrant(actor, traitId);
            return;
        }

        if (isRealm)
        {
            HandleRealmGrant(actor, traitId, realm);
            return;
        }

        HandleWorldSoulEntityGrant(actor);
    }

    private static void EnforceUniqueHeavenFavor(long recipientId)
    {
        try
        {
            // Iterate the tracked holder set only. Normally this is one element
            // (uniqueness is invariant), so the cost is O(holders) instead of
            // O(world population). Fall back to a population scan only when the
            // returned set is empty yet a favor grant is happening — meaning the
            // set may have desynced (e.g. favor granted outside the router); a
            // full scan once reconciles the world's existing holder.
            if (HeavenFavorHolders.Count > 0)
            {
                foreach (long previousId in HeavenFavorHolders)
                {
                    if (previousId <= 0L || previousId == recipientId) continue;
                    if (!MclslActorRegistry.Resolve(previousId, out Actor previous) || previous?.data == null) continue;
                    if (!previous.hasTrait(MclslTraitRegistration.HeavenFavorTraitId)) { HeavenFavorHolders.Remove(previousId); continue; }
                    SuppressRouting(() => previous.removeTrait(MclslTraitRegistration.HeavenFavorTraitId));
                    HeavenFavorHolders.Remove(previousId);
                }
                return;
            }
            // Reconcile fallback (set desync): scan the world once to pick up an
            // existing holder granted outside this router, then enforce uniqueness.
            var actors = World.world?.units?.getSimpleList();
            if (actors == null) return;
            foreach (Actor previous in actors)
            {
                if (previous?.data == null || MclslActorAccessor.Id(previous) == recipientId) continue;
                if (!previous.hasTrait(MclslTraitRegistration.HeavenFavorTraitId)) continue;
                SuppressRouting(() => previous.removeTrait(MclslTraitRegistration.HeavenFavorTraitId));
                HeavenFavorHolders.Remove(MclslActorAccessor.Id(previous));
                return; // heaven favor is unique; at most one other holder can exist
            }
        }
        catch (Exception ex) { MclslDiagnostics.Error("heaven-favor-unique", ex.Message); }
    }

    internal static void HandleRemovedTrait(Actor actor, string traitId)
    {
        if (IsSuppressed) return;
        if (actor?.data == null || string.IsNullOrWhiteSpace(traitId)) return;
        traitId = MclslTraitRegistration.NormalizeTraitId(traitId);
        if (!LooksLikeMclslTraitId(traitId)) return;
        if (traitId == MclslTraitRegistration.HeavenFavorTraitId)
            HeavenFavorHolders.Remove(MclslActorAccessor.Id(actor));
        if (Combat.MclslCombatCareerSystem.HandleRemoved(actor, traitId)) { MarkAndRefresh(actor); return; }
        if (MclslProfessionSystem.IsProfessionTrait(traitId))
        {
            MclslProfessionSystem.OnTraitRemoved(actor, traitId);
            MarkAndRefresh(actor);
            return;
        }
        if (MclslPhysiqueSystem.IsPhysique(traitId))
        {
            MarkAndRefresh(actor);
            return;
        }
        if (!MclslTraitRegistration.IsGiftTrait(traitId)
            && !MclslTraitRegistration.TryRealmForTrait(traitId, out _)
            && traitId != MclslTraitRegistration.HuanzhenTraitId
            && traitId != MclslTraitRegistration.WorldSoulEntityTraitId
            && !MclslImmortalPathSystem.IsPathTrait(traitId)) return;

        MclslWorldActorQuery.Track(actor);
        MclslTraitRegistration.ReconcileTraitState(actor);
        if (MclslTraitRegistration.IsGiftTrait(traitId))
            MclslPhysiqueSystem.ReconcileOwner(actor);
        MarkAndRefresh(actor);
    }

    private static void HandleGiftGrant(Actor actor, string traitId)
    {
        MclslActorAccessor.Set(actor, MclslActorDataKeys.ManualGrant, 1);
        MclslTraitRegistration.RemoveOtherGiftTraits(actor, traitId);
        MclslTraitRegistration.TryAutoCollectTrait(actor, traitId);
        MclslSpiritualRootEntrySystem.TryEnterFromGiftTrait(actor, MclslRuntime.CurrentYear());
        MarkAndRefresh(actor, ensureEntryFromGift: true);
    }

    private static void HandleRealmGrant(Actor actor, string traitId, string realm)
    {
        int currentYear = MclslRuntime.CurrentYear();
        bool forceAncient = MclslTraitRegistration.IsAncientRealmTrait(traitId);
        bool forceNewLaw = MclslTraitRegistration.IsNewLawRealmTrait(traitId);
        MclslCultivationSystem.ApplyManualRealmGrant(actor, traitId, realm, currentYear, forceAncient, forceNewLaw);
        MarkAndRefresh(actor);
    }

    private static void HandleHuanzhenGrant(Actor actor)
    {
        MclslHuanzhenSystem.OnTraitGranted(actor);
        MarkAndRefresh(actor);
    }

    private static void HandleWorldSoulEntityGrant(Actor actor)
    {
        MarkAndRefresh(actor);
    }

    private static void MarkAndRefresh(Actor actor, bool ensureEntryFromGift = false)
    {
        MclslCultivationWake.EnsureAwake(
            actor,
            ensureEntryFromGift: ensureEntryFromGift,
            enqueueAnnual: true,
            refreshUi: true);
    }

    private static bool LooksLikeMclslTraitId(string traitId)
    {
        traitId = MclslTraitRegistration.NormalizeTraitId(traitId);
        if (string.IsNullOrWhiteSpace(traitId)) return false;
        if (traitId == MclslTraitRegistration.HuanzhenTraitId) return true;
        if (traitId == MclslTraitRegistration.WorldSoulEntityTraitId) return true;
        return traitId.StartsWith("gifts_", StringComparison.Ordinal)
            || traitId.StartsWith("realm_", StringComparison.Ordinal)
            || traitId.StartsWith("Mclsl", StringComparison.Ordinal);
    }
}
