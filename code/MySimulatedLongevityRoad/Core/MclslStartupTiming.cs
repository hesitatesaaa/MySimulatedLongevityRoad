using System;
using System.Collections.Generic;
using System.Diagnostics;
using Newtonsoft.Json;
using UnityEngine;

namespace MySimulatedLongevityRoad.Core;

// One bounded report per startup; detailed timing does not require the Unity
// profiler or enable the much heavier runtime diagnostics overlay.
internal static class MclslStartupTiming
{
    private static readonly Dictionary<string, double> Milliseconds = new(StringComparer.Ordinal);
    internal static Scope Measure(string name) => new(name);
    internal readonly struct Scope : IDisposable
    {
        private readonly string _name;
        private readonly long _start;
        private readonly MclslUnityProfiler.Scope _profile;
        internal Scope(string name)
        {
            _name = name;
            _start = Stopwatch.GetTimestamp();
            _profile = MclslUnityProfiler.Sample(name);
        }
        public void Dispose()
        {
            _profile.Dispose();
            Milliseconds[_name] = (Stopwatch.GetTimestamp() - _start) * 1000d / Stopwatch.Frequency;
            if (_name == "MCLS/Load/OnModLoad")
                UnityEngine.Debug.Log("[MCLSL/Startup/ms] " + JsonConvert.SerializeObject(Milliseconds));
        }
    }
}
