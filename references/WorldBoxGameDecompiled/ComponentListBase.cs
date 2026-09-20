using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ComponentListBase<TListElement, TMetaObject, TData, TComponent> : MonoBehaviour, IComponentList, IShouldRefreshWindow where TListElement : WindowListElementBase<TMetaObject, TData> where TMetaObject : CoreSystemObject<TData> where TData : BaseSystemData where TComponent : ComponentListBase<TListElement, TMetaObject, TData, TComponent>
{
	public GameObject no_items;

	public SortingTab sorting_tab;

	public TListElement element_prefab;

	public Transform list_transform;

	public ScrollRect scroll_rect;

	[SerializeField]
	private Text _title_counter;

	[SerializeField]
	private Text _favorites_counter;

	[SerializeField]
	private Text _dead_counter;

	private ListItemsFilter _show_items;

	public GetListOfObjectsFunc<TListElement, TMetaObject, TData, TComponent> get_objects_delegate = getObjects;

	private ObjectPoolGenericMono<TListElement> _pool_elements;

	private ObjectPoolGenericMono<BaseEmptyListMono> _pool_empty_elements;

	protected Comparison<TMetaObject> current_sort;

	public readonly List<NanoObject> meta_list = new List<NanoObject>();

	private bool autolayout_done;

	private const int PADDING_ELEMENTS = 3;

	private static readonly bool _debug;

	private bool _created;

	protected int latest_counted;

	private float _element_height;

	protected virtual MetaType meta_type
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	private MetaTypeAsset _meta_type_asset => AssetManager.meta_type_library.getAsset(meta_type);

	protected virtual bool change_asset_sort_order => true;

	protected virtual IEnumerable<TMetaObject> getObjectsList()
	{
		return get_objects_delegate((TComponent)this);
	}

	protected ObjectPoolGenericMono<BaseEmptyListMono> getPoolEmpty()
	{
		return _pool_empty_elements;
	}

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
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		_pool_elements = new ObjectPoolGenericMono<TListElement>(element_prefab, list_transform);
		_element_height = ((Component)((Component)element_prefab).transform).GetComponent<RectTransform>().sizeDelta.y;
		addEmptyPoolSystem();
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

	private void OnRenderObject()
	{
		autolayout_done = true;
	}

	private void LateUpdate()
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		if (!autolayout_done)
		{
			return;
		}
		IReadOnlyList<BaseEmptyListMono> listTotal = _pool_empty_elements.getListTotal();
		int num = int.MaxValue;
		int num2 = int.MinValue;
		float y = ((Transform)scroll_rect.content).localPosition.y;
		Rect rect = scroll_rect.viewport.rect;
		float pScrollRectTop = y + ((Rect)(ref rect)).height;
		for (int i = 0; i < listTotal.Count; i++)
		{
			BaseEmptyListMono baseEmptyListMono = listTotal[i];
			if (!((Component)baseEmptyListMono).gameObject.activeSelf)
			{
				continue;
			}
			if (IsVisibleInScrollRect(baseEmptyListMono.rect_transform, scroll_rect, pScrollRectTop, y))
			{
				if (num == int.MaxValue)
				{
					num = i;
				}
				num2 = i;
			}
			else if (num2 > int.MinValue)
			{
				break;
			}
		}
		if (num2 == int.MaxValue || num == int.MinValue)
		{
			return;
		}
		int num3 = Math.Max(0, num - 3);
		int num4 = Math.Min(listTotal.Count - 1, num2 + 3);
		for (int j = 0; j < listTotal.Count; j++)
		{
			if (j < num3 || j > num4)
			{
				BaseEmptyListMono pEmptyMono = listTotal[j];
				releaseElement(pEmptyMono);
			}
		}
		for (int k = num3; k <= num4; k++)
		{
			BaseEmptyListMono baseEmptyListMono2 = listTotal[k];
			if (((Component)baseEmptyListMono2).gameObject.activeSelf && !baseEmptyListMono2.hasElement())
			{
				makeElementVisible(baseEmptyListMono2);
			}
		}
		if (_debug)
		{
			debugUpdateElementNames(listTotal, pScrollRectTop, y);
		}
	}

	private void makeElementVisible(BaseEmptyListMono pEmptyMono)
	{
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		TListElement next = _pool_elements.getNext();
		next.show((TMetaObject)pEmptyMono.meta_object);
		((Component)next).transform.SetParent(((Component)pEmptyMono).transform);
		((Component)next).transform.localPosition = Vector3.zero;
		pEmptyMono.assignElement((MonoBehaviour)(object)next);
	}

	private bool IsVisibleInScrollRect(RectTransform pRectTransform, ScrollRect pScrollRect, float pScrollRectTop, float pScrollRectBottom)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		Vector2 val = Vector2.op_Implicit(((Transform)pRectTransform).localPosition);
		val *= -1f;
		float num = pRectTransform.sizeDelta.y * 0.6f;
		if (val.y <= pScrollRectTop + num + ((Component)this).transform.localPosition.y)
		{
			return val.y >= pScrollRectBottom - num + ((Component)this).transform.localPosition.y;
		}
		return false;
	}

	private void addEmptyPoolSystem()
	{
		BaseEmptyListMono baseEmptyListMono = Resources.Load<BaseEmptyListMono>("ui/list_element_empty");
		baseEmptyListMono = Object.Instantiate<BaseEmptyListMono>(baseEmptyListMono, list_transform);
		((Component)baseEmptyListMono).gameObject.SetActive(false);
		LayoutElement val = default(LayoutElement);
		if (_element_height > 0f && ((Component)baseEmptyListMono).TryGetComponent<LayoutElement>(ref val))
		{
			val.minHeight = _element_height;
		}
		_pool_empty_elements = new ObjectPoolGenericMono<BaseEmptyListMono>(baseEmptyListMono, list_transform);
	}

	private void showElement(TMetaObject pObject)
	{
		_pool_empty_elements.getNext().assignObject(pObject);
	}

	protected static IEnumerable<TMetaObject> getObjects(ComponentListBase<TListElement, TMetaObject, TData, TComponent> pComponentList)
	{
		IEnumerable<TMetaObject> pList = pComponentList._meta_type_asset.get_list().Cast<TMetaObject>();
		foreach (TMetaObject item in pComponentList.getFiltered(pList))
		{
			yield return item;
		}
	}

	protected virtual IEnumerable<TMetaObject> getFiltered(IEnumerable<TMetaObject> pList)
	{
		switch (getCurrentFilter())
		{
		case ListItemsFilter.Favorites:
			foreach (TMetaObject p in pList)
			{
				if (p.isFavorite())
				{
					yield return p;
				}
			}
			yield break;
		case ListItemsFilter.Dead:
			foreach (TMetaObject p2 in pList)
			{
				if (p2.hasDied())
				{
					yield return p2;
				}
			}
			yield break;
		case ListItemsFilter.OnlyAlive:
			foreach (TMetaObject p3 in pList)
			{
				if (!p3.hasDied())
				{
					yield return p3;
				}
			}
			yield break;
		}
		foreach (TMetaObject p4 in pList)
		{
			yield return p4;
		}
	}

	private void OnEnable()
	{
		checkCreate();
		showSortingTabs();
		show();
	}

	protected virtual void show()
	{
		if (!Config.game_loaded)
		{
			return;
		}
		clear();
		latest_counted = 0;
		if (isEmpty())
		{
			if ((Object)(object)no_items != (Object)null)
			{
				no_items.SetActive(true);
			}
		}
		else
		{
			if ((Object)(object)no_items != (Object)null)
			{
				no_items.SetActive(false);
			}
			showElements();
			latest_counted = _pool_empty_elements.countActive();
		}
		if ((Object)(object)_title_counter != (Object)null)
		{
			_title_counter.text = latest_counted.ToString();
		}
		if ((Object)(object)_favorites_counter != (Object)null)
		{
			_favorites_counter.text = latest_counted.ToString();
		}
		if ((Object)(object)_dead_counter != (Object)null)
		{
			_dead_counter.text = latest_counted.ToString();
		}
		_pool_empty_elements.disableInactive();
		ScrollWindow.checkElements();
	}

	public ListPool<NanoObject> getElements()
	{
		meta_list.Clear();
		meta_list.AddRange(getObjectsList());
		meta_list.Sort((NanoObject a, NanoObject b) => current_sort(a as TMetaObject, b as TMetaObject));
		SortButton currentButton = sorting_tab.getCurrentButton();
		if (currentButton != null && currentButton.getState() == SortButtonState.Down)
		{
			meta_list.Reverse();
		}
		return new ListPool<NanoObject>(meta_list);
	}

	protected void showElements()
	{
		using ListPool<NanoObject> listPool = getElements();
		for (int i = 0; i < listPool.Count; i++)
		{
			NanoObject nanoObject = listPool[i];
			showElement(nanoObject as TMetaObject);
		}
		if (change_asset_sort_order)
		{
			_meta_type_asset.setListGetter(getElements);
		}
	}

	public virtual bool isEmpty()
	{
		IEnumerable<TMetaObject> objectsList = getObjectsList();
		if (objectsList == null)
		{
			return true;
		}
		return !objectsList.Any();
	}

	public virtual void clear()
	{
		IReadOnlyList<BaseEmptyListMono> listTotal = _pool_empty_elements.getListTotal();
		for (int i = 0; i < listTotal.Count; i++)
		{
			BaseEmptyListMono baseEmptyListMono = listTotal[i];
			releaseElement(baseEmptyListMono);
			baseEmptyListMono.clearObject();
		}
		_pool_empty_elements.clear();
		_pool_elements.resetParent();
		meta_list.Clear();
		_meta_type_asset.setListGetter(null);
	}

	private void releaseElement(BaseEmptyListMono pEmptyMono)
	{
		if (pEmptyMono.hasElement())
		{
			TListElement pElement = (TListElement)(WindowListElementBase<TMetaObject, TData>)(object)pEmptyMono.element;
			pEmptyMono.clearElement();
			_pool_elements.release(pElement);
		}
	}

	private void debugUpdateElementNames(IReadOnlyList<BaseEmptyListMono> pList, float pScrollRectTop, float pScrollRectBottom)
	{
		for (int i = 0; i < pList.Count; i++)
		{
			BaseEmptyListMono baseEmptyListMono = pList[i];
			bool tVisible = IsVisibleInScrollRect(baseEmptyListMono.rect_transform, scroll_rect, pScrollRectTop, pScrollRectBottom);
			baseEmptyListMono.debugUpdateName(tVisible);
		}
	}

	private void OnDisable()
	{
		clear();
	}

	public void setShowFavoritesOnly()
	{
		_show_items = ListItemsFilter.Favorites;
	}

	public void setShowAll()
	{
		_show_items = ListItemsFilter.All;
	}

	public void setShowDeadOnly()
	{
		_show_items = ListItemsFilter.Dead;
	}

	public void setShowAliveOnly()
	{
		_show_items = ListItemsFilter.OnlyAlive;
	}

	public virtual void setDefault()
	{
	}

	public ListItemsFilter getCurrentFilter()
	{
		return _show_items;
	}

	public void init(GameObject pNoItems, SortingTab pSortingTab, GameObject pListElementPrefab, Transform pListTransform, ScrollRect pScrollRect, Text pTitleCounter, Text pFavoritesCounter, Text pDeadCounter)
	{
		no_items = pNoItems;
		sorting_tab = pSortingTab;
		element_prefab = pListElementPrefab.GetComponent<TListElement>();
		list_transform = pListTransform;
		scroll_rect = pScrollRect;
		_title_counter = pTitleCounter;
		_favorites_counter = pFavoritesCounter;
		_dead_counter = pDeadCounter;
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

	protected void genericMetaSortByAge(Comparison<TMetaObject> pAction)
	{
		sorting_tab.tryAddButton("ui/Icons/iconAge", "sort_by_age", show, delegate
		{
			current_sort = pAction;
		});
	}

	protected void genericMetaSortByRenown(Comparison<TMetaObject> pAction)
	{
		sorting_tab.tryAddButton("ui/Icons/iconRenown", "sort_by_renown", show, delegate
		{
			current_sort = pAction;
		});
	}

	protected void genericMetaSortByPopulation(Comparison<TMetaObject> pAction)
	{
		sorting_tab.tryAddButton("ui/Icons/iconPopulation", "sort_by_members", show, delegate
		{
			current_sort = pAction;
		});
	}

	protected void genericMetaSortByKills(Comparison<TMetaObject> pAction)
	{
		sorting_tab.tryAddButton("ui/Icons/iconKills", "sort_by_kills", show, delegate
		{
			current_sort = pAction;
		});
	}

	protected void genericMetaSortByDeath(Comparison<TMetaObject> pAction)
	{
		sorting_tab.tryAddButton("ui/Icons/iconDead", "sort_by_dead", show, delegate
		{
			current_sort = pAction;
		});
	}

	protected int sortByRenown(IMetaObject p1, IMetaObject p2)
	{
		return p2.getRenown().CompareTo(p1.getRenown());
	}

	protected int sortByAge(IMetaObject p1, IMetaObject p2)
	{
		return -p2.getMetaData().created_time.CompareTo(p1.getMetaData().created_time);
	}

	public static int sortByPopulation(IMetaObject p1, IMetaObject p2)
	{
		return p2.getPopulationPeople().CompareTo(p1.getPopulationPeople());
	}

	public static int sortByKills(IMetaObject p1, IMetaObject p2)
	{
		return p2.getTotalKills().CompareTo(p1.getTotalKills());
	}

	public static int sortByDeaths(IMetaObject p1, IMetaObject p2)
	{
		return p2.getTotalDeaths().CompareTo(p1.getTotalDeaths());
	}
}
