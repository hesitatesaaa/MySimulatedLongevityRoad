using System.Collections.Generic;
using UnityEngine;

namespace MySimulatedLongevityRoad.UI;

// The first holder captures the native state; intermediate window closures must
// not restore it while another mod window still owns the pause.
internal static class MclslWindowPause
{
    private static readonly HashSet<object> Holders = new();
    private static bool _paused;
    private static float _scale;
    internal static void Acquire(object owner)
    {
        if (Holders.Count == 0) { _paused = Config.paused; _scale = Time.timeScale; }
        Holders.Add(owner);
        Enforce();
    }
    internal static void Enforce() { Config.paused = true; Time.timeScale = 0f; }
    internal static void Release(object owner)
    {
        if (!Holders.Remove(owner) || Holders.Count != 0) return;
        Config.paused = _paused;
        Time.timeScale = _scale;
    }
}
