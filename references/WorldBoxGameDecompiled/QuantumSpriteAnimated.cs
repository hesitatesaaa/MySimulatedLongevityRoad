using UnityEngine;

public class QuantumSpriteAnimated : QuantumSpriteArrows
{
	[SerializeField]
	private SpriteAnimation _animation;

	private void Update()
	{
		_animation.update(World.world.delta_time);
	}
}
