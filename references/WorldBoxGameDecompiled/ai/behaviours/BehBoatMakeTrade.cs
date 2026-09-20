namespace ai.behaviours;

public class BehBoatMakeTrade : BehaviourActionActor
{
	public override BehResult execute(Actor pActor)
	{
		pActor.addToInventory("gold", 5);
		return BehResult.Continue;
	}
}
