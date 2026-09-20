using UnityEngine;

public class BuildingEffectSpawnDrop : BaseBuildingComponent
{
	private float _timer;

	public override void update(float pElapsed)
	{
		if (building.data.hasFlag("stop_spawn_drops"))
		{
			return;
		}
		if (_timer >= 0f)
		{
			_timer -= pElapsed;
			return;
		}
		int num = Mathf.CeilToInt(0f - _timer / building.asset.spawn_drop_interval);
		if (num < 1)
		{
			num = 1;
		}
		_timer = building.asset.spawn_drop_interval;
		building.spawnBurstSpecial(num);
	}
}
