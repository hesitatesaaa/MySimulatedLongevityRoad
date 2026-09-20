using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

[Serializable]
public class GameLanguageAsset : Asset
{
	public string name;

	public bool main;

	public bool export = true;

	public bool is_rtl;

	public bool is_hanzi;

	public bool is_hindi;

	public bool debug_only;

	public string path_icon;

	public bool show_translators = true;

	public FontGetter font = () => LocalizedTextManager.instance.default_font;

	public ForcedFontStyle force_style;

	private Dictionary<string, Dictionary<string, string>> _translations;

	private static Dictionary<string, GameLanguageData> _language_data;

	[JsonIgnore]
	public Dictionary<string, Dictionary<string, string>> translations
	{
		get
		{
			if (_translations == null)
			{
				_translations = new Dictionary<string, Dictionary<string, string>>();
				TextAsset[] array = Resources.LoadAll<TextAsset>("locales/" + id);
				foreach (TextAsset obj in array)
				{
					string text = obj.text;
					string key = ((Object)obj).name;
					Dictionary<string, string> value = JsonConvert.DeserializeObject<Dictionary<string, string>>(text);
					_translations[key] = value;
				}
			}
			return _translations;
		}
	}

	public IEnumerable<string> getGroups()
	{
		return translations.Keys;
	}

	public GameLanguageData getLanguageData()
	{
		if (_language_data == null)
		{
			TextAsset val = Resources.Load<TextAsset>("texts/tooltip_translators");
			if ((Object)(object)val == (Object)null)
			{
				Debug.LogError((object)("No tooltip translators found for language: " + id));
				return null;
			}
			_language_data = JsonConvert.DeserializeObject<Dictionary<string, GameLanguageData>>(val.text);
		}
		_language_data.TryGetValue(id, out var value);
		return value;
	}

	public bool isRTL()
	{
		return is_rtl;
	}

	public bool isHanzi()
	{
		return is_hanzi;
	}

	public bool isHindi()
	{
		return is_hindi;
	}

	public bool hasForcedStyle()
	{
		return force_style != null;
	}
}
