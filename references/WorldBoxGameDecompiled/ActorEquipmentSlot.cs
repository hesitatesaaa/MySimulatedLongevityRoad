using UnityEngine;

public class ActorEquipmentSlot
{
	private Item _item;

	public EquipmentType type;

	public bool is_empty => isEmpty();

	public ActorEquipmentSlot(EquipmentType pType = EquipmentType.Armor)
	{
		type = pType;
	}

	public Item getItem()
	{
		return _item;
	}

	public bool isEmpty()
	{
		if (_item == null)
		{
			return true;
		}
		if (_item.shouldbe_removed)
		{
			Debug.LogError((object)"Item should be removed but it's still in the slot!");
			return true;
		}
		return false;
	}

	public void takeAwayItem()
	{
		if (!isEmpty())
		{
			_item.clearUnit();
			_item = null;
		}
	}

	public void setEmptyDebug()
	{
		_item = null;
	}

	internal void setItem(Item pItem, Actor pActor)
	{
		if (!isEmpty())
		{
			takeAwayItem();
		}
		_item = pItem;
		_item.setUnitHasIt(pActor);
		pActor.setStatsDirty();
	}

	public bool canChangeSlot()
	{
		if (!isEmpty())
		{
			return !getItem().isCursed();
		}
		return true;
	}
}
