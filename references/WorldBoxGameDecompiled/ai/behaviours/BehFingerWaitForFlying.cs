namespace ai.behaviours;

public class BehFingerWaitForFlying : BehFinger
{
	public override BehResult execute(Actor pActor)
	{
		if (finger.flying_target != pActor.position_height)
		{
			return BehResult.RepeatStep;
		}
		return BehResult.Continue;
	}
}
