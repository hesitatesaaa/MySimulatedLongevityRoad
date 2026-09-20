using UnityEngine;

public class TabbedWindow : MonoBehaviour
{
	protected ScrollWindow scroll_window;

	protected WindowMetaTabButtonsContainer tabs => scroll_window.tabs;

	protected void Awake()
	{
		scroll_window = ((Component)((Component)this).transform).GetComponentInParent<ScrollWindow>();
		create();
	}

	protected virtual void create()
	{
		tabs.init();
	}

	internal virtual bool checkCancelWindow()
	{
		return false;
	}

	public void showTab(WindowMetaTab pTab)
	{
		tabs.showTab(pTab);
	}
}
