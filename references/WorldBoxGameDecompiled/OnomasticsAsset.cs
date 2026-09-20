using System;
using System.ComponentModel;
using UnityEngine;

[Serializable]
public class OnomasticsAsset : Asset, IDescription2Asset, IDescriptionAsset, ILocalizedAsset
{
	public OnomasticsAssetType type;

	public string path_icon;

	public string color_text = "#FF0000";

	[NonSerialized]
	private Sprite _cached_sprite;

	public bool affects_left;

	public bool affects_left_word;

	public bool affects_left_group_only;

	public bool affects_everything;

	public bool is_divider;

	public bool is_upper;

	public bool is_word_divider;

	public bool is_immune;

	[DefaultValue(-1)]
	public int group_id = -1;

	[DefaultValue('?')]
	public char short_id = '?';

	[DefaultValue("")]
	public string forced_locale_subname_id = string.Empty;

	[DefaultValue("")]
	public string forced_locale_description_id = string.Empty;

	[DefaultValue("")]
	public string forced_locale_description_id_2 = string.Empty;

	public OnomasticsNameMakerDelegate namemaker_delegate;

	public OnomasticsCheckDelegate check_delegate;

	public bool isGroupType()
	{
		return type == OnomasticsAssetType.Group;
	}

	public Sprite getSprite()
	{
		if (_cached_sprite == null)
		{
			_cached_sprite = SpriteTextureLoader.getSprite(path_icon);
		}
		return _cached_sprite;
	}

	public string getLocaleID()
	{
		return "onomastics_" + id;
	}

	public string getIDSubname()
	{
		if (forced_locale_subname_id != string.Empty)
		{
			return forced_locale_subname_id;
		}
		return "onomastics_" + id + "_subname";
	}

	public string getDescriptionID()
	{
		if (forced_locale_description_id != string.Empty)
		{
			return forced_locale_description_id;
		}
		return getLocaleID() + "_info";
	}

	public string getDescriptionID2()
	{
		if (forced_locale_description_id_2 != string.Empty)
		{
			return forced_locale_description_id_2;
		}
		return getLocaleID() + "_info_2";
	}
}
