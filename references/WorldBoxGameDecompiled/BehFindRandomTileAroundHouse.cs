using ai.behaviours;

public class BehFindRandomTileAroundHouse : BehaviourActionActor
{
	public override BehResult execute(Actor pActor)
	{
		Building homeBuilding = pActor.getHomeBuilding();
		if (homeBuilding == null)
		{
			return BehResult.Stop;
		}
		if (!homeBuilding.current_tile.isSameIsland(pActor.current_tile))
		{
			return BehResult.Stop;
		}
		MapRegion mapRegion = homeBuilding.current_tile.region;
		if (Randy.randomChance(0.2f) && mapRegion.neighbours.Count > 0)
		{
			mapRegion = mapRegion.neighbours.GetRandom();
		}
		WorldTile random = mapRegion.tiles.GetRandom();
		pActor.beh_tile_target = random;
		return BehResult.Continue;
	}
}
