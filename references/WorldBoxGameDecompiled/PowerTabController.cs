using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class PowerTabController : MonoBehaviour
{
	public PowersTab tab_main;

	public PowersTab tab_selected_unit;

	public PowersTab tab_multiple_units;

	public PowersTab tab_selected_kingdom;

	public PowersTab tab_selected_subspecies;

	public PowersTab tab_selected_alliance;

	public PowersTab tab_selected_army;

	public PowersTab tab_selected_family;

	public PowersTab tab_selected_language;

	public PowersTab tab_selected_culture;

	public PowersTab tab_selected_religion;

	public PowersTab tab_selected_clan;

	public PowersTab tab_selected_city;

	[Space]
	public Button t_main;

	public Button t_drawing;

	public Button t_kingdoms;

	public Button t_creatures;

	public Button t_bombs;

	public Button t_nature;

	public Button t_other;

	public Transform copyTarget;

	[Space]
	internal static PowerTabController instance;

	public ToolbarArrow arrowLeft;

	public ToolbarArrow arrowRight;

	public RectTransform rect;

	public ScrollRectExtended scrollRect;

	private List<Button> _buttons;

	public static string prev_selected_meta_id;

	private void Awake()
	{
		instance = this;
		_buttons = new List<Button> { t_drawing, t_kingdoms, t_creatures, t_nature, t_bombs, t_other };
	}

	private void Update()
	{
		PowersTab activeTab = PowersTab.getActiveTab();
		PowerTabAsset asset = activeTab.getAsset();
		if (asset.on_update_check_active != null && !asset.on_update_check_active(asset))
		{
			activeTab.hideTab();
			showMainTab();
			World.world.selected_buttons.unselectTabs();
		}
		else
		{
			activeTab.update();
		}
	}

	internal static float currentScrollPosition()
	{
		return instance.scrollRect.horizontalNormalizedPosition;
	}

	public static void loadScrollPosition(float pPosition)
	{
	}

	internal void resetToStartScrollPosition()
	{
		scrollRect.horizontalNormalizedPosition = 0f;
	}

	public Button getTabForTabGroup(string pGroupName)
	{
		foreach (Button button in _buttons)
		{
			if (((UnityEventBase)button.onClick).GetPersistentTarget(0).name == pGroupName)
			{
				return button;
			}
		}
		return null;
	}

	public Button getNext(string pActiveTab)
	{
		_buttons.Sort((Button a, Button b) => ((Component)a).transform.GetSiblingIndex().CompareTo(((Component)b).transform.GetSiblingIndex()));
		for (int num = 0; num < _buttons.Count; num++)
		{
			if (((UnityEventBase)_buttons[num].onClick).GetPersistentTarget(0).name == pActiveTab && num < _buttons.Count - 1)
			{
				return _buttons[num + 1];
			}
		}
		return _buttons.First();
	}

	public Button getPrev(string pActiveTab)
	{
		_buttons.Sort((Button a, Button b) => ((Component)a).transform.GetSiblingIndex().CompareTo(((Component)b).transform.GetSiblingIndex()));
		for (int num = 0; num < _buttons.Count; num++)
		{
			if (((UnityEventBase)_buttons[num].onClick).GetPersistentTarget(0).name == pActiveTab && num > 0)
			{
				return _buttons[num - 1];
			}
		}
		return _buttons.Last();
	}

	public static void showMainTab()
	{
		instance.tab_main.showTab(null);
	}

	public static void showTabSelectedUnit()
	{
		SelectedTabsHistory.addToHistory(SelectedUnit.unit);
		PowerTabAsset asset = getAsset("selected_unit");
		asset.tryToShowPowerTab();
		showWorldTipSelected(asset.id);
	}

	public static void showTabMultipleUnits()
	{
		PowerTabAsset asset = getAsset("multiple_units");
		asset.tryToShowPowerTab();
		showWorldTipSelected(asset.id);
	}

	public static void showTabSelectedMeta(MetaTypeAsset pMetaTypeAsset)
	{
		PowerTabAsset asset = getAsset(pMetaTypeAsset.power_tab_id);
		asset.tryToShowPowerTab();
		MetaTypeAsset.last_meta_type = pMetaTypeAsset.id;
		showWorldTipSelected(asset.id);
	}

	public static void showWorldTipSelected(string pPowerTabId, bool pBar = true)
	{
		string typeID = SelectedObjects.getSelectedNanoObject().getTypeID();
		if (!(prev_selected_meta_id == typeID))
		{
			prev_selected_meta_id = typeID;
			PowerTabAsset powerTabAsset = AssetManager.power_tab_library.get(pPowerTabId);
			string pText = powerTabAsset.get_localized_worldtip(powerTabAsset);
			if (pBar)
			{
				WorldTip.instance.showToolbarText(pText);
			}
			else
			{
				WorldTip.showNowTop(pText, pTranslate: false);
			}
		}
	}

	public static PowerTabAsset getAsset(string pID)
	{
		return AssetManager.power_tab_library.get(pID);
	}
}
