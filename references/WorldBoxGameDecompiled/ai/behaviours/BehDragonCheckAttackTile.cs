namespace ai.behaviours;

public class BehDragonCheckAttackTile : BehDragon
{
	public override BehResult execute(Actor pActor)
	{
		if (dragon.aggroTargets.Count == 0)
		{
			return BehResult.Continue;
		}
		Actor closestActor = Toolbox.getClosestActor(dragon.aggroTargets, pActor.current_tile);
		if (closestActor != null && closestActor.data != null && closestActor.isAlive() && closestActor.current_tile != null)
		{
			pActor.beh_tile_target = dragon.randomTileWithinLandAttackRange(closestActor.current_tile);
			if (pActor.current_tile != dragon.lastLanded && dragon.landAttackRange(closestActor.current_tile) && Dragon.canLand(pActor))
			{
				return forceTask(pActor, "dragon_land");
			}
		}
		if (pActor.isFlying())
		{
			foreach (Actor aggroTarget in dragon.aggroTargets)
			{
				if (aggroTarget != null && aggroTarget.isAlive() && dragon.targetWithinSlide(aggroTarget.current_tile))
				{
					return forceTask(pActor, "dragon_slide");
				}
			}
		}
		return BehResult.Continue;
	}
}
