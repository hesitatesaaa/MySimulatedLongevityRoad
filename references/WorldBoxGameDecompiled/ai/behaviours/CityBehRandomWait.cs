namespace ai.behaviours;

public class CityBehRandomWait : BehaviourActionCity
{
	private float min;

	private float max;

	public override bool shouldRetry(City pObject)
	{
		return false;
	}

	public CityBehRandomWait(float pMin = 0f, float pMax = 1f)
	{
		min = pMin;
		max = pMax;
	}

	public override BehResult execute(City pCity)
	{
		pCity.timer_action = Randy.randomFloat(min, max);
		return BehResult.Continue;
	}
}
