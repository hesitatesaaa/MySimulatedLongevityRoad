namespace ai.behaviours;

public class BehFindRandomTile4Directions : BehaviourActionActor
{
	public override BehResult execute(Actor pActor)
	{
		pActor.data.get("direction", out var pResult, -1);
		ActorDirection actorDirection;
		if (pResult == -1)
		{
			actorDirection = Randy.getRandom(Toolbox.directions);
			pResult = Toolbox.directions.IndexOf(actorDirection);
			pActor.data.set("direction", pResult);
		}
		else
		{
			actorDirection = Toolbox.directions[pResult];
		}
		pActor.beh_tile_target = Ant.getNextTile(pActor.current_tile, actorDirection);
		if (pActor.beh_tile_target == null)
		{
			pActor.beh_tile_target = Ant.randomNeighbour(pActor.current_tile);
		}
		return BehResult.Continue;
	}
}
