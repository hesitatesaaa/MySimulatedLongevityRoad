using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UnitEquipmentContainer : UnitElement
{
	[SerializeField]
	private EquipmentButton _prefab_equipment;

	[SerializeField]
	private Transform _grid;

	private ObjectPoolGenericMono<EquipmentButton> _pool_equipment;

	private Dictionary<Item, EquipmentButton> _items = new Dictionary<Item, EquipmentButton>();

	protected override void Awake()
	{
		_pool_equipment = new ObjectPoolGenericMono<EquipmentButton>(_prefab_equipment, _grid);
		base.Awake();
	}

	protected override IEnumerator showContent()
	{
		if (actor != null && actor.isAlive())
		{
			yield return loadEquipment();
		}
	}

	private IEnumerator loadEquipment(bool pAnimated = true)
	{
		using ListPool<Item> tListPool = new ListPool<Item>();
		if (actor.hasEquipment())
		{
			tListPool.AddRange(actor.equipment.getItems());
		}
		using ListPool<Item> tRemovedItems = new ListPool<Item>(_items.Keys.Except(tListPool));
		using ListPool<Item> tAddedItems = new ListPool<Item>(tListPool.Except(_items.Keys));
		foreach (ref Item item in tRemovedItems)
		{
			Item current = item;
			_pool_equipment.release(_items[current]);
			_items.Remove(current);
			if (pAnimated)
			{
				yield return (object)new WaitForEndOfFrame();
			}
		}
		bool flag = true;
		foreach (ref Item item2 in tAddedItems)
		{
			Item current2 = item2;
			loadEquipmentButton(current2);
			if (pAnimated & flag)
			{
				yield return (object)new WaitForEndOfFrame();
			}
			flag = false;
		}
	}

	private void loadEquipmentButton(Item pItem)
	{
		EquipmentButton next = _pool_equipment.getNext();
		next.load(pItem);
		_items[pItem] = next;
		AugmentationUnlockedAction pAction = ((IAugmentationsWindow<IEquipmentEditor>)unit_window).getEditor().reloadButtons;
		next.removeElementUnlockedAction(pAction);
		next.addElementUnlockedAction(pAction);
	}

	protected override void clear()
	{
		_items.Clear();
		_pool_equipment?.clear();
		base.clear();
	}

	protected override void clearInitial()
	{
		for (int i = 0; i < _grid.childCount; i++)
		{
			Transform child = _grid.GetChild(i);
			if (!(((Object)child).name == "Title"))
			{
				Object.Destroy((Object)(object)((Component)child).gameObject);
			}
		}
		base.clearInitial();
	}

	public void reloadEquipment(bool pAnimated)
	{
		((MonoBehaviour)this).StopAllCoroutines();
		((MonoBehaviour)this).StartCoroutine(loadEquipment(pAnimated));
	}
}
