using System.Collections.Generic;
using UnityEngine;

namespace MySimulatedLongevityRoad.Systems.Visual;

// A dying actor can be removed by WorldBox before its next render pass.
// This transient sprite has no Actor or Subspecies and therefore cannot
// change population, lineage, or save data.
internal static class MclslBeastDeathVisualSystem
{
    private sealed class Entry
    {
        internal GameObject Object;
        internal SpriteRenderer Renderer;
        internal Sprite[] Frames;
        internal int Start;
    }

    private static readonly List<Entry> Active = new();
    private const int FramesPerPose = 7;

    internal static void Play(Vector2 position, float scale, Sprite[] frames)
    {
        if (frames == null || frames.Length == 0 || frames[0] == null) return;
        if (!MclslWorldSpriteRenderLayer.TryCreateStaticWorldSprite(
                position, scale, 2, frames[0], out GameObject obj, out SpriteRenderer renderer)) return;
        obj.name = "mclsl_actor_death_visual";
        Active.Add(new Entry { Object = obj, Renderer = renderer, Frames = frames, Start = Time.frameCount });
    }

    internal static void Tick()
    {
        int now = Time.frameCount;
        for (int i = Active.Count - 1; i >= 0; i--)
        {
            Entry entry = Active[i];
            int frame = (now - entry.Start) / FramesPerPose;
            if (entry.Object == null || entry.Renderer == null || frame >= entry.Frames.Length)
            {
                if (entry.Object != null) Object.Destroy(entry.Object);
                Active.RemoveAt(i);
                continue;
            }
            entry.Renderer.sprite = entry.Frames[frame];
        }
    }

    internal static void Clear()
    {
        foreach (Entry entry in Active)
            if (entry.Object != null) Object.Destroy(entry.Object);
        Active.Clear();
    }
}
