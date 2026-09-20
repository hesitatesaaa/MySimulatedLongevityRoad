namespace ai.behaviours;

public class BehActorSetInt : BehaviourActionActor
{
	private string intName;

	private int intValue;

	public BehActorSetInt(string pIntName, int pIntValue)
	{
		intName = pIntName;
		intValue = pIntValue;
	}

	public override BehResult execute(Actor pActor)
	{
		pActor.data.set(intName, intValue);
		return BehResult.Continue;
	}
}
