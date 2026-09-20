using System.Collections;
using System.Collections.Generic;

public class ActorEquipment : IEnumerable<ActorEquipmentSlot>, IEnumerable
{
	public const int SLOTS_AMOUNT = 6;

	public const string NONE = "none";

	public ActorEquipmentSlot helmet = new ActorEquipmentSlot(EquipmentType.Helmet);

	public ActorEquipmentSlot armor = new ActorEquipmentSlot();

	public ActorEquipmentSlot weapon = new ActorEquipmentSlot(EquipmentType.Weapon);

	public ActorEquipmentSlot boots = new ActorEquipmentSlot(EquipmentType.Boots);

	public ActorEquipmentSlot ring = new ActorEquipmentSlot(EquipmentType.Ring);

	public ActorEquipmentSlot amulet = new ActorEquipmentSlot(EquipmentType.Amulet);

	private Dictionary<EquipmentType, ActorEquipmentSlot> _dictionary;

	public ActorEquipment()
	{
		initDictionary();
	}

	public void destroyAllEquipment()
	{
		foreach (ActorEquipmentSlot value in _dictionary.Values)
		{
			if (!value.isEmpty())
			{
				value.takeAwayItem();
			}
		}
	}

	private void initDictionary()
	{
		_dictionary = new Dictionary<EquipmentType, ActorEquipmentSlot>();
		_dictionary.Add(EquipmentType.Helmet, helmet);
		_dictionary.Add(EquipmentType.Armor, armor);
		_dictionary.Add(EquipmentType.Weapon, weapon);
		_dictionary.Add(EquipmentType.Boots, boots);
		_dictionary.Add(EquipmentType.Ring, ring);
		_dictionary.Add(EquipmentType.Amulet, amulet);
	}

	public bool hasItems()
	{
		foreach (ActorEquipmentSlot value in _dictionary.Values)
		{
			if (!value.isEmpty())
			{
				return true;
			}
		}
		return false;
	}

	public IEnumerable<Item> getItems()
	{
		foreach (ActorEquipmentSlot value in _dictionary.Values)
		{
			if (!value.isEmpty())
			{
				yield return value.getItem();
			}
		}
	}

	public List<long> getDataForSave()
	{
		List<long> list = new List<long>();
		using (IEnumerator<ActorEquipmentSlot> enumerator = GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				ActorEquipmentSlot current = enumerator.Current;
				list.Add(current.getItem().data.id);
			}
		}
		if (list.Count == 0)
		{
			return null;
		}
		return list;
	}

	public void load(List<long> pList, Actor pActor)
	{
		if (pList == null || pList.Count == 0)
		{
			return;
		}
		foreach (long p in pList)
		{
			Item item = World.world.items.get(p);
			if (item != null)
			{
				EquipmentAsset asset = item.getAsset();
				if (asset != null)
				{
					getSlot(asset.equipment_type).setItem(item, pActor);
				}
			}
		}
		initDictionary();
	}

	public void setItem(Item pItem, Actor pActor)
	{
		getSlot(pItem.getAsset().equipment_type).setItem(pItem, pActor);
	}

	public ActorEquipmentSlot getSlot(EquipmentType pType)
	{
		return _dictionary[pType];
	}

	public IEnumerator<ActorEquipmentSlot> GetEnumerator()
	{
		foreach (ActorEquipmentSlot value in _dictionary.Values)
		{
			if (!value.isEmpty())
			{
				yield return value;
			}
		}
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}
