using UnityEngine;

public class StatusParticle : BaseEffect
{
	public void spawnParticle(Vector3 pVector, Color pColor, float pScale = 0.25f)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		base.prepare(Vector2.op_Implicit(pVector), pScale);
		((Component)this).GetComponent<SpriteRenderer>().color = pColor;
	}

	public override void update(float pElapsed)
	{
		base.update(pElapsed);
		setScale(scale - pElapsed * 0.2f);
		if (scale <= 0f)
		{
			kill();
		}
	}
}
