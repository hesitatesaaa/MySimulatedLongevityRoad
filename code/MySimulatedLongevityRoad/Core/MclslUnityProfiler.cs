using System;
using UnityEngine.Profiling;

namespace MySimulatedLongevityRoad.Core;

/// <summary>
/// Allocation-free scope for Unity Profiler samples. Calls are skipped when the
/// Unity Profiler is not recording, while named samples remain available in
/// both the normal and developer builds when profiling is enabled.
/// </summary>
internal static class MclslUnityProfiler
{
    internal static Scope Sample(string name)
    {
        if (!Profiler.enabled) return default;
        Profiler.BeginSample(name);
        return new Scope(true);
    }

    internal readonly struct Scope : IDisposable
    {
        private readonly bool _active;

        internal Scope(bool active) => _active = active;

        public void Dispose()
        {
            if (_active) Profiler.EndSample();
        }
    }
}
