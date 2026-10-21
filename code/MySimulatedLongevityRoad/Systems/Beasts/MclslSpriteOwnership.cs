using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace MySimulatedLongevityRoad.Systems;

// Reference identity avoids Unity's overloaded equality/GetInstanceID on render
// workers. Registration and removal happen only on the texture owner thread.
internal static class MclslSpriteOwnership
{
    private sealed class Identity : System.Collections.Generic.IEqualityComparer<Sprite>
    {
        public bool Equals(Sprite x, Sprite y) => ReferenceEquals(x, y);
        public int GetHashCode(Sprite value) => RuntimeHelpers.GetHashCode(value);
    }

    private static readonly ConcurrentDictionary<Sprite, byte> Sprites = new(new Identity());
    internal static void Register(Sprite sprite)
    {
        if (!ReferenceEquals(sprite, null)) Sprites.TryAdd(sprite, 0);
    }
    internal static void Unregister(Sprite sprite)
    {
        if (!ReferenceEquals(sprite, null)) Sprites.TryRemove(sprite, out _);
    }
    internal static bool Contains(Sprite sprite)
        => !ReferenceEquals(sprite, null) && Sprites.ContainsKey(sprite);
}
