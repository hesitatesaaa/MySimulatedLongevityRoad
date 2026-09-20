using UnityEngine;

public class HeatRayEffect : BaseAnimatedObject
{
	public SpriteAnimation ray;

	public SpriteAnimation heat;

	private bool active;

	private int ticksActive;

	private bool touchedGround;

	private float rayScaleY;

	private float rayWidth = 1f;

	public override void Awake()
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		base.Awake();
		((Component)ray).transform.localScale = new Vector3(1f, 0f, 1f);
	}

	private void Update()
	{
		update(World.world.elapsed);
		ray.update(World.world.elapsed);
		heat.update(World.world.elapsed);
		if (ticksActive > 0)
		{
			ticksActive--;
		}
		else
		{
			active = false;
		}
	}

	internal bool isReady()
	{
		return touchedGround;
	}

	public Vector2 getPosForLight()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		return Vector2.op_Implicit(((Component)heat).transform.position);
	}

	public override void update(float pElapsed)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		base.update(pElapsed);
		Vector3 position = ((Component)ray).transform.position;
		position.z = ((Component)this).transform.position.y;
		((Component)ray).transform.position = position;
		position = ((Component)heat).transform.position;
		((Component)heat).transform.position = position;
		if (active)
		{
			if (rayScaleY < 2000f)
			{
				rayScaleY += pElapsed * 7000f;
				if (rayScaleY >= 2000f)
				{
					rayScaleY = 2000f;
					touchedGround = true;
				}
				((Component)ray).transform.localScale = new Vector3(rayWidth, rayScaleY, 1f);
			}
		}
		else
		{
			touchedGround = false;
			if (rayScaleY > 0f)
			{
				rayScaleY -= pElapsed * 4000f;
				if (rayScaleY < 0f)
				{
					rayScaleY = 0f;
					((Component)this).gameObject.SetActive(false);
				}
				((Component)ray).transform.localScale = new Vector3(rayWidth, rayScaleY, 1f);
			}
		}
		((Component)heat).gameObject.SetActive(touchedGround);
	}

	internal void play(Vector2 pPos, int pSize)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		if (pSize >= 10)
		{
			rayWidth = 1f;
		}
		else
		{
			rayWidth = 0.4f;
		}
		((Component)this).transform.localPosition = new Vector3(pPos.x, pPos.y);
		active = true;
		ticksActive = 4;
		((Component)this).gameObject.SetActive(true);
	}
}
