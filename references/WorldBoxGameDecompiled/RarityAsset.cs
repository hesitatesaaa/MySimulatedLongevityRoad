using System;

[Serializable]
public class RarityAsset : Asset, ILocalizedAsset
{
	public string color_hex;

	public string material_path;

	public string rarity_trait_string;

	[NonSerialized]
	public ContainerItemColor color_container;

	public string getLocaleID()
	{
		return rarity_trait_string;
	}
}
