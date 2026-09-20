using System.Collections.Generic;
using UnityEngine;

public class MagnetThrow
{
	private Vector2 _mouse_velocity;

	private Vector2 _last_mouse_position;

	private readonly List<Vector2> _velocity_samples;

	private const int MAX_VELOCITY_SAMPLES = 5;

	private const float THROW_FORCE_MULTIPLIER = 5f;

	public const float MIN_THROW_FORCE = 0.1f;

	private const float MAX_THROW_FORCE = 10f;

	private Vector2 _throw_momentum;

	private const float MOMENTUM_DECAY = 0.85f;

	private const float MOMENTUM_BUILD_RATE = 0.7f;

	public void initializeMouseTracking()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		_last_mouse_position = World.world.getMousePos();
		_velocity_samples.Clear();
		_throw_momentum = Vector2.zero;
	}

	public void trackMouseMovement(int pMagnetState)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		if (pMagnetState == 1)
		{
			Vector2 mousePos = World.world.getMousePos();
			Vector2 val = mousePos - _last_mouse_position;
			_mouse_velocity = val * 60f;
			_velocity_samples.Add(_mouse_velocity);
			if (_velocity_samples.Count > 5)
			{
				_velocity_samples.RemoveAt(0);
			}
			Vector2 val2 = val * 0.7f;
			_throw_momentum = Vector2.Lerp(_throw_momentum, val2, Time.deltaTime * 10f);
			_last_mouse_position = mousePos;
		}
	}

	public Vector2 calculateThrowForce()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		Vector2 val = Vector2.zero;
		if (_velocity_samples.Count > 0)
		{
			foreach (Vector2 velocity_sample in _velocity_samples)
			{
				val += velocity_sample;
			}
			val /= (float)_velocity_samples.Count;
		}
		Vector2 result = val * 5f * Time.deltaTime;
		float magnitude = ((Vector2)(ref result)).magnitude;
		if (magnitude > 10f)
		{
			result = ((Vector2)(ref result)).normalized * 10f;
		}
		else if (magnitude < 0.1f && magnitude > 0.1f)
		{
			result = ((Vector2)(ref result)).normalized * 0.1f;
		}
		return result;
	}

	public void clear()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		_velocity_samples.Clear();
		_throw_momentum = Vector2.zero;
	}

	public MagnetThrow()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		_mouse_velocity = Vector2.zero;
		_velocity_samples = new List<Vector2>();
		_throw_momentum = Vector2.zero;
		base._002Ector();
	}
}
