namespace ai.behaviours;

public class BehCheckCityDestroyed : BehaviourActionActor
{
	public override BehResult execute(Actor pActor)
	{
		if (pActor.city == null)
		{
			if (pActor.profession_asset.cancel_when_no_city)
			{
				pActor.stopBeingWarrior();
			}
			pActor.endJob();
			return BehResult.Stop;
		}
		return BehResult.Continue;
	}
}
