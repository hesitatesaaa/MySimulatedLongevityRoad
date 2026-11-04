using System;
using UnityEngine;
using MySimulatedLongevityRoad.Core;
using MySimulatedLongevityRoad.Data;
using MySimulatedLongevityRoad.Systems;

namespace MySimulatedLongevityRoad.Core;

internal static class MclslSpriteLoader
{
    internal static Sprite TryLoad(string path)
    {
        try
        {
            return SpriteTextureLoader.getSprite(path)
                ?? SpriteTextureLoader.getSprite("GameResources/" + path)
                ?? Resources.Load<Sprite>("GameResources/" + path)
                ?? Resources.Load<Sprite>(path);
        }
        catch
        {
            return null;
        }
    }
}
