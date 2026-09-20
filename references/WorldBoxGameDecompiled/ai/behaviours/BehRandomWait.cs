namespace ai.behaviours;

public class BehRandomWait : BehaviourActionActor
{
	private float min;

	private float max;

	public BehRandomWait(float pMin = 0f, float pMax = 1f, bool pLand = false)
	{
		min = pMin;
		max = pMax;
		land_if_hovering = pLand;
	}

	public override BehResult execute(Actor pActor)
	{
		pActor.timer_action = Randy.randomFloat(min, max);
		return BehResult.Continue;
	}
}
