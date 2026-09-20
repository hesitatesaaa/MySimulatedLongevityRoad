using ai.behaviours;

public class BehFindRandomTileNearBuildingTarget : BehaviourActionActor
{
	public override BehResult execute(Actor pActor)
	{
		if (pActor.beh_building_target == null)
		{
			return BehResult.Stop;
		}
		if (!pActor.beh_building_target.current_tile.isSameIsland(pActor.current_tile))
		{
			return BehResult.Stop;
		}
		MapRegion mapRegion = pActor.beh_building_target.current_tile.region;
		if (Randy.randomChance(0.2f) && mapRegion.neighbours.Count > 0)
		{
			mapRegion = mapRegion.neighbours.GetRandom();
		}
		WorldTile random = mapRegion.tiles.GetRandom();
		pActor.beh_tile_target = random;
		return BehResult.Continue;
	}
}
