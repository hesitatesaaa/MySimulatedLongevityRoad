using ai.behaviours;

public class BehCheckVegetativeReproduction : BehaviourActionActor
{
	public override BehResult execute(Actor pActor)
	{
		if (pActor.hasStatus("taking_roots"))
		{
			return BehResult.Stop;
		}
		pActor.addStatusEffect("taking_roots", pActor.getMaturationTimeSeconds());
		pActor.subspecies.counterReproduction();
		return BehResult.Continue;
	}
}
