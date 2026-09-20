namespace ai.behaviours;

public class BehDragonFlyUp : BehaviourActionActor
{
	public override BehResult execute(Actor pActor)
	{
		pActor.data.set("landAttacks", 0);
		return BehResult.Continue;
	}
}
