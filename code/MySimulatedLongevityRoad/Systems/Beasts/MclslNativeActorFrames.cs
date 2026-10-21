using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.IO;
using MySimulatedLongevityRoad.Core;
using UnityEngine;

namespace MySimulatedLongevityRoad.Systems;

// Native art lives outside GameResources so NeoModLoader never imports every PNG.
// Registration checks the complete source set without decoding it. Render workers
// queue keys; only the main runtime tick creates or releases Unity textures.
internal static class MclslNativeActorFrames
{
    // A fixed 2 GB ceiling never binds: every frame decoded once is about 356 MB,
    // so the cache would pin the whole set on machines that cannot afford it.
    // Budget by reported VRAM instead, within bounds that still allow streaming.
    private const long MinimumBudgetBytes = 96L * 1024 * 1024;
    private const long MaximumBudgetBytes = 384L * 1024 * 1024;
    private const long FallbackBudgetBytes = 192L * 1024 * 1024;
    private const long InitialBudgetBytes = 64L * 1024 * 1024;
    private static long BudgetBytes;
    private static volatile MclslActorFrameCache<Sprite> Cache;
    internal static long ResidentBytes => (Cache?.ResidentBytes ?? 0) + InitialBytes;
    internal static long StreamingResidentBytes => Cache?.ResidentBytes ?? 0;
    internal static long NativeBudgetBytes => BudgetBytes;
    internal static int PendingFrameCount => Cache?.PendingCount ?? 0;
    // Asset initialization holds these current-art sprites across world loads.
    private static readonly ConcurrentDictionary<string, Sprite> Initials = new(StringComparer.Ordinal);
    private static long InitialBytes;
    private static readonly Dictionary<string, List<string>> Groups = new(StringComparer.Ordinal);
    private static readonly Dictionary<string, string> Files = new(StringComparer.Ordinal);
    private static readonly Dictionary<string, MclslActorAnimationCatalog.FrameProfile> Profiles = new(StringComparer.Ordinal);

    internal static void Register(string resourcePath, MclslActorAnimationCatalog.Form form)
    {
        if (!MclslRuntimeChanges.IsMainThread)
            throw new InvalidOperationException("Native frame registration must run on the main thread");
        EnsureCache();
        if (form?.NativeResourcePath == null)
            throw new InvalidDataException("Native resource group is missing");
        if (Files.ContainsKey(resourcePath)) return;
        string name = Path.GetFileName(resourcePath);
        if (!form.FrameProfiles.TryGetValue(name, out var profile) || !profile.AnchorApproved)
            throw new InvalidDataException("Native frame geometry is not reviewed: " + resourcePath);
        string file = MclslActorAnimationCatalog.ResolveNativeFile(form.NativeResourcePath + "/main/" + name);
        if (!File.Exists(file)) throw new FileNotFoundException("Native animation frame is missing", file);
        long bytes = checked((long)profile.Width * profile.Height * 4);
        Cache.Register(resourcePath, bytes);
        Files[resourcePath] = file;
        Profiles[resourcePath] = profile;
        string group = resourcePath.Substring(0, resourcePath.LastIndexOf('/'));
        if (!Groups.TryGetValue(group, out var keys)) Groups[group] = keys = new List<string>();
        keys.Add(resourcePath);
    }

    internal static Sprite Initial(string resourcePath)
    {
        if (!MclslRuntimeChanges.IsMainThread)
            throw new InvalidOperationException("Initial actor sprites must load on the main thread");
        EnsureCache();
        if (Initials.TryGetValue(resourcePath, out Sprite existing)) return existing;
        var profile = Profiles[resourcePath];
        long bytes = checked((long)profile.Width * profile.Height * 4);
        if (InitialBytes + bytes > InitialBudgetBytes)
            throw new InvalidDataException("Initial actor sprites exceed their reserved budget");
        Sprite sprite = MclslActorPixelResources.LoadNative(resourcePath, Files[resourcePath], profile);
        if (sprite == null) throw new InvalidDataException("Current initial frame failed: " + resourcePath);
        Initials.TryAdd(resourcePath, sprite);
        InitialBytes += bytes;
        // Populate engine frame metadata before overrides run. Waiting poses share
        // this current-art texture; actual animation frames still load on demand.
        string mainPath = resourcePath.Substring(0, resourcePath.LastIndexOf('/'));
        var metadata = new List<Sprite>();
        foreach (string key in Groups[mainPath])
        {
            Sprite entry = sprite;
            if (key != resourcePath)
            {
                entry = Sprite.Create(sprite.texture, sprite.rect,
                    new Vector2(sprite.pivot.x / sprite.rect.width, sprite.pivot.y / sprite.rect.height),
                    sprite.pixelsPerUnit, 0, SpriteMeshType.FullRect);
                entry.name = Path.GetFileName(key);
                MclslSpriteOwnership.Register(entry);
            }
            SpriteTextureLoader._cached_sprites[key] = entry;
            metadata.Add(entry);
        }
        SpriteTextureLoader._cached_sprite_list[mainPath] = metadata.ToArray();
        return sprite;
    }

    internal static Sprite Request(string resourcePath)
        => Initials.TryGetValue(resourcePath, out Sprite initial) ? initial : Cache?.Request(resourcePath);

    private static void EnsureCache()
    {
        if (Cache != null) return;
        if (!MclslRuntimeChanges.IsMainThread)
            throw new InvalidOperationException("Native cache initialization must run on the main thread");
        BudgetBytes = ResolveBudgetBytes();
        Cache = new MclslActorFrameCache<Sprite>(BudgetBytes - InitialBudgetBytes);
    }

    // Called once on the main thread when the first frame registers. SystemInfo is
    // main-thread only, so this must not run from a static initializer off-thread.
    private static long ResolveBudgetBytes()
    {
        int reportedMb = 0;
        try { reportedMb = SystemInfo.graphicsMemorySize; }
        catch (Exception ex) { MclslDiagnostics.Error("native-frame-budget", "读取显存失败: " + ex.Message); }
        if (reportedMb <= 0) return FallbackBudgetBytes;
        long budget = reportedMb * 1024L * 1024L / 6L;
        return Math.Min(MaximumBudgetBytes, Math.Max(MinimumBudgetBytes, budget));
    }

    internal static void Tick(double maxMilliseconds, int maxLoads)
    {
        if (Files.Count == 0) return;
        int loaded = Cache.Pump(Time.realtimeSinceStartup, Math.Max(1, maxLoads), Math.Max(0.5d, maxMilliseconds),
            resource => MclslActorPixelResources.LoadNative(resource, Files[resource], Profiles[resource]), Release);
        if (loaded == 0 && Cache.PendingCount > 0 && Cache.IsBudgetBlocked)
            MclslDiagnostics.Once("native-frame-cache-full", "原生动画缓存已达到显存预算; 正等待未使用帧释放");
    }

    internal static void Clear() => Cache?.Clear(Release);

    private static void Release(Sprite sprite)
    {
        if (sprite == null) return;
        MclslSpriteOwnership.Unregister(sprite);
        Texture2D texture = sprite.texture;
        UnityEngine.Object.Destroy(sprite);
        if (texture != null) UnityEngine.Object.Destroy(texture);
    }
}
