using ai.behaviours;

public class BehCheckDivineReproduction : BehaviourActionActor
{
	public override BehResult execute(Actor pActor)
	{
		if (pActor.hasTrait("miracle_bearer"))
		{
			return BehResult.Stop;
		}
		BabyMaker.startMiracleBirth(pActor);
		return BehResult.Continue;
	}
}
