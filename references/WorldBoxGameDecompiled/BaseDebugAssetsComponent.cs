using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class BaseDebugAssetsComponent<TAsset, TAssetElement, TAssetElementPlace> : MonoBehaviour where TAsset : Asset where TAssetElement : BaseDebugAssetElement<TAsset> where TAssetElementPlace : BaseAssetElementPlace<TAsset, TAssetElement>
{
	public TAssetElementPlace place_prefab;

	public TAssetElement element_prefab;

	public ScrollRect scroll_rect;

	private RectTransform _scroll_rect_transform;

	private Rect _scroll_world_rect;

	public InputField search_input_field;

	public SortingTab sorting_tab;

	protected List<TAsset> list_assets_sorted;

	protected List<TAsset> list_assets_sorting;

	protected List<TAsset> list_assets_sorting_default;

	protected bool default_sort_reversed;

	protected List<TAssetElementPlace> list_places;

	private bool _initialized;

	protected virtual List<TAsset> getAssetsList()
	{
		throw new NotImplementedException();
	}

	protected virtual List<TAsset> getListCivsSort()
	{
		throw new NotImplementedException();
	}

	private void OnEnable()
	{
		refresh();
	}

	private void Start()
	{
		_scroll_rect_transform = ((Component)scroll_rect).GetComponent<RectTransform>();
		((UnityEvent<string>)(object)search_input_field.onValueChanged).AddListener((UnityAction<string>)setDataSearched);
		init();
	}

	protected virtual void init()
	{
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		list_assets_sorted = new List<TAsset>(getAssetsList());
		list_assets_sorting = new List<TAsset>(getAssetsList());
		list_assets_sorting_default = new List<TAsset>(getAssetsList());
		foreach (Transform item in ((Component)this).transform)
		{
			Object.Destroy((Object)(object)((Component)item).gameObject);
		}
		list_places = new List<TAssetElementPlace>();
		foreach (TAsset assets in getAssetsList())
		{
			TAssetElementPlace val = Object.Instantiate<TAssetElementPlace>(place_prefab, ((Component)this).transform);
			list_places.Add(val);
			val.setData(assets, element_prefab);
		}
		sorting_tab.addButton("ui/Icons/iconHumans", "sort_by_civs", setDataResorted, delegate
		{
			list_assets_sorted = getListCivsSort();
		});
		sorting_tab.addButton("ui/Icons/actor_traits/iconClumsy", "default_sort", setDataResorted, delegate
		{
			list_assets_sorted = list_assets_sorting_default;
			if (sorting_tab.getCurrentButton().getState() == SortButtonState.Down || default_sort_reversed)
			{
				default_sort_reversed = !default_sort_reversed;
				list_assets_sorted.Reverse();
			}
		}).click();
		_initialized = true;
	}

	private void Update()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		if (!_initialized)
		{
			return;
		}
		_scroll_world_rect = _scroll_rect_transform.GetWorldRect();
		foreach (TAssetElementPlace list_place in list_places)
		{
			if (list_place.game_object_cache.activeSelf)
			{
				if ((Object)(object)list_place.element != (Object)null)
				{
					list_place.element.update();
				}
				checkVisible(list_place);
			}
		}
	}

	private void checkVisible(TAssetElementPlace pPlace)
	{
		if (((Component)pPlace).gameObject.activeSelf)
		{
			bool flag = isElementVisible(pPlace);
			if (!flag && pPlace.has_element)
			{
				pPlace.clear();
			}
			else if (flag && !pPlace.has_element)
			{
				TAsset pAsset = list_assets_sorted[((Transform)pPlace.rect_transform).GetSiblingIndex()];
				pPlace.setData(pAsset, element_prefab);
			}
		}
	}

	public void refresh()
	{
		if (_initialized)
		{
			setDataResorted();
		}
	}

	public bool isElementVisible(TAssetElementPlace pPlace)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		return ((Rect)(ref _scroll_world_rect)).Overlaps(pPlace.rect_transform.GetWorldRect());
	}

	protected void setDataResorted()
	{
		int num = list_assets_sorted.Count - 1;
		for (int i = 0; i < list_places.Count; i++)
		{
			TAssetElementPlace val = list_places[i];
			if (i > num)
			{
				val.game_object_cache.SetActive(false);
				val.allowed_for_search = false;
				continue;
			}
			val.game_object_cache.SetActive(true);
			val.allowed_for_search = true;
			if (isElementVisible(val) && val.has_element)
			{
				TAsset data = list_assets_sorted[i];
				val.element.setData(data);
			}
		}
		setDataSearched(search_input_field.text);
	}

	protected void checkReverseSort()
	{
		if (sorting_tab.getCurrentButton().getState() == SortButtonState.Down)
		{
			list_assets_sorted.Reverse();
		}
	}

	private void setDataSearched(string pValue)
	{
		if (!((Component)this).gameObject.activeSelf)
		{
			return;
		}
		pValue = pValue.ToLower();
		if (string.IsNullOrEmpty(pValue))
		{
			foreach (TAssetElementPlace list_place in list_places)
			{
				if (list_place.allowed_for_search)
				{
					list_place.game_object_cache.SetActive(true);
				}
			}
			return;
		}
		for (int i = 0; i < list_assets_sorted.Count; i++)
		{
			TAssetElementPlace val = list_places[i];
			if (val.allowed_for_search)
			{
				bool active = list_assets_sorted[i].id.ToLower().Contains(pValue);
				val.game_object_cache.SetActive(active);
			}
		}
	}
}
