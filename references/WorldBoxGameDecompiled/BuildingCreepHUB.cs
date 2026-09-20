public class BuildingCreepHUB : BaseBuildingComponent
{
	private float _interval = 0.1f;

	private float _timer;

	private ListPool<BuildingCreepWorker> _workers;

	internal override void create(Building pBuilding)
	{
		_workers = new ListPool<BuildingCreepWorker>();
		base.create(pBuilding);
		for (int i = 0; i < building.asset.grow_creep_workers; i++)
		{
			_workers.Add(new BuildingCreepWorker(this));
		}
		_interval = building.asset.grow_creep_step_interval;
		_timer = _interval;
	}

	public override void update(float pElapsed)
	{
		base.update(pElapsed);
		if (_timer > 0f)
		{
			_timer -= pElapsed;
			return;
		}
		_timer = _interval;
		ListPool<BuildingCreepWorker> workers = _workers;
		for (int i = 0; i < workers.Count; i++)
		{
			workers[i].update();
		}
	}

	public override void Dispose()
	{
		for (int i = 0; i < _workers.Count; i++)
		{
			_workers[i].Dispose();
		}
		_workers.Dispose();
		_workers = null;
		base.Dispose();
	}
}
