using UnityEngine;

public class BurnedTilesLayer : MapLayer
{
	public Color color;

	private WorldBehaviour worldBehaviour;

	internal override void create()
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		colorValues = new Color(color.r, color.g, color.b, 0.5f);
		colors_amount = 15;
		autoDisable = false;
		base.create();
		((Behaviour)this).enabled = true;
	}

	public void setTileDirty(WorldTile pTile)
	{
		if (!pixels_to_update.Contains(pTile))
		{
			pixels_to_update.Add(pTile);
		}
	}

	protected override void UpdateDirty(float pElapsed)
	{
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		if (pixels_to_update.Count <= 0)
		{
			return;
		}
		foreach (WorldTile item in pixels_to_update)
		{
			if (item.burned_stages > 0)
			{
				pixels[item.data.tile_id] = colors[item.burned_stages - 1];
			}
			else
			{
				pixels[item.data.tile_id] = Toolbox.clear;
			}
		}
		pixels_to_update.Clear();
		updatePixels();
	}
}
