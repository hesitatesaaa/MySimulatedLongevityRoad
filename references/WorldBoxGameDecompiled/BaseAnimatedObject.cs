using UnityEngine;

public class BaseAnimatedObject : BaseMapObject
{
	internal SpriteAnimation sprite_animation;

	private bool _has_sprite_animation;

	public virtual void Awake()
	{
		sprite_animation = ((Component)this).gameObject.GetComponent<SpriteAnimation>();
		_has_sprite_animation = (Object)(object)sprite_animation != (Object)null;
	}

	internal override void create()
	{
		base.create();
		if (_has_sprite_animation)
		{
			sprite_animation.create();
		}
	}

	public override void update(float pElapsed)
	{
		base.update(pElapsed);
		updateSpriteAnimation(pElapsed);
	}

	internal void resetAnim()
	{
		if (_has_sprite_animation)
		{
			sprite_animation.resetAnim();
		}
	}

	internal void updateSpriteAnimation(float pElapsed, bool pForce = false)
	{
		if (_has_sprite_animation)
		{
			sprite_animation.update(pElapsed);
		}
	}

	public override void Dispose()
	{
		sprite_animation = null;
		base.Dispose();
	}
}
