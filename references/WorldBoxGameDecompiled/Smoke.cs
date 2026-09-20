using UnityEngine;

public class Smoke : BaseEffect
{
	private float timer_scale;

	private void Update()
	{
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		if (timer_scale > 0f)
		{
			timer_scale -= World.world.elapsed;
			return;
		}
		timer_scale = 0.01f;
		setAlpha(alpha - 0.01f);
		if (alpha <= 0f)
		{
			controller.killObject(this);
			return;
		}
		if (((Component)this).transform.localScale.x < 4f)
		{
			((Component)this).transform.localScale = new Vector3(((Component)this).transform.localScale.x + 0.03f, ((Component)this).transform.localScale.y + 0.03f);
		}
		((Component)this).transform.localPosition = new Vector3(((Component)this).transform.localPosition.x + World.world.wind_direction.x * 0.5f, ((Component)this).transform.localPosition.y + World.world.wind_direction.y * 0.5f);
	}
}
