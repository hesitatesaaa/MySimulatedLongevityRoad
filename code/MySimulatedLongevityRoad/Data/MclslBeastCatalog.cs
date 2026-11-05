using System;
using System.Collections.Generic;

namespace MySimulatedLongevityRoad.Data;

internal enum MclslBeastSurface : byte { Land, Water }

internal sealed class MclslBeastDefinition
{
    internal readonly string Id;
    internal readonly string Name;
    internal readonly string AnimalTemplate;
    internal readonly MclslBeastSurface Surface;
    internal readonly string PopulationTrait;
    internal readonly string AssetPrefix;
    internal readonly string ResourceFolder;

    internal string BeastAssetId => AssetPrefix + Id;
    internal string AscendedAssetId => BeastAssetId + "_yuan";

    internal MclslBeastDefinition(string id, string name, string animalTemplate,
        MclslBeastSurface surface, bool minimal,
        string assetPrefix = "mclsl_beast_", string resourceFolder = null)
    {
        Id = id;
        Name = name;
        AnimalTemplate = animalTemplate;
        Surface = surface;
        PopulationTrait = minimal ? "population_minimal" : "population_small";
        AssetPrefix = assetPrefix;
        ResourceFolder = resourceFolder ?? "actors/Beasts/" + id;
    }
}

internal static class MclslBeastCatalog
{
    internal static readonly MclslBeastDefinition[] All =
    {
        new("kunpeng", "鲲鹏", "piranha", MclslBeastSurface.Water, true),
        new("qinglong", "青龙", "turtle", MclslBeastSurface.Water, true),
        new("huanglong", "黄龙", "turtle", MclslBeastSurface.Land, true),
        new("zhuque", "朱雀", "butterfly", MclslBeastSurface.Land, true),
        new("baihu", "白虎", "wolf", MclslBeastSurface.Land, true),
        new("qilin", "麒麟", "sheep", MclslBeastSurface.Land, true),
        new("moqilin", "墨麒麟", "sheep", MclslBeastSurface.Land, true),
        new("xuanwu", "玄武", "turtle", MclslBeastSurface.Land, true),
        new("fenghuang", "凤凰", "butterfly", MclslBeastSurface.Land, true),
        new("dafeng", "大风", "butterfly", MclslBeastSurface.Land, false),
        new("manman", "蛮蛮", "butterfly", MclslBeastSurface.Land, false),
        new("xuanniao", "玄鸟", "butterfly", MclslBeastSurface.Land, true),
        new("qingluan", "青鸾", "butterfly", MclslBeastSurface.Land, false),
        new("changliao", "长獠凶犬", "wolf", MclslBeastSurface.Land, false),
        new("mo", "貘", "sheep", MclslBeastSurface.Land, false),
        new("hongmao", "红毛异兽", "wolf", MclslBeastSurface.Land, false),
        new("feiqiu", "飞球兽", "sheep", MclslBeastSurface.Land, false),
        new("yingying", "阴影兽", "wolf", MclslBeastSurface.Land, true),
        new("goushou", "狗首妖兽", "wolf", MclslBeastSurface.Land, true),
        new("liejiejing", "裂解鲸", "piranha", MclslBeastSurface.Water, true),
        new("tianshou", "兲兽", "sheep", MclslBeastSurface.Land, true)
    };

    // Named beast lineages are manual entries.
    internal static readonly MclslBeastDefinition[] Named =
    {
        new("diyi", "帝一", "sheep", MclslBeastSurface.Land, true,
            "mclsl_named_", "actors/Named/DiYi"),
        new("disanmo", "帝叁貘", "sheep", MclslBeastSurface.Land, true,
            "mclsl_named_", "actors/Named/DiSanMo"),
        new("qingshenlong", "青神龙", "turtle", MclslBeastSurface.Water, true,
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
