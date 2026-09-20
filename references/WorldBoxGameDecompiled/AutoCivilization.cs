using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ai.behaviours;

public class AutoCivilization
{
	private const int KIDS_AGE = 5;

	private const float KIDS_PERCENT = 0.5f;

	private const int EGGS_AGE = 0;

	private const float EGGS_PERCENT_OF_KIDS = 0.5f;

	private const float WARRIORS_PERCENT_OF_ADULTS = 0.1f;

	private const float ITEMS_HOLDER_PERCENT = 0.6f;

	private const int TICKS_WITHOUT_BUILDING_TO_STOP = 100;

	private const int MAXIMUM_TICKS = 5000;

	private const int UNITS_AMOUNT = 100;

	private const float ELAPSED_PER_TICK = 1.5f;

	private const int ITEM_PRODUCTION_PER_UNIT = 5;

	private const int MAXIMUM_TASK_ACTIONS = 500;

	private const int FRAMES_PER_ROUTINE_UPDATE = 4;

	private List<BehaviourTaskActor> _tasks_building = new List<BehaviourTaskActor>();

	private List<BehaviourTaskActor> _tasks_gathering = new List<BehaviourTaskActor>();

	private List<BehaviourTaskActor> _tasks_farming = new List<BehaviourTaskActor>();

	private BehaviourTaskActor _task_take_items;

	private City _city;

	private Actor _unit;

	private List<Actor> _units_list = new List<Actor>(100);

	private BuildingBiomeFoodProducer _food_producer_bonfire;

	private BuildingBiomeFoodProducer _food_producer_hall;

	private Coroutine _routine;

	private int _action_index;

	public AutoCivilization()
	{
		_tasks_building.Add(AssetManager.tasks_actor.get("try_build_building"));
		_tasks_building.Add(AssetManager.tasks_actor.get("build_building"));
		_tasks_building.Add(AssetManager.tasks_actor.get("build_road"));
		_tasks_building.Add(AssetManager.tasks_actor.get("cleaning"));
		_tasks_gathering.Add(AssetManager.tasks_actor.get("chop_trees"));
		_tasks_gathering.Add(AssetManager.tasks_actor.get("mine_deposit"));
		_tasks_gathering.Add(AssetManager.tasks_actor.get("mine"));
		_tasks_gathering.Add(AssetManager.tasks_actor.get("collect_fruits"));
		_tasks_gathering.Add(AssetManager.tasks_actor.get("collect_herbs"));
		_tasks_gathering.Add(AssetManager.tasks_actor.get("collect_honey"));
		_tasks_gathering.Add(AssetManager.tasks_actor.get("manure_cleaning"));
		_tasks_gathering.Add(AssetManager.tasks_actor.get("store_resources"));
		_tasks_farming.Add(AssetManager.tasks_actor.get("farmer_make_field"));
		_tasks_farming.Add(AssetManager.tasks_actor.get("farmer_plant_crops"));
		_tasks_farming.Add(AssetManager.tasks_actor.get("farmer_harvest"));
		_tasks_farming.Add(AssetManager.tasks_actor.get("farmer_fertilize_crops"));
		_task_take_items = AssetManager.tasks_actor.get("try_to_take_city_item");
	}

	public void makeCivilization(Actor pUnit)
	{
		if (!pUnit.isSapient())
		{
			return;
		}
		_unit = pUnit;
		clear();
		if (_unit.buildCityAndStartCivilization())
		{
			Religion religion = World.world.religions.newReligion(_unit, pAddDefaultTraits: true);
			_unit.setReligion(religion);
			_city = _unit.current_tile.zone.city;
			_unit.setCity(_city);
			_unit.createNewWeapon("flame_sword");
			_unit.kingdom.setCapital(_city);
			_units_list.Add(_unit);
			Actor pActor = newUnit(_unit.culture, _unit.language);
			_city.setLeader(pActor, pNew: true);
			spawnUnits(_unit.culture, _unit.language);
			for (int i = 0; i < 50; i++)
			{
				makeBooks(_unit);
			}
			_routine = ((MonoBehaviour)World.world).StartCoroutine(civilizationMakingRoutine());
		}
	}

	private void makeBooks(Actor pActor)
	{
		if (pActor.hasLanguage() && pActor.hasCulture())
		{
			World.world.books.generateNewBook(pActor);
		}
	}

	private Actor newUnit(Culture pCulture, Language pLanguage)
	{
		Actor actor = World.world.units.spawnNewUnit(_unit.asset.id, _city.zones.GetRandom().centerTile, pSpawnSound: true, pMiracleSpawn: true, 3f);
		actor.setCity(_city);
		actor.setSubspecies(_unit.subspecies);
		actor.setCulture(pCulture);
		actor.joinLanguage(pLanguage);
		_units_list.Add(actor);
		return actor;
	}

	private void spawnUnits(Culture pCulture, Language pLanguage)
	{
		for (int i = 0; i < 99; i++)
		{
			newUnit(pCulture, pLanguage).data.age_overgrowth = Randy.randomInt(0, 100);
		}
	}

	private IEnumerator civilizationMakingRoutine()
	{
		int tNoBuiltTicks = 0;
		for (int i = 0; i < 5000; i++)
		{
			Actor random = _units_list.GetRandom();
			claimZone(random);
			gatherResources(random);
			bool flag = buildBuilding(random);
			doFarming(random);
			makeFood(random);
			refertilizeTiles(random.current_zone);
			if (!flag)
			{
				randomTeleport(random);
				tNoBuiltTicks++;
			}
			else
			{
				tNoBuiltTicks = 0;
			}
			if (tNoBuiltTicks > 100)
			{
				break;
			}
			if (i % 4 == 0)
			{
				yield return (object)new WaitForEndOfFrame();
			}
		}
		foreach (Actor item in _units_list)
		{
			craftAndTakeItems(item);
			item.setTask("citizen");
			item.setStatsDirty();
		}
	}

	private void claimZone(Actor pUnit)
	{
		TileZone zoneToClaim = MapBox.instance.city_zone_helper.city_growth.getZoneToClaim(null, _city);
		if (zoneToClaim != null)
		{
			pUnit.setCurrentTilePosition(zoneToClaim.centerTile);
			BehClaimZoneForCityActorBorder.tryClaimZone(pUnit);
		}
	}

	private void gatherResources(Actor pUnit)
	{
		foreach (BehaviourTaskActor item in _tasks_gathering)
		{
			doTask(item, pUnit);
		}
		_city.addResourcesToRandomStockpile("wood", 8);
		_city.addResourcesToRandomStockpile("stone", 8);
		_city.addResourcesToRandomStockpile("common_metals", 4);
		_city.addResourcesToRandomStockpile("gold", 2);
		_city.addResourcesToRandomStockpile("fertilizer");
	}

	private bool buildBuilding(Actor pActor)
	{
		bool result = CityBehBuild.buildTick(_city);
		foreach (BehaviourTaskActor item in _tasks_building)
		{
			doTask(item, pActor);
		}
		World.world.cities.setDirtyBuildings(_city);
		World.world.cities.beginChecksBuildings();
		BuildingZonesSystem.setDirty();
		BuildingZonesSystem.update();
		return result;
	}

	private void doFarming(Actor pUnit)
	{
		foreach (BehaviourTaskActor item in _tasks_farming)
		{
			doTask(item, pUnit);
		}
		foreach (WorldTile calculated_farm_field in _city.calculated_farm_fields)
		{
			Building building = calculated_farm_field.building;
			if (building != null && building.asset.wheat)
			{
				building.component_wheat.update(1.5f);
			}
		}
	}

	private void makeFood(Actor pUnit)
	{
		if (_food_producer_bonfire == null)
		{
			_food_producer_bonfire = _city.getBuildingOfType("type_bonfire")?.component_food_producer;
		}
		if (_food_producer_hall == null)
		{
			_food_producer_hall = _city.getBuildingOfType("type_hall")?.component_food_producer;
		}
		_food_producer_bonfire?.update(1.5f);
		_food_producer_hall?.update(1.5f);
	}

	private void refertilizeTiles(TileZone pTileZone)
	{
		WorldTile[] tiles = pTileZone.tiles;
		foreach (WorldTile pTile in tiles)
		{
			if (Randy.randomChance(0.05f))
			{
				DropsLibrary.action_fertilizer_trees(pTile);
			}
			if (Randy.randomChance(0.05f))
			{
				DropsLibrary.action_fertilizer_plants(pTile);
			}
		}
	}

	private void randomTeleport(Actor pUnit)
	{
		pUnit.setCurrentTilePosition(_city.zones.GetRandom().centerTile);
	}

	private void craftAndTakeItems(Actor pUnit)
	{
		if (Randy.randomChance(0.6f))
		{
			for (int i = 0; i < 5; i++)
			{
				ItemCrafting.tryToCraftRandomArmor(pUnit, _city);
				ItemCrafting.tryToCraftRandomWeapon(pUnit, _city);
			}
			doTask(_task_take_items, pUnit);
		}
	}

	private void doTask(BehaviourTaskActor pTask, Actor pActor)
	{
		_action_index = 0;
		for (int i = 0; i < 500; i++)
		{
			BehResult pResult = pTask.list[_action_index].startExecute(pActor);
			if (!updateTaskIndex(pResult) || _action_index > pTask.list.Count - 1)
			{
				break;
			}
		}
	}

	private bool updateTaskIndex(BehResult pResult)
	{
		switch (pResult)
		{
		case BehResult.Continue:
			_action_index++;
			break;
		case BehResult.StepBack:
			_action_index--;
			if (_action_index < 0)
			{
				_action_index = 0;
			}
			break;
		case BehResult.Stop:
		case BehResult.Skip:
		case BehResult.RestartTask:
		case BehResult.ActiveTaskReturn:
		case BehResult.ImmediateRun:
			return false;
		}
		return true;
	}

	private void clear()
	{
		if (_routine != null)
		{
			((MonoBehaviour)World.world).StopCoroutine(_routine);
		}
		_action_index = 0;
		_food_producer_bonfire = null;
		_food_producer_hall = null;
		_units_list.Clear();
	}
}
