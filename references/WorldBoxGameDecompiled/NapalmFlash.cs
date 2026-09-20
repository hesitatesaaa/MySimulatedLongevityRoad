using UnityEngine;

public class NapalmFlash : BaseEffect
{
	private bool killing;

	private bool bombSpawned;

	internal void spawnFlash(WorldTile pTile)
	{
		tile = pTile;
		bombSpawned = false;
		killing = false;
		prepare(pTile, 0.1f);
	}

	public static bool napalmEffect(WorldTile pTile, string pPowerID)
	{
		pTile.startFire(pForce: true);
		return true;
	}

	private void Update()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		if (((Component)this).transform.localScale.x < 1f && !killing)
		{
			Vector3 localScale = ((Component)this).transform.localScale;
			localScale.x += World.world.elapsed * 0.7f;
			if (localScale.x >= 0.6f && !bombSpawned)
			{
				bombSpawned = true;
				World.world.loopWithBrush(tile, Brush.get(12), napalmEffect);
			}
			if (localScale.x >= 0.7f)
			{
				localScale.x = 0.7f;
				killing = true;
			}
			localScale.y = localScale.x;
			((Component)this).transform.localScale = localScale;
		}
		else if (killing)
		{
			Vector3 localScale2 = ((Component)this).transform.localScale;
			localScale2.x -= World.world.elapsed * 1.5f;
			localScale2.y = localScale2.x;
			if (localScale2.x <= 0f)
			{
				localScale2.x = 0f;
				kill();
			}
			((Component)this).transform.localScale = localScale2;
		}
	}
}
