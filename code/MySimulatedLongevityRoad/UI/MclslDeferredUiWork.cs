using System.Collections.Generic;
using UnityEngine;

namespace MySimulatedLongevityRoad.UI;

internal static class MclslDeferredUiWork
{
    private static readonly Dictionary<MclslBackpackTabElement, int> Pending = new();
    private static readonly List<MclslBackpackTabElement> Ready = new();
    internal static void Schedule(MclslBackpackTabElement element) => Pending[element] = Time.frameCount;
    internal static void Cancel(MclslBackpackTabElement element) => Pending.Remove(element);
    internal static void Tick()
    {
        Ready.Clear();
        foreach (var pair in Pending)
            if (pair.Key == null || Time.frameCount > pair.Value) Ready.Add(pair.Key);
        foreach (var element in Ready)
        {
            Pending.Remove(element);
            if (element != null) element.CompleteDeferredCleanup();
        }
        Ready.Clear();
    }
}
