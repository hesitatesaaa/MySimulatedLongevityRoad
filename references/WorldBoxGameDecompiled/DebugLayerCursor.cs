using System.Collections.Generic;
using UnityEngine;

public class DebugLayerCursor : MapLayer
{
	private Color color_highlight_white;

	private Color color_main;

	private Color color_neighbour;

	private Color color_neighbour_2;

	private Color color_region;

	private Color color_edges;

	private Color color_chunk_bounds;

	private Color color_edges_blink;

	private List<WorldTile> _tiles = new List<WorldTile>();

	private bool blink = true;

	private float timerBlink = 0.2f;

	private float timerRecalc = 0.1f;

	private MapChunk lastChunk;

	internal override void create()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		base.create();
		color_highlight_white = Toolbox.makeColor("#FFFFFF77");
		color_main = new Color(0f, 1f, 0f, 0.1f);
		color_neighbour = new Color(1f, 0f, 1f, 0.8f);
		color_neighbour_2 = new Color(1f, 0f, 1f, 0.3f);
		color_edges = new Color(1f, 0f, 0f, 0.5f);
		color_chunk_bounds = new Color(0f, 1f, 1f, 0.5f);
		color_edges_blink = new Color(0.1f, 0.1f, 1f, 1f);
		color_region = new Color(0f, 0f, 1f, 0.8f);
	}

	protected override void UpdateDirty(float pElapsed)
	{
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_038c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0207: Unknown result type (might be due to invalid IL or missing references)
		//IL_0518: Unknown result type (might be due to invalid IL or missing references)
		//IL_0278: Unknown result type (might be due to invalid IL or missing references)
		//IL_0173: Unknown result type (might be due to invalid IL or missing references)
		//IL_0552: Unknown result type (might be due to invalid IL or missing references)
		//IL_05dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_034b: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0606: Unknown result type (might be due to invalid IL or missing references)
		//IL_0598: Unknown result type (might be due to invalid IL or missing references)
		//IL_0496: Unknown result type (might be due to invalid IL or missing references)
		//IL_04d7: Unknown result type (might be due to invalid IL or missing references)
		if (ScrollWindow.isWindowActive())
		{
			return;
		}
		if (!Config.isEditor && !DebugConfig.instance.debugButton.gameObject.activeSelf)
		{
			clear();
			return;
		}
		if (timerBlink > 0f)
		{
			timerBlink -= Time.deltaTime;
		}
		else
		{
			timerBlink = 0.2f;
			blink = !blink;
		}
		if (timerRecalc > 0f)
		{
			timerRecalc -= pElapsed;
			clear();
			WorldTile mouseTilePos = World.world.getMouseTilePos();
			if (mouseTilePos == null)
			{
				return;
			}
			lastChunk = mouseTilePos.chunk;
			_ = mouseTilePos.chunk;
			_ = lastChunk;
			if (DebugConfig.isOn(DebugOption.RenderIslands) && mouseTilePos?.region?.island != null)
			{
				drawIsland(mouseTilePos.region.island);
			}
			if (DebugConfig.isOn(DebugOption.CursorChunk))
			{
				fill(lastChunk.tiles, color_highlight_white);
			}
			if (DebugConfig.isOn(DebugOption.RenderConnectedIslands) && mouseTilePos?.region?.island != null)
			{
				foreach (TileIsland connectedIsland in mouseTilePos.region.island.getConnectedIslands())
				{
					foreach (MapRegion region2 in connectedIsland.regions)
					{
						fill(region2.tiles, Color.blue);
					}
				}
			}
			if (DebugConfig.isOn(DebugOption.PossibleCityReach))
			{
				renderPossibleCityReach();
			}
			if (DebugConfig.isOn(DebugOption.RenderIslandsInsideRegionCorners) && mouseTilePos?.region?.island != null)
			{
				foreach (MapRegion insideRegionEdge in mouseTilePos.region.island.insideRegionEdges)
				{
					fill(insideRegionEdge.tiles, Color.magenta);
				}
			}
			if (DebugConfig.isOn(DebugOption.RenderIslandsTileCorners) && mouseTilePos?.region?.island != null)
			{
				foreach (MapRegion insideRegionEdge2 in mouseTilePos.region.island.insideRegionEdges)
				{
					fill(insideRegionEdge2.getEdgeTiles(), Color.red);
				}
			}
			if (DebugConfig.isOn(DebugOption.RenderIslandCenterRegions) && mouseTilePos?.region?.island != null)
			{
				foreach (MapRegion region3 in mouseTilePos.region.island.regions)
				{
					if (!region3.center_region)
					{
						fill(region3.tiles, Color.red);
					}
				}
			}
			if (DebugConfig.isOn(DebugOption.RenderRegionOutsideRegionCorners) && mouseTilePos?.region != null)
			{
				foreach (MapRegion edgeRegion in mouseTilePos.region.getEdgeRegions())
				{
					fill(edgeRegion.tiles, Color.yellow);
				}
			}
			if (DebugConfig.isOn(DebugOption.RenderMapRegionEdges) && mouseTilePos.region != null)
			{
				fill(mouseTilePos.region.getEdgeTiles(), Color.red);
			}
			if (DebugConfig.isOn(DebugOption.RegionNeighbours) && mouseTilePos.region != null)
			{
				HashSet<MapRegion> hashSet = new HashSet<MapRegion>();
				HashSet<MapRegion> hashSet2 = new HashSet<MapRegion>();
				hashSet.Add(mouseTilePos.region);
				foreach (MapRegion neighbour in mouseTilePos.region.neighbours)
				{
					hashSet.Add(neighbour);
				}
				foreach (MapRegion item in hashSet)
				{
					foreach (MapRegion neighbour2 in item.neighbours)
					{
						if (!hashSet.Contains(neighbour2))
						{
							hashSet2.Add(neighbour2);
						}
					}
				}
				foreach (MapRegion item2 in hashSet)
				{
					fill(item2.tiles, color_neighbour);
				}
				foreach (MapRegion item3 in hashSet2)
				{
					fill(item3.tiles, color_neighbour_2);
				}
			}
			if (DebugConfig.isOn(DebugOption.Region) && mouseTilePos.region != null)
			{
				fill(mouseTilePos.region.tiles, color_region);
			}
			if (DebugConfig.isOn(DebugOption.ConnectedZones) && mouseTilePos.zone != null)
			{
				TileZone zone = mouseTilePos.zone;
				MapRegion region = mouseTilePos.region;
				fill(zone.tiles, color_region);
				using ListPool<MapRegion> listPool = new ListPool<MapRegion>();
				TileZone[] neighbours = zone.neighbours;
				foreach (TileZone tileZone in neighbours)
				{
					listPool.Clear();
					if (TileZone.hasZonesConnectedViaRegions(zone, tileZone, region, listPool))
					{
						fill(tileZone.tiles, color_neighbour);
					}
				}
			}
			if (DebugConfig.isOn(DebugOption.ChunkEdges) && mouseTilePos.chunk != null)
			{
				fill(mouseTilePos.chunk.edges_all, color_edges);
			}
			if (DebugConfig.isOn(DebugOption.ChunkBounds) && mouseTilePos.chunk != null)
			{
				fill(mouseTilePos.chunk.chunk_bounds, color_chunk_bounds);
			}
			if (DebugConfig.isOn(DebugOption.Connections) && mouseTilePos.region != null)
			{
				drawConnections(mouseTilePos);
			}
			updatePixels();
		}
		else
		{
			timerRecalc = 0.1f;
		}
	}

	private void renderPossibleCityReach()
	{
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		WorldTile mouseTilePos = World.world.getMouseTilePos();
		if (mouseTilePos.zone.city == null)
		{
			return;
		}
		TileIsland island = mouseTilePos.region.island;
		foreach (ref TileIsland island2 in World.world.islands_calculator.islands)
		{
			TileIsland current = island2;
			if (island == current || !island.reachableByCityFrom(current))
			{
				continue;
			}
			foreach (MapRegion region in current.regions)
			{
				fill(region.tiles, Color.blue);
			}
		}
	}

	private void drawIsland(TileIsland pIsland)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		Color32 val = Color32.op_Implicit(Color.red);
		foreach (MapRegion region in pIsland.regions)
		{
			_tiles.AddRange(region.tiles);
			foreach (WorldTile tile in region.tiles)
			{
				pixels[tile.data.tile_id] = val;
			}
		}
	}

	private void drawConnections(WorldTile pTile)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		if (blink && pTile.region.debug_blink_edges_up != null)
		{
			fill(pTile.region.debug_blink_edges_up, color_edges_blink, pEdge: true);
			fill(pTile.region.debug_blink_edges_down, color_edges_blink, pEdge: true);
			fill(pTile.region.debug_blink_edges_left, color_edges_blink, pEdge: true);
			fill(pTile.region.debug_blink_edges_right, color_edges_blink, pEdge: true);
		}
	}

	private void fill(List<WorldTile> pTiles, Color pColor, bool pEdge = false)
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		createTextureNew();
		for (int i = 0; i < pTiles.Count; i++)
		{
			WorldTile worldTile = pTiles[i];
			if (!pEdge || worldTile.region != null)
			{
				_tiles.Add(worldTile);
				pixels[worldTile.data.tile_id] = Color32.op_Implicit(pColor);
			}
		}
	}

	private void fill(WorldTile[] pTiles, Color pColor, bool pEdge = false)
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		createTextureNew();
		foreach (WorldTile worldTile in pTiles)
		{
			if (!pEdge || worldTile.region != null)
			{
				_tiles.Add(worldTile);
				pixels[worldTile.data.tile_id] = Color32.op_Implicit(pColor);
			}
		}
	}

	internal override void clear()
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		if (_tiles.Count != 0)
		{
			_tiles.Clear();
			for (int i = 0; i < pixels.Length; i++)
			{
				pixels[i] = Color32.op_Implicit(Color.clear);
			}
			createTextureNew();
		}
	}
}
