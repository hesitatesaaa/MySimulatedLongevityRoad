using System.Collections.Generic;
using UnityEngine;

public class TabTogglesGroup : MonoBehaviour
{
	private readonly List<TabToggleContainer> _toggles = new List<TabToggleContainer>();

	private TabToggle _current_toggle;

	public TabToggle getCurrentButton()
	{
		return _current_toggle;
	}

	public void clearButtons()
	{
		foreach (TabToggleContainer toggle in _toggles)
		{
			((Component)toggle).gameObject.SetActive(false);
		}
	}

	public void tryAddButton(string pIcon, string pTooltip, TabToggleAction pShowAction, TabToggleAction pAction)
	{
		if (!switchButton(pTooltip, pEnabled: true))
		{
			addButton(pIcon, pTooltip, pShowAction, pAction);
		}
	}

	public bool switchButton(string pTooltip, bool pEnabled)
	{
		foreach (TabToggleContainer toggle in _toggles)
		{
			if (((Object)((Component)toggle).gameObject).name == pTooltip)
			{
				((Component)toggle).gameObject.SetActive(pEnabled);
				return true;
			}
		}
		return false;
	}

	public void addButton(string pIcon, string pTooltip, TabToggleAction pShowAction, TabToggleAction pAction)
	{
		TabToggleContainer tabToggleContainer = Object.Instantiate<TabToggleContainer>(Resources.Load<TabToggleContainer>("ui/TabToggleGeneric"), ((Component)this).transform);
		TabToggle componentInChildren = ((Component)tabToggleContainer).GetComponentInChildren<TabToggle>();
		PowerButton component = ((Component)componentInChildren).GetComponent<PowerButton>();
		component.icon.sprite = SpriteTextureLoader.getSprite(pIcon);
		((Component)component).GetComponent<TipButton>().textOnClick = pTooltip;
		((Component)component).GetComponent<TipButton>().textOnClickDescription = pTooltip + "_description";
		componentInChildren.icon = component.icon;
		componentInChildren.select_action = selectAction;
		componentInChildren.action = pAction;
		componentInChildren.post_action = pShowAction;
		((Object)((Component)componentInChildren).gameObject).name = pTooltip;
		((Object)((Component)tabToggleContainer).gameObject).name = pTooltip;
		_toggles.Add(tabToggleContainer);
	}

	private void selectAction(TabToggle pToggle)
	{
		foreach (TabToggleContainer toggle in _toggles)
		{
			if (!((Object)(object)toggle.toggle == (Object)(object)pToggle))
			{
				toggle.toggle.unselect();
			}
		}
		_current_toggle = pToggle;
	}

	internal void enableFirst()
	{
		if (_toggles.Count == 0)
		{
			return;
		}
		selectAction(null);
		foreach (TabToggleContainer toggle in _toggles)
		{
			if (((Component)toggle).gameObject.activeSelf)
			{
				toggle.toggle.click();
				break;
			}
		}
	}
}
