using System;
using System.Collections.Generic;

public class MapRegion : IEquatable<MapRegion>
{
	public int id;

	public bool used_by_path_lock;

	public int region_path_id = -1;

	public TileLayerType type;

	public readonly float created;

	private readonly List<WorldTile> _edge_tiles = new List<WorldTile>(4);

	public readonly HashSet<WorldTile> edge_tiles_set = new HashSet<WorldTile>();

	private readonly HashSet<MapRegion> _outside_edge_regions = new HashSet<MapRegion>();

	private readonly HashSet<TileIsland> _outside_islands = new HashSet<TileIsland>();

	public readonly List<WorldTile> tiles = new List<WorldTile>(256);

	private readonly List<RegionLink> _links = new List<RegionLink>(4);

	public TileIsland island;

	public bool is_island_checked;

	public bool is_checked_path;

	public int path_wave_id = -1;

	public bool region_path;

	public MapChunk chunk;

	public readonly HashSet<TileZone> zones = new HashSet<TileZone>();

	public bool center_region;

	public readonly List<MapRegion> neighbours = new List<MapRegion>(4);

	private readonly HashSet<MapRegion> _neighbours_hash = new HashSet<MapRegion>();

	public static float created_time_last;

	public List<WorldTile> debug_blink_edges_left;

	public List<WorldTile> debug_blink_edges_up;

	public List<WorldTile> debug_blink_edges_down;

	public List<WorldTile> debug_blink_edges_right;

	public MapRegion()
	{
		created = created_time_last;
	}

	public void reset()
	{
		_edge_tiles.Clear();
		edge_tiles_set.Clear();
		_outside_edge_regions.Clear();
		_outside_islands.Clear();
		tiles.Clear();
		zones.Clear();
		_links.Clear();
		island = null;
		is_island_checked = false;
		is_checked_path = false;
		path_wave_id = -1;
		region_path = false;
		chunk = null;
		center_region = false;
		neighbours.Clear();
		_neighbours_hash.Clear();
		debug_blink_edges_left?.Clear();
		debug_blink_edges_up?.Clear();
		debug_blink_edges_down?.Clear();
		debug_blink_edges_right?.Clear();
	}

	public void checkZones()
	{
		if (chunk.regions.Count == 1)
		{
			zones.UnionWith(chunk.zones);
			return;
		}
		List<WorldTile> list = tiles;
		int count = list.Count;
		for (int i = 0; i < count; i++)
		{
			zones.Add(list[i].zone);
		}
	}

	private void checkDebugLists()
	{
		if (DebugConfig.isOn(DebugOption.Connections) && debug_blink_edges_left == null)
		{
			debug_blink_edges_left = new List<WorldTile>();
			debug_blink_edges_up = new List<WorldTile>();
			debug_blink_edges_down = new List<WorldTile>();
			debug_blink_edges_right = new List<WorldTile>();
		}
	}

	internal int newConnection(List<WorldTile> pTiles, LinkDirection pDirection, LinkDirection pDirectionID)
	{
		return newHash(pTiles, pTiles.Count, pDirection, pDirectionID);
	}

	private void checkDebugEdges(List<WorldTile> pTiles, LinkDirection pDirection)
	{
		if (DebugConfig.isOn(DebugOption.Connections))
		{
			List<WorldTile> list = null;
			checkDebugLists();
			switch (pDirection)
			{
			case LinkDirection.Left:
				list = debug_blink_edges_left;
				break;
			case LinkDirection.Down:
				list = debug_blink_edges_down;
				break;
			case LinkDirection.Right:
				list = debug_blink_edges_right;
				break;
			case LinkDirection.Up:
				list = debug_blink_edges_up;
				break;
			}
			list?.AddRange(pTiles);
		}
	}

	private int newHash(List<WorldTile> pTiles, int pLen, LinkDirection pDirection, LinkDirection pDirectionID)
	{
		WorldTile worldTile = pTiles[0];
		int x = worldTile.x;
		int y = worldTile.y;
		int num = (int)(worldTile.Type.layer_type + 1) * 100000 + (x * 11 + y * 3) * 10000 + pLen * 1000 + x * 10 + y;
		if (pDirectionID == LinkDirection.LR)
		{
			num = -num;
		}
		return num;
	}

	public void clear()
	{
		for (int i = 0; i < _links.Count; i++)
		{
			RegionLinkHashes.remove(_links[i], this);
		}
		_links.Clear();
		region_path_id = -1;
	}

	public void addLink(RegionLink pLink)
	{
		_links.Add(pLink);
	}

	public void calculateNeighbours()
	{
		neighbours.Clear();
		_neighbours_hash.Clear();
		for (int i = 0; i < _links.Count; i++)
		{
			foreach (MapRegion region in _links[i].regions)
			{
				if (region != this)
				{
					_neighbours_hash.Add(region);
				}
			}
		}
		neighbours.AddRange(_neighbours_hash);
	}

	public bool hasNeighbour(MapRegion pRegion)
	{
		return _neighbours_hash.Contains(pRegion);
	}

	public void debugLinks(DebugTool pTool)
	{
		pTool.setText("- links:", _links.Count, 0f, pShowBar: false, 0L);
		List<RegionLink> list = new List<RegionLink>(_links);
		list.Sort(linkSortByID);
		for (int i = 0; i < list.Count; i++)
		{
			_ = list[i];
			pTool.setText("- hash " + i + ":", list[i].id, 0f, pShowBar: false, 0L);
		}
	}

	private int linkSortByID(RegionLink o1, RegionLink o2)
	{
		return o2.id.CompareTo(o1.id);
	}

	public void calculateCenterRegion()
	{
		center_region = true;
		if (chunk.regions.Count > 1)
		{
			center_region = false;
			return;
		}
		MapChunk[] neighbours_all = chunk.neighbours_all;
		for (int i = 0; i < neighbours_all.Length; i++)
		{
			List<MapRegion> regions = neighbours_all[i].regions;
			if (regions.Count > 1)
			{
				center_region = false;
				break;
			}
			if (regions[0].island != island)
			{
				center_region = false;
				break;
			}
		}
	}

	public void calculateTileEdges()
	{
		if (center_region)
		{
			return;
		}
		_edge_tiles.Clear();
		_outside_edge_regions.Clear();
		_outside_islands.Clear();
		foreach (WorldTile item in edge_tiles_set)
		{
			if (item.Type.layer_type != type)
			{
				_edge_tiles.Add(item);
				_outside_edge_regions.Add(item.region);
			}
		}
	}

	public WorldTile getRandomTile()
	{
		return tiles.GetRandom();
	}

	public bool isTypeGround()
	{
		return type == TileLayerType.Ground;
	}

	public List<WorldTile> getEdgeTiles()
	{
		return _edge_tiles;
	}

	public HashSet<MapRegion> getEdgeRegions()
	{
		return _outside_edge_regions;
	}

	public override int GetHashCode()
	{
		return id;
	}

	public override bool Equals(object obj)
	{
		return Equals(obj as MapRegion);
	}

	public bool Equals(MapRegion pObject)
	{
		return id == pObject.id;
	}
}
