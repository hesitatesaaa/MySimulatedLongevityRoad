using System;
using System.Collections.Generic;
using MySimulatedLongevityRoad.Core;
using MySimulatedLongevityRoad.Data;
using MySimulatedLongevityRoad.Systems;
using UnityEngine;

namespace MySimulatedLongevityRoad.Systems.Visual;

internal static class MclslMapMarkerVisualSystem
{
    private const int RefreshCadenceFrames = 30;
    private const int AnimationCadenceFrames = 4;
    private const int MaxTemporaryVisuals = 48;
    // These are target world-unit sizes, not texture stretches. The runtime
    // converts them to a uniform transform scale from the sprite's bounds.
    private const float PersistentScale = 14.5f;
    private const float TemporaryScale = 11.2f;
    // Both layers are below the native effect layer and therefore below actors,
    // while remaining visible above the terrain. Temporary effects sit above
    // persistent location markers when the two occupy the same tile.
    private const int PersistentSortingOrder = -4;
    private const int TemporarySortingOrder = -3;
    private static readonly Dictionary<string, MarkerEntry> Entries = new(StringComparer.Ordinal);
    private static readonly Dictionary<string, Sprite> SpriteCache = new(StringComparer.Ordinal);
    private static readonly List<string> RemovalBuffer = new(64);
    private static readonly List<MarkerDescriptor> DesiredMarkers = new(96);
    private static readonly HashSet<string> DesiredKeys = new(StringComparer.Ordinal);
    private static int _lastRefreshFrame = -1;
    private static int _lastAnimationFrame = -1;
    private static int _lastDataRevision = -1;
    private static int _lastYear = -1;
    private static bool _needsReconcile;
    private static bool _assetValidationLogged;

    internal static void OnWorldLoaded()
    {
        Clear();
        _lastRefreshFrame = -1;
        _lastAnimationFrame = -1;
        _lastDataRevision = -1;
        _lastYear = -1;
    }

    internal static void Tick(int frame)
    {
        if (World.world == null) return;
        if (_lastRefreshFrame < 0 || frame - _lastRefreshFrame >= RefreshCadenceFrames)
        {
            _lastRefreshFrame = frame;
            ValidateAssetsOnce();
            int year = MclslRuntime.CurrentYear();
            int dataRevision = MclslWorldRunRepository.MapMarkerDataRevision;
            if (_lastDataRevision != dataRevision || _lastYear != year || _needsReconcile)
            {
                using (MclslUnityProfiler.Sample("MCLS/Visual/MapMarkerReconcile"))
                    _needsReconcile = !Reconcile(year);
                _lastDataRevision = dataRevision;
                _lastYear = year;
            }
        }

        if (_lastAnimationFrame < 0 || frame - _lastAnimationFrame >= AnimationCadenceFrames)
        {
            _lastAnimationFrame = frame;
            using (MclslUnityProfiler.Sample("MCLS/Visual/MapMarkerAnimation"))
                AnimateTemporaryMarkers(frame);
        }
    }

    internal static void Clear()
    {
        foreach (MarkerEntry entry in Entries.Values) DestroyEntry(entry);
        Entries.Clear();
        SpriteCache.Clear();
        RemovalBuffer.Clear();
        DesiredMarkers.Clear();
        DesiredKeys.Clear();
        _lastRefreshFrame = -1;
        _lastAnimationFrame = -1;
        _lastDataRevision = -1;
        _lastYear = -1;
        _needsReconcile = false;
        _assetValidationLogged = false;
    }

    private static bool Reconcile(int year)
    {
        DesiredMarkers.Clear();
        MclslWorldRunState run = MclslWorldRunRepository.Current;
        if (run != null)
        {
            AddPersistentRuinMarkers(run.SectRuins);
            AddTemporaryEventMarkers(run.Events, year);
        }

        DesiredKeys.Clear();
        bool complete = true;
        for (int i = 0; i < DesiredMarkers.Count; i++)
        {
            MarkerDescriptor descriptor = DesiredMarkers[i];
            DesiredKeys.Add(descriptor.Key);
            if (!Entries.TryGetValue(descriptor.Key, out MarkerEntry entry) || !IsUsable(entry))
            {
                entry = CreateEntry(descriptor);
                if (entry == null)
                {
                    complete = false;
                    continue;
                }
                Entries[descriptor.Key] = entry;
            }
            ApplyDescriptor(entry, descriptor);
        }

        RemovalBuffer.Clear();
        foreach (string key in Entries.Keys)
            if (!DesiredKeys.Contains(key)) RemovalBuffer.Add(key);
        for (int i = 0; i < RemovalBuffer.Count; i++)
        {
            string key = RemovalBuffer[i];
            if (Entries.TryGetValue(key, out MarkerEntry entry)) DestroyEntry(entry);
            Entries.Remove(key);
        }
        RemovalBuffer.Clear();
        return complete;
    }

    private static void AddPersistentRuinMarkers(List<MclslSectRuinRecord> ruins)
    {
        if (ruins == null) return;
        for (int i = 0; i < ruins.Count; i++)
        {
            MclslSectRuinRecord ruin = ruins[i];
            if (!ShouldShowRuin(ruin) || !IsValidCoordinate(ruin.MapX, ruin.MapY)) continue;
            string kind = IsSecretRuin(ruin) ? "secret_realm" : "ruin";
            float alpha = string.Equals(ruin.State, "残破", StringComparison.Ordinal) ? 0.62f
                : string.Equals(ruin.State, "探索中", StringComparison.Ordinal) ? 0.88f : 1f;
            DesiredMarkers.Add(new MarkerDescriptor(
                "ruin:" + ruin.Id,
                kind,
                ruin.MapX,
                ruin.MapY,
                PersistentScale,
                new Color(1f, 1f, 1f, alpha),
                false,
                PersistentSortingOrder));
        }
    }

    private static void AddTemporaryEventMarkers(List<MclslRunEventRecord> events, int year)
    {
        if (events == null) return;
        int added = 0;
        for (int i = events.Count - 1; i >= 0 && added < MaxTemporaryVisuals; i--)
        {
            MclslRunEventRecord record = events[i];
            if (record == null || string.IsNullOrWhiteSpace(record.MapVisualKind)
                || record.MapVisualEndYear < year || !IsValidCoordinate(record.MapX, record.MapY)) continue;
            string kind = NormalizeVisualKind(record.MapVisualKind);
            if (string.IsNullOrWhiteSpace(kind)) continue;
            DesiredMarkers.Add(new MarkerDescriptor(
                EventKey(record),
                kind,
                record.MapX,
                record.MapY,
                TemporaryScale,
                EventColor(kind),
                true,
                TemporarySortingOrder));
            added++;
        }
    }

    private static MarkerEntry CreateEntry(MarkerDescriptor descriptor)
    {
        Sprite sprite = LoadMarkerSprite(descriptor.Kind);
        if (sprite == null) return null;
        try
        {
            float worldScale = GetMarkerScale(sprite, descriptor.Scale);
            if (!MclslWorldSpriteRenderLayer.TryCreateStaticWorldSprite(
                    new Vector2(descriptor.X, descriptor.Y), worldScale, descriptor.SortingOrder, sprite,
                    out GameObject gameObject, out SpriteRenderer renderer)) return null;
            gameObject.name = "mclsl_map_marker_" + descriptor.Key.GetHashCode().ToString("x8");
            gameObject.hideFlags = HideFlags.DontSave;
            return new MarkerEntry(gameObject, gameObject.transform, renderer);
        }
        catch
        {
            return null;
        }
    }

    private static void ApplyDescriptor(MarkerEntry entry, MarkerDescriptor descriptor)
    {
        if (entry?.Transform == null || entry.Renderer == null) return;
        entry.Kind = descriptor.Kind;
        entry.Temporary = descriptor.Temporary;
        Sprite sprite = LoadMarkerSprite(descriptor.Kind);
        entry.BaseScale = GetMarkerScale(sprite, descriptor.Scale);
        entry.BaseColor = descriptor.Color;
        entry.Phase = StablePhase(descriptor.Key);
        entry.Transform.position = new Vector3(descriptor.X, descriptor.Y, entry.Transform.position.z);
        entry.Transform.localScale = new Vector3(entry.BaseScale, entry.BaseScale, 1f);
        entry.Renderer.sprite = sprite;
        entry.Renderer.color = descriptor.Color;
        entry.Renderer.enabled = true;
        entry.GameObject.SetActive(true);
    }

    private static void AnimateTemporaryMarkers(int frame)
    {
        foreach (MarkerEntry entry in Entries.Values)
        {
            if (entry == null || !entry.Temporary || entry.Transform == null || entry.Renderer == null) continue;
            float wave = 0.90f + 0.12f * (0.5f + 0.5f * Mathf.Sin((frame + entry.Phase) * 0.12f));
            float alpha = entry.BaseColor.a * (0.76f + 0.24f * (0.5f + 0.5f * Mathf.Sin((frame + entry.Phase) * 0.10f)));
            entry.Transform.localScale = new Vector3(entry.BaseScale * wave, entry.BaseScale * wave, 1f);
            entry.Renderer.color = new Color(entry.BaseColor.r, entry.BaseColor.g, entry.BaseColor.b, alpha);
        }
    }

    private static Sprite LoadMarkerSprite(string kind)
    {
        if (SpriteCache.TryGetValue(kind, out Sprite cached)) return cached;
        string path = "map/markers/" + kind;
        Sprite sprite = LoadSprite(path) ?? LoadSprite(path + ".png");
        if (sprite == null) return null;
        ConfigurePixelTexture(sprite);
        SpriteCache[kind] = sprite;
        return sprite;
    }

    private static Sprite LoadSprite(string path)
    {
        try
        {
            return SpriteTextureLoader.getSprite(path)
                ?? SpriteTextureLoader.getSprite("GameResources/" + path)
                ?? Resources.Load<Sprite>(path)
                ?? Resources.Load<Sprite>("GameResources/" + path);
        }
        catch { return null; }
    }

    private static void ConfigurePixelTexture(Sprite sprite)
    {
        try
        {
            Texture2D texture = sprite?.texture;
            if (texture == null) return;
            texture.filterMode = FilterMode.Point;
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.anisoLevel = 0;
        }
        catch { }
    }

    private static float GetMarkerScale(Sprite sprite, float targetWorldSize)
    {
        if (sprite == null || targetWorldSize <= 0f) return 1f;
        try
        {
            float sourceSize = Mathf.Max(sprite.bounds.size.x, sprite.bounds.size.y);
            return sourceSize > 0.0001f ? targetWorldSize / sourceSize : targetWorldSize;
        }
        catch { return targetWorldSize; }
    }

    private static void ValidateAssetsOnce()
    {
        if (_assetValidationLogged) return;
        _assetValidationLogged = true;
        string[] kinds = { "ruin", "secret_realm", "spiritual_convergence", "earthfire", "meteor_stone" };
        for (int i = 0; i < kinds.Length; i++)
            if (LoadMarkerSprite(kinds[i]) == null)
                Debug.LogWarning("[模拟长生路][地图贴图] 缺少资源 GameResources/map/markers/" + kinds[i] + ".png");
    }

    private static bool IsUsable(MarkerEntry entry) => entry?.Transform != null && entry.Renderer != null && entry.GameObject != null;

    private static void DestroyEntry(MarkerEntry entry)
    {
        try
        {
            if (entry?.GameObject != null) UnityEngine.Object.Destroy(entry.GameObject);
        }
        catch { }
    }

    private static bool ShouldShowRuin(MclslSectRuinRecord ruin)
    {
        if (ruin == null || ruin.RemainingValue <= 0) return false;
        return !string.Equals(ruin.State, "搜尽", StringComparison.Ordinal)
            && !string.Equals(ruin.State, "封绝", StringComparison.Ordinal)
            && !string.Equals(ruin.State, "崩毁", StringComparison.Ordinal)
            && !string.Equals(ruin.State, "沉寂", StringComparison.Ordinal);
    }

    private static bool IsSecretRuin(MclslSectRuinRecord ruin)
    {
        return ruin != null && ((ruin.Category ?? string.Empty).IndexOf("秘境", StringComparison.Ordinal) >= 0
            || (ruin.Name ?? string.Empty).IndexOf("秘境", StringComparison.Ordinal) >= 0
            || (ruin.Description ?? string.Empty).IndexOf("秘境", StringComparison.Ordinal) >= 0);
    }

    private static bool IsValidCoordinate(int x, int y) => x >= 0 && y >= 0 && x < MapBox.width && y < MapBox.height;

    private static string NormalizeVisualKind(string kind)
    {
        return kind switch
        {
            "ruin" => "ruin",
            "secret_realm" => "secret_realm",
            "spiritual_convergence" => "spiritual_convergence",
            "earthfire" => "earthfire",
            "meteor_stone" => "meteor_stone",
            _ => string.Empty
        };
    }

    private static Color EventColor(string kind)
    {
        return kind switch
        {
            "secret_realm" => new Color(0.78f, 0.70f, 1f, 0.96f),
            "spiritual_convergence" => new Color(0.72f, 1f, 0.84f, 0.96f),
            "earthfire" => new Color(1f, 0.58f, 0.42f, 0.96f),
            "meteor_stone" => new Color(0.68f, 0.82f, 1f, 0.96f),
            _ => new Color(1f, 0.86f, 0.55f, 0.96f)
        };
    }

    private static string EventKey(MclslRunEventRecord record)
    {
        return "event:" + record.Year + "|" + record.EventType + "|" + record.Title + "|" + record.Body;
    }

    private static int StablePhase(string value)
    {
        unchecked
        {
            int hash = 23;
            foreach (char c in value ?? string.Empty) hash = hash * 37 + c;
            return hash & int.MaxValue;
        }
    }

    private readonly struct MarkerDescriptor
    {
        internal MarkerDescriptor(string key, string kind, int x, int y, float scale, Color color, bool temporary, int sortingOrder)
        {
            Key = key;
            Kind = kind;
            X = x;
            Y = y;
            Scale = scale;
            Color = color;
            Temporary = temporary;
            SortingOrder = sortingOrder;
        }

        internal string Key { get; }
        internal string Kind { get; }
        internal int X { get; }
        internal int Y { get; }
        internal float Scale { get; }
        internal Color Color { get; }
        internal bool Temporary { get; }
        internal int SortingOrder { get; }
    }

    private sealed class MarkerEntry
    {
        internal MarkerEntry(GameObject gameObject, Transform transform, SpriteRenderer renderer)
        {
            GameObject = gameObject;
            Transform = transform;
            Renderer = renderer;
        }

        internal GameObject GameObject { get; }
        internal Transform Transform { get; }
        internal SpriteRenderer Renderer { get; }
        internal string Kind { get; set; } = string.Empty;
        internal bool Temporary { get; set; }
        internal float BaseScale { get; set; }
        internal Color BaseColor { get; set; }
        internal int Phase { get; set; }
    }
}
