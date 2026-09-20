using System;
using System.Collections.Generic;

[Serializable]
public class CityEquipment : IDisposable
{
	internal Dictionary<EquipmentType, List<long>> items_dicts;

	public List<long> item_storage_weapons = new List<long>();

	public List<long> item_storage_helmets = new List<long>();

	public List<long> item_storage_armor = new List<long>();

	public List<long> item_storage_boots = new List<long>();

	public List<long> item_storage_rings = new List<long>();

	public List<long> item_storage_amulets = new List<long>();

	public CityEquipment()
	{
		init();
	}

	internal void init()
	{
		if (items_dicts == null)
		{
			items_dicts = new Dictionary<EquipmentType, List<long>>();
		}
		else
		{
			items_dicts.Clear();
		}
		items_dicts.Add(EquipmentType.Weapon, item_storage_weapons);
		items_dicts.Add(EquipmentType.Helmet, item_storage_helmets);
		items_dicts.Add(EquipmentType.Armor, item_storage_armor);
		items_dicts.Add(EquipmentType.Boots, item_storage_boots);
		items_dicts.Add(EquipmentType.Ring, item_storage_rings);
		items_dicts.Add(EquipmentType.Amulet, item_storage_amulets);
	}

	public void clearItems()
	{
		foreach (List<long> allEquipmentList in getAllEquipmentLists())
		{
			foreach (long item in allEquipmentList)
			{
				World.world.items.get(item)?.clearCity();
			}
		}
		clearCollections();
	}

	public int countItems()
	{
		int num = 0;
		foreach (List<long> allEquipmentList in getAllEquipmentLists())
		{
			num += allEquipmentList.Count;
		}
		return num;
	}

	public bool hasAnyItem()
	{
		foreach (List<long> allEquipmentList in getAllEquipmentLists())
		{
			if (allEquipmentList.Count > 0)
			{
				return true;
			}
		}
		return false;
	}

	public void addItem(City pCity, Item pItem, List<long> pList = null)
	{
		EquipmentAsset asset = pItem.getAsset();
		if (pList == null)
		{
			pList = getEquipmentList(asset.equipment_type);
		}
		pItem.setInCityStorage(pCity);
		pList.Add(pItem.id);
	}

	public List<long> getEquipmentList(EquipmentType pType)
	{
		return items_dicts[pType];
	}

	public IEnumerable<List<long>> getAllEquipmentLists()
	{
		foreach (List<long> value in items_dicts.Values)
		{
			yield return value;
		}
	}

	public void loadFromSave(City pCity)
	{
		init();
		foreach (List<long> allEquipmentList in getAllEquipmentLists())
		{
			allEquipmentList.RemoveAll(delegate(long pID2)
			{
				Item item = World.world.items.get(pID2);
				if (item == null)
				{
					return true;
				}
				return item.getAsset() == null;
			});
		}
		foreach (List<long> allEquipmentList2 in getAllEquipmentLists())
		{
			for (int num = 0; num < allEquipmentList2.Count; num++)
			{
				long pID = allEquipmentList2[num];
				World.world.items.get(pID).setInCityStorage(pCity);
			}
		}
	}

	public void Dispose()
	{
		items_dicts?.Clear();
		clearCollections();
	}

	private void clearCollections()
	{
		item_storage_weapons.Clear();
		item_storage_helmets.Clear();
		item_storage_armor.Clear();
		item_storage_boots.Clear();
		item_storage_rings.Clear();
		item_storage_amulets.Clear();
	}
}
