using System.Collections.Generic;

namespace ai.behaviours;

public class CityBehCheckCitizenTasks : BehaviourActionCity
{
	private int _citizens_left;

	public override BehResult execute(City pCity)
	{
		CityTasksData tasks = pCity.tasks;
		CitizenJobs jobs = pCity.jobs;
		CityStatus status = pCity.status;
		jobs.clearJobs();
		checkOccupied(pCity);
		_citizens_left = pCity.status.population_adults;
		tasks.clear();
		if (!DebugConfig.isOn(DebugOption.SystemCityTasks))
		{
			return BehResult.Continue;
		}
		countFires(pCity, tasks);
		countResources(pCity, tasks);
		countRoads(pCity, tasks);
		int num = 0;
		bool flag = false;
		int pTaskAmountMax = 0;
		bool flag2 = pCity.hasStorageBuilding();
		int totalFood = pCity.getTotalFood();
		if (WorldLawLibrary.world_law_civ_army.isEnabled() && status.population_adults > 15 && pCity.hasEnoughFoodForArmy())
		{
			flag = true;
			pTaskAmountMax = getPossibleWarriors(pCity);
		}
		int num2 = 0;
		num2 = ((totalFood > 100) ? 1 : ((totalFood > 60) ? 4 : ((totalFood > 40) ? 5 : ((totalFood <= 20) ? 10 : 7))));
		int num3 = 0;
		num3 = ((pCity.getResourcesAmount("wood") > 15) ? 1 : 3);
		bool flag3 = pCity.hasBuildingType("type_windmill");
		bool flag4 = pCity.hasBuildingType("type_mine");
		bool flag5 = pCity.hasBuildingToBuild();
		int num4 = _citizens_left * 2;
		while (_citizens_left >= 0)
		{
			if (flag5)
			{
				addToJob(CitizenJobLibrary.builder, jobs, 1, 3);
			}
			if (flag2)
			{
				addToJob(CitizenJobLibrary.gatherer_bushes, jobs, 1, tasks.bushes, num2);
				addToJob(CitizenJobLibrary.gatherer_herbs, jobs, 1, tasks.plants, num2);
				addToJob(CitizenJobLibrary.gatherer_honey, jobs, 1, tasks.hives, num2);
			}
			if (flag3)
			{
				addToJob(CitizenJobLibrary.farmer, jobs, 1, pCity.calculated_place_for_farms.Count);
			}
			if (World.world_era.flag_crops_grow && flag3)
			{
				addToJob(CitizenJobLibrary.farmer, jobs, 1, tasks.farms_total);
			}
			if (flag2)
			{
				addToJob(CitizenJobLibrary.miner_deposit, jobs, 1, tasks.minerals);
				addToJob(CitizenJobLibrary.woodcutter, jobs, 1, tasks.trees, num3);
			}
			if (flag4)
			{
				addToJob(CitizenJobLibrary.miner, jobs, 1, 5);
			}
			if (flag)
			{
				addToJob(CitizenJobLibrary.attacker, jobs, 2, pTaskAmountMax);
			}
			addToJob(CitizenJobLibrary.road_builder, jobs, 1, tasks.roads, 1);
			addToJob(CitizenJobLibrary.cleaner, jobs, 1, tasks.ruins, 1);
			if (flag2)
			{
				addToJob(CitizenJobLibrary.hunter, jobs, 1, 1);
				addToJob(CitizenJobLibrary.manure_cleaner, jobs, 1, tasks.poops, 3);
			}
			num++;
			if (num > num4)
			{
				break;
			}
		}
		return BehResult.Continue;
	}

	private void addToJob(CitizenJobAsset pJobAsset, CitizenJobs pJobsContainer, int pAdd, int pTaskAmountMax, int pJobMax = 0)
	{
		if (pAdd == 0 || pTaskAmountMax == 0)
		{
			return;
		}
		int num = pJobsContainer.countCurrentJobs(pJobAsset);
		if (num < pTaskAmountMax && (pJobMax == 0 || num < pJobMax))
		{
			int num2 = pAdd;
			if (_citizens_left <= num2)
			{
				num2 = _citizens_left;
			}
			_citizens_left -= num2;
			pJobsContainer.addToJob(pJobAsset, num2);
		}
	}

	private int getPossibleWarriors(City pCity)
	{
		float armyMaxMultiplier = pCity.getArmyMaxMultiplier();
		return (int)((float)pCity.status.population_adults * armyMaxMultiplier);
	}

	private void countRoads(City pCity, CityTasksData pTasks)
	{
		if (pCity.road_tiles_to_build.Count > 0)
		{
			pTasks.roads = 1;
		}
	}

	private void countResources(City pCity, CityTasksData pTasks)
	{
		bool flag = false;
		if (pCity.hasSpaceForResourceInStockpile(ResourceLibrary.berries))
		{
			flag = true;
		}
		bool flag2 = false;
		if (pCity.hasSpaceForResourceInStockpile(ResourceLibrary.herbs))
		{
			flag2 = true;
		}
		bool flag3 = false;
		if (pCity.hasSpaceForResourceInStockpile(ResourceLibrary.honey))
		{
			flag3 = true;
		}
		bool flag4 = false;
		if (pCity.hasSpaceForResourceInStockpile(ResourceLibrary.wood))
		{
			flag4 = true;
		}
		bool flag5 = false;
		if (pCity.hasSpaceForResourceInStockpile(ResourceLibrary.fertilizer))
		{
			flag5 = true;
		}
		for (int i = 0; i < pCity.zones.Count; i++)
		{
			TileZone tileZone = pCity.zones[i];
			if (flag4)
			{
				pTasks.trees += tileZone.countBuildingsType(BuildingList.Trees);
			}
			pTasks.minerals += tileZone.countBuildingsType(BuildingList.Minerals);
			pTasks.ruins += tileZone.countBuildingsType(BuildingList.Ruins);
			if (flag)
			{
				HashSet<Building> hashset = tileZone.getHashset(BuildingList.Food);
				if (hashset != null)
				{
					foreach (Building item in hashset)
					{
						if (item.hasResourcesToCollect())
						{
							pTasks.bushes++;
						}
					}
				}
			}
			if (flag2)
			{
				pTasks.plants += tileZone.countBuildingsType(BuildingList.Flora);
			}
			if (flag5)
			{
				pTasks.poops += tileZone.countBuildingsType(BuildingList.Poops);
			}
			if (flag3)
			{
				HashSet<Building> hashset2 = tileZone.getHashset(BuildingList.Food);
				if (hashset2 != null)
				{
					foreach (Building item2 in hashset2)
					{
						if (item2.hasResourcesToCollect())
						{
							pTasks.hives++;
						}
					}
				}
			}
			HashSet<WorldTile> tilesOfType = tileZone.getTilesOfType(TopTileLibrary.field);
			if (tilesOfType == null)
			{
				continue;
			}
			foreach (WorldTile item3 in tilesOfType)
			{
				pTasks.farms_total++;
				if (!item3.hasBuilding())
				{
					pTasks.farm_fields++;
				}
				else if (item3.building.asset.wheat && item3.building.component_wheat.isMaxLevel())
				{
					pTasks.wheats++;
				}
			}
		}
	}

	private void countFires(City pCity, CityTasksData pTasks)
	{
		foreach (TileZone neighbour_zone in pCity.neighbour_zones)
		{
			if (neighbour_zone.city == null)
			{
				pTasks.fire += WorldBehaviourActionFire.countFires(neighbour_zone);
			}
		}
		for (int i = 0; i < pCity.zones.Count; i++)
		{
			TileZone pZone = pCity.zones[i];
			pTasks.fire += WorldBehaviourActionFire.countFires(pZone);
		}
	}

	private void checkOccupied(City pCity)
	{
		Dictionary<CitizenJobAsset, int> occupied = pCity.jobs.occupied;
		occupied.Clear();
		for (int i = 0; i < pCity.units.Count; i++)
		{
			Actor actor = pCity.units[i];
			if (actor.citizen_job != null)
			{
				occupied.TryGetValue(actor.citizen_job, out var value);
				occupied[actor.citizen_job] = value + 1;
			}
		}
	}
}
