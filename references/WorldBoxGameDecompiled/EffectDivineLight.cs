using UnityEngine;

public class EffectDivineLight : BaseAnimatedObject
{
	public SpriteAnimation raySpawn;

	public SpriteAnimation rayIdle;

	public SpriteAnimation baseSpawn;

	public SpriteAnimation baseIdle;

	public bool isOn;

	private DivineLightState state;

	public override void Awake()
	{
		base.Awake();
		setState(DivineLightState.SpawnFirstStage);
	}

	private void setState(DivineLightState pState)
	{
		state = pState;
		switch (state)
		{
		case DivineLightState.SpawnFirstStage:
			((Component)raySpawn).gameObject.SetActive(true);
			((Component)rayIdle).gameObject.SetActive(false);
			((Component)baseSpawn).gameObject.SetActive(false);
			((Component)baseIdle).gameObject.SetActive(false);
			break;
		case DivineLightState.SpawnSecondStage:
			((Component)raySpawn).gameObject.SetActive(false);
			((Component)rayIdle).gameObject.SetActive(true);
			((Component)baseSpawn).gameObject.SetActive(true);
			((Component)baseIdle).gameObject.SetActive(false);
			break;
		case DivineLightState.Idle:
			((Component)raySpawn).gameObject.SetActive(false);
			((Component)rayIdle).gameObject.SetActive(true);
			((Component)baseSpawn).gameObject.SetActive(false);
			((Component)baseIdle).gameObject.SetActive(true);
			break;
		case DivineLightState.Hide:
			((Component)raySpawn).gameObject.SetActive(true);
			((Component)rayIdle).gameObject.SetActive(false);
			((Component)baseSpawn).gameObject.SetActive(true);
			((Component)baseIdle).gameObject.SetActive(false);
			break;
		}
	}

	private void stopEffet()
	{
	}

	private void useEffect()
	{
	}

	private void Update()
	{
		if (isOn)
		{
			raySpawn.playType = AnimPlayType.Forward;
			baseSpawn.playType = AnimPlayType.Forward;
			if (raySpawn.isLastFrame())
			{
				((Component)raySpawn).gameObject.SetActive(false);
				((Component)rayIdle).gameObject.SetActive(true);
			}
			else
			{
				((Component)raySpawn).gameObject.SetActive(true);
				((Component)rayIdle).gameObject.SetActive(false);
			}
			if (baseSpawn.isLastFrame())
			{
				((Component)baseSpawn).gameObject.SetActive(false);
				((Component)baseIdle).gameObject.SetActive(true);
			}
			else
			{
				((Component)baseSpawn).gameObject.SetActive(true);
				((Component)baseIdle).gameObject.SetActive(false);
			}
		}
		else
		{
			raySpawn.playType = AnimPlayType.Backward;
			baseSpawn.playType = AnimPlayType.Backward;
			((Component)rayIdle).gameObject.SetActive(false);
			((Component)baseIdle).gameObject.SetActive(false);
			if (raySpawn.isFirstFrame())
			{
				((Component)raySpawn).gameObject.SetActive(false);
			}
			else
			{
				((Component)raySpawn).gameObject.SetActive(true);
			}
			if (baseSpawn.isFirstFrame())
			{
				((Component)baseSpawn).gameObject.SetActive(false);
			}
			else
			{
				((Component)baseSpawn).gameObject.SetActive(true);
			}
		}
		if (((Component)baseSpawn).gameObject.activeSelf)
		{
			baseSpawn.update(World.world.delta_time);
		}
		if (((Component)baseIdle).gameObject.activeSelf)
		{
			baseIdle.update(World.world.delta_time);
		}
		if (((Component)raySpawn).gameObject.activeSelf)
		{
			raySpawn.update(World.world.delta_time);
		}
		if (((Component)rayIdle).gameObject.activeSelf)
		{
			rayIdle.update(World.world.delta_time);
		}
		isOn = false;
	}

	public void playOn(WorldTile pTile)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		((Component)this).gameObject.transform.localPosition = pTile.posV3;
		isOn = true;
	}
}
