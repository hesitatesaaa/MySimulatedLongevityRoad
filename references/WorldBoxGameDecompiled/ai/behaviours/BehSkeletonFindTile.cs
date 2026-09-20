using System.Collections.Generic;

namespace ai.behaviours;

public class BehSkeletonFindTile : BehaviourActionActor
{
	public override BehResult execute(Actor pActor)
	{
		using (IEnumerator<Actor> enumerator = Finder.findSpeciesAroundTileChunk(pActor.current_tile, "necromancer").GetEnumerator())
		{
			if (enumerator.MoveNext())
			{
				Actor current = enumerator.Current;
				pActor.beh_tile_target = current.current_tile.region.tiles.GetRandom();
				return BehResult.Continue;
			}
		}
		MapRegion mapRegion = pActor.current_tile.region;
		if (mapRegion.neighbours.Count > 0 && Randy.randomBool())
		{
			mapRegion = mapRegion.neighbours.GetRandom();
		}
		if (mapRegion.tiles.Count > 0)
		{
			pActor.beh_tile_target = mapRegion.tiles.GetRandom();
			return BehResult.Continue;
		}
		return BehResult.Stop;
	}
}
