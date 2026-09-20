namespace ai.behaviours;

public class BehBoatDamageCheck : BehBoat
{
	public override BehResult execute(Actor pActor)
	{
		if (pActor.getHealthRatio() < 0.8f)
		{
			checkHomeDocks(pActor);
			if (boat.actor.getHomeBuilding() != null)
			{
				pActor.cancelAllBeh();
				return forceTask(pActor, "boat_return_to_dock");
			}
		}
		return BehResult.Continue;
	}
}
