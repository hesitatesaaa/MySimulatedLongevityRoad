using System;
using System.Collections.Generic;
using System.IO;
using MySimulatedLongevityRoad.Core;
using NeoModLoader.General;
using UnityEngine;

namespace MySimulatedLongevityRoad.Systems;

// World frames retain their complete canvas. UI icons have a separate namespace.
// Source pixels are never resized, quantized, compressed or trimmed here.
internal static class MclslActorPixelResources
{
    private static readonly Dictionary<string, Sprite> Loaded = new(StringComparer.Ordinal);
    private static Sprite _transparent;
    internal static Sprite TransparentSprite
    {
        get
        {
            if (_transparent != null) return _transparent;
            Texture2D texture = new(1, 1, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            texture.SetPixel(0, 0, Color.clear);
            texture.Apply(false, true);
            _transparent = Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
            _transparent.name = "mclsl_hidden_native_corpse";
            MclslSpriteOwnership.Register(_transparent);
            return _transparent;
        }
    }

    private static Sprite Load(string resourcePath)
    {
        if (string.IsNullOrEmpty(resourcePath)) return null;
        if (Loaded.TryGetValue(resourcePath, out Sprite cached)) return cached;
        Sprite original = SpriteTextureLoader.getSprite(resourcePath)
            ?? SpriteTextureLoader.getSprite("GameResources/" + resourcePath);
        // NeoModLoader caches one sprite per imported path. Decoding the same PNG
        // again would keep a second full-size RGBA texture resident for the whole
        // session (roughly 6 MB for a 1254x1254 icon), so reuse the imported
        // texture and only rebuild the full-canvas sprite the UI expects.
        Sprite shared = ReuseImported(resourcePath, original);
        if (shared != null) { MclslSpriteOwnership.Register(shared); Loaded[resourcePath] = shared; return shared; }
        Texture2D texture = null;
        try
        {
            // Restrict reads to this mod's resource tree, including on Windows.
            string file = MclslActorAnimationCatalog.ResolveFile(resourcePath);
            if (!File.Exists(file)) { Loaded[resourcePath] = null; return null; }
            texture = new Texture2D(2, 2, TextureFormat.RGBA32, false)
            {
                name = "mclsl_raw_" + resourcePath,
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
                anisoLevel = 0
            };
            // Keep native GPU pixels, but release the unused CPU-readable copy.
            // All rendering consumes this sprite; authoring/validation reads the PNG file.
            if (!ImageConversion.LoadImage(texture, File.ReadAllBytes(file), true))
                throw new InvalidDataException("PNG decode failed: " + resourcePath);
            // LoadImage decodes PNG into an uncompressed format. Do not call Compress.
            if (texture.mipmapCount != 1 || (texture.format != TextureFormat.RGBA32
                && texture.format != TextureFormat.ARGB32 && texture.format != TextureFormat.RGB24))
                throw new InvalidDataException("Unexpected texture format/mipmap: " + resourcePath);
            Vector2 pivot = original == null ? new Vector2(0.5f, 0.5f)
                : new Vector2(original.pivot.x / original.rect.width, original.pivot.y / original.rect.height);
            float pixelsPerUnit = original == null ? 1f : original.pixelsPerUnit;
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height),
                pivot, pixelsPerUnit, 0, SpriteMeshType.FullRect);
            sprite.name = Path.GetFileName(resourcePath);
            MclslSpriteOwnership.Register(sprite);
            Loaded[resourcePath] = sprite;
            MclslDiagnostics.Once("pixel-texture:" + resourcePath,
                resourcePath + " " + texture.width + "x" + texture.height
                + " format=" + texture.format + " filter=" + texture.filterMode
                + " mipmaps=" + texture.mipmapCount + " ppu=" + pixelsPerUnit + " pivot=" + pivot);
            return sprite;
        }
        catch (Exception ex)
        {
            if (texture != null) UnityEngine.Object.Destroy(texture);
            MclslDiagnostics.Error("pixel-load:" + resourcePath, ex.Message);
            Loaded[resourcePath] = null;
            return null;
        }
    }

    // Called only on the main thread by Load. Returns null when the imported
    // sprite is missing or unusable, so the caller falls back to decoding.
    private static Sprite ReuseImported(string resourcePath, Sprite original)
    {
        if (original == null) return null;
        Texture2D texture;
        try { texture = original.texture; }
        catch (Exception ex) { MclslDiagnostics.Error("pixel-reuse:" + resourcePath, ex.Message); return null; }
        if (texture == null || texture.width <= 0 || texture.height <= 0) return null;
        try
        {
            Vector2 pivot = new(original.pivot.x / original.rect.width, original.pivot.y / original.rect.height);
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height),
                pivot, original.pixelsPerUnit, 0, SpriteMeshType.FullRect);
            if (sprite == null) return null;
            sprite.name = Path.GetFileName(resourcePath);
            MclslDiagnostics.Once("pixel-reuse:" + resourcePath,
                resourcePath + " 复用已导入贴图 " + texture.width + "x" + texture.height);
            return sprite;
        }
        catch (Exception ex)
        {
            MclslDiagnostics.Error("pixel-reuse:" + resourcePath, "复用贴图失败: " + ex.Message);
            return null;
        }
    }

    // Called only by the bounded cache's main-thread pump. This sprite is never
    // inserted into Loaded; eviction must be able to release its GPU texture.
    internal static Sprite LoadNative(string resourcePath, string file,
        MclslActorAnimationCatalog.FrameProfile profile)
    {
        using var profileScope = MclslUnityProfiler.Sample("MCLS/Resources/NativeFrame");
        Texture2D texture = null;
        try
        {
            texture = new Texture2D(2, 2, TextureFormat.RGBA32, false)
            {
                name = "mclsl_native_" + resourcePath,
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
                anisoLevel = 0
            };
            byte[] png;
            using (MclslUnityProfiler.Sample("MCLS/Resources/ReadPNG"))
                png = File.ReadAllBytes(file);
            bool decoded;
            using (MclslUnityProfiler.Sample("MCLS/Resources/DecodeAndUploadPNG"))
                decoded = ImageConversion.LoadImage(texture, png, true);
            if (!decoded
                || texture.width != profile.Width || texture.height != profile.Height
                || texture.mipmapCount != 1 ||
                (texture.format != TextureFormat.RGBA32 && texture.format != TextureFormat.ARGB32
                 && texture.format != TextureFormat.RGB24))
                throw new InvalidDataException("Native PNG format/canvas mismatch: " + resourcePath);
            Vector2 pivot = new(profile.AnchorX / texture.width, 1f - profile.AnchorY / texture.height);
            Sprite sprite;
            using (MclslUnityProfiler.Sample("MCLS/Resources/CreateSprite"))
                sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height),
                    pivot, profile.PixelsPerUnit, 0, SpriteMeshType.FullRect);
            if (sprite == null)
                throw new InvalidDataException("Native sprite creation failed: " + resourcePath);
            sprite.name = Path.GetFileName(resourcePath);
            MclslSpriteOwnership.Register(sprite);
            return sprite;
        }
        catch (Exception ex)
        {
            if (texture != null) UnityEngine.Object.Destroy(texture);
            MclslDiagnostics.Error("native-frame:" + resourcePath, ex.Message);
            return null;
        }
    }

    internal static Sprite PlacementIcon(string actorId)
    {
        Sprite icon = Load("ui/ActorIcons/" + actorId);
        if (icon != null) return icon;
        MclslDiagnostics.Error("actor-icon:" + actorId, "专用按钮图标尚未完成: ui/ActorIcons/" + actorId);
        return null;
    }

    internal static void ConfigurePlacementButton(PowerButton button)
    {
        if (button == null) return;
        button.transform.localScale = Vector3.one;
        if (button.icon == null) return;
        button.icon.preserveAspect = true;
        button.icon.rectTransform.localScale = Vector3.one;
    }

    internal static float DisplayScale(string formPath, float previousScale)
    {
        MclslActorAnimationCatalog.Form profile = MclslActorAnimationCatalog.Get(formPath);
        return profile?.ScaleApproved == true && profile.Scale > 0f ? profile.Scale : previousScale;
    }


}
