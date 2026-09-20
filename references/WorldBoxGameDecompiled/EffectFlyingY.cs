using UnityEngine;

public class EffectFlyingY : BaseEffect
{
	public override void update(float pElapsed)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		base.update(pElapsed);
		Vector3 position = ((Component)this).transform.position;
		float x = position.x;
		float num = position.y + pElapsed * 1f / Config.time_scale_asset.multiplier;
		((Component)this).transform.position = new Vector3(x, num);
	}
}
