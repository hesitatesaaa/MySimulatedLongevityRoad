using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class DebugKingdomFoes : MonoBehaviour
{
	[SerializeField]
	private DebugKingdomButton _prefab_button;

	[SerializeField]
	private Image _selector;

	[SerializeField]
	private GridLayoutGroup _grid_main;

	[SerializeField]
	private GridLayoutGroup _grid_civs;

	[SerializeField]
	private GridLayoutGroup _grid_minicivs;

	[SerializeField]
	private GridLayoutGroup _grid_minicivs_special;

	[SerializeField]
	private GridLayoutGroup _grid_concepts;

	[SerializeField]
	private GridLayoutGroup _grid_mobs;

	[SerializeField]
	private GridLayoutGroup _grid_creeps;

	[SerializeField]
	private GridLayoutGroup _grid_others;

	private List<DebugKingdomButton> _buttons = new List<DebugKingdomButton>();

	private KingdomAsset _current_selected;

	private bool _initialized;

	private void Awake()
	{
		create();
	}

	private void create()
	{
		//IL_0157: Unknown result type (might be due to invalid IL or missing references)
		//IL_0161: Expected O, but got Unknown
		if (_initialized)
		{
			return;
		}
		_initialized = true;
		AssetManager.kingdoms.checkForMissingTags();
		foreach (KingdomAsset item in AssetManager.kingdoms.list)
		{
			if (!item.isTemplateAsset())
			{
				Transform val = (item.group_main ? ((Component)_grid_main).transform : (item.group_creeps ? ((Component)_grid_creeps).transform : (item.concept ? ((Component)_grid_concepts).transform : (item.is_forced_by_trait ? ((Component)_grid_others).transform : (item.group_minicivs_cool ? ((Component)_grid_minicivs_special).transform : (item.group_miniciv ? ((Component)_grid_minicivs).transform : (item.civ ? ((Component)_grid_civs).transform : ((!item.mobs) ? ((Component)_grid_others).transform : ((Component)_grid_mobs).transform))))))));
				DebugKingdomButton tNewButton = Object.Instantiate<DebugKingdomButton>(_prefab_button, val);
				tNewButton.setAsset(item);
				_buttons.Add(tNewButton);
				((UnityEvent)((Component)tNewButton).GetComponent<Button>().onClick).AddListener((UnityAction)delegate
				{
					select(tNewButton);
				});
			}
		}
		select(_buttons.GetRandom());
	}

	private void select(DebugKingdomButton pButton)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		_current_selected = pButton.kingdom_asset;
		((Component)_selector).transform.position = ((Component)pButton).transform.position;
		updateButtons();
	}

	private void updateButtons()
	{
		foreach (DebugKingdomButton button in _buttons)
		{
			button.checkSelected(_current_selected);
		}
	}
}
