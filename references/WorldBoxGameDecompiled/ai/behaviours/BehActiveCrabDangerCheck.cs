namespace ai.behaviours;

public class BehActiveCrabDangerCheck : BehActive
{
	public override BehResult execute(Actor pActor)
	{
		if (pActor.isHungry())
		{
			return BehResult.Continue;
		}
		if (Toolbox.hasDifferentSpeciesInChunkAround(pActor.current_tile, pActor.asset.id))
		{
			pActor.cancelAllBeh();
			pActor.ai.setJob("crab_burrow");
		}
		return BehResult.Continue;
	}
}
