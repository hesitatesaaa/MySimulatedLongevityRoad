using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class WindowListBase<TListElement, TMetaObject, TData> : MonoBehaviour, IShouldRefreshWindow where TListElement : WindowListElementBase<TMetaObject, TData> where TMetaObject : CoreSystemObject<TData> where TData : BaseSystemData
{
	public GameObject noItems;

	protected ObjectPoolGenericMono<TListElement> pool_elements;

	protected ObjectPoolGenericMono<BaseEmptyListMono> pool_empty_elements;

	public Transform transformContent;

	public TListElement element_prefab;

	public SortingTab sorting_tab;

	private bool _created;

	protected Comparison<TMetaObject> current_sort;

	public readonly List<TMetaObject> meta_list = new List<TMetaObject>();

	private ScrollWindow _scroll_window;

	private float _element_width;

	private float _element_height;

	private ScrollRect _scroll_rect;

	private bool autolayout_done;

	private const int PADDING_ELEMENTS = 3;

	private static readonly bool _debug;

	protected virtual MetaType meta_type
	{
		get
		{
			throw new NotImplementedException(((object)this).GetType().Name);
		}
	}

	private MetaTypeAsset _meta_type_asset => AssetManager.meta_type_library.getAsset(meta_type);

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
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		pool_elements = new ObjectPoolGenericMono<TListElement>(element_prefab, transformContent);
		_scroll_window = ((Component)this).gameObject.GetComponent<ScrollWindow>();
		_element_width = ((Component)((Component)element_prefab).transform).GetComponent<RectTransform>().sizeDelta.x;
		_element_height = ((Component)((Component)element_prefab).transform).GetComponent<RectTransform>().sizeDelta.y;
		_scroll_rect = ((Component)this).gameObject.GetComponentInChildren<ScrollRect>();
		addEmptyPoolSystem();
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
		IReadOnlyList<BaseEmptyListMono> listTotal = pool_empty_elements.getListTotal();
		int num = int.MaxValue;
		int num2 = int.MinValue;
		float y = ((Transform)_scroll_rect.content).localPosition.y;
		Rect rect = _scroll_rect.viewport.rect;
		float pScrollRectTop = y + ((Rect)(ref rect)).height;
		for (int i = 0; i < listTotal.Count; i++)
		{
			BaseEmptyListMono baseEmptyListMono = listTotal[i];
			if (!((Component)baseEmptyListMono).gameObject.activeSelf)
			{
				continue;
			}
			if (IsVisibleInScrollRect(baseEmptyListMono.rect_transform, _scroll_rect, pScrollRectTop, y))
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
		TListElement next = pool_elements.getNext();
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
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		Vector2 val = Vector2.op_Implicit(((Transform)pRectTransform).localPosition);
		val *= -1f;
		float num = pRectTransform.sizeDelta.y * 0.6f;
		if (val.y <= pScrollRectTop + num)
		{
			return val.y >= pScrollRectBottom - num;
		}
		return false;
	}

	private void addEmptyPoolSystem()
	{
		BaseEmptyListMono pPrefab = Resources.Load<BaseEmptyListMono>("ui/list_element_empty");
		pool_empty_elements = new ObjectPoolGenericMono<BaseEmptyListMono>(pPrefab, transformContent);
	}

	private void showElement(TMetaObject pObject)
	{
		pool_empty_elements.getNext().assignObject(pObject);
	}

	protected virtual IEnumerable<TMetaObject> getObjects()
	{
		return _meta_type_asset.get_list().Cast<TMetaObject>();
	}

	private void OnEnable()
	{
		checkCreate();
		show();
	}

	private void show()
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
			pool_empty_elements.disableInactive();
			ScrollWindow.checkElements();
		}
	}

	private ListPool<TMetaObject> getElements()
	{
		meta_list.Clear();
		meta_list.AddRange(getObjects());
		meta_list.Sort(current_sort);
		SortButton currentButton = sorting_tab.getCurrentButton();
		if (currentButton != null && currentButton.getState() == SortButtonState.Down)
		{
			meta_list.Reverse();
		}
		return new ListPool<TMetaObject>(meta_list);
	}

	private void showElements()
	{
		using ListPool<TMetaObject> listPool = getElements();
		for (int i = 0; i < listPool.Count; i++)
		{
			TMetaObject pObject = listPool[i];
			showElement(pObject);
		}
	}

	private bool isEmpty()
	{
		IEnumerable<TMetaObject> objects = getObjects();
		if (objects == null)
		{
			return true;
		}
		return Enumerable.Count(objects) == 0;
	}

	private void clear()
	{
		IReadOnlyList<BaseEmptyListMono> listTotal = pool_empty_elements.getListTotal();
		for (int i = 0; i < listTotal.Count; i++)
		{
			BaseEmptyListMono baseEmptyListMono = listTotal[i];
			releaseElement(baseEmptyListMono);
			baseEmptyListMono.clearObject();
		}
		pool_empty_elements.clear();
		pool_elements.resetParent();
		meta_list.Clear();
	}

	private void releaseElement(BaseEmptyListMono pEmptyMono)
	{
		if (pEmptyMono.hasElement())
		{
			TListElement pElement = (TListElement)(WindowListElementBase<TMetaObject, TData>)(object)pEmptyMono.element;
			pEmptyMono.clearElement();
			pool_elements.release(pElement);
		}
	}

	private void debugUpdateElementNames(IReadOnlyList<BaseEmptyListMono> pList, float pScrollRectTop, float pScrollRectBottom)
	{
		for (int i = 0; i < pList.Count; i++)
		{
			BaseEmptyListMono baseEmptyListMono = pList[i];
			bool tVisible = IsVisibleInScrollRect(baseEmptyListMono.rect_transform, _scroll_rect, pScrollRectTop, pScrollRectBottom);
			baseEmptyListMono.debugUpdateName(tVisible);
		}
	}

	private void OnDisable()
	{
		clear();
	}

	public virtual bool checkRefreshWindow()
	{
		foreach (TMetaObject item in meta_list)
		{
			if (item.isRekt())
			{
				return true;
			}
		}
		return false;
	}
}
