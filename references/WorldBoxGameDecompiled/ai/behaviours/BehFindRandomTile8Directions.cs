namespace ai.behaviours;

public class BehFindRandomTile8Directions : BehaviourActionActor
{
	public override BehResult execute(Actor pActor)
	{
		pActor.data.get("random_steps", out var pResult, 0);
		pActor.data.get("direction", out var pResult2, -1);
		ActorDirection actorDirection;
		if (pResult > 0)
		{
			if (pActor.beh_tile_target != null && pActor.current_tile != pActor.beh_tile_target)
			{
				return BehResult.Continue;
			}
			actorDirection = Toolbox.directions_all[pResult2];
		}
		else
		{
			int pMinInclusive = Randy.randomInt(1, 6);
			int pMaxExclusive = Randy.randomInt(10, 60);
			pResult = Randy.randomInt(pMinInclusive, pMaxExclusive);
			if (pResult2 < 0)
			{
				actorDirection = Randy.getRandom(Toolbox.directions_all);
				pResult2 = Toolbox.directions_all.IndexOf(actorDirection);
			}
			else
			{
				actorDirection = Toolbox.directions_all[pResult2];
				actorDirection = Randy.getRandom(Toolbox.directions_all_turns[actorDirection]);
				pResult2 = Toolbox.directions_all.IndexOf(actorDirection);
			}
			pActor.data.set("direction", pResult2);
		}
		pResult--;
		pActor.beh_tile_target = Ant.getNextTile(pActor.current_tile, actorDirection);
		if (pActor.beh_tile_target == null)
		{
			pActor.data.set("random_steps", 0);
			pActor.data.set("direction", -1);
			return BehResult.RepeatStep;
		}
		pActor.data.set("random_steps", pResult);
		return BehResult.Continue;
	}
}
