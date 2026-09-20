namespace ai.behaviours;

public class BehFindRandomCivBuildingTile : BehaviourActionActor
{
	public override BehResult execute(Actor pActor)
	{
		MapRegion region = pActor.current_tile.region;
		if (Randy.randomChance(0.65f) && region.tiles.Count > 0)
		{
			pActor.beh_tile_target = region.getRandomTile();
			return BehResult.Continue;
		}
		Building building = null;
		foreach (Building item in Finder.getBuildingsFromChunk(pActor.current_tile, 1, 0, pRandom: true))
		{
			if (item.asset.city_building && item.current_tile.isSameIsland(pActor.current_tile) && item.isCiv())
			{
				building = item;
				break;
			}
		}
		if (building == null)
		{
			if (region.tiles.Count > 0)
			{
				pActor.beh_tile_target = region.getRandomTile();
				return BehResult.Continue;
			}
			return BehResult.Stop;
		}
		pActor.beh_tile_target = building.current_tile.region.getRandomTile();
		return BehResult.Continue;
	}
}
