using System.Collections.Generic;
using UnityEngine;

public class BoulderCharge : BaseEffect
{
	private const float BASE_ALPHA = 1f;

	private const float ALPHA_CHANGE = 0.001f;

	private const float RANDOM_OFFSET = 20f;

	private const float BASE_TIME_BETWEEN_FRAMES = 0.2f;

	[SerializeField]
	private List<SpriteSet> _sprite_sets;

	private Vector2 _direction;

	internal override void prepare(Vector2 pVector, float pScale = 1f)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		base.prepare(pVector, pScale);
		_direction = Boulder.chargeVector();
		_direction.x += Randy.randomFloat(-20f, 20f);
		_direction.y += Randy.randomFloat(-20f, 20f);
		setAlpha(1f);
		sprite_animation.setFrames(_sprite_sets.GetRandom().sprites);
		sprite_animation.timeBetweenFrames = 0.2f / ((Vector2)(ref _direction)).magnitude;
	}

	public override void update(float pElapsed)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		base.update(pElapsed);
		Transform transform = ((Component)this).transform;
		transform.position += new Vector3((_direction.x + Randy.randomFloat(-20f, 20f)) * Time.deltaTime, _direction.y * Time.deltaTime, 0f);
		setAlpha(alpha - 0.001f);
	}
}
