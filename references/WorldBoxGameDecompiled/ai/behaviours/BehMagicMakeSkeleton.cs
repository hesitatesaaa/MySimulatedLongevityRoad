namespace ai.behaviours;

public class BehMagicMakeSkeleton : BehaviourActionActor
{
	public override BehResult execute(Actor pActor)
	{
		int num = 0;
		foreach (Actor item in Finder.findSpeciesAroundTileChunk(pActor.current_tile, "skeleton"))
		{
			_ = item;
			if (num++ > 6)
			{
				return BehResult.Stop;
			}
		}
		WorldTile worldTile = pActor.current_tile?.region?.tiles.GetRandom();
		if (worldTile == null)
		{
			return BehResult.Stop;
		}
		if (worldTile.hasUnits())
		{
			return BehResult.Stop;
		}
		pActor.doCastAnimation();
		ActionLibrary.spawnSkeleton(pActor, worldTile);
		return BehResult.Continue;
	}
}
