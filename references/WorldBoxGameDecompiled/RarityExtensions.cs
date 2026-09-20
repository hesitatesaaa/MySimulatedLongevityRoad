using UnityEngine;

public static class RarityExtensions
{
	public static string getRarityColorHex(this Rarity pRarity)
	{
		return pRarity.getAsset().color_hex;
	}

	public static RarityAsset getAsset(this Rarity pRarity)
	{
		string stringID = pRarity.getStringID();
		return AssetManager.rarity_library.get(stringID);
	}

	public static Color getRarityColor(this Rarity pRarity)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		return pRarity.getAsset().color_container.color;
	}

	public static string getStringID(this Rarity pRarity)
	{
		return pRarity.ToString().ToLower();
	}

	public static int GetRate(this Rarity pRarity)
	{
		return pRarity switch
		{
			Rarity.R0_Normal => 10, 
			Rarity.R1_Rare => 6, 
			Rarity.R2_Epic => 3, 
			Rarity.R3_Legendary => 1, 
			_ => 0, 
		};
	}
}
