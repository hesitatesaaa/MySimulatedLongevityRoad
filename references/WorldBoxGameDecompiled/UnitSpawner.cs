public class UnitSpawner : BaseBuildingComponent
{
	private const float SPAWN_INTERVAL = 10f;

	private float _spawn_timer = 1f;

	public override void update(float pElapsed)
	{
		base.update(pElapsed);
		if (_spawn_timer > 0f)
		{
			_spawn_timer -= pElapsed;
			return;
		}
		_spawn_timer = 10f;
		trySpawnUnit();
	}

	private void trySpawnUnit()
	{
		int num = building.countResidents();
		if (num >= building.asset.housing_slots)
		{
			return;
		}
		Subspecies subspecies = null;
		if (num > 0)
		{
			foreach (long resident in building.residents)
			{
				Actor actor = World.world.units.get(resident);
				if (!actor.isRekt())
				{
					subspecies = actor.subspecies;
					break;
				}
			}
		}
		if (subspecies.isRekt() || !subspecies.hasReachedPopulationLimit())
		{
			spawnUnit(subspecies);
		}
	}

	private void spawnUnit(Subspecies pSubspecies)
	{
		string spawn_units_asset = building.asset.spawn_units_asset;
		Actor actor = World.world.units.createNewUnit(spawn_units_asset, building.current_tile, pMiracleSpawn: false, 0f, pSubspecies, null, pSpawnWithItems: true, pAdultAge: true);
		actor.applyRandomForce();
		setUnitFromHere(actor);
	}

	public void setUnitFromHere(Actor pActor)
	{
		pActor.setHomeBuilding(building);
	}
}
