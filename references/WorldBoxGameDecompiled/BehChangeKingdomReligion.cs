using ai.behaviours;

public class BehChangeKingdomReligion : BehaviourActionActor
{
	public override BehResult execute(Actor pActor)
	{
		if (pActor.hasReligion())
		{
			pActor.kingdom.setReligion(pActor.religion);
		}
		return BehResult.Continue;
	}
}
