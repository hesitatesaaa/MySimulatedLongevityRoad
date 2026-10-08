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
            EnforceUniqueHeavenFavor(actor);
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

    private static void EnforceUniqueHeavenFavor(Actor recipient)
    {
        try
        {
            var actors = World.world?.units?.getSimpleList();
            if (actors == null) return;
            for (int i = 0; i < actors.Count; i++)
            {
                Actor previous = actors[i];
                if (previous == null || previous == recipient || !previous.hasTrait(MclslTraitRegistration.HeavenFavorTraitId)) continue;
                SuppressRouting(() => previous.removeTrait(MclslTraitRegistration.HeavenFavorTraitId));
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
