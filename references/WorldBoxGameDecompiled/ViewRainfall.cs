using UnityEngine;

public class ViewRainfall : MapLayer
{
	internal override void create()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		colorValues = new Color(0f, 0f, 1f);
		colors_amount = 10;
		base.create();
		sprRnd.color = new Color(1f, 1f, 1f, 0.6f);
	}

	public void setTileDirty(WorldTile pTile)
	{
	}

	protected override void UpdateDirty(float pElapsed)
	{
	}
}
