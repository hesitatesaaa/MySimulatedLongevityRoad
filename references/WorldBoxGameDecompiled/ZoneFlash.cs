using UnityEngine;

public class ZoneFlash : BaseEffect
{
	public void start(Color pColor, float pAlpha = 0.2f)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		sprite_renderer.color = pColor;
		setAlpha(pAlpha);
	}

	public override void update(float pElapsed)
	{
		setAlpha(alpha - pElapsed * 0.1f);
		if (alpha <= 0f)
		{
			kill();
		}
	}
}
