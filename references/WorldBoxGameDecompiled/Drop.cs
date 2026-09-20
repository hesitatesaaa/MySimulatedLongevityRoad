using UnityEngine;

public class Drop : BaseMapObject
{
	private readonly bool DEBUG_COLOR;

	public int drop_index;

	internal bool active;

	private SpriteRenderer _sprite_renderer;

	private SpriteAnimation _sprite_animation;

	private float _currentHeightZ;

	private bool _landed;

	private DropAsset _asset;

	internal bool soundOn;

	private bool _parabolic;

	private float _falling_speed;

	private float _scale;

	private bool _force_surprise;

	private long _caster_id;

	private Vector2 _targetPosition;

	private Vector2 _startPosition;

	private float _targetHeight;

	private float _timeToTarget;

	private float _timeInAir;

	private Color _gizmoColor;

	private Color _gizmoColor2;

	private float _rotation_speed;

	private void Awake()
	{
		_sprite_renderer = ((Component)this).gameObject.GetComponent<SpriteRenderer>();
		_sprite_animation = ((Component)this).gameObject.GetComponent<SpriteAnimation>();
	}

	public void setForceSurprise()
	{
		_force_surprise = true;
	}

	internal void prepare()
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		if (!created)
		{
			create();
		}
		((Component)this).gameObject.SetActive(true);
		m_transform.localScale = Vector3.one;
		active = true;
		_force_surprise = false;
		_timeInAir = 0f;
		_timeToTarget = 0f;
		_landed = false;
		_parabolic = false;
		soundOn = false;
		_currentHeightZ = 0f;
		_caster_id = -1L;
		((Component)this).transform.rotation = Quaternion.Euler(new Vector3(0f, 0f, 0f));
		if (DEBUG_COLOR)
		{
			_sprite_renderer.color = Randy.getRandomColor();
		}
	}

	internal void launchStraight(WorldTile pTile, DropAsset pAsset, float zDropHeight = -1f)
	{
		//IL_0216: Unknown result type (might be due to invalid IL or missing references)
		//IL_021b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0222: Unknown result type (might be due to invalid IL or missing references)
		//IL_0227: Unknown result type (might be due to invalid IL or missing references)
		_asset = pAsset;
		if (_asset.animation_rotation)
		{
			_rotation_speed = Randy.randomFloat(_asset.animation_rotation_speed_min, _asset.animation_rotation_speed_max);
			if (Randy.randomBool())
			{
				_rotation_speed *= -1f;
			}
		}
		if (!string.IsNullOrEmpty(_asset.sound_launch))
		{
			MusicBox.playSound(_asset.sound_launch, pTile);
		}
		if (_asset.action_launch != null)
		{
			_asset.action_launch();
		}
		_falling_speed = _asset.falling_speed + Randy.randomFloat(0f, _asset.falling_speed_random);
		if (_asset.cached_sprites == null || _asset.cached_sprites.Length == 0)
		{
			_asset.cached_sprites = SpriteTextureLoader.getSpriteList(_asset.path_texture);
		}
		((Renderer)_sprite_renderer).sharedMaterial = LibraryMaterials.instance.dict[_asset.material];
		_sprite_animation.setFrames(_asset.cached_sprites);
		if (_asset.random_flip)
		{
			_sprite_renderer.flipX = (Randy.randomBool() ? true : false);
		}
		if (_asset.animated)
		{
			_sprite_animation.isOn = true;
			_sprite_animation.timeBetweenFrames = _asset.animation_speed + Randy.randomFloat(0f, _asset.animation_speed_random);
		}
		else
		{
			_sprite_animation.isOn = false;
		}
		if (_asset.random_frame)
		{
			_sprite_animation.setRandomFrame();
		}
		_sprite_animation.forceUpdateFrame();
		current_tile = pTile;
		if (zDropHeight != -1f)
		{
			_currentHeightZ = zDropHeight;
		}
		else
		{
			_currentHeightZ = (int)Randy.randomFloat(pAsset.falling_height.x, pAsset.falling_height.y);
		}
		current_position = new Vector2(pTile.posV3.x, pTile.posV3.y);
		_startPosition = current_position;
		updatePosition();
	}

	public void launchParabolic(float pStartHeight, float pMinHeight, float pMaxHeight, float pMinRadius, float pMaxRadius)
	{
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		Vector2 val = Randy.randomPointOnCircle(pMinRadius, pMaxRadius);
		_targetPosition = _startPosition + val;
		_targetHeight = Randy.randomFloat(pMinHeight, pMaxHeight);
		_startPosition.y += pStartHeight;
		_currentHeightZ = _startPosition.y;
		_timeInAir = 0f;
		if (_scale < 1f)
		{
			_falling_speed /= _scale * 2f;
		}
		float num = Toolbox.DistVec2Float(_startPosition, _targetPosition);
		_timeToTarget = (num + _targetHeight * 3f) * 0.25f / _falling_speed;
		if (_timeToTarget < 1f)
		{
			_timeToTarget += 0.5f;
		}
		_parabolic = true;
		updatePosition();
	}

	private void updateStraightFall(float pElapsed)
	{
		float num = 15f * pElapsed;
		num = ((!(_scale < 1f)) ? (num * _falling_speed) : (num * (_falling_speed / (_scale * 2f))));
		if (_currentHeightZ < 0f)
		{
			num = 0f;
		}
		_currentHeightZ -= num * _scale;
		applyRandomXMove(num);
		if (_currentHeightZ <= 0f)
		{
			_currentHeightZ = 0f;
			updatePosition();
			current_tile = World.world.GetTile((int)current_position.x, (int)current_position.y);
			land();
		}
		else
		{
			updatePosition();
		}
	}

	private void applyRandomXMove(float pChangeX)
	{
		if (_asset.falling_random_x_move && !(pChangeX <= 0f) && Randy.randomBool())
		{
			if (Randy.randomBool())
			{
				current_position.x -= 1f * _scale;
			}
			else
			{
				current_position.x += 1f * _scale;
			}
		}
	}

	private void land()
	{
		if (current_tile != null)
		{
			if (_asset.action_landed != null)
			{
				_asset.action_landed(current_tile, _asset.id);
			}
			if (_asset.action_landed_drop != null)
			{
				_asset.action_landed_drop(this, current_tile, _asset.id);
			}
			if (current_tile.zone.visible && _asset.sound_drop != string.Empty)
			{
				MusicBox.playSound(_asset.sound_drop, current_tile);
			}
			if (_force_surprise || _asset.surprises_units)
			{
				ActionLibrary.suprisedByArchitector(null, current_tile);
			}
		}
		World.world.drop_manager.landDrop(this);
		_landed = true;
	}

	public override void update(float pElapsed)
	{
		if (!_landed)
		{
			_sprite_animation.update(pElapsed);
			if (_parabolic)
			{
				updateParabolicFall(pElapsed);
			}
			else
			{
				updateStraightFall(pElapsed);
			}
			if (!_landed && _asset.animation_rotation)
			{
				updateRotation(pElapsed);
			}
		}
	}

	private void updateRotation(float pElapsed)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		Quaternion rotation = ((Component)this).transform.rotation;
		float num = ((Quaternion)(ref rotation)).eulerAngles.z + _rotation_speed * pElapsed;
		((Component)this).transform.rotation = Quaternion.Euler(new Vector3(0f, 0f, num));
	}

	private void updateParabolicFall(float pElapsed)
	{
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		if (!(_timeInAir > _timeToTarget))
		{
			_timeInAir += pElapsed;
			if (_timeInAir > _timeToTarget)
			{
				_timeInAir = _timeToTarget;
			}
			float num = _timeInAir / _timeToTarget;
			Vector2 val = Toolbox.ParabolaDrag(_startPosition, _targetPosition, _targetHeight, num);
			Vector2 val2 = Vector2.Lerp(_startPosition, _targetPosition, num);
			_currentHeightZ = val.y - val2.y;
			float x = val.x;
			float num2 = val.y - _currentHeightZ;
			((Vector2)(ref current_position)).Set(x, num2);
			if (current_position == _targetPosition)
			{
				current_tile = World.world.GetTile((int)_targetPosition.x, (int)_targetPosition.y);
				land();
			}
			else if (_timeInAir >= _timeToTarget)
			{
				current_tile = World.world.GetTile((int)_targetPosition.x, (int)_targetPosition.y);
				land();
			}
			else
			{
				updatePosition();
			}
		}
	}

	private void updatePosition()
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		Vector3 position = default(Vector3);
		((Vector3)(ref position))._002Ector(current_position.x, current_position.y + _currentHeightZ, _currentHeightZ);
		m_transform.position = position;
	}

	public void setScale(Vector3 pVec)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		m_transform.localScale = pVec;
		_scale = pVec.x;
	}

	public void setCasterId(long pCasterId)
	{
		_caster_id = pCasterId;
	}

	public long getCasterId()
	{
		return _caster_id;
	}

	public void makeInactive()
	{
		reset();
		active = false;
		((Component)this).gameObject.SetActive(false);
	}

	public void reset()
	{
		_asset = null;
		current_tile = null;
	}

	private void OnDrawGizmos()
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		if (_parabolic && !_landed && _timeToTarget != 0f && !(_timeInAir > _timeToTarget))
		{
			if (((Color)(ref _gizmoColor)).Equals(Color.op_Implicit(Vector4.zero)))
			{
				_gizmoColor = Randy.ColorHSV();
			}
			if (((Color)(ref _gizmoColor2)).Equals(Color.op_Implicit(Vector4.zero)))
			{
				_gizmoColor2 = Randy.ColorHSV();
				_gizmoColor2.a = 0.5f;
			}
			Gizmos.color = _gizmoColor;
			Vector2 val = _startPosition;
			Vector2 val2 = _startPosition;
			int num = 60;
			for (int i = 1; i <= num; i++)
			{
				float pTime = (float)i / (float)num * _timeToTarget;
				Vector2 val3 = Toolbox.ParabolaDrag(_startPosition, _targetPosition, _targetHeight, pTime);
				Vector2 val4 = Toolbox.Parabola(_startPosition, _targetPosition, _targetHeight, pTime);
				Gizmos.color = _gizmoColor;
				Gizmos.DrawLine(Vector2.op_Implicit(val), Vector2.op_Implicit(val3));
				Gizmos.color = _gizmoColor2;
				Gizmos.DrawLine(Vector2.op_Implicit(val), Vector2.op_Implicit(val4));
				Gizmos.DrawLine(Vector2.op_Implicit(val3), Vector2.op_Implicit(val4));
				Gizmos.DrawLine(Vector2.op_Implicit(val2), Vector2.op_Implicit(val4));
				val = val3;
				val2 = val4;
			}
		}
	}

	public Drop()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		_scale = 1f;
		_caster_id = -1L;
		_gizmoColor = Color.op_Implicit(Vector4.zero);
		_gizmoColor2 = Color.op_Implicit(Vector4.zero);
		base._002Ector();
	}
}
