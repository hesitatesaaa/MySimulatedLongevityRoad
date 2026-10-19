using UnityEngine;

namespace UnityEngine
{
    public class Object { public bool Destroyed; public static void Destroy(Object o) => o.Destroyed = true; }
    public class Sprite : Object { }
    public class Material : Object { }
    public struct Vector2
    {
        public float x, y;
        public Vector2(float x, float y) { this.x = x; this.y = y; }
        public static implicit operator Vector2(Vector3 v) => new(v.x, v.y);
    }
    public struct Vector3
    {
        public float x, y, z;
        public Vector3(float x, float y, float z) { this.x = x; this.y = y; this.z = z; }
    }
    public struct Color { public float r, g, b, a; }
    public class Transform { public Vector3 position, localScale, eulerAngles; }
    public class GameObject : Object { public string name; public Transform transform = new(); }
    public class SpriteRenderer : Object { public Sprite sprite; public bool flipX; public Color color; public Material sharedMaterial; }
}

public class Actor
{
    public object data = new();
    public Vector2 current_position, shake_offset, move_jump_offset;
    public float position_height;
    public Vector3 current_scale, current_rotation;
    public bool flip, Alive;
    public Color color;
    public Dictionary<string, float> stats = new() { ["scale"] = 1 };
    public ActorAsset asset = new();
    public bool isAlive() => Alive;
}
public class ActorAsset { public Dictionary<string, float> base_stats = new() { ["scale"] = 1 }; }
public class ActorVisibleDataArray { public Actor[] array; public int count; }
public class ActorRenderData
{
    public Vector3[] positions, scales, rotations;
    public bool[] flip_x_states;
    public Material[] materials;
    public Color[] colors;
    public Sprite[] main_sprites, main_sprite_colored;
}
public class ActorManager { public ActorVisibleDataArray visible_units; public ActorRenderData render_data; }
public class World { public static World world; public ActorManager units; }

namespace MySimulatedLongevityRoad.Systems
{
    internal static class MclslActorAnimationTime { internal static double Now; }
    internal static class MclslBeastAnimation
    {
        internal sealed class Clip
        {
            internal Sprite[] Sprites = { new(), new(), new() };
            internal bool Complete = true;
            private MclslAnimationClipData Data = new(new[] { "death_0", "death_1", "death_2" }, new[] { 0.1, 0.2, 0.3 }, false);
            internal double Duration => Data.Duration;
            internal Sprite At(double elapsed) => Sprites[Data.FrameIndex(elapsed)];
        }
    }
}
namespace MySimulatedLongevityRoad.Systems.Visual
{
    internal static class MclslWorldSpriteRenderLayer
    {
        internal static List<(GameObject obj, SpriteRenderer renderer)> Created = new();
        internal static bool Fail;
        internal static bool TryCreateStaticWorldSprite(Vector2 pos, float scale, int sorting, Sprite sprite, out GameObject obj, out SpriteRenderer renderer)
        {
            obj = new(); renderer = new() { sprite = sprite };
            if (Fail) return false;
            Created.Add((obj, renderer));
            return true;
        }
    }
}
