namespace ai.behaviours;

public class BehBoatCollectFish : BehaviourActionActor
{
	public override BehResult execute(Actor pActor)
	{
		pActor.addToInventory("fish", 1);
		return BehResult.Continue;
	}
}
