using System;
using System.Collections.Generic;
using UnityEngine;
using UnityPools;

public class ItemManager : CoreSystemManager<Item, ItemData>
{
	private HashSet<string> unique_legendary_names = new HashSet<string>();

	private List<Item> _to_remove = new List<Item>();

	private bool _dirty;

	public ItemManager()
	{
		type_id = "item";
		MapBox.on_world_loaded = (Action)Delegate.Combine(MapBox.on_world_loaded, (Action)delegate
		{
			diagnostic();
		});
	}

	public bool isDirty()
	{
		return _dirty;
	}

	public void setDirty()
	{
		_dirty = true;
	}

	public Item newItem(EquipmentAsset pAsset)
	{
		Item item = newObject();
		item.newItem(pAsset);
		return item;
	}

	public void diagnostic()
	{
		Dictionary<Item, int> dictionary = UnsafeCollectionPool<Dictionary<Item, int>, KeyValuePair<Item, int>>.Get();
		Dictionary<Item, int> dictionary2 = UnsafeCollectionPool<Dictionary<Item, int>, KeyValuePair<Item, int>>.Get();
		foreach (City city in World.world.cities)
		{
			foreach (List<long> allEquipmentList in city.data.equipment.getAllEquipmentLists())
			{
				foreach (long item3 in allEquipmentList)
				{
					Item item = get(item3);
					if (item != null)
					{
						if (!dictionary.ContainsKey(item))
						{
							dictionary.Add(item, 0);
						}
						dictionary[item]++;
					}
				}
			}
		}
		foreach (Actor unit in World.world.units)
		{
			if (!unit.hasEquipment())
			{
				continue;
			}
			foreach (ActorEquipmentSlot item4 in unit.equipment)
			{
				Item item2 = item4.getItem();
				if (item2 != null)
				{
					if (!dictionary2.ContainsKey(item2))
					{
						dictionary2.Add(item2, 0);
					}
					dictionary2[item2]++;
				}
			}
		}
		foreach (Item item5 in list)
		{
			if (dictionary.ContainsKey(item5) && dictionary2.ContainsKey(item5))
			{
				Debug.LogError((object)("Item Error. Item in city and in unit " + item5.id));
			}
		}
		UnsafeCollectionPool<Dictionary<Item, int>, KeyValuePair<Item, int>>.Release(dictionary);
		UnsafeCollectionPool<Dictionary<Item, int>, KeyValuePair<Item, int>>.Release(dictionary2);
	}

	public override Item loadObject(ItemData pData)
	{
		if (AssetManager.items.get(pData.asset_id) == null)
		{
			return null;
		}
		return base.loadObject(pData);
	}

	private List<ItemModAsset> getModPool(EquipmentType pType)
	{
		switch (pType)
		{
		case EquipmentType.Ring:
		case EquipmentType.Amulet:
			return AssetManager.items_modifiers.pools["accessory"];
		case EquipmentType.Weapon:
			return AssetManager.items_modifiers.pools["weapon"];
		default:
			return AssetManager.items_modifiers.pools["armor"];
		}
	}

	private ItemModAsset getRandomModFromPool(EquipmentType pType)
	{
		return getModPool(pType).GetRandom();
	}

	public void generateModsFor(Item pItem, int pTries = 1, Actor pActor = null, bool pAddName = true)
	{
		EquipmentAsset asset = pItem.getAsset();
		using ListPool<string> listPool = new ListPool<string>();
		for (int i = 0; i < pTries; i++)
		{
			if (Randy.randomBool())
			{
				continue;
			}
			ItemModAsset randomModFromPool = getRandomModFromPool(asset.equipment_type);
			if (randomModFromPool.mod_can_be_given)
			{
				bool flag = tryToAddMod(pItem, randomModFromPool);
				if ((pAddName & flag) && checkModName(pItem, randomModFromPool, asset, pActor, out var pName))
				{
					listPool.Add(pName);
				}
			}
		}
		if (asset.item_modifiers != null)
		{
			for (int j = 0; j < asset.item_modifiers.Length; j++)
			{
				ItemModAsset pModAsset = asset.item_modifiers[j];
				bool flag2 = tryToAddMod(pItem, pModAsset);
				if ((pAddName & flag2) && checkModName(pItem, pModAsset, asset, pActor, out var pName2))
				{
					listPool.Add(pName2);
				}
			}
		}
		listPool.RemoveAll((string tName) => string.IsNullOrEmpty(tName));
		if (listPool.Count > 0)
		{
			pItem.setName(Randy.getRandom(listPool));
		}
	}

	public Item generateItem(EquipmentAsset pItemAsset, Kingdom pKingdom = null, string pWho = null, int pTries = 1, Actor pActor = null, int pFakeCreationYear = 0, bool pByPlayer = false)
	{
		Item item = newItem(pItemAsset);
		generateModsFor(item, pTries, pActor);
		item.data.asset_id = pItemAsset.id;
		item.data.by = pWho;
		if (!pByPlayer && !pActor.isRekt() && pActor.name == pWho)
		{
			item.data.creator_id = pActor.getID();
		}
		else
		{
			item.data.creator_id = -1L;
		}
		item.created_time_unscaled = Time.time;
		item.data.created_time -= (float)pFakeCreationYear * 60f;
		item.data.created_by_player = pByPlayer;
		if (pKingdom != null)
		{
			item.data.byColor = pKingdom.getColor().color_text;
			item.data.creator_kingdom_id = pKingdom.id;
			item.data.from = pKingdom.name;
			item.data.fromColor = pKingdom.getColor().color_text;
		}
		item.initItem();
		return item;
	}

	public override void removeObject(Item pObject)
	{
		base.removeObject(pObject);
		pObject.setShouldBeRemoved();
	}

	public override void clear()
	{
		base.clear();
		unique_legendary_names.Clear();
	}

	private bool tryToAddMod(Item pItem, ItemModAsset pModAsset)
	{
		return pItem.addMod(pModAsset);
	}

	private bool checkModName(Item pItem, ItemModAsset pModAsset, EquipmentAsset pItemAsset, Actor pActor, out string pName)
	{
		pName = null;
		if (pModAsset.quality == Rarity.R3_Legendary)
		{
			int num = 0;
			while (string.IsNullOrEmpty(pName) || unique_legendary_names.Contains(pName))
			{
				string randomNameTemplate = pItemAsset.getRandomNameTemplate(pActor);
				NameGeneratorAsset pAsset = AssetManager.name_generator.get(randomNameTemplate);
				pName = NameGenerator.generateNameFromTemplate(pAsset, pActor);
				if (++num > 100)
				{
					unique_legendary_names.Clear();
				}
			}
			return true;
		}
		return false;
	}

	public override void checkDeadObjects()
	{
		base.checkDeadObjects();
		if (!isDirty())
		{
			return;
		}
		using (IEnumerator<Item> enumerator = GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				Item current = enumerator.Current;
				if (current.isReadyForRemoval())
				{
					_to_remove.Add(current);
				}
			}
		}
		foreach (Item item in _to_remove)
		{
			removeObject(item);
		}
		_to_remove.Clear();
		_dirty = false;
	}
}
