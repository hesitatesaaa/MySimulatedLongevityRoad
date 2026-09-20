using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public sealed class TileIsland
{
	public readonly RegionsContainer regions = new RegionsContainer();

	public readonly HashSet<MapRegion> insideRegionEdges = new HashSet<MapRegion>();

	public readonly HashSet<MapRegion> outsideRegionEdges = new HashSet<MapRegion>();

	public readonly float created;

	public TileLayerType type = TileLayerType.Ocean;

	public bool dirty;

	private bool _dirty_neighbours = true;

	public readonly int id;

	public readonly int debug_hash_code;

	public readonly HashSetWorldTile tiles_roads = new HashSetWorldTile();

	internal bool removed;

	public readonly List<Actor> actors = new List<Actor>();

	public ListPool<Docks> docks;

	private int _tile_count;

	private readonly Dictionary<TileIsland, bool> _cached_reachable_islands_check = new Dictionary<TileIsland, bool>();

	private readonly HashSet<TileIsland> _connected_islands = new HashSet<TileIsland>();

	public TileIsland(int pID)
	{
		id = pID;
		debug_hash_code = GetHashCode();
		created = Time.time;
	}

	public void reset()
	{
		removed = false;
		_tile_count = 0;
		regions.Clear();
		insideRegionEdges.Clear();
		outsideRegionEdges.Clear();
		tiles_roads.Clear();
		actors.Clear();
		docks?.Dispose();
		docks = null;
		_cached_reachable_islands_check.Clear();
		_connected_islands.Clear();
		dirty = false;
		_dirty_neighbours = true;
	}

	public void addDock(Building pBuilding)
	{
		if (docks == null)
		{
			docks = new ListPool<Docks>();
		}
		docks.Add(pBuilding.component_docks);
	}

	public void clearCache()
	{
		_cached_reachable_islands_check.Clear();
	}

	public void addRegion(MapRegion pRegion)
	{
		regions.Add(pRegion);
	}

	public void removeRegion(MapRegion pRegion)
	{
		regions.Remove(pRegion);
		pRegion.island = null;
		dirty = true;
	}

	public void clearRegionsFromIsland()
	{
		if (!removed)
		{
			removed = true;
			List<MapRegion> simpleList = regions.getSimpleList();
			for (int i = 0; i < simpleList.Count; i++)
			{
				simpleList[i].island = null;
			}
			regions.Clear();
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int getTileCount()
	{
		return _tile_count;
	}

	internal void countTiles()
	{
		_tile_count = 0;
		List<MapRegion> simpleList = regions.getSimpleList();
		for (int i = 0; i < simpleList.Count; i++)
		{
			MapRegion mapRegion = simpleList[i];
			_tile_count += mapRegion.tiles.Count;
		}
	}

	internal WorldTile getRandomTile()
	{
		return regions.GetRandom().tiles.GetRandom();
	}

	internal HashSet<TileIsland> getConnectedIslands()
	{
		return _connected_islands;
	}

	public void setDirtyIslandNeighbours()
	{
		_dirty_neighbours = true;
	}

	public bool isDirtyNeighbours()
	{
		return _dirty_neighbours;
	}

	public void calcNeighbourIslands()
	{
		if (!_dirty_neighbours)
		{
			return;
		}
		_dirty_neighbours = false;
		HashSet<TileIsland> connected_islands = _connected_islands;
		HashSet<MapRegion> hashSet = outsideRegionEdges;
		HashSet<MapRegion> hashSet2 = insideRegionEdges;
		connected_islands.Clear();
		hashSet.Clear();
		foreach (MapRegion item in hashSet2)
		{
			HashSet<MapRegion> edgeRegions = item.getEdgeRegions();
			if (edgeRegions.Count == 0)
			{
				continue;
			}
			foreach (MapRegion item2 in edgeRegions)
			{
				if (hashSet.Add(item2))
				{
					connected_islands.Add(item2.island);
				}
			}
		}
	}

	public bool goodForDocks()
	{
		return getTileCount() >= 2500;
	}

	public bool reachableByCityFrom(TileIsland pIsland)
	{
		if (_cached_reachable_islands_check.ContainsKey(pIsland))
		{
			return _cached_reachable_islands_check[pIsland];
		}
		if (pIsland == this)
		{
			return false;
		}
		HashSet<TileIsland> connectedIslands = getConnectedIslands();
		HashSet<TileIsland> connectedIslands2 = pIsland.getConnectedIslands();
		foreach (TileIsland item in connectedIslands)
		{
			foreach (TileIsland item2 in connectedIslands2)
			{
				if (item == item2 && item.type == TileLayerType.Ocean && item.goodForDocks())
				{
					_cached_reachable_islands_check[pIsland] = true;
					return true;
				}
			}
		}
		_cached_reachable_islands_check[pIsland] = false;
		return false;
	}

	public bool isConnectedWith(TileIsland pIsland)
	{
		if (_connected_islands.Contains(pIsland))
		{
			return true;
		}
		return false;
	}

	internal bool isGoodIslandForActor(Actor pActor)
	{
		if (type == TileLayerType.Block)
		{
			return false;
		}
		if (type == TileLayerType.Ocean && !pActor.isWaterCreature())
		{
			return false;
		}
		if (type == TileLayerType.Ground && pActor.isWaterCreature())
		{
			return false;
		}
		if (type == TileLayerType.Lava && pActor.asset.die_in_lava)
		{
			return false;
		}
		if (_tile_count <= 5)
		{
			return false;
		}
		if (_tile_count < 100 && _tile_count < actors.Count)
		{
			return false;
		}
		return true;
	}
}
