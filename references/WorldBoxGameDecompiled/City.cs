using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using UnityEngine;
using db;

public class City : MetaObject<CityData>
{
	private static readonly HashSet<City> _connected_checked = new HashSet<City>();

	private static readonly HashSet<City> _connected_next_wave = new HashSet<City>();

	private static readonly HashSet<City> _connected_current_wave = new HashSet<City>();

	private readonly Dictionary<string, CityStorageSlot> _total_resource_slots = new Dictionary<string, CityStorageSlot>();

	private readonly Dictionary<UnitProfession, List<Actor>> _professions_dict = new Dictionary<UnitProfession, List<Actor>>();

	private readonly List<Actor> _boats = new List<Actor>();

	private readonly Dictionary<string, long> _species = new Dictionary<string, long>();

	public readonly List<Building> buildings = new List<Building>();

	public readonly Dictionary<string, List<Building>> buildings_dict_type = new Dictionary<string, List<Building>>();

	public readonly Dictionary<string, List<Building>> buildings_dict_id = new Dictionary<string, List<Building>>();

	public readonly CityTasksData tasks = new CityTasksData();

	public readonly CitizenJobs jobs = new CitizenJobs();

	public readonly CityStatus status = new CityStatus();

	public float mark_scale_effect;

	[NonSerialized]
	internal Kingdom kingdom;

	public Culture culture;

	public Language language;

	public Religion religion;

	public Actor leader;

	public Army army;

	internal readonly List<TileZone> zones = new List<TileZone>();

	internal readonly HashSet<TileZone> neighbour_zones = new HashSet<TileZone>();

	internal readonly HashSet<TileZone> border_zones = new HashSet<TileZone>();

	internal readonly HashSet<City> neighbours_cities = new HashSet<City>();

	internal readonly HashSet<City> neighbours_cities_kingdom = new HashSet<City>();

	internal readonly HashSet<Kingdom> neighbours_kingdoms = new HashSet<Kingdom>();

	internal Building under_construction_building;

	internal readonly List<Building> stockpiles = new List<Building>();

	internal readonly List<Building> storages = new List<Building>();

	internal float timer_build_boat;

	internal float timer_build;

	public float timer_action;

	private float _timer_capture;

	private float _timer_warrior;

	internal readonly List<WorldTile> road_tiles_to_build = new List<WorldTile>();

	private readonly List<WorldTile> tiles_to_remove = new List<WorldTile>();

	internal TileZone target_attack_zone;

	internal City target_attack_city;

	internal WorldTile _city_tile;

	internal string _debug_last_possible_build_orders;

	internal string _debug_last_possible_build_orders_no_resources;

	internal string _debug_last_build_order_try;

	internal Kingdom being_captured_by;

	private float _capture_ticks;

	public int last_visual_capture_ticks;

	private bool _dirty_citizens;

	private bool _dirty_city_status;

	private bool _dirty_abandoned_zones;

	internal Vector2 city_center;

	internal Vector2 last_city_center;

	public readonly WorldTileContainer calculated_place_for_farms = new WorldTileContainer();

	public readonly WorldTileContainer calculated_farm_fields = new WorldTileContainer();

	public readonly WorldTileContainer calculated_crops = new WorldTileContainer();

	public readonly WorldTileContainer calculated_grown_wheat = new WorldTileContainer();

	private readonly Dictionary<Kingdom, int> _capturing_units = new Dictionary<Kingdom, int>();

	internal readonly HashSet<TileZone> danger_zones = new HashSet<TileZone>();

	public AiSystemCity ai;

	private int _current_total_food;

	private int _last_checked_job_id;

	private double _loyalty_last_time;

	private int _loyalty_cached;

	private readonly List<long> _cached_book_ids = new List<long>();

	private readonly List<Building> _cached_buildings_with_book_slots = new List<Building>();

	public double timestamp_shrink;

	private int _storage_version;

	protected override MetaType meta_type => MetaType.City;

	public override BaseSystemManager manager => World.world.cities;

	protected override bool track_death_types => true;

	public int amount_wood => getResourcesAmount("wood");

	public int amount_gold => getResourcesAmount("gold");

	public int amount_stone => getResourcesAmount("stone");

	public int amount_common_metals => getResourcesAmount("common_metals");

	public int getStorageVersion()
	{
		return _storage_version;
	}

	public override void increaseBirths()
	{
		base.increaseBirths();
		addRenown(1);
	}

	public void increaseLeft()
	{
		if (isAlive())
		{
			data.left++;
		}
	}

	public void increaseJoined()
	{
		if (isAlive())
		{
			data.joined++;
			addRenown(1);
		}
	}

	public void increaseMoved()
	{
		if (isAlive())
		{
			data.moved++;
			addRenown(2);
		}
	}

	public void increaseMigrants()
	{
		if (isAlive())
		{
			data.migrated++;
		}
	}

	public long getTotalLeft()
	{
		return data.left;
	}

	public long getTotalJoined()
	{
		return data.joined;
	}

	public long getTotalMoved()
	{
		return data.moved;
	}

	public long getTotalMigrated()
	{
		return data.migrated;
	}

	public bool isZoneToClaimStillGood(Actor pActor, TileZone pZone, WorldTile pCityTile)
	{
		if (!pZone.canBeClaimedByCity(this))
		{
			return false;
		}
		if (!pZone.checkCanSettleInThisBiomes(pActor.subspecies))
		{
			return false;
		}
		TileZone[] neighbours = pZone.neighbours;
		foreach (TileZone tileZone in neighbours)
		{
			if (tileZone.hasCity() && tileZone.city == this)
			{
				return true;
			}
		}
		return false;
	}

	internal override void clearListUnits()
	{
		base.clearListUnits();
		_boats.Clear();
		_species.Clear();
	}

	public override ActorAsset getActorAsset()
	{
		if (hasLeader())
		{
			return leader.getActorAsset();
		}
		return getFounderSpecies();
	}

	public ActorAsset getFounderSpecies()
	{
		return AssetManager.actor_library.get(data.original_actor_asset);
	}

	public CityLayoutTilePlacement getTilePlacementFromZone()
	{
		if (hasCulture())
		{
			if (culture.hasTrait("city_layout_the_grand_arrangement"))
			{
				return CityLayoutTilePlacement.CenterTile;
			}
			if (culture.hasTrait("city_layout_tile_wobbly_pattern"))
			{
				return CityLayoutTilePlacement.CenterTileDrunk;
			}
			if (culture.hasTrait("city_layout_tile_moonsteps"))
			{
				return CityLayoutTilePlacement.Moonsteps;
			}
		}
		return CityLayoutTilePlacement.Random;
	}

	public string getSpecies()
	{
		return getActorAsset().id;
	}

	public override bool isReadyForRemoval()
	{
		if (zones.Count != 0)
		{
			return false;
		}
		return true;
	}

	public void clearBuildingList()
	{
		buildings.Clear();
		foreach (List<Building> value in buildings_dict_type.Values)
		{
			value.Clear();
		}
		foreach (List<Building> value2 in buildings_dict_id.Values)
		{
			value2.Clear();
		}
		stockpiles.Clear();
		storages.Clear();
		_cached_book_ids.Clear();
		_cached_buildings_with_book_slots.Clear();
	}

	public override void listUnit(Actor pActor)
	{
		if (pActor.asset.is_boat)
		{
			_boats.Add(pActor);
			return;
		}
		base.units.Add(pActor);
		if (pActor.hasSubspecies())
		{
			_species[pActor.asset.id] = pActor.subspecies.id;
		}
	}

	public Subspecies getSubspecies(string pSpeciesId)
	{
		long subspeciesId = getSubspeciesId(pSpeciesId);
		return World.world.subspecies.get(subspeciesId);
	}

	public long getSubspeciesId(string pSpeciesId)
	{
		if (_species.TryGetValue(pSpeciesId, out var value))
		{
			return value;
		}
		return -1L;
	}

	public bool hasFreeHouseSlots()
	{
		if (status.housing_free == 0)
		{
			return false;
		}
		return true;
	}

	public bool hasReachedWorldLawLimit()
	{
		if (WorldLawLibrary.world_law_civ_limit_population_100.isEnabled() && getPopulationPeople() >= 100)
		{
			return true;
		}
		return false;
	}

	public void listBuilding(Building pBuilding)
	{
		buildings.Add(pBuilding);
		BuildingAsset asset = pBuilding.asset;
		if (asset.type == "type_stockpile")
		{
			stockpiles.Add(pBuilding);
		}
		if (asset.storage)
		{
			storages.Add(pBuilding);
		}
		if (asset.book_slots > 0)
		{
			_cached_buildings_with_book_slots.Add(pBuilding);
			if (pBuilding.data.books != null)
			{
				_cached_book_ids.AddRange(pBuilding.data.books.list_books);
			}
		}
		setBuildingDictType(pBuilding);
		setBuildingDictID(pBuilding);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[CanBeNull]
	public WorldTile getTile(bool pForceRecalc = false)
	{
		if ((_city_tile == null) | pForceRecalc)
		{
			recalculateCityTile();
		}
		return _city_tile;
	}

	internal void recalculateCityTile()
	{
		_city_tile = null;
		Building building = getBuildingOfType("type_bonfire");
		if (building != null)
		{
			_city_tile = building.current_tile;
			return;
		}
		foreach (Building item in buildings.LoopRandom())
		{
			if (!item.asset.docks && !item.current_tile.Type.ocean)
			{
				if (building == null)
				{
					building = item;
				}
				else if (item.asset.priority > building.asset.priority)
				{
					building = item;
				}
			}
		}
		if (building != null)
		{
			_city_tile = building.current_tile;
			return;
		}
		List<TileZone> list = zones;
		if (list.Count == 0)
		{
			return;
		}
		for (int i = 0; i < list.Count; i++)
		{
			TileZone tileZone = list[i];
			if (!tileZone.centerTile.Type.ocean)
			{
				_city_tile = tileZone.centerTile;
				break;
			}
		}
	}

	internal int countInHouses()
	{
		int num = 0;
		List<Actor> list = base.units;
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i].is_inside_building)
			{
				num++;
			}
		}
		return num;
	}

	public int countBookSlots()
	{
		int num = 0;
		for (int i = 0; i < _cached_buildings_with_book_slots.Count; i++)
		{
			Building building = _cached_buildings_with_book_slots[i];
			num += building.asset.book_slots;
		}
		return num;
	}

	public bool hasBookSlots()
	{
		int num = countBookSlots();
		if (countBooks() >= num)
		{
			return false;
		}
		return true;
	}

	public Building getBuildingWithBookSlot()
	{
		foreach (Building cached_buildings_with_book_slot in _cached_buildings_with_book_slots)
		{
			if (cached_buildings_with_book_slot.hasFreeBookSlot())
			{
				return cached_buildings_with_book_slot;
			}
		}
		return null;
	}

	public int countBooks()
	{
		return _cached_book_ids.Count;
	}

	private void setKingdomTimestamp()
	{
		data.timestamp_kingdom = World.world.getCurWorldTime();
	}

	public override ColorAsset getColor()
	{
		return kingdom.getColor();
	}

	internal void setKingdom(Kingdom pKingdom, bool pFromLoad = false)
	{
		World.world.kingdoms.setDirtyCities();
		if (isCapitalCity())
		{
			kingdom.clearCapital();
		}
		kingdom = pKingdom;
		if (kingdom != null && kingdom != WildKingdomsManager.neutral)
		{
			data.last_kingdom_id = kingdom.id;
		}
		if (!pFromLoad)
		{
			checkArmyExistence();
			if (hasArmy())
			{
				army.checkCity();
			}
		}
	}

	internal void newForceKingdomEvent(List<Actor> pUnits, List<Actor> pBoats, Kingdom pKingdom, string pHappinessEvent)
	{
		setKingdomTimestamp();
		forceUnitsIntoThisKingdom(pUnits, pKingdom, pBoats: false, pHappinessEvent);
		forceUnitsIntoThisKingdom(pBoats, pKingdom, pBoats: true);
	}

	internal void forceBuildingsToKingdom(List<Building> pBuildings, Kingdom pKingdom)
	{
		for (int i = 0; i < pBuildings.Count; i++)
		{
			pBuildings[i].setKingdom(pKingdom);
		}
	}

	internal void forceUnitsIntoThisKingdom(List<Actor> pActors, Kingdom pKingdom, bool pBoats, string pHappinessEvent = null)
	{
		if (pBoats)
		{
			for (int i = 0; i < pActors.Count; i++)
			{
				Actor actor = pActors[i];
				if (!actor.isRekt())
				{
					actor.joinKingdom(pKingdom);
				}
			}
			return;
		}
		for (int j = 0; j < pActors.Count; j++)
		{
			Actor actor2 = pActors[j];
			if (actor2.isRekt())
			{
				continue;
			}
			if (actor2.isKing())
			{
				if (actor2.city != this || actor2.kingdom == pKingdom)
				{
					continue;
				}
				actor2.kingdom.kingLeftEvent();
			}
			actor2.joinKingdom(pKingdom);
			if (pHappinessEvent != null)
			{
				actor2.changeHappiness(pHappinessEvent);
			}
		}
	}

	internal Building getStorageNear(WorldTile pTile, bool pOnlyFood = false)
	{
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		Building result = null;
		int num = int.MaxValue;
		List<Building> list = storages;
		for (int i = 0; i < list.Count; i++)
		{
			Building building = list[i];
			if (!building.isUsable() || !building.current_tile.isSameIsland(pTile))
			{
				continue;
			}
			if (pOnlyFood && building.asset.storage_only_food)
			{
				result = building;
				continue;
			}
			int num2 = Toolbox.SquaredDistVec2(building.current_tile.pos, pTile.pos);
			if (num2 < num)
			{
				num = num2;
				result = building;
			}
		}
		return result;
	}

	internal Building getStorageWithFoodNear(WorldTile pTile)
	{
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		Building result = null;
		int num = int.MaxValue;
		List<Building> list = storages;
		for (int i = 0; i < list.Count; i++)
		{
			Building building = list[i];
			if (building.isUsable() && building.current_tile.isSameIsland(pTile) && building.countFood() != 0)
			{
				int num2 = Toolbox.SquaredDistVec2(building.current_tile.pos, pTile.pos);
				if (num2 < num)
				{
					num = num2;
					result = building;
				}
			}
		}
		return result;
	}

	internal bool hasStorageBuilding()
	{
		List<Building> list = storages;
		for (int i = 0; i < list.Count; i++)
		{
			if (!list[i].isUnderConstruction())
			{
				return true;
			}
		}
		return false;
	}

	public WorldTile getRoadTileToBuild(Actor pBuilder)
	{
		tiles_to_remove.Clear();
		for (int i = 0; i < road_tiles_to_build.Count; i++)
		{
			WorldTile worldTile = road_tiles_to_build[i];
			if (worldTile.Type.road)
			{
				tiles_to_remove.Add(worldTile);
			}
		}
		for (int j = 0; j < tiles_to_remove.Count; j++)
		{
			WorldTile item = tiles_to_remove[j];
			road_tiles_to_build.Remove(item);
		}
		tiles_to_remove.Clear();
		if (road_tiles_to_build.Count > 0)
		{
			return road_tiles_to_build[0];
		}
		return null;
	}

	internal void init()
	{
		createAI();
		setStatusDirty();
	}

	private void createAI()
	{
		if (Globals.AI_TEST_ACTIVE)
		{
			if (ai == null)
			{
				ai = new AiSystemCity(this);
			}
			ai.next_job_delegate = getNextJob;
			ai.jobs_library = AssetManager.job_city;
			ai.task_library = AssetManager.tasks_city;
			ai.addSingleTask("build");
			ai.addSingleTask("check_loyalty");
			ai.addSingleTask("check_destruction");
		}
	}

	protected sealed override void setDefaultValues()
	{
		base.setDefaultValues();
		mark_scale_effect = 1f;
		timer_build_boat = 10f;
		timer_build = 0f;
		timer_action = 0f;
		_timer_capture = 0f;
		_timer_warrior = 0f;
		_capture_ticks = 0f;
		last_visual_capture_ticks = 0;
		_dirty_citizens = true;
		_dirty_city_status = false;
		_dirty_abandoned_zones = false;
		_current_total_food = 0;
		_last_checked_job_id = 0;
		_loyalty_last_time = -1.0;
		_loyalty_cached = -1;
	}

	private string getNextJob()
	{
		return "city";
	}

	public bool isValidTargetForWar()
	{
		if (!hasZones())
		{
			return false;
		}
		return true;
	}

	public bool hasZones()
	{
		return zones.Count > 0;
	}

	public bool needSettlers()
	{
		int populationPeople = getPopulationPeople();
		if (getAge() < 5)
		{
			return true;
		}
		if (populationPeople >= 22)
		{
			return false;
		}
		if (populationPeople < 22 && status.housing_free == 0 && getAge() > 10 && getHouseCurrent() > 2)
		{
			return false;
		}
		return true;
	}

	internal void generateName(Actor pActor)
	{
		string pName = pActor.generateName(MetaType.City, getID());
		setName(pName);
		data.name_culture_id = culture?.id ?? (-1);
	}

	public void loadLeader()
	{
		if (data.leaderID.hasValue())
		{
			Actor pActor = World.world.units.get(data.leaderID);
			setLeader(pActor, pNew: false);
		}
	}

	public void newCityEvent(Actor pActor)
	{
		recalculateCityTile();
		generateName(pActor);
	}

	private void loadCityZones(List<ZoneData> pZoneData)
	{
		if (pZoneData == null)
		{
			return;
		}
		for (int i = 0; i < pZoneData.Count; i++)
		{
			ZoneData zoneData = pZoneData[i];
			TileZone zone = World.world.zone_calculator.getZone(zoneData.x, zoneData.y);
			if (zone != null)
			{
				addZone(zone);
			}
		}
	}

	public void loadCity(CityData pData)
	{
		loadCityZones(pData.zones);
		setData(pData);
		if (data.id_culture.hasValue())
		{
			setCulture(World.world.cultures.get(data.id_culture));
		}
		if (data.id_language.hasValue())
		{
			setLanguage(World.world.languages.get(data.id_language));
		}
		if (data.id_religion.hasValue())
		{
			setReligion(World.world.religions.get(data.id_religion));
		}
		if (data.equipment == null)
		{
			data.equipment = new CityEquipment();
		}
		else
		{
			data.equipment.loadFromSave(this);
		}
		Kingdom pKingdom = ((!pData.kingdomID.hasValue() || pData.kingdomID == 0L) ? WildKingdomsManager.neutral : World.world.kingdoms.get(pData.kingdomID));
		setKingdom(pKingdom, pFromLoad: true);
	}

	public void forceDoChecks()
	{
		updateTotalFood();
		updateCitizens();
		updateCityStatus();
	}

	public void executeAllActionsForCity()
	{
		AssetManager.tasks_city.get("do_initial_load_check").executeAllActionsForCity(this);
	}

	public void eventUnitAdded(Actor pActor)
	{
		if (!pActor.asset.is_boat)
		{
			setCitizensDirty();
		}
		setStatusDirty();
	}

	public void eventUnitRemoved(Actor pActor)
	{
		setStatusDirty();
		setCitizensDirty();
		if (pActor.isCityLeader())
		{
			removeLeader();
		}
	}

	public void setAbandonedZonesDirty()
	{
		_dirty_abandoned_zones = true;
	}

	public void setCitizensDirty()
	{
		_dirty_citizens = true;
	}

	public void setStatusDirty()
	{
		_dirty_city_status = true;
	}

	private void sortZonesByDistanceToCenter()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		WorldTile tile = getTile();
		if (tile != null)
		{
			Vector2Int tCenterPos = tile.pos;
			zones.Sort(delegate(TileZone a, TileZone b)
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_000c: Unknown result type (might be due to invalid IL or missing references)
				//IL_001d: Unknown result type (might be due to invalid IL or missing references)
				//IL_0023: Unknown result type (might be due to invalid IL or missing references)
				int num = Toolbox.SquaredDistVec2(a.centerTile.pos, tCenterPos);
				int value = Toolbox.SquaredDistVec2(b.centerTile.pos, tCenterPos);
				return num.CompareTo(value);
			});
		}
	}

	private void updateCityStatus()
	{
		_dirty_city_status = false;
		status.clear();
		recalculateCityTile();
		sortZonesByDistanceToCenter();
		recalculateNeighbourZones();
		recalculateNeighbourCities();
		List<Building> list = buildings;
		int num = countPopulationChildren();
		status.population = getPopulationPeople();
		status.population_adults = status.population - num;
		status.population_children = num;
		MetaObject<CityData>._family_counter.Clear();
		List<Actor> list2 = base.units;
		for (int i = 0; i < list2.Count; i++)
		{
			Actor actor = list2[i];
			if (actor.isHungry())
			{
				status.hungry++;
			}
			if (actor.isSexMale())
			{
				status.males++;
			}
			else
			{
				status.females++;
			}
			if (actor.hasFamily())
			{
				MetaObject<CityData>._family_counter.Add(actor.family);
			}
			if (actor.isSick())
			{
				status.sick++;
			}
			if (actor.hasHouse())
			{
				status.housed++;
			}
			else
			{
				status.homeless++;
			}
		}
		status.families = MetaObject<CityData>._family_counter.Count;
		MetaObject<CityData>._family_counter.Clear();
		for (int j = 0; j < list.Count; j++)
		{
			Building building = list[j];
			if (!building.isUnderConstruction() && building.asset.hasHousingSlots())
			{
				status.housing_total += building.asset.housing_slots;
			}
		}
		if (status.population > status.housing_total)
		{
			status.housing_occupied = status.housing_total;
		}
		else
		{
			status.housing_occupied = status.population;
		}
		status.housing_free = status.housing_total - status.housing_occupied;
		status.maximum_items = 15;
		recalculateMaxHouses();
		status.warrior_slots = jobs.countCurrentJobs(CitizenJobLibrary.attacker);
		status.warriors_current = countProfession(UnitProfession.Warrior);
		CityBehCheckFarms.check(this);
	}

	private void recalculateMaxHouses()
	{
		if (DebugConfig.isOn(DebugOption.CityUnlimitedHouses))
		{
			status.houses_max = 9999;
			return;
		}
		float num = zones.Count;
		if (hasCulture())
		{
			if (culture.hasTrait("dense_dwellings"))
			{
				num = zones.Count * 2;
			}
			if (culture.hasTrait("solitude_seekers"))
			{
				num = (float)zones.Count / 3f;
			}
			if (culture.hasTrait("hive_society"))
			{
				num = (float)zones.Count * 3f;
			}
		}
		foreach (Building building in buildings)
		{
			num += (float)building.asset.max_houses;
		}
		status.houses_max = (int)num;
	}

	public bool hasBooksToRead(Actor pActor)
	{
		if (pActor.hasTag("can_read_any_book"))
		{
			return countBooks() > 0;
		}
		if (!pActor.hasLanguage())
		{
			return false;
		}
		if (!hasBooksOfLanguage(pActor.language))
		{
			return false;
		}
		return true;
	}

	public bool hasBooksOfLanguage(Language pLanguage)
	{
		int i = 0;
		for (int num = countBooks(); i < num; i++)
		{
			long pID = _cached_book_ids[i];
			Book book = World.world.books.get(pID);
			if (!book.isRekt() && book.isReadyToBeRead())
			{
				Language language = book.getLanguage();
				if (language.id == pLanguage.id || language.hasTrait("magic_words"))
				{
					return true;
				}
			}
		}
		return false;
	}

	public Book getRandomBookOfLanguage(Language pLanguage)
	{
		using ListPool<Book> listPool = new ListPool<Book>();
		int i = 0;
		for (int num = countBooks(); i < num; i++)
		{
			long pID = _cached_book_ids[i];
			Book book = World.world.books.get(pID);
			if (!book.isRekt() && book.isReadyToBeRead())
			{
				Language language = book.getLanguage();
				if (language.id == pLanguage.id || language.hasTrait("magic_words"))
				{
					listPool.Add(book);
				}
			}
		}
		if (listPool.Count == 0)
		{
			return null;
		}
		return listPool.GetRandom();
	}

	public Book getRandomBook()
	{
		using ListPool<Book> listPool = new ListPool<Book>();
		int i = 0;
		for (int num = countBooks(); i < num; i++)
		{
			long pID = _cached_book_ids[i];
			Book book = World.world.books.get(pID);
			if (!book.isRekt() && book.isReadyToBeRead())
			{
				listPool.Add(book);
			}
		}
		if (listPool.Count == 0)
		{
			return null;
		}
		return listPool.GetRandom();
	}

	public List<long> getBooks()
	{
		return _cached_book_ids;
	}

	public int getHouseCurrent()
	{
		return countBuildingsType("type_house", pCountOnlyFinished: false);
	}

	public int getHouseLimit()
	{
		return status.houses_max;
	}

	public bool isConnectedToCapital()
	{
		if (!kingdom.hasCapital())
		{
			return false;
		}
		recalculateNeighbourCities();
		if (neighbours_cities_kingdom.Contains(this))
		{
			return true;
		}
		kingdom.calculateNeighbourCities();
		_connected_checked.Clear();
		_connected_next_wave.Clear();
		_connected_current_wave.Clear();
		_connected_next_wave.UnionWith(kingdom.capital.neighbours_cities_kingdom);
		int num = 0;
		while (_connected_next_wave.Count > 0)
		{
			_connected_current_wave.UnionWith(_connected_next_wave);
			_connected_next_wave.Clear();
			num++;
			foreach (City item in _connected_current_wave)
			{
				if (item == this)
				{
					return true;
				}
				_connected_checked.Add(item);
				foreach (City item2 in item.neighbours_cities_kingdom)
				{
					if (!_connected_checked.Contains(item2))
					{
						_connected_next_wave.Add(item2);
					}
				}
			}
			if (num > 30)
			{
				break;
			}
		}
		return false;
	}

	public void recalculateNeighbourCities()
	{
		neighbours_cities.Clear();
		neighbours_cities_kingdom.Clear();
		neighbours_kingdoms.Clear();
		foreach (TileZone neighbour_zone in neighbour_zones)
		{
			City city = neighbour_zone.city;
			if (city != this && city != null)
			{
				neighbours_cities.Add(city);
				if (city.kingdom == kingdom)
				{
					neighbours_cities_kingdom.Add(city);
				}
				else
				{
					neighbours_kingdoms.Add(city.kingdom);
				}
			}
		}
	}

	public void recalculateNeighbourZones()
	{
		border_zones.Clear();
		neighbour_zones.Clear();
		List<TileZone> list = zones;
		for (int i = 0; i < list.Count; i++)
		{
			TileZone tileZone = list[i];
			TileZone[] neighbours_all = tileZone.neighbours_all;
			foreach (TileZone tileZone2 in neighbours_all)
			{
				if (tileZone2.city != this)
				{
					border_zones.Add(tileZone);
					neighbour_zones.Add(tileZone2);
				}
			}
		}
	}

	internal void setCulture(Culture pCulture)
	{
		if (culture != pCulture)
		{
			culture = pCulture;
			World.world.cultures.setDirtyCities();
		}
	}

	public Culture getCulture()
	{
		return culture;
	}

	public Language getLanguage()
	{
		return language;
	}

	public Religion getReligion()
	{
		return religion;
	}

	public void checkAbandon()
	{
		if (_dirty_abandoned_zones)
		{
			_dirty_abandoned_zones = false;
			World.world.city_zone_helper.city_abandon.check(this);
		}
	}

	public void update(float pElapsed)
	{
		if (timer_build > 0f)
		{
			timer_build -= pElapsed;
		}
		updateTotalFood();
		if (data.timer_supply > 0f)
		{
			data.timer_supply -= pElapsed;
		}
		if (data.timer_trade > 0f)
		{
			data.timer_trade -= pElapsed;
		}
		if (_timer_warrior > 0f)
		{
			_timer_warrior -= pElapsed;
		}
		if (isDirtyUnits())
		{
			return;
		}
		if (!kingdom.wild && !hasUnits())
		{
			turnCityToNeutral();
			return;
		}
		if (_dirty_city_status)
		{
			updateCityStatus();
		}
		if (_dirty_citizens)
		{
			updateCitizens();
		}
		if (World.world.isPaused())
		{
			return;
		}
		if (timer_build_boat > 0f)
		{
			timer_build_boat -= pElapsed;
		}
		if (ai != null)
		{
			if (timer_action > 0f)
			{
				timer_action -= pElapsed;
			}
			else
			{
				ai.update();
			}
			ai.updateSingleTasks(pElapsed);
		}
		updateCapture(pElapsed);
	}

	private void turnCityToNeutral()
	{
		makeBoatsAbandonCity();
		setKingdom(WildKingdomsManager.neutral);
		forceBuildingsToKingdom(buildings, WildKingdomsManager.neutral);
	}

	private void makeBoatsAbandonCity()
	{
		if (countBoats() == 0)
		{
			return;
		}
		foreach (Actor boat in _boats)
		{
			if (!boat.isRekt())
			{
				boat.setCity(null);
			}
		}
	}

	private void updateTotalFood()
	{
		_current_total_food = countFoodTotal();
	}

	private void updateCapture(float pElapsed)
	{
		if (last_visual_capture_ticks == 0 && !isGettingCaptured())
		{
			return;
		}
		if ((int)_capture_ticks != last_visual_capture_ticks)
		{
			if ((int)_capture_ticks > last_visual_capture_ticks)
			{
				last_visual_capture_ticks++;
			}
			else
			{
				last_visual_capture_ticks--;
			}
		}
		last_visual_capture_ticks = Mathf.Clamp(last_visual_capture_ticks, 0, 100);
		if (_timer_capture > 0f)
		{
			_timer_capture -= pElapsed;
			return;
		}
		_timer_capture = 0.1f;
		int num = countBuildingsType("type_watch_tower");
		if (num > 0)
		{
			addCapturePoints(this.kingdom, 10 * num);
		}
		Kingdom kingdom = null;
		foreach (Kingdom key in _capturing_units.Keys)
		{
			if (kingdom == null)
			{
				kingdom = key;
			}
			else if (_capturing_units[key] > _capturing_units[kingdom])
			{
				kingdom = key;
			}
		}
		if (kingdom == null)
		{
			_capture_ticks -= 0.5f;
			if (_capture_ticks <= 0f)
			{
				clearCapture();
			}
			return;
		}
		bool flag = false;
		if (_capturing_units.ContainsKey(this.kingdom) && _capturing_units[this.kingdom] > 0 && countWarriors() > 0)
		{
			flag = true;
		}
		if (being_captured_by != null && !being_captured_by.isAlive())
		{
			being_captured_by = null;
		}
		bool flag2 = false;
		if (this.kingdom == kingdom)
		{
			flag2 = true;
		}
		if (flag && _capturing_units.Count == 1)
		{
			flag2 = true;
		}
		if (flag2)
		{
			_capture_ticks--;
			if (_capture_ticks <= 0f)
			{
				clearCapture();
			}
		}
		else
		{
			if (!kingdom.isEnemy(this.kingdom) || (flag && !(_capture_ticks < 5f)))
			{
				return;
			}
			if (being_captured_by == null || being_captured_by == kingdom)
			{
				_capture_ticks += 1f + 1f * pElapsed;
				being_captured_by = kingdom;
				if (_capture_ticks >= 100f)
				{
					finishCapture(kingdom);
				}
			}
			else if (kingdom.isEnemy(being_captured_by))
			{
				_capture_ticks -= 0.5f;
				if (_capture_ticks <= 0f)
				{
					clearCapture();
				}
			}
			else
			{
				_capture_ticks += 1f + 1f * pElapsed;
				if (_capture_ticks >= 100f)
				{
					finishCapture(being_captured_by);
				}
			}
		}
	}

	public bool isGettingCaptured()
	{
		if (_capturing_units.Count == 0)
		{
			return false;
		}
		if (_capturing_units.Count == 1 && _capturing_units.ContainsKey(kingdom))
		{
			return false;
		}
		return true;
	}

	public bool isGettingCapturedBy(Kingdom pKingdom)
	{
		if (_capturing_units.TryGetValue(pKingdom, out var value) && value > 0)
		{
			return true;
		}
		return false;
	}

	public Kingdom getCapturingKingdom()
	{
		return being_captured_by;
	}

	private void clearCapture()
	{
		_capture_ticks = 0f;
		being_captured_by = null;
	}

	public float getCaptureTicks()
	{
		return _capture_ticks;
	}

	private void prepareProfessionDicts()
	{
		if (_professions_dict.Count == 0)
		{
			for (int i = 0; i < ProfessionLibrary.list_enum_profession_ids.Length; i++)
			{
				UnitProfession key = ProfessionLibrary.list_enum_profession_ids[i];
				_professions_dict.Add(key, new List<Actor>());
			}
		}
	}

	private void updateCitizens()
	{
		_dirty_citizens = false;
		prepareProfessionDicts();
		foreach (List<Actor> value in _professions_dict.Values)
		{
			value.Clear();
		}
		List<Actor> list = base.units;
		for (int i = 0; i < list.Count; i++)
		{
			Actor actor = list[i];
			if (actor != null && actor.isAlive())
			{
				_professions_dict[actor.getProfession()].Add(actor);
			}
		}
	}

	public bool canGrowZones()
	{
		if (!DebugConfig.isOn(DebugOption.SystemZoneGrowth))
		{
			return false;
		}
		if (_dirty_abandoned_zones)
		{
			return false;
		}
		if (getPopulationPeople() == 0)
		{
			return false;
		}
		return true;
	}

	internal int countProfession(UnitProfession pType)
	{
		if (_professions_dict.TryGetValue(pType, out var value))
		{
			return value.Count;
		}
		return 0;
	}

	public void destroyCity()
	{
		removeLeader();
		disbandArmy();
		foreach (TileZone zone in zones)
		{
			zone.setCity(null);
		}
		foreach (Actor unit in World.world.units)
		{
			if (unit.city == this)
			{
				unit.setCity(null);
			}
		}
		data.equipment.clearItems();
		base.units.Clear();
		_boats.Clear();
		zones.Clear();
		if (hasKingdom())
		{
			removeFromCurrentKingdom();
		}
	}

	public override void Dispose()
	{
		DBInserter.deleteData(getID(), "city");
		_connected_checked.Clear();
		_connected_next_wave.Clear();
		_connected_current_wave.Clear();
		stockpiles.Clear();
		storages.Clear();
		_cached_book_ids.Clear();
		_cached_buildings_with_book_slots.Clear();
		base.units.Clear();
		_boats.Clear();
		buildings.Clear();
		buildings_dict_id.Clear();
		buildings_dict_type.Clear();
		zones.Clear();
		road_tiles_to_build.Clear();
		calculated_place_for_farms.Clear();
		calculated_farm_fields.Clear();
		calculated_crops.Clear();
		calculated_grown_wheat.Clear();
		_professions_dict.Clear();
		neighbour_zones.Clear();
		border_zones.Clear();
		neighbours_cities.Clear();
		neighbours_cities_kingdom.Clear();
		neighbours_kingdoms.Clear();
		tiles_to_remove.Clear();
		danger_zones.Clear();
		_capturing_units.Clear();
		_city_tile = null;
		target_attack_zone = null;
		target_attack_city = null;
		army = null;
		tasks.clear();
		jobs.clear();
		status.clear();
		under_construction_building = null;
		culture = null;
		language = null;
		religion = null;
		kingdom = null;
		leader = null;
		being_captured_by = null;
		_debug_last_possible_build_orders = null;
		_debug_last_possible_build_orders_no_resources = null;
		_debug_last_build_order_try = null;
		timestamp_shrink = 0.0;
		ai.reset();
		base.Dispose();
	}

	public bool hasAttackZoneOrder()
	{
		return target_attack_zone != null;
	}

	internal void spendResourcesForBuildingAsset(ConstructionCost pCost)
	{
		takeResource("wood", pCost.wood);
		takeResource("gold", pCost.gold);
		takeResource("stone", pCost.stone);
		takeResource("common_metals", pCost.common_metals);
	}

	internal bool hasEnoughResourcesFor(ConstructionCost pCost)
	{
		if (DebugConfig.isOn(DebugOption.CityInfiniteResources))
		{
			return true;
		}
		if (amount_wood < pCost.wood)
		{
			return false;
		}
		if (amount_common_metals < pCost.common_metals)
		{
			return false;
		}
		if (amount_stone < pCost.stone)
		{
			return false;
		}
		if (amount_gold < pCost.gold)
		{
			return false;
		}
		return true;
	}

	internal Building getBuildingToBuild()
	{
		if (under_construction_building != null && (!under_construction_building.isAlive() || !under_construction_building.isUnderConstruction()))
		{
			under_construction_building = null;
		}
		return under_construction_building;
	}

	internal bool hasBuildingToBuild()
	{
		if (under_construction_building != null)
		{
			if (!under_construction_building.isAlive() || !under_construction_building.isUnderConstruction())
			{
				under_construction_building = null;
				return false;
			}
			return true;
		}
		return false;
	}

	internal void setBuildingDictType(Building pBuilding)
	{
		List<Building> list = getBuildingListOfType(pBuilding.asset.type);
		if (list == null)
		{
			list = new List<Building>();
			buildings_dict_type.Add(pBuilding.asset.type, list);
		}
		list.Add(pBuilding);
	}

	internal List<Building> getBuildingListOfID(string pBuildingID)
	{
		buildings_dict_id.TryGetValue(pBuildingID, out var value);
		return value;
	}

	public int countZones()
	{
		return zones.Count;
	}

	public int countBuildings()
	{
		return buildings.Count;
	}

	public int countBuildingsOfID(string pBuildingID)
	{
		return getBuildingListOfID(pBuildingID)?.Count ?? 0;
	}

	internal void setBuildingDictID(Building pBuilding)
	{
		if (!buildings_dict_id.TryGetValue(pBuilding.asset.id, out var value))
		{
			buildings_dict_id.Add(pBuilding.asset.id, value = new List<Building>());
		}
		value.Add(pBuilding);
	}

	public int countBuildingsType(string pBuildingTypeID, bool pCountOnlyFinished = true)
	{
		List<Building> buildingListOfType = getBuildingListOfType(pBuildingTypeID);
		if (buildingListOfType == null)
		{
			return 0;
		}
		if (pCountOnlyFinished)
		{
			int num = 0;
			{
				foreach (Building item in buildingListOfType)
				{
					if (!item.isUnderConstruction())
					{
						num++;
					}
				}
				return num;
			}
		}
		return buildingListOfType.Count;
	}

	internal bool hasBuildingType(string pBuildingTypeID, bool pCountOnlyFinished = true, TileIsland pLimitIsland = null)
	{
		List<Building> buildingListOfType = getBuildingListOfType(pBuildingTypeID);
		if (buildingListOfType == null)
		{
			return false;
		}
		if (buildingListOfType.Count == 0)
		{
			return false;
		}
		bool flag = pLimitIsland != null;
		foreach (Building item in buildingListOfType)
		{
			if ((!pCountOnlyFinished || (!item.isUnderConstruction() && item.isUsable())) && (!flag || item.current_island == pLimitIsland))
			{
				return true;
			}
		}
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal List<Building> getBuildingListOfType(string pType)
	{
		buildings_dict_type.TryGetValue(pType, out var value);
		return value;
	}

	internal Building getBuildingOfType(string pBuildingTypeID, bool pCountOnlyFinished = true, bool pRandom = false, bool pOnlyFreeTile = false, TileIsland pLimitIsland = null)
	{
		List<Building> buildingListOfType = getBuildingListOfType(pBuildingTypeID);
		if (buildingListOfType == null)
		{
			return null;
		}
		if (buildingListOfType.Count == 0)
		{
			return null;
		}
		bool flag = pLimitIsland != null;
		IEnumerable<Building> enumerable2;
		if (!pRandom)
		{
			IEnumerable<Building> enumerable = buildingListOfType;
			enumerable2 = enumerable;
		}
		else
		{
			enumerable2 = buildingListOfType.LoopRandom();
		}
		foreach (Building item in enumerable2)
		{
			if ((!pCountOnlyFinished || (!item.isUnderConstruction() && item.isUsable())) && (!pOnlyFreeTile || !item.current_tile.isTargeted()) && (!flag || item.current_island == pLimitIsland))
			{
				return item;
			}
		}
		return null;
	}

	public void addRoads(List<WorldTile> pTiles)
	{
		for (int i = 0; i < pTiles.Count; i++)
		{
			WorldTile worldTile = pTiles[i];
			if (!worldTile.Type.road && !road_tiles_to_build.Contains(worldTile))
			{
				road_tiles_to_build.Add(worldTile);
			}
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private bool isArmyFull()
	{
		if (status.warriors_current >= status.warrior_slots)
		{
			return true;
		}
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private bool isArmyOverLimit()
	{
		if (status.warriors_current > status.warrior_slots)
		{
			return true;
		}
		return false;
	}

	private bool tryToMakeWarrior(Actor pActor)
	{
		if (!checkCanMakeWarrior(pActor))
		{
			return false;
		}
		makeWarrior(pActor);
		_timer_warrior = 15f;
		if (hasLeader())
		{
			float num = leader.stats["warfare"] / 2f;
			_timer_warrior -= num;
			if (_timer_warrior < 1f)
			{
				_timer_warrior = 1f;
			}
		}
		if (hasBuildingType("type_barracks"))
		{
			_timer_warrior /= 2f;
		}
		return true;
	}

	public bool checkCanMakeWarrior(Actor pActor)
	{
		if (isArmyFull())
		{
			return false;
		}
		if (pActor.isBaby())
		{
			return false;
		}
		if (hasCulture())
		{
			if (pActor.isSexFemale() && culture.hasTrait("conscription_male_only"))
			{
				return false;
			}
			if (pActor.isSexMale() && culture.hasTrait("conscription_female_only"))
			{
				return false;
			}
		}
		return true;
	}

	public void makeWarrior(Actor pActor)
	{
		pActor.setProfession(UnitProfession.Warrior);
		if (pActor.equipment.weapon.isEmpty())
		{
			giveItem(pActor, getEquipmentList(EquipmentType.Weapon), this);
		}
		status.warriors_current++;
	}

	public bool checkIfWarriorStillOk(Actor pActor)
	{
		bool flag = true;
		if (isArmyOverLimit())
		{
			flag = false;
		}
		else if (!hasEnoughFoodForArmy())
		{
			flag = false;
		}
		if (!flag)
		{
			pActor.stopBeingWarrior();
			_timer_warrior = 30f;
		}
		return flag;
	}

	public void setCitizenJob(Actor pActor)
	{
		if ((!isGettingCaptured() && _timer_warrior <= 0f && pActor.isProfession(UnitProfession.Unit) && getResourcesAmount("gold") > 10 && hasEnoughFoodForArmy() && tryToMakeWarrior(pActor)) || checkCitizenJobList(AssetManager.citizen_job_library.list_priority_high, pActor) || (!hasAnyFood() && checkCitizenJobList(AssetManager.citizen_job_library.list_priority_high_food, pActor)))
		{
			return;
		}
		List<CitizenJobAsset> list_priority_normal = AssetManager.citizen_job_library.list_priority_normal;
		for (int i = 0; i < list_priority_normal.Count; i++)
		{
			_last_checked_job_id++;
			if (_last_checked_job_id > list_priority_normal.Count - 1)
			{
				_last_checked_job_id = 0;
			}
			CitizenJobAsset citizenJobAsset = list_priority_normal[_last_checked_job_id];
			if ((citizenJobAsset.ok_for_king || !pActor.isKing()) && (citizenJobAsset.ok_for_leader || !pActor.isCityLeader()) && checkCitizenJob(citizenJobAsset, this, pActor))
			{
				break;
			}
		}
	}

	private bool checkCitizenJobList(List<CitizenJobAsset> pList, Actor pActor)
	{
		for (int i = 0; i < pList.Count; i++)
		{
			CitizenJobAsset pJobAsset = pList[i];
			if (checkCitizenJob(pJobAsset, this, pActor))
			{
				return true;
			}
		}
		return false;
	}

	private bool checkCitizenJob(CitizenJobAsset pJobAsset, City pCity, Actor pActor)
	{
		if (pJobAsset.only_leaders && !pActor.isKing() && !pActor.isCityLeader())
		{
			return false;
		}
		if (pJobAsset.should_be_assigned != null && !pJobAsset.should_be_assigned(pActor))
		{
			return false;
		}
		if (jobs.hasJob(pJobAsset))
		{
			jobs.takeJob(pJobAsset);
			pActor.setCitizenJob(pJobAsset);
			return true;
		}
		return false;
	}

	public bool hasSuitableFood(Subspecies pSubspecies)
	{
		HashSet<string> allowedFoodByDiet = pSubspecies.getAllowedFoodByDiet();
		foreach (Building storage in storages)
		{
			if (!storage.isUsable())
			{
				continue;
			}
			foreach (string item in allowedFoodByDiet)
			{
				if (storage.getResourcesAmount(item) != 0)
				{
					return true;
				}
			}
		}
		return false;
	}

	internal ResourceAsset getFoodItem(Subspecies pSubspecies, string pFavoriteFood = null)
	{
		if (!string.IsNullOrEmpty(pFavoriteFood) && getResourcesAmount(pFavoriteFood) > 0)
		{
			return AssetManager.resources.get(pFavoriteFood);
		}
		return getRandomSuitableFood(pSubspecies);
	}

	internal void eatFoodItem(string pItem)
	{
		if (pItem != null)
		{
			takeResource(pItem, 1);
			data.total_food_consumed++;
		}
	}

	internal void removeZone(TileZone pZone)
	{
		setAbandonedZonesDirty();
		if (zones.Remove(pZone))
		{
			pZone.setCity(null);
			World.world.city_zone_helper.city_place_finder.setDirty();
		}
		updateCityCenter();
		setStatusDirty();
	}

	internal void addZone(TileZone pZone)
	{
		if (!zones.Contains(pZone))
		{
			if (pZone.city != null)
			{
				pZone.city.removeZone(pZone);
			}
			zones.Add(pZone);
			pZone.setCity(this);
			updateCityCenter();
			if (World.world.city_zone_helper.city_place_finder.hasPossibleZones())
			{
				World.world.city_zone_helper.city_place_finder.setDirty();
			}
			setStatusDirty();
		}
	}

	public int getLoyalty(bool pForceRecalc = false)
	{
		if (kingdom.isNeutral())
		{
			_loyalty_cached = 0;
		}
		else if ((World.world.getWorldTimeElapsedSince(_loyalty_last_time) > 3f) | pForceRecalc)
		{
			_loyalty_cached = LoyaltyCalculator.calculate(this);
			_loyalty_last_time = World.world.getCurWorldTime();
		}
		return _loyalty_cached;
	}

	public int getCachedLoyalty()
	{
		return _loyalty_cached;
	}

	public bool isCapitalCity()
	{
		if (kingdom == null)
		{
			return false;
		}
		return this == kingdom.capital;
	}

	internal void updateAge()
	{
		if (hasLeader() && leader.hasClan())
		{
			leader.addRenown(1);
		}
	}

	private void updateCityCenter()
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0154: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		if (!hasZones())
		{
			city_center = Globals.POINT_IN_VOID_2;
			return;
		}
		float num = 0f;
		float num2 = 0f;
		float num3 = float.MaxValue;
		TileZone tileZone = null;
		for (int i = 0; i < zones.Count; i++)
		{
			TileZone tileZone2 = zones[i];
			num += tileZone2.centerTile.posV3.x;
			num2 += tileZone2.centerTile.posV3.y;
		}
		city_center.x = num / (float)zones.Count;
		city_center.y = num2 / (float)zones.Count;
		for (int j = 0; j < zones.Count; j++)
		{
			TileZone tileZone3 = zones[j];
			float num4 = Toolbox.SquaredDist(tileZone3.centerTile.x, tileZone3.centerTile.y, city_center.x, city_center.y);
			if (num4 < num3)
			{
				tileZone = tileZone3;
				num3 = num4;
			}
		}
		city_center.x = tileZone.centerTile.posV3.x;
		city_center.y = tileZone.centerTile.posV3.y + 2f;
		last_city_center = city_center;
	}

	internal void removeFromCurrentKingdom()
	{
		kingdom.checkClearCapital(this);
	}

	internal void switchedKingdom()
	{
		List<Building> list = buildings;
		for (int i = 0; i < list.Count; i++)
		{
			Building building = list[i];
			if (!building.isRemoved())
			{
				building.setKingdomCiv(kingdom);
			}
		}
		World.world.zone_calculator.setDrawnZonesDirty();
	}

	internal void useInspire(Actor pActor)
	{
		Kingdom pAttacker = kingdom;
		makeOwnKingdom(pActor, pRebellion: true);
		World.world.diplomacy.startWar(pAttacker, kingdom, WarTypeLibrary.inspire, pLog: false);
	}

	internal void clearCurrentCaptureAmounts()
	{
		_capturing_units.Clear();
	}

	internal void clearDangerZones()
	{
		danger_zones.Clear();
	}

	public bool isInDanger()
	{
		if (danger_zones.Count > 0)
		{
			return true;
		}
		return false;
	}

	internal void updateConquest(Actor pActor)
	{
		if (pActor.isKingdomCiv() && (pActor.kingdom == kingdom || pActor.kingdom.isEnemy(kingdom)))
		{
			addCapturePoints(pActor, 1);
		}
	}

	public void addCapturePoints(BaseSimObject pObject, int pValue)
	{
		addCapturePoints(pObject.kingdom, pValue);
	}

	public void addCapturePoints(Kingdom pKingdom, int pValue)
	{
		_capturing_units.TryGetValue(pKingdom, out var value);
		_capturing_units[pKingdom] = value + pValue;
	}

	public void debugCaptureUnits(DebugTool pTool)
	{
		pTool.setText("capture units:", _capturing_units.Count, 0f, pShowBar: false, 0L);
		pTool.setText("isGettingCaptured()", isGettingCaptured(), 0f, pShowBar: false, 0L);
		foreach (Kingdom key in _capturing_units.Keys)
		{
			pTool.setText("-" + key.name, _capturing_units[key], 0f, pShowBar: false, 0L);
		}
	}

	internal void finishCapture(Kingdom pNewKingdom)
	{
		if (this.kingdom.hasKing() && this.kingdom.king.city == this)
		{
			this.kingdom.kingFledCity();
		}
		if (World.world.cities.isLocked())
		{
			return;
		}
		clearCapture();
		recalculateNeighbourCities();
		pNewKingdom.increaseHappinessFromNewCityCapture();
		this.kingdom.decreaseHappinessFromLostCityCapture(this);
		using ListPool<War> pWars = new ListPool<War>(pNewKingdom.getWars());
		Kingdom kingdom = findKingdomToJoinAfterCapture(pNewKingdom, pWars);
		if (!checkRebelWar(kingdom, pWars))
		{
			kingdom.data.timestamp_new_conquest = World.world.getCurWorldTime();
		}
		removeSoldiers();
		joinAnotherKingdom(kingdom, pCaptured: true);
	}

	private Kingdom findKingdomToJoinAfterCapture(Kingdom pKingdom, ListPool<War> pWars)
	{
		Kingdom kingdom = null;
		for (int i = 0; i < pWars.Count; i++)
		{
			War war = pWars[i];
			if (war.isTotalWar() || !war.hasKingdom(this.kingdom) || !war.isInWarWith(pKingdom, this.kingdom))
			{
				continue;
			}
			if (war.isMainAttacker(pKingdom) || war.isMainDefender(pKingdom))
			{
				break;
			}
			if (war.isAttacker(this.kingdom))
			{
				Kingdom main_defender = war.main_defender;
				if (!main_defender.isRekt())
				{
					kingdom = ((!neighbours_kingdoms.Contains(main_defender)) ? ((!neighbours_kingdoms.Contains(pKingdom)) ? main_defender : pKingdom) : main_defender);
					break;
				}
			}
			if (war.isDefender(this.kingdom))
			{
				Kingdom main_attacker = war.main_attacker;
				if (!main_attacker.isRekt())
				{
					kingdom = ((!neighbours_kingdoms.Contains(main_attacker)) ? ((!neighbours_kingdoms.Contains(pKingdom)) ? main_attacker : pKingdom) : main_attacker);
					break;
				}
			}
		}
		if (kingdom == null)
		{
			kingdom = pKingdom;
		}
		else if (kingdom.getSpecies() != this.kingdom.getSpecies())
		{
			kingdom = pKingdom;
		}
		return kingdom;
	}

	private bool checkRebelWar(Kingdom pKingdomToJoin, ListPool<War> pWars)
	{
		foreach (ref War pWar in pWars)
		{
			War current = pWar;
			if (current.getAsset().rebellion && current.isMainAttacker(pKingdomToJoin) && current.isInWarWith(pKingdomToJoin, kingdom))
			{
				return true;
			}
		}
		return false;
	}

	private void removeSoldiers()
	{
		foreach (Actor item in _professions_dict[UnitProfession.Warrior])
		{
			item.setProfession(UnitProfession.Unit);
		}
		disbandArmy();
	}

	public void disbandArmy()
	{
		checkArmyExistence();
		if (hasArmy())
		{
			army.disband();
			checkArmyExistence();
		}
	}

	public void checkArmyExistence()
	{
		if (hasArmy() && (!army.isAlive() || army.countUnits() <= 0))
		{
			setArmy(null);
		}
	}

	public bool hasArmy()
	{
		return army != null;
	}

	public Army getArmy()
	{
		return army;
	}

	public void setArmy(Army pArmy)
	{
		if (army != null && army != pArmy)
		{
			army.clearCity();
		}
		army = pArmy;
	}

	public Actor getRandomWarrior()
	{
		return _professions_dict[UnitProfession.Warrior].GetRandom();
	}

	internal Kingdom makeOwnKingdom(Actor pActor, bool pRebellion = false, bool pFellApart = false)
	{
		string pHappinessEvent = null;
		if (pRebellion)
		{
			World.world.game_stats.data.citiesRebelled++;
			World.world.map_stats.citiesRebelled++;
			pHappinessEvent = "just_rebelled";
		}
		if (pFellApart)
		{
			pHappinessEvent = "kingdom_fell_apart";
		}
		Kingdom pKingdom = this.kingdom;
		removeFromCurrentKingdom();
		removeLeader();
		Kingdom kingdom = World.world.kingdoms.makeNewCivKingdom(pActor);
		setKingdom(kingdom);
		newForceKingdomEvent(base.units, _boats, kingdom, pHappinessEvent);
		switchedKingdom();
		kingdom.copyMetasFromOtherKingdom(pKingdom);
		kingdom.setCityMetas(this);
		return kingdom;
	}

	public override int getPopulationPeople()
	{
		return countUnits();
	}

	public int getPopulationMaximum()
	{
		if (WorldLawLibrary.world_law_civ_limit_population_100.isEnabled())
		{
			if (status.housing_total >= 100)
			{
				return 100;
			}
			return status.housing_total;
		}
		return status.housing_total;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int getUnitsTotal()
	{
		return countUnits() + countBoats();
	}

	public int countPopulationChildren()
	{
		int num = 0;
		foreach (Actor unit in base.units)
		{
			if (unit.isAlive() && unit.isBaby())
			{
				num++;
			}
		}
		return num;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int countBoats()
	{
		return _boats.Count;
	}

	public void joinAnotherKingdom(Kingdom pNewSetKingdom, bool pCaptured = false, bool pRebellion = false)
	{
		string pHappinessEvent = null;
		if (pCaptured)
		{
			World.world.game_stats.data.citiesConquered++;
			World.world.map_stats.citiesConquered++;
			pHappinessEvent = "was_conquered";
		}
		if (pRebellion)
		{
			World.world.game_stats.data.citiesRebelled++;
			World.world.map_stats.citiesRebelled++;
			pHappinessEvent = "just_rebelled";
		}
		Kingdom pKingdom = kingdom;
		removeFromCurrentKingdom();
		setKingdom(pNewSetKingdom);
		newForceKingdomEvent(base.units, _boats, pNewSetKingdom, pHappinessEvent);
		switchedKingdom();
		pNewSetKingdom.capturedFrom(pKingdom);
	}

	public int countWeapons()
	{
		return getEquipmentList(EquipmentType.Weapon).Count;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int countFoodTotal()
	{
		return countFood();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool hasEnoughFoodForArmy()
	{
		return true;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int getTotalFood()
	{
		return _current_total_food;
	}

	public bool hasAnyFood()
	{
		return _current_total_food > 0;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int countWarriors()
	{
		return countProfession(UnitProfession.Warrior);
	}

	public bool hasAnyWarriors()
	{
		return countWarriors() > 0;
	}

	public bool isHappy()
	{
		if (getCachedLoyalty() >= 0)
		{
			return true;
		}
		return false;
	}

	public float getArmyMaxMultiplier()
	{
		float num = 0f + getActorAsset().civ_base_army_multiplier;
		float armyMaxLeaderMultiplier = getArmyMaxLeaderMultiplier();
		return num + armyMaxLeaderMultiplier;
	}

	public float getArmyMaxLeaderMultiplier()
	{
		float num = 0f;
		if (hasLeader())
		{
			num += leader.stats["army"];
			float num2 = leader.stats["warfare"] * 2f / 100f;
			num += num2;
		}
		return num;
	}

	public int getMaxWarriors()
	{
		return status.warrior_slots;
	}

	public void removeLeader()
	{
		leader = null;
		data.leaderID = -1L;
		rulerLeft();
	}

	public void setLeader(Actor pActor, bool pNew)
	{
		if (pActor != null && kingdom.king != pActor)
		{
			leader = pActor;
			leader.setProfession(UnitProfession.Leader);
			CityData cityData = data;
			long leaderID = (data.last_leader_id = pActor.data.id);
			cityData.leaderID = leaderID;
			if (pNew)
			{
				data.total_leaders++;
				leader.changeHappiness("become_leader");
				addRuler(pActor);
			}
		}
	}

	public void updateRulers()
	{
		if (data.past_rulers == null || data.past_rulers.Count == 0)
		{
			return;
		}
		foreach (LeaderEntry past_ruler in data.past_rulers)
		{
			Actor actor = World.world.units.get(past_ruler.id);
			if (!actor.isRekt())
			{
				past_ruler.name = actor.name;
			}
		}
	}

	public void addRuler(Actor pActor)
	{
		CityData cityData = data;
		if (cityData.past_rulers == null)
		{
			cityData.past_rulers = new List<LeaderEntry>();
		}
		rulerLeft();
		data.past_rulers.Add(new LeaderEntry
		{
			id = pActor.getID(),
			name = pActor.name,
			color_id = (pActor.kingdom?.data.color_id ?? (-1)),
			timestamp_ago = World.world.getCurWorldTime()
		});
		if (data.past_rulers.Count > 30)
		{
			data.past_rulers.Shift();
		}
	}

	public void rulerLeft()
	{
		if (data.past_rulers != null && data.past_rulers.Count != 0)
		{
			LeaderEntry leaderEntry = data.past_rulers.Last();
			if (!(leaderEntry.timestamp_end >= leaderEntry.timestamp_ago))
			{
				leaderEntry.timestamp_end = World.world.getCurWorldTime();
				updateRulers();
			}
		}
	}

	public static bool nearbyBorders(City pA, City pB)
	{
		City city;
		City city2;
		if (pA.zones.Count > pB.zones.Count)
		{
			city = pB;
			city2 = pA;
		}
		else
		{
			city = pA;
			city2 = pB;
		}
		for (int i = 0; i < city.zones.Count; i++)
		{
			TileZone[] neighbours_all = city.zones[i].neighbours_all;
			for (int j = 0; j < neighbours_all.Length; j++)
			{
				if (neighbours_all[j].city == city2)
				{
					return true;
				}
			}
		}
		return false;
	}

	public static bool giveItem(Actor pActor, List<long> pItems, City pCity)
	{
		if (pItems.Count == 0)
		{
			return false;
		}
		if (!pActor.understandsHowToUseItems())
		{
			return false;
		}
		long random = pItems.GetRandom();
		Item item = World.world.items.get(random);
		EquipmentAsset asset = item.getAsset();
		ActorEquipmentSlot slot = pActor.equipment.getSlot(asset.equipment_type);
		if (!slot.isEmpty())
		{
			int value = slot.getItem().getValue();
			if (item.getValue() <= value)
			{
				return false;
			}
		}
		Item item2 = null;
		if (!slot.isEmpty())
		{
			item2 = slot.getItem();
			slot.takeAwayItem();
		}
		pItems.Remove(random);
		slot.setItem(item, pActor);
		pActor.setStatsDirty();
		if (item2 != null)
		{
			pCity.data.equipment.addItem(pCity, item2, pItems);
		}
		pCity._storage_version++;
		return true;
	}

	public int getLimitOfBuildingsType(BuildOrder pElement)
	{
		int num = pElement.limit_type;
		if (hasCulture())
		{
			string type = pElement.getBuildingAsset(this).type;
			if (!(type == "type_statue"))
			{
				if (type == "type_watch_tower")
				{
					if (culture.hasTrait("tower_lovers"))
					{
						num += CultureTraitLibrary.getValue("tower_lovers");
					}
					if (hasLeader())
					{
						num += (int)leader.stats["bonus_towers"];
					}
				}
			}
			else if (culture.hasTrait("statue_lovers"))
			{
				num += CultureTraitLibrary.getValue("statue_lovers");
			}
		}
		return num;
	}

	public Alliance getAlliance()
	{
		return kingdom.getAlliance();
	}

	public Clan getRoyalClan()
	{
		Clan clan = null;
		if (clan == null && hasLeader())
		{
			clan = leader.clan;
		}
		if (clan == null && kingdom.hasKing())
		{
			clan = kingdom.king.clan;
		}
		return clan;
	}

	public bool isOkToSendArmy()
	{
		if (!hasArmy())
		{
			return false;
		}
		float num = getMaxWarriors();
		return (float)army.countUnits() / num >= 0.7f;
	}

	public void tryToPutItem(Item pItem)
	{
		List<long> equipmentList = data.equipment.getEquipmentList(pItem.getAsset().equipment_type);
		if (equipmentList.Count >= status.maximum_items)
		{
			tryToPutItemInStorage(pItem);
			return;
		}
		data.equipment.addItem(this, pItem, equipmentList);
		_storage_version++;
	}

	public void tryToPutItems(IEnumerable<Item> pItems)
	{
		foreach (Item pItem in pItems)
		{
			tryToPutItem(pItem);
		}
	}

	private void tryToPutItemInStorage(Item pNewItem)
	{
		float num = pNewItem.getValue();
		EquipmentType equipment_type = pNewItem.getAsset().equipment_type;
		List<long> equipmentList = data.equipment.getEquipmentList(equipment_type);
		for (int i = 0; i < equipmentList.Count; i++)
		{
			long pID = equipmentList[i];
			Item item = World.world.items.get(pID);
			float num2 = item.getValue();
			if (num > num2)
			{
				item.clearCity();
				equipmentList[i] = pNewItem.id;
				pNewItem.setInCityStorage(this);
				_storage_version++;
				break;
			}
		}
	}

	public int getZoneRange(bool pAllowCheat = true)
	{
		if (pAllowCheat && DebugConfig.isOn(DebugOption.CityUnlimitedZoneRange))
		{
			return 999;
		}
		return 13;
	}

	public bool reachableFrom(City pCity)
	{
		WorldTile tile = getTile();
		if (tile == null)
		{
			return false;
		}
		WorldTile tile2 = pCity.getTile();
		if (tile2 == null)
		{
			return false;
		}
		return tile.reachableFrom(tile2);
	}

	public bool hasLeader()
	{
		if (leader == null)
		{
			return false;
		}
		if (!leader.isAlive())
		{
			removeLeader();
			return false;
		}
		return true;
	}

	public override void convertSameSpeciesAroundUnit(Actor pActorMain, bool pOverride = false)
	{
		foreach (Actor item in Finder.getUnitsFromChunk(pActorMain.current_tile, 2))
		{
			if (!item.hasCity() && !item.isKingdomCiv() && item.isSameSpecies(pActorMain) && item.isSapient())
			{
				item.joinCity(this);
			}
		}
	}

	public override void forceConvertSameSpeciesAroundUnit(Actor pActorMain)
	{
		convertSameSpeciesAroundUnit(pActorMain, true);
	}

	public void setUnitMetas(Actor pActor)
	{
		if (pActor.hasCulture())
		{
			setCulture(pActor.culture);
		}
		if (pActor.hasLanguage())
		{
			setLanguage(pActor.language);
		}
		if (pActor.hasReligion())
		{
			setReligion(pActor.religion);
		}
	}

	public override void save()
	{
		base.save();
		if (hasCulture())
		{
			data.id_culture = culture.id;
		}
		if (hasReligion())
		{
			data.id_religion = religion.id;
		}
		if (hasLanguage())
		{
			data.id_language = language.id;
		}
		if (kingdom == null)
		{
			data.kingdomID = -1L;
		}
		else
		{
			data.kingdomID = kingdom.id;
		}
		data.zones.Clear();
		foreach (TileZone zone in zones)
		{
			ZoneData item = new ZoneData
			{
				x = zone.x,
				y = zone.y
			};
			data.zones.Add(item);
		}
	}

	public bool hasCulture()
	{
		if (culture != null && !culture.isAlive())
		{
			setCulture(null);
		}
		return culture != null;
	}

	public bool hasLanguage()
	{
		if (language != null && !language.isAlive())
		{
			setLanguage(null);
		}
		return language != null;
	}

	internal void setLanguage(Language pLanguage)
	{
		if (language != pLanguage)
		{
			language = pLanguage;
			World.world.languages.setDirtyCities();
		}
	}

	internal void setReligion(Religion pReligion)
	{
		if (religion != pReligion)
		{
			religion = pReligion;
			World.world.religions.setDirtyCities();
		}
	}

	public Subspecies getMainSubspecies()
	{
		if (hasLeader())
		{
			return leader.subspecies;
		}
		if (getPopulationPeople() == 0)
		{
			return null;
		}
		return base.units[0].subspecies;
	}

	public bool hasReligion()
	{
		if (religion != null && !religion.isAlive())
		{
			setReligion(null);
		}
		return religion != null;
	}

	public bool hasStockpiles()
	{
		return stockpiles.Count > 0;
	}

	public bool hasStorages()
	{
		return storages.Count > 0;
	}

	public Building getRandomStockpile()
	{
		if (!hasStockpiles())
		{
			return null;
		}
		foreach (Building item in stockpiles.LoopRandom())
		{
			if (item.isUsable())
			{
				return item;
			}
		}
		return null;
	}

	public void takeResource(string pResourceID, int pAmount)
	{
		if (!hasStorages())
		{
			return;
		}
		int num = pAmount;
		foreach (Building storage in storages)
		{
			if (storage.isUsable())
			{
				int num2 = 0;
				num2 = ((storage.getResourcesAmount(pResourceID) < num) ? storage.getResourcesAmount(pResourceID) : num);
				storage.takeResource(pResourceID, num2);
				num -= num2;
				if (num == 0)
				{
					break;
				}
			}
		}
		_storage_version++;
	}

	public int getResourcesAmount(string pResourceID)
	{
		if (!hasStorages())
		{
			return 0;
		}
		int num = 0;
		foreach (Building storage in storages)
		{
			if (storage.isUsable())
			{
				num += storage.getResourcesAmount(pResourceID);
			}
		}
		return num;
	}

	public int addResourcesToRandomStockpile(string pResourceID, int pAmount = 1)
	{
		Building randomStockpile = getRandomStockpile();
		if (randomStockpile == null)
		{
			return 0;
		}
		_storage_version++;
		return randomStockpile.addResources(pResourceID, pAmount);
	}

	public bool hasSpaceForResourceInStockpile(ResourceAsset pResourceAsset)
	{
		if (!hasStockpiles())
		{
			return false;
		}
		foreach (Building stockpile in stockpiles)
		{
			if (stockpile.isUsable() && stockpile.hasSpaceForResource(pResourceAsset))
			{
				return true;
			}
		}
		return false;
	}

	public bool hasResourcesForNewItems()
	{
		if (!hasStorages())
		{
			return false;
		}
		foreach (Building storage in storages)
		{
			if (storage.isUsable() && storage.hasResourcesForNewItems())
			{
				return true;
			}
		}
		return false;
	}

	public ResourceAsset getRandomSuitableFood(Subspecies pSubspecies)
	{
		if (!hasStorages())
		{
			return null;
		}
		foreach (Building storage in storages)
		{
			if (storage.isUsable())
			{
				ResourceAsset randomSuitableFood = storage.getRandomSuitableFood(pSubspecies);
				if (randomSuitableFood != null)
				{
					return randomSuitableFood;
				}
			}
		}
		return null;
	}

	public int countFood()
	{
		if (!hasStorages())
		{
			return 0;
		}
		int num = 0;
		foreach (Building storage in storages)
		{
			if (storage.isUsable())
			{
				num += storage.countFood();
			}
		}
		return num;
	}

	public ListPool<CityStorageSlot> getTotalResourceSlots(ResType[] pResTypes)
	{
		foreach (CityStorageSlot value2 in _total_resource_slots.Values)
		{
			ResourceAsset asset = value2.asset;
			if (pResTypes.IndexOf(asset.type) != -1)
			{
				value2.amount = 0;
			}
		}
		foreach (Building storage in storages)
		{
			if (!storage.isUsable())
			{
				continue;
			}
			foreach (CityStorageSlot slot in storage.resources.getSlots())
			{
				_total_resource_slots.TryGetValue(slot.id, out var value);
				if (value == null)
				{
					value = new CityStorageSlot(slot.id);
					_total_resource_slots[slot.id] = value;
				}
				value.amount += slot.amount;
			}
		}
		ListPool<CityStorageSlot> listPool = new ListPool<CityStorageSlot>(_total_resource_slots.Count);
		foreach (CityStorageSlot value3 in _total_resource_slots.Values)
		{
			ResourceAsset asset2 = value3.asset;
			if (pResTypes.IndexOf(asset2.type) != -1 && value3.amount != 0)
			{
				listPool.Add(value3);
			}
		}
		listPool.Sort((CityStorageSlot a, CityStorageSlot b) => a.asset.order.CompareTo(b.asset.order));
		return listPool;
	}

	public bool hasKingdom()
	{
		return kingdom != null;
	}

	public float getTimerForNewWarrior()
	{
		return _timer_warrior;
	}

	public List<long> getEquipmentList(EquipmentType pType)
	{
		return data.equipment.getEquipmentList(pType);
	}

	public bool planAllowsToPlaceBuildingInZone(TileZone pZone, TileZone pCenterZone)
	{
		if (status.housing_total < 10 && zones.Count < 20)
		{
			return true;
		}
		return culture.planAllowsToPlaceBuildingInZone(pZone, pCenterZone);
	}

	public bool hasSpecialTownPlans()
	{
		if (!hasCulture())
		{
			return false;
		}
		return culture.hasSpecialTownPlans();
	}

	public bool isNeutral()
	{
		return kingdom.isNeutral();
	}

	public bool isWelcomedToJoin(Actor pActor)
	{
		if (pActor.kingdom == kingdom)
		{
			return true;
		}
		if (pActor.isSameSubspecies(getMainSubspecies()))
		{
			return true;
		}
		if (!hasCulture())
		{
			return false;
		}
		if (culture.hasTrait("xenophobic"))
		{
			return false;
		}
		if (pActor.hasCultureTrait("xenophobic"))
		{
			return false;
		}
		if (culture.hasTrait("xenophiles"))
		{
			if (!pActor.hasCulture())
			{
				return true;
			}
			if (pActor.hasCultureTrait("xenophiles"))
			{
				return true;
			}
		}
		if (isSameSpeciesAsActor(pActor))
		{
			return true;
		}
		return false;
	}

	public bool isSameSpeciesAsActor(Actor pActor)
	{
		if (pActor.isSameSpecies(getCurrentSpecies()))
		{
			return true;
		}
		return false;
	}

	public string getCurrentSpecies()
	{
		Subspecies mainSubspecies = getMainSubspecies();
		if (mainSubspecies != null)
		{
			return mainSubspecies.getActorAsset().id;
		}
		return getActorAsset().id;
	}

	public Sprite getCurrentSpeciesIcon()
	{
		Subspecies mainSubspecies = getMainSubspecies();
		if (mainSubspecies != null)
		{
			return mainSubspecies.getSpriteIcon();
		}
		return getActorAsset().getSpriteIcon();
	}

	public bool hasTransportBoats()
	{
		foreach (Actor boat in _boats)
		{
			if (boat.asset.is_boat_transport)
			{
				return true;
			}
		}
		return false;
	}

	public bool isCityUnderDangerFire()
	{
		return tasks.fire > 0;
	}

	public bool isPossibleToJoin(Actor pActor)
	{
		if (this == pActor.city)
		{
			return false;
		}
		if (isNeutral())
		{
			return true;
		}
		if (!isWelcomedToJoin(pActor))
		{
			return false;
		}
		if (pActor.city != null)
		{
			if (pActor.isKing())
			{
				return false;
			}
			if (pActor.isCityLeader())
			{
				return false;
			}
			if (pActor.city.getPopulationPeople() < getPopulationPeople())
			{
				return false;
			}
		}
		return true;
	}

	public override string ToString()
	{
		if (data == null)
		{
			return "[City is null]";
		}
		using StringBuilderPool stringBuilderPool = new StringBuilderPool();
		stringBuilderPool.Append($"[City:{base.id} ");
		if (!isAlive())
		{
			stringBuilderPool.Append("[DEAD] ");
		}
		stringBuilderPool.Append("\"" + name + "\" ");
		stringBuilderPool.Append($"Kingdom:{kingdom?.id ?? (-1)} ");
		if (hasArmy())
		{
			stringBuilderPool.Append($"Army:{army.id} ");
		}
		stringBuilderPool.Append($"Units:{base.units.Count} ");
		if (isDirtyUnits())
		{
			stringBuilderPool.Append("[Dirty] ");
		}
		if (!leader.isRekt())
		{
			stringBuilderPool.Append($"Leader:{leader.id} ");
		}
		if (kingdom?.king?.city == this)
		{
			stringBuilderPool.Append($"King:{kingdom.king.id} ");
		}
		return stringBuilderPool.ToString().Trim() + "]";
	}
}
