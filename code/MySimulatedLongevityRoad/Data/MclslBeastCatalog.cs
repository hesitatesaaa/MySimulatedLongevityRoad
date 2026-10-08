using System;
using System.Collections.Generic;

namespace MySimulatedLongevityRoad.Data;

internal enum MclslBeastHabitat : byte
{
    DeepSea, RiverLake, Mountain, Volcano, MountainForest, ForestGrass,
    LakeWetlandCoast, BurntForest, RidgeCoast, WetForest, OldForestRuin,
    WildMountain, IslandCoast, GrassHill, RedMoon, CorruptBorder,
    SeaRift, HighMountain
}

internal sealed class MclslBeastDefinition
{
    internal readonly string Id;
    internal readonly string Name;
    internal readonly string AnimalTemplate;
    internal readonly MclslBeastHabitat Habitat;
    internal readonly int ChancePerMillion;
    internal readonly byte MinGroup;
    internal readonly byte MaxGroup;
    internal readonly string PopulationTrait;
    internal readonly string AssetPrefix;
    internal readonly string ResourceFolder;

    internal string BeastAssetId => AssetPrefix + Id;
    internal string AscendedAssetId => BeastAssetId + "_yuan";
    internal string ConfigId => "MCLSL_config_beast_" + Id + "_chance";

    internal MclslBeastDefinition(string id, string name, string animalTemplate,
        MclslBeastHabitat habitat, int chancePerMillion, byte minGroup, byte maxGroup, bool minimal,
        string assetPrefix = "mclsl_beast_", string resourceFolder = null)
    {
        Id = id;
        Name = name;
        AnimalTemplate = animalTemplate;
        Habitat = habitat;
        ChancePerMillion = chancePerMillion;
        MinGroup = minGroup;
        MaxGroup = maxGroup;
        PopulationTrait = minimal ? "population_minimal" : "population_small";
        AssetPrefix = assetPrefix;
        ResourceFolder = resourceFolder ?? "actors/Beasts/" + id;
    }
}

internal static class MclslBeastCatalog
{
    // One roll per world year and eligible species. Values are parts per million.
    internal static readonly MclslBeastDefinition[] All =
    {
        new("kunpeng", "鲲鹏", "piranha", MclslBeastHabitat.DeepSea, 200, 1, 1, true),
        new("qinglong", "青龙", "turtle", MclslBeastHabitat.RiverLake, 1000, 1, 1, true),
        new("huanglong", "黄龙", "turtle", MclslBeastHabitat.Mountain, 1200, 1, 1, true),
        new("zhuque", "朱雀", "butterfly", MclslBeastHabitat.Volcano, 800, 1, 1, true),
        new("baihu", "白虎", "wolf", MclslBeastHabitat.MountainForest, 1800, 1, 1, true),
        new("qilin", "麒麟", "sheep", MclslBeastHabitat.ForestGrass, 800, 1, 1, true),
        new("moqilin", "墨麒麟", "sheep", MclslBeastHabitat.ForestGrass, 100000, 1, 1, true),
        new("xuanwu", "玄武", "turtle", MclslBeastHabitat.LakeWetlandCoast, 1000, 1, 1, true),
        new("fenghuang", "凤凰", "butterfly", MclslBeastHabitat.BurntForest, 400, 1, 1, true),
        new("dafeng", "大风", "butterfly", MclslBeastHabitat.RidgeCoast, 8000, 1, 1, false),
        new("manman", "蛮蛮", "butterfly", MclslBeastHabitat.WetForest, 12000, 1, 2, false),
        new("xuanniao", "玄鸟", "butterfly", MclslBeastHabitat.OldForestRuin, 800, 1, 1, true),
        new("qingluan", "青鸾", "butterfly", MclslBeastHabitat.MountainForest, 3500, 1, 1, false),
        new("changliao", "长獠凶犬", "wolf", MclslBeastHabitat.WildMountain, 7500, 1, 2, false),
        new("mo", "貘", "sheep", MclslBeastHabitat.WetForest, 6000, 1, 1, false),
        new("hongmao", "红毛异兽", "wolf", MclslBeastHabitat.IslandCoast, 30000, 2, 3, false),
        new("feiqiu", "飞球兽", "sheep", MclslBeastHabitat.GrassHill, 40000, 1, 2, false),
        new("yingying", "阴影兽", "wolf", MclslBeastHabitat.RedMoon, 300000, 2, 3, true),
        new("goushou", "狗首妖兽", "wolf", MclslBeastHabitat.CorruptBorder, 1500, 1, 1, true),
        new("liejiejing", "裂解鲸", "piranha", MclslBeastHabitat.SeaRift, 500, 1, 1, true),
        // The separate 0.01% revival event gates this conditional 100% roll.
        new("tianshou", "兲兽", "sheep", MclslBeastHabitat.HighMountain, 1000000, 1, 1, true)
    };

    // Existing manual placement IDs are preserved for old saves. They are
    // beast lineages, but never enter the annual natural-spawn catalog.
    internal static readonly MclslBeastDefinition[] Named =
    {
        new("diyi", "帝一", "sheep", MclslBeastHabitat.ForestGrass, 0, 1, 1, true,
            "mclsl_named_", "actors/Named/DiYi"),
        new("disanmo", "帝叁貘", "sheep", MclslBeastHabitat.WetForest, 0, 1, 1, true,
            "mclsl_named_", "actors/Named/DiSanMo"),
        new("qingshenlong", "青神龙", "turtle", MclslBeastHabitat.RiverLake, 0, 1, 1, true,
            "mclsl_named_", "actors/Named/QingShenLong")
    };

    private static readonly Dictionary<string, MclslBeastDefinition> ByAsset = BuildAssetMap();

    internal static MclslBeastDefinition ForAsset(string assetId)
        => assetId != null && ByAsset.TryGetValue(assetId, out MclslBeastDefinition value) ? value : null;

    private static Dictionary<string, MclslBeastDefinition> BuildAssetMap()
    {
        var result = new Dictionary<string, MclslBeastDefinition>(StringComparer.Ordinal);
        foreach (MclslBeastDefinition beast in All)
        {
            result.Add(beast.BeastAssetId, beast);
            result.Add(beast.AscendedAssetId, beast);
        }
        foreach (MclslBeastDefinition beast in Named)
        {
            result.Add(beast.BeastAssetId, beast);
            result.Add(beast.AscendedAssetId, beast);
        }
        return result;
    }
}
