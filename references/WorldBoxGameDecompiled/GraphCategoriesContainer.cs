using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GraphCategoriesContainer : MonoBehaviour
{
	public GraphController graph_controller;

	public GraphCategoryGroup category_group = GraphCategoryGroup.General;

	private GraphCategoryGroup _last_category_group;

	private GraphCategoryGroup _last_category_groups;

	private List<HistoryDataAsset> _current_list = new List<HistoryDataAsset>();

	private Dictionary<string, ButtonGraphCategory> _category_buttons = new Dictionary<string, ButtonGraphCategory>();

	private ButtonGraphCategory _prefab_button;

	private bool _is_initialized;

	[SerializeField]
	private TabTogglesGroup _category_groups;

	private void init()
	{
		if (!_is_initialized)
		{
			_is_initialized = true;
			ButtonGraphCategory[] componentsInChildren = ((Component)this).GetComponentsInChildren<ButtonGraphCategory>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				Object.Destroy((Object)(object)((Component)componentsInChildren[i]).gameObject);
			}
			_prefab_button = Resources.Load<ButtonGraphCategory>("ui/graphs/GraphCategoryButton");
		}
	}

	public void apply()
	{
		init();
		List<HistoryDataAsset> categories = graph_controller.getCategories();
		if (_last_category_group == category_group && _current_list.Count > 0 && _current_list.Count == categories.Count && _current_list.All(categories.Contains) && categories.All(_current_list.Contains))
		{
			foreach (ButtonGraphCategory value2 in _category_buttons.Values)
			{
				graph_controller.setCategoryEnabled(((Object)((Component)value2).gameObject).name, value2.is_on, pUpdateGraph: false);
			}
			return;
		}
		foreach (ButtonGraphCategory value3 in _category_buttons.Values)
		{
			((Component)value3).gameObject.SetActive(false);
		}
		GraphCategoryGroup graphCategoryGroup = GraphCategoryGroup.None;
		_current_list = new List<HistoryDataAsset>(categories);
		foreach (HistoryDataAsset item in _current_list)
		{
			graphCategoryGroup |= item.category_group;
			if (item.category_group.HasFlag(category_group))
			{
				if (!_category_buttons.TryGetValue(item.id, out var value))
				{
					value = Object.Instantiate<ButtonGraphCategory>(_prefab_button, ((Component)this).transform);
					((Object)((Component)value).gameObject).name = item.id;
					((Component)value).transform.SetParent(((Component)this).transform);
					value.init();
					value.setAsset(item);
					value.is_on = graph_controller.isCategoryEnabled(item.id);
					_category_buttons.Add(item.id, value);
				}
				((Component)value).gameObject.SetActive(true);
			}
		}
		_last_category_group = category_group;
		showCategoryGroups(graphCategoryGroup);
	}

	private void showCategoryGroups(GraphCategoryGroup pGroups)
	{
		((Component)_category_groups).gameObject.SetActive(pGroups.Count() > 1);
		if (_last_category_groups == pGroups)
		{
			return;
		}
		_category_groups.clearButtons();
		if (pGroups.HasFlag(GraphCategoryGroup.General))
		{
			_category_groups.tryAddButton("ui/Icons/iconRenown", "tab_general_stats", apply, delegate
			{
				category_group = GraphCategoryGroup.General;
			});
		}
		if (pGroups.HasFlag(GraphCategoryGroup.Noosphere))
		{
			_category_groups.tryAddButton("ui/Icons/iconKnowledge", "tab_noosphere", apply, delegate
			{
				category_group = GraphCategoryGroup.Noosphere;
			});
		}
		if (pGroups.HasFlag(GraphCategoryGroup.Deaths))
		{
			_category_groups.tryAddButton("civ/map_mark_death", "tab_deaths", apply, delegate
			{
				category_group = GraphCategoryGroup.Deaths;
			});
		}
		if (pGroups.HasFlag(GraphCategoryGroup.Biomes))
		{
			_category_groups.tryAddButton("ui/Icons/iconSeedClover", "tab_biomes", apply, delegate
			{
				category_group = GraphCategoryGroup.Biomes;
			});
		}
		if (pGroups.HasFlag(GraphCategoryGroup.Tiles))
		{
			_category_groups.tryAddButton("ui/Icons/iconZones", "tab_tiles", apply, delegate
			{
				category_group = GraphCategoryGroup.Tiles;
			});
		}
		_last_category_groups = pGroups;
		_category_groups.enableFirst();
	}

	public void setCategoryEnabled(string pId, bool pEnabled)
	{
		if (graph_controller.multi_chart)
		{
			foreach (ButtonGraphCategory value in _category_buttons.Values)
			{
				value.is_on = ((Object)((Component)value).gameObject).name == pId;
			}
			graph_controller.disableAllCategories(pId);
		}
		graph_controller.setCategoryEnabled(pId, pEnabled, pUpdateGraph: false);
		graph_controller.adjustCharts();
	}
}
