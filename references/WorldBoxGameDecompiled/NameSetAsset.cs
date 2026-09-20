using System;
using System.Collections.Generic;
using System.ComponentModel;

[Serializable]
public class NameSetAsset : Asset
{
	[DefaultValue("")]
	public string city = string.Empty;

	[DefaultValue("")]
	public string clan = string.Empty;

	[DefaultValue("")]
	public string culture = string.Empty;

	[DefaultValue("")]
	public string family = string.Empty;

	[DefaultValue("")]
	public string kingdom = string.Empty;

	[DefaultValue("")]
	public string language = string.Empty;

	[DefaultValue("")]
	public string unit = string.Empty;

	[DefaultValue("")]
	public string religion = string.Empty;

	public string get(MetaType pType)
	{
		return pType switch
		{
			MetaType.City => city, 
			MetaType.Clan => clan, 
			MetaType.Culture => culture, 
			MetaType.Family => family, 
			MetaType.Kingdom => kingdom, 
			MetaType.Language => language, 
			MetaType.Unit => unit, 
			MetaType.Religion => religion, 
			_ => null, 
		};
	}

	public static IEnumerable<MetaType> getTypes()
	{
		yield return MetaType.City;
		yield return MetaType.Clan;
		yield return MetaType.Culture;
		yield return MetaType.Family;
		yield return MetaType.Kingdom;
		yield return MetaType.Language;
		yield return MetaType.Unit;
		yield return MetaType.Religion;
	}
}
