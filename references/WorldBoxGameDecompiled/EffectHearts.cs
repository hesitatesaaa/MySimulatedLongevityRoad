using UnityEngine;

public class EffectHearts : BaseEffect
{
	internal override void spawnOnTile(WorldTile pTile)
	{
		float pScale = Randy.randomFloat(0.3f, 0.5f);
		prepare(pTile, pScale);
	}

	public override void update(float pElapsed)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		base.update(pElapsed);
		float x = ((Component)this).transform.position.x;
		float num = ((Component)this).transform.position.y + pElapsed * 3f / Config.time_scale_asset.multiplier;
		((Component)this).transform.position = new Vector3(x, num);
	}
}
