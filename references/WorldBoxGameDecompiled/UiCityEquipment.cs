using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UiCityEquipment : CitySortableElement
{
	[SerializeField]
	private EquipmentType _equipment_type;

	[SerializeField]
	private EquipmentButton _prefab_equipment;

	private ObjectPoolGenericMono<EquipmentButton> _pool_equipment;

	private Dictionary<long, EquipmentButton> _equipment = new Dictionary<long, EquipmentButton>();

	protected override void Awake()
	{
		_pool_equipment = new ObjectPoolGenericMono<EquipmentButton>(_prefab_equipment, ((Component)this).transform);
		base.Awake();
	}

	protected override IEnumerator showContent()
	{
		_equipment.Clear();
		_pool_equipment.clear();
		using ListPool<long> tItemIds = new ListPool<long>(base.city.getEquipmentList(_equipment_type));
		foreach (ref long item in tItemIds)
		{
			long current = item;
			Item pItem = World.world.items.get(current);
			loadEquipmentButton(pItem, current);
		}
		yield return (object)new WaitForEndOfFrame();
	}

	private void loadEquipmentButton(Item pItem, long pItemID)
	{
		EquipmentButton next = _pool_equipment.getNext();
		next.load(pItem);
		_equipment[pItemID] = next;
	}

	protected override void onListChange()
	{
		List<long> equipmentList = base.city.getEquipmentList(_equipment_type);
		if (equipmentList.SetEquals(_equipment.Keys))
		{
			equipmentList.Sort((long a, long b) => ((Component)_equipment[a]).transform.GetSiblingIndex().CompareTo(((Component)_equipment[b]).transform.GetSiblingIndex()));
		}
	}

	protected override void clear()
	{
		_equipment.Clear();
		_pool_equipment.clear();
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
