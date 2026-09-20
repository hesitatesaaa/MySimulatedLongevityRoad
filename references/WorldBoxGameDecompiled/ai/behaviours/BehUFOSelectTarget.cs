namespace ai.behaviours;

public class BehUFOSelectTarget : BehaviourActionActor
{
	public override BehResult execute(Actor pActor)
	{
		UFO actorComponent = pActor.getActorComponent<UFO>();
		if (actorComponent.aggroTargets.Count > 0)
		{
			BehaviourActionActor.temp_actors.Clear();
			foreach (Actor aggroTarget in actorComponent.aggroTargets)
			{
				if (aggroTarget != null && aggroTarget.isAlive())
				{
					BehaviourActionActor.temp_actors.Add(aggroTarget);
				}
			}
			actorComponent.aggroTargets.Clear();
			Actor closestActor = Toolbox.getClosestActor(BehaviourActionActor.temp_actors, pActor.current_tile);
			if (closestActor != null)
			{
				if (closestActor.city != null)
				{
					pActor.data.get("cityToAttack", out var pResult, -1L);
					if (!pResult.hasValue())
					{
						pActor.data.set("cityToAttack", closestActor.city.data.id);
						pActor.data.set("attacksForCity", Randy.randomInt(3, 10));
					}
				}
				pActor.beh_tile_target = closestActor.current_tile;
				return forceTask(pActor, "ufo_chase", pClean: false);
			}
		}
		return BehResult.Continue;
	}
}
