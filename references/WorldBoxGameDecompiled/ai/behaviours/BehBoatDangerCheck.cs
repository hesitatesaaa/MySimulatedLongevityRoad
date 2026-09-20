namespace ai.behaviours;

public class BehBoatDangerCheck : BehBoat
{
	public override BehResult execute(Actor pActor)
	{
		if (pActor.attackedBy != null)
		{
			if (pActor.getHealthRatio() < 0.25f)
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
		return BehResult.Stop;
	}
}
