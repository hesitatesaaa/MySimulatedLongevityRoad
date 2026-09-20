using System.Collections.Generic;

public class SimObjectsZones
{
	private float _timer;

	private const float INTERVAL = 0.1f;

	private readonly List<WorldTile> _to_clear_tiles = new List<WorldTile>();

	private readonly HashSet<MapChunk> _dirty_building_chunks = new HashSet<MapChunk>();

	private bool _buildings_dirty;

	public void setBuildingsDirty(MapChunk pChunk)
	{
		_buildings_dirty = true;
		pChunk.setBuildingsDirty();
		_dirty_building_chunks.Add(pChunk);
	}

	internal void update()
	{
		Bench.bench("sim_zones", "game_total");
		if (_timer > 0f)
		{
			_timer -= World.world.delta_time;
		}
		else
		{
			_timer = 0.1f;
			recalc();
		}
		Bench.benchEnd("sim_zones", "game_total", pSaveCounter: false, 0L);
	}

	private void recalc()
	{
		reset();
		Bench.bench("islands.recalcActors", "sim_zones");
		World.world.islands_calculator.recalcActors();
		Bench.benchEnd("islands.recalcActors", "sim_zones", pSaveCounter: false, 0L);
		Bench.bench("checkUnits", "sim_zones");
		checkUnits();
		Bench.benchEnd("checkUnits", "sim_zones", pSaveCounter: false, 0L);
		Bench.bench("checkBuildings", "sim_zones");
		if (_buildings_dirty)
		{
			checkBuildings();
			_buildings_dirty = false;
			foreach (MapChunk dirty_building_chunk in _dirty_building_chunks)
			{
				dirty_building_chunk.finishBuildingsCheck();
			}
			_dirty_building_chunks.Clear();
		}
		Bench.benchEnd("checkBuildings", "sim_zones", pSaveCounter: false, 0L);
	}

	private void checkUnits()
	{
		List<Actor> simpleList = World.world.units.getSimpleList();
		int i = 0;
		for (int count = simpleList.Count; i < count; i++)
		{
			Actor actor = simpleList[i];
			if (actor.isAlive())
			{
				WorldTile current_tile = actor.current_tile;
				addUnit(actor, current_tile);
				current_tile.chunk.objects.addActor(actor);
			}
		}
	}

	private void checkBuildings()
	{
		List<Building> simpleList = World.world.buildings.getSimpleList();
		int i = 0;
		for (int count = simpleList.Count; i < count; i++)
		{
			Building building = simpleList[i];
			if (!building.isUsable())
			{
				continue;
			}
			MapChunk chunk = building.chunk;
			if (chunk.buildings_dirty)
			{
				if (building.isCiv() && building.asset.docks && building.component_docks.hasOceanTiles())
				{
					building.component_docks.tiles_ocean[0].region.island.addDock(building);
				}
				chunk.objects.addBuilding(building);
			}
		}
	}

	private void addUnit(Actor pActor, WorldTile pTile)
	{
		if (!pTile.hasUnits())
		{
			_to_clear_tiles.Add(pTile);
		}
		pTile.addUnit(pActor);
		TileZone zone = pTile.zone;
		City zone_city = pTile.zone_city;
		if (zone_city != null && !pActor.isInsideSomething())
		{
			Kingdom kingdom = pActor.kingdom;
			if (pActor.profession_asset.can_capture)
			{
				zone_city.updateConquest(pActor);
			}
			else if (kingdom.isCiv())
			{
				return;
			}
			if (!zone_city.danger_zones.Contains(zone) && (!kingdom.isMobs() || !WorldLawLibrary.world_law_peaceful_monsters.isEnabled()) && kingdom != zone_city.kingdom && kingdom.asset.count_as_danger && kingdom.isEnemy(zone_city.kingdom))
			{
				zone_city.danger_zones.Add(zone);
			}
		}
	}

	private void clearTileUnits()
	{
		List<WorldTile> to_clear_tiles = _to_clear_tiles;
		int i = 0;
		for (int count = to_clear_tiles.Count; i < count; i++)
		{
			to_clear_tiles[i].clearUnits();
		}
		to_clear_tiles.Clear();
	}

	private void clearChunkObjects(bool pForceClearBuildings)
	{
		MapChunk[] chunks = World.world.map_chunk_manager.chunks;
		int i = 0;
		for (int num = chunks.Length; i < num; i++)
		{
			MapChunk mapChunk = chunks[i];
			if (!mapChunk.objects.isEmpty())
			{
				mapChunk.clearObjects(pForceClearBuildings);
			}
		}
	}

	private void clearIslandsDocks()
	{
		if (_buildings_dirty)
		{
			ListPool<TileIsland> islands = World.world.islands_calculator.islands;
			int i = 0;
			for (int count = islands.Count; i < count; i++)
			{
				TileIsland tileIsland = islands[i];
				tileIsland.docks?.Dispose();
				tileIsland.docks = null;
			}
		}
	}

	private void clearCaptureAndDangerZones()
	{
		foreach (City city in World.world.cities)
		{
			city.clearCurrentCaptureAmounts();
			city.clearDangerZones();
		}
	}

	private void clearAllDisposed()
	{
		foreach (BaseSystemManager list_all_sim_manager in World.world.list_all_sim_managers)
		{
			list_all_sim_manager.ClearAllDisposed();
		}
	}

	private void reset(bool pForceClearBuildings = false)
	{
		if (pForceClearBuildings)
		{
			_buildings_dirty = true;
		}
		Bench.bench("clear_tiles", "sim_zones");
		clearTileUnits();
		Bench.benchEnd("clear_tiles", "sim_zones", pSaveCounter: false, 0L);
		Bench.bench("clear_chunks", "sim_zones");
		clearChunkObjects(pForceClearBuildings);
		Bench.benchEnd("clear_chunks", "sim_zones", pSaveCounter: false, 0L);
		Bench.bench("clear_islands_docks", "sim_zones");
		clearIslandsDocks();
		Bench.benchEnd("clear_islands_docks", "sim_zones", pSaveCounter: false, 0L);
		Bench.bench("clear_capture_and_danger_zones", "sim_zones");
		clearCaptureAndDangerZones();
		Bench.benchEnd("clear_capture_and_danger_zones", "sim_zones", pSaveCounter: false, 0L);
		Bench.bench("clear_all_disposed", "sim_zones");
		clearAllDisposed();
		Bench.benchEnd("clear_all_disposed", "sim_zones", pSaveCounter: false, 0L);
	}

	public void fullClear()
	{
		reset(pForceClearBuildings: true);
	}
}
