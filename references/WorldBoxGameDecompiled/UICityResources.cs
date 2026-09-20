using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UICityResources : CitySortableElement
{
	[SerializeField]
	private ResType[] _res_types;

	[SerializeField]
	private ButtonResource _prefab_resource;

	private ObjectPoolGenericMono<ButtonResource> _pool_resources;

	private Dictionary<CityStorageSlot, ButtonResource> _loaded_slots = new Dictionary<CityStorageSlot, ButtonResource>();

	protected override void Awake()
	{
		_pool_resources = new ObjectPoolGenericMono<ButtonResource>(_prefab_resource, ((Component)this).transform);
		base.Awake();
	}

	protected override IEnumerator showContent()
	{
		showResources();
		yield return (object)new WaitForEndOfFrame();
	}

	protected void showResources()
	{
		_loaded_slots.Clear();
		_pool_resources.clear();
		if (!base.city.hasStorages())
		{
			return;
		}
		using ListPool<CityStorageSlot> listPool = base.city.getTotalResourceSlots(_res_types);
		foreach (ref CityStorageSlot item in listPool)
		{
			CityStorageSlot current = item;
			loadResource(current);
		}
	}

	private void loadResource(CityStorageSlot pSlot)
	{
		ButtonResource next = _pool_resources.getNext();
		next.load(pSlot.asset, pSlot.amount);
		_loaded_slots[pSlot] = next;
	}

	protected override void onListChange()
	{
		if (!base.city.hasStorages())
		{
			return;
		}
		using ListPool<CityStorageSlot> pList = base.city.getTotalResourceSlots(_res_types);
		if (!pList.SetEquals(_loaded_slots.Keys))
		{
			return;
		}
		using ListPool<CityStorageSlot> listPool = new ListPool<CityStorageSlot>(_loaded_slots.Keys);
		listPool.Sort((CityStorageSlot a, CityStorageSlot b) => a.asset.order.CompareTo(b.asset.order));
		listPool.RemoveAll((CityStorageSlot pSlot) => pSlot.amount == 0);
		using ListPool<int> listPool2 = new ListPool<int>(listPool.Count);
		foreach (ref CityStorageSlot item in listPool)
		{
			CityStorageSlot current = item;
			listPool2.Add(current.asset.order);
		}
		listPool.Sort((CityStorageSlot a, CityStorageSlot b) => ((Component)_loaded_slots[a]).transform.GetSiblingIndex().CompareTo(((Component)_loaded_slots[b]).transform.GetSiblingIndex()));
		for (int num = 0; num < listPool2.Count; num++)
		{
			listPool[num].asset.order = listPool2[num];
		}
	}

	protected override void clear()
	{
		_loaded_slots.Clear();
		_pool_resources.clear();
		base.clear();
	}

	protected override void clearInitial()
	{
		for (int i = 0; i < ((Component)this).transform.childCount; i++)
		{
			Transform child = ((Component)this).transform.GetChild(i);
			if (!(((Object)child).name == "Title"))
			{
				Object.Destroy((Object)(object)((Component)child).gameObject);
			}
		}
		base.clearInitial();
	}
}
