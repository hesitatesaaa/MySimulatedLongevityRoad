namespace ai.behaviours;

public class BehDragonSetAnimation : BehDragon
{
	private DragonState state;

	private bool looped;

	private bool forceRestart;

	public BehDragonSetAnimation(DragonState pState, bool pLooped = true, bool pForceRestart = true)
	{
		state = pState;
		looped = pLooped;
		forceRestart = pForceRestart;
	}

	public override BehResult execute(Actor pActor)
	{
		if (pActor.flipAnimationActive())
		{
			return BehResult.RepeatStep;
		}
		SpriteAnimation spriteAnimation = dragon.spriteAnimation;
		dragon.setFrames(state, forceRestart);
		spriteAnimation.looped = looped;
		return BehResult.Continue;
	}
}
