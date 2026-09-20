namespace ai.behaviours;

public class BehActorCheckBool : BehaviourActionActor
{
	private string actionIfBool;

	private string boolCheck;

	public BehActorCheckBool(string pBool, string pActionIfHit)
	{
		actionIfBool = pActionIfHit;
		boolCheck = pBool;
	}

	public override BehResult execute(Actor pActor)
	{
		pActor.data.get(boolCheck, out var pResult, false);
		if (pResult)
		{
			pActor.data.removeBool(boolCheck);
			return forceTask(pActor, actionIfBool);
		}
		return BehResult.Continue;
	}
}
