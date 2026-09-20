public class WorldBehaviour
{
	private float _timer;

	private WorldBehaviourAsset _asset;

	internal void clear()
	{
		_timer = _asset.interval;
		if (_asset.action_world_clear != null)
		{
			_asset.action_world_clear();
		}
	}

	public WorldBehaviour(WorldBehaviourAsset pAsset)
	{
		_asset = pAsset;
		_timer = _asset.interval;
	}

	public void timerClear()
	{
		_timer = 0f;
	}

	public void update(float pElapsed)
	{
		if ((MapBox.isRenderMiniMap() && !_asset.enabled_on_minimap) || (World.world.isPaused() && _asset.stop_when_world_on_pause))
		{
			return;
		}
		if (_timer > 0f)
		{
			_timer -= pElapsed;
			if (_timer > 0f)
			{
				return;
			}
		}
		float num = _asset.interval + Randy.randomFloat(0f, _asset.interval_random);
		_timer += num;
		_asset.action();
	}
}
