using System;

[Serializable]
public class MonthAsset : Asset, IDescriptionAsset, ILocalizedAsset
{
	public string english_name;

	public int month_index;

	public string getLocaleID()
	{
		return id;
	}

	public string getDescriptionID()
	{
		return "inflected_" + id;
	}
}
