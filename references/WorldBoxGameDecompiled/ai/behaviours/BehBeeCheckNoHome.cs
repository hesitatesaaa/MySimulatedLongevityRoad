namespace ai.behaviours;

public class BehBeeCheckNoHome : BehaviourActionActor
{
	public override BehResult execute(Actor pActor)
	{
		if (pActor.getHomeBuilding() == null)
		{
			return BehResult.Continue;
		}
		return BehResult.Stop;
	}
}
