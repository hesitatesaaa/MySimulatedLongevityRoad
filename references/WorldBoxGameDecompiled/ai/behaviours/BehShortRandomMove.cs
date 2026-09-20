namespace ai.behaviours;

public class BehShortRandomMove : BehaviourActionActor
{
	public override BehResult execute(Actor pActor)
	{
		foreach (WorldTile item in pActor.current_tile.neighboursAll.LoopRandom())
		{
			if (item.Type.layer_type == pActor.current_tile.Type.layer_type)
			{
				pActor.beh_tile_target = item;
				return BehResult.Continue;
			}
		}
		return BehResult.Stop;
	}
}
