namespace ai.behaviours;

public class BehActorRemoveStatus : BehaviourActionActor
{
	private string status;

	public BehActorRemoveStatus(string pStatus)
	{
		status = pStatus;
	}

	public override BehResult execute(Actor pActor)
	{
		pActor.finishStatusEffect(status);
		return BehResult.Continue;
	}
}
