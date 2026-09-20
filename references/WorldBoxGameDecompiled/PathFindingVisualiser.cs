using System.Collections.Generic;
using EpPathFinding.cs;
using UnityEngine;

public class PathFindingVisualiser : MapLayer
{
	public Color default_color;

	private List<WorldTile> tiles = new List<WorldTile>();

	internal override void create()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		colorValues = new Color(1f, 0.46f, 0.19f, 1f);
		colorValues = default_color;
		base.create();
	}

	protected override void UpdateDirty(float pElapsed)
	{
		if (DebugConfig.isOn(DebugOption.LastPath))
		{
			if (!((Component)this).gameObject.activeSelf)
			{
				((Component)this).gameObject.SetActive(true);
			}
		}
		else if (((Component)this).gameObject.activeSelf)
		{
			((Component)this).gameObject.SetActive(false);
		}
	}

	internal override void clear()
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		if (tiles.Count != 0)
		{
			tiles.Clear();
			for (int i = 0; i < pixels.Length; i++)
			{
				pixels[i] = Color32.op_Implicit(Color.clear);
			}
			createTextureNew();
		}
	}

	internal void showPath(StaticGrid pGrid, List<WorldTile> pTilePath)
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		if (!DebugConfig.isOn(DebugOption.LastPath))
		{
			return;
		}
		clear();
		if (pGrid != null)
		{
			WorldTile[] tiles_list = World.world.tiles_list;
			foreach (WorldTile worldTile in tiles_list)
			{
				tiles.Add(worldTile);
				Vector2Int pos = worldTile.pos;
				int x = ((Vector2Int)(ref pos)).x;
				pos = worldTile.pos;
				Node nodeAt = pGrid.GetNodeAt(x, ((Vector2Int)(ref pos)).y);
				if (nodeAt.isClosed)
				{
					pixels[worldTile.data.tile_id] = Color32.op_Implicit(Color.red);
				}
				else if (nodeAt.isOpened)
				{
					pixels[worldTile.data.tile_id] = Color32.op_Implicit(Color.green);
				}
				else
				{
					pixels[worldTile.data.tile_id] = Color32.op_Implicit(Color.clear);
				}
			}
		}
		foreach (WorldTile item in pTilePath)
		{
			pixels[item.data.tile_id] = Color32.op_Implicit(Color.blue);
			tiles.Add(item);
		}
		updatePixels();
	}
}
