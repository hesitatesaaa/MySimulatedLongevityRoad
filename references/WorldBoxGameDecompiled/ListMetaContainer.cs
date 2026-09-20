using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ListMetaContainer<TListElement, TMetaObject, TMetaData> : WindowMetaElementBase where TListElement : WindowListElementBase<TMetaObject, TMetaData> where TMetaObject : CoreSystemObject<TMetaData> where TMetaData : BaseSystemData
{
	[SerializeField]
	private TListElement _prefab;

	[SerializeField]
	private Transform _container;

	private StatsWindow _window;

	private ObjectPoolGenericMono<TListElement> _pool_elements;

	protected override void Awake()
	{
		_window = ((Component)this).GetComponentInParent<StatsWindow>();
		_pool_elements = new ObjectPoolGenericMono<TListElement>(_prefab, _container);
		base.Awake();
	}

	protected override IEnumerator showContent()
	{
		using ListPool<TMetaObject> tListObjects = new ListPool<TMetaObject>(getMetaList());
		track_objects.AddRange(tListObjects);
		tListObjects.Sort(getSorting());
		for (int i = 0; i < tListObjects.Count; i++)
		{
			TMetaObject tObject = tListObjects[i];
			yield return (object)new WaitForSecondsRealtime(0.025f);
			showElement(tObject);
		}
	}

	private void showElement(TMetaObject pMeta)
	{
		_pool_elements.getNext().show(pMeta);
	}

	protected override void clear()
	{
		_pool_elements.clear();
		base.clear();
	}

	protected IMetaObject getMeta()
	{
		return AssetManager.meta_type_library.getAsset(_window.meta_type).get_selected() as IMetaObject;
	}

	protected virtual IEnumerable<TMetaObject> getMetaList()
	{
		throw new NotImplementedException();
	}

	protected virtual Comparison<TMetaObject> getSorting()
	{
		throw new NotImplementedException();
	}
}
