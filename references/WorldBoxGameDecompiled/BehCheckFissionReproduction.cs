using ai.behaviours;

public class BehCheckFissionReproduction : BehaviourActionActor
{
	public override BehResult execute(Actor pActor)
	{
		BabyMaker.makeBabyViaFission(pActor);
		return BehResult.Continue;
	}
}
