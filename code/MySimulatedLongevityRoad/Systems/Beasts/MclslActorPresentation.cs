using System;
using System.Collections.Generic;
using MySimulatedLongevityRoad.Data;
using MySimulatedLongevityRoad.Traits;
using UnityEngine;

namespace MySimulatedLongevityRoad.Systems;

// UI uses its own centered sprite and native pixel sizing, never the world pivot/PPU.
internal static class MclslActorPresentation
{
    private sealed class Profile
    {
        internal string SpeciesId;
        internal string Name;
        internal float SelectionScale;
        internal Sprite[] Avatar;
    }

    private static readonly Dictionary<string, Profile> Profiles = new(StringComparer.Ordinal);
    private static readonly Dictionary<Subspecies, Subspecies> MigratedLineages = new();

    internal static void ClearRuntime() => MigratedLineages.Clear();

    internal static string CanonicalSpeciesId(string assetId)
        => MclslBeastCatalog.ForAsset(assetId)?.BeastAssetId ?? assetId;

    internal static void Configure(ActorAsset asset, Sprite first)
    {
        if (first == null) throw new InvalidOperationException("头像源图缺失：" + asset.id);
        MclslBeastDefinition beast = MclslBeastCatalog.ForAsset(asset.id);
        string speciesId = CanonicalSpeciesId(asset.id);
        string name = beast?.Name ?? asset.getLocalizedName();
        string templateId = beast != null
            ? (asset.id == beast.AscendedAssetId ? "orc" : beast.AnimalTemplate)
            : asset.id is MclslImmortalActorRegistration.BaiId or MclslImmortalActorRegistration.ChuanfaId ? "sheep" : "human";
        ActorAsset template = AssetManager.actor_library.get(templateId);
        if (!Profiles.TryGetValue(asset.id, out Profile profile))
        {
            Sprite portrait = Sprite.Create(first.texture, first.rect, new Vector2(.5f, .5f), 1f, 0, SpriteMeshType.FullRect);
            portrait.name = "mclsl_ui_" + asset.id;
            profile = new Profile { Avatar = new[] { portrait } };
            Profiles.Add(asset.id, profile);
        }
        profile.SpeciesId = speciesId;
        profile.Name = name;
        profile.SelectionScale = template?.base_stats["scale"] ?? 1f;
        asset.can_have_subspecies = true;
        asset.name_taxonomic_genus = name + "族";
        asset.name_taxonomic_species = string.Empty;
        asset.name_subspecies_add_biome_suffix = false;
        asset.has_override_avatar_frames = true;
        asset.get_override_avatar_frames = _ => profile.Avatar;
        asset.inspect_avatar_scale = 2.5f * 24f / Math.Max(first.rect.width, first.rect.height);
        asset.inspect_avatar_offset_x = 0f;
        asset.inspect_avatar_offset_y = 0f;
        asset.icon = asset.id;
        asset.show_icon_inspect_window_id = asset.id;
        SpriteTextureLoader._cached_sprites["ui/Icons/" + asset.id] = profile.Avatar[0];
        SpriteTextureLoader._cached_sprites["ui/icons/" + asset.id] = profile.Avatar[0];
    }

    internal static Sprite Portrait(string assetId)
        => assetId != null && Profiles.TryGetValue(assetId, out Profile profile) ? profile.Avatar[0] : null;

    internal static float SelectionScale(Actor actor)
        => actor?.asset != null && Profiles.TryGetValue(actor.asset.id, out Profile profile)
            ? profile.SelectionScale : actor?.current_scale.y ?? 1f;

    internal static void NormalizeSubspecies(Subspecies lineage)
    {
        if (lineage?.data == null || !Profiles.TryGetValue(lineage.data.species_id, out Profile profile)) return;
        if ((!lineage.data.custom_name && !(lineage.data.name ?? string.Empty).Contains(profile.Name))
            || string.Equals(lineage.data.name, "NAME", StringComparison.Ordinal))
            lineage.setName(profile.Name + "族·" + lineage.data.id);
    }

    internal static void ReconcileLineage(Actor actor)
    {
        if (actor?.asset == null || actor.data == null || actor.current_tile == null
            || World.world?.subspecies == null || !Profiles.TryGetValue(actor.asset.id, out Profile profile)) return;
        Subspecies previous = actor.subspecies;
        if (previous?.data?.species_id == profile.SpeciesId)
        {
            NormalizeSubspecies(previous);
            return;
        }
        ActorAsset source = AssetManager.actor_library.get(profile.SpeciesId);
        if (source == null) return;
        Subspecies lineage = null;
        // An old ascended lineage is a real edited variant; retain its genes separately.
        if (previous?.data != null && Profiles.TryGetValue(previous.data.species_id, out Profile previousProfile)
            && previousProfile.SpeciesId == profile.SpeciesId)
        {
            if (!MigratedLineages.TryGetValue(previous, out lineage))
            {
                foreach (Subspecies candidate in World.world.subspecies)
                    if (candidate.data?.species_id == profile.SpeciesId
                        && candidate.data.parent_subspecies == previous.data.id) { lineage = candidate; break; }
                if (lineage == null)
                {
                    lineage = World.world.subspecies.newSpecies(source, actor.current_tile, false);
                    // mutateFrom deliberately adds random mutations; migration must only copy.
                    lineage.cloneSubspeciesTraits(previous);
                    lineage.nucleus.cloneFrom(previous.nucleus);
                    lineage.getActorBirthTraits().fillTraitAssetsFromStringList(previous.getActorBirthTraits().getTraitsAsStrings());
                    lineage.data.parent_subspecies = previous.data.id;
                    lineage.genesChangedEvent();
                    if (previous.data.custom_name) lineage.setName(previous.data.name);
                }
                MigratedLineages.Add(previous, lineage);
            }
        }
        else
        {
            foreach (Subspecies candidate in World.world.subspecies)
                if (candidate.data?.species_id == profile.SpeciesId) { lineage = candidate; break; }
            lineage ??= World.world.subspecies.newSpecies(source, actor.current_tile, false);
        }
        if (lineage == null) return;
        actor.setSubspecies(lineage);
        NormalizeSubspecies(lineage);
    }
}
