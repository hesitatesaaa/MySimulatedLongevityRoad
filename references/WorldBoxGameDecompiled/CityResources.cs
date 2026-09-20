using System;
using System.Collections.Generic;

[Serializable]
public class CityResources : IDisposable
{
	[NonSerialized]
	private Dictionary<string, CityStorageSlot> _resources = new Dictionary<string, CityStorageSlot>();

	[NonSerialized]
	private List<CityStorageSlot> _list_food = new List<CityStorageSlot>();

	[NonSerialized]
	private List<CityStorageSlot> _list_other = new List<CityStorageSlot>();

	public List<CityStorageSlot> saved_resources;

	public void loadFromSave()
	{
		if (saved_resources == null)
		{
			return;
		}
		foreach (CityStorageSlot saved_resource in saved_resources)
		{
			if (AssetManager.resources.get(saved_resource.id) != null && saved_resource.amount >= 0)
			{
				saved_resource.create(saved_resource.id);
				putToDict(saved_resource);
			}
		}
	}

	public int get(string pRes)
	{
		if (_resources.TryGetValue(pRes, out var value))
		{
			return value.amount;
		}
		return 0;
	}

	public int change(string pRes, int pAmount = 1)
	{
		int num = 0;
		if (DebugConfig.isOn(DebugOption.CityInfiniteResources))
		{
			pAmount = 999;
		}
		if (_resources.TryGetValue(pRes, out var value))
		{
			value.amount += pAmount;
			if (value.amount > value.asset.maximum)
			{
				value.amount = value.asset.maximum;
			}
			return value.amount;
		}
		return addNew(pRes, pAmount);
	}

	private int addNew(string pResID, int pAmount)
	{
		CityStorageSlot cityStorageSlot = new CityStorageSlot(pResID);
		cityStorageSlot.amount = pAmount;
		putToDict(cityStorageSlot);
		return cityStorageSlot.amount;
	}

	public bool hasSpaceForResource(ResourceAsset pAsset)
	{
		if (get(pAsset.id) < pAsset.storage_max)
		{
			return true;
		}
		return false;
	}

	public bool hasResourcesForNewItems()
	{
		foreach (ResourceAsset strategic_resource_asset in AssetManager.resources.strategic_resource_assets)
		{
			if (get(strategic_resource_asset.id) > 10)
			{
				return true;
			}
		}
		return false;
	}

	public void set(string pRes, int pAmount)
	{
		if (_resources.TryGetValue(pRes, out var value))
		{
			value.amount = pAmount;
		}
		else
		{
			addNew(pRes, pAmount);
		}
	}

	private void putToDict(CityStorageSlot pRes)
	{
		if (!_resources.ContainsKey(pRes.id))
		{
			_resources.Add(pRes.id, pRes);
			if (pRes.asset.food)
			{
				_list_food.Add(pRes);
			}
			else
			{
				_list_other.Add(pRes);
			}
		}
	}

	public ResourceAsset getRandomSuitableFood(Subspecies pSubspecies, string pSpecificFood = null)
	{
		if (_list_food.Count == 0)
		{
			return null;
		}
		if (!string.IsNullOrEmpty(pSpecificFood) && get(pSpecificFood) > 0)
		{
			return AssetManager.resources.get(pSpecificFood);
		}
		HashSet<string> allowedFoodByDiet = pSubspecies.getAllowedFoodByDiet();
		ResourceAsset availableFoodAsset = getAvailableFoodAsset(_list_food, allowedFoodByDiet, pSort: true);
		if (availableFoodAsset == null)
		{
			availableFoodAsset = getAvailableFoodAsset(_list_other, allowedFoodByDiet, pSort: false);
		}
		return availableFoodAsset;
	}

	private ResourceAsset getAvailableFoodAsset(List<CityStorageSlot> pList, HashSet<string> pAllowedFood, bool pSort)
	{
		if (pSort)
		{
			pList.Sort(foodSorter);
		}
		for (int i = 0; i < pList.Count; i++)
		{
			CityStorageSlot cityStorageSlot = pList[i];
			if (cityStorageSlot.amount != 0 && pAllowedFood.Contains(cityStorageSlot.id))
			{
				return cityStorageSlot.asset;
			}
		}
		return null;
	}

	public int foodSorter(CityStorageSlot o1, CityStorageSlot o2)
	{
		return o2.amount.CompareTo(o1.amount);
	}

	public int countFood()
	{
		int num = 0;
		foreach (CityStorageSlot item in _list_food)
		{
			num += item.amount;
		}
		return num;
	}

	public ResourceAsset getRandomFoodAsset()
	{
		if (_list_food.Count == 0)
		{
			return null;
		}
		return _list_food.GetRandom().asset;
	}

	public void save()
	{
		saved_resources = new List<CityStorageSlot>();
		foreach (CityStorageSlot slot in getSlots())
		{
			if (slot.amount != 0)
			{
				saved_resources.Add(slot);
			}
		}
	}

	public IEnumerable<string> getKeys()
	{
		return _resources.Keys;
	}

	public IEnumerable<CityStorageSlot> getSlots()
	{
		return _resources.Values;
	}

	public void Dispose()
	{
		_resources.Clear();
		_list_food.Clear();
		_list_other.Clear();
		saved_resources?.Clear();
	}
}
