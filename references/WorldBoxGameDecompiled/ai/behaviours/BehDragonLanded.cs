namespace ai.behaviours;

public class BehDragonLanded : BehDragon
{
	public override BehResult execute(Actor pActor)
	{
		dragon.lastLanded = pActor.current_tile;
		return BehResult.Continue;
	}
}
