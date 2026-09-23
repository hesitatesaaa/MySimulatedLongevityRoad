using System;
using System.Text;
using UnityEngine;

namespace MySimulatedLongevityRoad.Systems.Visual;

internal static class MclslWorldSpriteRenderLayer
{
    private const string TemplateEffectId = "fx_slash";
    private const int NativeResolveRetryFrames = 30;
    private const int BackLayerResolveRetryFrames = 90;
    private const int BackMaterialQueueOffset = 10;
    private static readonly Vector2 HiddenTemplatePosition = new(-10000f, -10000f);
    private static readonly string[] RejectedBackLayerTokens =
    {
        "effect", "fx", "top", "front", "foreground", "overlay", "ui", "icon",
        "status", "weather", "cloud", "particle", "projectile", "spell", "light",
        "cursor", "selection", "debug", "screen", "text"
    };

    private static EffectLayerDescriptor _effectLayer;
    private static bool _hasEffectLayer;
    private static int _lastEffectResolveFrame = -10000;
    private static BackLayerDescriptor _backLayer;
    private static bool _hasBackLayer;
    private static int _lastBackLayerResolveFrame = -10000;
    private static Material _backMaterial;
    private static int _lastRedrawResetFrame = -10000;

    internal static bool TrySpawnActorBackEffect(
        Vector2 position,
        float scale,
        int relativeSortingOrder,
        out BaseEffect effect,
        out SpriteRenderer renderer)
    {
        effect = null;
        renderer = null;
        try
        {
            if (!TryResolveNativeEffectLayer(out EffectLayerDescriptor effectLayer)
                || !TryResolveBackLayer(in effectLayer, out BackLayerDescriptor backLayer))
            {
                return false;
            }

            effect = EffectsLibrary.spawnAt(TemplateEffectId, position, scale <= 0f ? 1f : scale);
            if ((UnityEngine.Object)(object)effect == (UnityEngine.Object)null) return false;

            Component component = effect as Component;
            renderer = effect.sprite_renderer;
            if (renderer == null && component != null)
            {
                renderer = component.GetComponent<SpriteRenderer>()
                    ?? component.GetComponentInChildren<SpriteRenderer>(true);
            }
            if (renderer == null)
            {
                effect.kill();
                effect = null;
                return false;
            }

            if (component != null)
            {
                SpriteAnimation animation = component.GetComponent<SpriteAnimation>();
                if (animation != null)
                {
                    animation.looped = true;
                    animation.returnToPool = false;
                }
            }

            ApplyBackMaterial(renderer, in effectLayer);
            ConfigureActorBackSorting(renderer, in backLayer, relativeSortingOrder);
            renderer.enabled = true;
            return true;
        }
        catch
        {
            try { effect?.kill(); } catch (System.Exception mclslEmptyCatchEx) { MySimulatedLongevityRoad.Core.MclslDiagnostics.Error("empty-catch-code-MySimulatedLongevityRoad-Systems-Visual-MclslWorldSpriteRenderLayer-cs-1", "空 catch 捕获: code/MySimulatedLongevityRoad/Systems/Visual/MclslWorldSpriteRenderLayer.cs #1: " + mclslEmptyCatchEx.Message); }
            effect = null;
            renderer = null;
            return false;
        }
    }

    internal static bool TrySpawnStaticBackSprite(
        Vector2 position,
        float scale,
        int relativeSortingOrder,
        Sprite sprite,
        out BaseEffect effect,
        out SpriteRenderer renderer)
    {
        effect = null;
        renderer = null;
        if (sprite == null) return false;
        if (!TrySpawnActorBackEffect(position, scale, relativeSortingOrder, out effect, out renderer)) return false;
        try
        {
            StopNativeAnimator(effect);
            renderer.sprite = sprite;
            renderer.enabled = true;
            return true;
        }
        catch
        {
            try { effect?.kill(); } catch { }
            effect = null;
            renderer = null;
            return false;
        }
    }

    internal static bool TryCreateStaticWorldSprite(
        Vector2 position,
        float scale,
        int relativeSortingOrder,
        Sprite sprite,
        out GameObject gameObject,
        out SpriteRenderer renderer)
    {
        gameObject = null;
        renderer = null;
        if (sprite == null) return false;

        try
        {
            gameObject = new GameObject("mclsl_static_world_sprite", typeof(SpriteRenderer));
            if (World.world != null)
                gameObject.transform.SetParent(((Component)World.world).transform, true);

            gameObject.transform.position = new Vector3(position.x, position.y, 0f);
            gameObject.transform.localScale = new Vector3(scale <= 0f ? 1f : scale, scale <= 0f ? 1f : scale, 1f);
            gameObject.hideFlags = HideFlags.DontSave;

            renderer = gameObject.GetComponent<SpriteRenderer>();
            if (renderer == null) return false;
            renderer.sprite = sprite;
            renderer.color = Color.white;
            ConfigureStaticWorldSorting(renderer, relativeSortingOrder);
            renderer.enabled = true;
            gameObject.SetActive(true);
            RequestRedraw();
            return true;
        }
        catch
        {
            if (gameObject != null) UnityEngine.Object.Destroy(gameObject);
            gameObject = null;
            renderer = null;
            return false;
        }
    }

    internal static void StopNativeAnimator(BaseEffect effect)
    {
        try
        {
            Component component = effect as Component;
            SpriteAnimation animation = component != null ? component.GetComponent<SpriteAnimation>() : null;
            if (animation != null)
            {
                animation.isOn = false;
                animation.looped = false;
                animation.returnToPool = false;
                if (animation is Behaviour behaviour) behaviour.enabled = false;
            }
            Behaviour animator = component != null ? component.GetComponent("SpriteAnimator") as Behaviour : null;
            if (animator != null) animator.enabled = false;
        }
        catch { }
    }

    internal static bool TryBindLoopAnimation(BaseEffect effect, Sprite[] frames, float frameIntervalSeconds, int startFrame)
    {
        if ((UnityEngine.Object)(object)effect == (UnityEngine.Object)null
            || frames == null
            || frames.Length == 0)
        {
            return false;
        }

        try
        {
            Component component = effect as Component;
            if (component == null) return false;
            SpriteAnimation animation = component.GetComponent<SpriteAnimation>();
            if (animation == null) return false;

            animation.setFrames(frames);
            animation.timeBetweenFrames = frameIntervalSeconds > 0f ? frameIntervalSeconds : 0.08f;
            animation.looped = true;
            animation.returnToPool = false;
            animation.currentFrameIndex = Math.Abs(startFrame) % frames.Length;
            animation.isOn = true;

            SpriteRenderer renderer = effect.sprite_renderer
                ?? component.GetComponent<SpriteRenderer>()
                ?? component.GetComponentInChildren<SpriteRenderer>(true);
            if (renderer != null)
            {
                renderer.sprite = frames[Math.Abs(startFrame) % frames.Length];
                renderer.enabled = true;
            }

            RequestRedraw();
            return true;
        }
        catch
        {
            return false;
        }
    }

    internal static void ResetActorLayerCache()
    {
        _effectLayer = default;
        _hasEffectLayer = false;
        _lastEffectResolveFrame = -10000;
        _backLayer = default;
        _hasBackLayer = false;
        _lastBackLayerResolveFrame = -10000;
        _lastRedrawResetFrame = -10000;

        if (_backMaterial != null)
        {
            try { UnityEngine.Object.Destroy(_backMaterial); } catch (System.Exception mclslEmptyCatchEx) { MySimulatedLongevityRoad.Core.MclslDiagnostics.Error("empty-catch-code-MySimulatedLongevityRoad-Systems-Visual-MclslWorldSpriteRenderLayer-cs-3", "空 catch 捕获: code/MySimulatedLongevityRoad/Systems/Visual/MclslWorldSpriteRenderLayer.cs #3: " + mclslEmptyCatchEx.Message); }
            _backMaterial = null;
        }
    }

    private static void ConfigureActorBackSorting(SpriteRenderer renderer, in BackLayerDescriptor backLayer, int relativeSortingOrder)
    {
        if (renderer == null) return;
        renderer.sortingLayerID = backLayer.SortingLayerId;
        renderer.sortingOrder = Math.Min(-1, relativeSortingOrder);
    }

    private static void RequestRedraw()
    {
        if (World.world == null) return;
        int frame = Time.frameCount;
        if (frame == _lastRedrawResetFrame) return;
        _lastRedrawResetFrame = frame;
        try { World.world.resetRedrawTimer(); }
        catch (System.Exception exception) { MySimulatedLongevityRoad.Core.MclslDiagnostics.Error("world-sprite-redraw", exception.Message); }
    }

    private static void ConfigureStaticWorldSorting(SpriteRenderer renderer, int relativeSortingOrder)
    {
        if (renderer == null) return;

        SortingLayer[] layers;
        try { layers = SortingLayer.layers; }
        catch { layers = null; }

        string[] preferredLayers = { "EffectsBack", "Objects", "MapOverlay" };
        if (layers != null)
        {
            for (int preferredIndex = 0; preferredIndex < preferredLayers.Length; preferredIndex++)
            {
                for (int layerIndex = 0; layerIndex < layers.Length; layerIndex++)
                {
                    if (!string.Equals(layers[layerIndex].name, preferredLayers[preferredIndex], StringComparison.Ordinal)) continue;
                    renderer.sortingLayerID = layers[layerIndex].id;
                    renderer.sortingOrder = Math.Min(-1, relativeSortingOrder);
                    return;
                }
            }
        }

        try
        {
            SpriteRenderer worldRenderer = World.world?.GetComponent<SpriteRenderer>();
            if (worldRenderer != null) renderer.sortingLayerID = worldRenderer.sortingLayerID;
        }
        catch { }
        renderer.sortingOrder = Math.Min(-1, relativeSortingOrder);
    }

    private static void ApplyBackMaterial(SpriteRenderer renderer, in EffectLayerDescriptor effectLayer)
    {
        if (renderer == null || effectLayer.SharedMaterial == null) return;
        if (_backMaterial == null)
        {
            _backMaterial = new Material(effectLayer.SharedMaterial)
            {
                name = "MclslActorBackEffectMaterial",
                hideFlags = HideFlags.HideAndDontSave
            };
            int queue = effectLayer.MaterialQueue;
            if (queue >= 3000) _backMaterial.renderQueue = queue - BackMaterialQueueOffset;
        }
        renderer.sharedMaterial = _backMaterial;
    }

    private static bool TryResolveBackLayer(in EffectLayerDescriptor effectLayer, out BackLayerDescriptor descriptor)
    {
        descriptor = default;
        if (_hasBackLayer)
        {
            descriptor = _backLayer;
            return true;
        }

        int frame = Time.frameCount;
        if (frame - _lastBackLayerResolveFrame < BackLayerResolveRetryFrames) return false;
        _lastBackLayerResolveFrame = frame;

        SortingLayer[] layers;
        try { layers = SortingLayer.layers; }
        catch { return false; }
        if (layers == null || layers.Length == 0) return false;

        int nativeValue = effectLayer.SortingLayerValue;
        int bestScore = int.MinValue;
        SortingLayer best = default;
        bool found = false;
        StringBuilder snapshot = new(160);

        for (int i = 0; i < layers.Length; i++)
        {
            SortingLayer layer = layers[i];
            int value;
            try { value = SortingLayer.GetLayerValueFromID(layer.id); }
            catch { value = i; }

            if (snapshot.Length > 0) snapshot.Append(',');
            snapshot.Append(layer.name).Append('(').Append(value).Append(')');

            if (layer.id == effectLayer.SortingLayerId || value >= nativeValue) continue;
            string name = layer.name ?? string.Empty;
            if (ContainsRejectedToken(name)) continue;

            int score = ScoreBackLayerName(name) + value;
            if (score <= bestScore) continue;
            bestScore = score;
            best = layer;
            found = true;
        }

        if (!found)
        {
            int highestValue = int.MinValue;
            for (int i = 0; i < layers.Length; i++)
            {
                SortingLayer layer = layers[i];
                int value;
                try { value = SortingLayer.GetLayerValueFromID(layer.id); }
                catch { value = i; }
                if (layer.id == effectLayer.SortingLayerId || value >= nativeValue || value <= highestValue) continue;
                highestValue = value;
                best = layer;
                found = true;
            }
        }

        if (!found) return false;

        int bestValue;
        try { bestValue = SortingLayer.GetLayerValueFromID(best.id); }
        catch { bestValue = 0; }

        _backLayer = new BackLayerDescriptor
        {
            SortingLayerId = best.id,
            SortingLayerValue = bestValue,
            LayerSnapshot = snapshot.ToString()
        };
        _hasBackLayer = true;
        descriptor = _backLayer;
        return true;
    }

    private static int ScoreBackLayerName(string layerName)
    {
        string name = (layerName ?? string.Empty).ToLowerInvariant();
        int score = 0;
        if (name.Contains("actor") || name.Contains("unit")) score += 100000;
        if (name.Contains("creature") || name.Contains("character") || name.Contains("living")) score += 90000;
        if (name.Contains("object") || name.Contains("entity")) score += 70000;
        if (name.Contains("world") || name.Contains("game") || name.Contains("map")) score += 50000;
        if (name.Contains("building") || name.Contains("structure")) score += 20000;
        if (name.Contains("default")) score += 1000;
        return score;
    }

    private static bool ContainsRejectedToken(string layerName)
    {
        string name = (layerName ?? string.Empty).ToLowerInvariant();
        for (int i = 0; i < RejectedBackLayerTokens.Length; i++)
        {
            if (name.Contains(RejectedBackLayerTokens[i])) return true;
        }
        return false;
    }

    private static bool TryResolveNativeEffectLayer(out EffectLayerDescriptor descriptor)
    {
        descriptor = default;
        if (_hasEffectLayer)
        {
            descriptor = _effectLayer;
            return true;
        }

        int frame = Time.frameCount;
        if (frame - _lastEffectResolveFrame < NativeResolveRetryFrames) return false;
        _lastEffectResolveFrame = frame;

        BaseEffect template = null;
        try
        {
            template = EffectsLibrary.spawnAt(TemplateEffectId, HiddenTemplatePosition, 0.01f);
            if ((UnityEngine.Object)(object)template == (UnityEngine.Object)null) return false;

            Component component = template as Component;
            SpriteRenderer source = template.sprite_renderer;
            if (source == null && component != null)
            {
                source = component.GetComponent<SpriteRenderer>()
                    ?? component.GetComponentInChildren<SpriteRenderer>(true);
            }
            if (source == null) return false;

            int layerValue;
            try { layerValue = SortingLayer.GetLayerValueFromID(source.sortingLayerID); }
            catch { layerValue = 0; }

            _effectLayer = new EffectLayerDescriptor
            {
                SortingLayerId = source.sortingLayerID,
                SortingLayerValue = layerValue,
                MaterialQueue = source.sharedMaterial != null ? source.sharedMaterial.renderQueue : -1,
                SharedMaterial = source.sharedMaterial
            };
            _hasEffectLayer = true;
            descriptor = _effectLayer;
            return true;
        }
        catch
        {
            _hasEffectLayer = false;
            return false;
        }
        finally
        {
            try { template?.kill(); } catch (System.Exception mclslEmptyCatchEx) { MySimulatedLongevityRoad.Core.MclslDiagnostics.Error("empty-catch-code-MySimulatedLongevityRoad-Systems-Visual-MclslWorldSpriteRenderLayer-cs-4", "空 catch 捕获: code/MySimulatedLongevityRoad/Systems/Visual/MclslWorldSpriteRenderLayer.cs #4: " + mclslEmptyCatchEx.Message); }
        }
    }

    private struct EffectLayerDescriptor
    {
        internal int SortingLayerId;
        internal int SortingLayerValue;
        internal Material SharedMaterial;
        internal int MaterialQueue;
    }

    private struct BackLayerDescriptor
    {
        internal int SortingLayerId;
        internal int SortingLayerValue;
        internal string LayerSnapshot;
    }
}
