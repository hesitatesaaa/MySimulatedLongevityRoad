namespace ai.behaviours;

public class BehFindTargetForHunter : BehCityActor
{
	public override BehResult execute(Actor pActor)
	{
		if (pActor.beh_actor_target != null && pActor.isTargetOkToAttack(pActor.beh_actor_target.a))
		{
			return BehResult.Continue;
		}
		pActor.beh_actor_target = getClosestMeatActor(pActor, 3);
		if (pActor.beh_actor_target != null)
		{
			return BehResult.Continue;
		}
		return BehResult.Stop;
	}

	private Actor getClosestMeatActor(Actor pActor, int pMinAge = 0, bool pCheckSame = false)
	{
		BehaviourActionActor.temp_actors.Clear();
		foreach (Actor item in Finder.getUnitsFromChunk(pActor.current_tile, 3))
		{
			if (!item.isSameKingdom(pActor) && pActor.isTargetOkToAttack(item) && item.asset.source_meat && (pMinAge <= 0 || item.getAge() >= pMinAge))
			{
				BehaviourActionActor.temp_actors.Add(item);
			}
		}
		return Toolbox.getClosestActor(BehaviourActionActor.temp_actors, pActor.current_tile);
	}
}
