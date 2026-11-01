using System;
using System.IO;
using System.Threading;
using UnityEngine;

namespace MySimulatedLongevityRoad.Core;

// World identity is persisted; Generation belongs to this process and invalidates
// work submitted by native workers before a load/clear boundary.
internal static class MclslWorldContext
{
    internal const string Version = "0.8.0";
    internal const string SavePrefix = "mclsl.080.";
    private static long _generation;
    internal static long Generation => Volatile.Read(ref _generation);
    internal static string WorldId { get; private set; } = string.Empty;
    internal static double Clock => World.world?.getCurWorldTime() ?? 0d;
    internal static string StorageRoot => Path.Combine(Application.persistentDataPath,
        "MySimulatedLongevityRoad", Version);

    internal static void Bind()
    {
        // A load is a boundary even if the native clear callback was skipped.
        Interlocked.Increment(ref _generation);
        World.world.map_stats.custom_data ??= new SaveCustomData();
        var data = World.world.map_stats.custom_data;
        data.get(SavePrefix + "world", out string id, string.Empty);
        if (id.Length == 0) { id = Guid.NewGuid().ToString("N"); data.set(SavePrefix + "world", id); }
        WorldId = id;
    }

    internal static void Clear()
    {
        Interlocked.Increment(ref _generation);
        WorldId = string.Empty;
    }
}
