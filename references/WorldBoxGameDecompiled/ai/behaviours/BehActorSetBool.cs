namespace ai.behaviours;

public class BehActorSetBool : BehaviourActionActor
{
	private string boolName;

	private bool boolValue;

	public BehActorSetBool(string pBoolName, bool pBoolValue)
	{
		boolName = pBoolName;
		boolValue = pBoolValue;
	}

	public override BehResult execute(Actor pActor)
	{
		pActor.data.set(boolName, boolValue);
		return BehResult.Continue;
	}
}
