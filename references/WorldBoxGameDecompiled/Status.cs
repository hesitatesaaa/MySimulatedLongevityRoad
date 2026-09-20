using System.Runtime.CompilerServices;

public class Status : CoreSystemObject<StatusData>
{
	private float _action_timer;

	private StatusAsset _asset;

	private bool _finished;

	private BaseSimObject _sim_object;

	private int _anim_frame;

	private float _anim_time_between_frames;

	private float _anim_timer;

	public bool flip_x;

	private double _end_time;

	public float duration;

	private bool _has_action;

	private bool _is_animated;

	public override BaseSystemManager manager => World.world.statuses;

	public int get_sprites_count => _asset.get_sprites_count(_sim_object, _asset);

	public BaseSimObject sim_object => _sim_object;

	public bool is_finished => _finished;

	public StatusAsset asset => _asset;

	public int anim_frame => _anim_frame;

	public void setDuration(float pDuration)
	{
		_end_time = World.world.getCurWorldTime() + (double)pDuration;
		duration = pDuration;
		_finished = false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public double getRemainingTime()
	{
		return _end_time - World.world.getCurWorldTime();
	}

	protected override void setDefaultValues()
	{
		_finished = false;
		_action_timer = 0f;
		_anim_frame = 0;
		_anim_time_between_frames = 0f;
		_anim_timer = 0f;
		flip_x = false;
		_end_time = 0.0;
		duration = 0f;
		_sim_object = null;
		_has_action = false;
		_is_animated = false;
	}

	public void start(BaseSimObject pSimObject, StatusAsset pAsset)
	{
		_sim_object = pSimObject;
		_asset = pAsset;
		_action_timer = _asset.action_interval;
		setDuration(_asset.duration);
		if (_asset.random_frame)
		{
			_anim_frame = Randy.randomInt(0, get_sprites_count);
		}
		if (_asset.random_flip)
		{
			flip_x = Randy.randomBool();
		}
		if (_asset.sprite_list != null)
		{
			_anim_time_between_frames = _asset.animation_speed + Randy.randomFloat(0f, _asset.animation_speed_random);
			_anim_timer = _anim_time_between_frames;
		}
		_has_action = _asset.action != null;
		_is_animated = _asset.animated && _asset.texture != null;
	}

	internal void updateAnimationFrame(float pElapsed)
	{
		if (!_is_animated)
		{
			return;
		}
		_anim_timer -= pElapsed;
		if (_anim_timer <= 0f)
		{
			int num = get_sprites_count;
			_anim_timer = _anim_time_between_frames;
			_anim_frame++;
			if (_anim_frame >= num && _asset.loop)
			{
				_anim_frame = 0;
			}
			else if (_anim_frame >= num)
			{
				_anim_frame = num - 1;
			}
		}
	}

	private void updateActionTimer(float pElapsed)
	{
		float action_interval = _asset.action_interval;
		if (_action_timer > 0f)
		{
			_action_timer -= pElapsed;
			return;
		}
		_action_timer = action_interval;
		if (_sim_object.isAlive())
		{
			_asset.action?.Invoke(_sim_object, _sim_object.current_tile);
		}
	}

	public void update(float pElapsed, float pWorldTime)
	{
		if (_has_action)
		{
			updateActionTimer(pElapsed);
		}
		if ((double)pWorldTime >= _end_time)
		{
			finish();
			if (_sim_object.isAlive())
			{
				_asset.action_finish?.Invoke(_sim_object, _sim_object.current_tile);
			}
		}
	}

	public void finish()
	{
		_finished = true;
	}
}
