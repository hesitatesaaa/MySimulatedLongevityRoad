public class AssetsDebugManager
{
	public static ActorSex actors_sex;

	public static void changeSex()
	{
		if (actors_sex == ActorSex.Female)
		{
			actors_sex = ActorSex.Male;
		}
		else
		{
			actors_sex = ActorSex.Female;
		}
	}

	public static void newKingdomColors()
	{
		foreach (KingdomAsset item in AssetManager.kingdoms.list)
		{
			item.debug_color_asset = AssetManager.kingdom_colors_library.list.GetRandom();
		}
	}

	public static void setRandomKingdomColor(string pKingdomAssetId)
	{
		KingdomAsset kingdomAsset = AssetManager.kingdoms.get(pKingdomAssetId);
		ColorAsset random = AssetManager.kingdom_colors_library.list.GetRandom();
		kingdomAsset.debug_color_asset = random;
	}

	public static void newSkinColors()
	{
		foreach (ActorAsset item in AssetManager.actor_library.list)
		{
			if (item.use_phenotypes)
			{
				setRandomSkinColor(item);
			}
		}
	}

	public static void setRandomSkinColor(ActorAsset pAsset)
	{
		string randomSkinColor = getRandomSkinColor(pAsset);
		pAsset.debug_phenotype_colors = randomSkinColor;
	}

	private static string getRandomSkinColor(ActorAsset pAsset)
	{
		if (pAsset.phenotypes_list == null || pAsset.phenotypes_list.Count == 0)
		{
			return null;
		}
		return pAsset.phenotypes_list.GetRandom();
	}
}
