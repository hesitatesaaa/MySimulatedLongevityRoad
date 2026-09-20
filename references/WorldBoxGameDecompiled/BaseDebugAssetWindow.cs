using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class BaseDebugAssetWindow<TAsset, TAssetElement> : TabbedWindow where TAsset : Asset where TAssetElement : BaseDebugAssetElement<TAsset>
{
	public static TAssetElement current_element;

	public SpriteElement sprite_element_prefab;

	public TAssetElement asset_debug_element;

	public Transform sprite_elements_parent;

	public SortingTab sorting_tab;

	public FieldInfoList field_infos;

	public PowerButton show_sprites_button;

	public GameObject hidden_sprites_placeholder;

	private List<FieldInfo> _sorted_fields;

	private List<FieldInfo> _sorting_fields;

	private List<FieldInfo> _default_sorting_fields;

	private bool _default_reversed;

	protected TAsset asset;

	private SortButton _default_sort_button;

	private bool _initialized;

	protected override void create()
	{
		base.create();
		asset = BaseDebugAssetElement<TAsset>.selected_asset;
		sorting_tab.addButton("ui/Icons/onomastics/onomastics_vowel_separator", "sort_by_alphabet", setDataResorted, delegate
		{
			_sorted_fields = _sorting_fields;
			_sorted_fields.Sort(sortByName);
			checkReverseSort();
		});
		sorting_tab.addButton("ui/Icons/onomastics/onomastics_consonant_separator", "sort_by_type", setDataResorted, delegate
		{
			_sorted_fields = _sorting_fields;
			_sorted_fields.Sort(sortByType);
			checkReverseSort();
		});
		_default_sort_button = sorting_tab.addButton("ui/Icons/actor_traits/iconClumsy", "default_sort", setDataResorted, delegate
		{
			_sorted_fields = _default_sorting_fields;
			if (sorting_tab.getCurrentButton().getState() == SortButtonState.Down || _default_reversed)
			{
				_default_reversed = !_default_reversed;
				_sorted_fields.Reverse();
			}
		});
	}

	private void OnEnable()
	{
		asset = BaseDebugAssetElement<TAsset>.selected_asset;
		current_element = asset_debug_element;
		_initialized = false;
	}

	private void Update()
	{
		load();
		asset_debug_element.update();
	}

	private void load()
	{
		if (!_initialized)
		{
			_initialized = true;
			scroll_window.titleText.text = asset.id;
			asset_debug_element.setData(asset);
			initSprites();
			field_infos.init<TAsset>();
			field_infos.setData(asset);
			_sorted_fields = new List<FieldInfo>(field_infos.field_infos);
			_sorting_fields = new List<FieldInfo>(field_infos.field_infos);
			_default_sorting_fields = new List<FieldInfo>(field_infos.field_infos);
			_default_sort_button.click();
		}
	}

	protected virtual void initSprites()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		foreach (Transform item in sprite_elements_parent)
		{
			Object.Destroy((Object)(object)((Component)item).gameObject);
		}
	}

	public void clickShowAllSprites()
	{
		GameObject gameObject = ((Component)sprite_elements_parent).gameObject;
		bool flag = !gameObject.activeSelf;
		gameObject.SetActive(flag);
		hidden_sprites_placeholder.SetActive(!flag);
		if (flag)
		{
			show_sprites_button.icon.sprite = SpriteTextureLoader.getSprite("ui/icons/IconOn");
		}
		else
		{
			show_sprites_button.icon.sprite = SpriteTextureLoader.getSprite("ui/icons/IconOff");
		}
	}

	private void setDataResorted()
	{
		field_infos.clear();
		Dictionary<string, FieldInfoListItem> fields_collection_data = field_infos.fields_collection_data;
		fields_collection_data.Clear();
		for (int i = 0; i < _sorted_fields.Count; i++)
		{
			FieldInfoListItem fieldData = field_infos.getFieldData(_sorted_fields[i], asset);
			fields_collection_data.Add(fieldData.field_name, fieldData);
			field_infos.addRow(fieldData.field_name, fieldData.field_value);
		}
		field_infos.setDataSearched(field_infos.search_input_field.text);
	}

	private void checkReverseSort()
	{
		if (sorting_tab.getCurrentButton().getState() == SortButtonState.Down)
		{
			_sorted_fields.Reverse();
		}
	}

	private int sortByName(FieldInfo pObject1, FieldInfo pObject2)
	{
		return string.Compare(pObject1.Name, pObject2.Name, StringComparison.InvariantCulture);
	}

	private int sortByType(FieldInfo pObject1, FieldInfo pObject2)
	{
		return string.Compare(pObject1.FieldType.Name, pObject2.FieldType.Name, StringComparison.InvariantCulture);
	}
}
