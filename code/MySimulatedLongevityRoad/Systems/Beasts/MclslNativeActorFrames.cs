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
    private const long BudgetBytes = 2L * 1024 * 1024 * 1024;
    private const long InitialBudgetBytes = 64L * 1024 * 1024;
    private static readonly MclslActorFrameCache<Sprite> Cache = new(BudgetBytes - InitialBudgetBytes);
    // Asset initialization holds these current-art sprites across world loads.
    private static readonly ConcurrentDictionary<string, Sprite> Initials = new(StringComparer.Ordinal);
    private static long InitialBytes;
    private static readonly Dictionary<string, string> Files = new(StringComparer.Ordinal);
    private static readonly Dictionary<string, MclslActorAnimationCatalog.FrameProfile> Profiles = new(StringComparer.Ordinal);

    internal static void Register(string resourcePath, MclslActorAnimationCatalog.Form form)
    {
        if (!MclslRuntimeChanges.IsMainThread)
            throw new InvalidOperationException("Native frame registration must run on the main thread");
        if (form?.NativeResourcePath == null)
            throw new InvalidDataException("Native resource group is missing");
        string name = Path.GetFileName(resourcePath);
        if (!form.FrameProfiles.TryGetValue(name, out var profile) || !profile.AnchorApproved)
            throw new InvalidDataException("Native frame geometry is not reviewed: " + resourcePath);
        string file = MclslActorAnimationCatalog.ResolveNativeFile(form.NativeResourcePath + "/main/" + name);
        if (!File.Exists(file)) throw new FileNotFoundException("Native animation frame is missing", file);
        long bytes = checked((long)profile.Width * profile.Height * 4);
        Cache.Register(resourcePath, bytes);
        Files[resourcePath] = file;
        Profiles[resourcePath] = profile;
    }

    internal static Sprite Initial(string resourcePath)
    {
        if (!MclslRuntimeChanges.IsMainThread)
            throw new InvalidOperationException("Initial actor sprites must load on the main thread");
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
        foreach (string key in Profiles.Keys)
        {
            if (!key.StartsWith(mainPath + "/", StringComparison.Ordinal)) continue;
            Sprite entry = sprite;
            if (key != resourcePath)
            {
                entry = Sprite.Create(sprite.texture, sprite.rect,
                    new Vector2(sprite.pivot.x / sprite.rect.width, sprite.pivot.y / sprite.rect.height),
                    sprite.pixelsPerUnit, 0, SpriteMeshType.FullRect);
                entry.name = Path.GetFileName(key);
            }
            SpriteTextureLoader._cached_sprites[key] = entry;
            metadata.Add(entry);
        }
        SpriteTextureLoader._cached_sprite_list[mainPath] = metadata.ToArray();
        return sprite;
    }

    internal static Sprite Request(string resourcePath)
        => Initials.TryGetValue(resourcePath, out Sprite initial) ? initial : Cache.Request(resourcePath);

    internal static void Tick()
    {
        if (Files.Count == 0) return;
        int loaded = Cache.Pump(Time.realtimeSinceStartup, 4, 4d,
            resource => MclslActorPixelResources.LoadNative(resource, Files[resource], Profiles[resource]), Release);
        if (loaded == 0 && Cache.PendingCount > 0 && Cache.ResidentBytes >= BudgetBytes)
            MclslDiagnostics.Once("native-frame-cache-full", "原生动画缓存已达到内存预算; 正等待未使用帧释放");
    }

    internal static void Clear() => Cache.Clear(Release);

    private static void Release(Sprite sprite)
    {
        if (sprite == null) return;
        Texture2D texture = sprite.texture;
        UnityEngine.Object.Destroy(sprite);
        if (texture != null) UnityEngine.Object.Destroy(texture);
    }
}
