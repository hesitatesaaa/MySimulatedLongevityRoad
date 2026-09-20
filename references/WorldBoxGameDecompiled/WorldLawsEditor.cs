using System.Collections.Generic;
using UnityEngine;

public class WorldLawsEditor : MonoBehaviour
{
	[SerializeField]
	private WorldLawElement _element_prefab;

	[SerializeField]
	private WorldLawCategory _category_prefab;

	[SerializeField]
	private Transform _categories_parent;

	private Dictionary<string, WorldLawCategory> _categories_dict = new Dictionary<string, WorldLawCategory>();

	private void Awake()
	{
		create();
	}

	private void OnEnable()
	{
		updateButtons();
	}

	private void create()
	{
		createCategories();
		createElements();
	}

	private void createCategories()
	{
		foreach (WorldLawGroupAsset item in AssetManager.world_law_groups.list)
		{
			Transform categories_parent = _categories_parent;
			WorldLawCategory worldLawCategory = Object.Instantiate<WorldLawCategory>(_category_prefab, categories_parent);
			_categories_dict.Add(item.id, worldLawCategory);
			worldLawCategory.init(item);
		}
	}

	private void createElements()
	{
		foreach (WorldLawAsset item in AssetManager.world_laws_library.list)
		{
			if (!string.IsNullOrEmpty(item.group_id))
			{
				WorldLawCategory worldLawCategory = _categories_dict[item.group_id];
				WorldLawElement worldLawElement = Object.Instantiate<WorldLawElement>(_element_prefab, ((Component)worldLawCategory.grid).transform);
				((Object)worldLawElement).name = item.id;
				worldLawElement.init(item);
				worldLawCategory.addElement(worldLawElement);
			}
		}
	}

	private void updateButtons()
	{
		foreach (WorldLawCategory value in _categories_dict.Values)
		{
			value.updateButtons();
		}
	}

	public void resetToDefault()
	{
		foreach (WorldLawAsset item in AssetManager.world_laws_library.list)
		{
			if (item.can_turn_off)
			{
				PlayerOptionData option = item.getOption();
				bool boolVal = option.boolVal;
				option.boolVal = item.default_state;
				if (option.boolVal && !boolVal)
				{
					item.on_state_enabled?.Invoke(option);
				}
				item.on_state_change?.Invoke(option);
			}
		}
		World.world.world_laws.updateCaches();
		updateButtons();
	}
}
