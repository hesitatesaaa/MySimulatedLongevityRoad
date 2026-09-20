using UnityEngine;

public class BuildingMonolith : BaseBuildingComponent
{
	private const float ACTION_INTERVAL = 10f;

	private float _action_timer = 10f;

	public override void update(float pElapsed)
	{
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		base.update(pElapsed);
		if (Date.isMonolithMonth() && building.is_visible && building.isNormal() && Time.frameCount % 30 == 0)
		{
			EffectsLibrary.spawnAt("fx_monolith_glow_2", building.current_tile.posV3, building.current_scale.y);
		}
		if (_action_timer > 0f)
		{
			_action_timer -= pElapsed;
		}
		else if (Date.isMonolithMonth())
		{
			_action_timer = 10f;
			doMonolithAction(building.current_tile);
		}
	}

	internal void doMonolithAction(WorldTile pFromTile, bool pForce = false)
	{
		if (!WorldLawLibrary.world_law_evolution_events.isEnabled())
		{
			return;
		}
		spawnMainEffect();
		World.world.applyForceOnTile(building.current_tile, 10, 3f);
		int num = 3;
		int num2 = 0;
		foreach (Actor item in Finder.getUnitsFromChunk(pFromTile, 1, 0f, pRandom: true))
		{
			if (!item.hasStatus("confused") && item.hasSubspecies() && (Date.isMonolithMonth() || pForce))
			{
				if (ActionLibrary.tryToEvolveUnitViaMonolith(item))
				{
					num2++;
				}
				if (num2 >= num)
				{
					break;
				}
			}
		}
	}

	public void spawnMainEffect()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		EffectsLibrary.spawnAt("fx_monolith_launch_bottom", building.current_tile.posV3, building.current_scale.y);
		EffectsLibrary.spawnAt("fx_monolith_launch", building.current_tile.posV3, building.current_scale.y);
	}
}
