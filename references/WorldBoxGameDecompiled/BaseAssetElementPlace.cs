using UnityEngine;
using UnityEngine.UI;

public class BaseAssetElementPlace<TAsset, TAssetElement> : MonoBehaviour where TAsset : Asset where TAssetElement : BaseDebugAssetElement<TAsset>
{
	public GameObject game_object_cache;

	public RectTransform rect_transform;

	public LayoutElement layout_element;

	public bool has_element;

	public TAssetElement element;

	public GameObject element_game_object_cache;

	public bool allowed_for_search = true;

	public void clear()
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		if (has_element)
		{
			LayoutElement obj = layout_element;
			Rect rect = element.rect_transform.rect;
			obj.minHeight = ((Rect)(ref rect)).height;
			Object.Destroy((Object)(object)element_game_object_cache);
			element_game_object_cache = null;
			element = null;
			has_element = false;
		}
	}

	public void setData(TAsset pAsset, TAssetElement pPrefab)
	{
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		if (has_element)
		{
			clear();
		}
		layout_element.minHeight = -1f;
		TAssetElement val = Object.Instantiate<TAssetElement>(pPrefab, (Transform)(object)rect_transform);
		val.setData(pAsset);
		((Transform)val.rect_transform).localScale = Vector3.one;
		element = val;
		element_game_object_cache = ((Component)val).gameObject;
		has_element = true;
	}
}
