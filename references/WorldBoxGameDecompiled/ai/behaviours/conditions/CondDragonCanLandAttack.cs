namespace ai.behaviours.conditions;

public class CondDragonCanLandAttack : BehaviourActorCondition
{
	public override bool check(Actor pActor)
	{
		pActor.data.get("landAttacks", out var pResult, 0);
		if (pResult >= 2)
		{
			return false;
		}
		pActor.data.get("attacksForCity", out var pResult2, 0);
		if (pResult2 > 0 && pActor.current_tile.zone.city != null)
		{
			pActor.data.get("cityToAttack", out var pResult3, -1L);
			if (pResult3 == pActor.current_tile.zone.city.data.id)
			{
				return true;
			}
		}
		Dragon actorComponent = pActor.getActorComponent<Dragon>();
		if (actorComponent.targetsWithinLandAttackRange())
		{
			return true;
		}
		if (pActor.hasTrait("zombie") && World.world.kingdoms_wild.get("golden_brain").hasBuildings())
		{
			foreach (Building building in World.world.kingdoms_wild.get("golden_brain").buildings)
			{
				if (actorComponent.landAttackRange(building.current_tile))
				{
					return true;
				}
			}
		}
		return false;
	}
}
