namespace ai.behaviours;

public class BehFamilyFollowAlpha : BehaviourActionActor
{
	public override BehResult execute(Actor pActor)
	{
		if (!pActor.hasFamily())
		{
			return BehResult.Stop;
		}
		if (pActor.family.isAlpha(pActor))
		{
			return BehResult.Stop;
		}
		pActor.family.checkAlpha();
		Actor actor = pActor.family.getAlpha();
		if (pActor.family.isAlpha(pActor))
		{
			return BehResult.Stop;
		}
		if (actor != null && !actor.current_tile.isSameIsland(pActor.current_tile))
		{
			actor = null;
		}
		if (actor == null)
		{
			return BehResult.Stop;
		}
		if (pActor.distanceToObjectTarget(actor) > 400f)
		{
			return BehResult.Stop;
		}
		pActor.beh_actor_target = actor;
		return BehResult.Continue;
	}
}
