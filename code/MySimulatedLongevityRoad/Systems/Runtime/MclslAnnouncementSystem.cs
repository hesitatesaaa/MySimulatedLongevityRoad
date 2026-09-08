using System;
using System.Collections.Generic;
using UnityEngine;

namespace MySimulatedLongevityRoad.Systems;

internal static class MclslAnnouncementSystem
{
    private const int MaxQueueSize = 24;
    private const float RepeatCooldownSeconds = 90f;
    private const float ShowIntervalSeconds = 2.4f;

    private sealed class PendingAnnouncement
    {
        internal string Text = string.Empty;
        internal string Color = "#D95B5B";
        internal float Duration = 8f;
        internal int EarliestFrame;
    }

    private static readonly Queue<PendingAnnouncement> Queue = new();
    private static readonly HashSet<string> PendingTexts = new(StringComparer.Ordinal);
    private static readonly Dictionary<string, float> LastShownByText = new(StringComparer.Ordinal);
    private static float _lastShown = -9999f;

    internal static void Enqueue(string text, string color = "#D95B5B", float duration = 8f, int delayFrames = 0)
    {
        string normalized = NormalizeTopText(text);
        if (string.IsNullOrWhiteSpace(normalized) || Queue.Count >= MaxQueueSize || PendingTexts.Contains(normalized)) return;
        if (LastShownByText.TryGetValue(normalized, out float last) && Time.unscaledTime - last < RepeatCooldownSeconds) return;
        Queue.Enqueue(new PendingAnnouncement
        {
            Text = normalized,
            Color = string.IsNullOrWhiteSpace(color) ? "#D95B5B" : color,
            Duration = Math.Clamp(duration, 4f, 15f),
            EarliestFrame = Time.frameCount + Math.Max(0, delayFrames)
        });
        PendingTexts.Add(normalized);
    }

    internal static void Tick()
    {
        if (Queue.Count == 0 || Time.unscaledTime - _lastShown < ShowIntervalSeconds) return;
        PendingAnnouncement pending = Queue.Peek();
        if (pending.EarliestFrame > Time.frameCount) return;
        Queue.Dequeue();
        PendingTexts.Remove(pending.Text);
        _lastShown = Time.unscaledTime;
        LastShownByText[pending.Text] = _lastShown;
        Cleanup();
        try { WorldTip.showNow(pending.Text, false, "top", pending.Duration, pending.Color); }
        catch { Debug.Log("[模拟长生路][公告] " + pending.Text); }
    }

    internal static void Clear()
    {
        Queue.Clear();
        PendingTexts.Clear();
        LastShownByText.Clear();
        _lastShown = -9999f;
    }

    private static void Cleanup()
    {
        if (LastShownByText.Count <= 120) return;
        List<string> expired = new();
        float cutoff = Time.unscaledTime - 240f;
        foreach (KeyValuePair<string, float> pair in LastShownByText)
            if (pair.Value < cutoff) expired.Add(pair.Key);
        for (int i = 0; i < expired.Count; i++) LastShownByText.Remove(expired[i]);
    }

    private static string NormalizeTopText(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return string.Empty;
        string value = text
            .Replace("\r", " ")
            .Replace("\n", " ")
            .Replace("｜", "，")
            .Replace("|", "，")
            .Trim();
        while (value.Contains("  ", StringComparison.Ordinal))
            value = value.Replace("  ", " ");
        const int maxLength = 58;
        return value.Length <= maxLength ? value : value.Substring(0, maxLength - 1) + "…";
    }
}
