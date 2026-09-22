using System;
using System.Collections.Generic;
using MySimulatedLongevityRoad.Data;
using MySimulatedLongevityRoad.Systems;
using UnityEngine;

namespace MySimulatedLongevityRoad.Systems.Visual;

internal static class MclslMapNodeMarkerSystem
{
    private sealed class Marker
    {
        internal BaseEffect Effect;
        internal SpriteRenderer Renderer;
        internal int X;
        internal int Y;
    }

    private static readonly Dictionary<string, Marker> Markers = new(StringComparer.Ordinal);
    private static int _lastRefreshFrame = -10000;

    internal static void TickFrame(int frameCounter)
    {
        if (frameCounter - _lastRefreshFrame < 45) return;
        _lastRefreshFrame = frameCounter;
        IReadOnlyList<MclslMapNodeRecord> nodes = MclslMapNodeSystem.SnapshotNodes();
        HashSet<string> live = new(StringComparer.Ordinal);
        int spawned = 0;
        for (int i = 0; i < nodes.Count && spawned < 64; i++)
        {
            MclslMapNodeRecord node = nodes[i];
            if (node == null || node.MapX < 0 || node.MapY < 0 || node.VisibilityState == "lost") continue;
            live.Add(node.MarkerKey);
            if (!Markers.TryGetValue(node.MarkerKey, out Marker marker) || marker == null || marker.Effect == null)
            {
                if (!MclslWorldSpriteRenderLayer.TrySpawnActorBackEffect(new Vector2(node.MapX, node.MapY), 0.7f, 0, out BaseEffect effect, out SpriteRenderer renderer))
                    continue;
                marker = new Marker { Effect = effect, Renderer = renderer, X = node.MapX, Y = node.MapY };
                Markers[node.MarkerKey] = marker;
                spawned++;
            }
            if (marker.Renderer != null) marker.Renderer.color = ColorFor(node);
        }

        List<string> stale = new();
        foreach (KeyValuePair<string, Marker> pair in Markers)
        {
            if (live.Contains(pair.Key)) continue;
            try { pair.Value?.Effect?.kill(); } catch { }
            stale.Add(pair.Key);
        }
        for (int i = 0; i < stale.Count; i++) Markers.Remove(stale[i]);
    }

    internal static void Clear()
    {
        foreach (Marker marker in Markers.Values)
        {
            try { marker?.Effect?.kill(); } catch { }
        }
        Markers.Clear();
        _lastRefreshFrame = -10000;
        MclslWorldSpriteRenderLayer.ResetActorLayerCache();
    }

    private static Color ColorFor(MclslMapNodeRecord node)
    {
        Color color = node.NodeType switch
        {
            MclslMapNodeSystem.Cave => new Color(0.35f, 0.72f, 0.82f, 0.95f),
            MclslMapNodeSystem.Ruin => new Color(0.86f, 0.72f, 0.35f, 0.95f),
            MclslMapNodeSystem.WorldChange => new Color(0.70f, 0.46f, 0.86f, 0.95f),
            _ => Color.white
        };
        if (node.LifecycleState == "contested") color = Color.Lerp(color, Color.red, 0.45f);
        if (node.LifecycleState == "depleted" || node.LifecycleState == "collapsed") color.a = 0.45f;
        return color;
    }
}
