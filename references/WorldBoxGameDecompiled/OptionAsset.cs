using System;
using System.Collections.Generic;
using System.ComponentModel;
using Newtonsoft.Json;

[Serializable]
public class OptionAsset : Asset, IDescription2Asset, IDescriptionAsset, ILocalizedAsset, IMultiLocalesAsset
{
	public OptionType type;

	[DefaultValue(true)]
	public bool has_locales = true;

	public string translation_key;

	public string translation_key_description;

	public string translation_key_description_2;

	[DefaultValue(true)]
	public bool reset_to_default_on_launch;

	[DefaultValue(true)]
	public bool default_bool = true;

	public bool computer_only;

	public bool override_bool_mobile;

	[DefaultValue(true)]
	public bool default_bool_mobile = true;

	public string default_string = string.Empty;

	public int default_int;

	public ActionOptionAsset action;

	public ActionFormatCounterOptionAsset counter_format;

	public int min_value;

	public int max_value;

	public bool multi_toggle;

	public bool counter_percent;

	public string[] locale_options_ids;

	public bool update_all_elements_after_click;

	[DefaultValue(true)]
	public bool interactable = true;

	[JsonIgnore]
	public PlayerOptionData data => PlayerConfig.dict[id];

	[JsonIgnore]
	public int current_int_value => PlayerConfig.dict[id].intVal;

	[JsonIgnore]
	public bool current_bool_value => PlayerConfig.optionBoolEnabled(id);

	public string getLocaleID()
	{
		if (!has_locales)
		{
			return null;
		}
		if (!string.IsNullOrEmpty(translation_key))
		{
			return translation_key;
		}
		return id;
	}

	public string getDescriptionID()
	{
		if (!has_locales)
		{
			return null;
		}
		return translation_key_description;
	}

	public string getDescriptionID2()
	{
		if (!has_locales)
		{
			return null;
		}
		return translation_key_description_2;
	}

	public string getOptionLocaleID(int pIndex)
	{
		return locale_options_ids[pIndex];
	}

	public string getOptionLocaleID()
	{
		return getOptionLocaleID(current_int_value);
	}

	public string getTranslatedOption()
	{
		return getOptionLocaleID().Localize();
	}

	public IEnumerable<string> getLocaleIDs()
	{
		if (!has_locales)
		{
			yield break;
		}
		yield return getLocaleID();
		yield return getDescriptionID();
		yield return getDescriptionID2();
		if (locale_options_ids != null)
		{
			string[] array = locale_options_ids;
			for (int i = 0; i < array.Length; i++)
			{
				yield return array[i];
			}
		}
	}

	public bool isActive()
	{
		if (type == OptionType.Bool)
		{
			return PlayerConfig.optionBoolEnabled(id);
		}
		return PlayerConfig.getOptionInt(id) > 0;
	}
}
