using System;
using System.Collections.Generic;
using UnityPools;

[Serializable]
public class CityBuildOrderAsset : Asset
{
	[NonSerialized]
	public string[] list_for_generation;

	public List<BuildOrder> list = new List<BuildOrder>();

	public void addUpgrade(string pID, int pLimitType = 0, int pPop = 0, int pBuildings = 0, bool pCheckFullVillage = false, bool pZonesCheck = false, int pMinZones = 0)
	{
		addBuilding(pID, pLimitType, pPop, pBuildings, pCheckFullVillage, pZonesCheck, pMinZones).upgrade = true;
	}

	public BuildOrder addBuilding(string pID, int pLimitType = 0, int pPop = 0, int pBuildings = 0, bool pCheckFullVillage = false, bool pCheckHouseLimit = false, int pMinZones = 0)
	{
		BuildOrder buildOrder = new BuildOrder();
		buildOrder.id = pID;
		buildOrder.limit_type = pLimitType;
		buildOrder.required_pop = pPop;
		buildOrder.required_buildings = pBuildings;
		buildOrder.check_full_village = pCheckFullVillage;
		buildOrder.check_house_limit = pCheckHouseLimit;
		buildOrder.min_zones = pMinZones;
		list.Add(buildOrder);
		BuildOrderLibrary.b = buildOrder;
		return buildOrder;
	}

	public void prepareForAssetGeneration()
	{
		HashSet<string> hashSet = UnsafeCollectionPool<HashSet<string>, string>.Get();
		foreach (BuildOrder item in list)
		{
			hashSet.Add(item.id);
			if (item.requirements_orders != null)
			{
				hashSet.UnionWith(item.requirements_orders);
			}
		}
		list_for_generation = hashSet.ToArray();
		UnsafeCollectionPool<HashSet<string>, string>.Release(hashSet);
	}
}
