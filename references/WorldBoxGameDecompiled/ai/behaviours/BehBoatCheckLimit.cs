namespace ai.behaviours;

public class BehBoatCheckLimit : BehBoat
{
	public override BehResult execute(Actor pActor)
	{
		if (pActor.getSimpleComponent<Boat>().isHomeDockFull())
		{
			return BehResult.Continue;
		}
		return BehResult.Stop;
	}
}
