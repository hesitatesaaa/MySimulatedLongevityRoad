namespace ai.behaviours;

public class BehFamilyGroupLeave : BehaviourActionActor
{
	public override BehResult execute(Actor pActor)
	{
		if (!pActor.hasFamily())
		{
			return BehResult.Stop;
		}
		if (!pActor.family.isFull())
		{
			return BehResult.Stop;
		}
		pActor.setFamily(null);
		return BehResult.Continue;
	}
}
