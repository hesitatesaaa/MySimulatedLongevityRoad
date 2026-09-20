using System.Collections.Generic;

namespace ai.behaviours;

public class BehAnimalFindTile : BehaviourActionActor
{
	public override BehResult execute(Actor pActor)
	{
		if (Randy.randomChance(0.8f))
		{
			using IEnumerator<Actor> enumerator = Finder.findSpeciesAroundTileChunk(pActor.current_tile, "druid").GetEnumerator();
			if (enumerator.MoveNext())
			{
				Actor current = enumerator.Current;
				pActor.beh_tile_target = current.current_tile.region.getRandomTile();
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
			pActor.beh_tile_target = mapRegion.getRandomTile();
			return BehResult.Continue;
		}
		return BehResult.Stop;
	}
}
