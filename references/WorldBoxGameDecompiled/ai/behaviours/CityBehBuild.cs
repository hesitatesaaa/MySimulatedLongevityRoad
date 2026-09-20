using System.Collections.Generic;
using UnityEngine;
using UnityPools;
using tools;

namespace ai.behaviours;

public class CityBehBuild : BehaviourActionCity
{
	private static readonly List<BuildOrder> _possible_buildings = new List<BuildOrder>();

	private static readonly List<BuildOrder> _possible_buildings_no_resources = new List<BuildOrder>();

	private static readonly List<TileZone> _possible_zones = new List<TileZone>();

	public override bool shouldRetry(City pCity)
	{
		return false;
	}

	public override BehResult execute(City pCity)
	{
		if (pCity.timer_build > 0f)
		{
			return BehResult.Continue;
		}
		if (!DebugConfig.isOn(DebugOption.SystemBuildTick))
		{
			return BehResult.Continue;
		}
		if (pCity.isGettingCaptured())
		{
			return BehResult.Continue;
		}
		if (pCity.isInDanger())
		{
			return BehResult.Continue;
		}
		pCity.timer_build = 5f;
		buildTick(pCity);
		if (DebugConfig.isOn(DebugOption.CityFastUpgrades))
		{
			pCity.timer_build = 0.2f;
		}
		return BehResult.Continue;
	}

	public static bool buildTick(City pCity)
	{
		if (pCity.buildings.Count > 2 && pCity.hasCulture() && pCity.culture.canUseRoads())
		{
			Building random = pCity.buildings.GetRandom();
			if (random != null)
			{
				makeRoadsBuildings(pCity, random);
			}
		}
		if (pCity.isCityUnderDangerFire() && pCity.hasLeader() && !pCity.leader.isImmuneToFire())
		{
			return false;
		}
		if (pCity.under_construction_building == null)
		{
			foreach (Building building2 in pCity.buildings)
			{
				if (building2.isUnderConstruction())
				{
					pCity.under_construction_building = building2;
					break;
				}
			}
		}
		if (pCity.under_construction_building != null)
		{
			return false;
		}
		calcPossibleBuildings(pCity);
		if (DebugConfig.isOn(DebugOption.OverlayCity))
		{
			pCity._debug_last_possible_build_orders = string.Empty;
			pCity._debug_last_possible_build_orders_no_resources = string.Empty;
			pCity._debug_last_build_order_try = string.Empty;
		}
		if (_possible_buildings.Count == 0)
		{
			return false;
		}
		BuildOrder random2 = _possible_buildings.GetRandom();
		if (DebugConfig.isOn(DebugOption.OverlayCity))
		{
			foreach (BuildOrder possible_building in _possible_buildings)
			{
				pCity._debug_last_possible_build_orders = pCity._debug_last_possible_build_orders + (possible_building.upgrade ? "U-" : "") + possible_building.id + "; ";
			}
			foreach (BuildOrder possible_buildings_no_resource in _possible_buildings_no_resources)
			{
				pCity._debug_last_possible_build_orders_no_resources = pCity._debug_last_possible_build_orders_no_resources + (possible_buildings_no_resource.upgrade ? "U-" : "") + possible_buildings_no_resource.id + "; ";
			}
			pCity._debug_last_build_order_try = (random2.upgrade ? "U-" : "") + random2.id;
		}
		_possible_buildings_no_resources.Clear();
		_possible_buildings.Clear();
		if (random2.upgrade)
		{
			List<Building> buildingListOfID = pCity.getBuildingListOfID(random2.getBuildingAsset(pCity).id);
			if (buildingListOfID == null)
			{
				return false;
			}
			Building random3 = buildingListOfID.GetRandom();
			if (random3 == null)
			{
				return false;
			}
			return upgradeBuilding(random3, pCity);
		}
		Building building = tryToBuild(pCity, random2.getBuildingAsset(pCity));
		if (building == null)
		{
			return false;
		}
		if (DebugConfig.isOn(DebugOption.CityFastConstruction))
		{
			building?.updateBuild(1000);
			pCity.under_construction_building = null;
		}
		if (pCity.hasCulture())
		{
			pCity.culture.canUseRoads();
		}
		return true;
	}

	private void upgradeRandomBuilding(City pCity)
	{
		if (pCity.buildings.Count == 0)
		{
			return;
		}
		foreach (Building item in pCity.buildings.LoopRandom())
		{
			if (item.canBeUpgraded())
			{
				upgradeBuilding(item, pCity);
				break;
			}
		}
	}

	public static bool upgradeBuilding(Building pBuilding, City pCity)
	{
		string upgrade_to = pBuilding.asset.upgrade_to;
		BuildingAsset buildingAsset = AssetManager.buildings.get(upgrade_to);
		if (!pCity.hasEnoughResourcesFor(buildingAsset.cost))
		{
			return false;
		}
		bool num = pBuilding.upgradeBuilding();
		if (num)
		{
			pCity.spendResourcesForBuildingAsset(buildingAsset.cost);
		}
		return num;
	}

	public static void calcPossibleBuildings(City pCity)
	{
		ActorAsset actorAsset = pCity.getActorAsset();
		CityBuildOrderAsset cityBuildOrderAsset = AssetManager.city_build_orders.get(actorAsset.build_order_template_id);
		bool flag = DebugConfig.isOn(DebugOption.OverlayCity);
		foreach (BuildOrder item in cityBuildOrderAsset.list)
		{
			if (!canUseBuildAsset(item, pCity))
			{
				continue;
			}
			if (!hasResourcesForBuildAsset(item, pCity))
			{
				if (flag)
				{
					_possible_buildings_no_resources.Add(item);
				}
			}
			else
			{
				_possible_buildings.Add(item);
			}
		}
	}

	public static bool hasResourcesForBuildAsset(BuildOrder pBuildAsset, City pCity)
	{
		BuildingAsset buildingAsset = pBuildAsset.getBuildingAsset(pCity);
		if (pCity.hasEnoughResourcesFor(buildingAsset.cost))
		{
			return true;
		}
		return false;
	}

	public static bool canUseBuildAsset(BuildOrder pBuildAsset, City pCity)
	{
		BuildingAsset buildingAsset = pBuildAsset.getBuildingAsset(pCity);
		if (pBuildAsset.min_zones != 0 && pCity.zones.Count < pBuildAsset.min_zones)
		{
			return false;
		}
		int num = pCity.countBuildingsType(buildingAsset.type, pCountOnlyFinished: false);
		if (pBuildAsset.check_house_limit)
		{
			if (pCity.status.housing_free > 10)
			{
				return false;
			}
			int houseLimit = pCity.getHouseLimit();
			if (num >= houseLimit)
			{
				return false;
			}
		}
		int limitOfBuildingsType = pCity.getLimitOfBuildingsType(pBuildAsset);
		if (limitOfBuildingsType != 0 && num >= limitOfBuildingsType)
		{
			return false;
		}
		if (pBuildAsset.check_full_village && pCity.status.housing_free != 0)
		{
			return false;
		}
		if (pCity.status.population < pBuildAsset.required_pop)
		{
			return false;
		}
		if (pCity.buildings.Count < pBuildAsset.required_buildings)
		{
			return false;
		}
		if (!haveRequiredBuildings(pBuildAsset, pCity))
		{
			return false;
		}
		if (!haveRequiredBuildingTypes(pBuildAsset.requirements_types, pCity))
		{
			return false;
		}
		if (pBuildAsset.upgrade)
		{
			List<Building> buildingListOfID = pCity.getBuildingListOfID(buildingAsset.id);
			if (buildingListOfID == null || buildingListOfID.Count == 0)
			{
				return false;
			}
		}
		else if (buildingAsset.docks && getDockTile(pCity) == null)
		{
			return false;
		}
		return true;
	}

	private static bool haveRequiredBuildings(BuildOrder pOrder, City pCity)
	{
		if (pOrder.requirements_orders == null)
		{
			return true;
		}
		for (int i = 0; i < pOrder.requirements_orders.Length; i++)
		{
			string pOrderID = pOrder.requirements_orders[i];
			BuildingAsset buildingAsset = pOrder.getBuildingAsset(pCity, pOrderID);
			if (buildingAsset.id == buildingAsset.upgrade_to)
			{
				Debug.LogError((object)("(!) Building is set to be upgraded to self: " + buildingAsset.id));
				continue;
			}
			while (pCity.countBuildingsOfID(buildingAsset.id) == 0)
			{
				if (!buildingAsset.can_be_upgraded || string.IsNullOrEmpty(buildingAsset.upgrade_to))
				{
					return false;
				}
				buildingAsset = AssetManager.buildings.get(buildingAsset.upgrade_to);
			}
		}
		return true;
	}

	private static bool haveRequiredBuildingTypes(string[] pRequiredBuildingTypes, City pCity)
	{
		if (pRequiredBuildingTypes == null)
		{
			return true;
		}
		foreach (string pBuildingTypeID in pRequiredBuildingTypes)
		{
			if (!pCity.hasBuildingType(pBuildingTypeID))
			{
				return false;
			}
		}
		return true;
	}

	public static Building tryToBuild(City pCity, BuildingAsset pBuildingAsset)
	{
		if (!pCity.hasEnoughResourcesFor(pBuildingAsset.cost))
		{
			return null;
		}
		WorldTile worldTile = null;
		List<TileZone> possible_zones = _possible_zones;
		if (pBuildingAsset.type == "type_training_dummies")
		{
			worldTile = getTileTrainingDummy(pBuildingAsset, pCity);
		}
		else if (pBuildingAsset.docks)
		{
			worldTile = getDockTile(pCity);
		}
		else
		{
			if (pBuildingAsset.build_prefer_replace_house)
			{
				worldTile = getOnHouseTile(pCity, pBuildingAsset);
			}
			if (worldTile == null)
			{
				fillPossibleZones(pBuildingAsset, pCity, possible_zones);
			}
		}
		if (worldTile == null && possible_zones.Count > 0)
		{
			if (pBuildingAsset.build_place_center)
			{
				worldTile = tryToBuildInZones(possible_zones, pBuildingAsset, pCity, pForceCenterZone: true);
			}
			if (worldTile == null)
			{
				worldTile = tryToBuildInZones(possible_zones, pBuildingAsset, pCity);
			}
			if (worldTile != null && pBuildingAsset.needs_farms_ground && !checkFarmGround(worldTile, pBuildingAsset, pCity))
			{
				worldTile = null;
			}
		}
		possible_zones.Clear();
		if (worldTile == null)
		{
			return null;
		}
		Building building = (pCity.under_construction_building = BehaviourActionBase<City>.world.buildings.addBuilding(pBuildingAsset, worldTile));
		building.setUnderConstruction();
		pCity.spendResourcesForBuildingAsset(pBuildingAsset.cost);
		return building;
	}

	private static void fillPossibleZones(BuildingAsset pBuildingAsset, City pCity, List<TileZone> pPossibleZones)
	{
		for (int i = 0; i < pCity.zones.Count; i++)
		{
			TileZone tileZone = pCity.zones[i];
			if ((!pBuildingAsset.build_place_single || isZonesClear(tileZone, pBuildingAsset, pCity)) && (!pBuildingAsset.build_place_batch || !isNearbySingleBuilding(tileZone, pBuildingAsset, pCity)) && (!pBuildingAsset.build_place_borders || isZoneNearbyBorder(tileZone, pBuildingAsset, pCity)))
			{
				pPossibleZones.Add(tileZone);
			}
		}
	}

	public static WorldTile getOnHouseTile(City pCity, BuildingAsset pAsset)
	{
		foreach (Building item in pCity.buildings.LoopRandom())
		{
			if (item.asset.priority <= pAsset.priority && item.asset.hasHousingSlots() && isGoodTileForBuilding(item.current_tile, pAsset, pCity))
			{
				return item.current_tile;
			}
		}
		return null;
	}

	public static WorldTile tryToBuildInZones(List<TileZone> pList, BuildingAsset pBuildingAsset, City pCity, bool pForceCenterZone = false)
	{
		CityLayoutTilePlacement tilePlacementFromZone = pCity.getTilePlacementFromZone();
		bool num = pCity.hasCulture() && pCity.culture.hasTrait("buildings_spread");
		bool flag = pCity.hasSpecialTownPlans();
		WorldTile tile = pCity.getTile();
		TileZone pCenterZone = tile?.zone;
		WorldTile result = null;
		int num2 = int.MaxValue;
		bool flag2 = !num && tile != null;
		foreach (TileZone item in pList.LoopRandom())
		{
			WorldTile worldTile = null;
			if (pForceCenterZone)
			{
				if (isGoodTileForBuilding(item.centerTile, pBuildingAsset, pCity))
				{
					worldTile = item.centerTile;
				}
			}
			else if (pBuildingAsset.docks)
			{
				worldTile = tryToBuildInZoneRandomly(item, pBuildingAsset, pCity);
			}
			else
			{
				if (flag && !pCity.planAllowsToPlaceBuildingInZone(item, pCenterZone))
				{
					continue;
				}
				worldTile = getTileBasedOnLayout(tilePlacementFromZone, item, pBuildingAsset, pCity);
			}
			if (worldTile != null)
			{
				if (!flag2)
				{
					result = worldTile;
					break;
				}
				int num3 = Toolbox.SquaredDistTile(tile, worldTile);
				if (num3 < num2)
				{
					result = worldTile;
					num2 = num3;
				}
			}
		}
		return result;
	}

	private static WorldTile getTileBasedOnLayout(CityLayoutTilePlacement pCityLayoutTilePlacement, TileZone pTileZone, BuildingAsset pBuildingAsset, City pCity)
	{
		switch (pCityLayoutTilePlacement)
		{
		case CityLayoutTilePlacement.Random:
		{
			WorldTile worldTile = tryToBuildInZoneRandomly(pTileZone, pBuildingAsset, pCity);
			if (worldTile != null)
			{
				return worldTile;
			}
			break;
		}
		case CityLayoutTilePlacement.CenterTile:
		{
			WorldTile worldTile = tryToBuildInZoneCenter(pTileZone, pBuildingAsset, pCity);
			if (worldTile != null)
			{
				return worldTile;
			}
			break;
		}
		case CityLayoutTilePlacement.Moonsteps:
		{
			WorldTile worldTile = tryToBuildInZoneMoonsteps(pTileZone, pBuildingAsset, pCity);
			if (worldTile != null)
			{
				return worldTile;
			}
			break;
		}
		case CityLayoutTilePlacement.CenterTileDrunk:
		{
			WorldTile worldTile = tryToBuildInZoneDrunk(pTileZone, pBuildingAsset, pCity);
			if (worldTile != null)
			{
				return worldTile;
			}
			break;
		}
		}
		return null;
	}

	private static WorldTile tryToBuildInZoneMoonsteps(TileZone pZone, BuildingAsset pBuildingAsset, City pCity)
	{
		WorldTile worldTile = (((pZone.x + pZone.y) % 2 == 0) ? pZone.centerTile.tile_up.tile_up.tile_up : pZone.centerTile.tile_down.tile_down.tile_down);
		if (isGoodTileForBuilding(worldTile, pBuildingAsset, pCity))
		{
			return worldTile;
		}
		return null;
	}

	private static WorldTile tryToBuildInZoneDrunk(TileZone pZone, BuildingAsset pBuildingAsset, City pCity)
	{
		WorldTile worldTile = tryToBuildInZoneCenter(pZone, pBuildingAsset, pCity);
		if (Randy.randomChance(0.6f) && worldTile != null)
		{
			WorldTile random = worldTile.neighboursAll.GetRandom();
			if (isGoodTileForBuilding(random, pBuildingAsset, pCity))
			{
				worldTile = random;
			}
		}
		return worldTile;
	}

	private static WorldTile tryToBuildInZoneRandomly(TileZone pZone, BuildingAsset pBuildingAsset, City pCity)
	{
		foreach (WorldTile item in pZone.tiles.LoopRandom())
		{
			if (isGoodTileForBuilding(item, pBuildingAsset, pCity))
			{
				return item;
			}
		}
		return null;
	}

	private static WorldTile tryToBuildInZoneCenter(TileZone pZone, BuildingAsset pBuildingAsset, City pCity)
	{
		if (!isGoodTileForBuilding(pZone.centerTile, pBuildingAsset, pCity))
		{
			return null;
		}
		return pZone.centerTile;
	}

	internal static bool isZoneNearbyBorder(TileZone pParentZone, BuildingAsset pAsset, City pCity)
	{
		TileZone[] neighbours_all = pParentZone.neighbours_all;
		for (int i = 0; i < neighbours_all.Length; i++)
		{
			if (neighbours_all[i].city != pCity)
			{
				return true;
			}
		}
		return false;
	}

	internal static bool isNearbySingleBuilding(TileZone pParentZone, BuildingAsset pAsset, City pCity)
	{
		if (checkZoneNearbySignleBuilding(pParentZone, pAsset, pCity))
		{
			return true;
		}
		TileZone[] neighbours_all = pParentZone.neighbours_all;
		for (int i = 0; i < neighbours_all.Length; i++)
		{
			if (checkZoneNearbySignleBuilding(neighbours_all[i], pAsset, pCity))
			{
				return true;
			}
		}
		return false;
	}

	internal static bool checkZoneNearbySignleBuilding(TileZone pZone, BuildingAsset pAsset, City pCity)
	{
		if (!pZone.hasBuildingOf(pCity))
		{
			return false;
		}
		HashSet<Building> hashset = pZone.getHashset(BuildingList.Civs);
		if (hashset != null)
		{
			foreach (Building item in hashset)
			{
				if (item.asset.build_place_single)
				{
					return true;
				}
			}
		}
		return false;
	}

	internal static bool isZonesNearbyBuilding(TileZone pParentZone, BuildingAsset pAsset, City pCity)
	{
		if (checkZoneNearbyBuilding(pParentZone, pAsset, pCity))
		{
			return true;
		}
		TileZone[] neighbours_all = pParentZone.neighbours_all;
		for (int i = 0; i < neighbours_all.Length; i++)
		{
			if (checkZoneNearbyBuilding(neighbours_all[i], pAsset, pCity))
			{
				return true;
			}
		}
		return false;
	}

	internal static bool checkZoneNearbyBuilding(TileZone pZone, BuildingAsset pAsset, City pCity)
	{
		if (!pZone.isSameCityHere(pCity))
		{
			return false;
		}
		if (!pZone.hasBuildingOf(pCity))
		{
			return false;
		}
		return true;
	}

	internal static bool isZonesClear(TileZone pParentZone, BuildingAsset pAsset, City pCity)
	{
		if (!checkZoneClear(pParentZone, pAsset, pCity))
		{
			return false;
		}
		TileZone[] neighbours_all = pParentZone.neighbours_all;
		for (int i = 0; i < neighbours_all.Length; i++)
		{
			if (!checkZoneClear(neighbours_all[i], pAsset, pCity))
			{
				return false;
			}
		}
		return true;
	}

	internal static bool checkFarmGround(WorldTile pTile, BuildingAsset pAsset, City pCity)
	{
		int num = 0;
		num += countGoodForFarms(pTile.region, pCity);
		for (int i = 0; i < pTile.region.neighbours.Count; i++)
		{
			MapRegion mapRegion = pTile.region.neighbours[i];
			List<TileZone> zones = mapRegion.chunk.zones;
			for (int j = 0; j < zones.Count; j++)
			{
				if (zones[j].city == pCity)
				{
					num += countGoodForFarms(mapRegion, pCity);
				}
			}
		}
		return num > 30;
	}

	internal static int countGoodForFarms(MapRegion pRegion, City pCity)
	{
		int num = 0;
		List<WorldTile> tiles = pRegion.tiles;
		for (int i = 0; i < tiles.Count; i++)
		{
			if (tiles[i].Type.can_be_farm)
			{
				num++;
			}
		}
		return num;
	}

	internal static bool checkZoneClear(TileZone pZone, BuildingAsset pAsset, City pCity)
	{
		if (pZone.hasAnyBuildingsInSet(BuildingList.Civs))
		{
			return false;
		}
		return true;
	}

	public static bool isGoodTileForBuilding(WorldTile pTile, BuildingAsset pAsset, City pCity)
	{
		if (!pTile.canBuildOn(pAsset))
		{
			return false;
		}
		if (BehaviourActionBase<City>.world.buildings.canBuildFrom(pTile, pAsset, pCity))
		{
			return true;
		}
		return false;
	}

	public static void debugRoards(City pCity, Building pBuilding)
	{
		makeRoadsBuildings(pCity, pBuilding);
	}

	public static void makeRoadsBuildings(City pCity, Building pBuilding)
	{
		if (pCity.road_tiles_to_build.Count > 0 || !pBuilding.asset.build_road_to)
		{
			return;
		}
		WorldTile current_tile = pBuilding.current_tile;
		if (current_tile.Type.liquid)
		{
			return;
		}
		using ListPool<WorldTile> listPool = new ListPool<WorldTile>(pCity.buildings.Count);
		foreach (Building building in pCity.buildings)
		{
			if (building != pBuilding && building.asset.build_road_to && !building.current_tile.Type.liquid && building.current_tile.isSameIsland(pBuilding.current_tile))
			{
				listPool.Add(building.current_tile);
			}
		}
		if (listPool.Count != 0)
		{
			bool pForceFinished = false;
			if (DebugConfig.isOn(DebugOption.CityFastConstruction))
			{
				pForceFinished = true;
			}
			WorldTile closestTile = Toolbox.getClosestTile(listPool, current_tile);
			if (closestTile != null)
			{
				listPool.Remove(closestTile);
				MapAction.makeRoadBetween(closestTile, current_tile, pCity, pForceFinished);
			}
			closestTile = Toolbox.getClosestTile(listPool, current_tile);
			if (closestTile != null)
			{
				MapAction.makeRoadBetween(closestTile, current_tile, pCity, pForceFinished);
			}
		}
	}

	public static WorldTile getTileTrainingDummy(BuildingAsset pBuildingAsset, City pCity)
	{
		Building buildingOfType = pCity.getBuildingOfType("type_barracks", pCountOnlyFinished: true, pRandom: true);
		if (buildingOfType == null)
		{
			return null;
		}
		HashSet<WorldTile> hashSet = UnsafeCollectionPool<HashSet<WorldTile>, WorldTile>.Get();
		using ListPool<WorldTile> listPool = new ListPool<WorldTile>();
		using ListPool<WorldTile> listPool2 = new ListPool<WorldTile>();
		foreach (WorldTile tile in buildingOfType.tiles)
		{
			WorldTile[] neighbours = tile.neighbours;
			foreach (WorldTile worldTile in neighbours)
			{
				if (!hashSet.Contains(worldTile))
				{
					hashSet.Add(worldTile);
					if (!worldTile.hasBuilding() && isGoodTileForBuilding(worldTile, pBuildingAsset, pCity))
					{
						listPool.Add(worldTile);
					}
				}
			}
		}
		foreach (ref WorldTile item in listPool)
		{
			WorldTile[] neighbours = item.neighbours;
			foreach (WorldTile worldTile2 in neighbours)
			{
				if (!hashSet.Contains(worldTile2))
				{
					hashSet.Add(worldTile2);
					if (!worldTile2.hasBuilding() && isGoodTileForBuilding(worldTile2, pBuildingAsset, pCity))
					{
						listPool2.Add(worldTile2);
					}
				}
			}
		}
		hashSet.Clear();
		UnsafeCollectionPool<HashSet<WorldTile>, WorldTile>.Release(hashSet);
		if (listPool2.Count == 0)
		{
			return null;
		}
		return listPool2.GetRandom();
	}

	public static WorldTile getDockTile(City pCity)
	{
		BuildingAsset buildingDockAsset = pCity.getActorAsset().getBuildingDockAsset();
		if (buildingDockAsset == null)
		{
			return null;
		}
		OceanHelper.clearOceanPools();
		OceanHelper.saveOceanPoolsWithDocks(pCity);
		if (pCity.getTile() == null)
		{
			return null;
		}
		foreach (TileZone item in pCity.zones.LoopRandom())
		{
			if (item.tiles_with_liquid == 0)
			{
				continue;
			}
			MapChunk chunk = item.chunk;
			if (chunk.regions.Count <= 1)
			{
				continue;
			}
			bool flag = false;
			for (int i = 0; i < chunk.regions.Count; i++)
			{
				MapRegion mapRegion = chunk.regions[i];
				if (mapRegion.type == TileLayerType.Ocean && OceanHelper.goodForNewDock(mapRegion.tiles[0]))
				{
					flag = true;
				}
			}
			if (!flag)
			{
				continue;
			}
			foreach (WorldTile item2 in item.tiles.LoopRandom())
			{
				if (item2.Type.ocean && BehaviourActionBase<City>.world.buildings.canBuildFrom(item2, buildingDockAsset, pCity))
				{
					return item2;
				}
			}
		}
		return null;
	}
}
