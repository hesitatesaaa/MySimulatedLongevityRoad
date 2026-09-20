using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WindowMetaTabButtonsContainer : MonoBehaviour
{
	private static Sprite _tab_button_on;

	private static Sprite _tab_button_off;

	public WindowMetaTab tab_default;

	private ScrollWindow _scroll_window;

	private List<WindowMetaTab> _tabs = new List<WindowMetaTab>();

	private readonly List<WindowMetaTab> _tabs_with_content = new List<WindowMetaTab>();

	private WindowMetaTab _tab_last;

	private TabShowAction _on_tab_show;

	private TabHideAction _on_tab_hide;

	private bool _initialized;

	private void Awake()
	{
		init();
	}

	public void init()
	{
		if (_initialized)
		{
			return;
		}
		_initialized = true;
		if ((Object)(object)_tab_button_on == (Object)null)
		{
			_tab_button_on = SpriteTextureLoader.getSprite("ui/tab_button_vertical_selected");
			_tab_button_off = SpriteTextureLoader.getSprite("ui/tab_button_vertical");
		}
		if (_tabs.Count > 0 && (Object)(object)tab_default == (Object)null)
		{
			tab_default = _tabs[0];
		}
		_scroll_window = ((Component)((Component)this).transform).GetComponentInParent<ScrollWindow>();
		WindowMetaTab[] componentsInChildren = ((Component)((Component)this).transform).GetComponentsInChildren<WindowMetaTab>(true);
		if (componentsInChildren.Length != 0)
		{
			WindowMetaTab[] array = componentsInChildren;
			foreach (WindowMetaTab windowMetaTab in array)
			{
				_tabs.Add(windowMetaTab);
				windowMetaTab.container = this;
			}
			refillTabsWithContent();
		}
	}

	private void OnEnable()
	{
		refillTabsWithContent();
		initialTabAction();
	}

	private void OnDisable()
	{
		hideAllTabContent();
	}

	private void refillTabsWithContent()
	{
		_tabs_with_content.Clear();
		foreach (WindowMetaTab tab in _tabs)
		{
			if (((Component)tab).gameObject.activeSelf && tab.getState() && tab.tab_elements.Count != 0)
			{
				_tabs_with_content.Add(tab);
			}
		}
	}

	public Transform addTabContent(WindowMetaTab pTabButton, Transform pContent)
	{
		string name = ((Object)pTabButton).name;
		if (!_tabs.Contains(pTabButton))
		{
			Debug.LogError((object)("[addTabContent] Tab " + name + " not found in window " + ((Object)_scroll_window).name));
			return null;
		}
		if ((Object)(object)pContent == (Object)null)
		{
			return null;
		}
		pTabButton.tab_elements.Add(pContent);
		return pContent;
	}

	public Transform addTabContent(string pTabId, Transform pContent)
	{
		WindowMetaTab windowMetaTab = _tabs.Find((WindowMetaTab c) => ((Object)c).name == pTabId);
		if ((Object)(object)windowMetaTab == (Object)null)
		{
			Debug.LogError((object)("[addTabContent] Tab " + pTabId + " not found in window " + ((Object)_scroll_window).name));
			return null;
		}
		return addTabContent(windowMetaTab, pContent);
	}

	internal void removeTab(WindowMetaTab pTab)
	{
		_tabs.Remove(pTab);
		if ((Object)(object)_tab_last == (Object)(object)pTab)
		{
			startTabAction(tab_default);
		}
	}

	public void initialTabAction()
	{
		if (!((Object)(object)tab_default == (Object)null))
		{
			if ((Object)(object)_tab_last == (Object)null)
			{
				tab_default.doAction();
			}
			else
			{
				_tab_last.doAction();
			}
		}
	}

	public void showTab(WindowMetaTab pTabButton, bool pSkipActionIfSame = false)
	{
		startTabAction(pTabButton, pSkipActionIfSame);
	}

	public void showTab(WindowMetaTab pTabButton)
	{
		startTabAction(pTabButton);
	}

	public void showTab(string pId)
	{
		foreach (WindowMetaTab tab in _tabs)
		{
			if (!(((Object)tab).name != pId))
			{
				showTab(tab);
				tab.checkShowWorldTip();
				break;
			}
		}
	}

	public void startTabAction(WindowMetaTab pTab, bool pSkipIfSame = false)
	{
		if (pTab.destroyed || (pSkipIfSame && isActiveTab(pTab)))
		{
			return;
		}
		_tab_last = pTab;
		hideAllTabContent();
		enableTab(_tab_last);
		foreach (Transform tab_element in pTab.tab_elements)
		{
			if (!((Object)(object)tab_element == (Object)null))
			{
				((Component)tab_element).gameObject.SetActive(true);
			}
		}
		_on_tab_show?.Invoke(_tab_last);
		_scroll_window.resetScroll();
	}

	public bool isActiveTab(WindowMetaTab pTab)
	{
		return (Object)(object)_tab_last == (Object)(object)pTab;
	}

	protected void enableTab(WindowMetaTab pTabButton)
	{
		((Component)pTabButton).GetComponent<Image>().sprite = _tab_button_on;
	}

	public void hideAllTabContent()
	{
		disableTabs();
		foreach (WindowMetaTab tab in _tabs)
		{
			foreach (Transform tab_element in tab.tab_elements)
			{
				if (!((Object)(object)tab_element == (Object)null))
				{
					((Component)tab_element).gameObject.SetActive(false);
				}
			}
		}
		_on_tab_hide?.Invoke();
		Tooltip.hideTooltipNow();
	}

	protected void disableTabs()
	{
		foreach (WindowMetaTab tab in _tabs)
		{
			((Component)tab).GetComponent<Image>().sprite = _tab_button_off;
		}
	}

	public List<WindowMetaTab> getContentTabs()
	{
		_tabs_with_content.Sort((WindowMetaTab p1, WindowMetaTab p2) => ((Component)p1).transform.GetSiblingIndex().CompareTo(((Component)p2).transform.GetSiblingIndex()));
		return _tabs_with_content;
	}

	public WindowMetaTab getActiveTab()
	{
		if ((Object)(object)_tab_last != (Object)null)
		{
			return _tab_last;
		}
		return tab_default;
	}

	public void reloadActiveTab()
	{
		getActiveTab().doAction();
	}

	public void addTabShowCallback(TabShowAction pCallback)
	{
		_on_tab_show = (TabShowAction)Delegate.Combine(_on_tab_show, pCallback);
	}

	public void removeTabShowCallback(TabShowAction pCallback)
	{
		_on_tab_show = (TabShowAction)Delegate.Remove(_on_tab_show, pCallback);
	}

	public void addTabHideCallback(TabHideAction pCallback)
	{
		_on_tab_hide = (TabHideAction)Delegate.Combine(_on_tab_hide, pCallback);
	}

	public void removeTabHideCallback(TabHideAction pCallback)
	{
		_on_tab_hide = (TabHideAction)Delegate.Remove(_on_tab_hide, pCallback);
	}
}
