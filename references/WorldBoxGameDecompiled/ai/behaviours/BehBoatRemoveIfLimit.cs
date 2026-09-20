namespace ai.behaviours;

public class BehBoatRemoveIfLimit : BehBoat
{
	public override BehResult execute(Actor pActor)
	{
		if (boat.isHomeDockOverfilled())
		{
			boat.destroyBecauseOverfilled();
			return BehResult.Stop;
		}
		return BehResult.Continue;
	}
}
