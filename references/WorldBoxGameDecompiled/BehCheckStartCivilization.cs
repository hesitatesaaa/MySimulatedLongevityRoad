using ai.behaviours;

public class BehCheckStartCivilization : BehaviourActionActor
{
	public override BehResult execute(Actor pActor)
	{
		pActor.buildCityAndStartCivilization();
		return BehResult.Continue;
	}
}
