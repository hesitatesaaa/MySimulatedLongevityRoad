using System;

[Serializable]
public class ChromosomeTypeAsset : Asset, IDescriptionAsset, ILocalizedAsset
{
	public int amount_loci;

	public int amount_loci_min_amplifier;

	public int amount_loci_max_amplifier;

	public int amount_loci_min_empty;

	public int amount_loci_max_empty;

	public string name;

	public string description;

	public string getLocaleID()
	{
		return id;
	}

	public string getDescriptionID()
	{
		return id + "_description";
	}
}
