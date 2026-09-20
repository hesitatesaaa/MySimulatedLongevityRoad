namespace ai.behaviours;

public class BehFindRandomFarmTile : BehaviourActionActor
{
	public override BehResult execute(Actor pActor)
	{
		MapRegion mapRegion = pActor.current_tile.region;
		if (Randy.randomChance(0.65f) && mapRegion.tiles.Count > 0)
		{
			pActor.beh_tile_target = mapRegion.tiles.GetRandom();
			return BehResult.Continue;
		}
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
