using System.Collections.Generic;
using UnityEngine;

public class ConwayLife : MapLayer
{
	public static Color32 colorEater;

	public static Color32 colorCreator;

	public bool makeFlash = true;

	private HashSetWorldTile newList;

	private float nextTickTimer;

	private float nextTickInterval = 0.05f;

	private int decreaseTick;

	private List<WorldTile> toRemove = new List<WorldTile>();

	internal override void create()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		base.create();
		colorCreator = Color32.op_Implicit(Toolbox.makeColor("#3BCC55"));
		hashsetTiles = new HashSetWorldTile();
		newList = new HashSetWorldTile();
	}

	protected override void UpdateDirty(float pElapsed)
	{
		UpdateVisual();
		if (World.world.isPaused())
		{
			return;
		}
		if (nextTickTimer > 0f)
		{
			nextTickTimer -= pElapsed;
			return;
		}
		nextTickTimer = nextTickInterval;
		for (int i = 0; i < Config.time_scale_asset.conway_ticks; i++)
		{
			updateTick();
		}
	}

	private void UpdateVisual()
	{
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		if (pixels_to_update.Count == 0)
		{
			return;
		}
		foreach (WorldTile item in pixels_to_update)
		{
			if (hashsetTiles.Contains(item))
			{
				if (item.data.conwayType == ConwayType.Eater)
				{
					pixels[item.data.tile_id] = colorEater;
				}
				else if (item.data.conwayType == ConwayType.Creator)
				{
					pixels[item.data.tile_id] = colorCreator;
				}
				else
				{
					pixels[item.data.tile_id] = Toolbox.clear;
				}
			}
			else
			{
				item.data.conwayType = ConwayType.None;
				pixels[item.data.tile_id] = Toolbox.clear;
			}
		}
		pixels_to_update.Clear();
		updatePixels();
	}

	public void remove(WorldTile pTile)
	{
		if (hashsetTiles.Count != 0)
		{
			hashsetTiles.Remove(pTile);
			pixels_to_update.Add(pTile);
			pTile.data.conwayType = ConwayType.None;
		}
	}

	public void add(WorldTile pTile, string pType)
	{
		if (pType == "conway")
		{
			pTile.data.conwayType = ConwayType.Eater;
		}
		else
		{
			pTile.data.conwayType = ConwayType.Creator;
		}
		hashsetTiles.Add(pTile);
		pixels_to_update.Add(pTile);
	}

	private void updateTick()
	{
		if (decreaseTick-- <= 0)
		{
			decreaseTick = 5;
		}
		if (hashsetTiles.Count <= 0 && newList.Count <= 0)
		{
			return;
		}
		newList.Clear();
		foreach (WorldTile hashsetTile in hashsetTiles)
		{
			checkCell(hashsetTile);
			WorldTile[] neighboursAll = hashsetTile.neighboursAll;
			foreach (WorldTile pCell in neighboursAll)
			{
				checkCell(pCell);
			}
		}
		HashSetWorldTile hashSetWorldTile = hashsetTiles;
		hashsetTiles = newList;
		newList = hashSetWorldTile;
		UpdateVisual();
	}

	private void makeAlive(WorldTile pCell)
	{
		if (decreaseTick == 5)
		{
			MusicBox.playSound("event:/SFX/UNIQUE/ConwayMove", pCell);
			if (pCell.data.conwayType == ConwayType.Eater)
			{
				MapAction.decreaseTile(pCell, pDamage: true, "destroy_no_flash");
			}
			else
			{
				MapAction.increaseTile(pCell, pDamage: true, "destroy_no_flash");
			}
		}
		newList.Add(pCell);
		if (makeFlash)
		{
			makeFlashh(pCell, 25);
		}
	}

	internal void makeFlashh(WorldTile pCell, int pAmount)
	{
		if (pCell.data.conwayType != ConwayType.None)
		{
			_ = pCell.data.conwayType;
		}
	}

	internal override void clear()
	{
		base.clear();
		newList.Clear();
		hashsetTiles.Clear();
	}

	private void checkCell(WorldTile pCell)
	{
		if (pixels_to_update.Contains(pCell))
		{
			return;
		}
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		pixels_to_update.Add(pCell);
		if (pCell.data.conwayType == ConwayType.Eater)
		{
			num2++;
		}
		if (pCell.data.conwayType == ConwayType.Creator)
		{
			num3++;
		}
		WorldTile[] neighboursAll;
		if (hashsetTiles.Contains(pCell))
		{
			neighboursAll = pCell.neighboursAll;
			foreach (WorldTile worldTile in neighboursAll)
			{
				if (hashsetTiles.Contains(worldTile))
				{
					num++;
					if (worldTile.data.conwayType == ConwayType.Creator)
					{
						num3++;
					}
					else if (worldTile.data.conwayType == ConwayType.Eater)
					{
						num2++;
					}
				}
				if (num >= 4)
				{
					if (makeFlash)
					{
						makeFlashh(pCell, 15);
					}
					pCell.data.conwayType = ConwayType.None;
					return;
				}
			}
			if (num == 2 || num == 3)
			{
				if (pCell.data.conwayType == ConwayType.None && (num2 != 0 || num3 != 0))
				{
					if (num2 >= num3)
					{
						pCell.data.conwayType = ConwayType.Eater;
					}
					else
					{
						pCell.data.conwayType = ConwayType.Creator;
					}
				}
				makeAlive(pCell);
			}
			else
			{
				pCell.data.conwayType = ConwayType.None;
			}
			return;
		}
		neighboursAll = pCell.neighboursAll;
		foreach (WorldTile worldTile2 in neighboursAll)
		{
			if (hashsetTiles.Contains(worldTile2))
			{
				num++;
			}
			if (worldTile2.data.conwayType == ConwayType.Eater)
			{
				num2++;
			}
			if (worldTile2.data.conwayType == ConwayType.Creator)
			{
				num3++;
			}
		}
		if (num != 3)
		{
			return;
		}
		if (pCell.data.conwayType == ConwayType.None && (num2 != 0 || num3 != 0))
		{
			if (num2 >= num3)
			{
				pCell.data.conwayType = ConwayType.Eater;
			}
			else
			{
				pCell.data.conwayType = ConwayType.Creator;
			}
		}
		makeAlive(pCell);
	}

	internal void checkKillRange(Vector2Int pPos, int pRad)
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		if (hashsetTiles.Count == 0)
		{
			return;
		}
		toRemove.Clear();
		foreach (WorldTile hashsetTile in hashsetTiles)
		{
			if (Toolbox.DistVec2(hashsetTile.pos, pPos) <= (float)pRad)
			{
				hashsetTile.data.conwayType = ConwayType.None;
				toRemove.Add(hashsetTile);
			}
		}
		foreach (WorldTile item in toRemove)
		{
			remove(item);
		}
	}

	static ConwayLife()
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		colorEater = Color32.op_Implicit(new Color(1f, 0.2f, 1f));
	}
}
