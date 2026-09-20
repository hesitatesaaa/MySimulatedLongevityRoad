public class BuildingFruitGrowth : BaseBuildingComponent
{
	private float _resource_reset_time;

	public override void update(float pElapsed)
	{
		if (building.isNormal() && !building.hasResourcesToCollect())
		{
			if (_resource_reset_time > 0f)
			{
				_resource_reset_time -= pElapsed;
				return;
			}
			building.setHaveResourcesToCollect(pValue: true);
			building.setScaleTween(0.75f);
		}
	}

	public void reset()
	{
		_resource_reset_time = 90f;
	}
}
