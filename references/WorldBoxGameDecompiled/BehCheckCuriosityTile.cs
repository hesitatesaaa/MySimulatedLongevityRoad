using ai.behaviours;

public class BehCheckCuriosityTile : BehaviourActionActor
{
	public override BehResult execute(Actor pActor)
	{
		if (pActor.scheduled_tile_target == null)
		{
			return BehResult.Stop;
		}
		WorldTile scheduled_tile_target = pActor.scheduled_tile_target;
		pActor.scheduled_tile_target = null;
		float num = 0.6f;
		if (pActor.hasSubspecies() && pActor.subspecies.has_trait_curious)
		{
			num += 0.3f;
		}
		if (!Randy.randomChance(num))
		{
			return BehResult.Stop;
		}
		WorldTile walkableTileAround = scheduled_tile_target.getWalkableTileAround(pActor.current_tile);
		if (walkableTileAround == null)
		{
			return BehResult.Stop;
		}
		pActor.beh_tile_target = walkableTileAround;
		return BehResult.Continue;
	}
}
