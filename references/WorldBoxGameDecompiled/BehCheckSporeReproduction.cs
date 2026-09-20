using ai.behaviours;

public class BehCheckSporeReproduction : BehaviourActionActor
{
	public override BehResult execute(Actor pActor)
	{
		BabyMaker.spawnSporesFor(pActor);
		return BehResult.Continue;
	}
}
