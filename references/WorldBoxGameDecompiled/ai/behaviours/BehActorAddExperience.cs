namespace ai.behaviours;

public class BehActorAddExperience : BehaviourActionActor
{
	private int _min;

	private int _max;

	public BehActorAddExperience(int pMin, int pMax)
	{
		_min = pMin;
		_max = pMax;
	}

	public override BehResult execute(Actor pActor)
	{
		int pValue = Randy.randomInt(_min, _max + 1);
		pActor.addExperience(pValue);
		return BehResult.Continue;
	}
}
