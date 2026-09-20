using System;
using System.Collections.Generic;
using UnityEngine;

public class MultiBannerPool
{
	private Dictionary<string, ObjectPoolGenericMono<MonoBehaviour>> _pool_banners;

	private Transform _pool_container;

	private Transform _prefab_area;

	public MultiBannerPool(Transform pPoolContainer)
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Expected O, but got Unknown
		base._002Ector();
		_pool_banners = new Dictionary<string, ObjectPoolGenericMono<MonoBehaviour>>();
		_pool_container = pPoolContainer;
		GameObject val = new GameObject("PrefabArea", new Type[1] { typeof(RectTransform) });
		val.transform.SetParent(_pool_container);
		_prefab_area = val.transform;
		((Component)_prefab_area).gameObject.SetActive(false);
	}

	public IBanner getNext(NanoObject pObject)
	{
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Expected O, but got Unknown
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Expected O, but got Unknown
		string type = pObject.getType();
		MetaCustomizationAsset metaCustomizationAsset = AssetManager.meta_customization_library.get(type);
		if (!_pool_banners.TryGetValue(type, out var value))
		{
			GameObject val = new GameObject("BannerArea " + type, new Type[1] { typeof(RectTransform) });
			val.transform.SetParent(_pool_container, false);
			MonoBehaviour val2 = (MonoBehaviour)metaCustomizationAsset.get_banner(metaCustomizationAsset, pObject, _prefab_area);
			((Object)((Component)val2).gameObject).name = type;
			_pool_banners.Add(type, new ObjectPoolGenericMono<MonoBehaviour>(val2, val.transform));
			value = _pool_banners[type];
		}
		return value.getNext() as IBanner;
	}

	public void release(IBanner pItem)
	{
		getItemPool(pItem).release((MonoBehaviour)((pItem is MonoBehaviour) ? pItem : null));
	}

	public void resetParent(IBanner pItem)
	{
		getItemPool(pItem).resetParent((MonoBehaviour)((pItem is MonoBehaviour) ? pItem : null));
	}

	private ObjectPoolGenericMono<MonoBehaviour> getItemPool(IBanner pItem)
	{
		MetaCustomizationAsset meta_asset = pItem.meta_asset;
		if (_pool_banners.TryGetValue(meta_asset.id, out var value))
		{
			return value;
		}
		return null;
	}

	public void clear()
	{
		foreach (ObjectPoolGenericMono<MonoBehaviour> value in _pool_banners.Values)
		{
			value.clear();
			value.resetParent();
		}
	}
}
