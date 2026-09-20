using ai.behaviours;

public class BehMakeDecision : BehaviourActionActor
{
	public override BehResult execute(Actor pActor)
	{
		if (pActor.isEgg())
		{
			return BehResult.Stop;
		}
		pActor.batch.c_make_decision.Add(pActor);
		return BehResult.Stop;
	}
}
