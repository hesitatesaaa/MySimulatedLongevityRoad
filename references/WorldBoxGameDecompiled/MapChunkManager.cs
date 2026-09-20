using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using UnityEngine;

public class MapChunkManager
{
	private readonly Color _color_1_gray;

	private readonly Color _color_2_white;

	private MapChunk[,] _map;

	public MapChunk[] chunks;

	private readonly List<MapChunk> _dirty_chunks_regions;

	private readonly List<MapChunk> _dirty_chunks_links;

	private int m_dirtyChunks;

	private static int m_dirtyCorners;

	private static int m_newRegions;

	public static int m_newLinks;

	internal static int m_newIslands;

	internal static int m_dirtyIslands;

	private float _timer;

	private int _get_amount_x;

	private int _amount_y;

	private readonly List<MapChunk> _last_dirty_chunks;

	private readonly List<MapChunk> _last_dirty_links;

	private readonly HashSet<MapRegion> _region_set;

	private readonly List<TileIsland> _temp_dirty_neighbours_islands;

	public int amount_x => _get_amount_x;

	private bool _is_parallel_enabled => DebugConfig.isOn(DebugOption.ParallelChunks);

	private bool _batches_enabled => DebugConfig.isOn(DebugOption.ChunkBatches);

	public void checkDiagnosticRegions()
	{
		diagnosticRegions();
	}

	public void update(float pElapsed, bool pForce = false)
	{
		if (_timer > 0f && !pForce)
		{
			_timer -= pElapsed;
		}
		else
		{
			updateDirty();
		}
	}

	private void diagnosticRegions()
	{
		HashSet<MapRegion> hashSet = new HashSet<MapRegion>();
		HashSet<MapRegion> hashSet2 = new HashSet<MapRegion>();
		MapChunk[] array = chunks;
		int num = array.Length;
		for (int i = 0; i < num; i++)
		{
			foreach (MapRegion region in array[i].regions)
			{
				if (region.tiles.Count == 0 || region.chunk == null)
				{
					hashSet.Add(region);
				}
				foreach (MapRegion neighbour in region.neighbours)
				{
					if (neighbour.tiles.Count == 0 || neighbour.chunk == null)
					{
						hashSet2.Add(region);
					}
				}
			}
		}
		if (hashSet.Count > 0 || hashSet2.Count > 0)
		{
			Debug.LogError((object)"Something is wrong with regions");
			Debug.LogError((object)("tRegionMain: " + hashSet.Count));
			Debug.LogError((object)("tRegionNeighbour: " + hashSet2.Count));
		}
	}

	public void prepare()
	{
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		int num = 4;
		_get_amount_x = Config.ZONE_AMOUNT_X * num;
		_amount_y = Config.ZONE_AMOUNT_Y * num;
		_map = new MapChunk[_get_amount_x, _amount_y];
		int num2 = _get_amount_x * _amount_y;
		if (num2 != chunks.Length)
		{
			chunks = new MapChunk[num2];
		}
		else
		{
			Array.Clear(chunks, 0, chunks.Length);
		}
		int num3 = 0;
		int num4 = 0;
		for (int i = 0; i < _amount_y; i++)
		{
			for (int j = 0; j < _get_amount_x; j++)
			{
				MapChunk mapChunk = new MapChunk();
				mapChunk.id = num3++;
				mapChunk.x = j;
				mapChunk.y = i;
				_map[j, i] = mapChunk;
				if ((j + i) % 2 == 0)
				{
					mapChunk.color = _color_1_gray;
				}
				else
				{
					mapChunk.color = _color_2_white;
				}
				chunks[num4] = mapChunk;
				fill(mapChunk);
				num4++;
			}
		}
		fillAndLinkTileZones();
		generateNeighbours();
		generateEdgeConnections();
	}

	private void generateEdgeConnections()
	{
		MapChunk[] array = chunks;
		int num = array.Length;
		for (int i = 0; i < num; i++)
		{
			array[i].generateEdgeConnections();
		}
	}

	private void fillAndLinkTileZones()
	{
		for (int i = 0; i < World.world.zone_calculator.zones.Count; i++)
		{
			TileZone tileZone = World.world.zone_calculator.zones[i];
			tileZone.chunk.zones.Add(tileZone);
		}
	}

	private void fill(MapChunk pChunk)
	{
		int num = 16;
		int num2 = pChunk.x * num;
		int num3 = pChunk.y * num;
		for (int i = 0; i < num; i++)
		{
			for (int j = 0; j < num; j++)
			{
				WorldTile tileSimple = World.world.GetTileSimple(i + num2, j + num3);
				tileSimple.chunk = pChunk;
				pChunk.addTile(tileSimple, i, j);
			}
		}
		pChunk.world_center_x = num2 + 8;
		pChunk.world_center_y = num3 + 8;
		for (int k = 0; k < 16; k++)
		{
			WorldTile tileSimple = World.world.GetTileSimple(k + num2, num3);
			pChunk.bounds_down.Add(tileSimple);
		}
		for (int l = 0; l < 16; l++)
		{
			WorldTile tileSimple = World.world.GetTileSimple(num2, num3 + l);
			pChunk.bounds_left.Add(tileSimple);
		}
		for (int m = 0; m < 16; m++)
		{
			WorldTile tileSimple = World.world.GetTileSimple(num2 + 16 - 1, num3 + m);
			pChunk.bounds_right.Add(tileSimple);
		}
		for (int n = 0; n < 16; n++)
		{
			WorldTile tileSimple = World.world.GetTileSimple(n + num2, num3 + 16 - 1);
			pChunk.bounds_up.Add(tileSimple);
		}
		pChunk.edge_up_left = pChunk.bounds_up[0];
		pChunk.edge_up_right = pChunk.bounds_up[15];
		pChunk.edge_down_left = pChunk.bounds_down[0];
		pChunk.edge_down_right = pChunk.bounds_down[15];
		pChunk.combineEdges();
	}

	private void generateNeighbours()
	{
		MapChunk[] array = chunks;
		int num = array.Length;
		using ListPool<MapChunk> listPool = new ListPool<MapChunk>(4);
		using ListPool<MapChunk> listPool2 = new ListPool<MapChunk>(8);
		for (int i = 0; i < num; i++)
		{
			MapChunk mapChunk = array[i];
			MapChunk pNeighbour = get(mapChunk.x - 1, mapChunk.y);
			mapChunk.addNeighbour(pNeighbour, TileDirection.Left, listPool, listPool2);
			pNeighbour = get(mapChunk.x + 1, mapChunk.y);
			mapChunk.addNeighbour(pNeighbour, TileDirection.Right, listPool, listPool2);
			pNeighbour = get(mapChunk.x, mapChunk.y - 1);
			mapChunk.addNeighbour(pNeighbour, TileDirection.Down, listPool, listPool2);
			pNeighbour = get(mapChunk.x, mapChunk.y + 1);
			mapChunk.addNeighbour(pNeighbour, TileDirection.Up, listPool, listPool2);
			pNeighbour = get(mapChunk.x - 1, mapChunk.y - 1);
			mapChunk.addNeighbour(pNeighbour, TileDirection.Null, listPool, listPool2, pDiagonal: true);
			pNeighbour = get(mapChunk.x - 1, mapChunk.y + 1);
			mapChunk.addNeighbour(pNeighbour, TileDirection.Null, listPool, listPool2, pDiagonal: true);
			pNeighbour = get(mapChunk.x + 1, mapChunk.y - 1);
			mapChunk.addNeighbour(pNeighbour, TileDirection.Null, listPool, listPool2, pDiagonal: true);
			pNeighbour = get(mapChunk.x + 1, mapChunk.y + 1);
			mapChunk.addNeighbour(pNeighbour, TileDirection.Null, listPool, listPool2, pDiagonal: true);
			mapChunk.neighbours = listPool.ToArray();
			mapChunk.neighbours_all = listPool2.ToArray();
			listPool.Clear();
			listPool2.Clear();
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public MapChunk get(int pX, int pY)
	{
		if (pX < 0 || pX >= _get_amount_x || pY < 0 || pY >= _amount_y)
		{
			return null;
		}
		return _map[pX, pY];
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public MapChunk get(ref Vector2Int pPos)
	{
		if (((Vector2Int)(ref pPos)).x < 0 || ((Vector2Int)(ref pPos)).x >= _get_amount_x || ((Vector2Int)(ref pPos)).y < 0 || ((Vector2Int)(ref pPos)).y >= _amount_y)
		{
			return null;
		}
		return _map[((Vector2Int)(ref pPos)).x, ((Vector2Int)(ref pPos)).y];
	}

	public MapChunk getByID(int pID)
	{
		return chunks[pID];
	}

	public void clearAll()
	{
		World.world.islands_calculator.clear();
		_dirty_chunks_links.Clear();
		_dirty_chunks_regions.Clear();
		int num = chunks.Length;
		for (int i = 0; i < num; i++)
		{
			chunks[i].clearRegions();
		}
	}

	public void clean()
	{
		clearAll();
		MapChunk[] array = chunks;
		int num = array.Length;
		for (int i = 0; i < num; i++)
		{
			array[i].Dispose();
		}
		Array.Clear(chunks, 0, chunks.Length);
	}

	public void setAllLinksDirty()
	{
		MapChunk[] array = chunks;
		foreach (MapChunk pChunk in array)
		{
			setDirty(pChunk, pRegions: false);
		}
	}

	public void setDirty(MapChunk pChunk, bool pRegions = true, bool pLinks = true)
	{
		if (pRegions && !pChunk.dirty_regions)
		{
			pChunk.dirty_regions = true;
			_dirty_chunks_regions.Add(pChunk);
		}
		if (pLinks && !pChunk.dirty_links)
		{
			pChunk.dirty_links = true;
			_dirty_chunks_links.Add(pChunk);
		}
	}

	public void allDirty()
	{
		_dirty_chunks_links.Clear();
		_dirty_chunks_regions.Clear();
		MapChunk[] array = chunks;
		int num = array.Length;
		for (int i = 0; i < num; i++)
		{
			MapChunk obj = array[i];
			obj.dirty_links = true;
			obj.dirty_regions = true;
		}
		_dirty_chunks_links.AddRange(chunks);
		_dirty_chunks_regions.AddRange(chunks);
	}

	private bool isAllChunksDirty()
	{
		return chunks.Length == _dirty_chunks_regions.Count;
	}

	private void updateDirty()
	{
		if (!DebugConfig.isOn(DebugOption.SystemUpdateDirtyChunks) || (!isAllChunksDirty() && World.world.isActionHappening()) || (_dirty_chunks_links.Count == 0 && _dirty_chunks_regions.Count == 0))
		{
			return;
		}
		Bench.bench("chunks", "chunks_total");
		_timer = 0.4f;
		m_dirtyChunks = _dirty_chunks_regions.Count;
		m_newRegions = 0;
		m_newLinks = 0;
		m_newIslands = 0;
		m_dirtyIslands = 0;
		m_dirtyCorners = 0;
		MapRegion.created_time_last = Time.time;
		World.world.islands_calculator.prepareCalc();
		Bench.bench("clear_regions", "chunks");
		calc_clearRegions();
		Bench.benchEnd("clear_regions", "chunks", pSaveCounter: true, _dirty_chunks_links.Count);
		Bench.bench("clear_dirty_islands", "chunks");
		World.world.islands_calculator.clearDirty();
		Bench.benchEnd("clear_dirty_islands", "chunks", pSaveCounter: true, _dirty_chunks_links.Count);
		Bench.bench("P calc_regions", "chunks");
		calc_regions();
		Bench.benchEnd("P calc_regions", "chunks", pSaveCounter: true, _dirty_chunks_regions.Count);
		Bench.bench("shuffle_region_tiles", "chunks");
		calc_shuffleRegionTiles();
		Bench.benchEnd("shuffle_region_tiles", "chunks", pSaveCounter: true, m_newRegions);
		Bench.bench("P calc_links", "chunks");
		calc_links();
		Bench.benchEnd("P calc_links", "chunks", pSaveCounter: true, _dirty_chunks_links.Count);
		Bench.bench("create_links", "chunks");
		calc_checkLinkResults();
		Bench.benchEnd("create_links", "chunks", pSaveCounter: true, _dirty_chunks_links.Count);
		Bench.bench("P calc_linked_regions", "chunks");
		calc_linkedRegions();
		Bench.benchEnd("P calc_linked_regions", "chunks", pSaveCounter: true, _dirty_chunks_links.Count);
		using ListPool<TileIsland> pNewIslands = new ListPool<TileIsland>();
		World.world.islands_calculator.findIslands(pNewIslands);
		Bench.bench("tile_corners_prepare", "chunks");
		calc_tileCornersPrepare(_region_set);
		m_dirtyCorners = _region_set.Count;
		Bench.benchEnd("tile_corners_prepare", "chunks", pSaveCounter: true, _dirty_chunks_links.Count);
		Bench.bench("center_regions", "chunks");
		calc_centerRegions();
		Bench.benchEnd("center_regions", "chunks", pSaveCounter: true, _region_set.Count);
		Bench.bench("island_region_edges", "chunks");
		int num = calc_islandRegionEdges(pNewIslands);
		Bench.benchEnd("island_region_edges", "chunks", pSaveCounter: true, num);
		Bench.bench("PH tile_edges", "chunks");
		calc_tileEdges();
		Bench.benchEnd("PH tile_edges", "chunks", pSaveCounter: true, _region_set.Count);
		Bench.bench("prepare_d_neighbour_islands", "chunks");
		prepareDirtyNeighbourIslands();
		Bench.benchEnd("prepare_d_neighbour_islands", "chunks", pSaveCounter: true, _temp_dirty_neighbours_islands.Count);
		Bench.bench("P neighbour_islands", "chunks");
		calc_neighbourIslands();
		Bench.benchEnd("P neighbour_islands", "chunks", pSaveCounter: true, _temp_dirty_neighbours_islands.Count);
		Bench.bench("clear_end", "chunks");
		_region_set.Clear();
		_dirty_chunks_links.Clear();
		_dirty_chunks_regions.Clear();
		World.world.city_zone_helper.city_place_finder.setDirty();
		World.world.region_path_finder.clearCache();
		Bench.benchEnd("clear_end", "chunks", pSaveCounter: false, 0L);
		Bench.benchSetValue("m_dirtyChunks", m_dirtyChunks, "chunks");
		Bench.benchSetValue("m_newRegions", m_newRegions, "chunks");
		Bench.benchSetValue("m_newLinks", m_newLinks, "chunks");
		Bench.benchSetValue("m_newIslands", m_newIslands, "chunks");
		Bench.benchSetValue("m_dirtyIslands", m_dirtyIslands, "chunks");
		Bench.benchSetValue("m_dirtyCorners", m_dirtyCorners, "chunks");
		Bench.benchSetValue("m_dirtyIslandNeighb", _temp_dirty_neighbours_islands.Count, "chunks");
		Bench.benchEnd("chunks", "chunks_total", pSaveCounter: false, 0L);
	}

	private void calc_clearRegions()
	{
		List<MapChunk> dirty_chunks_regions = _dirty_chunks_regions;
		int count = dirty_chunks_regions.Count;
		for (int i = 0; i < count; i++)
		{
			dirty_chunks_regions[i].clearRegions();
		}
		List<MapChunk> dirty_chunks_links = _dirty_chunks_links;
		int count2 = dirty_chunks_links.Count;
		for (int j = 0; j < count2; j++)
		{
			dirty_chunks_links[j].clearIsland();
		}
	}

	private void calc_regions()
	{
		if (_is_parallel_enabled)
		{
			List<MapChunk> tDirtyChunks = _dirty_chunks_regions;
			int tCount = tDirtyChunks.Count;
			if (!_batches_enabled)
			{
				Parallel.For(0, tCount, World.world.parallel_options, delegate(int pIndex)
				{
					tDirtyChunks[pIndex].calculateRegions();
				});
				return;
			}
			int tDynamicBatchSize = ParallelHelper.getDynamicBatchSize(tCount);
			int toExclusive = ParallelHelper.calcTotalBatches(tCount, tDynamicBatchSize);
			Parallel.For(0, toExclusive, World.world.parallel_options, delegate(int pBatchIndex)
			{
				int num2 = ParallelHelper.calculateBatchBeg(pBatchIndex, tDynamicBatchSize);
				int num3 = ParallelHelper.calculateBatchEnd(num2, tDynamicBatchSize, tCount);
				for (int i = num2; i < num3; i++)
				{
					tDirtyChunks[i].calculateRegions();
				}
			});
		}
		else
		{
			List<MapChunk> dirty_chunks_regions = _dirty_chunks_regions;
			int count = dirty_chunks_regions.Count;
			for (int num = 0; num < count; num++)
			{
				dirty_chunks_regions[num].calculateRegions();
			}
		}
	}

	private void calc_shuffleRegionTiles()
	{
		List<MapChunk> dirty_chunks_regions = _dirty_chunks_regions;
		int count = dirty_chunks_regions.Count;
		for (int i = 0; i < count; i++)
		{
			MapChunk mapChunk = dirty_chunks_regions[i];
			m_newRegions += mapChunk.regions.Count;
			mapChunk.shuffleRegionTiles();
		}
	}

	private void calc_links()
	{
		if (_is_parallel_enabled)
		{
			List<MapChunk> tDirtyChunks = _dirty_chunks_links;
			int tCount = tDirtyChunks.Count;
			if (!_batches_enabled)
			{
				Parallel.For(0, tCount, World.world.parallel_options, delegate(int pIndex)
				{
					tDirtyChunks[pIndex].calculateLinks();
				});
				return;
			}
			int tDynamicBatchSize = ParallelHelper.getDynamicBatchSize(tCount);
			int toExclusive = ParallelHelper.calcTotalBatches(tCount, tDynamicBatchSize);
			Parallel.For(0, toExclusive, World.world.parallel_options, delegate(int pBatchIndex)
			{
				int num2 = ParallelHelper.calculateBatchBeg(pBatchIndex, tDynamicBatchSize);
				int num3 = ParallelHelper.calculateBatchEnd(num2, tDynamicBatchSize, tCount);
				for (int i = num2; i < num3; i++)
				{
					tDirtyChunks[i].calculateLinks();
				}
			});
		}
		else
		{
			List<MapChunk> dirty_chunks_links = _dirty_chunks_links;
			int count = dirty_chunks_links.Count;
			for (int num = 0; num < count; num++)
			{
				dirty_chunks_links[num].calculateLinks();
			}
		}
	}

	private void calc_checkLinkResults()
	{
		List<MapChunk> dirty_chunks_links = _dirty_chunks_links;
		int count = dirty_chunks_links.Count;
		for (int i = 0; i < count; i++)
		{
			dirty_chunks_links[i].checkLinksResults();
		}
	}

	private void calc_linkedRegions()
	{
		if (_is_parallel_enabled)
		{
			List<MapChunk> tDirtyChunks = _dirty_chunks_links;
			int tCount = tDirtyChunks.Count;
			if (!_batches_enabled)
			{
				Parallel.For(0, tCount, World.world.parallel_options, delegate(int pIndex)
				{
					MapChunk pChunk2 = tDirtyChunks[pIndex];
					calculateRegionNeighbours(pChunk2);
				});
				return;
			}
			int tDynamicBatchSize = ParallelHelper.getDynamicBatchSize(tCount);
			int toExclusive = ParallelHelper.calcTotalBatches(tCount, tDynamicBatchSize);
			Parallel.For(0, toExclusive, World.world.parallel_options, delegate(int pBatchIndex)
			{
				int num2 = ParallelHelper.calculateBatchBeg(pBatchIndex, tDynamicBatchSize);
				int num3 = ParallelHelper.calculateBatchEnd(num2, tDynamicBatchSize, tCount);
				for (int i = num2; i < num3; i++)
				{
					MapChunk pChunk2 = tDirtyChunks[i];
					calculateRegionNeighbours(pChunk2);
				}
			});
		}
		else
		{
			List<MapChunk> dirty_chunks_links = _dirty_chunks_links;
			int count = dirty_chunks_links.Count;
			for (int num = 0; num < count; num++)
			{
				MapChunk pChunk = dirty_chunks_links[num];
				calculateRegionNeighbours(pChunk);
			}
		}
	}

	private void calc_tileCornersPrepare(HashSet<MapRegion> pSetRegionsResult)
	{
		List<MapChunk> dirty_chunks_links = _dirty_chunks_links;
		int count = dirty_chunks_links.Count;
		for (int i = 0; i < count; i++)
		{
			List<MapRegion> regions = dirty_chunks_links[i].regions;
			int count2 = regions.Count;
			for (int j = 0; j < count2; j++)
			{
				MapRegion item = regions[j];
				pSetRegionsResult.Add(item);
			}
		}
	}

	private void calc_centerRegions()
	{
		foreach (MapRegion item in _region_set)
		{
			item.calculateCenterRegion();
		}
	}

	private int calc_islandRegionEdges(ListPool<TileIsland> pNewIslands)
	{
		int num = 0;
		for (int i = 0; i < pNewIslands.Count; i++)
		{
			TileIsland tileIsland = pNewIslands[i];
			List<MapRegion> simpleList = tileIsland.regions.getSimpleList();
			for (int j = 0; j < simpleList.Count; j++)
			{
				MapRegion mapRegion = simpleList[j];
				num++;
				if (!mapRegion.center_region)
				{
					tileIsland.insideRegionEdges.Add(mapRegion);
				}
			}
		}
		return num;
	}

	private void calc_tileEdges()
	{
		HashSet<MapRegion> region_set = _region_set;
		if (_is_parallel_enabled)
		{
			Parallel.ForEach(region_set, World.world.parallel_options, delegate(MapRegion tReg)
			{
				tReg.calculateTileEdges();
			});
			return;
		}
		foreach (MapRegion item in region_set)
		{
			item.calculateTileEdges();
		}
	}

	private void prepareDirtyNeighbourIslands()
	{
		ListPool<TileIsland> islands = World.world.islands_calculator.islands;
		List<TileIsland> temp_dirty_neighbours_islands = _temp_dirty_neighbours_islands;
		temp_dirty_neighbours_islands.Clear();
		foreach (ref TileIsland item in islands)
		{
			TileIsland current = item;
			if (current.isDirtyNeighbours())
			{
				temp_dirty_neighbours_islands.Add(current);
			}
		}
	}

	private void calc_neighbourIslands()
	{
		List<TileIsland> tIslandsWithDirtyNeighbours = _temp_dirty_neighbours_islands;
		if (_is_parallel_enabled)
		{
			int tCount = tIslandsWithDirtyNeighbours.Count;
			if (!_batches_enabled)
			{
				Parallel.For(0, tCount, World.world.parallel_options, delegate(int pIndex)
				{
					tIslandsWithDirtyNeighbours[pIndex].calcNeighbourIslands();
				});
				return;
			}
			int tDynamicBatchSize = ParallelHelper.DEBUG_BATCH_SIZE;
			int toExclusive = ParallelHelper.calcTotalBatches(tCount, tDynamicBatchSize);
			Parallel.For(0, toExclusive, World.world.parallel_options, delegate(int pBatchIndex)
			{
				int num2 = ParallelHelper.calculateBatchBeg(pBatchIndex, tDynamicBatchSize);
				int num3 = ParallelHelper.calculateBatchEnd(num2, tDynamicBatchSize, tCount);
				for (int i = num2; i < num3; i++)
				{
					tIslandsWithDirtyNeighbours[i].calcNeighbourIslands();
				}
			});
		}
		else
		{
			for (int num = 0; num < tIslandsWithDirtyNeighbours.Count; num++)
			{
				tIslandsWithDirtyNeighbours[num].calcNeighbourIslands();
			}
		}
	}

	private void checkWrongIslands()
	{
		MapChunk[] array = chunks;
		int num = array.Length;
		for (int i = 0; i < num; i++)
		{
			MapChunk mapChunk = array[i];
			foreach (MapRegion region in mapChunk.regions)
			{
				foreach (WorldTile tile in region.tiles)
				{
					if (tile.Type.layer_type != region.island.type)
					{
						bool flag = _last_dirty_chunks.Contains(mapChunk);
						bool flag2 = _last_dirty_links.Contains(mapChunk);
						Debug.LogError((object)("Wrong island type: " + tile.Type.layer_type.ToString() + " != " + region.island.type.ToString() + " " + tile.chunk.id + " - was dirty: " + flag + " | " + flag2));
						break;
					}
				}
			}
		}
	}

	private void calculateRegionNeighbours(MapChunk pChunk)
	{
		for (int i = 0; i < pChunk.regions.Count; i++)
		{
			pChunk.regions[i].calculateNeighbours();
		}
	}

	public int countRegions()
	{
		int num = 0;
		MapChunk[] array = chunks;
		int num2 = array.Length;
		for (int i = 0; i < num2; i++)
		{
			MapChunk mapChunk = array[i];
			num += mapChunk.regions.Count;
		}
		return num;
	}

	public MapChunkManager()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		_color_1_gray = Color.gray;
		_color_2_white = Color.white;
		chunks = new MapChunk[0];
		_dirty_chunks_regions = new List<MapChunk>();
		_dirty_chunks_links = new List<MapChunk>();
		_timer = 0.4f;
		_last_dirty_chunks = new List<MapChunk>();
		_last_dirty_links = new List<MapChunk>();
		_region_set = new HashSet<MapRegion>();
		_temp_dirty_neighbours_islands = new List<TileIsland>();
		base._002Ector();
	}
}
