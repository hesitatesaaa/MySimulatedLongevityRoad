using System.Collections.Generic;

public static class ItemCrafting
{
	private static readonly EquipmentType[] list_equipments = new EquipmentType[5]
	{
		EquipmentType.Helmet,
		EquipmentType.Armor,
		EquipmentType.Boots,
		EquipmentType.Amulet,
		EquipmentType.Ring
	};

	public static bool tryToCraftRandomWeapon(Actor pActor, City pCity)
	{
		int num = pActor.asset.item_making_skill;
		if (pActor.hasCulture() && pActor.culture.hasTrait("weaponsmith_mastery"))
		{
			num += CultureTraitLibrary.getValue("weaponsmith_mastery");
		}
		return craftItem(pActor, pActor.getName(), EquipmentType.Weapon, num, pCity);
	}

	public static bool tryToCraftRandomArmor(Actor pActor, City pCity)
	{
		int num = pActor.asset.item_making_skill;
		if (pActor.hasCulture() && pActor.culture.hasTrait("armorsmith_mastery"))
		{
			num += CultureTraitLibrary.getValue("armorsmith_mastery");
		}
		EquipmentType random = list_equipments.GetRandom();
		return craftItem(pActor, pActor.getName(), random, num, pCity);
	}

	public static bool tryToCraftRandomEquipment(Actor pActor, City pCity)
	{
		bool num = tryToCraftRandomArmor(pActor, pCity);
		bool flag = tryToCraftRandomWeapon(pActor, pCity);
		return num | flag;
	}

	public static bool craftItem(Actor pActor, string pCreatorName, EquipmentType pType, int pTries, City pCity)
	{
		string text = null;
		if (pType == EquipmentType.Weapon)
		{
			if (pActor.hasCulture())
			{
				text = pActor.culture.getPreferredWeaponSubtypeIDs();
			}
			if (string.IsNullOrEmpty(text))
			{
				text = ItemLibrary.default_weapon_pool.GetRandom();
			}
		}
		else
		{
			text = AssetManager.items.getEquipmentType(pType);
		}
		EquipmentAsset equipmentAsset = null;
		ActorEquipmentSlot slot = pActor.equipment.getSlot(pType);
		Item item = slot.getItem();
		if (item != null && item.isCursed())
		{
			return false;
		}
		int pCurrentItemValue = item?.asset.equipment_value ?? 0;
		if (pType == EquipmentType.Weapon && pActor.hasCulture() && pActor.culture.hasPreferredWeaponsToCraft() && Randy.randomBool())
		{
			equipmentAsset = getItemAssetToCraft(pActor, pActor.culture.getPreferredWeaponAssets(), pCity, pCurrentItemValue, pShuffle: true);
		}
		if (equipmentAsset == null)
		{
			List<EquipmentAsset> pItemList = AssetManager.items.equipment_by_subtypes[text];
			equipmentAsset = getItemAssetToCraft(pActor, pItemList, pCity, pCurrentItemValue);
		}
		if (equipmentAsset == null)
		{
			return false;
		}
		Item pItem = World.world.items.generateItem(equipmentAsset, pActor.kingdom, pCreatorName, pTries, pActor);
		if (slot.isEmpty())
		{
			slot.setItem(pItem, pActor);
		}
		else
		{
			Item item2 = slot.getItem();
			slot.takeAwayItem();
			pCity.tryToPutItem(item2);
			slot.setItem(pItem, pActor);
		}
		pActor.spendMoney(equipmentAsset.get_total_cost);
		if (equipmentAsset.cost_resource_id_1 != "none")
		{
			pCity.takeResource(equipmentAsset.cost_resource_id_1, equipmentAsset.cost_resource_1);
		}
		if (equipmentAsset.cost_resource_id_2 != "none")
		{
			pCity.takeResource(equipmentAsset.cost_resource_id_2, equipmentAsset.cost_resource_2);
		}
		return true;
	}

	public static EquipmentAsset getItemAssetToCraft(Actor pActor, List<EquipmentAsset> pItemList, City pCity, int pCurrentItemValue, bool pShuffle = false)
	{
		if (pShuffle)
		{
			pItemList.Shuffle();
		}
		for (int num = pItemList.Count - 1; num >= 0; num--)
		{
			EquipmentAsset equipmentAsset = pItemList[num];
			if (equipmentAsset.equipment_value > pCurrentItemValue && hasEnoughResourcesToCraft(pActor, equipmentAsset, pCity))
			{
				return equipmentAsset;
			}
		}
		return null;
	}

	private static bool hasEnoughResourcesToCraft(Actor pActor, EquipmentAsset pAsset, City pCity)
	{
		int get_total_cost = pAsset.get_total_cost;
		if (!pActor.hasEnoughMoney(get_total_cost))
		{
			return false;
		}
		if (pAsset.cost_resource_id_1 != "none" && pAsset.cost_resource_1 > pCity.getResourcesAmount(pAsset.cost_resource_id_1))
		{
			return false;
		}
		if (pAsset.cost_resource_id_2 != "none" && pAsset.cost_resource_2 > pCity.getResourcesAmount(pAsset.cost_resource_id_2))
		{
			return false;
		}
		return true;
	}
}
