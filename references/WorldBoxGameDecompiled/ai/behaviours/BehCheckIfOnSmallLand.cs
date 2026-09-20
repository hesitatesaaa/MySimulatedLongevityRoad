namespace ai.behaviours;

public class BehCheckIfOnSmallLand : BehaviourActionActor
{
	public override BehResult execute(Actor pActor)
	{
		if (pActor.current_tile.region.island.isGoodIslandForActor(pActor))
		{
			return BehResult.Stop;
		}
		return BehResult.Continue;
	}
}
