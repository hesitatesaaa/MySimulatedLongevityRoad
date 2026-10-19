using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Profiling;

namespace MySimulatedLongevityRoad.Systems;

// Explicit environment opt-in for real Unity decoder/cache measurements.
// This does not certify placed-unit geometry or population performance.
internal static class MclslNativeFrameProbe
{
    private static readonly string Output = Environment.GetEnvironmentVariable("MCLSL_NATIVE_FRAME_PROBE");
    private static bool _done;
    private static double _start = -1, _ready = -1;
    private static object _baseline;
    private static JObject _report;
    private static double _releaseAt = -1;
    private static long? Available(long value) => value > 0 ? value : (long?)null;
    private static object Memory() => new
    {
        unityAllocatedBytes = Profiler.GetTotalAllocatedMemoryLong(),
        unityReservedBytes = Profiler.GetTotalReservedMemoryLong(),
        graphicsDriverBytes = Available(Profiler.GetAllocatedMemoryForGraphicsDriver()),
        managedBytes = GC.GetTotalMemory(false)
    };

    internal static void Tick(IEnumerable<string> keys, Func<string, Sprite> request, Action clear, long residentBytes, int pending)
    {
        if (_done || string.IsNullOrWhiteSpace(Output)) return;
        try
        {
            double now = Time.realtimeSinceStartup;
            // Do not compare memory while the game is still loading mods/world data.
            if (_start < 0 && now < 120) return;
            if (_releaseAt >= 0)
            {
                if (now - _releaseAt < 15) return;
                _report["afterRelease"] = JObject.FromObject(Memory());
                _report["cacheResidentBytesAfterRelease"] = residentBytes;
                _report["phase"] = "released";
                File.WriteAllText(Output, _report.ToString());
                _done = true;
                Debug.Log("[MCLSL native probe] released measurement saved " + Output);
                return;
            }
            if (_start < 0) { _start = now; _baseline = Memory(); }
            var frames = new List<object>();
            int total = 0, loaded = 0;
            long actualTextureBytes = 0;
            foreach (string key in keys)
            {
                total++;
                Sprite sprite = request(key);
                if (sprite == null) continue;
                loaded++;
                Texture2D t = sprite.texture;
                long bytes = Profiler.GetRuntimeMemorySizeLong(t);
                actualTextureBytes += bytes;
                frames.Add(new { resource = key, width = t.width, height = t.height,
                    mipmaps = t.mipmapCount, readable = t.isReadable, format = t.format.ToString(),
                    filter = t.filterMode.ToString(),
                    textureBytes = bytes, ppu = sprite.pixelsPerUnit,
                    spriteBounds = new[] { sprite.bounds.size.x, sprite.bounds.size.y } });
            }
            if (total == 0) return;
            if (loaded == total && _ready < 0) _ready = now;
            if ((_ready < 0 || now - _ready < 15) && now - _start < 120) return;
            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(Output)));
            _report = JObject.FromObject(new
            {
                kind = "unity-native-texture-probe", capturedAtUtc = DateTime.UtcNow,
                unityVersion = Application.unityVersion, device = SystemInfo.graphicsDeviceName,
                graphicsMemoryMiB = SystemInfo.graphicsMemorySize,
                totalFrames = total, loadedFrames = loaded, pendingFrames = pending,
                cacheResidentBytes = residentBytes, actualTextureBytes = Available(actualTextureBytes),
                elapsedSeconds = now - _start, steadySeconds = _ready < 0 ? 0 : now - _ready,
                baseline = _baseline, loaded = Memory(), frames,
                phase = "loaded", profilerZeroMeansUnavailable = true,
                placedUnitSizeVerified = false, populationPerformanceVerified = false
            });
            File.WriteAllText(Output, _report.ToString());
            clear();
            _releaseAt = now;
            Debug.Log("[MCLSL native probe] saved " + Output);
        }
        catch (Exception ex) { _done = true; Debug.LogWarning("[MCLSL native probe] " + ex); }
    }
}
