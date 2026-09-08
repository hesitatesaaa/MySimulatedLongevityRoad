using System;
using System.Collections.Generic;
using MySimulatedLongevityRoad.Data;
using MySimulatedLongevityRoad.Systems.Death;
using MySimulatedLongevityRoad.Traits;
using UnityEngine;

namespace MySimulatedLongevityRoad.Systems;

internal static partial class MclslWorldSoulSystem
{
    private static bool Manifest(MclslWorldSoulRecord soul, int year, bool manual = false)
    {
        Actor target = PickManifestTarget();
        WorldTile tile = target?.current_tile ?? PickGroundTile();
        if (tile == null) return false;
        Actor entity = CreateManifestEntity(tile, soul);
        if (entity?.data == null) return false;
        string[] tags = MclslGeneratedObjectFactory.SplitTags(soul.LawTags);
        string manifestName = MclslProceduralLexicon.WorldSoulManifestName(soul.Name, tags, StableHash(soul.Id + "|manifest_name|" + year + "|" + soul.ManifestCount));
        try { entity.data.name = manifestName; } catch (System.Exception mclslEmptyCatchEx) { MySimulatedLongevityRoad.Core.MclslDiagnostics.Error("empty-catch-code-MySimulatedLongevityRoad-Systems-WorldSoul-MclslWorldSoulSystem-Manifestation-cs-1", "空 catch 捕获: code/MySimulatedLongevityRoad/Systems/WorldSoul/MclslWorldSoulSystem.Manifestation.cs #1: " + mclslEmptyCatchEx.Message); }
        MclslActorAccessor.Set(entity, MclslActorDataKeys.WorldSoulEntityId, soul.Id);
        MclslActorAccessor.Set(entity, MclslActorDataKeys.WorldSoulEntityQuality, soul.Quality);
        ActorTrait marker = AssetManager.traits.get(MclslTraitRegistration.WorldSoulEntityTraitId);
        try { if (marker != null) entity.addTrait(marker, true); } catch (System.Exception mclslEmptyCatchEx) { MySimulatedLongevityRoad.Core.MclslDiagnostics.Error("empty-catch-code-MySimulatedLongevityRoad-Systems-WorldSoul-MclslWorldSoulSystem-Manifestation-cs-2", "空 catch 捕获: code/MySimulatedLongevityRoad/Systems/WorldSoul/MclslWorldSoulSystem.Manifestation.cs #2: " + mclslEmptyCatchEx.Message); }
        try
        {
            entity.updateStats();
            float max = entity.getMaxHealth();
            if (max > 0f) entity.data.health = Math.Max(1, (int)Math.Min((float)int.MaxValue, max));
        }
        catch (System.Exception mclslEmptyCatchEx) { MySimulatedLongevityRoad.Core.MclslDiagnostics.Error("empty-catch-code-MySimulatedLongevityRoad-Systems-WorldSoul-MclslWorldSoulSystem-Manifestation-cs-3", "空 catch 捕获: code/MySimulatedLongevityRoad/Systems/WorldSoul/MclslWorldSoulSystem.Manifestation.cs #3: " + mclslEmptyCatchEx.Message); }
        MaintainWorldSoulEntity(entity);

        soul.State = "显化";
        soul.ManifestActorId = MclslActorAccessor.Id(entity);
        soul.ManifestActorName = manifestName;
        soul.ManifestYear = year;
        soul.ManifestCombatStartedYear = 0;
        soul.LastHunterDispatchYear = 0;
        soul.ManifestCount++;
        soul.LocationName = ResolveManifestLocation(tile, target);
        soul.NativeKingdomName = string.IsNullOrWhiteSpace(target?.kingdom?.data?.name) ? "无主" : target.kingdom.data.name;
        soul.MapX = tile.pos.x;
        soul.MapY = tile.pos.y;
        soul.NativeTerrainEffect = MclslNativeTerrainProfileCatalog.ForSoul(tags, soul.HeavenlyDuty);
        string prefix = manual ? "玄黄仙录一动，" : string.Empty;
        MclslWorldRunRepository.AddEvent(year, "world_soul_manifest", "天地之魄·" + soul.Name + "临世", prefix + manifestName + "现于" + soul.LocationName + "。", soul.MapX, soul.MapY, soul.LocationName, soul.NativeKingdomName);
        AnnounceWorldSoul("天地之魄·" + soul.Name + "临世于" + soul.LocationName + "。", "#D0B067", 11f);
        MclslWorldArchiveStore.MarkDirty();
        return true;
    }

    private static string ResolveManifestLocation(WorldTile tile, Actor target)
    {
        string city = target?.city?.data?.name;
        string coordinate = tile == null ? string.Empty : "（" + tile.pos.x + "," + tile.pos.y + "）";
        if (!string.IsNullOrWhiteSpace(city)) return city + "附近" + coordinate;
        string kingdom = target?.kingdom?.data?.name;
        if (!string.IsNullOrWhiteSpace(kingdom)) return kingdom + "境内" + coordinate;
        return tile == null ? "无主荒域" : "无主荒域" + coordinate;
    }

    private static void DispatchHuntersIfNeeded(MclslWorldSoulRecord soul, Actor manifest, int year)
    {
        if (soul == null || !MclslActorAccessor.Alive(manifest)) return;
        if (soul.LastHunterDispatchYear > 0 && year - soul.LastHunterDispatchYear < HunterDispatchIntervalYears) return;
        IReadOnlyList<Actor> candidates = MclslCultivatorCandidateIndex.SelectRealm(
            MclslRealmIds.HuaShen,
            60,
            actor => IsQualifiedHunter(actor) && actor.current_tile != null,
            actor => HunterScore(actor, manifest, soul));
        int sent = 0;
        for (int i = 0; i < candidates.Count && sent < MaxHuntersPerManifest; i++)
        {
            Actor hunter = candidates[i];
            if (!MclslActorAccessor.Alive(hunter) || hunter == manifest) continue;
            try
            {
                hunter.setAttackTarget(manifest);
                hunter.beh_actor_target = manifest;
                sent++;
            }
            catch (System.Exception mclslEmptyCatchEx) { MySimulatedLongevityRoad.Core.MclslDiagnostics.Error("empty-catch-code-MySimulatedLongevityRoad-Systems-WorldSoul-MclslWorldSoulSystem-Manifestation-cs-5", "空 catch 捕获: code/MySimulatedLongevityRoad/Systems/WorldSoul/MclslWorldSoulSystem.Manifestation.cs #5: " + mclslEmptyCatchEx.Message); }
        }

        soul.LastHunterDispatchYear = year;
        if (sent <= 0) return;
        MclslWorldArchiveStore.MarkDirty();
    }

    private static bool IsQualifiedHunter(Actor actor)
    {
        if (!MclslEligibility.CanClaimWorldSoul(actor)) return false;
        if (!string.Equals(MclslActorAccessor.Realm(actor), MclslRealmIds.HuaShen, StringComparison.Ordinal)) return false;
        string system = MclslActorAccessor.GetString(actor, MclslActorDataKeys.CultivationSystem, string.Empty);
        return string.Equals(system, MclslCultivationSystemIds.NewLaw, StringComparison.Ordinal);
    }

    private static int HunterScore(Actor actor, Actor manifest, MclslWorldSoulRecord soul)
    {
        int score = Math.Max(0, MclslRealmIds.Index(MclslActorAccessor.Realm(actor))) * 1000;
        score += Math.Clamp(MclslCultivationLineage.Compatibility(actor, soul.LawTags), 0, 100) * 5;
        score -= Math.Min(500, DistanceSquared(actor, manifest) / 20);
        score += StableHash(MclslActorAccessor.Id(actor) + "|" + soul.Id + "|hunt") % 80;
        return score;
    }

    private static void DepartPeacefully(MclslWorldSoulRecord soul, Actor manifest, int year)
    {
        if (soul == null) return;
        string manifestName = string.IsNullOrWhiteSpace(soul.ManifestActorName) ? soul.Name : soul.ManifestActorName;
        try
        {
            if (MclslActorAccessor.Alive(manifest))
            {
                MclslWorldSoulActorRegistration.ForgetActorRuntime(MclslActorAccessor.Id(manifest));
                MclslActorAccessor.Set(manifest, MclslActorDataKeys.WorldSoulEntityId, string.Empty);
                MclslNativeKillStatisticsSystem.DetachScriptedDeathAttacker(manifest);
                manifest.die(false, AttackType.Other, false, false);
            }
        }
        catch (System.Exception mclslEmptyCatchEx) { MySimulatedLongevityRoad.Core.MclslDiagnostics.Error("empty-catch-code-MySimulatedLongevityRoad-Systems-WorldSoul-MclslWorldSoulSystem-Manifestation-cs-6", "空 catch 捕获: code/MySimulatedLongevityRoad/Systems/WorldSoul/MclslWorldSoulSystem.Manifestation.cs #6: " + mclslEmptyCatchEx.Message); }

        soul.ManifestActorId = 0;
        soul.ManifestActorName = string.Empty;
        soul.State = "散逸";
        soul.NextManifestYear = year + 70 + StableHash(soul.Id + "|peaceful_departure|" + year) % 81;
        MclslWorldRunRepository.AddEvent(year, "world_soul_departed", "天地之魄·" + soul.Name + "归隐天地", "“" + manifestName + "”隐没于天地之间。");
        AnnounceWorldSoul("天地之魄·" + soul.Name + "归隐天地。", "#D0B067", 8f);
        MclslWorldArchiveStore.MarkDirty();
    }

    private static void MarkManifestCombatStarted(MclslWorldSoulRecord soul, int year)
    {
        if (soul == null || soul.ManifestCombatStartedYear > 0) return;
        soul.ManifestCombatStartedYear = year;
        MclslWorldArchiveStore.MarkDirty();
    }

    private static bool IsManifestCombatOpened(Actor manifest)
    {
        if (!MclslActorAccessor.Alive(manifest)) return false;
        try { if (manifest.attackedBy is Actor attacker && attacker != manifest && MclslActorAccessor.Alive(attacker)) return true; } catch (System.Exception mclslEmptyCatchEx) { MySimulatedLongevityRoad.Core.MclslDiagnostics.Error("empty-catch-code-MySimulatedLongevityRoad-Systems-WorldSoul-MclslWorldSoulSystem-Manifestation-cs-7", "空 catch 捕获: code/MySimulatedLongevityRoad/Systems/WorldSoul/MclslWorldSoulSystem.Manifestation.cs #7: " + mclslEmptyCatchEx.Message); }
        try { if (manifest.has_attack_target && manifest.attack_target != null) return true; } catch (System.Exception mclslEmptyCatchEx) { MySimulatedLongevityRoad.Core.MclslDiagnostics.Error("empty-catch-code-MySimulatedLongevityRoad-Systems-WorldSoul-MclslWorldSoulSystem-Manifestation-cs-8", "空 catch 捕获: code/MySimulatedLongevityRoad/Systems/WorldSoul/MclslWorldSoulSystem.Manifestation.cs #8: " + mclslEmptyCatchEx.Message); }
        try { if (manifest.beh_actor_target != null) return true; } catch (System.Exception mclslEmptyCatchEx) { MySimulatedLongevityRoad.Core.MclslDiagnostics.Error("empty-catch-code-MySimulatedLongevityRoad-Systems-WorldSoul-MclslWorldSoulSystem-Manifestation-cs-9", "空 catch 捕获: code/MySimulatedLongevityRoad/Systems/WorldSoul/MclslWorldSoulSystem.Manifestation.cs #9: " + mclslEmptyCatchEx.Message); }
        try
        {
            float max = manifest.getMaxHealth();
            if (max > 0f && manifest.data.health < max) return true;
        }
        catch (System.Exception mclslEmptyCatchEx) { MySimulatedLongevityRoad.Core.MclslDiagnostics.Error("empty-catch-code-MySimulatedLongevityRoad-Systems-WorldSoul-MclslWorldSoulSystem-Manifestation-cs-10", "空 catch 捕获: code/MySimulatedLongevityRoad/Systems/WorldSoul/MclslWorldSoulSystem.Manifestation.cs #10: " + mclslEmptyCatchEx.Message); }
        return false;
    }

    private static Actor PickManifestTarget()
    {
        IReadOnlyList<Actor> units = MclslCultivatorCandidateIndex.SelectRealm(
            MclslRealmIds.HuaShen,
            120,
            actor => IsQualifiedHunter(actor) && actor.current_tile != null,
            ManifestTargetScore);
        Actor best = null;
        int bestScore = int.MinValue;
        for (int i = 0; i < units.Count; i++)
        {
            Actor actor = units[i];
            int score = ManifestTargetScore(actor);
            if (score > bestScore) { bestScore = score; best = actor; }
        }
        return best;
    }

    private static WorldTile PickGroundTile()
    {
        if (World.world == null || MapBox.width <= 0 || MapBox.height <= 0) return null;
        for (int i = 0; i < 80; i++)
        {
            int x = UnityEngine.Random.Range(5, Math.Max(6, MapBox.width - 5));
            int y = UnityEngine.Random.Range(5, Math.Max(6, MapBox.height - 5));
            WorldTile tile = World.world.GetTile(x, y);
            if (tile?.Type != null && tile.Type.ground && !tile.Type.block) return tile;
        }
        return null;
    }

    private static void CountWorldMaturity(out int huaShen, out int goldenCore, out int cultivators)
    {
        cultivators = MclslCultivatorCandidateIndex.GetCultivatorActorsSnapshot().Count;
        goldenCore = MclslCultivatorCandidateIndex.CountRealmAtLeast(MclslRealmIds.JinDan);
        IReadOnlyList<Actor> huaShenActors = MclslCultivatorCandidateIndex.SelectRealm(
            MclslRealmIds.HuaShen,
            0,
            IsQualifiedHunter,
            null);
        huaShen = huaShenActors.Count;
    }

    private static int ManifestTargetScore(Actor actor)
    {
        int realm = MclslRealmIds.Index(MclslActorAccessor.Realm(actor));
        int score = realm * 100 + StableHash(MclslActorAccessor.Id(actor) + "|manifest") % 100;
        if (actor?.city != null) score += 30;
        return score;
    }

    private static int DistanceSquared(Actor a, Actor b)
    {
        try
        {
            if (a?.data == null || b?.data == null) return int.MaxValue / 4;
            int dx = a.data.x - b.data.x;
            int dy = a.data.y - b.data.y;
            return dx * dx + dy * dy;
        }
        catch
        {
            return int.MaxValue / 4;
        }
    }

    private static Actor CreateManifestEntity(WorldTile tile, MclslWorldSoulRecord soul)
    {
        if (tile == null || World.world?.units == null) return null;
        string primaryAssetId = MclslWorldSoulActorRegistration.ActorAssetIdFor(soul);
        string[] assetIds = { primaryAssetId, MclslWorldSoulActorRegistration.ActorAssetId };
        for (int i = 0; i < assetIds.Length; i++)
        {
            string assetId = assetIds[i];
            if (string.IsNullOrWhiteSpace(assetId)) continue;
            bool duplicate = false;
            for (int j = 0; j < i; j++)
            {
                if (!string.Equals(assetIds[j], assetId, StringComparison.Ordinal)) continue;
                duplicate = true;
                break;
            }
            if (duplicate) continue;
            try
            {
                Actor entity = World.world.units.createNewUnit(assetId, tile, false, 0f, null, null, true, false, false, false);
                if (entity?.data != null)
                {
                    MclslWorldSoulActorRegistration.NormalizeActor(entity);
                    return entity;
                }
            }
            catch (System.Exception mclslEmptyCatchEx) { MySimulatedLongevityRoad.Core.MclslDiagnostics.Error("empty-catch-code-MySimulatedLongevityRoad-Systems-WorldSoul-MclslWorldSoulSystem-Manifestation-cs-11", "空 catch 捕获: code/MySimulatedLongevityRoad/Systems/WorldSoul/MclslWorldSoulSystem.Manifestation.cs #11: " + mclslEmptyCatchEx.Message); }
        }
        return null;
    }
}
