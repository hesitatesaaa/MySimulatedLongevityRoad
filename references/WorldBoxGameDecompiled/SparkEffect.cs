using System.Collections.Generic;
using UnityEngine;

public class SparkEffect : BaseEffect
{
	private const float BASE_ALPHA = 1f;

	private const float BASE_SPEED = 10f;

	private const float RANDOM_OFFSET = 5f;

	[SerializeField]
	private List<SpriteSet> _sprite_sets;

	private float _speed = 10f;

	internal override void prepare(Vector2 pVector, float pScale = 1f)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		base.prepare(pVector, pScale);
		setAlpha(1f);
		sprite_animation.setFrames(_sprite_sets.GetRandom().sprites);
		_speed = 10f + Randy.randomFloat(-5f, 5f);
	}

	public override void update(float pElapsed)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		base.update(pElapsed);
		Transform transform = ((Component)this).transform;
		transform.position += new Vector3(0f, _speed * Time.deltaTime, 0f);
	}
}
