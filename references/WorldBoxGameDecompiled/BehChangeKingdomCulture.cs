using ai.behaviours;

public class BehChangeKingdomCulture : BehaviourActionActor
{
	public override BehResult execute(Actor pActor)
	{
		if (pActor.hasCulture())
		{
			pActor.kingdom.setCulture(pActor.culture);
		}
		return BehResult.Continue;
	}
}
