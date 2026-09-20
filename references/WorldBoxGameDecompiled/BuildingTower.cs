using UnityEngine;

public class BuildingTower : BaseBuildingComponent
{
	protected float _check_targets_timeout = 1f;

	private bool _test_shooting;

	protected int _shooting_amount;

	private bool _shooting_active;

	protected BaseSimObject _shooting_target;

	public override void update(float pElapsed)
	{
		base.update(pElapsed);
		if (!building.isUnderConstruction())
		{
			updateTestShooting();
			updateTower(pElapsed);
		}
	}

	protected void updateTestShooting()
	{
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		if (_test_shooting && Input.GetMouseButtonDown(2))
		{
			Vector3 pLaunchPosition = default(Vector3);
			((Vector3)(ref pLaunchPosition))._002Ector(building.current_tile.posV3.x, building.current_tile.posV3.y);
			pLaunchPosition.y += building.asset.tower_projectile_offset;
			World.world.projectiles.spawn(building, null, building.asset.tower_projectile, pLaunchPosition, World.world.getMouseTilePos().posV3);
		}
	}

	protected virtual void updateTower(float pElapsed)
	{
		if (_shooting_active)
		{
			shootAtTarget();
		}
		else
		{
			updateCheckTargets(pElapsed);
		}
	}

	protected virtual void updateCheckTargets(float pElapsed)
	{
		if (_check_targets_timeout > 0f)
		{
			_check_targets_timeout -= pElapsed;
		}
		else
		{
			checkTargets();
		}
	}

	protected virtual void resetTimeout()
	{
		_check_targets_timeout = building.asset.tower_projectile_reload + Randy.randomFloat(0f, building.asset.tower_projectile_reload);
	}

	protected virtual void shootAtTarget()
	{
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		//IL_0166: Unknown result type (might be due to invalid IL or missing references)
		if (_shooting_target == null || !_shooting_target.isAlive())
		{
			_shooting_active = false;
			return;
		}
		_shooting_amount--;
		if (_shooting_amount <= 0)
		{
			_shooting_active = false;
		}
		Vector3 pLaunchPosition = default(Vector3);
		((Vector3)(ref pLaunchPosition))._002Ector(building.current_tile.posV3.x, building.current_tile.posV3.y);
		pLaunchPosition.y += building.asset.tower_projectile_offset;
		Vector3 posV = _shooting_target.current_tile.posV3;
		posV.x += Randy.randomFloat(0f - (_shooting_target.stats["size"] + 1f), _shooting_target.stats["size"] + 1f);
		posV.y += Randy.randomFloat(0f - _shooting_target.stats["size"], _shooting_target.stats["size"]);
		float pTargetZ = 0f;
		if (_shooting_target.isInAir())
		{
			pTargetZ = _shooting_target.getHeight();
		}
		projectileStarted();
		World.world.projectiles.spawn(building, _shooting_target, building.asset.tower_projectile, pLaunchPosition, posV, pTargetZ);
	}

	protected virtual void projectileStarted()
	{
	}

	protected virtual void checkTargets()
	{
		resetTimeout();
		_shooting_target = null;
		_shooting_active = false;
		_shooting_amount = 0;
		BaseSimObject baseSimObject = findTarget();
		if (baseSimObject != null)
		{
			_shooting_active = true;
			_shooting_target = baseSimObject;
			_shooting_amount = building.asset.tower_projectile_amount;
		}
	}

	protected virtual BaseSimObject findTarget()
	{
		return building.findEnemyObjectTarget(building.asset.tower_attack_buildings);
	}

	public override void Dispose()
	{
		_shooting_target = null;
		base.Dispose();
	}
}
