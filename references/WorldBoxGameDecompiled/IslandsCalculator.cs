using System.Collections.Generic;

public class IslandsCalculator
{
	private float _timer_update_actors;

	public ListPool<TileIsland> islands = new ListPool<TileIsland>();

	public readonly List<TileIsland> islands_ground = new List<TileIsland>();

	private readonly List<MapRegion> _temp_regions = new List<MapRegion>();

	private readonly List<MapRegion> _temp_regions_cur_wave = new List<MapRegion>();

	private readonly List<MapRegion> _temp_regions_next_wave = new List<MapRegion>();

	private int _last_island_id;

	private readonly HashSet<TileIsland> _dirty_islands = new HashSet<TileIsland>();

	private readonly Queue<MapRegion> _wave = new Queue<MapRegion>();

	private readonly Stack<TileIsland> _island_pool = new Stack<TileIsland>();

	public void prepareCalc()
	{
		_dirty_islands.Clear();
	}

	public void makeDirty(TileIsland pIsland)
	{
		_dirty_islands.Add(pIsland);
	}

	public void clearDirty()
	{
		using ListPool<TileIsland> listPool = islands;
		islands = new ListPool<TileIsland>(listPool.Count);
		foreach (TileIsland dirty_island in _dirty_islands)
		{
			dirty_island.clearRegionsFromIsland();
			dirty_island.insideRegionEdges.Clear();
			foreach (TileIsland connectedIsland in dirty_island.getConnectedIslands())
			{
				connectedIsland.setDirtyIslandNeighbours();
				MapChunkManager.m_dirtyIslands++;
			}
		}
		for (int i = 0; i < listPool.Count; i++)
		{
			TileIsland tileIsland = listPool[i];
			if (tileIsland.removed)
			{
				tileIsland.reset();
				_island_pool.Push(tileIsland);
			}
			else
			{
				islands.Add(tileIsland);
			}
		}
	}

	public void clear()
	{
		_last_island_id = 0;
		ListPool<TileIsland> listPool = islands;
		for (int i = 0; i < listPool.Count; i++)
		{
			TileIsland tileIsland = listPool[i];
			tileIsland.reset();
			_island_pool.Push(tileIsland);
		}
		_dirty_islands.Clear();
		islands.Clear();
		islands_ground.Clear();
		_wave.Clear();
		_temp_regions.Clear();
		_temp_regions_cur_wave.Clear();
		_temp_regions_next_wave.Clear();
	}

	public WorldTile tryGetRandomGround()
	{
		WorldTile worldTile = null;
		if (islands.Count > 0)
		{
			TileIsland randomIslandGround = getRandomIslandGround();
			if (randomIslandGround != null && randomIslandGround.regions.Count > 0)
			{
				worldTile = randomIslandGround.getRandomTile();
			}
		}
		if (worldTile == null)
		{
			worldTile = World.world.tiles_list.GetRandom();
		}
		return worldTile;
	}

	internal bool hasGround()
	{
		return islands_ground.Count > 0;
	}

	internal bool hasNonGround()
	{
		return islands.Count > islands_ground.Count;
	}

	internal float groundIslandRatio()
	{
		if (islands.Count == 0)
		{
			return 0f;
		}
		return (float)islands_ground.Count / (float)islands.Count;
	}

	internal float realGroundRatio()
	{
		if (!hasNonGround())
		{
			return 1f;
		}
		if (!hasGround())
		{
			return 0f;
		}
		int num = 0;
		int num2 = 0;
		foreach (ref TileIsland island in islands)
		{
			TileIsland current = island;
			if (current.type == TileLayerType.Ground)
			{
				num += current.getTileCount();
			}
			else
			{
				num2 += current.getTileCount();
			}
		}
		if (num == 0)
		{
			return 0f;
		}
		if (num2 == 0)
		{
			return 1f;
		}
		return (float)num / (float)(num + num2);
	}

	internal TileIsland getRandomIslandGroundWeighted(bool pMinRegions = true)
	{
		if (islands_ground.Count == 0)
		{
			return null;
		}
		int num = 0;
		for (int i = 0; i < islands_ground.Count; i++)
		{
			TileIsland tileIsland = islands_ground[i];
			if (!pMinRegions || tileIsland.regions.Count >= 4)
			{
				num += tileIsland.regions.Count;
			}
		}
		if (num == 0)
		{
			return null;
		}
		using ListPool<TileIsland> listPool = new ListPool<TileIsland>(num);
		for (int j = 0; j < islands_ground.Count; j++)
		{
			TileIsland tileIsland2 = islands_ground[j];
			if (!pMinRegions || tileIsland2.regions.Count >= 4)
			{
				listPool.AddTimes(tileIsland2.regions.Count, tileIsland2);
			}
		}
		return listPool.GetRandom();
	}

	internal TileIsland getRandomIslandGround(bool pMinRegions = true)
	{
		if (islands_ground.Count == 0)
		{
			return null;
		}
		if (!pMinRegions)
		{
			return islands_ground.GetRandom();
		}
		foreach (TileIsland item in islands_ground.LoopRandom())
		{
			if (item.regions.Count >= 4)
			{
				return item;
			}
		}
		return null;
	}

	internal TileIsland getRandomIslandNonGroundWeighted(bool pMinRegions = true)
	{
		if (islands.Count == 0)
		{
			return null;
		}
		if (islands_ground.Count == islands.Count)
		{
			return null;
		}
		int num = 0;
		for (int i = 0; i < islands.Count; i++)
		{
			TileIsland tileIsland = islands[i];
			if (tileIsland.type != TileLayerType.Ground && (!pMinRegions || tileIsland.regions.Count >= 4))
			{
				num += tileIsland.regions.Count;
			}
		}
		if (num == 0)
		{
			return null;
		}
		using ListPool<TileIsland> listPool = new ListPool<TileIsland>(num);
		for (int j = 0; j < islands.Count; j++)
		{
			TileIsland tileIsland2 = islands[j];
			if (tileIsland2.type != TileLayerType.Ground && (!pMinRegions || tileIsland2.regions.Count >= 4))
			{
				listPool.AddTimes(tileIsland2.regions.Count, tileIsland2);
			}
		}
		return listPool.GetRandom();
	}

	internal TileIsland getRandomIslandNonGround(bool pMinRegions = true)
	{
		if (islands.Count == 0)
		{
			return null;
		}
		if (islands_ground.Count == islands.Count)
		{
			return null;
		}
		foreach (TileIsland item in islands.LoopRandom())
		{
			if (item.type != TileLayerType.Ground && (!pMinRegions || item.regions.Count >= 4))
			{
				return item;
			}
		}
		return null;
	}

	public int countLandIslands()
	{
		int num = 0;
		for (int i = 0; i < islands.Count; i++)
		{
			TileIsland tileIsland = islands[i];
			if (tileIsland.type == TileLayerType.Ground && tileIsland.regions.Count >= 4)
			{
				num++;
			}
		}
		return num;
	}

	internal void recalcActors()
	{
		ListPool<TileIsland> listPool = islands;
		for (int i = 0; i < listPool.Count; i++)
		{
			listPool[i].actors.Clear();
		}
		List<Actor> simpleList = World.world.units.getSimpleList();
		for (int j = 0; j < simpleList.Count; j++)
		{
			Actor actor = simpleList[j];
			if (actor.isAlive())
			{
				actor.current_tile.region.island.actors.Add(actor);
			}
		}
	}

	private void clearCaches()
	{
		for (int i = 0; i < islands.Count; i++)
		{
			islands[i].clearCache();
		}
	}

	public void findIslands(ListPool<TileIsland> pNewIslands)
	{
		Bench.bench("find_islands_prepare", "chunks");
		_temp_regions.Clear();
		islands_ground.Clear();
		clearCaches();
		Bench.benchEnd("find_islands_prepare", "chunks", pSaveCounter: false, 0L);
		Bench.bench("find_islands_temp_regions", "chunks");
		MapChunk[] chunks = World.world.map_chunk_manager.chunks;
		int num = chunks.Length;
		for (int i = 0; i < num; i++)
		{
			MapChunk mapChunk = chunks[i];
			for (int j = 0; j < mapChunk.regions.Count; j++)
			{
				MapRegion mapRegion = mapChunk.regions[j];
				if (mapRegion.island == null)
				{
					mapRegion.is_island_checked = false;
					_temp_regions.Add(mapRegion);
				}
			}
		}
		Bench.benchEnd("find_islands_temp_regions", "chunks", pSaveCounter: true, _temp_regions.Count);
		Bench.bench("find_islands_new_islands", "chunks");
		for (int k = 0; k < _temp_regions.Count; k++)
		{
			MapRegion mapRegion2 = _temp_regions[k];
			if (mapRegion2.island == null)
			{
				TileIsland item = newIslandFrom(mapRegion2);
				pNewIslands.Add(item);
				MapChunkManager.m_newIslands++;
			}
		}
		Bench.benchEnd("find_islands_new_islands", "chunks", pSaveCounter: true, MapChunkManager.m_newIslands);
		Bench.bench("find_islands_fin", "chunks");
		for (int l = 0; l < islands.Count; l++)
		{
			TileIsland tileIsland = islands[l];
			tileIsland.countTiles();
			if (tileIsland.type == TileLayerType.Ground)
			{
				islands_ground.Add(tileIsland);
			}
		}
		Bench.benchEnd("find_islands_fin", "chunks", pSaveCounter: false, 0L);
	}

	private TileIsland newIslandFrom(MapRegion pRegion)
	{
		_temp_regions_cur_wave.Clear();
		_temp_regions_next_wave.Clear();
		TileIsland tileIsland;
		if (_island_pool.Count > 0)
		{
			tileIsland = _island_pool.Pop();
		}
		else
		{
			tileIsland = new TileIsland(_last_island_id);
			_last_island_id++;
		}
		tileIsland.reset();
		tileIsland.type = pRegion.type;
		islands.Add(tileIsland);
		_temp_regions_next_wave.Add(pRegion);
		_wave.Clear();
		_wave.Enqueue(pRegion);
		startFill(tileIsland);
		return tileIsland;
	}

	private void startFill(TileIsland pIsland)
	{
		while (_wave.Count > 0)
		{
			MapRegion mapRegion = _wave.Dequeue();
			if (!mapRegion.is_island_checked)
			{
				mapRegion.island = pIsland;
				pIsland.addRegion(mapRegion);
			}
			mapRegion.is_island_checked = true;
			for (int i = 0; i < mapRegion.neighbours.Count; i++)
			{
				MapRegion mapRegion2 = mapRegion.neighbours[i];
				if (!mapRegion2.is_island_checked)
				{
					mapRegion2.is_island_checked = true;
					mapRegion2.island = pIsland;
					pIsland.addRegion(mapRegion2);
					_wave.Enqueue(mapRegion2);
				}
			}
		}
		pIsland.regions.checkAddRemove();
	}
}
