using UnityEngine;

public class EffectParticlesCursor : MonoBehaviour
{
	private SpriteAnimationSimple _sprite_animation;

	private float _speed = 50f;

	private void Awake()
	{
		_sprite_animation = ((Component)this).GetComponent<SpriteAnimationSimple>();
	}

	public void launch()
	{
		_sprite_animation.resetAnim();
		_speed = 50f + Randy.randomFloat(-10f, 10f);
	}

	public void update()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		_sprite_animation.update(Time.deltaTime);
		Transform transform = ((Component)this).transform;
		transform.position += new Vector3(0f, _speed * Time.deltaTime, 0f);
	}

	public SpriteAnimationSimple getAnimation()
	{
		return _sprite_animation;
	}

	public void setFrames(Sprite[] pFrames)
	{
		_sprite_animation.setFrames(pFrames);
	}
}
