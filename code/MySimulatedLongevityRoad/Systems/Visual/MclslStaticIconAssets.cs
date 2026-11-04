using System.IO;
using UnityEngine;

namespace MySimulatedLongevityRoad.Systems;

// Nearest-neighbour sampling for the mod's static pixel icons so 64px art shown
// at 25/28/32 stays crisp. Covers exactly the four icon roots (153 icons):
// trait(76) + ui/CareerSkills(30) + ui/Spells(26) + ui/BeastTalents(21).
internal static class MclslStaticIconAssets
{
    private static readonly string[] Roots =
    {
        "GameResources/trait",
        "GameResources/ui/CareerSkills",
        "GameResources/ui/Spells",
        "GameResources/ui/BeastTalents"
    };

    internal static void Init()
    {
        string folder = MclslMod.I.GetDeclaration().FolderPath;
        foreach (string root in Roots)
        {
            string keyRoot = root.Substring("GameResources/".Length);
            foreach (string file in Directory.GetFiles(Path.Combine(folder, root), "*.png"))
            {
                Sprite sprite = SpriteTextureLoader.getSprite(keyRoot + "/" + Path.GetFileNameWithoutExtension(file));
                if (sprite != null)
                {
                    sprite.texture.filterMode = FilterMode.Point;
                    sprite.texture.wrapMode = TextureWrapMode.Clamp;
                    sprite.texture.anisoLevel = 0;
                }
            }
        }
    }
}
