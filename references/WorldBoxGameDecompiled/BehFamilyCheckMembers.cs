using ai.behaviours;

public class BehFamilyCheckMembers : BehaviourActionActor
{
	public override BehResult execute(Actor pActor)
	{
		if (pActor.family.countUnits() > 1)
		{
			return BehResult.Stop;
		}
		pActor.setFamily(null);
		return BehResult.Continue;
	}
}
