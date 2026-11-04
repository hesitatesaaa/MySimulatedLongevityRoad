using System;
using System.Collections.Generic;
using MySimulatedLongevityRoad.Traits;
using UnityEngine;

namespace MySimulatedLongevityRoad.Systems.Visual;

// Native held weapons/tools are drawn from ActorRenderData.item_scale, which the
// vanilla ActorManager fills with the actor's whole-body current_scale. The mod
// 仙道人物 bodies are authored on a 256px canvas at a high pixels-per-unit and then
// shown through renderScale (about 1.6 for human forms), so inheriting
// current_scale makes their original vanilla tools roughly 16x too large next to
// the body.
//
// Vanilla humanoids use the same item_scale convention, so the item/body ratio is
// just the ratio of their sprite pixel heights (both use PPU 1). The reference
// humanoid recorded by resources/actor-size-policy.json ("human"/SpiritTraveler)
// has a visible pixel height of 64 - 21 = 43. Scaling an item so that its world
// height equals (body visible pixel height / 43) keeps the vanilla proportion:
// item_scale = StandingVisibleHeight / PixelsPerUnit * current_scale / 43.
//
// Coverage is limited to the manually placed original characters shown in the
// 仙道人物 page: the two authored immortals (白先生/传法天尊) and the non-beast
// entries of MclslNamedCharacterRegistration. The three beast-form originals
// (帝一/帝叁貘/青神龙) are excluded entirely, including their "_yuan" ascension
// forms, so they keep the native scale. Ordinary vanilla actors and all other
// beasts also keep current_scale, and the engine's own debug item-size multiplier
// is preserved because the correction is applied as a ratio against the value
// ActorManager just wrote.
internal static class MclslHeldItemRenderScale
{
    // resources/actor-size-policy.json -> reference "human" (SpiritTraveler):
    // canvas 64x64, bounds [24, 21, 44, 64], pixelsPerUnit 1, visible height 43px.
    internal const float VanillaHumanVisiblePixelHeight = 43f;

    private static readonly Dictionary<string, float> BodyWorldHeightPerScale = new(StringComparer.Ordinal);
    private static int _lastNormalizedFrame = -1;
    private static ActorRenderData _lastNormalizedRenderData;

    // Called from the render pre-calculation postfix. Idempotent per frame: the
    // vanilla value is recomputed from current_scale each frame, but a second call
    // for the same render data must not apply the ratio twice.
    internal static int Normalize(ActorManager manager)
    {
        if (manager?.visible_units == null || manager.render_data == null) return 0;
        ActorRenderData renderData = manager.render_data;
        int frame = Time.frameCount;
        if (_lastNormalizedFrame == frame && ReferenceEquals(_lastNormalizedRenderData, renderData)) return 0;
        _lastNormalizedFrame = frame;
        _lastNormalizedRenderData = renderData;

        Actor[] actors = manager.visible_units.array;
        Vector3[] itemScales = renderData.item_scale;
        if (actors == null || itemScales == null) return 0;

        int count = Math.Min(manager.visible_units.count, Math.Min(actors.Length, itemScales.Length));
        int normalized = 0;
        for (int i = 0; i < count; i++)
        {
            Actor actor = actors[i];
            if (!TryResolveFormPath(actor, out string path)) continue;
            if (BodyScale(actor) <= 0f) continue;
            float ratio = BodyWorldHeightPerScaleOf(path) / VanillaHumanVisiblePixelHeight;
            if (ratio <= 0f) continue;
            Vector3 value = itemScales[i];
            itemScales[i] = new Vector3(value.x * ratio, value.y * ratio, value.z);
            normalized++;
        }
        return normalized;
    }

    internal static void Clear()
    {
        BodyWorldHeightPerScale.Clear();
        _lastNormalizedFrame = -1;
        _lastNormalizedRenderData = null;
    }

    // Resolves the authored humanoid form path only for the originals shown in
    // the 仙道人物 page. Vanilla actors, ordinary/other beasts and the beast-form
    // originals (MclslNamedCharacterRegistration Beast entries, including their
    // "_yuan" ascension ids) return false and are left untouched. This deliberately
    // does not widen IsImmortal: the immortal check only matches the two authored
    // immortals.
    private static bool TryResolveFormPath(Actor actor, out string path)
    {
        path = null;
        string id = actor?.asset?.id;
        if (string.IsNullOrEmpty(id)) return false;

        if (MclslImmortalActorRegistration.IsImmortal(actor))
        {
            path = MclslImmortalActorRegistration.FormPath(actor);
            return !string.IsNullOrEmpty(path);
        }

        // Only the non-beast entries of the named registry resolve to a path; the
        // registry returns null for beast definitions and unknown asset ids.
        path = MclslNamedCharacterRegistration.HumanoidHeldItemFormPath(id);
        return !string.IsNullOrEmpty(path);
    }

    // The world height the mod body would have at scale 1. current_scale is applied
    // separately by the renderer, so this is a per-form constant.
    private static float BodyWorldHeightPerScaleOf(string path)
    {
        if (string.IsNullOrEmpty(path)) return 0f;
        if (BodyWorldHeightPerScale.TryGetValue(path, out float cached)) return cached;

        MclslActorAnimationCatalog.Form form = MclslActorAnimationCatalog.Get(path);
        if (form == null) return 0f;
        float pixelsPerUnit = form.PixelsPerUnit;
        if (form.FrameProfiles.TryGetValue("idle_0", out MclslActorAnimationCatalog.FrameProfile idle)
            && idle.PixelsPerUnit > 0f)
        {
            pixelsPerUnit = idle.PixelsPerUnit;
        }
        float visibleHeight = form.StandingVisibleHeight > 0f ? form.StandingVisibleHeight : form.Height;
        if (pixelsPerUnit <= 0f || visibleHeight <= 0f) return 0f;
        float value = visibleHeight / pixelsPerUnit;
        BodyWorldHeightPerScale[path] = value;
        return value;
    }

    private static float BodyScale(Actor actor)
    {
        float scale = actor.current_scale.y;
        if (scale > 0f) return scale;
        return actor.actor_scale > 0f ? actor.actor_scale : MclslActorPresentation.NativeAdultMarkerScale;
    }
}
