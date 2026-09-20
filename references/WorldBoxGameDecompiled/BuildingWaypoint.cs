public abstract class BuildingWaypoint : BaseBuildingComponent
{
	private const int UNITS_AFFECTED_PER_ACTION = 5;

	private const float ACTION_INTERVAL = 20f;

	private float _action_timer = 20f;

	protected abstract string effect_id { get; }

	protected abstract string trait_id { get; }

	public override void update(float pElapsed)
	{
		base.update(pElapsed);
		if (_action_timer > 0f)
		{
			_action_timer -= pElapsed;
			return;
		}
		_action_timer = 20f;
		doAction(building.current_tile);
	}

	internal void doAction(WorldTile pFromTile)
	{
		spawnMainEffect();
		World.world.applyForceOnTile(building.current_tile, 10, 3f);
		int num = 0;
		foreach (Actor item in Finder.getUnitsFromChunk(pFromTile, 1, 0f, pRandom: true))
		{
			if (!item.hasTrait(trait_id))
			{
				if (item.addTrait(trait_id))
				{
					num++;
				}
				if (num >= 5)
				{
					break;
				}
			}
		}
	}

	public void spawnMainEffect()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		EffectsLibrary.spawnAt(effect_id, building.current_tile.posV3, building.current_scale.y);
	}
}
