namespace ai.behaviours;

public class BehReplenishEnergy : BehaviourActionActor
{
	public override BehResult execute(Actor pActor)
	{
		pActor.setMaxMana();
		pActor.setMaxStamina();
		return BehResult.Continue;
	}
}
