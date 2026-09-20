using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using UnityEngine;
using UnityPools;

public class TileZone : IDisposable
{
	private const int MAX_BUILDING_COLLECTIONS = 16;

	public int last_drawn_id;

	public int last_drawn_hashcode;

	public int last_color_asset_id;

	private bool? _last_animated;

	public (string, int)[] debug_args;

	public bool debug_show;

	private bool _dirty;

	public bool visible;

	public bool visible_main_centered;

	public int x;

	public int y;

	public int id;

	public readonly WorldTile[] tiles = new WorldTile[64];

	public Color debug_zone_color;

	[CanBeNull]
	public City city;

	public TileZone[] neighbours;

	public TileZone[] neighbours_all;

	public bool world_edge;

	public WorldTile centerTile;

	public int tiles_with_liquid;

	public int tiles_with_ground;

	public readonly List<Building> buildings_render_list = new List<Building>();

	public readonly HashSet<Building> buildings_all = new HashSet<Building>();

	private readonly HashSet<Building>[] _building_hashset_types_array = new HashSet<Building>[16];

	internal TileZone zone_up;

	internal TileZone zone_down;

	internal TileZone zone_left;

	internal TileZone zone_right;

	private readonly Dictionary<TileTypeBase, HashSet<WorldTile>> _tile_types = new Dictionary<TileTypeBase, HashSet<WorldTile>>();

	private bool _good_for_new_city;

	public static int debug_adapted;

	public static int debug_not_adapted;

	public static int debug_soil;

	public static bool debug_can_settle;

	public WorldTile top_left_corner_tile => tiles.Last();

	private CityPlaceFinder _city_place_finder => World.world.city_zone_helper.city_place_finder;

	public MapChunk chunk => centerTile.chunk;

	public bool checkShouldReRender(int pHashCode, int pID, int pColorAssetID, bool pLastAnimated)
	{
		if (last_drawn_id == pID && last_drawn_hashcode == pHashCode && _last_animated == pLastAnimated && last_color_asset_id == pColorAssetID)
		{
			return false;
		}
		last_drawn_id = pID;
		last_drawn_hashcode = pHashCode;
		_last_animated = pLastAnimated;
		last_color_asset_id = pColorAssetID;
		return true;
	}

	public void resetRenderHelpers()
	{
		last_color_asset_id = 0;
		last_drawn_hashcode = 0;
		last_drawn_id = 0;
		_last_animated = null;
	}

	public void clearDebug()
	{
		debug_args = null;
		debug_show = false;
	}

	public void showDebug(params (string key, int value)[] pArgs)
	{
		if (DebugConfig.isOn(DebugOption.DebugZones))
		{
			debug_show = true;
			debug_args = pArgs;
		}
	}

	public bool checkCanSettleInThisBiomes(Subspecies pSubspecies)
	{
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		foreach (KeyValuePair<TileTypeBase, HashSet<WorldTile>> tile_type in _tile_types)
		{
			TileTypeBase key = tile_type.Key;
			if (!key.biome_build_check)
			{
				continue;
			}
			if (key.soil)
			{
				num3 += tile_type.Value.Count;
				continue;
			}
			if (!key.is_biome)
			{
				num += tile_type.Value.Count;
				continue;
			}
			string only_allowed_to_build_with_tag = key.only_allowed_to_build_with_tag;
			if (string.IsNullOrEmpty(only_allowed_to_build_with_tag))
			{
				num += tile_type.Value.Count;
			}
			else if (!pSubspecies.hasMetaTag(only_allowed_to_build_with_tag))
			{
				num2 += tile_type.Value.Count;
			}
			else
			{
				num += tile_type.Value.Count;
			}
		}
		num3 -= num;
		num3 -= num2;
		bool flag = false;
		flag = num3 > num2 || ((num2 <= num) ? true : false);
		debug_adapted = num;
		debug_not_adapted = num2;
		debug_soil = num3;
		debug_can_settle = flag;
		return flag;
	}

	public bool hasAnyBuildings()
	{
		return buildings_all.Count > 0;
	}

	public HashSet<Building> getHashset(BuildingList pType)
	{
		return _building_hashset_types_array[(int)pType];
	}

	private HashSet<Building> getHashsetOrCreate(BuildingList pType)
	{
		HashSet<Building> hashSet = _building_hashset_types_array[(int)pType];
		if (hashSet == null)
		{
			hashSet = UnsafeCollectionPool<HashSet<Building>, Building>.Get();
			_building_hashset_types_array[(int)pType] = hashSet;
		}
		return hashSet;
	}

	public bool hasAnyBuildingsInSet(BuildingList pType)
	{
		HashSet<Building> hashset = getHashset(pType);
		if (hashset == null)
		{
			return false;
		}
		return hashset.Count > 0;
	}

	public int countBuildingsType(BuildingList pType)
	{
		return getHashset(pType)?.Count ?? 0;
	}

	public bool isGoodForNewCity(Actor pActor)
	{
		if (!checkCanSettleInThisBiomes(pActor.subspecies))
		{
			return false;
		}
		if (_city_place_finder.isDirty())
		{
			if (hasCity())
			{
				return false;
			}
			if (pActor.hasCity() && pActor.city.neighbour_zones.Contains(this))
			{
				return false;
			}
		}
		return isGoodForNewCity();
	}

	public bool isGoodForNewCity()
	{
		if (hasCity())
		{
			return false;
		}
		if (_city_place_finder.isDirty())
		{
			TileZone[] array = neighbours_all;
			int num = array.Length;
			for (int i = 0; i < num; i++)
			{
				if (array[i].hasCity())
				{
					return false;
				}
			}
			_city_place_finder.recalc();
		}
		return _good_for_new_city;
	}

	public void setGoodForNewCity(bool pValue)
	{
		_good_for_new_city = pValue;
	}

	public bool hasCity()
	{
		return city != null;
	}

	public bool isSameCityHere(City pCity)
	{
		return city == pCity;
	}

	public static bool hasZonesConnectedViaRegions(TileZone pZone1, TileZone pZone2, MapRegion pMainRegion1, ListPool<MapRegion> pListToFill)
	{
		if (pZone2.tiles_with_ground == 0)
		{
			return false;
		}
		if (!pMainRegion1.isTypeGround())
		{
			return false;
		}
		MapChunk mapChunk = pZone1.chunk;
		MapChunk mapChunk2 = pZone2.chunk;
		if (mapChunk == mapChunk2)
		{
			if (mapChunk.regions.Count == 1 && pMainRegion1.isTypeGround())
			{
				pListToFill.Add(pMainRegion1);
				return true;
			}
		}
		else if (mapChunk.regions.Count == 1 && mapChunk2.regions.Count == 1)
		{
			MapRegion mapRegion = mapChunk2.regions[0];
			if (pMainRegion1.isTypeGround() && mapRegion.isTypeGround())
			{
				pListToFill.Add(mapRegion);
				return true;
			}
		}
		List<MapRegion> regions = mapChunk2.regions;
		for (int i = 0; i < regions.Count; i++)
		{
			MapRegion mapRegion2 = regions[i];
			if (mapRegion2.isTypeGround() && mapRegion2.zones.Contains(pZone2))
			{
				if (pMainRegion1 == mapRegion2)
				{
					pListToFill.Add(pMainRegion1);
					return true;
				}
				if (pMainRegion1.island == mapRegion2.island && pMainRegion1.hasNeighbour(mapRegion2))
				{
					pListToFill.Add(mapRegion2);
				}
			}
		}
		return pListToFill.Count > 0;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool hasLiquid()
	{
		return tiles_with_liquid > 0;
	}

	public bool hasBuildingOf(City pCity)
	{
		foreach (Building item in getHashsetOrCreate(BuildingList.Civs))
		{
			if (item.isUsable() && item.city == pCity)
			{
				return true;
			}
		}
		return false;
	}

	public void addTileType(TileTypeBase pType, WorldTile pTile)
	{
		if (!_tile_types.TryGetValue(pType, out var value))
		{
			value = UnsafeCollectionPool<HashSet<WorldTile>, WorldTile>.Get();
			_tile_types.Add(pType, value);
		}
		value.Add(pTile);
		chunk.setTileTypesDirty();
	}

	public void removeTileType(TileTypeBase pType, WorldTile pTile)
	{
		if (_tile_types.TryGetValue(pType, out var value))
		{
			value.Remove(pTile);
			chunk.setTileTypesDirty();
		}
	}

	public HashSet<WorldTile> getTilesOfType(TileTypeBase pType)
	{
		if (_tile_types.TryGetValue(pType, out var value))
		{
			return value;
		}
		return null;
	}

	public bool hasTilesOfType(TileTypeBase pType)
	{
		if (_tile_types.TryGetValue(pType, out var value))
		{
			return value.Count > 0;
		}
		return false;
	}

	internal void addTile(WorldTile pTile, int pX, int pY)
	{
		tiles[tiles.FreeIndex()] = pTile;
		switch (pX)
		{
		case 0:
			pTile.world_tile_zone_border.border_left = true;
			pTile.world_tile_zone_border.border = true;
			break;
		case 7:
			pTile.world_tile_zone_border.border_right = true;
			pTile.world_tile_zone_border.border = true;
			break;
		}
		switch (pY)
		{
		case 0:
			pTile.world_tile_zone_border.border_down = true;
			pTile.world_tile_zone_border.border = true;
			break;
		case 7:
			pTile.world_tile_zone_border.border = true;
			pTile.world_tile_zone_border.border_up = true;
			break;
		}
	}

	internal void setCity(City pCity)
	{
		setDirty(pValue: true);
		if (hasCity() && city != pCity)
		{
			World.world.cities.setDirtyBuildings(city);
		}
		city = pCity;
	}

	public bool isDirty()
	{
		return _dirty;
	}

	public void setDirty(bool pValue)
	{
		_dirty = pValue;
		if (pValue)
		{
			BuildingZonesSystem.setDirty();
		}
		if (hasCity())
		{
			World.world.cities.setDirtyBuildings(city);
			city.setStatusDirty();
		}
	}

	public void addBuildingMain(Building pBuilding)
	{
		buildings_all.Add(pBuilding);
		setDirty(pValue: true);
	}

	public void removeBuildingMain(Building pBuilding)
	{
		buildings_all.Remove(pBuilding);
		setDirty(pValue: true);
	}

	internal void addBuildingToSet(Building pBuilding)
	{
		HashSet<Building> hashSet = (pBuilding.isRuin() ? getHashsetOrCreate(BuildingList.Ruins) : (pBuilding.asset.city_building ? ((!pBuilding.isAbandoned()) ? getHashsetOrCreate(BuildingList.Civs) : getHashsetOrCreate(BuildingList.Abandoned)) : (pBuilding.asset.building_type switch
		{
			BuildingType.Building_Plant => getHashsetOrCreate(BuildingList.Flora), 
			BuildingType.Building_Tree => getHashsetOrCreate(BuildingList.Trees), 
			BuildingType.Building_Mineral => getHashsetOrCreate(BuildingList.Minerals), 
			BuildingType.Building_Fruits => getHashsetOrCreate(BuildingList.Food), 
			BuildingType.Building_Hives => getHashsetOrCreate(BuildingList.Hives), 
			BuildingType.Building_Poops => getHashsetOrCreate(BuildingList.Poops), 
			BuildingType.Building_Wheat => getHashsetOrCreate(BuildingList.Wheat), 
			_ => getHashsetOrCreate(BuildingList.Civs), 
		})));
		hashSet.Add(pBuilding);
	}

	internal void addNeighbour(TileZone pNeighbour, TileDirection pDirection, IList<TileZone> pNeighbours, IList<TileZone> pNeighboursAll, bool pDiagonal = false)
	{
		if (pNeighbour == null)
		{
			world_edge = true;
			return;
		}
		if (!pDiagonal)
		{
			pNeighbours.Add(pNeighbour);
		}
		pNeighboursAll.Add(pNeighbour);
		switch (pDirection)
		{
		case TileDirection.Up:
			zone_up = pNeighbour;
			break;
		case TileDirection.Down:
			zone_down = pNeighbour;
			break;
		case TileDirection.Left:
			zone_left = pNeighbour;
			break;
		case TileDirection.Right:
			zone_right = pNeighbour;
			break;
		}
	}

	public bool canStartCityHere()
	{
		if (hasCity())
		{
			return false;
		}
		if (tiles_with_ground < 64)
		{
			return false;
		}
		if (hasTilesOfType(TileLibrary.hills))
		{
			return false;
		}
		return true;
	}

	public bool hasLava()
	{
		foreach (TileType lava_type in TileLibrary.lava_types)
		{
			if (hasTilesOfType(lava_type))
			{
				return true;
			}
		}
		return false;
	}

	public int countLava()
	{
		int num = 0;
		foreach (TileType lava_type in TileLibrary.lava_types)
		{
			HashSet<WorldTile> tilesOfType = getTilesOfType(lava_type);
			if (tilesOfType != null)
			{
				num += tilesOfType.Count;
			}
		}
		return num;
	}

	public IEnumerable<WorldTile> loopLava()
	{
		foreach (TileType item in TileLibrary.lava_types.LoopRandom())
		{
			HashSet<WorldTile> tilesOfType = getTilesOfType(item);
			if (tilesOfType == null)
			{
				continue;
			}
			foreach (WorldTile item2 in tilesOfType)
			{
				yield return item2;
			}
		}
	}

	public bool goodForExpansion()
	{
		if (hasLava())
		{
			return false;
		}
		return true;
	}

	public bool hasReachedBuildingLimit(WorldTile pTile, BuildingAsset pAsset)
	{
		int num = pAsset.limit_per_zone;
		if (pTile.isTileRank(TileRank.Low) && pAsset.building_type == BuildingType.Building_Tree)
		{
			num = (int)((float)num * 0.5f);
			if (num == 0)
			{
				num = 1;
			}
		}
		if (WorldLawLibrary.world_law_spread_density_high.isEnabled())
		{
			num = (int)((float)num * 1.5f);
		}
		if (pAsset.limit_in_radius > 0)
		{
			foreach (Building buildingFromZone in World.world.buildings.getBuildingFromZones(pTile, pAsset.limit_in_radius))
			{
				if (buildingFromZone.asset == pAsset)
				{
					return true;
				}
			}
		}
		BuildingList? buildingList = null;
		switch (pAsset.building_type)
		{
		case BuildingType.Building_Tree:
			buildingList = BuildingList.Trees;
			break;
		case BuildingType.Building_Plant:
			buildingList = BuildingList.Flora;
			break;
		case BuildingType.Building_Fruits:
			buildingList = BuildingList.Food;
			break;
		case BuildingType.Building_Hives:
			buildingList = BuildingList.Hives;
			break;
		case BuildingType.Building_Poops:
			buildingList = BuildingList.Poops;
			break;
		case BuildingType.Building_Mineral:
			buildingList = BuildingList.Minerals;
			break;
		}
		if (buildingList.HasValue && countBuildingsType(buildingList.Value) >= num)
		{
			return true;
		}
		return false;
	}

	public void clearBuildingLists()
	{
		buildings_render_list.Clear();
		for (int i = 0; i < _building_hashset_types_array.Length; i++)
		{
			HashSet<Building> hashSet = _building_hashset_types_array[i];
			if (hashSet != null)
			{
				hashSet.Clear();
				UnsafeCollectionPool<HashSet<Building>, Building>.Release(hashSet);
				_building_hashset_types_array[i] = null;
			}
		}
	}

	public Dictionary<TileTypeBase, HashSet<WorldTile>> getTileTypes()
	{
		return _tile_types;
	}

	public void clear()
	{
		foreach (HashSet<WorldTile> value in _tile_types.Values)
		{
			value.Clear();
		}
		clearBuildingLists();
		buildings_all.Clear();
		city = null;
		_good_for_new_city = false;
		resetRenderHelpers();
	}

	public void Dispose()
	{
		clear();
		tiles.Clear();
		neighbours.Clear();
		neighbours_all.Clear();
		neighbours = null;
		neighbours_all = null;
		zone_up = null;
		zone_down = null;
		zone_left = null;
		zone_right = null;
		centerTile = null;
		foreach (HashSet<WorldTile> value in _tile_types.Values)
		{
			value.Clear();
			UnsafeCollectionPool<HashSet<WorldTile>, WorldTile>.Release(value);
		}
		_tile_types.Clear();
	}

	public bool canBeClaimedByCity(City pCity)
	{
		if (pCity.hasLeader() && !checkCanSettleInThisBiomes(pCity.leader.subspecies))
		{
			return false;
		}
		if (!hasCity())
		{
			return true;
		}
		if (isSameCityHere(pCity))
		{
			return false;
		}
		if (!WorldLawLibrary.world_law_border_stealing.isEnabled())
		{
			return false;
		}
		if (pCity.hasKingdom() && city.hasKingdom() && !pCity.kingdom.isEnemy(city.kingdom))
		{
			return false;
		}
		if (pCity.kingdom == city.kingdom)
		{
			return false;
		}
		return true;
	}

	public bool isZoneOnFire()
	{
		return WorldBehaviourActionFire.countFires(this) > 0;
	}

	public WorldTile getRandomTile()
	{
		return tiles.GetRandom();
	}

	public int countNotNullTypes()
	{
		int num = 0;
		for (int i = 0; i < _building_hashset_types_array.Length; i++)
		{
			if (_building_hashset_types_array[i] != null)
			{
				num++;
			}
		}
		return num;
	}

	public IMetaObject getClanOnZone(int pZoneOption)
	{
		switch (pZoneOption)
		{
		case 0:
		{
			City city2 = this.city;
			if (city2.isRekt())
			{
				return null;
			}
			return city2.kingdom.getKingClan();
		}
		case 1:
		{
			City city = this.city;
			if (city.isRekt())
			{
				return null;
			}
			return city.getRoyalClan();
		}
		default:
			return getDefaultMetaOnZone();
		}
	}

	public IMetaObject getFamilyOnZone(int pZoneOption)
	{
		switch (pZoneOption)
		{
		case 0:
		{
			City city2 = this.city;
			if (city2.isRekt())
			{
				return null;
			}
			return city2.kingdom.king?.family;
		}
		case 1:
		{
			City city = this.city;
			if (city.isRekt())
			{
				return null;
			}
			return city.leader?.family;
		}
		default:
			return getDefaultMetaOnZone();
		}
	}

	public IMetaObject getArmyOnZone(int pZoneOption)
	{
		return getDefaultMetaOnZone();
	}

	public IMetaObject getCityOnZone(int pZoneOption)
	{
		if (pZoneOption == 0)
		{
			City city = this.city;
			if (city.isRekt())
			{
				return null;
			}
			return city;
		}
		return getDefaultMetaOnZone();
	}

	public IMetaObject getKingdomOnZone(int pZoneOption)
	{
		if (pZoneOption == 0)
		{
			City city = this.city;
			if (city.isRekt())
			{
				return null;
			}
			return city.kingdom;
		}
		return getDefaultMetaOnZone();
	}

	public IMetaObject getLanguageOnZone(int pZoneOption)
	{
		switch (pZoneOption)
		{
		case 0:
		{
			City city2 = this.city;
			if (city2.isRekt())
			{
				return null;
			}
			return city2.kingdom.getLanguage();
		}
		case 1:
		{
			City city = this.city;
			if (city.isRekt())
			{
				return null;
			}
			return city.getLanguage();
		}
		default:
			return getDefaultMetaOnZone();
		}
	}

	public IMetaObject getReligionOnZone(int pZoneOption)
	{
		switch (pZoneOption)
		{
		case 0:
		{
			City city2 = this.city;
			if (city2.isRekt())
			{
				return null;
			}
			return city2.kingdom.getReligion();
		}
		case 1:
		{
			City city = this.city;
			if (city.isRekt())
			{
				return null;
			}
			return city.getReligion();
		}
		default:
			return getDefaultMetaOnZone();
		}
	}

	public IMetaObject getSubspeciesOnZone(int pZoneOption)
	{
		switch (pZoneOption)
		{
		case 0:
		{
			City city2 = this.city;
			if (city2.isRekt())
			{
				return null;
			}
			return city2.kingdom.getMainSubspecies();
		}
		case 1:
		{
			City city = this.city;
			if (city.isRekt())
			{
				return null;
			}
			return city.getMainSubspecies();
		}
		default:
			return getDefaultMetaOnZone();
		}
	}

	public IMetaObject getCultureOnZone(int pZoneOption)
	{
		switch (pZoneOption)
		{
		case 0:
		{
			City city2 = this.city;
			if (city2.isRekt())
			{
				return null;
			}
			return city2.kingdom.getCulture();
		}
		case 1:
		{
			City city = this.city;
			if (city.isRekt())
			{
				return null;
			}
			return city.getCulture();
		}
		default:
			return getDefaultMetaOnZone();
		}
	}

	public Alliance getAllianceOnZone(int pZoneOption)
	{
		return city?.getAlliance();
	}

	private IMetaObject getDefaultMetaOnZone()
	{
		ZoneMetaData zoneMetaData = ZoneMetaDataVisualizer.getZoneMetaData(this);
		if (zoneMetaData.meta_object == null)
		{
			return null;
		}
		if (!zoneMetaData.meta_object.isAlive())
		{
			return null;
		}
		return zoneMetaData.meta_object;
	}
}
