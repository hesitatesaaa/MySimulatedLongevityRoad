using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

public class WorldLanguagesWindow : MonoBehaviour
{
	[SerializeField]
	private LocalizationButton _language_button;

	private ObjectPoolGenericMono<LocalizationButton> _pool;

	[SerializeField]
	private Transform _content;

	private static HashSet<LocalizationButton> _all_buttons = new HashSet<LocalizationButton>();

	private Dictionary<string, int> _percentage_data;

	private void Start()
	{
		TextAsset val = Resources.Load<TextAsset>("locales/percentages");
		_percentage_data = JsonConvert.DeserializeObject<Dictionary<string, int>>(val.text);
		_pool = new ObjectPoolGenericMono<LocalizationButton>(_language_button, _content);
		foreach (GameLanguageAsset item in AssetManager.game_language_library.list)
		{
			if (item != null)
			{
				LocalizationButton next = _pool.getNext();
				_all_buttons.Add(next);
				_percentage_data.TryGetValue(item.id, out var value);
				next.SetAsset(item, value);
			}
		}
		checkDebug();
	}

	private void OnEnable()
	{
		checkDebug();
	}

	private void checkDebug()
	{
		bool active = DebugConfig.isOn(DebugOption.DebugButton);
		foreach (LocalizationButton all_button in _all_buttons)
		{
			if (all_button.getAsset().debug_only)
			{
				((Component)all_button).gameObject.SetActive(active);
			}
		}
	}

	public static void updateButtons()
	{
		foreach (LocalizationButton all_button in _all_buttons)
		{
			all_button.checkSprite();
		}
	}
}
