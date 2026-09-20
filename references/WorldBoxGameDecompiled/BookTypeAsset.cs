using System;
using UnityEngine;

[Serializable]
public class BookTypeAsset : Asset, IDescriptionAsset, ILocalizedAsset
{
	public int writing_rate = 1;

	public string name_template;

	public string path_icons;

	public string color_text;

	public BookReadAction read_action;

	public bool save_culture = true;

	public bool save_religion = true;

	public BookRateCalc rate_calc;

	public BookRequirementCheck requirement_check;

	private Sprite[] _cached_icons;

	public BaseStats base_stats = new BaseStats();

	public string getNewIconPath()
	{
		if (_cached_icons == null)
		{
			_cached_icons = SpriteTextureLoader.getSpriteList(getFullIconPath());
		}
		return ((Object)_cached_icons.GetRandom()).name;
	}

	public string getFullIconPath()
	{
		return "books/book_icons/" + path_icons;
	}

	public string getTypeID()
	{
		return "book_type_" + id;
	}

	public string getLocaleID()
	{
		return getTypeID();
	}

	public string getDescriptionID()
	{
		return "book_type_info_" + id;
	}

	public string getDescriptionTranslated()
	{
		return LocalizedTextManager.getText(getDescriptionID());
	}
}
