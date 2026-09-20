namespace ai.behaviours;

public class BehUnloadResources : BehCityActor
{
	public override BehResult execute(Actor pActor)
	{
		if (!pActor.isCarryingResources())
		{
			return BehResult.Continue;
		}
		pActor.giveInventoryResourcesToCity();
		return BehResult.Continue;
	}
}
