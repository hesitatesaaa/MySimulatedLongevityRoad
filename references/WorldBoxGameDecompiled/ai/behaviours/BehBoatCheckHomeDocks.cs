namespace ai.behaviours;

public class BehBoatCheckHomeDocks : BehBoat
{
	public override BehResult execute(Actor pActor)
	{
		checkHomeDocks(pActor);
		if (boat.actor.getHomeBuilding() == null)
		{
			return BehResult.Stop;
		}
		return BehResult.Continue;
	}
}
