using ai.behaviours;

public class BehChangeKingdomLanguage : BehaviourActionActor
{
	public override BehResult execute(Actor pActor)
	{
		if (pActor.hasLanguage())
		{
			pActor.kingdom.setLanguage(pActor.language);
		}
		return BehResult.Continue;
	}
}
