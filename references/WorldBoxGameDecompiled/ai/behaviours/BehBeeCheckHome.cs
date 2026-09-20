namespace ai.behaviours;

public class BehBeeCheckHome : BehaviourActionActor
{
	public override BehResult execute(Actor pActor)
	{
		if (pActor.asset.id != "bee")
		{
			return BehResult.Continue;
		}
		if (pActor.getHomeBuilding() != null)
		{
			return BehResult.Continue;
		}
		return BehResult.Stop;
	}
}
