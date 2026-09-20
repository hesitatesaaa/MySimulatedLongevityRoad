using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WindowListBaseActor : MonoBehaviour, IComponentList, IShouldRefreshWindow
{
	public GameObject noItems;

	protected ObjectPoolGenericMono<PrefabUnitElement> pool_elements;

	public Transform transformContent;

	public PrefabUnitElement element_prefab;

	public SortingTab sorting_tab;

	[SerializeField]
	protected Text _title_counter;

	private bool _created;

	protected Comparison<Actor> current_sort;

	internal ScrollWindow _scrollWindow;

	public readonly List<NanoObject> meta_list = new List<NanoObject>();

	private void checkCreate()
	{
		if (!_created)
		{
			_created = true;
			create();
		}
	}

	protected virtual void create()
	{
		pool_elements = new ObjectPoolGenericMono<PrefabUnitElement>(element_prefab, transformContent);
		_scrollWindow = ((Component)this).gameObject.GetComponent<ScrollWindow>();
		showSortingTabs();
	}

	protected virtual void setupSortingTabs()
	{
	}

	protected virtual void showSortingTabs()
	{
		sorting_tab.clearButtons();
		setupSortingTabs();
		sorting_tab.enableFirstIfNone();
	}

	public void init(GameObject pNoItems, SortingTab pSortingTab, GameObject pListElementPrefab, Transform pListTransform, ScrollRect pScrollRect, Text pTitleCounter, Text pFavoritesCounter, Text pDeadCounter)
	{
		noItems = pNoItems;
		sorting_tab = pSortingTab;
		element_prefab = pListElementPrefab.GetComponent<PrefabUnitElement>();
		transformContent = pListTransform;
		_title_counter = pTitleCounter;
	}

	private void showElement(Actor pObject)
	{
		pool_elements.getNext().show(pObject);
	}

	protected virtual List<Actor> getObjects()
	{
		return null;
	}

	private void OnEnable()
	{
		checkCreate();
		showSortingTabs();
		show();
	}

	protected virtual void show()
	{
		if (Config.game_loaded)
		{
			clear();
			if (isEmpty())
			{
				noItems.SetActive(true);
			}
			else
			{
				noItems.SetActive(false);
				showElements();
			}
			pool_elements.disableInactive();
			ScrollWindow.checkElements();
		}
	}

	public ListPool<NanoObject> getElements()
	{
		meta_list.Clear();
		meta_list.AddRange(getObjects());
		meta_list.Sort((NanoObject a, NanoObject b) => current_sort(a as Actor, b as Actor));
		SortButton currentButton = sorting_tab.getCurrentButton();
		if (currentButton != null && currentButton.getState() == SortButtonState.Down)
		{
			meta_list.Reverse();
		}
		return new ListPool<NanoObject>(meta_list);
	}

	private void showElements()
	{
		using ListPool<NanoObject> listPool = getElements();
		for (int i = 0; i < listPool.Count; i++)
		{
			NanoObject nanoObject = listPool[i];
			showElement(nanoObject as Actor);
		}
		AssetManager.meta_type_library.getAsset(MetaType.Unit).setListGetter(getElements);
	}

	private bool isEmpty()
	{
		List<Actor> objects = getObjects();
		if (objects == null)
		{
			return true;
		}
		return objects.Count == 0;
	}

	private void clear()
	{
		pool_elements.clear(pDisable: false);
		meta_list.Clear();
		AssetManager.meta_type_library.getAsset(MetaType.Unit).setListGetter(null);
	}

	private void OnDisable()
	{
		clear();
	}

	public void setShowFavoritesOnly()
	{
		throw new NotImplementedException();
	}

	public void setShowDeadOnly()
	{
		throw new NotImplementedException();
	}

	public void setShowAliveOnly()
	{
		throw new NotImplementedException();
	}

	public void setShowAll()
	{
		throw new NotImplementedException();
	}

	public void setDefault()
	{
		throw new NotImplementedException();
	}

	public virtual bool checkRefreshWindow()
	{
		foreach (NanoObject item in meta_list)
		{
			if (item.isRekt())
			{
				return true;
			}
		}
		return false;
	}
}
