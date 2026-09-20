namespace ai.behaviours;

public class KingdomBehRandomWait : BehaviourActionKingdom
{
	private float min;

	private float max;

	public override bool shouldRetry(Kingdom pObject)
	{
		return false;
	}

	public KingdomBehRandomWait(float pMin = 0f, float pMax = 1f)
	{
		min = pMin;
		max = pMax;
	}

	public override BehResult execute(Kingdom pKingdom)
	{
		pKingdom.timer_action = Randy.randomFloat(min, max);
		return BehResult.Continue;
	}
}
