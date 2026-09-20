using System.Collections.Generic;
using UnityEngine;

public class SortingTab : MonoBehaviour
{
	public bool scrollable;

	private readonly List<SortButtonContainer> _buttons = new List<SortButtonContainer>();

	private SortButton _current_sort_button;

	public SortButton getCurrentButton()
	{
		return _current_sort_button;
	}

	public void clearButtons()
	{
		foreach (SortButtonContainer button in _buttons)
		{
			((Component)button).gameObject.SetActive(false);
		}
	}

	public SortButton tryAddButton(string pIcon, string pTooltip, SortButtonAction pShowAction, SortButtonAction pAction)
	{
		if (switchButton(pTooltip, pEnabled: true))
		{
			return null;
		}
		return addButton(pIcon, pTooltip, pShowAction, pAction);
	}

	public bool switchButton(string pTooltip, bool pEnabled)
	{
		foreach (SortButtonContainer button in _buttons)
		{
			if (((Object)((Component)button).gameObject).name == pTooltip)
			{
				((Component)button).gameObject.SetActive(pEnabled);
				return true;
			}
		}
		return false;
	}

	public SortButton addButton(string pIcon, string pTooltip, SortButtonAction pShowAction, SortButtonAction pAction)
	{
		SortButtonContainer sortButtonContainer = Object.Instantiate<SortButtonContainer>(Resources.Load<SortButtonContainer>("ui/SortButtonGeneric"), ((Component)this).transform);
		SortButton componentInChildren = ((Component)sortButtonContainer).GetComponentInChildren<SortButton>();
		PowerButton component = ((Component)componentInChildren).GetComponent<PowerButton>();
		component.icon.sprite = SpriteTextureLoader.getSprite(pIcon);
		((Component)component).GetComponent<TipButton>().textOnClick = pTooltip;
		componentInChildren.icon = component.icon;
		componentInChildren.select_action = selectAction;
		componentInChildren.action = pAction;
		componentInChildren.post_action = pShowAction;
		((Object)((Component)componentInChildren).gameObject).name = pTooltip;
		((Object)((Component)sortButtonContainer).gameObject).name = pTooltip;
		_buttons.Add(sortButtonContainer);
		if (scrollable)
		{
			((Component)componentInChildren).gameObject.AddComponent<ScrollableButton>();
		}
		return componentInChildren;
	}

	private void selectAction(SortButton pButton)
	{
		foreach (SortButtonContainer button in _buttons)
		{
			if (!((Object)(object)button.sort_button == (Object)(object)pButton))
			{
				button.sort_button.turnOff();
			}
		}
		_current_sort_button = pButton;
	}

	internal void enableFirstIfNone()
	{
		if (_buttons.Count == 0)
		{
			return;
		}
		foreach (SortButtonContainer button in _buttons)
		{
			if (((Component)button).gameObject.activeSelf && (Object)(object)button.sort_button == (Object)(object)_current_sort_button)
			{
				return;
			}
		}
		selectAction(null);
		foreach (SortButtonContainer button2 in _buttons)
		{
			if (((Component)button2).gameObject.activeSelf)
			{
				button2.sort_button.click();
				break;
			}
		}
	}
}
