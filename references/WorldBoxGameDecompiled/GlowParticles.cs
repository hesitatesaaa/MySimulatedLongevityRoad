using UnityEngine;

public class GlowParticles : MonoBehaviour
{
	private float cooldown;

	public ParticleSystem particles;

	private void Awake()
	{
		particles = ((Component)this).GetComponent<ParticleSystem>();
	}

	private void Update()
	{
		if (cooldown > 0f)
		{
			cooldown -= Time.deltaTime;
		}
	}

	public void spawn(float pX, float pY, bool pRemoveCooldown = false)
	{
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		if (((Behaviour)this).enabled && particles.particleCount <= 50 && MapBox.isRenderGameplay())
		{
			if (pRemoveCooldown)
			{
				cooldown = 0f;
			}
			if (!(cooldown > 0f))
			{
				cooldown = 0.2f + Randy.randomFloat(0f, 0.3f);
				EmitParams val = default(EmitParams);
				((EmitParams)(ref val)).position = new Vector3(pX, pY);
				particles.Emit(val, 1);
			}
		}
	}

	public void spawn(Vector3 pPos)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		spawn(pPos.x, pPos.y);
	}

	public void clear()
	{
		particles.Clear();
	}
}
