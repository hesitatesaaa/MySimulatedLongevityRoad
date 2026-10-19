using System;
using System.Collections.Generic;
using System.IO;
using MySimulatedLongevityRoad.Core;
using UnityEngine;

namespace MySimulatedLongevityRoad.Systems;

// Native art lives outside GameResources so NeoModLoader never imports every PNG.
// Registration checks the complete source set without decoding it. Render workers
// queue keys; only the main runtime tick creates or releases Unity textures.
internal static class MclslNativeActorFrames
{
    // Development limit. Final value requires GPU-memory measurements in WorldBox.
    private const long BudgetBytes = 512L * 1024 * 1024;
    private static readonly MclslActorFrameCache<Sprite> Cache = new(BudgetBytes);
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

    internal static Sprite Request(string resourcePath) => Cache.Request(resourcePath);

    internal static void Tick()
    {
        if (Files.Count == 0) return;
        int loaded = Cache.Pump(Time.realtimeSinceStartup, 4, 4d,
            resource => MclslActorPixelResources.LoadNative(resource, Files[resource], Profiles[resource]), Release);
        if (loaded == 0 && Cache.PendingCount > 0 && Cache.ResidentBytes >= BudgetBytes)
            MclslDiagnostics.Once("native-frame-cache-full", "原生动画缓存已达到内存预算; 正等待未使用帧释放");
        MclslNativeFrameProbe.Tick(Files.Keys, Request, Clear, Cache.ResidentBytes, Cache.PendingCount);
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
