using UnityEngine;

public class ExplosionFlash : BaseEffect
{
	private float speed;

	public void start(Vector3 pVector, float pRadius, float pSpeed = 1f)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		speed = pSpeed;
		((Component)this).transform.position = new Vector3(pVector.x, pVector.y);
		setScale(0.005f * pRadius);
		setAlpha(1f);
	}

	public override void update(float pElapsed)
	{
		setAlpha(alpha - pElapsed * speed * 0.5f);
		setScale(scale += pElapsed * speed * 0.1f);
		if (alpha <= 0f)
		{
			kill();
		}
	}
}
