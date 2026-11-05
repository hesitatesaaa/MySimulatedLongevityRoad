using System;
using System.Collections.Generic;
using MySimulatedLongevityRoad.Data;
using MySimulatedLongevityRoad.Traits;
using UnityEngine;

namespace MySimulatedLongevityRoad.Systems;

// UI uses its own centered sprite and native pixel sizing, never the world pivot/PPU.
internal static class MclslActorPresentation
{
    internal const float NativeAdultMarkerScale = 0.1f;
    private sealed class Profile
    {
        internal string SpeciesId;
        internal float ShieldDiameter;
        internal Vector2 ShieldCenter;
        internal string Name;
        internal float SelectionScale;
        internal Sprite[] Avatar;
        internal Sprite FeedbackIcon;
    }

    private static readonly Dictionary<string, Profile> Profiles = new(StringComparer.Ordinal);
    private static readonly Dictionary<Subspecies, Subspecies> MigratedLineages = new();

    internal static void ClearRuntime() => MigratedLineages.Clear();

    internal static string CanonicalSpeciesId(string assetId)
        => MclslBeastCatalog.ForAsset(assetId)?.BeastAssetId ?? assetId;

    internal static void Configure(ActorAsset asset, Sprite first)
    {
        if (first == null) throw new InvalidOperationException("头像源图缺失：" + asset.id);
        Configure(asset, first, VisibleBounds(first.texture, first.rect));
    }

    internal static Rect VisibleBounds(Texture2D texture, Rect rect)
    {
        Color32[] pixels = texture.GetPixels32();
        int left=(int)rect.width, right=0, bottom=(int)rect.height, top=0;
        for(int y=0;y<(int)rect.height;y++) for(int x=0;x<(int)rect.width;x++)
        {
            if(pixels[((int)rect.y+y)*texture.width+(int)rect.x+x].a==0)continue;
            left=Math.Min(left,x);right=Math.Max(right,x+1);bottom=Math.Min(bottom,y);top=Math.Max(top,y+1);
        }
        return Rect.MinMaxRect(left,bottom,right,top);
    }

    internal static void Configure(ActorAsset asset, Sprite first, Rect body)
    {
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
            MclslSpriteOwnership.Register(portrait);
            profile = new Profile { Avatar = new[] { portrait } };
            // Native world feedback renders icons in world units. A UI portrait
            // at PPU=1 turns a 128–400px creature into a map-sized spawn marker.
            profile.FeedbackIcon = Sprite.Create(first.texture,first.rect,new Vector2(.5f,.5f),
                Math.Max(first.rect.width,first.rect.height)/12f,0,SpriteMeshType.FullRect);
            profile.FeedbackIcon.name="mclsl_ui_feedback_"+asset.id;
            MclslSpriteOwnership.Register(profile.FeedbackIcon);
            Profiles.Add(asset.id, profile);
        }
        profile.ShieldDiameter=1.2f*Math.Max(body.width,body.height)/first.pixelsPerUnit;
        profile.ShieldCenter=(body.center-first.pivot)/first.pixelsPerUnit;
        profile.SpeciesId = speciesId;
        profile.Name = name;
        // Indicators use the native adult baseline, independent of the large
        // creature template, body artwork, traits and ascended form.
        profile.SelectionScale = NativeAdultMarkerScale;
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
        SpriteTextureLoader._cached_sprites["ui/Icons/" + asset.id] = profile.FeedbackIcon;
        SpriteTextureLoader._cached_sprites["ui/icons/" + asset.id] = profile.FeedbackIcon;
    }

    internal static Sprite Portrait(string assetId)
        => assetId != null && Profiles.TryGetValue(assetId, out Profile profile) ? profile.Avatar[0] : null;

    internal static Sprite FeedbackIcon(string assetId)
        => assetId != null && Profiles.TryGetValue(assetId,out Profile profile) ? profile.FeedbackIcon : null;

    // Mod creatures are drawn far larger than vanilla units, and every native
    // feedback marker multiplies its offset and icon size by current_scale.
    // Left alone, a favourite star or an attack arrow becomes map-sized. Those
    // markers are UI affordances, so mod actors use the vanilla marker size
    // while vanilla units keep their own scale (giants and dragons stay legible).
    internal static bool UsesVanillaMarker(BaseSimObject unit)
        => unit is Actor actor && IsModCreature(actor);

    // Every mod-registered creature, by identity rather than by asset-id prefix
    // alone. Cheap checks first; the two xuanhuang-derived clones do not carry
    // the mclsl_ prefix, so they are matched explicitly.
    private static bool IsModCreature(Actor actor)
    {
        string id = actor?.asset?.id;
        if (id == null) return false;
        return id.StartsWith("mclsl_", StringComparison.Ordinal)
            || Profiles.ContainsKey(id)
            || MclslBeastCatalog.ForAsset(id) != null
            || MclslNamedCharacterRegistration.IsNamedAsset(id)
            || MclslImmortalActorRegistration.IsImmortal(actor)
            || MclslWorldSoulActorRegistration.IsWorldSoulAssetId(id)
            || id == "zombie_mclsl_xuanhuang"
            || id == "nomads_mclsl_xuanhuang";
    }

    // Human-form frames already have normalized PPU. Ignore individual size
    // modifiers only at draw time so placement, traits and age cannot resize them.
    internal static Vector3 HumanoidBodyScale(BaseSimObject unit)
    {
        if (unit is not Actor actor || !MclslBeastActorRegistration.IsAscended(actor))
            return unit.current_scale;
        float scale = actor.asset.base_stats["scale"];
        return new Vector3(scale, scale, 1f);
    }

    internal static float SelectionScale(BaseSimObject unit)
        => UsesVanillaMarker(unit) ? NativeAdultMarkerScale : unit?.current_scale.y ?? 1f;

    internal static Vector3 IndicatorScale(BaseSimObject unit)
        => UsesVanillaMarker(unit) ? new Vector3(NativeAdultMarkerScale, NativeAdultMarkerScale, 1f)
            : unit?.current_scale ?? Vector3.one;

    // Native status offsets multiply by getScaleMod() = actor_scale / 0.1, and
    // Actor.setActorScale keeps current_scale == (actor_scale, actor_scale, 1).
    // A mod actor therefore resolves to the adult baseline (1.0) here.
    internal static float MarkerScaleMod(Actor actor)
        => UsesVanillaMarker(actor) ? 1f : actor == null ? 1f : actor.current_scale.y / NativeAdultMarkerScale;

    internal static float StatusScale(BaseSimObject unit, StatusAsset status)
    {
        if(status.id!="shield" || unit is not Actor actor || !Profiles.TryGetValue(actor.asset.id,out var profile))
            return SelectionScale(unit);
        Sprite bubble=status.sprite_list[0];
        float diameter=Math.Max(bubble.bounds.size.x,bubble.bounds.size.y);
        return profile.ShieldDiameter*HumanoidBodyScale(unit).y/diameter/status.scale;
    }

    internal static ref Vector3 StatusPosition(ref Vector3 position,BaseSimObject unit,StatusAsset status)
    {
        if(status.id=="shield" && unit is Actor actor && Profiles.TryGetValue(actor.asset.id,out var profile))
        {
            Vector2 center=profile.ShieldCenter*HumanoidBodyScale(unit).y;
            position.x+=center.x;
            position.y+=center.y;
        }
        return ref position;
    }

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
