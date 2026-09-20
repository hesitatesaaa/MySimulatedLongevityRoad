namespace ai.behaviours;

public class BehFamilyGroupJoin : BehaviourActionActor
{
	public override BehResult execute(Actor pActor)
	{
		if (pActor.hasFamily())
		{
			return BehResult.Stop;
		}
		Family nearbyFamily = BehaviourActionBase<Actor>.world.families.getNearbyFamily(pActor.asset, pActor.current_tile);
		if (nearbyFamily != null)
		{
			pActor.setFamily(nearbyFamily);
		}
		return BehResult.Continue;
	}
}
