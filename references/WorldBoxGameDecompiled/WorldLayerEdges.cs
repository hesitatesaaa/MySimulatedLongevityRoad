using System.Collections.Generic;

public class WorldLayerEdges : MapLayer
{
	private HashSet<MapChunk> _dirty_chunks = new HashSet<MapChunk>();

	private HashSet<MapChunk> _chunks_to_redraw = new HashSet<MapChunk>();

	public override void update(float pElapsed)
	{
	}

	public override void draw(float pElapsed)
	{
		UpdateDirty(pElapsed);
	}

	public void addDirtyChunk(MapChunk pChunk)
	{
		_dirty_chunks.Add(pChunk);
	}

	internal override void clear()
	{
		_dirty_chunks.Clear();
		base.clear();
	}

	public void redraw()
	{
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		_chunks_to_redraw.UnionWith(_dirty_chunks);
		foreach (MapChunk dirty_chunk in _dirty_chunks)
		{
			_chunks_to_redraw.UnionWith(dirty_chunk.neighbours_all);
		}
		_dirty_chunks.Clear();
		foreach (MapChunk item in _chunks_to_redraw)
		{
			WorldTile[] tiles = item.tiles;
			int num = tiles.Length;
			for (int i = 0; i < num; i++)
			{
				WorldTile worldTile = tiles[i];
				pixels[worldTile.data.tile_id] = Toolbox.clear;
				redrawTile(worldTile);
			}
		}
		_chunks_to_redraw.Clear();
		updatePixels();
	}

	private void redrawTile(WorldTile pTile)
	{
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		//IL_016d: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0230: Unknown result type (might be due to invalid IL or missing references)
		//IL_0235: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		WorldTile tile_down = pTile.tile_down;
		WorldTile tile_up = pTile.tile_up;
		if (!pTile.Type.check_edge || pTile.Type.wall)
		{
			return;
		}
		if (tile_down != null && !tile_down.Type.wall)
		{
			if (pTile.Type.edge_hills && !tile_down.Type.rocks)
			{
				pixels[pTile.data.tile_id] = TileTypeBase.edge_color_hills;
				return;
			}
			if (pTile.Type.edge_mountains && tile_down.Type.edge_hills)
			{
				pixels[pTile.data.tile_id] = TileTypeBase.edge_color_mountain;
				return;
			}
			if (pTile.Type.rocks && !tile_down.Type.rocks)
			{
				pixels[pTile.data.tile_id] = TileTypeBase.edge_color_mountain;
				return;
			}
			if (!tile_down.Type.ocean && !pTile.Type.ocean && tile_down.Type != pTile.Type && tile_down.Type.height_min < pTile.Type.height_min)
			{
				pixels[pTile.data.tile_id] = Toolbox.edge_alpha;
				return;
			}
		}
		if (tile_up == null)
		{
			return;
		}
		if (tile_up.Type.wall)
		{
			pixels[pTile.data.tile_id] = tile_up.Type.edge_color;
		}
		else if (pTile.Type.ocean && tile_up.Type.ocean && tile_up.Type.height_min > pTile.Type.height_min)
		{
			pixels[pTile.data.tile_id] = Toolbox.edge_alpha;
		}
		else if (tile_up.Type.ground && !tile_up.Type.can_be_filled_with_ocean)
		{
			if (pTile.Type.layer_type == TileLayerType.Ocean)
			{
				pixels[pTile.data.tile_id] = TileTypeBase.edge_color_ocean;
			}
			else if (pTile.Type.can_be_filled_with_ocean && !pTile.Type.explodable)
			{
				pixels[pTile.data.tile_id] = TileTypeBase.edge_color_no_ocean;
			}
		}
	}
}
