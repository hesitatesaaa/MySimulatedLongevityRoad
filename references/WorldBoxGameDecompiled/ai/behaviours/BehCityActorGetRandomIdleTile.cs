namespace ai.behaviours;

public class BehCityActorGetRandomIdleTile : BehCityActor
{
	public override BehResult execute(Actor pActor)
	{
		if (!pActor.city.hasZones())
		{
			return BehResult.Stop;
		}
		WorldTile worldTile = tryToGetBonfireTile(pActor);
		if (worldTile == null)
		{
			worldTile = getRandomCityZoneTile(pActor);
		}
		if (worldTile == null)
		{
			return BehResult.Stop;
		}
		pActor.beh_tile_target = worldTile;
		return BehResult.Continue;
	}

	private WorldTile getRandomCityZoneTile(Actor pActor)
	{
		WorldTile worldTile = null;
		if (!pActor.current_tile.isSameCityHere(pActor.city) || Randy.randomChance(0.2f))
		{
			worldTile = pActor.city.zones.GetRandom().tiles.GetRandom();
			if (!worldTile.isSameIsland(pActor.current_tile))
			{
				return null;
			}
		}
		else
		{
			worldTile = pActor.current_tile.region.tiles.GetRandom();
		}
		return worldTile;
	}

	private WorldTile tryToGetBonfireTile(Actor pActor)
	{
		Building buildingOfType = pActor.city.getBuildingOfType("type_bonfire", pCountOnlyFinished: true, pRandom: true, pOnlyFreeTile: false, pActor.current_island);
		if (buildingOfType == null)
		{
			return null;
		}
		WorldTile random = buildingOfType.tiles.GetRandom();
		if (Randy.randomChance(0.3f))
		{
			MapRegion mapRegion = buildingOfType.current_tile.region;
			if (mapRegion.neighbours.Count > 0 && Randy.randomChance(0.3f))
			{
				mapRegion = mapRegion.neighbours.GetRandom();
			}
			random = mapRegion.tiles.GetRandom();
		}
		return random;
	}
}
