namespace UnityEngine
{
    public class Object { public static void Destroy(object value) { } }
    public class Texture2D { }
    public struct Rect { public float width = 256, height = 256; public Rect() { } }
    public struct Vector2 { public float x, y; public Vector2(float a, float b) { x = a; y = b; } }
    public enum SpriteMeshType { FullRect }
    public class Sprite
    {
        public Texture2D texture = new();
        public Rect rect = new();
        public Vector2 pivot = new(128, 128);
        public float pixelsPerUnit = 1;
        public string name;
        public static Sprite Create(Texture2D texture, Rect rect, Vector2 pivot, float ppu, uint extrude, SpriteMeshType mesh)
            => new() { texture = texture, rect = rect, pivot = pivot, pixelsPerUnit = ppu };
    }
    public static class Time { public static float realtimeSinceStartup; }
}
public static class SpriteTextureLoader
{
    public static Dictionary<string, UnityEngine.Sprite> _cached_sprites = new();
    public static Dictionary<string, UnityEngine.Sprite[]> _cached_sprite_list = new();
}
namespace MySimulatedLongevityRoad.Core
{
    internal static class MclslRuntimeChanges
    {
        internal static int Owner = Environment.CurrentManagedThreadId;
        internal static bool IsMainThread => Environment.CurrentManagedThreadId == Owner;
    }
    internal static class MclslDiagnostics { internal static void Once(string key, string message) { } }
}
namespace MySimulatedLongevityRoad.Systems
{
    internal static class MclslActorAnimationCatalog
    {
        internal static string Root;
        internal sealed class FrameProfile { internal int Width = 256, Height = 256; internal bool AnchorApproved = true; }
        internal sealed class Form
        {
            internal string NativeResourcePath;
            internal Dictionary<string, FrameProfile> FrameProfiles = new();
        }
        internal static string ResolveNativeFile(string path) => Path.Combine(Root, path + ".png");
    }
    internal static class MclslActorPixelResources
    {
        internal static Dictionary<string, int> Loads = new();
        internal static string Failed;
        internal static UnityEngine.Sprite LoadNative(string key, string file, MclslActorAnimationCatalog.FrameProfile profile)
        {
            if (!MySimulatedLongevityRoad.Core.MclslRuntimeChanges.IsMainThread) throw new Exception("Worker decoded a texture");
            Loads[key] = Loads.GetValueOrDefault(key) + 1;
            return key == Failed ? null : new UnityEngine.Sprite();
        }
    }
}
