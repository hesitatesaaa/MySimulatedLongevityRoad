using System.Collections.Generic;
using UnityEngine;

public class LavaLayer : MapLayer
{
	private List<WorldTile> _to_clear = new List<WorldTile>();

	internal override void create()
	{
		base.create();
	}

	protected override void checkAutoDisable()
	{
		bool flag = WorldBehaviourActionLava.hasLava();
		if (!MapBox.isRenderMiniMap())
		{
			flag = false;
		}
		if (flag)
		{
			if (!((Renderer)sprRnd).enabled)
			{
				((Renderer)sprRnd).enabled = true;
			}
		}
		else if (((Renderer)sprRnd).enabled)
		{
			((Renderer)sprRnd).enabled = false;
		}
	}

	public override void update(float pElapsed)
	{
		checkAutoDisable();
		updateLava();
	}

	public override void draw(float pElapsed)
	{
	}

	private void updateLava()
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		bool flag = false;
		if (_to_clear.Count > 0)
		{
			foreach (WorldTile item in _to_clear)
			{
				pixels[item.data.tile_id] = Toolbox.clear;
			}
			_to_clear.Clear();
			flag = true;
		}
		if (WorldBehaviourActionLava.hasLava())
		{
			flag = true;
			drawLavaPixel(TileLibrary.lava0);
			drawLavaPixel(TileLibrary.lava1);
			drawLavaPixel(TileLibrary.lava2);
			drawLavaPixel(TileLibrary.lava3);
		}
		if (flag)
		{
			updatePixels();
		}
	}

	internal override void clear()
	{
		base.clear();
		_to_clear.Clear();
	}

	private void drawLavaPixel(TileType pType)
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		if (pType.hashset.Count == 0)
		{
			return;
		}
		foreach (WorldTile item in pType.hashset)
		{
			pixels[item.data.tile_id] = pType.color;
			_to_clear.Add(item);
		}
	}
}
