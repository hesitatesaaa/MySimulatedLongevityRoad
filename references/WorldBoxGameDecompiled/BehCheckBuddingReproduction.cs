using ai.behaviours;

public class BehCheckBuddingReproduction : BehaviourActionActor
{
	public override BehResult execute(Actor pActor)
	{
		if (pActor.hasStatus("budding"))
		{
			return BehResult.Stop;
		}
		pActor.addStatusEffect("budding", pActor.getMaturationTimeSeconds());
		pActor.subspecies.counterReproduction();
		return BehResult.Continue;
	}
}
