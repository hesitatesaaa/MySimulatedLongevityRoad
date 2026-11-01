using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using MySimulatedLongevityRoad.Traits;
using UnityEngine;

namespace MySimulatedLongevityRoad.Systems;

// Xuanhuang-exclusive building shadows.
//
// Native loads build a full-frame silhouette in the main-thread entry
// DynamicSpriteCreator.createBuildingShadow and cache it in
// DynamicSpritesLibrary.building_shadows keyed by mainSprite.GetHashCode();
// the render thread only reads it via DynamicSprites.getShadowBuilding.
//
// For Xuanhuang-exclusive buildings we instead take the source silhouette's
// alpha bounding box and nearest-neighbour resample it into a target texture
// sized to the building's fundament (footprint). The target pixel size is
// fundment * pixelsPerUnit / scale_base, so with the sprite's PPU and a
// normalized pivot the rendered world rectangle equals the footprint; this
// compensates scale_base instead of abusing shadow_bound as a width hack.
// The cache is filled with the native addSprite(hash, sprite); the render read
// path is untouched. Shared/vanilla buildings keep the native generation.
internal static class MclslXuanhuangBuildingShadow
{
    private const string HarmonyId = "mclsl.xuanhuang.building_shadow";
    private static Harmony _harmony;
    private static readonly Dictionary<int, Held> Cache = new();

    private sealed class Held
    {
        public Sprite Sprite;
        public Texture2D Texture;
        // Inputs that fully determine the generated shadow. When all of them are
        // unchanged we reuse the existing sprite/texture instead of re-extracting
        // pixels, resampling and allocating a new resource.
        public BuildingAsset Building;
        public Sprite SourceSprite;
        public Rect SourceRect;
        public float PixelsPerUnit;
        public float FundamentWidth;
        public float FundamentHeight;
        public float FundamentLeft;
        public float FundamentBottom;
        public Vector3 ScaleBase;

        public bool Matches(BuildingAsset building, Sprite source)
        {
            if (!ReferenceEquals(Building, building) || !ReferenceEquals(SourceSprite, source)) return false;
            if (SourceRect != source.rect || PixelsPerUnit != source.pixelsPerUnit) return false;
            BuildingFundament fundament = building.fundament;
            return FundamentWidth == fundament.width && FundamentHeight == fundament.height
                && FundamentLeft == fundament.left && FundamentBottom == fundament.bottom
                && ScaleBase == building.scale_base;
        }
    }

    internal static void Init()
    {
        if (_harmony != null) return;
        _harmony = new Harmony(HarmonyId);
        MethodInfo target = AccessTools.Method(
            typeof(DynamicSpriteCreator),
            "createBuildingShadow",
            new[] { typeof(BuildingAsset), typeof(Sprite), typeof(bool) });
        MethodInfo prefix = AccessTools.Method(
            typeof(MclslXuanhuangBuildingShadow),
            nameof(CreateBuildingShadowPrefix));
        _harmony.Patch(target, prefix: new HarmonyMethod(prefix));
        MethodInfo clearTarget = AccessTools.Method(typeof(DynamicSpritesAsset), "clear", Type.EmptyTypes);
        MethodInfo clearPostfix = AccessTools.Method(
            typeof(MclslXuanhuangBuildingShadow),
            nameof(BuildingShadowsCleared));
        _harmony.Patch(clearTarget, postfix: new HarmonyMethod(clearPostfix));
    }

    // The native cache reset only clears its dictionary/atlas; release our
    // sprites and textures at the same lifecycle point so nothing leaks.
    private static void BuildingShadowsCleared(DynamicSpritesAsset __instance)
    {
        if (ReferenceEquals(__instance, DynamicSpritesLibrary.building_shadows)) Clear();
    }

    [HarmonyPriority(Priority.First)]
    private static bool CreateBuildingShadowPrefix(BuildingAsset __0, Sprite __1, bool __2)
    {
        if (__0.group != MclslXuanhuangRegistration.Id) return true;
        int hash = __1.GetHashCode();
        Held previous;
        Cache.TryGetValue(hash, out previous);
        if (previous != null && previous.Matches(__0, __1))
        {
            // Inputs unchanged: keep the existing resource, do no pixel work.
            DynamicSpritesLibrary.building_shadows.addSprite(hash, previous.Sprite);
            return false;
        }
        Held current = BuildFootprintShadow(__0, __1);
        Cache[hash] = current;
        DynamicSpritesLibrary.building_shadows.addSprite(hash, current.Sprite);
        if (previous != null) Release(previous);
        return false;
    }

    internal static void Clear()
    {
        foreach (Held held in Cache.Values) Release(held);
        Cache.Clear();
    }

    private static void Release(Held held)
    {
        if (held.Sprite != null)
        {
            MclslSpriteOwnership.Unregister(held.Sprite);
            UnityEngine.Object.Destroy(held.Sprite);
            held.Sprite = null;
        }
        if (held.Texture != null)
        {
            UnityEngine.Object.Destroy(held.Texture);
            held.Texture = null;
        }
    }

    private static Held BuildFootprintShadow(BuildingAsset building, Sprite mainSprite)
    {
        Rect rect = mainSprite.rect;
        int frameW = Mathf.RoundToInt(rect.width);
        int frameH = Mathf.RoundToInt(rect.height);
        int textureW = mainSprite.texture.width;
        Color32[] source = mainSprite.texture.GetPixels32();
        int originX = Mathf.RoundToInt(rect.x);
        int originY = Mathf.RoundToInt(rect.y);

        // 1) Source alpha bounding box in frame pixels (bottom-left origin).
        int minX = frameW, minY = frameH, maxX = -1, maxY = -1;
        for (int y = 0; y < frameH; y++)
        {
            int row = (originY + y) * textureW + originX;
            for (int x = 0; x < frameW; x++)
            {
                if (source[row + x].a == 0) continue;
                if (x < minX) minX = x;
                if (x > maxX) maxX = x;
                if (y < minY) minY = y;
                if (y > maxY) maxY = y;
            }
        }
        if (maxX < minX)
        {
            minX = 0;
            minY = 0;
            maxX = frameW - 1;
            maxY = frameH - 1;
        }
        int sourceW = maxX - minX + 1;
        int sourceH = maxY - minY + 1;

        // 2) Target size: world width/height must equal fundament after scale_base.
        BuildingFundament fundament = building.fundament;
        Vector3 scale = building.scale_base;
        float ppu = mainSprite.pixelsPerUnit;
        int targetW = Mathf.Max(1, Mathf.RoundToInt(fundament.width * ppu / scale.x));
        int targetH = Mathf.Max(1, Mathf.RoundToInt(fundament.height * ppu / scale.y));

        // 3) Nearest-neighbour resample the silhouette bbox onto the footprint texture.
        Color32 black = new Color32(0, 0, 0, 255);
        Color32 clear = new Color32(0, 0, 0, 0);
        Color32[] output = new Color32[targetW * targetH];
        for (int ty = 0; ty < targetH; ty++)
        {
            int sourceY = minY + Mathf.Min(sourceH - 1, (int)((ty + 0.5f) * sourceH / targetH));
            int row = (originY + sourceY) * textureW + originX;
            for (int tx = 0; tx < targetW; tx++)
            {
                int sourceX = minX + Mathf.Min(sourceW - 1, (int)((tx + 0.5f) * sourceW / targetW));
                output[ty * targetW + tx] = source[row + sourceX].a != 0 ? black : clear;
            }
        }

        Texture2D texture = null;
        try
        {
            texture = new Texture2D(targetW, targetH, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
                name = "mclsl_shadow_" + mainSprite.GetHashCode()
            };
            texture.SetPixels32(output);
            texture.Apply(false, false);

            // 4) Normalized pivot: world x of pixel px = (px - pivotPx)/ppu*scale,
            //    footprint spans [-left, -left+width]; pivotPx = left*ppu/scale, so
            //    normalized pivot = left/width (likewise bottom/height).
            Vector2 pivot = new Vector2((float)fundament.left / fundament.width, (float)fundament.bottom / fundament.height);
            Sprite sprite = Sprite.Create(
                texture,
                new Rect(0f, 0f, targetW, targetH),
                pivot,
                ppu,
                0,
                SpriteMeshType.FullRect);
            sprite.name = mainSprite.name + "_mclsl_shadow";
            MclslSpriteOwnership.Register(sprite);
            return new Held
            {
                Sprite = sprite,
                Texture = texture,
                Building = building,
                SourceSprite = mainSprite,
                SourceRect = rect,
                PixelsPerUnit = ppu,
                FundamentWidth = fundament.width,
                FundamentHeight = fundament.height,
                FundamentLeft = fundament.left,
                FundamentBottom = fundament.bottom,
                ScaleBase = scale
            };
        }
        catch
        {
            // Never orphan a texture if sprite creation fails midway.
            if (texture != null) UnityEngine.Object.Destroy(texture);
            throw;
        }
    }
}
