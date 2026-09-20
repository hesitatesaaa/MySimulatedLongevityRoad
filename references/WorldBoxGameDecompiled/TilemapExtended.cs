using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapExtended : MonoBehaviour
{
	public int z;

	private Tilemap _tilemap;

	private readonly List<Vector3Int> _vec = new List<Vector3Int>();

	private readonly ListPool<TileBase> _tiles = new ListPool<TileBase>();

	public void create(TileTypeBase pTileBase)
	{
		z = pTileBase.render_z;
		((Object)((Component)this).gameObject).name = pTileBase.draw_layer_name;
		TilemapRenderer component = ((Component)this).GetComponent<TilemapRenderer>();
		((Renderer)component).sortingOrder = pTileBase.render_z;
		((Renderer)component).sharedMaterial = LibraryMaterials.instance.dict[pTileBase.material];
		if (pTileBase.id == "deep_ocean")
		{
			((Component)this).gameObject.SetActive(false);
		}
		_tilemap = ((Component)this).GetComponent<Tilemap>();
	}

	internal void prepareDraw()
	{
		_vec.Clear();
		_tiles.Clear();
	}

	internal void addToQueueToRedraw(WorldTile pWorldTile, Vector3Int pPosition, TileBase pTileGraphics, bool pSkipCheck = false)
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		((Vector3Int)(ref pPosition)).z = 0;
		if (!pSkipCheck)
		{
			if ((Object)(object)pWorldTile.current_rendered_tile_graphics == (Object)(object)pTileGraphics && pTileGraphics != null)
			{
				return;
			}
			pWorldTile.current_rendered_tile_graphics = pTileGraphics;
		}
		_vec.Add(pPosition);
		_tiles.Add(pTileGraphics);
	}

	internal void clear()
	{
		_tilemap.ClearAllTiles();
	}

	internal void redraw()
	{
		if (_vec.Count != 0)
		{
			_tilemap.SetTiles(_vec.ToArray(), _tiles.GetRawBuffer());
		}
	}
}
