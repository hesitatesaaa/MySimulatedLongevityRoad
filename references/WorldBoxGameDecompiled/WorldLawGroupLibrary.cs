public class WorldLawGroupLibrary : BaseCategoryLibrary<WorldLawGroupAsset>
{
	public override void init()
	{
		base.init();
		add(new WorldLawGroupAsset
		{
			id = "harmony",
			name = "world_laws_tab_harmony",
			color = "#FFFAA3"
		});
		add(new WorldLawGroupAsset
		{
			id = "diplomacy",
			name = "world_laws_tab_diplomacy",
			color = "#BAF0F4"
		});
		add(new WorldLawGroupAsset
		{
			id = "civilizations",
			name = "world_laws_tab_civilizations",
			color = "#BAF0F4"
		});
		add(new WorldLawGroupAsset
		{
			id = "units",
			name = "world_laws_tab_units",
			color = "#FF6B86"
		});
		add(new WorldLawGroupAsset
		{
			id = "mobs",
			name = "world_laws_tab_mobs",
			color = "#BAFFC2"
		});
		add(new WorldLawGroupAsset
		{
			id = "spawn",
			name = "world_laws_tab_spawn",
			color = "#F482FF"
		});
		add(new WorldLawGroupAsset
		{
			id = "nature",
			name = "world_laws_tab_nature",
			color = "#68FF77"
		});
		add(new WorldLawGroupAsset
		{
			id = "trees",
			name = "world_laws_tab_trees",
			color = "#DEEA5D"
		});
		add(new WorldLawGroupAsset
		{
			id = "plants",
			name = "world_laws_tab_plants",
			color = "#E1E894"
		});
		add(new WorldLawGroupAsset
		{
			id = "fungi",
			name = "world_laws_tab_fungi",
			color = "#FF9699"
		});
		add(new WorldLawGroupAsset
		{
			id = "biomes",
			name = "world_laws_tab_biomes",
			color = "#FFDD32"
		});
		add(new WorldLawGroupAsset
		{
			id = "weather",
			name = "world_laws_tab_weather",
			color = "#59B9FF"
		});
		add(new WorldLawGroupAsset
		{
			id = "disasters",
			name = "world_laws_tab_disasters",
			color = "#FF6B86"
		});
		add(new WorldLawGroupAsset
		{
			id = "other",
			name = "world_laws_tab_other",
			color = "#D8D8D8"
		});
	}
}
