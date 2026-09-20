using System;
using System.Collections.Generic;
using UnityEngine;

public class BannersMetaContainer<TMetaBanner, TMetaObject, TMetaData> : WindowMetaElementBase where TMetaBanner : BannerGeneric<TMetaObject, TMetaData> where TMetaObject : CoreSystemObject<TMetaData> where TMetaData : BaseSystemData
{
	[SerializeField]
	private TMetaBanner _prefab;

	[SerializeField]
	private Transform _container;

	private StatsWindow _window;

	private ObjectPoolGenericMono<TMetaBanner> _pool_elements;

	protected override void Awake()
	{
		base.Awake();
		_pool_elements = new ObjectPoolGenericMono<TMetaBanner>(_prefab, _container);
	}

	protected override void OnEnable()
	{
	}

	public void update(NanoObject pNano)
	{
		clear();
		_pool_elements.clear();
		showContent(pNano);
	}

	private void showContent(NanoObject pNano)
	{
		using ListPool<TMetaObject> listPool = new ListPool<TMetaObject>(getMetaList(pNano as IMetaObject));
		for (int i = 0; i < listPool.Count; i++)
		{
			TMetaObject val = listPool[i];
			track_objects.Add(val);
			showElement(val);
		}
	}

	private void showElement(TMetaObject pMeta)
	{
		TMetaBanner next = _pool_elements.getNext();
		next.enable_tab_show_click = true;
		next.enable_default_click = false;
		if (!((Component)(object)next).HasComponent<DraggableLayoutElement>())
		{
			((Component)(object)next).AddComponent<DraggableLayoutElement>();
		}
		next.load(pMeta);
	}

	protected virtual IEnumerable<TMetaObject> getMetaList(IMetaObject pMeta)
	{
		throw new NotImplementedException();
	}
}
