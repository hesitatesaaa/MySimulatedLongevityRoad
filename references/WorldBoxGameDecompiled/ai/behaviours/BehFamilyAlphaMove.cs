namespace ai.behaviours;

public class BehFamilyAlphaMove : BehaviourActionActor
{
	public override BehResult execute(Actor pActor)
	{
		WorldTile worldTile = null;
		if (pActor.isHerbivore())
		{
			worldTile = findTileForHerbivore(pActor);
		}
		else if (pActor.isCarnivore())
		{
			worldTile = findTileForCarnivore(pActor);
		}
		if (worldTile != null)
		{
			worldTile = worldTile.region.tiles.GetRandom();
		}
		if (worldTile == null)
		{
			return forceTask(pActor, "random_move");
		}
		pActor.beh_tile_target = worldTile.region.tiles.GetRandom();
		return BehResult.Continue;
	}

	private Building getNearbyBuildings(WorldTile pTile)
	{
		float num = float.MaxValue;
		Building result = null;
		foreach (Building item in Finder.getBuildingsFromChunk(pTile, 3, 0, pRandom: true))
		{
			float num2 = Toolbox.SquaredDistTile(item.current_tile, pTile);
			if (!(num2 >= num) && item.asset.flora && item.current_tile.isSameIsland(pTile))
			{
				result = item;
				num = num2;
				if (num < 25f)
				{
					return result;
				}
			}
		}
		return result;
	}

	private Actor getNearbyActor(Actor pActor, WorldTile pTile)
	{
		float num = float.MaxValue;
		Actor result = null;
		foreach (Actor item in Finder.getUnitsFromChunk(pTile, 3, 0f, pRandom: true))
		{
			float num2 = Toolbox.SquaredDistTile(item.current_tile, pTile);
			if (!(num2 >= num) && item.family != pActor.family && !item.isSameSpecies(pActor) && item.current_tile.isSameIsland(pTile) && item.asset.source_meat)
			{
				result = item;
				num = num2;
				if (num < 5f)
				{
					return result;
				}
			}
		}
		return result;
	}

	private WorldTile findTileForHerbivore(Actor pActor)
	{
		return getNearbyBuildings(pActor.current_tile)?.current_tile.region.tiles.GetRandom();
	}

	private WorldTile findTileForCarnivore(Actor pActor)
	{
		WorldTile current_tile = pActor.current_tile;
		Actor nearbyActor = getNearbyActor(pActor, current_tile);
		if (nearbyActor != null)
		{
			return nearbyActor.current_tile.region.tiles.GetRandom();
		}
		if (nearbyActor == null)
		{
			return current_tile.region.island.getRandomTile();
		}
		return null;
	}
}
