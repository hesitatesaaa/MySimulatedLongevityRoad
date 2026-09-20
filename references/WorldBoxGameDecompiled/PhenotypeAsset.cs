using System;
using UnityEngine;

[Serializable]
public class PhenotypeAsset : BaseAugmentationAsset, ISkipLocaleAsset
{
	public string shades_from;

	public string shades_to;

	public string color_eyes;

	public string color_details_1;

	public string color_details_2;

	public int phenotype_index;

	[NonSerialized]
	public Color32[] colors = (Color32[])(object)new Color32[4];

	public string subspecies_trait_id;

	public override BaseCategoryAsset getGroup()
	{
		return AssetManager.subspecies_trait_groups.get(group_id);
	}

	public PhenotypeAsset()
	{
		has_locales = false;
	}
}
