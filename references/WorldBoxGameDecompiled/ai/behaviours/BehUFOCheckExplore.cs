namespace ai.behaviours;

public class BehUFOCheckExplore : BehaviourActionActor
{
	public override BehResult execute(Actor pActor)
	{
		pActor.data.get("exploringTicks", out var pResult, 0);
		if (pResult > 0)
		{
			pResult--;
			pActor.data.set("exploringTicks", pResult);
			if (pActor.current_tile.zone.city != null)
			{
				pActor.data.set("cityToAttack", pActor.current_tile.zone.city.data.id);
				pActor.data.set("attacksForCity", Randy.randomInt(3, 10));
				return forceTask(pActor, "ufo_fly", pClean: false);
			}
			if (pActor.ai.task?.id == "ufo_explore")
			{
				return BehResult.RestartTask;
			}
			return forceTask(pActor, "ufo_explore", pClean: false);
		}
		return BehResult.Continue;
	}
}
