using System;
using MySimulatedLongevityRoad.Systems;
using UnityEngine;

namespace MySimulatedLongevityRoad.Core;

internal static class MclslMemoryMaintenance
{
    private static float _next;

    internal static void Tick()
    {
        if (Time.realtimeSinceStartup < _next) return;
        _next = Time.realtimeSinceStartup + 30f;
        MclslNativeActorFrames.TrimCold();
    }

    internal static void CleanManual()
    {
        long before = MclslNativeActorFrames.ResidentBytes;
        int count = MclslNativeActorFrames.TrimCold(64);
        long released = Math.Max(0, before - MclslNativeActorFrames.ResidentBytes);
        WorldTip.showNow("清理完成：释放闲置动画帧 " + count + " 张，贴图缓存减少 "
            + (released / 1048576d).ToString("F2") + " MiB。保留正在使用的资源。",
            false, "top", 8f, "#9CD7FF");
        // Destroy is deferred by Unity. Cache accounting is not a CPU/GPU memory reading.
    }
}
