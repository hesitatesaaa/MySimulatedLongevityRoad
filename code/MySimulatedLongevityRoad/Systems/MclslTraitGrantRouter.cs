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
        if (actor?.data == null || string.IsNullOrWhiteSpace(traitId)) return;
        traitId = MclslTraitRegistration.NormalizeTraitId(traitId);
        if (!LooksLikeMclslTraitId(traitId)) return;

        bool isHuanzhen = traitId == MclslTraitRegistration.HuanzhenTraitId;
        bool isGift = MclslTraitRegistration.IsGiftTrait(traitId);
        bool isRealm = MclslTraitRegistration.TryRealmForTrait(traitId, out string realm);
        bool isWorldSoulEntity = traitId == MclslTraitRegistration.WorldSoulEntityTraitId;
        if (!isHuanzhen && !isGift && !isRealm && !isWorldSoulEntity) return;

        MclslWorldActorQuery.Track(actor);
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

    internal static void HandleRemovedTrait(Actor actor, string traitId)
    {
        if (IsSuppressed) return;
        if (actor?.data == null || string.IsNullOrWhiteSpace(traitId)) return;
        traitId = MclslTraitRegistration.NormalizeTraitId(traitId);
        if (!LooksLikeMclslTraitId(traitId)) return;
        if (!MclslTraitRegistration.IsGiftTrait(traitId)
            && !MclslTraitRegistration.TryRealmForTrait(traitId, out _)
            && traitId != MclslTraitRegistration.HuanzhenTraitId
            && traitId != MclslTraitRegistration.WorldSoulEntityTraitId) return;

        MclslWorldActorQuery.Track(actor);
        MclslTraitRegistration.ReconcileTraitState(actor);
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
