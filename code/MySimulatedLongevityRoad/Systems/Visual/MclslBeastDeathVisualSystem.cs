using System.Collections.Generic;
using UnityEngine;

namespace MySimulatedLongevityRoad.Systems.Visual;

// A dying actor can be removed by WorldBox before its next render pass.
// This transient sprite has no Actor or Subspecies and therefore cannot
// change population, lineage, or save data.
internal static class MclslBeastDeathVisualSystem
{
    internal struct Pose
    {
        internal bool Valid;
        internal Vector3 Position;
        internal Vector3 Scale;
        internal Vector3 Rotation;
        internal bool Flip;
        internal Material Material;
        internal Color Color;
    }

    internal static Pose Capture(Actor actor)
    {
        if (actor?.data == null) return default;
        Pose pose = new()
        {
            Valid = true,
            Position = new Vector3(actor.current_position.x + actor.shake_offset.x + actor.move_jump_offset.x,
                actor.current_position.y + actor.position_height + actor.shake_offset.y + actor.move_jump_offset.y,
                actor.position_height),
            Scale = actor.current_scale,
            Rotation = actor.current_rotation,
            Flip = actor.flip,
            Color = actor.color
        };
        // Copy the last native render pose before die() removes or resets the actor.
        ActorManager manager = World.world?.units;
        ActorRenderData render = manager?.render_data;
        Actor[] actors = manager?.visible_units?.array;
        int count = actors == null ? 0 : System.Math.Min(actors.Length, manager.visible_units.count);
        for (int i = 0; i < count && render != null; i++)
        {
            if (!ReferenceEquals(actors[i], actor)) continue;
            bool rendered = render.main_sprites != null && i < render.main_sprites.Length && render.main_sprites[i] != null
                || render.main_sprite_colored != null && i < render.main_sprite_colored.Length && render.main_sprite_colored[i] != null;
            if (!rendered) break;
            if (render.positions != null && i < render.positions.Length) pose.Position = render.positions[i];
            if (render.scales != null && i < render.scales.Length) pose.Scale = render.scales[i];
            if (render.rotations != null && i < render.rotations.Length) pose.Rotation = render.rotations[i];
            if (render.flip_x_states != null && i < render.flip_x_states.Length) pose.Flip = render.flip_x_states[i];
            if (render.materials != null && i < render.materials.Length) pose.Material = render.materials[i];
            if (render.colors != null && i < render.colors.Length) pose.Color = render.colors[i];
            break;
        }
        if (pose.Scale.x == 0 || pose.Scale.y == 0)
        {
            float scale = actor.stats == null ? actor.asset.base_stats["scale"] : actor.stats["scale"];
            pose.Scale = new Vector3(scale > 0 ? scale : 1, scale > 0 ? scale : 1, 1);
        }
        return pose;
    }

    private sealed class Entry
    {
        internal GameObject Object;
        internal SpriteRenderer Renderer;
        internal MclslBeastAnimation.Clip Clip;
        internal double Start;
        internal Actor Owner;
    }

    private static readonly List<Entry> Active = new();
    internal static bool Play(Actor owner, Pose pose, MclslBeastAnimation.Clip clip)
        => Play(owner, pose, clip, null);
    internal static bool Play(Actor owner, Pose pose, MclslBeastAnimation.Clip clip, Sprite fallback)
    {
        if (!pose.Valid || clip?.Complete != true) return false;
        if (!MclslWorldSpriteRenderLayer.TryCreateStaticWorldSprite(
                pose.Position, 1f, 2, clip.At(0) ?? fallback, out GameObject obj, out SpriteRenderer renderer)) return false;
        obj.name = "mclsl_actor_death_visual";
        obj.transform.position = pose.Position;
        obj.transform.localScale = pose.Scale;
        obj.transform.eulerAngles = pose.Rotation;
        renderer.flipX = pose.Flip;
        renderer.color = pose.Color;
        if (pose.Material != null) renderer.sharedMaterial = pose.Material;
        Active.Add(new Entry { Owner = owner, Object = obj, Renderer = renderer, Clip = clip, Start = MclslActorAnimationTime.Now });
        return true;
    }

    internal static void Tick()
    {
        double now = MclslActorAnimationTime.Now;
        for (int i = Active.Count - 1; i >= 0; i--)
        {
            Entry entry = Active[i];
            double elapsed = now - entry.Start;
            if (entry.Object == null || entry.Renderer == null || entry.Owner?.isAlive() == true
                || elapsed < 0 || elapsed >= entry.Clip.Duration)
            {
                if (entry.Object != null) UnityEngine.Object.Destroy(entry.Object);
                Active.RemoveAt(i);
                continue;
            }
            entry.Renderer.sprite = entry.Clip.At(elapsed) ?? entry.Renderer.sprite;
        }
    }

    internal static void Forget(Actor owner)
    {
        for (int i = Active.Count - 1; i >= 0; i--)
        {
            if (!ReferenceEquals(Active[i].Owner, owner)) continue;
            if (Active[i].Object != null) UnityEngine.Object.Destroy(Active[i].Object);
            Active.RemoveAt(i);
        }
    }

    internal static void Clear()
    {
        foreach (Entry entry in Active)
            if (entry.Object != null) UnityEngine.Object.Destroy(entry.Object);
        Active.Clear();
    }
}
