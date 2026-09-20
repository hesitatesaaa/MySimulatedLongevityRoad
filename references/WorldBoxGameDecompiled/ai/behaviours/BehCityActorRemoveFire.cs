using UnityEngine;

namespace ai.behaviours;

public class BehCityActorRemoveFire : BehCityActor
{
	protected override void setupErrorChecks()
	{
		base.setupErrorChecks();
		null_check_tile_target = true;
	}

	public override BehResult execute(Actor pActor)
	{
		foreach (WorldTile item in pActor.current_tile.getTilesAround(3))
		{
			if (item != null)
			{
				putOutFireForTile(item);
			}
		}
		return BehResult.Continue;
	}

	private void putOutFireForTile(WorldTile pTile, bool pForceEffect = false)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		bool flag = false;
		if (pTile.isOnFire())
		{
			pTile.stopFire();
			flag = true;
		}
		if (flag | pForceEffect)
		{
			EffectsLibrary.spawnAt("fx_water_splash", Vector2Int.op_Implicit(pTile.pos), 0.1f);
		}
		if (pTile.hasBuilding())
		{
			pTile.building.stopFire();
		}
	}
}
