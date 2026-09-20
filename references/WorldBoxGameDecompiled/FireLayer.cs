using UnityEngine;

public class FireLayer : MapLayer
{
	internal override void create()
	{
		base.create();
	}

	public void setTileDirty(WorldTile pTile)
	{
		if (!pixels_to_update.Contains(pTile))
		{
			pixels_to_update.Add(pTile);
		}
	}

	protected override void checkAutoDisable()
	{
		bool flag = WorldBehaviourActionFire.hasFires();
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

	protected override void UpdateDirty(float pElapsed)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		if (pixels_to_update.Count <= 0)
		{
			return;
		}
		Color val = Color32.op_Implicit(Toolbox.color_fire);
		foreach (WorldTile item in pixels_to_update)
		{
			if (item.isOnFire())
			{
				float worldTimeElapsedSince = World.world.getWorldTimeElapsedSince(item.data.fire_timestamp);
				val.a = 0.5f + (1f - worldTimeElapsedSince / SimGlobals.m.fire_stop_time);
				pixels[item.data.tile_id] = Color32.op_Implicit(val);
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
