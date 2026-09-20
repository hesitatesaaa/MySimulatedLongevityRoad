public class RarityLibrary : AssetLibrary<RarityAsset>
{
	public static RarityAsset normal;

	public static RarityAsset rare;

	public static RarityAsset epic;

	public static RarityAsset legendary;

	public override void init()
	{
		base.init();
		normal = add(new RarityAsset
		{
			id = Rarity.R0_Normal.ToString().ToLower(),
			color_hex = "#FFFFFF",
			material_path = string.Empty,
			rarity_trait_string = "trait_common"
		});
		rare = add(new RarityAsset
		{
			id = Rarity.R1_Rare.ToString().ToLower(),
			color_hex = "#66AFFF",
			material_path = "materials/ItemRare",
			rarity_trait_string = "trait_rare"
		});
		epic = add(new RarityAsset
		{
			id = Rarity.R2_Epic.ToString().ToLower(),
			color_hex = "#FFF15E",
			material_path = "materials/ItemEpic",
			rarity_trait_string = "trait_epic"
		});
		legendary = add(new RarityAsset
		{
			id = Rarity.R3_Legendary.ToString().ToLower(),
			color_hex = "#FF7028",
			material_path = "materials/ItemLegendary",
			rarity_trait_string = "trait_legendary"
		});
	}

	public override void linkAssets()
	{
		base.linkAssets();
		foreach (RarityAsset item in list)
		{
			item.color_container = new ContainerItemColor(item.color_hex, item.material_path);
		}
	}

	public override void editorDiagnosticLocales()
	{
		base.editorDiagnosticLocales();
		foreach (RarityAsset item in list)
		{
			checkLocale(item, item.getLocaleID());
		}
	}
}
