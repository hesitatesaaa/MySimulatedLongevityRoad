using System;
using System.Collections.Generic;
using UnityEngine;

public class MapChunk : IDisposable
{
	private readonly Queue<WorldTile> _wave = new Queue<WorldTile>(256);

	public readonly ChunkObjectContainer objects = new ChunkObjectContainer();

	public MapChunk[] neighbours;

	public MapChunk[] neighbours_all;

	public readonly WorldTile[] tiles = new WorldTile[256];

	public readonly List<MapRegion> regions = new List<MapRegion>(4);

	public List<WorldTile> edges_all;

	public List<WorldTile> chunk_bounds;

	public readonly List<WorldTile> bounds_left = new List<WorldTile>(16);

	public readonly List<WorldTile> bounds_up = new List<WorldTile>(16);

	public readonly List<WorldTile> bounds_down = new List<WorldTile>(16);

	public readonly List<WorldTile> bounds_right = new List<WorldTile>(16);

	public float world_center_x;

	public float world_center_y;

	public WorldTile edge_up_left;

	public WorldTile edge_up_right;

	public WorldTile edge_down_left;

	public WorldTile edge_down_right;

	private WorldTile _edge_up_left_connection;

	private WorldTile _edge_up_right_connection;

	private WorldTile _edge_down_left_connection;

	private WorldTile _edge_down_right_connection;

	public bool world_edge;

	public int x;

	public int y;

	public int id;

	public Color color;

	public bool dirty_regions;

	public bool dirty_links;

	private readonly List<WorldTile> _temp_tiles = new List<WorldTile>(16);

	private readonly List<TempLinkStruct> _new_hashes = new List<TempLinkStruct>();

	internal MapChunk chunk_up;

	internal MapChunk chunk_down;

	internal MapChunk chunk_left;

	internal MapChunk chunk_right;

	private readonly StackPool<MapRegion> _region_pool = new StackPool<MapRegion>();

	public readonly List<TileZone> zones = new List<TileZone>();

	private bool _buildings_dirty;

	private bool _tile_types_dirty;

	private const int MAX_TILE_TYPES = 256;

	private readonly Dictionary<TileTypeBase, int> _tile_types_count = new Dictionary<TileTypeBase, int>(256);

	private readonly List<MusicBoxTileData> _simple_data = new List<MusicBoxTileData>();

	public bool buildings_dirty => _buildings_dirty;

	internal void addTile(WorldTile pTile, int pX, int pY)
	{
		tiles[tiles.FreeIndex()] = pTile;
	}

	internal void addNeighbour(MapChunk pNeighbour, TileDirection pDirection, IList<MapChunk> pNeighbours, IList<MapChunk> pNeighboursAll, bool pDiagonal = false)
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
			chunk_up = pNeighbour;
			break;
		case TileDirection.Down:
			chunk_down = pNeighbour;
			break;
		case TileDirection.Left:
			chunk_left = pNeighbour;
			break;
		case TileDirection.Right:
			chunk_right = pNeighbour;
			break;
		}
	}

	public void calculateRegions()
	{
		dirty_regions = false;
		WorldTile[] array = tiles;
		List<MapRegion> list = regions;
		StackPool<MapRegion> region_pool = _region_pool;
		clearTiles();
		int num = array.Length;
		for (int i = 0; i < num; i++)
		{
			WorldTile worldTile = array[i];
			if (worldTile.region == null)
			{
				MapRegion mapRegion = region_pool.get();
				mapRegion.reset();
				mapRegion.type = worldTile.Type.layer_type;
				mapRegion.chunk = this;
				fillRegion(worldTile, mapRegion);
				mapRegion.id = worldTile.zone.id * 1000 + list.Count;
				list.Add(mapRegion);
				if (mapRegion.tiles.Count == array.Length)
				{
					break;
				}
			}
		}
		clearTiles(pClearRegion: false);
		num = list.Count;
		for (int j = 0; j < num; j++)
		{
			list[j].checkZones();
		}
	}

	private void clearTiles(bool pClearRegion = true)
	{
		WorldTile[] array = tiles;
		if (pClearRegion)
		{
			int num = array.Length;
			for (int i = 0; i < num; i++)
			{
				WorldTile obj = array[i];
				obj.is_checked_tile = false;
				obj.region = null;
			}
		}
		else
		{
			int num2 = array.Length;
			for (int j = 0; j < num2; j++)
			{
				array[j].is_checked_tile = false;
			}
		}
	}

	internal void shuffleRegionTiles()
	{
		for (int i = 0; i < regions.Count; i++)
		{
			MapRegion mapRegion = regions[i];
			if (regions.Count > 1)
			{
				mapRegion.center_region = false;
			}
			else
			{
				mapRegion.center_region = true;
			}
			mapRegion.tiles.Shuffle();
		}
	}

	private void fillRegion(WorldTile pTile, MapRegion pTargetRegion)
	{
		Queue<WorldTile> wave = _wave;
		wave.Enqueue(pTile);
		while (wave.Count > 0)
		{
			WorldTile worldTile = wave.Dequeue();
			worldTile.is_checked_tile = true;
			worldTile.region = pTargetRegion;
			pTargetRegion.tiles.Add(worldTile);
			processTileNeighbours(worldTile, pTargetRegion, wave);
		}
		wave.Clear();
	}

	private void processTileNeighbours(WorldTile pTileMain, MapRegion pTargetRegion, Queue<WorldTile> pWave)
	{
		WorldTile[] neighboursAll = pTileMain.neighboursAll;
		foreach (WorldTile worldTile in neighboursAll)
		{
			TileTypeBase type = worldTile.Type;
			if (!worldTile.is_checked_tile)
			{
				if (type.layer_type != pTileMain.region.type || worldTile.chunk != this)
				{
					pTargetRegion.edge_tiles_set.Add(worldTile);
				}
				else if (!isDiagonalBlockedByCorners(pTileMain, worldTile))
				{
					worldTile.is_checked_tile = true;
					worldTile.region = pTargetRegion;
					pWave.Enqueue(worldTile);
				}
			}
		}
	}

	private bool isDiagonalBlockedByCorners(WorldTile pTileFrom, WorldTile pTileTo)
	{
		int num = pTileTo.x - pTileFrom.x;
		int num2 = pTileTo.y - pTileFrom.y;
		if (Math.Abs(num) != 1 || Math.Abs(num2) != 1)
		{
			return false;
		}
		WorldTile tile = World.world.GetTile(pTileFrom.x + num, pTileFrom.y);
		WorldTile tile2 = World.world.GetTile(pTileFrom.x, pTileFrom.y + num2);
		bool num3 = tile?.Type.block ?? true;
		bool flag = tile2?.Type.block ?? true;
		return num3 | flag;
	}

	public void clearObjects(bool pForceClearBuildings = false)
	{
		if (pForceClearBuildings)
		{
			setBuildingsDirty();
		}
		objects.reset(buildings_dirty);
		_temp_tiles.Clear();
		_new_hashes.Clear();
	}

	public void clearRegions()
	{
		clearIsland();
		for (int i = 0; i < regions.Count; i++)
		{
			MapRegion mapRegion = regions[i];
			mapRegion.reset();
			_region_pool.release(mapRegion);
		}
		regions.Clear();
	}

	public void Dispose()
	{
		clearRegions();
		clearObjects(pForceClearBuildings: true);
		setBuildingsDirty();
		objects.Dispose();
		neighbours.Clear();
		neighbours_all.Clear();
		neighbours = null;
		neighbours_all = null;
		tiles.Clear();
		_wave.Clear();
		edges_all?.Clear();
		chunk_bounds?.Clear();
		bounds_left.Clear();
		bounds_up.Clear();
		bounds_down.Clear();
		bounds_right.Clear();
		chunk_up = null;
		chunk_down = null;
		chunk_left = null;
		chunk_right = null;
		edge_down_left = null;
		edge_down_right = null;
		edge_up_left = null;
		edge_up_right = null;
		_edge_down_left_connection = null;
		_edge_down_right_connection = null;
		_edge_up_left_connection = null;
		_edge_up_right_connection = null;
		_region_pool.clear();
		zones.Clear();
	}

	public void clearIsland()
	{
		for (int i = 0; i < regions.Count; i++)
		{
			MapRegion mapRegion = regions[i];
			mapRegion.clear();
			if (mapRegion.island != null)
			{
				World.world.islands_calculator.makeDirty(mapRegion.island);
			}
		}
	}

	internal void combineEdges()
	{
		HashSet<WorldTile> hashSet = new HashSet<WorldTile>();
		edges_all = new List<WorldTile>(hashSet);
		hashSet.Clear();
		hashSet.UnionWith(bounds_down);
		hashSet.UnionWith(bounds_left);
		hashSet.UnionWith(bounds_right);
		hashSet.UnionWith(bounds_up);
		chunk_bounds = new List<WorldTile>(hashSet);
	}

	public void generateEdgeConnections()
	{
		_edge_up_left_connection = chunk_left?.chunk_up?.edge_down_right;
		_edge_up_right_connection = chunk_right?.chunk_up?.edge_down_left;
		_edge_down_left_connection = chunk_left?.chunk_down?.edge_up_right;
		_edge_down_right_connection = chunk_right?.chunk_down?.edge_up_left;
	}

	public void checkLinksResults()
	{
		for (int i = 0; i < _new_hashes.Count; i++)
		{
			TempLinkStruct tempLinkStruct = _new_hashes[i];
			RegionLinkHashes.addHash(tempLinkStruct.hash, tempLinkStruct.region);
		}
		MapChunkManager.m_newLinks += _new_hashes.Count;
		_new_hashes.Clear();
	}

	internal void calculateLinks()
	{
		dirty_links = false;
		calculateLink(bounds_right, chunk_right?.bounds_left, LinkDirection.Right, LinkDirection.LR, pUseTargetList: true);
		calculateLink(bounds_left, chunk_left?.bounds_right, LinkDirection.Left, LinkDirection.LR);
		calculateLink(bounds_up, chunk_up?.bounds_down, LinkDirection.Up, LinkDirection.UD, pUseTargetList: true);
		calculateLink(bounds_down, chunk_down?.bounds_up, LinkDirection.Down, LinkDirection.UD);
		checkSpecialDiagonalConnection(edge_up_left, _edge_up_left_connection, LinkDirection.Up, LinkDirection.UD);
		checkSpecialDiagonalConnection(edge_up_right, _edge_up_right_connection, LinkDirection.Up, LinkDirection.UD, pUseTargetList: true);
		checkSpecialDiagonalConnection(edge_down_left, _edge_down_left_connection, LinkDirection.Down, LinkDirection.UD);
		checkSpecialDiagonalConnection(edge_down_right, _edge_down_right_connection, LinkDirection.Down, LinkDirection.UD, pUseTargetList: true);
	}

	private void checkSpecialDiagonalConnection(WorldTile pTileMain, WorldTile pTileTarget, LinkDirection pDirection, LinkDirection pGroupID, bool pUseTargetList = false)
	{
		if (pTileTarget != null && WorldTile.isSameLayer(pTileMain, pTileTarget) && !isDiagonalBlockedByCorners(pTileMain, pTileTarget))
		{
			if (pUseTargetList)
			{
				_temp_tiles.Add(pTileTarget);
			}
			else
			{
				_temp_tiles.Add(pTileMain);
			}
			makeLink(_temp_tiles, pDirection, pGroupID, pTileMain.region);
		}
	}

	private void calculateLink(List<WorldTile> pOurBounds, List<WorldTile> pTargetEdgeTiles, LinkDirection pDirection, LinkDirection pGroupID, bool pUseTargetList = false)
	{
		if (pTargetEdgeTiles == null)
		{
			return;
		}
		int count = pOurBounds.Count;
		List<WorldTile> temp_tiles = _temp_tiles;
		temp_tiles.Clear();
		bool flag = false;
		MapRegion pRegionMain = null;
		for (int i = 0; i < count; i++)
		{
			bool flag2 = i == count - 1;
			WorldTile worldTile = pOurBounds[i];
			WorldTile worldTile2 = pTargetEdgeTiles[i];
			bool flag3 = WorldTile.isSameLayer(worldTile, worldTile2);
			if (flag3 && !flag)
			{
				flag = true;
				pRegionMain = worldTile.region;
			}
			if (!flag2)
			{
				if (flag3)
				{
					WorldTile pTile = pOurBounds[i + 1];
					flag3 = WorldTile.isSameLayer(worldTile, pTile);
				}
				if (flag3)
				{
					WorldTile pTile2 = pTargetEdgeTiles[i + 1];
					flag3 = WorldTile.isSameLayer(worldTile, pTile2);
				}
			}
			if (!flag3 && !flag && tryDiagonal(worldTile, worldTile2, pDirection, pGroupID, pUseTargetList, temp_tiles))
			{
				flag = false;
				pRegionMain = null;
				continue;
			}
			if (flag2)
			{
				flag3 = false;
			}
			if (flag || flag3)
			{
				if (!flag & flag3)
				{
					flag = true;
					pRegionMain = worldTile.region;
					saveToConnection(temp_tiles, worldTile, worldTile2, pUseTargetList);
				}
				else if (flag && !flag3)
				{
					saveToConnection(temp_tiles, worldTile, worldTile2, pUseTargetList);
					makeLink(temp_tiles, pDirection, pGroupID, pRegionMain);
					flag = false;
					pRegionMain = null;
				}
				else
				{
					saveToConnection(temp_tiles, worldTile, worldTile2, pUseTargetList);
				}
			}
		}
	}

	private bool tryDiagonal(WorldTile pMainTile, WorldTile pTargetTile, LinkDirection pDirection, LinkDirection pGroupID, bool pUseTargetList, List<WorldTile> pListConnections)
	{
		bool result = false;
		WorldTile diagonalConnection = getDiagonalConnection(pMainTile, pTargetTile, pDirection, pFirst: true);
		if (diagonalConnection != null)
		{
			saveToConnection(pListConnections, pMainTile, diagonalConnection, pUseTargetList);
			makeLink(pListConnections, pDirection, pGroupID, pMainTile.region);
			result = true;
		}
		WorldTile diagonalConnection2 = getDiagonalConnection(pMainTile, pTargetTile, pDirection, pFirst: false);
		if (diagonalConnection2 != null)
		{
			saveToConnection(pListConnections, pMainTile, diagonalConnection2, pUseTargetList);
			makeLink(pListConnections, pDirection, pGroupID, pMainTile.region);
			result = true;
		}
		return result;
	}

	private void saveToConnection(List<WorldTile> pList, WorldTile pOurTile, WorldTile pTargetTile, bool pUseTargetList)
	{
		if (pUseTargetList)
		{
			pList.Add(pTargetTile);
		}
		else
		{
			pList.Add(pOurTile);
		}
	}

	private WorldTile getDiagonalConnection(WorldTile pOurTile, WorldTile pTargetTile, LinkDirection pDirection, bool pFirst)
	{
		TileLayerType layer_type = pOurTile.Type.layer_type;
		WorldTile worldTile = null;
		switch (pDirection)
		{
		case LinkDirection.Up:
		case LinkDirection.Down:
			worldTile = (pFirst ? pTargetTile.tile_right : pTargetTile.tile_left);
			break;
		case LinkDirection.Left:
		case LinkDirection.Right:
			worldTile = (pFirst ? pTargetTile.tile_up : pTargetTile.tile_down);
			break;
		}
		if (worldTile == null)
		{
			return null;
		}
		if (isDiagonalBlockedByCorners(pOurTile, worldTile))
		{
			return null;
		}
		if (worldTile.Type.layer_type != layer_type)
		{
			return null;
		}
		return worldTile;
	}

	private void makeLink(List<WorldTile> pConnectionList, LinkDirection pDirection, LinkDirection pGroupID, MapRegion pRegionMain)
	{
		int hash = pRegionMain.newConnection(pConnectionList, pDirection, pGroupID);
		_new_hashes.Add(new TempLinkStruct
		{
			region = pRegionMain,
			hash = hash
		});
		pConnectionList.Clear();
	}

	public void setBuildingsDirty()
	{
		_buildings_dirty = true;
	}

	public void finishBuildingsCheck()
	{
		_buildings_dirty = false;
	}

	public List<MusicBoxTileData> getSimpleData()
	{
		if (_tile_types_dirty)
		{
			musicBoxCheckCount();
		}
		return _simple_data;
	}

	private void musicBoxCheckCount()
	{
		_tile_types_dirty = false;
		_tile_types_count.Clear();
		_simple_data.Clear();
		int i = 0;
		TileTypeBase key;
		for (int count = zones.Count; i < count; i++)
		{
			foreach (KeyValuePair<TileTypeBase, HashSet<WorldTile>> tileType in zones[i].getTileTypes())
			{
				tileType.Deconstruct(out key, out var value);
				TileTypeBase tileTypeBase = key;
				HashSet<WorldTile> hashSet = value;
				if (tileTypeBase.music_assets != null)
				{
					int count2 = hashSet.Count;
					if (!_tile_types_count.TryAdd(tileTypeBase, count2))
					{
						Dictionary<TileTypeBase, int> tile_types_count = _tile_types_count;
						key = tileTypeBase;
						tile_types_count[key] += count2;
					}
				}
			}
		}
		foreach (KeyValuePair<TileTypeBase, int> item in _tile_types_count)
		{
			item.Deconstruct(out key, out var value2);
			TileTypeBase tileTypeBase2 = key;
			int amount = value2;
			_simple_data.Add(new MusicBoxTileData
			{
				tile_type_id = tileTypeBase2.index_id,
				amount = amount
			});
		}
	}

	public Dictionary<TileTypeBase, int> getTileTypesCount()
	{
		if (_tile_types_dirty)
		{
			musicBoxCheckCount();
		}
		return _tile_types_count;
	}

	public int countTilesOfType(TileTypeBase pType)
	{
		if (_tile_types_dirty)
		{
			musicBoxCheckCount();
		}
		_tile_types_count.TryGetValue(pType, out var value);
		return value;
	}

	public void setTileTypesDirty()
	{
		_tile_types_dirty = true;
	}
}
