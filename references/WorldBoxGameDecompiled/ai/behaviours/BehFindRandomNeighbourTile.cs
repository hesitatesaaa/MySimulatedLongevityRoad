namespace ai.behaviours;

public class BehFindRandomNeighbourTile : BehaviourActionActor
{
	public override BehResult execute(Actor pActor)
	{
		if ((pActor.beh_tile_target = pActor.current_tile.getWalkableTileAround(pActor.current_tile)) == null)
		{
			return BehResult.Stop;
		}
		return BehResult.Continue;
	}
}
